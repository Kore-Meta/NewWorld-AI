# New World Desktop

The Windows companion for **New World**'s *Connect a computer* panel. It shares every display of the PC with the
headset over the local network, following [`docs/monitor-connection/protocol.md`](../../docs/monitor-connection/protocol.md).
How it reads the parts the protocol leaves open is in [PROTOCOL-NOTES.md](PROTOCOL-NOTES.md).

Electron, plain JS/HTML/CSS, no framework.

## Privacy

- Nothing leaves the local network. The headset finds the PC by UDP broadcast, signalling is a direct TCP connection
  on the LAN, and the video is WebRTC (DTLS-SRTP) with `iceServers: []`, so there are only host candidates and no
  STUN or TURN.
- No telemetry, crash reporting, auto-update, remote fonts or remote content. The pages have a strict CSP, the
  spellchecker is off (it would download dictionaries), new windows and navigation are blocked, and only the hidden
  capture page may use screen capture.
- Nothing about the computer, not even its name, is sent until the 6-digit code from the headset checks out.
  Every display is chosen by the user, a monitor attached mid-session is never shared without asking, and a clear
  "Sharing N displays with …" indicator with a Stop button shows the whole time (also in the tray).
- The only thing written to disk is `settings.json` in the app data folder, which holds the two switches on the
  idle screen: "Let headsets find this computer" and "Smoother motion (60 fps)".

## Monitor sizes

Each display sent to the headset carries its physical size (`widthMm`, `heightMm`) so it can be drawn at its real
size. The app reads it from the monitor's EDID with a local, hidden PowerShell WMI query at startup and when
monitors change. Nothing leaves the computer except these numbers, and only after the code checks out. A monitor
with no EDID gets an estimate from its pixels at 110 ppi (`sizeSource: "estimate"`).

## Stream quality

Each display is captured at its native pixel size, capped at **1920 px on the long side** (aspect ratio kept), at
**30 fps** and about **8 Mbps**, with `maintain-resolution` so text stays sharp. On the Quest 3S, two 2560-wide
60 fps streams made New World processor-bound (72 → 53-58 fps), hence these caps. The **Smoother motion (60 fps)**
switch on the idle screen raises the cap to 60 fps and 12 Mbps for video or games; it costs headset battery and
frame rate, so it is off by default. It applies from the next share.

## Run

Needs Node 22.12 or newer (tested with Node 24.21, Electron 45.2.2).

```bash
npm install
```

```bash
npm start
```

The first `npm start` downloads the Electron binary (npm's install-script policy may skip it during `npm install`).
The first time it listens, Windows may ask whether `electron.exe` may use the network. Allow it on **Private**
networks. The installed app doesn't ask, because the installer adds the rules (see below).

Closing the window keeps the app running in the tray (the brass ring icon, with a sage dot while sharing). Use
**Quit** from the tray menu to exit.

## Test without a headset

Start the app, then in a second terminal:

```bash
npm run fake-headset -- 123456
```

It listens for the beacon on UDP 47800, connects to TCP 47801 and sends `hello` with that code. Type **123456** into
the app, press **Share**, and the tool prints the `approved` displays and the offer, then checks:

- each shared mid's video m-line lists H.264 first
- every mid in `mids` has an m-line
- there are host candidates only, with no mDNS `.local` names and at least one 192.168.x.x address
- the candidates are inside the SDP (non-trickle)
- the offer `mac` equals HMAC(code, "offer:" + fingerprint)

Options:

| Option | Effect |
|---|---|
| `--host 192.168.x.y` | Skip discovery and connect to that PC |
| `--exit-after-offer` | Send `stop` and exit once the offer has been checked |
| `--receive` | Also answer, with a real WebRTC receiver in an Electron window, like the headset. It prefers H.264, puts a `mac` on the first answer and none on later ones, shows the streams and prints the decoded size, fps and frame count for each mid (expect at most 1920 on the long side and 30 fps, or 60 with Smoother motion) |

While it is connected, type `stop-display 2` or `stop` and press Enter to send those messages.

`--receive` tests the whole path on one PC: pairing, the answer `mac` check, ICE over the LAN address, H.264 frames
and renegotiation (press **Share again** on a stopped display in the app to send `display-added`). The real test
on the Quest is done with `WebRtcScreenSource`.

## Build the installer

```bash
npm run dist
```

This writes `dist/NewWorldDesktop-Setup-<version>.exe`, an NSIS installer that installs per machine and asks for
admin rights. [`build/installer.nsh`](build/installer.nsh) adds three inbound **allow** rules with
`netsh advfirewall`, all tied to the app executable and to the **Private** profile only:

| Rule | Protocol / port |
|---|---|
| New World Desktop - signalling (TCP 47801) | TCP 47801 |
| New World Desktop - discovery (UDP 47800) | UDP 47800 |
| New World Desktop - screen video (WebRTC UDP) | UDP, any port (WebRTC picks ports at run time) |

Uninstalling removes them. If the Wi-Fi is set to **Public** in Windows, the rules don't apply and the headset can't
connect. Switch the network to Private in Settings → Network & internet.

To check the rules after installing:

```bash
netsh advfirewall firewall show rule name=all | findstr /C:"New World Desktop"
```

**Code signing is not configured.** The installer and app are unsigned, so SmartScreen warns on first run
("Windows protected your PC" → More info → Run anyway). Before release, add a certificate to the `win` section of
`package.json` (`certificateFile`/`certificatePassword`, or `azureSignOptions` for Azure Trusted Signing) and keep
the secrets out of the repo.

## Layout

| Path | What |
|---|---|
| `src/main.js` | Main process: windows, tray, session state, display add/remove handling, IPC |
| `src/protocol.js` | Ports, HMACs, fingerprint, JSON-line framing (shared with the fake headset) |
| `src/discovery.js` | UDP beacons to 255.255.255.255 and every interface's directed broadcast |
| `src/signalling.js` | TCP 47801 server and the per-headset session (code check, approve, offer/answer, ping) |
| `src/displays.js` | `screen.getAllDisplays()` matched to `desktopCapturer` sources by `display_id`, capture sizes |
| `src/monitors.js` | Physical monitor sizes from the EDID through WMI (a local PowerShell query), matched to displays |
| `src/engine/` | Hidden renderer: `getUserMedia` desktop capture and the `RTCPeerConnection` |
| `src/ui/` | The window: idle, code, approve (layout map + checklist), sharing, new-monitor prompt |
| `build/` | `installer.nsh` (firewall rules) and `icon.ico` |
| `scripts/make-icons.js` | Draws the icons (`npm run icons`; the outputs are committed) |
| `tools/fake-headset.js`, `tools/receiver/` | The test headset |

Palette: ink `#1d2424`, paper `#f6f1e7`, brass `#b08a4a`, sage `#6f8f7a`. Fonts: Segoe UI / system UI only.
