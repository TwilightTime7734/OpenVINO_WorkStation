using static WorkStationC_.AppFiles;
using static WorkStationC_.ModelFiles;
using static WorkStationC_.WorkstationInfo;

namespace WorkStationC_;

internal sealed class WorkstationTab(OvmsServer server, TextBox root, TextBox model, Label modelSize,
    ComboBox target, ComboBox multiTarget, TextBox details, TextBox address, TextBox status,
    TextBox ovmsExecutable, TextBox ovmsPython, Action<string> output)
{
    // Name and usable memory of each GPU, filled by Check Workstation; used to warn before loading a model that will not fit.
    private readonly Dictionary<string, (string Name, ulong MemoryBytes)> _gpus = new(StringComparer.OrdinalIgnoreCase);

    // OVMS splits one model across several GPUs with HETERO, not MULTI.
    public string Target => target.Text == "MULTI" ? multiTarget.Text.Replace("MULTI:", "HETERO:", StringComparison.OrdinalIgnoreCase) : target.Text;

    public Uri Address
    {
        get
        {
            string text = address.Text.Trim();
            if (!text.Contains("://", StringComparison.Ordinal)) text = "http://" + text;
            if (!Uri.TryCreate(text, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttp)
                throw new InvalidOperationException("Enter a server address such as http://localhost:8000.");
            return new UriBuilder(uri) { Path = "/" }.Uri;
        }
    }

    public void Initialize() => root.Text = GetModelRoot();

    public void SaveRoot()
    {
        SaveModelRoot(root.Text);
        root.Text = GetConfiguredModelRoot();
    }

    public void BrowseRoot()
    {
        using FolderBrowserDialog dialog = new()
        {
            Description = "Select the main model folder",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true,
            SelectedPath = Directory.Exists(root.Text) ? root.Text : string.Empty,
        };
        if (dialog.ShowDialog(root.FindForm()) != DialogResult.OK) return;
        root.Text = dialog.SelectedPath;
        SaveRoot();
    }

    // Shows the OpenVINO model folders (names ending in -ov) inside the main model folder.
    public void BrowseModel()
    {
        string modelRoot = root.Text.Trim();
        if (!Directory.Exists(modelRoot)) throw new DirectoryNotFoundException($"The main model folder does not exist: {modelRoot}");
        string[] names = FindModelFolders(modelRoot);
        if (names.Length == 0)
        {
            output($"No model folders ending in -ov, and no models pulled with OVMS Pull, were found in {modelRoot}.");
            return;
        }
        string selectedPath = Path.TrimEndingDirectorySeparator(model.Text.Trim());
        string current = selectedPath.Length > 0 && IsPathUnder(selectedPath, modelRoot)
            ? Path.GetRelativePath(modelRoot, selectedPath)
            : string.Empty;
        using ModelPickerForm picker = new(modelRoot, names, current);
        if (picker.ShowDialog(model.FindForm()) != DialogResult.OK || picker.SelectedModel.Length == 0) return;
        model.Text = Path.Combine(modelRoot, picker.SelectedModel);
        ulong bytes = GetWeightBytes(model.Text);
        modelSize.Text = bytes == 0 ? "Size unknown" : $"≈ {FormatBytes(bytes)}";
        output($"Selected {picker.SelectedModel}: about {FormatBytes(bytes)} of model weights to load.");
    }

    // Model folders for the Select list, relative to the main model folder: folders ending in -ov directly inside it,
    // and models pulled by OVMS Pull, which land one or two levels down (for example Qwen\Qwen3.6-27B) with a graph.pbtxt.
    internal static string[] FindModelFolders(string modelRoot)
    {
        var options = new EnumerationOptions { IgnoreInaccessible = true, AttributesToSkip = FileAttributes.ReparsePoint };
        List<string> found = [];
        foreach (string top in Directory.EnumerateDirectories(modelRoot, "*", options))
        {
            if (top.EndsWith("-ov", StringComparison.OrdinalIgnoreCase))
            {
                found.Add(Path.GetFileName(top));
                continue;
            }
            foreach (string second in Directory.EnumerateDirectories(top, "*", options))
            {
                if (File.Exists(Path.Combine(second, "graph.pbtxt"))) { found.Add(Path.GetRelativePath(modelRoot, second)); continue; }
                foreach (string third in Directory.EnumerateDirectories(second, "*", options))
                    if (File.Exists(Path.Combine(third, "graph.pbtxt"))) found.Add(Path.GetRelativePath(modelRoot, third));
            }
        }
        return found.Order(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    // OVMS loads a model from the folder that describes the whole model: the one with graph.pbtxt or, for image
    // models, model_index.json. That is often above the main model file (an image model's file is in unet\).
    // Use the highest such folder between the main model file and the selected folder.
    internal static string FindOvmsModelFolder(string selectedFolder, string runtimePath)
    {
        string runtimeFolder = Directory.Exists(runtimePath) ? runtimePath : Path.GetDirectoryName(runtimePath)!;
        string top = Path.TrimEndingDirectorySeparator(Path.GetFullPath(selectedFolder));
        string? found = null;
        for (string? folder = Path.TrimEndingDirectorySeparator(Path.GetFullPath(runtimeFolder));
             folder is not null && IsPathUnder(folder, top);
             folder = Path.GetDirectoryName(folder))
        {
            if (File.Exists(Path.Combine(folder, "graph.pbtxt")) || File.Exists(Path.Combine(folder, "model_index.json"))) found = folder;
        }
        return found ?? runtimeFolder;
    }

    // A model larger than the GPU's memory spills into system memory: very slow, then out of memory.
    // Ask before loading one that cannot fit; warn when little room is left for the conversation's working memory.
    private const ulong WorkingMemoryBytes = 4UL << 30;

    private void CheckModelFits(string modelPath)
    {
        if (!_gpus.TryGetValue(Target, out var gpu) || gpu.MemoryBytes == 0) return;
        ulong weights = GetWeightBytes(modelPath);
        if (weights > gpu.MemoryBytes)
        {
            var answer = MessageBox.Show(model.FindForm(),
                $"This model needs about {FormatBytes(weights)} for its weights, but {Target} ({gpu.Name}) has {FormatBytes(gpu.MemoryBytes)}.\n\n" +
                "It will likely be very slow or run out of memory. Load it anyway?",
                "Model is larger than the GPU", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (answer != DialogResult.Yes) throw new OperationCanceledException();
            output($"Warning: loading {FormatBytes(weights)} of weights on {Target}, which has {FormatBytes(gpu.MemoryBytes)}.");
        }
        else if (weights + WorkingMemoryBytes > gpu.MemoryBytes)
        {
            output($"Warning: this model uses {FormatBytes(weights)} of {Target}'s {FormatBytes(gpu.MemoryBytes)}, leaving " +
                $"{FormatBytes(gpu.MemoryBytes - weights)} for working memory. Long conversations may become slow or run out of memory.");
        }
    }

    // OVMS chat models need OpenVINO tokenizer files beside the model. A folder with only Hugging Face's
    // tokenizer.json is an incomplete conversion, and OVMS cannot use it.
    private static void CheckTokenizerFiles(string modelPath)
    {
        string[] missing = ["openvino_tokenizer.xml", "openvino_detokenizer.xml"];
        missing = missing.Where(file => !File.Exists(Path.Combine(modelPath, file))).ToArray();
        if (missing.Length == 0) return;
        throw new InvalidOperationException(
            $"This model folder is missing {string.Join(" and ", missing)}, so OVMS cannot load it. " +
            "The download or conversion is incomplete. Convert it again (for example with OVMS Pull) or download a complete OpenVINO version.");
    }

    // Reserve a fixed working memory (KV cache) that fits beside the weights, at 8-bit precision, so a long
    // conversation cannot grow into system memory. Graph files get the setting written in; models without a
    // graph get command-line options. Returns those options.
    private IReadOnlyList<string> SetWorkingMemory(string modelPath)
    {
        server.ContextTokens = null;
        if (!_gpus.TryGetValue(Target, out var gpu) || gpu.MemoryBytes == 0)
        {
            output("Working memory is not limited because the GPU's memory size is unknown. Press Check Workstation, then load again.");
            return [];
        }
        int cacheGigabytes = ModelMemory.CacheGigabytes(gpu.MemoryBytes, GetWeightBytes(modelPath));
        string? graphChange = OvmsGraph.SetMemory(modelPath, cacheGigabytes, reducedPrecision: true);
        IReadOnlyList<string> arguments = graphChange is null
            ? ["--cache_size", cacheGigabytes.ToString(), "--kv_cache_precision", "u8"]
            : [];
        if (graphChange is { Length: > 0 }) output($"Updated {OvmsGraph.GraphPath(modelPath)}: {graphChange}.");
        server.ContextTokens = ModelMemory.ContextTokens(modelPath, cacheGigabytes, reducedPrecision: true);
        output($"Working memory: {cacheGigabytes} GB reserved on {Target} at 8-bit precision" +
            (server.ContextTokens is { } tokens ? $", room for about {tokens:N0} tokens of conversation." : "."));
        return arguments;
    }

    // Approximate load size: the OpenVINO weight files (.bin), plus any GGUF file.
    internal static ulong GetWeightBytes(string folder) => Directory
        .EnumerateFiles(folder, "*", SearchOption.AllDirectories)
        .Where(file => Path.GetExtension(file).ToLowerInvariant() is ".bin" or ".gguf")
        .Aggregate(0UL, (total, file) => total + (ulong)new FileInfo(file).Length);

    public async Task CheckAsync(CancellationToken cancellationToken)
    {
        var devices = await Task.Run(() =>
        {
            OpenVinoRuntime.Configure();
            using OpenVinoSharp.Core core = new();
            var available = core.GetAvailableDevices();
            var targets = BuildCompileTargetChoices(core, available);
            var gpus = targets.Where(IsGpuCompileTarget)
                .Select(gpu => (Device: gpu, Info: DescribeGpu(core, gpu)))
                .Where(item => item.Info is not null)
                .ToDictionary(item => item.Device, item => item.Info!.Value, StringComparer.OrdinalIgnoreCase);
            return (Targets: targets, Devices: available.ToArray(), Gpus: gpus);
        }, cancellationToken);
        output($"{System.Runtime.InteropServices.RuntimeInformation.OSDescription}; {Environment.ProcessorCount} CPU threads");
        output($"OpenVINO devices: {string.Join(", ", devices.Devices)}");
        _gpus.Clear();
        foreach (var (device, info) in devices.Gpus)
        {
            _gpus[device] = info;
            output($"{device}: {info.Name}, {FormatBytes(info.MemoryBytes)} usable memory");
        }
        string previous = target.Text;
        string previousMulti = multiTarget.Text;
        string[] singles = devices.Targets.Where(value => !IsOpenVinoMultiTarget(value)).ToArray();
        string[] multis = devices.Targets.Where(IsOpenVinoMultiTarget).ToArray();
        target.Items.Clear();
        target.Items.AddRange(singles);
        if (multis.Length > 0) target.Items.Add("MULTI");
        multiTarget.Items.Clear();
        multiTarget.Items.AddRange(multis);
        if (target.Items.Count > 0) target.SelectedIndex = Math.Max(0, target.Items.IndexOf(previous));
        if (multis.Length > 0) multiTarget.SelectedItem = multis.Contains(previousMulti) ? previousMulti : multis[0];
        UpdateTarget();
        await ConnectAsync(cancellationToken);
    }

    public void UpdateTarget() => multiTarget.Enabled = target.Text == "MULTI";

    public async Task LoadAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Target)) throw new InvalidOperationException("Choose a model target first. Press Check Workstation to find devices.");
        if (string.IsNullOrWhiteSpace(model.Text)) throw new InvalidOperationException("Select a model folder first.");
        var source = ModelSession.Resolve(model.Text.Trim(), Target);
        var capabilities = ClassifyModelRunPath(source.SelectedFolder, source.RuntimePath, source.Kind);
        string task = capabilities.RunPath switch
        {
            ModelRunPath.TextGeneration or ModelRunPath.VisionLanguage => "text_generation",
            ModelRunPath.TextToImage or ModelRunPath.ImageToImage => "image_generation",
            ModelRunPath.TextEmbedding => "embeddings",
            _ => throw new InvalidOperationException($"OVMS cannot serve this kind of model from here ({DescribeModelRunPath(capabilities.RunPath)})."),
        };
        string modelPath = FindOvmsModelFolder(source.SelectedFolder, source.RuntimePath);
        string modelName = Path.GetFileName(Path.TrimEndingDirectorySeparator(source.SelectedFolder)).ToLowerInvariant();
        if (task == "text_generation" && source.Kind != ModelFileKind.Gguf) CheckTokenizerFiles(modelPath);
        CheckModelFits(modelPath);
        var replaced = OvmsGraph.SetDevice(modelPath, Target);
        if (replaced.Count > 0)
            output($"Updated {OvmsGraph.GraphPath(modelPath)}: device {string.Join(", ", replaced)} changed to {Target}.");
        output($"Loading {modelName} on {Target} with OVMS...");
        status.Text = "Loading...";
        try
        {
            IReadOnlyList<string> extra = task == "text_generation"
                ? [.. OvmsServer.ChatParserArguments(modelPath, modelName), .. SetWorkingMemory(modelPath)]
                : [];
            await server.StartAsync(ovmsExecutable.Text, ovmsPython.Text, modelPath, modelName, task, Target, Address, extra, cancellationToken);
        }
        finally { ShowStatus(); }
        details.Text = $"Loaded: {modelName} on {server.Device}. {DescribeModelRunPath(capabilities.RunPath)}. OVMS task: {task}.";
        output($"{modelName} is ready at {Address}v3. Use the Chat tab to talk to it.");
    }

    public async Task UnloadAsync()
    {
        if (!server.IsRunning)
        {
            output("This app has not started an OVMS server. Close any other OVMS window yourself.");
            return;
        }
        await server.StopAsync();
        ShowStatus();
    }

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        string[] models;
        try { models = await OvmsChat.ListModelsAsync(Address, cancellationToken); }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            output($"No OVMS server is answering at {Address}.");
            ShowStatus();
            return;
        }
        status.Text = server.IsRunning ? $"Running (started here): {server.ModelName}" : $"Connected to a server started outside this app: {string.Join(", ", models)}";
        output($"OVMS at {Address} is serving: {(models.Length == 0 ? "no models yet" : string.Join(", ", models))}");
    }

    public void ShowStatus()
    {
        status.Text = server.IsRunning ? $"Running (started here): {server.ModelName}" : "Stopped";
        if (!server.IsRunning) details.Text = "No model loaded by this app.";
    }
}
