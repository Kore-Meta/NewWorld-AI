// Bridge for the hidden capture/WebRTC renderer: commands in, replies and events out.
'use strict';

const { contextBridge, ipcRenderer } = require('electron');

contextBridge.exposeInMainWorld('engine', {
  onCommand(handler) {
    ipcRenderer.on('engine:cmd', async (_e, { reqId, cmd, args }) => {
      try {
        ipcRenderer.send('engine:reply', { reqId, result: await handler(cmd, args) });
      } catch (err) {
        ipcRenderer.send('engine:reply', { reqId, error: String((err && err.message) || err) });
      }
    });
    ipcRenderer.send('engine:ready');
  },
  event(name, data) { ipcRenderer.send('engine:event', { name, data }); },
});
