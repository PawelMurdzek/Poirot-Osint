# Country OSINT — Japan (日本)

Japan's internet ecosystem is unusual: many globally niche platforms are dominant locally (LINE, 5ch, Niconico, Mixi). Privacy norms are conservative; explicit linkage of identity to public posts is uncommon — which makes Japanese OSINT methodology different.

---

## Languages & Script

- **Japanese (日本語)** — three scripts mixed: **Kanji** (漢字, Chinese characters), **Hiragana** (ひらがな, native syllabary), **Katakana** (カタカナ, foreign-loanword syllabary).
- **Romaji (ローマ字)** — Latin transliteration; conventions vary (Hepburn, Kunrei). Searches often need both Romaji and original-script forms.
- **Names:** A Japanese name may have multiple valid kanji renderings for the same pronunciation (e.g., 田中 たかし could be Takashi spelled 隆 / 崇 / 貴 etc.). Cross-reference is harder than in alphabetic languages.

### Translation
- **DeepL** — by far the best for Japanese-English
- **Google Translate** — reasonable
- **Mazii / Jisho** ([jisho.org](https://jisho.org/)) — kanji + word lookup, essential for verifying names and place-names

---

## Search

- **Yahoo Japan** ([yahoo.co.jp](https://www.yahoo.co.jp/)) — surprisingly major, separate ecosystem from Yahoo US (owned by SoftBank/Z Holdings since 2019). Historically larger market share than Google in Japan, narrowly so today.
- **Google.co.jp** — competitive
- **Bing Japan** — niche

---

## Social Networks

### Messaging — LINE dominates
**LINE** ([line.me](https://line.me/)) — ~80M users in Japan, the dominant messenger. Stickers, group chats, payments, news, mini-apps.
- **OSINT angle:** LINE accounts are tied to phone numbers; profile photos and display names are usually public to anyone who knows the user ID. The "LINE ID" search is heavily restricted.
- LINE has been documented to share metadata with Korean / Chinese affiliates — privacy concerns recurring.

### Microblogs / social
- **X / Twitter** — disproportionately popular in Japan, the *highest* user-per-capita country (cultural fit with anonymous short-form posting). Major OSINT surface.
- **Mixi** ([mixi.jp](https://mixi.jp/)) — once-dominant, now in decline. Older demographic, gaming pivot. Niche but archives are gold for 2005–2015 individual histories.
- **Mastodon** — Japanese instances (`mstdn.jp`, `pawoo.net`) are some of the largest globally. **Pawoo** specifically is/was popular for adult-content posting that left mainstream platforms.
- **Bluesky** — rising, with a strong Japanese fandom community

### Imageboards (anonymous, hugely influential)
- **5channel / 5ch** ([5ch.net](https://5ch.net/)) — successor to **2channel (2ch)**, the original anonymous imageboard. Massively influential on Japanese internet culture; spawned 4chan globally. Search via `https://find.5ch.net/`
- **Open2ch / **etc. — splinters from the 2009 ownership disputes
- **Futaba Channel (2chan, ふたば☆ちゃんねる)** — the *image* board that inspired 4chan

### Video / creative
- **Niconico Douga (ニコニコ動画)** ([nicovideo.jp](https://www.nicovideo.jp/)) — video sharing, distinctive overlay-comment culture
- **Pixiv** ([pixiv.net](https://www.pixiv.net/)) — illustration / fanart platform, dominant in Japan-origin creative scene
- **Booth** ([booth.pm](https://booth.pm/)) — Pixiv's marketplace
- **YouTube** — mainstream, large Japanese creator community
- **TikTok** — youth

### Blogging / content
- **Note** ([note.com](https://note.com/)) — Substack-like, popular with journalists/writers/professionals
- **Hatena Blog** ([hatenablog.com](https://hatenablog.com/)) — long-form blogs, somewhat techie demographic
- **Ameba Blog (Ameblo)** — celebrity-blog hub
- **Livedoor Blog** — declining but huge archives

### Career / professional
- **LinkedIn** — used but not dominant
- **Wantedly** ([wantedly.com](https://www.wantedly.com/)) — Japan-specific career platform
- **BizReach, Doda** — recruiting platforms

---

## News & Media

### Major dailies ("five big newspapers")
- **Yomiuri Shimbun** (読売新聞) — largest circulation globally, conservative-leaning
- **Asahi Shimbun** (朝日新聞) — left-of-centre
- **Mainichi Shimbun** (毎日新聞) — centre
- **Nikkei** (日本経済新聞) — financial / business; English version `asia.nikkei.com`
- **Sankei Shimbun** (産経新聞) — right

### Public broadcaster
- **NHK** ([nhk.or.jp](https://www.nhk.or.jp/)) — public broadcaster, also `NHK World` (English international)

### Online-native / English-language Japan news
- **Japan Times** (eng) — older institution
- **Mainichi English**
- **Asahi English**
- **Nippon.com**
- **Tokyo Reporter** — crime-focused English-language

### Niche / investigative
- **Tansa** ([tansajp.org](https://tansajp.org/)) — investigative non-profit
- **Bunshun (週刊文春)** — magazine, breaks scandals routinely (politicians, celebrities)
- **Shukan Bunshun, Shukan Shincho, Shukan Post, Friday** — weekly tabloid magazines, massive scoop-driven culture

---

## Government & Corporate Records

| Source | Purpose |
|:-------|:--------|
| **National Tax Agency 国税庁** ([nta.go.jp](https://www.nta.go.jp/)) | Corporate tax registration, partial public lookup |
| **Houjin-Bangou (法人番号)** ([houjin-bangou.nta.go.jp](https://www.houjin-bangou.nta.go.jp/)) | Corporate Number lookup — every registered company has a 13-digit Houjin Bangou |
| **METI (経済産業省)** | Industry filings |
| **EDINET** ([disclosure.edinet-fsa.go.jp](https://disclosure2.edinet-fsa.go.jp/)) | Listed-company financial filings (Japanese SEC equivalent) |
| **FSA Japan** | Financial Services Agency registers |
| **JIPDEC** | Privacy registrations |
| **Court records** | Largely not online; physical access at courts. Major rulings via 裁判所 ([courts.go.jp](https://www.courts.go.jp/)) |
| **JPNIC WHOIS** | `.jp` domain registrant lookup ([whois.nic.ad.jp](https://whois.nic.ad.jp/)) |
| **JPRS** | `.jp` registry data, structured reports |
| **MLIT** | Ministry of Land — vehicle registration lookups, partial |
| **City offices (役所 yakusho)** | Family registers (戸籍) — restricted access, requires relationship |

---

## Specifically Japanese OSINT Tools

- **Tofugu / Imiwa / Yomiwa** — kanji/name lookup tools
- **Postal Code Search** ([postcode.jp](https://postcode.jp/)) — Japanese addresses are weird (block + lot, not street + number); postal-code lookup is essential
- **iタウンページ** ([itp.ne.jp](https://itp.ne.jp/)) — Yellow Pages
- **Mapion** ([mapion.co.jp](https://www.mapion.co.jp/)) — Japan-detailed map service

### Maps
- **Google Maps Japan** — works well
- **Mapion**, **Mapfan**, **NAVITIME**, **Goo Map** — Japanese alternatives, sometimes more detailed POI

---

## Cultural OSINT Notes

- **Japanese privacy norms:** real names are routinely *not* attached to social-media posts. Twitter handles are typically pseudonymous; many users have multiple accounts (`本垢` real-account, `裏垢` private-account) compartmentalising identity.
- **"Rear account" (裏垢) culture** — users post sensitive content on a separate "private" account, often loosely tied (same friends, similar timing) to their public account. This linkage is exactly where OSINT lives.
- **Online "dox" culture (特定 *tokutei*)** — coordinated 5ch/Twitter sleuthing identifies posters of viral content, often within hours. Fast and unforgiving.
- **Names + workplace + school photos** — combining these from disparate platforms is the standard *tokutei* methodology.

---

## OPSEC / Legal Notes

- **Personal Information Protection Act (個人情報保護法 / APPI)** — Japan's GDPR-equivalent. Less aggressive than GDPR but does cover OSINT outputs at scale.
- **Defamation (名誉毀損)** is criminally liable in Japan. Truth is *not* always a defence (only public-interest truth is). Investigative reporting on private individuals is risky.
- **Japan has cooperative LE relationships with most Western jurisdictions** but does *not* extradite its own nationals. Has historical patchy cooperation on cybercrime.

---

## See Also

- [[Country_SouthKorea]] — Adjacent platform-ecosystem (Naver/Kakao/etc.); some Japan-Korea overlap (LINE was Korean-owned until recently)
- [[Regional_China]] — Different ecosystem entirely; CN content is *not* indexed by Japanese platforms
- [[Social_Media_APIs]] — LINE / Mixi / Niconico API details
- [[OSINT]] — Folder index
