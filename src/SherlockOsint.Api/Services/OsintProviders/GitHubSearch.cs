using SherlockOsint.Shared.Models;
using SherlockOsint.Api.Services;
using System.Text.Json;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// Searches GitHub for users matching name or email.
/// No API key required for unauthenticated requests (60/hour limit).
/// </summary>
public class GitHubSearch
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GitHubSearch> _logger;

    public GitHubSearch(IHttpClientFactory httpClientFactory, ILogger<GitHubSearch> logger)
    {
        _httpClient = httpClientFactory.CreateClient("OsintClient");
        _logger = logger;
    }

    public async Task<List<OsintNode>> SearchAsync(string fullName, string? email, CancellationToken ct = default)
    {
        var results = new List<OsintNode>();

        // Try searching by name
        if (!string.IsNullOrWhiteSpace(fullName))
        {
            var nameResults = await SearchUsersAsync(fullName, ct);
            results.AddRange(nameResults);

            // Fallback for accented names — many GitHub bios store ASCII form even
            // when the full name has diacritics (e.g. "Władysław" registered as
            // "Wladyslaw"). Only retry when first pass found nothing.
            if (nameResults.Count == 0 && TextNormalization.HasDiacritics(fullName))
            {
                var asciiResults = await SearchUsersAsync(TextNormalization.StripDiacritics(fullName), ct);
                results.AddRange(asciiResults);
            }
        }

        // Try searching by email
        if (!string.IsNullOrWhiteSpace(email))
        {
            var emailResults = await SearchByEmailAsync(email, ct);
            results.AddRange(emailResults);
        }

        return results;
    }

    private async Task<List<OsintNode>> SearchUsersAsync(string query, CancellationToken ct)
    {
        var results = new List<OsintNode>();
        
        try
        {
            var url = $"https://api.github.com/search/users?q={Uri.EscapeDataString(query)}&per_page=5";
            
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "Poirot-OSINT/1.0");
            request.Headers.Add("Accept", "application/vnd.github.v3+json");
            
            var response = await _httpClient.SendAsync(request, ct);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogDebug("GitHub search returned {Status}", response.StatusCode);
                return results;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            var data = JsonDocument.Parse(json);
            
            if (data.RootElement.TryGetProperty("items", out var items))
            {
                foreach (var user in items.EnumerateArray().Take(5))
                {
                    var login = user.TryGetProperty("login", out var l) ? l.GetString() : "";
                    var htmlUrl = user.TryGetProperty("html_url", out var h) ? h.GetString() : "";
                    var avatarUrl = user.TryGetProperty("avatar_url", out var a) ? a.GetString() : "";
                    
                    if (!string.IsNullOrEmpty(htmlUrl))
                    {
                        var children = new List<OsintNode>
                        {
                            new()
                            {
                                Id = Guid.NewGuid().ToString(),
                                Label = "Username",
                                Value = login ?? "",
                                Depth = 2
                            }
                        };
                        
                        // Add avatar URL if available
                        if (!string.IsNullOrEmpty(avatarUrl))
                        {
                            children.Add(new OsintNode
                            {
                                Id = Guid.NewGuid().ToString(),
                                Label = "📷 Profile Photo",
                                Value = avatarUrl,
                                Depth = 2
                            });
                        }
                        
                        // Fetch detailed user info (location, bio)
                        var details = await FetchUserDetailsAsync(login!, ct);
                        children.AddRange(details);
                        
                        results.Add(new OsintNode
                        {
                            Id = Guid.NewGuid().ToString(),
                            Label = "GitHub User",
                            Value = htmlUrl,
                            Depth = 1,
                            Children = children
                        });
                        
                        _logger.LogInformation("Found GitHub user: {Login}", login);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("GitHub user search failed: {Error}", ex.Message);
        }

        return results;
    }

    private async Task<List<OsintNode>> FetchUserDetailsAsync(string username, CancellationToken ct)
    {
        var details = new List<OsintNode>();
        
        try
        {
            var url = $"https://api.github.com/users/{username}";
            
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "Poirot-OSINT/1.0");
            request.Headers.Add("Accept", "application/vnd.github.v3+json");
            
            var response = await _httpClient.SendAsync(request, ct);
            
            if (!response.IsSuccessStatusCode)
                return details;

            var json = await response.Content.ReadAsStringAsync(ct);
            var data = JsonDocument.Parse(json);
            
            // Location
            if (data.RootElement.TryGetProperty("location", out var location))
            {
                var loc = location.GetString();
                if (!string.IsNullOrEmpty(loc))
                {
                    details.Add(new OsintNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        Label = "Location",
                        Value = loc,
                        Depth = 2
                    });
                }
            }
            
            // Bio
            if (data.RootElement.TryGetProperty("bio", out var bio))
            {
                var bioText = bio.GetString();
                if (!string.IsNullOrEmpty(bioText))
                {
                    details.Add(new OsintNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        Label = "📝 Bio",
                        Value = bioText,
                        Depth = 2
                    });
                }
            }
            
            // Company
            if (data.RootElement.TryGetProperty("company", out var company))
            {
                var companyName = company.GetString();
                if (!string.IsNullOrEmpty(companyName))
                {
                    details.Add(new OsintNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        Label = "🏢 Company",
                        Value = companyName,
                        Depth = 2
                    });
                }
            }
            
            // Twitter
            if (data.RootElement.TryGetProperty("twitter_username", out var twitter))
            {
                var twitterHandle = twitter.GetString();
                if (!string.IsNullOrEmpty(twitterHandle))
                {
                    details.Add(new OsintNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        Label = "🐦 Twitter",
                        Value = $"https://twitter.com/{twitterHandle}",
                        Depth = 2
                    });
                }
            }
            
            // Blog/Website
            if (data.RootElement.TryGetProperty("blog", out var blog))
            {
                var blogUrl = blog.GetString();
                if (!string.IsNullOrEmpty(blogUrl))
                {
                    details.Add(new OsintNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        Label = "Website",
                        Value = blogUrl,
                        Depth = 2
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Failed to fetch user details for {Username}: {Error}", username, ex.Message);
        }

        return details;
    }

    private async Task<List<OsintNode>> SearchByEmailAsync(string email, CancellationToken ct)
    {
        var results = new List<OsintNode>();
        
        try
        {
            // GitHub allows searching by email in commits
            var url = $"https://api.github.com/search/users?q={Uri.EscapeDataString(email)}+in:email&per_page=3";
            
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "Poirot-OSINT/1.0");
            request.Headers.Add("Accept", "application/vnd.github.v3+json");
            
            var response = await _httpClient.SendAsync(request, ct);
            
            if (!response.IsSuccessStatusCode)
                return results;

            var json = await response.Content.ReadAsStringAsync(ct);
            var data = JsonDocument.Parse(json);
            
            if (data.RootElement.TryGetProperty("items", out var items))
            {
                foreach (var user in items.EnumerateArray().Take(3))
                {
                    var login = user.TryGetProperty("login", out var l) ? l.GetString() : "";
                    var htmlUrl = user.TryGetProperty("html_url", out var h) ? h.GetString() : "";
                    
                    if (!string.IsNullOrEmpty(htmlUrl))
                    {
                        results.Add(new OsintNode
                        {
                            Id = Guid.NewGuid().ToString(),
                            Label = "GitHub (by email)",
                            Value = htmlUrl,
                            Depth = 1
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("GitHub email search failed: {Error}", ex.Message);
        }

        return results;
    }
}
