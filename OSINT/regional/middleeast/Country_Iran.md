# Country OSINT — Iran (ایران)

Iran's internet ecosystem is **Persian (Farsi)** — written right-to-left, distinct from Arabic, with a partly-domestic ("Halal Net") platform stack mandated by state policy.

> [!IMPORTANT]
> Iran has one of the most sophisticated internet-control regimes globally. Many global platforms are blocked at the ISP level (Twitter/X, Facebook, YouTube, Telegram all officially banned but routinely used via VPN). State-promoted domestic alternatives are partly real, partly Potemkin.

---

## Language & Script

- **Persian (Farsi):** Arabic script + four extra letters (پ ، چ ، ژ ، گ). Don't search with the Arabic spelling — `ك` (Arabic kaf) ≠ `ک` (Persian kaf), `ي` (Arabic yeh) ≠ `ی` (Persian yeh). Many Iranian sites accept both, but indexing differs.
- **Pinglish / Finglish:** Latin-letter transliteration with numerals — heavy on social media. `Salam` for سلام, `kheili khoob` for خیلی خوب.
- **Translation:** Google Translate is OK for news; **Reverso** and **Google Translate Persian Wikipedia** training data give better dialect coverage. Glossika has Persian.

---

## Search

| Engine | Notes |
|:-------|:------|
| **Google.com with `lr=lang_fa`** | Default, best Persian index |
| **`google.ir`** | Different ranking; sometimes blocked or DNS-poisoned inside Iran |
| **Yandex** | Partly indexes Persian, useful counterweight |
| **Parsijoo** | Iranian state-promoted search engine, low quality, mainly indexes domestic sites |
| **Yooz, Salam, Yaranha** | Other domestic engines, niche |

---

## Social Networks

### Officially banned but heavily used (via VPN)
- **Telegram** — was once Iran's dominant social platform, banned 2018, still 30%+ of users access via VPN. Channels still active for opposition, news, commerce.
- **Instagram** — was the *one* major platform officially permitted; banned September 2022 during Mahsa Amini protests, still widely used via VPN.
- **Twitter / X** — officially banned, used by every major political figure including the Supreme Leader (state hypocrisy is a documented OSINT angle).
- **Facebook** — banned 2009, mostly displaced.
- **YouTube** — banned, used via VPN.

### Domestic state-promoted alternatives
| Platform | Type | Notes |
|:---------|:-----|:------|
| **Aparat** ([aparat.com](https://www.aparat.com/)) | YouTube alternative | Largest Iranian video site, regulated. *De facto* state-aligned, content removed quickly |
| **Soroush+** | Telegram alternative | State-promoted, low adoption |
| **Eitaa** ([eitaa.com](https://eitaa.com/)) | Telegram alternative | IRGC-affiliated, used by hardliners and clerics |
| **Bale** | Telegram alternative | Bank-Mellat-affiliated, used for banking |
| **Rubika** | Super-app | Telecom-affiliated, video + messaging |
| **iGap** | Messenger | Niche |
| **Anar** | Twitter clone | Failed, low adoption |

> **OSINT angle:** Account presence on **Eitaa** is itself a signal — it's primarily used by IRGC, Basij, and hardline clerical networks. Cross-referenced with the same handle on banned platforms is useful.

---

## News & Media

### State / regime-aligned
- **IRNA** ([irna.ir](https://www.irna.ir/)) — official state news agency
- **ISNA** ([isna.ir](https://www.isna.ir/)) — semi-official student news
- **Fars News** ([farsnews.ir](https://www.farsnews.ir/)) — IRGC-aligned
- **Tasnim News** ([tasnimnews.com](https://www.tasnimnews.com/)) — IRGC-aligned
- **Mehr News** — state-aligned
- **Press TV** (English) — international propaganda outlet
- **Kayhan** — extreme hardline daily, treat as Supreme Leader office mouthpiece

### Reformist / centrist (limited press freedom, may be intermittently shut down)
- **Etemad** (اعتماد), **Shargh** (شرق), **Ham-Mihan**, **Donya-ye Eqtesad** (Economy)

### Exile / diaspora (most influential investigative)
- **Iran International** (iranintl.com) — UK/Saudi-funded, Persian-language; sanctioned by Iran
- **BBC Persian** ([bbc.com/persian](https://www.bbc.com/persian))
- **VOA Persian / Radio Farda** — US-government funded
- **Manoto** — UK-based broadcaster
- **Radio Zamaneh**
- **IranWire** — citizen journalism
- **Kayhan London** (different from regime Kayhan)

---

## Government & Corporate Data

| Source | Purpose |
|:-------|:--------|
| **dolat.ir** | Government portal |
| **rrk.ir** | Official Gazette (Roozname Rasmi) — corporate registrations |
| **ilenc.ir** / **ssaa.ir** | Securities & Exchange Organization |
| **codal.ir** | Listed-company filings (Iranian SEC equivalent) |
| **iribu.ac.ir / etc.** | Educational/research databases |
| **OFAC SDN list** | Iran-specific sanctions designations are heavy here |

---

## Notable OSINT Case Studies

These are documented investigations to learn methodology from:

- **Mahsa Amini protests (2022):** Bellingcat, IranWire, Bellingcat Persian, GeoConfirmed verified hundreds of incident videos despite internet blackouts.
- **Operation Kayhan / IRGC officer doxing:** repeated leaks of internal regime documents; methodology heavy on Telegram channel scraping + cross-reference with corporate filings.
- **Mahan Air sanctions evasion:** flight tracking (FR24, ADS-B Exchange) + corporate cross-referencing.
- **Soleimani strike (2020):** real-time geolocation from Iraqi airport CCTV stills posted to Telegram.

---

## OPSEC Notes

- **VPN provides little protection on Iranian platforms.** State has direct telecom-level access. Aparat, Soroush, Eitaa accounts are **not safe** for sensitive sources.
- **Iranian state surveillance vendors** (e.g., MOIS-linked operations documented by Citizen Lab) target diaspora extensively. Investigators outside Iran are not safe by default.
- **Halal Net / National Information Network (NIN):** Iran has been building a domestic intranet for years. During shutdowns, only NIN sites remain reachable internally; external OSINT becomes one-way.
- **DNS poisoning is regular**, not just blocking — `dig` results from inside Iran are often spoofed.

---

## See Also

- [[Regional_RUNet]] — Many Iranian regime-Russia connections; Persian-Russian academic / military OSINT overlaps
- [[Regional_Arabic]] — Iranian-Arab politics overlap (Iraq, Lebanon, Syria, Yemen)
- [[VMs_and_Compartmentalization]] — Whonix essential for any Iran-targeted work
- [[Darkweb_Forums]] — Iranian dissident SecureDrop / Tor usage
- [[OSINT]] — Folder index
