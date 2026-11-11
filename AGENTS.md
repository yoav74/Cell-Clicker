# AGENTS.md — context for AI coding tools

AI tools read this file before working on the repo. Keep it short, true and up to date.
If you use Claude Code, also add a `CLAUDE.md` containing one line: `@AGENTS.md`.

## The game
**Cell Clicker** is a hybrid-casual idle tap game for Android, played in portrait with one thumb. The player starts as a single cell and taps a DNA Hive to earn DNA. DNA is spent on upgrading DNA per tap, recruiting organisms that generate DNA on their own, and unlocking upgrades, abilities and boosters. Achievements (total DNA earned, most DNA held, DNA/sec records) grant generation multipliers. At DNA/sec milestones the hive's theme evolves: cell → microscopic creatures → bugs → animals → humans (maybe space and aliens). DNA keeps coming in at a slower rate while the app is closed. Every day the player gets 3 free Super-Boosters (+300% generation), and can earn more by watching ads. One booster scales with skill: the more taps in the 5 seconds after activating it, the bigger the boost. Sessions can be a quick minute or a long tapping session. The fun comes from fast-growing numbers, VFX that sell the feeling of making lots of DNA, and seeing all of the player's units on screen.

## Tech
- Engine: Unity 6 LTS, C#
- Target: Android (portrait)
- UI: uGUI + TextMeshPro (in `com.unity.ugui` 2.0; TMP Essential Resources in `Assets/TextMesh Pro/`), URP, new Input System
- [Add packages and services as you add them]

## Architecture
- Unity project lives in `Cell Clicker/`. Scripts in `Assets/Scripts/{Core,Hive,Organisms,UI,Editor}`, namespaces `CellClicker.*`.
- `DnaBank` (Core) owns the DNA total; everything earns through `Add()` and spends through `TrySpend()`, UI listens to `Changed` (fires every frame while DNA/sec flows, so listeners must be cheap).
- DNA/sec: anything that produces it implements `IDnaSource`; `DnaGenerator` sums the sources and adds the DNA each frame. UI reads the total from `DnaGenerator`.
- Organisms: `OrganismDefinition` ScriptableObjects (name, tint, cost curve, DNA/sec per Organism Level) in `Assets/Settings/Organisms`; `OrganismRoster` owns the player's `Organism`s, levels them up and is an `IDnaSource`. New Organism = new asset.
- `DnaHive` (Hive) is a uGUI Image; tapping it (`IPointerDownHandler`) adds `dnaPerTap` and raises `Tapped(HiveTap)`. It does nothing visual.
- Tap feedback = subclasses of `HiveTapFeedback` (subscribe to `Tapped`): `HiveSquish`, `HiveAura` (glow + rays, tap "energy"), `FloatingTextSpawner` ("+1" popups), `TapBurstFeedback` (ring + particles). New effect = new subclass; don't touch `DnaHive`.
- `DnaCounterUI` (rolling total, flashes on tap), `DnaRateUI` (DNA/sec), `HintPrompt`, `BackgroundMotes` (UI).
- Popups: `PopupPanel` is a bottom sheet (slide up, dim backdrop, closes on backdrop/X/Android back). The Organisms popup = `PopupPanel` + `OrganismShopUI` (one `OrganismRowUI` per Organism, cloned from a template row). `OrganismShopButton` (bottom-left, below the play area) opens it and shows a badge while something is affordable.
- Text: TextMeshPro only (`TextMeshProUGUI`; scripts reference `TMP_Text`). Font: Lilita One (`Assets/Art/Fonts/LilitaOne-Regular.ttf`, OFL licence) as a dynamic `LilitaOne SDF` asset with LiberationSans as fallback. Styles are TMP material presets in `Assets/Art/Fonts/` (DNA Glow, Popup, Soft Shadow) on the Mobile SDF shader. `Editor/TmpSetup.cs` creates the font asset and presets. Fade text with `canvasRenderer.SetAlpha`, not `color` (no mesh rebuild). Don't use uGUI `Outline`/`Shadow` on TMP.
- Placeholder art is drawn in code: `ProceduralSprites` (cached) + `ProceduralImage` (sets `overrideSprite`, also in edit mode, never saved). Replace with real sprites later.
- **Cell Clicker > Apply Visual Polish** (`Editor/HiveVisualPolish.cs`) adds the visual layer to an existing scene without deleting anything.
- Input: new Input System only (`InputSystemUIInputModule` on the EventSystem).
- Scene setup: menu **Cell Clicker > Build Hive Scene** (`Editor/HiveSceneBuilder.cs`, with `OrganismShopBuilder`, `OrganismDefaults` and the `UiFactory` helpers) builds and wires the scene (rerun replaces it).
- Layout: portrait 1080x1920 reference, scaled by width. `SafeArea` holds Header (top: helix icon + DNA total, Tap Power / DNA/sec pill), Play Area (hive fits inside via AspectRatioFitter) and a bottom strip (Organisms button bottom-left, Hint). Nothing tappable overlaps the Play Area.
- Mobile perf: hive, aura, header, motes and effects each have their own nested Canvas; popups and burst pieces are pooled; `GameBootstrap` sets 60 FPS; camera renders nothing (UI is overlay).
- Orientation: portrait only, set via **Cell Clicker > Apply Portrait Settings** (`Editor/MobileProjectSettings.cs`).

## Before writing any code
Review the planned change against this checklist first, then write it. In the reply, briefly say how it holds up (and what you changed because of it).

1. **Correct**
   - Does it actually work in Unity 6 with this project's setup (new Input System only, URP, uGUI)?
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
- Follow the existing folder structure and naming. Don't create new top-level folders.
- Don't invent APIs. If unsure about an SDK or Unity API, say so and point to the docs.
- Don't change Player Settings, package name, API levels or build settings unless asked.
- Keep changes small and explain what you changed and why.
- Never commit secrets (keystores, passwords, API keys).
- `DECISIONS.md` belongs to the team. Touch it only when the user explicitly asks in that request. Even then, only add new text (e.g. a new entry or a formatting fix they asked for). Never change or delete text the team wrote. Otherwise, suggest decisions worth logging in chat.
- Use the terms in `GLOSSARY.md` in code, UI text and docs.

## Conventions
- [Naming, code style, where scripts go — fill in as a team]
