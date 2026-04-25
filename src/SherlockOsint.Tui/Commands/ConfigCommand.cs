using Spectre.Console;
using SherlockOsint.Tui.Configuration;

namespace SherlockOsint.Tui.Commands;

/// <summary>
/// `poirot config ...` subcommands — manage the per-user config file
/// at ~/.config/poirot/config.json or %APPDATA%/Poirot/config.json.
/// </summary>
public static class ConfigCommand
{
    public static int Run(string[] args)
    {
        if (args.Length == 0)
        {
            ShowHelp();
            return 0;
        }

        return args[0] switch
        {
            "show" => ShowCurrent(),
            "set-claude-key" => SetClaudeKey(args.Skip(1).ToArray()),
            "set-model" => SetModel(args.Skip(1).ToArray()),
            "set-api-url" => SetApiUrl(args.Skip(1).ToArray()),
            "path" => ShowPath(),
            _ => UnknownSubcommand(args[0])
        };
    }

    private static int ShowHelp()
    {
        AnsiConsole.MarkupLine("[bold]poirot config[/] — manage local user config");
        AnsiConsole.MarkupLine("");
        AnsiConsole.MarkupLine("  [cyan]show[/]                   print resolved config");
        AnsiConsole.MarkupLine("  [cyan]path[/]                   print path to config file");
        AnsiConsole.MarkupLine("  [cyan]set-claude-key[/] <KEY>   save Claude API key (overrides env var)");
        AnsiConsole.MarkupLine("  [cyan]set-model[/] <MODEL>      e.g. claude-opus-4-7 or claude-sonnet-4-6");
        AnsiConsole.MarkupLine("  [cyan]set-api-url[/] <URL>      e.g. http://localhost:57063");
        return 0;
    }

    private static int ShowCurrent()
    {
        var cfg = UserConfig.Load();
        var table = new Table().AddColumns("[bold]key[/]", "[bold]value[/]", "[bold]source[/]");

        // Claude API key — show masked
        var resolvedKey = UserConfig.ResolveClaudeKey(null);
        var keySource = ResolveSourceLabel(
            cli: null,
            env: Environment.GetEnvironmentVariable("CLAUDE_API_KEY"),
            cfg: cfg.ClaudeApiKey);
        table.AddRow("ClaudeApiKey", Mask(resolvedKey), keySource);

        var resolvedModel = UserConfig.ResolveClaudeModel(null);
        var modelSource = ResolveSourceLabel(
            cli: null,
            env: Environment.GetEnvironmentVariable("CLAUDE_MODEL"),
            cfg: cfg.ClaudeModel);
        table.AddRow("ClaudeModel", resolvedModel, modelSource);

        var resolvedApi = UserConfig.ResolveApiUrl(null);
        var apiSource = ResolveSourceLabel(
            cli: null,
            env: Environment.GetEnvironmentVariable("POIROT_API_URL"),
            cfg: cfg.ApiUrl);
        table.AddRow("ApiUrl", resolvedApi, apiSource);

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"[dim]config file:[/] {Markup.Escape(UserConfig.ConfigPath)}");
        return 0;
    }

    private static string ResolveSourceLabel(string? cli, string? env, string? cfg)
    {
        if (!string.IsNullOrWhiteSpace(cli)) return "cli";
        if (!string.IsNullOrWhiteSpace(env)) return "env";
        if (!string.IsNullOrWhiteSpace(cfg)) return "config.json";
        return "default";
    }

    private static string Mask(string? secret)
    {
        if (string.IsNullOrEmpty(secret)) return "[red](unset)[/]";
        if (secret.Length < 8) return "***";
        return $"{secret[..4]}…{secret[^4..]}";
    }

    private static int SetClaudeKey(string[] args)
    {
        if (args.Length == 0)
        {
            AnsiConsole.MarkupLine("[red]usage:[/] poirot config set-claude-key <KEY>");
            return 1;
        }
        var cfg = UserConfig.Load();
        cfg.ClaudeApiKey = args[0];
        cfg.Save();
        AnsiConsole.MarkupLine("[green]✔[/] Claude API key saved.");
        AnsiConsole.MarkupLine($"[dim]written to:[/] {Markup.Escape(UserConfig.ConfigPath)}");
        return 0;
    }

    private static int SetModel(string[] args)
    {
        if (args.Length == 0)
        {
            AnsiConsole.MarkupLine("[red]usage:[/] poirot config set-model <MODEL>");
            return 1;
        }
        var cfg = UserConfig.Load();
        cfg.ClaudeModel = args[0];
        cfg.Save();
        AnsiConsole.MarkupLine($"[green]✔[/] model set to {args[0]}");
        return 0;
    }

    private static int SetApiUrl(string[] args)
    {
        if (args.Length == 0)
        {
            AnsiConsole.MarkupLine("[red]usage:[/] poirot config set-api-url <URL>");
            return 1;
        }
        var cfg = UserConfig.Load();
        cfg.ApiUrl = args[0];
        cfg.Save();
        AnsiConsole.MarkupLine($"[green]✔[/] api URL set to {args[0]}");
        return 0;
    }

    private static int ShowPath()
    {
        AnsiConsole.WriteLine(UserConfig.ConfigPath);
        return 0;
    }

    private static int UnknownSubcommand(string name)
    {
        AnsiConsole.MarkupLine($"[red]unknown subcommand:[/] {Markup.Escape(name)}");
        return 1;
    }
}
