# Poirot OSINT

**Open Source Intelligence Mobile Application**

A comprehensive OSINT application for discovering digital footprints across 21+ online platforms. Built with .NET MAUI for Android and ASP.NET Core for the backend API.

## Features

- **21 OSINT Providers** - Search across GitHub, LinkedIn, Twitter, Instagram, Reddit, and more
- **Real-time Streaming** - Results appear instantly via SignalR WebSockets
- **Identity Correlation** - Aggregate profiles into suspected identities with probability scoring
- **Clickable Source Links** - Direct access to discovered profiles
- **Mobile-First** - Native Android app with modern UI

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Mobile Client                             │
│                  (.NET MAUI Android)                         │
└─────────────────────────┬───────────────────────────────────┘
                          │ SignalR WebSocket
┌─────────────────────────┴───────────────────────────────────┐
│                      API Server                              │
│                   (ASP.NET Core)                             │
│  ┌─────────────┐  ┌──────────────────┐  ┌────────────────┐  │
│  │  OsintHub   │──│ RealSearchService│──│ 21 Providers   │  │
│  │  (SignalR)  │  │   (Orchestrator) │  │ (GitHub, etc.) │  │
│  └─────────────┘  └──────────────────┘  └────────────────┘  │
│                            │                                 │
│                   ┌────────┴────────┐                       │
│                   │ CandidateAggregator                     │
│                   │ (Identity Scoring)                      │
│                   └─────────────────┘                       │
└─────────────────────────────────────────────────────────────┘
```

## Project Structure

```
prnet_project/
├── src/
│   ├── SherlockOsint.Api/          # Backend API
│   │   ├── Hubs/                   # SignalR hub
│   │   ├── Services/               # Business logic
│   │   │   └── OsintProviders/     # 21 data providers
│   │   └── Program.cs              # Entry point
│   │
│   ├── SherlockOsint.Mobile/       # Android Mobile App
│   │   ├── Views/                  # XAML pages
│   │   ├── ViewModels/             # MVVM view models
│   │   └── Services/               # SignalR client
│   │
│   └── SherlockOsint.Shared/       # Shared models
│
├── Dockerfile                      # Container config
├── docker-compose.yml              # Orchestration
├── FINAL_DOCUMENTATION.md          # Full documentation
└── DEPLOYMENT_GUIDE.md             # Deployment instructions
```

## Technology Stack

| Component | Technology |
|-----------|------------|
| Mobile Framework | .NET MAUI 10.0 |
| Backend | ASP.NET Core 10.0 |
| Real-time | SignalR |
| MVVM | CommunityToolkit.Mvvm |
| Containerization | Docker |

## OSINT Providers

| Provider | Data Source |
|----------|-------------|
| UsernameSearch | 30+ platforms |
| GitHubSearch | GitHub API |
| GitLabSearch | GitLab API |
| EmailRepCheck | EmailRep.io |
| HunterIoLookup | Hunter.io |
| GravatarLookup | Gravatar |
| HibpBreachCheck | Have I Been Pwned |
| RedditDiscovery | Reddit API |
| StackOverflow | Stack Overflow |
| YouTubeDiscovery | YouTube |
| And 11 more... | Various sources |

## Quick Start

### Run API Locally

```bash
cd src/SherlockOsint.Api
dotnet run
```

### Build Android APK

```bash
dotnet publish src/SherlockOsint.Mobile/SherlockOsint.Mobile.csproj -f net10.0-android -c Release
```

### Docker Deployment

```bash
docker-compose up -d
```

## Configuration

API keys are configured in `appsettings.json`:

```json
{
  "Osint": {
    "HunterApiKey": "",
    "HibpApiKey": "",
    "ClearbitApiKey": ""
  }
}
```

## Documentation

- [Final Documentation](FINAL_DOCUMENTATION.md) - Complete project documentation
- [Deployment Guide](DEPLOYMENT_GUIDE.md) - Cloud deployment instructions

## Author

**Piotr Szewczyk**  
Politechnika Warszawska  
February 2026

## License

This project is for educational purposes.
