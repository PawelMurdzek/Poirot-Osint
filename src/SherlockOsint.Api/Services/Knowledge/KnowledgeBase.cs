namespace SherlockOsint.Api.Services.Knowledge;

/// <summary>
/// Loads and indexes the OSINT/ markdown knowledge base once at startup.
/// Singleton + IHostedService — StartAsync builds the index synchronously before
/// the API begins serving requests, so the first search request is never blocked
/// on I/O.
/// </summary>
public class KnowledgeBase : IHostedService
{
    private readonly ILogger<KnowledgeBase> _logger;
    private readonly Bm25Index _index = new();
    private readonly Dictionary<string, string> _filesByRelativePath = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _osintRoot;

    public KnowledgeBase(IWebHostEnvironment env, ILogger<KnowledgeBase> logger)
    {
        _logger = logger;
        // OSINT/ lives at the repo root, two levels up from src/SherlockOsint.Api
        var contentRoot = env.ContentRootPath;
        var candidate = Path.GetFullPath(Path.Combine(contentRoot, "..", "..", "OSINT"));
        if (!Directory.Exists(candidate))
        {
            // Fallback: search a few levels up
            var dir = new DirectoryInfo(contentRoot);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "OSINT")))
                dir = dir.Parent;
            candidate = dir != null ? Path.Combine(dir.FullName, "OSINT") : candidate;
        }
        _osintRoot = candidate;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!Directory.Exists(_osintRoot))
        {
            _logger.LogWarning("Knowledge base not built — OSINT/ directory not found (looked at {Path})", _osintRoot);
            return Task.CompletedTask;
        }

        var files = Directory.GetFiles(_osintRoot, "*.md", SearchOption.AllDirectories);
        var totalChunks = 0;

        foreach (var file in files)
        {
            try
            {
                var content = File.ReadAllText(file);
                var relative = Path.GetRelativePath(Path.GetDirectoryName(_osintRoot)!, file).Replace('\\', '/');
                _filesByRelativePath[relative] = content;

                foreach (var chunk in MarkdownChunker.Split(relative, content))
                {
                    _index.Add(chunk);
                    totalChunks++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to index knowledge file {File}", file);
            }
        }

        _index.Build();

        _logger.LogInformation("Knowledge base ready: {Files} files, {Chunks} chunks indexed from {Root}",
            files.Length, totalChunks, _osintRoot);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public List<(MarkdownChunker.Chunk Chunk, double Score)> Search(string query, int topN = 5)
        => _index.Search(query, topN);

    /// <summary>
    /// Reads a full markdown file by its repo-relative path (e.g. "OSINT/Regional_RUNet.md").
    /// Returns null if the path is not in the indexed set (defensive — prevents path traversal).
    /// </summary>
    public string? ReadFullFile(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return null;
        var normalized = relativePath.Replace('\\', '/').TrimStart('/');
        return _filesByRelativePath.GetValueOrDefault(normalized);
    }

    public IEnumerable<string> ListIndexedFiles() => _filesByRelativePath.Keys;
}
