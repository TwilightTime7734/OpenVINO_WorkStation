using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.Json;
using OpenVinoSharp.GenAI;
using static WorkStationC_.WorkstationInfo;

namespace WorkStationC_;

internal sealed record GenerationResult(long LoadMilliseconds, long GenerateMilliseconds, string GeneratedText,
    ulong PromptTokens, ulong CompletionTokens, string FinishReason);

// Owns the one loaded chat model. Callers serialize operations before using it.
internal sealed class ChatModel : IDisposable
{
    private LLMPipeline? _textModel;
    private VLMPipeline? _visionModel;
    private string _modelPath = string.Empty;
    private string _deviceKey = string.Empty;

    public bool IsLoaded => _textModel is not null || _visionModel is not null;

    public GenerationResult Generate(
        string modelPath, string device, string prompt,
        IProgress<string>? progress, IProgress<string>? streamedText,
        CancellationToken cancellationToken, bool useVisionPipeline,
        ulong maxNewTokens = 64, bool enableThinking = false, bool preserveThinking = false,
        LocalModelApiTextRequest? request = null, OpenVinoSharp.Tensor[]? images = null)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var settings = BuildGenAiDeviceSettings(device);
        bool reuse = IsLoaded
            && _modelPath.Equals(modelPath, StringComparison.OrdinalIgnoreCase)
            && _deviceKey.Equals(settings.CacheKey, StringComparison.OrdinalIgnoreCase)
            && useVisionPipeline == (_visionModel is not null);

        Stopwatch watch = Stopwatch.StartNew();
        if (!reuse)
        {
            Dispose();
            var properties = new Dictionary<string, string>(settings.Properties);
            if (useVisionPipeline)
                _visionModel = new VLMPipeline(modelPath, settings.PipelineDevice, properties);
            else
            {
                _textModel = new LLMPipeline(modelPath, settings.PipelineDevice, properties);
                _textModel.StartChat();
            }
            _modelPath = modelPath;
            _deviceKey = settings.CacheKey;
        }

        long loadMilliseconds = reuse ? 0 : watch.ElapsedMilliseconds;
        cancellationToken.ThrowIfCancellationRequested();
        progress?.Report("Model ready. Generating response...");
        using GenerationConfig config = new();
        config.SetMaxNewTokens(maxNewTokens);
        double temperature = request?.Temperature ?? 0;
        config.SetDoSample(temperature > 0);
        if (temperature > 0) config.SetTemperature((float)temperature);

        StringBuilder text = new();
        ExceptionDispatchInfo? streamingError = null;
        StreamingStatus OnText(string chunk)
        {
            if (cancellationToken.IsCancellationRequested)
                return StreamingStatus.Cancel;
            try
            {
                text.Append(chunk);
                streamedText?.Report(chunk);
                // GenAI invokes this synchronously on the generation worker. Await the HTTP
                // flush here so chunks stay ordered and a slow reader cannot fill an unbounded queue.
                if (request?.OnTextChunk is { } writeChunk)
                    writeChunk(chunk).AsTask().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                streamingError = ExceptionDispatchInfo.Capture(ex);
                return StreamingStatus.Cancel;
            }
            return StreamingStatus.Running;
        }

        watch.Restart();
        string resultText;
        ulong promptTokens, completionTokens;
        if (useVisionPipeline)
        {
            // This C runtime supports VLM Generate, but not GenerateWithHistory.
            using VLMDecodedResults result = _visionModel!.Generate(request?.ToolCalling.VisionPrompt(prompt) ?? prompt, images, config, OnText);
            resultText = result.GetText();
            using var metrics = result.GetPerformanceMetrics();
            promptTokens = metrics.NumInputTokens;
            completionTokens = metrics.NumGenerationTokens;
        }
        else if (request?.ResponseKind == LocalModelApiTextResponseKind.Text)
        {
            _textModel!.FinishChat();
            using DecodedResults result = _textModel.Generate(prompt, config, OnText);
            resultText = result.GetText();
            using var metrics = result.GetPerformanceMetrics();
            promptTokens = metrics.NumInputTokens;
            completionTokens = metrics.NumGenerationTokens;
        }
        else
        {
            _textModel!.FinishChat();
            _textModel.StartChat();
            using ChatHistory history = new();
            if (request?.ToolCalling.Enabled == true)
            {
                using var tools = JsonContainer.FromJsonString(JsonSerializer.Serialize(request.ToolCalling.Tools));
                history.SetTools(tools);
                if (request.ToolCalling.Instructions.Length > 0)
                    history.AddMessage("system", request.ToolCalling.Instructions);
            }
            if (request?.Messages.Count > 0)
                foreach (string message in request.Messages) history.PushBackJson(message);
            else
                history.AddUserMessage(prompt);
            using JsonContainer? context = modelPath.Contains("qwen", StringComparison.OrdinalIgnoreCase)
                ? JsonContainer.FromJsonString(JsonSerializer.Serialize(new
                {
                    enable_thinking = enableThinking,
                    preserve_thinking = preserveThinking,
                })) : null;
            if (context is not null)
                history.SetExtraContext(context);
            using DecodedResults result = _textModel.GenerateWithHistory(history, config, OnText);
            resultText = result.GetText();
            using var metrics = result.GetPerformanceMetrics();
            promptTokens = metrics.NumInputTokens;
            completionTokens = metrics.NumGenerationTokens;
        }

        watch.Stop();
        streamingError?.Throw();
        cancellationToken.ThrowIfCancellationRequested();
        return new GenerationResult(loadMilliseconds, watch.ElapsedMilliseconds,
            request is null ? NormalizeAnswer(text.Length > 0 ? text.ToString() : resultText)
                : text.Length > 0 ? text.ToString() : resultText,
            promptTokens, completionTokens, completionTokens >= maxNewTokens ? "length" : "stop");
    }

    private static string NormalizeAnswer(string text)
    {
        int endThinking = text.LastIndexOf("</think>", StringComparison.OrdinalIgnoreCase);
        return (endThinking >= 0 ? text[(endThinking + "</think>".Length)..] : text).Trim();
    }

    public void Dispose()
    {
        _textModel?.Dispose();
        _visionModel?.Dispose();
        _textModel = null;
        _visionModel = null;
        _modelPath = string.Empty;
        _deviceKey = string.Empty;
    }
}
