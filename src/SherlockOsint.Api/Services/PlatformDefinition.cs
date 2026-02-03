namespace SherlockOsint.Api.Services;

/// <summary>
/// Defines how to check if a username exists on a platform
/// </summary>
public class PlatformDefinition
{
    /// <summary>Platform name (e.g., "GitHub", "Twitter")</summary>
    public required string Name { get; init; }
    
    /// <summary>Category (e.g., "Social Media", "Development")</summary>
    public required string Category { get; init; }
    
    /// <summary>URL pattern with {username} placeholder</summary>
    public required string UrlPattern { get; init; }
    
    /// <summary>HTTP status code that indicates user exists (usually 200)</summary>
    public int ExpectedStatusCode { get; init; } = 200;
    
    /// <summary>If response body contains this text, user does NOT exist (null = don't check)</summary>
    public string? NotFoundText { get; init; }
    
    /// <summary>If true, check for redirect (redirect often means user not found)</summary>
    public bool CheckRedirect { get; init; } = false;
    
    /// <summary>Icon/emoji for display</summary>
    public string Icon { get; init; } = "[--]";
    
    /// <summary>Get the profile URL for a specific username</summary>
    public string GetProfileUrl(string username) => UrlPattern.Replace("{username}", username);
}
