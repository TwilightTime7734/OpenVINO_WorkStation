using System.Diagnostics;
using System.Text.Json;
using static WorkStationC_.AppFiles;

namespace WorkStationC_;

// Starts, watches, and stops the one ovms.exe process this app uses to serve a model.
internal sealed class OvmsServer(Action<string> output) : IAsyncDisposable
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(5) };
    private Process? _process;

    public string ModelName { get; private set; } = string.Empty;
    public string Device { get; private set; } = string.Empty;
    public bool IsRunning => _process is { HasExited: false };

    // Roughly how many tokens of conversation the reserved working memory holds, when known.
    // Set by Load Model before the server starts; only meaningful while IsRunning.
    public int? ContextTokens { get; set; }

    internal static IReadOnlyList<string> BuildArguments(string modelPath, string modelName, string task, string device, Uri address,
        IReadOnlyList<string>? extraArguments = null) =>
    [
        "--model_path", modelPath,
        "--model_name", modelName,
        "--task", task,
        "--target_device", device,
        "--rest_bind_address", "127.0.0.1",
        "--rest_port", address.Port.ToString(),
        "--grpc_bind_address", "127.0.0.1",
        "--port", (address.Port + 1000).ToString(),
        .. extraArguments ?? [],
    ];

    // Tool and reasoning parsers let OVMS return tool calls and thinking as structured data.
    // A model's own graph.pbtxt already sets them, so they are only added when there is no graph file.
    internal static IReadOnlyList<string> ChatParserArguments(string modelPath, string modelName)
    {
        if (File.Exists(OvmsGraph.GraphPath(modelPath))) return [];
        string modelType = string.Empty;
        string config = Path.Combine(modelPath, "config.json");
        if (File.Exists(config))
        {
            try
            {
                using var json = JsonDocument.Parse(File.ReadAllText(config));
                if (json.RootElement.TryGetProperty("model_type", out var type)) modelType = type.GetString() ?? string.Empty;
            }
            catch (JsonException) { }
        }
        bool coder = modelName.Contains("coder", StringComparison.OrdinalIgnoreCase);
        string toolParser = modelType switch
        {
            "qwen3_5" => "qwen3coder",
            "qwen3_moe" when coder => "qwen3coder",
            "qwen2" or "qwen3" or "qwen3_moe" or "qwen3_vl" or "qwen3_vl_moe" => "hermes3",
            _ => string.Empty,
        };
        bool thinks = modelType is "qwen3" or "qwen3_5" || (modelType is "qwen3_moe" && !coder)
            || (modelType is "qwen3_vl" or "qwen3_vl_moe" && modelName.Contains("thinking", StringComparison.OrdinalIgnoreCase));
        List<string> arguments = [];
        if (toolParser.Length > 0) arguments.AddRange(["--tool_parser", toolParser]);
        if (thinks) arguments.AddRange(["--reasoning_parser", "qwen3"]);
        return arguments;
    }

    public async Task StartAsync(string executable, string pythonFolder, string modelPath, string modelName,
        string task, string device, Uri address, IReadOnlyList<string> extraArguments, CancellationToken cancellationToken)
    {
        await StopAsync();
        executable = ExpandPathText(executable.Trim());
        pythonFolder = ExpandPathText(pythonFolder.Trim());
        if (!File.Exists(executable)) throw new FileNotFoundException($"The OVMS program was not found: {executable}. Set it on the OVMS Pull tab.");
        if (await IsAnsweringAsync(address, cancellationToken))
            throw new InvalidOperationException($"Another server is already using {address}. Stop it first, or press Connect to chat with it.");

        string ovmsFolder = Path.GetDirectoryName(executable)!;
        ProcessStartInfo info = new(executable)
        {
            WorkingDirectory = ovmsFolder,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
        var arguments = BuildArguments(modelPath, modelName, task, device, address, extraArguments);
        foreach (string argument in arguments) info.ArgumentList.Add(argument);
        // Same environment as OVMS setupvars: its folder and bundled Python first on PATH.
        List<string> path = [ovmsFolder];
        if (Directory.Exists(pythonFolder))
        {
            path.Insert(0, pythonFolder);
            info.Environment["PYTHONHOME"] = pythonFolder;
        }
        path.Add(Environment.GetEnvironmentVariable("PATH") ?? string.Empty);
        info.Environment["PATH"] = string.Join(Path.PathSeparator, path);
        info.Environment["OVMS_DIR"] = ovmsFolder;

        lock (_errors)
        {
            _errors.Clear();
            _errorContinues = false;
        }
        Process process = new() { StartInfo = info, EnableRaisingEvents = true };
        process.OutputDataReceived += Process_OutputDataReceived;
        process.ErrorDataReceived += Process_OutputDataReceived;
        output($"Starting OVMS: {QuoteCommandArgument(executable)} {FormatCommandArguments(arguments)}");
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        _process = process;
        ModelName = modelName;
        Device = device;
        try { await WaitUntilReadyAsync(process, address, modelName, cancellationToken); }
        catch
        {
            await StopAsync();
            throw;
        }
    }

    // OVMS error lines look like "[time][thread][area][error][file:line] message"; the reason often follows on
    // the next line without a bracket. Kept so a failed load can say why instead of only "exit code 1".
    private readonly List<string> _errors = [];
    private bool _errorContinues;

    private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.Data)) return;
        output($"OVMS: {e.Data}");
        lock (_errors)
        {
            int marker = e.Data.IndexOf("][error][", StringComparison.Ordinal);
            if (marker >= 0)
            {
                int end = e.Data.IndexOf("] ", marker + 9, StringComparison.Ordinal);
                _errors.Add(end >= 0 ? e.Data[(end + 2)..].Trim() : e.Data.Trim());
                _errorContinues = true;
            }
            else if (_errorContinues && !e.Data.StartsWith('['))
            {
                _errors.Add(e.Data.Trim());
            }
            else
            {
                _errorContinues = false;
            }
        }
    }

    // The most specific reason: a detail line (such as "Unsupported 'x' VLM model type") before the generic ones.
    private string FailureReason()
    {
        lock (_errors)
        {
            string? reason = _errors.FirstOrDefault(line => line.Contains("Unsupported", StringComparison.OrdinalIgnoreCase)
                    || line.Contains("Failed to open", StringComparison.OrdinalIgnoreCase)
                    || line.Contains("not found", StringComparison.OrdinalIgnoreCase))
                ?? _errors.FirstOrDefault();
            if (reason is null) return " See the OVMS messages above.";
            string hint = reason.Contains("Unsupported", StringComparison.OrdinalIgnoreCase)
                ? " This OVMS version does not support this kind of model."
                : " See the OVMS messages above.";
            return $" Reason: {reason}.{hint}";
        }
    }

    private async Task WaitUntilReadyAsync(Process process, Uri address, string modelName, CancellationToken cancellationToken)
    {
        var started = Stopwatch.StartNew();
        TimeSpan nextNotice = TimeSpan.FromSeconds(30);
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (process.HasExited)
            {
                process.WaitForExit(); // Let the last output lines arrive before reading the reason.
                throw new InvalidOperationException($"OVMS stopped while loading the model (exit code {process.ExitCode}).{FailureReason()}");
            }
            string? state = await GetModelStateAsync(address, modelName, cancellationToken);
            if (state == "AVAILABLE") return;
            if (state is "END" or "UNLOADING")
                throw new InvalidOperationException($"OVMS could not load the model (state {state}).{FailureReason()}");
            if (started.Elapsed >= nextNotice)
            {
                nextNotice += TimeSpan.FromSeconds(30);
                output($"Still loading {modelName} ({started.Elapsed.TotalSeconds:0} s). Large models can take several minutes.");
            }
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }
    }

    // Reads /v1/config. Returns the model's best state, or null while the server is not answering yet.
    internal static async Task<string?> GetModelStateAsync(Uri address, string modelName, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await Http.GetAsync(new Uri(address, "/v1/config"), cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            foreach (var model in json.RootElement.EnumerateObject())
            {
                if (!model.Name.Equals(modelName, StringComparison.OrdinalIgnoreCase)) continue;
                if (!model.Value.TryGetProperty("model_version_status", out var versions)) return null;
                string[] states = versions.EnumerateArray()
                    .Select(version => version.TryGetProperty("state", out var value) ? value.GetString() ?? string.Empty : string.Empty)
                    .ToArray();
                return states.Contains("AVAILABLE") ? "AVAILABLE" : states.FirstOrDefault();
            }
            return "LOADING";
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    internal static async Task<bool> IsAnsweringAsync(Uri address, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await Http.GetAsync(new Uri(address, "/v1/config"), cancellationToken);
            return true;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            return false;
        }
    }

    public async Task StopAsync()
    {
        var process = _process;
        _process = null;
        ModelName = string.Empty;
        Device = string.Empty;
        if (process is null) return;
        try
        {
            if (!process.HasExited)
            {
                output("Stopping OVMS...");
                process.Kill(entireProcessTree: true);
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                await process.WaitForExitAsync(timeout.Token);
            }
            output("OVMS stopped. The model is unloaded.");
        }
        finally
        {
            process.OutputDataReceived -= Process_OutputDataReceived;
            process.ErrorDataReceived -= Process_OutputDataReceived;
            process.Dispose();
        }
    }

    public async ValueTask DisposeAsync() => await StopAsync();
}
