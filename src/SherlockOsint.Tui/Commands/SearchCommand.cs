using Spectre.Console;
using SherlockOsint.Shared.Models;
using SherlockOsint.Tui.Configuration;
using SherlockOsint.Tui.Render;
using SherlockOsint.Tui.SignalR;

namespace SherlockOsint.Tui.Commands;

/// <summary>
/// `poirot search [--nick X] [--email Y] ...` — connects to the Poirot API,
/// streams a live result tree to the terminal, prints aggregated profile +
/// candidates + auto personality profiles when the search completes, and
/// finally renders a copy-paste-ready Claude prompt for max-effort manual
/// analysis on Opus.
/// </summary>
public static class SearchCommand
{
    public static async Task<int> RunAsync(string[] args, CancellationToken ct)
    {
        var parsed = Parse(args);
        if (parsed == null) return 1;

        var apiUrl = UserConfig.ResolveApiUrl(parsed.ApiUrl);
        var request = parsed.Request;

        if (!request.HasSearchCriteria)
        {
            AnsiConsole.MarkupLine("[red]error:[/] supply at least one of --nick, --email, --phone, --full-name");
            return 1;
        }

        AnsiConsole.MarkupLine($"[dim]Connecting to[/] [link]{Markup.Escape(apiUrl)}[/]…");

        await using var client = new PoirotClient(apiUrl);
        var renderer = new LiveTreeRenderer();
        var profile = (DigitalProfile?)null;
        var candidates = new List<TargetCandidate>();
        var personalityProfiles = new List<PersonalityProfile>();
        var completedSignal = new TaskCompletionSource();

        client.OnNode += n => renderer.AddNode(n);
        client.OnProfile += p => profile = p;
        client.OnCandidates += c => candidates = c ?? new();
        client.OnPersonalityProfile += p => personalityProfiles.Add(p);
        client.OnStarted += msg => renderer.SetStatus(msg);
        client.OnCompleted += msg =>
        {
            renderer.SetStatus("✔ search complete");
            completedSignal.TrySetResult();
        };
        client.OnError += msg =>
        {
            AnsiConsole.MarkupLine($"[red]API error:[/] {Markup.Escape(msg)}");
            completedSignal.TrySetResult();
        };

        try
        {
            await client.ConnectAsync(ct);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Could not connect to[/] {Markup.Escape(apiUrl)}: {Markup.Escape(ex.Message)}");
            AnsiConsole.MarkupLine("[dim]Tip: ensure the API is running (`dotnet run --project src/SherlockOsint.Api`).[/]");
            return 2;
        }

        await client.StartSearchAsync(request, ct);

        await AnsiConsole.Live(renderer.Build())
            .StartAsync(async ctx =>
            {
                while (!completedSignal.Task.IsCompleted && !ct.IsCancellationRequested)
                {
                    ctx.UpdateTarget(renderer.Build());
                    ctx.Refresh();
                    try { await Task.Delay(250, ct); }
                    catch (OperationCanceledException) { break; }
                }
                ctx.UpdateTarget(renderer.Build());
                ctx.Refresh();
            });

        // ── Post-search summary ────────────────────────────────────────────────
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold cyan]Aggregated profile[/]").LeftJustified());
        if (profile == null)
        {
            AnsiConsole.MarkupLine("[dim]no aggregated profile received[/]");
        }
        else
        {
            var t = new Table().HideHeaders().AddColumns("k", "v");
            t.AddRow("Name", Markup.Escape(profile.Name ?? "(unknown)"));
            t.AddRow("Email", Markup.Escape(profile.Email ?? ""));
            t.AddRow("Platforms", profile.Platforms.Count.ToString());
            t.AddRow("Confidence", $"{profile.ConfidenceScore}%");
            AnsiConsole.Write(t);
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule($"[bold cyan]Candidates ({candidates.Count})[/]").LeftJustified());
        if (candidates.Count == 0)
        {
            AnsiConsole.MarkupLine("[dim]no candidates[/]");
        }
        else
        {
            var ct1 = new Table().AddColumns("[bold]username[/]", "[bold]prob[/]", "[bold]platforms[/]", "[bold]location[/]");
            foreach (var c in candidates.Take(10))
            {
                ct1.AddRow(
                    Markup.Escape(c.PrimaryUsername),
                    $"{c.ProbabilityScore}%",
                    c.Sources.Count.ToString(),
                    Markup.Escape(c.ProbableLocation ?? string.Empty));
            }
            AnsiConsole.Write(ct1);
        }

        if (personalityProfiles.Count > 0)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule($"[bold cyan]Personality profiles (auto)[/]").LeftJustified());
            foreach (var p in personalityProfiles)
            {
                var panel = new Panel(BuildPersonalityPanelText(p))
                    .Header($"[bold yellow]{Markup.Escape(p.CandidateUsername)}[/]  [dim]confidence {p.Confidence}%[/]")
                    .Border(BoxBorder.Rounded);
                AnsiConsole.Write(panel);
            }
        }

        // ── Copy-paste-ready prompt for Claude Code (Opus) ─────────────────────
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold magenta]Ready-to-paste prompt for Claude Code (Opus)[/]").LeftJustified());

        var prompt = PromptBuilder.BuildPersonalityPrompt(request, profile, candidates, personalityProfiles);

        AnsiConsole.MarkupLine("[dim]Skopiuj poniższy blok i wklej do `claude` w nowym terminalu[/]");
        AnsiConsole.MarkupLine($"[dim](aktualny model do manualnej analizy:[/] [yellow]{Markup.Escape(UserConfig.ResolveClaudeModel(parsed.Model))}[/][dim])[/]");
        AnsiConsole.WriteLine();

        var promptPanel = new Panel(new Text(prompt))
            .Border(BoxBorder.Heavy)
            .BorderColor(Color.Magenta1);
        AnsiConsole.Write(promptPanel);

        if (!string.IsNullOrEmpty(parsed.OutJson))
        {
            try
            {
                File.WriteAllText(parsed.OutJson, System.Text.Json.JsonSerializer.Serialize(new
                {
                    request,
                    profile,
                    candidates,
                    personalityProfiles,
                    prompt
                }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                AnsiConsole.MarkupLine($"[green]✔[/] full result also written to [link]{Markup.Escape(parsed.OutJson)}[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]could not write {Markup.Escape(parsed.OutJson)}:[/] {Markup.Escape(ex.Message)}");
            }
        }

        return 0;
    }

    private static string BuildPersonalityPanelText(PersonalityProfile p)
    {
        var lines = new List<string>();
        lines.Add(p.Summary);
        if (p.BehavioralIndicators.Count > 0)
        {
            lines.Add("");
            lines.Add("[bold]Behavioral indicators[/]:");
            foreach (var ind in p.BehavioralIndicators) lines.Add($"  • {Markup.Escape(ind)}");
        }
        if (!string.IsNullOrEmpty(p.RegionalContext))
        {
            lines.Add("");
            lines.Add($"[bold]Region[/]: {Markup.Escape(p.RegionalContext)}");
        }
        if (p.SockPuppetRedFlags.Count > 0)
        {
            lines.Add("");
            lines.Add("[bold red]Sock-puppet red flags[/]:");
            foreach (var f in p.SockPuppetRedFlags) lines.Add($"  ⚠ {Markup.Escape(f)}");
        }
        return string.Join("\n", lines);
    }

    // ── Args parsing ────────────────────────────────────────────────────────────

    private record Parsed(SearchRequest Request, string? ApiUrl, string? Model, string? OutJson);

    private static Parsed? Parse(string[] args)
    {
        var req = new SearchRequest();
        string? apiUrl = null;
        string? model = null;
        string? outJson = null;

        for (int i = 0; i < args.Length; i++)
        {
            var a = args[i];
            string? Next()
            {
                if (i + 1 >= args.Length)
                {
                    AnsiConsole.MarkupLine($"[red]missing value for[/] {Markup.Escape(a)}");
                    return null;
                }
                return args[++i];
            }

            switch (a)
            {
                case "--nick":
                case "-n":
                    req.Nickname = Next(); if (req.Nickname == null) return null; break;
                case "--email":
                case "-e":
                    req.Email = Next(); if (req.Email == null) return null; break;
                case "--phone":
                case "-p":
                    req.Phone = Next(); if (req.Phone == null) return null; break;
                case "--full-name":
                case "-f":
                    req.FullName = Next(); if (req.FullName == null) return null; break;
                case "--api":
                case "--api-url":
                    apiUrl = Next(); if (apiUrl == null) return null; break;
                case "--model":
                case "-m":
                    model = Next(); if (model == null) return null; break;
                case "--out-json":
                case "-o":
                    outJson = Next(); if (outJson == null) return null; break;
                default:
                    AnsiConsole.MarkupLine($"[red]unknown flag:[/] {Markup.Escape(a)}");
                    return null;
            }
        }

        return new Parsed(req, apiUrl, model, outJson);
    }
}
