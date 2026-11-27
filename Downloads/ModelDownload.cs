using System.Net.Http;
using System.Text.Json;
using static WorkStationC_.AppFiles;

namespace WorkStationC_;

// How a Hugging Face repository should be brought in: downloaded as is, or converted/pulled by OVMS Pull.
internal enum DownloadRoute { DirectDownload, MissingTokenizer, ConvertOriginal, Gguf, Unknown }

internal sealed record PullAdvice(DownloadRoute Route, string Message, string SourceModel, string SourceFormat,
    string PipelineType, IReadOnlyList<string> GgufFiles)
{
    public bool CanPull => SourceModel.Length > 0 && Route is DownloadRoute.ConvertOriginal or DownloadRoute.Gguf or DownloadRoute.MissingTokenizer;
}

internal sealed class ModelDownload(HttpClient client)
{
    internal sealed record FileToDownload(Uri Address, string Destination);

    internal sealed record FindResult(IReadOnlyList<FileToDownload> Files, PullAdvice? Advice);

    public async Task<FindResult> FindAsync(string address, CancellationToken cancellationToken)
    {
        if (!TryNormalizeDownloadWebAddress(address, out Uri? uri, out string error)) throw new ArgumentException(error);
        if (!uri!.Host.Equals("huggingface.co", StringComparison.OrdinalIgnoreCase))
        {
            string fileName = SanitizePathSegment(Uri.UnescapeDataString(Path.GetFileName(uri.LocalPath)));
            if (!fileName.Contains('.')) throw new ArgumentException("Paste a Hugging Face model URL or a direct file URL.");
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();
            return new([new(uri, BuildDirectWebFileDownloadPath(uri, fileName))], null);
        }

        string[] parts = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries).Select(Uri.UnescapeDataString).ToArray();
        if (parts.Length < 2) throw new ArgumentException("Include the owner and model name in the Hugging Face URL.");
        string repo = $"{parts[0]}/{parts[1]}";
        string revision = parts.Length >= 4 ? parts[3] : "main";
        string relativePath = string.Join('/', parts.Skip(4));
        string mode = parts.Length > 2 ? parts[2] : "tree";
        if (mode is not ("tree" or "blob" or "resolve")) throw new ArgumentException("Use a model, tree, or file URL.");

        if (mode is "blob" or "resolve" && relativePath.Length == 0) throw new ArgumentException("The URL is missing the filename.");

        // The model information also says what the repository holds, which decides the advice below.
        using var modelInfo = await client.GetAsync($"https://huggingface.co/api/models/{EscapePath(repo)}?revision={Uri.EscapeDataString(revision)}", cancellationToken);
        modelInfo.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await modelInfo.Content.ReadAsStringAsync(cancellationToken));
        string[] allFiles = json.RootElement.GetProperty("siblings").EnumerateArray()
            .Select(item => item.GetProperty("rfilename").GetString()!)
            .Distinct().Order().ToArray();
        string[] files = mode is "blob" or "resolve"
            ? [relativePath]
            : allFiles.Where(file => relativePath.Length == 0 || file.StartsWith(relativePath.TrimEnd('/') + "/", StringComparison.Ordinal)).ToArray();
        if (files.Length == 0) throw new InvalidOperationException("No files were found at that address.");

        var advice = Advise(repo, allFiles, mode is "blob" or "resolve" ? relativePath : null,
            ReadString(json.RootElement, "config", "model_type"), ReadString(json.RootElement, "pipeline_tag"), ReadBaseModel(json.RootElement));
        return new(files.Select(file => new FileToDownload(
            new Uri($"https://huggingface.co/{EscapePath(repo)}/resolve/{Uri.EscapeDataString(revision)}/{EscapePath(file)}"),
            BuildModelFileDownloadPath(repo, file, revision))).ToArray(), advice);
    }

    // Decides whether a repository can be downloaded as is or should go through OVMS Pull.
    internal static PullAdvice Advise(string repo, IReadOnlyList<string> files, string? selectedFile,
        string modelType, string pipelineTag, string baseModel)
    {
        string[] names = files.Select(file => Path.GetFileName(file)).ToArray();
        bool Has(string name) => names.Contains(name, StringComparer.OrdinalIgnoreCase);
        bool openVino = names.Any(name => name.StartsWith("openvino_", StringComparison.OrdinalIgnoreCase)
            && name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));
        bool openVinoTokenizer = Has("openvino_tokenizer.xml") && Has("openvino_detokenizer.xml");
        bool imageModel = Has("model_index.json");
        bool original = names.Any(name => name.EndsWith(".safetensors", StringComparison.OrdinalIgnoreCase)
            || (name.StartsWith("pytorch_model", StringComparison.OrdinalIgnoreCase) && name.EndsWith(".bin", StringComparison.OrdinalIgnoreCase)));
        List<string> ggufs = files.Where(file => file.EndsWith(".gguf", StringComparison.OrdinalIgnoreCase)).ToList();
        // Default to the file the address pointed at; otherwise a common good-quality size rather than the largest.
        string? preferred = selectedFile is not null && ggufs.Contains(selectedFile) ? selectedFile
            : new[] { "Q8_0", "Q6_K", "Q4_K_M" }.Select(tag => ggufs.FirstOrDefault(file => file.Contains(tag, StringComparison.OrdinalIgnoreCase)))
                .FirstOrDefault(file => file is not null);
        if (preferred is not null && ggufs.Remove(preferred)) ggufs.Insert(0, preferred);
        bool vision = pipelineTag is "image-text-to-text" or "image-to-text"
            || modelType.Contains("_vl", StringComparison.OrdinalIgnoreCase) || modelType.StartsWith("llava", StringComparison.OrdinalIgnoreCase);
        string pipeline = vision ? "VLM" : "LM";
        string kind = modelType.Length > 0 ? $" (model type {modelType})" : string.Empty;

        if (openVino && (openVinoTokenizer || imageModel))
            return new(DownloadRoute.DirectDownload, $"Already converted to OpenVINO{kind}. Use Download.",
                string.Empty, string.Empty, string.Empty, []);
        if (openVino)
            return new(DownloadRoute.MissingTokenizer,
                $"This OpenVINO copy{kind} has no OpenVINO tokenizer files, so OVMS cannot load it for chat. " +
                (baseModel.Length > 0
                    ? $"Convert the original model ({baseModel}) with OVMS Pull instead."
                    : "Convert the original model with OVMS Pull instead; enter its name on the OVMS Pull tab."),
                baseModel, OvmsPull.OvmsSourceFormatExport, pipeline, []);
        if (ggufs.Count > 0)
            return new(DownloadRoute.Gguf,
                $"GGUF model with {ggufs.Count} file(s). Use Convert with OVMS Pull to pull {Path.GetFileName(ggufs[0])}" +
                (ggufs.Count > 1
                    ? $"; to use another, change GGUF filename on the OVMS Pull tab ({string.Join(", ", ggufs.Skip(1).Take(5).Select(file => Path.GetFileName(file)))}" +
                      (ggufs.Count > 6 ? $", and {ggufs.Count - 6} more)." : ").")
                    : "."),
                repo, OvmsPull.OvmsSourceFormatGguf, string.Empty, ggufs.Select(file => Path.GetFileName(file)).ToArray());
        if (original && imageModel)
            return new(DownloadRoute.Unknown, "Original image-generation model. Convert with OVMS Pull here is set up for chat models only.",
                string.Empty, string.Empty, string.Empty, []);
        if (original)
            return new(DownloadRoute.ConvertOriginal,
                $"Original Hugging Face model{kind}, not yet in OpenVINO format. Use Convert with OVMS Pull ({pipeline}); " +
                "it converts the model and creates the tokenizer files OVMS needs.",
                repo, OvmsPull.OvmsSourceFormatExport, pipeline, []);
        return new(DownloadRoute.Unknown, "No model files that OVMS recognizes were found. Download is still possible.",
            string.Empty, string.Empty, string.Empty, []);
    }

    private static string ReadString(JsonElement root, params string[] path)
    {
        JsonElement current = root;
        foreach (string name in path)
            if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(name, out current)) return string.Empty;
        return current.ValueKind == JsonValueKind.String ? current.GetString() ?? string.Empty : string.Empty;
    }

    // cardData.base_model is a string or a list; the first entry names the original model.
    private static string ReadBaseModel(JsonElement root)
    {
        if (!root.TryGetProperty("cardData", out var card) || card.ValueKind != JsonValueKind.Object
            || !card.TryGetProperty("base_model", out var value)) return string.Empty;
        if (value.ValueKind == JsonValueKind.String) return value.GetString() ?? string.Empty;
        if (value.ValueKind == JsonValueKind.Array)
            return value.EnumerateArray().Select(item => item.GetString() ?? string.Empty).FirstOrDefault(item => item.Length > 0) ?? string.Empty;
        return string.Empty;
    }

    public async Task DownloadAsync(IReadOnlyList<FileToDownload> files, Action<string, long, long?, TimeSpan> progress,
        Action<string> output, CancellationToken cancellationToken)
    {
        for (int index = 0; index < files.Count; index++)
        {
            var file = files[index];
            var result = await FileDownload.DownloadAsync(client, file.Address, file.Destination,
                $"{index + 1}/{files.Count} {Path.GetFileName(file.Destination)}", progress, cancellationToken);
            output($"{(result.Skipped ? "Already complete" : "Downloaded")}: {file.Destination}");
        }
    }

    private static string EscapePath(string path) => string.Join('/', path.Split('/').Select(Uri.EscapeDataString));
}
