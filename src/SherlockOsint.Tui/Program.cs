using Spectre.Console;
using SherlockOsint.Tui.Commands;

// Top-level entry. Subcommands:
//   poirot search [flags]   — run an OSINT search and stream results live
//   poirot config <action>  — manage local config (Claude key / model / API URL)
//   poirot help             — show this help

if (args.Length == 0 || args[0] is "-h" or "--help" or "help")
{
    PrintHelp();
    return 0;
}

var subcommand = args[0];
var rest = args.Skip(1).ToArray();

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

try
{
    return subcommand switch
    {
        "search" => await SearchCommand.RunAsync(rest, cts.Token),
        "config" => ConfigCommand.Run(rest),
        _ => UnknownSubcommand(subcommand)
    };
}
catch (OperationCanceledException)
{
    AnsiConsole.MarkupLine("[yellow]cancelled.[/]");
    return 130;
}

static void PrintHelp()
{
    AnsiConsole.MarkupLine("[bold cyan]poirot[/] — Poirot OSINT terminal client");
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("[bold]usage:[/]");
    AnsiConsole.MarkupLine("  poirot [cyan]search[/] [[flags]]");
    AnsiConsole.MarkupLine("  poirot [cyan]config[/] <show | path | set-claude-key | set-model | set-api-url>");
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("[bold]search flags:[/]");
    AnsiConsole.MarkupLine("  -n, --nick      <nickname>");
    AnsiConsole.MarkupLine("  -e, --email     <email>");
    AnsiConsole.MarkupLine("  -p, --phone     <phone>");
    AnsiConsole.MarkupLine("  -f, --full-name <name>");
    AnsiConsole.MarkupLine("      --api       <url>          override API URL (default http://localhost:57063)");
    AnsiConsole.MarkupLine("  -m, --model     <model-id>     hint for the copy-paste prompt label (e.g. claude-opus-4-7)");
    AnsiConsole.MarkupLine("  -o, --out-json  <path>         also write the full payload as JSON");
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("[bold]examples:[/]");
    AnsiConsole.MarkupLine("  [dim]# Run a search by nickname[/]");
    AnsiConsole.MarkupLine("  poirot search --nick targetUser");
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("  [dim]# Save Claude API key (used only for the prompt-builder UI hint)[/]");
    AnsiConsole.MarkupLine("  poirot config set-claude-key sk-ant-api03-…");
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("  [dim]# Switch model used in the manual / copy-paste flow[/]");
    AnsiConsole.MarkupLine("  poirot config set-model claude-opus-4-7");
}

static int UnknownSubcommand(string name)
{
    AnsiConsole.MarkupLine($"[red]unknown subcommand:[/] {Markup.Escape(name)}");
    AnsiConsole.MarkupLine("[dim]run `poirot --help` for usage[/]");
    return 1;
}
