using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace WorkStationC_;

// Tools the chat model may call: web_search (Brave, then Serper) and read_web_page.
// Keys come from SearchApiKeys.txt ("Brave Search API-Key: ..." and "Serper Search API-Key: ...").
internal sealed partial class WebTools
{
    public const string KeyFileName = "SearchApiKeys.txt";
    private const int MaxPageCharacters = 6000;
    private static readonly HttpClient Http = CreateClient();

    private readonly string _braveKey;
    private readonly string _serperKey;

    private WebTools(string braveKey, string serperKey)
    {
        _braveKey = braveKey;
        _serperKey = serperKey;
    }

    public bool CanSearch => _braveKey.Length > 0 || _serperKey.Length > 0;
    public string Providers => string.Join(" and ", new[] { _braveKey.Length > 0 ? "Brave" : "", _serperKey.Length > 0 ? "Serper" : "" }.Where(name => name.Length > 0));

    public static WebTools Load()
    {
        string? path = new[] { AppContext.BaseDirectory, AppFiles.GetProjectDirectory() }
            .Select(folder => Path.Combine(folder, KeyFileName))
            .FirstOrDefault(File.Exists);
        string brave = string.Empty, serper = string.Empty;
        if (path is not null)
        {
            foreach (string line in File.ReadAllLines(path))
            {
                int colon = line.IndexOf(':');
                if (colon < 0) continue;
                string name = line[..colon];
                string key = line[(colon + 1)..].Trim();
                if (name.Contains("brave", StringComparison.OrdinalIgnoreCase)) brave = key;
                else if (name.Contains("serper", StringComparison.OrdinalIgnoreCase)) serper = key;
            }
        }
        return new WebTools(brave, serper);
    }

    public JsonArray Definitions()
    {
        JsonArray tools = [];
        if (CanSearch)
        {
            tools.Add(Function("web_search", "Search the web for current information. Returns titles, links, and short snippets.",
                new JsonObject
                {
                    ["query"] = new JsonObject { ["type"] = "string", ["description"] = "What to search for." },
                    ["count"] = new JsonObject { ["type"] = "integer", ["description"] = "How many results, 1 to 10. Default 5." },
                }, "query"));
        }
        tools.Add(Function("read_web_page", "Download a web page and return its readable text. Use it on links from web_search.",
            new JsonObject { ["url"] = new JsonObject { ["type"] = "string", ["description"] = "The http or https address of the page." } }, "url"));
        return tools;
    }

    internal static JsonObject Function(string name, string description, JsonObject properties, string required) => new()
    {
        ["type"] = "function",
        ["function"] = new JsonObject
        {
            ["name"] = name,
            ["description"] = description,
            ["parameters"] = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = properties,
                ["required"] = new JsonArray(required),
            },
        },
    };

    // Short text for the transcript, such as: Searching the web: "weather in Paris".
    public static string Describe(string name, JsonObject arguments) => name switch
    {
        "web_search" => $"Searching the web: \"{Text(arguments, "query")}\"",
        "read_web_page" => $"Reading page: {Text(arguments, "url")}",
        _ => $"Using tool {name}",
    };

    public async Task<string> RunAsync(string name, JsonObject arguments, CancellationToken cancellationToken)
    {
        try
        {
            return name switch
            {
                "web_search" => await SearchAsync(Text(arguments, "query"), Count(arguments), cancellationToken),
                "read_web_page" => await ReadPageAsync(Text(arguments, "url"), cancellationToken),
                _ => $"Error: there is no tool named {name}.",
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception ex) { return $"Error: {ex.Message}"; }
    }

    private async Task<string> SearchAsync(string query, int count, CancellationToken cancellationToken)
    {
        if (query.Length == 0) return "Error: the search query is empty.";
        if (!CanSearch) return $"Error: no search keys were found in {KeyFileName}.";
        string? braveError = null;
        if (_braveKey.Length > 0)
        {
            try { return await BraveAsync(query, count, cancellationToken); }
            catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
            {
                braveError = ex.Message;
            }
        }
        if (_serperKey.Length > 0) return await SerperAsync(query, count, cancellationToken);
        return $"Error: Brave search failed: {braveError}";
    }

    private async Task<string> BraveAsync(string query, int count, CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new(HttpMethod.Get,
            $"https://api.search.brave.com/res/v1/web/search?q={Uri.EscapeDataString(query)}&count={count}");
        request.Headers.Add("X-Subscription-Token", _braveKey);
        request.Headers.Add("Accept", "application/json");
        using var response = await Http.SendAsync(request, cancellationToken);
        string body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode) throw new HttpRequestException($"Brave answered {(int)response.StatusCode}.");
        using var json = JsonDocument.Parse(body);
        List<(string Title, string Url, string Snippet)> results = [];
        if (json.RootElement.TryGetProperty("web", out var web) && web.TryGetProperty("results", out var items))
            foreach (var item in items.EnumerateArray())
                results.Add((Read(item, "title"), Read(item, "url"), Read(item, "description")));
        return FormatResults("Brave", query, results);
    }

    private async Task<string> SerperAsync(string query, int count, CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, "https://google.serper.dev/search")
        {
            Content = new StringContent(new JsonObject { ["q"] = query, ["num"] = count }.ToJsonString(), Encoding.UTF8, "application/json"),
        };
        request.Headers.Add("X-API-KEY", _serperKey);
        using var response = await Http.SendAsync(request, cancellationToken);
        string body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode) return $"Error: Serper answered {(int)response.StatusCode}.";
        using var json = JsonDocument.Parse(body);
        List<(string Title, string Url, string Snippet)> results = [];
        if (json.RootElement.TryGetProperty("answerBox", out var answer))
            results.Add((FirstText(Read(answer, "title"), "Answer"), Read(answer, "link"), FirstText(Read(answer, "answer"), Read(answer, "snippet"))));
        if (json.RootElement.TryGetProperty("organic", out var items))
            foreach (var item in items.EnumerateArray())
                results.Add((Read(item, "title"), Read(item, "link"), Read(item, "snippet")));
        return FormatResults("Serper", query, results.Take(count).ToList());
    }

    private static string FormatResults(string provider, string query, IReadOnlyList<(string Title, string Url, string Snippet)> results)
    {
        if (results.Count == 0) return $"No results from {provider} for \"{query}\".";
        StringBuilder text = new($"Web results from {provider} for \"{query}\" ({DateTime.Now:yyyy-MM-dd}):\n");
        for (int i = 0; i < results.Count; i++)
            text.Append($"{i + 1}. {StripTags(results[i].Title)}\n   {results[i].Url}\n   {StripTags(results[i].Snippet)}\n");
        return text.ToString();
    }

    private static async Task<string> ReadPageAsync(string url, CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            return "Error: give a full http or https address.";
        if (uri.IsLoopback) return "Error: local addresses cannot be read.";
        using var response = await Http.GetAsync(uri, cancellationToken);
        if (!response.IsSuccessStatusCode) return $"Error: the page answered {(int)response.StatusCode}.";
        string type = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
        if (type.Length > 0 && !type.StartsWith("text/", StringComparison.OrdinalIgnoreCase) && !type.Contains("html") && !type.Contains("json") && !type.Contains("xml"))
            return $"Error: this address is not a web page ({type}).";
        string body = await response.Content.ReadAsStringAsync(cancellationToken);
        string text = type.Contains("html") || body.TrimStart().StartsWith('<') ? HtmlToText(body) : body;
        if (text.Length > MaxPageCharacters) text = text[..MaxPageCharacters] + "\n[page shortened]";
        return $"Text of {uri}:\n{text}";
    }

    private static string HtmlToText(string html)
    {
        string text = NonContent().Replace(html, " ");
        text = BlockTags().Replace(text, "\n");
        text = WebUtility.HtmlDecode(AnyTag().Replace(text, " "));
        text = Spaces().Replace(text, " ");
        return BlankLines().Replace(text, "\n\n").Trim();
    }

    private static string StripTags(string text) => WebUtility.HtmlDecode(AnyTag().Replace(text, string.Empty));
    private static string FirstText(string first, string second) => first.Length > 0 ? first : second;
    private static string Read(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() ?? string.Empty : string.Empty;
    private static string Text(JsonObject arguments, string name) => arguments[name]?.ToString().Trim() ?? string.Empty;
    private static int Count(JsonObject arguments) =>
        int.TryParse(arguments["count"]?.ToString(), out int count) ? Math.Clamp(count, 1, 10) : 5;

    private static HttpClient CreateClient()
    {
        HttpClient client = new(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All }) { Timeout = TimeSpan.FromSeconds(20) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) OpenVINOWorkbench/1.0");
        return client;
    }

    [GeneratedRegex(@"<(script|style|noscript|svg|head)[\s\S]*?</\1>|<!--[\s\S]*?-->", RegexOptions.IgnoreCase)]
    private static partial Regex NonContent();
    [GeneratedRegex(@"<(br|/p|/div|/li|/h[1-6]|/tr|/section|/article)[^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex BlockTags();
    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex AnyTag();
    [GeneratedRegex(@"[ \t\r\f\v]+")]
    private static partial Regex Spaces();
    [GeneratedRegex(@"\s*\n\s*\n\s*")]
    private static partial Regex BlankLines();
}
