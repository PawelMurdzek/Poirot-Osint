# VMs & Compartmentalization for OSINT

How to structure isolated environments so an investigation can't leak data about you, taint other cases, or cross-contaminate sock-puppet identities.

---

## Why Compartmentalize

| Risk | What happens without isolation |
|:-----|:------------------------------|
| **Browser fingerprinting** | Sites correlate your investigation visits with your daily-driver identity via canvas/font/audio fingerprint |
| **Account contamination** | One logged-in Google tab = all your searches tied to your real name |
| **Cross-case leakage** | Cookies/auto-fill from Case A appear when you investigate Case B's target |
| **Sock-puppet exposure** | Two sock-puppets logged in from the same IP and fingerprint = one sock-puppet (target spots it) |
| **Malware** | Investigating a phishing kit drops malware on your daily-driver |
| **Legal discovery** | "Why was this evidence on the same machine as your personal banking?" |

---

## The Three-Tier Model

```
┌──────────────────────────────────────────┐
│   HOST OS (your physical laptop)         │
│   - Boring daily-driver                  │
│   - Never used for investigations        │
│   - Full disk encryption mandatory       │
└──────────────────────────────────────────┘
              │ runs ▼
┌──────────────────────────────────────────┐
│   INVESTIGATION VM(s)                    │
│   - Per-case OR per-identity             │
│   - Snapshot before each session         │
│   - Roll back when done                  │
└──────────────────────────────────────────┘
              │ optionally routes through ▼
┌──────────────────────────────────────────┐
│   ANONYMITY GATEWAY                      │
│   - Whonix Gateway / Tor / VPN-chain     │
│   - All investigation traffic exits here │
└──────────────────────────────────────────┘
```

---

## Hypervisor Options

### VirtualBox
Free, cross-platform. Snapshots, OVA import, NAT/Bridged networking.

```bash
# Import an OVA (e.g. TraceLabs OSINT VM)
VBoxManage import tracelabs.ova

# Take a snapshot before each session
VBoxManage snapshot "OSINT-VM" take "before-case-X"

# Roll back after the session
VBoxManage snapshot "OSINT-VM" restore "before-case-X"

# Headless mode
VBoxManage startvm "OSINT-VM" --type headless
```

**Strengths:** Easy, free, runs everywhere.
**Weaknesses:** Weaker isolation than Type-1 hypervisors (Qubes/ESXi), occasional graphics weirdness.

---

### VMware Workstation Pro / Player
Now free for personal use (2024+). Snapshots, linked clones, better graphics than VirtualBox.

```
File → Open → tracelabs.ova
VM → Snapshot → Take Snapshot...
VM → Snapshot → Revert to Snapshot
```

**Linked clones** — spin up many lightweight VMs from one base image. Useful for one-VM-per-sock-puppet.

---

### Qubes OS
Type-1 hypervisor (Xen) where the *whole OS* is compartmentalisation. Replaces your host OS rather than running on top of one.

**Concepts:**
- **TemplateVM** — base OS image (Debian, Fedora, Whonix-Gateway, Whonix-Workstation). You install software here.
- **AppVM** — usable VM cloned from a template. Persistent home directory only.
- **DispVM (Disposable)** — fully throwaway VM, destroyed when its window closes. Perfect for "open this suspicious link".
- **Qube** — generic name for any of the above.

**Typical OSINT layout:**

```
sys-net           — network-facing qube (USB Wi-Fi, etc.)
sys-firewall      — firewall qube, all traffic flows through here
sys-whonix        — Whonix Gateway, tor connection
anon-whonix       — Whonix Workstation for dark-web work
osint-case-A      — AppVM for active case A, persistent
osint-case-B      — AppVM for active case B, persistent
disp-firefox      — disposable, opens once for one-off browsing
sock-alice        — AppVM dedicated to sock-puppet "Alice"
sock-bob          — AppVM dedicated to sock-puppet "Bob"
vault             — offline qube holding case notes, no network
```

**Strengths:** Best-in-class isolation by default, USB/network attacks are contained, easy to throw away tainted qubes.
**Weaknesses:** Steep learning curve, demanding hardware (16 GB RAM minimum, IOMMU required), some hardware unsupported.

---

### Whonix Gateway + Workstation (any hypervisor)

Two-VM pattern that forces a workstation through Tor:

```
[Workstation VM] --- internal-network --- [Gateway VM] --- Tor --- Internet
                                              ▲
                                only Gateway can reach internet directly
```

**Setup in VirtualBox/VMware:**
1. Import both Whonix-Gateway and Whonix-Workstation OVAs.
2. Both VMs have a NIC on the `Whonix` internal network.
3. Gateway has a second NIC on NAT/Bridged for upstream internet.
4. Workstation has *only* the internal NIC.
5. Boot Gateway first, wait for Tor circuit, then boot Workstation.

In **Qubes**, this is a one-click template — just clone the Whonix templates and assign Workstations to use the Whonix Gateway as `NetVM`.

---

## Snapshot Hygiene

| Stage | Action |
|:------|:-------|
| **Build** | Install fresh distro, harden, install tools, log in to nothing → snapshot as `clean` |
| **Per case** | Clone `clean` → name `case-<id>` → work, save case data to host-mounted folder or `vault` qube |
| **Per session** | Snapshot `case-<id>` as `case-<id>-day-N` before logging in to any account |
| **End of case** | Export case data, then delete the clone entirely |
| **If anything weird happens** | Roll back to the last snapshot, do not continue forward |

---

## Browser Fingerprint Hardening

Even inside a fresh VM, the browser fingerprint is a giveaway. Layered defence:

1. **Use a fresh Firefox profile per identity** — not just a new container.
2. **CanvasBlocker + uBlock Origin + NoScript** — see [[Browser_Extensions]].
3. **Resist fingerprinting flag** in Firefox: `about:config` → `privacy.resistFingerprinting = true`.
4. **Don't resize the window** while resistFingerprinting is on (it spoofs a fixed resolution; resizing leaks).
5. **Disable WebRTC**: `media.peerconnection.enabled = false`.
6. **Test:** [coveryourtracks.eff.org](https://coveryourtracks.eff.org/), [browserleaks.com](https://browserleaks.com/).

Tor Browser already does most of this. For non-Tor work, **Mullvad Browser** (Tor Browser without Tor) is a good base.

---

## Sock-Puppet Identity Hygiene

A sock-puppet is a believable persona used to access platforms that require login (LinkedIn, Facebook, dark-web forums).

**Per-identity isolation:**
- Dedicated VM or Qubes AppVM, *never reused* for another identity
- Dedicated VPN exit (or dedicated Tor circuit, where allowed)
- Dedicated email (ProtonMail / Tutanota / fastmail aliases)
- Dedicated phone number for SMS verification (Twilio / textverified — note many platforms now block VoIP numbers)
- Believable backstory: name, age, employer, location, profile photo (use ThisPersonDoesNotExist + check it's not in PimEyes)
- Believable history: ageing the account for weeks before using it for investigation
- **Never log in to two sock-puppets from the same VM, IP, or browser fingerprint**

> [!CAUTION]
> Sock-puppets are a legal grey area. Many platforms' Terms of Service prohibit fake accounts; some jurisdictions criminalise impersonating real people. Get legal sign-off before going further than passive monitoring.

---

## Networking Patterns

### Plain VM → NAT
Default. Your VM goes out via your home/office IP.
- **Use when:** Target is non-sensitive, no anonymity required.

### VM → VPN (host or VM-level)
Adds one hop. VPN provider sees your traffic patterns; site sees the VPN exit.
- **Use when:** You want geographic flexibility (US-only sites, RU-only sites) or basic IP-decoupling.

### VM → Tor (Whonix Gateway, Tails)
Strong anonymity, slow, many sites block Tor.
- **Use when:** Dark-web work, sensitive sources, attribution must be hard. See [[Darkweb_Forums]].

### VM → VPN → Tor *or* Tor → VPN
- **VPN → Tor:** ISP sees VPN, Tor entry sees VPN, exit sees nothing about you. Hides Tor use from ISP.
- **Tor → VPN:** Lets you reach sites that block Tor exits. Adds attack surface.
- Don't combine these unless you know exactly why.

---

## What Goes Wrong

| Mistake | Consequence |
|:--------|:------------|
| Logging into Google in the investigation VM | Every search forever tied to that account |
| Using the same VM for two sock-puppets | Platforms link the accounts immediately |
| Forgetting to roll back the snapshot | Cookies/history accumulate, fingerprint stabilises |
| Mounting host folders read-write into investigation VM | Malware in the VM can write to host |
| Using the same Tor circuit for personal browsing and investigation | Correlation attack feasible by anyone watching the exit |
| Resizing window with `resistFingerprinting=true` | Reveals real screen size |
| Pasting clipboard from investigation VM into host | Side-channel exfil to host clipboard manager |

---

## See Also

- [[OSINT]] — Folder index and methodology
- [[Distros]] — Which OS goes inside the VM
- [[Browser_Extensions]] — In-browser hardening
- [[Darkweb_Forums]] — Where this hardening matters most
