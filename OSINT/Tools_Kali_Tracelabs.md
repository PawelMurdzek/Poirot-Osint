# OSINT Tools — Kali / TraceLabs OSINT VM

CLI and GUI tools shipped (or one-line installable) on **Kali Linux** and the **TraceLabs OSINT VM**. Grouped by selector / use case.

> Many of these tools rely on third-party APIs that rate-limit or require keys. Always read the tool's README before running.

---

## Username Enumeration

Find every site where a given username has an account.

### Sherlock
```bash
# Pre-installed on TraceLabs OSINT VM, available via apt on Kali
sherlock <username>
sherlock <username> --timeout 5 --print-found

# Multiple usernames
sherlock user1 user2 user3
```

### Maigret
Sherlock's bigger sibling — checks 3000+ sites, also extracts profile metadata.
```bash
pip3 install maigret
maigret <username>
maigret <username> --html --pdf   # report formats
```

### WhatsMyName
Curated, less noisy than Sherlock. Web app + CLI.
- Web: [whatsmyname.app](https://whatsmyname.app/)

### Holehe
Check which sites have an account registered to a given email — without sending password resets.
```bash
pip3 install holehe
holehe target@example.com
```

---

## Email / Breach / Credential

| Tool | Purpose |
|:-----|:--------|
| **HaveIBeenPwned** (web) | Free breach lookup |
| **DeHashed** (commercial) | Plaintext credential search |
| **Hunter.io** | Email pattern guessing for company domains |
| **EmailRep.io** | Email reputation, age, social presence |
| `theHarvester` | Email/subdomain harvesting from search engines, see below |
| `h8mail` | Multi-source breach lookup via APIs |

```bash
# h8mail — uses Snusbase, Dehashed, Leak-Lookup, etc. (API keys required)
h8mail -t target@example.com
```

---

## Domain / Subdomain / DNS

> See [[DNS_Enumeration]] for the full DNS workflow. OSINT-specific subset:

| Tool | Purpose |
|:-----|:--------|
| `theHarvester` | Subdomains + emails from Google, Bing, crt.sh, DNSdumpster |
| `Amass` (`amass enum -passive`) | Largest passive subdomain database |
| `Sublist3r` | Quick passive subdomain enum |
| `Photon` | Web crawler that extracts emails, URLs, secrets |
| **crt.sh** (web) | Certificate transparency lookup |
| **SecurityTrails** | Historical DNS records |
| **Whoisology** | Historical WHOIS pivots |
| **DNSdumpster** | Visual subdomain map |

```bash
theHarvester -d example.com -b google,bing,crtsh,duckduckgo,dnsdumpster
amass enum -passive -d example.com -o subs.txt
sublist3r -d example.com -o subs.txt
```

---

## Search-Engine Recon Frameworks

### Recon-ng
Modular framework, MSF-like console.
```bash
recon-ng
> marketplace install all
> workspaces create acme
> modules load recon/domains-hosts/hackertarget
> options set SOURCE example.com
> run
```

### SpiderFoot
Automated, GUI-driven, 200+ modules.
```bash
spiderfoot -l 127.0.0.1:5001    # web UI
```

### Maltego CE
Visual link-analysis. Free Community Edition has limited transforms; commercial Classic / XL adds more.
```bash
maltego
```

---

## Social Media

> APIs for X/Twitter and Instagram have been heavily restricted since the 2023 closures and have only tightened through 2024-2026. Many old tools (Twint, Instaloader's anonymous mode, snscrape Twitter mode) are fully broken — check current status before relying on them.

| Tool | Platform | Status (as of 2026) |
|:-----|:---------|:-------------------|
| `snscrape` | Reddit, Mastodon, Telegram, VK, Weibo | Twitter mode dead; works on the others |
| `twscrape` | Twitter/X | Auth-required (sock-puppet rotation); current best-effort Twitter scraper |
| `instaloader` | Instagram | Public profiles only; logged-in scraping risks ban |
| `Osintgram` | Instagram | Works, requires login; ban risk |
| `Toutatis` | Instagram | Pulls obfuscated email/phone hints from public profiles |
| `tweepy` (with API key) | Twitter/X | Paid API only |
| `telethon` / `tg-archive` | Telegram | Works — Telegram's API remains open |
| `atproto` SDK | Bluesky | Public read endpoints work without auth |

### Quick examples
```bash
# Snscrape — Telegram channel
snscrape telegram-channel <channel_name>

# Instaloader — public profile
instaloader --no-pictures --no-videos --no-video-thumbnails --geotags --comments <username>

# Toutatis — leaked email/phone hints
toutatis -u <username> -s <session_id>
```

---

## Phone Numbers

```bash
# PhoneInfoga — number type, carrier, OSINT pivots
phoneinfoga scan -n "+48123456789"

# Web alternatives
# - sync.me
# - truecaller (regional, often paywalled)
# - emobiletracker.com
```

---

## Image / Document / Metadata

### ExifTool
The single most useful OSINT tool. Reads metadata from almost any file format.
```bash
# Basic
exiftool image.jpg

# Strip metadata before publishing
exiftool -all= image.jpg

# Bulk
exiftool -r /folder/of/images/ > metadata.txt

# Extract GPS coordinates only
exiftool -gpslatitude -gpslongitude -gpsdatetime image.jpg
```

### Metagoofil
Searches Google for documents on a domain, downloads them, runs ExifTool.
```bash
metagoofil -d example.com -t pdf,doc,xls,ppt -l 100 -n 25 -o results/
```

### FOCA (Windows GUI)
Same idea as Metagoofil but with a UI and richer parsing.

### Reverse image search workflow

```
1. Yandex Images       — best for FACES and Cyrillic-language sites
2. Google Images       — broadest index
3. TinEye              — best for finding the original / earliest version
4. Bing Visual Search  — sometimes catches what Google misses
5. PimEyes (paid)      — face-only commercial engine, very effective
```

---

## Geolocation

| Tool | Purpose |
|:-----|:--------|
| **SunCalc.org** | Verify time-of-day from shadow direction |
| **What3Words** | Decode `///word.word.word` references |
| **Mapillary** / **KartaView** | Crowdsourced street-view, covers areas Google doesn't |
| **Google Earth** | Historical imagery layer is huge |
| **Sentinel Hub / Copernicus** | Free recent satellite imagery |
| **ADS-B Exchange / Flightradar24** | Aircraft tracking |
| **MarineTraffic** | Ship tracking |

---

## Internet Infrastructure

| Tool | Purpose |
|:-----|:--------|
| **Shodan** | Internet-connected device search (free tier limited) |
| **Censys** | Same idea, often complementary |
| **GreyNoise** | Filter out background internet noise |
| **FOFA** (China) | Chinese counterpart to Shodan, covers RU/CN better |
| **ZoomEye** | Another China-based search engine for devices |
| **Onyphe** | French, with PCAP/leak data |
| **urlscan.io** | Sandbox-render any URL, get IOCs |
| **VirusTotal** | File / URL / domain reputation |

---

## Organisations / Corporate

| Source | Region |
|:-------|:-------|
| **OpenCorporates** | Global, free |
| **Companies House** | UK |
| **KRS** / **CEIDG** | Poland |
| **SEC EDGAR** | US public companies |
| **Tianyancha** / **Qichacha** | China — see [[Regional_China]] |
| **СПАРК** / **Контур.Фокус** | Russia — see [[Regional_RUNet]] |
| **OFAC SDN List** | US sanctions |
| **EU Sanctions Map** | EU sanctions |

---

## Code / Repo / Leak

```bash
# Trufflehog — find secrets in GitHub repos
trufflehog github --repo https://github.com/example/repo

# Gitleaks
gitleaks detect --source . -v

# Github-search
github-search -k <api_key> -q "company.com password"
```

| Tool | Purpose |
|:-----|:--------|
| **Trufflehog** | Find committed secrets in repos |
| **Gitleaks** | Same idea, faster |
| **GitHub code search** (web) | `org:target "AWS_SECRET"` |
| **Pastebin / Doxbin / Ghostbin** scrapers | Leaked data, dox |
| **GHunt** | OSINT on Google accounts (email → owner profile) |

---

## TraceLabs OSINT VM Specific

The [[Distros#TraceLabs OSINT VM|TraceLabs OSINT VM]] (built on Kali) ships with the above tools plus a curated set focused on **missing-persons CTFs**:
- **Browser bookmark folders** organised by selector type
- **Hunchly** pre-installed (licence required)
- **Maltego CE** pre-configured
- **Recon-ng** with marketplace pre-populated
- A **TraceLabs report template** for case write-ups

---

## See Also

- [[OSINT]] — Folder index and methodology
- [[Browser_Extensions]] — Browser-side counterparts to these CLI tools
- [[Distros]] — Where these tools come pre-installed
- [[DNS_Enumeration]] — Active follow-up on discovered domains
- [[Wordlists]] — Wordlists for username / subdomain brute-forcing
