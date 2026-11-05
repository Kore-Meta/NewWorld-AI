# New World monitor connection — protocol v1

The headset (New World, `WebRtcScreenSource`) receives every display of one Windows PC from the **New World Desktop**
companion app (`Tools/DesktopCompanion`, Electron). Everything stays on the local network: no cloud signalling, no
STUN/TURN, no relay. The video is WebRTC (DTLS-SRTP encrypted), H.264, one stream per display.

The spike (`Tools/ScreenSpike`) proved the receiving side on Quest 3S: H.264 on the hardware decoder
(`c2.qti.avc.decoder`), three 1080p streams with the app at 71-72 fps, the picture arrives upright.

## Ports

| Port | Transport | Who listens | Purpose |
|---|---|---|---|
| 47800 | UDP broadcast | headset | PC beacons (discovery) |
| 47801 | TCP | PC | signalling (newline-delimited JSON) |

The desktop installer adds Windows Firewall inbound rules for both the app's TCP 47801 and its WebRTC UDP (program rule
for the app executable, Private profile only).

## Discovery

The desktop app, while it is running and sharing is allowed, sends a beacon once a second to `255.255.255.255:47800`
and to the directed broadcast address of every IPv4 interface that is up (e.g. `192.168.68.255`):

```json
{"app":"newworld-desktop","v":1,"name":"MIJI-PC","port":47801}
```

`name` is the computer name. The headset takes the sender's IP from the UDP packet, not from the beacon.

## Signalling

One TCP connection per pairing attempt, opened by the **headset** to `PC:47801`. Each message is one line of UTF-8 JSON
terminated by `\n`. Unknown message types are ignored. Either side closing the socket ends the session.

### Pairing

1. The user presses **Connect a computer** in the headset. The headset makes a random 6-digit code (shown to the user,
   valid 120 s) and a random 16-byte nonce, then connects to every PC it hears a beacon from and sends:

   ```json
   {"type":"hello","v":1,"device":"Meta Quest 3S","nonce":"<hex 16 bytes>","proof":"<hex HMAC-SHA256(code, 'hello:'+nonce)>"}
   ```

2. The desktop app shows **"A headset wants to connect. Enter the code shown in the headset."** The user types it. The
   app checks `proof`. Wrong code: the app says so and lets the user retry (max 5 tries, then it sends `declined`).
   It never sends anything identifying until the code checks out.
3. With the right code, the app asks the user to **approve** and pick the displays to share (all ticked by default).
   Declined:

   ```json
   {"type":"declined","reason":"Sharing was declined on the computer."}
   ```

   Approved:

   ```json
   {"type":"approved","name":"MIJI-PC","displays":[
     {"id":1,"name":"Display 1","primary":true,"x":0,"y":0,"width":2560,"height":1440,
      "widthMm":597,"heightMm":336,"sizeSource":"edid"},
     {"id":2,"name":"Display 2","primary":false,"x":-1920,"y":0,"width":1920,"height":1080,
      "widthMm":443,"heightMm":249,"sizeSource":"estimate"}]}
   ```

   `x/y/width/height` are the display's bounds on the Windows desktop in logical (DPI-scaled) units, y down. `id` is a
   small positive integer, stable for the session.

   `widthMm`/`heightMm` (integers, optional) are the monitor's physical size, so the headset can draw it at its real
   size. `sizeSource` is `"edid"` when they come from the monitor's EDID, or `"estimate"` when there was no EDID or
   no match (native pixels at 110 ppi). The width is kept within 150-1600 mm. The same fields are on the display in
   `display-added`. When they are missing, the headset uses its own estimate.

### Streams

4. The PC creates one `RTCPeerConnection` with `iceServers: []` and Chromium's mDNS host-candidate hiding turned off
   (Electron: `--disable-features=WebRtcHideLocalIpsWithMdns`), so it offers plain LAN host candidates. One `sendonly`
   video transceiver per shared display, H.264 first in the codec preferences, `degradationPreference
   'maintain-resolution'`, `contentHint 'detail'`. Capture at the display's native pixel size, capped at 2560 px on the
   long side; 60 fps max; `maxBitrate` 12 Mbps per display up to 1080p, 20 Mbps above. It waits for ICE gathering to
   complete (no trickle), then sends:

   ```json
   {"type":"offer","sdp":"...","mids":{"0":1,"1":2},"mac":"<hex HMAC-SHA256(code, 'offer:'+fingerprint)>"}
   ```

   `mids` maps each transceiver's mid to the display id. `fingerprint` is the value of the first `a=fingerprint:` line
   of the SDP (e.g. `sha-256 AB:CD:...`).

   Note (after the Quest 3S test): New World Desktop now caps capture at 1920 px on the long side and 30 fps by
   default, at about 8 Mbps per display (12 Mbps with its optional "Smoother motion (60 fps)" setting), because two
   2560-wide 60 fps streams made the headset processor-bound.

5. The headset checks `mac` (wrong: close, "The connection could not be verified"), sets the offer, prefers H.264,
   answers after gathering, and sends:

   ```json
   {"type":"answer","sdp":"...","mac":"<hex HMAC-SHA256(code, 'answer:'+fingerprint of the answer)>"}
   ```

   The PC checks `mac` the same way. The code is forgotten by both sides once the answer is verified.

### During the session

| Message | Direction | Meaning |
|---|---|---|
| `{"type":"display-added","display":{...},"sdp":"...","mids":{...}}` | PC → headset | A monitor was attached and the user confirmed it on the PC. Carries a new offer (renegotiation, same rules as step 4 without `mac`); the headset replies `answer` without `mac`. |
| `{"type":"display-removed","id":2}` | PC → headset | A display was unplugged or unshared on the PC. Its transceiver is stopped. |
| `{"type":"stop-display","id":2}` | headset → PC | The user closed that display's panel. The PC stops capturing it. |
| `{"type":"stop","reason":"..."}` | either | End the whole session. The PC stops all capture. |
| `{"type":"ping"}` / `{"type":"pong"}` | either | Every 5 s; 15 s without any message ends the session. |

### Typing

The PC opens a data channel labelled `keys` (ordered) in its first offer, so it rides in the same DTLS connection as the video: encrypted, and only between the two devices whose fingerprints the code signed. The headset sends, as JSON text messages:

| Message | Meaning |
|---|---|
| `{"type":"text","text":"..."}` | Type this text (at most 1000 characters; `
` is Enter, `	` is Tab). |
| `{"type":"key","key":"Backspace","times":2}` | Press a key 1-200 times: `Backspace`, `Tab`, `Enter`, `Escape`, `Left`, `Up`, `Right`, `Down` or `Delete`. |

The PC types them into the window in focus only while sharing and while its "Let the headset type" switch is on (the default), and ignores anything else. Windows keeps them out of apps running as administrator. Nothing typed is logged. Desktop apps before 1.1.0 open no channel; the headset then says to update.

## Privacy

- Video and signalling never leave the local network; there is no server of ours (or anyone's) in the path.
- The desktop app shows a clear "Sharing with Meta Quest 3S" indicator and a Stop button for the whole session.
- Nothing is recorded or stored on either side.
