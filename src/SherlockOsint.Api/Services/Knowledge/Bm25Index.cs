using System.Text.RegularExpressions;

namespace SherlockOsint.Api.Services.Knowledge;

/// <summary>
/// In-memory BM25 index over markdown chunks. No external deps.
/// k1=1.5, b=0.75. Tokenizes on [A-Za-z0-9_]+, lowercased, no stop words
/// (regional / platform names matter and are often short).
/// </summary>
public class Bm25Index
{
    private const double K1 = 1.5;
    private const double B = 0.75;

    private static readonly Regex Tokenizer = new(@"[A-Za-z0-9_]+", RegexOptions.Compiled);

    private readonly List<MarkdownChunker.Chunk> _chunks = new();
    private readonly List<int> _docLengths = new();
    private readonly Dictionary<string, Dictionary<int, int>> _invertedIndex = new(StringComparer.Ordinal);
    private double _avgDocLength;

    public int DocumentCount => _chunks.Count;

    public void Add(MarkdownChunker.Chunk chunk)
    {
        var docId = _chunks.Count;
        _chunks.Add(chunk);

        var tokens = Tokenize(chunk.Text);
        _docLengths.Add(tokens.Count);

        foreach (var group in tokens.GroupBy(t => t))
        {
            if (!_invertedIndex.TryGetValue(group.Key, out var postings))
            {
                postings = new Dictionary<int, int>();
                _invertedIndex[group.Key] = postings;
            }
            postings[docId] = group.Count();
        }
    }

    public void Build()
    {
        _avgDocLength = _docLengths.Count > 0 ? _docLengths.Average() : 0;
    }

    public List<(MarkdownChunker.Chunk Chunk, double Score)> Search(string query, int topN)
    {
        if (_chunks.Count == 0 || string.IsNullOrWhiteSpace(query)) return new();

        var queryTokens = Tokenize(query);
        if (queryTokens.Count == 0) return new();

        var scores = new Dictionary<int, double>();
        var n = (double)_chunks.Count;

        foreach (var qt in queryTokens.Distinct())
        {
            if (!_invertedIndex.TryGetValue(qt, out var postings)) continue;
            var df = postings.Count;
            // BM25 IDF: ln((N - df + 0.5)/(df + 0.5) + 1)
            var idf = Math.Log((n - df + 0.5) / (df + 0.5) + 1);

            foreach (var (docId, tf) in postings)
            {
                var dl = _docLengths[docId];
                var norm = 1 - B + B * (dl / Math.Max(_avgDocLength, 1.0));
                var contribution = idf * (tf * (K1 + 1)) / (tf + K1 * norm);
                scores[docId] = scores.GetValueOrDefault(docId) + contribution;
            }
        }

        return scores
            .OrderByDescending(kv => kv.Value)
            .Take(topN)
            .Select(kv => (_chunks[kv.Key], kv.Value))
            .ToList();
    }

    private static List<string> Tokenize(string text)
    {
        var tokens = new List<string>();
        foreach (Match m in Tokenizer.Matches(text))
        {
            tokens.Add(m.Value.ToLowerInvariant());
        }
        return tokens;
    }
}
