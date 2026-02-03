namespace SherlockOsint.Api.Services;

/// <summary>
/// Database of platforms to check for username existence.
/// Each platform is configured with proper detection to avoid false positives.
/// </summary>
public static class PlatformDatabase
{
    public static IReadOnlyList<PlatformDefinition> Platforms { get; } = new List<PlatformDefinition>
    {
        // Development - these return 404 for non-existent users (reliable)
        new() { Name = "GitHub", Category = "Development", UrlPattern = "https://github.com/{username}", ExpectedStatusCode = 200 },
        new() { Name = "GitLab", Category = "Development", UrlPattern = "https://gitlab.com/{username}", ExpectedStatusCode = 200 },
        new() { Name = "npm", Category = "Development", UrlPattern = "https://www.npmjs.com/~{username}", ExpectedStatusCode = 200 },
        new() { Name = "PyPI", Category = "Development", UrlPattern = "https://pypi.org/user/{username}/", ExpectedStatusCode = 200 },
        new() { Name = "Replit", Category = "Development", UrlPattern = "https://replit.com/@{username}", ExpectedStatusCode = 200 },
        
        // Social Media - need body checks as many return 200 always
        new() { Name = "Reddit", Category = "Social Media", UrlPattern = "https://www.reddit.com/user/{username}", ExpectedStatusCode = 200, NotFoundText = "Sorry, nobody on Reddit goes by that name" },
        new() { Name = "Medium", Category = "Content", UrlPattern = "https://medium.com/@{username}", ExpectedStatusCode = 200 },
        
        // Gaming - Steam returns 404 for non-existent custom URLs
        new() { Name = "Steam", Category = "Gaming", UrlPattern = "https://steamcommunity.com/id/{username}", ExpectedStatusCode = 200 },
        new() { Name = "Twitch", Category = "Gaming", UrlPattern = "https://www.twitch.tv/{username}", ExpectedStatusCode = 200 },
        
        // Creative - reliable 404 detection
        new() { Name = "SoundCloud", Category = "Content", UrlPattern = "https://soundcloud.com/{username}", ExpectedStatusCode = 200 },
        new() { Name = "Behance", Category = "Content", UrlPattern = "https://www.behance.net/{username}", ExpectedStatusCode = 200 },
        new() { Name = "Dribbble", Category = "Content", UrlPattern = "https://dribbble.com/{username}", ExpectedStatusCode = 200 },
        new() { Name = "DeviantArt", Category = "Content", UrlPattern = "https://www.deviantart.com/{username}", ExpectedStatusCode = 200, NotFoundText = "page isn't available" },
        
        // Other platforms with reliable detection
        new() { Name = "Gravatar", Category = "Other", UrlPattern = "https://en.gravatar.com/{username}", ExpectedStatusCode = 200 },
        new() { Name = "About.me", Category = "Other", UrlPattern = "https://about.me/{username}", ExpectedStatusCode = 200 },
        new() { Name = "Keybase", Category = "Other", UrlPattern = "https://keybase.io/{username}", ExpectedStatusCode = 200 },
        new() { Name = "Patreon", Category = "Other", UrlPattern = "https://www.patreon.com/{username}", ExpectedStatusCode = 200, NotFoundText = "This page is no longer available" },
        new() { Name = "Ko-fi", Category = "Other", UrlPattern = "https://ko-fi.com/{username}", ExpectedStatusCode = 200 },
    };
}

