'use strict';

let pc = null;
const tiles = new Map();   // mid -> figcaption

function waitForGathering() {
  return new Promise(resolve => {
    if (pc.iceGatheringState === 'complete') return resolve();
    const t = setTimeout(resolve, 5000);
    pc.addEventListener('icegatheringstatechange', () => { if (pc.iceGatheringState === 'complete') { clearTimeout(t); resolve(); } });
  });
}

function preferH264() {
  const caps = RTCRtpReceiver.getCapabilities('video').codecs;
  const h264 = caps.filter(c => c.mimeType.toLowerCase() === 'video/h264');
  for (const t of pc.getTransceivers()) if (t.receiver.track && t.receiver.track.kind === 'video') {
    try { t.setCodecPreferences([...h264, ...caps.filter(c => !h264.includes(c))]); } catch { }
  }
}

window.bridge.onMessage(msg => {
  if (msg.type === 'offer') answer(msg).catch(e => window.bridge.send({ type: 'error', message: `${e.name}: ${e.message}` }));
});

async function answer(msg) {
  if (!pc) {
    pc = new RTCPeerConnection({ iceServers: [] });
    pc.onconnectionstatechange = () => window.bridge.send({ type: 'state', state: pc.connectionState });
    pc.ontrack = e => {
      const mid = e.transceiver.mid;
      if (tiles.has(mid)) return;
      const v = Object.assign(document.createElement('video'), { autoplay: true, muted: true, srcObject: new MediaStream([e.track]) });
      const cap = document.createElement('figcaption');
      cap.textContent = `mid ${mid}`;
      const fig = document.createElement('figure');
      fig.append(v, cap);
      document.getElementById('grid').append(fig);
      tiles.set(mid, cap);
    };
    setInterval(stats, 2000);
  }
  await pc.setRemoteDescription({ type: 'offer', sdp: msg.sdp });
  preferH264();
  await pc.setLocalDescription(await pc.createAnswer());
  await waitForGathering();
  window.bridge.send({ type: 'answer', sdp: pc.localDescription.sdp });
}

async function stats() {
  const report = await pc.getStats();
  const codecs = {}, rows = [];
  report.forEach(s => { if (s.type === 'codec') codecs[s.id] = s.mimeType; });
  report.forEach(s => {
    if (s.type !== 'inbound-rtp' || s.kind !== 'video') return;
    const row = { mid: s.mid, codec: codecs[s.codecId] || '?', width: s.frameWidth || 0, height: s.frameHeight || 0,
      fps: Math.round(s.framesPerSecond || 0), decoded: s.framesDecoded || 0, decoder: s.decoderImplementation || '?' };
    rows.push(row);
    const cap = tiles.get(s.mid);
    if (cap) cap.textContent = `mid ${row.mid} · ${row.codec} ${row.width}×${row.height} @ ${row.fps} fps · ${row.decoder}`;
  });
  window.bridge.send({ type: 'stats', rows });
}
