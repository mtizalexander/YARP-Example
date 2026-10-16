using System.Net;
using System.Text.RegularExpressions;

namespace SM.Proxy.Tests;

public sealed class StarMapHttpClient : IDisposable
{
    private readonly HttpClient _httpClient;

    public StarMapHttpClient(string baseAddress)
    {
        if (string.IsNullOrWhiteSpace(baseAddress))
        {
            throw new ArgumentException("Base address is required.", nameof(baseAddress));
        }

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseAddress.TrimEnd('/') + "/")
        };
    }

    public Task<StarMapHttpResult> GetHomeAsync(CancellationToken cancellationToken = default)
        => GetAsync("/", cancellationToken);

    public Task<StarMapHttpResult> GetPrivacyAsync(CancellationToken cancellationToken = default)
        => GetAsync("/Home/Privacy", cancellationToken);

    public Task<StarMapHttpResult> GetKepler90Async(CancellationToken cancellationToken = default)
        => GetAsync("/kepler90/", cancellationToken);

    public Task<StarMapHttpResult> GetProximaCentauriAsync(CancellationToken cancellationToken = default)
        => GetAsync("/proximaCentauri/", cancellationToken);

    public Task<StarMapHttpResult> GetTrappist1Async(CancellationToken cancellationToken = default)
        => GetAsync("/trappist1/", cancellationToken);

    public Task<StarMapHttpResult> GetPathAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path is required.", nameof(path));
        }

        var normalized = path.StartsWith('/') ? path : $"/{path}";
        return GetAsync(normalized, cancellationToken);
    }

    private async Task<StarMapHttpResult> GetAsync(string path, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken)
            .ConfigureAwait(false);

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return ParseResult(path, response, body);
    }

    private static StarMapHttpResult ParseResult(string requestedPath, HttpResponseMessage response, string body)
    {
        var title = ExtractTextFromTag(body, "title");
        var primaryHeading = ExtractTextFromTag(body, "h1");
        var extractedTitles = ExtractCardTitles(body)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new StarMapHttpResult
        {
            RequestUri = response.RequestMessage?.RequestUri?.ToString() ?? requestedPath,
            Path = requestedPath,
            StatusCode = (int)response.StatusCode,
            ReasonPhrase = response.ReasonPhrase ?? string.Empty,
            ContentType = response.Content.Headers.ContentType?.ToString() ?? string.Empty,
            IsSuccessStatusCode = response.IsSuccessStatusCode,
            Body = body,
            Title = title,
            PrimaryHeading = primaryHeading,
            ExtractedTitles = extractedTitles
        };
    }

    private static string? ExtractTextFromTag(string html, string tagName)
    {
        var pattern = $@"<{tagName}[^>]*>\s*(.*?)\s*</{tagName}>";
        var match = Regex.Match(html, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        return match.Success ? StripHtml(match.Groups[1].Value) : null;
    }

    private static IReadOnlyList<string> ExtractCardTitles(string html)
    {
        var patterns = new[]
        {
            @"<h2[^>]*>\s*(?:<a[^>]*>)?\s*(.*?)\s*(?:</a>)?\s*</h2>",
            @"<h2[^>]*class=""[^""]*card-title[^""]*""[^>]*>\s*<a[^>]*>\s*(.*?)\s*</a>\s*</h2>"
        };

        var values = new List<string>();

        foreach (var pattern in patterns)
        {
            var matches = Regex.Matches(html, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            foreach (Match match in matches)
            {
                var value = StripHtml(match.Groups[1].Value);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    values.Add(value);
                }
            }

            if (values.Count > 0)
            {
                break;
            }
        }

        return values;
    }

    private static string StripHtml(string input)
    {
        var withoutTags = Regex.Replace(input, "<[^>]+>", " ", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        var decoded = WebUtility.HtmlDecode(withoutTags);
        return Regex.Replace(decoded, @"\s+", " ").Trim();
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}

public sealed record StarMapHttpResult
{
    public string RequestUri { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public int StatusCode { get; init; }
    public string ReasonPhrase { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public bool IsSuccessStatusCode { get; init; }
    public string Body { get; init; } = string.Empty;
    public string? Title { get; init; }
    public string? PrimaryHeading { get; init; }
    public IReadOnlyList<string> ExtractedTitles { get; init; } = Array.Empty<string>();
}
