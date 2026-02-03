# Poirot OSINT

## Open Source Intelligence Mobile Application

---

**Project Documentation**

---

**Author:** Piotr Szewczyk  
**Student ID:** 01159112  
**Institution:** Politechnika Warszawska  
**Date:** February 2026

---

## Table of Contents

1. [Introduction](#1-introduction)
2. [System Architecture](#2-system-architecture)
3. [Technology Stack](#3-technology-stack)
4. [Solution Structure](#4-solution-structure)
5. [OSINT Data Providers](#5-osint-data-providers)
6. [Key Features](#6-key-features)
7. [Data Flow](#7-data-flow)
8. [Mobile Application](#8-mobile-application)
9. [Backend API](#9-backend-api)
10. [Deployment](#10-deployment)
11. [Testing](#11-testing)
12. [Conclusions](#12-conclusions)
13. [References](#13-references)

---

## 1. Introduction

### 1.1 Project Overview

**Poirot OSINT** is a comprehensive Open Source Intelligence (OSINT) application designed to search for digital footprints of individuals across multiple online platforms. The system consists of two main components:

- **Mobile Application** - Built with .NET MAUI for Android devices
- **Backend API** - Built with ASP.NET Core providing real-time data streaming

### 1.2 Project Objectives

The main objectives of this project are:

1. Aggregate publicly available information from 21+ online platforms
2. Correlate data to identify identity candidates with probability scoring
3. Provide real-time streaming of results to mobile devices
4. Enable clickable source links for direct profile access

### 1.3 Search Capabilities

Users can search using the following identifiers:

- Email address
- Nickname/username
- Full name
- Phone number

---

## 2. System Architecture

### 2.1 High-Level Architecture

The system follows a client-server architecture with real-time communication via SignalR WebSockets.

**Figure 2.1: System Architecture Diagram**

```
+------------------------------------------------------------------+
|                      MOBILE CLIENT                               |
|                    (.NET MAUI Android)                           |
|                                                                  |
|  +-------------+    +--------------+    +----------------+       |
|  | SearchPage  |--->| ResultsPage  |--->| SignalRService |       |
|  |   (Input)   |    |  (Display)   |    | (Real-time)    |       |
|  +-------------+    +--------------+    +-------+--------+       |
+-----------------------------------------------------+------------+
                                                      | WebSocket
                                                      | Connection
+-----------------------------------------------------+------------+
|                       API SERVER                    |            |
|                   (ASP.NET Core)                    v            |
|                                                                  |
|  +------------------------------------------------------+       |
|  |                    OsintHub (SignalR)                |       |
|  +------------------------+-----------------------------+       |
|                           |                                      |
|  +------------------------v-----------------------------+       |
|  |              RealSearchService (Orchestrator)         |       |
|  |              System.Threading.Channels                |       |
|  +------------------------+------------------------------+       |
|                           |                                      |
|  +------------------------v------------------------------+       |
|  |                 21 OSINT PROVIDERS                    |       |
|  |  GitHub | GitLab | Twitter | Instagram | LinkedIn    |       |
|  +------------------------+------------------------------+       |
|                           |                                      |
|  +------------------------v------------------------------+       |
|  |           CandidateAggregator (Correlation)           |       |
|  |              Identity linking and scoring             |       |
|  +-------------------------------------------------------+       |
+------------------------------------------------------------------+
```

### 2.2 Communication Protocol

The system uses **SignalR** for bidirectional real-time communication between the mobile client and the backend API. SignalR automatically negotiates the best transport protocol (WebSocket, Server-Sent Events, or Long Polling).

### 2.3 Data Processing Pipeline

1. **Input Validation** - User input is validated on the mobile client
2. **Request Transmission** - SearchRequest is sent via SignalR to the API
3. **Parallel Processing** - 21 OSINT providers execute concurrently
4. **Streaming Results** - Results are streamed back as they arrive
5. **Aggregation** - CandidateAggregator correlates identities
6. **Display** - Mobile app displays results in real-time

---

## 3. Technology Stack

### 3.1 Technologies Used

**Table 3.1: Technology Stack**

| Layer | Technology | Version | Purpose |
|-------|------------|---------|---------|
| Mobile Framework | .NET MAUI | 10.0 | Cross-platform mobile UI |
| Backend Framework | ASP.NET Core | 10.0 | REST API and SignalR hub |
| Real-time Communication | SignalR | - | WebSocket streaming |
| MVVM Toolkit | CommunityToolkit.Mvvm | - | Reactive data bindings |
| HTTP Client | IHttpClientFactory | - | Optimized HTTP requests |
| Async Streaming | System.Threading.Channels | - | Non-blocking data flow |
| Containerization | Docker | - | Cloud deployment |
| Target Platform | Android | API 21+ | Mobile devices |

### 3.2 Development Environment

- **IDE**: Visual Studio 2022 / VS Code
- **SDK**: .NET 10.0 Preview
- **Build System**: MSBuild
- **Version Control**: Git

---

## 4. Solution Structure

### 4.1 Project Organization

**Figure 4.1: Solution Structure**

```
prnet_project/
|
|-- FINAL_DOCUMENTATION.md      # This documentation
|-- DEPLOYMENT_GUIDE.md         # Deployment instructions
|-- Dockerfile                  # Container build configuration
|-- docker-compose.yml          # Container orchestration
|
+-- src/
    |
    +-- SherlockOsint.Api/          # Backend API Project
    |   |-- Hubs/
    |   |   +-- OsintHub.cs         # SignalR hub endpoint
    |   |-- Services/
    |   |   |-- RealSearchService.cs
    |   |   |-- CandidateAggregator.cs
    |   |   +-- OsintProviders/     # 21 data providers
    |   +-- Program.cs              # Application entry point
    |
    +-- SherlockOsint.Mobile/       # Android Mobile App
    |   |-- Views/
    |   |   |-- SearchPage.xaml     # Search input form
    |   |   +-- ResultsPage.xaml    # Results display
    |   |-- ViewModels/
    |   |   |-- SearchViewModel.cs
    |   |   +-- ResultsViewModel.cs
    |   +-- Services/
    |       +-- SignalRService.cs   # Real-time client
    |
    +-- SherlockOsint.Shared/       # Shared Class Library
        +-- Models/
            |-- SearchRequest.cs
            |-- OsintNode.cs
            |-- TargetCandidate.cs
            +-- SourceEvidence.cs
```

### 4.2 Project Dependencies

**Figure 4.2: Project Dependency Graph**

```
SherlockOsint.Mobile
        |
        +---> SherlockOsint.Shared
        
SherlockOsint.Api
        |
        +---> SherlockOsint.Shared
```

---

## 5. OSINT Data Providers

### 5.1 Provider Overview

The system includes 21 specialized data providers that query different online platforms and services.

**Table 5.1: OSINT Data Providers**

| No. | Provider | Data Source | Information Retrieved |
|-----|----------|-------------|----------------------|
| 1 | UsernameSearch | 30+ platforms | Profile existence, URLs |
| 2 | GitHubSearch | GitHub API | Repositories, email, contributions |
| 3 | GitLabSearch | GitLab API | Projects, profile data |
| 4 | EmailRepCheck | EmailRep.io | Email reputation, breach status |
| 5 | HunterIoLookup | Hunter.io | Email verification, domain info |
| 6 | ClearbitLookup | Clearbit | Person/company enrichment |
| 7 | GravatarLookup | Gravatar | Profile photo, verified email |
| 8 | PhoneValidator | LibPhoneNumber | Country, carrier, phone type |
| 9 | HibpBreachCheck | Have I Been Pwned | Data breach history |
| 10 | WaybackMachine | Archive.org | Historical web snapshots |
| 11 | IdentityLinker | Cross-platform | Identity correlation signals |
| 12 | CountryDetector | Inference engine | Location probabilities |
| 13 | DomainWhoisLookup | WHOIS servers | Domain ownership records |
| 14 | EmailDiscovery | Pattern generator | Possible email addresses |
| 15 | FullContactLookup | FullContact | Demographics data |
| 16 | PgpKeyserverLookup | PGP keyservers | Public encryption keys |
| 17 | ProfileVerifier | Verification engine | Confidence scoring |
| 18 | RedditDiscovery | Reddit API | User profile, post history |
| 19 | StackOverflow | Stack Overflow API | Reputation, expertise tags |
| 20 | YouTubeDiscovery | YouTube | Channel information |
| 21 | WebSearchProvider | Web scraping | General web mentions |

### 5.2 Provider Categories

**Free Providers (No API Key Required):**
- UsernameSearch, GitHubSearch, GitLabSearch, GravatarLookup
- PhoneValidator, WaybackMachine, PgpKeyserverLookup
- RedditDiscovery, StackOverflow, YouTubeDiscovery

**Premium Providers (API Key Required):**
- HunterIoLookup, ClearbitLookup, FullContactLookup, HibpBreachCheck

---

## 6. Key Features

### 6.1 Real-time Streaming

Results appear instantly as they are discovered. The system does not wait for all providers to complete before displaying results.

**Implementation:** Uses `System.Threading.Channels` for asynchronous, non-blocking data streaming.

### 6.2 Identity Correlation

The CandidateAggregator correlates data across platforms to identify probable matches:

- Username similarity matching using Levenshtein distance
- Cross-platform identity linking
- Email verification and domain analysis

### 6.3 Suspect Grading System (Probability Scoring)

The suspect grading system is a core component that calculates the probability that discovered profiles belong to the same real-world individual. This multi-factor scoring algorithm considers platform reliability, input matching, and evidence verification.

#### 6.3.1 Scoring Overview

Each identity candidate receives a **Probability Score** from 0% to 95%, representing confidence that the discovered profiles belong to the same person.

**Table 6.1: Score Interpretation**

| Score Range | Confidence Level | Description |
|-------------|------------------|-------------|
| 0-35% | Low | Profile exists but no verification of identity |
| 36-50% | Medium | Some matching criteria but limited evidence |
| 51-70% | High | Multiple matching platforms with input correlation |
| 71-85% | Very High | High-confidence matches with email/phone verification |
| 86-95% | Confirmed | Email verified AND multiple high-priority platform matches |

#### 6.3.2 Platform Priority System

Not all platforms are equal in terms of identity verification. The system assigns priority levels based on how reliably a platform verifies user identity.

**Table 6.2: Platform Priority Levels**

| Priority | Platforms | Weight Multiplier | Reasoning |
|----------|-----------|-------------------|-----------|
| 1 (Highest) | GitHub, LinkedIn, Twitter/X, Instagram, Gravatar | 2.5x | Often email-verified, professional profiles |
| 2 | GitLab, Reddit | 1.5x | Developer platforms with consistent usernames |
| 4 | SoundCloud, PyPI, Replit | 0.6x | Niche platforms, less verification |
| 5 | YouTube, Twitch | 0.5x | Entertainment platforms, less reliable matching |
| 6 (Lowest) | StackOverflow, Steam | 0.2x | Common usernames, high false positive rate |

**Code Implementation:**
```csharp
// Platform multipliers for score calculation
float multiplier = evidence.Platform.ToLower() switch
{
    "twitter" or "x" or "instagram" or "linkedin" or "github" => 2.5f,
    _ => evidence.PlatformPriority switch
    {
        1 => 1.5f,
        2 => 1.0f,
        4 => 0.6f,
        6 => 0.2f,
        _ => 0.5f
    }
};
```

#### 6.3.3 Evidence Scoring Algorithm

Each piece of evidence (discovered profile) receives a **Contribution Score** based on how well it matches the search input.

**Score Calculation Formula:**

```
ContributionScore = min(BaseScore × PlatformMultiplier, 60)
```

**Table 6.3: Base Score Components**

| Match Type | Base Score | Description |
|------------|------------|-------------|
| High-priority platform | +15 | Priority 1 platforms get automatic bonus |
| Exact nickname match | +30 | Username exactly matches input nickname |
| Name contains input | +20 | Profile name contains searched full name |
| Provider confidence < 20% | -25 | Low provider-level confidence reduces score |
| Provider confidence < 40% | -15 | Medium provider-level confidence reduces score |

**Code Implementation:**
```csharp
private void ScoreEvidence(SearchRequest request, SourceEvidence evidence)
{
    var score = 0;
    var reasons = new List<string>();

    // High-priority platform bonus
    if (evidence.PlatformPriority == 1)
    {
        score += 15;
        reasons.Add("High-priority platform");
    }

    // Nickname exact match with multiplier
    if (normalizedUsername.Equals(inputNickname, StringComparison.OrdinalIgnoreCase))
    {
        var matchScore = 30;
        
        // Scale by provider confidence if available
        if (evidence.ExtractedData.TryGetValue("Confidence", out var confStr))
        {
            if (decimal.TryParse(confStr, out var conf))
            {
                if (conf < 20) matchScore = 5;      // Very low confidence
                else if (conf < 40) matchScore = 15; // Medium confidence
            }
        }

        score += (int)(matchScore * multiplier);
        reasons.Add($"Nickname match ({evidence.Platform} x {multiplier:0.0})");
    }

    // Name match with multiplier
    if (!string.IsNullOrEmpty(inputFullName) && normalizedUsername.Contains(inputFullName))
    {
        score += (int)(20 * multiplier);
        reasons.Add("Name match");
    }

    evidence.ContributionScore = Math.Min(score, 60);  // Cap at 60
    evidence.Explanation = string.Join(", ", reasons);
}
```

#### 6.3.4 Overall Candidate Score Calculation

The final probability score is calculated based on all evidence sources and verification status.

**Scoring Tiers:**

**Tier 1: No Verification (Max 35%)**
- No email or phone provided in search
- No high-confidence matches found
- Score = min(SourceCount × 5, 25) + EmailBonus

**Tier 2: Input Without Matches (Max 45%)**
- Email or phone provided but no verified matches
- Score = min(SourceCount × 8, 35) + LocationBonus

**Tier 3: High-Confidence Matches (Max 85-95%)**
- High-confidence platform matches found
- Score = (HighConfCount × 20) + (OtherCount × 8) + Bonuses

**Code Implementation:**
```csharp
private int CalculateScore(SearchRequest request, TargetCandidate candidate)
{
    bool hasEmailInput = !string.IsNullOrEmpty(request.Email);
    bool hasPhoneInput = !string.IsNullOrEmpty(request.Phone);
    bool hasHighConfidenceMatch = candidate.Sources.Any(s => s.IsHighConfidence);
    bool hasVerifiedEmail = candidate.VerifiedEmails.Any(e => e.IsVerified);

    // TIER 1: NO VERIFICATION = MAX 35%
    if (!hasEmailInput && !hasPhoneInput && !hasHighConfidenceMatch)
    {
        var lowScore = Math.Min(candidate.Sources.Count * 5, 25);
        if (candidate.VerifiedEmails.Count > 0)
            lowScore += 10;
        return Math.Min(lowScore, 35);
    }

    // TIER 2: WITH INPUT BUT NO MATCHES = MAX 45%
    if (!hasHighConfidenceMatch && !hasVerifiedEmail)
    {
        var mediumScore = Math.Min(candidate.Sources.Count * 8, 35);
        if (candidate.ProbableLocation.Contains(phoneCountry))
            mediumScore += 10;
        return Math.Min(mediumScore, 45);
    }

    // TIER 3: WITH HIGH-CONFIDENCE MATCHES = Higher scores
    var baseScore = 0;
    var highConfCount = candidate.Sources.Count(s => s.IsHighConfidence);
    baseScore += highConfCount * 20;              // High-confidence matches
    baseScore += Math.Min((candidate.Sources.Count - highConfCount) * 8, 20);

    if (hasVerifiedEmail)
        baseScore += 15;  // Email verification bonus

    if (!string.IsNullOrEmpty(phoneCountry) && candidate.ProbableLocation.Contains(phoneCountry))
        baseScore += 10;  // Location match bonus

    // Dynamic score cap based on evidence quality
    var maxScore = 50;
    if (hasEmailInput || hasPhoneInput)
        maxScore = 70;
    if (hasHighConfidenceMatch)
        maxScore = 85;
    if (hasVerifiedEmail && hasHighConfidenceMatch)
        maxScore = 95;

    return Math.Min(baseScore, maxScore);
}
```

#### 6.3.5 Confidence Interval Calculation

To communicate uncertainty, each score includes a confidence interval showing the range of possible true values.

**Formula:**
```
Low = Score - (Uncertainty × (1 - HighConfRatio))
High = Score + (Uncertainty × HighConfRatio)
```

Where:
- `Uncertainty` = base uncertainty value (typically 10-15%)
- `HighConfRatio` = proportion of high-confidence sources

#### 6.3.6 Score Display

The mobile app displays scores as follows:

| Component | Description |
|-----------|-------------|
| Main Score | Large percentage (e.g., "72%") |
| Confidence Range | Range shown as "65-79%" |
| Color Coding | Green (>70%), Yellow (40-70%), Red (<40%) |
| Source Count | "Found on 5 platforms" |

**Example Display:**
```
+-----------------------------------+
|  John Doe                   72%  |
|  Range: 65-79%                   |
|  Found on 5 platforms            |
|                                  |
|  [GH] GitHub: johndoe      +25%  |
|  [LI] LinkedIn: john-doe   +20%  |
|  [TW] Twitter: @johndoe    +15%  |
|  [IG] Instagram: johndoe   +12%  |
+-----------------------------------+
```

### 6.4 Clickable Source Links

All source evidence items are clickable, opening the original profile URL in the device browser.

**Implementation:** ContentView with TapGestureRecognizer pattern for reliable touch handling on Android.

### 6.5 Throttled Recursion

Smart depth limiting prevents API rate limits while maximizing discovery:

- Maximum recursion depth: 2 levels
- Configurable permutation limits
- Provider-specific rate limiting

---

## 7. Data Flow

### 7.1 Sequence Diagram

**Figure 7.1: Data Flow Sequence**

```
User                Mobile App           SignalR Hub         SearchService        Providers
  |                      |                   |                    |                   |
  |-- Enter search ----->|                   |                    |                   |
  |                      |                   |                    |                   |
  |                      |-- StartSearch --->|                    |                   |
  |                      |                   |-- QueueSearch ---->|                   |
  |                      |                   |                    |                   |
  |                      |                   |                    |-- Query --------->|
  |                      |                   |                    |                   |
  |                      |                   |                    |<-- OsintNode -----|
  |                      |                   |<-- ReceiveNode ----|                   |
  |                      |<-- Display -------|                    |                   |
  |<-- See result -------|                   |                    |                   |
  |                      |                   |                    |                   |
  |                      |                   |                    |-- Aggregate --+   |
  |                      |                   |<-- Candidates -----|              |   |
  |                      |<-- Display -------|                    |<-------------+   |
  |<-- See matches ------|                   |                    |                   |
```

### 7.2 Data Models

**SearchRequest:**
```csharp
public class SearchRequest
{
    public string? Email { get; set; }
    public string? Nickname { get; set; }
    public string? FullName { get; set; }
    public string? Phone { get; set; }
}
```

**TargetCandidate:**
```csharp
public class TargetCandidate
{
    public string Name { get; set; }
    public int ProbabilityScore { get; set; }
    public int ConfidenceLow { get; set; }
    public int ConfidenceHigh { get; set; }
    public List<SourceEvidence> Sources { get; set; }
}
```

---

## 8. Mobile Application

### 8.1 User Interface

The mobile application consists of two main screens:

1. **Search Page** - Input form for search criteria
2. **Results Page** - Real-time display of findings

### 8.2 Search Page

Allows users to enter one or more search identifiers:

- Email address field with validation
- Nickname/username field
- Full name field
- Phone number field with country code

### 8.3 Results Page

Displays search results in real-time:

- Target candidate cards with probability scores
- Expandable source evidence lists
- Clickable profile links
- Country distribution visualization

### 8.4 SignalR Client Service

The mobile app maintains a persistent WebSocket connection to the API for receiving real-time updates.

**Key Events:**
- `NodeReceived` - Individual OSINT finding
- `CandidatesReceived` - Aggregated identity matches
- `SearchCompleted` - Search finished
- `SearchError` - Error occurred

---

## 9. Backend API

### 9.1 API Endpoints

**Table 9.1: REST Endpoints**

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/` | GET | API status message |
| `/health` | GET | Health check for load balancers |

### 9.2 SignalR Hub Methods

**Table 9.2: SignalR Hub Methods**

| Method | Direction | Parameters | Description |
|--------|-----------|------------|-------------|
| StartSearch | Client -> Server | SearchRequest | Begin OSINT search |
| CancelSearch | Client -> Server | - | Cancel ongoing search |
| ReceiveNode | Server -> Client | OsintNode | Stream single finding |
| ReceiveCandidates | Server -> Client | List of TargetCandidate | Send aggregated results |
| SearchCompleted | Server -> Client | string | Notify search finished |
| SearchError | Server -> Client | string | Send error message |

### 9.3 Configuration

API configuration is managed through `appsettings.json`:

```json
{
  "Osint": {
    "HunterApiKey": "",
    "HibpApiKey": "",
    "ClearbitApiKey": "",
    "FullContactApiKey": ""
  },
  "SignalR": {
    "ConnectionTimeout": 60,
    "KeepAliveInterval": 30
  }
}
```

---

## 10. Deployment

### 10.1 Current Deployment Status

The application is currently deployed in a **local network configuration**:

- **Backend API**: Running on development PC (Windows)
- **Mobile App**: Installed on Xiaomi Android device
- **Communication**: Via local WiFi network (192.168.1.x)

### 10.2 Local Network Setup

**Configuration:**
- API listens on: `http://0.0.0.0:57063`
- Mobile app connects to: `http://192.168.1.129:57063`

**Requirements:**
- Both devices on the same WiFi network
- Windows Firewall rule allowing port 57063
- API server running on PC

### 10.3 Cloud Deployment (Future)

The application is **fully prepared for cloud deployment** and includes:

- `Dockerfile` - Container build configuration
- `docker-compose.yml` - Container orchestration
- `appsettings.Production.json` - Production configuration

**Planned Cloud Platform:** Microsoft Azure

**Deployment will be completed** after obtaining Azure for Students subscription, which provides:
- Free Azure credits for educational use
- Azure App Service for API hosting
- Azure Container Instances for Docker deployment

**To deploy to Azure:**
```bash
# Build Docker image
docker build -t poirot-osint-api .

# Push to Azure Container Registry
az acr login --name <registry>
docker push <registry>.azurecr.io/poirot-osint-api

# Deploy to Azure Container Instances
az container create --name poirot-api --image <image>
```

---

## 11. Testing

### 11.1 Testing Methodology

The application was tested using the following approaches:

1. **Unit Testing** - Individual provider functionality
2. **Integration Testing** - API endpoint verification
3. **End-to-End Testing** - Full flow from mobile to API

### 11.2 Test Cases

**Table 11.1: Test Cases**

| Test ID | Description | Input | Expected Result | Status |
|---------|-------------|-------|-----------------|--------|
| T01 | Search by email | test@example.com | Returns matching profiles | PASS |
| T02 | Search by nickname | testuser123 | Returns platform matches | PASS |
| T03 | Real-time streaming | Any search | Results appear progressively | PASS |
| T04 | Clickable links | Tap source | Opens browser with URL | PASS |
| T05 | Cancel search | Press back | Search stops, returns | PASS |
| T06 | Network disconnect | Disable WiFi | Shows error message | PASS |
| T07 | Invalid input | Empty fields | Shows validation error | PASS |

### 11.3 Test Results

All core functionality tests passed successfully. The application correctly:

- Searches across 21 OSINT providers
- Streams results in real-time
- Correlates identity candidates
- Opens source links in browser
- Handles errors gracefully

---

## 12. Conclusions

### 12.1 Achievements

The Poirot OSINT project successfully achieved its objectives:

1. Created a functional OSINT aggregation system with 21 data providers
2. Implemented real-time streaming via SignalR for responsive user experience
3. Developed identity correlation with probability scoring
4. Built a cross-platform mobile application for Android
5. Prepared cloud deployment configuration for future Azure hosting

### 12.2 Technical Highlights

- **Modern Architecture**: Clean separation between mobile client, API, and shared models
- **Real-time Communication**: Efficient WebSocket-based streaming
- **Scalable Design**: Ready for cloud deployment and horizontal scaling
- **User-Friendly**: Intuitive mobile interface with clickable source links

### 12.3 Future Work

1. **Azure Deployment**: Deploy to Microsoft Azure after obtaining student subscription
2. **Additional Providers**: Integrate more social platforms (TikTok, Discord, Telegram)
3. **iOS Support**: Extend mobile app to iOS platform
4. **Export Features**: Add PDF/JSON export of search results
5. **Search History**: Implement local search history storage

---

## 13. References

1. Microsoft .NET MAUI Documentation - https://docs.microsoft.com/dotnet/maui
2. ASP.NET Core SignalR Documentation - https://docs.microsoft.com/aspnet/core/signalr
3. Azure Container Instances Documentation - https://docs.microsoft.com/azure/container-instances
4. CommunityToolkit.Mvvm - https://github.com/CommunityToolkit/dotnet
5. Docker Documentation - https://docs.docker.com

---

## Appendix A: Key Code Sections

This appendix contains the most important code sections with detailed explanations.

---

### A.1 SignalR Hub (OsintHub.cs)

The SignalR hub is the communication endpoint between mobile clients and the backend API.

**File:** `src/SherlockOsint.Api/Hubs/OsintHub.cs`

```csharp
using Microsoft.AspNetCore.SignalR;
using SherlockOsint.Api.Services;
using SherlockOsint.Shared.Models;

namespace SherlockOsint.Api.Hubs;

/// <summary>
/// SignalR hub for streaming OSINT search results to connected clients
/// </summary>
public class OsintHub : Hub
{
    private readonly ISearchOrchestrator _searchOrchestrator;
    private readonly ILogger<OsintHub> _logger;

    public OsintHub(ISearchOrchestrator searchOrchestrator, ILogger<OsintHub> logger)
    {
        _searchOrchestrator = searchOrchestrator;
        _logger = logger;
    }

    /// <summary>
    /// Called when a client connects to the hub
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        _searchOrchestrator.CancelSearch(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Initiates an OSINT search for the connected client
    /// </summary>
    public async Task StartSearch(SearchRequest request)
    {
        if (!request.HasSearchCriteria)
        {
            await Clients.Caller.SendAsync("SearchError", "Please provide at least one search criterion.");
            return;
        }

        request.ConnectionId = Context.ConnectionId;
        
        _logger.LogInformation("Starting search for {ConnectionId}", Context.ConnectionId);

        await Clients.Caller.SendAsync("SearchStarted", "Search initiated.");
        
        // Queue the search request for background processing
        _searchOrchestrator.QueueSearch(request, Context.ConnectionId);
    }

    /// <summary>
    /// Cancels an ongoing search for the connected client
    /// </summary>
    public Task CancelSearch()
    {
        _logger.LogInformation("Search cancelled by client: {ConnectionId}", Context.ConnectionId);
        _searchOrchestrator.CancelSearch(Context.ConnectionId);
        return Task.CompletedTask;
    }
}
```

**Description:**
- Inherits from `Hub` base class provided by SignalR
- `OnConnectedAsync` / `OnDisconnectedAsync` - lifecycle methods for connection management
- `StartSearch` - validates input and queues search for background processing
- `CancelSearch` - allows clients to stop an ongoing search
- Uses dependency injection to access `ISearchOrchestrator` service

---

### A.2 Real-time Search Service (RealSearchService.cs)

The search service orchestrates all 21 OSINT providers and streams results using `System.Threading.Channels`.

**File:** `src/SherlockOsint.Api/Services/RealSearchService.cs`

```csharp
using SherlockOsint.Shared.Models;
using System.Threading.Channels;

namespace SherlockOsint.Api.Services;

public interface IRealSearchService
{
    IAsyncEnumerable<OsintNode> SearchAsync(SearchRequest request, CancellationToken ct);
}

public class RealSearchService : IRealSearchService
{
    // Dependency injection of all 21 OSINT providers
    private readonly GravatarLookup _gravatarLookup;
    private readonly GitHubSearch _githubSearch;
    private readonly UsernameSearch _usernameSearch;
    // ... (16 more providers)
    
    public async IAsyncEnumerable<OsintNode> SearchAsync(
        SearchRequest request,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        // Yield immediate confirmation to client
        yield return new OsintNode 
        { 
            Label = "Discovery Started", 
            Value = "Stage 1: Input Enrichment", 
            Depth = 0 
        };

        // Create unbounded channel for non-blocking streaming
        var channel = Channel.CreateUnbounded<OsintNode>();
        
        // Track seen URLs to prevent duplicates
        var seenUrls = new ConcurrentDictionary<string, bool>();
        
        // Helper to deduplicate and stream results
        void AddResult(OsintNode node)
        {
            if (string.IsNullOrEmpty(node.Value) || !node.Value.StartsWith("http"))
            {
                channel.Writer.TryWrite(node);
                return;
            }
            
            // Deduplicate by URL
            if (seenUrls.TryAdd(node.Value, true))
            {
                channel.Writer.TryWrite(node);
            }
        }

        // Background task: Run all providers in parallel
        _ = Task.Run(async () =>
        {
            var tasks = new List<Task>();
            
            // Email-based providers
            if (!string.IsNullOrEmpty(request.Email))
            {
                tasks.Add(RunProvider(() => _gravatarLookup.SearchAsync(request.Email), AddResult));
                tasks.Add(RunProvider(() => _githubSearch.SearchByEmailAsync(request.Email), AddResult));
                // ... more email providers
            }
            
            // Username-based providers
            if (!string.IsNullOrEmpty(request.Nickname))
            {
                tasks.Add(RunProvider(() => _usernameSearch.SearchAsync(request.Nickname), AddResult));
                tasks.Add(RunProvider(() => _githubSearch.SearchByUsernameAsync(request.Nickname), AddResult));
                // ... more username providers
            }
            
            await Task.WhenAll(tasks);
            channel.Writer.TryComplete();
        }, ct);

        // Stream results as they arrive
        await foreach (var node in channel.Reader.ReadAllAsync(ct))
        {
            yield return node;
        }
    }
}
```

**Description:**
- Uses `IAsyncEnumerable<OsintNode>` for streaming results
- `Channel.CreateUnbounded<T>()` enables non-blocking producer-consumer pattern
- `ConcurrentDictionary` prevents duplicate URLs in results
- All 21 providers run in parallel using `Task.WhenAll`
- Results stream to client immediately as they are discovered

---

### A.3 Candidate Aggregator (CandidateAggregator.cs)

The aggregator correlates OSINT results into identity candidates with probability scoring.

**File:** `src/SherlockOsint.Api/Services/CandidateAggregator.cs`

```csharp
using SherlockOsint.Shared.Models;

namespace SherlockOsint.Api.Services;

/// <summary>
/// Aggregates OSINT results into target candidates with probability scoring
/// </summary>
public class CandidateAggregator
{
    // Platform priorities (1 = highest importance)
    private static readonly Dictionary<string, (int Priority, string Icon)> PlatformInfo = new()
    {
        { "github", (1, "[GH]") },
        { "linkedin", (1, "[LI]") },
        { "twitter", (1, "[TW]") },
        { "instagram", (1, "[IG]") },
        { "reddit", (1, "[RD]") },
        { "gitlab", (1, "[GL]") },
        { "gravatar", (1, "[GR]") }, // High priority - email verified
        { "youtube", (5, "[YT]") },
        { "stackoverflow", (6, "[SO]") },
    };

    /// <summary>
    /// Build target candidates from search results
    /// Groups by unique username, merges only with strong evidence
    /// </summary>
    public List<TargetCandidate> BuildCandidates(SearchRequest request, List<OsintNode> results)
    {
        var candidates = new List<TargetCandidate>();
        
        // Extract and normalize input data
        var inputEmail = request.Email?.ToLower().Trim();
        var inputNickname = NormalizeUsername(request.Nickname ?? "");
        
        // Group platform nodes by normalized username
        var usernameGroups = new Dictionary<string, List<SourceEvidence>>();
        
        foreach (var node in results)
        {
            if (!IsPlatformNode(node)) continue;
            
            var evidence = BuildSourceEvidence(node);
            if (evidence == null) continue;
            
            // Group by normalized username
            var normalizedUsername = NormalizeUsername(evidence.Username);
            if (!usernameGroups.ContainsKey(normalizedUsername))
                usernameGroups[normalizedUsername] = new List<SourceEvidence>();
            
            usernameGroups[normalizedUsername].Add(evidence);
        }
        
        // Build candidates from groups
        foreach (var (username, sources) in usernameGroups)
        {
            var candidate = new TargetCandidate
            {
                Name = username,
                Sources = sources,
                ProbabilityScore = CalculateProbability(sources, inputNickname, inputEmail)
            };
            candidates.Add(candidate);
        }
        
        return candidates.OrderByDescending(c => c.ProbabilityScore).ToList();
    }

    /// <summary>
    /// Extract a valid HTTP URL from a node
    /// </summary>
    private string ExtractValidUrl(OsintNode node)
    {
        // Check if node.Value is a valid URL
        if (!string.IsNullOrEmpty(node.Value) && 
            node.Value.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return node.Value;
        }

        // Check children for URLs
        foreach (var child in node.Children)
        {
            if (child.Value?.StartsWith("http") == true)
                return child.Value;
        }

        return "";
    }
}
```

**Description:**
- Groups OSINT findings by normalized username
- Assigns platform priorities (GitHub, LinkedIn highest; YouTube, StackOverflow lower)
- `ExtractValidUrl` ensures only HTTP URLs are stored (fixes clickable links)
- Calculates probability score based on number of sources and input matching
- Returns candidates sorted by probability score (highest first)

---

### A.4 Mobile SignalR Client (SignalRService.cs)

The mobile client service manages WebSocket connection to the backend.

**File:** `src/SherlockOsint.Mobile/Services/SignalRService.cs`

```csharp
using Microsoft.AspNetCore.SignalR.Client;
using SherlockOsint.Shared.Models;

namespace SherlockOsint.Mobile.Services;

public class SignalRService : ISignalRService, IAsyncDisposable
{
    private HubConnection? _hubConnection;
    private readonly string _hubUrl;

    // Configurable API base URL for cloud deployment
    public static string ApiBaseUrl { get; set; } = "http://192.168.1.129:57063";

    // Events for UI binding
    public event EventHandler<OsintNode>? NodeReceived;
    public event EventHandler<List<TargetCandidate>>? CandidatesReceived;
    public event EventHandler<string>? SearchCompleted;
    public event EventHandler<string>? SearchError;

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public SignalRService()
    {
        // Use configured URL or fallback to localhost
        if (!string.IsNullOrEmpty(ApiBaseUrl))
        {
            _hubUrl = $"{ApiBaseUrl.TrimEnd('/')}/osinthub";
        }
        else
        {
            // Default for Android emulator
            _hubUrl = DeviceInfo.Platform == DevicePlatform.Android 
                ? "http://10.0.2.2:57063/osinthub" 
                : "http://localhost:57063/osinthub";
        }
    }

    public async Task ConnectAsync()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_hubUrl)
            .WithAutomaticReconnect()
            .Build();

        // Register event handlers for server messages
        _hubConnection.On<OsintNode>("ReceiveNode", node =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                NodeReceived?.Invoke(this, node);
            });
        });

        _hubConnection.On<List<TargetCandidate>>("ReceiveCandidates", candidates =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                CandidatesReceived?.Invoke(this, candidates);
            });
        });

        await _hubConnection.StartAsync();
    }

    public async Task StartSearchAsync(SearchRequest request)
    {
        if (_hubConnection != null)
        {
            await _hubConnection.InvokeAsync("StartSearch", request);
        }
    }
}
```

**Description:**
- Uses `HubConnectionBuilder` to create SignalR connection
- `WithAutomaticReconnect()` handles network interruptions
- Event handlers marshal callbacks to UI thread using `MainThread.BeginInvokeOnMainThread`
- `ApiBaseUrl` is configurable for local/cloud deployment
- Implements `IAsyncDisposable` for proper resource cleanup

---

### A.5 Clickable Links Implementation (ResultsPage.xaml)

XAML pattern for making source links clickable on Android.

**File:** `src/SherlockOsint.Mobile/Views/ResultsPage.xaml`

```xml
<!-- Source Evidence List with BindableLayout -->
<VerticalStackLayout BindableLayout.ItemsSource="{Binding Sources}">
    <BindableLayout.ItemTemplate>
        <DataTemplate x:DataType="models:SourceEvidence">
            
            <!-- ContentView captures tap gestures -->
            <ContentView Padding="0" Margin="0,2">
                <ContentView.GestureRecognizers>
                    <TapGestureRecognizer 
                        Command="{Binding Source={RelativeSource AncestorType={x:Type viewmodels:ResultsViewModel}}, 
                                         Path=OpenSourceUrlCommand}"
                        CommandParameter="{Binding .}" />
                </ContentView.GestureRecognizers>
                
                <!-- Frame with InputTransparent allows taps to pass through -->
                <Frame BackgroundColor="{StaticResource Background}"
                       BorderColor="{StaticResource Primary}"
                       CornerRadius="8"
                       Padding="10,8"
                       InputTransparent="True">
                    
                    <Grid ColumnDefinitions="Auto,*,Auto" InputTransparent="True">
                        <!-- Platform icon -->
                        <Label Text="{Binding Icon}"
                               FontSize="16"
                               InputTransparent="True" />
                        
                        <!-- Platform and username (clickable link style) -->
                        <VerticalStackLayout Grid.Column="1" InputTransparent="True">
                            <Label TextColor="{StaticResource Primary}"
                                   TextDecorations="Underline"
                                   InputTransparent="True">
                                <Label.FormattedText>
                                    <FormattedString>
                                        <Span Text="{Binding Platform}" FontAttributes="Bold" />
                                        <Span Text=": " />
                                        <Span Text="{Binding Username}" />
                                    </FormattedString>
                                </Label.FormattedText>
                            </Label>
                            <Label Text="{Binding Explanation}"
                                   FontSize="11"
                                   InputTransparent="True" />
                        </VerticalStackLayout>
                        
                        <!-- Contribution score -->
                        <Label Grid.Column="2"
                               Text="{Binding ContributionScore, StringFormat='+{0}%'}"
                               TextColor="{StaticResource Success}"
                               InputTransparent="True" />
                    </Grid>
                </Frame>
            </ContentView>
            
        </DataTemplate>
    </BindableLayout.ItemTemplate>
</VerticalStackLayout>
```

**Description:**
- Uses `BindableLayout` instead of nested `CollectionView` (fixes Android touch issues)
- `ContentView` with `TapGestureRecognizer` captures all tap events
- All child elements have `InputTransparent="True"` so taps pass through to parent
- `RelativeSource AncestorType` binds to ViewModel command
- `TextDecorations="Underline"` provides visual link affordance

---

### A.6 Open URL Command (ResultsViewModel.cs)

ViewModel command that opens URLs in device browser.

**File:** `src/SherlockOsint.Mobile/ViewModels/ResultsViewModel.cs`

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SherlockOsint.Mobile.ViewModels;

public partial class ResultsViewModel : ObservableObject
{
    [RelayCommand]
    private async Task OpenUrlAsync(object parameter)
    {
        string? url = null;
        
        // Handle different parameter types
        if (parameter is OsintNode node) 
            url = node.Value;
        else if (parameter is SourceEvidence evidence) 
            url = evidence.Url;
        else if (parameter is string s) 
            url = s;

        // Validate URL before opening
        if (!string.IsNullOrEmpty(url) && url.StartsWith("http"))
        {
            try
            {
                await Launcher.OpenAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                StatusMessage = $"Could not open URL: {ex.Message}";
            }
        }
    }

    [RelayCommand]
    private async Task OpenSourceUrlAsync(SourceEvidence evidence)
    {
        await OpenUrlAsync(evidence);
    }
}
```

**Description:**
- Uses `[RelayCommand]` attribute from CommunityToolkit.Mvvm to generate ICommand
- Handles multiple parameter types (OsintNode, SourceEvidence, string)
- Validates URL starts with "http" before attempting to open
- Uses MAUI `Launcher.OpenAsync()` to open URL in device browser
- Graceful error handling with user feedback

---

**End of Documentation**

*Document Version: 1.0*  
*Last Updated: February 2026*

