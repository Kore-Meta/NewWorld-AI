'use strict';
const { contextBridge, ipcRenderer } = require('electron');
contextBridge.exposeInMainWorld('bridge', {
  onMessage: fn => ipcRenderer.on('in', (_e, msg) => fn(msg)),
  send: msg => ipcRenderer.send('out', msg),
});
