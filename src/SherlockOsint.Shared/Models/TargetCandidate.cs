namespace SherlockOsint.Shared.Models;

/// <summary>
/// Represents a potential target identity with probability scoring.
/// Each candidate represents ONE person - multiple profiles only merged with evidence.
/// </summary>
public class TargetCandidate
{
    /// <summary>
    /// Unique identifier for this candidate
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Full name of the potential target (discovered from APIs, not just input)
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Most likely country based on evidence
    /// </summary>
    public string ProbableLocation { get; set; } = "";

    /// <summary>
    /// Probability score 0-100% that this is the correct identity
    /// </summary>
    public int ProbabilityScore { get; set; }

    /// <summary>
    /// Best available profile photo URL
    /// </summary>
    public string? PhotoUrl { get; set; }

    /// <summary>
    /// Evidence from each source platform
    /// </summary>
    public List<SourceEvidence> Sources { get; set; } = new();

    /// <summary>
    /// Key attributes inferred from aggregated data
    /// </summary>
    public List<string> InferredAttributes { get; set; } = new();

    /// <summary>
    /// Cross-source consistency analysis summary
    /// </summary>
    public string ConsistencyAnalysis { get; set; } = "";

    /// <summary>
    /// Notes about assumptions and uncertainties
    /// </summary>
    public string UncertaintyNotes { get; set; } = "";

    /// <summary>
    /// Primary username used across platforms
    /// </summary>
    public string? PrimaryUsername { get; set; }

    /// <summary>
    /// Country probability distribution based on evidence
    /// </summary>
    public List<CountryProbability> CountryDistribution { get; set; } = new();

    /// <summary>
    /// Identity linking signals found across platforms
    /// </summary>
    public List<IdentitySignal> IdentitySignals { get; set; } = new();

    /// <summary>
    /// Lower bound of confidence interval (e.g., 65 for 65-85%)
    /// </summary>
    public int ConfidenceLow { get; set; }

    /// <summary>
    /// Upper bound of confidence interval (e.g., 85 for 65-85%)
    /// </summary>
    public int ConfidenceHigh { get; set; }

    /// <summary>
    /// When this candidate was identified
    /// </summary>
    public DateTime IdentifiedAt { get; set; } = DateTime.UtcNow;

    // === NEW FIELDS FOR PROPER IDENTITY MODEL ===

    /// <summary>
    /// Other usernames/aliases confirmed to belong to this person
    /// </summary>
    public List<string> KnownAliases { get; set; } = new();

    /// <summary>
    /// Verified emails associated with this person
    /// </summary>
    public List<VerifiedEmail> VerifiedEmails { get; set; } = new();

    /// <summary>
    /// If profiles were merged, explains why (e.g., "Same email: user@example.com")
    /// Empty if no merge occurred
    /// </summary>
    public string? MergeReason { get; set; }

    /// <summary>
    /// Professional role/focus inferred from platforms
    /// </summary>
    public string? ProfessionalRole { get; set; }

    /// <summary>
    /// Brief activity summary across platforms
    /// </summary>
    public string? ActivitySummary { get; set; }
}

/// <summary>
/// Verified email with confidence level
/// </summary>
public class VerifiedEmail
{
    public string Email { get; set; } = "";
    public string Source { get; set; } = "";  // Where it was found (GitHub commits, Hunter.io, etc.)
    public int Confidence { get; set; }  // 0-100
    public bool IsVerified { get; set; }  // Actually verified (not just discovered)
}

/// <summary>
/// Country probability for a target candidate
/// </summary>
public class CountryProbability
{
    public string Country { get; set; } = "";
    public string CountryCode { get; set; } = "";
    public string Flag { get; set; } = "🌍";
    public int Probability { get; set; }
    public string Evidence { get; set; } = "";
}

/// <summary>
/// Evidence from a single source platform
/// </summary>
public class SourceEvidence
{
    /// <summary>
    /// Platform name (GitHub, X, LinkedIn, Steam, etc.)
    /// </summary>
    public string Platform { get; set; } = "";

    /// <summary>
    /// Platform icon/emoji
    /// </summary>
    public string Icon { get; set; } = "🌐";

    /// <summary>
    /// Profile URL on this platform
    /// </summary>
    public string Url { get; set; } = "";

    /// <summary>
    /// Username on this platform
    /// </summary>
    public string Username { get; set; } = "";

    /// <summary>
    /// How much this source contributes to probability (0-100)
    /// </summary>
    public int ContributionScore { get; set; }

    /// <summary>
    /// Explanation of how this source contributes
    /// Example: "Username exact match, location matches Poland"
    /// </summary>
    public string Explanation { get; set; } = "";

    /// <summary>
    /// Data extracted from this source
    /// </summary>
    public Dictionary<string, string> ExtractedData { get; set; } = new();

    /// <summary>
    /// Whether this is a high-confidence match
    /// </summary>
    public bool IsHighConfidence { get; set; }

    /// <summary>
    /// Platform priority (1=highest: GitHub, LinkedIn, X)
    /// </summary>
    public int PlatformPriority { get; set; } = 5;

    /// <summary>
    /// True if this data was inferred/estimated rather than directly observed
    /// </summary>
    public bool IsInferred { get; set; }

    /// <summary>
    /// Display name found on this platform (different from username)
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Bio/description from this platform
    /// </summary>
    public string? Bio { get; set; }
}
