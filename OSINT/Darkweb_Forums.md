# Dark-Web Forums — Threat Intel & OSINT

Educational reference for analysts who monitor dark-web forums for **threat intelligence**, **brand protection**, **leak detection**, and **missing-persons / human-rights** investigations.

> [!CAUTION]
> **Legal warning.** Accessing dark-web forums for legitimate research is generally lawful in most jurisdictions, but specific actions are not — purchasing illegal goods, downloading CSAM (criminal in every jurisdiction, no exception), engaging with illegal services, or even *registering* on some forums can expose you to criminal liability. Some countries (e.g. parts of the EU, the UK, Russia) have additional restrictions on accessing certain content. **Get legal sign-off before proceeding.** Document your authorisation before you ever connect.

> [!IMPORTANT]
> This page does **not** list `.onion` URLs or instruct on accessing illegal markets. Live URLs change weekly, are typically harvested from analyst-only sources (Recorded Future, Flashpoint, DarkOwl), and re-publishing them lowers the safety floor for everyone. The categorisation below is what you need to do the analyst work; the URL-of-the-day is the tradecraft you bring or buy.

---

## Networks

### Tor (.onion)
Most dark-web forums live here. Onion routing through 3+ relays.

- **Tor Browser:** [torproject.org](https://www.torproject.org/) — for occasional use only
- **Tails / Whonix:** [[Distros]] — for repeated investigation work
- **Bridges (obfs4, snowflake, meek):** for circumventing Tor blocks (China, Iran, parts of Russia)

### I2P
"Invisible Internet Project". Different routing model, has its own ecosystem (eepsites). Smaller user base, fewer forums than Tor but a few persistent communities.
- [geti2p.net](https://geti2p.net/)

### Freenet / Hyphanet
Distributed publishing network. Mostly content-focused (blogs, file shares) rather than forum-focused. Used by privacy advocates, dissidents, and a small archive scene.
- [hyphanet.org](https://www.hyphanet.org/)

### Clearnet "fringe" forums
Many forums analysts care about are **not** on Tor — they're on the open web behind invite-only registration, paywalls, or country-specific TLDs. Don't assume "dark web" = Tor.

---

## Forum Categories (analyst taxonomy)

This is the categorisation Recorded Future, Intel471, Flashpoint, and academic projects use. Knowing the category narrows what tradecraft you need.

### 1. Cybercrime / Hacker forums
Members trade exploits, stolen data, malware, initial-access broker (IAB) listings, ransomware-as-a-service (RaaS) affiliate programs.

**What analysts look for:**
- Stolen credentials matching their org's domain (brand protection)
- Mentions of their org by name (early-warning of breach)
- Sale of access to systems matching their network footprint
- New TTPs and exploit chatter (CTI)

**Notable historical examples (most defunct/seized — **do not search for replacements**):** Raidforums (seized 2022), Breached (seized 2023), Exploit.in, XSS.is. Replacements churn constantly.

### 2. Carding / Fraud forums
Stolen credit cards, fullz (PII bundles), bank-drop services, money-mule recruitment.

**What analysts look for:**
- Card BINs matching their issuer
- Methodology shifts (e.g., new ATM-skimming tooling, new bypass for 3DS)

### 3. Leak / Doxing sites
Sites whose primary product is publishing leaked data — corporate breaches, personal dox, politically-motivated leaks.

**What analysts look for:**
- New leaks affecting their org or sector
- Reposts of older breaches

### 4. Ransomware "shaming" sites
Operator-run sites where ransomware groups publish stolen data when victims refuse to pay.

**What analysts look for:**
- New victim postings (early-warning if it's their org or a supplier)
- Group activity / dormancy patterns
- Attribution clues (writing style, branding, hosting)

### 5. CSAM
**Do not investigate CSAM forums independently.** Possession of these images is criminal everywhere. If you encounter CSAM during another investigation, stop, document the URL only (not the content), report to **NCMEC** (US), **IWF** (UK), or your country's national hotline, and consult your legal team. Independent CSAM investigation is the work of dedicated law-enforcement units only.

### 6. Drug / weapons marketplaces
Successors to Silk Road. Vendors, vendor-review forums, buyer guides.

**What analysts look for (non-LEO):** rarely. This is mostly a law-enforcement and academic-research domain.

### 7. Hacktivism / political
Anonymous-style forums, hacktivist discussion, leaks driven by political motive.

### 8. Whistleblowing / dissident
SecureDrop instances at major newspapers, dissident discussion forums in countries where free speech is restricted (see [[Regional_RUNet]], [[Regional_China]]).

**What analysts look for:** Same techniques sometimes used to *protect* sources — analysts here are often the journalists or NGOs themselves.

### 9. Extremist / terrorist
Closed forums and Telegram channels. Heavily monitored by national agencies. Independent research carries legal risk in many jurisdictions (proscribed-organisation membership offences).

---

## Operational Approach

```
1. Authorisation        — written scope, legal sign-off, kill criteria
2. Environment           — Whonix or Tails, fresh snapshot ([[VMs_and_Compartmentalization]])
3. Identity             — investigation-only sock-puppet, never reused
4. Collection           — Hunchly capture (every page), no downloads of illegal content
5. Pivoting              — selectors → other forums, but never log in unnecessarily
6. Reporting            — sanitised, source-quoted, IOCs extracted
7. Tear-down             — destroy VM snapshot, retain encrypted evidence per policy
```

---

## Tooling

| Tool | Purpose |
|:-----|:--------|
| **Hunchly** | Auto-capture every page, hash, timestamp — non-negotiable for evidence chain |
| **OnionScan** (legacy) | Static analysis of `.onion` services for misconfig |
| **Ahmia** | Tor search engine that filters out CSAM, used as a safer entry-point |
| **DarkSearch** / **Onion Search Engine** | General Tor search engines (use with care) |
| **OnionLand Search** | Tor index |
| **Recorded Future** (commercial) | Aggregated dark-web intelligence feed |
| **Flashpoint** (commercial) | Dark-web threat intel |
| **DarkOwl** (commercial) | Dark-web data lake |
| **Intel471** (commercial) | Underground actor tracking |
| **Have I Been Pwned** | Free breach data, indirectly sourced from leaks |

> The commercial feeds are how most enterprise SOCs do dark-web monitoring without their analysts touching the forums directly. Buying a feed is often safer (legally and operationally) than direct collection.

---

## Indicators You're In the Wrong Place

Stop and re-evaluate if you see any of these:
- The forum requires posting illegal content (e.g., a stolen card, a CSAM image, a child's PII) to register or "prove yourself"
- You're being asked to pay to access content
- You're being asked to download executables or run scripts to "prove" you're not law enforcement
- You're being directly asked to commit a crime as a precondition

These are common gatekeeping mechanisms on serious cybercrime forums. If your authorisation doesn't explicitly cover the action being asked of you, you don't proceed.

---

## Reading List (academic & journalistic)

- **Bellingcat** — public-interest investigations using OSINT and dark-web sources
- **KrebsOnSecurity** — long-running cybercrime journalism
- **The Record (Recorded Future)** — vendor blog with substantive cybercrime reporting
- **Flashpoint Intel Blog**
- **DarkOwl Resources**
- **Darknet Diaries** (podcast) — accessible case studies
- **Academic: USENIX Security / IMC / WWW** — papers on dark-web measurement studies are a goldmine for taxonomy and methodology

---

## See Also

- [[OSINT]] — Folder index and methodology
- [[VMs_and_Compartmentalization]] — Whonix / Qubes / Tails setup
- [[Distros]] — Whonix and Tails details
- [[Browser_Extensions]] — Tor Browser and CanvasBlocker hardening
- [[Regional_RUNet]] / [[Regional_China]] / [[Regional_Arabic]] — Many regional analogues live on regional clearnet forums, not Tor
