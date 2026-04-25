# Country OSINT — United States

The richest public-records ecosystem in the world. Federal + 50 state + 3000+ county jurisdictions, each with its own portals, formats, and quirks.

---

## Search

- **Google.com** — works fine; use site/state-specific operators (`site:gov`, `inurl:state.tx.us`)
- **DuckDuckGo / Brave Search** — useful counterweights when Google personalisation skews results

---

## Federal Public Records

### Court Records
| Source | Purpose | Cost |
|:-------|:--------|:-----|
| **PACER** ([pacer.uscourts.gov](https://pacer.uscourts.gov/)) | All federal court filings (district, appellate, bankruptcy) | $0.10/page, capped per quarter |
| **CourtListener** ([courtlistener.com](https://www.courtlistener.com/)) | Free PACER mirror via RECAP project + opinion search | Free |
| **Justia** ([justia.com](https://www.justia.com/)) | Free case-law and dockets | Free |
| **Bloomberg Law / Westlaw / LexisNexis** | Commercial legal databases | Paid |
| **Free Law Project** | Bulk legal data | Free |

### Securities & Companies
- **SEC EDGAR** ([sec.gov/edgar](https://www.sec.gov/edgar)) — all public-company filings, 10-K, 10-Q, 8-K, proxies, insider trading. Free, full-text searchable.
- **OpenCorporates** ([opencorporates.com](https://opencorporates.com/)) — aggregates state-level corporate filings
- **OpenSecrets** ([opensecrets.org](https://www.opensecrets.org/)) — campaign finance, lobbying
- **FEC** ([fec.gov](https://www.fec.gov/)) — federal election finance raw data
- **FollowTheMoney** — state-level political finance

### Federal sanctions / law enforcement
- **OFAC SDN List** — sanctions
- **FBI Vault** — declassified docs
- **DEA Public** — registrants, scheduled-substance lists
- **USDA / FDA** — recalls, registrants
- **FAA** — aircraft registry, pilot certificates
- **FCC** — spectrum licences (ham radio, broadcast)

### FOIA (Freedom of Information Act)
- **FOIA.gov** — federal portal
- **MuckRock** ([muckrock.com](https://www.muckrock.com/)) — community-funded FOIA, archives released documents
- **Property of the People** — FOIA-focused NGO

---

## State & County Records (50 separate ecosystems)

> Every state and most counties have their own portals. There is **no national US public-records search** — you go state-by-state.

### Voter records
- Public availability varies by state. Some (FL, OH, NC) sell or freely publish voter rolls; others (AK, CA) restrict access.
- **L2 Political** / **Catalist** / **Aristotle** — paid commercial voter-file aggregators

### Real estate
- County assessor / recorder offices — usually free online, varying quality
- **Zillow** ([zillow.com](https://www.zillow.com/)) — listings + estimated values
- **Redfin** ([redfin.com](https://www.redfin.com/)) — listings, more transparent fee data
- **Realtor.com**
- **Trulia**
- Title/deed: paid via county recorder portals or commercial title companies

### Business filings
- 50 separate Secretary of State databases
- **OpenCorporates** centralises most
- **Bizapedia**, **Buzzfile** — free aggregators
- **DnB Hoovers** — paid

### Court records (state)
- Each state has its own court system; many have unified electronic search:
  - **TX:** odyssey-style portals per county
  - **CA:** county-level Superior Court portals
  - **NY:** WebCivilSupreme / WebCriminal
  - **FL:** county Clerk of Courts portals
  - **MA:** masscourts.org
- **UniCourt** ([unicourt.com](https://unicourt.com/)) — paid aggregator across states

### Vital records
- Birth, death, marriage records — state by state. Many require requestor relationship.
- **FamilySearch** ([familysearch.org](https://www.familysearch.org/)) — LDS-run, free, massive genealogy database
- **Ancestry.com** — paid genealogy
- **Find a Grave** ([findagrave.com](https://www.findagrave.com/)) — cemetery records, photos

---

## People-Search Aggregators

> These aggregate from public records, voter rolls, leaked breaches, and credit-header data. Quality varies; outputs include addresses, phones, relatives, vehicles.

### Free (ad-supported)
- **TruePeopleSearch** ([truepeoplesearch.com](https://www.truepeoplesearch.com/))
- **FastPeopleSearch** ([fastpeoplesearch.com](https://www.fastpeoplesearch.com/))
- **ThatsThem** ([thatsthem.com](https://thatsthem.com/))
- **WhitePages**
- **411.com**

### Paid (consumer)
- **BeenVerified** ([beenverified.com](https://www.beenverified.com/))
- **Spokeo** ([spokeo.com](https://www.spokeo.com/))
- **Intelius**
- **PeopleFinders**
- **Pipl** (now restricted to vetted use)

### Professional / investigator-only (gated)
> See [[Commercial_Tools]] for full coverage.
- **TLO / TransUnion TLOxp** — restricted to PIs, lawyers, LEO
- **LexisNexis Accurint** — same
- **IDI Core / Cognyte LIRX** — same
- **CLEAR (Thomson Reuters)** — same

---

## Vehicles / Transportation

- **VINCheck** (NICB) — stolen-vehicle check
- **NHTSA VIN Decoder**
- **Carfax / AutoCheck** — paid history reports
- **DMV records** — restricted (Driver's Privacy Protection Act 1994), accessible to PIs/insurers/LEO
- **FAA Aircraft Registry** ([registry.faa.gov](https://registry.faa.gov/)) — every US-registered aircraft, owner, history
- **FAA Pilot Records**
- **USCG vessel documentation**
- **MarineTraffic / VesselFinder** — AIS-based ship tracking
- **ADS-B Exchange** — uncensored ADS-B aircraft tracking

---

## Social Networks (US-centric)

- **LinkedIn** — dominant for US professional OSINT
- **Facebook** — widespread but visibility varies; cross-reference with `intelx.io`-style breach lookups
- **X / Twitter** — politicians, journalists, public figures heavily on it
- **Reddit** — US-skewed but global; account-by-account history is gold
- **Truth Social** — Trump-era right-wing platform
- **Gab, Parler (now defunct, archived), MeWe** — fringe right alternatives
- **Discord** — gaming/community, often where extremist groups organise privately
- **Snapchat / Instagram / TikTok** — youth demographics
- **Patreon / OnlyFans** — creator pages, often link to other identifiers
- **Cameo** — sometimes reveals public-figure pricing/availability
- **NextDoor** — neighbourhood-level, useful for local OSINT (account required)

---

## News & Media

### National
- **NYT, WaPo, WSJ, USA Today, AP, Reuters**
- **NPR, PBS** (public)
- **Bloomberg, Forbes, Fortune**
- **Politico, Axios, Punchbowl** — politics
- **Atlantic, New Yorker, Harper's**
- **Fox News, Newsmax, OAN** — right-leaning
- **MSNBC, CNN** — left-leaning
- **The Daily Wire, Breitbart** — far right
- **Mother Jones, The Intercept, Jacobin** — left

### Investigative non-profit
- **ProPublica** ([propublica.org](https://www.propublica.org/))
- **ICIJ** (Panama Papers etc.)
- **OCCRP US presence**
- **Reveal / Center for Investigative Reporting**
- **Bellingcat (US-incorporated NGO)**
- **The Marshall Project** — criminal justice
- **Documented** — immigration

### Local
- City/state newspapers vary wildly in quality and paywalling. **NewsBank, ProQuest** for archival access.

---

## Notable OSINT Case Patterns

- **Capitol riot (Jan 6, 2021):** mass community OSINT — Sedition Hunters, BellingCat, FBI most-wanted lists. Methodology: video-frame extraction → facial features → social-media reverse search.
- **Doxbin / Discord leaks:** US right-wing extremist comms repeatedly leaked / dumped.
- **Corporate / political accountability:** OpenSecrets + SEC EDGAR + court records is a powerful chain.
- **Missing persons (TraceLabs):** US is the primary jurisdiction for TraceLabs CTFs.

---

## OPSEC Notes

- **Driver's Privacy Protection Act (DPPA, 1994):** restricts DMV records to permissible uses. Aggregators that resell vehicle data sometimes violate this; their use can taint your investigation.
- **Fair Credit Reporting Act (FCRA):** background-check sites that produce reports for *employment / housing / credit* decisions must comply with FCRA. OSINT-only use is mostly outside scope, but be careful when output is shared with hiring managers.
- **Stalkerware overlap:** many paid people-search sites have been used in domestic-abuse cases. Some states (CA, NY) have data-broker registration / opt-out laws.
- **Different states have different records-access laws.** TX is liberal; CA is restrictive. Don't assume one state's openness applies elsewhere.

---

## See Also

- [[Commercial_Tools]] — TLO, Accurint, IDI, Maltego paid — the gated-access end of US OSINT
- [[OSS_Tools]] — Free counterparts where the data is reachable
- [[Country_UK]] — Companies House contrast — UK transparency exceeds most US states
- [[Social_Media_APIs]] — API access notes for US-headquartered platforms
- [[OSINT]] — Folder index
