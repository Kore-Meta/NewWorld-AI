// Physical monitor sizes from the EDID, read through WMI (local only, no network), so the headset can draw each
// screen at its real size. Matched to Electron displays by friendly name, then by native resolution.
'use strict';

const { execFile } = require('child_process');

const PPI_ESTIMATE = 110;
const MIN_WIDTH_MM = 150, MAX_WIDTH_MM = 1600;

// One line of JSON per run: every monitor's name, preferred mode and image size. The preferred mode's image size
// is in mm (EDID detailed timing); WmiMonitorBasicDisplayParams only has whole cm, so it is the fallback.
const SCRIPT = `
$ErrorActionPreference = 'SilentlyContinue'
$out = @{}
foreach ($b in Get-CimInstance -Namespace root\\wmi -ClassName WmiMonitorBasicDisplayParams) {
  $out[$b.InstanceName] = [ordered]@{ instance = $b.InstanceName; active = [bool]$b.Active; name = '';
    px = 0; py = 0; wmm = 10 * [int]$b.MaxHorizontalImageSize; hmm = 10 * [int]$b.MaxVerticalImageSize }
}
foreach ($i in Get-CimInstance -Namespace root\\wmi -ClassName WmiMonitorID) {
  if ($out.Contains($i.InstanceName)) { $out[$i.InstanceName].name = (($i.UserFriendlyName | Where-Object { $_ -ne 0 } | ForEach-Object { [char]$_ }) -join '') }
}
foreach ($s in Get-CimInstance -Namespace root\\wmi -ClassName WmiMonitorListedSupportedSourceModes) {
  if (-not $out.Contains($s.InstanceName)) { continue }
  $m = $s.MonitorSourceModes[$s.PreferredMonitorSourceModeIndex]
  $o = $out[$s.InstanceName]
  $o.px = [int]$m.HorizontalActivePixels; $o.py = [int]$m.VerticalActivePixels
  if ($m.HorizontalImageSize -gt 0 -and $m.VerticalImageSize -gt 0) { $o.wmm = [int]$m.HorizontalImageSize; $o.hmm = [int]$m.VerticalImageSize }
}
ConvertTo-Json -Compress -InputObject @($out.Values)
`;

let monitors = [];   // last good WMI read
let reading = null;

// Re-reads the EDID sizes (at startup and on display changes). Never rejects: on failure the old list stays.
function refresh(timeoutMs = 8000) {
  if (reading) return reading;
  reading = new Promise(resolve => {
    execFile('powershell.exe', ['-NoProfile', '-NonInteractive', '-ExecutionPolicy', 'Bypass', '-Command', SCRIPT],
      { timeout: timeoutMs, windowsHide: true }, (err, stdout) => {
        if (!err) {
          try {
            const list = JSON.parse(stdout.trim() || '[]');
            monitors = (Array.isArray(list) ? list : [list]).filter(m => m && m.active !== false);
          } catch { }
        }
        reading = null;
        resolve(monitors);
      });
  });
  return reading;
}

// Waits for a read already under way (the startup one), without starting another.
const settled = () => reading || Promise.resolve(monitors);

const long = (a, b) => Math.max(a, b), short = (a, b) => Math.min(a, b);

// The EDID size is for the panel's landscape orientation; a rotated display gets it swapped.
function orient(m, w, h) {
  return (w >= h) === (m.wmm >= m.hmm) ? { w: m.wmm, h: m.hmm } : { w: m.hmm, h: m.wmm };
}

function estimate(w, h) {
  return { widthMm: Math.round(w / PPI_ESTIMATE * 25.4), heightMm: Math.round(h / PPI_ESTIMATE * 25.4), sizeSource: 'estimate' };
}

// Width clamped to 150-1600 mm; the height follows the display's pixel aspect when the width had to move.
function clamp(size, w, h) {
  if (size.widthMm >= MIN_WIDTH_MM && size.widthMm <= MAX_WIDTH_MM) return size;
  const widthMm = Math.min(MAX_WIDTH_MM, Math.max(MIN_WIDTH_MM, size.widthMm));
  return { ...size, widthMm, heightMm: Math.round(widthMm * h / w) };
}

// sizes for [{ key, label, w, h }] (native pixels) -> Map key -> { widthMm, heightMm, sizeSource }.
// Name match first (Electron's label is the EDID friendly name on Windows), then native resolution, then aspect,
// each taking the remaining monitors in order.
function sizesFor(displays) {
  const free = monitors.filter(m => m.wmm > 0 && m.hmm > 0);
  const take = test => d => { const i = free.findIndex(m => test(m, d)); return i >= 0 ? free.splice(i, 1)[0] : null; };
  const byName = take((m, d) => m.name && d.label && m.name.trim() === d.label.trim());
  const byRes = take((m, d) => m.px && long(m.px, m.py) === long(d.w, d.h) && short(m.px, m.py) === short(d.w, d.h));
  const byAspect = take((m, d) => Math.abs(long(m.wmm, m.hmm) / short(m.wmm, m.hmm) - long(d.w, d.h) / short(d.w, d.h)) < 0.05);

  const found = new Map();
  for (const pass of [byName, byRes, byAspect])
    for (const d of displays) if (!found.has(d.key)) { const m = pass(d); if (m) found.set(d.key, m); }

  const out = new Map();
  for (const d of displays) {
    const m = found.get(d.key);
    let size;
    if (m) { const o = orient(m, d.w, d.h); size = { widthMm: o.w, heightMm: o.h, sizeSource: 'edid' }; }
    else size = estimate(d.w, d.h);
    out.set(d.key, clamp(size, d.w, d.h));
  }
  return out;
}

module.exports = { refresh, settled, sizesFor, estimate };
