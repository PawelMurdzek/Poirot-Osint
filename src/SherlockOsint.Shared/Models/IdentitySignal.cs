namespace SherlockOsint.Shared.Models;

/// <summary>
/// Represents a signal that links identities across platforms
/// </summary>
public class IdentitySignal
{
    /// <summary>
    /// Type of signal: "photo_match", "location", "linked_mention", "username_pattern", "email_verified"
    /// </summary>
    public string SignalType { get; set; } = "";

    /// <summary>
    /// Source platforms involved (e.g., "GitHub + Twitter")
    /// </summary>
    public string Source { get; set; } = "";

    /// <summary>
    /// Score contribution from this signal
    /// </summary>
    public int Weight { get; set; }

    /// <summary>
    /// Human-readable description of the evidence
    /// </summary>
    public string Evidence { get; set; } = "";

    /// <summary>
    /// Confidence level: "high", "medium", "low"
    /// </summary>
    public string Confidence { get; set; } = "medium";
}
