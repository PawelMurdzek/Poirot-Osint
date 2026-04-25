using SherlockOsint.Shared.Models;
using SherlockOsint.Api.Services;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// GitLab commit email scraping - same technique as GitHub
/// Scrapes emails from public repository commits
/// </summary>
public class GitLabSearch
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GitLabSearch> _logger;

    public GitLabSearch(IHttpClientFactory httpClientFactory, ILogger<GitLabSearch> logger)
    {
        _httpClient = httpClientFactory.CreateClient("OsintClient");
        _logger = logger;
    }

    public async Task<List<OsintNode>> SearchAsync(string fullName, string? email, CancellationToken ct = default)
    {
        var results = new List<OsintNode>();

        if (!string.IsNullOrWhiteSpace(fullName))
        {
            var nameResults = await SearchUsersAsync(fullName, ct);
            results.AddRange(nameResults);

            // Diacritic fallback — same rationale as GitHubSearch / OrcidLookup.
            if (nameResults.Count == 0 && TextNormalization.HasDiacritics(fullName))
            {
                var asciiResults = await SearchUsersAsync(TextNormalization.StripDiacritics(fullName), ct);
                results.AddRange(asciiResults);
            }
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            // Currently GitLab search API doesn't support searching by email directly like GitHub
            // But we can search for the email string in the search API
            var emailResults = await SearchUsersAsync(email, ct);
            results.AddRange(emailResults);
        }

        return results.GroupBy(n => n.Value).Select(g => g.First()).ToList();
    }

    /// <summary>
    /// Search GitLab for users by name or username
    /// </summary>
    public async Task<List<OsintNode>> SearchUsersAsync(string query, CancellationToken ct = default)
    {
        var results = new List<OsintNode>();

        if (string.IsNullOrWhiteSpace(query))
            return results;

        try
        {
            // GitLab public API - no auth needed for public users
            var url = $"https://gitlab.com/api/v4/users?search={Uri.EscapeDataString(query)}&per_page=5";
            
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "SherlockOSINT/1.0");

            var response = await _httpClient.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogDebug("GitLab search returned {Status}", response.StatusCode);
                return results;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            var users = JsonDocument.Parse(json);

            foreach (var user in users.RootElement.EnumerateArray())
            {
                var username = user.TryGetProperty("username", out var u) ? u.GetString() : "";
                var name = user.TryGetProperty("name", out var n) ? n.GetString() : "";
                var webUrl = user.TryGetProperty("web_url", out var w) ? w.GetString() : "";
                var avatarUrl = user.TryGetProperty("avatar_url", out var a) ? a.GetString() : "";
                var bio = user.TryGetProperty("bio", out var b) ? b.GetString() : "";
                var location = user.TryGetProperty("location", out var l) ? l.GetString() : "";

                if (string.IsNullOrEmpty(username))
                    continue;

                var children = new List<OsintNode>
                {
                    new() { Id = Guid.NewGuid().ToString(), Label = "Username", Value = username, Depth = 2 }
                };

                if (!string.IsNullOrEmpty(name))
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Name", Value = name, Depth = 2 });

                if (!string.IsNullOrEmpty(location))
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Location", Value = location, Depth = 2 });

                if (!string.IsNullOrEmpty(bio))
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Bio", Value = bio, Depth = 2 });

                if (!string.IsNullOrEmpty(avatarUrl))
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Photo", Value = avatarUrl, Depth = 2 });

                // Try to scrape commit emails
                var commitEmails = await ScrapeCommitEmailsAsync(username, ct);
                children.AddRange(commitEmails);

                results.Add(new OsintNode
                {
                    Id = Guid.NewGuid().ToString(),
                    Label = "[GL] GitLab User",
                    Value = webUrl ?? $"https://gitlab.com/{username}",
                    Depth = 1,
                    Children = children
                });

                _logger.LogInformation("Found GitLab user: {Username}", username);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("GitLab search failed: {Error}", ex.Message);
        }

        return results;
    }

    /// <summary>
    /// Scrape emails from GitLab commit history
    /// </summary>
    public async Task<List<OsintNode>> ScrapeCommitEmailsAsync(string username, CancellationToken ct = default)
    {
        var results = new List<OsintNode>();
        var foundEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            // Get user's public projects
            var projectsUrl = $"https://gitlab.com/api/v4/users/{username}/projects?visibility=public&per_page=5";
            
            using var projectsRequest = new HttpRequestMessage(HttpMethod.Get, projectsUrl);
            projectsRequest.Headers.Add("User-Agent", "SherlockOSINT/1.0");

            var projectsResponse = await _httpClient.SendAsync(projectsRequest, ct);

            if (!projectsResponse.IsSuccessStatusCode)
                return results;

            var projectsJson = await projectsResponse.Content.ReadAsStringAsync(ct);
            var projects = JsonDocument.Parse(projectsJson);

            foreach (var project in projects.RootElement.EnumerateArray().Take(3))
            {
                var projectId = project.TryGetProperty("id", out var id) ? id.GetInt32() : 0;
                var projectName = project.TryGetProperty("name", out var pn) ? pn.GetString() : "";

                if (projectId == 0)
                    continue;

                // Get commits from project
                var commitsUrl = $"https://gitlab.com/api/v4/projects/{projectId}/repository/commits?per_page=20";
                
                using var commitsRequest = new HttpRequestMessage(HttpMethod.Get, commitsUrl);
                commitsRequest.Headers.Add("User-Agent", "SherlockOSINT/1.0");

                var commitsResponse = await _httpClient.SendAsync(commitsRequest, ct);

                if (!commitsResponse.IsSuccessStatusCode)
                    continue;

                var commitsJson = await commitsResponse.Content.ReadAsStringAsync(ct);
                var commits = JsonDocument.Parse(commitsJson);

                foreach (var commit in commits.RootElement.EnumerateArray())
                {
                    var authorEmail = commit.TryGetProperty("author_email", out var ae) ? ae.GetString() : "";
                    var authorName = commit.TryGetProperty("author_name", out var an) ? an.GetString() : "";
                    var committerEmail = commit.TryGetProperty("committer_email", out var ce) ? ce.GetString() : "";

                    // Add author email
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
                            Depth = 2
                        });

                        _logger.LogInformation("Found GitLab commit email: {Email}", authorEmail);
                    }

                    // Add committer email if different
                    if (!string.IsNullOrEmpty(committerEmail) && 
                        !committerEmail.Contains("noreply") &&
                        !foundEmails.Contains(committerEmail))
                    {
                        foundEmails.Add(committerEmail);
                        results.Add(new OsintNode
                        {
                            Id = Guid.NewGuid().ToString(),
                            Label = "[COMMIT] Committer Email",
                            Value = committerEmail,
                            Depth = 2
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("GitLab commit scraping failed: {Error}", ex.Message);
        }

        return results;
    }
}
