# Public Databases & APIs for OSINT

Cross-cutting catalogue of **public databases and APIs** that aren't tied to a single platform / region. Most have free tiers or are entirely free for non-commercial / journalistic use.

This file is the "what data exists?" companion to:
- [[OSS_Tools]] — open-source tooling (organised by category)
- [[Tools_Kali_Tracelabs]] — tooling organised by selector
- [[Commercial_Tools]] — paid/vetted-access counterparts
- Country / regional pages under `regional/` for jurisdiction-specific registries

> **Scope note:** Some databases are unrestricted; others require academic affiliation, an API key, or a journalist credential. Free-tier limits change — verify before relying on them in a live investigation.

---

## Academic & Scholarly (the people-finding angle)

> Researchers, academics, and scientists leave an exceptionally rich open footprint: papers, affiliations, co-author networks, ORCIDs, grant numbers, conference attendance. For target profiles in academia, government R&D, defence research, or biotech, **start here before social media**.

| Source | Coverage | API | Use for |
|:-------|:---------|:----|:--------|
| **ORCID** ([orcid.org](https://orcid.org/)) | Researcher identity registry — name, affiliations, employment history, education, publications, peer-review activity | Free public API + OAuth | Person → all known publications + employer chain |
| **Google Scholar** | Citation graph, near-complete academic index, author profiles | No official API (SerpAPI / scholarly Python lib) | Author profile, h-index, citing papers (forward-citation pivots) |
| **Semantic Scholar** ([semanticscholar.org](https://www.semanticscholar.org/)) | 200M+ papers, AI-extracted entities, citation context | Free API, generous rate limits | Programmatic paper / author search, influential-citation analysis |
| **OpenAlex** ([openalex.org](https://openalex.org/)) | Open replacement for Microsoft Academic — works, authors, institutions, concepts | Free, no key for ≤10 req/s | Bulk academic graph queries, ROR-mapped institutions |
| **CrossRef** ([crossref.org](https://www.crossref.org/)) | DOI registration agency — metadata for ~150M scholarly works | Free REST API | Resolve DOI → authors, affiliations, funders, ORCID |
| **DataCite** | DOIs for datasets, software, samples | Free API | Find research datasets and software releases tied to a person |
| **OpenAIRE** ([openaire.eu](https://www.openaire.eu/)) | EU-funded research, projects, grants, datasets | Free API | Track EU Horizon / FP7 funding to PI / institution |
| **CORE** ([core.ac.uk](https://core.ac.uk/)) | Largest open-access full-text aggregator | Free API key | Full-text search where Scholar only gives snippets |
| **BASE** ([base-search.net](https://www.base-search.net/)) | 400M+ open-access docs, Bielefeld | OAI-PMH | Repository-level search, theses |
| **arXiv** | Pre-prints in physics, math, CS, q-bio, q-fin, stat | Free OAI / REST API | Pre-publication research, often more current than peer-reviewed |
| **bioRxiv / medRxiv** | Bio + medical pre-prints | Free API | Same, life-sciences |
| **PubMed / PMC** | Biomedical literature, full-text in PMC | E-utilities API (NCBI), free | Medical research, clinicians, pharma authors |
| **PubMed Central (PMC)** | Open-access full text subset of PubMed | Same E-utilities | Full-text mining |
| **ClinicalTrials.gov** | US-registered clinical trials, PIs, sponsors, sites | Free API | Trial PIs, sponsors, sites — pharma OSINT, conflicts of interest |
| **EU CTR (Clinical Trials Register)** | EU clinical trials | Web + downloadable | EU-side equivalent |
| **WHO ICTRP** | Global clinical-trial meta-registry | Web | Cross-reference national trial registries |
| **GRID / ROR** ([ror.org](https://ror.org/)) | Research Organisation Registry — institutional IDs | Free API | Disambiguate "MIT" vs "Manchester Institute of Tech" — links to all affiliated researchers |
| **ROR-affiliated lookup** | Institution → researchers via OpenAlex | OpenAlex | Map every researcher at a target institution |
| **Lens.org** | Patents + scholarly works in one graph (free for non-commercial) | Free + paid API tiers | **Best free patent/paper crossover** — researcher-to-patent link |
| **Dimensions.ai** | Scholarly + patents + grants + clinical trials | Free for academic; commercial API | Comprehensive but gated for commercial |
| **Scopus / Web of Science** | Premier paid citation indices | Paid API (institutional) | Better than Scholar for citation metrics if you have access |
| **Microsoft Academic** | **Discontinued 2021** — replaced by OpenAlex | — | (legacy reference) |
| **ResearchGate** | Researcher social network | No public API; scraping ToS-violating | Manual lookup only — affiliations, papers, requested copies |
| **Academia.edu** | Same idea, more humanities | No public API | Manual lookup |
| **Mathematics Genealogy Project** | PhD advisor / advisee tree | Web only | Academic lineage — useful for elite-network mapping |
| **DBLP** ([dblp.org](https://dblp.org/)) | Computer science bibliography, exhaustive | Free API | CS-specific author + co-author graph |
| **INSPIRE-HEP** | High-energy physics literature | Free API | Particle physics / HEP community |
| **NASA ADS** | Astronomy + astrophysics | Free API | Astro-specific, also covers physics overlap |
| **SSRN** | Social science pre-prints | Web | Economics, law, finance pre-prints |
| **RePEc / IDEAS** | Economics literature, author profiles | Free + scrapable | Economist profiles, central-bank research |
| **PhilPapers** | Philosophy papers + author directory | Web | Niche but exhaustive in field |
| **HAL** | French national open archive | Free OAI | French researchers, theses |
| **DOAJ** | Directory of Open Access Journals | Free API | Journal credibility checks |
| **Retraction Watch Database** | Retracted papers, fraud, misconduct | Free CSV / API | Misconduct flags on a target |
| **PubPeer** | Post-publication peer review, fraud allegations | Free API | Same — open allegations, image manipulation cases |
| **GRO / Beall's List (archived)** | Predatory journal lists | Web archive | Quality-flag a publication record |

### Patterns for academic-target OSINT
1. **Name → ORCID → all affiliations + grant numbers** (gateway selector for academia).
2. **ORCID → CrossRef / OpenAlex → co-author graph** (network).
3. **Co-authors → institutional ROR → conferences / programme committees** (events they attended).
4. **Funder name + grant number → funding-body database** (NIH RePORTER, NSF Awards, UKRI Gateway, EU CORDIS).
5. **Patents (Lens / Espacenet / Google Patents) → assignee company** (academia ↔ industry bridge).

### Funding-body databases (researcher → grant → money trail)

| Source | Coverage |
|:-------|:---------|
| **NIH RePORTER** | All NIH-funded research, PI, dollar amounts |
| **NSF Awards** | National Science Foundation grants |
| **EU CORDIS** | All EU framework programme projects (FP7, H2020, Horizon Europe) |
| **UKRI Gateway to Research** | UK research-council funding |
| **DFG GEPRIS** | German DFG-funded projects |
| **JSPS KAKEN** | Japanese research grants |
| **NSERC, CIHR, SSHRC** | Canadian tri-council |
| **ARC, NHMRC** | Australian research council, medical |
| **DARPA / IARPA** | US defence research — partly public |
| **ERC Project Database** | European Research Council |
| **Welcome Trust Grants** | Biomedical philanthropy |
| **Gates Foundation** | Searchable grants DB |

---

## Sanctions, PEP, Watchlist & Beneficial Ownership

| Source | Coverage | API | Notes |
|:-------|:---------|:----|:------|
| **OFAC SDN List** (US Treasury) | US sanctions | Free CSV / SDN.XML | Authoritative US list |
| **OFAC Consolidated** | All non-SDN US sanctions lists | Free | |
| **EU Consolidated Financial Sanctions List** | EU sanctions | Free download | XML/CSV |
| **UK OFSI Consolidated List** | UK sanctions | Free CSV | Post-Brexit divergence from EU |
| **UN Consolidated Sanctions List** | UN Security Council sanctions | Free XML | Cross-jurisdiction baseline |
| **OpenSanctions** ([opensanctions.org](https://www.opensanctions.org/)) | **Aggregates 200+ lists** — sanctions, PEP, criminals, debarred | Free + commercial API | Single best free entrypoint, deduplicated entities |
| **OpenSanctions Yente** | Self-host OpenSanctions matching API | Open source | Privacy-preserving on-prem screening |
| **WikiData PEP queries** | Politically-exposed persons via SPARQL | Free SPARQL | Useful when name is generic |
| **World Bank Debarred Firms** | Firms banned from WB-funded projects | Free | Procurement integrity |
| **Interpol Red Notices** | Wanted persons (excluding political/military) | Web only, no bulk | Per-person manual lookup |
| **Europol Most Wanted** | EU equivalent | Web only | |
| **FBI Wanted** | US wanted persons | Web + RSS | Per-list scraping |
| **Offshore Leaks Database (ICIJ)** ([offshoreleaks.icij.org](https://offshoreleaks.icij.org/)) | Panama / Paradise / Pandora / Bahamas / Offshore Leaks | Free web search + bulk CSV | Beneficial-ownership leaks, 800k+ entities |
| **OCCRP Aleph** ([aleph.occrp.org](https://aleph.occrp.org/)) | Investigative-journalism dataset hub: registries, leaks, court docs | Free API (key required) | **Single best free investigative DB** |
| **OpenOwnership Register** ([register.openownership.org](https://register.openownership.org/)) | Beneficial-ownership data from UK PSC + open jurisdictions | Free API | UBO graph |
| **PEP database (Wikidata-derived)** | Open PEP via Wikidata classes | SPARQL | Free alternative to commercial PEP feeds |
| **OpenCorporates** ([opencorporates.com](https://opencorporates.com/)) | 200M companies across 140 jurisdictions | Free + paid API | Company graph; some reconciliation requires paid |
| **GLEIF (LEI)** ([gleif.org](https://www.gleif.org/)) | Legal Entity Identifier — global ID for legal entities | Free download / API | Cross-jurisdiction company canonical ID |
| **EU Transparency Register** | Lobbyists registered with EU institutions | Free CSV | Lobbyist OSINT |
| **US LDA Lobbying Disclosures** | US federal lobbying | Free download | |
| **OpenSecrets** | US campaign-finance + lobbying | Free API | |
| **FollowTheMoney (NIMP)** | US state-level campaign finance | Free | |

---

## Patents & Intellectual Property

| Source | Coverage | API | Notes |
|:-------|:---------|:----|:------|
| **Google Patents** | Global patents, full text + machine translation | Web only (BigQuery dataset for bulk) | **Best free UX**, indexes most jurisdictions |
| **USPTO PatFT / AppFT** | US issued + applications | Bulk download + PEDS API | Authoritative US |
| **USPTO PEDS** | Patent application status, file wrapper | Free API | Prosecution history |
| **EPO Espacenet** | European Patent Office, 130M+ patents | Free + OPS API | Family search across jurisdictions |
| **WIPO PATENTSCOPE** | International PCT applications | Free API | Earliest-filing layer |
| **Google Patents Public Datasets** (BigQuery) | Bulk SQL on global patents | Free tier in BigQuery | Best for graph / co-inventor / assignee analysis |
| **Lens.org** | Patents + papers + collections | Free for non-commercial | Cross-link patents ↔ scholarly articles |
| **PatentsView** | USPTO bulk + APIs for inventor / assignee disambiguation | Free | Inventor identity resolution |
| **Korea KIPRIS** | Korean patents | Free | |
| **CNIPA** | Chinese patents (web only, JP / KR DBs better for filings) | — | |
| **J-PlatPat** | Japanese patents | Free | |
| **TMView / Designview** | EU+ trademark + design search | Free | |
| **USPTO TESS** | US trademarks | Free | Trademark → entity |

---

## Court Records & Litigation

| Source | Coverage | Notes |
|:-------|:---------|:------|
| **PACER (US Federal)** | All US federal cases | $0.10/page, free public terminals; **CourtListener / RECAP** mirror is free for documents already pulled |
| **CourtListener / RECAP** ([courtlistener.com](https://www.courtlistener.com/)) | Free PACER mirror, opinions, oral arguments | Free API |
| **Justia** | US case law, dockets | Free web |
| **Harvard Caselaw Access Project** | 6.7M US cases, complete | Free API |
| **State court systems** | Highly fragmented; often per-county portals | See [[Country_USA]] |
| **BAILII** | UK + Ireland case law | Free web |
| **CanLII** | Canadian case law | Free web |
| **AustLII** | Australian + Pacific case law | Free web |
| **WorldLII / CommonLII / AsianLII** | Federated free case-law search | Free web |
| **EUR-Lex / CURIA** | EU legislation + Court of Justice | Free |
| **HUDOC** | European Court of Human Rights case law | Free |
| **ICC + ICTR + ICTY archives** | International criminal tribunals | Free |
| **Justice.cz, Pappers, KRS** etc. | National registries — see country pages |

---

## Maritime, Aviation & Vehicle

| Source | Coverage | Notes |
|:-------|:---------|:------|
| **MarineTraffic** | Live + historical AIS | Free tier limited, paid for history |
| **VesselFinder** | Same as above | |
| **ShipSpotting** | Crowdsourced ship photos | Free, useful for visual ID |
| **Equasis** | Ship safety + ownership records | Free, registration required |
| **IMO GISIS** | IMO ship database, port state control | Free, registration required |
| **OpenSky Network** | Academic ADS-B, free history | Free for research |
| **ADS-B Exchange** | Uncensored aircraft tracking (incl. blocked tail numbers) | Free + paid |
| **FlightAware** | Flight tracking | Free tier |
| **FAA Aircraft Registry** | US aircraft owner search | Free web |
| **EASA / national civil-aviation registries** | EU member states; varies | Free / per-jurisdiction |
| **VIN-decoder APIs** (NHTSA vPIC) | US VIN decoding | Free |
| **MMSI lookup (ITU)** | Marine MMSI → vessel | Free |
| **Hexcode → ICAO 24-bit registry mapping** | Aircraft ID resolution | Free |
| **Nautical Almanac, Time and Date** | Sun, tides, daylight — geolocation use | Free |

---

## Datasets, Leaks & Investigative Hubs

| Source | What | API |
|:-------|:-----|:----|
| **OCCRP Aleph** | Aggregator of leaks, registries, court records | Free API key |
| **ICIJ Offshore Leaks DB** | Panama / Paradise / Pandora / Bahamas / Offshore Leaks | Free + bulk CSV |
| **Distributed Denial of Secrets (DDoSecrets)** | Leaked datasets, journalist-grade | Magnet links + web |
| **WikiLeaks** | Historic + current leaks | Web archive |
| **Internet Archive Datasets** | archive.org datasets section, BookSearch, Wayback CDX | Free APIs |
| **Wayback CDX API** | Time-machine programmatic search | Free |
| **GDELT** | Global news event data, multilingual | Free, BigQuery |
| **MediaCloud** | Global news media analysis | Free, account |
| **Common Crawl** | Petabytes of crawled web | Free in S3 |
| **Pushshift (Reddit)** | Reddit history archive — **mostly closed since 2023**, partial mirrors at Arctic Shift | Limited |
| **Bellingcat Online Investigation Toolkit** | Curated dataset + tool list | Free web |
| **OpenStreetMap (Overpass API)** | Map features, POIs, infrastructure | Free |
| **OSM Wiki + Taginfo** | Tag schema, POI types | Free |

---

## News, Archive & Press

| Source | Coverage |
|:-------|:---------|
| **Wayback Machine** | Web archive — `web.archive.org/web/*/<URL>` |
| **archive.today** | Single-snapshot alternative, less censorable |
| **NewsAPI / GNews / Mediastack** | Aggregator APIs, free tiers |
| **GDELT 2.0 GKG** | Global Knowledge Graph of news entities |
| **MediaCloud** | Multilingual news source analysis |
| **Internet Archive TV News Archive** | Searchable broadcast TV transcripts (US-heavy) |
| **EBU + national broadcaster archives** | Per-country |

---

## Health & Medical Public Records

| Source | Coverage |
|:-------|:---------|
| **ClinicalTrials.gov** | US + many international trials (PIs, sponsors) |
| **EU Clinical Trials Register** | EU equivalent |
| **WHO ICTRP** | Cross-jurisdiction meta |
| **OpenPaymentsData (CMS)** | US doctor → pharma payment records |
| **NHS England Open Data** | NHS spending, prescribing, GP records (de-id) |
| **NPI Registry (CMS)** | All US healthcare providers |
| **State medical-licensing boards** | Per-state US |
| **GMC (UK), Bundesärztekammer, etc.** | National regulators |

---

## Crypto & Blockchain

| Source | Use |
|:-------|:----|
| **Block explorers** (Etherscan, Mempool.space, Blockchair) | Free address / tx lookup |
| **Blockchair** | Multi-chain explorer + API |
| **Chainabuse** | Community-reported scam addresses |
| **ScamSniffer / GoPlus** | Address risk-scoring |
| **OFAC SDN crypto address list** | US-sanctioned wallets |
| **Wallet Explorer / OXT (Bitcoin)** | BTC clustering heuristics |
| **WhaleAlert** | Large-tx feed |
| **DeBank / Zapper** | Wallet portfolio across chains |
| **Sanctioned-address lists** | OpenSanctions includes these |

> Commercial-grade attribution (Chainalysis, TRM, Elliptic) lives in [[Commercial_Tools]].

---

## Government Open-Data Portals

A starting set — most countries have one. Search "{country} open data portal" if missing here. Country-specific registries live on the relevant country page.

| Portal | Country |
|:-------|:--------|
| **data.gov** | USA |
| **data.gov.uk** | UK |
| **data.europa.eu** | EU consolidated |
| **data.gouv.fr** | France |
| **GovData** | Germany |
| **dane.gov.pl** | Poland |
| **data.gov.au** | Australia |
| **data.gc.ca** | Canada |
| **e-Stat** | Japan |
| **dados.gov.br** | Brazil |
| **datos.gob.mx** | Mexico |
| **NHS England Open Data** | UK NHS-specific |
| **data.world** | Community-uploaded datasets |
| **Kaggle Datasets** | Same |
| **Hugging Face Datasets** | ML-oriented but contains many OSINT-relevant releases |

---

## Identity & Identifier Resolution

| Source | Resolves |
|:-------|:---------|
| **WikiData SPARQL** | Person / org / place → cross-IDs (ORCID, ISNI, VIAF, IMDb, Twitter, …) |
| **VIAF** | Authority file — author identity across libraries |
| **ISNI** | International Standard Name Identifier |
| **GeoNames** | Place-name → coordinates + variants |
| **WorldCat** | Library holdings — books, authored works |
| **CitizenDB / Wikidata People** | People linked to public IDs |
| **Crossref Funder Registry** | Funder name disambiguation |

---

## Stub: things to add (research with the prompt below, then fill in)

- Per-country FOIA / public-records request portals
- Educational accreditation databases (degree verification)
- Professional licensing (engineers, lawyers, accountants — per jurisdiction)
- Real-estate / land registries beyond country pages
- Charity / non-profit registries (IRS Form 990, UK Charity Commission, etc.)
- Trade / customs data (Panjiva, ImportGenius — partly paid)
- Procurement portals (TED EU, SAM.gov, UK Contracts Finder, Polish BZP)
- Conference programme committees / speaker rosters (research events)
- Defence / arms-export licensing data
- Spectrum / radio licensee databases (FCC ULS, Ofcom, UKE)

---

## Wired into Poirot today

These sources from this catalogue are **already implemented as providers** in `src/SherlockOsint.Api/Services/OsintProviders/` and fire automatically during a search:

| Source | Provider class | Triggers on |
|:-------|:---------------|:------------|
| ORCID | `OrcidLookup.cs` | `fullName` populated |
| OpenAlex | `OpenAlexLookup.cs` | `fullName` populated |
| Wayback Machine (CDX) | `WaybackMachineLookup.cs` (registered, not wired into pipeline yet) | — |

Everything else in this file is roadmap.

---

## See Also

- [[OSINT]] — Folder index
- [[OSS_Tools]] — Tooling that consumes these APIs
- [[Tools_Kali_Tracelabs]] — Selector-organised tooling
- [[Commercial_Tools]] — Paid alternatives where free tiers don't reach
- [[Social_Media_APIs]] — Platform APIs (separate file because of scale)
- Country pages under `regional/` for jurisdiction-specific registries
