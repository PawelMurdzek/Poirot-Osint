using System.Text.Json;
using System.Text.Json.Serialization;

namespace SherlockOsint.Tui.Configuration;

/// <summary>
/// Per-user TUI config stored in:
///   - Windows: %APPDATA%/Poirot/config.json
///   - Linux/macOS: ~/.config/poirot/config.json
/// Holds Claude API key and the default Anthropic model used for "ready-to-paste"
/// prompts (defaults to the latest Opus for max effort).
/// </summary>
public class UserConfig
{
    public string? ClaudeApiKey { get; set; }
    public string? ClaudeModel { get; set; }
    public string? ApiUrl { get; set; }

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string ConfigPath
    {
        get
        {
            var baseDir = OperatingSystem.IsWindows()
                ? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
            return Path.Combine(baseDir, "Poirot", "config.json");
        }
    }

    public static UserConfig Load()
    {
        try
        {
            if (!File.Exists(ConfigPath)) return new UserConfig();
            var json = File.ReadAllText(ConfigPath);
            return JsonSerializer.Deserialize<UserConfig>(json) ?? new UserConfig();
        }
        catch
        {
            return new UserConfig();
        }
    }

    public void Save()
    {
        var dir = Path.GetDirectoryName(ConfigPath);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(ConfigPath, JsonSerializer.Serialize(this, JsonOpts));
    }

    /// <summary>
    /// Three-tier resolution for the Claude API key:
    ///   1. CLI flag (passed in)
    ///   2. CLAUDE_API_KEY environment variable
    ///   3. config.json
    /// First non-empty wins.
    /// </summary>
    public static string? ResolveClaudeKey(string? cliFlag)
    {
        if (!string.IsNullOrWhiteSpace(cliFlag)) return cliFlag;
        var env = Environment.GetEnvironmentVariable("CLAUDE_API_KEY");
        if (!string.IsNullOrWhiteSpace(env)) return env;
        var cfg = Load();
        return cfg.ClaudeApiKey;
    }

    /// <summary>
    /// Three-tier resolution for the model:
    ///   1. CLI flag
    ///   2. CLAUDE_MODEL env var
    ///   3. config.json
    ///   4. Built-in default (latest Opus for max effort in the manual flow).
    /// </summary>
    public static string ResolveClaudeModel(string? cliFlag)
    {
        if (!string.IsNullOrWhiteSpace(cliFlag)) return cliFlag;
        var env = Environment.GetEnvironmentVariable("CLAUDE_MODEL");
        if (!string.IsNullOrWhiteSpace(env)) return env;
        var cfg = Load();
        return !string.IsNullOrWhiteSpace(cfg.ClaudeModel) ? cfg.ClaudeModel : "claude-opus-4-7";
    }

    public static string ResolveApiUrl(string? cliFlag)
    {
        if (!string.IsNullOrWhiteSpace(cliFlag)) return cliFlag;
        var env = Environment.GetEnvironmentVariable("POIROT_API_URL");
        if (!string.IsNullOrWhiteSpace(env)) return env;
        var cfg = Load();
        return !string.IsNullOrWhiteSpace(cfg.ApiUrl) ? cfg.ApiUrl : "http://localhost:57063";
    }
}
