// Discovery beacons: once a second to 255.255.255.255:47800 and to every IPv4 interface's directed broadcast.
'use strict';

const dgram = require('dgram');
const os = require('os');
const { BEACON_PORT, SIGNAL_PORT, PROTOCOL_VERSION, APP_ID } = require('./protocol');

function broadcastAddresses() {
  const out = new Set(['255.255.255.255']);
  for (const addrs of Object.values(os.networkInterfaces())) {
    for (const a of addrs || []) {
      if (a.family !== 'IPv4' || a.internal || !a.netmask) continue;
      const ip = a.address.split('.').map(Number), mask = a.netmask.split('.').map(Number);
      if (ip[0] === 169 && ip[1] === 254) continue;   // link-local: no DHCP, no headset there
      out.add(ip.map((b, i) => (b | (~mask[i] & 255)) & 255).join('.'));
    }
  }
  return [...out];
}

class Beacon {
  constructor(name) {
    this.payload = Buffer.from(JSON.stringify({ app: APP_ID, v: PROTOCOL_VERSION, name, port: SIGNAL_PORT }));
    this.socket = null;
    this.timer = null;
  }

  start() {
    if (this.timer) return;
    this.socket = dgram.createSocket({ type: 'udp4', reuseAddr: true });
    this.socket.on('error', err => console.warn('[beacon]', err.message));
    this.socket.bind(0, () => {
      this.socket.setBroadcast(true);
      this.tick();
    });
    this.timer = setInterval(() => this.tick(), 1000);
  }

  tick() {
    if (!this.socket) return;
    for (const addr of broadcastAddresses()) {
      // An adapter that is going down makes send fail; the next second tries again.
      this.socket.send(this.payload, BEACON_PORT, addr, err => { if (err && err.code !== 'ENETUNREACH') console.warn('[beacon]', addr, err.message); });
    }
  }

  stop() {
    clearInterval(this.timer);
    this.timer = null;
    if (this.socket) { try { this.socket.close(); } catch { } }
    this.socket = null;
  }

  get running() { return !!this.timer; }
}

module.exports = { Beacon, broadcastAddresses };
