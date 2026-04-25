using SherlockOsint.Shared.Models;
using SherlockOsint.Api.Services.OsintProviders;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Text.RegularExpressions;

namespace SherlockOsint.Api.Services;

public interface IRealSearchService
{
    IAsyncEnumerable<OsintNode> SearchAsync(SearchRequest request, CancellationToken cancellationToken = default);
}

public class RealSearchService : IRealSearchService
{
    private readonly GravatarLookup _gravatarLookup;
    private readonly GitHubSearch _githubSearch;
    private readonly PhoneValidator _phoneValidator;
    private readonly UsernameSearch _usernameSearch;
    private readonly HibpBreachCheck _hibpBreachCheck;
    private readonly EmailDiscovery _emailDiscovery;
    private readonly HunterIoLookup _hunterIoLookup;
    private readonly ClearbitLookup _clearbitLookup;
    private readonly FullContactLookup _fullContactLookup;
    private readonly YouTubeDiscovery _youtubeDiscovery;
    private readonly RedditDiscovery _redditDiscovery;
    private readonly StackOverflowDiscovery _stackOverflowDiscovery;
    private readonly GitLabSearch _gitlabSearch;
    private readonly WebSearchProvider _webSearchProvider;
    private readonly EmailRepCheck _emailRepCheck;
    private readonly NicknamePermutator _nicknamePermutator;
    private readonly HackerNewsLookup _hackerNewsLookup;
    private readonly DevToLookup _devToLookup;
    private readonly BlueskyLookup _blueskyLookup;
    private readonly LemmyLookup _lemmyLookup;
    private readonly MastodonLookup _mastodonLookup;
    private readonly WykopLookup _wykopLookup;
    private readonly FourChanArchiveLookup _fourChanArchiveLookup;
    private readonly TwitchLookup _twitchLookup;
    private readonly BilibiliLookup _bilibiliLookup;
    private readonly VkLookup _vkLookup;
    private readonly TelegramLookup _telegramLookup;
    private readonly HackerRankLookup _hackerRankLookup;
    private readonly FourProgrammersLookup _fourProgrammersLookup;
    private readonly NumverifyLookup _numverifyLookup;
    private readonly ILogger<RealSearchService> _logger;

    public RealSearchService(
        GravatarLookup gravatarLookup,
        GitHubSearch githubSearch,
        PhoneValidator phoneValidator,
        UsernameSearch usernameSearch,
        HibpBreachCheck hibpBreachCheck,
        EmailDiscovery emailDiscovery,
        HunterIoLookup hunterIoLookup,
        ClearbitLookup clearbitLookup,
        FullContactLookup fullContactLookup,
        YouTubeDiscovery youtubeDiscovery,
        RedditDiscovery redditDiscovery,
        StackOverflowDiscovery stackOverflowDiscovery,
        GitLabSearch gitlabSearch,
        WebSearchProvider webSearchProvider,
        EmailRepCheck emailRepCheck,
        NicknamePermutator nicknamePermutator,
        HackerNewsLookup hackerNewsLookup,
        DevToLookup devToLookup,
        BlueskyLookup blueskyLookup,
        LemmyLookup lemmyLookup,
        MastodonLookup mastodonLookup,
        WykopLookup wykopLookup,
        FourChanArchiveLookup fourChanArchiveLookup,
        TwitchLookup twitchLookup,
        BilibiliLookup bilibiliLookup,
        VkLookup vkLookup,
        TelegramLookup telegramLookup,
        HackerRankLookup hackerRankLookup,
        FourProgrammersLookup fourProgrammersLookup,
        NumverifyLookup numverifyLookup,
        ILogger<RealSearchService> logger)
    {
        _gravatarLookup = gravatarLookup;
        _githubSearch = githubSearch;
        _phoneValidator = phoneValidator;
        _usernameSearch = usernameSearch;
        _hibpBreachCheck = hibpBreachCheck;
        _emailDiscovery = emailDiscovery;
        _hunterIoLookup = hunterIoLookup;
        _clearbitLookup = clearbitLookup;
        _fullContactLookup = fullContactLookup;
        _youtubeDiscovery = youtubeDiscovery;
        _redditDiscovery = redditDiscovery;
        _stackOverflowDiscovery = stackOverflowDiscovery;
        _gitlabSearch = gitlabSearch;
        _webSearchProvider = webSearchProvider;
        _emailRepCheck = emailRepCheck;
        _nicknamePermutator = nicknamePermutator;
        _hackerNewsLookup = hackerNewsLookup;
        _devToLookup = devToLookup;
        _blueskyLookup = blueskyLookup;
        _lemmyLookup = lemmyLookup;
        _mastodonLookup = mastodonLookup;
        _wykopLookup = wykopLookup;
        _fourChanArchiveLookup = fourChanArchiveLookup;
        _twitchLookup = twitchLookup;
        _bilibiliLookup = bilibiliLookup;
        _vkLookup = vkLookup;
        _telegramLookup = telegramLookup;
        _hackerRankLookup = hackerRankLookup;
        _fourProgrammersLookup = fourProgrammersLookup;
        _numverifyLookup = numverifyLookup;
        _logger = logger;
    }

    public async IAsyncEnumerable<OsintNode> SearchAsync(
        SearchRequest request,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        _logger.LogInformation("Search started for Nickname={Nickname}, Email={Email}", request.Nickname, request.Email);

        // Yield first node IMMEDIATELY to confirm connection to client
        yield return new OsintNode { Label = "Discovery Started", Value = "Stage 1: Input Enrichment & Handle Extraction", Depth = 0 };

        var channel = Channel.CreateUnbounded<OsintNode>();
        var seenUrls = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        var discoveredHandles = new ConcurrentBag<string>();
        var discoveredEmails = new ConcurrentBag<string>();
        var processedEmails = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        var processedNicks = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrEmpty(request.Nickname)) discoveredHandles.Add(request.Nickname);
        if (!string.IsNullOrEmpty(request.Email)) discoveredEmails.Add(request.Email);

        // Seed handles from email local part and / or full name when no nickname
        // was supplied so Stage 2 has something to fan out on. Always seed the
        // permutator-derived candidates too, even if a nickname *was* supplied —
        // it's effectively free and catches first-initial / last-only patterns.
        // We deliberately do NOT pre-filter weak seeds (e.g. bare-first 'pawel',
        // bare-last 'murdzek'): the IsFromUserInput flag on TargetCandidate plus
        // the /sessions Claude memory pass let the ranker downrank speculative
        // permutations without us throwing away potentially valid handles upfront.
        if (!string.IsNullOrEmpty(request.Email))
        {
            foreach (var h in _nicknamePermutator.EmailToHandleCandidates(request.Email, request.FullName))
                discoveredHandles.Add(h);
        }
        if (!string.IsNullOrEmpty(request.FullName))
        {
            foreach (var h in _nicknamePermutator.FullNameToHandleCandidates(request.FullName))
                discoveredHandles.Add(h);
        }

        // Helper to deduplicate and stream
        void AddResult(OsintNode node)
        {
            if (string.IsNullOrEmpty(node.Value) || !node.Value.StartsWith("http"))
            {
                channel.Writer.TryWrite(node);
                return;
            }

            string normalized;
            try
            {
                var uri = new Uri(node.Value);
                var host = uri.Host.ToLowerInvariant();
                if (host.StartsWith("www.")) host = host[4..];
                // Treat x.com as twitter.com — but only when it really is the X host
                // (substring replace previously turned "roblox.com" into "roblotwitter.com").
                if (host == "x.com" || host.EndsWith(".x.com")) host = "twitter.com";
                normalized = (host + uri.PathAndQuery).TrimEnd('/').ToLowerInvariant();
            }
            catch
            {
                normalized = node.Value.TrimEnd('/').ToLowerInvariant()
                    .Replace("https://", "").Replace("http://", "").Replace("www.", "");
            }

            if (seenUrls.TryAdd(normalized, true))
            {
                channel.Writer.TryWrite(node);
            }
        }

        // Background worker using the token
        _ = Task.Run(async () =>
        {
            try
            {
                var enrichmentTasks = new List<Task>();
                if (!string.IsNullOrEmpty(request.Email))
                {
                    enrichmentTasks.Add(_hunterIoLookup.VerifyEmailAsync(request.Email!, ct).ContinueWith(t => 
                        { if (t.IsCompletedSuccessfully) foreach (var n in t.Result) AddResult(n); }));

                    enrichmentTasks.Add(_clearbitLookup.EnrichEmailAsync(request.Email!, ct).ContinueWith(t => 
                        { if (t.IsCompletedSuccessfully) foreach (var n in t.Result) { AddResult(n); foreach(var h in ExtractHandlesFromNode(n)) discoveredHandles.Add(h); } }));

                    enrichmentTasks.Add(_fullContactLookup.EnrichPersonAsync(request.Email!, ct).ContinueWith(t => 
                        { if (t.IsCompletedSuccessfully) foreach (var n in t.Result) { AddResult(n); foreach(var h in ExtractHandlesFromNode(n)) discoveredHandles.Add(h); } }));

                    enrichmentTasks.Add(_gravatarLookup.LookupAsync(request.Email!, ct).ContinueWith(t => 
                        { if (t.IsCompletedSuccessfully) foreach (var n in t.Result) { AddResult(n); if (n.Label != null && n.Label.Contains("Username")) discoveredHandles.Add(n.Value ?? ""); } }));
                }

                if (!string.IsNullOrEmpty(request.Phone))
                {
                    enrichmentTasks.Add(_phoneValidator.ValidateAsync(request.Phone!, ct).ContinueWith(t =>
                        { if (t.IsCompletedSuccessfully) foreach (var n in t.Result) AddResult(n); }));

                    enrichmentTasks.Add(_fullContactLookup.EnrichPhoneAsync(request.Phone!, ct).ContinueWith(t =>
                        { if (t.IsCompletedSuccessfully) foreach (var n in t.Result) { AddResult(n); foreach(var h in ExtractHandlesFromNode(n)) discoveredHandles.Add(h); } }));

                    enrichmentTasks.Add(_numverifyLookup.SearchAsync(request.Phone!, ct).ContinueWith(t =>
                        { if (t.IsCompletedSuccessfully) foreach (var n in t.Result) AddResult(n); }));
                }

                if (!string.IsNullOrEmpty(request.Email) || !string.IsNullOrEmpty(request.FullName))
                {
                    enrichmentTasks.Add(_githubSearch.SearchAsync(request.FullName ?? "", request.Email, ct).ContinueWith(t => 
                        { if (t.IsCompletedSuccessfully) foreach (var n in t.Result) { AddResult(n); var h = ExtractHandleFromUrl(n.Value); if (h != null) discoveredHandles.Add(h); foreach (var email in ExtractEmailsFromNode(n)) discoveredEmails.Add(email); } }));
                    
                    enrichmentTasks.Add(_gitlabSearch.SearchAsync(request.FullName ?? "", request.Email, ct).ContinueWith(t => 
                        { if (t.IsCompletedSuccessfully) foreach (var n in t.Result) { AddResult(n); var h = ExtractHandleFromUrl(n.Value); if (h != null) discoveredHandles.Add(h); foreach (var email in ExtractEmailsFromNode(n)) discoveredEmails.Add(email); } }));
                }

                var searchQuery = request.Nickname ?? request.FullName;
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    enrichmentTasks.Add(_webSearchProvider.SearchAsync(searchQuery, ct).ContinueWith(t => 
                        { if (t.IsCompletedSuccessfully) foreach (var n in t.Result) { AddResult(n); foreach (var h in ExtractHandlesFromNode(n)) discoveredHandles.Add(h); } }));
                }

                await Task.WhenAll(enrichmentTasks);

                var seeds = discoveredHandles.Where(h => !string.IsNullOrEmpty(h)).Distinct().ToList();
                AddResult(new OsintNode { Label = "Discovery Expanded", Value = $"Scanning {seeds.Count} potential handles...", Depth = 0 });

                var activeDiscoveryTasks = new ConcurrentBag<Task>();

                async Task RunSearchRoundAsync(string nick, int depth)
                {
                    if (depth > 1 || string.IsNullOrEmpty(nick) || ct.IsCancellationRequested) return;
                    if (!processedNicks.TryAdd(nick, true) && depth > 0) return;

                    var perHandleTasks = new List<Task>
                    {
                        Task.Run(async () =>
                        {
                            await foreach (var node in _usernameSearch.SearchAsync(nick, ct)) AddResult(node);
                        }, ct),
                        _hackerNewsLookup.SearchAsync(nick, ct).ContinueWith(t =>
                            { if (t.IsCompletedSuccessfully) foreach (var n in t.Result) { AddResult(n); foreach (var email in ExtractEmailsFromNode(n)) discoveredEmails.Add(email); } }, ct),
                        _devToLookup.SearchAsync(nick, ct).ContinueWith(t =>
                            { if (t.IsCompletedSuccessfully) foreach (var n in t.Result) { AddResult(n); foreach (var h in ExtractHandlesFromNode(n)) discoveredHandles.Add(h); foreach (var email in ExtractEmailsFromNode(n)) discoveredEmails.Add(email); } }, ct),
                        _blueskyLookup.SearchAsync(nick, ct).ContinueWith(t =>
                            { if (t.IsCompletedSuccessfully) foreach (var n in t.Result) AddResult(n); }, ct),
                        _lemmyLookup.SearchAsync(nick, ct).ContinueWith(t =>
                            { if (t.IsCompletedSuccessfully) foreach (var n in t.Result) AddResult(n); }, ct),
                        _mastodonLookup.SearchAsync(nick, ct).ContinueWith(t =>
                            { if (t.IsCompletedSuccessfully) foreach (var n in t.Result) AddResult(n); }, ct),
                        _wykopLookup.SearchAsync(nick, ct).ContinueWith(t =>
                            { if (t.IsCompletedSuccessfully) foreach (var n in t.Result) AddResult(n); }, ct),
                        _fourChanArchiveLookup.SearchAsync(nick, ct).ContinueWith(t =>
                            { if (t.IsCompletedSuccessfully) foreach (var n in t.Result) AddResult(n); }, ct),
                        _twitchLookup.SearchAsync(nick, ct).ContinueWith(t =>
                            { if (t.IsCompletedSuccessfully) foreach (var n in t.Result) AddResult(n); }, ct),
                        _bilibiliLookup.SearchAsync(nick, ct).ContinueWith(t =>
                            { if (t.IsCompletedSuccessfully) foreach (var n in t.Result) AddResult(n); }, ct),
                        _vkLookup.SearchAsync(nick, ct).ContinueWith(t =>
                            { if (t.IsCompletedSuccessfully) foreach (var n in t.Result) AddResult(n); }, ct),
                        _telegramLookup.SearchAsync(nick, ct).ContinueWith(t =>
                            { if (t.IsCompletedSuccessfully) foreach (var n in t.Result) AddResult(n); }, ct),
                        _hackerRankLookup.SearchAsync(nick, ct).ContinueWith(t =>
                            { if (t.IsCompletedSuccessfully) foreach (var n in t.Result) { AddResult(n); foreach (var h in ExtractHandlesFromNode(n)) discoveredHandles.Add(h); } }, ct),
                        _fourProgrammersLookup.SearchAsync(nick, ct).ContinueWith(t =>
                            { if (t.IsCompletedSuccessfully) foreach (var n in t.Result) AddResult(n); }, ct),
                        _stackOverflowDiscovery.SearchAsync(nick, ct).ContinueWith(t =>
                            { if (t.IsCompletedSuccessfully) foreach (var p in t.Result) AddResult(VerifiedProfileToNode(p)); }, ct),
                        _redditDiscovery.SearchAsync(nick, ct).ContinueWith(t =>
                            { if (t.IsCompletedSuccessfully) foreach (var p in t.Result) AddResult(VerifiedProfileToNode(p)); }, ct),
                        _youtubeDiscovery.SearchAsync(nick, ct).ContinueWith(t =>
                            { if (t.IsCompletedSuccessfully) foreach (var p in t.Result) AddResult(VerifiedProfileToNode(p)); }, ct)
                    };
                    await Task.WhenAll(perHandleTasks);
                }

                async Task RunEmailEnrichmentAsync(string email)
                {
                    if (string.IsNullOrEmpty(email) || !processedEmails.TryAdd(email, true) || ct.IsCancellationRequested) return;
                    
                    var emailTasks = new List<Task>
                    {
                        _hunterIoLookup.VerifyEmailAsync(email, ct).ContinueWith(t => { if (t.IsCompletedSuccessfully) foreach (var n in t.Result) AddResult(n); }),
                        _clearbitLookup.EnrichEmailAsync(email, ct).ContinueWith(t => { if (t.IsCompletedSuccessfully) foreach (var n in t.Result) AddResult(n); }),
                        _emailRepCheck.CheckEmailAsync(email, ct).ContinueWith(t => { if (t.IsCompletedSuccessfully) foreach (var n in t.Result) AddResult(n); })
                    };
                    await Task.WhenAll(emailTasks);
                }

                foreach (var email in discoveredEmails.Distinct()) activeDiscoveryTasks.Add(RunEmailEnrichmentAsync(email));

                foreach (var handle in seeds)
                {
                    bool isInitialNick = string.Equals(handle, request.Nickname, StringComparison.OrdinalIgnoreCase);
                    if (isInitialNick)
                    {
                        var variations = _nicknamePermutator.GeneratePermutations(handle, request.FullName);
                        foreach (var nick in variations) activeDiscoveryTasks.Add(RunSearchRoundAsync(nick, 0));
                    }
                    else activeDiscoveryTasks.Add(RunSearchRoundAsync(handle, 1));
                }

                await Task.WhenAll(activeDiscoveryTasks);
                AddResult(new OsintNode { Label = "Search Complete", Value = "Discovery analysis finished.", Depth = 0 });
            }
            catch (Exception ex) { _logger.LogError(ex, "Search worker failed"); }
            finally { channel.Writer.TryComplete(); }
        }, ct);

        await foreach (var node in channel.Reader.ReadAllAsync(ct)) yield return node;
    }

    private IEnumerable<string> ExtractHandlesFromNode(OsintNode node)
    {
        var handles = new List<string>();
        var h = ExtractHandleFromUrl(node.Value);
        if (h != null) handles.Add(h);

        foreach (var child in node.Children)
        {
            h = ExtractHandleFromUrl(child.Value);
            if (h != null) handles.Add(h);
            
            if (child.Label != null && (child.Label.Contains("Username") || child.Label.Contains("Handle") || child.Label.Contains("GitHub") || child.Label.Contains("Twitter") || child.Label.Contains("X")))
                handles.Add(child.Value ?? "");
        }
        return handles.Distinct();
    }

    private string? ExtractHandleFromUrl(string? url)
    {
        if (string.IsNullOrEmpty(url) || !url.StartsWith("http")) return null;

        string host;
        try
        {
            var uri = new Uri(url);
            host = uri.Host.ToLowerInvariant();
            if (host.StartsWith("www.")) host = host[4..];
        }
        catch { return null; }

        // Host-based — substring matched "x.com/" inside "roblox.com/" and "xbox.com/".
        var isHandleHost = host is "github.com" or "twitter.com" or "x.com" or "instagram.com"
            || (host == "linkedin.com" && url.Contains("/in/", StringComparison.OrdinalIgnoreCase));
        if (!isHandleHost) return null;

        var parts = url.TrimEnd('/').Split('/');
        if (parts.Length == 0) return null;
        var handle = parts.Last().TrimStart('@');
        if (handle.Contains("?")) handle = handle.Split('?')[0];
        return handle;
    }

    private OsintNode VerifiedProfileToNode(VerifiedProfile p)
    {
        var children = new List<OsintNode>();
        if (!string.IsNullOrEmpty(p.Username))
            children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Username", Value = p.Username, Depth = 2 });
        if (!string.IsNullOrEmpty(p.DisplayName))
            children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Display Name", Value = p.DisplayName, Depth = 2 });
        if (!string.IsNullOrEmpty(p.Bio))
            children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "📝 Bio", Value = p.Bio, Depth = 2 });
        if (!string.IsNullOrEmpty(p.Location))
            children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Location", Value = p.Location, Depth = 2 });
        foreach (var ev in p.Evidence)
            children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Evidence", Value = ev, Depth = 2 });

        return new OsintNode
        {
            Id = Guid.NewGuid().ToString(),
            Label = $"{p.Platform} User",
            Value = p.Url,
            Depth = 1,
            Children = children
        };
    }

    private IEnumerable<string> ExtractEmailsFromNode(OsintNode node)
    {
        var emails = new List<string>();
        var emailRegex = new Regex(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", RegexOptions.IgnoreCase);

        void ExtractRecursive(OsintNode n)
        {
            if (n.Value != null)
            {
                var matches = emailRegex.Matches(n.Value);
                foreach (Match m in matches) emails.Add(m.Value);
            }
            if (n.Children != null)
            {
                foreach (var child in n.Children) ExtractRecursive(child);
            }
        }

        ExtractRecursive(node);
        return emails.Distinct();
    }
}
