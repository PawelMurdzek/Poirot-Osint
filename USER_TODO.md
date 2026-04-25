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

Bilibili nie wymaga klucza dla podstawowego user search. Provider działa od razu po dodaniu, ale **anti-bot Bilibili reaguje na masowe zapytania** — przy intensywnym użyciu może blokować IP. Jeśli zaczniesz dostawać błędy, dodaj realne nagłówki przeglądarki lub rate-limit po stronie OsintClient.

---

## TUI client (planowane, jeszcze nie zaimplementowane)

⬜ **Wymagasz drugiego frontendu w terminalu** — oprócz aplikacji Android. Ma działać na tej samej szynie SignalR co mobilna apka.

**Plan implementacji** (kiedy zaczniemy):
- Nowy projekt: `src/SherlockOsint.Tui` — .NET 10 console app
- Biblioteka: **Spectre.Console** (najpopularniejsza w ekosystemie .NET, bogate widgety: drzewa, tabele, progress bars, live updates)
- Współdzieli `SherlockOsint.Shared` z mobile/api
- Łączy się jak mobile via `Microsoft.AspNetCore.SignalR.Client` z `OsintHub`
- Layout: live drzewo wyników (jak mobile `ResultsPage`), panel boczny z `DigitalProfile` + listą `TargetCandidate`, na dole input dla query
- Po implementacji `PersonalityProfilerService` — TUI dostaje też `ReceivePersonalityProfile` i renderuje jako rozwijaną sekcję per kandydat
- Komenda uruchomienia: `dotnet run --project src/SherlockOsint.Tui -- --api http://localhost:57063 --nick targetUser`

**Nic od Ciebie nie potrzebne na razie** — to do implementacji przez agenta. Pozycja tutaj żeby wymaganie było zapisane i nie zginęło.

---

## Konfiguracja appsettings.json

Plik `src/SherlockOsint.Api/appsettings.json` zawiera teraz **listy instancji** dla Mastodon (91), Lemmy (35) i 4chan archives (7) — pokrywają większość krajów europejskich, JP/KR, Brazyl, niche-themed instancje, oraz Mastodon-fork extremism platforms (Truth Social, Gab).

Możesz edytować te listy bezpośrednio — nie musisz nic kompilować, ASP.NET Core ładuje konfigurację przy starcie.

**Ostrzeżenie:** Im więcej instancji w liście, tym wolniejszy lookup (wszystkie odpalają się równolegle, ale każda ma 10s timeout). Jeśli wyniki przychodzą za wolno — zredukuj listę do ~20 najważniejszych dla twojego use case.

---

## Inne ręczne kroki (jeśli się pojawią)

(pusto na razie)
