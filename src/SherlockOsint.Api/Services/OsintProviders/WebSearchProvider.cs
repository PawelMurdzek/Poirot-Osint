using SherlockOsint.Shared.Models;
using System.Text.RegularExpressions;
using System.Web;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// DuckDuckGo web search integration
/// Uses the Instant Answer API (no API key required)
/// </summary>
public class WebSearchProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebSearchProvider> _logger;

    private const string DdgApiUrl = "https://api.duckduckgo.com/";
    private const string DdgHtmlSearchUrl = "https://html.duckduckgo.com/html/";

    public WebSearchProvider(IHttpClientFactory httpClientFactory, ILogger<WebSearchProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Search for a person's digital footprint using DuckDuckGo
    /// </summary>
    public async Task<List<OsintNode>> SearchAsync(string query, CancellationToken ct = default)
    {
        var results = new List<OsintNode>();

        if (string.IsNullOrWhiteSpace(query))
            return results;

        try
        {
            var client = _httpClientFactory.CreateClient("OsintClient");
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Add("User-Agent", "SherlockOsint-Security-Tool");

            var encodedQuery = HttpUtility.UrlEncode(query);
            var url = $"{DdgApiUrl}?q={encodedQuery}&format=json&no_redirect=1&no_html=1";

            var response = await client.GetAsync(url, ct);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(ct);
                var ddgResponse = System.Text.Json.JsonSerializer.Deserialize<DdgResponse>(json, 
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (ddgResponse != null)
                {
                    if (!string.IsNullOrEmpty(ddgResponse.Abstract))
                    {
                        results.Add(new OsintNode
                        {
                            Id = Guid.NewGuid().ToString(),
                            Label = "Web Summary",
                            Value = ddgResponse.Abstract,
                            Depth = 1,
                            Children = new List<OsintNode>
                            {
                                new OsintNode
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    Label = "Source",
                                    Value = ddgResponse.AbstractURL ?? "DuckDuckGo",
                                    Depth = 2
                                }
                            }
                        });
                    }

                    if (ddgResponse.RelatedTopics?.Count > 0)
                    {
                        var relatedNode = new OsintNode
                        {
                            Id = Guid.NewGuid().ToString(),
                            Label = $"Related Results ({ddgResponse.RelatedTopics.Count})",
                            Value = "Web search results",
                            Depth = 1,
                            Children = new List<OsintNode>()
                        };

                        foreach (var topic in ddgResponse.RelatedTopics.Take(10))
                        {
                            if (!string.IsNullOrEmpty(topic.Text))
                            {
                                relatedNode.Children.Add(new OsintNode
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    Label = "Result",
                                    Value = TruncateText(topic.Text, 150),
                                    Depth = 2,
                                    Children = !string.IsNullOrEmpty(topic.FirstURL) 
                                        ? new List<OsintNode>
                                        {
                                            new OsintNode
                                            {
                                                Id = Guid.NewGuid().ToString(),
                                                Label = "Link",
                                                Value = topic.FirstURL,
                                                Depth = 3
                                            }
                                        }
                                        : new List<OsintNode>()
                                });
                            }
                        }

                        if (relatedNode.Children.Count > 0)
                        {
                            results.Add(relatedNode);
                        }
                    }

                    if (ddgResponse.Infobox?.Content?.Count > 0)
                    {
                        var infoNode = new OsintNode
                        {
                            Id = Guid.NewGuid().ToString(),
                            Label = "Profile Info",
                            Value = "Structured data from web",
                            Depth = 1,
                            Children = new List<OsintNode>()
                        };

                        foreach (var item in ddgResponse.Infobox.Content.Take(10))
                        {
                            if (!string.IsNullOrEmpty(item.Label) && !string.IsNullOrEmpty(item.Value))
                            {
                                infoNode.Children.Add(new OsintNode
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    Label = item.Label,
                                    Value = item.Value,
                                    Depth = 2
                                });
                            }
                        }

                        if (infoNode.Children.Count > 0)
                        {
                            results.Add(infoNode);
                        }
                    }

                    _logger.LogInformation("DuckDuckGo search complete for: {Query}", query);
                }
            }
        }
        catch (TaskCanceledException)
        {
            _logger.LogDebug("Web search timed out for {Query}", query);
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Web search failed: {Error}", ex.Message);
        }

        return results;
    }

    /// <summary>
    /// Discover candidate LinkedIn profiles for a real name by running a site-scoped
    /// search on DuckDuckGo's HTML endpoint (`html.duckduckgo.com/html/`). The Instant
    /// Answer API used by <see cref="SearchAsync"/> rarely surfaces LinkedIn snippets;
    /// the HTML endpoint exposes regular web search results that can be parsed without
    /// a JavaScript engine. No API key required.
    ///
    /// Returns one OsintNode per discovered linkedin.com/in/ URL, with the page title
    /// (typically "Name - Headline | LinkedIn") as a child for downstream verification.
    /// </summary>
    public async Task<List<OsintNode>> SearchLinkedInProfilesAsync(string fullName, CancellationToken ct = default)
    {
        var results = new List<OsintNode>();
        if (string.IsNullOrWhiteSpace(fullName)) return results;

        try
        {
            var client = _httpClientFactory.CreateClient("OsintClient");
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

            var query = $"site:linkedin.com/in/ \"{fullName.Trim()}\"";
            var url = $"{DdgHtmlSearchUrl}?q={HttpUtility.UrlEncode(query)}";

            var response = await client.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogDebug("DDG HTML search returned {Status} for LinkedIn lookup of {Name}", response.StatusCode, fullName);
                return results;
            }

            var html = await response.Content.ReadAsStringAsync(ct);
            var hits = ParseDdgHtmlResults(html);

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var parent = new OsintNode
            {
                Id = Guid.NewGuid().ToString(),
                Label = "[LI] LinkedIn (web)",
                Value = $"DDG site search: site:linkedin.com/in/ \"{fullName.Trim()}\"",
                Depth = 1,
                Children = new List<OsintNode>()
            };

            foreach (var hit in hits)
            {
                // DDG returns its own redirect URLs — unwrap them to a real
                // linkedin.com/in/<handle> URL so the downstream URL-normalised
                // dedup in RealSearchService can compare them sanely.
                var resolved = UnwrapDdgRedirect(hit.Url);
                if (!resolved.Contains("linkedin.com/in/", StringComparison.OrdinalIgnoreCase)) continue;
                if (!seen.Add(resolved)) continue;

                var profileNode = new OsintNode
                {
                    Id = Guid.NewGuid().ToString(),
                    Label = "Profile",
                    Value = resolved,
                    Depth = 2,
                    Children = new List<OsintNode>()
                };

                if (!string.IsNullOrWhiteSpace(hit.Title))
                {
                    profileNode.Children.Add(new OsintNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        Label = "Title",
                        Value = TruncateText(hit.Title, 200),
                        Depth = 3
                    });
                }

                if (!string.IsNullOrWhiteSpace(hit.Snippet))
                {
                    profileNode.Children.Add(new OsintNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        Label = "Snippet",
                        Value = TruncateText(hit.Snippet, 250),
                        Depth = 3
                    });
                }

                parent.Children.Add(profileNode);
                if (parent.Children.Count >= 10) break; // cap noise
            }

            if (parent.Children.Count > 0)
            {
                results.Add(parent);
                _logger.LogInformation("LinkedIn DDG lookup for {Name}: {Count} candidate profile(s)",
                    fullName, parent.Children.Count);
            }
            else
            {
                _logger.LogDebug("LinkedIn DDG lookup for {Name}: no parseable results", fullName);
            }
        }
        catch (TaskCanceledException)
        {
            _logger.LogDebug("LinkedIn DDG lookup timed out for {Name}", fullName);
        }
        catch (Exception ex)
        {
            _logger.LogDebug("LinkedIn DDG lookup failed: {Error}", ex.Message);
        }

        return results;
    }

    /// <summary>
    /// Parse DuckDuckGo's HTML search results page. The page is intentionally
    /// minimal HTML (no JS), so a regex over &lt;a class="result__a" href=...&gt;
    /// + &lt;a class="result__snippet"&gt; is enough.
    /// </summary>
    private static List<DdgHtmlHit> ParseDdgHtmlResults(string html)
    {
        var hits = new List<DdgHtmlHit>();
        if (string.IsNullOrEmpty(html)) return hits;

        var resultRegex = new Regex(
            @"<a[^>]*class=""[^""]*result__a[^""]*""[^>]*href=""([^""]+)""[^>]*>(.*?)</a>",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);
        var snippetRegex = new Regex(
            @"<a[^>]*class=""[^""]*result__snippet[^""]*""[^>]*>(.*?)</a>",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        var titleMatches = resultRegex.Matches(html);
        var snippetMatches = snippetRegex.Matches(html);

        for (var i = 0; i < titleMatches.Count; i++)
        {
            var url = HttpUtility.HtmlDecode(titleMatches[i].Groups[1].Value);
            var titleHtml = titleMatches[i].Groups[2].Value;
            var snippetHtml = i < snippetMatches.Count ? snippetMatches[i].Groups[1].Value : "";

            hits.Add(new DdgHtmlHit
            {
                Url = url,
                Title = StripTags(titleHtml).Trim(),
                Snippet = StripTags(snippetHtml).Trim()
            });
        }

        return hits;
    }

    /// <summary>
    /// DDG wraps result URLs in `/l/?uddg=&lt;encoded-target&gt;`. Returns the
    /// decoded target when the input matches that shape, or the input unchanged.
    /// </summary>
    private static string UnwrapDdgRedirect(string url)
    {
        if (string.IsNullOrEmpty(url)) return "";
        // Handle protocol-relative DDG redirects like //duckduckgo.com/l/?uddg=...
        var normalised = url.StartsWith("//") ? "https:" + url : url;
        if (!normalised.Contains("duckduckgo.com/l/", StringComparison.OrdinalIgnoreCase)
            && !normalised.StartsWith("/l/", StringComparison.OrdinalIgnoreCase))
        {
            return normalised;
        }

        var idx = normalised.IndexOf("uddg=", StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return normalised;
        var raw = normalised[(idx + "uddg=".Length)..];
        var amp = raw.IndexOf('&');
        if (amp >= 0) raw = raw[..amp];
        try
        {
            return HttpUtility.UrlDecode(raw) ?? normalised;
        }
        catch
        {
            return normalised;
        }
    }

    private static string StripTags(string html)
    {
        if (string.IsNullOrEmpty(html)) return "";
        var noTags = Regex.Replace(html, "<[^>]+>", "");
        return HttpUtility.HtmlDecode(noTags);
    }

    private sealed class DdgHtmlHit
    {
        public string Url { get; set; } = "";
        public string Title { get; set; } = "";
        public string Snippet { get; set; } = "";
    }

    private static string TruncateText(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;
        return text.Substring(0, maxLength) + "...";
    }

    #region Response Models

    private class DdgResponse
    {
        public string? Abstract { get; set; }
        public string? AbstractText { get; set; }
        public string? AbstractSource { get; set; }
        public string? AbstractURL { get; set; }
        public string? Image { get; set; }
        public string? Heading { get; set; }
        public List<DdgTopic>? RelatedTopics { get; set; }
        public DdgInfobox? Infobox { get; set; }
    }

    private class DdgTopic
    {
        public string? Text { get; set; }
        public string? FirstURL { get; set; }
        public string? Icon { get; set; }
    }

    private class DdgInfobox
    {
        public List<DdgInfoItem>? Content { get; set; }
    }

    private class DdgInfoItem
    {
        public string? Label { get; set; }
        public string? Value { get; set; }
        public string? DataType { get; set; }
    }

    #endregion
}
