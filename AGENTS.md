# AGENTS.md — context for AI coding tools

AI tools read this file before working on the repo. Keep it short, true and up to date.
If you use Claude Code, also add a `CLAUDE.md` containing one line: `@AGENTS.md`.

## The game
**Cell Clicker** is a hybrid-casual idle tap game for Android, played in portrait with one thumb. The player starts as a single cell and taps a DNA Hive to earn DNA. DNA is spent on upgrading DNA per tap, recruiting organisms that generate DNA on their own, and unlocking upgrades, abilities and boosters. Achievements (total DNA earned, most DNA held, DNA/sec records) grant generation multipliers. At DNA/sec milestones the hive's theme evolves: cell → microscopic creatures → bugs → animals → humans (maybe space and aliens). DNA keeps coming in at a slower rate while the app is closed. Every day the player gets 3 free Super-Boosters (+300% generation), and can earn more by watching ads. One booster scales with skill: the more taps in the 5 seconds after activating it, the bigger the boost. Sessions can be a quick minute or a long tapping session. The fun comes from fast-growing numbers, VFX that sell the feeling of making lots of DNA, and seeing all of the player's units on screen.

## Tech
- Engine: Unity 6000.6.7f1, C#
- Target: Android (portrait)
- UI: uGUI + TextMeshPro (in `com.unity.ugui` 2.6; TMP Essential Resources in `Assets/TextMesh Pro/`), URP 17.6, Input System 1.20
- No third-party SDKs or online services yet. Planned services: Cloud Save (server), Ads (rewarded, for Super-Boosters), Notifications, Analytics, Trusted Time. Each will be a new service in `Services/<Name>/` (see the README diagram).

## Architecture

### Project layout
- Unity project lives in `Cell Clicker/`. Everything the team makes is in `Assets/Game/`: `Art/`, `Data/` (tunable ScriptableObjects), `Scenes/`, `Scripts/`. Script paths below are relative to `Assets/Game/Scripts/`.
- The rest of `Assets/` belongs to Unity and packages; leave it alone unless asked: `Settings/Rendering/` (URP assets, renderers, volume profiles), `Settings/PlayMode/` (Play Mode scenario that starts from Bootstrap), `TextMesh Pro/` (TMP resources), `InputSystem_Actions.inputactions`.
- Outside Unity, `docs/` at the repo root holds media for the README and course tasks: `diagrams/`, `screenshots/`, `videos/` (keep clips short; link long videos instead of committing them).

### Start-up, scenes and assemblies
- Scene List: **Bootstrap** → **Home** → **Level** (`Assets/Game/Scenes/`).
- **Bootstrap** scene: only `Bootstrap` (Core). It sets 60 FPS, creates the services in this order: **Settings → Save → Sound → Haptics** (Sound and Haptics read Settings), then loads Home. Nothing else creates services. Always press Play from Bootstrap.
- **Home**: title, DNA total, "While you were away" card, Sound/Haptics toggles (top-right), Play. **Level**: the hive. `ScreenFlow` (UI) has `Play()` / `Back()`; Level has a Home button top-left.
- Three assemblies, dependencies point one way: **Game.Editor → Game → Services**.
  - `Services` (`Assets/Game/Scripts/Services/Services.asmdef`, namespace `CellClicker.Platform`): references nothing. Never reference Game code from here.
  - `Game` (`Assets/Game/Scripts/Game.asmdef`): everything in `Scripts/` except `Services/` and `Editor/`. References Services, TextMeshPro, uGUI, Input System.
  - `Game.Editor` (`Assets/Game/Scripts/Editor/Game.Editor.asmdef`, Editor only): menus and scene builders. Never in the APK.
  - A script that needs a new package → add it to that assembly's references.
- Scripts by feature: `Core/{Dna,Save,OfflineEarnings}`, `Hive/{Feedback,TapPower}`, `Organisms`, `UI/{Hud,TapEffects,Shop,Screens,Settings,Placeholders}`, `Services/{Save,Settings,Sound,Haptics}`, `Editor/{SceneBuilders,Defaults,Tools}`. Namespaces `CellClicker.Core/Hive/Organisms/UI/Platform/EditorTools` (by top folder, not subfolder).

### Services (`Services` assembly)
- `Services` is a static class with one property per service, set only by `Bootstrap`. Screens and systems use `Services.X`, never PlayerPrefs, files or platform APIs directly. Calls that can run without Bootstrap use `?.` (e.g. `Services.Sound?.Play(...)`).
- **Save** – `ISaveService` / `JsonFileSave`: `save.json` in `Application.persistentDataPath`, read once at start-up and kept in memory for every scene. `Load(key)` / `Store(key, json)` / `Flush()` / `DeleteAll()` / `LastSavedUtc`. `SaveFileStore` writes a temp file and keeps a `.bak` backup.
- **Settings** – `ISettingsService` / `PlayerPrefsSettings`: `SoundOn`, `HapticsOn` (default on), `Changed` event. The only class that touches PlayerPrefs. Settings belong to the device, not the save.
- **Sound** – `ISoundService` / `AudioSourceSound`: `Play(clip, volume, pitch)` on a pool of 8 AudioSources that survives scene changes; silent while Sound is off.
- **Haptics** – `IHapticsService` / `AndroidHaptics`: `Pulse(HapticStrength.Light/Medium/Heavy)` via Android `VibrationEffect`; does nothing in the Editor or while Haptics is off.
- New service = interface + implementation in its own `Services/<Name>/` folder, a property on `Services`, one line in `Bootstrap` in the right order.

### Game systems
- `DnaBank` (Core) owns the DNA total; everything earns through `Add()` and spends through `TrySpend()`, UI listens to `Changed` (fires every frame while DNA/sec flows, so listeners must be cheap).
- DNA/sec: anything that produces it implements `IDnaSource`; `DnaGenerator` sums the sources and adds the DNA each frame. UI reads the total from `DnaGenerator`.
- Organisms: `OrganismDefinition` ScriptableObjects (name, tint, cost curve, DNA/sec per Organism Level) in `Assets/Game/Data/Organisms`; `OrganismRoster` owns the player's `Organism`s, levels them up and is an `IDnaSource`. New Organism = new asset.
- Saving: `SaveSystem` (Core, on Game Systems in Level) restores every `ISaveable` from `Services.Save` at start-up and stores + flushes them every 5 s, on pause/quit and in `OnDestroy` (scene change). Anything with state implements `ISaveable` (own `SaveKey` + JSON) and is added to the SaveSystem's `saveables`; `DnaBank`, `TapPower`, `OrganismRoster` and `ProgressSummary` (DNA + DNA/sec for the Home screen) do now. Never rename a `SaveKey` or an Organism's `saveId` after release. **Cell Clicker > Delete Save** resets progress in the editor (not in Play mode).
- Tap Power: `TapPower` (Hive, on Game Systems) owns the Tap Power level, buys levels with DNA and is `ISaveable`; the numbers live in the `TapPowerDefinition` asset `Assets/Game/Data/Tap Power.asset`.
- `DnaHive` (Hive) is a uGUI Image; tapping it (`IPointerDownHandler`) adds `TapPower.Value` and raises `Tapped(HiveTap)`. It does nothing visual.
- Tap feedback = subclasses of `HiveTapFeedback` (subscribe to `Tapped`): `HiveSquish`, `HiveAura` (glow + rays, tap "energy"), `FloatingTextSpawner` ("+1" popups), `TapBurstFeedback` (ring + particles), `HiveTapSound` (blip, random pitch), `HiveTapHaptics` (Light pulse). New effect = new subclass; don't touch `DnaHive`.
- Offline Earnings: `OfflineEarningsSettings` asset (`Assets/Game/Data/Offline Earnings.asset`: 25% of DNA/sec, max 2 h, min 60 s). `OfflineEarningsCollector` (Level) pays it on load and on resume; `HomeScreenUI` previews it via `ProgressSummaryReader(Services.Save)` (read-only, never writes the save).

### UI
- `DnaCounterUI` (rolling total, flashes on tap), `DnaRateUI` (DNA/sec), `HintPrompt`, `BackgroundMotes` (UI).
- Popups: `PopupPanel` is a bottom sheet (slide up, dim backdrop, closes on backdrop/X/Android back). The Organisms popup = `PopupPanel` + a `TapPowerRowUI` at the top + `OrganismShopUI` (one `OrganismRowUI` per Organism, cloned from a template row). Shop rows subclass `ShopRowUI` (shared look, buy button, punch, buy sound + Medium haptic). `OrganismShopButton` (bottom-left, below the play area) opens it and shows a badge while something is affordable.
- Text: TextMeshPro only (`TextMeshProUGUI`; scripts reference `TMP_Text`). Font: Lilita One (`Assets/Game/Art/Fonts/LilitaOne-Regular.ttf`, OFL licence) as a dynamic `LilitaOne SDF` asset with LiberationSans as fallback. Styles are TMP material presets in `Assets/Game/Art/Fonts/` (DNA Glow, Popup, Soft Shadow) on the Mobile SDF shader. `Editor/Tools/TmpSetup.cs` creates the font asset and presets. Fade text with `canvasRenderer.SetAlpha`, not `color` (no mesh rebuild). Don't use uGUI `Outline`/`Shadow` on TMP.
- Settings toggles: `SettingToggleUI` (button + ON/OFF label + colour, click sound + Light haptic) with `SoundToggleUI` and `HapticsToggleUI` subclasses. New setting toggle = new subclass.
- Placeholder art and sound are made in code: `ProceduralSprites` (cached) + `ProceduralImage` (sets `overrideSprite`, also in edit mode, never saved); `ProceduralSounds` (Tap, Buy, Click blips, cached). Every sound user has an `AudioClip` field that replaces the placeholder; resolve the clip once (Awake/setup), not per play.
- Input: new Input System only (`InputSystemUIInputModule` on the EventSystem).

### Editor tools (menu **Cell Clicker**)
- **Build Hive Scene** (`Editor/SceneBuilders/HiveSceneBuilder.cs`, with `OrganismShopBuilder`, the `Editor/Defaults` assets and the `UiFactory` helpers) builds and wires the open scene (rerun replaces it); use it in Level.
- **Build Home Scene** (`HomeSceneBuilder.cs`) rebuilds `Home.unity` and sets the Scene List to Bootstrap, Home, Level.
- **Apply Visual Polish** and **Add Sound and Haptics** add to an existing Level without deleting anything (safe to rerun).
- **Delete Save**, **Open Save Folder**, **Apply Portrait Settings** (`Editor/Tools/`).

### Layout and mobile
- Layout: portrait 1080x1920 reference, scaled by width (Match 0, not the course's 0.5, so tall phones keep the full 1080 width). Buttons at least 140x140 (`UiFactory.MinTapSize`). `SafeArea` holds Header (top: helix icon + DNA total, Tap Power / DNA/sec pill), Play Area (hive fits inside via AspectRatioFitter) and a bottom strip (Organisms button bottom-left, Hint). Nothing tappable overlaps the Play Area.
- Mobile perf: hive, aura, header, motes and effects each have their own nested Canvas; popups, burst pieces and sound voices are pooled; `Bootstrap` sets 60 FPS; camera renders nothing (UI is overlay).
- Orientation: portrait only, set via **Cell Clicker > Apply Portrait Settings** (`Editor/Tools/MobileProjectSettings.cs`).

## Before writing any code
Review the planned change against this checklist first, then write it. In the reply, briefly say how it holds up (and what you changed because of it).

1. **Correct**
   - Does it actually work in Unity 6000.6 with this project's setup (new Input System only, URP, uGUI)?
   - Edge cases covered: null references, 0 or huge numbers (DNA is `double`), multi-touch, app pause/resume.
   - Compiles with no new errors or warnings (apart from CS0649 on `[SerializeField]` fields).
2. **Readable**
   - Clear names, small methods, one class per file, and the file is named after the class.
   - Short comments that explain *why*, not *what*. No dead code or leftover debug logs.
   - Values a designer would tune are `[SerializeField]` with sensible defaults, not magic numbers.
3. **SOLID**
   - **S**ingle responsibility: one job per class (e.g. `DnaBank` stores DNA, `DnaHive` handles taps, UI only displays).
   - **O**pen/closed: add features by adding classes or data (ScriptableObjects for upgrades, units and boosters), not by growing `if`/`switch` chains.
   - **L**iskov: subclasses and interface implementations must be drop-in replacements.
   - **I**nterface segregation: small, focused interfaces (e.g. `IDnaSource`), not one big manager.
   - **D**ependency inversion: depend on events and interfaces. Systems never reach into UI; UI listens to events. Avoid singletons and `Find` at runtime.
4. **Fun**
   - Does the change make the game feel better within the first seconds? Every player action gets instant feedback (motion, numbers, popups, sound or haptics).
   - Keep the progression curve rewarding: the next goal should always be visible and reachable soon.
   - If a cheap tweak would add "juice" (squish, bounce, particles, number punch), suggest it or add it.
5. **Mobile**
   - Portrait, one thumb: tap targets at least about 120 px at the 1080x1920 reference, with key buttons in the lower half of the screen.
   - No per-frame allocations, `Find` or `GetComponent` in `Update`. Pool anything spawned often.
   - Keep things that change often on their own nested Canvas.

## Rules for AI
- Follow the existing folder structure and naming. New game files go under `Assets/Game/`; don't create new folders at the root of `Assets/`.
- Don't invent APIs. If unsure about an SDK or Unity API, say so and point to the docs.
- Don't change Player Settings, package name, API levels or build settings unless asked.
- Keep changes small and explain what you changed and why.
- Never commit secrets (keystores, passwords, API keys).
- `DECISIONS.md` belongs to the team. Touch it only when the user explicitly asks in that request. Even then, only add new text (e.g. a new entry or a formatting fix they asked for). Never change or delete text the team wrote. Otherwise, suggest decisions worth logging in chat.
- Use the terms in `GLOSSARY.md` in code, UI text and docs.

## Conventions
- **Where scripts go:** by feature under the top folders above (e.g. a new booster's logic in `Core/Boosters/`, its button in `UI/Boosters/`). Platform/outside-world code (storage, audio, vibration, network) goes in `Services/<Name>/`.
- **Naming:** PascalCase types and methods, camelCase private fields (no `_` or `m_`), `I` prefix for interfaces. UI components end in `UI` (`DnaRateUI`), definitions in `Definition` (`OrganismDefinition`), tap effects in what they do (`HiveSquish`, `HiveTapSound`).
- **Fields:** `[SerializeField] private` with a `[Tooltip]` for anything a designer tunes; read-only state as `{ get; private set; }`. Public fields only in plain `[Serializable]` save-data classes (JsonUtility needs them).
- **Code style:** one class per file named after the class, block `namespace { }`, Allman braces, 4 spaces, `var` only when the type is obvious. XML `<summary>` on every class.
- **Data:** tunable numbers live in ScriptableObjects in `Assets/Game/Data/`, each with a `Defaults` creator in `Editor/Defaults/`. Editor tools get folders from `ProjectPaths` (`Editor/Tools/`), never hard-coded strings.
- **Git:** one feature per branch (`feature/…`, `docs/…`, `chore/…`), merged through a pull request whose description ends with *what the AI did / what I checked*. Commit `.meta` files with their assets; move scripts together with their `.meta`.
