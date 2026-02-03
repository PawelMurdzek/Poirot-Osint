using SherlockOsint.Shared.Models;

namespace SherlockOsint.Api.Services;

/// <summary>
/// Interface for the mock search service that simulates OSINT data discovery
/// </summary>
public interface IMockSearchService
{
    /// <summary>
    /// Generates mock OSINT nodes based on search criteria
    /// </summary>
    IAsyncEnumerable<OsintNode> SearchAsync(SearchRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Mock search service that simulates a multi-step OSINT search process
/// </summary>
public class MockSearchService : IMockSearchService
{
    private readonly Random _random = new();

    public async IAsyncEnumerable<OsintNode> SearchAsync(
        SearchRequest request, 
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Step 1: Person root node
        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            var personNode = new OsintNode
            {
                Label = "Person",
                Value = request.FullName,
                Depth = 0
            };
            yield return personNode;
            await Task.Delay(_random.Next(300, 800), cancellationToken);
        }

        // Step 1b: Email node
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var emailNode = new OsintNode
            {
                Label = "Email",
                Value = request.Email,
                Depth = 0
            };
            yield return emailNode;
            await Task.Delay(_random.Next(200, 500), cancellationToken);

            // Step 2: Find social media from email
            foreach (var social in GenerateSocialMediaFromEmail(request.Email))
            {
                social.Depth = 1;
                yield return social;
                await Task.Delay(_random.Next(400, 1000), cancellationToken);
            }
        }

        // Step 1c: Phone node
        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            var phoneNode = new OsintNode
            {
                Label = "Phone",
                Value = request.Phone,
                Depth = 0
            };
            yield return phoneNode;
            await Task.Delay(_random.Next(300, 600), cancellationToken);
        }

        // Step 1d: Nickname search
        if (!string.IsNullOrWhiteSpace(request.Nickname))
        {
            var nicknameNode = new OsintNode
            {
                Label = "Username",
                Value = request.Nickname,
                Depth = 0
            };
            yield return nicknameNode;
            await Task.Delay(_random.Next(200, 400), cancellationToken);

            // Step 2: Find profiles from nickname
            foreach (var profile in GenerateProfilesFromNickname(request.Nickname))
            {
                profile.Depth = 1;
                yield return profile;
                await Task.Delay(_random.Next(500, 1200), cancellationToken);
            }

            // Step 3: Find repositories from nickname
            foreach (var repo in GenerateRepositoriesFromNickname(request.Nickname))
            {
                repo.Depth = 1;
                yield return repo;
                await Task.Delay(_random.Next(300, 700), cancellationToken);
            }
        }

        // Additional discovered data
        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            await Task.Delay(_random.Next(800, 1500), cancellationToken);
            
            foreach (var website in GenerateWebsitesFromName(request.FullName))
            {
                website.Depth = 1;
                yield return website;
                await Task.Delay(_random.Next(400, 900), cancellationToken);
            }
        }
    }

    private IEnumerable<OsintNode> GenerateSocialMediaFromEmail(string email)
    {
        var username = email.Split('@')[0];
        
        if (_random.NextDouble() > 0.3)
        {
            yield return new OsintNode
            {
                Label = "Twitter",
                Value = $"twitter.com/{username}",
            };
        }

        if (_random.NextDouble() > 0.4)
        {
            yield return new OsintNode
            {
                Label = "LinkedIn",
                Value = $"linkedin.com/in/{username}",
            };
        }

        if (_random.NextDouble() > 0.5)
        {
            yield return new OsintNode
            {
                Label = "GitHub",
                Value = $"github.com/{username}",
            };
        }
    }

    private IEnumerable<OsintNode> GenerateProfilesFromNickname(string nickname)
    {
        var platforms = new[] { "Twitter", "Instagram", "TikTok", "Reddit", "Discord", "Steam" };
        var selectedPlatforms = platforms.OrderBy(_ => _random.Next()).Take(_random.Next(2, 5));

        foreach (var platform in selectedPlatforms)
        {
            yield return new OsintNode
            {
                Label = "Social Media",
                Value = $"{platform.ToLower()}.com/{nickname}",
            };
        }
    }

    private IEnumerable<OsintNode> GenerateRepositoriesFromNickname(string nickname)
    {
        var repoNames = new[] { "dotnet-project", "my-website", "cool-tool", "api-client", "data-scraper" };
        var selectedRepos = repoNames.OrderBy(_ => _random.Next()).Take(_random.Next(1, 4));

        foreach (var repo in selectedRepos)
        {
            yield return new OsintNode
            {
                Label = "Repository",
                Value = $"github.com/{nickname}/{repo}",
            };
        }
    }

    private IEnumerable<OsintNode> GenerateWebsitesFromName(string fullName)
    {
        var slug = fullName.ToLower().Replace(" ", "");
        
        if (_random.NextDouble() > 0.4)
        {
            yield return new OsintNode
            {
                Label = "Website",
                Value = $"{slug}.dev",
            };
        }

        if (_random.NextDouble() > 0.6)
        {
            yield return new OsintNode
            {
                Label = "Website",
                Value = $"{slug}.com",
            };
        }
    }
}
