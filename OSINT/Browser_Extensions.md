# Browser Extensions for OSINT

Firefox/Chrome add-ons used in OSINT investigations — most ship with the [[Distros#TraceLabs OSINT VM|TraceLabs OSINT VM]] but each is independently installable.

> [!CAUTION]
> Install extensions only in your **investigation profile / VM**, never in your daily-driver browser. Each extension is another chunk of fingerprint and another potential exfil channel.

---

## Capture & Note-Taking

| Extension | Browser | Purpose |
|:----------|:--------|:--------|
| **Hunchly** (commercial) | Firefox/Chrome | Auto-captures every page you visit during a case — full HTML, screenshot, hash, timestamp. Industry standard for evidence chain |
| **Save Page WE** | Firefox/Chrome | One-click full-page archive (HTML + assets) |
| **FireShot** | Firefox/Chrome | Full-page screenshots (entire scrollable page) |
| **GoFullPage** | Chrome | Full-page screenshot, simple |
| **Nimbus Capture** | Firefox/Chrome | Screenshots + screen recording |
| **Page Screenshot to PDF** | Firefox | Quick export to PDF |
| **SingleFile** | Firefox/Chrome | Save complete page as a single HTML file |

---

## Reverse Image Search & Visual

| Extension | Browser | Purpose |
|:----------|:--------|:--------|
| **RevEye Reverse Image Search** | Firefox/Chrome | Right-click → search Google / Yandex / TinEye / Bing simultaneously |
| **Search by Image** | Firefox/Chrome | Same idea, more engines (incl. Yandex which is best for faces) |
| **TinEye Reverse Image Search** | Firefox/Chrome | Direct TinEye integration |
| **Image Search Options** | Firefox | Configurable multi-engine reverse image search |

---

## Archive & Cache Recovery

| Extension | Browser | Purpose |
|:----------|:--------|:--------|
| **Wayback Machine** | Firefox/Chrome | One-click archive lookup + "save now" for current page |
| **Resurrect Pages** | Firefox | Pulls cached versions from Wayback, Google cache, archive.today |
| **Web Archives** | Firefox/Chrome | Multi-source archive lookup (Wayback, archive.today, Yandex, Bing cache) |

---

## IOC / Threat-Intel Pivoting

| Extension | Browser | Purpose |
|:----------|:--------|:--------|
| **Mitaka** | Firefox/Chrome | Right-click any IOC (IP/domain/hash/email) → query VirusTotal, Shodan, urlscan, etc. |
| **CyberChef** browser | n/a | Decode/encode pipelines (web app, not extension) |
| **ThreatPinch Lookup** | Chrome | Hover over IOC for a tooltip with reputation data |

---

## Identity / Account Enumeration

| Extension | Browser | Purpose |
|:----------|:--------|:--------|
| **Vortimo OSINT** | Chrome | Right-click an entity → enrich from configured OSINT sources |
| **OSINT.industries Search** | Firefox/Chrome | Quick selector lookup |
| **SputnikChrome** | Chrome | Multi-engine OSINT pivot menu |

---

## Privacy / Anti-Tracking (REQUIRED on investigation profile)

| Extension | Browser | Purpose |
|:----------|:--------|:--------|
| **uBlock Origin** | Firefox/Chrome | Ad/tracker blocking — also reduces noise on investigation pages |
| **Privacy Badger** | Firefox/Chrome | EFF's auto-learning tracker blocker |
| **NoScript** | Firefox | Granular JS allow/deny — stops most fingerprinting on first visit |
| **CanvasBlocker** | Firefox | Spoof canvas/WebGL/font/audio fingerprints |
| **Cookie AutoDelete** | Firefox/Chrome | Wipe cookies when tab closes |
| **Decentraleyes** | Firefox/Chrome | Local CDN replacement — stops Google/Cloudflare seeing every page |
| **HTTPS Everywhere** | Firefox/Chrome | Force HTTPS where available (built into modern Firefox now) |

---

## Identity Compartmentalization (Firefox-specific)

| Extension | Purpose |
|:----------|:--------|
| **Firefox Multi-Account Containers** | Separate cookie jars per "container" (Personal, Work, Investigation-A, Sock-Puppet-1) — single browser, multiple identities |
| **Temporary Containers** | Spin up a throwaway container per tab, auto-destroyed on close |
| **Cookie Quick Manager** | Inspect / edit / export cookies for the current site |
| **User-Agent Switcher and Manager** | Spoof UA per site/container — defeats simple bot/region filters |

---

## Translation & Language

| Extension | Purpose |
|:----------|:--------|
| **Mate Translate** | Inline translation, supports many languages incl. RU, ZH, AR — useful for [[Regional_RUNet]] / [[Regional_China]] / [[Regional_Arabic]] |
| **DeepL Translator** | Higher-quality EU-language translation |
| **Yandex Translate** browser | Best Russian translation, less reliable for everything else |
| **Google Translate** | Default fallback |

---

## Geolocation

| Extension | Purpose |
|:----------|:--------|
| **What3Words** | Decode `///three.word.codes` to lat/lon |
| **Distance Measurement Tool** (on Google Maps) | Range estimates from photos |
| **Map Switcher** | Open the same coordinates in Google / Yandex / Bing / OSM in one click — Yandex maps cover RU/CIS far better than Google |

---

## Social Media Specific

| Extension | Purpose |
|:----------|:--------|
| **Treeverse** | Visualise Twitter/X conversation trees |
| **Twitter Web Exporter** | Export tweets/lists/likes (when API access is gone) |
| **Web Scraper** | Generic browser-side scraper for sites without APIs |

---

## Recommended Minimal Stack (start here)

If you don't want to install everything, this minimal set covers most cases:

1. **Firefox Multi-Account Containers** + **Temporary Containers** — compartmentalisation
2. **uBlock Origin** + **CanvasBlocker** + **NoScript** — privacy
3. **Hunchly** *(or Save Page WE if you don't have a Hunchly licence)* — capture
4. **Wayback Machine** — archive recovery
5. **RevEye** or **Search by Image** — reverse image
6. **Mitaka** — IOC pivoting
7. **Mate Translate** — language coverage
8. **Map Switcher** — geolocation across providers

---

## See Also

- [[OSINT]] — Folder index and methodology
- [[Distros]] — Distros that ship with these pre-configured
- [[VMs_and_Compartmentalization]] — Why your investigation browser needs to be isolated
- [[Tools_Kali_Tracelabs]] — Command-line counterparts (Sherlock, Maigret, Holehe, etc.)
