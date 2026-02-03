namespace SherlockOsint.Shared.Models;

/// <summary>
/// Aggregated digital profile estimated from OSINT findings
/// </summary>
public class DigitalProfile
{
    /// <summary>
    /// Unique identifier for this profile
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Full name of the person (from search input or found data)
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Primary email address
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Phone number with country code
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Phone carrier if detected
    /// </summary>
    public string? PhoneCarrier { get; set; }

    /// <summary>
    /// Primary country (from phone or location data)
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Best profile photo URL (prioritized: GitHub > Gravatar > other)
    /// </summary>
    public string? PhotoUrl { get; set; }

    /// <summary>
    /// Location info (city, country from GitHub/other sources)
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Bio/description found from profiles
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// Company/organization
    /// </summary>
    public string? Company { get; set; }

    /// <summary>
    /// Personal website/blog URL
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// Twitter/X handle
    /// </summary>
    public string? Twitter { get; set; }

    /// <summary>
    /// Primary username used across platforms
    /// </summary>
    public string? PrimaryUsername { get; set; }

    /// <summary>
    /// All usernames found across platforms
    /// </summary>
    public List<string> Usernames { get; set; } = new();

    /// <summary>
    /// List of verified platform profiles
    /// </summary>
    public List<FoundPlatform> Platforms { get; set; } = new();

    /// <summary>
    /// Confidence score 0-100 for overall profile accuracy
    /// </summary>
    public int ConfidenceScore { get; set; }

    /// <summary>
    /// When the profile was generated
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// A platform where the user was found
/// </summary>
public class FoundPlatform
{
    public string Name { get; set; } = "";
    public string Url { get; set; } = "";
    public string? Username { get; set; }
    public string? Icon { get; set; }
    public string Category { get; set; } = "Social";
}
