#requires -Version 5.1
param(
    [string]$FullName = "Pawel Murdzek",
    [string]$Nickname = "pmurdzek",
    [string]$Email    = "pawel.murdzek@gmail.com",
    [string]$Phone    = "",
    [switch]$KeepServer
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $repoRoot

$logPath = Join-Path $env:TEMP "poirot.log"
$errPath = Join-Path $env:TEMP "poirot.err.log"
$pidPath = Join-Path $env:TEMP "poirot.pid"

function Write-Step($msg) { Write-Host "[poirot] $msg" -ForegroundColor Cyan }

function Stop-PoirotServer {
    if (Test-Path $pidPath) {
        $sid = Get-Content $pidPath -ErrorAction SilentlyContinue
        if ($sid -and (Get-Process -Id $sid -ErrorAction SilentlyContinue)) {
            Write-Step "stopping server pid=$sid"
            Stop-Process -Id $sid -Force -ErrorAction SilentlyContinue
        }
        Remove-Item $pidPath -ErrorAction SilentlyContinue
    }
}

# 1) Boot API
Write-Step "[1/6] starting API in background"
$srv = Start-Process -FilePath dotnet `
    -ArgumentList 'run','--project','src/SherlockOsint.Api' `
    -RedirectStandardOutput $logPath `
    -RedirectStandardError $errPath `
    -NoNewWindow -PassThru
$srv.Id | Set-Content -Encoding utf8 $pidPath
Write-Step "      pid=$($srv.Id) | log=$logPath"

try {
    # 2) Wait for /health. Use 127.0.0.1 explicitly: PowerShell on Windows often
    #    resolves "localhost" to ::1 first, but Kestrel here binds 0.0.0.0 only
    #    (see Properties/launchSettings.json) — so localhost would loop forever.
    Write-Step "[2/6] waiting for /health (up to 60s)"
    $apiBase = "http://127.0.0.1:57063"
    $deadline = (Get-Date).AddSeconds(60)
    $attempt = 0
    do {
        Start-Sleep -Seconds 1
        $attempt++
        try {
            $resp = Invoke-WebRequest "$apiBase/health" -UseBasicParsing -TimeoutSec 2 -ErrorAction Stop
            $up = ($resp.StatusCode -eq 200)
        } catch { $up = $false }
        if (-not $up -and ($attempt % 5 -eq 0)) {
            Write-Step "      still waiting... ($attempt s)"
        }
        if ((Get-Date) -gt $deadline) {
            $tail = Get-Content $logPath -Tail 30 -ErrorAction SilentlyContinue
            $errTail = Get-Content $errPath -Tail 30 -ErrorAction SilentlyContinue
            throw "API did not come up within 60s.`n--- stdout tail ---`n$tail`n--- stderr tail ---`n$errTail"
        }
    } while (-not $up)
    Write-Step "      server up after $attempt s"

    # 3) Ensure SignalR client deps
    Write-Step "[3/6] ensuring Node + @microsoft/signalr"
    if (-not (Get-Command node -ErrorAction SilentlyContinue)) {
        throw "node not on PATH. Install Node.js or run from a shell where it is."
    }
    $scriptDir = Join-Path $repoRoot "scripts"
    if (-not (Test-Path $scriptDir)) { New-Item -ItemType Directory $scriptDir | Out-Null }
    Push-Location $scriptDir
    try {
        if (-not (Test-Path 'package.json')) {
            npm init -y | Out-Null
        }
        if (-not (Test-Path 'node_modules\@microsoft\signalr')) {
            Write-Step "      installing @microsoft/signalr"
            npm install '@microsoft/signalr' | Out-Null
        }
    } finally {
        Pop-Location
    }

    # 4) Write static test-search.mjs (reads request from POIROT_REQUEST env var)
    Write-Step "[4/6] writing test-search.mjs"
    $jsBody = @'
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";

const req = JSON.parse(process.env.POIROT_REQUEST || "{}");
let lastSession = null;

const c = new HubConnectionBuilder()
  .withUrl("http://127.0.0.1:57063/osinthub")
  .configureLogging(LogLevel.Warning)
  .build();

c.on("ReceiveNode", n => {
  const v = (n.value || "").toString();
  console.log("[node]", n.label, "->", v.length > 120 ? v.slice(0, 117) + "..." : v);
});
c.on("ReceiveCandidates", a => console.log(`[candidates] ${a.length} total`));
c.on("ReceiveProfile", p => console.log(`[profile] platforms=${(p && p.platforms ? p.platforms.length : 0)}`));
c.on("ReceiveSessionMemory", r => {
  lastSession = r;
  console.log("\n[session] folder=" + r.folderPath);
  console.log("[session] json=" + r.jsonPath);
  console.log("[session] md=" + r.markdownPath);
  console.log("[session] aiConfigured=" + r.claudeApiConfigured);
});
c.on("ReceivePersonalityProfile", p => console.log("[personality]", (p && (p.candidateUsername || p.username)) || "?"));
c.on("SearchStarted", m => console.log("[started]", m));
c.on("SearchError", e => { console.error("[err]", e); process.exit(1); });
c.on("SearchCancelled", m => { console.error("[cancelled]", m); process.exit(2); });
c.on("SearchCompleted", () => {
  console.log("[done]");
  if (lastSession && lastSession.claudeCommand) {
    console.log("\n=========================================================");
    console.log("PASTE THIS INTO ANOTHER TERMINAL FOR LOCAL CLAUDE RANKING:");
    console.log("=========================================================");
    console.log(lastSession.claudeCommand);
    console.log("=========================================================\n");
  }
  c.stop().then(() => process.exit(0));
});

await c.start();
console.log("[connected] invoking StartSearch with", JSON.stringify(req));
await c.invoke("StartSearch", req);
'@
    $jsPath = Join-Path $scriptDir 'test-search.mjs'
    $jsBody | Set-Content -Encoding utf8 $jsPath

    # 5) Run search — pass request as env var so we don't have to splice JSON
    Write-Step "[5/6] running search: name=$FullName nick=$Nickname email=$Email phone=$Phone"
    $reqObj = [ordered]@{
        fullName = $FullName
        nickname = $Nickname
        email    = $Email
        phone    = $Phone
    }
    $env:POIROT_REQUEST = ($reqObj | ConvertTo-Json -Compress)
    & node $jsPath
    $exit = $LASTEXITCODE
    Write-Step "      node exit=$exit"

    # 6) Show last session snapshot from disk + reprint claude command
    Write-Step "[6/6] /sessions snapshot"
    $sessionsDir = Join-Path $repoRoot "sessions"
    if (Test-Path $sessionsDir) {
        Get-ChildItem $sessionsDir |
            Sort-Object LastWriteTime -Descending |
            Select-Object -First 6 Name, Length, LastWriteTime |
            Format-Table | Out-String | Write-Host
        $lastMd = Get-ChildItem (Join-Path $sessionsDir '*.md') -ErrorAction SilentlyContinue |
                    Sort-Object LastWriteTime -Descending | Select-Object -First 1
        if ($lastMd) {
            Write-Host "----- $($lastMd.Name) (head) -----" -ForegroundColor Yellow
            Get-Content $lastMd.FullName -TotalCount 80 | Write-Host
            Write-Host "----- end -----" -ForegroundColor Yellow
        }
    } else {
        Write-Warning "no /sessions directory was written - check $logPath"
    }
}
finally {
    if (-not $KeepServer) {
        Stop-PoirotServer
    } else {
        $kid = $srv.Id
        Write-Step "server left running on pid=$kid (Stop-Process -Id $kid to kill)"
    }
}
