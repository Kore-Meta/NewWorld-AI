// Bridge for the visible window: state in, user actions out.
'use strict';

const { contextBridge, ipcRenderer } = require('electron');

contextBridge.exposeInMainWorld('desktop', {
  onState(handler) { ipcRenderer.on('ui:state', (_e, state) => handler(state)); },
  action(name, args) { return ipcRenderer.invoke('ui:action', name, args); },
});
