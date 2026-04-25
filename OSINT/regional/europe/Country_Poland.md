# Country OSINT — Poland (Polska)

OSINT resources for Poland. Strong public-records access compared to neighbours, vibrant independent media, EU-aligned data-protection rules.

---

## Languages

- **Polish (Polski)** — single dominant language, Latin script with diacritics (ą ć ę ł ń ó ś ź ż)
- Searches without diacritics often work but missing them changes meaning ("zamek" castle vs "ząmek" — nonsense, but real diacritic mistakes change semantics)

---

## Search & Tooling

- Google.pl (with `lr=lang_pl`) is the default
- **Onet, WP, Interia** are major portals with their own search but mostly powered by Google/Bing under the hood
- **Allegro** is the Polish eBay/Amazon — useful for OSINT on individuals via marketplace listings/reviews

---

## Social Networks

| Platform | Penetration | Notes |
|:---------|:------------|:------|
| **Facebook** | Dominant | Most adults have one, common entry point |
| **Instagram** | Heavy among under-35s | |
| **X / Twitter** | Strong politically/professionally — journalists, politicians, public commentariat |
| **TikTok** | Growing fast |
| **LinkedIn** | Standard for professional OSINT |
| **YouTube** | Mainstream |
| **Telegram** | Niche, but growing on the political fringes |
| **Wykop.pl** ([wykop.pl](https://www.wykop.pl/)) | Polish Reddit/Digg hybrid — historically loud right-leaning, useful for tracking subculture trends |
| **Pudelek / plotek** | Celebrity gossip, often leak personal-life data |
| **Demotywatory** | Image-board / meme aggregator |
| **Mikroblog Wykop** | Twitter-like microblog inside Wykop |
| **Snapchat** | Common with under-25s |

---

## News & Media

### Public broadcaster
- **TVP** ([tvp.pl](https://www.tvp.pl/)) — public broadcaster, was state-aligned 2015-2023, now restructured under post-2023 government
- **Polskie Radio** — public radio
- **PAP** ([pap.pl](https://www.pap.pl/)) — Polish Press Agency

### Major commercial dailies / news
- **Gazeta Wyborcza** ([wyborcza.pl](https://wyborcza.pl/)) — centre-left, paywalled
- **Rzeczpospolita** ([rp.pl](https://www.rp.pl/)) — centre-right
- **Dziennik Gazeta Prawna**
- **Onet** ([onet.pl](https://www.onet.pl/)) — major portal/news
- **WP / Wirtualna Polska** ([wp.pl](https://www.wp.pl/))
- **Interia**
- **Polityka, Newsweek Polska, Wprost, Do Rzeczy** — weeklies
- **TVN24** — news channel (commercial)
- **Polsat News**
- **TV Republika, wPolsce.pl, Niezależna.pl** — right/conservative

### Independent investigative
- **OKO.press** ([oko.press](https://oko.press/)) — fact-check + investigative
- **Frontstory.pl** — investigative
- **Outriders** — multimedia investigative
- **Goniec, Onet investigations**
- **Konkret24** (TVN24 fact-check)
- **Demagog** — fact-check NGO

---

## Government & Corporate Registries

| Source | Purpose | Free? |
|:-------|:--------|:------|
| **KRS** (Krajowy Rejestr Sądowy) | National Court Register — companies, foundations, associations | Free at [ekrs.ms.gov.pl](https://ekrs.ms.gov.pl/) |
| **CEIDG** (Centralna Ewidencja i Informacja o Działalności Gospodarczej) | Sole-prop / individual business registry | Free at [aplikacja.ceidg.gov.pl](https://aplikacja.ceidg.gov.pl/) |
| **Monitor Sądowy i Gospodarczy (MSiG)** | Official gazette — corporate notices, bankruptcies, court notices | Free archive at [imsig.pl](https://www.imsig.pl/) |
| **REGON** ([wyszukiwarkaregon.stat.gov.pl](https://wyszukiwarkaregon.stat.gov.pl/)) | Statistical registry, sometimes catches what KRS doesn't | Free |
| **Geoportal** ([geoportal.gov.pl](https://www.geoportal.gov.pl/)) | Cadastral, parcel data, satellite layers | Free, very rich |
| **Elektroniczna Księga Wieczysta (EKW)** | Mortgage / property ownership registry | Free read at [ekw.ms.gov.pl](https://ekw.ms.gov.pl/) — need property number |
| **Portal Orzeczeń** ([orzeczenia.ms.gov.pl](https://orzeczenia.ms.gov.pl/)) | Court rulings (limited, anonymised) | Free |
| **NSA / SN** | Supreme administrative / supreme court rulings | Free |
| **BIK** (Biuro Informacji Kredytowej) | Credit registry | Paid, restricted |
| **CBO / Sanepid / UOKiK** | Consumer protection, regulatory decisions | Free |
| **PIT-y / KRD** | Debt registers | Paid |
| **EZD UMP** | Government doc register (varies by ministry) | FOIA-equivalent |

> Poland's **public-records ecosystem is unusually rich** for the EU — KRS, CEIDG, MSiG, EKW, and Geoportal between them give you ownership chains, property holdings, court history, and parcel data, all free. Strong starting point.

---

## Real Estate / Vehicles

- **Otodom** ([otodom.pl](https://www.otodom.pl/)) — primary real-estate listings
- **OLX** ([olx.pl](https://www.olx.pl/)) — classifieds, also real estate, vehicles, services
- **Allegro** ([allegro.pl](https://allegro.pl/)) — major marketplace, useful for individual seller research
- **Otomoto** ([otomoto.pl](https://www.otomoto.pl/)) — vehicle listings
- **Historiapojazdu.gov.pl** — official vehicle history (needs VIN + registration date)
- **CEPiK** — vehicle/driver registry, partial public

---

## Specifically Polish OSINT Tools / Sources

- **OOC.pl** — Polish company snapshot aggregator
- **EmIs Polska** — paid corporate intelligence
- **InfoCredit** — paid corporate
- **AI-Tools.pl** — directory of Polish-language LLMs / NLP for Polish text
- **mojepanstwo.pl** — civic-tech aggregator pulling from KRS/MSiG/etc.
- **Stop-cor.pl, Sieci Obywatelskie** — civic-monitoring groups
- **bdl.stat.gov.pl** — Local Data Bank, very granular regional stats

---

## OPSEC / Legal Notes

- **GDPR applies fully.** OSINT processing of PII for journalistic/research purposes has carve-outs but they're not unlimited. Document your basis if collecting PII at scale.
- **Poland has a SLAPP problem** — investigative journalists frequently face civil/criminal complaints. Defamation can be criminal (Article 212 of the Criminal Code, suspended sentences common).
- **Polish state surveillance** — Pegasus revelations (Citizen Lab) showed extensive 2017-2022 use against opposition lawyers, activists, journalists. Threat model accordingly if work is politically sensitive.
- **Ukrainian refugee context (post-2022)** — Poland hosts ~1M+ Ukrainians; OSINT on Ukrainian-Polish individuals often spans both ecosystems.

---

## See Also

- [[Country_Belarus]] — Many BY exile media operate from Poland
- [[Regional_RUNet]] — Russian-language content overlap, especially eastern Poland and refugee community
- [[OSS_Tools]] / [[Commercial_Tools]] — Maltego, etc., for working KRS/MSiG link analysis
- [[OSINT]] — Folder index
