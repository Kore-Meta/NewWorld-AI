// A stand-in for the headset's WebRTC side, used by `npm run fake-headset -- <code> --receive`.
// Talks JSON lines with the fake headset over a loopback socket (Electron's main process on Windows cannot read a
// piped stdin): takes offers, answers after gathering (no trickle), shows the streams and reports stats.
'use strict';

const { app, BrowserWindow, ipcMain } = require('electron');
const net = require('net');
const path = require('path');
const P = require('../../src/protocol');

app.commandLine.appendSwitch('disable-features', 'WebRtcHideLocalIpsWithMdns');
const port = Number(process.argv[process.argv.length - 1]);

app.whenReady().then(() => {
  const win = new BrowserWindow({
    width: 1100, height: 560, title: 'Fake headset receiver', backgroundColor: '#1d2424',
    webPreferences: { preload: path.join(__dirname, 'preload.js'), contextIsolation: true, sandbox: true },
  });
  win.setMenu(null);
  win.loadFile(path.join(__dirname, 'index.html'));
  win.webContents.once('did-finish-load', () => {
    const link = net.connect(port, '127.0.0.1');
    link.on('error', () => app.quit());
    link.on('close', () => app.quit());
    P.attachLineReader(link, msg => win.webContents.send('in', msg));
    ipcMain.on('out', (_e, msg) => P.send(link, msg));
  });
});
app.on('window-all-closed', () => app.quit());
