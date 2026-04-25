using Spectre.Console;
using SherlockOsint.Shared.Models;

namespace SherlockOsint.Tui.Render;

/// <summary>
/// Live-redraws the result tree as nodes stream in. Spectre's Live display
/// rebuilds the tree on each Refresh() — the volume is small enough that this
/// is fine.
/// </summary>
public class LiveTreeRenderer
{
    private readonly object _lock = new();
    private readonly List<OsintNode> _nodes = new();
    private string _status = "Connecting…";

    public void AddNode(OsintNode node)
    {
        lock (_lock) _nodes.Add(node);
    }

    public void SetStatus(string status)
    {
        lock (_lock) _status = status;
    }

    public Tree Build()
    {
        lock (_lock)
        {
            var root = new Tree($"[bold cyan]Poirot OSINT[/]  [dim]{Markup.Escape(_status)}[/]");

            // Group children under their parent root nodes (Depth=0 are status / discovery markers)
            var current = root.AddNode(new Markup("[grey]waiting for results…[/]"));

            foreach (var node in _nodes)
            {
                if (node.Depth == 0)
                {
                    // Banner / discovery-started lines
                    current = root.AddNode($"[yellow]●[/] [bold]{Markup.Escape(node.Label)}[/]: {Markup.Escape(node.Value ?? "")}");
                }
                else
                {
                    var label = !string.IsNullOrEmpty(node.Label) ? node.Label : "(no label)";
                    var url = node.Value ?? "";
                    var line = $"[green]{node.Icon}[/] [bold]{Markup.Escape(label)}[/] [link]{Markup.Escape(url)}[/]";
                    var child = current.AddNode(line);

                    foreach (var sub in node.Children ?? new List<OsintNode>())
                    {
                        var subLabel = sub.Label ?? "";
                        var subValue = sub.Value ?? "";
                        child.AddNode($"[dim]{Markup.Escape(subLabel)}:[/] {Markup.Escape(subValue)}");
                    }
                }
            }

            return root;
        }
    }
}
