# OSINT Distros

Linux distributions optimised for OSINT, threat intel, and digital forensics. Use a fresh, snapshotted instance per case — see [[VMs_and_Compartmentalization]].

---

## TraceLabs OSINT VM

**The reference OSINT distro.** Built by [Trace Labs](https://www.tracelabs.org/) for their missing-persons CTFs, but used widely for general OSINT.

| Property | Detail |
|:---------|:-------|
| Base | Kali Linux |
| Download | [tracelabs.org/initiatives/osint-vm](https://www.tracelabs.org/initiatives/osint-vm) |
| Format | OVA (VirtualBox/VMware import) |
| Username / password | `osint` / `osint` |
| Updated | Periodically — check release notes |

**What it ships with:**
- All [[Tools_Kali_Tracelabs|Kali OSINT tools]] pre-configured
- Curated [[Browser_Extensions|Firefox extension stack]]
- Bookmark folders organised by selector type (email, username, phone, etc.)
- Hunchly (licence required separately)
- Maltego CE pre-installed
- Recon-ng with marketplace populated
- TraceLabs report template

**When to use it:** First choice for general-purpose OSINT, especially if you're new. Saves hours of setup.

---

## Kali Linux

Standard pentesting distro. Includes most OSINT tools (`theHarvester`, `recon-ng`, `maltego`, `sherlock`, `spiderfoot`, …) but **not** the curated browser stack or bookmarks.

| Property | Detail |
|:---------|:-------|
| Download | [kali.org](https://www.kali.org/) |
| Format | ISO / pre-built VMware/VirtualBox/Hyper-V/QEMU images / WSL / cloud / ARM |
| Username / password | `kali` / `kali` (default), change on first boot |

```bash
# Install OSINT meta-package
sudo apt update
sudo apt install kali-tools-osint   # or: kali-linux-large for everything
```

**When to use it:** You already work in Kali for active recon and want OSINT in the same environment.

---

## CSI Linux

Investigations-focused distro: OSINT, dark-web, incident response, forensics. Heavier than TraceLabs OSINT VM, more SOC/IR oriented.

| Property | Detail |
|:---------|:-------|
| Base | Ubuntu |
| Download | [csilinux.com](https://csilinux.com/) |
| Format | OVA / installer |
| Notes | Wraps Tor, I2P, blockchain analysis tools, social-media scrapers, malware sandboxing |

**When to use it:** Investigations that touch dark-web ([[Darkweb_Forums]]), cryptocurrency tracing, or blend OSINT with incident response.

---

## Tsurugi Linux

DFIR distro with strong OSINT chapter. Italian project.

| Property | Detail |
|:---------|:-------|
| Base | Ubuntu |
| Download | [tsurugi-linux.org](https://tsurugi-linux.org/) |
| Variants | Tsurugi Lab (full DFIR), Tsurugi Acquire (live boot, hash & image), Tsurugi Bento (portable USB toolkit) |

**When to use it:** Forensics-heavy work where OSINT is one phase of a wider DFIR investigation.

---

## Parrot Security OS

Debian-based; **privacy / anti-forensics first**, OSINT and pentest second. Strong overlap with Kali but with a different posture.

| Property | Detail |
|:---------|:-------|
| Base | Debian |
| Download | [parrotsec.org](https://www.parrotsec.org/) |
| Format | ISO / OVA / Docker |
| Default user | `user` / install-time password |
| Editions | Home (daily-driver privacy), Security (pentest+OSINT), HTB (HackTheBox) |

**What sets Parrot apart for OSINT:**
- **AnonSurf** — one-command system-wide Tor routing (more comprehensive than `proxychains`)
- **Pandora** — RAM wiping on shutdown (anti-cold-boot)
- **MAT2** — metadata stripping (preinstalled)
- **Ricochet Refresh** — anonymous IM over Tor hidden services
- **i2p, Tor, OnionShare** — preinstalled and integrated
- **Lighter on resources** than Kali — runs reasonably on older hardware

**Trade-off vs TraceLabs:** Parrot does *not* ship with the curated OSINT browser stack, bookmark folders, or Hunchly preinstalled — those need manual setup. But its **anonymity tooling** is stronger out-of-box.

**When to use it:** OSINT involving sensitive sources, dissident or political research, or any investigation where the device might face seizure. Often combined with Whonix Workstation for layered anonymity.

> Full deep-dive: [[Parrot_OS]]

---

## Buscador (legacy — unmaintained)

Once the standard before TraceLabs OSINT VM. Last released around 2019, **no longer updated**, kept here as a historical pointer. Don't use it for new investigations.

---

## Whonix

Two-VM anonymity-focused distro: a **Gateway** that routes all traffic through Tor, and a **Workstation** that has no other route to the internet. Even if the Workstation is compromised, the attacker sees only the Gateway's exit-node IP.

| Property | Detail |
|:---------|:-------|
| Base | Debian + Tor |
| Download | [whonix.org](https://www.whonix.org/) |
| Format | OVA pair (Gateway + Workstation), or Qubes templates |
| Default user | `user` (no password by default) |

**When to use it:** Any work that touches dark-web sources, sensitive targets, or where attribution back to you is unacceptable. See [[Darkweb_Forums]] and [[VMs_and_Compartmentalization]].

> [!IMPORTANT]
> Whonix routes Workstation traffic through Tor, but it does not anonymise your **investigation behaviour** — fingerprinting, account logins, and writing style still identify you. OPSEC is layered.

---

## Tails

Amnesic, live-USB OS. Boots from USB, leaves no trace on the host disk, all traffic forced through Tor.

| Property | Detail |
|:---------|:-------|
| Base | Debian + Tor |
| Download | [tails.net](https://tails.net/) |
| Format | USB image, written via `dd` / Rufus / official installer |

**When to use it:** Single-session work from a borrowed/untrusted machine, or when you want guaranteed no on-disk artefacts. Persistent storage is opt-in (encrypted).

**Limitations:** Slower than a full VM, can't snapshot, fewer tools, and writing custom configs is annoying.

---

## Qubes OS

Hypervisor-as-the-OS. Each "qube" (VM) is isolated; you compose your workflow from disposable qubes, named templates, and explicit inter-qube communication.

| Property | Detail |
|:---------|:-------|
| Base | Xen hypervisor + Fedora/Debian/Whonix templates |
| Download | [qubes-os.org](https://www.qubes-os.org/) |
| Hardware | Demanding — 16 GB RAM minimum, IOMMU/VT-d required |

**When to use it:** Long-running OSINT analyst workflow where you need strong compartmentalisation by default. Each case = own template + disposable qubes spawned from it. Whonix Gateway/Workstation integrate as Qubes templates.

See [[VMs_and_Compartmentalization#Qubes OS|VMs and Compartmentalization → Qubes OS]] for setup notes.

---

## Choosing — Quick Decision Matrix

| Need | Pick |
|:-----|:-----|
| First OSINT distro, general use | [[#TraceLabs OSINT VM]] |
| Already work in pentesting | [[#Kali Linux]] |
| Privacy + anti-forensics first | [[Parrot_OS]] |
| Heavy dark-web / crypto / IR | [[#CSI Linux]] |
| Forensics with OSINT phase | [[#Tsurugi Linux]] |
| Strong anonymity (dark-web sources) | [[#Whonix]] inside [[#Qubes OS]] |
| Single-session, no traces | [[#Tails]] |
| Long-running analyst workflow | [[#Qubes OS]] |

---

## See Also

- [[OSINT]] — Folder index and methodology
- [[VMs_and_Compartmentalization]] — How to wire these into a workflow
- [[Browser_Extensions]] — Extensions to layer onto whichever distro
- [[Tools_Kali_Tracelabs]] — Tooling that ships with most of these
- [[Darkweb_Forums]] — When you need Whonix/Tails specifically
