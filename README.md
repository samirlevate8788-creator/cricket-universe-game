# Cricket Universe — playable cricket prototype

An original cricket game prototype with a playable 3D browser match and a separate Unity foundation project. It uses fictional teams, procedural players, and original stadium branding; it does not copy Real Cricket/Dream Cricket teams, players, marks, animations, or assets.

The repository root also contains `index.html`, the browser-playable match, and its local 3D renderer modules. GitHub Pages publishes this static build through `.github/workflows/pages.yml`.

## Open and play

1. Install **Unity 6.0** with the Windows build support module in Unity Hub.
2. Open this `CricketUniverse` folder as a Unity project and allow Package Manager to resolve the packages.
3. In Unity, choose **Cricket Universe → Create Demo Scene**.
4. To activate URP, create an asset from **Assets → Create → Rendering → URP Asset (with Universal Renderer)**, select that asset in the Project window, then choose **Cricket Universe → Assign Selected URP Asset**.
5. Open `Assets/CricketUniverse/Scenes/CricketUniverseDemo.unity`, then press Play.

The scene setup command attempts to set Active Input Handling to **Both** and creates the demo match configuration. If Unity asks for a restart or the controls do not respond, set **Edit → Project Settings → Player → Active Input Handling → Both**, then restart the Editor. The demo runs with the default renderer too, while assigning the URP asset activates the requested render pipeline.

## Browser game

Open the live [Cricket Universe 3D match](https://samirlevate8788-creator.github.io/cricket-universe-game/). Choose two teams, match length, venue, weather, pitch and difficulty; call the toss; then bat through a short exhibition match.

- Real-time 3D broadcast view with original stadium stands, instanced crowd, floodlight towers, pitch wear, creases, boundary boards, stumps, fielders and low-poly player rigs.
- Four switchable cameras: broadcast, side, bowler and batter. The camera button or controller Y cycles views.
- 3D ball flight, bounce, swing, seam spin and a shadow that tracks the ball. Batting input drives a visible bat swing and animated shot arc.
- Six fictional teams, three original venue looks, sunny/cloudy/evening-dew conditions, four pitch choices, three difficulties, toss, two innings, a chase and live score/over/run-rate/result HUD.
- Delivery choices include good length, swing, yorker, short ball and spin. Drive, cut, pull, sweep and defend combine with timing, aim and grounded/lofted intent.
- Touch swipe, on-screen buttons, keyboard/mouse and gamepad controls. Local Three.js modules are included, so the game does not need an external CDN while playing.
- Tournament, career and world-cricket tiles are roadmap items. ODI/Test labels are short exhibitions, and the ball path/outcomes use simplified arcade logic rather than a full cricket simulation.

This browser build is a 3D vertical slice, not a full production-scale Real Cricket or Dream Cricket equivalent. It does not yet include a full motion-captured animation library, long-form official rules, online play, career, DRS, or full broadcast/audio content. The bundled `vendor/three.module.js` and `vendor/three.core.js` files are from Three.js r186 and are distributed under the included MIT license.

## Browser controls

| Input | Action |
| --- | --- |
| Enter / controller Start | Bowl next delivery / continue |
| Space / controller A | Play the shot |
| ArrowLeft / ArrowRight / controller D-pad | Aim left or right |
| Shift / controller B | Toggle ground or lofted intent |
| R / controller X / on-screen RUN | Call a quick single |
| 1-5 | Select drive, cut, pull, sweep or defend |
| Touch swipe on the pitch | Aim horizontally; swipe up to loft, down to keep it grounded and release to play |

Controller Y or the camera chip cycles the broadcast, side, bowler and batter views. A/D moves across the crease.

Overs and wickets are shortened for the prototype. Full-length official formats, DRS, multiplayer, career and tournament systems remain future work. Team and venue names are fictional; real player likenesses, league marks and stadium branding need appropriate rights.

The Unity folder remains a separate foundation project built with Unity primitives. It is not the renderer used by the GitHub Pages browser build.

## Controls

| Input | Action |
| --- | --- |
| Enter / controller Start | Bowl next delivery / continue |
| Space / controller South button / on-screen SHOT | Play the shot |
| Left / Right arrows / controller D-pad | Aim left or right |
| A / D | Move across the crease |
| Shift / on-screen LOFT | Toggle lofted intent |
| On-screen RUN | Attempt one extra run while the ball is live |

The Unity touch HUD uses large buttons; keyboard and controller input use the Unity Input System. Customize bindings and touch layout in a later controls/settings pass.

## Project shape

- `Assets/CricketUniverse/Scripts/Core` — match state and score model.
- `Assets/CricketUniverse/Scripts/Data` — ScriptableObject configuration for players, teams, and match rules.
- `Assets/CricketUniverse/Scripts/Gameplay` — runtime bootstrap, batting input, match controller, and procedural demo stadium.
- `Assets/CricketUniverse/Editor` — one-click scene creation.

## Next implementation stages

This prototype intentionally establishes a playable vertical slice before expanding the whole specification. Next: delivery/pitch physics and fielding; then AI, tournament/career, production art/audio, platform settings and optimization; online play comes after server-authoritative match simulation is designed.

The Unity Editor is not installed in the authoring environment, so this project has not been opened or compiled in Unity here.
