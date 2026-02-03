using SherlockOsint.Shared.Models;

namespace SherlockOsint.Api.Services;

/// <summary>
/// Aggregates OsintNode results into a consolidated DigitalProfile
/// </summary>
public class ProfileAggregator
{
    private readonly ILogger<ProfileAggregator> _logger;

    public ProfileAggregator(ILogger<ProfileAggregator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Build a DigitalProfile from search results
    /// </summary>
    public DigitalProfile BuildProfile(SearchRequest request, List<OsintNode> results)
    {
        var profile = new DigitalProfile
        {
            Name = request.FullName,
            Email = request.Email,
            Phone = request.Phone
        };

        var platforms = new List<FoundPlatform>();
        var usernames = new HashSet<string>();

        foreach (var node in results)
        {
            ProcessNode(node, profile, platforms, usernames);
            
            // Also process children
            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    ProcessChildNode(child, profile);
                }
            }
        }

        profile.Platforms = platforms;
        profile.Usernames = usernames.ToList();
        
        // Set primary username (most common)
        if (usernames.Any())
        {
            profile.PrimaryUsername = usernames.First();
        }

        // Calculate confidence score based on data found
        profile.ConfidenceScore = CalculateConfidence(profile);

        _logger.LogInformation("Built profile with {PlatformCount} platforms, confidence {Score}%",
            platforms.Count, profile.ConfidenceScore);

        return profile;
    }

    private void ProcessNode(OsintNode node, DigitalProfile profile, List<FoundPlatform> platforms, HashSet<string> usernames)
    {
        var label = node.Label?.ToLowerInvariant() ?? "";
        var value = node.Value ?? "";

        // Platform detection
        if (IsPlatformNode(label) && !string.IsNullOrEmpty(value) && value.StartsWith("http"))
        {
            var platformName = ExtractPlatformName(label, value);
            var username = ExtractUsernameFromUrl(value);
            
            platforms.Add(new FoundPlatform
            {
                Name = platformName,
                Url = value,
                Username = username,
                Icon = GetPlatformIcon(platformName),
                Category = GetPlatformCategory(platformName)
            });

            if (!string.IsNullOrEmpty(username))
            {
                usernames.Add(username);
            }
        }

        // Photo URL
        if (label.Contains("photo") || label.Contains("avatar"))
        {
            if (string.IsNullOrEmpty(profile.PhotoUrl))
            {
                profile.PhotoUrl = value;
            }
        }

        // Country from phone
        if (label.Contains("country") && !label.Contains("code"))
        {
            if (string.IsNullOrEmpty(profile.Country))
            {
                profile.Country = value;
            }
        }

        // Phone carrier
        if (label.Contains("carrier"))
        {
            profile.PhoneCarrier = value;
        }
    }

    private void ProcessChildNode(OsintNode child, DigitalProfile profile)
    {
        var label = child.Label?.ToLowerInvariant() ?? "";
        var value = child.Value ?? "";

        // Photo URL from children
        if (label.Contains("photo") || label.Contains("avatar"))
        {
            if (string.IsNullOrEmpty(profile.PhotoUrl))
            {
                profile.PhotoUrl = value;
            }
        }

        // Location
        if (label.Contains("location") && !string.IsNullOrEmpty(value))
        {
            profile.Location = value;
        }

        // Bio
        if (label.Contains("bio") && !string.IsNullOrEmpty(value))
        {
            profile.Bio = value;
        }

        // Company
        if (label.Contains("company") && !string.IsNullOrEmpty(value))
        {
            profile.Company = value;
        }

        // Website
        if (label.Contains("website") || label.Contains("blog"))
        {
            if (!string.IsNullOrEmpty(value))
            {
                profile.Website = value;
            }
        }

        // Twitter
        if (label.Contains("twitter") && !string.IsNullOrEmpty(value))
        {
            profile.Twitter = value;
        }
    }

    private bool IsPlatformNode(string label)
    {
        var platformLabels = new[] { "github", "twitch", "pypi", "steam", "replit", "reddit", 
            "gitlab", "soundcloud", "gravatar", "linkedin", "twitter", "instagram", "youtube" };
        return platformLabels.Any(p => label.Contains(p));
    }

    private string ExtractPlatformName(string label, string url)
    {
        // Try to get from URL first
        if (url.Contains("github.com")) return "GitHub";
        if (url.Contains("gitlab.com")) return "GitLab";
        if (url.Contains("twitter.com") || url.Contains("x.com")) return "Twitter/X";
        if (url.Contains("twitch.tv")) return "Twitch";
        if (url.Contains("pypi.org")) return "PyPI";
        if (url.Contains("steamcommunity.com")) return "Steam";
        if (url.Contains("replit.com")) return "Replit";
        if (url.Contains("reddit.com")) return "Reddit";
        if (url.Contains("soundcloud.com")) return "SoundCloud";
        if (url.Contains("gravatar.com")) return "Gravatar";
        if (url.Contains("linkedin.com")) return "LinkedIn";
        if (url.Contains("instagram.com")) return "Instagram";
        if (url.Contains("youtube.com")) return "YouTube";

        // Fall back to label
        return label.Replace("user", "").Trim();
    }

    private string? ExtractUsernameFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            var path = uri.AbsolutePath.Trim('/');
            
            // Remove common prefixes
            foreach (var prefix in new[] { "user/", "users/", "@", "u/", "id/" })
            {
                if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    path = path.Substring(prefix.Length);
                }
            }
            
            // Take first segment
            var segments = path.Split('/');
            return segments.FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    private string GetPlatformIcon(string platform)
    {
        return platform.ToLower() switch
        {
            "github" => "🐙",
            "gitlab" => "🦊",
            "twitter/x" => "🐦",
            "twitch" => "🎮",
            "steam" => "🎮",
            "reddit" => "🤖",
            "soundcloud" => "🎵",
            "youtube" => "[YT]",
            "linkedin" => "💼",
            "instagram" => "📷",
            "pypi" => "🐍",
            "replit" => "💻",
            _ => "[--]"
        };
    }

    private string GetPlatformCategory(string platform)
    {
        return platform.ToLower() switch
        {
            "github" or "gitlab" or "replit" or "pypi" => "Development",
            "twitter/x" or "instagram" or "reddit" => "Social",
            "twitch" or "youtube" or "soundcloud" or "steam" => "Entertainment",
            "linkedin" => "Professional",
            _ => "Other"
        };
    }

    private int CalculateConfidence(DigitalProfile profile)
    {
        int score = 0;

        // Base points for having data
        if (!string.IsNullOrEmpty(profile.Name)) score += 10;
        if (!string.IsNullOrEmpty(profile.Email)) score += 10;
        if (!string.IsNullOrEmpty(profile.Phone)) score += 5;
        if (!string.IsNullOrEmpty(profile.PhotoUrl)) score += 15;
        if (!string.IsNullOrEmpty(profile.Location)) score += 10;
        if (!string.IsNullOrEmpty(profile.Bio)) score += 10;
        if (!string.IsNullOrEmpty(profile.Company)) score += 5;
        if (!string.IsNullOrEmpty(profile.Website)) score += 5;

        // Points for platforms (max 30)
        score += Math.Min(profile.Platforms.Count * 5, 30);

        return Math.Min(score, 100);
    }
}
