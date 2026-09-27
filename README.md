# Cricket Universe — playable foundation

This is the first playable Unity slice of the larger game specification. It is a clean, fictional prototype built from Unity primitives so no team, player likeness, badge, or stadium branding is copied.

The repository root also contains `index.html`, a standalone browser-playable version of the quick-match loop. It has no external runtime dependencies and is published with GitHub Pages by `.github/workflows/pages.yml`.

## Open and play

1. Install **Unity 6.0** with the Windows build support module in Unity Hub.
2. Open this `CricketUniverse` folder as a Unity project and allow Package Manager to resolve the packages.
3. In Unity, choose **Cricket Universe → Create Demo Scene**.
4. To activate URP, create an asset from **Assets → Create → Rendering → URP Asset (with Universal Renderer)**, select that asset in the Project window, then choose **Cricket Universe → Assign Selected URP Asset**.
5. Open `Assets/CricketUniverse/Scenes/CricketUniverseDemo.unity`, then press Play.

The scene setup command attempts to set Active Input Handling to **Both** and creates the demo match configuration. If Unity asks for a restart or the controls do not respond, set **Edit → Project Settings → Player → Active Input Handling → Both**, then restart the Editor. The demo runs with the default renderer too, while assigning the URP asset activates the requested render pipeline.

## Playable loop

- Two innings, two overs per innings, fictional teams, score/overs/wickets, target and result.
- Bowl a delivery, read its flight, choose ground/lofted intent and aim, then time the shot.
- Timing grades are Perfect, Good, Poor, or Miss. Contact quality affects outcomes; runs, boundaries, wickets, and optional quick runs are recorded.
- The stadium, pitch, players, ball, score HUD, and touch buttons are generated at runtime. No external art assets are required.

## Controls

| Input | Action |
| --- | --- |
| Enter / controller Start | Bowl next delivery / continue |
| Space / controller South button / on-screen SHOT | Play the shot |
| Left / Right arrows / controller D-pad | Aim left or right |
| A / D / left stick | Move across the crease |
| Shift / on-screen LOFT | Toggle lofted intent |
| On-screen RUN | Attempt one extra run while the ball is live |

The touch HUD uses large buttons; keyboard and controller input use the Unity Input System. Customize bindings and touch layout in a later controls/settings pass.

## Project shape

- `Assets/CricketUniverse/Scripts/Core` — match state and score model.
- `Assets/CricketUniverse/Scripts/Data` — ScriptableObject configuration for players, teams, and match rules.
- `Assets/CricketUniverse/Scripts/Gameplay` — runtime bootstrap, batting input, match controller, and procedural demo stadium.
- `Assets/CricketUniverse/Editor` — one-click scene creation.

## Next implementation stages

This prototype intentionally establishes a playable vertical slice before expanding the whole specification. Next: delivery/pitch physics and fielding; then AI, tournament/career, production art/audio, platform settings and optimization; online play comes after server-authoritative match simulation is designed.

The Unity Editor is not installed in the authoring environment, so this project has not been opened or compiled in Unity here.
