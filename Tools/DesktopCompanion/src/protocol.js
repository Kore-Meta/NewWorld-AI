// Shared pieces of docs/monitor-connection/protocol.md v1: ports, HMACs and newline-delimited JSON framing.
// Used by the app's main process and by tools/fake-headset.js.
'use strict';

const crypto = require('crypto');

const BEACON_PORT = 47800;
const SIGNAL_PORT = 47801;
const PROTOCOL_VERSION = 1;
const APP_ID = 'newworld-desktop';
const MAX_LINE = 1 << 20;   // an SDP for a few displays is a few KB; anything near 1 MB is not ours

function hmacHex(code, message) {
  return crypto.createHmac('sha256', String(code)).update(message, 'utf8').digest('hex');
}

// Constant-time compare of two hex strings; false for anything malformed.
function hexEquals(a, b) {
  if (typeof a !== 'string' || typeof b !== 'string' || a.length !== b.length || !/^[0-9a-f]+$/i.test(a)) return false;
  const x = Buffer.from(a.toLowerCase(), 'hex'), y = Buffer.from(b.toLowerCase(), 'hex');
  return x.length === y.length && crypto.timingSafeEqual(x, y);
}

// The value of the first a=fingerprint: line, e.g. "sha-256 AB:CD:...".
function sdpFingerprint(sdp) {
  const m = /^a=fingerprint:(.+)$/m.exec(String(sdp || ''));
  return m ? m[1].trim() : null;
}

const helloProof = (code, nonce) => hmacHex(code, 'hello:' + nonce);
const offerMac = (code, sdp) => hmacHex(code, 'offer:' + sdpFingerprint(sdp));
const answerMac = (code, sdp) => hmacHex(code, 'answer:' + sdpFingerprint(sdp));

// Splits a socket's byte stream into JSON messages. Bad JSON lines are skipped; an overlong line closes the socket.
function attachLineReader(socket, onMessage) {
  let buf = '';
  socket.setEncoding('utf8');
  socket.on('data', chunk => {
    buf += chunk;
    let nl;
    while ((nl = buf.indexOf('\n')) >= 0) {
      const line = buf.slice(0, nl).trim();
      buf = buf.slice(nl + 1);
      if (!line) continue;
      let msg;
      try { msg = JSON.parse(line); } catch { continue; }
      if (msg && typeof msg === 'object' && typeof msg.type === 'string') onMessage(msg);
    }
    if (buf.length > MAX_LINE) socket.destroy();
  });
}

function send(socket, msg) {
  if (socket && !socket.destroyed && socket.writable) socket.write(JSON.stringify(msg) + '\n');
}

module.exports = {
  BEACON_PORT, SIGNAL_PORT, PROTOCOL_VERSION, APP_ID,
  hmacHex, hexEquals, sdpFingerprint, helloProof, offerMac, answerMac, attachLineReader, send,
};
