# NewWorld AI relay

The AI characters' brain. The headset hears you with Meta's Voice SDK (Wit.ai) and sends the words here, with what the
character is doing. The relay asks **Muse**, Meta's Model API (`muse-spark-1.3`, low reasoning effort), for the reply
and the character's actions, and the headset speaks the reply with the Voice SDK's text-to-speech. The Muse key lives
only on this server.

## Run

Node.js 20 or later; no packages to install.

```bash
MUSE_API_KEY=... CAT_APP_TOKEN=<any secret> node server.mjs
```

`PORT` defaults to 8787 (hosts like Render set it themselves). `MODEL_API_KEY`, as Meta's docs call it, also works as
the key's name; `MUSE_MODEL` picks another Muse model. The headset needs the same `CAT_APP_TOKEN` as `appToken` in
`Assets/StudyEverywhere/Resources/VoiceRelay.json`, and release builds only talk to an HTTPS relay (or one on the local
network).

To host it on Render, use the `render.yaml` blueprint at the repository root.

## Characters

| Endpoint | Who | Actions (Muse tool calls) |
|---|---|---|
| `POST /orbit/talk` | Orbit, the holodeck guide in the Orbit library | show a craft, spread its parts, point at a part, power, play or stop a mission |
| `POST /pet/hori/talk` | Hori, the tiger cub in the Seoul café | come over, go somewhere, run around, chuff, slow blink, play ball, sleep, ... |
| `POST /pet/calypso/talk` | Calypso, Seabreeze's voice | hatch the turtles, set the time of day, start or pause the day cycle, calm or restore the waves, light or put out the fire |
| `POST /pet/bubble/talk`, `/cat/<name>/talk`, `/composer/talk` | the premium places' characters (locked in this edition) | |

Each has a matching `/reset`. `GET /health` returns whether the relay is up and has a key. The request and reply
formats are listed at the top of `server.mjs`.

## Privacy

Each person's conversation is kept apart (the app's random per-launch session id), in memory only, and dropped after
30 turns or 30 minutes of quiet. The log records which character answered, how long it took and the tokens, never
what was said.
