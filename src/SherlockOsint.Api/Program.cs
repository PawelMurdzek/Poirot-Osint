using SherlockOsint.Api.Hubs;
using SherlockOsint.Api.Services;
using SherlockOsint.Api.Services.OsintProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddSignalR();

// Register HttpClientFactory for optimal HTTP performance
builder.Services.AddHttpClient("OsintClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
    client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml");
});

// Dedicated client for Claude API
builder.Services.AddHttpClient("Claude", client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
    client.DefaultRequestHeaders.Add("x-api-key", builder.Configuration["Osint:ClaudeApiKey"] ?? "");
    client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
});

// Register OSINT providers
builder.Services.AddSingleton<GravatarLookup>();
builder.Services.AddSingleton<GitHubSearch>();
builder.Services.AddSingleton<PhoneValidator>();
builder.Services.AddSingleton<UsernameSearch>();
builder.Services.AddSingleton<CountryDetector>();
// Phase 2 providers
builder.Services.AddSingleton<HibpBreachCheck>();
builder.Services.AddSingleton<EmailRepCheck>();
builder.Services.AddSingleton<WebSearchProvider>();
// Phase 3: Identity linking
builder.Services.AddSingleton<IdentityLinker>();
// Phase 4: Real discovery (no API keys)
builder.Services.AddSingleton<EmailDiscovery>();
builder.Services.AddSingleton<PgpKeyserverLookup>();
builder.Services.AddSingleton<DomainWhoisLookup>();
builder.Services.AddSingleton<GitLabSearch>();
// Phase 5: Premium APIs (require keys in appsettings.json)
builder.Services.AddSingleton<HunterIoLookup>();
builder.Services.AddSingleton<ClearbitLookup>();
builder.Services.AddSingleton<FullContactLookup>();
// Phase 6: Profile verification and advanced discovery
builder.Services.AddSingleton<ProfileVerifier>();
builder.Services.AddSingleton<YouTubeDiscovery>();
builder.Services.AddSingleton<RedditDiscovery>();
builder.Services.AddSingleton<StackOverflowDiscovery>();
builder.Services.AddSingleton<NicknamePermutator>();

// Register application services
builder.Services.AddSingleton<ProfileAggregator>();
builder.Services.AddSingleton<ClaudeAnalysisService>();
builder.Services.AddSingleton<CandidateAggregator>();
builder.Services.AddSingleton<IRealSearchService, RealSearchService>();
builder.Services.AddSingleton<ISearchOrchestrator, SearchOrchestrator>();
builder.Services.AddHostedService(sp => (SearchOrchestrator)sp.GetRequiredService<ISearchOrchestrator>());

// Configure CORS for mobile client
builder.Services.AddCors(options =>
{
    options.AddPolicy("MobileClient", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add health checks for K8s readiness
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseCors("MobileClient");

app.UseHealthChecks("/health");

app.MapHub<OsintHub>("/osinthub");

app.MapGet("/", () => "Poirot OSINT API is running. Connect to /osinthub for real-time updates.");

app.Run();
