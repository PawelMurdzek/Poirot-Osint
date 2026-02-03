using SherlockOsint.Shared.Models;
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
