# Social Media APIs & Scraping

Comprehensive matrix of social-media platforms — official API existence, auth model, rate limits, scraping difficulty, account requirements, and tooling. Reflects the landscape **as of 2026**, after the 2023 API closures (Twitter/X, Reddit) and the 2024 platform consolidation (CrowdTangle shutdown, X Brazilian ban-and-restore, Bluesky opening, Meta Threads federation).

> [!IMPORTANT]
> Most platforms' Terms of Service prohibit scraping in some form. **Legal risk** depends on jurisdiction and method:
> - In the US, **hiQ Labs v. LinkedIn** (9th Circuit, 2022) held that scraping public data isn't a CFAA violation, but ToS violations remain a contract issue.
> - In the EU, **Article 17 GDPR + Database Directive** complicates bulk scraping of PII.
> - **Account-creation under false pretences** can violate the Computer Fraud and Abuse Act (US) if it bypasses access controls.
> - For **research / journalism / OSINT**, exemptions often exist but rely on documented intent.

> **Difficulty rating:** 1 = trivial public scrape · 2 = needs simple workarounds · 3 = needs auth or rate-limit dance · 4 = needs sock-puppet account + active anti-bot · 5 = effectively impossible without insider/leak access

---

## Tier 1 — Major Platforms (current landscape, 2026)

### Twitter / X

| Field | Value |
|:------|:------|
| **API** | Yes — paid only (free tier essentially unusable since April 2023, status unchanged in 2026) |
| **Tiers (2026)** | Free (1500 posts/month write, no read), Basic ($200/mo as of 2025 price hike), Pro ($5k/mo), Enterprise ($42k+/mo) |
| **Auth** | OAuth 2.0 + API key |
| **Scraping difficulty** | **4-5** — tightening year-over-year; logged-out browse increasingly limited |
| **Account required for scraping** | **Yes (logged-in)** — guest API access mostly killed by 2024 |
| **Tools (current)** | `nitter` (most public instances dead by 2025), `twscrape` (auth + sock-puppet rotation, the de facto OSS option), commercial: Brandwatch / Talkwalker / Pulsar |
| **Notes** | Effectively closed for free OSINT. `snscrape` Twitter functionality is dead. Public profile pages still render to logged-out users in some regions, but feed access requires login. **Brazilian ban (Sep–Oct 2024)** was a major OSINT case study itself — investigators used Bluesky in parallel during the outage |

### Meta (Facebook + Instagram + Threads)

| Field | Facebook | Instagram | Threads |
|:------|:---------|:----------|:--------|
| **API** | Graph API — heavily restricted | Graph API — business accounts only | Limited API |
| **Auth** | OAuth, app review | OAuth, business verification | OAuth |
| **Scraping difficulty** | **4-5** | **4-5** | **3-4** |
| **Account required** | Yes (and ageing required to avoid flagging) | Yes | Yes |
| **Tools** | Public-page scrapers fragile; **CrowdTangle** was the OSINT standard but was discontinued by Meta in 2024 | `instaloader` (public profiles only), `Osintgram` (login required, ban risk), `Toutatis` (selector hints) | Federated post via ActivityPub partly |
| **Notes** | Meta aggressively detects automation; account bans common. **CrowdTangle replacement** is the **Meta Content Library** — gated to vetted academic/journalist researchers |

### LinkedIn

| Field | Value |
|:------|:------|
| **API** | Yes — **partner-only** for most useful endpoints |
| **Auth** | OAuth 2.0 |
| **Scraping difficulty** | **4** (active anti-bot) |
| **Account required** | Yes |
| **Tools** | `linkedin-scraper` (Selenium), `phantombuster` (commercial, semi-grey), Sales Navigator |
| **Notes** | Famous *hiQ v. LinkedIn* litigation; legal precedent says public scraping is OK under CFAA, but LinkedIn enforces aggressively at the technical level. Account bans common. Heavy CAPTCHA / device fingerprinting |

### Reddit

| Field | Value |
|:------|:------|
| **API** | Yes — paid for meaningful scale since June 2023, unchanged through 2026 |
| **Tiers** | Free (100 req/min logged-in, 10 req/min anon), Enterprise (~$0.24/1000 calls — based on the leaked Apollo / 3rd-party-app figures) |
| **Auth** | OAuth 2.0 |
| **Scraping difficulty** | **2-3** |
| **Account required** | Recommended; rate limits much lower without |
| **Tools** | `praw` (official Python), `pullpush.io` and `arctic-shift` for historical (limited) — `pushshift` was killed in 2023 |
| **Notes** | Pushshift's 2023 death is still felt — historical Reddit search is mostly impossible at scale. **Reddit IPO (March 2024)** further tightened API monetisation. Free-tier hobbyist scraping works; bulk historical is gated to commercial (e.g., Reddit's own data licensing partners) |

### YouTube

| Field | Value |
|:------|:------|
| **API** | Yes — Google YouTube Data API v3, free with quota |
| **Quota** | 10,000 units/day default, plenty for moderate use |
| **Auth** | API key (read) or OAuth (account actions) |
| **Scraping difficulty** | **1-2** |
| **Account required** | No (key-only for read) |
| **Tools** | `pytchat`, `yt-dlp` for video download, `youtube-search-python`, `youtube-transcript-api` for captions |
| **Notes** | Official API is generous and stable. Comments / live chat / metadata all accessible |

### TikTok

| Field | Value |
|:------|:------|
| **API** | Research API (academic-vetted since 2023, expanded 2024) + Display API + Commercial API |
| **Auth** | OAuth, vetted-researcher status (Research API) |
| **Scraping difficulty** | **3-4** |
| **Account required** | Recommended |
| **Tools** | `TikTokApi` (unofficial), commercial: Sprout Social, Brandwatch, Pulsar |
| **Notes** | Research API access is slow but real. **US "ban or divest" law (signed April 2024)** triggered a brief shutdown in January 2025 before being lifted; OSINT analysts now hedge across TikTok and Reels. EU **DSA "Very Large Online Platform"** designation (2023) gives EU researchers somewhat more leverage |

### Snapchat

| Field | Value |
|:------|:------|
| **API** | Marketing API only |
| **Scraping difficulty** | **5** for content (ephemeral by design) |
| **Account required** | Yes if used |
| **Tools** | **Snap Map** ([map.snapchat.com](https://map.snapchat.com/)) — only public-Snap data point |
| **Notes** | Designed against OSINT. Public Stories rare; Snap Map is the analyst's main artefact |

### Discord

| Field | Value |
|:------|:------|
| **API** | Yes — bot API + user API (gated) |
| **Auth** | Bot token or OAuth |
| **Scraping difficulty** | **3** for public servers, **5** for private |
| **Account required** | Yes |
| **Tools** | `discord.py`, `DiscordChatExporter` (third-party, ToS-violating) |
| **Notes** | Public server discoverable via Disboard, Top.gg directories. Private servers require sock-puppet infiltration. **DiscordChatExporter** is the standard OSINT tool but violates ToS. Used regularly in extremist-group investigations |

### Twitch

| Field | Value |
|:------|:------|
| **API** | Yes — Helix API, free with auth |
| **Auth** | OAuth 2.0 (client credentials or user) |
| **Scraping difficulty** | **2** |
| **Account required** | App registration |
| **Tools** | `twitchAPI` (Python), `chat-downloader` for VOD chat |

### Pinterest

| Field | Value |
|:------|:------|
| **API** | Yes — limited, business-focused |
| **Scraping difficulty** | **2** |
| **Tools** | `pinscrape`, `py3-pinterest` |
| **Notes** | Public boards scrapable; OSINT relevance is mostly geolocation (vacation pins, home photos) |

---

## Tier 2 — Messaging Platforms

### Telegram

| Field | Value |
|:------|:------|
| **API** | Yes — **MTProto (full client API)** + **Bot API** |
| **Auth** | Phone number (full client) or bot token |
| **Scraping difficulty** | **1-2** for public channels/groups |
| **Account required** | Yes (phone number) |
| **Tools** | `telethon`, `Pyrogram`, `tg-archive`, `snscrape telegram-channel`, **TGStat** (channel directory + analytics) |
| **Notes** | The most OSINT-friendly major messaging platform. Public channels are fully scrapable. Private groups need infiltration. Heavy use in [[Country_Iran]], [[Country_Belarus]], [[Country_NorthKorea]]-related Lazarus comms, [[Regional_RUNet]] |

### WhatsApp

| Field | Value |
|:------|:------|
| **API** | WhatsApp Business API (paid, gated) |
| **Auth** | Business Manager + Cloud API token |
| **Scraping difficulty** | **5** (E2E encryption) |
| **Account required** | Phone number |
| **Tools** | **WhatsApp Web automation** (`whatsapp-web.js`, `Baileys`) — exists but ToS-violating |
| **Notes** | OSINT mostly via screenshots / leaks / mutual-group reconnaissance. Used universally in [[Country_Brazil]], [[Country_India]], MENA |

### Signal

| Field | Value |
|:------|:------|
| **API** | None for client; signal-cli for self-hosting |
| **Scraping difficulty** | **5** |
| **Notes** | Designed against OSINT |

### Line

| Field | Value |
|:------|:------|
| **API** | Yes — Line Messaging API |
| **Auth** | Channel access token |
| **Scraping difficulty** | **4-5** for personal messaging |
| **Notes** | Dominant in [[Country_Japan]]. Public Line accounts (brands) limited; personal accounts effectively closed |

### KakaoTalk

| Field | Value |
|:------|:------|
| **API** | Limited (login + messaging integrations) |
| **Scraping difficulty** | **4-5** |
| **Notes** | Korean platform, real-name verification required for sock-puppets — see [[Country_SouthKorea]] |

---

## Tier 3 — Federated / Decentralised (the friendly tier for OSINT)

> **The federated platforms are unusually open to OSINT** — most expose ActivityPub or AT Protocol APIs without auth. Account creation is usually frictionless.

### Mastodon (ActivityPub)

| Field | Value |
|:------|:------|
| **API** | Yes — Mastodon REST API on each instance |
| **Auth** | OAuth or unauthenticated public endpoints |
| **Scraping difficulty** | **1** |
| **Account required** | No for public posts |
| **Tools** | `Mastodon.py`, `Pinafore`, snscrape (mastodon support) |
| **Notes** | Each instance has its own admin policy. Some instances rate-limit public APIs aggressively (`mstdn.social`); others are very open. **Federated timelines** show what an instance sees from across the fediverse. **OSINT angle:** users often migrate from Twitter/X with same handles — easy identity continuity |
| **Account creation** | Trivial on most instances. `mastodon.social`, `fosstodon.org`, `mstdn.social` are the largest English ones; `mstdn.jp` and `pawoo.net` are the largest Japanese |

```python
# Quick public-timeline scrape (no auth)
import requests
r = requests.get('https://mastodon.social/api/v1/timelines/public?local=false&limit=40')
toots = r.json()
```

### Bluesky (AT Protocol)

| Field | Value |
|:------|:------|
| **API** | Yes — open AT Protocol XRPC endpoints |
| **Auth** | App password (free), or unauthenticated for public timelines |
| **Scraping difficulty** | **1** |
| **Account required** | No (most read endpoints are public) |
| **Tools** | `atproto` (official Python SDK), `bskyx` |
| **Notes** | Surge of activity since X exodus 2023-2024. Big communities in [[Country_Brazil]], Japan, scientific Twitter |
| **Account creation** | Open since Feb 2024 — bsky.app, no invites needed |

```bash
# Get a public profile feed (no auth)
curl "https://public.api.bsky.app/xrpc/app.bsky.feed.getAuthorFeed?actor=alice.bsky.social&limit=50"
```

### Lemmy (Reddit alternative on ActivityPub)

| Field | Value |
|:------|:------|
| **API** | Yes — Lemmy HTTP API per-instance |
| **Auth** | None for read |
| **Scraping difficulty** | **1** |
| **Tools** | `lemmy.py`, regular HTTP |
| **Notes** | Major instances: `lemmy.world`, `lemmy.ml`, `sh.itjust.works`. Smaller than Reddit but growing |

### Pixelfed (Instagram alternative on ActivityPub)

| Field | Value |
|:------|:------|
| **API** | ActivityPub + Mastodon-compatible |
| **Scraping difficulty** | **1** |

### PeerTube (YouTube alternative on ActivityPub)

| Field | Value |
|:------|:------|
| **API** | Yes — REST API per-instance |
| **Scraping difficulty** | **1** |
| **Notes** | Federated video. Smaller, but content includes some that's been removed from YouTube |

### Misskey / Pleroma / Friendica / Akkoma / Calckey

ActivityPub-based — same posture as Mastodon. Different UI, similar APIs. Mostly Japanese / European / niche communities.

### Diaspora

Older federated network. Smaller. ActivityPub-adjacent. APIs available per-pod.

---

## Tier 4 — Regional Platforms

### VK / OK / Mail.ru

| Field | VK | OK |
|:------|:---|:---|
| **API** | Yes — VK API | Yes — limited |
| **Auth** | App ID + access token | App ID |
| **Scraping difficulty** | **2-3** | **3** |
| **Tools** | `vk_api`, `vkscraper`, `220vk.com` (web) | `okru-scraper` |

See [[Regional_RUNet]] for context.

### Weibo / Douyin / WeChat / Xiaohongshu

See [[Regional_China]]. APIs partly exist (Weibo Open Platform) but require CN business verification. Most OSINT done via web scraping with sock-puppets (CN phone number gating is the hard part).

| Platform | API | Difficulty | Notes |
|:---------|:----|:-----------|:------|
| **Weibo** | Limited (Open Platform — CN gated) | **3** | `weiboscraper`, snscrape weibo |
| **Douyin** | None for read | **4** | Anti-bot heavy; tiktok-api Mainland forks |
| **WeChat** | None for personal | **5** | Public Accounts via Sogou search only |
| **Xiaohongshu / RedNote** | None public | **4** | Anti-bot heavy |
| **Bilibili** | Yes (Bilibili API) | **2** | `bilibili-api-python` |
| **Zhihu** | None public | **3** | Web scraping required |

### Naver / Kakao / DC Inside

See [[Country_SouthKorea]]. Korean platforms have **real-name verification** that makes sock-puppets very hard.

| Platform | API | Difficulty |
|:---------|:----|:-----------|
| **Naver** | Yes — search/translate/maps APIs (free with KR phone) | **2** for read APIs, **4** for accounts |
| **Naver Cafe** | None public | **4** (login + KR real-name) |
| **KakaoTalk** | Limited | **4-5** |
| **AfreecaTV / SOOP** | Yes — limited | **3** |

### 5ch / Niconico / Mixi (Japan)

See [[Country_Japan]].

| Platform | API | Difficulty |
|:---------|:----|:-----------|
| **5ch** | None official | **2** (HTML scrape, anti-bot moderate) |
| **Niconico** | Yes — limited | **2** |
| **Mixi** | None public | **3** |

### ShareChat / Moj / Koo (India)

See [[Country_India]].

---

## Tier 5 — Niche / Fringe / "Alt-Tech"

### Truth Social (Mastodon fork)

| Field | Value |
|:------|:------|
| **API** | Yes — fork of Mastodon, broadly compatible |
| **Scraping difficulty** | **2-3** |
| **Notes** | OSINT-heavy in US political / extremism research. Anti-scraping countermeasures applied periodically |

### Gab (Mastodon fork)

| Field | Value |
|:------|:------|
| **API** | Mastodon-compatible |
| **Scraping difficulty** | **2** |
| **Notes** | Far-right platform, frequent OSINT target |

### Parler (defunct)

Wiped 2021; **archived** versions exist (`parler.io` archives, ddosecrets dumps). OSINT done on archive snapshots.

### 4chan / 8kun

| Field | Value |
|:------|:------|
| **API** | 4chan has a public read-only JSON API |
| **Auth** | None |
| **Scraping difficulty** | **1-2** |
| **Tools** | `4chan-api` (Python), `archived.moe`, `desuarchive.org` |
| **Notes** | Anonymous boards; **archive sites** keep deleted threads. OSINT analysts use these heavily for fringe-content tracking |

### BitChute / Rumble / Odyssey / DLive

Video platforms with right-leaning / unmoderated content. APIs limited; web scraping required. Frequent OSINT subjects.

### Substack / Ghost / Medium

| Platform | API | Notes |
|:---------|:----|:------|
| **Substack** | Limited (public RSS, post API for owners) | Email newsletter platform; analyst use to track political-commentary writers |
| **Medium** | Limited |  |
| **Ghost** | Yes — full REST API |  |

### OnlyFans / Patreon / Cameo

| Platform | API | Difficulty |
|:---------|:----|:-----------|
| **OnlyFans** | None public | **4** (paid + login) |
| **Patreon** | Yes (limited) | **2** for public, **3** for patron-only |
| **Cameo** | None | **3** |
| **Buy Me a Coffee** | Limited |  |

OSINT angle: identity-resolution. Same handle on Patreon as on Twitter / personal site is common.

### Forums / classifieds (often more revealing than social)

- **Stack Exchange / Stack Overflow** — full Stack Exchange API, free, generous
- **GitHub** — full REST + GraphQL API (huge OSINT surface — code, contacts, organisations)
- **GitLab** — full REST API
- **Bitbucket** — REST API
- **DEV.to** — REST API
- **HackerNews** — Firebase REST API, no auth
- **Wykop (PL)** — REST API; see [[Country_Poland]]
- **DC Inside (KR)** — none official; see [[Country_SouthKorea]]
- **Yelp / Glassdoor / Indeed** — official APIs limited; review-content scraping common

---

## Sock-Puppet Account Provisioning

For platforms that require login, you need a sock-puppet — see [[VMs_and_Compartmentalization#Sock-Puppet Identity Hygiene]] for the full hygiene model. Quick reference for **what each platform needs to register**:

| Platform | Phone | ID | Real name | Workaround |
|:---------|:------|:---|:----------|:-----------|
| **Mastodon** | No | No | No | Trivial |
| **Bluesky** | No (email only) | No | No | Trivial |
| **Reddit** | No (email) | No | No | Easy |
| **Telegram** | **Yes** | No | No | Receive-SMS service or burner SIM |
| **Twitter/X** | Yes (sometimes) | No | No | Same |
| **LinkedIn** | Email + phone for many features | Sometimes | Yes (real-name expected) | Hardest of the global platforms |
| **Facebook / Instagram** | Yes | Sometimes (verification) | Yes | Hard — Meta detects sock-puppets aggressively |
| **WeChat** | **Yes — CN preferred** | Sometimes | Sometimes | Very hard outside CN |
| **VK** | Yes | No | No | Russian SIM ideal |
| **Weibo** | **Yes — CN required** | No | No | Very hard outside CN |
| **Naver / Kakao** | KR phone + real-name verification | Yes | Yes | Effectively impossible non-resident |
| **Truth Social** | Email | No | No | Easy |

### SMS-receiving services (use carefully)
- **5sim.net, sms-activate.org, smspool, OnlineSim** — paid SMS-receiving by country
- Many platforms (Twitter, Meta, WhatsApp) detect VoIP / virtual numbers — physical SIMs from prepaid carriers are more durable
- **CN, KR, JP, DE numbers** are scarce and expensive on these services because of high demand

---

## Anti-Bot / Detection Considerations

When scraping or running sock-puppets:

| Signal | What it tells the platform |
|:-------|:--------------------------|
| **Datacenter IP** | "Almost certainly a bot" |
| **Residential proxy** | Better, but ASN heuristics catch reused exits |
| **Mobile IP** | Best — short of using a real mobile device |
| **Browser fingerprint** | Canvas/font/screen/audio quirks identify bots from real browsers |
| **TLS fingerprint (JA3)** | Differentiates Python `requests` from Chrome instantly |
| **Behavioural** | No mouse movement / no scroll / fast clicks = bot |
| **Account age + history** | New account with no posts and weird friends graph = sock-puppet |
| **Time-of-day patterns** | 24/7 activity = automation |

Tools to mitigate:
- **Playwright / Puppeteer with stealth plugins** instead of `requests`
- **Residential or mobile proxies** (Bright Data, Oxylabs, Smartproxy — paid)
- **Headed browsers** with random delays
- **Account ageing** — let an account live 2-4 weeks before using it for investigation
- **TLS fingerprint tools** — `curl_cffi`, `tls-client` libs that mimic Chrome's JA3

---

## See Also

- [[VMs_and_Compartmentalization]] — Sock-puppet hygiene, fingerprint hardening
- [[Browser_Extensions]] — Extensions for capture / scraping / fingerprint defence
- [[Tools_Kali_Tracelabs]] — CLI tools that wrap many of these APIs
- [[Commercial_Tools]] — Commercial monitoring platforms that operate at API scale
- [[OSS_Tools]] — Free scraping / API client libraries
- [[Regional_RUNet]] / [[Regional_China]] / [[Regional_Arabic]] — Regional platforms with their own auth quirks
- [[Country_Japan]] / [[Country_SouthKorea]] / [[Country_India]] / [[Country_Brazil]] — Country-specific platform notes
- [[OSINT]] — Folder index
