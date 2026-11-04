// The PC's displays: Electron screen info matched to desktopCapturer sources by display_id, with session ids.
'use strict';

const { screen, desktopCapturer } = require('electron');
const monitors = require('./monitors');

// 1920, not 2560: two 2560-wide streams made the Quest 3S processor-bound (72 -> 53-58 fps).
const MAX_LONG_SIDE = 1920;

// Capture size: native pixels, capped at 1920 on the long side, even numbers for the encoder.
function captureSize(d) {
  let w = Math.round(d.bounds.width * d.scaleFactor), h = Math.round(d.bounds.height * d.scaleFactor);
  const k = Math.min(1, MAX_LONG_SIDE / Math.max(w, h));
  w = Math.round(w * k / 2) * 2; h = Math.round(h * k / 2) * 2;
  return { width: w, height: h };
}

// Stable order for numbering: the primary display first, then left to right, top to bottom.
function orderedDisplays() {
  const primary = screen.getPrimaryDisplay().id;
  return screen.getAllDisplays().sort((a, b) =>
    (b.id === primary) - (a.id === primary) || a.bounds.x - b.bounds.x || a.bounds.y - b.bounds.y);
}

async function screenSources() {
  return desktopCapturer.getSources({ types: ['screen'], thumbnailSize: { width: 0, height: 0 } });
}

// Pairs every Electron display with its capture source. display_id is the normal match; when Windows leaves it
// empty (it can for some adapters), sources without an id are given out in order to displays without a match.
function matchSources(displays, sources) {
  const byId = new Map(sources.filter(s => s.display_id).map(s => [String(s.display_id), s]));
  const loose = sources.filter(s => !s.display_id || !displays.some(d => String(d.id) === String(s.display_id)));
  const out = new Map();
  for (const d of displays) {
    const s = byId.get(String(d.id)) || loose.shift();
    if (s) out.set(d.id, s.id);
  }
  return out;
}

// Tracks session ids ("Display N") for Electron displays. Ids are small, positive and stable for a session.
class DisplayBook {
  constructor() { this.reset(); }

  reset() {
    this.ids = new Map();   // electron display id -> session id
    this.next = 1;
  }

  idFor(electronId) {
    if (!this.ids.has(electronId)) this.ids.set(electronId, this.next++);
    return this.ids.get(electronId);
  }

  // Snapshot of the current displays, each with its capture source.
  async list() {
    const displays = orderedDisplays();
    const sources = matchSources(displays, await screenSources());
    const primary = screen.getPrimaryDisplay().id;
    const sizes = monitors.sizesFor(displays.map(d => ({
      key: d.id, label: d.label,
      w: Math.round(d.bounds.width * d.scaleFactor), h: Math.round(d.bounds.height * d.scaleFactor),
    })));
    return displays.map(d => {
      const id = this.idFor(d.id);
      return {
        id,
        electronId: d.id,
        name: `Display ${id}`,
        primary: d.id === primary,
        x: d.bounds.x, y: d.bounds.y, width: d.bounds.width, height: d.bounds.height,
        scaleFactor: d.scaleFactor,
        ...sizes.get(d.id),
        capture: captureSize(d),
        sourceId: sources.get(d.id) || null,
      };
    });
  }
}

// The display object as the protocol sends it.
const wireDisplay = d => ({
  id: d.id, name: d.name, primary: d.primary, x: d.x, y: d.y, width: d.width, height: d.height,
  widthMm: d.widthMm, heightMm: d.heightMm, sizeSource: d.sizeSource,
});

module.exports = { DisplayBook, wireDisplay, captureSize };
