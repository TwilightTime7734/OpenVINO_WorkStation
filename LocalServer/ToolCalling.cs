using System.Text;
using System.Text.Json;

namespace WorkStationC_;

// Tool execution belongs to the client. This class handles the model's wire format only.
internal sealed class ToolCalling
{
    public static ToolCalling Disabled { get; } = new();
    public JsonElement[] Tools { get; private init; } = [];
    public bool Enabled => Tools.Length > 0;
    public bool Required { get; private init; }
    public bool Parallel { get; private init; } = true;

    public static ToolCalling FromRequest(JsonElement root)
    {
        JsonElement[] tools = [];
        if (root.TryGetProperty("tools", out var definitions))
        {
            if (definitions.ValueKind != JsonValueKind.Array)
                throw new InvalidDataException("tools must be an array of function definitions.");
            tools = definitions.EnumerateArray().Select(t => t.Clone()).ToArray();
        }
        HashSet<string> names = new(StringComparer.Ordinal);
        foreach (var tool in tools)
        {
            if (tool.ValueKind != JsonValueKind.Object || !tool.TryGetProperty("type", out var type)
                || type.ValueKind != JsonValueKind.String || type.GetString() != "function"
                || !tool.TryGetProperty("function", out var function) || function.ValueKind != JsonValueKind.Object
                || !function.TryGetProperty("name", out var name) || name.ValueKind != JsonValueKind.String
                || string.IsNullOrWhiteSpace(name.GetString()) || !names.Add(name.GetString()!))
                throw new InvalidDataException("Each tool must have type 'function' and a unique function name.");
            if (function.TryGetProperty("parameters", out var parameters) && parameters.ValueKind != JsonValueKind.Object)
                throw new InvalidDataException("Function parameters must be a JSON schema object.");
        }
        bool parallel = true;
        if (root.TryGetProperty("parallel_tool_calls", out var parallelValue))
        {
            if (parallelValue.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
                throw new InvalidDataException("parallel_tool_calls must be a boolean.");
            parallel = parallelValue.GetBoolean();
        }
        string choice = "auto";
        if (root.TryGetProperty("tool_choice", out var selection))
        {
            if (selection.ValueKind == JsonValueKind.String)
            {
                choice = selection.GetString()!;
                if (choice is not ("auto" or "none" or "required"))
                    throw new InvalidDataException("tool_choice must be auto, none, required, or a named function.");
            }
            else if (selection.ValueKind == JsonValueKind.Object
                && selection.TryGetProperty("type", out var type) && type.ValueKind == JsonValueKind.String && type.GetString() == "function"
                && selection.TryGetProperty("function", out var function) && function.ValueKind == JsonValueKind.Object
                && function.TryGetProperty("name", out var name) && name.ValueKind == JsonValueKind.String && names.Contains(name.GetString()!))
            {
                tools = tools.Where(t => t.GetProperty("function").GetProperty("name").GetString() == name.GetString()).ToArray();
                choice = "required";
            }
            else throw new InvalidDataException("tool_choice must name a function supplied in tools.");
        }
        if (choice == "required" && tools.Length == 0)
            throw new InvalidDataException("tool_choice requires at least one tool.");
        return new() { Tools = choice == "none" ? [] : tools, Required = choice == "required", Parallel = parallel };
    }

    public string Instructions => !Enabled ? "" :
        (Required ? "You must call one of the supplied tools in this response. " : "")
        + (!Parallel ? "Call at most one tool in this response. " : "");

    public string VisionPrompt(string prompt) => !Enabled ? prompt :
        "You have access to these tools:\n<tools>\n" + JsonSerializer.Serialize(Tools) + "\n</tools>\n"
        + "To call a tool, output <tool_call>{\"name\":\"function_name\",\"arguments\":{}}</tool_call> with its actual arguments. "
        + "Use only supplied tools. Do not invent tool results; the client will return them in a tool response. "
        + Instructions + "\n\nConversation:\n" + prompt;

    public ToolReply Parse(string text)
    {
        if (!Enabled) return new(text, []);
        List<ModelToolCall> calls = [];
        StringBuilder content = new();
        int offset = 0;
        // Some thinking templates prefill <think>, so the generated text may only contain its closing tag.
        int reasoningEnd = text.LastIndexOf("</think>", StringComparison.Ordinal);
        int searchFrom = reasoningEnd < 0 ? 0 : reasoningEnd + "</think>".Length;
        while (true)
        {
            int start = text.IndexOf("<tool_call>", searchFrom, StringComparison.Ordinal);
            if (start < 0) break;
            // Examples inside fenced code or unfinished reasoning are ordinary text.
            string prefix = text[searchFrom..start];
            if (prefix.Contains("<think>", StringComparison.Ordinal) || (text[..start].Split("```").Length - 1) % 2 != 0)
            {
                searchFrom = start + "<tool_call>".Length;
                continue;
            }
            content.Append(text[offset..start]);
            string remaining = text[(start + "<tool_call>".Length)..].TrimStart();
            string name;
            string arguments;
            int consumed;
            if (remaining.StartsWith('{'))
            {
                byte[] utf8 = Encoding.UTF8.GetBytes(remaining);
                var reader = new Utf8JsonReader(utf8);
                using var document = JsonDocument.ParseValue(ref reader);
                var call = document.RootElement;
                if (!call.TryGetProperty("name", out var functionName) || functionName.ValueKind != JsonValueKind.String
                    || !call.TryGetProperty("arguments", out var args))
                    throw new InvalidDataException("The model returned a tool call without a name or arguments.");
                name = functionName.GetString()!;
                arguments = args.ValueKind == JsonValueKind.String ? args.GetString()! : args.GetRawText();
                consumed = Encoding.UTF8.GetCharCount(utf8, 0, checked((int)reader.BytesConsumed));
            }
            else
            {
                int end = remaining.IndexOf("</function>", StringComparison.Ordinal);
                if (!remaining.StartsWith("<function=", StringComparison.Ordinal) || end < 0)
                    throw new InvalidDataException("The model returned an incomplete tool call.");
                int nameEnd = remaining.IndexOf('>');
                if (nameEnd < "<function=".Length || nameEnd > end)
                    throw new InvalidDataException("The model returned an invalid function name.");
                name = remaining["<function=".Length..nameEnd];
                arguments = ReadXmlArguments(name, remaining[(nameEnd + 1)..end]);
                consumed = end + "</function>".Length;
            }
            string tail = remaining[consumed..].TrimStart();
            if (!tail.StartsWith("</tool_call>", StringComparison.Ordinal))
                throw new InvalidDataException("The model returned an incomplete tool call.");
            FindFunction(name);
            using var argumentDocument = JsonDocument.Parse(arguments);
            if (argumentDocument.RootElement.ValueKind != JsonValueKind.Object)
                throw new InvalidDataException("Tool arguments must be a JSON object.");
            calls.Add(new("call_" + Guid.NewGuid().ToString("N"), name, arguments));
            offset = text.Length - tail.Length + "</tool_call>".Length;
            searchFrom = offset;
        }
        content.Append(text[offset..]);
        if (Required && calls.Count == 0)
            throw new InvalidDataException("The model did not return the required tool call.");
        if (!Parallel && calls.Count > 1)
            throw new InvalidDataException("The model returned multiple calls with parallel_tool_calls disabled.");
        return new(content.ToString(), calls);
    }

    private JsonElement FindFunction(string name)
    {
        foreach (var tool in Tools)
        {
            var function = tool.GetProperty("function");
            if (function.GetProperty("name").GetString() == name) return function;
        }
        throw new InvalidDataException($"The model requested an unavailable tool: {name}.");
    }

    // Qwen Coder / newer Qwen templates use <function=...> and <parameter=...>, not valid XML.
    private string ReadXmlArguments(string name, string body)
    {
        var function = FindFunction(name);
        Dictionary<string, object?> arguments = new(StringComparer.Ordinal);
        while (!string.IsNullOrWhiteSpace(body))
        {
            body = body.TrimStart();
            int nameEnd = body.IndexOf('>');
            int valueEnd = body.IndexOf("</parameter>", StringComparison.Ordinal);
            if (!body.StartsWith("<parameter=", StringComparison.Ordinal) || nameEnd < "<parameter=".Length || valueEnd < nameEnd)
                throw new InvalidDataException("The model returned an invalid tool parameter.");
            string key = body["<parameter=".Length..nameEnd];
            string value = body[(nameEnd + 1)..valueEnd];
            // Remove the template's framing newlines, preserving spaces inside string values.
            if (value.StartsWith('\n')) value = value[1..];
            if (value.EndsWith('\n')) value = value[..^1];
            bool isString = function.TryGetProperty("parameters", out var schema)
                && schema.TryGetProperty("properties", out var properties) && properties.ValueKind == JsonValueKind.Object
                && properties.TryGetProperty(key, out var property) && property.ValueKind == JsonValueKind.Object
                && property.TryGetProperty("type", out var type)
                && (type.ValueKind == JsonValueKind.String && type.GetString() == "string"
                    || type.ValueKind == JsonValueKind.Array && type.EnumerateArray().Any(t => t.ValueKind == JsonValueKind.String && t.GetString() == "string"));
            object? parsed = value;
            if (!isString)
            {
                try { using var json = JsonDocument.Parse(value); parsed = json.RootElement.Clone(); }
                catch (JsonException) { }
            }
            if (!arguments.TryAdd(key, parsed)) throw new InvalidDataException("The model repeated a tool parameter.");
            body = body[(valueEnd + "</parameter>".Length)..];
        }
        return JsonSerializer.Serialize(arguments);
    }
}

internal sealed record ToolReply(string Content, IReadOnlyList<ModelToolCall> Calls);

internal sealed record ModelToolCall(string Id, string Name, string Arguments)
{
    public Dictionary<string, object?> ToPayload(int? index = null)
    {
        Dictionary<string, object?> result = new()
        {
            ["id"] = Id, ["type"] = "function", ["function"] = new { name = Name, arguments = Arguments },
        };
        if (index.HasValue) result["index"] = index.Value;
        return result;
    }
}
