# Decision log

## 2026-11-25 — Upgrade prices grow x3 per level
Options: x1.5 per level (the course example) / x2.5 / x3 (x4 for Cheaper Organisms)
Chose: x3 for Richer DNA (base 3,000) and Stronger Taps (base 1,000), x4 for Cheaper Organisms (base 5,000), 10 levels each
Why: every new Organism multiplies income about x4, so at x1.6 all 30 Upgrade levels were maxed too fast and didn't drive the player to push towards, at the rate of x3, the upgrades take meaningful time to reach.
AI: simulated 10 to 30 sessions of play with the new Organism curve and found that x1.6 maxed out too early. It compared x1.6, x2.5, x3, x3.5 and x4 and recommended x3. I picked x3.
Who: Yoav Gershman

## 2026-11-16 — Settings in PlayerPrefs, progress in the save file
Options: store Sound and Haptics inside save.json through `ISaveService` / store them in PlayerPrefs behind their own `ISettingsService`
Chose: PlayerPrefs behind `ISettingsService` (`PlayerPrefsSettings`, the only class that touches PlayerPrefs); progress stays in save.json
Why: the player keeps their preferences even if they delete their save.
AI: offered both options and recommended PlayerPrefs, because settings belong to the device and a future cloud restore wouldn't overwrite them. I went with it.
Who: Yoav Gershman

## 2026-11-16 — Get each sound clip once, not on every play
Options: call `ProceduralSounds.Get` every time a sound plays / get the clip once when the component starts (`Awake`)
Chose: get the clip once in `Awake` and keep it in a field (`ShopRowUI` does it in `ListenToBuyButton`, because its subclasses have their own `Awake`)
Why: looking up the sound on every tap is work the game doesn't need to repeat; getting it once is simpler and cleaner.
AI: wrote the first version that called `Get` on every play. When I questioned it, it explained the lookup itself is cheap but the first tap was generating the clip, which could cause a small hitch, and made the change. It also explained why `AudioSourceSound` must keep setting clip, volume and pitch on every play (the voices are shared), so I left that as it is.
Who: Yoav Gershman

## 2026-11-16 — Save goes through an ISaveService interface
Options: SaveSystem writes save.json directly / screens use PlayerPrefs / everything goes through `ISaveService` (in the Services assembly)
Chose: `ISaveService`, implemented by `JsonFileSave` (save.json kept in memory, temp file + .bak backup), created by Bootstrap; screens only use `Services.Save`
Why: a modular system where I can swap the pieces I work with without breaking and changing every piece of code that uses a service. Cloud saving comes later, and I want adding it to be seamless.
AI: suggested routing the existing JSON save through the interface and explained why JSON fits progress better than PlayerPrefs (DNA is a double, structured Organism data, one safe write, easy cloud upload). I kept JSON for progress and put the settings in PlayerPrefs behind their own service.
Who: Yoav Gershman

## 2026-11-11 — Home screen before the Level
Options: open straight into the Hive / Home screen first, then the Level
Chose: Home screen first (DNA total and the "While you were away" card), Play goes to the Level 
Why: the player sees their DNA total and Offline Earnings before playing, and has a calm place to come back to while rewarding returning users.
AI: suggested to separate into a Home screen and the play scene
Who: Yoav Gershman

## 2026-11-11 — Package name and minimum Android version
Options: Xision/YoavG/Gershboy for company name and CellClicker already decided as game name.
Chose: company "Xision", package com.Xision.CellClicker, minimum Android SDK 29 (Android 10)
Why: SDK 29 is the minimum shown in class; Xision is my gamer tag, so it became the company name.
AI: not used, Xision is my Gamer tag so i made that the company name and Cell Clicker was the name chosen for the game
Who: Yoav Gershman

## 2026-11-05 — Saves: local first, optional Account with Cloud Save
Options: local save only / local save + optional Account with Cloud Save / Account required
Chose: local save (JSON file, autosave every few seconds and when the app is paused or closed), plus an optional Account that backs progress up to the cloud. Linking an Account unlocks an Achievement.
Why: the game must work offline and with no sign-up; the Achievement rewards players who make an Account, so their progress is safe.
AI: suggested local JSON only. I added the optional Account + Cloud Save and the Achievement to encourage it.
Who: Yoav Gershman

## 2026-11-05 — Offline Earnings: 25% of DNA/sec, up to 2 hours
Options: full rate / reduced rate with a cap
Chose: 25% of DNA/sec for up to 2 hours, shown in a "While you were away" popup. Numbers may change after playtesting.
Why: gives a reason to come back without making active play pointless.
AI: suggested the numbers; accepted for now until playtesting.
Who: Yoav Gershman

## 2026-11-05 — Rewarding in both short and long sessions
Options: design for short sessions only / for long sessions only / for both
Chose: both. Every session, short or long, should reach something (an Upgrade, an Organism, a Booster, Evolution Progress).
Why: the game should feel addictive whether someone plays for one minute or thirty.
AI: suggested 1–3 minute sessions (the course target) as the main design goal. I want long sessions to feel rewarding too.
Who: Yoav Gershman

## 2026-11-05 — No reset when evolving (for now)
Options: keep progress / prestige-style reset with a permanent multiplier
Chose: no reset. A prestige system may be added later if development goes smoothly.
Why: simpler to build and balance; prestige is a stretch goal.
AI: debated between a prestige system to increase longevity and staying within scope and not going too far forward. I decided to focus on the main loop and, if development goes well, consider it.
Who: Yoav Gershman

## 2026-11-04 — Game idea: Cell Clicker
Options: Idle Tap game, match 3, jump tilt game
Chose: an idle tap game where you start as a single cell and tap a DNA Hive for DNA; the meta progression (stronger taps, organisms, upgrades, boosters) is written in the pitch file.
Why: I like Idle games and their addicting nature.
AI: not used
Who: Yoav Gershman