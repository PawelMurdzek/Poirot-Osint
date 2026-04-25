# Poirot OSINT

Mobile OSINT (Open Source Intelligence) aggregation platform. Given an email, phone, full name, or nickname, it queries 30+ public data sources concurrently, streams findings to the mobile client in real time, and aggregates them into ranked identity candidates.

- **Backend**: ASP.NET Core (`src/SherlockOsint.Api`) with SignalR for real-time streaming
- **Mobile**: .NET MAUI Android app (`src/SherlockOsint.Mobile`)
- **Shared**: Cross-project models (`src/SherlockOsint.Shared`)

All projects target **net10.0** (preview SDK required).

---

## Architecture

```
Mobile (MAUI)  ──SignalR──▶  OsintHub  ──▶  SearchOrchestrator
                                                │
                                                ▼
                                       RealSearchService
                                       │  Stage 1: input enrichment
                                       │  Stage 2: handle fan-out
                                       ▼
                                 30+ OsintProviders
                                       │
                                       ▼
                            ┌──────────┴──────────┐
                            ▼                     ▼
                   ProfileAggregator     CandidateAggregator
                                                  │
                                                  ▼
                                       ClaudeAnalysisService (optional)
                                                  │
                                                  ▼
                                       SessionMemoryService
                                          → /sessions/*.{json,md}
```

`RealSearchService` is a producer/consumer pipeline backed by `Channel<OsintNode>`: every provider's results are deduplicated by normalised URL and streamed live, while discovered handles and emails seed a second round of fan-out. After the stream completes, candidates are built, optionally scored by Claude (if `Osint:ClaudeApiKey` is set), and persisted to `/sessions` so a local Claude CLI can rank/rerank them later.

For deeper internals, see [`CLAUDE.md`](CLAUDE.md).

---

## Quick start

### Run the API
```bash
cd src/SherlockOsint.Api
dotnet run
# Listens on http://0.0.0.0:57063
# Health: GET /health   |   SignalR hub: /osinthub
```

### Run via Docker
```bash
docker-compose up -d --build
# API at http://localhost:8080
```

### Build the Android app (unsigned)
```bash
cd src/SherlockOsint.Mobile
dotnet build -f net10.0-android -c Release
# APK: bin/Release/net10.0-android/com.sherlock.osint.apk
```

For a signed release APK and cloud deployment notes, see [`DEPLOYMENT_GUIDE.md`](DEPLOYMENT_GUIDE.md).

The mobile client expects the backend at the URL set in `SignalRService.ApiBaseUrl` — update it to match the host running the API.

---

## OSINT providers

Active providers live in `src/SherlockOsint.Api/Services/OsintProviders/`. They split into three categories:

- **Input enrichment** (Stage 1): `HunterIoLookup`, `ClearbitLookup`, `FullContactLookup`, `GravatarLookup`, `PhoneValidator`, `NumverifyLookup`, `GitHubSearch`, `GitLabSearch`, `WebSearchProvider`
- **Username fan-out** (Stage 2): `UsernameSearch` (50+ platforms baked in), plus dedicated providers for `Bluesky`, `Lemmy`, `Mastodon`, `Wykop`, `FourChanArchive`, `Twitch`, `Bilibili`, `VK`, `Telegram`, `HackerRank`, `FourProgrammers`, `HackerNews`, `DevTo`, `StackOverflow`, `Reddit`, `YouTube`
- **Email enrichment loop**: `EmailRepCheck`, `HibpBreachCheck` (+ Stage 1 services re-run on discovered emails)

Helpers and not-yet-wired providers also live in this folder — `IdentityLinker`, `CountryDetector`, `ProfileVerifier`, `DomainWhoisLookup`, `PgpKeyserverLookup`, `WaybackMachineLookup`. See [`CLAUDE.md`](CLAUDE.md) for which ones are currently invoked by the pipeline.

For an exhaustive cross-reference against [Sherlock](https://github.com/sherlock-project/sherlock)'s site catalog (478 entries), see [`sherlock_not_poirot.md`](sherlock_not_poirot.md).

---

## Configuration

API keys are optional. Set them as environment variables (preferred) or in `appsettings.json` under the `Osint:` section:

| Key | Effect |
|-----|--------|
| `Osint__ClaudeApiKey` | Enables candidate scoring + personality profiler. Without it, the API persists candidates to `/sessions` and surfaces a `claude -p "..."` CLI command for local ranking |
| `Osint__ClaudeModel` | Overrides the Claude model (default `claude-sonnet-4-6`) |
| `Osint__SessionsPath` | Overrides the `/sessions` output folder location |
| `Osint__HunterApiKey` | Hunter.io email verification |
| `Osint__HibpApiKey` | Have I Been Pwned breach lookup |
| `Osint__ClearbitApiKey` | Clearbit person enrichment |
| `Osint__FullContactApiKey` | FullContact person enrichment |
| `Osint__TwitchClientId` / `Osint__TwitchClientSecret` | Twitch user lookup |
| `Osint__VkAccessToken` | VK user lookup |
| `Osint__TelegramBotToken` | Telegram username probe |
| `Osint__NumverifyApiKey` | Numverify phone reverse-lookup |

Providers gracefully skip when their key is absent — they don't fail the search.

---

## Real-time signals

The mobile client subscribes to these SignalR events on `/osinthub`:

| Signal | Payload |
|--------|---------|
| `ReceiveNode` | An `OsintNode` streamed live as providers find data |
| `ReceiveProfile` | The aggregated `DigitalProfile` once streaming completes |
| `ReceiveCandidates` | List of `TargetCandidate` ranked by probability |
| `ReceiveSessionMemory` | `{ folderPath, jsonPath, markdownPath, claudeCommand, claudeApiConfigured }` — points at the `/sessions` snapshot for this search |
| `ReceivePersonalityProfile` | Per-candidate deep profile from `PersonalityProfilerService` (only when Claude API is configured; top-3 candidates) |
| `SearchStarted` / `SearchCompleted` / `SearchCancelled` / `SearchError` | Lifecycle |

---

## Repository docs

- [`CLAUDE.md`](CLAUDE.md) — Deep dive into the pipeline, services, and conventions for AI agents working in the codebase
- [`DEPLOYMENT_GUIDE.md`](DEPLOYMENT_GUIDE.md) — Cloud / Docker deployment
- [`sherlock_not_poirot.md`](sherlock_not_poirot.md) — Full Sherlock site catalog with Poirot coverage cross-reference
- [`USER_TODO.md`](USER_TODO.md) — Open quality issues with the candidate pipeline

---

## Authors

**Piotr Szewczyk** & **Paweł Murdzek** — February 2026

For educational use.
