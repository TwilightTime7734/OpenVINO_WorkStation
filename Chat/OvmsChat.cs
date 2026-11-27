using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace WorkStationC_;

// Talks to an OVMS (OpenAI-compatible) server, keeps the conversation history, and runs tool calls.
internal sealed partial class OvmsChat
{
    private const int MaxToolRounds = 6;
    private static readonly HttpClient Http = new() { Timeout = Timeout.InfiniteTimeSpan };
    private readonly JsonArray _messages = [];

    public void Clear() => _messages.Clear();

    // The whole conversation is sent with every message, so its size is what the model has to process each time.
    public int ConversationCharacters => _messages.ToJsonString().Length;

    // About four characters per token. Before each message the oldest whole exchanges are dropped
    // until the conversation fits the budget, so it stays within the model's reserved working memory.
    public int TokenBudget { get; set; } = 24_000;
    public int LastDroppedExchanges { get; private set; }

    private void TrimToBudget()
    {
        LastDroppedExchanges = 0;
        while (ConversationCharacters / 4 > TokenBudget)
        {
            int nextExchange = -1;
            for (int i = 1; i < _messages.Count; i++)
            {
                if (_messages[i]?["role"]?.ToString() == "user") { nextExchange = i; break; }
            }
            if (nextExchange < 0) break; // Only the latest exchange is left; keep it.
            for (int i = 0; i < nextExchange; i++) _messages.RemoveAt(0);
            LastDroppedExchanges++;
        }
    }

    // Shrinks exchanges before the latest one: big tool results, big tool arguments (such as written file
    // contents), and images are replaced with short notes. The latest exchange stays complete for follow-ups.
    private const int KeepCharacters = 300;

    private void CompactBefore(int end)
    {
        for (int i = 0; i < end; i++)
        {
            if (_messages[i] is not JsonObject message) continue;
            string role = message["role"]?.ToString() ?? string.Empty;
            if (role == "tool" && message["content"]?.ToString() is { Length: > KeepCharacters } result)
                message["content"] = $"[Earlier tool result removed to save memory ({result.Length:N0} characters). Use the tool again if you need it.]";
            if (role == "assistant" && message["tool_calls"] is JsonArray calls)
                foreach (var call in calls)
                    if (call?["function"] is JsonObject function && function["arguments"]?.ToString() is { Length: > KeepCharacters } arguments)
                        function["arguments"] = ShortenArguments(arguments);
            if (role == "user" && message["content"] is JsonArray parts && parts.Any(part => part?["type"]?.ToString() == "image_url"))
            {
                JsonArray kept = [];
                foreach (var part in parts)
                    kept.Add(part?["type"]?.ToString() == "image_url"
                        ? new JsonObject { ["type"] = "text", ["text"] = "[image sent earlier]" }
                        : part!.DeepClone());
                message["content"] = kept;
            }
        }
    }

    private static string ShortenArguments(string arguments)
    {
        if (ParseArguments(arguments) is not { Count: > 0 } values) return "{}";
        JsonObject shortened = [];
        foreach (var (key, value) in values)
            shortened[key] = value is JsonValue text && text.TryGetValue(out string? s) && s.Length > KeepCharacters
                ? $"[{s.Length:N0} characters omitted]"
                : value?.DeepClone();
        return shortened.ToJsonString();
    }

    public static async Task<string[]> ListModelsAsync(Uri address, CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(5));
        using var response = await Http.GetAsync(new Uri(address, "/v3/models"), timeout.Token);
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(timeout.Token));
        return json.RootElement.GetProperty("data").EnumerateArray()
            .Select(model => model.GetProperty("id").GetString() ?? string.Empty)
            .Where(id => id.Length > 0)
            .ToArray();
    }

    private sealed class ToolCall
    {
        public string Id = string.Empty;
        public string Name = string.Empty;
        public StringBuilder Arguments = new();
    }

    // Sends one user message and streams the reply text to onText. When tools is not null the model may
    // call them; each call is reported through onTool, run here, and its result sent back to the model.
    public async Task<string> SendAsync(Uri address, string model, string text, IReadOnlyList<string> imagePaths,
        ChatTools? tools, Action<string> onText, Action<string> onTool, CancellationToken cancellationToken)
    {
        TrimToBudget();
        int historyCount = _messages.Count;
        _messages.Add(new JsonObject { ["role"] = "user", ["content"] = BuildContent(text, imagePaths) });
        try
        {
            for (int round = 0; ; round++)
            {
                bool allowTools = tools is not null && round < MaxToolRounds;
                var (reply, calls) = await StreamOnceAsync(address, model, allowTools ? tools : null, onText, cancellationToken);
                if (calls.Count == 0 && allowTools) calls = FindToolCallsInText(reply);
                if (calls.Count == 0)
                {
                    _messages.Add(new JsonObject { ["role"] = "assistant", ["content"] = reply });
                    CompactBefore(historyCount);
                    return reply;
                }

                JsonArray toolCalls = [];
                foreach (var call in calls)
                {
                    if (call.Id.Length == 0) call.Id = $"call_{Guid.NewGuid():N}"[..24];
                    toolCalls.Add(new JsonObject
                    {
                        ["id"] = call.Id,
                        ["type"] = "function",
                        ["function"] = new JsonObject { ["name"] = call.Name, ["arguments"] = call.Arguments.ToString() },
                    });
                }
                _messages.Add(new JsonObject { ["role"] = "assistant", ["content"] = reply, ["tool_calls"] = toolCalls });

                foreach (var call in calls)
                {
                    JsonObject arguments = ParseArguments(call.Arguments.ToString());
                    onTool(ChatTools.Describe(call.Name, arguments));
                    string result = await tools!.RunAsync(call.Name, arguments, cancellationToken);
                    _messages.Add(new JsonObject { ["role"] = "tool", ["tool_call_id"] = call.Id, ["name"] = call.Name, ["content"] = result });
                }
            }
        }
        catch
        {
            // Forget this unfinished exchange so the next message starts from a clean history.
            while (_messages.Count > historyCount) _messages.RemoveAt(_messages.Count - 1);
            throw;
        }
    }

    private async Task<(string Reply, List<ToolCall> Calls)> StreamOnceAsync(Uri address, string model, ChatTools? tools,
        Action<string> onText, CancellationToken cancellationToken)
    {
        JsonArray messages = [];
        if (tools is not null) messages.Add(new JsonObject { ["role"] = "system", ["content"] = tools.Instructions() });
        foreach (var message in _messages) messages.Add(message!.DeepClone());
        JsonObject body = new() { ["model"] = model, ["stream"] = true, ["messages"] = messages };
        if (tools is not null)
        {
            body["tools"] = tools.Definitions();
            body["tool_choice"] = "auto";
        }

        using HttpRequestMessage request = new(HttpMethod.Post, new Uri(address, "/v3/chat/completions"))
        {
            Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json"),
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        using var response = await Http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"The server answered {(int)response.StatusCode}: {error}");
        }

        StringBuilder reply = new();
        StringBuilder reasoningText = new();
        int checkedReply = 0, checkedReasoning = 0;
        SortedDictionary<int, ToolCall> calls = [];
        bool thinking = false;
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using StreamReader reader = new(stream);
        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            if (!line.StartsWith("data:", StringComparison.Ordinal)) continue;
            string data = line[5..].Trim();
            if (data == "[DONE]") break;
            using var chunk = JsonDocument.Parse(data);
            if (chunk.RootElement.TryGetProperty("error", out var streamError))
                throw new InvalidOperationException($"The server reported an error: {streamError}");
            if (!chunk.RootElement.TryGetProperty("choices", out var choices) || choices.GetArrayLength() == 0) continue;
            if (!choices[0].TryGetProperty("delta", out var delta)) continue;

            if (delta.TryGetProperty("tool_calls", out var toolDeltas) && toolDeltas.ValueKind == JsonValueKind.Array)
            {
                foreach (var toolDelta in toolDeltas.EnumerateArray())
                {
                    int index = toolDelta.TryGetProperty("index", out var indexValue) && indexValue.TryGetInt32(out int i) ? i : calls.Count;
                    if (!calls.TryGetValue(index, out var call)) calls[index] = call = new ToolCall();
                    string id = ReadText(toolDelta, "id");
                    if (id.Length > 0) call.Id = id;
                    if (toolDelta.TryGetProperty("function", out var function))
                    {
                        string name = ReadText(function, "name");
                        if (name.Length > 0) call.Name = name;
                        call.Arguments.Append(ReadText(function, "arguments"));
                    }
                }
            }

            string reasoning = ReadText(delta, "reasoning_content");
            if (reasoning.Length > 0)
            {
                if (!thinking) onText("(thinking) ");
                thinking = true;
                onText(reasoning);
                reasoningText.Append(reasoning);
                if (reasoningText.Length - checkedReasoning >= 200)
                {
                    checkedReasoning = reasoningText.Length;
                    if (RepeatDetector.IsRepeating(reasoningText)) throw new RepeatingReplyException(thinking: true);
                }
            }
            string content = ReadText(delta, "content");
            if (content.Length == 0) continue;
            if (thinking)
            {
                onText(Environment.NewLine + Environment.NewLine);
                thinking = false;
            }
            reply.Append(content);
            onText(content);
            if (reply.Length - checkedReply >= 200)
            {
                checkedReply = reply.Length;
                if (RepeatDetector.IsRepeating(reply)) throw new RepeatingReplyException(thinking: false);
            }
        }
        return (reply.ToString(), calls.Values.Where(call => call.Name.Length > 0).ToList());
    }

    // Used when the server returns tool calls as plain text because no tool parser is set for the model.
    // Handles the Hermes JSON form and the Qwen3-Coder XML form.
    private static List<ToolCall> FindToolCallsInText(string reply)
    {
        List<ToolCall> calls = [];
        foreach (Match match in ToolCallBlock().Matches(reply))
        {
            string inner = match.Groups[1].Value.Trim();
            if (inner.StartsWith('{'))
            {
                try
                {
                    var json = JsonNode.Parse(inner)?.AsObject();
                    string name = json?["name"]?.ToString() ?? string.Empty;
                    if (name.Length == 0) continue;
                    var arguments = json!["arguments"];
                    ToolCall call = new() { Name = name };
                    call.Arguments.Append(arguments is JsonValue value && value.TryGetValue(out string? textArguments) ? textArguments : arguments?.ToJsonString() ?? "{}");
                    calls.Add(call);
                }
                catch (JsonException) { }
                continue;
            }
            var function = XmlFunction().Match(inner);
            if (!function.Success) continue;
            JsonObject parameters = [];
            foreach (Match parameter in XmlParameter().Matches(function.Groups[2].Value))
                parameters[parameter.Groups[1].Value.Trim()] = parameter.Groups[2].Value.Trim();
            ToolCall xmlCall = new() { Name = function.Groups[1].Value.Trim() };
            xmlCall.Arguments.Append(parameters.ToJsonString());
            calls.Add(xmlCall);
        }
        return calls;
    }

    private static JsonObject ParseArguments(string text)
    {
        try { return JsonNode.Parse(text.Length == 0 ? "{}" : text) as JsonObject ?? []; }
        catch (JsonException) { return []; }
    }

    private static string ReadText(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() ?? string.Empty : string.Empty;

    private static JsonNode BuildContent(string text, IReadOnlyList<string> imagePaths)
    {
        if (imagePaths.Count == 0) return JsonValue.Create(text);
        JsonArray parts = [new JsonObject { ["type"] = "text", ["text"] = text }];
        foreach (string path in imagePaths)
        {
            string mime = Path.GetExtension(path).ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                _ => "image/png",
            };
            string url = $"data:{mime};base64,{Convert.ToBase64String(File.ReadAllBytes(path))}";
            parts.Add(new JsonObject { ["type"] = "image_url", ["image_url"] = new JsonObject { ["url"] = url } });
        }
        return parts;
    }

    [GeneratedRegex(@"<tool_call>([\s\S]*?)</tool_call>")]
    private static partial Regex ToolCallBlock();
    [GeneratedRegex(@"<function=([^>\s]+)>([\s\S]*?)</function>")]
    private static partial Regex XmlFunction();
    [GeneratedRegex(@"<parameter=([^>\s]+)>([\s\S]*?)</parameter>")]
    private static partial Regex XmlParameter();
}
