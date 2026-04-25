# Country OSINT — South Korea (대한민국)

South Korea has its own platform ecosystem dominated by **Naver** and **Kakao** rather than Google/Meta. Real-name verification is required by law for many platforms, which makes OSINT both easier (real names) and harder (high security on registration).

---

## Language

- **Korean (한국어)** — Hangul script, agglutinative grammar
- **Hanja (漢字)** — Chinese characters used historically, occasionally in legal/historical contexts
- **Romanisation:** Multiple systems (Revised Romanization is current standard, McCune-Reischauer in older sources)
- **Translation:** **DeepL Korean** (best), **Papago** (Naver's, contextually strong), **Google Translate**
- Names: ~50% of Koreans share five surnames (Kim, Lee, Park, Choi, Jung) — so always combine with given name + middle names + birth year etc.

---

## Search

| Engine | Notes |
|:-------|:------|
| **Naver** ([naver.com](https://www.naver.com/)) | **Dominant** — search engine + portal + Cafe (community) + Blog + Knowledge IN (Q&A). Returns *Naver-internal* results first |
| **Daum** ([daum.net](https://www.daum.net/)) | Second portal, owned by Kakao. Has its own community + blog ecosystem |
| **Google.co.kr** | Strong but second to Naver |
| **Zum** ([zum.com](https://www.zum.com/)) | Niche portal |

> **Important:** Naver and Daum effectively *don't* expose much external Korean content via Google. To search Korean blog posts, Naver Cafe threads, or Daum communities you need to use those engines directly.

---

## Social Networks

### Messaging — KakaoTalk dominates
**KakaoTalk** ([kakao.com](https://www.kakao.com/)) — universal in Korea, ~50M users out of 51M population.
- **OSINT angle:** KakaoTalk profiles are usually private but the linked account (real name + phone) is required. Search by Kakao ID is restricted.
- **KakaoStory** — declining social network (think Facebook circa 2015)
- **Kakao Mail, Kakao Maps, Kakao Pay** — full ecosystem

### Communities — Naver Cafe, Daum Cafe
- **Naver Cafe** ([cafe.naver.com](https://cafe.naver.com/)) — invite-only / open Korean communities, *huge* OSINT surface for niche topics, fan groups, professional networks
- **Daum Cafe** — similar
- **DC Inside** ([dcinside.com](https://www.dcinside.com/)) — anonymous boards, gallery-per-topic, very influential, often the source of viral content
- **Theqoo** ([theqoo.net](https://theqoo.net/)) — celebrity / K-pop gossip
- **Pann** (now Pann Nate) — celebrity / drama gossip
- **MLBPark, Ruliweb, Inven** — niche community boards (sports, gaming, etc.)
- **FMKorea** — formerly soccer-focused, now general
- **Today Humor** ([todayhumor.co.kr](https://www.todayhumor.co.kr/)) — humor/community
- **Bobaedream** — automotive

### Microblogs / social
- **X / Twitter** — moderate use, K-pop fandom is enormous here
- **Instagram** — heavy use
- **Facebook** — declining
- **TikTok** — youth
- **LINE** — niche in Korea (Japanese platform — was developed by Korean company Naver)
- **Cyworld** — legacy social network from 2000s, attempted revivals

### Blog / content
- **Naver Blog** ([blog.naver.com](https://blog.naver.com/)) — dominant blogging platform in Korea
- **Tistory** ([tistory.com](https://www.tistory.com/)) — second blogging platform, owned by Kakao
- **Brunch** ([brunch.co.kr](https://brunch.co.kr/)) — Kakao's curated long-form
- **Egloos** — declining

### Video
- **YouTube** — dominant for video
- **Naver TV** — integrated with Naver portal
- **AfreecaTV / SOOP** ([afreecatv.com](https://www.afreecatv.com/)) — live streaming, predates Twitch in Korea
- **VLive** — K-pop celebrity live streams (now merged into Weverse)
- **Weverse** ([weverse.io](https://weverse.io/)) — K-pop fandom platform

---

## Maps & Geolocation

> **Important:** Google Maps in Korea is **legally restricted** — Korean export-control laws prohibit detailed map data leaving the country. Google Maps Korea has limited POI, no driving directions for many routes, no proper street view. **Always use Naver Maps or Kakao Maps for serious Korean geolocation.**

| Service | Notes |
|:--------|:------|
| **Naver Maps** ([map.naver.com](https://map.naver.com/)) | Dominant, best POI density, includes "거리뷰" street view |
| **Kakao Maps** ([map.kakao.com](https://map.kakao.com/)) | Strong second, "로드뷰" street view |
| **Google Maps** | Restricted — useful for satellite imagery only |
| **VWorld** ([map.vworld.kr](https://map.vworld.kr/)) | Government open-map portal, cadastral, LiDAR layers |

---

## News & Media

### Major dailies (with political alignment)
- **Chosun Ilbo** (조선일보) — conservative, largest circulation
- **JoongAng Ilbo** (중앙일보) — centre-right
- **Donga Ilbo** (동아일보) — conservative
  - These three are sometimes called "조중동" (the conservative big-three)
- **Hankyoreh** (한겨레) — progressive
- **Kyunghyang Shinmun** (경향신문) — progressive
- **OhmyNews** ([ohmynews.com](https://www.ohmynews.com/)) — citizen journalism, progressive

### Public broadcaster
- **KBS** ([kbs.co.kr](https://www.kbs.co.kr/)) — public TV
- **MBC, SBS** — major commercial TV

### Online-native
- **Yonhap News** (연합뉴스) — major wire agency
- **Newsis, News1** — wire services
- **Pressian, Media Today** — independent
- **The Korea Times, Korea Herald** — English-language

### Investigative
- **Newstapa** ([newstapa.org](https://newstapa.org/)) — investigative non-profit
- **The Hankyoreh21** — magazine
- **Sisain** ([sisain.co.kr](https://www.sisain.co.kr/)) — weekly investigative

---

## Government & Corporate Records

| Source | Purpose |
|:-------|:--------|
| **DART** ([dart.fss.or.kr](https://dart.fss.or.kr/)) | Corporate disclosure system (KR equivalent of EDGAR). Listed companies + selected unlisted |
| **NTS (National Tax Service)** | Tax registration, very limited public lookup |
| **Court Information** ([scourt.go.kr](https://www.scourt.go.kr/)) | Supreme Court rulings; lower court rulings via paid subscription (KSCO) |
| **iros.go.kr** | Real estate registry — paid per query, ~1,000 KRW |
| **Government portal** ([korea.go.kr](https://www.korea.go.kr/)) | Government info aggregator |
| **e-Country (e-나라표준인증)** | Procurement |
| **Patent (KIPRIS)** ([kipris.or.kr](https://www.kipris.or.kr/)) | Patent search |
| **Statistics Korea (KOSIS)** ([kosis.kr](https://kosis.kr/)) | Statistics |
| **Open Data Portal** ([data.go.kr](https://www.data.go.kr/)) | Free open-government datasets |

---

## Real-Name Verification (실명인증) — Critical OSINT Concept

Most major Korean platforms (Naver, Kakao, DC Inside, gaming) require **real-name verification** tied to the **Resident Registration Number (주민등록번호)** for adult sign-up. This means:

- **Korean accounts are usually identifiable** to the platform — but not necessarily to outside observers
- **Foreigners can register** via i-PIN or alternative phone-verification, but the friction is high
- **Sock-puppet creation is hard** for non-residents — most paid SMS-receiving services don't have KR numbers
- **Anonymous-looking accounts may be more identifiable to the platform** than equivalents in the US/EU
- **Leaked databases** of registration data have been frequent — a 2014 KT leak alone was 12M users

---

## Specifically Korean OSINT Tools

- **인스피언** / **Cobaltstrike-KR** (private channels)
- **Korean SecLists subset** — username/password wordlists oriented to Korean keyboard / Romanization patterns
- **Naver Open API** — search, image search, blog search via official API (key required)
- **Open Data Portal API** — government datasets

---

## Notable OSINT Cases

- **Sewol Ferry tragedy (2014):** community OSINT (community-board users + AIS data) tracked the ship's pre-disaster history
- **Itaewon crowd crush (2022):** crowd-sourced video frame analysis identified critical decisions
- **K-pop industry doxing:** **tokutei**-style (cf. [[Country_Japan]]) celebrity-identity reveals are constant on Theqoo / Pann; methodology mirrors JP
- **North Korean defector tracking:** Daily NK and similar use OSINT to corroborate defector accounts; see [[Country_NorthKorea]]

---

## OPSEC / Legal Notes

- **PIPA (Personal Information Protection Act)** is GDPR-strict. Processing of Korean residents' personal data offshore can require notification.
- **Defamation (명예훼손)** is criminally liable in Korea, including for *truthful* statements that harm reputation without sufficient public interest. Be careful publishing identifying details.
- **National Security Act** can apply to "anti-state" content — any work touching North Korea or pro-DPRK material can intersect this.
- **Korean LE has strong cybercrime cooperation** with most Western jurisdictions; less so with CN.

---

## See Also

- [[Country_Japan]] — Adjacent ecosystem; LINE crosses both
- [[Country_NorthKorea]] — Critical context for KR-focused work
- [[Social_Media_APIs]] — Naver, KakaoTalk, AfreecaTV API specifics
- [[OSINT]] — Folder index
