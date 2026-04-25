# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Poirot OSINT** is a mobile OSINT (Open Source Intelligence) aggregation platform consisting of:
- **ASP.NET Core backend API** with 21 data providers in `Services/OsintProviders/`
- **.NET MAUI Android app** with real-time result streaming
- **Shared models library** used by both projects

## Commands

### Run the API locally
```bash
cd src/SherlockOsint.Api
dotnet run
# Dev port is http://0.0.0.0:57063 (configured in Properties/launchSettings.json)
# Health check: GET /health  | Root: GET /  | SignalR hub: /osinthub
# The mobile client's SignalRService.ApiBaseUrl defaults to :57063 — keep them in sync
```

### Build Android APK (unsigned, for development)
```bash
cd src/SherlockOsint.Mobile
dotnet build -f net10.0-android -c Release
# Output: bin/Release/net10.0-android/com.sherlock.osint.apk
```

### Build signed APK (for release)
```powershell
dotnet publish -f net10.0-android -c Release `
  -p:AndroidKeyStore=true `
  -p:AndroidSigningKeyStore="..\..\poirot-osint.keystore" `
  -p:AndroidSigningKeyAlias=poirot `
  -p:AndroidSigningKeyPass=PASSWORD `
  -p:AndroidSigningStorePass=PASSWORD
```

### Docker deployment
```bash
docker-compose up -d --build
# API accessible at http://localhost:8080, health check at /health
```

### Build entire solution
```bash
dotnet build SherlockOsint.sln
```

There are no test projects in this repository.

## Architecture

### Project Structure

```
src/
├── SherlockOsint.Api/       # ASP.NET Core backend
├── SherlockOsint.Mobile/    # .NET MAUI Android app
└── SherlockOsint.Shared/    # Shared models (referenced by both)
```

### Real-Time Data Flow

```
Mobile SearchPage
  → SignalR WebSocket (OsintHub.StartSearch)
    → SearchOrchestrator (System.Threading.Channels queue, one CTS per connection)
      → RealSearchService (two-stage pipeline, see below)
        → Each yielded OsintNode → ReceiveNode signal (streamed live)
      → After streaming completes:
        → CandidateAggregator.BuildCandidatesAsync → ClaudeAnalysisService (LLM scoring)
        → ProfileAggregator.Aggregate
          → ReceiveCandidates + ReceiveProfile signals
            → Mobile ResultsPage displays tree + identity candidates
```

### RealSearchService pipeline (important)

`RealSearchService.SearchAsync` is **not** a simple `Task.WhenAll` over all providers. It is a two-stage producer/consumer using an unbounded `Channel<OsintNode>`:

1. **Stage 1 — input enrichment.** Based on which fields are populated in `SearchRequest` (email / phone / fullName), runs the relevant subset of: `HunterIoLookup`, `ClearbitLookup`, `FullContactLookup`, `GravatarLookup`, `PhoneValidator`, `GitHubSearch`, `GitLabSearch`, `WebSearchProvider`. Each provider's results are scanned for new handles/emails which feed `discoveredHandles` / `discoveredEmails` (`ConcurrentBag`).
2. **Stage 2 — expanded discovery.** For every distinct discovered handle, runs `UsernameSearch` (with `NicknamePermutator` variations applied to the original seed nickname). For every discovered email, runs another round of email enrichment (`HunterIoLookup`, `ClearbitLookup`, `EmailRepCheck`).

Deduplication is by **normalized URL** (strips scheme, `www.`, trailing `/`, normalizes `x.com` → `twitter.com`) in a single `ConcurrentDictionary`. Nodes whose `Value` is not a URL bypass the dedup. Cancellation is cooperative via the `CancellationToken` from `SearchOrchestrator`.

Several providers exist in `Services/OsintProviders/` (e.g. `DomainWhoisLookup`, `PgpKeyserverLookup`, `ProfileVerifier`, `WaybackMachineLookup`) that are registered in `Program.cs` but **not currently invoked** by `RealSearchService`.

### Key Services (API)

- **`OsintHub`** — SignalR hub; receives `StartSearch`/`CancelSearch`, emits `ReceiveNode`, `ReceiveProfile`, `ReceiveCandidates`, `SearchStarted`, `SearchCompleted`, `SearchError`
- **`SearchOrchestrator`** — `IHostedService` managing `Channel<SearchRequest>`; one `CancellationTokenSource` per connection ID
- **`RealSearchService`** — see pipeline above; returns `IAsyncEnumerable<OsintNode>` backed by an unbounded channel
- **`CandidateAggregator`** — `BuildCandidatesAsync` is **async**; groups platform nodes by normalized username, merges via `IdentityLinker`, then delegates probability scoring to `ClaudeAnalysisService`
- **`ClaudeAnalysisService`** — Calls Claude API (`claude-sonnet-4-6` via `https://api.anthropic.com/v1/messages`) to score every candidate. Builds a structured prompt listing the search query and each candidate's platforms/aliases/emails, then parses a JSON-array response into `CandidateAssessment` objects (probability, consistency analysis, uncertainty notes, professional role, activity summary, confidence interval). **Skips silently if `Osint:ClaudeApiKey` is not configured** — replaces the older hardcoded probability scoring
- **`ProfileAggregator`** — Collapses all nodes into a single `DigitalProfile` with confidence score
- **`NicknamePermutator`** — Generates handle variations from the seed nickname + full name (used in Stage 2)

### OSINT Providers (`Services/OsintProviders/`)

The `OsintProviders/` folder contains 21 `.cs` files, but they fall into three groups:
- **Active providers** registered as singletons in `Program.cs` and invoked by `RealSearchService` (most files).
- **Registered but unused** — `DomainWhoisLookup`, `PgpKeyserverLookup`, `ProfileVerifier` are in `Program.cs` but not currently wired into either pipeline stage.
- **Not registered at all** — `WaybackMachineLookup.cs` exists on disk but is missing from `Program.cs`.
- **Helper, not a provider** — `IdentityLinker` is a merge utility used by `CandidateAggregator`, not a data source.

Each active provider exposes an async method returning `IAsyncEnumerable<OsintNode>` or `Task<IEnumerable<OsintNode>>`. Providers requiring API keys (Hunter.io, HIBP, Clearbit, FullContact) skip gracefully when keys are absent. Adding a new provider requires registration in `Program.cs` **and** wiring into the appropriate stage of `RealSearchService`.

### Mobile MVVM Pattern

Uses **CommunityToolkit.Mvvm** with source generators:
- ViewModels use `[ObservableProperty]` and `[RelayCommand]` attributes
- `ISignalRService` is a singleton; ViewModels are transients
- `SignalRService.ApiBaseUrl` static property controls the backend URL (default: `http://192.168.1.129:57063`) — must be updated to match the backend host

### Shared Models (`SherlockOsint.Shared/Models/`)

| Model | Purpose |
|-------|---------|
| `SearchRequest` | Input: email, phone, nickname, fullName, connectionId |
| `OsintNode` | Single result tree node with label, value, children, type, metadata |
| `DigitalProfile` | Aggregated profile: name, email, photo, platforms, confidence score |
| `TargetCandidate` | Identity candidate with probability score and source evidence |
| `IdentitySignal` | Cross-platform signal used by `IdentityLinker` for merging candidates |

`OsintNode.Icon` is a computed property returning an emoji based on the node label — it is not set by providers.

## Configuration

**API keys** are optional and configured via environment variables or `appsettings.json` under the `Osint:` section:
```
Osint__ClaudeApiKey       # Drives candidate scoring; without it candidates have no AI assessment
Osint__HunterApiKey
Osint__HibpApiKey
Osint__ClearbitApiKey
Osint__FullContactApiKey
```

The Claude API client is registered as a named `HttpClient` (`"Claude"`) in `Program.cs` with the `x-api-key` and `anthropic-version: 2023-06-01` headers and a 60s timeout — the OSINT provider clients use a separate `"OsintClient"` named client with a 10s timeout.

**SignalR settings** (in `appsettings.json`): 30s server timeout, 15s keep-alive interval.

**CORS** is configured wide-open for mobile client compatibility — do not restrict without testing the SignalR connection.

## .NET Version

All projects target **net10.0** (Preview). Ensure the .NET 10 SDK is installed before building.

## Other documentation in this repo

Additional Markdown files exist at the repo root and may have useful context (treat as snapshots — verify against current code before relying on details):
- `README.md` — high-level overview, tech stack table, author info
- `DEPLOYMENT_GUIDE.md` — cloud/Docker deployment notes
- `FINAL_DOCUMENTATION.md`, `PROJECT_DOCUMENTATION.md` — older long-form docs
