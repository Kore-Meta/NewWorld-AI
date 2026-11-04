# Protocol notes (desktop side)

How New World Desktop reads the gaps in [`docs/monitor-connection/protocol.md`](../../docs/monitor-connection/protocol.md).
`protocol.md` is unchanged apart from a note in step 4 about the lower caps (item 13) and the physical-size fields
in step 3 (item 16). Nothing here is unworkable; these are the choices the headset side should know about.

## Things the headset should expect

1. **Static displays send very few frames.** Chromium's desktop capture only produces a frame when the screen
   changes, so an idle display arrives at about 1 fps and a busy one at up to 30 (60 with Smoother motion). The
   headset should not treat a low frame rate, or several seconds without a frame, as a dead stream. Use the
   ping/pong and the ICE state for that.
2. **The video size is not the `approved` size.** `x/y/width/height` are logical (DPI-scaled) units, as the doc says.
   The stream is native pixels capped at 1920 on the long side. For example, a 3440×1440 monitor at 200% has bounds
   of 1720×720 and a 1920×804 stream. Size panels from the decoded frame, or from the bounds' aspect ratio, and use
   the bounds only for layout.
3. **`display-removed` and `stop-display` do not renegotiate.** The doc has no new offer for removal, so the PC does
   not call `transceiver.stop()` (that needs renegotiation). It calls `sender.replaceTrack(null)` and stops the
   capture. The m-line stays `sendonly` with no media. The headset should drop that panel and ignore the mid. The
   PC does not echo `display-removed` after a headset `stop-display`.
4. **`mids` in `display-added` is the full map** of every display being sent after the change, not only the new
   one. Mids of removed displays are left out.
5. **`display-added` can repeat a known id.** Besides a newly attached monitor (always confirmed on the PC first),
   the PC's sharing list has a "Share again" button for a display that was stopped, from either side. That sends
   `display-added` with the same id and reuses its old transceiver and mid. The headset should reopen the panel for
   that id.
6. **Renegotiation offers carry no `mac`, and the answer to one carries none either.** The PC checks only that it
   is waiting for that answer. The DTLS fingerprint cannot change within a session.

## Timing

7. **Ping from the start, silence enforced only after approval.** The PC sends `ping` every 5 s from the moment the
   hello arrives, so a headset that runs its 15 s watchdog from connect stays happy. The PC itself only ends a
   session for 15 s of silence once it has sent the offer. Before that, the user may still be typing the code,
   and the headset may not ping during pairing.
8. **Code lifetime.** The PC drops the pairing 120 s after the hello if it has not been approved by then (the
   headset's code is dead by then anyway). The idle screen then says the code expired.
9. **Waiting for an answer.** The offer is sent as soon as gathering completes (usually under 1 s, at most 5 s).
   The PC then waits for the answer with the ping watchdog active.

## Edge cases

10. **One headset at a time.** While a session is active (from the hello onwards), new TCP connections are closed
    straight away with no message, since nothing may be said before a code. A hello with `v != 1`, a malformed
    nonce or proof, or no hello within 10 s is also closed without a word.
11. **Five wrong codes** send `declined` with the reason "Too many wrong codes were entered on the computer."
12. **Beacons pause during a session.** "While sharing is allowed" is read as: the "Let headsets find this computer"
    switch is on (the default) and no headset is connected. Beacons resume when the session ends.
13. **Lower caps than step 4 first said.** After the Quest 3S test (two 2560-wide 60 fps streams took the app from
    72 to 53-58 fps, processor-bound), capture is capped at 1920 px on the long side and 30 fps, with `maxBitrate`
    8 Mbps per display. The "Smoother motion (60 fps)" setting, off by default, sends 60 fps at 12 Mbps. The
    setting is read when sharing starts and holds for the whole session, displays added later included. Step 4
    carries a note saying so.
14. **Answer `mac` failure** sends `stop` with "The connection could not be verified." and closes, mirroring step 5.
15. **Display ids.** Numbered once per session: the primary display first, then left to right and top to bottom. A
    monitor attached mid-session gets the next number ("Display 3"). These may not match the numbers Windows
    shows in Display settings.
16. **Physical sizes.** `widthMm`/`heightMm`/`sizeSource` come from the EDID, read through WMI
    (`WmiMonitorListedSupportedSourceModes`: the preferred mode's image size in mm, falling back to
    `WmiMonitorBasicDisplayParams` in whole cm). The read runs in the background at startup and again when a
    monitor is added or removed, with an 8 s limit; Share waits for a read already under way. A monitor is matched to
    an Electron display by its EDID friendly name (Electron's `display.label`), then by native resolution, then by
    aspect, in order. A rotated display gets the EDID size swapped. With no EDID or no match the size is native
    pixels at 110 ppi (`sizeSource: "estimate"`). The width is clamped to 150-1600 mm, and the height then follows
    the pixel aspect. On this PC both 34" LG ultrawides (3440×1440) read 800×334 and 800×335 mm.
