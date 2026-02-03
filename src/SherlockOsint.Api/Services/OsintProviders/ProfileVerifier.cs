using System.Text.RegularExpressions;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// Verifies that a profile actually exists and belongs to the target
/// HTTP 200 is NOT enough - must verify actual content
/// </summary>
public class ProfileVerifier
{
    private readonly ILogger<ProfileVerifier> _logger;

    public ProfileVerifier(ILogger<ProfileVerifier> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Verify that HTML content actually contains the expected profile
    /// </summary>
    public ProfileVerification VerifyContent(string platform, string html, string expectedUsername)
    {
        var verification = new ProfileVerification
        {
            Platform = platform,
            ExpectedUsername = expectedUsername
        };

        if (string.IsNullOrEmpty(html) || html.Length < 100)
        {
            verification.Reason = "Empty or minimal response";
            return verification;
        }

        var normalizedUsername = NormalizeUsername(expectedUsername);
        var lowerHtml = html.ToLower();

        // Check for 404/not found text patterns
        if (IsNotFoundPage(lowerHtml))
        {
            verification.Reason = "Page indicates profile not found";
            return verification;
        }

        // Platform-specific verification
        var result = platform.ToLower() switch
        {
            "github" => VerifyGitHub(html, normalizedUsername),
            "gitlab" => VerifyGitLab(html, normalizedUsername),
            "twitter" or "x" => VerifyTwitter(html, normalizedUsername),
            "instagram" => VerifyInstagram(html, normalizedUsername),
            "youtube" => VerifyYouTube(html, normalizedUsername),
            "twitch" => VerifyTwitch(html, normalizedUsername),
            "reddit" => VerifyReddit(html, normalizedUsername),
            "steam" => VerifySteam(html, normalizedUsername),
            "linkedin" => VerifyLinkedIn(html, normalizedUsername),
            _ => VerifyGeneric(html, normalizedUsername)
        };

        verification.IsVerified = result.IsVerified;
        verification.Confidence = result.Confidence;
        verification.Evidence = result.Evidence;
        verification.DisplayName = result.DisplayName;
        verification.Bio = result.Bio;
        verification.Location = result.Location;
        verification.Reason = result.Reason;

        _logger.LogInformation("Profile verification for {Platform}/{Username}: Verified={Verified}, Confidence={Confidence}",
            platform, expectedUsername, verification.IsVerified, verification.Confidence);

        return verification;
    }

    private bool IsNotFoundPage(string html)
    {
        var notFoundPatterns = new[]
        {
            "page not found",
            "404",
            "this page doesn't exist",
            "user not found",
            "profile not found",
            "account suspended",
            "account doesn't exist",
            "no user with that name",
            "the page you're looking for"
        };

        return notFoundPatterns.Any(p => html.Contains(p));
    }

    private VerificationResult VerifyGitHub(string html, string username)
    {
        var result = new VerificationResult();
        var evidence = new List<string>();

        // GitHub verification patterns
        var usernameInUrl = Regex.IsMatch(html, $@"github\.com/{Regex.Escape(username)}", RegexOptions.IgnoreCase);
        var usernameInMeta = Regex.IsMatch(html, $@"<meta[^>]*content=""@?{Regex.Escape(username)}""", RegexOptions.IgnoreCase);
        var profileData = Regex.IsMatch(html, $@"""login""\s*:\s*""{Regex.Escape(username)}""", RegexOptions.IgnoreCase);
        var userCard = html.Contains("vcard-username") || html.Contains("p-nickname");

        if (usernameInUrl || usernameInMeta || profileData)
        {
            result.IsVerified = true;
            evidence.Add("Username confirmed in page source");
            result.Confidence += 0.40m;
        }

        if (userCard)
        {
            evidence.Add("GitHub user card detected");
            result.Confidence += 0.20m;
        }

        // Extract display name
        var nameMatch = Regex.Match(html, @"<span[^>]*class=""p-name[^""]*""[^>]*>([^<]+)</span>", RegexOptions.IgnoreCase);
        if (nameMatch.Success)
        {
            result.DisplayName = nameMatch.Groups[1].Value.Trim();
            evidence.Add($"Display name: {result.DisplayName}");
            result.Confidence += 0.10m;
        }

        // Extract bio
        var bioMatch = Regex.Match(html, @"<div[^>]*class=""p-note[^""]*""[^>]*>([^<]+)</div>", RegexOptions.IgnoreCase);
        if (bioMatch.Success)
        {
            result.Bio = bioMatch.Groups[1].Value.Trim();
            evidence.Add($"Bio found: {result.Bio.Substring(0, Math.Min(50, result.Bio.Length))}...");
            result.Confidence += 0.10m;
        }

        // Extract location
        var locationMatch = Regex.Match(html, @"<span[^>]*class=""p-label""[^>]*>([^<]+)</span>", RegexOptions.IgnoreCase);
        if (locationMatch.Success)
        {
            result.Location = locationMatch.Groups[1].Value.Trim();
            evidence.Add($"Location: {result.Location}");
            result.Confidence += 0.05m;
        }

        result.Evidence = evidence;
        result.Reason = result.IsVerified ? "GitHub profile verified" : "Could not verify GitHub profile";
        return result;
    }

    private VerificationResult VerifyGitLab(string html, string username)
    {
        var result = new VerificationResult();
        var evidence = new List<string>();

        var usernameInUrl = Regex.IsMatch(html, $@"gitlab\.com/{Regex.Escape(username)}", RegexOptions.IgnoreCase);
        var profilePresent = html.Contains("user-profile") || html.Contains("gl-avatar");

        if (usernameInUrl && profilePresent)
        {
            result.IsVerified = true;
            evidence.Add("Username found in GitLab profile");
            result.Confidence = 0.50m;
        }

        result.Evidence = evidence;
        result.Reason = result.IsVerified ? "GitLab profile verified" : "Could not verify GitLab profile";
        return result;
    }

    private VerificationResult VerifyTwitter(string html, string username)
    {
        var result = new VerificationResult();
        var evidence = new List<string>();

        // Twitter/X patterns
        var screenName = Regex.IsMatch(html, $@"""screen_name""\s*:\s*""{Regex.Escape(username)}""", RegexOptions.IgnoreCase);
        var atUsername = Regex.IsMatch(html, $@"@{Regex.Escape(username)}", RegexOptions.IgnoreCase);

        if (screenName || atUsername)
        {
            result.IsVerified = true;
            evidence.Add("Twitter/X handle confirmed");
            result.Confidence = 0.50m;
        }

        result.Evidence = evidence;
        result.Reason = result.IsVerified ? "Twitter profile verified" : "Could not verify Twitter profile";
        return result;
    }

    private VerificationResult VerifyInstagram(string html, string username)
    {
        var result = new VerificationResult();
        var evidence = new List<string>();

        // Instagram patterns
        var usernameInMeta = Regex.IsMatch(html, $@"instagram\.com/{Regex.Escape(username)}", RegexOptions.IgnoreCase);
        var profilePage = html.Contains("ProfilePage") || html.Contains("graphql/query");

        if (usernameInMeta && profilePage)
        {
            result.IsVerified = true;
            evidence.Add("Instagram profile URL verified");
            result.Confidence = 0.45m;
        }

        result.Evidence = evidence;
        result.Reason = result.IsVerified ? "Instagram profile verified" : "Could not verify Instagram profile";
        return result;
    }

    private VerificationResult VerifyYouTube(string html, string username)
    {
        var result = new VerificationResult();
        var evidence = new List<string>();

        var channelId = Regex.IsMatch(html, @"channelId", RegexOptions.IgnoreCase);
        var usernameMatch = Regex.IsMatch(html, $@"@{Regex.Escape(username)}", RegexOptions.IgnoreCase);

        if (channelId && usernameMatch)
        {
            result.IsVerified = true;
            evidence.Add("YouTube channel verified");
            result.Confidence = 0.50m;
        }

        result.Evidence = evidence;
        result.Reason = result.IsVerified ? "YouTube channel verified" : "Could not verify YouTube channel";
        return result;
    }

    private VerificationResult VerifyTwitch(string html, string username)
    {
        var result = new VerificationResult();
        var evidence = new List<string>();

        var twitchUser = html.Contains("channel-header") || html.Contains("tw-avatar");
        var usernamePresent = Regex.IsMatch(html, $@"twitch\.tv/{Regex.Escape(username)}", RegexOptions.IgnoreCase);

        if (twitchUser && usernamePresent)
        {
            result.IsVerified = true;
            evidence.Add("Twitch channel verified");
            result.Confidence = 0.45m;
        }

        result.Evidence = evidence;
        result.Reason = result.IsVerified ? "Twitch channel verified" : "Could not verify Twitch channel";
        return result;
    }

    private VerificationResult VerifyReddit(string html, string username)
    {
        var result = new VerificationResult();
        var evidence = new List<string>();

        var userPage = html.Contains("profile-") || Regex.IsMatch(html, @"/user/\w+", RegexOptions.IgnoreCase);
        var usernamePresent = Regex.IsMatch(html, $@"u/{Regex.Escape(username)}", RegexOptions.IgnoreCase);

        if (userPage && usernamePresent)
        {
            result.IsVerified = true;
            evidence.Add("Reddit user verified");
            result.Confidence = 0.45m;
        }

        result.Evidence = evidence;
        result.Reason = result.IsVerified ? "Reddit user verified" : "Could not verify Reddit user";
        return result;
    }

    private VerificationResult VerifySteam(string html, string username)
    {
        var result = new VerificationResult();
        var evidence = new List<string>();

        var steamProfile = html.Contains("profile_header") || html.Contains("playerAvatarAutoSizeInner");
        var usernamePresent = html.Contains(username, StringComparison.OrdinalIgnoreCase);

        if (steamProfile && usernamePresent)
        {
            result.IsVerified = true;
            evidence.Add("Steam profile verified");
            result.Confidence = 0.40m;
        }

        result.Evidence = evidence;
        result.Reason = result.IsVerified ? "Steam profile verified" : "Could not verify Steam profile";
        return result;
    }

    private VerificationResult VerifyLinkedIn(string html, string username)
    {
        var result = new VerificationResult();
        var evidence = new List<string>();

        // LinkedIn is very restrictive - we can only verify public pages
        var profilePage = html.Contains("profile-section") || html.Contains("pv-top-card");
        var usernameInUrl = Regex.IsMatch(html, $@"linkedin\.com/in/{Regex.Escape(username)}", RegexOptions.IgnoreCase);

        if (profilePage && usernameInUrl)
        {
            result.IsVerified = true;
            evidence.Add("LinkedIn profile URL verified");
            result.Confidence = 0.35m;  // Lower confidence due to scraping limitations
        }

        result.Evidence = evidence;
        result.Reason = result.IsVerified ? "LinkedIn profile detected (limited)" : "Could not verify LinkedIn profile";
        return result;
    }

    private VerificationResult VerifyGeneric(string html, string username)
    {
        var result = new VerificationResult();
        var evidence = new List<string>();

        // Generic verification - username appears in content
        var usernameCount = Regex.Matches(html, Regex.Escape(username), RegexOptions.IgnoreCase).Count;

        if (usernameCount >= 2)
        {
            result.IsVerified = true;
            evidence.Add($"Username appears {usernameCount} times in page");
            result.Confidence = Math.Min(0.30m + (usernameCount * 0.05m), 0.50m);
        }

        result.Evidence = evidence;
        result.Reason = result.IsVerified ? "Generic profile verification passed" : "Could not verify profile";
        return result;
    }

    private string NormalizeUsername(string username)
    {
        return username.ToLower().Trim();
    }
}

/// <summary>
/// Result of profile verification
/// </summary>
public class ProfileVerification
{
    public bool IsVerified { get; set; }
    public decimal Confidence { get; set; }
    public List<string> Evidence { get; set; } = new();
    public string Platform { get; set; } = "";
    public string ExpectedUsername { get; set; } = "";
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? Location { get; set; }
    public string Reason { get; set; } = "";
}

/// <summary>
/// Internal verification result
/// </summary>
internal class VerificationResult
{
    public bool IsVerified { get; set; }
    public decimal Confidence { get; set; }
    public List<string> Evidence { get; set; } = new();
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? Location { get; set; }
    public string Reason { get; set; } = "";
}
