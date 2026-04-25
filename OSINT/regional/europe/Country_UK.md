# Country OSINT — United Kingdom

The UK has one of the most transparent corporate registries globally (**Companies House**) and a strong OSINT/investigative-journalism community. Other public records are more restricted.

---

## Companies House — The Crown Jewel

**[find-and-update.company-information.service.gov.uk](https://find-and-update.company-information.service.gov.uk/)** — free, no auth, full-text searchable.

What you get for every UK company:
- Full registered office history
- All officers (directors, secretaries) with date of birth (month + year only since 2015), nationality, occupation
- Persons of Significant Control (PSC) — beneficial-ownership declarations >25%
- Charges (mortgages, debentures)
- All filing history — annual returns, accounts, name changes, dissolutions
- Document downloads (PDF) — incorporation docs, accounts, articles of association

```bash
# Companies House also has a free REST API (10K calls/day per key)
# Apply at developer.companieshouse.gov.uk
curl -u "API_KEY:" \
  "https://api.company-information.service.gov.uk/search/companies?q=acme"
```

> The PSC register has been cited in countless OCCRP / ICIJ investigations. False PSC filings carry criminal liability — but enforcement is weak, so they're widely abused for shell companies. Expect noise.

---

## Land Registry

**[gov.uk/search-property-information-land-registry](https://www.gov.uk/search-property-information-land-registry)** — paid (£3 per title) but cheap and rich.

- **Title register** — current owner, charges (mortgages), restrictions
- **Title plan** — boundary
- **Historical title information** (some pages free at `landregistry.data.gov.uk`)
- **OCOD (Overseas Companies Owning Land)** — bulk dataset, free; scandal-fuel for offshore property holdings
- **Price Paid Data** — bulk free dataset of all transactions since 1995

---

## Other Government & Regulatory

| Source | Purpose |
|:-------|:--------|
| **Charity Commission** ([register-of-charities.charitycommission.gov.uk](https://register-of-charities.charitycommission.gov.uk/)) | All registered charities, trustees, accounts |
| **OSCR (Scotland)** | Scottish charities |
| **CCNI (NI)** | Northern Ireland charities |
| **The Gazette** ([thegazette.co.uk](https://www.thegazette.co.uk/)) | Official gazette — bankruptcy, dissolutions, military commissions |
| **Insolvency Service** | Bankruptcy / insolvency register |
| **Electoral Commission** | Political parties, donations |
| **DVLA** | Vehicle records — not free public, available via paid 3rd-party (`vehicle-enquiry`) for limited fields |
| **CAA G-INFO** | UK aircraft registry |
| **Office for Students** | Higher-education registrations |
| **Ofcom** | Broadcasting, telecoms, spectrum |
| **HSE** | Health & Safety Executive — incident notices |
| **FCA** ([register.fca.org.uk](https://register.fca.org.uk/)) | Financial Conduct Authority — registered persons, firms, sanctions |
| **HMRC** | VAT registry partial via `gov.uk/check-vat-number` |

---

## Court Records

| Source | Coverage |
|:-------|:---------|
| **BAILII** ([bailii.org](https://www.bailii.org/)) | Free judgments — England, Wales, Scotland, NI |
| **Caselaw.nationalarchives.gov.uk** | Official Find Case Law service |
| **Judiciary.uk** | Sentencing remarks for serious cases |
| **CaseTrack** / **HMCTS Online** | Practitioner / paid |
| **Daily Cause List** | Free, basic court schedules |

> UK criminal court records are **not** systematically public. Sentencing remarks and BAILII judgments cover the appellate / serious end; routine magistrates'-court material is largely inaccessible.

---

## Electoral Roll

- **Open Register** — opt-in, available via 3rd-party paid people-search (192.com, Findmypast)
- **Full Register** — restricted to credit reference, political parties, LEO

---

## Social Networks

| Platform | UK use |
|:---------|:-------|
| **X / Twitter** | Politicians, journalists, public figures heavily |
| **Facebook** | Mainstream |
| **LinkedIn** | Professional standard |
| **Instagram** | Mainstream |
| **TikTok** | Growing |
| **Mumsnet** | Hugely influential discussion forum, UK-specific |
| **Reddit** (`r/CasualUK`, `r/AskUK`, `r/UKPolitics` etc.) | Active UK communities |
| **Telegram** | Niche, occasional far-right groups |
| **Snapchat** | Youth |

---

## News & Media

### Public
- **BBC** (bbc.co.uk) — public broadcaster, news flagship
- **Channel 4 News** — left-of-centre TV news
- **ITV News**

### National dailies (with political lean)
- **The Guardian** — centre-left, free
- **The Times** — centre-right, paywall
- **The Telegraph** — right, paywall
- **Financial Times** — financial, paywall
- **Daily Mail** — right tabloid, free
- **The Sun** — tabloid, partial paywall
- **The Mirror** — left tabloid
- **Daily Express** — right tabloid
- **i / iNews** — centrist
- **Metro** — free commuter paper
- **Evening Standard** (London)
- **The Sunday Times, The Observer** — Sunday papers

### Investigative / accountability
- **Bellingcat** — UK-incorporated
- **The Bureau of Investigative Journalism** ([thebureauinvestigates.com](https://www.thebureauinvestigates.com/))
- **OpenDemocracy** — UK section
- **Tortoise** — slow journalism
- **Private Eye** — satirical + investigative, often breaks stories the broadsheets later run
- **Byline Times** — independent, post-2018

### Local
- Reach plc dominates local — most "local" sites are part of larger chains. **Local Democracy Reporting Service** funds council-meeting reporters across the country.

---

## Specifically UK OSINT Tools

- **192.com** — paid people search, electoral roll + telephone directory
- **PeopleSmart UK**
- **Endole** — corporate intel
- **Equifax / Experian** — credit (not public)
- **DueDil** — corporate intelligence (acquired by Artesian)
- **Beauhurst** — startup / scale-up tracking
- **Companies Made Simple** — formation agent (interesting: cross-reference with Companies House to spot shell-company patterns)
- **Find My Past, Ancestry UK** — genealogy + historical
- **British Newspaper Archive** ([britishnewspaperarchive.co.uk](https://www.britishnewspaperarchive.co.uk/)) — paywalled but huge

---

## Notable OSINT Case Patterns

- **Bellingcat / Skripal poisoning (2018):** classic masterclass — flight manifests + corporate filings + Russian leaked databases.
- **Wagner Group / Prigozhin:** Bellingcat methodology heavily based in UK.
- **Russian dirty money via UK property (post-Crimea):** OCOD dataset + Companies House + Land Registry.
- **Football-club beneficial ownership:** FA tests "fit and proper" but Companies House research has repeatedly exposed disqualified owners.

---

## OPSEC / Legal Notes

- **GDPR (UK GDPR post-Brexit) applies fully.** Some PII processing has journalistic exemptions; commercial OSINT often does not.
- **Defamation law is relatively claimant-friendly** — England has historically been a "libel tourism" venue. Be precise about claims.
- **DBS (Disclosure and Barring Service)** checks are *not* OSINT — they're employment-only formal vetting. Don't conflate.
- **Investigatory Powers Act 2016** — OSINT done by state actors falls under this; private investigation is mostly out of scope but bulk data acquisition can intersect.
- **PIs in the UK are not licensed** (long-running parliamentary debate) — anyone can call themselves one. ABI / WAPI memberships are voluntary.

---

## See Also

- [[Country_USA]] — UK Companies House transparency contrasts with most US states
- [[Country_Poland]] — Both have rich free corporate registries; similar EU/UK GDPR regime
- [[OSS_Tools]] / [[Commercial_Tools]] — Maltego heavily used on Companies House data; UK-developed too
- [[OSINT]] — Folder index
