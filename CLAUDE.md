# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Poirot OSINT** is a mobile OSINT (Open Source Intelligence) aggregation platform consisting of:
- **ASP.NET Core backend API** with 21 parallel data providers
- **.NET MAUI Android app** with real-time result streaming
- **Shared models library** used by both projects

## Commands

### Run the API locally
```bash
cd src/SherlockOsint.Api
dotnet run
# API starts at http://localhost:5000, SignalR hub at /osinthub
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
    → SearchOrchestrator (System.Threading.Channels queue)
      → RealSearchService (21 providers run concurrently as IAsyncEnumerable)
        → Each provider yields OsintNode
          → streamed immediately to client via ReceiveNode signal
      → After all nodes: ProfileAggregator + CandidateAggregator
        → ReceiveProfile + ReceiveCandidates signals
          → Mobile ResultsPage displays tree + identity candidates
```

### Key Services (API)

- **`OsintHub`** — SignalR hub; receives `StartSearch`/`CancelSearch`, emits `ReceiveNode`, `ReceiveProfile`, `ReceiveCandidates`, `SearchStarted`, `SearchCompleted`, `SearchError`
- **`SearchOrchestrator`** — `IHostedService` managing `Channel<SearchRequest>`; one `CancellationTokenSource` per connection ID
- **`RealSearchService`** — Spawns all 21 providers concurrently via `Task.WhenAll`; returns `IAsyncEnumerable<OsintNode>`; deduplicates discovered emails/usernames
- **`CandidateAggregator`** — Groups cross-platform signals, scores identity probability, builds `TargetCandidate` list
- **`ProfileAggregator`** — Collapses all nodes into a single `DigitalProfile` with confidence score

### OSINT Providers (`Services/OsintProviders/`)

21 providers registered as **singletons**. Each implements an async method returning `IAsyncEnumerable<OsintNode>`. Providers requiring API keys (Hunter.io, HIBP, Clearbit, FullContact) skip gracefully when keys are absent.

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

`OsintNode.Icon` is a computed property returning an emoji based on the node label — it is not set by providers.

## Configuration

**API keys** are optional and configured via environment variables or `appsettings.json`:
```
Osint__HunterApiKey
Osint__HibpApiKey
Osint__ClearbitApiKey
Osint__FullContactApiKey
```

**SignalR settings** (in `appsettings.json`): 30s server timeout, 15s keep-alive interval.

**CORS** is configured wide-open for mobile client compatibility — do not restrict without testing the SignalR connection.

## .NET Version

All projects target **net10.0** (Preview). Ensure the .NET 10 SDK is installed before building.
