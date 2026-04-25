# Sock Puppet Recipes — Platform-by-Platform Provisioning

Concrete step-by-step recipes for creating investigation-only ("sock puppet") accounts on platforms covered in [[Social_Media_APIs]]. Each recipe assumes you've already read [[VMs_and_Compartmentalization]] for environment hygiene.

> [!IMPORTANT]
> **Legal & ethical scope.** Sock puppets are common in legitimate OSINT (threat intel, brand protection, missing-persons CTFs, journalism). They are **not OK for**: harassment, impersonating a real person, gaining unauthorised access to accounts, or breaching platforms' explicit account-creation laws. Some jurisdictions have specific anti-impersonation criminal statutes. **Get legal sign-off before going beyond passive monitoring**, especially for engagement with sources, infiltration of private groups, or any commercial use.

---

## The Universal Pre-Flight Checklist

Before you create *any* sock puppet, confirm the following:

```
[ ] Investigation VM is fresh-snapshot (see [[VMs_and_Compartmentalization]])
[ ] Daily-driver browser / accounts are NOT logged into anywhere reachable from this VM
[ ] Network egress is decided (residential VPN / Tor / mobile / direct — see "Network" below)
[ ] Persona is documented:
       Name, age, location, employer, photo source, backstory, password
[ ] Email is provisioned (ProtonMail / Tutanota / fastmail alias / disposable)
[ ] Phone-receive plan is decided (physical SIM / SMS service / none)
[ ] Browser fingerprint is hardened (CanvasBlocker, resistFingerprinting)
[ ] You have a documented reason / authorisation in writing for the work
```

---

## Persona Building (the slow part)

Sock puppets fail on **persona consistency**, not technical IP/fingerprint detail. Spend more time here than on infrastructure.

### Identity components

| Component | How to source it |
|:----------|:-----------------|
| **Name** | Pick from the country's common names — see country pages in `regional/`. **Don't use real-person names.** Use US Census + per-country baby-name lists |
| **Age (DOB)** | 25-45 typical sweet spot — old enough to have history, young enough to use social media actively |
| **Location** | A real city in your target's country — pick a mid-size city, not the capital, to be less interrogable |
| **Employer / occupation** | Real but unverifiable — "self-employed contractor", "freelance copywriter", "small-business owner" |
| **Profile photo** | **NOT [thispersondoesnotexist.com](https://thispersondoesnotexist.com/)** anymore — too detectable. Better: photo of a real-but-tiny-public-presence person from a public photo-licensed site (Unsplash, Pexels portraits) where the photo is unlikely to be reverse-image-searchable to your sock puppet's name |
| **Backstory** | 3-5 sentences that explain why this account exists — "I joined because I'm interested in X" |

### Validation before going live
1. Reverse-image-search your chosen photo on Yandex + Google + TinEye + PimEyes — must return zero significant matches
2. Search the chosen name + city — no obvious real-person collision
3. Persona must be consistent across **email + display name + bio + first 5 posts**

### Ageing protocol
**Don't immediately use a fresh sock puppet for investigation.** Platforms heavily flag new accounts with no history.

```
Day 0:    Create account. Complete profile (photo, bio).
Day 1-7:  Light activity — follow ~50 mainstream accounts, like ~20 posts, no comments
Day 7-21: Comment occasionally on benign content (sports, weather, generic memes)
Day 21+:  Begin investigation use — slowly
```

For platforms where ageing matters most (Meta, LinkedIn, X), 4 weeks minimum is the analyst rule of thumb.

---

## Network Decisions

| Plan | Best for | Beware |
|:-----|:---------|:-------|
| **Direct (your home IP)** | Don't. | Every visit logged to you |
| **Commercial VPN** | Light passive work, geographic flexibility | Most sites detect commercial-VPN ASNs; account creation often blocked |
| **Residential proxy** (paid) | Account creation, plausibility | Cost; ASN reputation varies |
| **Mobile IP** (4G/5G dongle, mobile hotspot) | Best authenticity for new account creation | Tying it to a SIM = identity link |
| **Tor (Whonix)** | Anonymity for browsing public content | Many platforms outright block account creation; existing accounts often flagged |
| **VPN → Tor or Tor → VPN** | Hostile-environment work | See [[VMs_and_Compartmentalization#Networking Patterns]] |

> **Heuristic:** match the network exit to the persona's claimed location. A "Madrid-based digital nomad" coming from a Frankfurt datacenter VPN is suspicious.

---

## Email & Phone Pre-Provisioning

### Email
| Provider | Notes |
|:---------|:------|
| **ProtonMail** | Most platforms accept; ProtonMail may require a phone number for new accounts depending on signup IP |
| **Tutanota / Tuta** | Smaller; many platforms still accept |
| **Fastmail** (paid, custom-domain) | Most plausible; pay with a privacy-aware method |
| **Outlook.com / Gmail** | High plausibility; require phone for some signups |
| **Disposable services** (Guerrilla, Mailinator, TempMail) | **Most platforms block these** — only useful for one-shot signups |
| **Custom-domain email** (own domain via Cloudflare email routing or Fastmail) | Most plausible; small upfront cost |

### Phone
| Plan | Use case |
|:-----|:---------|
| **Real prepaid SIM in target country** | Best for serious sock puppet — durable, native to platform's expected number range |
| **Physical SIM in your country** | OK for most platforms, but sometimes flagged for cross-border accounts |
| **eSIM** (Airalo, Holafly) | Convenient, but some platforms (especially Asian) detect eSIM ranges |
| **SMS-receiving services** (5sim.net, sms-activate.org, smspool, OnlineSim) | **Most major platforms detect and reject these number ranges** as of 2024-2026; partial success only |
| **Google Voice / Skype / Twilio numbers** | Detected as VoIP and rejected by most major platforms |

> **Reality check (2026):** receiving SMS for new account creation has become much harder. WhatsApp, Telegram, Twitter/X, Meta, KakaoTalk, Naver, WeChat all detect VoIP numbers reliably. Plan for at least one **physical SIM** (in your jurisdiction or the persona's claimed country) for the "anchor" sock puppet.

---

## Per-Platform Recipes

### Twitter / X

```
Difficulty:        4 / 5
Phone required:    Often (especially after first few minutes / from new IP)
Email required:    Yes
Account ageing:    4-6 weeks before investigation use
Detection vectors: phone-VoIP, ASN, browser fingerprint, behavioural
```

**Recipe:**
1. **Network:** residential or mobile IP matching persona country
2. **Email:** ProtonMail or Outlook (Gmail's signup-flow now usually requires phone too)
3. Visit `x.com/i/flow/signup`
4. Use **mobile-style browser** (Firefox + UA spoof to mobile) — the mobile signup flow has historically been less aggressive
5. Phone verification will likely trigger — be ready with a **physical SIM**, not an SMS service. The "wait for SMS" timer is short; if you fail, the IP is partly burned for retry
6. Complete profile: photo, header, bio, location (text-only, not GPS)
7. **Ageing:**
   - Days 1-3: Follow 50-80 mainstream accounts in persona's interests (sports, news, cooking, dogs)
   - Days 4-14: Like ~5 posts/day, retweet ~1/day
   - Days 14-30: Begin replying with benign content, post 1-2 original posts/week
   - Day 30+: Investigation use

**Common failure modes:**
- "Verify it's you" challenge with phone — usually means the signup IP / fingerprint already looked off; restart from a clean snapshot
- Account suspended within hours — typically means VoIP number detected
- Account suspended at first follow — typically the photo or bio matched another sock puppet's

---

### Meta — Facebook & Instagram (one account creation, two products)

```
Difficulty:        5 / 5  (the hardest of the major Western platforms in 2026)
Phone required:    Yes, almost always
Email required:    Yes
Account ageing:    6-8 weeks for safe use
Detection vectors: device fingerprint, behavioural, social-graph anomaly, photo-search
```

**Reality check:** Meta has invested heavily in sock-puppet detection since 2018. Bans are common, and Facebook accounts created without a believable "social graph" (mutual friends with the persona's claimed origin) are flagged in days.

**Recipe:**
1. **Network:** residential or mobile IP from persona's claimed country (mobile is best)
2. **Device:** ideally an actual mobile device (not just a desktop with mobile UA) — Meta's app-based signup is more permissive than web
3. **Email:** ProtonMail / Outlook
4. **Phone:** real SIM, ideally prepaid in persona's country
5. Sign up via the **mobile app** on a fresh-imaged device or emulator (anti-detect browsers like GoLogin or Multilogin help)
6. Don't immediately set profile photo — Meta's photo-based dupe detection runs on every upload. Wait 24h.
7. Set photo, bio
8. **Ageing — Meta-specific protocol:**
   - Week 1: Add **2-3** friends (ideally other sock puppets in your stable, never the same IP). Like 5 posts/day from algorithmic feed
   - Week 2: Join 2-3 large generic groups (cooking, sports, gardening). React, don't post yet
   - Week 3-4: Post 1 benign content (recipe, photo, comment)
   - Week 5-6: Now usable for low-risk investigation
   - Week 8+: Now reasonably durable
9. **Instagram** uses the same Meta account; same protocol applies. Cross-post benign content from FB to IG to build "consistent" identity.

**Common failure modes:**
- "Confirm your identity" — Meta will demand government ID. **You will not pass this.** Account is dead. Don't argue.
- "Account temporarily restricted, your phone has been used too many times" — your SIM has been used for too many Meta accounts; rotate
- Sudden ban at 3-4 weeks — typically the social-graph anomaly check kicked in

---

### LinkedIn

```
Difficulty:        4 / 5
Phone required:    For most features
Email required:    Yes — work-style domain helps
Account ageing:    4 weeks
Detection vectors: real-name expectation, employment plausibility
```

**Recipe:**
1. **Network:** residential, ideally country-matched
2. **Email:** custom-domain email (Fastmail / your own domain) is **dramatically more plausible** here than free email for the corporate-feel
3. Sign up at `linkedin.com/signup`
4. Persona must include:
   - A real-but-vague employer ("Independent consultant" works; "ACME Corp Senior Engineer" gets reverse-checked)
   - A school (use a real but moderately-large university)
   - Location (city)
5. Profile must include some employment history (~3 jobs over ~10 years) — make them small enough to not get reverse-checked but real enough to look plausible
6. **Photo:** professional headshot, not casual social-media style
7. **Ageing:**
   - Day 1-7: Connect with **5-10 mainstream accounts** matching persona industry (recruiters happily accept)
   - Day 7-21: Engage with content — like, occasional comment
   - Day 21+: Investigation use
8. Avoid Sales Navigator unless you have a paid account that won't burn

**Common failure modes:**
- Restricted on first connection request — typical, give it 24h and try again
- "You've been viewed by recruiters" — good, your persona is plausible
- Account restricted after 30+ profile views/day — slow down

---

### Reddit

```
Difficulty:        2 / 5  (relatively easy)
Phone required:    No (email only)
Email required:    Yes
Account ageing:    2 weeks for trust, varies by subreddit
Detection vectors: behavioural patterns, karma anomaly
```

**Recipe:**
1. **Network:** residential or VPN OK; Tor often blocked at signup
2. **Email:** any email; verify
3. Sign up at `reddit.com/register`
4. Pick username consistent with persona
5. **Ageing:**
   - Week 1: Subscribe to ~20 generic subs (news, AskReddit, your country sub, hobby subs). Comment ~1-2x/day on benign content
   - Week 2: Build karma — aim for 100+ karma before sub-specific work
   - Week 2+: Investigation use; be aware many specialised subs require minimum karma + account age

**Common failure modes:**
- "Suspicious activity" — typically means the IP was used for too many recent signups
- Subreddit-specific shadowbans — common; check via `r/ShadowBan` testing posts

---

### Discord

```
Difficulty:        3 / 5
Phone required:    For most server access (verification)
Email required:    Yes
Account ageing:    1-2 weeks
Detection vectors: VoIP detection, behavioural
```

**Recipe:**
1. **Network:** residential VPN or mobile
2. **Email:** any
3. Sign up at `discord.com/register`
4. Verify email immediately
5. **Phone verification** is required for most servers (gateway to servers that have "verified phone" requirement). Use a real SIM
6. Set username and avatar
7. Join 3-5 large benign servers (gaming, general chat) for activity / status
8. **Ageing:** light chat activity for 1-2 weeks before investigation server entry
9. **For private/invite-only investigation servers:** the social engineering is the point — you'll need a referrer + plausible story for entry

**Common failure modes:**
- Phone reuse — Discord aggressively blocks phones used for >3 accounts
- New-account spam-flag — light activity in benign servers fixes it

---

### Telegram

```
Difficulty:        2 / 5  (one of the easier major messengers)
Phone required:    YES (no email-only signup)
Email required:    No
Account ageing:    Minimal
Detection vectors: phone-number country, behavioural in private channels
```

**Recipe:**
1. **Network:** Tor + Telegram works (unique among major messengers); residential / mobile better for plausibility
2. **Phone:** **REAL SIM REQUIRED** — Telegram detects most VoIP / SMS-service numbers as "spam-prone" and silently restricts the account
3. Install Telegram app (mobile preferred) or `telegram-desktop`
4. Verify with SMS code
5. Set username, photo, bio
6. **Public channel access** requires no further setup — many investigations are read-only on public channels
7. **For private group / channel infiltration:** plausibility within that group's culture is what matters; build mutual contact via ageing

**Common failure modes:**
- "Your account has been limited" — phone-number reputation; rotate SIM
- Channel-specific shadow filters — rare, hard to detect

---

### WhatsApp

```
Difficulty:        4 / 5
Phone required:    YES, primary identifier
Email required:    No
Account ageing:    Limited use case for OSINT — primarily for direct contact
Detection vectors: phone-number range, contact-graph anomaly
```

**Reality check:** WhatsApp is **not a typical OSINT target** because it's E2E-encrypted and group / contact discovery are limited. Sock puppet WhatsApp is mostly used for **direct source contact** in journalism / PI work, not bulk investigation.

**Recipe:**
1. **Phone:** real SIM, ideally prepaid in persona country
2. WhatsApp installs and verifies
3. Set photo, name, bio
4. **The phone number is the identity.** No persona consistency beyond that.

**Common failure modes:**
- Number reuse / rotation triggers ban
- Mass joining of public groups triggers limit

---

### Mastodon (Federated / ActivityPub)

```
Difficulty:        1 / 5  (easiest of all major platforms)
Phone required:    No
Email required:    Yes
Account ageing:    Minimal
Detection vectors: instance-level moderation rules vary
```

**Recipe:**
1. **Network:** anything works including Tor (instance-dependent)
2. Pick instance based on persona — `mastodon.social` (largest, English), `chaos.social` (DE/EN tech), `framapiaf.org` (FR), `mstdn.jp` (Japanese)
3. Sign up at `<instance>/auth/sign_up`
4. Verify email
5. Profile: photo, bio, header
6. Follow ~30 accounts immediately for federated-timeline visibility
7. **Investigation use:** can begin same-day for public-timeline analysis; private-channel infiltration works similar to Twitter

**Common failure modes:**
- Instance-specific moderation suspends the account — usually for explicit ToS violation
- Federated timeline empty — your instance has limited federation; pick a bigger instance

---

### Bluesky (AT Protocol)

```
Difficulty:        1 / 5
Phone required:    No (email only since Feb 2024)
Email required:    Yes
Account ageing:    Minimal
Detection vectors: low — Bluesky is currently sock-puppet permissive
```

**Recipe:**
1. **Network:** anything; even Tor mostly works
2. Visit `bsky.app/sign-up`
3. Email + password + handle
4. Set photo, bio
5. Investigation use can begin same-day
6. **Public read-only access** doesn't even require an account in many cases (`public.api.bsky.app`)

**Common failure modes:**
- Handle conflicts (someone else has it)
- Custom-domain handle setup is more involved

---

### VK (Russian / RUNet)

```
Difficulty:        3 / 5
Phone required:    Yes (RU SIM strongly preferred)
Email required:    Yes
Account ageing:    2-4 weeks
Detection vectors: foreign-IP flagging, phone-country mismatch
```

**Recipe:**
1. **Network:** **Russian residential IP** is dramatically more plausible than VPN exit anywhere else
2. **Phone:** Russian SIM is best; Eastern-European SIM is OK; Western SIMs are flagged
3. Sign up at `vk.com/join`
4. Profile: Russian-language bio, photo, location set to a real RU city
5. **Ageing:** join 5-10 RU-language groups (any topic), like and share within them for 2-4 weeks
6. Investigation use after ageing

**Common failure modes:**
- Foreign-IP signups are rate-limited / often outright blocked
- Western phone numbers fail SMS at signup
- Empty profile flagged within days

---

### Weibo (China)

```
Difficulty:        5 / 5  (effectively impossible without CN SIM)
Phone required:    YES — CN SIM REQUIRED
Email required:    No (phone-only signup typically)
Account ageing:    Long — 4-8 weeks
Detection vectors: every imaginable
```

**Reality check (2026):** Weibo, WeChat, Douyin, and most major CN platforms now require a **PRC mobile number** (and increasingly real-name / 实名认证). VoIP, foreign, and SMS-service numbers fail.

**Practical options:**
- **Get a real CN SIM via diaspora contact** (legal grey area depending on jurisdiction)
- **Pay for a vetted "aged" account from a CN broker** (legally and ethically dubious; account-history may be tied to a real person's PII)
- **Skip account creation; use logged-out web access** for what's available

**Recipe (if you have a CN SIM):**
1. **Network:** ideally CN residential (very hard outside CN); HK / SG residential as fallback
2. **Phone:** CN SIM required
3. Sign up via `weibo.com` mobile app
4. Profile + photo
5. **Real-name verification (实名认证)** — required for many features. This effectively requires a CN ID; without it, account is "limited"
6. **Ageing:** very slow; aggressive automated detection

**Common failure modes:**
- Phone country mismatch with IP / location → frequent account suspension
- Cannot pass real-name verification → permanent "limited" status

> Most analysts working on CN topics use **logged-out scraping** + paid commercial services like Babel X / DarkOwl rather than create CN accounts.

---

### Naver (South Korea)

```
Difficulty:        5 / 5
Phone required:    YES — KR SIM strongly preferred
Email required:    Yes
Account ageing:    4+ weeks
Detection vectors: real-name verification, KR-only IP for some features
```

**Reality check:** Naver, Daum, KakaoTalk all use **real-name verification (실명인증)** tied to Korean Resident Registration Numbers (주민등록번호). Foreigners use i-PIN or alternative, but the friction is high.

**Practical options:**
- **Obtain via KR contact** — diaspora / friend with Korean residency
- **Skip — use Google search + logged-out access for what's reachable**

---

### Truth Social

```
Difficulty:        2 / 5
Phone required:    Yes
Email required:    Yes
Account ageing:    Minimal
Detection vectors: low for now
```

**Recipe:**
1. **Network:** US IP improves plausibility; not strictly required
2. Email + phone
3. Sign up at `truthsocial.com`
4. Profile + bio
5. Investigation-relevant content (US far-right ecosystem) is broadly accessible without ageing

---

### 4chan / 8kun

```
Difficulty:        N/A — anonymous by design, no account
Network:           Tor often blocked at posting
Email required:    No
Detection vectors: post-style, IP at posting time
```

No account creation. Read-only access via web or `4chan-api`. Posting requires non-Tor IP usually; investigation rarely needs posting.

---

## Maintaining a Stable of Sock Puppets

If you do this work professionally, you'll have multiple sock puppets in different jurisdictions / cultures. Hygiene matters:

| Practice | Why |
|:---------|:----|
| **Document each sock puppet** in a password manager / case file: persona, email, password, phone (if applicable), creation date, ageing milestones, current trust score | You will forget which is which |
| **One VM (or browser profile + container) per sock puppet** | Cookies / fingerprints leak across identities otherwise |
| **Different exit IPs per sock puppet** | Same exit reusing across personas = correlation |
| **Different timezones / activity patterns per sock puppet** | All 9-to-5 UK is detectable |
| **Different writing styles per sock puppet** | Stylometry can correlate accounts |
| **Rotate or retire sock puppets** that stop working | Don't try to revive a "limited" account |
| **Periodic "warm-up" of dormant accounts** | Once-a-month likes / follows keep them looking alive |

---

## Detection Vectors to Beware (cross-platform)

| Vector | Explanation |
|:-------|:------------|
| **Browser fingerprint** | Canvas / fonts / screen / audio / WebGL — see [[VMs_and_Compartmentalization#Browser Fingerprint Hardening]] |
| **TLS fingerprint (JA3)** | Chrome's TLS handshake differs from Python `requests`'s. `curl_cffi`, `tls-client` libs help |
| **IP / ASN reputation** | Datacenter IPs are flagged; residential is better; mobile is best for new accounts |
| **Behavioural patterns** | No mouse movement, fast clicks, immediate follow of investigation target — all flags |
| **Account age + history** | Empty new accounts are flagged; activity history at follower scale helps |
| **Social graph anomaly** | New accounts with no mutuals followed by an investigation target = visible flag |
| **Photo / file fingerprint** | Same photo across accounts is detected within seconds at major platforms |
| **Stylometric correlation** | Same person writes the same way; multiple sock puppets posting alike = visible |
| **Time-of-day patterns** | 24/7 activity = bot / multi-puppet operator |
| **Email domain** | Disposable mail domains (Mailinator, Guerrilla) are blocklist-known |

---

## When Sock Puppets Aren't Enough — Other Options

Don't reach for a sock puppet first. Consider:

| Alternative | Use case |
|:------------|:---------|
| **Logged-out / public scraping** | Many platforms still serve public content without auth |
| **Wayback / Archive.today snapshots** | Historical reads with no live exposure |
| **Commercial monitoring tools** ([[Commercial_Tools#Threat-Intelligence Platforms]]) | Buying access via Recorded Future / Flashpoint avoids account-creation legal/ethical issues |
| **Vetted-researcher API tiers** | Twitter Research API (academic), Meta Content Library (post-CrowdTangle, journalist-vetted) |
| **Working with a partner who has access** | Diaspora collaborator with a real account often beats any sock puppet |

---

## Legal / Ethical Boundary Cases

| Situation | Position |
|:----------|:---------|
| Creating a sock to **monitor public posts** of a target | Generally OK in most jurisdictions; check ToS |
| Creating a sock to **infiltrate a private group with consent of a member** | Generally OK; document consent |
| Creating a sock to **infiltrate a private group without consent** | Likely violates ToS; may violate CFAA / equivalents in some jurisdictions for some groups |
| Creating a sock to **impersonate a real person** | Frequently illegal (anti-impersonation statutes); definitely a ToS violation |
| Creating a sock to **engage targets in conversation** | Edges into entrapment / agent-provocateur territory; legal review essential |
| Creating a sock to **harass / dox** | Illegal everywhere relevant |

---

## See Also

- [[Social_Media_APIs]] — Platform API matrix this page mirrors
- [[VMs_and_Compartmentalization]] — Environment hygiene before any sock-puppet work
- [[Browser_Extensions]] — Container / fingerprint extensions
- [[Distros]] — Where these recipes are typically run from
- [[Darkweb_Forums]] — Sock-puppet hygiene matters most for dark-web work
- [[Commercial_Tools]] — Alternatives to creating your own accounts
- [[OSINT]] — Folder index
