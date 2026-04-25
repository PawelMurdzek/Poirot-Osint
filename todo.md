# TODO — jakość kandydatów (false positives z permutatora)

Zauważone podczas searcha tylko z `FullName=Paweł Murdzek` (bez nickname). Wynik: 8 kandydatów, w tym `pm`, `pawel`, `murdzek` — to różne osoby, nie target.

## Objawy

- Wszyscy kandydaci mają `Uncertainty: AI analysis unavailable — scores are estimated.` → Claude w ogóle nie scoruje, leci `ApplyFallbackScoring`.
- Kandydaci `pm` (2 platformy), `pawel` (4), `murdzek` (2) trafiają do TOP — to popularne handle, nie ten Paweł Murdzek.
- Kandydaci `pm` / `pawel` mają atrybuty „Developer, Musician, Gamer” jednocześnie — znak że łączymy 3 różne osoby pod jednym handlem.

## Root cause

1. **`Osint:ClaudeApiKey` puste** w `src/SherlockOsint.Api/appsettings.json:14`. `CandidateAggregator.BuildCandidatesAsync` (linia 162) dostaje pustą listę z Claude i leci `ApplyFallbackScoring` (linia 209). Cap fallbacka = 75, stąd 30/60 dla wszystkich.

2. **Permutator generuje za szerokie seedy** w `RealSearchService.SearchAsync:143-147`:
   - `NicknamePermutator.FullNameToHandleCandidates("Paweł Murdzek")` (`NicknamePermutator.cs:133`) wrzuca m.in. `pawel` (sam first), `murdzek` (sam last) → linie 139, 142.
   - `AddInitialForms` (linia 240) dorzuca `pm` (sam initials).
   - Każdy z tych seedów leci jako pełny fan-out przez `UsernameSearch.SearchAsync` (50 platform) + Bluesky/Lemmy/Mastodon/Wykop/Twitch/VK/Telegram.

3. **`UsernameSearch` waliduje tylko „czy strona istnieje”**, nie „czy to ta osoba”. `github.com/pm`, `x.com/pawel`, `reddit.com/user/pawel` istnieją realnie → `ValidateGitHub`/`ValidateTwitter`/`ValidateReddit` zwracają true → trafiają do streamu.

4. **`CandidateAggregator` nie odróżnia** username z user-input od username wygenerowanego z permutacji. Każda kupka 2-4 platform per handle staje się równoprawnym kandydatem (`BuildCandidateFromGroup`, linia 368).

5. **`ProfileVerifier` istnieje i jest zarejestrowany** w `Program.cs`, ale `RealSearchService` go nie woła (CLAUDE.md to potwierdza). To on miał weryfikować że profil dotyczy tej samej osoby (display name / bio match).

## Plan naprawy

- [ ] Set `Osint__ClaudeApiKey` w env / user-secrets (najpierw — bez tego i tak nie zobaczymy efektu zmian w aggregatorze).
- [ ] W `RealSearchService.SearchAsync:143-147` odfiltrować z seedów stringi długości ≤ 3 i takie które są dokładnie samym `first` lub samym `last` z `SplitName`. Zostawić je tylko jako *wariacje* w `GeneratePermutations` (tam są w kontekście znanego nicku).
- [ ] W `CandidateAggregator.BuildCandidateFromGroup` dodać flagę `IsFromUserInput` (true gdy `primaryUsername` ∈ {`request.Nickname` zNormalizowany, local-part `request.Email`}). Permutation-only kandydatów wymagać drugiego sygnału (display-name match / shared email / shared phone) zanim trafią na listę — albo wsadzić do osobnego bucketu „weak matches” który nie idzie do Claude scoringu.
- [ ] Rozważyć podpięcie `ProfileVerifier` w stage 2 — jako gate przed dodaniem do `discoveredHandles` z permutacji.
- [ ] (Bonus) `InferAttributes` (linia 586) nie powinien dodawać Developer+Musician+Gamer do kandydata, którego primaryUsername jest 2-3 znakowy generic — to silny znak że łączymy różne osoby.

## Kontekst

Plik: `src/SherlockOsint.Api/Services/RealSearchService.cs`, `Services/CandidateAggregator.cs`, `Services/NicknamePermutator.cs`, `Services/OsintProviders/UsernameSearch.cs`, `Services/OsintProviders/ProfileVerifier.cs`.
