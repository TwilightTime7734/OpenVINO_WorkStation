using static WorkStationC_.ModelFiles;

namespace WorkStationC_;

// Owns native models and serializes loading, generation, and shutdown.
internal sealed class ModelSession : IAsyncDisposable
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly ChatModel _chat = new();
    private LoadedModel? _loaded;

    internal sealed record LoadedModel(RuntimeModelSelection Source, string Device,
        ModelConfigClassification Capabilities, LocalModelApiModel ApiModel);

    public LoadedModel? Loaded => _loaded;

    public async Task LoadAsync(string path, string device, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(device)) throw new InvalidOperationException("Choose a model target first.");
        await _gate.WaitAsync(cancellationToken);
        try
        {
            await Task.Run(() =>
            {
                ReleaseModel();
                var source = Resolve(path, device);
                var capabilities = ClassifyModelRunPath(source.SelectedFolder, source.RuntimePath, source.Kind);
                OpenVinoRuntime.Configure();
                if (source.Kind is ModelFileKind.Gguf or ModelFileKind.OpenVinoGenAiDirectory)
                {
                    if (capabilities.RunPath is not (ModelRunPath.TextGeneration or ModelRunPath.VisionLanguage))
                        throw new InvalidOperationException("This model is not a supported chat model.");
                    _chat.Generate(source.RuntimePath, device, "Say hello.", null, null, cancellationToken,
                        capabilities.RunPath == ModelRunPath.VisionLanguage, maxNewTokens: 8);
                }
                else
                {
                    using OpenVinoSharp.Core core = new();
                    using var model = core.ReadModelUnicode(source.RuntimePath);
                }
                cancellationToken.ThrowIfCancellationRequested();
                _loaded = new(source, device, capabilities, ModelMetadata.Describe(source, capabilities));
            }, cancellationToken);
        }
        catch
        {
            ReleaseModel();
            throw;
        }
        finally { _gate.Release(); }
    }

    public LocalModelApiSnapshot Snapshot()
    {
        var model = _loaded;
        return new()
        {
            ModelLoaded = model is not null,
            LoadedModelName = model?.ApiModel.Name ?? string.Empty,
            Models = model is null ? [] : [model.ApiModel],
        };
    }

    public async Task<LocalModelApiTextResult> GenerateAsync(LocalModelApiTextRequest request, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var model = _loaded;
            if (model is null) return LocalModelApiTextResult.Error(409, "Load a model on Workstation first.", "model_not_loaded");
            if (!string.IsNullOrWhiteSpace(request.Model) && !request.Model.Equals(model.ApiModel.Name, StringComparison.OrdinalIgnoreCase))
                return LocalModelApiTextResult.Error(404, "The requested model is not loaded. Use the name from /v3/models or /v1/models.", "model_not_found");
            if (!model.ApiModel.SupportsChat)
                return LocalModelApiTextResult.Error(400, "The loaded model does not support chat.", "unsupported_model");
            if (request.ImageUrls.Count > 0 && !model.ApiModel.SupportsImageInput)
                return LocalModelApiTextResult.Error(400, "The loaded model cannot process images. Load a vision-language model first.", "unsupported_model");
            using var images = new ModelImages();
            try { await images.LoadAsync(request.ImageUrls, cancellationToken); }
            catch (Exception ex) when (ex is InvalidDataException or HttpRequestException)
            {
                return LocalModelApiTextResult.Error(400, ex.Message, "invalid_request_error");
            }
            var result = await Task.Run(() => _chat.Generate(model.Source.RuntimePath, model.Device,
                request.Prompt, null, null, cancellationToken, model.Capabilities.RunPath == ModelRunPath.VisionLanguage,
                (ulong)request.MaxTokens, request.EnableThinking, request.PreserveThinking, request, images.Tensors), cancellationToken);
            return LocalModelApiTextResult.Ok(result.GeneratedText, model.ApiModel.Name, result.LoadMilliseconds, result.GenerateMilliseconds,
                result.PromptTokens, result.CompletionTokens, result.FinishReason);
        }
        finally { _gate.Release(); }
    }

    internal static RuntimeModelSelection Resolve(string path, string device)
    {
        string fullPath = Path.GetFullPath(path);
        if (File.Exists(fullPath))
        {
            var kind = GetModelFileKind(fullPath);
            if (kind is not (ModelFileKind.OpenVinoIr or ModelFileKind.OpenVinoCoreReadable or ModelFileKind.Gguf))
                throw new InvalidOperationException("Select an OpenVINO or GGUF model file.");
            return new(Path.GetDirectoryName(fullPath)!, fullPath, kind);
        }
        if (!Directory.Exists(fullPath)) throw new DirectoryNotFoundException("The model folder does not exist.");
        string[] files = Directory.GetFiles(fullPath, "*", SearchOption.AllDirectories);
        string[] preferred = GetPreferredMainRuntimeFiles(fullPath, files);
        if (preferred.Length == 0) preferred = files;
        foreach (string directory in preferred.Select(file => Path.GetDirectoryName(file)!).Distinct().OrderBy(p => p.Length))
            if (IsOpenVinoGenAiDirectory(directory)) return new(fullPath, directory, ModelFileKind.OpenVinoGenAiDirectory);
        string? model = FindRuntimeFile(preferred, ModelFileKind.OpenVinoIr)
            ?? FindRuntimeFile(preferred, ModelFileKind.OpenVinoCoreReadable)
            ?? FindRuntimeFile(files, ModelFileKind.Gguf)
            ?? FindRuntimeFile(files, ModelFileKind.OpenVinoIr)
            ?? FindRuntimeFile(files, ModelFileKind.OpenVinoCoreReadable);
        if (model is null) throw new InvalidOperationException("No runnable model was found in that folder.");
        return new(fullPath, model, GetModelFileKind(model));
    }

    private void ReleaseModel()
    {
        _loaded = null;
        _chat.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _gate.WaitAsync();
        try { ReleaseModel(); }
        finally { _gate.Release(); }
    }
}
