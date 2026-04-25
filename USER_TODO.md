# USER_TODO — co musisz zrobić ręcznie

Plik utrzymywany przez agenta `poirot-builder`. Każda nowa pozycja pojawia się dokładnie w momencie, w którym agent na coś trafi (np. provider wymaga klucza API, manualnej weryfikacji, dodania zmiennej środowiskowej).

**Konwencja statusów:**
- ⬜ — do zrobienia przez Ciebie
- ✅ — zrobione (skreślaj kiedy załatwisz)
- ⏭️ — pomijalne (provider zadziała bez tego, ale zyskasz dane jak zrobisz)

---

## Tier A — działa out-of-the-box

Wszystkie Tier A providery (Mastodon, Bluesky, Lemmy, HackerNews, 4chan archives, Wykop, DEV.to) **nie wymagają kluczy**. Nic nie musisz robić.

---

## Tier B — wymagane klucze API

Każdy z tych providerów ma w `appsettings.json` placeholder. **Provider zadziała bez klucza — po prostu loguje INFO i `yield break`** (graceful skip). Wypełnij klucz tylko jeśli chcesz konkretny provider aktywny.

### ⬜ Twitch (Helix API)

**Co dostaniesz:** lookup użytkownika Twitch po loginie — display name, opis kanału, total view count, broadcaster type, data utworzenia konta, zdjęcie profilowe.

**Krok po kroku:**
1. Wejdź na https://dev.twitch.tv/console/apps
2. Zaloguj się kontem Twitch (możesz utworzyć dedykowane jeśli chcesz)
3. Kliknij **"Register Your Application"**
4. Wypełnij:
   - **Name:** `Poirot OSINT` (cokolwiek)
   - **OAuth Redirect URLs:** `http://localhost`
   - **Category:** Application Integration
5. Skopiuj **Client ID** (widoczne od razu)
6. Kliknij **"New Secret"** → skopiuj **Client Secret** (pokazane raz, zapisz natychmiast)
7. Wklej oba do `src/SherlockOsint.Api/appsettings.json`:
   ```json
   "TwitchClientId": "<twoje_client_id>",
   "TwitchClientSecret": "<twoje_client_secret>"
   ```

**Koszt:** Darmowe. Limit: ~800 requestów/min na app access token.

---

### ⬜ Telegram (Bot API — kanały publiczne)

**Co dostaniesz:** lookup publicznego kanału/grupy Telegram po `@username` — nazwa, opis, liczba członków, link do kanału.

**Krok po kroku:**
1. Otwórz Telegram (mobile lub desktop)
2. Wyszukaj **@BotFather** i napisz `/start`
3. Napisz `/newbot`
4. Podaj nazwę bota (cokolwiek, np. "PoirotOSINT")
5. Podaj username bota (musi kończyć się na `_bot`, np. `poirot_osint_bot`)
6. BotFather odeśle ci **HTTP API Token** w formacie `123456789:ABCdef...`
7. Wklej do `appsettings.json`:
   ```json
   "TelegramBotToken": "<twoj_token>"
   ```

**Koszt:** Darmowe. Limit: 30 requestów/sekundę. **Działa tylko dla kanałów publicznych** (z `@username`), prywatne grupy wymagają infiltracji.

---

### ⏭️ VK (problematyczne spoza RU)

**Co dostaniesz:** lookup profilu VK po screen_name — imię, nazwisko, miasto, kraj, zdjęcia profilowe.

**Krok po kroku:**
1. Wejdź na https://vk.com/dev → musisz mieć konto VK (rejestracja wymaga numeru CIS — RU/BY/KZ/UA telefonu, polskie numery działają zmiennie)
2. Utwórz **Standalone Application** w sekcji "My Apps"
3. Pobierz **Service Token** (najprostszy — działa dla większości endpointów `users.get`)
4. Wklej do `appsettings.json`:
   ```json
   "VkAccessToken": "<twoj_service_token>"
   ```

**Uwaga:** Polskie/UE konta VK są często blokowane lub mają ograniczenia od 2022. Jeśli nie wyjdzie — **pomiń tego providera, reszta zadziała normalnie**.

**Koszt:** Darmowe.

---

### ⬜ Bilibili — bez klucza

Bilibili nie wymaga klucza dla podstawowego user search. Provider działa od razu, ale **anti-bot Bilibili reaguje na masowe zapytania** — przy intensywnym użyciu może blokować IP. Provider używa już realnych nagłówków przeglądarki + Referer.

---

### ⬜ Numverify (telefon → kraj / operator / line type)

**Co dostaniesz:** dla podanego numeru telefonu — kraj, operator, typ linii (mobile/landline/voip), format E.164. **Bez nazwiska** — to nie jest reverse-name-lookup.

**Krok po kroku:**
1. Wejdź na https://numverify.com/ → Sign Up
2. Free tier: **100 lookups/miesiąc**, HTTP-only (paid plan dodaje HTTPS)
3. W dashboardzie skopiuj **API Access Key**
4. Wklej do `appsettings.json`:
   ```json
   "NumverifyApiKey": "<twoj_klucz>"
   ```

**Koszt:** Darmowe (free tier). Paid od $14.99/mo za HTTPS + 5000 lookups.

### ⏭️ Reverse phone → name (PŁATNE — opcjonalne)

Numverify NIE robi `phone → real name` lookup. Dla tego potrzebujesz **płatnych providerów** — żaden nie ma free tier wartego implementacji:

| Provider | Coverage | Koszt | API |
|----------|----------|-------|-----|
| **TrueCaller** | Globalne (najsilniejsze w IN/MENA) | ~$3/mo subskrypcja, brak oficjalnego API | Mobile-only, scraping ToS-violating |
| **Sync.me** | Globalne | Pay-per-lookup, gated | Tak, ale enterprise |
| **Whitepages** | US-głównie | ~$4.99/lookup | Tak, oficjalne |
| **BeenVerified** | US | $26.89/mo | Tak |
| **Spokeo** | US | $0.95-13.95 trial | Tak |

**Rekomendacja:** Jeśli twój use-case to PL/EU — odpuść reverse-name lookup. Numverify + KRS/CEIDG (firmowe) + LinkedIn dadzą Ci więcej info. Jeśli US — WhitePages API ma najlepszy stosunek ceny do pokrycia.

Te providery NIE są zaimplementowane (poza placeholderem dla Numverify) — dopiszę kiedy zdecydujesz że potrzebujesz konkretnego.

---

## Personality profiler — ustawienia Claude

Personality profiler używa Claude API do analizy TOP-3 kandydatów po wyszukiwaniu. Wymaga **`Osint:ClaudeApiKey`** w `appsettings.json` lub jako zmiennej środowiskowej `Osint__ClaudeApiKey`.

**Bez klucza** — profiler `yield break` (czysto, bez błędu), reszta wyników nadal działa.

### ⬜ Wybór modelu Claude (backend)

W `appsettings.json`:
```json
"Osint": {
  "ClaudeApiKey": "sk-ant-…",
  "ClaudeModel": "claude-sonnet-4-6"
}
```

Domyślnie **`claude-sonnet-4-6`** — lżejszy/tańszy. Dla głębszej analizy zmień na **`claude-opus-4-7`** (~5× droższy, znacznie inteligentniejszy).

**Rekomendacja:** Zostaw Sonnet w API (uruchamia się automatycznie po każdym wyszukiwaniu). Opus używaj manualnie przez TUI + Claude Code (patrz niżej).

---

## TUI — `poirot` w terminalu

TUI to drugi klient (oprócz mobile MAUI) — Spectre.Console + SignalR.Client. Działa cross-platform: **Windows, Kali Linux, Parrot OS, macOS**.

### Uruchomienie z dev-runtime (najprostsze)

Z poziomu repo:
```bash
# 1. odpal API
cd src/SherlockOsint.Api && dotnet run

# 2. w drugim terminalu:
dotnet run --project src/SherlockOsint.Tui -- search --nick targetUser
```

### ⬜ Self-contained publish (zero zależności od .NET runtime)

Dla każdego OS-a osobno — wynik to pojedynczy plik `poirot` / `poirot.exe` który możesz przenieść gdziekolwiek:

**Windows x64:**
```powershell
dotnet publish src/SherlockOsint.Tui -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
# → src/SherlockOsint.Tui/bin/Release/net10.0/win-x64/publish/poirot.exe
```

**Linux x64 (Kali, Parrot, Debian, Ubuntu):**
```bash
dotnet publish src/SherlockOsint.Tui -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true
# → src/SherlockOsint.Tui/bin/Release/net10.0/linux-x64/publish/poirot
chmod +x src/SherlockOsint.Tui/bin/Release/net10.0/linux-x64/publish/poirot
```

**Linux ARM64 (Raspberry Pi, Apple Silicon w Parallels itp.):**
```bash
dotnet publish src/SherlockOsint.Tui -c Release -r linux-arm64 --self-contained true -p:PublishSingleFile=true
```

**macOS x64 / arm64:**
```bash
dotnet publish src/SherlockOsint.Tui -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true
# lub osx-arm64 dla Apple Silicon
```

Po publishu skopiuj plik do `~/.local/bin/poirot` (Linux/macOS) albo dodaj folder do `%PATH%` (Windows) — i masz `poirot` jako globalną komendę.

### Komendy TUI

```bash
# Wyszukiwanie
poirot search --nick targetUser
poirot search --email foo@bar.com --full-name "Jan Kowalski"
poirot search -n alice -e alice@x.com -o results.json

# Zarządzanie kluczem Claude (do generowania promptu copy-paste)
poirot config set-claude-key sk-ant-api03-…
poirot config set-model claude-opus-4-7
poirot config set-api-url http://192.168.1.42:57063
poirot config show
poirot config path
```

### ⬜ Łatwe przełączanie Claude API key — 3-tier resolution

TUI rozwiązuje klucz w tej kolejności (pierwszy niepusty wygrywa):

1. **CLI flag** — np. `--claude-key sk-ant-…` (jeśli kiedyś dodam)
2. **Zmienna środowiskowa** `CLAUDE_API_KEY` — najwygodniejsze do dziennej pracy:
   ```bash
   # bash/zsh:
   export CLAUDE_API_KEY="sk-ant-…"

   # PowerShell:
   $env:CLAUDE_API_KEY = "sk-ant-…"
   ```
3. **Plik konfiguracyjny** — per-user, edytowalny przez `poirot config set-claude-key`:
   - Windows: `%APPDATA%\Poirot\config.json`
   - Linux/macOS: `~/.config/poirot/config.json`

Sprawdź skąd jest aktualnie czytany:
```bash
poirot config show
```
Pokaże tabelkę z kolumną `source` — czy klucz pochodzi z env, config.json, czy domyślny.

**Backend API używa osobnego mechanizmu** (`appsettings.json` lub `Osint__ClaudeApiKey` env var) — TUI i API są niezależnymi konsumentami klucza.

---

## "Ready-to-paste" prompt dla Claude Code (Opus)

Po każdym wyszukiwaniu TUI rysuje na końcu **panel z gotowym promptem**:

```
╔══════════ Ready-to-paste prompt for Claude Code (Opus) ══════════╗
║ Skopiuj poniższy blok i wklej do `claude` w nowym terminalu       ║
║ (aktualny model do manualnej analizy: claude-opus-4-7)            ║
║                                                                    ║
║ # Pogłębiona analiza osobowości — Poirot OSINT                    ║
║ Działaj jako ekspert OSINT…                                        ║
║                                                                    ║
║ ## Search query                                                    ║
║ - Nickname: targetUser                                             ║
║ ## Kandydaci (3)                                                   ║
║ ### Candidate `targetUser` — probability 78/100                    ║
║ - Platforms (12): GitHub, Mastodon@fosstodon, HackerNews, …        ║
║ …                                                                   ║
║                                                                    ║
║ Zaczynaj.                                                          ║
╚════════════════════════════════════════════════════════════════════╝
```

**Use case:**
- Backend Sonnet daje szybką analizę automatycznie (PersonalityProfile widoczny w panelu wyżej)
- Jeśli chcesz głębiej / na Opusie / z dostępem do bazy wiedzy `OSINT/` — kopiujesz ten panel, otwierasz `claude` w nowym terminalu (z aktywnym Opusem) i wklejasz. Claude Code dostaje pełen kontekst + lokalny dostęp do plików → robi głębszą analizę.

---

## Konfiguracja appsettings.json — ekspansja list instancji

Plik `src/SherlockOsint.Api/appsettings.json` zawiera teraz:
- **91 Mastodon instancji** (general + tech + JP + DE + UK + FR + IT + ES + NL + PL + Czech + CA + AU + BR + KR + IN + niche-themed + Truth Social + Gab)
- **35 Lemmy instancji** (international + regional + NSFW + themed)
- **7 4chan archives** (desuarchive, archived.moe, 4plebs, archiveofsins, thebarchive, warosu, b4k)

Możesz edytować te listy bezpośrednio — ASP.NET Core ładuje konfigurację przy starcie.

**Ostrzeżenie:** Im więcej instancji w liście, tym wolniejszy lookup (wszystkie odpalają się równolegle, ale każda ma 10s timeout). Jeśli wyniki przychodzą za wolno — zredukuj listę do ~20 najważniejszych dla twojego use case.

---

## Inne ręczne kroki (jeśli się pojawią)

(pusto na razie)
