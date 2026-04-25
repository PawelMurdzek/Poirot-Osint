# Regional OSINT — Chinese Internet (中国互联网)

OSINT sources for the Chinese-language internet inside the Great Firewall (GFW). Covers Mainland China primarily; Hong Kong, Taiwan, Macao, and Singapore overlap but have their own ecosystems.

> [!IMPORTANT]
> Most of these platforms require a **Chinese phone number for registration**, geo-fence outside CN IP ranges, and have strong anti-bot. Plan for a CN-routed exit and a CN sock-puppet identity ([[VMs_and_Compartmentalization]]). Some Chinese platforms log out / block accounts that geolocate to certain countries.

---

## Search Engines

| Engine | Notes |
|:-------|:------|
| **Baidu** ([baidu.com](https://www.baidu.com/)) | Dominant in CN. Heavily censored. Aggressive on Mandarin language; weaker on English |
| **Baidu Images** ([image.baidu.com](https://image.baidu.com/)) | Reverse image search, biased toward CN-hosted images |
| **Sogou** ([sogou.com](https://www.sogou.com/)) | Tencent-owned. Distinctive: indexes WeChat public-account articles (otherwise hard to reach via search) |
| **360 Search / so.com** ([so.com](https://www.so.com/)) | Qihoo 360 — antivirus vendor's search engine |
| **Bing China** | Microsoft's China-compliant Bing — different result set than global Bing |
| **Yisou (神马)** | Mobile-focused, Alibaba |

> Many global Google dorks fail on Baidu — Baidu's `site:` operator works, but quoted phrases and `intitle:` are unreliable. Translate queries to simplified Chinese (`简体中文`) for best results.

---

## Social Networks

### Sina Weibo (新浪微博)
The dominant CN microblog. Often called "Chinese Twitter" but with its own culture and heavier censorship.
- [weibo.com](https://weibo.com/)
- Profile pattern: `weibo.com/u/<numeric_id>` or `weibo.com/<vanity>`
- **Topics (#话题#)** are the equivalent of hashtags; trending topics are often manually curated/censored
- **Tools:** weiboscraper, snscrape weibo, Weibo Search Engine

### WeChat (微信)
The dominant Chinese super-app. Messaging + payments + social ("Moments") + mini-apps + public accounts.

> WeChat is **closed** — there is no public API, friend lists are private, Moments aren't searchable from the outside. From an OSINT perspective WeChat is mostly a black box except for **WeChat Public Accounts** ("公众号").

| Resource | Use |
|:---------|:----|
| **Sogou WeChat search** ([weixin.sogou.com](https://weixin.sogou.com/)) | Search public-account articles |
| **WeChatScope** | Academic project tracking censored articles |
| **WeChatLog** | News aggregator built on public accounts |

### Douyin (抖音) / TikTok mainland
Mainland Chinese TikTok — separate platform, separate content. **TikTok International (`tiktok.com`) is blocked in CN; Douyin (`douyin.com`) is blocked outside CN.**
- [douyin.com](https://www.douyin.com/)
- Profile pattern: `douyin.com/user/<id>`

### Xiaohongshu (小红书 / "Little Red Book / RedNote")
Lifestyle / shopping / travel social network, growing fast internationally in 2024-2025.
- [xiaohongshu.com](https://www.xiaohongshu.com/)

### Zhihu (知乎)
Chinese Quora — long-form Q&A, more intellectual demographic.
- [zhihu.com](https://www.zhihu.com/)

### Bilibili (哔哩哔哩 / B站)
Video platform, originally anime/games-focused, now broad. Strong youth demographic.
- [bilibili.com](https://www.bilibili.com/)

### Baidu Tieba (百度贴吧)
Massive interest-based forum network — `tieba.baidu.com/f?kw=<keyword>`.

### Maimai (脉脉)
Chinese LinkedIn equivalent — note: Chinese LinkedIn shut down in 2023, so Maimai gained importance for professional OSINT.
- [maimai.cn](https://maimai.cn/)

### QQ
Tencent's older messenger. QQ Numbers (`<8-12 digit>`) are persistent identifiers, often used as usernames across CN sites.
- [qq.com](https://www.qq.com/)
- **Pivot:** QQ number → check Weibo, Bilibili, Zhihu for the same number used as username

---

## Maps & Geolocation

> Chinese maps use the **GCJ-02 ("Mars Coordinates") obfuscated coordinate system** by law — a non-trivial offset from WGS-84 (GPS). Plotting Chinese-source coordinates on Google/OSM directly is wrong by 50–500m. Convert with `eviltransform` or similar libraries.

| Service | Notes |
|:--------|:------|
| **Baidu Maps** ([map.baidu.com](https://map.baidu.com/)) | Dominant, includes street view |
| **AMap / Gaode (高德)** ([amap.com](https://www.amap.com/)) | Alibaba — best POI density, used in Apple Maps for CN |
| **Tencent Maps** ([map.qq.com](https://map.qq.com/)) | Powers WeChat location features |
| **Google Maps in CN** | Only shows GCJ-02 coords, limited POI, no street view |

---

## Corporate Registries — Critical for Sanctions / Due Diligence

| Service | Coverage | URL |
|:--------|:---------|:----|
| **Tianyancha (天眼查)** | Corporate intel, beneficial ownership, litigation, IP, hiring | [tianyancha.com](https://www.tianyancha.com/) — partial free, paid for full |
| **Qichacha (企查查)** | Same niche as Tianyancha, complementary | [qcc.com](https://www.qcc.com/) — paid |
| **Aiqicha (爱企查)** | Baidu-owned, free tier larger than the others | [aiqicha.baidu.com](https://aiqicha.baidu.com/) |
| **Qixin (启信宝)** | Paid corporate intel | [qixin.com](https://www.qixin.com/) |
| **National Enterprise Credit Information Publicity System (NECIPS)** | Official government registry | [gsxt.gov.cn](https://www.gsxt.gov.cn/) — slow, partial |
| **CSRC** | Listed-company filings | [csrc.gov.cn](https://www.csrc.gov.cn/) |
| **CNINFO** | Listed-company disclosures | [cninfo.com.cn](https://www.cninfo.com.cn/) |

> **Beneficial ownership pivots** in CN are non-trivial — companies routinely use nominee shareholders, VIE structures (variable-interest entities), and offshore (BVI, Cayman) holding companies. Tianyancha + OpenCorporates + cross-checks via SEC EDGAR (for US-listed) is the typical chain.

---

## Government & Regulatory

| Source | Purpose |
|:-------|:--------|
| **State Council (gov.cn)** | Official notices |
| **MOFA / 外交部** | Foreign-affairs press releases |
| **PBoC** | Central bank data |
| **National Bureau of Statistics** | stats.gov.cn |
| **MIIT (工信部)** ([miit.gov.cn](https://www.miit.gov.cn/)) | Industry regulations, ICP licence database (every CN-hosted site needs an ICP) |
| **ICP Beian Search** | `beian.miit.gov.cn` — domain-to-licensee lookup |
| **CNVD** ([cnvd.org.cn](https://www.cnvd.org.cn/)) | China National Vulnerability Database |

---

## Internet Infrastructure (CN-focused)

| Service | Notes |
|:--------|:------|
| **FOFA** ([fofa.info](https://fofa.info/)) | Chinese counterpart to Shodan, far better CN/AP coverage |
| **ZoomEye** ([zoomeye.org](https://www.zoomeye.org/)) | Same idea, by Knownsec |
| **Censys / Shodan** | Use for cross-reference; FOFA usually has more CN data |
| **APNIC WHOIS** | Asia-Pacific IP allocation |
| **CNNIC WHOIS** | `.cn` domain WHOIS (often heavily redacted) |
| **Quake** ([quake.360.cn](https://quake.360.cn/)) | Qihoo 360's device search |

---

## News & Media

### State / official
- **Xinhua** (xinhuanet.com), **People's Daily** (people.com.cn), **CCTV** (cctv.com), **Global Times** (globaltimes.cn — English-language state outlet)

### Commercial mainland
- **Caixin** (caixin.com) — relatively independent business journalism
- **The Paper** (thepaper.cn)
- **Sina** (sina.com.cn) — portal
- **Sohu** (sohu.com) — portal
- **NetEase / 163** (163.com)

### Hong Kong / Taiwan (different ecosystems, less censored)
- **South China Morning Post** (HK, scmp.com)
- **HK01** (HK, hk01.com)
- **Apple Daily / Next Digital** — shut down (HK)
- **Taiwan: Liberty Times, China Times, UDN, Storm Media**

### Diaspora / critical
- **China Digital Times** ([chinadigitaltimes.net](https://chinadigitaltimes.net/)) — censorship monitor (US-based)
- **What's on Weibo** — culture / OSINT-style commentary
- **SupChina / The China Project** (now defunct)
- **Sinocism** (newsletter)

---

## Censorship & Tracking Tools

| Tool | Use |
|:-----|:----|
| **GreatFire.org** | Tracks blocked sites in real-time, has alternate URLs / mirrors |
| **Citizen Lab** | Toronto-based research lab, reports on CN surveillance and censorship |
| **OONI** | Open Observatory of Network Interference — measures GFW behaviour |
| **China Digital Times Lexicon** | List of censored search terms |
| **WeChatScope** | Hong Kong U project tracking deleted WeChat articles |

---

## Translation

- **DeepL** — best CN→EN of the major engines
- **Google Translate** — fast, reasonable
- **Baidu Translate** — sometimes catches CN slang/internet-speak others miss
- **Pleco** — dictionary app (mobile), gold standard for individual character lookup

---

## OPSEC Notes

- **GFW reciprocity:** investigating CN platforms from outside CN often works for read-only access; investigating non-CN platforms from inside CN requires a VPN.
- **Phone-number gating:** registering for Weibo, Douyin, WeChat needs a CN-issued phone number. SMS-receiving services rarely have CN numbers; some platforms detect and block VoIP / virtual numbers.
- **Don't log in with anything tied to your identity.** WeChat has been documented to share data with PRC authorities upon request, including for non-CN nationals.
- **Field investigations inside CN** carry their own risk profile. Out of scope for this note — see Citizen Lab and FreedomHouse threat models.

---

## See Also

- [[OSINT]] — Folder index and methodology
- [[Regional_RUNet]] / [[Regional_Arabic]] — Other regional ecosystems
- [[VMs_and_Compartmentalization]] — Sock-puppet hygiene
- [[Browser_Extensions]] — Translation extensions, RevEye for Baidu Images
- [[Tools_Kali_Tracelabs]] — Snscrape works on Weibo, telethon for Telegram channels covering CN topics
