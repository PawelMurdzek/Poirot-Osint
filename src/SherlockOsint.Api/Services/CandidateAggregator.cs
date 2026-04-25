using SherlockOsint.Shared.Models;
using SherlockOsint.Api.Services.OsintProviders;

namespace SherlockOsint.Api.Services;

/// <summary>
/// Aggregates OSINT results into target candidates.
/// Uses Claude AI for probability scoring and identity analysis.
/// </summary>
public class CandidateAggregator
{
    private readonly ILogger<CandidateAggregator> _logger;
    private readonly CountryDetector _countryDetector;
    private readonly IdentityLinker _identityLinker;
    private readonly ClaudeAnalysisService _claudeAnalysis;

    // Platform priorities and icons (1 = highest importance)
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
        { "gravatar", (1, "[GR]") }, // High priority — email-verified
    };

    public CandidateAggregator(
        ILogger<CandidateAggregator> logger,
        CountryDetector countryDetector,
        IdentityLinker identityLinker,
        ClaudeAnalysisService claudeAnalysis)
    {
        _logger = logger;
        _countryDetector = countryDetector;
        _identityLinker = identityLinker;
        _claudeAnalysis = claudeAnalysis;
    }

    /// <summary>
    /// Build target candidates from search results.
    /// Groups results by username, merges where evidence links them,
    /// then calls Claude to assess probability and generate analysis.
    /// </summary>
    public async Task<List<TargetCandidate>> BuildCandidatesAsync(
        SearchRequest request,
        List<OsintNode> results,
        CancellationToken ct = default)
    {
        string? phoneCountry = ExtractPhoneCountry(results);

        _logger.LogInformation("Building candidates: Email={Email}, Nickname={Nickname}, Phone={Phone}",
            request.Email, request.Nickname, request.Phone);

        // STEP 1: Group all platform nodes by normalised username
        var usernameGroups = new Dictionary<string, List<(OsintNode Node, SourceEvidence Evidence)>>(StringComparer.OrdinalIgnoreCase);
        var emailToUsernames = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        var phoneToUsernames = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        var usernameToDisplayName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var seenUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Seed phone bucket with the search-input phone so any candidate associated
        // with this phone (e.g. via FullContact reverse-lookup) clusters together.
        var inputPhoneNormalised = NormalisePhone(request.Phone);

        foreach (var node in results)
        {
            if (!IsPlatformNode(node)) continue;

            var evidence = BuildSourceEvidence(node);
            if (evidence == null) continue;

            var normalizedUrl = NormalizeUrl(evidence.Url);
            if (string.IsNullOrEmpty(normalizedUrl) || seenUrls.Contains(normalizedUrl)) continue;
            seenUrls.Add(normalizedUrl);
            evidence.Url = normalizedUrl;

            var normalizedUsername = NormalizeUsername(evidence.Username);
            if (string.IsNullOrEmpty(normalizedUsername)) continue;

            if (!usernameGroups.ContainsKey(normalizedUsername))
                usernameGroups[normalizedUsername] = [];
            usernameGroups[normalizedUsername].Add((node, evidence));

            if (evidence.ExtractedData.TryGetValue("email", out var email) && !string.IsNullOrEmpty(email))
            {
                var lowerEmail = email.ToLower();
                if (!emailToUsernames.ContainsKey(lowerEmail))
                    emailToUsernames[lowerEmail] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                emailToUsernames[lowerEmail].Add(normalizedUsername);
            }

            if (evidence.ExtractedData.TryGetValue("phone", out var phone) && !string.IsNullOrEmpty(phone))
            {
                var p = NormalisePhone(phone);
                if (!string.IsNullOrEmpty(p))
                {
                    if (!phoneToUsernames.ContainsKey(p))
                        phoneToUsernames[p] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    phoneToUsernames[p].Add(normalizedUsername);
                }
            }

            // Also link the input phone to every candidate it touches via FullContact
            // (FullContact returns the phone in the parent node value).
            if (!string.IsNullOrEmpty(inputPhoneNormalised) && evidence.Platform.Contains("FullContact", StringComparison.OrdinalIgnoreCase))
            {
                if (!phoneToUsernames.ContainsKey(inputPhoneNormalised))
                    phoneToUsernames[inputPhoneNormalised] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                phoneToUsernames[inputPhoneNormalised].Add(normalizedUsername);
            }

            // Track display name per username for similarity-based merging
            var displayName = evidence.ExtractedData.GetValueOrDefault("name")
                ?? evidence.ExtractedData.GetValueOrDefault("displayname")
                ?? evidence.DisplayName;
            if (!string.IsNullOrWhiteSpace(displayName) && !usernameToDisplayName.ContainsKey(normalizedUsername))
                usernameToDisplayName[normalizedUsername] = displayName;
        }

        _logger.LogInformation("Found {Count} unique usernames", usernameGroups.Count);

        // STEP 2: Detect merges — shared email > shared phone > similar display name
        var mergeGroups = DetectMergeGroups(
            usernameGroups.Keys.ToList(),
            emailToUsernames,
            phoneToUsernames,
            usernameToDisplayName);

        // STEP 3: Build candidate objects (sources + data, no scoring yet)
        var candidates = new List<TargetCandidate>();
        var processedUsernames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var mergeGroup in mergeGroups)
        {
            var primaryUsername = mergeGroup.Usernames.First();
            if (processedUsernames.Contains(primaryUsername)) continue;

            var candidate = BuildCandidateFromGroup(
                request, mergeGroup.Usernames, usernameGroups,
                mergeGroup.MergeReason, phoneCountry, results);

            if (candidate != null && candidate.Sources.Count > 0)
            {
                candidates.Add(candidate);
                foreach (var u in mergeGroup.Usernames)
                    processedUsernames.Add(u);
            }
        }

        // STEP 4: Mark which candidates come from user-supplied input vs the
        // permutator. The flag travels with the candidate into the /sessions
        // memory file so Claude (live or via CLI) can downrank speculative ones.
        FlagUserInputCandidates(request, candidates);

        // STEP 5: Call Claude to assess all candidates at once. When Claude is
        // not configured we deliberately do NOT apply any arbitrary fallback —
        // the SearchOrchestrator persists candidates to /sessions and emits a
        // CLI command pointing the user's local Claude CLI at that folder.
        var assessments = await _claudeAnalysis.AnalyzeCandidatesAsync(request, candidates, ct);

        if (assessments.Count > 0)
        {
            ApplyClaudeAssessments(candidates, assessments);
            candidates = [.. candidates.OrderByDescending(c => c.ProbabilityScore)];
        }
        else
        {
            ApplyNoAiAnnotation(candidates);
            // Order: user-input first, then by source count + high-priority sources
            candidates = [.. candidates
                .OrderByDescending(c => c.IsFromUserInput)
                .ThenByDescending(c => c.Sources.Count(s => s.PlatformPriority <= 2))
                .ThenByDescending(c => c.Sources.Count)];
        }

        _logger.LogInformation("Built {CandidateCount} candidates from {UsernameCount} usernames (AI={AiUsed})",
            candidates.Count, usernameGroups.Count, assessments.Count > 0);
        return candidates;
    }

    /// <summary>
    /// True when the candidate's PrimaryUsername (or any alias) matches a token
    /// the user typed in: the explicit nickname or the email local-part.
    /// Also true when the candidate has a corroborating signal beyond mere
    /// username collision — shared email or shared phone with the input.
    /// </summary>
    private void FlagUserInputCandidates(SearchRequest request, List<TargetCandidate> candidates)
    {
        var inputTokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(request.Nickname))
            inputTokens.Add(NormalizeUsername(request.Nickname!));
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var at = request.Email!.IndexOf('@');
            if (at > 0) inputTokens.Add(NormalizeUsername(request.Email![..at]));
        }

        foreach (var candidate in candidates)
        {
            var primary = NormalizeUsername(candidate.PrimaryUsername ?? "");
            var aliasMatch = candidate.KnownAliases.Any(a => inputTokens.Contains(NormalizeUsername(a)));
            var emailMatch = !string.IsNullOrWhiteSpace(request.Email)
                && candidate.VerifiedEmails.Any(v => v.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase));
            var mergedByCorroboration = !string.IsNullOrEmpty(candidate.MergeReason)
                && (candidate.MergeReason.StartsWith("Same email", StringComparison.OrdinalIgnoreCase)
                 || candidate.MergeReason.StartsWith("Same phone", StringComparison.OrdinalIgnoreCase));

            candidate.IsFromUserInput = inputTokens.Contains(primary) || aliasMatch || emailMatch || mergedByCorroboration;
        }
    }

    // ── Scoring ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Apply Claude's assessments to the corresponding candidates.
    /// Falls back to simple scoring for any candidate Claude missed.
    /// </summary>
    private void ApplyClaudeAssessments(List<TargetCandidate> candidates, List<CandidateAssessment> assessments)
    {
        var byId = assessments.ToDictionary(a => a.CandidateId, StringComparer.OrdinalIgnoreCase);

        foreach (var candidate in candidates)
        {
            if (!byId.TryGetValue(candidate.Id, out var assessment))
            {
                _logger.LogWarning("No Claude assessment for candidate {Id} ({Username})",
                    candidate.Id, candidate.PrimaryUsername);
                candidate.ProbabilityScore = 0;
                candidate.ConfidenceLow = 0;
                candidate.ConfidenceHigh = 0;
                candidate.ConsistencyAnalysis = $"Found on {candidate.Sources.Count} platform(s) as '{candidate.PrimaryUsername}'.";
                candidate.UncertaintyNotes = "Claude returned no assessment for this candidate — see /sessions for raw evidence.";
                continue;
            }

            candidate.ProbabilityScore = Math.Clamp(assessment.ProbabilityScore, 0, 95);
            candidate.ConsistencyAnalysis = assessment.ConsistencyAnalysis;
            candidate.UncertaintyNotes = assessment.UncertaintyNotes;
            candidate.ProfessionalRole = assessment.ProfessionalRole ?? candidate.ProfessionalRole;
            candidate.ActivitySummary = assessment.ActivitySummary;
            candidate.ConfidenceLow = Math.Clamp(assessment.ConfidenceLow, 0, 100);
            candidate.ConfidenceHigh = Math.Clamp(assessment.ConfidenceHigh, 0, 100);
        }
    }

    /// <summary>
    /// When Claude is unavailable we annotate candidates instead of inventing
    /// probability numbers. The SearchOrchestrator persists everything to
    /// /sessions and surfaces a CLI command for the user's local Claude.
    /// </summary>
    private void ApplyNoAiAnnotation(List<TargetCandidate> candidates)
    {
        foreach (var candidate in candidates)
        {
            candidate.ProbabilityScore = 0;
            candidate.ConfidenceLow = 0;
            candidate.ConfidenceHigh = 0;
            candidate.ConsistencyAnalysis = $"Found on {candidate.Sources.Count} platform(s) as '{candidate.PrimaryUsername}'.";
            candidate.UncertaintyNotes = "Not scored — Claude API key not configured. " +
                "Run the `claude` command emitted on search-complete against the /sessions snapshot to rank these locally.";
        }
    }

    // ── Group detection ──────────────────────────────────────────────────────────

    private List<MergeGroup> DetectMergeGroups(
        List<string> usernames,
        Dictionary<string, HashSet<string>> emailToUsernames,
        Dictionary<string, HashSet<string>>? phoneToUsernames = null,
        Dictionary<string, string>? usernameToDisplayName = null)
    {
        var groups = new List<MergeGroup>();
        var assigned = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Pass 1 — merge by shared email (strongest signal)
        foreach (var email in emailToUsernames.Keys)
        {
            var shared = emailToUsernames[email].Where(u => usernames.Contains(u)).ToList();
            if (shared.Count <= 1) continue;

            var unassigned = shared.Where(u => !assigned.Contains(u)).ToList();
            if (unassigned.Count == 0) continue;

            groups.Add(new MergeGroup { Usernames = unassigned, MergeReason = $"Same email: {email}" });
            foreach (var u in unassigned) assigned.Add(u);
        }

        // Pass 2 — merge by shared phone (strong)
        if (phoneToUsernames != null)
        {
            foreach (var phone in phoneToUsernames.Keys)
            {
                var shared = phoneToUsernames[phone].Where(u => usernames.Contains(u) && !assigned.Contains(u)).ToList();
                if (shared.Count <= 1) continue;

                groups.Add(new MergeGroup { Usernames = shared, MergeReason = $"Same phone: {phone}" });
                foreach (var u in shared) assigned.Add(u);
            }
        }

        // Pass 3 — merge by display-name similarity (weakest, but catches "Pawel Murdzek" vs "Paweł Murdzek")
        if (usernameToDisplayName != null && usernameToDisplayName.Count > 1)
        {
            var pending = usernames.Where(u => !assigned.Contains(u)).ToList();
            for (int i = 0; i < pending.Count; i++)
            {
                if (assigned.Contains(pending[i])) continue;
                if (!usernameToDisplayName.TryGetValue(pending[i], out var nameI) || string.IsNullOrWhiteSpace(nameI)) continue;

                var clusterMembers = new List<string> { pending[i] };
                for (int j = i + 1; j < pending.Count; j++)
                {
                    if (assigned.Contains(pending[j])) continue;
                    if (!usernameToDisplayName.TryGetValue(pending[j], out var nameJ) || string.IsNullOrWhiteSpace(nameJ)) continue;

                    if (NamesAreSimilar(nameI, nameJ))
                        clusterMembers.Add(pending[j]);
                }

                if (clusterMembers.Count > 1)
                {
                    groups.Add(new MergeGroup { Usernames = clusterMembers, MergeReason = $"Similar display names: \"{nameI}\"" });
                    foreach (var u in clusterMembers) assigned.Add(u);
                }
            }
        }

        // Pass 4 — singletons
        foreach (var username in usernames)
        {
            if (!assigned.Contains(username))
                groups.Add(new MergeGroup { Usernames = [username], MergeReason = null });
        }

        return groups;
    }

    /// <summary>
    /// Two display names are "similar" if their normalised (diacritic-stripped, lowercase)
    /// forms have Levenshtein distance ≤ 2 over the longer string. Defensive: at least
    /// one of them must contain a space (i.e. plausibly first+last) to avoid clustering
    /// generic single-word handles.
    /// </summary>
    private static bool NamesAreSimilar(string a, string b)
    {
        var na = NormaliseForCompare(a);
        var nb = NormaliseForCompare(b);
        if (string.IsNullOrEmpty(na) || string.IsNullOrEmpty(nb)) return false;
        if (!na.Contains(' ') && !nb.Contains(' ')) return false; // require at least one to look like full name
        if (na == nb) return true;

        // Cheap shortcut: identical token sets in any order
        var ta = na.Split(' ', StringSplitOptions.RemoveEmptyEntries).OrderBy(x => x);
        var tb = nb.Split(' ', StringSplitOptions.RemoveEmptyEntries).OrderBy(x => x);
        if (ta.SequenceEqual(tb)) return true;

        // Levenshtein with cap on the longer string
        var dist = Levenshtein(na, nb);
        return dist <= 2;
    }

    private static string NormaliseForCompare(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        var normalised = s.Normalize(System.Text.NormalizationForm.FormD);
        var sb = new System.Text.StringBuilder(normalised.Length);
        foreach (var ch in normalised)
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch) != System.Globalization.UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }
        return sb.ToString().ToLowerInvariant()
            .Replace("ł", "l").Replace("Ł", "l")
            .Trim();
    }

    private static int Levenshtein(string a, string b)
    {
        if (a.Length == 0) return b.Length;
        if (b.Length == 0) return a.Length;
        var prev = new int[b.Length + 1];
        var curr = new int[b.Length + 1];
        for (int j = 0; j <= b.Length; j++) prev[j] = j;
        for (int i = 1; i <= a.Length; i++)
        {
            curr[0] = i;
            for (int j = 1; j <= b.Length; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                curr[j] = Math.Min(Math.Min(curr[j - 1] + 1, prev[j] + 1), prev[j - 1] + cost);
            }
            (prev, curr) = (curr, prev);
        }
        return prev[b.Length];
    }

    private sealed class MergeGroup
    {
        public List<string> Usernames { get; set; } = [];
        public string? MergeReason { get; set; }
    }

    // ── Candidate building ───────────────────────────────────────────────────────

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
            KnownAliases = usernames.Count > 1 ? usernames.Skip(1).ToList() : []
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
                if (!string.IsNullOrEmpty(evidence.Url) && seenUrls.Contains(evidence.Url)) continue;
                if (!string.IsNullOrEmpty(evidence.Url)) seenUrls.Add(evidence.Url);

                ScoreEvidence(request, evidence, phoneCountry);
                allSources.Add(evidence);

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

        // Enrich from Hunter/Clearbit/FullContact nodes if they match a discovered email
        foreach (var node in results)
        {
            if (IsPlatformNode(node)) continue;
            if (node.Label == null) continue;
            if (!node.Label.Contains("HUNTER") && !node.Label.Contains("CLEARBIT") && !node.Label.Contains("FULLCONTACT"))
                continue;

            if (node.Value?.Equals(request.Email, StringComparison.OrdinalIgnoreCase) == true ||
                discoveredEmails.Any(e => e.Email.Equals(node.Value, StringComparison.OrdinalIgnoreCase)))
            {
                ProcessEnrichmentNode(node, discoveredNames, discoveredEmails, locations, attributes, photos);
            }
        }

        if (allSources.Count == 0) return null;

        var bestName = GetMostCommon(discoveredNames);
        candidate.Name = !string.IsNullOrEmpty(bestName) ? bestName
            : !string.IsNullOrEmpty(request.FullName) ? request.FullName
            : usernames.First();

        // Best source per platform, filtered for noise
        var bestSourcesPerPlatform = allSources
            .Where(s => s.ContributionScore > 0 || s.PlatformPriority <= 2)
            .GroupBy(s => s.Platform.ToLower().Replace("twitter", "x"))
            .Select(g => g.OrderByDescending(s => s.ContributionScore).ThenBy(s => s.PlatformPriority).First())
            .ToList();

        candidate.Sources = [.. bestSourcesPerPlatform.OrderByDescending(s => s.ContributionScore).ThenBy(s => s.PlatformPriority)];
        candidate.VerifiedEmails = discoveredEmails.DistinctBy(e => e.Email.ToLower()).ToList();
        candidate.PhotoUrl = photos.FirstOrDefault();
        candidate.ProbableLocation = GetMostCommon(locations) ?? phoneCountry ?? "";
        candidate.InferredAttributes = attributes.ToList();
        candidate.ProfessionalRole = InferProfessionalRole(attributes);
        candidate.CountryDistribution = _countryDetector.AnalyzeCountryProbability(request.Phone, [], request.FullName);
        candidate.IdentitySignals = _identityLinker.AnalyzeLinks(allSources, request);

        return candidate;
    }

    // ── Evidence helpers ─────────────────────────────────────────────────────────

    private void ProcessEnrichmentNode(
        OsintNode node,
        List<string> names, List<VerifiedEmail> emails,
        List<string> locations, HashSet<string> attributes, List<string> photos)
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

    private SourceEvidence? BuildSourceEvidence(OsintNode node)
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

        ExtractNodeData(node, evidence.ExtractedData);
        evidence.DisplayName = evidence.ExtractedData.GetValueOrDefault("name");
        evidence.Bio = evidence.ExtractedData.GetValueOrDefault("bio");

        return evidence;
    }

    /// <summary>
    /// Scores each source's contribution — used to rank/filter sources within a candidate.
    /// The overall candidate probability score is set by Claude.
    /// </summary>
    private void ScoreEvidence(SearchRequest request, SourceEvidence evidence, string? phoneCountry)
    {
        var score = 0;
        var reasons = new List<string>();
        var isHighConf = false;

        var inputNickname = NormalizeUsername(request.Nickname ?? "");
        var inputFullName = NormalizeUsername(request.FullName ?? "");
        var normalizedUsername = NormalizeUsername(evidence.Username);

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

        if (evidence.PlatformPriority == 1)
        {
            score += 15;
            reasons.Add("High-priority platform");
        }

        if (!string.IsNullOrEmpty(inputNickname) &&
            normalizedUsername.Equals(inputNickname, StringComparison.OrdinalIgnoreCase))
        {
            var matchScore = 30;
            if (evidence.ExtractedData.TryGetValue("Confidence", out var confStr) &&
                decimal.TryParse(confStr, out var conf))
            {
                if (conf < 20) matchScore = 5;
                else if (conf < 40) matchScore = 15;
            }

            score += (int)(matchScore * multiplier);
            reasons.Add($"Nickname match ({evidence.Platform} x{multiplier:0.0})");
            isHighConf = evidence.PlatformPriority <= 2;
        }

        if (!string.IsNullOrEmpty(inputFullName) && normalizedUsername.Contains(inputFullName))
        {
            score += (int)(20 * multiplier);
            reasons.Add("Name match");
        }

        evidence.ContributionScore = Math.Min(score, 60);
        evidence.Explanation = string.Join(", ", reasons);
        evidence.IsHighConfidence = isHighConf;
    }

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

    // ── Extraction helpers ───────────────────────────────────────────────────────

    private string? ExtractPhoneCountry(List<OsintNode> results)
    {
        foreach (var node in results)
        {
            if (node.Label?.Contains("Country", StringComparison.OrdinalIgnoreCase) == true)
                return (node.Value ?? "").Split(' ').LastOrDefault()?.Trim();
        }
        return null;
    }

    private bool IsPlatformNode(OsintNode node) =>
        !string.IsNullOrEmpty(node.Value) && node.Value.StartsWith("http");

    private string NormalizeUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return "";
        return url.Trim().ToLower().TrimEnd('/')
            .Replace("https://", "")
            .Replace("http://", "")
            .Replace("www.", "");
    }

    private string? ExtractUsername(OsintNode node)
    {
        if (string.IsNullOrEmpty(node.Value)) return null;
        try
        {
            var uri = new Uri(node.Value);
            if (uri.Host.Contains("gravatar")) return null;

            var path = uri.AbsolutePath.Trim('/');
            foreach (var prefix in new[] { "user/", "users/", "@", "u/", "id/", "in/", "avatar/" })
            {
                if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    path = path[prefix.Length..];
            }
            return path.Split('/').FirstOrDefault();
        }
        catch { return null; }
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

    private string ExtractValidUrl(OsintNode node)
    {
        if (!string.IsNullOrEmpty(node.Value) && node.Value.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            return node.Value;

        foreach (var child in node.Children)
        {
            if (!string.IsNullOrEmpty(child.Value) && child.Value.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return child.Value;
        }
        return "";
    }

    private void ExtractNodeData(OsintNode node, Dictionary<string, string> data)
    {
        if (node.Children == null) return;

        foreach (var child in node.Children)
        {
            var label = child.Label?.ToLower() ?? "";
            var value = child.Value ?? "";
            if (string.IsNullOrEmpty(value)) continue;

            if (label.Contains("location")) data["location"] = value;
            if (label.Contains("photo") || label.Contains("avatar")) data["photo"] = value;
            if (label.Contains("bio")) data["bio"] = value;
            if (label == "name" || label == "full name" || label.Contains("fullname")) data["name"] = value;
            if (label.Contains("display") && label.Contains("name")) data["displayname"] = value;
            if (label == "first name" || label == "firstname") data["firstname"] = value;
            if (label == "last name" || label == "lastname") data["lastname"] = value;
            if (label == "username" || label == "login" || label == "handle") data["username"] = value;
            if (label.Contains("company") || label.Contains("organization") || label.Contains("org")) data["company"] = value;
            if (label.Contains("title") || label.Contains("position") || label.Contains("job")) data["title"] = value;
        }
    }

    private (int Priority, string Icon) GetPlatformInfo(string platform)
    {
        var key = platform.ToLower();
        return PlatformInfo.TryGetValue(key, out var info) ? info : (5, "[--]");
    }

    private string NormalizeUsername(string username) =>
        username.ToLower().Replace("-", "").Replace("_", "").Replace(".", "").Replace(" ", "");

    private string? GetMostCommon(List<string> items) =>
        items.GroupBy(x => x).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault();

    /// <summary>
    /// Normalise a phone number to digits-only with optional leading "+". Strips
    /// spaces, dashes, parens, "ext" / "x" extensions. Returns "" for clearly-invalid.
    /// </summary>
    private static string NormalisePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return "";
        var trimmed = phone.Trim();
        var prefix = trimmed.StartsWith("+") ? "+" : "";
        var digits = new string(trimmed.Where(char.IsDigit).ToArray());
        // Drop short codes / extensions
        if (digits.Length < 7) return "";
        return prefix + digits;
    }
}
