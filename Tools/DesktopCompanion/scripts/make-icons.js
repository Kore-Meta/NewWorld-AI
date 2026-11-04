// Draws the app and tray icons (a brass ring on ink; the sharing tray icon adds a sage dot) and writes
// src/assets/*.png and build/icon.ico. No dependencies: a tiny anti-aliased rasteriser and PNG/ICO writers.
// Run with `npm run icons`; the outputs are committed, so this is only needed to change the design.
'use strict';

const fs = require('fs');
const path = require('path');
const zlib = require('zlib');

const INK = [0x1d, 0x24, 0x24], PAPER = [0xf6, 0xf1, 0xe7], BRASS = [0xb0, 0x8a, 0x4a], SAGE = [0x8f, 0xb6, 0x9b];

// Coverage of a signed-distance shape at one pixel, with a 1 px soft edge.
const cover = dist => Math.max(0, Math.min(1, 0.5 - dist));

function draw(size, { tile, sharing }) {
  const px = Buffer.alloc(size * size * 4);
  const s = size / 32;   // design on a 32-unit grid
  const c = size / 2;
  for (let y = 0; y < size; y++) for (let x = 0; x < size; x++) {
    const fx = x + 0.5, fy = y + 0.5;
    let rgb = [0, 0, 0], a = 0;
    const over = (col, cov) => { if (cov <= 0) return; const na = cov + a * (1 - cov); rgb = rgb.map((v, i) => (col[i] * cov + v * a * (1 - cov)) / (na || 1)); a = na; };
    if (tile) {   // rounded square of ink
      const r = 7 * s, h = c - 0.5 * s;
      const qx = Math.abs(fx - c) - (h - r), qy = Math.abs(fy - c) - (h - r);
      over(INK, cover(Math.hypot(Math.max(qx, 0), Math.max(qy, 0)) + Math.min(Math.max(qx, qy), 0) - r));
    }
    const d = Math.hypot(fx - c, fy - c);
    const ringR = (tile ? 9.5 : 11.5) * s, ringW = (tile ? 2.6 : 3.4) * s;
    over(BRASS, cover(Math.abs(d - ringR) - ringW / 2));
    over(tile ? PAPER : BRASS, cover(d - (tile ? 3 : 4.2) * s));
    if (sharing) {
      const dx = fx - 25 * s, dy = fy - 25 * s, dd = Math.hypot(dx, dy);
      over(PAPER, cover(dd - 7 * s));
      over(SAGE, cover(dd - 5.4 * s));
    }
    const o = (y * size + x) * 4;
    px[o] = Math.round(rgb[0]); px[o + 1] = Math.round(rgb[1]); px[o + 2] = Math.round(rgb[2]); px[o + 3] = Math.round(a * 255);
  }
  return png(size, size, px);
}

const CRC = new Int32Array(256).map((_, n) => { let c = n; for (let k = 0; k < 8; k++) c = c & 1 ? 0xedb88320 ^ (c >>> 1) : c >>> 1; return c; });
const crc32 = buf => { let c = -1; for (const b of buf) c = CRC[(c ^ b) & 255] ^ (c >>> 8); return (c ^ -1) >>> 0; };
function chunk(type, data) {
  const len = Buffer.alloc(4); len.writeUInt32BE(data.length);
  const body = Buffer.concat([Buffer.from(type), data]);
  const crc = Buffer.alloc(4); crc.writeUInt32BE(crc32(body));
  return Buffer.concat([len, body, crc]);
}
function png(w, h, rgba) {
  const ihdr = Buffer.alloc(13);
  ihdr.writeUInt32BE(w, 0); ihdr.writeUInt32BE(h, 4); ihdr[8] = 8; ihdr[9] = 6;
  const raw = Buffer.alloc((w * 4 + 1) * h);
  for (let y = 0; y < h; y++) rgba.copy(raw, y * (w * 4 + 1) + 1, y * w * 4, (y + 1) * w * 4);
  return Buffer.concat([Buffer.from([0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a]), chunk('IHDR', ihdr), chunk('IDAT', zlib.deflateSync(raw, { level: 9 })), chunk('IEND', Buffer.alloc(0))]);
}
// ICO holding PNG images (Vista+).
function ico(images) {
  const head = Buffer.alloc(6 + 16 * images.length);
  head.writeUInt16LE(0, 0); head.writeUInt16LE(1, 2); head.writeUInt16LE(images.length, 4);
  let offset = head.length;
  images.forEach(({ size, data }, i) => {
    const e = 6 + 16 * i;
    head[e] = size >= 256 ? 0 : size; head[e + 1] = size >= 256 ? 0 : size;
    head.writeUInt16LE(1, e + 4); head.writeUInt16LE(32, e + 6);
    head.writeUInt32LE(data.length, e + 8); head.writeUInt32LE(offset, e + 12);
    offset += data.length;
  });
  return Buffer.concat([head, ...images.map(i => i.data)]);
}

const root = path.join(__dirname, '..');
const assets = path.join(root, 'src', 'assets'), build = path.join(root, 'build');
fs.mkdirSync(assets, { recursive: true }); fs.mkdirSync(build, { recursive: true });
fs.writeFileSync(path.join(assets, 'icon.png'), draw(256, { tile: true }));
for (const [name, sharing] of [['tray', false], ['tray-sharing', true]]) {
  fs.writeFileSync(path.join(assets, `${name}.png`), draw(16, { tile: false, sharing }));
  fs.writeFileSync(path.join(assets, `${name}@2x.png`), draw(32, { tile: false, sharing }));
}
fs.writeFileSync(path.join(build, 'icon.ico'), ico([16, 24, 32, 48, 64, 128, 256].map(size => ({ size, data: draw(size, { tile: true }) }))));
console.log('Icons written to src/assets and build/icon.ico');
