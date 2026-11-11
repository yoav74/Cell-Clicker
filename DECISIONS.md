# Decision log

Add an entry every time the team makes a real choice: a tool, a design, a trade-off.
About 1–2 entries a week. Newest at the top. Write the reasoning yourselves; AI may fix the wording.

## Template
```
## YYYY-MM-DD — [The decision, in a few words]
Options: [A] / [B] / [C]
Chose: [what] — [why]
AI: [what the AI suggested, and what you did with it — or "not used"]
Who: [name(s)]
```

## Example
```
## 2026-11-20 — Save progress locally first
Options: PlayerPrefs / JSON file / Cloud Save
Chose: JSON file now, Cloud Save later
Why: works offline, easy to move later
AI: suggested a SQLite database — rejected, too heavy for one save file
Who: Yuval
```

---

<!-- Your entries start here -->

## 2026-11-11 — Home screen before the Level
Options: open straight into the Hive / Home screen first, then the Level
Chose: Home screen first (DNA total and the "While you were away" card), Play goes to the Level 
AI: suggested to separate into a Home screen and the play scene
Who: Yoav Gershman

## 2026-11-11 — Package name and minimum Android version
Options: set my own
Chose: company "Xision", package com.Xision.CellClicker, minimum Android SDK 29 (Android 10) in the slides shown in class it was the minimum
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