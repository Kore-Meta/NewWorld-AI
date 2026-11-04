// New World Desktop: shares this PC's displays with New World on Meta Quest, over the local network only.
// Main process: window, tray, discovery beacons, the signalling server and the session state the UI shows.
'use strict';

const { app, BrowserWindow, Tray, Menu, nativeImage, ipcMain, screen, session: electronSession, dialog } = require('electron');
const path = require('path');
const fs = require('fs');
const os = require('os');
const { Beacon } = require('./discovery');
const { SignalServer, MAX_TRIES } = require('./signalling');
const { DisplayBook, wireDisplay } = require('./displays');
const monitors = require('./monitors');

// Plain LAN host candidates instead of mDNS names: the headset resolves nothing, it just connects to 192.168.x.y.
app.commandLine.appendSwitch('disable-features', 'WebRtcHideLocalIpsWithMdns');

const COMPUTER_NAME = os.hostname();
const ASSETS = path.join(__dirname, 'assets');

let uiWin = null, engineWin = null, tray = null, quitting = false;
let beacon = null, server = null, current = null;   // current: the live signalling Session
const book = new DisplayBook();

// Everything the UI draws. Pushed whole on every change.
const state = {
  phase: 'idle',            // idle | code | approve | sharing
  computerName: COMPUTER_NAME,
  discoverable: true,
  smoothMotion: false,      // 60 fps instead of 30; costs headset battery and frame rate
  device: '',
  triesLeft: MAX_TRIES,
  codeError: '',
  displays: [],             // approve: all displays, with layout
  shared: [],               // sharing: { id, name, width, height, primary, stopped, stats }
  connection: '',           // sharing: RTCPeerConnection state
  prompts: [],              // sharing: new monitors waiting for a yes/no { electronId, id, name, width, height }
  notice: '',               // a one-line message on the idle screen (why the last session ended)
  error: '',
};

// ---------- settings (the two switches; kept in the user's app data folder) ----------

const settingsFile = () => path.join(app.getPath('userData'), 'settings.json');
function loadSettings() {
  try {
    const s = JSON.parse(fs.readFileSync(settingsFile(), 'utf8'));
    state.discoverable = s.discoverable !== false;
    state.smoothMotion = s.smoothMotion === true;
  } catch { }
}
function saveSettings() {
  try { fs.writeFileSync(settingsFile(), JSON.stringify({ discoverable: state.discoverable, smoothMotion: state.smoothMotion })); } catch { }
}

// ---------- UI plumbing ----------

function push() {
  updateBeacon();
  updateTray();
  if (uiWin && !uiWin.isDestroyed()) uiWin.webContents.send('ui:state', state);
}

function showWindow() {
  if (!uiWin || uiWin.isDestroyed()) createUiWindow();
  if (uiWin.isMinimized()) uiWin.restore();
  uiWin.show();
  uiWin.focus();
}

function createUiWindow() {
  uiWin = new BrowserWindow({
    width: 560, height: 720, minWidth: 460, minHeight: 600,
    title: 'New World Desktop',
    backgroundColor: '#f6f1e7',
    icon: path.join(ASSETS, 'icon.png'),
    autoHideMenuBar: true,
    show: false,
    webPreferences: {
      preload: path.join(__dirname, 'preload-ui.js'),
      contextIsolation: true, sandbox: true, nodeIntegration: false, spellcheck: false,
    },
  });
  uiWin.setMenu(null);
  uiWin.loadFile(path.join(__dirname, 'ui', 'index.html'));
  uiWin.once('ready-to-show', () => uiWin.show());
  uiWin.webContents.on('did-finish-load', push);
  // Closing the window keeps the app (and any sharing) running in the tray.
  uiWin.on('close', e => { if (!quitting) { e.preventDefault(); uiWin.hide(); } });
}

// ---------- engine (hidden renderer doing capture + WebRTC) ----------

let engineReady = null, reqSeq = 0;
const pending = new Map();

function createEngineWindow() {
  engineWin = new BrowserWindow({
    show: false, width: 200, height: 200,
    webPreferences: {
      preload: path.join(__dirname, 'preload-engine.js'),
      contextIsolation: true, sandbox: true, nodeIntegration: false, spellcheck: false,
      backgroundThrottling: false,   // hidden, but it must keep encoding at full rate
    },
  });
  engineReady = new Promise(resolve => ipcMain.once('engine:ready', resolve));
  engineWin.loadFile(path.join(__dirname, 'engine', 'engine.html'));
  engineWin.webContents.on('render-process-gone', (_e, details) => {
    console.error('[engine] gone', details.reason);
    for (const p of pending.values()) p.reject(new Error('The capture engine stopped.'));
    pending.clear();
    if (current) current.stop('The computer stopped capturing.');
    if (!quitting) createEngineWindow();
  });
}

async function engine(cmd, args, timeoutMs = 20000) {
  await engineReady;
  const reqId = ++reqSeq;
  return new Promise((resolve, reject) => {
    const t = setTimeout(() => { pending.delete(reqId); reject(new Error(`Engine ${cmd} timed out`)); }, timeoutMs);
    pending.set(reqId, { resolve: v => { clearTimeout(t); resolve(v); }, reject: e => { clearTimeout(t); reject(e); } });
    engineWin.webContents.send('engine:cmd', { reqId, cmd, args });
  });
}

ipcMain.on('engine:reply', (e, { reqId, result, error }) => {
  if (!engineWin || e.sender !== engineWin.webContents) return;
  const p = pending.get(reqId);
  if (!p) return;
  pending.delete(reqId);
  error ? p.reject(new Error(error)) : p.resolve(result);
});

ipcMain.on('engine:event', (e, { name, data }) => {
  if (!engineWin || e.sender !== engineWin.webContents || state.phase !== 'sharing') return;
  if (name === 'connection') {
    state.connection = data.state || '';
    if (data.state === 'failed' && current) current.stop('The video connection failed. Check that the computer and the headset are on the same Wi-Fi.');
    push();
  } else if (name === 'stats') {
    for (const s of state.shared) s.stats = data.find(x => x.id === s.id) || null;
    push();
  } else if (name === 'track-ended') {
    stopDisplay(data.id, { tellHeadset: true });
  }
});

// ---------- discovery ----------

function updateBeacon() {
  if (!beacon) return;
  const want = state.discoverable && state.phase === 'idle' && !!server;
  if (want && !beacon.running) beacon.start();
  if (!want && beacon.running) beacon.stop();
}

// ---------- session flow ----------

function resetSessionState() {
  Object.assign(state, { phase: 'idle', device: '', triesLeft: MAX_TRIES, codeError: '', displays: [], shared: [], connection: '', prompts: [], error: '' });
}

function onSession(s) {
  current = s;
  book.reset();
  resetSessionState();
  Object.assign(state, { phase: 'code', device: s.device, notice: '' });
  s.on('answer', sdp => engine('setAnswer', { sdp }).catch(err => s.stop(`The computer could not start the video (${err.message}).`)));
  s.on('stop-display', id => stopDisplay(id, { tellHeadset: false }));
  s.on('ended', ({ reason, wasState }) => {
    if (current !== s) return;
    current = null;
    engine('stopAll').catch(() => { });
    resetSessionState();
    state.notice = wasState === 'code' && /disconnected/.test(reason) ? 'The headset stopped asking to connect.' : reason;
    push();
  });
  push();
  showWindow();
}

async function refreshApproveDisplays() {
  state.displays = (await book.list()).map(d => ({ ...wireDisplay(d), scaleFactor: d.scaleFactor, capture: d.capture, capturable: !!d.sourceId }));
}

async function share(ids) {
  if (!current || current.state !== 'approve') return;
  await monitors.settled();   // the sizes in "approved" come from the EDID read
  const all = await book.list();
  const chosen = all.filter(d => ids.includes(d.id) && d.sourceId);
  if (!chosen.length) { state.error = 'Choose at least one display to share.'; push(); return; }
  const s = current;
  if (!s.approve(COMPUTER_NAME, chosen.map(wireDisplay))) return;
  Object.assign(state, {
    phase: 'sharing', connection: 'new', error: '',
    shared: chosen.map(d => ({ ...wireDisplay(d), capture: d.capture, stopped: false, stats: null })),
  });
  push();
  try {
    const { sdp, mids } = await engine('start', { displays: chosen, smooth: state.smoothMotion });
    s.sendOffer(sdp, mids);
  } catch (err) {
    s.stop(`The computer could not start capturing (${err.message}).`);
  }
}

async function stopDisplay(id, { tellHeadset }) {
  const row = state.shared.find(d => d.id === id);
  if (!row || row.stopped) return;
  row.stopped = true;
  row.stats = null;
  push();
  await engine('remove', { id }).catch(() => { });
  if (tellHeadset && current) current.sendDisplayRemoved(id);
}

// Adds a display mid-session, only after the user said yes on this computer.
async function addDisplay(electronId) {
  if (!current || current.state !== 'sharing') return;
  const d = (await book.list()).find(x => x.electronId === electronId);
  if (!d || !d.sourceId) { state.error = 'That display can no longer be captured.'; push(); return; }
  let row = state.shared.find(x => x.id === d.id);
  if (row) Object.assign(row, wireDisplay(d), { capture: d.capture, stopped: false, stats: null });
  else state.shared.push(row = { ...wireDisplay(d), capture: d.capture, stopped: false, stats: null });
  push();
  try {
    const { sdp, mids } = await engine('add', { display: d });
    current && current.sendDisplayAdded(wireDisplay(d), sdp, mids);
  } catch (err) {
    row.stopped = true;
    state.error = `Could not share ${d.name} (${err.message}).`;
    push();
  }
}

async function onDisplayAdded(_e, display) {
  await monitors.refresh();
  if (state.phase === 'approve') { await refreshApproveDisplays(); push(); return; }
  if (state.phase !== 'sharing') return;
  const d = (await book.list()).find(x => x.electronId === display.id);
  if (!d || state.prompts.some(p => p.electronId === display.id)) return;
  state.prompts.push({ electronId: d.electronId, id: d.id, name: d.name, width: d.width, height: d.height });
  push();
  showWindow();
}

async function onDisplayRemoved(_e, display) {
  monitors.refresh();
  state.prompts = state.prompts.filter(p => p.electronId !== display.id);
  if (state.phase === 'approve') { await refreshApproveDisplays(); push(); return; }
  if (state.phase !== 'sharing') { push(); return; }
  const id = book.ids.get(display.id);
  if (id != null) await stopDisplay(id, { tellHeadset: true });
  push();
}

// ---------- UI actions ----------

const actions = {
  async submitCode({ code }) {
    if (!current || state.phase !== 'code') return;
    const r = current.tryCode(code);
    if (r.ok) {
      Object.assign(state, { phase: 'approve', codeError: '' });
      await refreshApproveDisplays();
    } else if (!r.declined) {
      state.triesLeft = r.triesLeft;
      state.codeError = `That code doesn't match the one in the headset. ${r.triesLeft} ${r.triesLeft === 1 ? 'try' : 'tries'} left.`;
    }
    push();
  },
  decline() { if (current) current.decline(); },
  share({ ids }) { return share((ids || []).map(Number)); },
  stopSharing() { if (current) current.stop(); },
  stopDisplay({ id }) { return stopDisplay(Number(id), { tellHeadset: true }); },
  reshareDisplay({ id }) {
    const electronId = [...book.ids].find(([, v]) => v === Number(id))?.[0];
    if (electronId != null) return addDisplay(electronId);
  },
  async answerPrompt({ electronId, yes }) {
    state.prompts = state.prompts.filter(p => p.electronId !== electronId);
    push();
    if (yes) await addDisplay(electronId);
  },
  setDiscoverable({ on }) { state.discoverable = !!on; saveSettings(); push(); },
  setSmoothMotion({ on }) { state.smoothMotion = !!on; saveSettings(); push(); },
  dismissNotice() { state.notice = ''; push(); },
};

ipcMain.handle('ui:action', async (e, name, args) => {
  if (!uiWin || e.sender !== uiWin.webContents || !actions[name]) return;
  await actions[name](args || {});
});

// ---------- tray ----------

function trayImage(sharing) {
  return nativeImage.createFromPath(path.join(ASSETS, sharing ? 'tray-sharing.png' : 'tray.png'));
}

function updateTray() {
  if (!tray) return;
  const sharing = state.phase === 'sharing';
  const live = state.shared.filter(d => !d.stopped).length;
  tray.setImage(trayImage(sharing));
  tray.setToolTip(sharing ? `New World Desktop: sharing ${live} display${live === 1 ? '' : 's'} with ${state.device}` : 'New World Desktop');
  tray.setContextMenu(Menu.buildFromTemplate([
    { label: sharing ? `Sharing ${live} display${live === 1 ? '' : 's'} with ${state.device}` : 'Not sharing', enabled: false },
    { label: 'Stop sharing', enabled: sharing, click: () => actions.stopSharing() },
    { type: 'separator' },
    { label: 'Open New World Desktop', click: showWindow },
    { label: 'Quit', click: () => app.quit() },
  ]));
}

// ---------- app ----------

if (!app.requestSingleInstanceLock()) {
  app.quit();
} else {
  app.on('second-instance', showWindow);
  app.setAppUserModelId('com.koremeta.newworld.desktop');

  app.whenReady().then(async () => {
    loadSettings();
    monitors.refresh();   // physical sizes from the EDID, read in the background
    // Screen capture for the hidden engine only (Chromium asks for desktop getUserMedia as 'display-capture');
    // every other permission is refused.
    const allowed = new Set(['media', 'display-capture']);
    const isEngine = wc => !!(engineWin && wc && wc === engineWin.webContents);
    electronSession.defaultSession.setPermissionRequestHandler((wc, perm, cb) => cb(allowed.has(perm) && isEngine(wc)));
    electronSession.defaultSession.setPermissionCheckHandler((wc, perm) => allowed.has(perm) && isEngine(wc));
    app.on('web-contents-created', (_e, wc) => {
      wc.setWindowOpenHandler(() => ({ action: 'deny' }));
      wc.on('will-navigate', e => e.preventDefault());
    });

    createEngineWindow();
    createUiWindow();
    tray = new Tray(trayImage(false));
    tray.on('click', showWindow);

    beacon = new Beacon(COMPUTER_NAME);
    server = new SignalServer();
    server.on('session', onSession);
    try {
      await server.listen();
    } catch (err) {
      server = null;
      state.error = err.code === 'EADDRINUSE'
        ? 'Port 47801 is already in use, so headsets cannot connect. Is New World Desktop already running?'
        : `Headsets cannot connect: ${err.message}`;
    }
    screen.on('display-added', onDisplayAdded);
    screen.on('display-removed', onDisplayRemoved);
    push();

    const codecs = await engine('codecs').catch(() => []);
    if (!codecs.some(c => /h264/i.test(c))) {
      state.error = 'This computer has no H.264 video encoder available, which the headset needs.';
      push();
    }
  });

  app.on('window-all-closed', () => { /* stay in the tray */ });
  app.on('before-quit', () => {
    quitting = true;
    if (current) current.stop('The computer closed New World Desktop.');
    if (beacon) beacon.stop();
    if (server) server.close();
  });
}

// A tray app that dies quietly looks like it is still sharing; show the error instead.
process.on('uncaughtException', err => {
  console.error(err);
  if (app.isReady() && !quitting) dialog.showErrorBox('New World Desktop', String(err && err.stack || err));
});
