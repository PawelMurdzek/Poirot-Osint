namespace SherlockOsint.Shared.Models;

/// <summary>
/// Production OSINT search result with strict verification
/// </summary>
public class OsintSearchResult
{
    /// <summary>
    /// Status: "success" | "partial" | "no_results"
    /// </summary>
    public string Status { get; set; } = "no_results";

    /// <summary>
    /// Profiles found with verified evidence
    /// </summary>
    public List<VerifiedProfile> ProfilesFound { get; set; } = new();

    /// <summary>
    /// Infrastructure findings (domains, IPs, certificates)
    /// </summary>
    public List<InfrastructureResult> Infrastructure { get; set; } = new();

    /// <summary>
    /// Breach exposure from HIBP and similar
    /// </summary>
    public List<BreachExposure> BreachExposure { get; set; } = new();

    /// <summary>
    /// Candidates that need more verification
    /// </summary>
    public List<UnverifiedCandidate> UnverifiedCandidates { get; set; } = new();

    /// <summary>
    /// Platforms explicitly searched but returned no results
    /// </summary>
    public List<string> NotFoundPlatforms { get; set; } = new();

    /// <summary>
    /// Limitations encountered (rate limits, errors, legal restrictions)
    /// </summary>
    public List<string> Limitations { get; set; } = new();

    /// <summary>
    /// Search metadata
    /// </summary>
    public SearchMetadata Metadata { get; set; } = new();
}

/// <summary>
/// A verified profile with clickable URL and evidence
/// </summary>
public class VerifiedProfile
{
    /// <summary>
    /// MANDATORY: Clickable URL to the profile
    /// </summary>
    public string Url { get; set; } = "";

    /// <summary>
    /// Platform name (GitHub, YouTube, etc.)
    /// </summary>
    public string Platform { get; set; } = "";

    /// <summary>
    /// Confidence score 0.00 - 1.00
    /// </summary>
    public decimal ConfidenceScore { get; set; }

    /// <summary>
    /// Evidence explaining why we believe this is the target
    /// </summary>
    public List<string> Evidence { get; set; } = new();

    /// <summary>
    /// Username on this platform
    /// </summary>
    public string Username { get; set; } = "";

    /// <summary>
    /// Display name if available
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Bio/description if available
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// Location if found
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Profile photo URL if available
    /// </summary>
    public string? PhotoUrl { get; set; }

    /// <summary>
    /// Last activity date if known
    /// </summary>
    public DateTime? LastActivity { get; set; }

    /// <summary>
    /// Emails discovered from this profile
    /// </summary>
    public List<string> DiscoveredEmails { get; set; } = new();
}

/// <summary>
/// Infrastructure findings (domains, certificates, etc.)
/// </summary>
public class InfrastructureResult
{
    public string Type { get; set; } = "";  // domain, certificate, dns
    public string Value { get; set; } = "";
    public string? Url { get; set; }
    public string Source { get; set; } = "";
    public Dictionary<string, string> Details { get; set; } = new();
}

/// <summary>
/// Breach exposure data
/// </summary>
public class BreachExposure
{
    public string BreachName { get; set; } = "";
    public string Domain { get; set; } = "";
    public DateTime? BreachDate { get; set; }
    public List<string> DataTypes { get; set; } = new();  // email, password, etc.
    public string Source { get; set; } = "HIBP";
}

/// <summary>
/// Candidate that needs more verification
/// </summary>
public class UnverifiedCandidate
{
    public string Url { get; set; } = "";
    public string Platform { get; set; } = "";
    public string Reason { get; set; } = "";  // Why it's unverified
    public decimal PotentialConfidence { get; set; }
}

/// <summary>
/// Search metadata
/// </summary>
public class SearchMetadata
{
    public DateTime SearchStarted { get; set; } = DateTime.UtcNow;
    public DateTime? SearchCompleted { get; set; }
    public int TotalPlatformsQueried { get; set; }
    public int PlatformsWithResults { get; set; }
    public int PlatformsWithNoResults { get; set; }
    public List<string> QueriedInputs { get; set; } = new();
}
