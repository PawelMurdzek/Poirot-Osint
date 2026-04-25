# Country OSINT — North Korea (DPRK / 조선민주주의인민공화국)

The world's most closed information environment. Almost no inside-the-country OSINT is possible directly; instead, the methodology is **outside-in**: state propaganda outputs + diaspora/defector sourcing + satellite imagery + cross-jurisdiction sanctions / financial trails.

> [!IMPORTANT]
> Direct contact with North Korean systems or nationals can have severe legal implications. Many countries' sanctions regimes (US OFAC, EU, UN) prohibit material support, financial transactions, or even certain types of information exchange with the DPRK. **Get legal review before any active engagement.**

---

## What Doesn't Exist

For context — much of what we'd use elsewhere is unavailable for the DPRK:

- No public corporate registry
- No public court records
- No social media accounts for ordinary citizens (no internet access)
- No domestic news independent of state
- No commercial flight tracking (limited DPRK airspace data)
- No usable phone numbers / SIM penetration data
- No diaspora-platform engagement from inside the country (with rare exceptions)

The DPRK has a **domestic intranet ("Kwangmyong" / 광명)** isolated from the global internet. Only ~2000 IP addresses are allocated to DPRK; most are within `175.45.176.0/22`.

---

## Outputs from the State

### State news / propaganda outlets
- **KCNA (Korean Central News Agency)** ([kcna.kp](http://www.kcna.kp/)) — official state news, English mirror at `kcnawatch.org`
- **Rodong Sinmun (로동신문)** ([rodong.rep.kp](http://www.rodong.rep.kp/)) — Workers' Party daily
- **Voice of Korea / Pyongyang Times** — outward-facing propaganda
- **Naenara** ([naenara.com.kp](http://www.naenara.com.kp/)) — official portal
- **Uriminzokkiri** ([uriminzokkiri.com](http://www.uriminzokkiri.com/)) — propaganda for South Korean / overseas Korean audience

### Aggregators (essential — preserve content that gets quietly edited/deleted)
- **KCNA Watch** ([kcnawatch.org](https://kcnawatch.org/)) — English-language KCNA archive, run by NK News
- **NK News** ([nknews.org](https://www.nknews.org/)) — paid daily news / analysis (Pro tier for analysts)
- **NK Pro** — premium analyst tier of NK News
- **Daily NK** ([dailynk.com](https://www.dailynk.com/)) — Seoul-based, sources inside the country (smuggled phones)
- **38 North** ([38north.org](https://www.38north.org/)) — analyst-driven (Stimson Center)

---

## Satellite Imagery — The Core Methodology

For DPRK, **commercial satellite imagery is the workhorse**. Major analyst projects:

- **Joseph S. Bermudez Jr.** (CSIS Beyond Parallel, formerly AllSource Analysis) — paywalled, gold-standard
- **Jacob Bogle / Access DPRK** ([accessdprk.com](https://accessdprk.com/)) — open mapping project, hand-annotated Google Earth maps of DPRK military / industrial sites
- **38 North Imagery** — Stimson Center analyses
- **Open Nuclear Network** — nuclear-monitoring focus
- **Planet Labs / Maxar / Capella Space / ICEYE** — commercial imagery providers; some have Twitter/X "snapshot" public posts of DPRK incidents
- **Sentinel Hub / Copernicus** — free European Space Agency imagery, lower-resolution

### Workflow
```
1. KCNA / Rodong claims an event (e.g., missile test, parade)
2. Geolocate the announced location (often deliberately vague)
3. Pull commercial imagery for date+1 to date+7
4. Compare to baseline imagery (year-over-year)
5. Cross-reference with seismic monitoring (USGS, KIGAM), AIS port traffic, ADS-B for closures
```

---

## Defector & Source-Inside Reporting

Three Seoul-based outlets have networks of sources inside DPRK who use smuggled Chinese phones along the border:

- **Daily NK** — fastest reporting on regime power dynamics, food crises, market prices
- **Rimjin-gang** ([rimjingang.com](https://www.rimjin-gang.com/)) — covert journalist network, sometimes publishes raw video
- **Asia Press** — Japan-based, Rimjin-gang's parent

**Defector testimonies** — collected by:
- **Committee for Human Rights in North Korea (HRNK)** ([hrnk.org](https://www.hrnk.org/))
- **Database Center for North Korean Human Rights (NKDB)**
- **TJWG (Transitional Justice Working Group)**
- **UN Commission of Inquiry report** (2014) — single most important corpus of defector testimony

> Defector testimony is biased by selection (those who escaped) and incentive (paid by some outlets / governments). Cross-corroboration matters.

---

## Sanctions, Finance, Cyber

DPRK is heavily sanctioned. OSINT often involves chasing **sanctions evasion**:

| Vector | Key OSINT sources |
|:-------|:------------------|
| **Maritime sanctions evasion** | UN Panel of Experts reports, MarineTraffic AIS, Lloyd's List Intelligence, NK News Pro tracking |
| **Crypto theft (Lazarus Group, APT38)** | Chainalysis, TRM Labs, SlowMist, Elliptic — DPRK is the largest state-actor crypto thief |
| **Sanctioned entities** | OFAC SDN list, EU sanctions map, UNSC consolidated list |
| **Front companies** | OpenCorporates + Companies House + Tianyancha cross-references reveal CN/RU/SE Asia DPRK shells |
| **Overseas IT workers** | FBI advisories on DPRK IT workers infiltrating Western companies as remote contractors — rich OSINT case studies |
| **Bunkering operations** | Open-source ship-tracking + UN reports on illegal STS transfers |

Notable analyst orgs:
- **Royal United Services Institute (RUSI) Project Sandstone** — sanctions evasion
- **C4ADS** — DPRK financial network mapping
- **Stimson 38 North** — broad analytic coverage
- **OFAC Public Designations + DOJ unsealed indictments** — golden source for shell-company identification

---

## Internet Activity Out of DPRK

Despite limited internet access, DPRK regime entities use the internet extensively:

- **DPRK official websites** — ~30 outward-facing sites, mostly in `175.45.176.0/22`
- **Lazarus / Andariel / Kimsuky** APT activity — extensive analyst literature (Mandiant, CrowdStrike, Kaspersky)
- **Diaspora-targeted "Uriminzokkiri" campaigns** — propaganda + occasionally phishing
- **DPRK Workers in China** — IP traffic from Shenyang / Dandong / Yanji often DPRK-linked

---

## Specifically NK OSINT Tools / Resources

- **NK Watch / NK Pro** — paid commercial analyst platform, the closest thing to a "Bloomberg Terminal for DPRK"
- **NK Leadership Watch** ([nkleadershipwatch.wordpress.com](https://nkleadershipwatch.wordpress.com/)) — Michael Madden's project, decades-long elite biographical tracking
- **DPRKLeaderWatch / TimeMagazine.com pieces** — Kim regime succession watching
- **CSIS Beyond Parallel** ([beyondparallel.csis.org](https://beyondparallel.csis.org/)) — multimedia analyst hub
- **One Free Korea** — long-running analyst blog on sanctions / human rights

---

## Reading List (essential to do this work seriously)

- **B.R. Myers, "The Cleanest Race"** — DPRK ideological framing
- **Andrei Lankov, "The Real North Korea"** — historical / political background
- **Jung H. Pak, "Becoming Kim Jong Un"** — leadership analysis
- **Joseph S. Bermudez Jr.'s monographs** — military OSINT methodology
- **UN Commission of Inquiry report (2014)** — human-rights baseline
- **All NK Pro analyst pieces by Chad O'Carroll, Colin Zwirko** — daily-life analysis

---

## OPSEC / Legal Notes

- **OFAC sanctions are extraterritorial** — even non-US persons risk secondary sanctions if they transact with DPRK-linked entities. OSINT collection is generally fine; *engagement* is not.
- **Importing/exporting** information about DPRK is sensitive in some jurisdictions (China, Russia have variable cooperation; ROK has the National Security Act).
- **Direct contact with DPRK servers** (probing, scraping) — most analysts avoid this. The handful of public sites are routinely scanned by researchers, but doing it yourself can be a legal/diplomatic problem.
- **Defector contact** — vetted only through established human-rights orgs. Direct contact risks outing sources still inside.

---

## See Also

- [[Country_SouthKorea]] — Most NK-focused tooling and journalism is Seoul-based
- [[Regional_China]] — DPRK-China border is the operational reality (sanctions, defectors, IT workers)
- [[Commercial_Tools]] — Maltego paid + sanctions-list integrations (Sayari, Castellum.AI, Kharon)
- [[OSINT]] — Folder index
