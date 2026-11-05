// Typing from the headset (protocol: "Typing"): text and a few named keys, typed into whatever window has focus on
// this PC through Windows' SendInput, by one PowerShell helper kept open while typing is used. Nothing typed is
// logged or kept. Windows itself keeps it out of windows running as administrator (UIPI).
'use strict';

const { spawn } = require('child_process');

const KEYS = { Backspace: 0x08, Tab: 0x09, Enter: 0x0D, Escape: 0x1B, Left: 0x25, Up: 0x26, Right: 0x27, Down: 0x28, Delete: 0x2E };

// INPUT is 40 bytes on x64: type, then KEYBDINPUT at offset 8 (the union is sized by MOUSEINPUT).
const HELPER = `
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class NwKeys {
  [StructLayout(LayoutKind.Explicit, Size = 40)]
  struct INPUT {
    [FieldOffset(0)] public uint type;
    [FieldOffset(8)] public ushort vk;
    [FieldOffset(10)] public ushort scan;
    [FieldOffset(12)] public uint flags;
    [FieldOffset(16)] public uint time;
    [FieldOffset(24)] public IntPtr extra;
  }
  [DllImport("user32.dll", SetLastError = true)] static extern uint SendInput(uint n, INPUT[] inputs, int size);
  const uint KEYUP = 2, UNICODE = 4;
  static void Send(ushort vk, ushort scan, uint flags) {
    var a = new INPUT[2];
    a[0].type = 1; a[0].vk = vk; a[0].scan = scan; a[0].flags = flags;
    a[1].type = 1; a[1].vk = vk; a[1].scan = scan; a[1].flags = flags | KEYUP;
    SendInput(2, a, Marshal.SizeOf(typeof(INPUT)));
  }
  public static void Text(string s) {
    foreach (char c in s) {
      if (c == '\\n') Send(0x0D, 0, 0);
      else if (c == '\\t') Send(0x09, 0, 0);
      else if (c >= ' ') Send(0, c, UNICODE);
    }
  }
  public static void Key(int vk, int times) { for (int i = 0; i < times; i++) Send((ushort)vk, 0, 0); }
}
'@
$in = [Console]::In
while (($line = $in.ReadLine()) -ne $null) {
  try {
    $m = $line | ConvertFrom-Json
    if ($m.t) { [NwKeys]::Text([string]$m.t) } elseif ($m.k) { [NwKeys]::Key([int]$m.k, [int]$m.n) }
  } catch { }
}
`;

let helper = null;

function start() {
  if (helper && !helper.killed && helper.exitCode === null) return helper;
  const encoded = Buffer.from(HELPER, 'utf16le').toString('base64');
  helper = spawn('powershell.exe', ['-NoProfile', '-NonInteractive', '-ExecutionPolicy', 'Bypass', '-WindowStyle', 'Hidden', '-EncodedCommand', encoded],
    { stdio: ['pipe', 'ignore', 'ignore'], windowsHide: true });
  helper.on('error', () => { helper = null; });
  helper.on('exit', () => { helper = null; });
  return helper;
}

function send(line) {
  const h = start();
  if (h && h.stdin.writable) h.stdin.write(JSON.stringify(line) + '\n');
}

// One message from the headset: {"type":"text","text":"..."} or {"type":"key","key":"Backspace","times":1}.
// Returns false for anything else.
function handle(raw) {
  let m;
  try { m = JSON.parse(raw); } catch { return false; }
  if (!m || typeof m !== 'object') return false;
  if (m.type === 'text' && typeof m.text === 'string' && m.text.length > 0 && m.text.length <= 1000) { send({ t: m.text }); return true; }
  if (m.type === 'key' && Object.prototype.hasOwnProperty.call(KEYS, m.key)) {
    const times = Math.max(1, Math.min(200, Number(m.times) | 0 || 1));
    send({ k: KEYS[m.key], n: times });
    return true;
  }
  return false;
}

function stop() {
  if (helper) { try { helper.stdin.end(); helper.kill(); } catch { } }
  helper = null;
}

module.exports = { handle, stop, KEYS };
