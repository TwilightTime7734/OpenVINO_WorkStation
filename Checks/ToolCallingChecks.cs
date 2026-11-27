using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json;
using WorkStationC_;

internal static class ToolCallingChecks
{
    private const string ToolsJson = """
        [{"type":"function","function":{"name":"read_file","description":"Read a file","parameters":{"type":"object","properties":{"path":{"type":"string"},"lines":{"type":"integer"},"options":{"type":"object"}}}}},
         {"type":"function","function":{"name":"list_files","parameters":{"type":"object","properties":{}}}}]
        """;
    private const string JsonCall = """<tool_call>{"name":"read_file","arguments":{"path":"D:\\Notes\\世界.txt","lines":10}}</tool_call>""";
    private const string XmlCall = """
        <tool_call>
        <function=read_file>
        <parameter=path>
        123
        </parameter>
        <parameter=lines>
        10
        </parameter>
        <parameter=options>
        {"head":true}
        </parameter>
        </function>
        </tool_call>
        """;

    private static void Check(bool condition, string description)
    {
        if (!condition) throw new Exception(description);
        Console.WriteLine("PASS: " + description);
    }

    private static ToolCalling Options(string extra = "")
    {
        using var document = JsonDocument.Parse("{\"tools\":" + ToolsJson + extra + "}");
        return ToolCalling.FromRequest(document.RootElement);
    }

    private static void Reject(Action action, string description)
    {
        try { action(); }
        catch (Exception ex) when (ex is InvalidDataException or JsonException)
        {
            Check(true, description);
            return;
        }
        throw new Exception(description);
    }

    public static async Task RunAsync()
    {
        var tools = Options();
        var json = tools.Parse(JsonCall);
        using var args = JsonDocument.Parse(json.Calls.Single().Arguments);
        Check(args.RootElement.GetProperty("path").GetString() == "D:\\Notes\\世界.txt", "JSON calls preserve Windows paths and Unicode");
        var xml = tools.Parse(XmlCall);
        using var xmlArgs = JsonDocument.Parse(xml.Calls.Single().Arguments);
        Check(xmlArgs.RootElement.GetProperty("path").GetString() == "123"
            && xmlArgs.RootElement.GetProperty("lines").GetInt32() == 10
            && xmlArgs.RootElement.GetProperty("options").GetProperty("head").GetBoolean(), "XML calls use schema types for string, number and object arguments");
        var multiple = tools.Parse("Reading now.\n" + JsonCall + "\n" + XmlCall);
        Check(multiple.Calls.Count == 2 && multiple.Calls[0].Id != multiple.Calls[1].Id && multiple.Content.Trim() == "Reading now.",
            "Multiple calls receive distinct IDs and are removed from assistant content");
        Check(tools.Parse("<think>Example " + JsonCall + "</think>\nAnswer").Calls.Count == 0, "Calls inside reasoning are not executed");
        Check(tools.Parse("```\n" + JsonCall + "\n```").Calls.Count == 0, "Calls in fenced examples are not executed");
        Check(tools.Parse("Normal answer").Content == "Normal answer", "Automatic tool choice allows a normal answer");
        Check(!Options(",\"tool_choice\":\"none\"").Enabled, "tool_choice none disables tool parsing and prompting");
        Reject(() => Options(",\"tool_choice\":\"required\"").Parse("No call"), "Required calls cannot silently turn into text");
        Reject(() => Options(",\"parallel_tool_calls\":false").Parse(JsonCall + XmlCall), "Parallel calls disabled is enforced");
        Reject(() => tools.Parse(JsonCall.Replace("read_file", "delete_file")), "Unknown tool names are rejected");
        Reject(() => tools.Parse(JsonCall[..^12]), "Truncated tool calls cannot execute");
        Reject(() => tools.Parse("<tool_call>{\"name\":\"read_file\",\"arguments\":[]}</tool_call>"), "Arguments must be a JSON object");
        Reject(() => Options(",\"tool_choice\":{\"type\":\"function\",\"function\":{\"name\":\"missing\"}}"), "Named choices must exist");
        var named = Options(",\"tool_choice\":{\"type\":\"function\",\"function\":{\"name\":\"read_file\"}}");
        Check(named.Tools.Length == 1 && named.Required, "Named choice restricts model tool definitions");
        Check(tools.VisionPrompt("user: read it").Contains("read_file") && tools.VisionPrompt("user: read it").Contains("user: read it"),
            "Vision prompts include definitions and the conversation");
        // Closing-tag text inside JSON arguments must not terminate the surrounding call.
        Check(tools.Parse("<tool_call>{\"name\":\"read_file\",\"arguments\":{\"path\":\"</tool_call>\"}}</tool_call>").Calls.Count == 1,
            "JSON string contents do not break tool-call framing");
        await CheckHttpAsync();
    }

    private static async Task CheckHttpAsync()
    {
        using TcpListener probe = new(IPAddress.Loopback, 0);
        probe.Start();
        int port = ((IPEndPoint)probe.LocalEndpoint).Port;
        probe.Stop();
        LocalModelApiTextRequest? received = null;
        string output = JsonCall;
        using var server = new LocalModelApiServer(() => new() { LoadedModelName = "fixture" }, async (request, token) =>
        {
            received = request;
            // Feed fragments as the native engine does, including split delimiters and JSON.
            if (request.OnTextChunk is { } write)
                foreach (char character in output) { token.ThrowIfCancellationRequested(); await write(character.ToString()); }
            return LocalModelApiTextResult.Ok(output, "fixture", 0, 1, 20, 10);
        }, Console.WriteLine);
        server.Start(IPAddress.Loopback, port);
        try
        {
            using HttpClient client = new() { BaseAddress = new(server.BaseUrl), Timeout = TimeSpan.FromSeconds(10) };
            using var definitions = JsonDocument.Parse(ToolsJson);
            foreach (string route in new[] { "/v3/chat/completions", "/v1/chat/completions" })
            {
                foreach (string modelOutput in new[] { JsonCall, XmlCall })
                {
                    output = modelOutput;
                    using var response = await client.PostAsJsonAsync(route, new
                    {
                        messages = new[] { new { role = "user", content = "Read my file" } }, tools = definitions.RootElement,
                    });
                    response.EnsureSuccessStatusCode();
                    using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                    var choice = body.RootElement.GetProperty("choices")[0];
                    var message = choice.GetProperty("message");
                    var call = message.GetProperty("tool_calls")[0];
                    Check(choice.GetProperty("finish_reason").GetString() == "tool_calls" && message.GetProperty("content").ValueKind == JsonValueKind.Null
                        && call.GetProperty("function").GetProperty("arguments").ValueKind == JsonValueKind.String, route + " returns a native tool call");
                    Check(received!.ToolCalling.Tools.Length == 2, "Tool definitions reach the generation handler");
                    string callId = call.GetProperty("id").GetString()!;
                    output = "The file contains hello.";
                    using var followup = await client.PostAsJsonAsync(route, new
                    {
                        messages = new object[] { new { role = "user", content = "Read my file" }, message.Clone(),
                            new { role = "tool", tool_call_id = callId, content = "hello" } }, tools = definitions.RootElement,
                    });
                    followup.EnsureSuccessStatusCode();
                    using var history = JsonDocument.Parse(received!.Messages[1]);
                    using var result = JsonDocument.Parse(received.Messages[2]);
                    Check(history.RootElement.GetProperty("tool_calls")[0].GetProperty("id").GetString() == callId
                        && result.RootElement.GetProperty("tool_call_id").GetString() == callId
                        && result.RootElement.GetProperty("content").GetString() == "hello", "Tool result round trip preserves call IDs and content");
                    using var answer = JsonDocument.Parse(await followup.Content.ReadAsStringAsync());
                    Check(answer.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() == output,
                        "Tool result round trip completes with an ordinary answer");

                    output = modelOutput + "\n" + JsonCall;
                    using var stream = await client.PostAsJsonAsync(route, new
                    {
                        messages = new[] { new { role = "user", content = "Read files" } }, tools = definitions.RootElement,
                        stream = true, stream_options = new { include_usage = true },
                    });
                    stream.EnsureSuccessStatusCode();
                    string sse = await stream.Content.ReadAsStringAsync();
                    string[] events = sse.Split('\n').Where(s => s.StartsWith("data: ")).Select(s => s[6..]).ToArray();
                    Check(events[^1] == "[DONE]" && events.Count(e => e == "[DONE]") == 1, "Tool stream terminates once");
                    using var callEvent = JsonDocument.Parse(events[1]);
                    var calls = callEvent.RootElement.GetProperty("choices")[0].GetProperty("delta").GetProperty("tool_calls");
                    Check(calls.GetArrayLength() == 2 && calls[0].GetProperty("index").GetInt32() == 0 && calls[1].GetProperty("index").GetInt32() == 1,
                        "Fragmented model output becomes indexed SSE tool calls");
                    using var finish = JsonDocument.Parse(events[2]);
                    using var usage = JsonDocument.Parse(events[3]);
                    Check(finish.RootElement.GetProperty("choices")[0].GetProperty("finish_reason").GetString() == "tool_calls"
                        && usage.RootElement.GetProperty("usage").GetProperty("total_tokens").GetInt32() == 30, "Tool stream reports finish reason and token usage");
                }
            }
            output = "<tool_call>{broken";
            using var invalid = await client.PostAsJsonAsync("/v3/chat/completions", new
            {
                messages = new[] { new { role = "user", content = "Read it" } }, tools = definitions.RootElement, stream = true,
            });
            Check(invalid.StatusCode == HttpStatusCode.BadGateway && invalid.Content.Headers.ContentType?.MediaType == "application/json",
                "Malformed model calls return an HTTP error before SSE starts");
            using var invalidRequest = await client.PostAsJsonAsync("/v3/chat/completions", new
            {
                messages = new[] { new { role = "user", content = "Read it" } }, tools = "not an array",
            });
            Check(invalidRequest.StatusCode == HttpStatusCode.BadRequest, "Malformed tool definitions return HTTP 400");
        }
        finally { await server.StopAsync(); }
    }
}
