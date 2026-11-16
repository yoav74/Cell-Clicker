# Cell Clicker

**Team:** Yoav Gershman
**Course:** Mobile Game Development, Shenkar

## The game in one sentence
In the game the player starts as a singular cell where he can create DNA by clicking on a Cell Hive, the meta progression will be that he will be able to increase the amount of DNA he gets by clicking,
Add Bigger units that can mine more DNA, give each unit upgrades and have boosters to increase the DNA further.

## How to open and build
1. Open the project in Unity 6000.6.7f1.
2. File → Build Profiles → Android → Switch Platform.
3. Build and run on an Android phone.
4. In the Editor, press Play from `Assets/Game/Scenes/Bootstrap.unity`.

## Architecture
![Cell Clicker architecture: scenes, services, init order and assemblies](docs/diagrams/architecture.png)

Editable source: [`docs/diagrams/architecture.svg`](docs/diagrams/architecture.svg).

- **Scenes:** Bootstrap → Home → Level. Bootstrap is first in the Scene List, runs once and never comes back.
- **Init order:** Bootstrap sets 60 FPS, then creates Settings → Save → Sound → Haptics (Sound and Haptics read Settings), then opens Home.
- **Services:** screens and systems reach them through `Services.X`, never PlayerPrefs or files directly. Dashed boxes are planned services: Cloud Save, Ads, Notifications, Analytics, Trusted Time.
- **Assemblies:** Game.Editor → Game → Services. Services never references Game.

## Repo map
| File | What it is |
| --- | --- |
| `pitch.md` | One-page pitch of the game |
| `DECISIONS.md` | Every real decision the team made, and why |
| `AGENTS.md` | Context for AI coding tools: architecture, rules, conventions |
| `docs/` | Media for the README and course tasks: `diagrams/` (architecture), `screenshots/`, `videos/` |
| `Cell Clicker/` | The Unity project; our files are in `Assets/Game/` |

## Task 2: on two screen shapes and on a phone
Device Simulator, two phones with different notches and aspect ratios. The safe area keeps the HUD clear of the notch.

<p>
  <img src="docs/screenshots/task2-simulator-1.png" alt="Level on a phone with a notch, 2.52K DNA" width="300">
  <img src="docs/screenshots/task2-simulator-2.png" alt="Level on a taller phone with a hole-punch camera, 4.88K DNA" width="300">
</p>

Phone video: [YouTube Short](https://www.youtube.com/shorts/boJ_uWDqo5M)