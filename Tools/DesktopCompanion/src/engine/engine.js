// Capture and WebRTC, in a hidden renderer. Driven by the main process over IPC (preload-engine.js).
// One RTCPeerConnection per session, one sendonly H.264 transceiver per shared display (protocol.md step 4).
'use strict';

let pc = null;
const shared = new Map();   // display id -> { transceiver, track, display }
let statsTimer = null;
let fps = 30;               // per session: 30, or 60 with "Smoother motion"

const MBPS = 1e6;
const bitrate = () => (fps > 30 ? 12 : 8) * MBPS;   // capture is at most 1920 on the long side

// H.264 first (packetization-mode=1, constrained baseline first: what the Quest's c2.qti.avc.decoder takes most
// happily), then everything else in Chromium's order so RTX/RED/FEC stay available.
function codecOrder() {
  const caps = RTCRtpSender.getCapabilities('video').codecs;
  const h264 = caps.filter(c => c.mimeType.toLowerCase() === 'video/h264');
  const score = c => (/packetization-mode=1/.test(c.sdpFmtpLine || '') ? 0 : 2) + (/profile-level-id=42e0/.test(c.sdpFmtpLine || '') ? 0 : 1);
  h264.sort((a, b) => score(a) - score(b));
  return [...h264, ...caps.filter(c => c.mimeType.toLowerCase() !== 'video/h264')];
}

async function capture(d) {
  const stream = await navigator.mediaDevices.getUserMedia({
    audio: false,
    video: {
      mandatory: {
        chromeMediaSource: 'desktop',
        chromeMediaSourceId: d.sourceId,
        maxWidth: d.capture.width, maxHeight: d.capture.height,
        minWidth: Math.min(640, d.capture.width), minHeight: Math.min(360, d.capture.height),
        maxFrameRate: fps,
      },
    },
  });
  const track = stream.getVideoTracks()[0];
  track.contentHint = 'detail';
  track.onended = () => window.engine.event('track-ended', { id: d.id });
  return track;
}

async function addTransceiver(d) {
  const track = await capture(d);
  const transceiver = pc.addTransceiver(track, { direction: 'sendonly' });
  transceiver.setCodecPreferences(codecOrder());
  const p = transceiver.sender.getParameters();
  p.degradationPreference = 'maintain-resolution';
  if (!p.encodings || !p.encodings.length) p.encodings = [{}];
  p.encodings[0].maxBitrate = bitrate();
  p.encodings[0].maxFramerate = fps;
  await transceiver.sender.setParameters(p).catch(e => console.warn('setParameters', e));
  shared.set(d.id, { transceiver, track, display: d });
}

function waitForGathering() {
  return new Promise(resolve => {
    if (pc.iceGatheringState === 'complete') return resolve();
    const t = setTimeout(resolve, 5000);   // a dead adapter can stall gathering; send what we have
    pc.addEventListener('icegatheringstatechange', function onChange() {
      if (pc.iceGatheringState === 'complete') { clearTimeout(t); pc.removeEventListener('icegatheringstatechange', onChange); resolve(); }
    });
  });
}

// mid -> display id for every display still being sent.
function mids() {
  const out = {};
  for (const [id, s] of shared) if (s.track && s.transceiver.mid != null) out[s.transceiver.mid] = id;
  return out;
}

async function makeOffer() {
  await pc.setLocalDescription(await pc.createOffer());
  await waitForGathering();
  return { sdp: pc.localDescription.sdp, mids: mids() };
}

const commands = {
  async start({ displays, smooth }) {
    commands.stopAll();
    fps = smooth ? 60 : 30;
    pc = new RTCPeerConnection({ iceServers: [], bundlePolicy: 'max-bundle' });   // LAN host candidates only
    pc.onconnectionstatechange = () => window.engine.event('connection', { state: pc && pc.connectionState });
    for (const d of displays) await addTransceiver(d);
    statsTimer = setInterval(sendStats, 2000);
    return makeOffer();
  },

  async add({ display }) {
    if (!pc) throw new Error('No session');
    const old = shared.get(display.id);
    if (old && !old.track) {   // shared before, then stopped: reuse its transceiver and mid
      old.track = await capture(display);
      old.display = display;
      await old.transceiver.sender.replaceTrack(old.track);
    } else {
      await addTransceiver(display);
    }
    return makeOffer();
  },

  async setAnswer({ sdp }) {
    if (!pc) throw new Error('No session');
    await pc.setRemoteDescription({ type: 'answer', sdp });
    return true;
  },

  // Stops one display without renegotiation: the track goes, the transceiver stays (protocol: display-removed).
  async remove({ id }) {
    const s = shared.get(id);
    if (!s || !s.track) return false;
    s.track.onended = null;
    s.track.stop();
    s.track = null;
    await s.transceiver.sender.replaceTrack(null).catch(() => { });
    return true;
  },

  stopAll() {
    clearInterval(statsTimer);
    statsTimer = null;
    for (const s of shared.values()) if (s.track) { s.track.onended = null; s.track.stop(); }
    shared.clear();
    if (pc) { pc.onconnectionstatechange = null; pc.close(); }
    pc = null;
    return true;
  },

  codecs() { return codecOrder().map(c => `${c.mimeType} ${c.sdpFmtpLine || ''}`.trim()); },
};

async function sendStats() {
  if (!pc) return;
  const report = await pc.getStats();
  const codecs = {}, out = [];
  report.forEach(s => { if (s.type === 'codec') codecs[s.id] = s.mimeType; });
  const byMid = {};
  for (const [id, s] of shared) if (s.transceiver.mid != null) byMid[s.transceiver.mid] = id;
  report.forEach(s => {
    if (s.type !== 'outbound-rtp' || s.kind !== 'video' || byMid[s.mid] == null) return;
    out.push({
      id: byMid[s.mid], codec: codecs[s.codecId] || null, width: s.frameWidth || 0, height: s.frameHeight || 0,
      fps: Math.round(s.framesPerSecond || 0), mbps: +((s.targetBitrate || 0) / MBPS).toFixed(1),
      encoder: s.encoderImplementation || null, limitedBy: s.qualityLimitationReason || 'none',
    });
  });
  window.engine.event('stats', out);
}

// Errors cross the context bridge as plain strings (a DOMException would arrive as "[object Object]").
window.engine.onCommand(async (cmd, args) => {
  if (!commands[cmd]) throw `Unknown command ${cmd}`;
  try {
    return await commands[cmd](args || {});
  } catch (e) {
    throw e && e.name ? `${e.name}: ${e.message}` : String(e);
  }
});
