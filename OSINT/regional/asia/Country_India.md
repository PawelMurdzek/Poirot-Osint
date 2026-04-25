# Country OSINT — India (भारत)

The world's largest internet user base by population, multi-lingual ecosystem (22 official languages), uneven public-records access, dramatic platform consolidation around WhatsApp + Instagram + YouTube.

---

## Languages

India has **22 scheduled languages** in the Constitution. For OSINT:

- **English** — government, business, urban middle class, most major media
- **Hindi (हिंदी)** — Devanagari script, dominant in northern India + popular media
- **Regional**: Tamil (தமிழ்), Telugu (తెలుగు), Bengali (বাংলা), Marathi (मराठी), Gujarati (ગુજરાતી), Kannada (ಕನ್ನಡ), Malayalam (മലയാളം), Punjabi (ਪੰਜਾਬੀ), Urdu (اردو) — significant content in each
- **"Hinglish"** — heavy Roman-script transliteration on social media

Translation:
- **Google Translate** — solid for major Indian languages
- **Bhashini** ([bhashini.gov.in](https://bhashini.gov.in/)) — Indian government's NLP/translation initiative
- **Anuvaad** — Indian-language OCR + translation

---

## Search

- **Google.co.in** — dominant
- **Bing India** — niche
- **Indic / regional language searches** often need both English-transliteration and native script

---

## Social Networks

| Platform | Penetration | Notes |
|:---------|:------------|:------|
| **WhatsApp** | ~500M+ users — universal | Closed/encrypted but widespread "WhatsApp University" forwarding of misinformation; OSINT is mostly via screenshots |
| **YouTube** | Largest in the world | Regional creator economies in every major language |
| **Facebook** | ~300M+ | Strong in tier-2/3 cities |
| **Instagram** | Heavy | Especially urban under-30 |
| **X / Twitter** | Politically influential, less mass | Politicians, journalists, urban elite |
| **LinkedIn** | Largest user base globally | Professional OSINT goldmine for India |
| **TikTok** | **Banned since 2020** | Replaced by Moj, MX TakaTak, Roposo, Josh, Bolo |
| **ShareChat** ([sharechat.com](https://sharechat.com/)) | Regional-language social — huge in non-English India |
| **Koo** | Twitter clone, government-favoured, declining |
| **Telegram** | Growing, including for piracy / political channels |
| **Snapchat** | Youth |
| **Quora** | Active Indian community |
| **Reddit** (`r/India`, `r/IndianGaming`, regional subs) | Moderate but active |

### India-only / Indian-specific platforms
- **Moj, MX TakaTak, Josh, Roposo, Chingari** — short-video apps that filled the TikTok void
- **Hike** (now defunct) — was India's WhatsApp competitor
- **Yatra Locale community apps**

---

## Government & Corporate Records

| Source | Purpose |
|:-------|:--------|
| **MCA21** ([mca.gov.in](https://www.mca.gov.in/)) | Ministry of Corporate Affairs — company registry, directors, filings. Searchable but per-document fees |
| **SEBI** | Securities and Exchange Board — listed-company filings, similar to SEC EDGAR |
| **NSE / BSE** ([nseindia.com](https://www.nseindia.com/), [bseindia.com](https://www.bseindia.com/)) | Stock exchanges — disclosures, insider trading filings |
| **Income Tax e-filing** | Restricted — used by tax pros |
| **Aadhaar** | Universal 12-digit ID — *not* publicly searchable, but heavily abused / leaked over years |
| **eCourts** ([ecourts.gov.in](https://ecourts.gov.in/)) | Lower court case search — patchy, varies by state |
| **Supreme Court of India** ([sci.gov.in](https://www.sci.gov.in/)) | Apex court rulings |
| **High Court rulings** | Each High Court has its own portal |
| **PRS Legislative Research** | Legislation tracking |
| **Lokniti / CSDS** | Election studies |
| **Right to Information (RTI)** | Federal FOIA — `rtionline.gov.in` |
| **GST portal** | GSTIN lookup — `gst.gov.in` |
| **Voter rolls** | Nominally restricted but routinely leaked at state level |

> **Context:** Indian public-records access is "available but obstructive" — most data exists in databases, but UX is poor, watermarked PDFs, captchas, fees per-doc, and sometimes manual records-room visits required. RTI requests are powerful but have multi-month timelines.

---

## News & Media

### National (English)
- **The Hindu** ([thehindu.com](https://www.thehindu.com/)) — centre-left, established
- **Times of India** ([timesofindia.indiatimes.com](https://timesofindia.indiatimes.com/)) — largest English daily
- **Hindustan Times**
- **Indian Express** — investigative tradition
- **Deccan Chronicle / Deccan Herald** — South India focus
- **Mint** — financial
- **Business Standard, Economic Times** — business
- **NDTV** ([ndtv.com](https://www.ndtv.com/)) — TV/online
- **Republic World, Times Now** — pro-government TV
- **Frontline** — magazine, left-leaning

### Hindi
- **Dainik Jagran, Dainik Bhaskar, Hindustan, Amar Ujala, Rajasthan Patrika** — major Hindi dailies
- **Aaj Tak, India TV, ABP News** — Hindi TV news

### Regional language flagships
Each region has dominant language papers — e.g., **Eenadu** (Telugu), **Mathrubhumi** (Malayalam), **Anandabazar Patrika** (Bengali), **Lokmat** (Marathi).

### Online-native investigative
- **The Wire** ([thewire.in](https://thewire.in/)) — investigative
- **Caravan** ([caravanmagazine.in](https://caravanmagazine.in/)) — long-form
- **Scroll.in**
- **NewsLaundry** — media criticism
- **AltNews** ([altnews.in](https://www.altnews.in/)) — fact-check
- **BoomLive** — fact-check
- **The Print** — politics
- **The News Minute** — South India focus

---

## Aadhaar Context (Unique to India)

**Aadhaar** is a 12-digit Unique Identification Number issued to nearly every Indian resident. **It is not publicly searchable**, and unauthorised possession/disclosure is a criminal offence under the Aadhaar Act 2016.

- **Cumulative leaks** — over the past decade many state and private databases that included Aadhaar numbers have leaked. These leaks make many investigations possible that would normally not be.
- **OSINT-relevance:** Aadhaar number alone leaks demographic data via demo-API misuses; combined with name + DoB it's effectively a master key.
- **Legal warning:** Possession of leaked Aadhaar databases carries criminal liability. Don't hold these without explicit legal authorisation.

---

## Specifically Indian OSINT Tools

- **Indian Cyber Bureau resources**
- **Digipin** — geolocation system being rolled out
- **Bhuvan** ([bhuvan.nrsc.gov.in](https://bhuvan.nrsc.gov.in/)) — Indian government satellite imagery / mapping
- **eCourts API** — patchy but exists
- **mca.gov.in API** — limited
- **TrueCaller** — Indian-origin company, central to phone-number OSINT in India

---

## Notable OSINT Patterns / Cases

- **Election OSINT (2014, 2019, 2024):** mass campaigns mapped via WhatsApp groups + Twitter network analysis
- **Communal violence verification:** AltNews + BoomLive routinely use reverse image search + geolocation to debunk viral misinformation
- **2020 Delhi riots:** community OSINT geolocated incidents from social media
- **Cobalt Strike / espionage:** documented APT activity from / against India by Citizen Lab
- **Cricket / IPL match-fixing investigations** routinely use OSINT-style approaches

---

## OPSEC / Legal Notes

- **DPDP Act 2023** — India's GDPR-equivalent. Penalties for data-fiduciary breaches up to ₹250 crore.
- **IT Rules 2021 + amendments** — content moderation regulations affect what can be published about Indian individuals
- **Sedition law** (Section 124A IPC, periodically suspended/revived) and **UAPA** can apply to material on Indian government / security forces
- **Press freedom is precarious** — outlets and journalists periodically face raids, blocked websites
- **State-level surveillance** — Pegasus revelations (Citizen Lab) showed extensive use against journalists, activists

---

## See Also

- [[Country_USA]] — Indian diaspora OSINT often spans both jurisdictions
- [[Commercial_Tools]] — Skopenow / IDI Core have moderate India coverage
- [[Social_Media_APIs]] — ShareChat, Moj, Koo API specifics
- [[OSINT]] — Folder index
