# Commercial OSINT / Investigator Tools

Paid software, vetted-access aggregators, and commercial threat-intelligence platforms — the **gated end** of investigation tradecraft.

Sister page: [[OSS_Tools]] for free / open-source equivalents.

> [!IMPORTANT]
> Many tools below require **professional licensing** (PI licence, attorney bar, LEO badge) or **vetted-customer status** (KYC for sanctions / fraud / threat-intel applications). Vendors verify; falsifying credentials triggers civil and criminal cases. The compliance regimes most commonly invoked: **FCRA, GLBA, DPPA** (US); **GDPR Art. 6 / Art. 9** + national exemptions (EU/UK).

---

## Link Analysis (Paid Tiers)

### Maltego — Paid Tiers
The de-facto OSINT graph tool. Free CE tier exists ([[OSS_Tools#Maltego CE]]); the paid tiers unlock the analyst-grade workflow.

| Edition | Cost (approx 2026) | Notes |
|:--------|:-------------------|:------|
| **Maltego Classic** | ~$1,200 / yr | Full transform set, no result expiry, larger graphs |
| **Maltego XL** | ~$2,400 / yr | 10,000+ entities per graph, enterprise viz |
| **Maltego Pro / ITDS** | Enterprise quote | Threat-intel platform integration, audit logs, multi-user |
| **Hub Items** | Per-transform | Paid third-party transform packs (ShadowDragon, Pipl, OCCRP Aleph, etc.) |

### IBM i2 Analyst's Notebook
Heavy enterprise / LEO favourite. Less OSINT-extensive than Maltego, deeper case-management. **i2 Connect** integrates external data sources.
- Interactive timeline, geospatial, social-network analysis
- Used by virtually every major LEO agency in the West
- Licensing through IBM, expensive (≥$5k/seat)

### Palantir Gotham / Foundry
Enterprise / government link analysis at scale. Deeply integrated with classified and operational systems — typically not available to solo investigators. Worth knowing about because case studies frequently reference it.

### Sentinel Visualizer
Lightweight Maltego/i2 alternative — link analysis + temporal + geospatial. Niche but appears in private-investigation / corporate-fraud workflows.

### ShadowDragon (SocialNet, Maltego transforms)
Visual link analysis with strong social-media and dark-web pivoting. Deep Maltego integration. Used in threat intelligence, missing persons, fraud.

---

## Gated Public-Records Aggregators

These ingest credit-header data, voter rolls, court records, deed transfers, vehicle registrations, etc. **Not free** — require professional credentials + signed compliance attestations.

### TLO / TransUnion TLOxp
- **Access:** US PIs, attorneys, LEO; vetted commercial fraud
- **Strengths:** Address history, relatives, vehicles, bankruptcies, civil cases, AKAs
- **Cost:** Per-search or subscription tier

### LexisNexis Accurint
- **Access:** Same gating as TLO
- **Strengths:** Person/business search, real-property holdings, asset locator, criminal records, professional licences
- **Variants:** Accurint for Investigations (PI), Accurint for Law Enforcement (LEO), Accurint for Legal Professionals
- Typically the strongest on **business affiliation** and **professional licence** linkage

### IDI Core / Cognyte LIRX
- **Access:** Vetted
- **Strengths:** Mobile phone analytics, TLO-comparable people search
- **Differentiator:** Mobile-phone metadata access

### CLEAR (Thomson Reuters)
- **Access:** Vetted
- **Strengths:** Litigation history, sanctions / PEP screening, deep court coverage
- **Usage:** Heavy in compliance / KYC / regulated-industry due diligence

### IRBSearch
- **Access:** PI / LEO
- **Strengths:** Comparable to TLO, often lower cost for similar feature set

### LocatePLUS
- **Access:** PI / LEO
- **Strengths:** Skip-tracing focus

### Pipl Pro
- **Access:** Vetted business
- **Strengths:** Deep-web identity records, **international scope** (less US-centric than the above)

### Skopenow
- **Access:** Lower bar than TLO/Accurint, paid
- **Strengths:** Social-media-focused, automated reports
- **Usage:** Insurance investigations, employee due diligence, modern PI work

### Babel Street Babel X
- **Access:** Vetted government / enterprise
- **Strengths:** **Multi-language social-media monitoring** (180+ languages) — deeper non-English coverage than most competitors
- **Usage:** National-security analysis, crisis monitoring

### IDIQ / SentryLink / Checkr
- **Access:** Various (FCRA-regulated for employment use)
- **Strengths:** Background-check workflow tools

---

## Threat-Intelligence Platforms

Used for cyber-threat OSINT — adversary tracking, dark-web monitoring, brand protection.

| Platform | Strength |
|:---------|:---------|
| **Recorded Future** | Largest TI platform; integrated dark-web/breach/IOC data, relationship graph |
| **Mandiant Advantage** (Google) | Adversary intelligence, especially nation-state APT |
| **Flashpoint** | Dark-web monitoring, illicit communities, credential theft |
| **Intel471** | Underground-actor tracking, cybercrime focus |
| **DarkOwl** | Dark-web data lake — more raw than aggregated |
| **CrowdStrike Falcon Intelligence** | Endpoint-tied TI, adversary profiles |
| **Anomali** | TI platform / aggregator |
| **ZeroFox** | Brand protection, executive protection, social-media monitoring |
| **Echosec** | Geo-tagged social media |
| **Bluedot** | Public-health / outbreak OSINT |
| **Janes** | Defence / military OSINT (deep tradition, century-old) |

---

## Capture / Evidence Chain

### Hunchly
**Industry-standard for OSINT evidence chains.** Auto-captures every page visited during a case — full HTML, screenshot, hash, timestamp. Generates court-admissible evidence packages.
- Cost: ~$130/yr individual, enterprise tiers available
- Standard analyst spend; if you can afford one paid tool, this is it
- Browser extension (Firefox/Chrome) — see [[Browser_Extensions#Capture & Note-Taking]]

---

## Beneficial Ownership / Corporate Networks

| Tool | Strength |
|:-----|:---------|
| **Sayari Graph** | Cross-jurisdiction BO graph, especially good for **opaque** jurisdictions (BVI, Cayman, UAE free zones, China) |
| **OpenCorporates Pro** | Aggregates 200+ jurisdictions; free tier for casual lookup, paid for bulk/API |
| **OpenSource.io** | Corporate-network mapping with sanctions overlay |
| **Quantexa** | Financial-crime entity resolution at bank scale |
| **Kharon** | Sanctions / PEP screening with corporate networks |
| **Castellum.AI** | Sanctions / regulatory monitoring |
| **C4ADS Sayari Sentry** | Sanctions evasion specifically |
| **DueDil / Artesian** | UK + EU corporate intelligence |
| **Beauhurst** | UK startup / scale-up tracking |

---

## Mobile / Digital Forensics (OSINT-adjacent)

Forensics rather than pure OSINT, but investigators frequently need both. All require vetted access in most jurisdictions.

| Tool | Use |
|:-----|:----|
| **Cellebrite UFED / Inseyets** | Mobile-device forensics. Vetted LEO/government primarily |
| **Magnet AXIOM / Forensic** | Comprehensive mobile + cloud forensics |
| **MSAB XRY** | Mobile forensics |
| **Oxygen Forensic Detective** | Mobile + cloud forensics |
| **Belkasoft Evidence Center** | Cloud / device forensics |
| **GrayKey / GrayShift** | iOS unlock — heavily restricted |

---

## Vehicle / Infrastructure Tracking (Commercial Tiers)

| Tool | Use |
|:-----|:----|
| **Lloyd's List Intelligence** | Maritime ownership chains, sanctions tracking — gold standard |
| **MarineTraffic** (paid tiers) | Historical AIS data |
| **FlightRadar24 Pro / Business** | Historical flight data |
| **PenLink / GrayKey** | Telecom-records analysis (LEO) |
| **Vigilant Solutions / Motorola Plate Hunter** | License-plate recognition (LEO / repo industry) — ethically contested |
| **Carfax / AutoCheck (commercial)** | Vehicle-history bulk |

---

## Photo / Face / Image-AI (Paid)

| Tool | Use |
|:-----|:----|
| **PimEyes** | Face-only reverse search — very effective, $30+/mo individual, vetted enterprise tiers |
| **Clearview AI** | LEO/government only — facial-recognition database scraped from social media (heavily contested legally) |
| **GeoSpy** ([geospy.ai](https://geospy.ai/)) | AI-driven photo geolocation |
| **Picarta** | AI photo geolocation |
| **FaceCheck.id** | Consumer face search (limited free tier) |

---

## Specialty / Niche Commercial

| Tool | Use |
|:-----|:----|
| **Brand24, Mention, Talkwalker, Pulsar** | Brand-monitoring (also OSINT-lite for selectors) |
| **Liferaft Navigator** | Threat / executive protection |
| **OSINT.industries** | Selector-pivoting service, paid (generous free tier) |
| **SpiderFoot HX** ([spiderfoot.net](https://www.spiderfoot.net/)) | Hosted, paid version of the OSS scanner |
| **Ahmia Pro / DarkSearch Pro** | Dark-web search with subscription features |

---

## Toolkit Recipes (commercial combinations)

### Brand-protection / executive protection
ZeroFox or Liferaft + commercial dark-web feed (DarkOwl / Recorded Future) + Hunchly + Maltego Classic

### Corporate due diligence
Sayari Graph + OpenCorporates Pro + LexisNexis Accurint (US) or Companies House (UK) + sanctions screening (Kharon / Castellum)

### Cybercrime threat intelligence
Recorded Future + Intel471 + Flashpoint + Maltego Pro + Hunchly + Whonix VM ([[Darkweb_Forums]] methodology)

### Skip-tracing (PI)
TLO / Accurint + IDI + Skopenow + court records + commercial people-search

### National-security / multi-language
Babel X + Recorded Future + ShadowDragon SocialNet + Maltego XL

---

## Cost Reality Check

For solo investigators / small teams, a workable commercial stack:
- **Hunchly** (~$130/yr) — non-negotiable
- **Maltego Classic** (~$1,200/yr) — link analysis
- **OpenCorporates Pro** (~$500-1500/yr depending on use) — corporate
- **Skopenow** or **Pipl Pro** (subscription) — people-search

Total: ~$3-5k/yr to be functional at a professional level. The big PI databases (TLO, Accurint, CLEAR) are typically gated through agency / firm employment rather than individual purchase.

---

## See Also

- [[OSS_Tools]] — Free / open-source counterparts; many tasks don't need commercial tools
- [[Tools_Kali_Tracelabs]] — Free CLI / GUI shipped with Kali / TraceLabs
- [[Browser_Extensions]] — Hunchly browser extension and other capture tools
- [[Distros]] — Where most OSS counterparts come pre-installed
- [[Social_Media_APIs]] — Many platforms above operate at API scale
- [[OSINT]] — Folder index
