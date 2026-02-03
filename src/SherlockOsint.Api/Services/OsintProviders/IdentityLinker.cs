using SherlockOsint.Shared.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// Cross-platform identity linking service
/// Correlates profiles across platforms using shared signals
/// </summary>
public class IdentityLinker
{
    private readonly ILogger<IdentityLinker> _logger;

    // Evidence weights for confidence scoring
    private static class Weights
    {
        public const int EmailVerified = 30;
        public const int PhotoMatch = 25;
        public const int LinkedMention = 20;
        public const int LocationMatch = 15;
        public const int BioSimilar = 10;
        public const int UsernamePattern = 10;
        public const int PlatformTier1 = 15;  // GitHub, LinkedIn, Twitter
        public const int PlatformTier2 = 10;  // Facebook, Instagram, YouTube
        public const int PlatformTier3 = 5;   // Steam, Reddit, etc.
    }

    public IdentityLinker(ILogger<IdentityLinker> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Analyze sources and generate identity linking signals
    /// </summary>
    public List<IdentitySignal> AnalyzeLinks(List<SourceEvidence> sources, SearchRequest request)
    {
        var signals = new List<IdentitySignal>();

        if (sources.Count < 2)
            return signals;

        _logger.LogInformation("Analyzing links across {SourceCount} sources", sources.Count);

        // 1. Check for email verification (Gravatar match)
        signals.AddRange(CheckEmailVerification(sources, request));

        // 2. Check for photo matches
        signals.AddRange(CheckPhotoMatches(sources));

        // 3. Check for location matches
        signals.AddRange(CheckLocationMatches(sources));

        // 4. Check for linked mentions in bios
        signals.AddRange(CheckLinkedMentions(sources));

        // 5. Check for username patterns
        signals.AddRange(CheckUsernamePatterns(sources, request));

        // 6. Check for bio similarity
        signals.AddRange(CheckBioSimilarity(sources));

        _logger.LogInformation("Found {SignalCount} identity signals", signals.Count);
        return signals;
    }

    /// <summary>
    /// Calculate confidence interval based on signals and sources
    /// </summary>
    public (int Low, int High) CalculateConfidenceInterval(
        int baseScore, 
        List<IdentitySignal> signals, 
        List<SourceEvidence> sources)
    {
        // Base uncertainty based on source count
        int uncertainty = sources.Count switch
        {
            1 => 25,
            2 => 20,
            3 => 15,
            4 => 10,
            _ => 5
        };

        // Reduce uncertainty for high-confidence signals
        int highConfidenceCount = signals.Count(s => s.Confidence == "high");
        uncertainty -= highConfidenceCount * 3;
        uncertainty = Math.Max(uncertainty, 3);

        // Calculate bonuses from signals
        int signalBonus = signals.Sum(s => s.Weight) / 2;

        int low = Math.Max(baseScore - uncertainty, 5);
        int high = Math.Min(baseScore + signalBonus, 95);

        // Ensure low <= high
        if (low > high) low = high;

        return (low, high);
    }

    /// <summary>
    /// Check for email verification via Gravatar
    /// </summary>
    private List<IdentitySignal> CheckEmailVerification(List<SourceEvidence> sources, SearchRequest request)
    {
        var signals = new List<IdentitySignal>();

        var gravatar = sources.FirstOrDefault(s => 
            s.Platform.Equals("Gravatar", StringComparison.OrdinalIgnoreCase));

        if (gravatar != null && !string.IsNullOrEmpty(request.Email))
        {
            signals.Add(new IdentitySignal
            {
                SignalType = "email_verified",
                Source = "Gravatar",
                Weight = Weights.EmailVerified,
                Evidence = $"Email '{request.Email}' verified via Gravatar profile",
                Confidence = "high"
            });
        }

        return signals;
    }

    /// <summary>
    /// Check for matching photos across platforms
    /// Uses simple hash comparison of photo URLs (in real implementation, use perceptual hash)
    /// </summary>
    private List<IdentitySignal> CheckPhotoMatches(List<SourceEvidence> sources)
    {
        var signals = new List<IdentitySignal>();
        var photoSources = sources
            .Where(s => s.ExtractedData.ContainsKey("photo") && 
                       !string.IsNullOrEmpty(s.ExtractedData["photo"]))
            .ToList();

        if (photoSources.Count < 2)
            return signals;

        // Group by photo hash
        var photoGroups = photoSources
            .GroupBy(s => GetPhotoHash(s.ExtractedData["photo"]))
            .Where(g => g.Count() > 1)
            .ToList();

        foreach (var group in photoGroups)
        {
            var platforms = group.Select(s => s.Platform).ToList();
            signals.Add(new IdentitySignal
            {
                SignalType = "photo_match",
                Source = string.Join(" + ", platforms),
                Weight = Weights.PhotoMatch,
                Evidence = $"Same profile photo detected on {string.Join(", ", platforms)}",
                Confidence = "high"
            });
        }

        return signals;
    }

    /// <summary>
    /// Check for matching locations across platforms
    /// </summary>
    private List<IdentitySignal> CheckLocationMatches(List<SourceEvidence> sources)
    {
        var signals = new List<IdentitySignal>();
        var locationSources = sources
            .Where(s => s.ExtractedData.ContainsKey("location") && 
                       !string.IsNullOrEmpty(s.ExtractedData["location"]))
            .ToList();

        if (locationSources.Count < 2)
            return signals;

        // Normalize and group locations
        var locationGroups = locationSources
            .GroupBy(s => NormalizeLocation(s.ExtractedData["location"]))
            .Where(g => g.Count() > 1 && !string.IsNullOrEmpty(g.Key))
            .ToList();

        foreach (var group in locationGroups)
        {
            var platforms = group.Select(s => s.Platform).ToList();
            signals.Add(new IdentitySignal
            {
                SignalType = "location_match",
                Source = string.Join(" + ", platforms),
                Weight = Weights.LocationMatch,
                Evidence = $"Location '{group.Key}' found on {string.Join(", ", platforms)}",
                Confidence = "medium"
            });
        }

        return signals;
    }

    /// <summary>
    /// Check for linked mentions in bios (e.g., "@githubuser" in Twitter bio)
    /// </summary>
    private List<IdentitySignal> CheckLinkedMentions(List<SourceEvidence> sources)
    {
        var signals = new List<IdentitySignal>();
        var bioPattern = new Regex(@"@(\w+)", RegexOptions.IgnoreCase);

        foreach (var source in sources)
        {
            if (!source.ExtractedData.TryGetValue("bio", out var bio) || 
                string.IsNullOrEmpty(bio))
                continue;

            var mentions = bioPattern.Matches(bio);
            foreach (Match mention in mentions)
            {
                var mentionedUsername = mention.Groups[1].Value.ToLower();
                
                // Check if this mention matches another source's username
                var matchingSource = sources.FirstOrDefault(s => 
                    s != source && 
                    NormalizeUsername(s.Username).Equals(mentionedUsername, StringComparison.OrdinalIgnoreCase));

                if (matchingSource != null)
                {
                    signals.Add(new IdentitySignal
                    {
                        SignalType = "linked_mention",
                        Source = $"{source.Platform} -> {matchingSource.Platform}",
                        Weight = Weights.LinkedMention,
                        Evidence = $"{source.Platform} bio mentions @{mentionedUsername} (matches {matchingSource.Platform})",
                        Confidence = "high"
                    });
                }
            }
        }

        return signals;
    }

    /// <summary>
    /// Check for username pattern matches (john_doe, johndoe, john-doe)
    /// </summary>
    private List<IdentitySignal> CheckUsernamePatterns(List<SourceEvidence> sources, SearchRequest request)
    {
        var signals = new List<IdentitySignal>();
        
        var inputUsername = NormalizeUsername(request.Nickname ?? request.FullName ?? "");
        if (string.IsNullOrEmpty(inputUsername))
            return signals;

        // Group sources by normalized username
        var usernameGroups = sources
            .GroupBy(s => NormalizeUsername(s.Username))
            .Where(g => !string.IsNullOrEmpty(g.Key) && g.Count() > 1)
            .ToList();

        foreach (var group in usernameGroups)
        {
            if (group.Key.Equals(inputUsername, StringComparison.OrdinalIgnoreCase))
            {
                var platforms = group.Select(s => s.Platform).ToList();
                if (platforms.Count >= 2)
                {
                    signals.Add(new IdentitySignal
                    {
                        SignalType = "username_pattern",
                        Source = string.Join(" + ", platforms),
                        Weight = Weights.UsernamePattern,
                        Evidence = $"Username pattern '{group.Key}' matches input across {string.Join(", ", platforms)}",
                        Confidence = "medium"
                    });
                }
            }
        }

        return signals;
    }

    /// <summary>
    /// Check for bio similarity across platforms
    /// </summary>
    private List<IdentitySignal> CheckBioSimilarity(List<SourceEvidence> sources)
    {
        var signals = new List<IdentitySignal>();
        var bioSources = sources
            .Where(s => s.ExtractedData.ContainsKey("bio") && 
                       !string.IsNullOrEmpty(s.ExtractedData["bio"]) &&
                       s.ExtractedData["bio"].Length > 20)
            .ToList();

        if (bioSources.Count < 2)
            return signals;

        // Compare bios for significant overlap
        for (int i = 0; i < bioSources.Count; i++)
        {
            for (int j = i + 1; j < bioSources.Count; j++)
            {
                var bio1 = bioSources[i].ExtractedData["bio"];
                var bio2 = bioSources[j].ExtractedData["bio"];

                var similarity = CalculateSimilarity(bio1, bio2);
                if (similarity > 0.6)
                {
                    signals.Add(new IdentitySignal
                    {
                        SignalType = "bio_similar",
                        Source = $"{bioSources[i].Platform} + {bioSources[j].Platform}",
                        Weight = Weights.BioSimilar,
                        Evidence = $"Similar bio text ({(int)(similarity * 100)}% match) on {bioSources[i].Platform} and {bioSources[j].Platform}",
                        Confidence = similarity > 0.8 ? "high" : "medium"
                    });
                }
            }
        }

        return signals;
    }

    /// <summary>
    /// Get platform tier weight
    /// </summary>
    public int GetPlatformWeight(string platform)
    {
        var tier1 = new[] { "github", "linkedin", "twitter", "x" };
        var tier2 = new[] { "facebook", "instagram", "youtube" };

        var lower = platform.ToLower();
        if (tier1.Contains(lower)) return Weights.PlatformTier1;
        if (tier2.Contains(lower)) return Weights.PlatformTier2;
        return Weights.PlatformTier3;
    }

    // Helper: Generate simple hash for photo URL
    private static string GetPhotoHash(string url)
    {
        if (string.IsNullOrEmpty(url)) return "";
        
        // Extract core URL without query params for comparison
        var uri = new Uri(url, UriKind.RelativeOrAbsolute);
        var core = uri.IsAbsoluteUri ? uri.GetLeftPart(UriPartial.Path) : url;
        
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(core));
        return Convert.ToHexString(hash);
    }

    // Helper: Normalize location for comparison
    private static string NormalizeLocation(string location)
    {
        if (string.IsNullOrEmpty(location)) return "";
        
        // Remove country-specific noise, standardize
        return location
            .ToLower()
            .Replace(",", " ")
            .Replace(".", " ")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault() ?? "";
    }

    // Helper: Normalize username
    private static string NormalizeUsername(string username)
    {
        if (string.IsNullOrEmpty(username)) return "";
        return username
            .ToLower()
            .Replace("-", "")
            .Replace("_", "")
            .Replace(".", "")
            .Trim();
    }

    // Helper: Calculate Jaccard similarity between two strings
    private static double CalculateSimilarity(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) return 0;

        var words1 = s1.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        var words2 = s2.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();

        if (words1.Count == 0 || words2.Count == 0) return 0;

        var intersection = words1.Intersect(words2).Count();
        var union = words1.Union(words2).Count();

        return (double)intersection / union;
    }
}
