using System;
using System.Collections.Generic;
using UnityEngine;

namespace StudyEverywhere
{
    // Stand-in for the desktop app, so the whole flow runs in the Editor and on Quest without a PC: it issues a code,
    // "approves" a few seconds later, then streams every monitor of a simulated three-monitor desk as a test pattern
    // (a faux desktop per display, with a moving sweep). The component's context menu simulates a stalled stream,
    // a monitor being attached or unplugged, and the computer stopping the share.
    public sealed class SimulatedScreenSource : MonoBehaviour, IScreenSource
    {
        [Serializable]
        public struct Monitor
        {
            public string name;
            public bool primary;
            [Tooltip("Position and size on the Windows desktop, logical units, y down.")]
            public RectInt layout;
        }

        public float approveAfter = 4f, connectAfter = 1.5f, codeLifetime = 120f;
        [Min(1)] public float framesPerSecond = 15f;
        [Tooltip("Desktop units per test-pattern pixel.")]
        [Min(1)] public int downscale = 3;
        // Main 1080p screen in the middle, a second 1080p to its left, a portrait screen to its right standing a little higher.
        public Monitor[] monitors =
        {
            new Monitor { name = "Display 1", primary = true, layout = new RectInt(0, 0, 1920, 1080) },
            new Monitor { name = "Display 2", layout = new RectInt(-1920, 0, 1920, 1080) },
            new Monitor { name = "Display 3", layout = new RectInt(1920, -420, 1080, 1920) },
        };
        [Tooltip("Monitor added by \"Simulate monitor attached\": an ultrawide above the main screen.")]
        public Monitor attachable = new Monitor { name = "Display 4", layout = new RectInt(-320, -1080, 2560, 1080) };
        [Tooltip("Display the stall menu item freezes; 0 freezes every display.")]
        public int stallDisplay = 2;
        [Tooltip("How long a simulated stall lasts, in seconds.")]
        public float stallSeconds = 4f;

        public event Action<string, float> CodeIssued;
        public event Action Approved;
        public event Action<SharedDisplay> DisplayAdded;
        public event Action<int> DisplayRemoved;
        public event Action<int> Frame;
        public event Action<string> Ended;

        enum Phase { Idle, Pairing, Connecting, Streaming }
        sealed class Stream
        {
            public Monitor monitor;
            public int id;
            public Texture2D texture;
            public Color32[] desktop, pixels;
            public int frame;
            public float stallUntil;
        }

        Phase phase;
        float timer, frameClock;
        readonly List<Stream> live = new List<Stream>();
        readonly Dictionary<int, Stream> streams = new Dictionary<int, Stream>();   // kept across sessions, textures reused

        public void Pair()
        {
            phase = Phase.Pairing;
            timer = 0;
            live.Clear();
            CodeIssued?.Invoke(UnityEngine.Random.Range(100000, 1000000).ToString(), codeLifetime);
        }

        public void Cancel() => Stop();
        public void Stop() { phase = Phase.Idle; live.Clear(); }
        public void StopDisplay(int id) => live.RemoveAll(s => s.id == id);

        [ContextMenu("Simulate stalled stream")]
        public void Stall() { foreach (var s in live) if (stallDisplay == 0 || s.id == stallDisplay) s.stallUntil = Time.time + stallSeconds; }

        [ContextMenu("Simulate monitor attached")]
        public void Attach()
        {
            int id = monitors.Length + 1;
            if (phase != Phase.Streaming || live.Exists(s => s.id == id)) return;
            Share(id, attachable);
        }

        [ContextMenu("Simulate monitor unplugged")]
        public void Unplug()
        {
            var last = live.FindLast(s => !s.monitor.primary);
            if (last == null) return;
            live.Remove(last);
            DisplayRemoved?.Invoke(last.id);
        }

        [ContextMenu("Simulate stop on the computer")]
        public void StopFromComputer()
        {
            if (phase == Phase.Idle) return;
            Stop();
            Ended?.Invoke("Sharing was stopped on the computer.");
        }

        void Update()
        {
            timer += Time.deltaTime;
            switch (phase)
            {
                case Phase.Pairing:
                    if (timer >= approveAfter) { phase = Phase.Connecting; timer = 0; Approved?.Invoke(); }
                    break;
                case Phase.Connecting:
                    if (timer < connectAfter) break;
                    phase = Phase.Streaming;
                    frameClock = 0;
                    for (int i = 0; i < monitors.Length && phase == Phase.Streaming; i++) Share(i + 1, monitors[i]);
                    break;
                case Phase.Streaming:
                    if ((frameClock += Time.deltaTime) < 1 / framesPerSecond) break;
                    frameClock = 0;
                    foreach (var s in live.ToArray())
                    {
                        if (Time.time < s.stallUntil || !live.Contains(s)) continue;
                        Draw(s);
                        Frame?.Invoke(s.id);
                    }
                    break;
            }
        }

        void Share(int id, Monitor m)
        {
            if (!streams.TryGetValue(id, out var s) || s.monitor.layout.size != m.layout.size) streams[id] = s = Paint(id, m);
            s.monitor = m;
            s.stallUntil = 0;
            live.Add(s);
            Draw(s);
            DisplayAdded?.Invoke(new SharedDisplay { id = id, name = m.name, primary = m.primary, layout = m.layout, texture = s.texture });
        }

        // Faux Windows desktop in the New World palette, painted once per display: wallpaper, taskbar, a notes window
        // and a colour-bar window placed differently on each display, and one brass tile per display number top left.
        Stream Paint(int id, Monitor m)
        {
            int w = Mathf.Max(64, m.layout.width / downscale), h = Mathf.Max(64, m.layout.height / downscale);
            var s = new Stream { id = id, monitor = m, desktop = new Color32[w * h], pixels = new Color32[w * h] };
            if (streams.TryGetValue(id, out var old) && old.texture) Destroy(old.texture);
            s.texture = new Texture2D(w, h, TextureFormat.RGBA32, false) { name = "Simulated " + m.name, wrapMode = TextureWrapMode.Clamp };
            var px = s.desktop;
            Color[] wallpapers = { UIStyle.Sage, new Color(.22f, .3f, .45f), new Color(.45f, .3f, .25f), new Color(.3f, .25f, .4f) };
            Color top = wallpapers[(id - 1) % wallpapers.Length];
            for (int y = 0; y < h; y++) Fill(s, px, 0, y / (float)h, 1, (y + 1f) / h, Color.Lerp(top, UIStyle.Ink, y / (float)h));
            float bar = 24f / h;
            Fill(s, px, 0, 1 - bar, 1, 1, new Color(.07f, .08f, .09f));                             // taskbar
            for (int i = 0; i < id; i++) Fill(s, px, .03f + i * .05f, .04f, .07f + i * .05f, .04f + .04f * w / h, UIStyle.Brass);
            float shift = (id - 1) % 2 * .1f;
            Window(s, px, .06f + shift, .14f, .6f + shift, .82f, UIStyle.Sage);                       // notes window with text lines
            for (int i = 0; i < 9; i++) Fill(s, px, .09f + shift, .24f + i * .06f, .3f + shift + .27f * ((i * 53) % 17) / 17f, .26f + i * .06f, UIStyle.LightBorder);
            Window(s, px, .64f - shift * .5f, .3f, .95f - shift * .5f, .8f, UIStyle.Brass);           // colour-bar window
            Color[] bars = { UIStyle.WarmIvory, UIStyle.Brass, UIStyle.Sage, UIStyle.Live, new Color(.3f, .45f, .8f), new Color(.75f, .2f, .3f), UIStyle.Ink };
            float bx = .66f - shift * .5f, bw = .27f / bars.Length;
            for (int i = 0; i < bars.Length; i++) Fill(s, px, bx + i * bw, .38f, bx + (i + 1) * bw, .66f, bars[i]);
            return s;
        }

        // Each frame: the painted desktop plus a moving sweep over the colour bars and a growing progress bar.
        void Draw(Stream s)
        {
            Array.Copy(s.desktop, s.pixels, s.desktop.Length);
            s.frame++;
            float shift = (s.id - 1) % 2 * .1f, bx = .66f - shift * .5f, t = s.frame % 90 / 90f;
            Fill(s, s.pixels, bx + .27f * t, .38f, bx + .27f * t + .006f, .66f, Color.white);
            Fill(s, s.pixels, bx, .7f, bx + .27f, .73f, UIStyle.LightBorder);
            Fill(s, s.pixels, bx, .7f, bx + .27f * (s.frame % 170 / 170f), .73f, UIStyle.Sage);
            s.texture.SetPixels32(s.pixels);
            s.texture.Apply(false);
        }

        void Window(Stream s, Color32[] px, float x0, float y0, float x1, float y1, Color accent)
        {
            float dx = 4f / s.texture.width, dy = 4f / s.texture.height;
            Fill(s, px, x0 + dx, y0 + dy, x1 + dx, y1 + dy, new Color(.08f, .1f, .1f));   // shadow
            Fill(s, px, x0, y0, x1, y1, Color.white);
            Fill(s, px, x0, y0, x1, y0 + 20f / s.texture.height, accent);
        }

        // Rectangle in fractions of the display, top-left origin, clipped to the texture.
        static void Fill(Stream s, Color32[] px, float x0, float y0, float x1, float y1, Color c)
        {
            int w = s.texture.width, h = s.texture.height;
            Color32 c32 = c;
            int ax = Mathf.Max(0, Mathf.RoundToInt(x0 * w)), bx = Mathf.Min(w, Mathf.RoundToInt(x1 * w));
            int ay = Mathf.Max(0, Mathf.RoundToInt(y0 * h)), by = Mathf.Min(h, Mathf.RoundToInt(y1 * h));
            for (int y = ay; y < by; y++)
                for (int x = ax; x < bx; x++) px[(h - 1 - y) * w + x] = c32;
        }

        void OnDestroy() { foreach (var s in streams.Values) if (s.texture) Destroy(s.texture); }
    }
}
