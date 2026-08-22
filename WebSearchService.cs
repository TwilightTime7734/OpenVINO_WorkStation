using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace WorkStationC_;

internal sealed class WebSearchService
{
    private const int DefaultMaxResults = 10;
    private const int MaximumMaxResults = 20;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
    };

    private static readonly HttpClient SearchClient = CreateSearchClient();

    private readonly IReadOnlyDictionary<string, IWebSearchProvider> _providers;

    public WebSearchService()
    {
        IWebSearchProvider[] providers =
        {
            new SerperSearchProvider(SearchClient),
            new BraveSearchProvider(SearchClient),
        };

        _providers = providers.ToDictionary(provider => provider.Name, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<WebSearchResponse> SearchAsync(WebSearchRequest request, CancellationToken cancellationToken)
    {
        string query = request.Query.Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            return WebSearchResponse.Fail(400, "Search request must include a query.", "bad_request");
        }

        string requestedProvider = NormalizeProviderName(request.Provider);
        IWebSearchProvider? provider;
        if (string.Equals(requestedProvider, "auto", StringComparison.OrdinalIgnoreCase))
        {
            provider = _providers.Values.FirstOrDefault(candidate => candidate.IsConfigured);
            if (provider is null)
            {
                return WebSearchResponse.Fail(
                    503,
                    "Web search is not configured. Set WORKSTATION_SERPER_API_KEY or WORKSTATION_BRAVE_SEARCH_API_KEY, then restart the app.",
                    "search_not_configured");
            }
        }
        else if (!_providers.TryGetValue(requestedProvider, out provider))
        {
            string names = string.Join(", ", _providers.Keys.OrderBy(name => name, StringComparer.OrdinalIgnoreCase));
            return WebSearchResponse.Fail(400, $"Unknown search provider `{request.Provider}`. Use auto, {names}.", "bad_request");
        }

        if (!provider.IsConfigured)
        {
            return WebSearchResponse.Fail(503, provider.MissingConfigurationMessage, "search_not_configured");
        }

        WebSearchRequest normalizedRequest = request with
        {
            Query = query,
            Provider = provider.Name,
            MaxResults = Clamp(request.MaxResults <= 0 ? DefaultMaxResults : request.MaxResults, 1, MaximumMaxResults),
            Country = NormalizeCountry(request.Country),
            Language = NormalizeLanguage(request.Language),
            Page = Math.Max(1, request.Page),
        };

        try
        {
            return await provider.SearchAsync(normalizedRequest, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return WebSearchResponse.Fail(502, $"The {provider.Name} search request failed: {ex.Message}", "search_provider_error", provider.Name, query);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            return WebSearchResponse.Fail(504, $"The {provider.Name} search request timed out: {ex.Message}", "search_provider_timeout", provider.Name, query);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (JsonException ex)
        {
            return WebSearchResponse.Fail(502, $"The {provider.Name} search response was not valid JSON: {ex.Message}", "search_provider_error", provider.Name, query);
        }
    }

    internal static string CleanText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        string withoutTags = Regex.Replace(text, "<.*?>", string.Empty, RegexOptions.Singleline, TimeSpan.FromMilliseconds(100));
        string decoded = WebUtility.HtmlDecode(withoutTags);
        return Regex.Replace(decoded, @"\s+", " ", RegexOptions.None, TimeSpan.FromMilliseconds(100)).Trim();
    }

    internal static string ReadString(JsonElement element, string propertyName) =>
        JsonHelper.ReadString(element, propertyName);

    internal static int? ReadInt(JsonElement element, string propertyName) =>
        JsonHelper.ReadInt(element, propertyName);

    internal static string GetHostName(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out Uri? uri)
            ? uri.Host
            : string.Empty;
    }

    internal static string ReadApiKey(params string[] names)
    {
        foreach (string name in names)
        {
            string? value = Environment.GetEnvironmentVariable(name);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return string.Empty;
    }

    internal static StringContent CreateJsonContent(object payload)
    {
        return new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");
    }

    internal static WebSearchResponse ProviderHttpError(string provider, string query, HttpStatusCode statusCode, string body)
    {
        string message = string.IsNullOrWhiteSpace(body)
            ? $"The {provider} search provider returned {(int)statusCode} {statusCode}."
            : $"The {provider} search provider returned {(int)statusCode} {statusCode}: {CleanText(body)}";
        return WebSearchResponse.Fail(502, message, "search_provider_error", provider, query);
    }

    private static HttpClient CreateSearchClient()
    {
        HttpClientHandler handler = new()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
        };

        HttpClient client = new(handler)
        {
            Timeout = TimeSpan.FromSeconds(20),
        };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("WorkStationCSharp-WebSearch/0.1");
        return client;
    }

    private static string NormalizeProviderName(string provider)
    {
        string normalized = provider.Trim().ToLowerInvariant().Replace("_", "-", StringComparison.Ordinal);
        return normalized switch
        {
            "" => "auto",
            "brave-search" => "brave",
            "bravesearch" => "brave",
            "google" => "serper",
            "google-serper" => "serper",
            _ => normalized,
        };
    }

    private static string NormalizeCountry(string country)
    {
        string normalized = country.Trim();
        return string.IsNullOrWhiteSpace(normalized)
            ? "us"
            : normalized.ToLowerInvariant();
    }

    private static string NormalizeLanguage(string language)
    {
        string normalized = language.Trim();
        return string.IsNullOrWhiteSpace(normalized)
            ? "en"
            : normalized.ToLowerInvariant();
    }

    private static int Clamp(int value, int minimum, int maximum)
    {
        return Math.Max(minimum, Math.Min(maximum, value));
    }
}

internal static class JsonHelper
{
    public static string ReadString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement property))
        {
            return string.Empty;
        }

        return property.ValueKind switch
        {
            JsonValueKind.String => property.GetString() ?? string.Empty,
            JsonValueKind.Number => property.GetRawText(),
            _ => string.Empty,
        };
    }

    public static int? ReadInt(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement property))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.Number when property.TryGetInt32(out int value) => value,
            JsonValueKind.String when int.TryParse(property.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) => value,
            _ => null,
        };
    }
}

internal interface IWebSearchProvider
{
    string Name { get; }
    bool IsConfigured { get; }
    string MissingConfigurationMessage { get; }
    Task<WebSearchResponse> SearchAsync(WebSearchRequest request, CancellationToken cancellationToken);
}

internal sealed class SerperSearchProvider : IWebSearchProvider
{
    private const string Endpoint = "https://google.serper.dev/search";

    private readonly HttpClient _client;

    public SerperSearchProvider(HttpClient client)
    {
        _client = client;
    }

    public string Name => "serper";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);

    public string MissingConfigurationMessage => "Serper search is not configured. Set WORKSTATION_SERPER_API_KEY or SERPER_API_KEY, then restart the app.";

    private static string ApiKey => WebSearchService.ReadApiKey("WORKSTATION_SERPER_API_KEY", "SERPER_API_KEY");

    public async Task<WebSearchResponse> SearchAsync(WebSearchRequest request, CancellationToken cancellationToken)
    {
        using HttpRequestMessage httpRequest = new(HttpMethod.Post, Endpoint);
        httpRequest.Headers.Add("X-API-KEY", ApiKey);
        httpRequest.Content = WebSearchService.CreateJsonContent(new
        {
            q = request.Query,
            gl = request.Country,
            hl = request.Language,
            num = request.MaxResults,
            page = request.Page,
        });

        using HttpResponseMessage response = await _client.SendAsync(httpRequest, cancellationToken);
        string body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return WebSearchService.ProviderHttpError(Name, request.Query, response.StatusCode, body);
        }

        using JsonDocument document = JsonDocument.Parse(body);
        return new WebSearchResponse
        {
            Success = true,
            Query = request.Query,
            Provider = Name,
            Results = ParseResults(document.RootElement, request.MaxResults),
        };
    }

    private static IReadOnlyList<WebSearchResult> ParseResults(JsonElement root, int maxResults)
    {
        List<WebSearchResult> results = new();

        AddSerperResultFromObject(results, root, "answerBox", maxResults);
        AddSerperResultFromObject(results, root, "knowledgeGraph", maxResults);

        if (root.TryGetProperty("organic", out JsonElement organic)
            && organic.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in organic.EnumerateArray())
            {
                if (results.Count >= maxResults)
                {
                    break;
                }

                if (item.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                AddResult(
                    results,
                    WebSearchService.ReadString(item, "title"),
                    WebSearchService.ReadString(item, "link"),
                    WebSearchService.ReadString(item, "snippet"),
                    WebSearchService.ReadString(item, "date"),
                    WebSearchService.ReadInt(item, "position"),
                    maxResults);
            }
        }

        return results;
    }

    private static void AddSerperResultFromObject(List<WebSearchResult> results, JsonElement root, string propertyName, int maxResults)
    {
        if (results.Count >= maxResults
            || !root.TryGetProperty(propertyName, out JsonElement item)
            || item.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        string snippet = WebSearchService.ReadString(item, "snippet");
        if (string.IsNullOrWhiteSpace(snippet))
        {
            snippet = WebSearchService.ReadString(item, "answer");
        }

        if (string.IsNullOrWhiteSpace(snippet))
        {
            snippet = WebSearchService.ReadString(item, "description");
        }

        string url = WebSearchService.ReadString(item, "link");
        if (string.IsNullOrWhiteSpace(url))
        {
            url = WebSearchService.ReadString(item, "website");
        }

        AddResult(
            results,
            WebSearchService.ReadString(item, "title"),
            url,
            snippet,
            WebSearchService.ReadString(item, "date"),
            null,
            maxResults);
    }

    private static void AddResult(
        List<WebSearchResult> results,
        string title,
        string url,
        string snippet,
        string publishedAt,
        int? sourceRank,
        int maxResults)
    {
        if (results.Count >= maxResults || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        string cleanUrl = url.Trim();
        if (results.Any(result => string.Equals(result.Url, cleanUrl, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        int rank = sourceRank is int value && value > 0
            ? value
            : results.Count + 1;
        results.Add(new WebSearchResult
        {
            Rank = rank,
            Title = WebSearchService.CleanText(title),
            Url = cleanUrl,
            Snippet = WebSearchService.CleanText(snippet),
            SourceName = WebSearchService.GetHostName(cleanUrl),
            PublishedAt = WebSearchService.CleanText(publishedAt),
        });
    }
}

internal sealed class BraveSearchProvider : IWebSearchProvider
{
    private const string Endpoint = "https://api.search.brave.com/res/v1/web/search";

    private readonly HttpClient _client;

    public BraveSearchProvider(HttpClient client)
    {
        _client = client;
    }

    public string Name => "brave";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);

    public string MissingConfigurationMessage => "Brave Search is not configured. Set WORKSTATION_BRAVE_SEARCH_API_KEY or BRAVE_SEARCH_API_KEY, then restart the app.";

    private static string ApiKey => WebSearchService.ReadApiKey("WORKSTATION_BRAVE_SEARCH_API_KEY", "BRAVE_SEARCH_API_KEY");

    public async Task<WebSearchResponse> SearchAsync(WebSearchRequest request, CancellationToken cancellationToken)
    {
        string url = BuildUrl(request);
        using HttpRequestMessage httpRequest = new(HttpMethod.Get, url);
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpRequest.Headers.Add("X-Subscription-Token", ApiKey);

        using HttpResponseMessage response = await _client.SendAsync(httpRequest, cancellationToken);
        string body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return WebSearchService.ProviderHttpError(Name, request.Query, response.StatusCode, body);
        }

        using JsonDocument document = JsonDocument.Parse(body);
        return new WebSearchResponse
        {
            Success = true,
            Query = request.Query,
            Provider = Name,
            Results = ParseResults(document.RootElement, request.MaxResults),
        };
    }

    private static string BuildUrl(WebSearchRequest request)
    {
        Dictionary<string, string> parameters = new()
        {
            ["q"] = request.Query,
            ["count"] = request.MaxResults.ToString(CultureInfo.InvariantCulture),
            ["country"] = request.Country,
            ["search_lang"] = request.Language,
            ["spellcheck"] = "1",
        };

        string query = string.Join(
            "&",
            parameters.Select(pair => $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}"));
        return $"{Endpoint}?{query}";
    }

    private static IReadOnlyList<WebSearchResult> ParseResults(JsonElement root, int maxResults)
    {
        List<WebSearchResult> results = new();
        if (!root.TryGetProperty("web", out JsonElement web)
            || web.ValueKind != JsonValueKind.Object
            || !web.TryGetProperty("results", out JsonElement webResults)
            || webResults.ValueKind != JsonValueKind.Array)
        {
            return results;
        }

        foreach (JsonElement item in webResults.EnumerateArray())
        {
            if (results.Count >= maxResults)
            {
                break;
            }

            if (item.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            string url = WebSearchService.ReadString(item, "url");
            if (string.IsNullOrWhiteSpace(url))
            {
                continue;
            }

            string sourceName = string.Empty;
            if (item.TryGetProperty("profile", out JsonElement profile)
                && profile.ValueKind == JsonValueKind.Object)
            {
                sourceName = WebSearchService.ReadString(profile, "name");
            }

            if (string.IsNullOrWhiteSpace(sourceName))
            {
                sourceName = WebSearchService.GetHostName(url);
            }

            results.Add(new WebSearchResult
            {
                Rank = results.Count + 1,
                Title = WebSearchService.CleanText(WebSearchService.ReadString(item, "title")),
                Url = url.Trim(),
                Snippet = WebSearchService.CleanText(WebSearchService.ReadString(item, "description")),
                SourceName = WebSearchService.CleanText(sourceName),
                PublishedAt = WebSearchService.CleanText(WebSearchService.ReadString(item, "age")),
            });
        }

        return results;
    }
}

internal sealed record WebSearchRequest
{
    public string Query { get; init; } = string.Empty;
    public string Provider { get; init; } = "auto";
    public int MaxResults { get; init; } = 10;
    public string Country { get; init; } = "us";
    public string Language { get; init; } = "en";
    public int Page { get; init; } = 1;
}

internal sealed record WebSearchResult
{
    public int Rank { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string Snippet { get; init; } = string.Empty;
    public string SourceName { get; init; } = string.Empty;
    public string PublishedAt { get; init; } = string.Empty;
}

internal sealed record WebSearchResponse
{
    public bool Success { get; init; }
    public int StatusCode { get; init; } = 200;
    public string ErrorMessage { get; init; } = string.Empty;
    public string ErrorType { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string Query { get; init; } = string.Empty;
    public IReadOnlyList<WebSearchResult> Results { get; init; } = Array.Empty<WebSearchResult>();

    public static WebSearchResponse Fail(
        int statusCode,
        string message,
        string errorType,
        string provider = "",
        string query = "")
    {
        return new WebSearchResponse
        {
            Success = false,
            StatusCode = statusCode,
            ErrorMessage = message,
            ErrorType = errorType,
            Provider = provider,
            Query = query,
        };
    }
}
