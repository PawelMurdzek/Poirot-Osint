# Parrot OS for OSINT

**Parrot Security OS** is a Debian-based security distro from the Parrot team — a competitor / complement to Kali. It has a stronger **anti-forensics + privacy** posture than Kali and ships several tools that aren't in Kali or the [[Distros#TraceLabs OSINT VM|TraceLabs OSINT VM]].

---

## Editions

| Edition | Use case |
|:--------|:---------|
| **Parrot Home** | Daily-driver privacy-respecting OS — no aggressive pen-test tools by default |
| **Parrot Security (formerly Parrot Sec)** | Full pen-test + OSINT toolkit, comparable to Kali |
| **Parrot Architect** | Minimal install, build from base |
| **Parrot HackTheBox** | HackTheBox-tuned variant |

For OSINT work, **Parrot Security** is the relevant edition.

- Download: [parrotsec.org](https://www.parrotsec.org/)
- Default user: `user` / install-time password
- Format: Live ISO, OVA, Docker

---

## What Parrot Has That TraceLabs / Kali Default Don't

### AnonSurf
**Parrot's flagship anti-forensics tool.** Routes all system traffic through Tor (or i2p), with one-click toggle. Goes further than Kali's `proxychains` — system-wide rather than per-app.

```bash
# Start full-system Tor routing
sudo anonsurf start

# Stop and clear Tor circuit
sudo anonsurf stop

# Restart for a fresh circuit
sudo anonsurf change

# Status
sudo anonsurf status

# Run only DNS through Tor (lighter)
sudo anonsurf dns
```

Why this matters for OSINT:
- One-command Tor routing for the entire investigation VM
- Less brittle than configuring per-tool proxychains
- Lower risk of accidental clear-net leaks during investigation

> Caveat: AnonSurf doesn't do everything Whonix does. Apps with their own networking stack can still leak. For sensitive work, use Parrot **inside** Whonix Workstation or use Qubes Whonix templates — see [[VMs_and_Compartmentalization]].

### Pandora
**RAM wiping tool** — overwrites RAM on shutdown so cold-boot attacks can't recover keys / session data. Particularly relevant for journalists / activists who may face physical seizure.

### MAT2 (Metadata Anonymisation Toolkit 2)
Strips metadata from files before sharing. Pre-installed on Parrot, also available on other distros via apt.

```bash
# Strip metadata from a single file
mat2 photo.jpg

# Bulk
mat2 *.jpg

# Inspect what would be stripped, without writing
mat2 --show photo.jpg
```

Useful when:
- Publishing OSINT findings — strip your investigation device's signature from leaked screenshots
- Cleaning sock-puppet uploaded content (avoid same-camera fingerprint linking accounts)
- Preparing evidence for sharing with sources

### Ricochet Refresh
**Anonymous instant messaging over Tor hidden services.** No central server, no metadata leak to a server operator. Niche but used by journalists / sources / activists.

### OnionShare (also on other distros, but well-integrated)
File / website / chat sharing through Tor hidden services, ephemeral.

```bash
# Share a file via .onion URL
onionshare-cli /path/to/file
```

### ZuluCrypt / ZuluMount
GUI for encrypted volume management — easier than CLI `cryptsetup`. Useful for compartmentalising case data.

### Frost Browser (preconfigured Firefox)
Firefox with privacy-hardening defaults baked in. Less than Tor Browser, more than vanilla Firefox.

### Other Parrot extras worth noting
- **GtkHash** — file integrity checksum GUI
- **Veracrypt** — preinstalled
- **i2p** — preinstalled and integrated, in addition to Tor
- **NoseyParker** — secret-finding (also available on other distros)
- **macchanger** with Parrot wrappers — convenient MAC randomisation per-boot

---

## What TraceLabs OSINT VM Has That Parrot Doesn't

| Tool / Feature | TraceLabs | Parrot |
|:---------------|:----------|:-------|
| Curated **Firefox extension stack** for OSINT | ✅ (preinstalled) | ❌ (you build it manually — see [[Browser_Extensions]]) |
| **Bookmark folders** organised by selector type | ✅ | ❌ |
| **Hunchly** preinstalled | ✅ (licence required) | ❌ |
| **Maltego CE** preconfigured | ✅ | Available via apt, not preconfigured |
| **Recon-ng** with marketplace | ✅ | Available via apt |
| **TraceLabs report template** | ✅ | ❌ |
| **Curated** out-of-box OSINT workflow | ✅ | ❌ — broader pentest focus |

So **TraceLabs is OSINT-first**, **Parrot is privacy-first**. They overlap but optimise different ends.

---

## Parrot vs Kali (at a glance)

| Aspect | Kali | Parrot |
|:-------|:-----|:-------|
| Package base | Debian-based | Debian-based |
| Pentest tooling | Comprehensive | Comprehensive |
| Anti-forensics | Light | **Heavy** (AnonSurf, Pandora, MAT2) |
| Anonymity stack | Manual setup (proxychains, Tor) | **Built-in** (AnonSurf, i2p preinstalled) |
| Default desktop | Xfce / GNOME / KDE options | MATE / KDE / Xfce |
| Resource use | Moderate | **Lighter** (especially on MATE) |
| HTB / CTF integration | Kali Purple, etc. | Parrot HTB edition |
| Documentation | Larger community | Smaller, but solid |
| Updates | Rolling | Rolling |
| Default shell hardening | Minimal | More aggressive |
| Encrypted persistence | Possible | Easier with built-in tools |

---

## When to use which

### Use Parrot when…
- You need **system-wide Tor routing** without VM gateway hassle (AnonSurf)
- You're investigating **sensitive sources** who may be doxxed if device is seized (Pandora RAM wipe)
- You want **lower resource usage** on older hardware
- You publish findings and need to **strip metadata** (MAT2 built-in)
- **Privacy is a primary concern**, not pen-testing

### Use TraceLabs OSINT VM when…
- You're doing **TraceLabs CTFs** or missing-persons OSINT specifically
- You want **zero-config OSINT workflow** out of the box
- You need **Hunchly / Maltego / Recon-ng** all pre-wired
- The pen-test toolchain isn't relevant — you want OSINT-only

### Use Kali when…
- OSINT is **one phase** of a wider pen-test engagement
- You want the **broadest pentest tool coverage**
- You're already trained on Kali muscle-memory

### Use Whonix / Qubes when…
- OPSEC stakes are **higher than Parrot's AnonSurf gives you**
- You're touching [[Darkweb_Forums|dark-web sources]] or politically-sensitive targets
- You need **hardware-level isolation** between investigation contexts

> **You can layer these.** A common analyst stack: Qubes-OS host → Whonix-Workstation template with Parrot tools installed → AnonSurf optional inside the Workstation. Layered defence; even if any one layer fails, the others contain the leak.

---

## Installing OSINT-specific tools on Parrot

If you want to add the TraceLabs-style OSINT toolkit to Parrot:

```bash
# Update first
sudo apt update && sudo apt upgrade

# Core OSINT tools (most ship by default but to be sure)
sudo apt install theharvester recon-ng maltego sherlock photon \
                 metagoofil exiftool spiderfoot

# Python tools via pip
pip3 install holehe maigret toutatis instaloader snscrape phoneinfoga

# h8mail (breach lookup, requires API keys)
pip3 install h8mail

# OSINT-specific Firefox extensions: install manually in your investigation Firefox profile
# See [[Browser_Extensions]] for the full list
```

You can also clone the **TraceLabs bookmarks** as JSON and import them into your Parrot Firefox profile — they're published on the TraceLabs GitHub.

---

## Hardening Parrot for OSINT

Beyond the defaults:

```bash
# Disable network managers that might leak DNS
# Use systemd-resolved with DNS-over-TLS

# MAC randomisation per boot (already easy on Parrot)
sudo macchanger -r eth0

# Firewall — Parrot ships with ufw
sudo ufw default deny incoming
sudo ufw default deny outgoing
sudo ufw allow out 9050/tcp     # Tor SocksPort if using AnonSurf
sudo ufw enable

# Disable Bluetooth
sudo systemctl disable bluetooth
sudo systemctl stop bluetooth

# Disable IPv6 if your investigation doesn't need it
sudo sysctl -w net.ipv6.conf.all.disable_ipv6=1
```

---

## See Also

- [[Distros]] — Comparison with TraceLabs / Kali / Whonix / Tails / Qubes
- [[VMs_and_Compartmentalization]] — Layering Parrot inside Qubes / Whonix
- [[Browser_Extensions]] — Building the OSINT browser stack manually on Parrot
- [[Tools_Kali_Tracelabs]] — Same tools, different distro
- [[Darkweb_Forums]] — Where Parrot's anti-forensics features matter most
- [[OSINT]] — Folder index
