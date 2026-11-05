# NewWorld AI

**New World** is a mixed-reality study and rest app for Meta Quest by Kore Meta: calm, beautiful places to focus,
explore and unwind, each with AI companions you talk to out loud. This repository is the hackathon edition. Its AI
characters run on **Muse**, Meta's Model API.

## What to try

Three places are open. The other six are premium and stay locked in this edition: they show on the place cards
with a crown, but can't be entered.

| Place | Kind | AI to talk to (Muse) |
|---|---|---|
| **Orbit** | Explore: a library in a rotunda circling the Earth | **Orbit**, the holodeck guide. Ask about the four spacecraft or the planets; she shows a craft, opens its parts, points at one, or plays its mission while she answers. |
| **Seoul** | Focus: a hanok tea room in Bukchon | **Hori**, a tiger cub. Chat with her; she comes over, chuffs, slow-blinks, plays with her ball or runs around the courtyard. |
| **Seabreeze** | Unwind: a palm lagoon beach | **Calypso**, the beach's own voice. Say *"Calypso, hatch the turtles"*, *"make it sunset"*, *"start the day cycle"*, *"calm the waves"* or *"put out the fire"*, or just chat. She is voice-first: her water orb shows what she's doing by colour, motion and sound (a chime as she hears her name, aqua listening, violet thinking, amber acting with a ring), the phrases to say flow out of her with small icons, she says a request with several steps back with a moment to say *"wait"*, leads your eye to the nest, puts things back on *"change it back"*, and docks at the edge of view when quiet. Captions are optional. |

Orbit and Seoul also have a **Connect a computer** card: run the Windows companion app (`Tools/DesktopCompanion`, see
its README) on a PC on the same Wi-Fi, enter the code the headset shows, and every display you pick stands round your
seat as a curved screen at its real size. The screens go straight from the PC to the headset over the local network
(WebRTC), never through the relay or any server. **Keyboard** on the screens' control bar opens the Quest's system
keyboard and types into the window in focus on the PC (encrypted in the same WebRTC connection; the desktop app's
"Let the headset type" switch turns it off).

Speech-to-text and the characters' voices use Meta's Voice SDK (Wit.ai) on the headset. What you say goes to a small
relay server (`Tools/CatServer`), which asks Muse for the reply and the in-world actions (Muse tool calls), so the
Muse key never ships in the app.

## Install the build

Download the APK from this repository's **Releases** and install it with Meta Quest Developer Hub or
`adb install NewWorld-AI.apk`. The release build already points at the hosted relay.

## Run your own relay

The relay is plain Node.js 20+ with no dependencies.

- **Render**: New > Blueprint, pick this repository (`render.yaml`). Paste your Muse key (from
  [dev.meta.ai](https://dev.meta.ai)) into `MUSE_API_KEY`.
- **Anywhere else**:
  ```bash
  cd Tools/CatServer
  MUSE_API_KEY=... CAT_APP_TOKEN=<any secret> node server.mjs
  ```

`GET /health` reports whether it's up and has a key. Endpoints and actions are listed at the top of `server.mjs`.

## Build from source

Unity **6000.6.3f1** with Android build support.

1. Create `Assets/StudyEverywhere/Resources/VoiceRelay.json` containing `{ "serverUrl": "https://your-relay", "appToken": "<CAT_APP_TOKEN>" }`.
2. Make a free [wit.ai](https://wit.ai) app, then in Unity choose **Meta > Voice SDK > Get Started**, paste its Client
   Access Token and save the configuration as `Assets/VoiceSDK.asset`.
3. Build with **New World > Build Quest APK** (or File > Build Profiles, Android).

Some files aren't in this repository because their licences don't allow publishing them. They're in the release APK:

- Unity Asset Store packs: Synty POLYGON Nature, World Materials Free, Yughues Free Palm Trees, Mirza Beig Inferno VFX.
  The three open places don't use these; the locked premium places do. Import them from the Asset Store to see those
  places in the Editor.
- The Wit.ai configuration (it holds an app token). Make your own as in step 2.

## Notes

- This is a hackathon edition. AI conversations go to Meta's Model API, and while free credits are in use Meta may use
  them to improve its products. The relay keeps each conversation in memory only, apart from everyone else's, and
  never logs what was said.
- © Kore Meta. The source is shared for the hackathon's review; see `LICENSE`.
