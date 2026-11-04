// The window: draws the state the main process pushes and sends the user's choices back.
'use strict';

const $ = id => document.getElementById(id);
const SVG = 'http://www.w3.org/2000/svg';
let state = null;
let phase = null;
let lastTries = null;
const ticked = new Set();      // approve: displays the user wants to share
const seen = new Set();        // approve: displays already offered (new ones arrive ticked)

const plural = (n, one, many) => `${n} ${n === 1 ? one : many}`;
const res = d => `${d.width} × ${d.height}`;
function el(tag, props = {}, ...kids) {
  const e = Object.assign(document.createElement(tag), props);
  e.append(...kids.filter(k => k != null));
  return e;
}

window.desktop.onState(s => { state = s; render(); });

function render() {
  const s = state;
  if (s.phase !== phase) enter(s.phase);
  phase = s.phase;
  for (const p of ['idle', 'code', 'approve', 'sharing']) $(`screen-${p}`).hidden = s.phase !== p;
  for (const n of document.querySelectorAll('.device-name')) n.textContent = s.device;
  $('error').hidden = !s.error;
  $('error').textContent = s.error;

  if (s.phase === 'idle') renderIdle(s);
  if (s.phase === 'code') renderCode(s);
  if (s.phase === 'approve') renderApprove(s);
  if (s.phase === 'sharing') renderSharing(s);
  renderPrompt(s);
}

function enter(p) {
  if (p === 'code') { clearCode(); lastTries = null; setTimeout(() => boxes()[0].focus(), 50); }
  if (p === 'approve') { ticked.clear(); seen.clear(); }
}

// ---------- idle ----------

function renderIdle(s) {
  $('computer-name').textContent = s.computerName;
  $('discoverable').checked = s.discoverable;
  $('smooth-motion').checked = s.smoothMotion;
  $('notice').hidden = !s.notice;
  $('notice-text').textContent = s.notice;
}
$('discoverable').onchange = e => window.desktop.action('setDiscoverable', { on: e.target.checked });
$('smooth-motion').onchange = e => window.desktop.action('setSmoothMotion', { on: e.target.checked });
$('notice-close').onclick = () => window.desktop.action('dismissNotice');

// ---------- code ----------

const boxes = () => [...document.querySelectorAll('#code-boxes input')];
const codeValue = () => boxes().map(b => b.value).join('');
function clearCode() { for (const b of boxes()) b.value = ''; $('code-submit').disabled = true; }

function renderCode(s) {
  const wrong = lastTries !== null && s.triesLeft < lastTries;
  lastTries = s.triesLeft;
  $('code-error').hidden = !s.codeError;
  $('code-error').textContent = s.codeError;
  if (wrong) {
    const row = $('code-boxes');
    row.classList.remove('wrong'); void row.offsetWidth; row.classList.add('wrong');
    clearCode();
    boxes()[0].focus();
  }
}

boxes().forEach((b, i, all) => {
  b.addEventListener('input', () => {
    $('code-boxes').classList.remove('wrong');
    const digits = b.value.replace(/\D/g, '');
    if (digits.length > 1) { fill(digits, i); return; }
    b.value = digits;
    if (digits && i < all.length - 1) all[i + 1].focus();
    $('code-submit').disabled = codeValue().length !== 6;
    if (codeValue().length === 6) $('code-form').requestSubmit();
  });
  b.addEventListener('keydown', e => {
    if (e.key === 'Backspace' && !b.value && i > 0) { all[i - 1].value = ''; all[i - 1].focus(); e.preventDefault(); }
    if (e.key === 'ArrowLeft' && i > 0) all[i - 1].focus();
    if (e.key === 'ArrowRight' && i < all.length - 1) all[i + 1].focus();
  });
  b.addEventListener('paste', e => { e.preventDefault(); fill((e.clipboardData.getData('text') || '').replace(/\D/g, ''), 0); });
  b.addEventListener('focus', () => b.select());
});

function fill(digits, from) {
  const all = boxes();
  for (let k = 0; k < digits.length && from + k < all.length; k++) all[from + k].value = digits[k];
  all[Math.min(from + digits.length, all.length - 1)].focus();
  $('code-submit').disabled = codeValue().length !== 6;
  if (codeValue().length === 6) $('code-form').requestSubmit();
}

$('code-form').onsubmit = e => {
  e.preventDefault();
  if (codeValue().length !== 6) return;
  $('code-submit').disabled = true;
  window.desktop.action('submitCode', { code: codeValue() });
};
$('code-decline').onclick = () => window.desktop.action('decline');

// ---------- approve ----------

function renderApprove(s) {
  for (const d of s.displays) {
    if (!seen.has(d.id)) { seen.add(d.id); if (d.capturable) ticked.add(d.id); }
    if (!d.capturable) ticked.delete(d.id);
  }
  for (const id of [...ticked]) if (!s.displays.some(d => d.id === id)) ticked.delete(id);
  drawMap($('map'), s.displays, d => ticked.has(d.id), toggle);

  $('display-list').replaceChildren(...s.displays.map(d => {
    const box = el('input', { type: 'checkbox', checked: ticked.has(d.id), disabled: !d.capturable });
    box.onchange = () => toggle(d);
    const detail = d.capturable
      ? `${res(d.capture)}${d.scaleFactor !== 1 ? ` · ${Math.round(d.scaleFactor * 100)}%` : ''}`
      : 'Cannot be captured';
    return el('li', {}, el('label', {}, box,
      el('span', { className: 'name', textContent: d.name }),
      d.primary ? el('span', { className: 'tag', textContent: 'Main' }) : null,
      el('span', { className: 'meta', textContent: detail })));
  }));
  const n = ticked.size;
  $('approve-share').disabled = n === 0;
  $('approve-share').textContent = n === s.displays.length && n > 1 ? `Share all ${n}` : `Share ${plural(n, 'display', 'displays')}`;
}

function toggle(d) {
  if (!d.capturable) return;
  ticked.has(d.id) ? ticked.delete(d.id) : ticked.add(d.id);
  renderApprove(state);
}

// The displays as Windows arranges them, scaled into the box.
function drawMap(svg, displays, isOn, onClick) {
  svg.replaceChildren();
  if (!displays.length) return;
  const W = svg.clientWidth || 480, H = svg.clientHeight || 150, pad = 16;
  const x0 = Math.min(...displays.map(d => d.x)), y0 = Math.min(...displays.map(d => d.y));
  const x1 = Math.max(...displays.map(d => d.x + d.width)), y1 = Math.max(...displays.map(d => d.y + d.height));
  const k = Math.min((W - 2 * pad) / (x1 - x0), (H - 2 * pad) / (y1 - y0));
  const ox = (W - (x1 - x0) * k) / 2, oy = (H - (y1 - y0) * k) / 2;
  svg.setAttribute('viewBox', `0 0 ${W} ${H}`);
  for (const d of displays) {
    const g = document.createElementNS(SVG, 'g');
    g.setAttribute('class', isOn(d) ? 'on' : 'off');
    const r = document.createElementNS(SVG, 'rect');
    const x = ox + (d.x - x0) * k + 2, y = oy + (d.y - y0) * k + 2, w = d.width * k - 4, h = d.height * k - 4;
    Object.entries({ x, y, width: w, height: h, rx: 6, 'stroke-width': 1.5 }).forEach(([a, v]) => r.setAttribute(a, v));
    r.addEventListener('click', () => onClick(d));
    const t = document.createElementNS(SVG, 'text');
    Object.entries({ x: x + w / 2, y: y + h / 2 + 5, 'text-anchor': 'middle' }).forEach(([a, v]) => t.setAttribute(a, v));
    t.textContent = String(d.id);
    g.append(r, t);
    if (d.primary && h > 44) {
      const m = document.createElementNS(SVG, 'text');
      Object.entries({ x: x + w / 2, y: y + h / 2 + 22, 'text-anchor': 'middle', class: 'star' }).forEach(([a, v]) => m.setAttribute(a, v));
      m.textContent = 'MAIN';
      g.append(m);
    }
    svg.append(g);
  }
}
window.addEventListener('resize', () => { if (state && state.phase === 'approve') renderApprove(state); });

$('approve-share').onclick = () => { $('approve-share').disabled = true; window.desktop.action('share', { ids: [...ticked] }); };
$('approve-decline').onclick = () => window.desktop.action('decline');

// ---------- sharing ----------

const CONNECTION_TEXT = {
  new: 'Waiting for the headset to accept the video…', connecting: 'Connecting to the headset over your Wi-Fi…',
  connected: 'Connected directly over your Wi-Fi, encrypted.', disconnected: 'The Wi-Fi connection dropped. Trying to recover…',
};

function renderSharing(s) {
  const live = s.shared.filter(d => !d.stopped).length;
  $('live-text').textContent = `Sharing ${plural(live, 'display', 'displays')} with ${s.device}`;
  $('connection-text').textContent = CONNECTION_TEXT[s.connection] || (s.connection ? `Connection: ${s.connection}` : '');
  $('shared-list').replaceChildren(...s.shared.map(d => {
    const st = d.stats;
    const meta = d.stopped ? 'Not shared' : st && st.width ? `${st.width} × ${st.height} · ${st.fps} fps` : `${res(d.capture)}`;
    const btn = d.stopped
      ? el('button', { className: 'quiet small', textContent: 'Share again', onclick: () => window.desktop.action('reshareDisplay', { id: d.id }) })
      : el('button', { className: 'quiet small', textContent: 'Stop', onclick: () => window.desktop.action('stopDisplay', { id: d.id }) });
    return el('li', { className: d.stopped ? 'stopped' : '' },
      el('span', { className: 'state' }),
      el('span', { className: 'grow' },
        el('span', { className: 'name', textContent: d.name }),
        el('span', { className: 'meta', textContent: meta })),
      btn);
  }));
}
$('stop-sharing').onclick = () => window.desktop.action('stopSharing');

// ---------- new monitor prompt ----------

function renderPrompt(s) {
  const p = s.phase === 'sharing' && s.prompts[0];
  $('prompt').hidden = !p;
  if (!p) return;
  $('prompt-title').textContent = `Share ${p.name} too?`;
  $('prompt-body').textContent = `A ${res(p)} display was just connected to this computer. It is not shared until you say so.`;
  $('prompt-yes').onclick = () => window.desktop.action('answerPrompt', { electronId: p.electronId, yes: true });
  $('prompt-no').onclick = () => window.desktop.action('answerPrompt', { electronId: p.electronId, yes: false });
}
