using SherlockOsint.Shared.Models;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// Real email-to-identity discovery service
/// Uses multiple techniques to find usernames/accounts from email
/// </summary>
public class EmailDiscovery
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EmailDiscovery> _logger;

    public EmailDiscovery(IHttpClientFactory httpClientFactory, ILogger<EmailDiscovery> logger)
    {
        _httpClient = httpClientFactory.CreateClient("OsintClient");
        _logger = logger;
    }

    /// <summary>
    /// Discover usernames and accounts from email address
    /// </summary>
    public async Task<List<OsintNode>> DiscoverFromEmailAsync(string email, CancellationToken ct = default)
    {
        var results = new List<OsintNode>();
        
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return results;

        _logger.LogInformation("Starting email discovery for: {Email}", email);

        // 1. GitHub email search (public emails only)
        var githubResults = await SearchGitHubByEmailAsync(email, ct);
        results.AddRange(githubResults);

        // 2. Gravatar profile extraction
        var gravatarResults = await GetGravatarProfileAsync(email, ct);
        results.AddRange(gravatarResults);

        // 3. If we found GitHub users, scrape their commit emails
        foreach (var node in githubResults.Where(n => n.Label?.Contains("GitHub") == true))
        {
            var username = ExtractGitHubUsername(node);
            if (!string.IsNullOrEmpty(username))
            {
                var commitEmails = await ScrapeGitHubCommitEmailsAsync(username, ct);
                results.AddRange(commitEmails);
            }
        }

        _logger.LogInformation("Email discovery complete. Found {Count} results", results.Count);
        return results;
    }

    /// <summary>
    /// Discover emails from a known username (reverse lookup)
    /// </summary>
    public async Task<List<OsintNode>> DiscoverEmailsFromUsernameAsync(string username, CancellationToken ct = default)
    {
        var results = new List<OsintNode>();
        
        if (string.IsNullOrWhiteSpace(username))
            return results;

        _logger.LogInformation("Starting reverse discovery for username: {Username}", username);

        // Scrape commit emails from GitHub repos
        var commitEmails = await ScrapeGitHubCommitEmailsAsync(username, ct);
        results.AddRange(commitEmails);

        return results;
    }

    /// <summary>
    /// Search GitHub for users with this email (only works for PUBLIC emails)
    /// </summary>
    private async Task<List<OsintNode>> SearchGitHubByEmailAsync(string email, CancellationToken ct)
    {
        var results = new List<OsintNode>();

        try
        {
            // GitHub API: search users by email
            var url = $"https://api.github.com/search/users?q={Uri.EscapeDataString(email)}+in:email";
            
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "SherlockOSINT/1.0");
            request.Headers.Add("Accept", "application/vnd.github.v3+json");

            var response = await _httpClient.SendAsync(request, ct);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogDebug("GitHub email search returned {Status}", response.StatusCode);
                return results;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            var data = JsonDocument.Parse(json);

            if (data.RootElement.TryGetProperty("total_count", out var countProp))
            {
                var count = countProp.GetInt32();
                _logger.LogInformation("GitHub email search found {Count} users", count);
            }

            if (data.RootElement.TryGetProperty("items", out var items))
            {
                foreach (var user in items.EnumerateArray())
                {
                    var login = user.TryGetProperty("login", out var l) ? l.GetString() : "";
                    var htmlUrl = user.TryGetProperty("html_url", out var h) ? h.GetString() : "";
                    var avatarUrl = user.TryGetProperty("avatar_url", out var a) ? a.GetString() : "";

                    if (!string.IsNullOrEmpty(login))
                    {
                        results.Add(new OsintNode
                        {
                            Id = Guid.NewGuid().ToString(),
                            Label = "[GH] GitHub (Email Match)",
                            Value = htmlUrl ?? $"https://github.com/{login}",
                            Depth = 1,
                            Children = new List<OsintNode>
                            {
                                new() { Id = Guid.NewGuid().ToString(), Label = "Username", Value = login, Depth = 2 },
                                new() { Id = Guid.NewGuid().ToString(), Label = "Discovery", Value = "Email matched public GitHub profile", Depth = 2 },
                                new() { Id = Guid.NewGuid().ToString(), Label = "Confidence", Value = "HIGH - Email verified", Depth = 2 }
                            }
                        });

                        if (!string.IsNullOrEmpty(avatarUrl))
                        {
                            results.Last().Children.Add(new OsintNode
                            {
                                Id = Guid.NewGuid().ToString(),
                                Label = "Photo",
                                Value = avatarUrl,
                                Depth = 2
                            });
                        }

                        _logger.LogInformation("Found GitHub user via email: {Login}", login);
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

    /// <summary>
    /// Get Gravatar profile data (includes linked accounts)
    /// </summary>
    private async Task<List<OsintNode>> GetGravatarProfileAsync(string email, CancellationToken ct)
    {
        var results = new List<OsintNode>();

        try
        {
            // Compute MD5 hash of lowercase email
            var emailLower = email.Trim().ToLower();
            using var md5 = MD5.Create();
            var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(emailLower));
            var hash = Convert.ToHexString(hashBytes).ToLower();

            // Gravatar JSON profile
            var url = $"https://en.gravatar.com/{hash}.json";
            
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "SherlockOSINT/1.0");

            var response = await _httpClient.SendAsync(request, ct);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Gravatar profile not found for {Email}", email);
                return results;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            var data = JsonDocument.Parse(json);

            if (data.RootElement.TryGetProperty("entry", out var entries))
            {
                foreach (var entry in entries.EnumerateArray())
                {
                    var children = new List<OsintNode>();

                    // Display name
                    if (entry.TryGetProperty("displayName", out var displayName))
                    {
                        var name = displayName.GetString();
                        if (!string.IsNullOrEmpty(name))
                        {
                            children.Add(new OsintNode
                            {
                                Id = Guid.NewGuid().ToString(),
                                Label = "Display Name",
                                Value = name,
                                Depth = 2
                            });
                        }
                    }

                    // Preferred username
                    if (entry.TryGetProperty("preferredUsername", out var username))
                    {
                        var user = username.GetString();
                        if (!string.IsNullOrEmpty(user))
                        {
                            children.Add(new OsintNode
                            {
                                Id = Guid.NewGuid().ToString(),
                                Label = "Username",
                                Value = user,
                                Depth = 2
                            });
                        }
                    }

                    // Linked accounts (the gold!)
                    if (entry.TryGetProperty("accounts", out var accounts))
                    {
                        foreach (var account in accounts.EnumerateArray())
                        {
                            var shortname = account.TryGetProperty("shortname", out var sn) ? sn.GetString() : "";
                            var accountUrl = account.TryGetProperty("url", out var au) ? au.GetString() : "";
                            var accountUsername = account.TryGetProperty("username", out var un) ? un.GetString() : "";

                            if (!string.IsNullOrEmpty(shortname) && !string.IsNullOrEmpty(accountUrl))
                            {
                                children.Add(new OsintNode
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    Label = $"Linked: {shortname}",
                                    Value = accountUrl,
                                    Depth = 2
                                });

                                _logger.LogInformation("Found linked account via Gravatar: {Platform} -> {Url}", shortname, accountUrl);
                            }
                        }
                    }

                    // Photo
                    if (entry.TryGetProperty("thumbnailUrl", out var thumbUrl))
                    {
                        var photo = thumbUrl.GetString();
                        if (!string.IsNullOrEmpty(photo))
                        {
                            children.Add(new OsintNode
                            {
                                Id = Guid.NewGuid().ToString(),
                                Label = "Photo",
                                Value = photo,
                                Depth = 2
                            });
                        }
                    }

                    if (children.Count > 0)
                    {
                        children.Add(new OsintNode
                        {
                            Id = Guid.NewGuid().ToString(),
                            Label = "Discovery",
                            Value = "Profile linked to email via Gravatar",
                            Depth = 2
                        });

                        results.Add(new OsintNode
                        {
                            Id = Guid.NewGuid().ToString(),
                            Label = "[GR] Gravatar Profile",
                            Value = $"https://gravatar.com/{hash}",
                            Depth = 1,
                            Children = children
                        });

                        _logger.LogInformation("Found Gravatar profile with {Count} linked accounts", 
                            children.Count(c => c.Label?.StartsWith("Linked") == true));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Gravatar profile extraction failed: {Error}", ex.Message);
        }

        return results;
    }

    /// <summary>
    /// Scrape emails from GitHub commit history
    /// This is powerful - reveals emails people didn't mean to expose
    /// </summary>
    private async Task<List<OsintNode>> ScrapeGitHubCommitEmailsAsync(string username, CancellationToken ct)
    {
        var results = new List<OsintNode>();
        var foundEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            // 1. Get user's public repos
            var reposUrl = $"https://api.github.com/users/{username}/repos?per_page=10&sort=updated";
            
            using var reposRequest = new HttpRequestMessage(HttpMethod.Get, reposUrl);
            reposRequest.Headers.Add("User-Agent", "SherlockOSINT/1.0");
            reposRequest.Headers.Add("Accept", "application/vnd.github.v3+json");

            var reposResponse = await _httpClient.SendAsync(reposRequest, ct);
            
            if (!reposResponse.IsSuccessStatusCode)
                return results;

            var reposJson = await reposResponse.Content.ReadAsStringAsync(ct);
            var repos = JsonDocument.Parse(reposJson);

            // 2. Check commits in each repo
            foreach (var repo in repos.RootElement.EnumerateArray().Take(5))
            {
                if (repo.TryGetProperty("fork", out var fork) && fork.GetBoolean())
                    continue; // Skip forks

                var repoName = repo.TryGetProperty("name", out var rn) ? rn.GetString() : "";
                if (string.IsNullOrEmpty(repoName))
                    continue;

                // Get recent commits
                var commitsUrl = $"https://api.github.com/repos/{username}/{repoName}/commits?per_page=20";
                
                using var commitsRequest = new HttpRequestMessage(HttpMethod.Get, commitsUrl);
                commitsRequest.Headers.Add("User-Agent", "SherlockOSINT/1.0");
                commitsRequest.Headers.Add("Accept", "application/vnd.github.v3+json");

                var commitsResponse = await _httpClient.SendAsync(commitsRequest, ct);
                
                if (!commitsResponse.IsSuccessStatusCode)
                    continue;

                var commitsJson = await commitsResponse.Content.ReadAsStringAsync(ct);
                var commits = JsonDocument.Parse(commitsJson);

                foreach (var commit in commits.RootElement.EnumerateArray())
                {
                    if (!commit.TryGetProperty("commit", out var commitData))
                        continue;

                    // Author email
                    if (commitData.TryGetProperty("author", out var author))
                    {
                        var authorEmail = author.TryGetProperty("email", out var ae) ? ae.GetString() : "";
                        var authorName = author.TryGetProperty("name", out var an) ? an.GetString() : "";
                        
                        if (!string.IsNullOrEmpty(authorEmail) && 
                            !authorEmail.Contains("noreply") && 
                            !foundEmails.Contains(authorEmail))
                        {
                            foundEmails.Add(authorEmail);
                            
                            results.Add(new OsintNode
                            {
                                Id = Guid.NewGuid().ToString(),
                                Label = "[COMMIT] Discovered Email",
                                Value = authorEmail,
                                Depth = 1,
                                Children = new List<OsintNode>
                                {
                                    new() { Id = Guid.NewGuid().ToString(), Label = "Name", Value = authorName ?? "", Depth = 2 },
                                    new() { Id = Guid.NewGuid().ToString(), Label = "Source", Value = $"github.com/{username}/{repoName}", Depth = 2 },
                                    new() { Id = Guid.NewGuid().ToString(), Label = "Discovery", Value = "Scraped from git commit history", Depth = 2 },
                                    new() { Id = Guid.NewGuid().ToString(), Label = "Confidence", Value = "HIGH - Actual email used", Depth = 2 }
                                }
                            });

                            _logger.LogInformation("Found commit email: {Email} for {Username}", authorEmail, username);
                        }
                    }

                    // Committer email (sometimes different)
                    if (commitData.TryGetProperty("committer", out var committer))
                    {
                        var committerEmail = committer.TryGetProperty("email", out var ce) ? ce.GetString() : "";
                        
                        if (!string.IsNullOrEmpty(committerEmail) && 
                            !committerEmail.Contains("noreply") && 
                            !foundEmails.Contains(committerEmail))
                        {
                            foundEmails.Add(committerEmail);
                            
                            results.Add(new OsintNode
                            {
                                Id = Guid.NewGuid().ToString(),
                                Label = "[COMMIT] Discovered Email",
                                Value = committerEmail,
                                Depth = 1,
                                Children = new List<OsintNode>
                                {
                                    new() { Id = Guid.NewGuid().ToString(), Label = "Role", Value = "Committer", Depth = 2 },
                                    new() { Id = Guid.NewGuid().ToString(), Label = "Source", Value = $"github.com/{username}/{repoName}", Depth = 2 },
                                    new() { Id = Guid.NewGuid().ToString(), Label = "Discovery", Value = "Scraped from git commit history", Depth = 2 }
                                }
                            });
                        }
                    }
                }
            }

            if (foundEmails.Count > 0)
            {
                _logger.LogInformation("Scraped {Count} unique emails from {Username}'s commits", foundEmails.Count, username);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("GitHub commit scraping failed: {Error}", ex.Message);
        }

        return results;
    }

    private static string? ExtractGitHubUsername(OsintNode node)
    {
        var usernameChild = node.Children?.FirstOrDefault(c => c.Label == "Username");
        return usernameChild?.Value;
    }
}
