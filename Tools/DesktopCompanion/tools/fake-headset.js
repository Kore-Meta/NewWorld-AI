#!/usr/bin/env node
// Plays the headset side of protocol.md against a running New World Desktop:
// hears a beacon, connects, sends hello with the code given, then prints the approved displays and the offer.
//
//   npm run fake-headset -- 123456                 discover over UDP 47800, then pair
//   npm run fake-headset -- 123456 --host 192.168.1.20
//   npm run fake-headset -- 123456 --exit-after-offer
//   npm run fake-headset -- 123456 --receive         also answer with a real WebRTC receiver (an Electron window)
//
// Without --receive it has no WebRTC stack and never answers: the computer waits until you stop either side.
// With --receive it answers like the headset (H.264 preferred, mac on the first answer), shows the streams and prints
// what it decodes. While connected, type `stop-display <id>` or `stop` and press Enter.
// Type the same code into the computer when it asks.
'use strict';

const dgram = require('dgram');
const net = require('net');
const crypto = require('crypto');
const P = require('../src/protocol');

const args = process.argv.slice(2);
const code = args.find(a => /^\d{6}$/.test(a));
const opt = name => { const i = args.indexOf(name); return i >= 0 ? args[i + 1] : null; };
const host = opt('--host');
const exitAfterOffer = args.includes('--exit-after-offer');
const receive = args.includes('--receive');
if (!code) {
  console.error('Usage: npm run fake-headset -- <6-digit code> [--host <pc ip>] [--exit-after-offer] [--receive]');
  process.exit(2);
}

const log = (...a) => console.log(`[${new Date().toISOString().slice(11, 19)}]`, ...a);
let socket = null, pinger = null, failures = 0, receiver = null, answered = false, mids = {};

// The WebRTC side: tools/receiver in Electron, talking JSON lines over a loopback socket.
let receiverLink = null;
const receiverQueue = [];
function startReceiver() {
  const { spawn } = require('child_process');
  const srv = net.createServer(link => {
    srv.close();
    receiverLink = link;
    receiverQueue.splice(0).forEach(toReceiver);
    P.attachLineReader(link, fromReceiver);
  });
  srv.listen(0, '127.0.0.1', () => {
    receiver = spawn(require('electron'), [require('path').join(__dirname, 'receiver', 'main.js'), String(srv.address().port)], { stdio: 'ignore' });
    receiver.on('exit', c => { if (receiver) log(`Receiver exited (${c}).`); receiver = null; });
  });
}
function fromReceiver(m) {
  if (m.type === 'answer') {
    const msg = { type: 'answer', sdp: m.sdp };
    if (!answered) msg.mac = P.answerMac(code, m.sdp);   // only the first answer carries a mac
    answered = true;
    P.send(socket, msg);
    log(`Sent answer${msg.mac ? ' with mac' : ''}.`);
  } else if (m.type === 'state') {
    log(`WebRTC ${m.state}`);
  } else if (m.type === 'stats') {
    for (const r of m.rows) log(`  recv mid ${r.mid} -> display ${mids[r.mid] ?? '-'}: ${r.codec} ${r.width}×${r.height} @ ${r.fps} fps, ${r.decoded} frames, ${r.decoder}`);
  } else if (m.type === 'error') {
    log(`Receiver error: ${m.message}`);
    failures++;
  }
}
function toReceiver(m) {
  if (receiverLink) P.send(receiverLink, m);
  else if (receive) receiverQueue.push(m);
}

// Typed commands while connected.
require('readline').createInterface({ input: process.stdin }).on('line', line => {
  const [cmd, arg] = line.trim().split(/\s+/);
  if (!socket) return;
  if (cmd === 'stop-display' && arg) { P.send(socket, { type: 'stop-display', id: Number(arg) }); log(`Sent stop-display ${arg}`); }
  else if (cmd === 'stop') finish();
});

function discover() {
  const udp = dgram.createSocket({ type: 'udp4', reuseAddr: true });
  udp.on('error', err => { console.error('UDP error:', err.message); process.exit(1); });
  udp.on('message', (buf, rinfo) => {
    let b;
    try { b = JSON.parse(buf.toString('utf8')); } catch { return; }
    if (b.app !== P.APP_ID || b.v !== P.PROTOCOL_VERSION || socket) return;
    log(`Beacon from ${rinfo.address}: ${JSON.stringify(b)}`);
    udp.close();
    connect(rinfo.address, b.port || P.SIGNAL_PORT);   // the address comes from the packet, not the beacon
  });
  udp.bind(P.BEACON_PORT, () => log(`Listening for beacons on UDP ${P.BEACON_PORT}…`));
}

function connect(ip, port) {
  log(`Connecting to ${ip}:${port}…`);
  socket = net.connect(port, ip, () => {
    const nonce = crypto.randomBytes(16).toString('hex');
    P.send(socket, { type: 'hello', v: 1, device: 'Meta Quest 3S', nonce, proof: P.helloProof(code, nonce) });
    log(`Sent hello. Type ${code} into New World Desktop.`);
    pinger = setInterval(() => P.send(socket, { type: 'ping' }), 5000);
  });
  P.attachLineReader(socket, onMessage);
  socket.on('error', err => { console.error('TCP error:', err.message); process.exit(1); });
  socket.on('close', () => {
    log('Connection closed.');
    clearInterval(pinger);
    if (receiver) receiver.kill();
    process.exit(failures ? 1 : 0);
  });
}

function onMessage(m) {
  switch (m.type) {
    case 'ping': P.send(socket, { type: 'pong' }); break;
    case 'pong': break;
    case 'declined': log(`DECLINED: ${m.reason}`); break;
    case 'approved':
      log(`APPROVED by ${m.name}:`);
      for (const d of m.displays) log(`  #${d.id} ${d.name}${d.primary ? ' (primary)' : ''}  ${d.width}×${d.height} at (${d.x}, ${d.y}), ${d.widthMm}×${d.heightMm} mm (${d.sizeSource})`);
      break;
    case 'offer':
      report(m, true);
      mids = m.mids;
      if (exitAfterOffer) finish();
      else if (!failures) toReceiver({ type: 'offer', sdp: m.sdp });
      break;
    case 'display-added':
      log(`DISPLAY ADDED: #${m.display.id} ${m.display.name} ${m.display.width}×${m.display.height}, ${m.display.widthMm}×${m.display.heightMm} mm (${m.display.sizeSource})`);
      report(m, false);
      mids = m.mids;
      toReceiver({ type: 'offer', sdp: m.sdp });
      break;
    case 'display-removed': log(`DISPLAY REMOVED: #${m.id}`); break;
    case 'stop': log(`STOP: ${m.reason}`); break;
    default: log(`(ignored ${m.type})`);
  }
}

// Prints the offer and checks what the real headset relies on.
function report(m, withMac) {
  const check = (ok, what) => { log(`  ${ok ? 'ok  ' : 'FAIL'} ${what}`); if (!ok) failures++; };
  const sdp = m.sdp || '';
  log(`${withMac ? 'OFFER' : 'RENEGOTIATION OFFER'} (${sdp.length} bytes), mids ${JSON.stringify(m.mids)}`);

  const sections = sdp.split(/\r?\n(?=m=)/).slice(1);
  const video = sections.filter(s => s.startsWith('m=video'));
  for (const s of video) {
    const mid = (/^a=mid:(.+)$/m.exec(s) || [])[1];
    const dir = (/^a=(sendonly|recvonly|sendrecv|inactive)$/m.exec(s) || [])[1];
    const pts = s.split(/\r?\n/)[0].split(' ').slice(3);
    const names = pts.map(pt => (new RegExp(`^a=rtpmap:${pt} ([^/]+)`, 'm').exec(s) || [])[1] || '?');
    const fmtp = (new RegExp(`^a=fmtp:${pts[0]} (.+)$`, 'm').exec(s) || [])[1] || '';
    log(`  m=video mid ${mid} ${dir} -> display ${m.mids[mid] ?? '(not shared)'}; codecs ${names.slice(0, 6).join(', ')}${names.length > 6 ? ', …' : ''}`);
    log(`    first: ${names[0]} ${fmtp}`);
    if (m.mids[mid] != null) check(names[0] === 'H264', `mid ${mid}: H.264 is the first codec`);
  }
  check(Object.keys(m.mids).length > 0 && Object.keys(m.mids).every(mid => video.some(s => new RegExp(`^a=mid:${mid}\\r?$`, 'm').test(s))), 'every mid in "mids" has a video m-line');

  const cands = [...sdp.matchAll(/^a=candidate:\S+ \d+ (\w+) \d+ (\S+) (\d+) typ (\w+)/gm)].map(c => ({ proto: c[1], ip: c[2], port: c[3], type: c[4] }));
  const uniq = [...new Map(cands.map(c => [`${c.proto} ${c.ip}:${c.port} ${c.type}`, c])).values()];
  log(`  candidates:`);
  for (const c of uniq) log(`    ${c.type} ${c.proto} ${c.ip}:${c.port}`);
  check(uniq.every(c => c.type === 'host'), 'host candidates only (no STUN/TURN)');
  check(!uniq.some(c => c.ip.endsWith('.local')), 'no mDNS (.local) candidates');
  check(uniq.some(c => /^192\.168\./.test(c.ip)), 'a 192.168.x.x host candidate');
  check(uniq.length > 0, 'candidates are in the SDP (no trickle)');

  const fp = P.sdpFingerprint(sdp);
  log(`  fingerprint ${fp}`);
  if (withMac) check(P.hexEquals(P.offerMac(code, sdp), m.mac), 'mac matches HMAC(code, "offer:"+fingerprint)');
  log(failures ? `${failures} check(s) failed.` : `All checks passed.${receive ? '' : ' (No answer is sent without --receive.)'}`);
}

function finish() {
  P.send(socket, { type: 'stop', reason: 'Fake headset finished.' });
  setTimeout(() => socket.end(), 200);
}

process.on('SIGINT', () => { if (socket) finish(); else process.exit(0); });

if (receive) startReceiver();
host ? connect(host, P.SIGNAL_PORT) : discover();
