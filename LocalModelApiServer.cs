using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace WorkStationC_;

internal sealed class LocalModelApiServer : IDisposable
{
    private const int MaxHeaderSizeBytes = 65_536;
    private const int MaxRequestBodyBytes = 4_194_304;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
    };

    private readonly Func<LocalModelApiSnapshot> _snapshotProvider;
    private readonly Func<LocalModelApiTextRequest, CancellationToken, Task<LocalModelApiTextResult>> _textGenerationHandler;
    private readonly Action<string> _log;
    private readonly object _stateLock = new();
    private TcpListener? _listener;
    private CancellationTokenSource? _cancellation;
    private Task? _listenerTask;

    public LocalModelApiServer(
        Func<LocalModelApiSnapshot> snapshotProvider,
        Func<LocalModelApiTextRequest, CancellationToken, Task<LocalModelApiTextResult>> textGenerationHandler,
        Action<string> log)
    {
        _snapshotProvider = snapshotProvider;
        _textGenerationHandler = textGenerationHandler;
        _log = log;
    }

    public bool IsRunning
    {
        get
        {
            lock (_stateLock)
            {
                return _listener is not null;
            }
        }
    }

    public string BaseUrl { get; private set; } = string.Empty;

    public DateTimeOffset? StartedAtUtc { get; private set; }

    public void Start(IPAddress bindAddress, int port, string advertisedHost = "")
    {
        lock (_stateLock)
        {
            if (_listener is not null)
            {
                return;
            }

            TcpListener listener = new(bindAddress, port);
            listener.Start();

            CancellationTokenSource cancellation = new();
            _listener = listener;
            _cancellation = cancellation;
            string hostForUrl = string.IsNullOrWhiteSpace(advertisedHost)
                ? FormatHostForUrl(bindAddress)
                : FormatHostForUrl(advertisedHost.Trim());
            BaseUrl = $"http://{hostForUrl}:{port.ToString(CultureInfo.InvariantCulture)}";
            StartedAtUtc = DateTimeOffset.UtcNow;
            _listenerTask = Task.Run(() => ListenAsync(listener, cancellation.Token));
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
        TcpListener? listener;
        CancellationTokenSource? cancellation;
        Task? listenerTask;

        lock (_stateLock)
        {
            listener = _listener;
            cancellation = _cancellation;
            listenerTask = _listenerTask;
            _listener = null;
            _cancellation = null;
            _listenerTask = null;
        }

        if (listener is null)
        {
            BaseUrl = string.Empty;
            StartedAtUtc = null;
            return;
        }

        cancellation?.Cancel();
        listener.Stop();

        if (listenerTask is not null)
        {
            try
            {
                await listenerTask.WaitAsync(TimeSpan.FromSeconds(2));
            }
            catch (OperationCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            catch (SocketException)
            {
            }
            catch (TimeoutException)
            {
                _log("OpenAI-compatible local server did not stop within 2 seconds.");
            }
        }

        cancellation?.Dispose();
        BaseUrl = string.Empty;
        StartedAtUtc = null;
    }

    public void Dispose()
    {
        TcpListener? listener;
        CancellationTokenSource? cancellation;

        lock (_stateLock)
        {
            listener = _listener;
            cancellation = _cancellation;
            _listener = null;
            _cancellation = null;
            _listenerTask = null;
        }

        cancellation?.Cancel();
        listener?.Stop();
        cancellation?.Dispose();
    }

    private async Task ListenAsync(TcpListener listener, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient client = await listener.AcceptTcpClientAsync(cancellationToken);
                _ = Task.Run(() => HandleClientSafelyAsync(client, cancellationToken), CancellationToken.None);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
        catch (SocketException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _log($"OpenAI-compatible local server stopped because of an error: {ex.Message}");
        }
    }

    private async Task HandleClientSafelyAsync(TcpClient client, CancellationToken serverCancellation)
    {
        using (client)
        {
            try
            {
                await HandleClientAsync(client, serverCancellation);
            }
            catch (OperationCanceledException)
            {
            }
            catch (IOException ex)
            {
                _log($"OpenAI-compatible local server request failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                _log($"OpenAI-compatible local server request failed: {ex.Message}");
            }
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken serverCancellation)
    {
        client.NoDelay = true;
        await using NetworkStream stream = client.GetStream();

        HttpRequestReadResult readResult = await ReadHttpRequestAsync(stream, serverCancellation);
        if (!readResult.Success)
        {
            await WriteOpenAiErrorAsync(
                stream,
                readResult.StatusCode,
                readResult.ErrorMessage,
                readResult.ErrorType,
                serverCancellation);
            return;
        }

        HttpRequestData request = readResult.Request!;
        await DispatchRouteAsync(
            stream,
            request.Method,
            request.Path,
            request.Body,
            serverCancellation);
    }

    private async Task DispatchRouteAsync(
        NetworkStream stream,
        string method,
        string path,
        string requestBody,
        CancellationToken serverCancellation)
    {
        if (method == "OPTIONS")
        {
            await WriteResponseAsync(
                stream,
                204,
                "No Content",
                "application/json; charset=utf-8",
                Array.Empty<byte>(),
                serverCancellation);
            return;
        }

        if (method == "GET" && (path == "/" || path == "/health"))
        {
            await WriteGeneratedJsonAsync(stream, BuildHealthPayload, serverCancellation);
            return;
        }

        if (method == "GET" && path == "/v1/models")
        {
            await WriteGeneratedJsonAsync(stream, BuildV1ModelsPayload, serverCancellation);
            return;
        }

        if (method == "POST" && path == "/v1/chat/completions")
        {
            await HandleTextGenerationAsync(
                stream,
                requestBody,
                LocalModelApiTextResponseKind.Chat,
                serverCancellation);
            return;
        }

        if (method == "POST" && path == "/v1/completions")
        {
            await HandleTextGenerationAsync(
                stream,
                requestBody,
                LocalModelApiTextResponseKind.Text,
                serverCancellation);
            return;
        }

        if (method == "POST" && (path == "/v1/images/generations" || path == "/v1/images/edits"))
        {
            await WriteOpenAiErrorAsync(
                stream,
                501,
                "Image generation over this local server is not implemented yet. Use Run Image Test in the app for loaded image-capable models.",
                "not_implemented",
                serverCancellation);
            return;
        }

        if (method is not ("GET" or "POST"))
        {
            await WriteOpenAiErrorAsync(
                stream,
                405,
                "Method not allowed.",
                "invalid_request_error",
                serverCancellation);
            return;
        }

        await WriteOpenAiErrorAsync(
            stream,
            404,
            $"Endpoint not found: {path}",
            "invalid_request_error",
            serverCancellation);
    }

    private async Task HandleTextGenerationAsync(
        NetworkStream stream,
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
            await WriteJsonAsync(stream, 400, errorPayload!, cancellationToken);
            return;
        }

        LocalModelApiTextRequest textRequest = request!;
        LocalModelApiTextResult result = await _textGenerationHandler(textRequest, cancellationToken);
        if (!result.Success)
        {
            await WriteOpenAiErrorAsync(
                stream,
                result.StatusCode,
                result.ErrorMessage,
                string.IsNullOrWhiteSpace(result.ErrorType) ? "server_error" : result.ErrorType,
                cancellationToken);
            return;
        }

        if (textRequest.Stream)
        {
            await WriteTextGenerationStreamAsync(
                stream,
                responseKind,
                textRequest,
                result,
                cancellationToken);
            return;
        }

        object payload = responseKind == LocalModelApiTextResponseKind.Chat
            ? BuildChatCompletionPayload(textRequest, result)
            : BuildTextCompletionPayload(textRequest, result);
        await WriteJsonAsync(stream, 200, payload, cancellationToken);
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
                    ["finish_reason"] = "stop",
                },
            },
            ["usage"] = new Dictionary<string, object?>
            {
                ["prompt_tokens"] = 0,
                ["completion_tokens"] = 0,
                ["total_tokens"] = 0,
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
                    finish_reason = "stop",
                },
            },
            usage = new
            {
                prompt_tokens = 0,
                completion_tokens = 0,
                total_tokens = 0,
            },
            workstation = new
            {
                load_ms = result.LoadMilliseconds,
                generate_ms = result.GenerateMilliseconds,
            },
        };
    }

    private static async Task WriteTextGenerationStreamAsync(
        NetworkStream stream,
        LocalModelApiTextResponseKind responseKind,
        LocalModelApiTextRequest request,
        LocalModelApiTextResult result,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<object> payloads = responseKind == LocalModelApiTextResponseKind.Chat
            ? BuildChatStreamPayloads(request, result)
            : BuildTextStreamPayloads(request, result);
        await WriteEventStreamAsync(stream, payloads, cancellationToken);
    }

    private static IReadOnlyList<object> BuildChatStreamPayloads(
        LocalModelApiTextRequest request,
        LocalModelApiTextResult result)
    {
        string id = $"chatcmpl-{Guid.NewGuid():N}";
        long created = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string model = string.IsNullOrWhiteSpace(result.ModelName) ? request.Model : result.ModelName;
        List<object> payloads = new();

        payloads.Add(BuildChatChunk(
            id,
            created,
            model,
            new Dictionary<string, object?> { ["role"] = "assistant" },
            null));

        if (!string.IsNullOrEmpty(result.Text))
        {
            payloads.Add(BuildChatChunk(
                id,
                created,
                model,
                new Dictionary<string, object?> { ["content"] = result.Text },
                null));
        }

        payloads.Add(BuildChatChunk(
            id,
            created,
            model,
            new Dictionary<string, object?>(),
            "stop"));
        return payloads;
    }

    private static object BuildChatChunk(
        string id,
        long created,
        string model,
        Dictionary<string, object?> delta,
        string? finishReason)
    {
        return new Dictionary<string, object?>
        {
            ["id"] = id,
            ["object"] = "chat.completion.chunk",
            ["created"] = created,
            ["model"] = model,
            ["choices"] = new object[]
            {
                new Dictionary<string, object?>
                {
                    ["index"] = 0,
                    ["delta"] = delta,
                    ["finish_reason"] = finishReason,
                },
            },
        };
    }

    private static IReadOnlyList<object> BuildTextStreamPayloads(
        LocalModelApiTextRequest request,
        LocalModelApiTextResult result)
    {
        string id = $"cmpl-{Guid.NewGuid():N}";
        long created = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string model = string.IsNullOrWhiteSpace(result.ModelName) ? request.Model : result.ModelName;

        return new object[]
        {
            new Dictionary<string, object?>
            {
                ["id"] = id,
                ["object"] = "text_completion",
                ["created"] = created,
                ["model"] = model,
                ["choices"] = new object[]
                {
                    new Dictionary<string, object?>
                    {
                        ["index"] = 0,
                        ["text"] = result.Text,
                        ["finish_reason"] = null,
                    },
                },
            },
            new Dictionary<string, object?>
            {
                ["id"] = id,
                ["object"] = "text_completion",
                ["created"] = created,
                ["model"] = model,
                ["choices"] = new object[]
                {
                    new Dictionary<string, object?>
                    {
                        ["index"] = 0,
                        ["text"] = string.Empty,
                        ["finish_reason"] = "stop",
                    },
                },
            },
        };
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

            string prompt = responseKind == LocalModelApiTextResponseKind.Chat
                ? ReadChatPrompt(root)
                : ReadTextPrompt(root);
            if (string.IsNullOrWhiteSpace(prompt))
            {
                errorPayload = responseKind == LocalModelApiTextResponseKind.Chat
                    ? BuildBadRequestError("Chat request must include at least one message with text content.")
                    : BuildBadRequestError("Text request must include a prompt.");
                return false;
            }

            int requestedMaxTokens = TryReadFirstInt(root, "max_completion_tokens", "max_tokens") ?? 64;
            request = new LocalModelApiTextRequest
            {
                ResponseKind = responseKind,
                Model = TryReadString(root, "model"),
                Prompt = prompt,
                MaxTokens = requestedMaxTokens > 0 ? Math.Min(requestedMaxTokens, 2048) : 64,
                Temperature = TryReadDouble(root, "temperature") ?? 0.2,
                Stream = TryReadBool(root, "stream") ?? false,
                ResponseFormat = ReadResponseFormat(root),
                EnableThinking = TryReadBool(root, "enable_thinking") ?? false,
                PreserveThinking = TryReadBool(root, "preserve_thinking") ?? false,
            };
            return true;
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

    private static string ReadChatPrompt(JsonElement root)
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

            string? formatted = FormatChatMessageLine(message, singleMessage);
            if (!string.IsNullOrWhiteSpace(formatted))
            {
                lines.Add(formatted);
            }
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string? FormatChatMessageLine(JsonElement message, bool isSingleMessage)
    {
        string content = ReadMessageContent(message);
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
            return $"{label}: {content.Trim()}";
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

    private static string ReadMessageContent(JsonElement message)
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
                lines.Add($"{label}: {name} {arguments}");
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
            started_at = StartedAtUtc.HasValue
                ? StartedAtUtc.Value.UtcDateTime.ToString("O", CultureInfo.InvariantCulture)
                : string.Empty,
            loaded_model = snapshot.LoadedModelName,
            model_loaded = snapshot.ModelLoaded,
            endpoints = new[]
            {
                "/health",
                "/v1/models",
                "/v1/chat/completions",
                "/v1/completions",
                "/v1/images/generations",
            },
            note = "Chat and text endpoints can use the loaded text model. Image generation is not wired yet.",
        };
    }

    private object BuildV1ModelsPayload()
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

    private static async Task<HttpRequestReadResult> ReadHttpRequestAsync(
        NetworkStream stream,
        CancellationToken cancellationToken)
    {
        List<byte> headerBytes = new();
        byte[] oneByte = new byte[1];
        int delimiterState = 0;

        while (headerBytes.Count <= MaxHeaderSizeBytes)
        {
            int read = await stream.ReadAsync(oneByte.AsMemory(0, 1), cancellationToken);
            if (read == 0)
            {
                return HttpRequestReadResult.Error(
                    400,
                    "The HTTP request ended before its headers were complete.",
                    "invalid_request_error");
            }

            byte value = oneByte[0];
            headerBytes.Add(value);
            delimiterState = (delimiterState, value) switch
            {
                (0, (byte)'\r') => 1,
                (1, (byte)'\n') => 2,
                (2, (byte)'\r') => 3,
                (3, (byte)'\n') => 4,
                (_, (byte)'\r') => 1,
                _ => 0,
            };

            if (delimiterState == 4)
            {
                break;
            }
        }

        if (delimiterState != 4)
        {
            return HttpRequestReadResult.Error(
                431,
                "Request headers exceed maximum allowed size (64 KB).",
                "request_header_fields_too_large");
        }

        string headerText = Encoding.ASCII.GetString(headerBytes.ToArray());
        string[] lines = headerText.Split(new[] { "\r\n" }, StringSplitOptions.None);
        if (lines.Length == 0 || string.IsNullOrWhiteSpace(lines[0]))
        {
            return HttpRequestReadResult.Error(
                400,
                "Missing HTTP request line.",
                "invalid_request_error");
        }

        string[] requestParts = lines[0].Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
        if (requestParts.Length < 2)
        {
            return HttpRequestReadResult.Error(
                400,
                "Malformed HTTP request line.",
                "invalid_request_error");
        }

        Dictionary<string, string> headers = new(StringComparer.OrdinalIgnoreCase);
        for (int index = 1; index < lines.Length; index++)
        {
            string line = lines[index];
            if (line.Length == 0)
            {
                break;
            }

            int separator = line.IndexOf(':', StringComparison.Ordinal);
            if (separator <= 0)
            {
                continue;
            }

            headers[line[..separator].Trim()] = line[(separator + 1)..].Trim();
        }

        if (!TryGetContentLength(headers, out int contentLength, out string lengthError))
        {
            return HttpRequestReadResult.Error(
                400,
                lengthError,
                "invalid_request_error");
        }

        if (contentLength > MaxRequestBodyBytes)
        {
            return HttpRequestReadResult.Error(
                413,
                "Request body is too large. Keep local server requests under 4 MB.",
                "request_too_large");
        }

        byte[] bodyBytes = new byte[contentLength];
        int bodyOffset = 0;
        while (bodyOffset < contentLength)
        {
            int read = await stream.ReadAsync(
                bodyBytes.AsMemory(bodyOffset, contentLength - bodyOffset),
                cancellationToken);
            if (read == 0)
            {
                return HttpRequestReadResult.Error(
                    400,
                    "The HTTP request body ended before Content-Length bytes were received.",
                    "invalid_request_error");
            }

            bodyOffset += read;
        }

        return HttpRequestReadResult.Ok(new HttpRequestData(
            requestParts[0].ToUpperInvariant(),
            NormalizeRequestPath(requestParts[1]),
            Encoding.UTF8.GetString(bodyBytes)));
    }

    private static bool TryGetContentLength(
        IReadOnlyDictionary<string, string> headers,
        out int contentLength,
        out string error)
    {
        contentLength = 0;
        error = string.Empty;
        if (!headers.TryGetValue("Content-Length", out string? value)
            || string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        if (!int.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out contentLength)
            || contentLength < 0)
        {
            error = "Content-Length must be a non-negative integer.";
            return false;
        }

        return true;
    }

    private static string NormalizeRequestPath(string target)
    {
        string path = target;
        if (Uri.TryCreate(target, UriKind.Absolute, out Uri? absolute))
        {
            path = absolute.AbsolutePath;
        }

        int queryIndex = path.IndexOf('?', StringComparison.Ordinal);
        if (queryIndex >= 0)
        {
            path = path[..queryIndex];
        }

        return string.IsNullOrWhiteSpace(path)
            ? "/"
            : Uri.UnescapeDataString(path);
    }

    private static async Task WriteGeneratedJsonAsync(
        NetworkStream stream,
        Func<object> payloadFactory,
        CancellationToken cancellationToken)
    {
        try
        {
            await WriteJsonAsync(stream, 200, payloadFactory(), cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await WriteOpenAiErrorAsync(
                stream,
                500,
                $"The local server could not build the response: {ex.Message}",
                "server_error",
                cancellationToken);
        }
    }

    private static async Task WriteOpenAiErrorAsync(
        NetworkStream stream,
        int statusCode,
        string message,
        string type,
        CancellationToken cancellationToken)
    {
        await WriteJsonAsync(
            stream,
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
        NetworkStream stream,
        int statusCode,
        object payload,
        CancellationToken cancellationToken)
    {
        byte[] body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload, JsonOptions));
        await WriteResponseAsync(
            stream,
            statusCode,
            ReasonPhrase(statusCode),
            "application/json; charset=utf-8",
            body,
            cancellationToken);
    }

    private static async Task WriteEventStreamAsync(
        NetworkStream stream,
        IReadOnlyList<object> payloads,
        CancellationToken cancellationToken)
    {
        string headers =
            "HTTP/1.1 200 OK\r\n" +
            $"Date: {DateTimeOffset.UtcNow:R}\r\n" +
            "Server: OpenVINO-Workbench-Local\r\n" +
            "Content-Type: text/event-stream; charset=utf-8\r\n" +
            "Cache-Control: no-cache\r\n" +
            "Connection: close\r\n" +
            "Access-Control-Allow-Origin: *\r\n" +
            "Access-Control-Allow-Methods: GET, POST, OPTIONS\r\n" +
            "Access-Control-Allow-Headers: content-type, authorization, x-requested-with, openai-organization, openai-project\r\n" +
            "\r\n";

        await stream.WriteAsync(Encoding.ASCII.GetBytes(headers), cancellationToken);
        foreach (object payload in payloads)
        {
            string json = JsonSerializer.Serialize(payload, JsonOptions);
            await stream.WriteAsync(
                Encoding.UTF8.GetBytes($"data: {json}\n\n"),
                cancellationToken);
        }

        await stream.WriteAsync(
            Encoding.UTF8.GetBytes("data: [DONE]\n\n"),
            cancellationToken);
    }

    private static async Task WriteResponseAsync(
        NetworkStream stream,
        int statusCode,
        string reasonPhrase,
        string contentType,
        byte[] body,
        CancellationToken cancellationToken)
    {
        string headers =
            $"HTTP/1.1 {statusCode.ToString(CultureInfo.InvariantCulture)} {reasonPhrase}\r\n" +
            $"Date: {DateTimeOffset.UtcNow:R}\r\n" +
            "Server: OpenVINO-Workbench-Local\r\n" +
            $"Content-Type: {contentType}\r\n" +
            $"Content-Length: {body.Length.ToString(CultureInfo.InvariantCulture)}\r\n" +
            "Connection: close\r\n" +
            "Access-Control-Allow-Origin: *\r\n" +
            "Access-Control-Allow-Methods: GET, POST, OPTIONS\r\n" +
            "Access-Control-Allow-Headers: content-type, authorization, x-requested-with, openai-organization, openai-project\r\n" +
            "\r\n";

        byte[] headerBytes = Encoding.ASCII.GetBytes(headers);
        await stream.WriteAsync(headerBytes, cancellationToken);
        if (body.Length > 0)
        {
            await stream.WriteAsync(body, cancellationToken);
        }
    }

    private static string ReasonPhrase(int statusCode) => statusCode switch
    {
        200 => "OK",
        204 => "No Content",
        400 => "Bad Request",
        404 => "Not Found",
        405 => "Method Not Allowed",
        409 => "Conflict",
        413 => "Payload Too Large",
        431 => "Request Header Fields Too Large",
        500 => "Internal Server Error",
        501 => "Not Implemented",
        502 => "Bad Gateway",
        503 => "Service Unavailable",
        504 => "Gateway Timeout",
        _ => "OK",
    };

    public static string CreateDigest(string source, long sizeBytes, DateTimeOffset modifiedAtUtc)
    {
        string text = $"{source}|{sizeBytes.ToString(CultureInfo.InvariantCulture)}|{modifiedAtUtc.UtcTicks.ToString(CultureInfo.InvariantCulture)}";
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return System.Convert.ToHexString(hash).ToLowerInvariant();
    }

    private sealed record HttpRequestData(string Method, string Path, string Body);

    private sealed class HttpRequestReadResult
    {
        public bool Success { get; init; }
        public HttpRequestData? Request { get; init; }
        public int StatusCode { get; init; } = 400;
        public string ErrorMessage { get; init; } = string.Empty;
        public string ErrorType { get; init; } = "invalid_request_error";

        public static HttpRequestReadResult Ok(HttpRequestData request)
        {
            return new HttpRequestReadResult
            {
                Success = true,
                Request = request,
            };
        }

        public static HttpRequestReadResult Error(int statusCode, string message, string type)
        {
            return new HttpRequestReadResult
            {
                Success = false,
                StatusCode = statusCode,
                ErrorMessage = message,
                ErrorType = type,
            };
        }
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
    public int MaxTokens { get; init; } = 64;
    public double Temperature { get; init; }
    public bool Stream { get; init; }
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

    public static LocalModelApiTextResult Ok(
        string text,
        string modelName,
        long loadMilliseconds,
        long generateMilliseconds)
    {
        return new LocalModelApiTextResult
        {
            Success = true,
            Text = text,
            ModelName = modelName,
            LoadMilliseconds = loadMilliseconds,
            GenerateMilliseconds = generateMilliseconds,
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
