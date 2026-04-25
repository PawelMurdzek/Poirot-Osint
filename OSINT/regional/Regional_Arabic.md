# Regional OSINT — Arabic-Speaking Internet (الشبكة العربية)

OSINT sources for the Arabic-language internet across MENA (Middle East & North Africa). Spans 22+ Arab League countries with very different platform mixes — Gulf countries lean global (X/Twitter, Snapchat, Instagram), Levant uses heavy Telegram and Facebook, North Africa skews Facebook/YouTube.

> [!IMPORTANT]
> "Arabic OSINT" is not a single ecosystem. **Country and dialect matter.** Egyptian, Levantine, Gulf, Maghrebi, and Iraqi Arabic are mutually distinct enough that searches in Modern Standard Arabic (الفصحى, *fuṣḥā*) often miss colloquial content. Always run dialect-specific searches as well as MSA.

---

## Languages & Search

### Use both scripts
Arabic search needs both:
- **Standard Arabic script:** الجزائر (Algeria)
- **Arabic chat alphabet (Franco-Arabic / Arabizi):** "el djazaer", "jazair" — Latin-letter transliteration with numerals (`3` for ع, `7` for ح, `2` for ء). Heavy on social media in Egypt, Levant, Maghreb.

### Search engines
| Engine | Notes |
|:-------|:------|
| **Google.com** with `lr=lang_ar` | Default — best Arabic index |
| **Google country TLDs** (`google.eg`, `google.sa`, `google.ae`, `google.ma`, …) | Country-tuned ranking |
| **Bing** | Sometimes catches Arabic news Google missed |
| **Yandex** | Surprisingly strong on Arabic image search (especially Levant content) |

### Translation
- **Google Translate** — solid for MSA, weaker on dialect
- **DeepL** — improving Arabic, MSA only
- **Reverso** — best for Levantine / dialect colloquialisms with example sentences
- **Almaany** ([almaany.com](https://www.almaany.com/)) — Arabic monolingual + bilingual dictionary, dialect labels

---

## Social Networks

### Twitter / X
**Disproportionately important** in the Arab world — used by activists, journalists, and citizens during the Arab Spring (2010-2011) and onwards. Saudi Arabia has one of the highest Twitter penetration rates globally.

- Country-specific trending: `twitter.com/explore/tabs/trending` with location set
- Arabic hashtag investigation: hashtags often mix MSA and dialect
- Tools: `twscrape` (sock-puppet auth, current best-effort), commercial Twitter monitoring (Brandwatch, Talkwalker, Pulsar) — `snscrape` Twitter mode is dead since 2023

### Telegram
**Heavy use across MENA** — especially Iraq, Iran's Arabic-speaking minorities, Syria, Lebanon, Yemen, Egypt. Channels for news, militia communications, dissidents, and everyday community.

- See [[Regional_RUNet#Telegram]] for tooling — same tools (TGStat, Telegago, Lyzem, snscrape, telethon)
- Many regional militia/political channels documented by **Bellingcat** and **GLOSAINT**
- **TGStat country filter:** filter Arabic-language channels

### Facebook
Dominant in Egypt, Maghreb, and broader. Lower in Saudi Arabia and UAE (X dominates there).
- Facebook public group search (broken since 2020s, partial)
- **CrowdTangle** (Meta tool — formerly free, now restricted to vetted researchers/journalists)
- **Facebook Marketplace** for local commerce intel

### Instagram
Massive in the Gulf states, especially KSA and UAE.
- Public profiles via [[Tools_Kali_Tracelabs#Social Media|Instaloader / Osintgram / Toutatis]]

### Snapchat
**Very dominant in Saudi Arabia / Gulf** — higher per-capita usage than almost anywhere else. Mostly closed to OSINT (ephemeral by design), but **Snap Map** is searchable for public Snaps.
- [map.snapchat.com](https://map.snapchat.com/)

### TikTok
Big across MENA, especially among under-30s. Non-China TikTok (`tiktok.com`) — see [[Regional_China]] for the mainland Douyin counterpart.

### YouTube
Major platform across MENA. Country-specific trending: `youtube.com/feed/trending?gl=EG` (etc.).

### Regional / niche platforms
| Platform | Country / region | Notes |
|:---------|:-----------------|:------|
| **Tarabezah** | Lebanon | Local social/news |
| **Dailymotion** | Maghreb (FR-influenced) | Video, often used where YouTube monetisation is restricted |
| **Mawdoo3** ([mawdoo3.com](https://mawdoo3.com/)) | Pan-Arab | Arabic Wikipedia-alternative for everyday topics |
| **Ymoblie / يمنية** | Yemen | Country-specific forums |
| **Hawamer** | Saudi Arabia | Forum |
| **Bayt.com** | Pan-Arab | Largest jobs site, useful for employment / company OSINT |

---

## Messaging

| Platform | Penetration |
|:---------|:------------|
| **WhatsApp** | Effectively universal across MENA. Encrypted, closed, but WhatsApp groups occasionally leak via screenshots |
| **Telegram** | Above |
| **iMessage** | Apple-heavy markets (KSA, UAE, KW) |
| **Signal** | Activists, journalists |
| **Botim** | Specifically used in **UAE** because WhatsApp/Skype voice calls are blocked there — its presence on a phone is a UAE-residence signal |
| **Imo** | Common in Iraq / Egypt for diaspora calls |

---

## Government & Corporate Registries

> Coverage varies hugely by country. Gulf states have moderate transparency; many MENA countries have no public corporate registry, or one that's barely searchable.

| Country | Registry | URL / Notes |
|:--------|:---------|:------------|
| **UAE** | Multiple (federal + free zones) | DIFC, ADGM (free zones); MOEC for federal; commercial paywall via OpenCorporates |
| **KSA** | MoCommerce | [mc.gov.sa](https://mc.gov.sa/) — partial public access |
| **Egypt** | GAFI | [gafi.gov.eg](https://www.gafi.gov.eg/) — limited search |
| **Lebanon** | Commercial Register | [cr.justice.gov.lb](https://cr.justice.gov.lb/) — fragmented |
| **Jordan** | CCD | [ccd.gov.jo](https://www.ccd.gov.jo/) |
| **Morocco** | OMPIC | [ompic.ma](https://www.ompic.ma/) |
| **Tunisia** | ANRJC | partial |
| **Iraq, Syria, Libya, Yemen** | Limited or no public registry | Often need NGO/journalism contacts on the ground |
| **Pan-Arab** | **OpenCorporates** | aggregates what's public |
| **Sanctions** | OFAC, EU, UK, UN sanctions lists | Heavy MENA coverage |

---

## News & Media (by tier)

### Pan-Arab major
- **Al Jazeera** (aljazeera.net Arabic, aljazeera.com English) — Qatar-funded, regionally controversial
- **Al Arabiya** (alarabiya.net) — Saudi-aligned
- **Sky News Arabia** — Abu Dhabi-based, partly UK-owned
- **BBC Arabic** ([bbc.com/arabic](https://www.bbc.com/arabic)) — pulled TV but online active
- **France 24 Arabic**, **DW Arabic**, **RFI Arabic** — Western state-funded Arabic outlets
- **Asharq Al-Awsat** — Saudi pan-Arab daily

### Country-specific independent / opposition
- **Mada Masr** (Egypt — heavily harassed, periodically blocked)
- **Daraj** (Lebanon — investigative)
- **Al-Akhbar** (Lebanon — Hezbollah-aligned)
- **L'Orient-Le Jour** (Lebanon — French)
- **The New Arab / Al-Araby Al-Jadeed**
- **Middle East Eye**
- **Raseef22** (pan-Arab independent)

### State-aligned (treat as such)
- **MENA News Agency** (Egypt, state)
- **WAM** (UAE, state)
- **SPA** (Saudi, state)
- **SANA** (Syria, regime)
- **NNA** (Lebanon, state)

### Investigative / accountability journalism
- **OCCRP** — multi-country investigative consortium, strong MENA work
- **Daraj**
- **ARIJ Network** — Arab Reporters for Investigative Journalism
- **Bellingcat** — frequent MENA OSINT investigations (Syria, Yemen, Libya)

---

## Conflict & Humanitarian OSINT

MENA hosts an outsize share of conflict/humanitarian OSINT work. Communities and resources:

| Resource | Focus |
|:---------|:------|
| **Bellingcat** | Syria, Yemen, Libya, Iran, Israel-Palestine — sets methodology benchmarks |
| **Airwars** | Civilian-harm tracking from coalition airstrikes |
| **Syrian Archive** ([syrianarchive.org](https://syrianarchive.org/)) | Curated, verified Syrian-conflict footage |
| **Yemeni Archive** | Same model for Yemen |
| **Mnemonic** | Parent organisation of the above |
| **ACLED** ([acleddata.com](https://acleddata.com/)) | Armed-conflict event data |
| **Liveuamap** | Real-time conflict map |
| **GeoConfirmed** | Volunteer-verified geolocations of conflict footage |
| **Forensic Architecture** | Investigative research collective, frequent MENA cases |

---

## Regional Specifics — by sub-region

### Gulf (KSA, UAE, KW, QA, BH, OM)
- **High X/Twitter, Instagram, Snapchat**
- **Phone-tied identities** (real-name registration mandatory in KSA, UAE)
- **Censorship is sophisticated** — VPNs partly criminalised in UAE
- **Corporate transparency** is moderate — UAE free zones publish, mainland less so

### Levant (LB, SY, JO, PS, IQ)
- **Heavy Telegram, Facebook, WhatsApp**
- **Twitter/X for activists & journalists**
- **Conflict OSINT** (Syria, Iraq) drives much of the regional methodology
- **Hezbollah / militia channels** primarily on Telegram

### Egypt
- **Facebook dominant**
- **Press freedom heavily restricted** — journalists routinely arrested
- **VPN/Tor use partly criminalised** since 2018
- Mada Masr is the main remaining independent voice

### Maghreb (MA, DZ, TN, LY)
- **Strong French-language overlap** — investigations often need both AR and FR sources
- **Facebook + WhatsApp dominant**
- **Algeria + Tunisia** — vibrant Twitter activist scene
- **Libya** — fragmented info ecosystem, limited registries

### North Africa overlap with Sahel (Sudan, Mauritania)
- Often needs **French + Arabic + English** triangulation
- Limited corporate/governmental transparency

---

## OPSEC Notes

- **VPN legality varies wildly.** UAE has criminalised VPN-as-mask-for-other-crime (2016 law); KSA/Iran have similar provisions. Practical risk is mainly when combined with another offence — but it's a factor.
- **Real-name registration** is the law in several Gulf states. Sock-puppets registered with non-attributable phone numbers are detectable.
- **Family / tribal networks** are stronger OSINT pivots in MENA than in Western contexts. A name + father's name + grandfather's name often uniquely identifies an individual.
- **Surveillance vendors** (NSO Group, Candiru, Cytrox) have been documented heavily in MENA — Citizen Lab is the canonical research source. If you're investigating someone who might be high-value, your phone is a target too.

---

## See Also

- [[OSINT]] — Folder index and methodology
- [[Regional_RUNet]] / [[Regional_China]] — Other regional ecosystems
- [[VMs_and_Compartmentalization]] — Telegram requires careful identity hygiene
- [[Browser_Extensions]] — Mate Translate and reverse image (Yandex strong here)
- [[Tools_Kali_Tracelabs]] — telethon for Telegram, snscrape for residual social-media coverage
