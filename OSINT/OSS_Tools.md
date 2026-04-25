# Open-Source / Free OSINT Tools

Free-as-in-libre and free-as-in-beer tools that get the job done without subscriptions. Sister page: [[Commercial_Tools]] for paid / vetted-access counterparts.

This page is organised **by tool category** (link analysis, automation framework, selector pivoting, etc.) — for tools organised by **selector type** (email, username, domain, etc.) see [[Tools_Kali_Tracelabs]].

---

## Link Analysis (Free Tiers)

### Maltego CE
The free tier of Maltego — limited transforms, 12-hour result expiry, but still the most accessible visual link-analysis tool.

| Property | Detail |
|:---------|:-------|
| Cost | Free (account required) |
| Source | Closed source (free-as-in-beer) |
| Pre-installed on | [[Distros#TraceLabs OSINT VM\|TraceLabs OSINT VM]], [[Distros#Kali Linux\|Kali]], [[Parrot_OS\|Parrot Security]] |
| Limits | Transform results expire after 12 hours; max ~12 entities per transform; 3rd-party transforms require Hub Items (some free) |

```bash
maltego        # GUI launch

# Common CE transforms:
# - Domain → DNS records, MX, NS
# - Email → social profiles, breaches (limited)
# - Person → related entities, social, employment
# - PhoneNumber → carrier, location guess
# - Document → metadata extraction
```

### OSINT-Combine Free Tools / Web-based
Browser-based link visualisations and OSINT workflows. Not as capable as Maltego but nothing to install.

### Gephi
General-purpose graph visualisation. Less OSINT-specific but widely used to render Maltego exports or custom selector-graphs.
- [gephi.org](https://gephi.org/)
- Open source, GPL

### Cytoscape
Bioinformatics-origin graph tool, sometimes used in OSINT for large-scale relationship mapping.

---

## Automation Frameworks

### SpiderFoot OSS
Free, self-hostable OSINT automation. Plugins for ~200 sources. Hosted commercial version exists ([[Commercial_Tools#Specialty / Niche Commercial|SpiderFoot HX]]).

```bash
# Install (also pre-installed on TraceLabs / Kali)
git clone https://github.com/smicallef/spiderfoot.git
cd spiderfoot
pip3 install -r requirements.txt
python3 sf.py -l 127.0.0.1:5001    # web UI
```

| Module category | Examples |
|:----------------|:---------|
| Email/breach | HIBP, EmailRep, Holehe |
| Domain/IP | Shodan, Censys, VirusTotal, urlscan |
| Social | Sherlock, Twitter (limited), GitHub |
| Threat | AbuseIPDB, MalwareBazaar |
| Dark | Tor onion-list scanning (with care) |

### Recon-ng
Modular framework, console-driven (think msfconsole for OSINT).

```bash
recon-ng
> marketplace install all
> workspaces create acme
> modules load recon/domains-hosts/hackertarget
> options set SOURCE example.com
> run
```

- API-key-driven for many modules — set up `.recon-ng/keys.db` first
- Saves to local SQLite, easy to export/pivot

### theHarvester
Email + subdomain harvesting from public search engines and OSINT sources.
```bash
theHarvester -d example.com -b google,bing,crtsh,duckduckgo,dnsdumpster
```

### Photon
Crawler that extracts emails, URLs, secrets, social-media handles from a site.
```bash
git clone https://github.com/s0md3v/Photon.git
python3 photon.py -u https://target.com -l 3 -t 100
```

### Datasploit (legacy but still used)
Pre-built recon pipelines.

### OSINT-SAN
Multi-tool aggregator with menu-driven interface (Russian-origin).

---

## Selector Pivoting

> Cross-reference with [[Tools_Kali_Tracelabs]] which is organised by selector.

### Username — Sherlock / Maigret / WhatsMyName
- **Sherlock** — fastest, ~400 sites checked
- **Maigret** — Sherlock-fork with 3000+ sites and metadata extraction
- **WhatsMyName** — curated, fewer false positives ([whatsmyname.app](https://whatsmyname.app/))
- **NameCheckr / Namechk** — web-based, faster casual check

### Email — Holehe / EmailRep / h8mail
- **Holehe** — checks 100+ sites without sending password resets
- **EmailRep** ([emailrep.io](https://emailrep.io/)) — reputation, age, social presence
- **h8mail** — multi-source breach lookup (API keys required)
- **HaveIBeenPwned** ([haveibeenpwned.com](https://haveibeenpwned.com/)) — free tier; account-side enrichment via API

### Phone — PhoneInfoga
```bash
phoneinfoga scan -n "+48123456789"
```

### Domain — Amass / Sublist3r / DNSenum
- **Amass** — largest passive subdomain database
- **Sublist3r** — quick passive enum
- **DNSenum / DNSrecon** — combined passive + active

---

## Image & Document

### ExifTool
The single most useful free OSINT tool. Reads metadata from almost any file.
```bash
exiftool image.jpg                      # show
exiftool -all= image.jpg                # strip
exiftool -gpslatitude -gpslongitude image.jpg
exiftool -r /folder/ > metadata.txt     # bulk
```

### MAT2 (Metadata Anonymisation Toolkit 2)
Strip metadata before publishing.
```bash
mat2 photo.jpg
mat2 --show photo.jpg   # inspect without writing
```

### Metagoofil
Searches Google/Bing for documents on a domain, downloads them, runs ExifTool. (Note: Google-search-side increasingly fragile.)

### FOCA
Windows GUI metadata-extraction tool — same idea as Metagoofil with richer parsing.

### InVID-WeVerify
Browser plugin for video frame extraction + reverse search + manipulation detection. Free, vital for [[Geolocation]] work.

### FotoForensics
Web tool — Error-Level Analysis (ELA), JPEG quality, hidden-pixel inspection.
- [fotoforensics.com](https://fotoforensics.com/)

---

## Capture & Note-Taking (free-tier alternatives to Hunchly)

> Hunchly is the standard, but it's commercial (~$130/yr). Free alternatives don't quite match it for evidence chain, but cover a lot:

| Tool | Use |
|:-----|:----|
| **Save Page WE** (browser ext) | Single-page archive (HTML + assets) |
| **SingleFile** | Save page as one self-contained HTML file |
| **GoFullPage / FireShot** | Full-page screenshots |
| **archive.today** + **Wayback Machine** | Public-archive snapshots |
| **CherryTree / Joplin / Obsidian** | Note-taking with screenshot linking |
| **OBS Studio** | Screen recording for video evidence |

For evidence-chain work where chain-of-custody matters, **Hunchly** ([[Commercial_Tools#Hunchly]]) is hard to replace.

---

## Threat Intel (Free Tier)

| Source | Use |
|:-------|:----|
| **VirusTotal** | File / URL / domain reputation; free tier limited |
| **urlscan.io** | Sandbox-render any URL; free public scans |
| **AbuseIPDB** | IP reputation, free API |
| **GreyNoise** | Free tier filters internet noise |
| **AlienVault OTX** | Open Threat Exchange — community-driven |
| **MISP** | Open-source threat-sharing platform; self-host |
| **MalwareBazaar / ThreatFox** | abuse.ch projects; free, API-driven |
| **CIRCL Passive DNS** | Free pDNS for incident response |
| **Yara-rules / Sigma-rules** | Open detection-rule corpora |

---

## Dark-Web (free)

> See [[Darkweb_Forums]] for methodology and OPSEC. Free tooling subset:

| Tool | Use |
|:-----|:----|
| **Tor Browser** | Standard entry, single-session |
| **OnionScan** (legacy) | Static analysis of `.onion` services for misconfig |
| **Ahmia** | CSAM-filtered Tor search engine |
| **OnionLand Search** | Tor index |
| **DarkSearch** / **Onion Search Engine** | General Tor search engines |

---

## Geolocation (free)

> See [[Geolocation]] for full methodology.

| Tool | Use |
|:-----|:----|
| **Sentinel Hub / Copernicus EO Browser** | Free European satellite imagery |
| **NASA Worldview** | Daily satellite imagery, fires, weather |
| **USGS EarthExplorer** | Free Landsat back to 1972 |
| **ADS-B Exchange** | Uncensored aircraft tracking |
| **OpenSky Network** | Academic ADS-B free historical |
| **OpenCellID** | Cell-tower locations worldwide |
| **Mapillary** / **KartaView** | Crowdsourced street imagery |
| **SunCalc** ([suncalc.org](https://www.suncalc.org/)) | Sun-position / shadow direction |
| **OGIMET** | METAR weather archive |
| **Time and Date** | Historical weather |

---

## Curated Tool-Launchers / Workbooks

These aren't tools themselves, but **directories** of vetted OSINT tools that save hours of "what should I use for X?":

- **OSINT Framework** ([osintframework.com](https://osintframework.com/)) — categorised tool tree, the canonical starting point
- **IntelTechniques Tools** ([inteltechniques.com/tools](https://inteltechniques.com/tools/)) — Michael Bazzell's free web tools and IntelTechniques OSINT Workbook
- **Bellingcat Online Investigation Toolkit** ([bellingcat.com/resources](https://www.bellingcat.com/resources/)) — curated by working investigators, updated regularly
- **OSINT Combine** ([osintcombine.com/tools](https://www.osintcombine.com/tools)) — workflow-oriented free tools
- **Awesome OSINT** ([github.com/jivoi/awesome-osint](https://github.com/jivoi/awesome-osint)) — community-maintained list
- **OSINT Industries Free Tier** ([osint.industries](https://osint.industries/)) — selector-pivoting with generous free tier
- **WhatsMyName** ([whatsmyname.app](https://whatsmyname.app/)) — username search, browser-based
- **EpieOS** — selector-pivoting alternative to OSINT.industries

---

## OSS Detective Tools (community projects)

| Tool | Use |
|:-----|:----|
| **GHunt** | Email → Google account profile (display name, photo, public Maps reviews) |
| **Toutatis** | Instagram selector hints (obfuscated email/phone) |
| **Osintgram** | Instagram OSINT (login required) |
| **Instaloader** | Instagram public-profile dump |
| **Telethon** / **tg-archive** | Telegram public-channel archive |
| **snscrape** | Multi-platform social scraper (Twitter dead, others work) |
| **twscrape** | Auth-required Twitter scraper (current best-effort) |
| **NoseyParker** | Find secrets in repos / archives |
| **Trufflehog** | Same idea, simpler |
| **Gitleaks** | Faster secret-scan |
| **GitDorker** | GitHub dorking automation |
| **EmailFinder / Mosint** | Email recon orchestrators |
| **Spiderpig** / **BlackBird** | Username enum alternatives |
| **Skiptracer** | Skip-tracing OSS — modest coverage vs commercial |

---

## Bulk Capture / Archive

| Tool | Use |
|:-----|:----|
| **wget / curl** | Standard mirroring |
| **HTTrack** | Site mirroring with crawler |
| **yt-dlp** | Video / audio download from 1000+ sites — gold for evidence preservation |
| **Heritrix** (Wayback's crawler) | Archive-grade crawling |
| **wpull** | Modern wget alternative |
| **gallery-dl** | Image-gallery / social-media album downloader |

---

## Why use OSS instead of Commercial?

- **Reproducibility** — open-source tools can be audited; results are deterministic
- **No vendor lock-in** — your workflow doesn't break when a vendor changes pricing or shuts down (cf. Pushshift, CrowdTangle, Twint)
- **Legally cleaner** for many use cases — no EULA constraint on output sharing
- **Air-gapped / classified work** — many commercial tools require cloud access; OSS often runs offline
- **Education and CTF** — TraceLabs CTFs explicitly use the OSS toolkit

## When to escalate to Commercial?

- **Time pressure** — commercial tools save hours per case
- **Evidence chain matters** — Hunchly + paid Maltego have audit-trail features OSS lacks
- **Coverage gap** — gated PI databases ([[Commercial_Tools#Gated Public-Records Aggregators]]) genuinely have data OSS can't reach
- **Sanctions / corporate due diligence at scale** — Sayari, OpenCorporates Pro, LexisNexis cover what OSS can't
- **Mobile / device forensics** — Cellebrite et al. have no real OSS equivalent

A working analyst stack typically uses **OSS for 80% of the work** and commercial tools for the high-value 20% where they pay for themselves.

---

## See Also

- [[Commercial_Tools]] — Paid / vetted-access counterparts
- [[Tools_Kali_Tracelabs]] — Selector-organised view of the same tooling
- [[Browser_Extensions]] — In-browser counterparts
- [[Distros]] — Where most of these come pre-installed
- [[Parrot_OS]] — Privacy-focused distro with extra OSS anonymity tools
- [[Geolocation]] — Geolocation-specific OSS tools
- [[Social_Media_APIs]] — API constraints behind the scraping tools
- [[OSINT]] — Folder index
