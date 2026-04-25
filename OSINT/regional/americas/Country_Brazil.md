# Country OSINT — Brazil (Brasil)

Latin America's largest country, Portuguese-speaking (Brazilian Portuguese), heavy Meta + WhatsApp + YouTube usage, surprisingly open public-records ecosystem.

---

## Languages

- **Portuguese (Português)** — Brazilian variant differs from European Portuguese in vocabulary, spelling, and slang
- **Translation:** Google Translate is solid; **DeepL** for nuance
- Cultural: heavy use of **internet slang and abbreviations** — kkkk = laughter, bjs = besos/kisses, vc = você, etc.

---

## Search

- **Google.com.br** — dominant
- **UOL Search** ([busca.uol.com.br](https://busca.uol.com.br/)) — niche
- **Bing Brasil** — niche

---

## Social Networks

| Platform | Penetration | Notes |
|:---------|:------------|:------|
| **WhatsApp** | **Universal** — ~165M users | Single most important platform in Brazil. Misinformation flows via WhatsApp groups, especially during elections |
| **Instagram** | Massive | Influencer economy is enormous |
| **Facebook** | Mainstream | Strong in older demographics |
| **YouTube** | Massive | Brazilian YouTubers among the most-subscribed globally |
| **TikTok** | Heavy | Huge Brazilian creator base |
| **X / Twitter** | Politically influential | Was banned briefly in 2024 — used to be enormous, declining post-Musk |
| **LinkedIn** | Standard professional |
| **Reddit** (`r/brasil`, `r/desabafos`) | Smaller than Anglophone |
| **Telegram** | Growing, especially political channels |
| **Bluesky** | Migration spike from X in 2024 — Brazilian community is one of the largest globally |
| **Kwai** | TikTok competitor, popular in lower-income demographics |

### Brazilian-specific platforms (mostly historical / declining)
- **Orkut** — Google's social network, Brazil was its dominant market 2004-2014. Defunct since 2014.
- **Twoo, Tagged** — used in Brazil
- **Vakinha, Catarse** — crowdfunding, OSINT-relevant for cause/project tracking
- **Reclame Aqui** ([reclameaqui.com.br](https://www.reclameaqui.com.br/)) — consumer-complaint platform, useful for identifying companies/scams

---

## Government & Corporate Records

> Brazil's public records are unusually open compared to most Latin American countries — driven by transparency laws (Lei de Acesso à Informação 2011) and a strong investigative-journalism tradition.

| Source | Purpose |
|:-------|:--------|
| **Receita Federal CNPJ** ([servicos.receita.fazenda.gov.br](https://servicos.receita.fazenda.gov.br/Servicos/cnpjreva/Cnpjreva_Solicitacao.asp)) | Free CNPJ (corporate ID) lookup — name, address, partners, capital |
| **Junta Comercial (state-level)** | Detailed corporate filings; varies by state — JUCESP (SP), JUCERJA (RJ), etc. |
| **CVM** ([cvm.gov.br](https://www.gov.br/cvm/)) | Securities & Exchange — listed-company filings (Brazilian SEC) |
| **Diário Oficial da União (DOU)** ([in.gov.br](https://www.in.gov.br/)) | Federal official gazette — public-sector appointments, government contracts |
| **Diários Oficiais Estaduais** | State-level gazettes |
| **Portal da Transparência** ([portaldatransparencia.gov.br](https://portaldatransparencia.gov.br/)) | Federal spending, public servants' salaries, contracts — *very* detailed |
| **TCU** ([portal.tcu.gov.br](https://portal.tcu.gov.br/)) | Federal Court of Accounts — audit reports |
| **Justiça** ([cnj.jus.br](https://www.cnj.jus.br/)) | National Justice Council |
| **Jusbrasil** ([jusbrasil.com.br](https://www.jusbrasil.com.br/)) | Aggregated court rulings, very rich, free + paid tiers |
| **STF / STJ** | Supreme courts |
| **TSE** ([tse.jus.br](https://www.tse.jus.br/)) | Electoral Court — election results, candidate filings, campaign finance |
| **CEPESP / DivulgaCandContas** | Election candidate filings, asset declarations |
| **TRE (state electoral courts)** | State-level electoral data |
| **SISBAJUD** | Banking judicial system (paid, professional access) |
| **INSS / e-Social** | Pensions, employment registrations (limited public) |

### Beneficial-ownership / "panama"-style work
- **OpenCorporates Brazil** — aggregated data
- **CNPJ.biz** — free CNPJ aggregator
- **Saude Trace** / **CompliancePro** — paid corporate-intel platforms

### Real estate
- **Cartórios** — notary offices hold property records, state-level access varies
- **REGIN** — real-estate national index
- **Zap Imóveis** ([zapimoveis.com.br](https://www.zapimoveis.com.br/)), **Vivareal**, **OLX** — listings sites for current/recent ownership

### Vehicles
- **Detran** (state-level) — vehicle registrations, partial public lookup
- **Sinesp Cidadão** — federal mobile app, plate lookup
- **Sintegra** — interstate ICMS taxpayer registry

### Personal IDs (sensitive)
- **CPF** (11-digit personal tax ID) — *not* publicly searchable, but heavily leaked over years; many sites offer paid lookup of dubious legality
- Possession of bulk CPF databases carries criminal risk

---

## News & Media

### Major media groups (and outlets)
- **Globo** — dominant. Owns G1 (news portal), O Globo (paper), GloboNews (TV), Valor Econômico (financial)
- **Folha de S.Paulo** ([folha.uol.com.br](https://www.folha.uol.com.br/)) — major centre-left daily, often investigative
- **Estadão** ([estadao.com.br](https://www.estadao.com.br/)) — centre-right
- **UOL** ([uol.com.br](https://www.uol.com.br/)) — major portal/news
- **Veja** — weekly news magazine, conservative-leaning
- **Carta Capital** — left-leaning weekly
- **IstoÉ, Época** — weeklies

### Online-native / investigative
- **The Intercept Brasil** ([theintercept.com/brasil](https://theintercept.com/brasil/)) — broke the "Vaza Jato" leaks
- **Agência Pública** ([apublica.org](https://apublica.org/)) — investigative non-profit
- **Abraji** — Brazilian Investigative Journalism Association
- **Piauí** — long-form journalism
- **Ponte Jornalismo** — criminal-justice focus
- **Marco Zero** — Northeast Brazil focus
- **JOTA** — judicial / regulatory focus

### Fact-check
- **Lupa** (Folha) — fact-check
- **Aos Fatos** — fact-check
- **Comprova** — election-fact-check consortium

---

## Specifically Brazilian OSINT Tools

- **CNPJ.ws / CNPJ.biz** — JSON/web CNPJ lookup
- **API Receita Federal** — semi-official CNPJ API endpoints
- **Brasil API** ([brasilapi.com.br](https://brasilapi.com.br/)) — open API for CEP (postal code), CNPJ, banks, holidays
- **DadosAbertos.gov.br** — open-data portal
- **BrasilTransparente** — transparency tooling
- **Polícia Federal** ([gov.br/pf](https://www.gov.br/pf/pt-br)) — wanted persons, sometimes searchable
- **Fundação Getulio Vargas (FGV)** — research datasets

---

## Notable OSINT Cases

- **Operação Lava Jato (2014-2021):** corruption investigation; OSINT around shell companies and offshore holdings.
- **Vaza Jato (2019, The Intercept):** leaked Telegram messages from prosecutors; OSINT verification of authenticity.
- **2018 / 2022 / 2024 elections:** massive misinformation OSINT — WhatsApp group monitoring, Telegram channel analysis. Established Brazilian fact-check ecosystem became internationally referenced.
- **8 January 2023 attacks (Brasília, "Brazilian January 6"):** crowd-sourced OSINT identified hundreds of attackers via face/clothing recognition.
- **Amazon deforestation / illegal mining tracking:** Bellingcat + InfoAmazonia + MapBiomas use satellite OSINT.

---

## OPSEC / Legal Notes

- **LGPD (Lei Geral de Proteção de Dados)** — Brazilian GDPR. Restricts PII processing.
- **Marco Civil da Internet** — Brazilian internet rights / data localisation framework.
- **Defamation (calúnia, difamação, injúria)** — criminal liability exists. Truth is sometimes a defence (calúnia) but not always (difamação).
- **Court orders to take down content** are common and frequently issued. Investigators publishing material on Brazilian individuals can face civil orders.
- **Pegasus / state surveillance** — documented use against Brazilian journalists/activists. Threat-model accordingly for sensitive work.
- **Election period restrictions** — Brazilian electoral law restricts some types of speech / data publication during campaign periods.

---

## See Also

- [[Country_USA]] — Brazilian diaspora large in US (FL, MA); cross-border OSINT common
- [[Country_Poland]] — Both have surprisingly rich free corporate registries
- [[OSS_Tools]] / [[Commercial_Tools]] — Maltego works well on CNPJ data; Brazilian shell-company structures are tractable with link-analysis
- [[OSINT]] — Folder index
