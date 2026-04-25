using SherlockOsint.Shared.Models;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// Sherlock-style username enumeration across 300+ platforms
/// Checks if a username exists on various social media and websites
/// </summary>
public class UsernameSearch
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<UsernameSearch> _logger;
    private readonly ProfileVerifier _profileVerifier;
    private readonly SemaphoreSlim _semaphore = new(20, 20); // Limit concurrent requests

    // Platforms to check - organized by OSINT importance
    // Priority 1 = Highest importance for identity correlation
    // Priority 6 = Lowest importance (gaming, casual - high false positive risk)
    private static readonly List<PlatformCheck> Platforms = new()
    {
        // Priority 1: HIGH VALUE - Identity-linked Social/Professional
        new("GitHub", "https://github.com/{}", "github.com", 1, "[GH]"),
        new("LinkedIn", "https://www.linkedin.com/in/{}", "linkedin.com", 1, "[LI]"),
        new("X/Twitter", "https://x.com/{}", "x.com", 1, "[X]"),
        new("Instagram", "https://www.instagram.com/{}/", "instagram.com", 1, "[IG]"),
        new("TikTok", "https://www.tiktok.com/@{}", "tiktok.com", 1, "[TK]"),
        new("Reddit", "https://www.reddit.com/user/{}", "reddit.com", 1, "[RD]"),
        new("GitLab", "https://gitlab.com/{}", "gitlab.com", 1, "[GL]"),

        // Priority 2: Professional networks
        new("Dev.to", "https://dev.to/{}", "dev.to", 2, "[DV]"),
        new("Medium", "https://medium.com/@{}", "medium.com", 2, "[MD]"),
        new("AngelList", "https://angel.co/u/{}", "angel.co", 2, "[AL]"),
        new("Crunchbase", "https://www.crunchbase.com/person/{}", "crunchbase.com", 2, "[CB]"),
        new("Keybase", "https://keybase.io/{}", "keybase.io", 2, "[KB]"),
        
        // Priority 3: Social with some identity value
        new("Facebook", "https://www.facebook.com/{}", "facebook.com", 3, "[FB]"),
        new("Quora", "https://www.quora.com/profile/{}", "quora.com", 3, "[QR]"),
        
        // Priority 4: Tech platforms (useful but username reuse common)
        new("BitBucket", "https://bitbucket.org/{}/", "bitbucket.org", 4, "[BB]"),
        new("StackOverflow", "https://stackoverflow.com/users/{}", null, 4, "[SO]"),
        new("Hashnode", "https://hashnode.com/@{}", "hashnode.com", 4, "[HN]"),
        new("CodePen", "https://codepen.io/{}", "codepen.io", 4, "[CP]"),
        new("Replit", "https://replit.com/@{}", "replit.com", 4, "[RP]"),
        new("PyPI", "https://pypi.org/user/{}/", "pypi.org", 4, "[PY]"),
        new("NPM", "https://www.npmjs.com/~{}", "npmjs.com", 4, "[NP]"),
        new("HackerRank", "https://www.hackerrank.com/{}", "hackerrank.com", 4, "[HR]"),
        new("LeetCode", "https://leetcode.com/{}/", "leetcode.com", 4, "[LC]"),
        new("Kaggle", "https://www.kaggle.com/{}", "kaggle.com", 4, "[KG]"),
        
        // Priority 5: Content platforms (high username collision)
        new("YouTube", "https://www.youtube.com/@{}", "youtube.com", 5, "[YT]"),
        new("SoundCloud", "https://soundcloud.com/{}", "soundcloud.com", 5, "[SC]"),
        new("Flickr", "https://www.flickr.com/people/{}", "flickr.com", 5, "[FL]"),
        new("Dribbble", "https://dribbble.com/{}", "dribbble.com", 5, "[DR]"),
        new("Behance", "https://www.behance.net/{}", "behance.net", 5, "[BE]"),
        new("DeviantArt", "https://www.deviantart.com/{}", "deviantart.com", 5, "[DA]"),
        
        // Priority 6: Gaming/Casual (LOWEST - very high false positive risk)
        new("Steam", "https://steamcommunity.com/id/{}", "steamcommunity.com", 6, "[ST]"),
        new("Twitch", "https://www.twitch.tv/{}", "twitch.tv", 6, "[TC]"),
        new("Xbox", "https://account.xbox.com/en-us/profile?gamertag={}", null, 6, "[XB]"),
        new("Discord", "https://discord.com/users/{}", null, 6, "[DC]"),
        new("Roblox", "https://www.roblox.com/user.aspx?username={}", null, 6, "[RB]"),
        new("Chess.com", "https://www.chess.com/member/{}", "chess.com", 6, "[CH]"),
        new("Spotify", "https://open.spotify.com/user/{}", null, 6, "[SP]"),
        new("Pinterest", "https://www.pinterest.com/{}/", "pinterest.com", 6, "[PT]"),
        new("Tumblr", "https://{}.tumblr.com", "tumblr.com", 6, "[TM]"),
        new("VK", "https://vk.com/{}", "vk.com", 6, "[VK]"),
        new("Telegram", "https://t.me/{}", "t.me", 6, "[TG]"),
        new("Gravatar", "https://en.gravatar.com/{}", "gravatar.com", 6, "[GR]"),
        new("About.me", "https://about.me/{}", "about.me", 6, "[AM]"),
        new("Bandcamp", "https://{}.bandcamp.com", "bandcamp.com", 6, "[BC]"),
        new("500px", "https://500px.com/{}", "500px.com", 6, "[5P]"),
        new("Patreon", "https://www.patreon.com/{}", "patreon.com", 6, "[PA]"),
        new("BuyMeACoffee", "https://www.buymeacoffee.com/{}", "buymeacoffee.com", 6, "[BM]"),
        new("Ko-fi", "https://ko-fi.com/{}", "ko-fi.com", 6, "[KF]"),
        new("Last.fm", "https://www.last.fm/user/{}", "last.fm", 6, "[LF]"),
        new("MyAnimeList", "https://myanimelist.net/profile/{}", "myanimelist.net", 6, "[MA]"),
    };

    public UsernameSearch(IHttpClientFactory httpClientFactory, ILogger<UsernameSearch> logger, ProfileVerifier profileVerifier)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _profileVerifier = profileVerifier;
    }

    /// <summary>
    /// Search for username across all platforms
    /// </summary>
    public async IAsyncEnumerable<OsintNode> SearchAsync(
        string username, 
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username))
            yield break;

        _logger.LogInformation("Starting username enumeration for: {Username} across {PlatformCount} platforms", 
            username, Platforms.Count);

        // Check all platforms concurrently with rate limiting
        var results = new ConcurrentBag<OsintNode>();
        var tasks = Platforms.Select(async platform =>
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                var result = await CheckPlatformAsync(username, platform, ct);
                if (result != null)
                {
                    results.Add(result);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);

        // Yield results sorted by priority
        foreach (var node in results.OrderBy(n => GetPlatformPriority(n.Label ?? "")))
        {
            yield return node;
        }

        _logger.LogInformation("Username enumeration complete. Found {Count} profiles for {Username}", 
            results.Count, username);
    }

    /// <summary>
    /// Search with strict verification - returns verified profiles and NOT_FOUND list
    /// </summary>
    public async Task<UsernameSearchResult> SearchWithVerificationAsync(
        string username, 
        CancellationToken ct = default)
    {
        var result = new UsernameSearchResult
        {
            Username = username
        };

        if (string.IsNullOrWhiteSpace(username))
            return result;

        _logger.LogInformation("Starting verified username search for: {Username}", username);

        var foundProfiles = new ConcurrentBag<VerifiedProfileResult>();
        var notFoundPlatforms = new ConcurrentBag<string>();
        var limitations = new ConcurrentBag<string>();

        var tasks = Platforms.Select(async platform =>
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                var verification = await CheckPlatformWithVerificationAsync(username, platform, ct);
                
                if (verification.IsVerified)
                {
                    foundProfiles.Add(verification);
                }
                else
                {
                    notFoundPlatforms.Add(platform.Name);
                }
            }
            catch (Exception ex)
            {
                limitations.Add($"{platform.Name}: {ex.Message}");
            }
            finally
            {
                _semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);

        result.VerifiedProfiles = foundProfiles.OrderBy(p => p.Platform).ToList();
        result.NotFoundPlatforms = notFoundPlatforms.ToList();
        result.Limitations = limitations.ToList();
        result.TotalQueried = Platforms.Count;

        _logger.LogInformation("Verified search complete: {Found} verified, {NotFound} not found, {Limitations} limitations",
            result.VerifiedProfiles.Count, result.NotFoundPlatforms.Count, result.Limitations.Count);

        return result;
    }

    private async Task<VerifiedProfileResult> CheckPlatformWithVerificationAsync(
        string username, 
        PlatformCheck platform, 
        CancellationToken ct)
    {
        var result = new VerifiedProfileResult
        {
            Platform = platform.Name,
            Url = platform.UrlTemplate.Replace("{}", username)
        };

        try
        {
            var client = _httpClientFactory.CreateClient("OsintClient");
            client.Timeout = TimeSpan.FromSeconds(5);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

            var response = await client.GetAsync(result.Url, ct);

            if (!response.IsSuccessStatusCode)
            {
                result.Reason = $"HTTP {(int)response.StatusCode}";
                return result;
            }

            var content = await response.Content.ReadAsStringAsync(ct);
            
            if (string.IsNullOrEmpty(content) || content.Length < 100)
            {
                result.Reason = "Empty response";
                return result;
            }

            // Strict validation
            if (!ValidateProfileExists(platform.Name, content, username))
            {
                result.Reason = "Profile not found in page content";
                return result;
            }

            // Extract data
            var (location, bio, photoUrl) = ExtractProfileData(platform.Name, content);

            result.IsVerified = true;
            result.Location = location;
            result.Bio = bio;
            result.PhotoUrl = photoUrl;
            result.Evidence = new List<string> { $"Username '{username}' verified on {platform.Name}" };
            result.Confidence = CalculateConfidence(platform, location, bio);

            return result;
        }
        catch (TaskCanceledException)
        {
            result.Reason = "Timeout";
            return result;
        }
        catch (Exception ex)
        {
            result.Reason = ex.Message;
            return result;
        }
    }

    private decimal CalculateConfidence(PlatformCheck platform, string? location, string? bio)
    {
        // High-priority platforms = higher base confidence
        // Low-priority platforms = much lower confidence (could be anyone with same username)
        var confidence = platform.Priority switch
        {
            1 => 0.55m,  // GitHub, LinkedIn, Twitter, GitLab - HIGH VALUE
            2 => 0.45m,  // Professional networks
            3 => 0.35m,  // Social with identity value
            4 => 0.25m,  // Tech platforms
            5 => 0.15m,  // Content platforms (high collision)
            6 => 0.08m,  // Gaming/Casual - LOWEST (very likely different person)
            _ => 0.10m
        };

        // Profile data adds confidence
        if (!string.IsNullOrEmpty(location)) confidence += 0.08m;
        if (!string.IsNullOrEmpty(bio)) confidence += 0.08m;

        // Cap based on priority - gaming CANNOT get high confidence from username alone
        var maxConfidence = platform.Priority switch
        {
            1 => 0.70m,
            2 => 0.55m,
            3 => 0.45m,
            4 => 0.35m,
            5 => 0.25m,
            6 => 0.15m,  // Gaming platforms capped at 15% confidence
            _ => 0.20m
        };

        return Math.Min(confidence, maxConfidence);
    }

    private async Task<OsintNode?> CheckPlatformAsync(string username, PlatformCheck platform, CancellationToken ct)
    {
        try
        {
            var url = platform.UrlTemplate.Replace("{}", username);
            var client = _httpClientFactory.CreateClient("OsintClient");
            client.Timeout = TimeSpan.FromSeconds(5);

            // Add user agent to avoid blocks
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

            var response = await client.GetAsync(url, ct);

            bool exists = false;
            string? location = null;
            string? bio = null;
            string? photoUrl = null;
            string? displayName = null;
            decimal verificationConfidence = 0m;
            List<string>? verificationEvidence = null;

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);

                // Platform-specific validation
                exists = ValidateProfileExists(platform.Name, content, username);

                if (exists)
                {
                    (location, bio, photoUrl) = ExtractProfileData(platform.Name, content);

                    // Second-pass verification on the same HTML — extracts display name,
                    // confirms profile is real (not a squatter placeholder), and adds
                    // a confidence score we can hand to CandidateAggregator.
                    var verification = _profileVerifier.VerifyContent(platform.Name, content, username);
                    displayName = verification.DisplayName;
                    if (string.IsNullOrEmpty(bio) && !string.IsNullOrEmpty(verification.Bio)) bio = verification.Bio;
                    if (string.IsNullOrEmpty(location) && !string.IsNullOrEmpty(verification.Location)) location = verification.Location;
                    verificationConfidence = verification.Confidence;
                    verificationEvidence = verification.Evidence;
                }
            }
            // Some platforms return 200 even for non-existent users, so we check content
            // Others return 404 for non-existent users
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                exists = false;
            }
            // For some platforms, we can't determine existence without more checks
            else
            {
                exists = false;
            }

            if (exists)
            {
                var node = new OsintNode
                {
                    Id = Guid.NewGuid().ToString(),
                    Label = $"{platform.Icon} {platform.Name}",
                    Value = url,
                    Depth = 1,
                    Children = new List<OsintNode>()
                };

                // Add extracted data as children
                if (!string.IsNullOrEmpty(location))
                {
                    node.Children.Add(new OsintNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        Label = "Location",
                        Value = location,
                        Depth = 2
                    });
                }

                if (!string.IsNullOrEmpty(bio))
                {
                    node.Children.Add(new OsintNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        Label = "📝 Bio",
                        Value = bio.Length > 100 ? bio.Substring(0, 100) + "..." : bio,
                        Depth = 2
                    });
                }

                if (!string.IsNullOrEmpty(photoUrl))
                {
                    node.Children.Add(new OsintNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        Label = "📷 Photo",
                        Value = photoUrl,
                        Depth = 2
                    });
                }

                // ProfileVerifier display name — labelled "Name" so CandidateAggregator's
                // ExtractNodeData maps it to evidence.DisplayName, which then drives
                // cross-platform name-match scoring in IdentityLinker.
                if (!string.IsNullOrEmpty(displayName))
                {
                    node.Children.Add(new OsintNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        Label = "Name",
                        Value = displayName,
                        Depth = 2
                    });
                }

                if (verificationConfidence > 0m)
                {
                    var summary = verificationEvidence != null && verificationEvidence.Count > 0
                        ? string.Join("; ", verificationEvidence)
                        : "verified";
                    node.Children.Add(new OsintNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        Label = "Verification",
                        Value = $"{verificationConfidence:0.00} — {summary}",
                        Depth = 2
                    });
                }

                // IDENTITY FLOW: Find handles mentioned in Bio
                if (!string.IsNullOrEmpty(bio))
                {
                    var handleMatches = Regex.Matches(bio, @"@([A-Z0-9_]{3,15})", RegexOptions.IgnoreCase);
                    foreach (Match m in handleMatches)
                    {
                        var discoveredHandle = m.Groups[1].Value;
                        node.Children.Add(new OsintNode
                        {
                            Id = Guid.NewGuid().ToString(),
                            Label = "Handle Discovery",
                            Value = discoveredHandle,
                            Depth = 2
                        });
                    }
                }

                _logger.LogDebug("Found profile on {Platform}: {Url}", platform.Name, url);
                return node;
            }
        }
        catch (TaskCanceledException)
        {
            // Timeout - skip platform
        }
        catch (HttpRequestException ex)
        {
            _logger.LogDebug("Failed to check {Platform}: {Error}", platform.Name, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Error checking {Platform}: {Error}", platform.Name, ex.Message);
        }

        return null;
    }

    private bool ValidateProfileExists(string platform, string content, string username)
    {
        // STRICT validation - require POSITIVE proof, not just absence of "not found"
        var lowerContent = content.ToLower();
        var lowerUsername = username.ToLower();

        // Universal NOT FOUND patterns - reject immediately
        // NOTE: Patterns must be specific - avoid single words that appear in valid page JS
        var notFoundPatterns = new[]
        {
            "page not found",
            "this page isn't available",
            "sorry, this page isn't available",
            "user not found",
            "profile not found",
            "account suspended",
            "this account doesn't exist",
            "this channel doesn't exist",
            "we couldn't find that page",
            "this user doesn't exist",
            "the profile you're looking for doesn't exist",
            "hmm...this page doesn't exist",
            "<title>404 not found</title>",
            "<h1>404</h1>",
            "error 404:",
            "niczego tu nie ma"  // Polish
        };

        // Check for any not-found pattern
        foreach (var pattern in notFoundPatterns)
        {
            if (lowerContent.Contains(pattern))
            {
                _logger.LogDebug("{Platform}: NOT FOUND pattern detected: '{Pattern}'", platform, pattern);
                return false;
            }
        }

        // Platform-specific POSITIVE validation - must have proof of profile
        return platform switch
        {
            "GitHub" => ValidateGitHub(content, username),
            "YouTube" => ValidateYouTube(content, username),
            "Instagram" => ValidateInstagram(content, username),
            "Twitter" => ValidateTwitter(content, username),
            "Reddit" => ValidateReddit(content, username),
            "Steam" => ValidateSteam(content, username),
            "Twitch" => ValidateTwitch(content, username),
            "TikTok" => ValidateTikTok(content, username),
            "LinkedIn" => ValidateLinkedIn(content, username),
            _ => ValidateGeneric(content, username)
        };
    }

    private bool ValidateGitHub(string content, string username)
    {
        // GitHub: Must have login JSON or profile-specific elements
        if (content.Contains($"\"login\":\"{username}\"", StringComparison.OrdinalIgnoreCase))
            return true;
        
        // Check for vcard (profile card) with the username
        if (content.Contains("vcard-username") && content.Contains(username, StringComparison.OrdinalIgnoreCase))
            return true;
            
        return false;
    }

    private bool ValidateYouTube(string content, string username)
    {
        // YouTube: Must have SPECIFIC channel indicators
        // Just checking for username is NOT enough - YouTube shows search results
        
        // Check for channelId in the response (proves it's a real channel page)
        if (!content.Contains("\"channelId\"", StringComparison.OrdinalIgnoreCase))
            return false;
        
        // Must also have the handle/username visible
        if (content.Contains($"\"@{username}\"", StringComparison.OrdinalIgnoreCase) ||
            content.Contains($">@{username}<", StringComparison.OrdinalIgnoreCase))
            return true;
            
        return false;
    }

    private bool ValidateInstagram(string content, string username)
    {
        // Instagram: Must have profile-specific indicators
        // Instagram often returns 200 with "Sorry, this page isn't available" or "Login • Instagram"
        
        // REJECT IF:
        if (content.Contains("Sorry, this page isn't available", StringComparison.OrdinalIgnoreCase)) return false;
        if (content.Contains("<title>Login • Instagram</title>", StringComparison.OrdinalIgnoreCase)) return false;
        if (content.Contains("this page isn't available", StringComparison.OrdinalIgnoreCase)) return false;
        if (content.Contains("\"user_not_found\"", StringComparison.OrdinalIgnoreCase)) return false;

        // Check for user JSON structure - this is the STRONGEST indicator
        if (content.Contains($"\"username\":\"{username}\"", StringComparison.OrdinalIgnoreCase))
            return true;
            
        // Check for profile page marker with username in meta tags
        if (content.Contains($"instagram.com/{username}/") || 
            content.Contains($"content=\"@{username}"))
        {
            // Must have some profile-like content beyond just the title
            if (content.Contains("Followers", StringComparison.OrdinalIgnoreCase) || 
                content.Contains("Following", StringComparison.OrdinalIgnoreCase) ||
                content.Contains("Posts", StringComparison.OrdinalIgnoreCase))
                return true;
        }
            
        return false;
    }

    private bool ValidateTwitter(string content, string username)
    {
        // Twitter/X: Must have screen_name in JSON
        if (content.Contains($"\"screen_name\":\"{username}\"", StringComparison.OrdinalIgnoreCase))
            return true;
            
        // Or the @username in profile context
        if (content.Contains("ProfileTimeline") && content.Contains($"@{username}", StringComparison.OrdinalIgnoreCase))
            return true;
            
        return false;
    }

    private bool ValidateReddit(string content, string username)
    {
        // Reddit: Must have user profile indicators
        if (content.Contains($"\"name\":\"{username}\"", StringComparison.OrdinalIgnoreCase) ||
            content.Contains($"reddit.com/user/{username}", StringComparison.OrdinalIgnoreCase))
        {
            // Must also have user-specific elements
            if (content.Contains("created_utc") || content.Contains("karma"))
                return true;
        }
        return false;
    }

    private bool ValidateSteam(string content, string username)
    {
        // Steam: Must have profile elements
        if (content.Contains("profile_header") || content.Contains("playerAvatarAutoSizeInner"))
        {
            if (content.Contains(username, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private bool ValidateTwitch(string content, string username)
    {
        // Twitch: Must have channel-specific elements
        if (content.Contains("channel-header") || content.Contains("tw-avatar"))
        {
            if (content.Contains($"\"{username}\"", StringComparison.OrdinalIgnoreCase) ||
                content.Contains($"twitch.tv/{username}", StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private bool ValidateTikTok(string content, string username)
    {
        // TikTok: Must have user-specific JSON
        if (content.Contains($"\"uniqueId\":\"{username}\"", StringComparison.OrdinalIgnoreCase) ||
            content.Contains($"\"nickname\"", StringComparison.OrdinalIgnoreCase) && 
            content.Contains($"@{username}", StringComparison.OrdinalIgnoreCase))
            return true;
        return false;
    }

    private bool ValidateLinkedIn(string content, string username)
    {
        // LinkedIn: Very restrictive - only if we have actual profile data
        if (content.Contains("pv-top-card") || content.Contains("profile-section"))
        {
            if (content.Contains($"linkedin.com/in/{username}", StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private bool ValidateGeneric(string content, string username)
    {
        // Generic: Must have username appear in profile-like context
        // Just appearing once is NOT enough - might be in search results
        
        var usernameCount = System.Text.RegularExpressions.Regex.Matches(
            content, 
            System.Text.RegularExpressions.Regex.Escape(username), 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase).Count;
        
        // Need at least 3 occurrences to suggest it's a real profile page
        if (usernameCount < 3)
            return false;
            
        // And should have profile-like elements
        var profileIndicators = new[] { "bio", "about", "profile", "avatar", "joined", "member since" };
        var hasProfileIndicator = profileIndicators.Any(p => content.Contains(p, StringComparison.OrdinalIgnoreCase));
        
        return hasProfileIndicator;
    }

    private (string? Location, string? Bio, string? PhotoUrl) ExtractProfileData(string platform, string content)
    {
        string? location = null;
        string? bio = null;
        string? photoUrl = null;

        try
        {
            var lowerContent = content.ToLower();

            // GitHub
            if (platform == "GitHub")
            {
                var locationMatch = Regex.Match(content, @"<span[^>]*itemprop=""homeLocation""[^>]*>([^<]+)</span>");
                if (locationMatch.Success) location = locationMatch.Groups[1].Value.Trim();

                var bioMatch = Regex.Match(content, @"<div[^>]*class=""p-note user-profile-bio""[^>]*>([^<]+)</div>");
                if (bioMatch.Success) bio = bioMatch.Groups[1].Value.Trim();

                var avatarMatch = Regex.Match(content, @"<img[^>]*class=""avatar[^""]*""[^>]*src=""([^""]+)""");
                if (avatarMatch.Success) photoUrl = avatarMatch.Groups[1].Value;
            }
            // Instagram
            else if (platform == "Instagram")
            {
                // Title extractor: "Piotr Szewczyk (@peszew) • Instagram photos and videos"
                var titleMatch = Regex.Match(content, @"<title>(.*?) \(@(.*?)\)");
                if (titleMatch.Success) bio = $"Name: {titleMatch.Groups[1].Value}"; // Use bio as temporary storage for display name if needed

                // Meta description often has bio content
                var descMatch = Regex.Match(content, @"<meta[^>]*name=""description""[^>]*content=""([^""]+)""");
                if (descMatch.Success) bio = descMatch.Groups[1].Value;

                // Og:image for photo
                var ogImage = Regex.Match(content, @"<meta[^>]*property=""og:image""[^>]*content=""([^""]+)""");
                if (ogImage.Success) photoUrl = ogImage.Groups[1].Value;
            }
            // X/Twitter
            else if (platform == "X/Twitter" || platform == "Twitter")
            {
                // X uses massive JSON for state, but og tags are usually there for scrapers
                var bioMatch = Regex.Match(content, @"<meta[^>]*name=""description""[^>]*content=""([^""]+)""");
                if (bioMatch.Success) bio = bioMatch.Groups[1].Value;

                var ogImage = Regex.Match(content, @"<meta[^>]*property=""og:image""[^>]*content=""([^""]+)""");
                if (ogImage.Success) photoUrl = ogImage.Groups[1].Value;
                
                // Try to find location in JSON-LD or state
                var locMatch = Regex.Match(content, @"""location"":\s*""([^""]+)""");
                if (locMatch.Success) location = locMatch.Groups[1].Value;
            }
            // LinkedIn
            else if (platform == "LinkedIn")
            {
                var titleMatch = Regex.Match(content, @"<title>(.*?) \| LinkedIn</title>");
                if (titleMatch.Success) bio = $"Name: {titleMatch.Groups[1].Value}";

                var descMatch = Regex.Match(content, @"<meta[^>]*name=""description""[^>]*content=""([^""]+)""");
                if (descMatch.Success) bio = descMatch.Groups[1].Value;
            }
            
            // DISCOVERY: Look for @handles in Bio (Identity Flow)
            if (!string.IsNullOrEmpty(bio))
            {
                var handleMatches = Regex.Matches(bio, @"@([A-Z0-9_]{3,15})", RegexOptions.IgnoreCase);
                foreach (Match m in handleMatches)
                {
                    var discoveredHandle = m.Groups[1].Value;
                    // We don't return handles directly from here, but we can tag the bio or add it as a specialized child
                    // The orchestration layer will pick these up
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Metadata extraction failed for {Platform}: {Error}", platform, ex.Message);
        }

        return (location, bio, photoUrl);
    }

    private int GetPlatformPriority(string label)
    {
        var platform = Platforms.FirstOrDefault(p => label.Contains(p.Name));
        return platform?.Priority ?? 10;
    }

    private record PlatformCheck(
        string Name, 
        string UrlTemplate, 
        string? Domain, 
        int Priority, 
        string Icon);
}

/// <summary>
/// Result of verified username search
/// </summary>
public class UsernameSearchResult
{
    public string Username { get; set; } = "";
    public List<VerifiedProfileResult> VerifiedProfiles { get; set; } = new();
    public List<string> NotFoundPlatforms { get; set; } = new();
    public List<string> Limitations { get; set; } = new();
    public int TotalQueried { get; set; }
}

/// <summary>
/// Verified profile result
/// </summary>
public class VerifiedProfileResult
{
    public bool IsVerified { get; set; }
    public string Platform { get; set; } = "";
    public string Url { get; set; } = "";
    public decimal Confidence { get; set; }
    public List<string> Evidence { get; set; } = new();
    public string? Location { get; set; }
    public string? Bio { get; set; }
    public string? PhotoUrl { get; set; }
    public string? DisplayName { get; set; }
    public string Reason { get; set; } = "";
}
