// Signalling: a TCP server on 47801 speaking newline-delimited JSON (protocol.md, "Signalling").
// One headset session at a time. The pairing code, HMACs and every message check live here, in the main process.
'use strict';

const net = require('net');
const { EventEmitter } = require('events');
const P = require('./protocol');

const MAX_TRIES = 5;
const CODE_LIFETIME_MS = 120 * 1000;   // the headset's code is valid for 120 s
const PING_EVERY_MS = 5000;
const SILENCE_LIMIT_MS = 15000;

// Session states: 'code' (waiting for the user to type the code), 'approve' (code good, choosing displays),
// 'offering' (approved, building the offer), 'answering' (offer sent), 'sharing' (answer verified), 'ended'.
class Session extends EventEmitter {
  constructor(socket, hello) {
    super();
    this.socket = socket;
    this.remote = socket.remoteAddress.replace(/^::ffff:/, '');
    this.device = String(hello.device || 'Meta Quest').replace(/[\u0000-\u001f]/g, '').slice(0, 60) || 'Meta Quest';
    this.nonce = hello.nonce;
    this.proof = hello.proof;
    this.code = null;
    this.triesLeft = MAX_TRIES;
    this.state = 'code';
    this.lastHeard = Date.now();
    this.pendingRenegotiation = false;

    P.attachLineReader(socket, msg => this.onMessage(msg));
    socket.on('close', () => this.end('The headset disconnected.', { silent: true }));
    socket.on('error', () => { });

    this.codeTimer = setTimeout(() => {
      if (this.state === 'code' || this.state === 'approve') this.end('The code in the headset expired. Choose Connect a computer again.');
    }, CODE_LIFETIME_MS);
    // ping carries nothing identifying, so it is sent from the start. Silence is only enforced once the session is
    // approved: before that the user may take longer than 15 s to type the code (see PROTOCOL-NOTES.md).
    this.pingTimer = setInterval(() => {
      P.send(this.socket, { type: 'ping' });
      const watching = this.state === 'answering' || this.state === 'sharing';
      if (watching && Date.now() - this.lastHeard > SILENCE_LIMIT_MS) this.end('The headset stopped responding.');
    }, PING_EVERY_MS);
  }

  get active() { return this.state !== 'ended'; }

  // Returns { ok, triesLeft, declined }.
  tryCode(code) {
    if (this.state !== 'code') return { ok: false, triesLeft: this.triesLeft };
    code = String(code || '').replace(/\D/g, '');
    if (code.length === 6 && P.hexEquals(P.helloProof(code, this.nonce), this.proof)) {
      this.code = code;
      this.state = 'approve';
      return { ok: true, triesLeft: this.triesLeft };
    }
    this.triesLeft--;
    if (this.triesLeft <= 0) {
      this.decline('Too many wrong codes were entered on the computer.');
      return { ok: false, triesLeft: 0, declined: true };
    }
    return { ok: false, triesLeft: this.triesLeft };
  }

  decline(reason = 'Sharing was declined on the computer.') {
    if (!this.active) return;
    P.send(this.socket, { type: 'declined', reason });
    this.end(reason, { local: true });
  }

  approve(name, displays) {
    if (this.state !== 'approve') return false;
    this.state = 'offering';
    P.send(this.socket, { type: 'approved', name, displays });
    return true;
  }

  sendOffer(sdp, mids) {
    if (this.state !== 'offering') return;
    this.state = 'answering';
    P.send(this.socket, { type: 'offer', sdp, mids, mac: P.offerMac(this.code, sdp) });
  }

  sendDisplayAdded(display, sdp, mids) {
    if (this.state !== 'sharing') return;
    this.pendingRenegotiation = true;
    P.send(this.socket, { type: 'display-added', display, sdp, mids });
  }

  sendDisplayRemoved(id) {
    if (this.state === 'sharing' || this.state === 'answering') P.send(this.socket, { type: 'display-removed', id });
  }

  stop(reason = 'Sharing was stopped on the computer.') {
    if (!this.active) return;
    P.send(this.socket, { type: 'stop', reason });
    this.end(reason, { local: true });
  }

  onMessage(msg) {
    this.lastHeard = Date.now();
    switch (msg.type) {
      case 'ping': P.send(this.socket, { type: 'pong' }); break;
      case 'answer': this.onAnswer(msg); break;
      case 'stop-display':
        if (this.state === 'sharing' || this.state === 'answering') this.emit('stop-display', Number(msg.id));
        break;
      case 'stop': this.end(typeof msg.reason === 'string' && msg.reason ? `The headset ended sharing: ${msg.reason.slice(0, 120)}` : 'The headset ended sharing.'); break;
      default: break;   // pong, a second hello, unknown types: ignored
    }
  }

  onAnswer(msg) {
    if (typeof msg.sdp !== 'string') return;
    if (this.state === 'answering') {
      if (!P.sdpFingerprint(msg.sdp) || !P.hexEquals(P.answerMac(this.code, msg.sdp), msg.mac)) {
        this.stop('The connection could not be verified.');
        return;
      }
      this.code = null;   // forgotten once the answer is verified
      this.state = 'sharing';
      this.emit('answer', msg.sdp);
    } else if (this.state === 'sharing' && this.pendingRenegotiation) {
      this.pendingRenegotiation = false;
      this.emit('answer', msg.sdp);
    }
  }

  // Ends the session once; 'ended' carries a reason for the UI.
  end(reason, { local = false, silent = false } = {}) {
    if (this.state === 'ended') return;
    const was = this.state;
    this.state = 'ended';
    this.code = null;
    clearTimeout(this.codeTimer);
    clearInterval(this.pingTimer);
    // Let a final declined/stop line flush before closing.
    this.socket.end();
    setTimeout(() => this.socket.destroy(), 500);
    this.emit('ended', { reason, local, wasState: was, silent });
  }
}

class SignalServer extends EventEmitter {
  constructor() {
    super();
    this.session = null;
    this.server = net.createServer(socket => this.onConnection(socket));
    this.server.on('error', err => this.emit('error', err));
  }

  listen() {
    return new Promise((resolve, reject) => {
      this.server.once('error', reject);
      this.server.listen(P.SIGNAL_PORT, '0.0.0.0', () => { this.server.off('error', reject); resolve(); });
    });
  }

  onConnection(socket) {
    socket.setNoDelay(true);
    socket.setKeepAlive(true, 5000);
    if (this.session && this.session.active) {   // busy: one headset at a time; nothing is said before a code
      socket.destroy();
      return;
    }
    let done = false;
    // The first line must be a valid hello within 10 s, or the socket is dropped without a word.
    const timer = setTimeout(() => { if (!done) socket.destroy(); }, 10000);
    socket.on('error', () => { });
    const onFirst = msg => {
      if (done || msg.type !== 'hello') return;
      done = true;
      clearTimeout(timer);
      socket.removeAllListeners('data');
      const okHello = msg.v === P.PROTOCOL_VERSION && /^[0-9a-f]{32}$/i.test(msg.nonce || '') && /^[0-9a-f]{64}$/i.test(msg.proof || '');
      if (!okHello || (this.session && this.session.active)) { socket.destroy(); return; }
      this.session = new Session(socket, msg);
      this.emit('session', this.session);
    };
    P.attachLineReader(socket, onFirst);
  }

  close() { this.server.close(); }
}

module.exports = { SignalServer, Session, MAX_TRIES };
