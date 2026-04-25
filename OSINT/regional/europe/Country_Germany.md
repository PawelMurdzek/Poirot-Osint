# Country OSINT — Germany (Deutschland)

Germany has rich corporate/legal records but **strong privacy norms** that make personal-OSINT work harder than in the US/UK. The legal framework (BDSG + GDPR + Bundesverfassungsgericht "informational self-determination" doctrine) is among the strictest globally.

---

## Languages

- **German (Deutsch)** — single dominant language, Latin script + ä, ö, ü, ß
- Spelling: ß can substitute "ss" (older texts use "ss"). Both work in search.
- Translation: **DeepL** (German-built, native quality), Google Translate

---

## Search

- **Google.de** — dominant
- **Bing Germany** — niche
- **Ecosia** ([ecosia.org](https://www.ecosia.org/)) — German-built, privacy-focused, popular alternative
- **MetaGer** ([metager.org](https://metager.org/)) — German privacy-focused meta-search

---

## Social Networks

| Platform | German use |
|:---------|:-----------|
| **WhatsApp** | Universal |
| **Facebook** | Mainstream, declining among young |
| **Instagram** | Heavy |
| **YouTube** | Mainstream |
| **TikTok** | Heavy youth |
| **LinkedIn** | Common professional use |
| **XING** ([xing.com](https://www.xing.com/)) | German-language LinkedIn alternative — declining but still relevant for older professionals |
| **Twitter/X** | Smaller than US/UK; politicians and journalists |
| **Telegram** | **Higher penetration than most Western countries** — heavy use by COVID-conspiracy / far-right movements (Reichsbürger, Querdenken); also legitimate news channels |
| **Mastodon** | Strong German community — `chaos.social`, `social.tchncs.de`, `troet.cafe` are major DE-language instances |
| **Reddit** (`r/de`, `r/germany`, `r/Finanzen`) | Active mid-sized communities |
| **WT.Social, MeWe** | Niche |
| **StudiVZ / SchülerVZ** | Defunct legacy networks (early-2010s OSINT) |
| **Jodel** | Anonymous location-based, popular with students |

---

## Government & Corporate Records

| Source | Purpose |
|:-------|:--------|
| **Handelsregister** ([handelsregister.de](https://www.handelsregister.de/)) | Commercial registry — companies, directors, registered address. Free since August 2022 (previously paid per-doc) |
| **Unternehmensregister** ([unternehmensregister.de](https://www.unternehmensregister.de/)) | Aggregator of corporate filings, financial statements |
| **Bundesanzeiger** ([bundesanzeiger.de](https://www.bundesanzeiger.de/)) | Federal Gazette — annual accounts (mandatory disclosure for many companies), corporate notices, insolvency, ESG reports |
| **Transparenzregister** ([transparenzregister.de](https://www.transparenzregister.de/)) | Beneficial-ownership registry — paid + identity-verification required for access (post-2024 reforms) |
| **DE Bundestag / Bundesrat** | Legislative records |
| **German Patent and Trade Mark Office (DPMA)** | Patents, trademarks |
| **Court records** | NOT systematically online; **Rechtsprechung im Internet** (rechtsprechung-im-internet.de) covers federal court decisions |
| **Bundesverwaltungsamt / Bundeszentralregister** | Federal Central Register — restricted; criminal records not public |
| **Real estate (Grundbuch)** | NOT publicly accessible — restricted to those with legitimate interest |
| **Vehicle registry (KBA)** | Largely restricted |
| **BaFin** ([bafin.de](https://www.bafin.de/)) | Financial regulator — listed-company filings, FinTec sanctions |
| **Federal Statistical Office (Destatis)** | Statistics |

> Beneficial-ownership work post-2022 GDPR ruling: the European Court of Justice **invalidated public access to BO registers**. Germany's Transparenzregister now requires "legitimate interest" demonstration — journalists, NGOs, and KYC professionals can access; casual public cannot.

---

## Real Estate / Vehicles

- **ImmoScout24, Immowelt, eBay Kleinanzeigen** — listings
- **Geoportals** (state-level) — cadastral / building permits
- **Vehicle records** — restricted, occasional commercial workarounds
- **TÜV** records — vehicle inspection histories accessible only to owner

---

## News & Media

### Major dailies / weeklies
- **Süddeutsche Zeitung (SZ)** — centre-left, investigative tradition (Panama Papers etc.)
- **Frankfurter Allgemeine Zeitung (FAZ)** — centre-right, intellectual
- **Die Welt** — conservative
- **Bild** — tabloid, largest circulation
- **Die Zeit** — weekly, intellectual centrist
- **Der Spiegel** ([spiegel.de](https://www.spiegel.de/)) — weekly news magazine, major investigative
- **Stern, Focus** — weeklies
- **Handelsblatt, Wirtschaftswoche** — business

### Public broadcasters (ARD / ZDF system)
- **ARD** (multi-station consortium), **ZDF** (single second channel), **Deutsche Welle (DW)** for international
- **tagesschau.de**, **heute.de** — flagship news sites

### Online-native / investigative
- **Correctiv** ([correctiv.org](https://correctiv.org/)) — investigative non-profit, Panama Papers consortium member
- **Netzpolitik.org** — digital-rights / surveillance focus
- **Übermedien** — media criticism
- **Krautreporter** — long-form online
- **Volksverpetzer** — fact-check / political watchdog

### Right / fringe
- **Junge Freiheit** — conservative weekly
- **Tichys Einblick, Compact, Achgut** — varying degrees of right
- **Nius** — Bild publisher's right-leaning project (post-2023)

---

## Specifically German OSINT Tools

- **OpenStreetMap Germany** — strong DE community
- **Geofabrik** — OSM data extracts
- **Mapnik** Germany style
- **NorthData** ([northdata.com](https://www.northdata.com/)) — paid corporate intelligence with strong DE coverage
- **Bisnode / Dun & Bradstreet** — paid
- **Implisense** — paid corporate
- **CompanyHouse.de** — corporate aggregator
- **FragdenStaat** ([fragdenstaat.de](https://fragdenstaat.de/)) — German FOIA portal, modeled on MuckRock

---

## OPSEC / Legal Notes

- **GDPR + BDSG (Bundesdatenschutzgesetz)** apply strictly — German data-protection authorities are aggressive
- **Right to be forgotten** is enforced — Google removes personal-name results that meet GDPR criteria more often in DE than most jurisdictions
- **Defamation** (§ 185-187 StGB) — criminal offences. **Persönlichkeitsrecht** (personality right) is constitutionally enshrined and provides civil remedies separate from defamation
- **Section 12 BGB** — name protection
- **Image rights (Recht am eigenen Bild)** — publishing photos without consent is restricted
- **Telecommunications surveillance** — German police OSINT operates under § 100a-100j StPO; private OSINT mostly outside that frame
- **Press shield** — strong protection for journalists, but not for non-credentialed researchers
- **Far-right monitoring** — Verfassungsschutz (BfV) and BKA actively monitor; analysts in this area are visible

---

## Notable OSINT Patterns

- **Far-right / extremism research** — strong tradition, often by NGO + academic combination (e.g., Amadeu Antonio Stiftung)
- **Sanctions evasion (post-2022 Russian-Ukraine war)** — German-incorporated shell companies have been used for evasion, well-documented by Correctiv / OCCRP
- **Corporate compliance / NS-era restitution** — long-running historical-OSINT use
- **Telegram / Querdenken / Reichsbürger monitoring** — extensive academic + journalistic work since 2020

---

## See Also

- [[Country_Poland]] — DE-PL border, both countries' KRS / Handelsregister are useful for cross-border due diligence
- [[Country_UK]] / [[Country_USA]] — Compare corporate-registry openness
- [[Country_Belarus]] / [[Country_Ukraine]] — Many exile media operations from Germany (Berlin)
- [[OSINT]] — Folder index
