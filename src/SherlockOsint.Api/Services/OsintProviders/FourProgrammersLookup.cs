using SherlockOsint.Shared.Models;
using System.Net;
using System.Text.RegularExpressions;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// 4programmers.net (Polish dev forum / blog hub) profile presence check.
/// No public API — we just GET the profile URL and confirm it returns 200 with the
/// expected username on the page. Used as a yes/no indicator for PL-dev footprint.
/// Endpoint: https://4programmers.net/Profile/{username}
/// </summary>
public class FourProgrammersLookup
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FourProgrammersLookup> _logger;

    public FourProgrammersLookup(IHttpClientFactory httpClientFactory, ILogger<FourProgrammersLookup> logger)
    {
        _httpClient = httpClientFactory.CreateClient("OsintClient");
        _logger = logger;
    }

    public async Task<List<OsintNode>> SearchAsync(string username, CancellationToken ct = default)
    {
        var results = new List<OsintNode>();
        if (string.IsNullOrWhiteSpace(username)) return results;

        try
        {
            var url = $"https://4programmers.net/Profile/{Uri.EscapeDataString(username)}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            request.Headers.Add("Accept", "text/html,application/xhtml+xml");
            request.Headers.Add("Accept-Language", "pl,en;q=0.5");

            var response = await _httpClient.SendAsync(request, ct);
            if (response.StatusCode == HttpStatusCode.NotFound) return results;
            if (!response.IsSuccessStatusCode) return results;

            var html = await response.Content.ReadAsStringAsync(ct);

            // Defensive presence check — 4programmers serves an empty-shell 200 if the user doesn't exist
            if (!html.Contains(username, StringComparison.OrdinalIgnoreCase)) return results;
            if (!html.Contains("class=\"profile", StringComparison.OrdinalIgnoreCase)
                && !html.Contains("user-profile", StringComparison.OrdinalIgnoreCase)
                && !html.Contains("Profil użytkownika", StringComparison.OrdinalIgnoreCase))
            {
                return results;
            }

            var children = new List<OsintNode>
            {
                new() { Id = Guid.NewGuid().ToString(), Label = "Username", Value = username, Depth = 2 }
            };

            // Reputation / posts / register date are inline in HTML — best-effort regex scrape
            var reputationMatch = Regex.Match(html, @"reputacja[^0-9]*([0-9,\.]+)", RegexOptions.IgnoreCase);
            if (reputationMatch.Success)
                children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Reputacja", Value = reputationMatch.Groups[1].Value, Depth = 2 });

            var postsMatch = Regex.Match(html, @"post[óy]w[^0-9]*([0-9,\.]+)", RegexOptions.IgnoreCase);
            if (postsMatch.Success)
                children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Posts", Value = postsMatch.Groups[1].Value, Depth = 2 });

            var registeredMatch = Regex.Match(html, @"(?:zarejestrowany|dolaczy[lł])[^0-9]*([0-9]{4}-[0-9]{2}-[0-9]{2})", RegexOptions.IgnoreCase);
            if (registeredMatch.Success)
                children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Zarejestrowany", Value = registeredMatch.Groups[1].Value, Depth = 2 });

            results.Add(new OsintNode
            {
                Id = Guid.NewGuid().ToString(),
                Label = "4programmers User",
                Value = url,
                Depth = 1,
                Children = children
            });

            _logger.LogInformation("Found 4programmers.net user: {Username}", username);
        }
        catch (Exception ex)
        {
            _logger.LogDebug("4programmers lookup failed for {Username}: {Error}", username, ex.Message);
        }

        return results;
    }
}
