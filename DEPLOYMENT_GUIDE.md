# Poirot OSINT - Cloud Deployment Guide

## Overview

This guide covers deploying the Poirot OSINT application to the cloud and generating a signed APK for Android distribution.

---

## Part 1: API Cloud Deployment

### Option A: Docker Deployment (Recommended)

#### Prerequisites
- Docker and Docker Compose installed
- Cloud VM/container service (Azure, AWS, DigitalOcean, etc.)

#### Step 1: Build and Run Locally
```bash
# Clone/copy project to server
cd prnet_project

# Set environment variables for API keys (optional)
export HUNTER_API_KEY=your_key_here
export HIBP_API_KEY=your_key_here

# Build and start container
docker-compose up -d --build

# Verify it's running
curl http://localhost:8080/health
```

#### Step 2: Deploy to Cloud Provider

**Azure Container Instances:**
```bash
# Login to Azure
az login

# Create resource group
az group create --name PoirotOSINT --location eastus

# Deploy container
az container create \
  --resource-group PoirotOSINT \
  --name poirot-api \
  --image your-registry/poirot-osint-api:latest \
  --ports 80 \
  --dns-name-label poirot-osint \
  --environment-variables \
    ASPNETCORE_ENVIRONMENT=Production \
    Osint__HunterApiKey=$HUNTER_API_KEY

# Get the public URL
az container show --resource-group PoirotOSINT --name poirot-api --query ipAddress.fqdn
# Result: poirot-osint.eastus.azurecontainer.io
```

**AWS ECS/Fargate:**
```bash
# Push to ECR
aws ecr create-repository --repository-name poirot-osint-api
docker tag poirot-osint-api:latest <account>.dkr.ecr.<region>.amazonaws.com/poirot-osint-api
docker push <account>.dkr.ecr.<region>.amazonaws.com/poirot-osint-api

# Create ECS task definition and service via console or CLI
```

**DigitalOcean App Platform:**
1. Create new App from GitHub/Docker
2. Select Dockerfile
3. Set environment variables
4. Deploy

---

### Option B: Direct .NET Deployment

For platforms like Azure App Service:

```bash
# Build for release
cd src/SherlockOsint.Api
dotnet publish -c Release -o ./publish

# Deploy to Azure App Service
az webapp deployment source config-zip \
  --resource-group PoirotOSINT \
  --name poirot-api \
  --src ./publish.zip
```

---

## Part 2: Generate Signed APK

### Step 1: Create Keystore (One-time setup)

```bash
# Generate a keystore for signing
keytool -genkey -v -keystore poirot-osint.keystore \
  -alias poirot -keyalg RSA -keysize 2048 -validity 10000

# Enter password and details when prompted
# SAVE THIS KEYSTORE AND PASSWORD SECURELY!
```

### Step 2: Configure API URL for Production

Edit `MauiProgram.cs` or set before app starts:

```csharp
// In MauiProgram.cs, before CreateMauiApp():
SignalRService.ApiBaseUrl = "https://your-cloud-url.com";
```

Or create a settings page where users can configure the URL.

### Step 3: Build Signed APK

```powershell
# Navigate to mobile project
cd src/SherlockOsint.Mobile

# Build signed release APK
dotnet publish -f net10.0-android -c Release `
  -p:AndroidKeyStore=true `
  -p:AndroidSigningKeyStore="..\..\poirot-osint.keystore" `
  -p:AndroidSigningKeyAlias=poirot `
  -p:AndroidSigningKeyPass=YOUR_KEYSTORE_PASSWORD `
  -p:AndroidSigningStorePass=YOUR_KEYSTORE_PASSWORD

# APK location:
# bin/Release/net10.0-android/publish/com.sherlock.osint-Signed.apk
```

### Step 4: Alternative - Build Unsigned APK (for testing)

```powershell
dotnet build -f net10.0-android -c Release
# APK at: bin/Release/net10.0-android/com.sherlock.osint.apk
```

---

## Part 3: Mobile App Configuration

### Hardcode URL (Simplest)
Edit `SignalRService.cs` and set the static property:

```csharp
public static string ApiBaseUrl { get; set; } = "https://poirot-osint.azurecontainer.io";
```

### Runtime Configuration (Recommended)
Create a settings page where users can enter the API URL, stored in `Preferences`:

```csharp
// Save
Preferences.Set("ApiBaseUrl", "https://your-url.com");

// Load in MauiProgram.cs
SignalRService.ApiBaseUrl = Preferences.Get("ApiBaseUrl", "");
```

---

## Part 4: Network Configuration

### HTTPS Requirements
For production, use HTTPS:

1. **Azure/AWS**: Free SSL certificates included
2. **Custom domain**: Use Let's Encrypt with nginx reverse proxy
3. **Android**: Android 9+ requires HTTPS by default

### Firewall Rules
Ensure these ports are open:
- **80/443**: HTTP/HTTPS for API
- **SignalR**: Uses WebSocket over HTTP/HTTPS

---

## Part 5: Environment Variables Reference

| Variable | Description | Required |
|----------|-------------|----------|
| `ASPNETCORE_ENVIRONMENT` | Set to `Production` | Yes |
| `Osint__HunterApiKey` | Hunter.io API key | Optional |
| `Osint__HibpApiKey` | Have I Been Pwned API key | Optional |
| `Osint__ClearbitApiKey` | Clearbit API key | Optional |
| `Osint__FullContactApiKey` | FullContact API key | Optional |

---

## Quick Deployment Checklist

- [ ] Build Docker image: `docker build -t poirot-osint-api .`
- [ ] Push to container registry (Docker Hub, ACR, ECR)
- [ ] Deploy to cloud (Azure, AWS, DigitalOcean)
- [ ] Verify API health: `curl https://your-url.com/health`
- [ ] Update mobile app API URL in `SignalRService.cs`
- [ ] Generate keystore for signing
- [ ] Build signed APK: `dotnet publish -f net10.0-android -c Release ...`
- [ ] Test APK on physical device
- [ ] (Optional) Upload to Google Play Store

---

## Troubleshooting

### SignalR Connection Failed
- Ensure CORS is configured (already done in Program.cs)
- Check if WebSocket is enabled on cloud provider
- Verify URL format: `https://domain.com` (no trailing slash)

### APK Won't Install
- Enable "Install from unknown sources" on Android device
- Make sure APK is signed for release builds

### API Rate Limited
- Some OSINT providers have rate limits
- Configure API keys for higher limits

---

*Deployment guide for Poirot OSINT project*
