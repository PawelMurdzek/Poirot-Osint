# Regional OSINT — RUNet (Russia / CIS)

OSINT sources for the Russian-language internet ("Рунет"). Covers Russia, Belarus, Kazakhstan, Ukraine (largely Russian-speaking pre-2022, now divergent), and broader CIS.

> [!IMPORTANT]
> Many RU government and corporate sites geo-fence outside the .ru/.by IP ranges, deploy aggressive anti-bot, and require Russian-speaking sock-puppets. Plan to investigate from a non-attributable IP geographically appropriate to your sock-puppet's claimed location. See [[VMs_and_Compartmentalization]].

---

## Search Engines

| Engine | Notes |
|:-------|:------|
| **Yandex** ([yandex.ru](https://yandex.ru/)) | Dominant in RU. Best image search anywhere — superb for **face recognition**, beats Google for Cyrillic-language results |
| **Yandex Images** | Right-click → Search Yandex (with the [RevEye browser extension](Browser_Extensions.md)) |
| **Yandex Maps** ([yandex.ru/maps](https://yandex.ru/maps/)) | Far better RU/CIS coverage than Google Maps. Includes "Panoramas" (street view) |
| **Mail.ru** ([go.mail.ru](https://go.mail.ru/)) | Secondary search portal |
| **Sputnik** | State-affiliated, deprecated |
| **Rambler** | Legacy, declining |

---

## Social Networks

### VKontakte (VK)
The dominant Russian social network. Looser privacy defaults than Facebook — public-by-default profiles, photos, friends lists.

| Resource | Use |
|:---------|:----|
| [vk.com](https://vk.com/) | Main site |
| `vk.com/id<numeric_id>` | Direct profile by numeric ID |
| `vk.com/<vanity>` | Vanity URL profile |
| **Advanced search** | `vk.com/people` — filter by age, city, school, employer, relationship status |
| **vk-osint-tools** | GitHub: tools for bulk profile pulling, friend graph analysis |
| **VKScraper** | Python scraper for posts/photos |
| **220vk.com** | Web tool to find "hidden friends" via public group memberships |
| **vk.watch** (commercial) | Profile change tracking |

**Pivots:** Profile photo → Yandex reverse image. Username → check Telegram, OK, Twitter for the same handle.

### Odnoklassniki (OK / OK.ru)
"Classmates" — older demographic than VK, strong in Russian regions and CIS. Less developer-friendly, fewer tools.
- [ok.ru](https://ok.ru/)
- Profile pattern: `ok.ru/profile/<numeric_id>`

### Telegram
**Massively important in RUNet.** Channels (one-to-many broadcast), groups (many-to-many), and private chats. Used for news, opposition activism, cybercrime, government communication, and everyday messaging.

| Tool | Purpose |
|:-----|:--------|
| **TGStat** ([tgstat.com](https://tgstat.com/)) | Telegram channel directory + analytics, full-text search across public channels |
| **Telegago** | Search public Telegram channels via Google CSE |
| **Lyzem** | Public Telegram message search |
| **snscrape telegram-channel** | CLI scraping of public channels |
| **Telethon** | Python library for Telegram client API (requires phone number for auth) |
| **TG-Archive / tg-archive.com** | Archive specific channels |

### LiveJournal
Historically huge in RU, declining but still active for political commentary.
- [livejournal.com](https://www.livejournal.com/)

### Ya.ru / Yandex Zen / Dzen
Yandex's blogging/news platform.
- [dzen.ru](https://dzen.ru/)

---

## Messaging

| Platform | Notes |
|:---------|:------|
| **Telegram** | Above |
| **WhatsApp** | Common, but encrypted/closed for OSINT |
| **VK Messenger** | Built into VK |
| **TamTam** | Mail.ru Group's messenger, niche |
| **ICQ New** | Mail.ru Group, niche but persistent |

---

## Government & Corporate Registries

| Registry | Purpose | URL |
|:---------|:--------|:----|
| **СПАРК (SPARK)** | Corporate intelligence, financials, litigation, sanctions | [spark-interfax.ru](https://spark-interfax.ru/) — paid |
| **Контур.Фокус** | Corporate registry + risk analytics | [focus.kontur.ru](https://focus.kontur.ru/) — paid |
| **ЕГРЮЛ / ЕГРИП** (FNS) | Free legal-entity / sole-prop registry | [egrul.nalog.ru](https://egrul.nalog.ru/) |
| **Rusprofile** | Free corporate snapshot, scrapes ЕГРЮЛ | [rusprofile.ru](https://www.rusprofile.ru/) |
| **List-Org** | Corporate connections, beneficial-ownership pivots | [list-org.com](https://www.list-org.com/) |
| **Rosreestr** | Real-estate / cadastral data | [rosreestr.gov.ru](https://rosreestr.gov.ru/) — partial public access |
| **Federal Bailiffs (ФССП)** | Outstanding court debts, civil judgments | [fssp.gov.ru](https://fssp.gov.ru/) |
| **Court records (sudrf.ru)** | Federal court case search | [sudrf.ru](https://sudrf.ru/) |
| **Arbitration courts (kad.arbitr.ru)** | Commercial litigation | [kad.arbitr.ru](https://kad.arbitr.ru/) |
| **Госзакупки** | Government procurement contracts | [zakupki.gov.ru](https://zakupki.gov.ru/) |

**Belarus equivalents:** ЕГР Беларуси, court.gov.by.
**Kazakhstan equivalents:** kgd.gov.kz, stat.gov.kz, sud.gov.kz.

---

## News & Media

### Pro-Kremlin / state
- **RIA Novosti** (ria.ru), **TASS** (tass.com), **Russia Today / RT** (rt.com), **Sputnik**, **Kommersant** (kommersant.ru — somewhat independent), **Izvestia**
- Useful for tracking the official narrative; treat all as state-aligned.

### Independent / opposition (mostly in exile post-2022)
- **Meduza** (meduza.io) — exile, Latvia
- **Novaya Gazeta Europe** (novayagazeta.eu) — exile
- **The Insider** (theins.ru / theins.press) — investigative
- **Mediazona** (zona.media) — court / penal-system focus
- **iStories / iStories.media** — investigative
- **Radio Svoboda / Radio Free Europe** (svoboda.org)

### Aggregators
- **Yandex.News** (news.yandex.ru) — algorithmic aggregator (state-curated)
- **MediaMetrics** — RU news headline aggregator

---

## Leaks & Breach Data (RU-specific)

> Many RU databases have leaked over the years; some sites republish them. Possessing PII may be illegal in your jurisdiction even if it's "public" elsewhere. Get legal review.

- **GetContact** — phone number → contacts that have you saved
- **Eye of God** (Telegram bot, шут, semi-legal) — phone/PII lookup, multiple offshoots
- **Probiv services** — paid PII-lookup services run on Telegram. **These are illegal services.** Knowing they exist matters for threat-intel; using them does not.
- **Global breach data** ([[Tools_Kali_Tracelabs#Email / Breach / Credential]]): HIBP, DeHashed — often contain RU breaches

---

## Geolocation Specific

| Resource | Why |
|:---------|:----|
| **Yandex Maps Panoramas** | Better street-view in RU/CIS than Google |
| **2GIS** ([2gis.ru](https://2gis.ru/)) | Detailed local map directory, business listings |
| **Wikimapia** | Crowdsourced annotated map, strong RU community |
| **OpenStreetMap RU** | Active mapping community |

---

## Analyst Toolchain — RUNet Quick Stack

1. **Yandex** for Cyrillic search and reverse image
2. **VK + 220vk.com** for VK profile work
3. **TGStat** for Telegram
4. **Rusprofile / List-Org** for corporate
5. **kad.arbitr.ru** for commercial litigation
6. **Meduza / The Insider / iStories** for verified investigative context
7. Translate with **Yandex Translate** (better for RU than Google)

---

## OPSEC Notes

- Many RU government sites geo-block. A residential RU exit is gold; data-centre VPN exits often blocked. Plan accordingly.
- **Do not log into VK, OK, or Telegram with any account tied to your real identity** — Russian platforms cooperate with FSB requests.
- **Russian "DPI"** (TSPU): traffic to certain platforms is throttled inside Russia. If you're investigating from inside RU, this affects what works.
- **2022+ context:** many RU sites now block .eu / .us IPs entirely; many EU sites block .ru IPs entirely. Check both directions when planning.

---

## See Also

- [[OSINT]] — Folder index and methodology
- [[Regional_China]] / [[Regional_Arabic]] — Other regional ecosystems
- [[VMs_and_Compartmentalization]] — Sock-puppet hygiene matters more here
- [[Browser_Extensions]] — Yandex search, translation, RevEye
- [[Tools_Kali_Tracelabs]] — Snscrape, Telethon, instaloader
