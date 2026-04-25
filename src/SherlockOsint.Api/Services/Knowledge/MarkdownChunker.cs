using System.Text.RegularExpressions;

namespace SherlockOsint.Api.Services.Knowledge;

/// <summary>
/// Splits a markdown document by H2/H3 headings into chunks.
/// Each chunk preserves the heading as its anchor and the body underneath as its text.
/// Chunks larger than 4KB are trimmed at sentence boundaries.
/// </summary>
public static class MarkdownChunker
{
    private const int MaxChunkBytes = 4096;
    private static readonly Regex HeadingRegex = new(@"^(#{2,3})\s+(.+?)\s*$", RegexOptions.Multiline | RegexOptions.Compiled);

    public record Chunk(string FilePath, string Anchor, string Text);

    public static List<Chunk> Split(string filePath, string content)
    {
        var chunks = new List<Chunk>();
        if (string.IsNullOrWhiteSpace(content)) return chunks;

        var matches = HeadingRegex.Matches(content);
        if (matches.Count == 0)
        {
            // No headings — treat the whole file as one chunk under the filename
            chunks.Add(new Chunk(filePath, Path.GetFileNameWithoutExtension(filePath), Trim(content)));
            return chunks;
        }

        // Preamble before the first heading
        if (matches[0].Index > 0)
        {
            var preamble = content[..matches[0].Index].Trim();
            if (preamble.Length > 0)
                chunks.Add(new Chunk(filePath, "(preamble)", Trim(preamble)));
        }

        for (int i = 0; i < matches.Count; i++)
        {
            var current = matches[i];
            var anchor = current.Groups[2].Value.Trim();
            var bodyStart = current.Index + current.Length;
            var bodyEnd = i + 1 < matches.Count ? matches[i + 1].Index : content.Length;
            var body = content[bodyStart..bodyEnd].Trim();
            if (body.Length == 0) continue;
            chunks.Add(new Chunk(filePath, anchor, Trim(body)));
        }

        return chunks;
    }

    private static string Trim(string s)
    {
        if (System.Text.Encoding.UTF8.GetByteCount(s) <= MaxChunkBytes) return s;
        // Cut at the closest sentence boundary before the byte limit
        var bytes = System.Text.Encoding.UTF8.GetBytes(s);
        var sliced = System.Text.Encoding.UTF8.GetString(bytes, 0, MaxChunkBytes);
        var lastDot = sliced.LastIndexOfAny(new[] { '.', '!', '?', '\n' });
        return lastDot > 0 ? sliced[..(lastDot + 1)] + " […]" : sliced + " […]";
    }
}
