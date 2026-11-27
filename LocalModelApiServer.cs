using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace WorkStationC_;

internal sealed class LocalModelApiServer : IDisposable
{
    private WebApplication? _application;
    private const int MaxHeaderSizeBytes = 65_536;
    internal const int MaxRequestBodyBytes = 33_554_432;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
    };

    private readonly Func<LocalModelApiSnapshot> _snapshotProvider;
    private readonly Func<LocalModelApiTextRequest, CancellationToken, Task<LocalModelApiTextResult>> _textGenerationHandler;
    private readonly Action<string> _log;

    public LocalModelApiServer(
        Func<LocalModelApiSnapshot> snapshotProvider,
        Func<LocalModelApiTextRequest, CancellationToken, Task<LocalModelApiTextResult>> textGenerationHandler,
        Action<string> log)
    {
        _snapshotProvider = snapshotProvider;
        _textGenerationHandler = textGenerationHandler;
        _log = log;
    }

    public bool IsRunning => _application is not null;

    public string BaseUrl { get; private set; } = string.Empty;

    public string ApiBaseUrl => IsRunning ? $"{BaseUrl}/v3" : string.Empty;

    public DateTimeOffset? StartedAtUtc { get; private set; }

    public void Start(IPAddress bindAddress, int port, string advertisedHost = "")
    {
        if (IsRunning) return;
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { ContentRootPath = AppContext.BaseDirectory });
        builder.Logging.ClearProviders();
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Listen(bindAddress, port);
            options.Limits.MaxRequestHeadersTotalSize = MaxHeaderSizeBytes;
            options.Limits.MaxRequestBodySize = MaxRequestBodyBytes;
        });
        WebApplication application = builder.Build();
        application.Run(HandleRequestAsync);
        try
        {
            application.StartAsync().GetAwaiter().GetResult();
            _application = application;
            string host = string.IsNullOrWhiteSpace(advertisedHost) ? FormatHostForUrl(bindAddress) : FormatHostForUrl(advertisedHost.Trim());
            BaseUrl = $"http://{host}:{port}";
            StartedAtUtc = DateTimeOffset.UtcNow;
        }
        catch
        {
            application.DisposeAsync().AsTask().GetAwaiter().GetResult();
            throw;
        }
    }

    private static string FormatHostForUrl(string host)
    {
        return IPAddress.TryParse(host, out IPAddress? address)
            ? FormatHostForUrl(address)
            : host;
    }

    private static string FormatHostForUrl(IPAddress address)
    {
        return address.AddressFamily == AddressFamily.InterNetworkV6
            ? $"[{address}]"
            : address.ToString();
    }

    public async Task StopAsync()
    {
        WebApplication? application = _application;
        _application = null;
        BaseUrl = string.Empty;
        StartedAtUtc = null;
        if (application is null) return;
        application.Lifetime.StopApplication();
        try
        {
            await application.StopAsync(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _log("Local server shutdown timed out.");
        }
        finally
        {
            await application.DisposeAsync().ConfigureAwait(false);
        }
    }

    public void Dispose()
    {
        // Start shutdown immediately without blocking the UI thread needed by active requests.
        _ = StopAsync();
    }

    private async Task HandleRequestAsync(HttpContext context)
    {
        context.Response.Headers.AccessControlAllowOrigin = "*";
        context.Response.Headers.AccessControlAllowMethods = "GET, POST, OPTIONS";
        context.Response.Headers.AccessControlAllowHeaders = "content-type, authorization, x-requested-with, openai-organization, openai-project";
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(
            context.RequestAborted, context.RequestServices.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping);
        try
        {
            using StreamReader reader = new(context.Request.Body, Encoding.UTF8);
            string body = await reader.ReadToEndAsync(cancellation.Token);
            await DispatchRouteAsync(context.Response, context.Request.Method,
                context.Request.Path.Value ?? "/", body, cancellation.Token);
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
        catch (Exception ex)
        {
            _log($"Local server request failed: {ex.Message}");
            if (context.Response.HasStarted) context.Abort();
            else await WriteOpenAiErrorAsync(context.Response,
                ex is BadHttpRequestException badRequest ? badRequest.StatusCode : 500,
                ex.Message, "server_error", context.RequestAborted);
        }
    }

    private async Task DispatchRouteAsync(
        HttpResponse response,
        string method,
        string path,
        string requestBody,
        CancellationToken serverCancellation)
    {
        // Use OVMS routes, with /v1 aliases for existing OpenAI clients.
        if (path.StartsWith("/v1/", StringComparison.Ordinal)) path = "/v3/" + path[4..];
        if (method == "OPTIONS")
        {
            await WriteResponseAsync(
                response,
                204,
                "application/json; charset=utf-8",
                Array.Empty<byte>(),
                serverCancellation);
            return;
        }

        if (method == "GET" && (path == "/" || path == "/health"))
        {
            await WriteJsonAsync(response, 200, BuildHealthPayload(), serverCancellation);
            return;
        }

        if (method == "GET" && (path == "/v2/health/live" || path == "/v2/health/ready"))
        {
            // Server readiness is independent of whether a model is loaded.
            await WriteResponseAsync(response, 200, "application/json", Array.Empty<byte>(), serverCancellation);
            return;
        }

        if (method == "GET" && path == "/v3/models")
        {
            await WriteJsonAsync(response, 200, BuildModelsPayload(), serverCancellation);
            return;
        }

        if (method == "GET" && path.StartsWith("/v3/models/", StringComparison.Ordinal))
        {
            string name = Uri.UnescapeDataString(path[11..]);
            var model = _snapshotProvider().Models.FirstOrDefault(model => model.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (model is null)
                await WriteOpenAiErrorAsync(response, 404, "The requested model is not loaded.", "model_not_found", serverCancellation);
            else
                await WriteJsonAsync(response, 200, new { id = model.Name, @object = "model", created = model.ModifiedAtUtc.ToUnixTimeSeconds(), owned_by = "OpenVINO Workbench" }, serverCancellation);
            return;
        }

        if (method == "POST" && path == "/v3/chat/completions")
        {
            await HandleTextGenerationAsync(
                response,
                requestBody,
                LocalModelApiTextResponseKind.Chat,
                serverCancellation);
            return;
        }

        if (method == "POST" && path == "/v3/completions")
        {
            await HandleTextGenerationAsync(
                response,
                requestBody,
                LocalModelApiTextResponseKind.Text,
                serverCancellation);
            return;
        }

        if (method == "POST" && (path == "/v3/images/generations" || path == "/v3/images/edits"))
        {
            await WriteOpenAiErrorAsync(
                response,
                501,
                "Image generation is not available through this local server.",
                "not_implemented",
                serverCancellation);
            return;
        }

        if (method is not ("GET" or "POST"))
        {
            await WriteOpenAiErrorAsync(
                response,
                405,
                "Method not allowed.",
                "invalid_request_error",
                serverCancellation);
            return;
        }

        await WriteOpenAiErrorAsync(
            response,
            404,
            $"Endpoint not found: {path}",
            "invalid_request_error",
            serverCancellation);
    }

    private async Task HandleTextGenerationAsync(
        HttpResponse response,
        string requestBody,
        LocalModelApiTextResponseKind responseKind,
        CancellationToken cancellationToken)
    {
        if (!TryParseTextRequest(
                requestBody,
                responseKind,
                out LocalModelApiTextRequest? request,
                out object? errorPayload))
        {
            await WriteJsonAsync(response, 400, errorPayload!, cancellationToken);
            return;
        }

        LocalModelApiTextRequest textRequest = request!;
        if (textRequest.Stream)
        {
            await WriteTextGenerationStreamAsync(response, responseKind, textRequest, cancellationToken);
            return;
        }

        LocalModelApiTextResult result = CompleteToolResponse(textRequest, await _textGenerationHandler(textRequest, cancellationToken));
        if (!result.Success)
        {
            await WriteOpenAiErrorAsync(
                response,
                result.StatusCode,
                result.ErrorMessage,
                string.IsNullOrWhiteSpace(result.ErrorType) ? "server_error" : result.ErrorType,
                cancellationToken);
            return;
        }

        object payload = responseKind == LocalModelApiTextResponseKind.Chat
            ? BuildChatCompletionPayload(textRequest, result)
            : BuildTextCompletionPayload(textRequest, result);
        await WriteJsonAsync(response, 200, payload, cancellationToken);
    }

    private static object BuildChatCompletionPayload(
        LocalModelApiTextRequest request,
        LocalModelApiTextResult result)
    {
        string model = string.IsNullOrWhiteSpace(result.ModelName) ? request.Model : result.ModelName;
        Dictionary<string, object?> message = new()
        {
            ["role"] = "assistant",
            ["content"] = result.Text,
        };
        if (result.ToolCalls.Count > 0)
        {
            message["tool_calls"] = result.ToolCalls.Select(call => call.ToPayload()).ToArray();
            if (string.IsNullOrWhiteSpace(result.Text)) message["content"] = null;
        }

        return new Dictionary<string, object?>
        {
            ["id"] = $"chatcmpl-{Guid.NewGuid():N}",
            ["object"] = "chat.completion",
            ["created"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ["model"] = model,
            ["choices"] = new object[]
            {
                new Dictionary<string, object?>
                {
                    ["index"] = 0,
                    ["message"] = message,
                    ["finish_reason"] = result.FinishReason,
                },
            },
            ["usage"] = new Dictionary<string, object?>
            {
                ["prompt_tokens"] = result.PromptTokens,
                ["completion_tokens"] = result.CompletionTokens,
                ["total_tokens"] = result.PromptTokens + result.CompletionTokens,
            },
            ["workstation"] = new Dictionary<string, object?>
            {
                ["load_ms"] = result.LoadMilliseconds,
                ["generate_ms"] = result.GenerateMilliseconds,
            },
        };
    }

    private static object BuildTextCompletionPayload(
        LocalModelApiTextRequest request,
        LocalModelApiTextResult result)
    {
        string model = string.IsNullOrWhiteSpace(result.ModelName) ? request.Model : result.ModelName;
        return new
        {
            id = $"cmpl-{Guid.NewGuid():N}",
            @object = "text_completion",
            created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            model,
            choices = new[]
            {
                new
                {
                    index = 0,
                    text = result.Text,
                    finish_reason = result.FinishReason,
                },
            },
            usage = new
            {
                prompt_tokens = result.PromptTokens,
                completion_tokens = result.CompletionTokens,
                total_tokens = result.PromptTokens + result.CompletionTokens,
            },
            workstation = new
            {
                load_ms = result.LoadMilliseconds,
                generate_ms = result.GenerateMilliseconds,
            },
        };
    }

    private async Task WriteTextGenerationStreamAsync(
        HttpResponse response,
        LocalModelApiTextResponseKind responseKind,
        LocalModelApiTextRequest request,
        CancellationToken cancellationToken)
    {
        string id = $"{(responseKind == LocalModelApiTextResponseKind.Chat ? "chatcmpl" : "cmpl")}-{Guid.NewGuid():N}";
        long created = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string model = string.IsNullOrWhiteSpace(request.Model) ? _snapshotProvider().LoadedModelName : request.Model;
        bool started = false;

        Dictionary<string, object?> Chunk(string text, string? finishReason, bool role = false, object? toolCalls = null)
        {
            Dictionary<string, object?> choice = new() { ["index"] = 0, ["finish_reason"] = finishReason };
            if (responseKind == LocalModelApiTextResponseKind.Chat)
                choice["delta"] = role ? new Dictionary<string, object?> { ["role"] = "assistant", ["content"] = "" }
                    : finishReason is null ? new Dictionary<string, object?> { ["content"] = text }
                    : new Dictionary<string, object?>();
            else
            {
                choice["text"] = text;
                choice["logprobs"] = null;
            }
            if (toolCalls is not null) choice["delta"] = new { tool_calls = toolCalls };
            Dictionary<string, object?> chunk = new()
            {
                ["id"] = id, ["created"] = created, ["model"] = model,
                ["object"] = responseKind == LocalModelApiTextResponseKind.Chat ? "chat.completion.chunk" : "text_completion",
                ["choices"] = new[] { choice },
            };
            if (request.IncludeUsage) chunk["usage"] = null;
            return chunk;
        }

        async ValueTask StartStreamAsync()
        {
            if (started) return;
            response.ContentType = "text/event-stream; charset=utf-8";
            response.Headers.CacheControl = "no-cache";
            response.Headers["X-Accel-Buffering"] = "no";
            response.HttpContext.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpResponseBodyFeature>()?.DisableBuffering();
            started = true;
            if (responseKind == LocalModelApiTextResponseKind.Chat)
                await WriteEventAsync(response, Chunk("", null, role: true), cancellationToken).ConfigureAwait(false);
        }

        request.OnTextChunk = async text =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Tool-enabled turns are parsed as a whole so partial JSON/XML is never exposed as a call.
            if (request.ToolCalling.Enabled) return;
            if (text.Length == 0) return;
            await StartStreamAsync().ConfigureAwait(false);
            await WriteEventAsync(response, Chunk(text, null), cancellationToken).ConfigureAwait(false);
        };
        try
        {
            // The handler awaits each write while generation is running, preserving order and
            // applying backpressure instead of accumulating the whole answer in an HTTP buffer.
            var result = CompleteToolResponse(request, await _textGenerationHandler(request, cancellationToken).ConfigureAwait(false));
            cancellationToken.ThrowIfCancellationRequested();
            if (!result.Success)
            {
                if (started) throw new InvalidOperationException(result.ErrorMessage);
                await WriteOpenAiErrorAsync(response, result.StatusCode, result.ErrorMessage,
                    string.IsNullOrWhiteSpace(result.ErrorType) ? "server_error" : result.ErrorType, cancellationToken).ConfigureAwait(false);
                return;
            }
            if (!started)
            {
                if (!string.IsNullOrWhiteSpace(result.ModelName)) model = result.ModelName;
                await StartStreamAsync().ConfigureAwait(false);
                if (result.Text.Length > 0)
                    await WriteEventAsync(response, Chunk(result.Text, null), cancellationToken).ConfigureAwait(false);
            }
            if (result.ToolCalls.Count > 0)
                await WriteEventAsync(response, Chunk("", null, toolCalls: result.ToolCalls.Select((call, index) => call.ToPayload(index)).ToArray()), cancellationToken).ConfigureAwait(false);
            await WriteEventAsync(response, Chunk("", result.FinishReason), cancellationToken).ConfigureAwait(false);
            if (request.IncludeUsage)
            {
                var usage = Chunk("", null);
                usage["choices"] = Array.Empty<object>();
                usage["usage"] = new { prompt_tokens = result.PromptTokens, completion_tokens = result.CompletionTokens,
                    total_tokens = result.PromptTokens + result.CompletionTokens };
                await WriteEventAsync(response, usage, cancellationToken).ConfigureAwait(false);
            }
            await response.WriteAsync("data: [DONE]\n\n", cancellationToken).ConfigureAwait(false);
            await response.Body.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        finally { request.OnTextChunk = null; }
    }

    private static async Task WriteEventAsync(HttpResponse response, object payload, CancellationToken cancellationToken)
    {
        await response.WriteAsync($"data: {JsonSerializer.Serialize(payload, JsonOptions)}\n\n", cancellationToken).ConfigureAwait(false);
        await response.Body.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    private static LocalModelApiTextResult CompleteToolResponse(LocalModelApiTextRequest request, LocalModelApiTextResult result)
    {
        if (!result.Success || !request.ToolCalling.Enabled) return result;
        try
        {
            var reply = request.ToolCalling.Parse(result.Text);
            return LocalModelApiTextResult.Ok(reply.Content, result.ModelName, result.LoadMilliseconds, result.GenerateMilliseconds,
                result.PromptTokens, result.CompletionTokens, reply.Calls.Count > 0 ? "tool_calls" : result.FinishReason, reply.Calls);
        }
        catch (Exception ex) when (ex is InvalidDataException or JsonException)
        {
            return LocalModelApiTextResult.Error(502, ex.Message, "tool_call_error");
        }
    }

    private static bool TryParseTextRequest(
        string requestBody,
        LocalModelApiTextResponseKind responseKind,
        out LocalModelApiTextRequest? request,
        out object? errorPayload)
    {
        request = null;
        errorPayload = null;
        if (string.IsNullOrWhiteSpace(requestBody))
        {
            errorPayload = BuildBadRequestError("Request body must be JSON.");
            return false;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(requestBody);
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                errorPayload = BuildBadRequestError("Request body must be a JSON object.");
                return false;
            }

            List<string> imageUrls = [];
            string prompt = responseKind == LocalModelApiTextResponseKind.Chat
                ? ReadChatPrompt(root, imageUrls)
                : ReadTextPrompt(root);
            if (string.IsNullOrWhiteSpace(prompt))
            {
                errorPayload = responseKind == LocalModelApiTextResponseKind.Chat
                    ? BuildBadRequestError("Chat request must include at least one message with text content.")
                    : BuildBadRequestError("Text request must include a prompt.");
                return false;
            }

            int requestedMaxTokens = TryReadFirstInt(root, "max_completion_tokens", "max_tokens") ?? 2048;
            double temperature = TryReadDouble(root, "temperature") ?? 0.2;
            if (requestedMaxTokens <= 0 || !double.IsFinite(temperature) || temperature < 0 || temperature > 2)
            {
                errorPayload = BuildBadRequestError("max_tokens must be positive and temperature must be between 0 and 2.");
                return false;
            }
            request = new LocalModelApiTextRequest
            {
                ResponseKind = responseKind,
                Model = TryReadString(root, "model"),
                Prompt = prompt,
                ImageUrls = imageUrls,
                Messages = responseKind == LocalModelApiTextResponseKind.Chat
                    ? root.GetProperty("messages").EnumerateArray().Where(m => m.ValueKind == JsonValueKind.Object)
                        .Select(m =>
                        {
                            var message = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(m.GetRawText())!;
                            message["content"] = JsonSerializer.SerializeToElement(ReadMessageContent(m));
                            return JsonSerializer.Serialize(message);
                        }).ToArray() : [],
                MaxTokens = requestedMaxTokens,
                Temperature = temperature,
                Stream = TryReadBool(root, "stream") ?? false,
                IncludeUsage = root.TryGetProperty("stream_options", out var options) && options.ValueKind == JsonValueKind.Object
                    && TryReadBool(options, "include_usage") == true,
                ResponseFormat = ReadResponseFormat(root),
                ToolCalling = responseKind == LocalModelApiTextResponseKind.Chat ? ToolCalling.FromRequest(root) : ToolCalling.Disabled,
                EnableThinking = ReadTemplateBool(root, "enable_thinking"),
                PreserveThinking = ReadTemplateBool(root, "preserve_thinking"),
            };
            return true;
        }
        catch (InvalidDataException ex)
        {
            errorPayload = BuildBadRequestError(ex.Message);
            return false;
        }
        catch (JsonException ex)
        {
            errorPayload = BuildBadRequestError($"Request body was not valid JSON: {ex.Message}");
            return false;
        }
    }

    private static object BuildBadRequestError(string message)
    {
        return new
        {
            error = new
            {
                message,
                type = "invalid_request_error",
                param = (string?)null,
                code = (string?)null,
            },
        };
    }

    private static bool ReadTemplateBool(JsonElement root, string name) =>
        (root.TryGetProperty("chat_template_kwargs", out var kwargs) && kwargs.ValueKind == JsonValueKind.Object
            ? TryReadBool(kwargs, name) : null) ?? TryReadBool(root, name) ?? false;

    private static string ReadChatPrompt(JsonElement root, List<string> imageUrls)
    {
        if (!root.TryGetProperty("messages", out JsonElement messages)
            || messages.ValueKind != JsonValueKind.Array
            || messages.GetArrayLength() == 0)
        {
            return string.Empty;
        }

        List<string> lines = new();
        bool singleMessage = messages.GetArrayLength() == 1;

        foreach (JsonElement message in messages.EnumerateArray())
        {
            if (message.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            string? formatted = FormatChatMessageLine(message, singleMessage, imageUrls);
            if (!string.IsNullOrWhiteSpace(formatted))
            {
                lines.Add(formatted);
            }
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string? FormatChatMessageLine(JsonElement message, bool isSingleMessage, List<string> imageUrls)
    {
        string content = ReadMessageContent(message, imageUrls);
        string role = TryReadString(message, "role");
        string toolCalls = ReadMessageToolCalls(message);
        string toolCallId = TryReadString(message, "tool_call_id");

        if (string.IsNullOrWhiteSpace(content) && string.IsNullOrWhiteSpace(toolCalls))
        {
            return null;
        }

        if (string.Equals(role, "tool", StringComparison.OrdinalIgnoreCase))
        {
            string label = string.IsNullOrWhiteSpace(toolCallId)
                ? "tool result"
                : $"tool result {toolCallId}";
            return $"{label}: <tool_response>\n{content}\n</tool_response>";
        }

        if (string.IsNullOrWhiteSpace(role) || isSingleMessage)
        {
            return content.Trim();
        }

        string line = string.IsNullOrWhiteSpace(content)
            ? $"{role.Trim()}:"
            : $"{role.Trim()}: {content.Trim()}";

        return string.IsNullOrWhiteSpace(toolCalls)
            ? line
            : $"{line}{Environment.NewLine}{toolCalls}";
    }

    private static string ReadTextPrompt(JsonElement root)
    {
        if (!root.TryGetProperty("prompt", out JsonElement prompt))
        {
            return string.Empty;
        }

        if (prompt.ValueKind == JsonValueKind.String)
        {
            return prompt.GetString() ?? string.Empty;
        }

        if (prompt.ValueKind == JsonValueKind.Array)
        {
            return string.Join(
                Environment.NewLine,
                prompt.EnumerateArray()
                    .Where(item => item.ValueKind == JsonValueKind.String)
                    .Select(item => item.GetString() ?? string.Empty)
                    .Where(text => !string.IsNullOrWhiteSpace(text)));
        }

        return string.Empty;
    }

    private static string ReadMessageContent(JsonElement message, List<string>? imageUrls = null)
    {
        if (!message.TryGetProperty("content", out JsonElement content))
        {
            return string.Empty;
        }

        if (content.ValueKind == JsonValueKind.String)
        {
            return content.GetString() ?? string.Empty;
        }

        if (content.ValueKind != JsonValueKind.Array)
        {
            return string.Empty;
        }

        List<string> parts = new();
        foreach (JsonElement part in content.EnumerateArray())
        {
            if (part.ValueKind == JsonValueKind.Object && TryReadString(part, "type") == "image_url" && imageUrls is not null)
            {
                if (!part.TryGetProperty("image_url", out var image) || image.ValueKind != JsonValueKind.Object
                    || string.IsNullOrWhiteSpace(TryReadString(image, "url")))
                    throw new InvalidDataException("Each image_url must contain a nonempty url.");
                if (imageUrls.Count >= ModelImages.MaxImages)
                    throw new InvalidDataException($"A request supports at most {ModelImages.MaxImages} images.");
                parts.Add($"<ov_genai_image_{imageUrls.Count}>");
                imageUrls.Add(TryReadString(image, "url"));
                continue;
            }
            if (part.ValueKind == JsonValueKind.String)
            {
                parts.Add(part.GetString() ?? string.Empty);
                continue;
            }

            if (part.ValueKind == JsonValueKind.Object
                && part.TryGetProperty("text", out JsonElement text)
                && text.ValueKind == JsonValueKind.String)
            {
                parts.Add(text.GetString() ?? string.Empty);
            }
        }

        return string.Join(
            Environment.NewLine,
            parts.Where(part => !string.IsNullOrWhiteSpace(part)));
    }

    private static string ReadMessageToolCalls(JsonElement message)
    {
        if (!message.TryGetProperty("tool_calls", out JsonElement toolCalls)
            || toolCalls.ValueKind != JsonValueKind.Array
            || toolCalls.GetArrayLength() == 0)
        {
            return string.Empty;
        }

        List<string> lines = new();
        foreach (JsonElement toolCall in toolCalls.EnumerateArray())
        {
            if (toolCall.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            string id = TryReadString(toolCall, "id");
            string name = string.Empty;
            string arguments = "{}";
            if (toolCall.TryGetProperty("function", out JsonElement function)
                && function.ValueKind == JsonValueKind.Object)
            {
                name = TryReadString(function, "name");
                arguments = ReadJsonStringOrRaw(function, "arguments", "{}");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                name = TryReadFirstString(toolCall, "name", "tool", "tool_name");
            }

            if (arguments == "{}")
            {
                arguments = ReadJsonStringOrRaw(toolCall, "arguments", "{}");
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                string label = string.IsNullOrWhiteSpace(id)
                    ? "assistant tool call"
                    : $"assistant tool call {id}";
                using var argumentDocument = JsonDocument.Parse(arguments);
                string call = JsonSerializer.Serialize(new { name, arguments = argumentDocument.RootElement });
                lines.Add($"{label}: <tool_call>{call}</tool_call>");
            }
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string ReadResponseFormat(JsonElement root)
    {
        if (!root.TryGetProperty("response_format", out JsonElement responseFormat))
        {
            return string.Empty;
        }

        return responseFormat.ValueKind == JsonValueKind.String
            ? responseFormat.GetString() ?? string.Empty
            : responseFormat.GetRawText();
    }

    private static string TryReadString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out JsonElement property)
            && property.ValueKind == JsonValueKind.String
                ? property.GetString() ?? string.Empty
                : string.Empty;
    }

    private static string TryReadFirstString(JsonElement element, params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            string value = TryReadString(element, propertyName);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return string.Empty;
    }

    private static string ReadJsonStringOrRaw(
        JsonElement element,
        string propertyName,
        string fallback)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement property))
        {
            return fallback;
        }

        return property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? fallback
            : property.GetRawText();
    }

    private static int? TryReadInt(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement property))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.Number when property.TryGetInt32(out int value) => value,
            JsonValueKind.String when int.TryParse(
                property.GetString(),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int value) => value,
            _ => null,
        };
    }

    private static int? TryReadFirstInt(JsonElement element, params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            int? value = TryReadInt(element, propertyName);
            if (value.HasValue)
            {
                return value;
            }
        }

        return null;
    }

    private static double? TryReadDouble(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement property))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.Number when property.TryGetDouble(out double value) => value,
            JsonValueKind.String when double.TryParse(
                property.GetString(),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double value) => value,
            _ => null,
        };
    }

    private static bool? TryReadBool(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement property))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String when bool.TryParse(property.GetString(), out bool value) => value,
            _ => null,
        };
    }

    private object BuildHealthPayload()
    {
        LocalModelApiSnapshot snapshot = _snapshotProvider();
        return new
        {
            status = "running",
            server = "OpenVINO Workbench",
            protocol = "OpenAI-compatible",
            local_only = true,
            base_url = BaseUrl,
            api_base_url = ApiBaseUrl,
            started_at = StartedAtUtc.HasValue
                ? StartedAtUtc.Value.UtcDateTime.ToString("O", CultureInfo.InvariantCulture)
                : string.Empty,
            loaded_model = snapshot.LoadedModelName,
            model_loaded = snapshot.ModelLoaded,
            endpoints = new[]
            {
                "/v2/health/live",
                "/v2/health/ready",
                "/v3/models",
                "/v3/models/{model}",
                "/v3/chat/completions",
                "/v3/completions",
            },
            note = "Chat and text endpoints stream live output from the loaded model. Chat accepts image attachments when a vision model is loaded. Image generation is not wired yet.",
        };
    }

    private object BuildModelsPayload()
    {
        LocalModelApiSnapshot snapshot = _snapshotProvider();
        return new
        {
            @object = "list",
            data = snapshot.Models.Select(model => new
            {
                id = model.Name,
                @object = "model",
                created = model.ModifiedAtUtc.ToUnixTimeSeconds(),
                owned_by = "OpenVINO Workbench",
                max_model_len = model.ContextWindowTokens,
                context_length = model.ContextWindowTokens,
                context_window = model.ContextWindowTokens,
                meta = new
                {
                    n_ctx = model.ContextWindowTokens,
                    n_ctx_train = model.ContextWindowTokens,
                },
                workstation = new
                {
                    source = model.Source,
                    kind = model.Kind,
                    format = model.Format,
                    family = model.Family,
                    quantization_level = model.QuantizationLevel,
                    loaded = model.Loaded,
                    path = model.RuntimePath,
                    size = model.SizeBytes,
                    digest = model.Digest,
                    supports_chat = model.SupportsChat,
                    supports_image_input = model.SupportsImageInput,
                },
            }).ToArray(),
        };
    }

    private static async Task WriteOpenAiErrorAsync(
        HttpResponse response,
        int statusCode,
        string message,
        string type,
        CancellationToken cancellationToken)
    {
        await WriteJsonAsync(
            response,
            statusCode,
            new
            {
                error = new
                {
                    message,
                    type,
                    param = (string?)null,
                    code = (string?)null,
                },
            },
            cancellationToken);
    }

    private static async Task WriteJsonAsync(
        HttpResponse response,
        int statusCode,
        object payload,
        CancellationToken cancellationToken)
    {
        byte[] body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload, JsonOptions));
        await WriteResponseAsync(
            response,
            statusCode,
            "application/json; charset=utf-8",
            body,
            cancellationToken);
    }

    private static async Task WriteResponseAsync(HttpResponse response, int statusCode,
        string contentType, byte[] body, CancellationToken cancellationToken)
    {
        response.StatusCode = statusCode;
        response.ContentType = contentType;
        response.ContentLength = body.Length;
        if (body.Length > 0) await response.Body.WriteAsync(body, cancellationToken);
    }

    public static string CreateDigest(string source, long sizeBytes, DateTimeOffset modifiedAtUtc)
    {
        string text = $"{source}|{sizeBytes.ToString(CultureInfo.InvariantCulture)}|{modifiedAtUtc.UtcTicks.ToString(CultureInfo.InvariantCulture)}";
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return System.Convert.ToHexString(hash).ToLowerInvariant();
    }

}

internal sealed class LocalModelApiSnapshot
{
    public bool ModelLoaded { get; init; }
    public string LoadedModelName { get; init; } = string.Empty;
    public IReadOnlyList<LocalModelApiModel> Models { get; init; } = Array.Empty<LocalModelApiModel>();
}

internal sealed class LocalModelApiModel
{
    public required string Name { get; init; }
    public required string Source { get; init; }
    public required string RuntimePath { get; init; }
    public string Kind { get; init; } = string.Empty;
    public string Format { get; init; } = string.Empty;
    public string Family { get; init; } = string.Empty;
    public string QuantizationLevel { get; init; } = string.Empty;
    public int ContextWindowTokens { get; init; }
    public bool SupportsChat { get; init; }
    public bool SupportsImageInput { get; init; }
    public bool Loaded { get; init; }
    public DateTimeOffset ModifiedAtUtc { get; init; }
    public long SizeBytes { get; init; }
    public string Digest { get; init; } = string.Empty;
}

internal enum LocalModelApiTextResponseKind
{
    Chat,
    Text,
}

internal sealed class LocalModelApiTextRequest
{
    public LocalModelApiTextResponseKind ResponseKind { get; init; }
    public string Model { get; init; } = string.Empty;
    public string Prompt { get; init; } = string.Empty;
    public IReadOnlyList<string> Messages { get; init; } = [];
    public IReadOnlyList<string> ImageUrls { get; init; } = [];
    public int MaxTokens { get; init; } = 64;
    public double Temperature { get; init; }
    public bool Stream { get; init; }
    public bool IncludeUsage { get; init; }
    public ToolCalling ToolCalling { get; init; } = ToolCalling.Disabled;
    internal Func<string, ValueTask>? OnTextChunk { get; set; }
    public string ResponseFormat { get; init; } = string.Empty;
    public bool EnableThinking { get; init; }
    public bool PreserveThinking { get; init; }
}

internal sealed class LocalModelApiTextResult
{
    public bool Success { get; init; }
    public int StatusCode { get; init; } = 200;
    public string ErrorMessage { get; init; } = string.Empty;
    public string ErrorType { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public string ModelName { get; init; } = string.Empty;
    public long LoadMilliseconds { get; init; }
    public long GenerateMilliseconds { get; init; }
    public ulong PromptTokens { get; init; }
    public ulong CompletionTokens { get; init; }
    public string FinishReason { get; init; } = "stop";
    public IReadOnlyList<ModelToolCall> ToolCalls { get; init; } = [];

    public static LocalModelApiTextResult Ok(
        string text,
        string modelName,
        long loadMilliseconds,
        long generateMilliseconds,
        ulong promptTokens = 0,
        ulong completionTokens = 0,
        string finishReason = "stop",
        IReadOnlyList<ModelToolCall>? toolCalls = null)
    {
        return new LocalModelApiTextResult
        {
            Success = true,
            Text = text,
            ModelName = modelName,
            LoadMilliseconds = loadMilliseconds,
            GenerateMilliseconds = generateMilliseconds,
            PromptTokens = promptTokens,
            CompletionTokens = completionTokens,
            FinishReason = finishReason,
            ToolCalls = toolCalls ?? [],
        };
    }

    public static LocalModelApiTextResult Error(int statusCode, string message, string type)
    {
        return new LocalModelApiTextResult
        {
            Success = false,
            StatusCode = statusCode,
            ErrorMessage = message,
            ErrorType = type,
        };
    }
}
