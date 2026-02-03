namespace SherlockOsint.Shared.Models;

/// <summary>
/// Request model for initiating an OSINT search
/// </summary>
public class SearchRequest
{
    /// <summary>
    /// Full name of the person to search for
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// Email address to search for
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Phone number to search for
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Nickname/username to search for
    /// </summary>
    public string? Nickname { get; set; }

    /// <summary>
    /// Unique connection ID for the requesting client
    /// </summary>
    public string? ConnectionId { get; set; }

    /// <summary>
    /// Returns true if at least one search field has a value
    /// </summary>
    public bool HasSearchCriteria =>
        !string.IsNullOrWhiteSpace(FullName) ||
        !string.IsNullOrWhiteSpace(Email) ||
        !string.IsNullOrWhiteSpace(Phone) ||
        !string.IsNullOrWhiteSpace(Nickname);
}
