# USER_TODO — co musisz zrobić ręcznie

Plik utrzymywany przez agenta `poirot-builder`. Każda nowa pozycja pojawia się dokładnie w momencie, w którym agent na coś trafi (np. provider wymaga klucza API, manualnej weryfikacji, dodania zmiennej środowiskowej).

**Konwencja statusów:**
- ⬜ — do zrobienia przez Ciebie
- ✅ — zrobione (skreślaj kiedy załatwisz)
- ⏭️ — pomijalne (provider zadziała bez tego, ale zyskasz dane jak zrobisz)

---

## Tier A — providery aktualnie w trakcie implementacji

Wszystkie Tier A providery są **bez kluczy** — działają out-of-the-box po zbudowaniu projektu. **Nic od Ciebie nie potrzebne dla Tier A.**

> Status implementacji widzisz w odpowiedzi agenta (TaskList). Ten plik dotyczy tylko rzeczy po Twojej stronie.

---

## Tier B (na później) — będzie wymagało kluczy

Dopiszę tu konkretne instrukcje dopiero w momencie, gdy zacznę implementować Tier B. Na razie placeholder żebyś wiedział co Cię czeka:

| Provider | Klucz/dane | Gdzie zdobyć | Koszt |
|----------|-----------|---------------|-------|
| Twitch (Helix) | Client ID + Client Secret | Zarejestruj aplikację na https://dev.twitch.tv/console/apps | Darmowe |
| VK | Access Token | Utworzyć aplikację na https://vk.com/dev — wymaga numeru RU/CIS | Darmowe ale dostęp problematyczny spoza RU |
| Telegram | Bot Token | @BotFather w Telegramie | Darmowe |
| Bilibili | Brak klucza dla podstawowych endpointów | — | Darmowe |

Konkretne kroki dopisze agent kiedy będzie wpinał te providery.

---

## Konfiguracja appsettings.json

Sekcja `Osint` w `src/SherlockOsint.Api/appsettings.json` zostanie rozszerzona o nowe klucze tylko wtedy, gdy będę implementować providera, który ich wymaga. Aktualnie dla Tier A — **żadnych zmian w appsettings nie robisz**.

---

## Inne ręczne kroki (jeśli się pojawią)

(pusto na razie)
