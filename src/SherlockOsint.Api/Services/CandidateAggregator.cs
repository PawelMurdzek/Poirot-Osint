using SherlockOsint.Shared.Models;
using SherlockOsint.Api.Services.OsintProviders;

namespace SherlockOsint.Api.Services;

/// <summary>
/// Aggregates OSINT results into target candidates with probability scoring
/// Uses INPUT DATA (email, phone, nickname) as primary matching criteria
/// </summary>
public class CandidateAggregator
{
    private readonly ILogger<CandidateAggregator> _logger;
    private readonly CountryDetector _countryDetector;
    private readonly IdentityLinker _identityLinker;

    // Platform priorities (1 = highest importance)
    private static readonly Dictionary<string, (int Priority, string Icon)> PlatformInfo = new()
    {
        { "github", (1, "[GH]") },
        { "linkedin", (1, "[LI]") },
        { "twitter", (1, "[TW]") },
        { "x", (1, "[TW]") },
        { "instagram", (1, "[IG]") },
        { "tiktok", (1, "[TK]") },
        { "reddit", (1, "[RD]") },
        { "gitlab", (1, "[GL]") },
        { "youtube", (5, "[YT]") },
        { "twitch", (5, "[TC]") },
        { "stackoverflow", (6, "[SO]") },
        { "steam", (6, "[ST]") },
        { "pypi", (4, "[PY]") },
        { "replit", (4, "[RP]") },
        { "soundcloud", (4, "[SC]") },
        { "gravatar", (1, "[GR]") }, // High priority because it's EMAIL-VERIFIED
    };

    public CandidateAggregator(ILogger<CandidateAggregator> logger, CountryDetector countryDetector, IdentityLinker identityLinker)
    {
        _logger = logger;
        _countryDetector = countryDetector;
        _identityLinker = identityLinker;
    }

    /// <summary>
    /// Build target candidates from search results
    /// NEW APPROACH: Group by unique username, merge only with strong evidence
    /// </summary>
    public List<TargetCandidate> BuildCandidates(SearchRequest request, List<OsintNode> results)
    {
        var candidates = new List<TargetCandidate>();
        
        // Extract input data
        var inputEmail = request.Email?.ToLower().Trim();
        var inputNickname = NormalizeUsername(request.Nickname ?? "");
        var inputFullName = NormalizeUsername(request.FullName ?? "");
        var inputPhone = request.Phone;
        
        string? phoneCountry = ExtractPhoneCountry(results);
        
        _logger.LogInformation("Building candidates with inputs: Email={Email}, Nickname={Nickname}, Phone={Phone}",
            inputEmail, inputNickname, inputPhone);

        // STEP 1: Group all platform nodes by normalized username
        var usernameGroups = new Dictionary<string, List<(OsintNode Node, SourceEvidence Evidence)>>(StringComparer.OrdinalIgnoreCase);
        var emailToUsernames = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        var seenUrlsInCandidate = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var node in results)
        {
            if (!IsPlatformNode(node)) continue;

            var evidence = BuildSourceEvidence(node, phoneCountry);
            if (evidence == null) continue;

            // STRICT URL NORMALIZATION FOR DEDUPLICATION
            var normalizedUrl = NormalizeUrl(evidence.Url);
            if (string.IsNullOrEmpty(normalizedUrl) || seenUrlsInCandidate.Contains(normalizedUrl)) continue;
            
            seenUrlsInCandidate.Add(normalizedUrl);
            evidence.Url = normalizedUrl; // Update with normalized version

            var normalizedUsername = NormalizeUsername(evidence.Username);
            if (string.IsNullOrEmpty(normalizedUsername)) continue;

            // Group by username
            if (!usernameGroups.ContainsKey(normalizedUsername))
                usernameGroups[normalizedUsername] = new List<(OsintNode, SourceEvidence)>();
            usernameGroups[normalizedUsername].Add((node, evidence));

            // Track emails for merge detection
            if (evidence.ExtractedData.TryGetValue("email", out var email) && !string.IsNullOrEmpty(email))
            {
                var lowerEmail = email.ToLower();
                if (!emailToUsernames.ContainsKey(lowerEmail))
                    emailToUsernames[lowerEmail] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                emailToUsernames[lowerEmail].Add(normalizedUsername);
            }
        }

        _logger.LogInformation("Found {count} unique usernames across platforms", usernameGroups.Count);

        // STEP 2: Detect merges based on shared emails
        var mergeGroups = DetectMergeGroups(usernameGroups.Keys.ToList(), emailToUsernames);

        // STEP 3: Build one candidate per merge group (or single username)
        var processedUsernames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        foreach (var mergeGroup in mergeGroups)
        {
            var primaryUsername = mergeGroup.Usernames.First();
            if (processedUsernames.Contains(primaryUsername)) continue;

            var candidate = BuildCandidateFromGroup(
                request, 
                mergeGroup.Usernames, 
                usernameGroups, 
                mergeGroup.MergeReason,
                phoneCountry,
                results);

            if (candidate != null && candidate.Sources.Count > 0)
            {
                candidates.Add(candidate);
                foreach (var u in mergeGroup.Usernames)
                    processedUsernames.Add(u);
            }
        }

        // Sort by probability score descending
        candidates = candidates.OrderByDescending(c => c.ProbabilityScore).ToList();

        _logger.LogInformation("Built {CandidateCount} candidates from {UsernameCount} usernames", 
            candidates.Count, usernameGroups.Count);
        return candidates;
    }

    /// <summary>
    /// Detect which usernames should be merged based on shared emails
    /// </summary>
    private List<MergeGroup> DetectMergeGroups(
        List<string> usernames, 
        Dictionary<string, HashSet<string>> emailToUsernames)
    {
        var groups = new List<MergeGroup>();
        var assigned = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Find usernames that share the same email
        foreach (var email in emailToUsernames.Keys)
        {
            var sharedUsernames = emailToUsernames[email].Where(u => usernames.Contains(u)).ToList();
            if (sharedUsernames.Count > 1)
            {
                // These usernames share an email - they should be merged
                var unassigned = sharedUsernames.Where(u => !assigned.Contains(u)).ToList();
                if (unassigned.Count > 0)
                {
                    groups.Add(new MergeGroup
                    {
                        Usernames = unassigned,
                        MergeReason = $"Same email: {email}"
                    });
                    foreach (var u in unassigned)
                        assigned.Add(u);
                }
            }
        }

        // Add remaining usernames as single-user groups (no merge)
        foreach (var username in usernames)
        {
            if (!assigned.Contains(username))
            {
                groups.Add(new MergeGroup
                {
                    Usernames = new List<string> { username },
                    MergeReason = null  // No merge
                });
            }
        }

        return groups;
    }

    private class MergeGroup
    {
        public List<string> Usernames { get; set; } = new();
        public string? MergeReason { get; set; }
    }

    /// <summary>
    /// Build a single candidate from one or more usernames
    /// </summary>
    private TargetCandidate? BuildCandidateFromGroup(
        SearchRequest request,
        List<string> usernames,
        Dictionary<string, List<(OsintNode Node, SourceEvidence Evidence)>> usernameGroups,
        string? mergeReason,
        string? phoneCountry,
        List<OsintNode> results)
    {
        var candidate = new TargetCandidate
        {
            PrimaryUsername = usernames.First(),
            MergeReason = mergeReason,
            KnownAliases = usernames.Count > 1 ? usernames.Skip(1).ToList() : new List<string>()
        };

        var allSources = new List<SourceEvidence>();
        var discoveredNames = new List<string>();
        var discoveredEmails = new List<VerifiedEmail>();
        var locations = new List<string>();
        var photos = new List<string>();
        var attributes = new HashSet<string>();

        var seenUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var username in usernames)
        {
            if (!usernameGroups.TryGetValue(username, out var group)) continue;

            foreach (var (node, evidence) in group)
            {
                // Deduplicate by URL (already normalized in BuildSourceEvidence phase)
                if (!string.IsNullOrEmpty(evidence.Url) && seenUrls.Contains(evidence.Url)) continue;
                if (!string.IsNullOrEmpty(evidence.Url)) seenUrls.Add(evidence.Url);

                // Score based on input matching
                ScoreEvidence(request, evidence, phoneCountry);
                allSources.Add(evidence);

                // Collect data
                if (evidence.ExtractedData.TryGetValue("name", out var name) && !string.IsNullOrEmpty(name))
                    discoveredNames.Add(name);
                if (evidence.ExtractedData.TryGetValue("location", out var loc) && !string.IsNullOrEmpty(loc))
                    locations.Add(loc);
                if (evidence.ExtractedData.TryGetValue("photo", out var photo) && !string.IsNullOrEmpty(photo))
                    photos.Add(photo);
                if (evidence.ExtractedData.TryGetValue("email", out var email) && !string.IsNullOrEmpty(email))
                {
                    discoveredEmails.Add(new VerifiedEmail
                    {
                        Email = email,
                        Source = evidence.Platform,
                        Confidence = evidence.IsHighConfidence ? 90 : 60,
                        IsVerified = evidence.Platform.Contains("Hunter") || evidence.Platform.Contains("HIBP")
                    });
                }

                if (evidence.ExtractedData.TryGetValue("Title", out var title) && !string.IsNullOrEmpty(title))
                    attributes.Add(title);
                if (evidence.ExtractedData.TryGetValue("Company", out var company) && !string.IsNullOrEmpty(company))
                    attributes.Add($"At {company}");

                InferAttributes(evidence, attributes);
            }
        }

        // STEP 2: Process NON-username nodes (Enrichment, Breaches) for this candidate
        // If an enrichment node matches the candidate's discovered name or email
        foreach (var node in results)
        {
            if (IsPlatformNode(node)) continue; // Already processed

            // Case 1: Hunter.io / Clearbit / FullContact nodes
            if (node.Label != null && (node.Label.Contains("HUNTER") || node.Label.Contains("CLEARBIT") || node.Label.Contains("FULLCONTACT")))
            {
                // If the node value matches candidate's primary email or a verified email
                if (node.Value?.Equals(request.Email, StringComparison.OrdinalIgnoreCase) == true ||
                    discoveredEmails.Any(e => e.Email.Equals(node.Value, StringComparison.OrdinalIgnoreCase)))
                {
                    ProcessEnrichmentNode(node, discoveredNames, discoveredEmails, locations, attributes, photos);
                }
            }
        }

        if (allSources.Count == 0)
            return null;

        // Set name from discovered data, fallback to input
        var bestName = GetMostCommon(discoveredNames);
        if (!string.IsNullOrEmpty(bestName))
            candidate.Name = bestName;
        else if (!string.IsNullOrEmpty(request.FullName))
            candidate.Name = request.FullName;
        else
            candidate.Name = usernames.First();

        // STEP 3: PLATFORM DEDUPLICATION (Best-Per-Platform)
        // Group all sources by platform and select the one with the highest score
        var bestSourcesPerPlatform = allSources
            .Where(s => s.ContributionScore > 0 || s.PlatformPriority <= 2) // Filter noise
            .GroupBy(s => s.Platform.ToLower().Replace("twitter", "x"))
            .Select(g => g.OrderByDescending(s => s.ContributionScore)
                          .ThenBy(s => s.PlatformPriority)
                          .First())
            .ToList();

        candidate.Sources = bestSourcesPerPlatform.OrderByDescending(s => s.ContributionScore).ThenBy(s => s.PlatformPriority).ToList();
        candidate.VerifiedEmails = discoveredEmails.DistinctBy(e => e.Email.ToLower()).ToList();
        candidate.PhotoUrl = photos.FirstOrDefault();
        candidate.ProbableLocation = GetMostCommon(locations) ?? phoneCountry ?? "";
        candidate.InferredAttributes = attributes.ToList();

        // Professional role
        candidate.ProfessionalRole = InferProfessionalRole(attributes);
        
        // Calculate score
        candidate.ProbabilityScore = CalculateScore(request, candidate, phoneCountry);
        
        // Confidence interval
        var (low, high) = CalculateConfidenceInterval(candidate.ProbabilityScore, allSources);
        candidate.ConfidenceLow = low;
        candidate.ConfidenceHigh = high;

        // Analysis
        candidate.ConsistencyAnalysis = GenerateAnalysis(candidate, mergeReason);
        candidate.UncertaintyNotes = GenerateUncertainty(candidate, request);

        // Country distribution
        candidate.CountryDistribution = _countryDetector.AnalyzeCountryProbability(request.Phone, new List<OsintNode>(), request.FullName);

        // Identity signals
        candidate.IdentitySignals = _identityLinker.AnalyzeLinks(allSources, request);

        return candidate;
    }

    private void ProcessEnrichmentNode(OsintNode node, List<string> names, List<VerifiedEmail> emails, List<string> locations, HashSet<string> attributes, List<string> photos)
    {
        foreach (var child in node.Children)
        {
            var label = child.Label?.ToLower() ?? "";
            var val = child.Value ?? "";
            if (string.IsNullOrEmpty(val)) continue;

            if (label.Contains("name")) names.Add(val);
            if (label.Contains("location")) locations.Add(val);
            if (label.Contains("company") || label.Contains("title") || label.Contains("position")) attributes.Add(val);
            if (label.Contains("photo") || label.Contains("avatar")) photos.Add(val);
            if (label.Contains("quality") || label.Contains("status")) attributes.Add($"Email: {val}");
        }
    }

    /// <summary>
    /// Build basic source evidence from a node
    /// </summary>
    private SourceEvidence? BuildSourceEvidence(OsintNode node, string? phoneCountry)
    {
        var platform = ExtractPlatformName(node);
        if (string.IsNullOrEmpty(platform)) return null;

        var username = ExtractUsername(node);
        var (priority, icon) = GetPlatformInfo(platform);

        // Only use node.Value as URL if it's actually a URL
        var url = ExtractValidUrl(node);

        var evidence = new SourceEvidence
        {
            Platform = platform,
            Icon = icon,
            Url = url,
            Username = username ?? "",
            PlatformPriority = priority
        };

        ExtractNodeData(node, evidence.ExtractedData);
        
        // Set display name and bio from extracted data
        evidence.DisplayName = evidence.ExtractedData.GetValueOrDefault("name");
        evidence.Bio = evidence.ExtractedData.GetValueOrDefault("bio");

        return evidence;
    }

    /// <summary>
    /// Extract a valid HTTP URL from a node, checking node.Value and children
    /// </summary>
    private string ExtractValidUrl(OsintNode node)
    {
        // Check if node.Value is a valid URL
        if (!string.IsNullOrEmpty(node.Value) && node.Value.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return node.Value;
        }

        // Check children for URLs (some nodes have URL in children)
        foreach (var child in node.Children)
        {
            if (!string.IsNullOrEmpty(child.Value) && child.Value.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                return child.Value;
            }
        }

        return "";
    }

    /// <summary>
    /// Score evidence based on input matching
    /// </summary>
    private void ScoreEvidence(SearchRequest request, SourceEvidence evidence, string? phoneCountry)
    {
        var score = 0;
        var reasons = new List<string>();
        var isHighConf = false;

        var inputNickname = NormalizeUsername(request.Nickname ?? "");
        var inputFullName = NormalizeUsername(request.FullName ?? "");
        var inputEmail = request.Email?.ToLower().Trim();
        var normalizedUsername = NormalizeUsername(evidence.Username);

        // High-priority platform bonus
        if (evidence.PlatformPriority == 1)
        {
            score += 15;
            reasons.Add("High-priority platform");
        }

        // PRIORITY MULTIPLIER: Core platforms (X, IG, LI, GH) get a significant bonus
        float multiplier = evidence.Platform.ToLower() switch
        {
            "twitter" or "x" or "instagram" or "linkedin" or "github" => 2.5f,
            _ => evidence.PlatformPriority switch
            {
                1 => 1.5f,
                2 => 1.0f,
                4 => 0.6f,
                6 => 0.2f,
                _ => 0.5f
            }
        };

        // Nickname exact match
        if (!string.IsNullOrEmpty(inputNickname) && normalizedUsername.Equals(inputNickname, StringComparison.OrdinalIgnoreCase))
        {
            // Base score for match
            var matchScore = 30;
            
            // If we have platform-level confidence, use it to scale the score
            if (evidence.ExtractedData.TryGetValue("Confidence", out var confStr) && decimal.TryParse(confStr, out var conf))
            {
                // conf is 0-100 (from MapToNode)
                if (conf < 20) matchScore = 5; // Very low confidence = very low match score
                else if (conf < 40) matchScore = 15;
            }

            score += (int)(matchScore * multiplier);
            reasons.Add($"Nickname match ({evidence.Platform} x {multiplier:0.0})");
            
            // Only consider high-confidence if it's a high-priority platform or has good provider-level confidence
            isHighConf = evidence.PlatformPriority <= 2;
        }

        // Name match - scaled by multiplier
        if (!string.IsNullOrEmpty(inputFullName) && normalizedUsername.Contains(inputFullName))
        {
            score += (int)(20 * multiplier);
            reasons.Add("Name match");
        }

        evidence.ContributionScore = Math.Min(score, 60);  // Cap at 60
        evidence.Explanation = string.Join(", ", reasons);
        evidence.IsHighConfidence = isHighConf;
    }

    /// <summary>
    /// Calculate overall score for a candidate
    /// CRITICAL: Requires REAL EVIDENCE, not just platform existence
    /// </summary>
    private int CalculateScore(SearchRequest request, TargetCandidate candidate, string? phoneCountry)
    {
        // Check what verification we have
        bool hasEmailInput = !string.IsNullOrEmpty(request.Email);
        bool hasPhoneInput = !string.IsNullOrEmpty(request.Phone);
        bool hasNicknameInput = !string.IsNullOrEmpty(request.Nickname);
        bool hasNameInput = !string.IsNullOrEmpty(request.FullName);
        
        // Check if we found any HIGH-CONFIDENCE matches (exact nickname match, email verification)
        bool hasHighConfidenceMatch = candidate.Sources.Any(s => s.IsHighConfidence);
        bool hasVerifiedEmail = candidate.VerifiedEmails.Any(e => e.IsVerified);
        bool hasDiscoveredEmail = candidate.VerifiedEmails.Count > 0;
        
        // NO VERIFICATION = MAX 35%
        // Just finding platforms for a random string means nothing
        if (!hasEmailInput && !hasPhoneInput && !hasHighConfidenceMatch)
        {
            // Without any input verification, cap very low
            var lowScore = Math.Min(candidate.Sources.Count * 5, 25);
            
            // Only boost if we found actual data (not just "platform exists")
            if (hasDiscoveredEmail)
                lowScore += 10;
            
            return Math.Min(lowScore, 35);
        }

        // WITH INPUT BUT NO MATCHES = MAX 45%
        if (!hasHighConfidenceMatch && !hasVerifiedEmail)
        {
            var mediumScore = Math.Min(candidate.Sources.Count * 8, 35);
            
            if (!string.IsNullOrEmpty(phoneCountry) && candidate.ProbableLocation.Contains(phoneCountry))
                mediumScore += 10;
            
            return Math.Min(mediumScore, 45);
        }

        // WITH HIGH-CONFIDENCE MATCHES = Can go higher
        var baseScore = 0;

        // High-confidence matches are worth more
        var highConfCount = candidate.Sources.Count(s => s.IsHighConfidence);
        baseScore += highConfCount * 20;

        // Other sources contribute less
        var otherCount = candidate.Sources.Count - highConfCount;
        baseScore += Math.Min(otherCount * 8, 20);

        // Verified email is strong evidence
        if (hasVerifiedEmail)
            baseScore += 15;

        // Phone country match
        if (!string.IsNullOrEmpty(phoneCountry) && candidate.ProbableLocation.Contains(phoneCountry))
            baseScore += 10;

        // Cap based on evidence quality
        var maxScore = 50;  // Default max
        if (hasEmailInput || hasPhoneInput)
            maxScore = 70;
        if (hasHighConfidenceMatch)
            maxScore = 85;
        if (hasVerifiedEmail && hasHighConfidenceMatch)
            maxScore = 95;

        return Math.Min(baseScore, maxScore);
    }

    /// <summary>
    /// Infer professional role from attributes
    /// </summary>
    private string? InferProfessionalRole(HashSet<string> attributes)
    {
        var roles = new List<string>();
        if (attributes.Contains("Developer/Programmer")) roles.Add("Developer");
        if (attributes.Contains("Content Creator")) roles.Add("Content Creator");
        if (attributes.Contains("Professional")) roles.Add("Professional");
        if (attributes.Contains("Gamer")) roles.Add("Gamer");
        if (attributes.Contains("Musician/Audio Creator")) roles.Add("Musician");

        return roles.Count > 0 ? string.Join(", ", roles) : null;
    }

    /// <summary>
    /// Generate analysis text
    /// </summary>
    private string GenerateAnalysis(TargetCandidate candidate, string? mergeReason)
    {
        var parts = new List<string>();
        
        parts.Add($"Found {candidate.Sources.Count} platform(s) with username '{candidate.PrimaryUsername}'");
        
        if (candidate.KnownAliases.Count > 0)
            parts.Add($"Also uses: {string.Join(", ", candidate.KnownAliases)}");
        
        if (!string.IsNullOrEmpty(mergeReason))
            parts.Add($"Profiles linked: {mergeReason}");
        
        if (candidate.VerifiedEmails.Count > 0)
            parts.Add($"Email(s) found: {string.Join(", ", candidate.VerifiedEmails.Select(e => e.Email))}");

        return string.Join(". ", parts);
    }

    /// <summary>
    /// Generate uncertainty notes
    /// </summary>
    private string GenerateUncertainty(TargetCandidate candidate, SearchRequest request)
    {
        var notes = new List<string>();

        if (candidate.Sources.Count == 1)
            notes.Add("Single source only - limited verification");

        if (string.IsNullOrEmpty(request.Email))
            notes.Add("No email provided - identity not email-verified");

        if (candidate.VerifiedEmails.Count == 0)
            notes.Add("No email discovered from platforms");

        if (candidate.Sources.All(s => !s.IsHighConfidence))
            notes.Add("No high-confidence matches");

        return notes.Count > 0 ? string.Join(". ", notes) : "Good confidence in this identity";
    }

    /// <summary>
    /// Calculate confidence interval
    /// </summary>
    private (int Low, int High) CalculateConfidenceInterval(int score, List<SourceEvidence> sources)
    {
        var margin = 15;

        // More sources = tighter interval
        if (sources.Count >= 3) margin -= 3;
        if (sources.Count >= 5) margin -= 3;

        // High-confidence sources = tighter interval
        var highConfCount = sources.Count(s => s.IsHighConfidence);
        if (highConfCount >= 2) margin -= 3;

        var low = Math.Max(0, score - margin);
        var high = Math.Min(100, score + margin);

        return (low, high);
    }

    /// <summary>
    /// Build primary candidate from INPUT-VERIFIED sources:
    /// - Gravatar (email match)
    /// - Profiles with nickname/name match
    /// - Phone country concordance
    /// </summary>
    private TargetCandidate? BuildPrimaryCandidate(SearchRequest request, List<OsintNode> results, string? phoneCountry)
    {
        var candidate = new TargetCandidate
        {
            // Will be updated with discovered name if found
            Name = "Target",
            PrimaryUsername = NormalizeUsername(request.Nickname ?? request.FullName ?? "")
        };

        var sources = new List<SourceEvidence>();
        var locations = new List<string>();
        var photos = new List<string>();
        var discoveredNames = new List<string>(); // NEW: Collect actual names from APIs
        var attributes = new HashSet<string>();
        
        var inputEmail = request.Email?.ToLower().Trim();
        var inputNickname = NormalizeUsername(request.Nickname ?? "");
        var inputFullName = NormalizeUsername(request.FullName ?? "");

        foreach (var node in results)
        {
            if (!IsPlatformNode(node)) continue;

            var evidence = BuildSourceEvidenceWithInputMatching(request, node, phoneCountry);
            if (evidence == null) continue;
            
            // Only include in PRIMARY candidate if it has a good match to input
            bool isInputMatch = evidence.IsHighConfidence || 
                               evidence.Explanation.Contains("Email verified") ||
                               evidence.Explanation.Contains("exact match") ||
                               evidence.ContributionScore >= 20;
            
            if (isInputMatch)
            {
                sources.Add(evidence);

                // Collect data for analysis
                if (evidence.ExtractedData.TryGetValue("location", out var loc) && !string.IsNullOrEmpty(loc))
                    locations.Add(loc);
                if (evidence.ExtractedData.TryGetValue("photo", out var photo) && !string.IsNullOrEmpty(photo))
                    photos.Add(photo);
                
                // NEW: Collect discovered names from API responses
                if (evidence.ExtractedData.TryGetValue("name", out var name) && !string.IsNullOrEmpty(name))
                    discoveredNames.Add(name);
                if (evidence.ExtractedData.TryGetValue("fullname", out var fullname) && !string.IsNullOrEmpty(fullname))
                    discoveredNames.Add(fullname);
                if (evidence.ExtractedData.TryGetValue("displayname", out var displayname) && !string.IsNullOrEmpty(displayname))
                    discoveredNames.Add(displayname);
                
                InferAttributes(evidence, attributes);
            }
        }
        
        // Add phone country as location
        if (!string.IsNullOrEmpty(phoneCountry))
        {
            locations.Add(phoneCountry);
            attributes.Add($"Located in {phoneCountry}");
        }

        if (sources.Count == 0)
            return null;

        // NEW: Set candidate name from discovered names, fallback to input
        var bestName = GetMostCommon(discoveredNames);
        if (!string.IsNullOrEmpty(bestName))
        {
            candidate.Name = bestName;
            _logger.LogInformation("Using discovered name: {Name}", bestName);
        }
        else if (!string.IsNullOrEmpty(request.FullName))
        {
            candidate.Name = request.FullName;
        }
        else if (!string.IsNullOrEmpty(request.Nickname))
        {
            candidate.Name = request.Nickname;
        }

        candidate.Sources = sources.OrderByDescending(s => s.ContributionScore)
                                    .ThenBy(s => s.PlatformPriority)
                                    .ToList();
        candidate.InferredAttributes = attributes.ToList();
        
        // Set photo (prioritize Gravatar/email-verified, then high-priority platforms)
        var gravatarPhoto = sources.FirstOrDefault(s => s.Platform == "Gravatar")?.ExtractedData.GetValueOrDefault("photo");
        candidate.PhotoUrl = gravatarPhoto ?? photos.FirstOrDefault();
        
        // Set location
        candidate.ProbableLocation = GetMostCommon(locations) ?? phoneCountry ?? "";
        
        // Calculate probability score - PRIMARY gets higher base
        candidate.ProbabilityScore = CalculatePrimaryProbability(request, candidate, phoneCountry);
        
        // Generate analysis
        candidate.ConsistencyAnalysis = GenerateConsistencyAnalysis(candidate, locations, request);
        candidate.UncertaintyNotes = GenerateUncertaintyNotes(candidate, request);

        // Calculate country probability distribution
        candidate.CountryDistribution = _countryDetector.AnalyzeCountryProbability(
            request.Phone, 
            results, 
            request.FullName);

        // Phase 3: Identity linking analysis
        candidate.IdentitySignals = _identityLinker.AnalyzeLinks(sources, request);
        
        // Calculate confidence interval
        var (low, high) = _identityLinker.CalculateConfidenceInterval(
            candidate.ProbabilityScore, 
            candidate.IdentitySignals, 
            sources);
        candidate.ConfidenceLow = low;
        candidate.ConfidenceHigh = high;

        // Update analysis with identity signals
        if (candidate.IdentitySignals.Count > 0)
        {
            var signalSummary = string.Join(", ", candidate.IdentitySignals
                .Take(3)
                .Select(s => s.SignalType.Replace("_", " ")));
            candidate.ConsistencyAnalysis += $" Identity signals: {signalSummary}.";
        }

        return candidate;
    }

    /// <summary>
    /// Build secondary candidates from unmatched sources
    /// </summary>
    private List<TargetCandidate> BuildSecondaryCandidates(SearchRequest request, List<OsintNode> results, string? primaryUsername)
    {
        var clusters = new Dictionary<string, List<OsintNode>>(StringComparer.OrdinalIgnoreCase);

        foreach (var node in results)
        {
            if (!IsPlatformNode(node)) continue;
            
            var username = ExtractUsername(node);
            if (string.IsNullOrEmpty(username)) continue;
            
            var normalizedUsername = NormalizeUsername(username);
            
            // Skip if matches primary
            if (!string.IsNullOrEmpty(primaryUsername) && 
                AreUsernamesSimilar(normalizedUsername, primaryUsername))
                continue;
            
            // Skip if matches input (already in primary)
            var inputNickname = NormalizeUsername(request.Nickname ?? "");
            var inputFullName = NormalizeUsername(request.FullName ?? "");
            if (AreUsernamesSimilar(normalizedUsername, inputNickname) ||
                AreUsernamesSimilar(normalizedUsername, inputFullName))
                continue;

            var matchingCluster = FindMatchingCluster(clusters, normalizedUsername);
            if (matchingCluster != null)
            {
                clusters[matchingCluster].Add(node);
            }
            else
            {
                clusters[normalizedUsername] = new List<OsintNode> { node };
            }
        }

        var candidates = new List<TargetCandidate>();
        foreach (var cluster in clusters)
        {
            var candidate = BuildSecondaryCandidate(request, cluster.Key, cluster.Value);
            if (candidate != null && candidate.Sources.Count > 0)
            {
                candidates.Add(candidate);
            }
        }

        return candidates;
    }

    private TargetCandidate? BuildSecondaryCandidate(SearchRequest request, string clusterName, List<OsintNode> nodes)
    {
        var candidate = new TargetCandidate
        {
            Name = clusterName, // Don't use input name for secondary
            PrimaryUsername = clusterName
        };

        var sources = new List<SourceEvidence>();
        var locations = new List<string>();
        var photos = new List<string>();
        var attributes = new HashSet<string>();

        foreach (var node in nodes)
        {
            var evidence = BuildSourceEvidence(request, node);
            if (evidence != null)
            {
                sources.Add(evidence);

                if (evidence.ExtractedData.TryGetValue("location", out var loc) && !string.IsNullOrEmpty(loc))
                    locations.Add(loc);
                if (evidence.ExtractedData.TryGetValue("photo", out var photo) && !string.IsNullOrEmpty(photo))
                    photos.Add(photo);
                
                InferAttributes(evidence, attributes);
            }
        }

        if (sources.Count == 0) return null;

        candidate.Sources = sources.OrderByDescending(s => s.ContributionScore).ToList();
        candidate.InferredAttributes = attributes.ToList();
        candidate.PhotoUrl = photos.FirstOrDefault();
        candidate.ProbableLocation = GetMostCommon(locations) ?? "";
        
        // Secondary candidates get LOWER scores
        candidate.ProbabilityScore = CalculateSecondaryProbability(candidate);
        candidate.ConsistencyAnalysis = $"No direct match to input data. Found {sources.Count} platform(s) for username '{clusterName}'.";
        candidate.UncertaintyNotes = "May be coincidental match - username doesn't match provided input";

        return candidate;
    }

    private SourceEvidence? BuildSourceEvidenceWithInputMatching(SearchRequest request, OsintNode node, string? phoneCountry)
    {
        var platform = ExtractPlatformName(node);
        if (string.IsNullOrEmpty(platform)) return null;

        var username = ExtractUsername(node);
        var (priority, icon) = GetPlatformInfo(platform);

        var evidence = new SourceEvidence
        {
            Platform = platform,
            Icon = icon,
            Url = ExtractValidUrl(node),
            Username = username ?? "",
            PlatformPriority = priority
        };

        // Calculate contribution WITH input matching
        var (score, explanation, isHighConf) = CalculateContributionWithInput(request, node, username, phoneCountry, platform);
        evidence.ContributionScore = score;
        evidence.Explanation = explanation;
        evidence.IsHighConfidence = isHighConf;

        ExtractNodeData(node, evidence.ExtractedData);

        return evidence;
    }

    private (int Score, string Explanation, bool IsHighConf) CalculateContributionWithInput(
        SearchRequest request, OsintNode node, string? username, string? phoneCountry, string platform)
    {
        var score = 0;
        var reasons = new List<string>();
        var isHighConf = false;

        var inputNickname = NormalizeUsername(request.Nickname ?? "");
        var inputFullName = NormalizeUsername(request.FullName ?? "");
        var inputEmail = request.Email?.ToLower().Trim();
        var normalizedUsername = NormalizeUsername(username ?? "");

        // EMAIL VERIFICATION (Gravatar)
        if (platform.Equals("Gravatar", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(inputEmail))
        {
            score += 35;
            reasons.Add($"Email verified ({inputEmail})");
            isHighConf = true;
        }

        // NICKNAME EXACT MATCH (highest weight for non-email)
        if (!string.IsNullOrEmpty(inputNickname))
        {
            if (normalizedUsername.Equals(inputNickname, StringComparison.OrdinalIgnoreCase))
            {
                score += 30;
                reasons.Add("Nickname exact match");
                isHighConf = true;
            }
            else if (normalizedUsername.Contains(inputNickname) || inputNickname.Contains(normalizedUsername))
            {
                score += 20;
                reasons.Add("Nickname partial match");
            }
        }

        // FULL NAME MATCH
        if (!string.IsNullOrEmpty(inputFullName) && inputFullName != inputNickname)
        {
            if (normalizedUsername.Equals(inputFullName, StringComparison.OrdinalIgnoreCase))
            {
                score += 25;
                reasons.Add("Name exact match");
                isHighConf = true;
            }
            else if (normalizedUsername.Contains(inputFullName) || inputFullName.Contains(normalizedUsername))
            {
                score += 15;
                reasons.Add("Name partial match");
            }
        }

        // LOCATION MATCHES PHONE COUNTRY
        if (!string.IsNullOrEmpty(phoneCountry))
        {
            // Check if node's location matches phone country
            var location = ExtractLocationFromNode(node)?.ToLower() ?? "";
            if (location.Contains(phoneCountry.ToLower()) || phoneCountry.ToLower().Contains(location))
            {
                score += 15;
                reasons.Add($"Location matches phone country ({phoneCountry})");
            }
        }

        // Platform priority bonus
        var (priority, _) = GetPlatformInfo(platform);
        if (priority == 1)
        {
            score += 10;
            reasons.Add("High-priority platform");
        }
        else if (priority == 2)
        {
            score += 5;
        }

        // Verified profile URL
        if (node.Value?.StartsWith("http") == true)
        {
            score += 5;
            reasons.Add("Profile verified");
        }

        return (Math.Min(score, 50), string.Join(", ", reasons), isHighConf);
    }

    private int CalculatePrimaryProbability(SearchRequest request, TargetCandidate candidate, string? phoneCountry)
    {
        var baseScore = 0;

        // Sum contribution scores
        baseScore = candidate.Sources.Sum(s => s.ContributionScore);

        // BONUS: Has email-verified source (Gravatar)
        if (candidate.Sources.Any(s => s.Explanation.Contains("Email verified")))
        {
            baseScore += 20;
        }

        // BONUS: Has nickname exact match
        if (candidate.Sources.Any(s => s.Explanation.Contains("Nickname exact match")))
        {
            baseScore += 15;
        }

        // BONUS: Location matches phone country
        if (!string.IsNullOrEmpty(phoneCountry) && 
            candidate.ProbableLocation.Contains(phoneCountry, StringComparison.OrdinalIgnoreCase))
        {
            baseScore += 15;
        }

        // Bonus for multiple high-priority sources
        var highPrioritySources = candidate.Sources.Count(s => s.PlatformPriority <= 2);
        if (highPrioritySources >= 2) baseScore += 10;
        else if (highPrioritySources >= 1) baseScore += 5;

        // Photo available bonus
        if (!string.IsNullOrEmpty(candidate.PhotoUrl))
        {
            baseScore += 5;
        }

        // Cap at 95 (never 100% certain)
        return Math.Min(baseScore, 95);
    }

    private int CalculateSecondaryProbability(TargetCandidate candidate)
    {
        // Secondary candidates get lower base scores
        var baseScore = candidate.Sources.Sum(s => s.ContributionScore);
        
        // Penalty for not matching input
        baseScore = Math.Max(5, baseScore / 2);
        
        // Cap low
        return Math.Min(baseScore, 30);
    }

    private string GenerateConsistencyAnalysis(TargetCandidate candidate, List<string> locations, SearchRequest request)
    {
        var analysis = new List<string>();

        // Input match status
        var hasEmailMatch = candidate.Sources.Any(s => s.Explanation.Contains("Email verified"));
        var hasNicknameMatch = candidate.Sources.Any(s => s.Explanation.Contains("Nickname exact match"));
        
        if (hasEmailMatch && hasNicknameMatch)
        {
            analysis.Add("✓ Email AND nickname verified - HIGH CONFIDENCE");
        }
        else if (hasEmailMatch)
        {
            analysis.Add("✓ Email verified via Gravatar");
        }
        else if (hasNicknameMatch)
        {
            analysis.Add("✓ Nickname exact match on platforms");
        }

        // Location consistency
        if (locations.Distinct().Count() == 1 && locations.Count > 1)
        {
            analysis.Add($"Location '{locations.First()}' confirmed by multiple sources");
        }
        else if (locations.Count > 0)
        {
            analysis.Add($"Location data from {locations.Count} source(s)");
        }

        // Platform coverage
        var platformCount = candidate.Sources.Count;
        if (platformCount >= 5)
        {
            analysis.Add($"Strong online presence ({platformCount} platforms)");
        }
        else if (platformCount >= 3)
        {
            analysis.Add($"Moderate online presence ({platformCount} platforms)");
        }

        return string.Join(". ", analysis);
    }

    private string GenerateUncertaintyNotes(TargetCandidate candidate, SearchRequest request)
    {
        var notes = new List<string>();

        var hasEmailMatch = candidate.Sources.Any(s => s.Explanation.Contains("Email verified"));
        var hasNicknameMatch = candidate.Sources.Any(s => s.Explanation.Contains("Nickname exact match"));

        if (!hasEmailMatch && !string.IsNullOrEmpty(request.Email))
        {
            notes.Add("Gravatar not found for provided email");
        }

        if (!hasNicknameMatch && !string.IsNullOrEmpty(request.Nickname))
        {
            notes.Add($"No exact match for nickname '{request.Nickname}'");
        }

        if (string.IsNullOrEmpty(candidate.PhotoUrl))
        {
            notes.Add("No profile photo for visual verification");
        }

        if (candidate.Sources.Count < 2)
        {
            notes.Add("Limited source data");
        }

        if (notes.Count == 0)
        {
            return "Good confidence based on input matches";
        }

        return string.Join(". ", notes);
    }

    // Helper methods

    private string? ExtractPhoneCountry(List<OsintNode> results)
    {
        foreach (var node in results)
        {
            if (node.Label?.Contains("Country", StringComparison.OrdinalIgnoreCase) == true)
            {
                // Extract country name (remove flag emoji)
                var value = node.Value ?? "";
                return value.Split(' ').LastOrDefault()?.Trim();
            }
        }
        return null;
    }

    private string? ExtractEmailMatch(List<OsintNode> results)
    {
        foreach (var node in results)
        {
            if (node.Value?.Contains("gravatar.com") == true)
            {
                return "gravatar"; // Email was verified
            }
        }
        return null;
    }

    private string? ExtractLocationFromNode(OsintNode node)
    {
        if (node.Children != null)
        {
            foreach (var child in node.Children)
            {
                if (child.Label?.Contains("location", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return child.Value;
                }
            }
        }
        return null;
    }

    private bool IsPlatformNode(OsintNode node)
    {
        if (string.IsNullOrEmpty(node.Value)) return false;
        return node.Value.StartsWith("http");
    }

    private bool IsProfileNode(OsintNode node)
    {
        return IsPlatformNode(node);
    }

    private string NormalizeUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return "";
        var normalized = url.Trim().ToLower().TrimEnd('/');
        
        // Remove protocol and www for absolute deduplication
        normalized = normalized.Replace("https://", "").Replace("http://", "");
        normalized = normalized.Replace("www.", "");
        
        return normalized;
    }

    private string? ExtractUsername(OsintNode node)
    {
        if (string.IsNullOrEmpty(node.Value)) return null;
        
        try
        {
            var uri = new Uri(node.Value);
            var path = uri.AbsolutePath.Trim('/');
            
            foreach (var prefix in new[] { "user/", "users/", "@", "u/", "id/", "in/", "avatar/" })
            {
                if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    path = path.Substring(prefix.Length);
                }
            }
            
            // For gravatar, the username is the email hash, skip it
            if (uri.Host.Contains("gravatar")) return null;
            
            return path.Split('/').FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    private string ExtractPlatformName(OsintNode node)
    {
        var label = node.Label?.ToLower() ?? "";
        var url = node.Value?.ToLower() ?? "";

        if (url.Contains("github.com")) return "GitHub";
        if (url.Contains("linkedin.com")) return "LinkedIn";
        if (url.Contains("twitter.com") || url.Contains("x.com")) return "X";
        if (url.Contains("facebook.com")) return "Facebook";
        if (url.Contains("instagram.com")) return "Instagram";
        if (url.Contains("youtube.com")) return "YouTube";
        if (url.Contains("twitch.tv")) return "Twitch";
        if (url.Contains("steamcommunity.com")) return "Steam";
        if (url.Contains("reddit.com")) return "Reddit";
        if (url.Contains("gitlab.com")) return "GitLab";
        if (url.Contains("pypi.org")) return "PyPI";
        if (url.Contains("replit.com")) return "Replit";
        if (url.Contains("soundcloud.com")) return "SoundCloud";
        if (url.Contains("gravatar.com")) return "Gravatar";

        return label.Replace("user", "").Trim();
    }

    private (int Priority, string Icon) GetPlatformInfo(string platform)
    {
        var key = platform.ToLower();
        return PlatformInfo.TryGetValue(key, out var info) ? info : (5, "[--]");
    }

    private string NormalizeUsername(string username)
    {
        return username.ToLower()
            .Replace("-", "")
            .Replace("_", "")
            .Replace(".", "")
            .Replace(" ", "");
    }

    private string? FindMatchingCluster(Dictionary<string, List<OsintNode>> clusters, string username)
    {
        foreach (var key in clusters.Keys)
        {
            if (AreUsernamesSimilar(key, username))
            {
                return key;
            }
        }
        return null;
    }

    private bool AreUsernamesSimilar(string a, string b)
    {
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return false;
        if (a.Equals(b, StringComparison.OrdinalIgnoreCase)) return true;
        if (a.Contains(b, StringComparison.OrdinalIgnoreCase) ||
            b.Contains(a, StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    private string? GetMostCommon(List<string> items)
    {
        return items.GroupBy(x => x)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .FirstOrDefault();
    }

    private SourceEvidence? BuildSourceEvidence(SearchRequest request, OsintNode node)
    {
        var platform = ExtractPlatformName(node);
        if (string.IsNullOrEmpty(platform)) return null;

        var username = ExtractUsername(node);
        var (priority, icon) = GetPlatformInfo(platform);

        var evidence = new SourceEvidence
        {
            Platform = platform,
            Icon = icon,
            Url = ExtractValidUrl(node),
            Username = username ?? "",
            PlatformPriority = priority,
            ContributionScore = 10,
            Explanation = "Found on platform",
            IsHighConfidence = false
        };

        ExtractNodeData(node, evidence.ExtractedData);
        return evidence;
    }

    private void ExtractNodeData(OsintNode node, Dictionary<string, string> data)
    {
        if (node.Children != null)
        {
            foreach (var child in node.Children)
            {
                var label = child.Label?.ToLower() ?? "";
                var value = child.Value ?? "";

                if (string.IsNullOrEmpty(value)) continue;

                // Location
                if (label.Contains("location"))
                    data["location"] = value;
                
                // Photo/Avatar
                if (label.Contains("photo") || label.Contains("avatar"))
                    data["photo"] = value;
                
                // Bio
                if (label.Contains("bio"))
                    data["bio"] = value;
                
                // Names - important for candidate identification
                if (label == "name" || label == "full name" || label.Contains("fullname"))
                    data["name"] = value;
                if (label.Contains("display") && label.Contains("name"))
                    data["displayname"] = value;
                if (label == "first name" || label == "firstname")
                    data["firstname"] = value;
                if (label == "last name" || label == "lastname")
                    data["lastname"] = value;
                
                // Username
                if (label == "username" || label == "login" || label == "handle")
                    data["username"] = value;
                
                // Company/Organization
                if (label.Contains("company") || label.Contains("organization") || label.Contains("org"))
                    data["company"] = value;
                
                // Title/Position
                if (label.Contains("title") || label.Contains("position") || label.Contains("job"))
                    data["title"] = value;
            }
        }
    }

    private void InferAttributes(SourceEvidence evidence, HashSet<string> attributes)
    {
        switch (evidence.Platform.ToLower())
        {
            case "github":
            case "gitlab":
            case "pypi":
            case "replit":
                attributes.Add("Developer/Programmer");
                break;
            case "linkedin":
                attributes.Add("Professional");
                break;
            case "twitch":
            case "youtube":
                attributes.Add("Content Creator");
                break;
            case "steam":
                attributes.Add("Gamer");
                break;
            case "soundcloud":
                attributes.Add("Musician/Audio Creator");
                break;
        }
    }
}
