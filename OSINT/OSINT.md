# OSINT — Open Source Intelligence

Top-level hub for open-source intelligence: passive recon, social-media analysis, geolocation, breach data, regional / dark-web sources, and detective / investigator tooling.

> **Scope:** OSINT is dual-use — used by red teams, threat intel, brand protection, missing-persons CTFs, journalists, and private investigators. This folder is structured around capability rather than role.

---

## Folder Layout

### Methodology & Tradecraft
| File | Purpose |
|:-----|:--------|
| [[OSINT]] | This file — index and methodology |
| [[VMs_and_Compartmentalization]] | Qubes / Whonix / VirtualBox setup, snapshot hygiene, sock-puppet identity, browser fingerprint hardening |
| [[Geolocation]] | Pinpointing photo / video locations — Bellingcat methodology, sun position, satellite imagery, reverse image search |
| [[Darkweb_Forums]] | Tor / I2P primer, forum taxonomy, threat-intel sourcing, OPSEC (educational, no live `.onion` URLs) |

### Tooling
| File | Purpose |
|:-----|:--------|
| [[Browser_Extensions]] | Firefox / Chrome extensions used by TraceLabs and analysts |
| [[Tools_Kali_Tracelabs]] | CLI / GUI tools shipped with Kali and TraceLabs OSINT VM — Sherlock, Maigret, theHarvester, Recon-ng, etc. |
| [[Commercial_Tools]] | Paid + vetted-access stack — Maltego (paid), i2, LexisNexis Accurint, TLO, Skopenow, ShadowDragon, threat-intel platforms |
| [[OSS_Tools]] | Free / open-source — Maltego CE, SpiderFoot OSS, Recon-ng, Sherlock, Trufflehog, curated tool-launchers |
| [[Social_Media_APIs]] | Extensive matrix: official APIs, scraping difficulty, account requirements, the locked-down landscape as of 2026, federated platforms (Mastodon, Bluesky, Lemmy) |
| [[Sock_Puppet_Recipes]] | Concrete platform-by-platform sock-puppet provisioning — persona building, network/email/phone, ageing, per-platform recipes |

### Operating Systems
| File | Purpose |
|:-----|:--------|
| [[Distros]] | TraceLabs OSINT VM, Kali, CSI Linux, Tsurugi, Whonix, Tails, Qubes, Parrot |
| [[Parrot_OS]] | Deep-dive on Parrot Security tools that don't ship with TraceLabs / Kali (AnonSurf, Pandora, MAT2, Ricochet) |

### Regional Ecosystems
| File | Coverage |
|:-----|:---------|
| [[Regional_RUNet]] | Russia / Belarus / Kazakhstan / wider CIS — VK, Yandex, OK, Telegram, RU corporate registries |
| [[Regional_China]] | Chinese-language internet — Baidu, Weibo, WeChat, Douyin, Tianyancha / Qichacha |
| [[Regional_Arabic]] | Arabic-language internet across MENA — country-by-country, conflict OSINT, MSA vs dialect |

### Country Deep-Dives

> Now organised under `regional/<region>/` subfolders. Wiki-links resolve by basename, so `[[Country_Poland]]` works regardless of which subfolder it sits in.

**`regional/europe/` (17 countries):**
| File | Country |
|:-----|:--------|
| [[Country_Ukraine]] | Most OSINT-active conflict zone in history — Bellingcat / DeepState / GeoConfirmed methodology |
| [[Country_Belarus]] | Distinct from RUNet — Cyber Partisans, NEXTA, exile media |
| [[Country_Poland]] | KRS / CEIDG / MSiG, Polish media, Wykop |
| [[Country_Germany]] | Handelsregister + Bundesanzeiger, strict privacy regime |
| [[Country_France]] | Pappers + Mediapart + francophone-Africa bridge |
| [[Country_Italy]] | Antimafia public registries, IRPI investigative |
| [[Country_Spain]] | Latin-America bridge, BORME, Civio |
| [[Country_Netherlands]] | KVK transparency, Bellingcat / Lighthouse Reports HQ |
| [[Country_UK]] | Companies House (the gold standard), Land Registry, BAILII |
| [[Country_Ireland]] | EU HQ for global tech (Meta / Google / X / TikTok), DPC / GDPR enforcement |
| [[Country_Greece]] | Predatorgate spyware deployment, Mediterranean migration |
| [[Country_Romania]] | Termene.ro, RISE Project, Hackerville cybercrime context |
| [[Country_Hungary]] | KESMA media consolidation, Direkt36, Pegasus deployment |
| [[Country_Czech]] | Justice.cz transparency, Investigace.cz, Russian-evasion context |
| [[Country_Sweden]] | Exceptional public-records access (income, vehicles, court) |
| [[Country_Denmark]] | CVR free transparency, Danske Bank case |
| [[Country_Switzerland]] | Banking secrecy + commodity-trading + sanctions evasion |

**`regional/middleeast/` (3 countries):**
| File | Country |
|:-----|:--------|
| [[Country_Iran]] | Persian-language ecosystem, Aparat, Eitaa, regime news vs exile |
| [[Country_Turkey]] | Eksi Sözlük, MERSIS, exile media post-2016 |
| [[Country_Israel]] | Hebrew + surveillance-tech industry, [[Regional_Arabic]] cross-reference |

**`regional/americas/` (9 countries):**
| File | Country |
|:-----|:--------|
| [[Country_USA]] | PACER, SEC EDGAR, 50 state ecosystems, gated PI tools |
| [[Country_Canada]] | Citizen Lab HQ, federal + provincial registries, PIPEDA |
| [[Country_Mexico]] | Narco-OSINT, INAI / FOIA, Pegasus deployment |
| [[Country_Brazil]] | WhatsApp dominance, CNPJ open registry, election OSINT |
| [[Country_Argentina]] | Capital flight, AMIA, vibrant investigative culture |
| [[Country_Colombia]] | RUES, narco / FARC / paramilitary OSINT, Connectas hub |
| [[Country_Chile]] | CIPER, lithium / mining, social-uprising aftermath |
| [[Country_Peru]] | Lava Jato Peru, mining-conflict OSINT |
| [[Country_Venezuela]] | Sanctions evasion, mass migration, exile-press ecosystem |

**`regional/asia/` (11 countries):**
| File | Country |
|:-----|:--------|
| [[Country_Japan]] | LINE, 5ch, Mixi, Niconico, Pixiv, Naver-Pawoo Mastodon |
| [[Country_SouthKorea]] | Naver / Kakao / DC Inside, real-name verification challenge |
| [[Country_NorthKorea]] | Outside-in methodology — KCNA, satellite imagery, defector sources |
| [[Country_India]] | Multilingual, Aadhaar context, MCA21, ShareChat |
| [[Country_Pakistan]] | Distinct from India, CPEC + counter-terrorism context |
| [[Country_Indonesia]] | WhatsApp + Kaskus + Zalo, world's largest Muslim population |
| [[Country_Vietnam]] | Zalo, Voz Forums, South China Sea OSINT |
| [[Country_Singapore]] | Sanctions-evasion / commodity hub, exceptionally efficient records |
| [[Country_Malaysia]] | 1MDB legacy, Bahasa Indonesia mutual intelligibility |
| [[Country_Thailand]] | Lèse-majesté constraints, Pantip forum, royal+military OSINT |
| [[Country_Philippines]] | Rappler / Maria Ressa context, WPS disputes, Marcos-era OSINT |

**`regional/oceania/` (1 country):**
| File | Country |
|:-----|:--------|
| [[Country_Australia]] | ASIC + AustLII, Pacific-region analyst hub |

**`regional/africa/` (6 countries):**
| File | Country |
|:-----|:--------|
| [[Country_Nigeria]] | Largest African pop., Premium Times, BEC / Yahoo-boys context |
| [[Country_SouthAfrica]] | Sub-Saharan analyst hub, amaBhungane, Daily Maverick, Gupta Leaks |
| [[Country_Kenya]] | East-Africa analyst hub, Africa Uncensored, M-Pesa OSINT |
| [[Country_Ethiopia]] | Tigray-War OSINT corpus, internet-shutdown context |
| [[Country_Egypt]] | Largest Arab country, Mada Masr, Sisi-era constraints, Suez/Nile |
| [[Country_Morocco]] | Pegasus deployment, Maghreb migration, francophone-Arabic bridge |

---

## OPSEC Cardinal Rules

> [!IMPORTANT]
> Every OSINT investigation leaks data about **you**. Plan compartmentalisation before you click anything. Full setup in [[VMs_and_Compartmentalization]].

1. **Never investigate from your daily-driver browser or account.** Use a dedicated VM / container per case.
2. **Assume sites log everything.** Visiting a target's site, querying a username, downloading their photo — all leave traces (IP, User-Agent, browser fingerprint, sometimes account-tied analytics).
3. **No personal accounts on investigation infrastructure.** A logged-in Google search is logged against your real identity.
4. **Use throwaway "sock puppet" accounts** with believable history if you need to access social platforms.
5. **Document as you go.** [[Browser_Extensions#Capture & Note-Taking|Hunchly]] or a dedicated screenshot tool — sources rot fast.
6. **Legal review** on any cross-border or dark-web work *before* you start. Some passive collection is illegal in some jurisdictions.

---

## Standard Methodology

```
1. Define scope and goal      → what question are we answering?
2. Choose investigation OS    → fresh VM snapshot, see [[Distros]]
3. Identify selectors         → emails, usernames, phone numbers, domains, names, images
4. Pivot through selectors    → each hit reveals new selectors
5. Capture continuously       → Hunchly / screenshots with hashes + timestamps
6. Cross-reference            → never trust a single source
7. Verify (esp. images)       → see [[Geolocation]] for visual cases
8. Report                     → narrative + evidence chain
```

---

## Selector Cheat-Sheet

| Selector | Where to start |
|:---------|:---------------|
| **Email** | Holehe, EmailRep, Hunter.io, HaveIBeenPwned, DeHashed |
| **Username** | [[Tools_Kali_Tracelabs#Sherlock\|Sherlock]], WhatsMyName, NameCheckr, Maigret |
| **Phone** | PhoneInfoga, TrueCaller (regional), Sync.me, OpenCellID |
| **Domain** | [[DNS_Enumeration]], crt.sh, SecurityTrails, ViewDNS, Whoisology |
| **IP** | Shodan, Censys, GreyNoise, AbuseIPDB |
| **Image** | Yandex Images (best for faces), Google, TinEye, PimEyes — see [[Geolocation]] |
| **Document** | Metagoofil, [[Tools_Kali_Tracelabs#ExifTool\|ExifTool]], FOCA |
| **Person (US)** | TLO, Accurint, BeenVerified — see [[Commercial_Tools]] |
| **Person (UK)** | Companies House, electoral roll, 192.com — see [[Country_UK]] |
| **Person (PL)** | KRS, CEIDG, REGON — see [[Country_Poland]] |
| **Company (global)** | OpenCorporates, Sayari Graph, regional registries |

---

## Common OSINT Frameworks & Lists

- **OSINT Framework** — [osintframework.com](https://osintframework.com/) — categorised tool list, the canonical starting point
- **TraceLabs OSINT VM** — preconfigured tooling, see [[Distros]]
- **Bellingcat Online Investigation Toolkit** — [bellingcat.com/resources](https://www.bellingcat.com/resources/) — curated by working investigators
- **IntelTechniques** — [inteltechniques.com/tools](https://inteltechniques.com/tools/) — Michael Bazzell's tools
- **Awesome OSINT** — [github.com/jivoi/awesome-osint](https://github.com/jivoi/awesome-osint)

---

## See Also

- [[DNS_Enumeration]] — Passive DNS / cert transparency overlap
- [[Getting_Started]] — Where OSINT fits in the kill chain
- [[Burp_Suite]] — When you pivot to active web recon
