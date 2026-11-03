using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StudyEverywhere
{
    // The deck's Mission mode: each craft's story played out in miniature above the table, stage by stage, with the
    // stage explained on the card (and spoken by Orbit) and, for the two NASA craft, the pictures they took. Falcon 9
    // launches in fire and smoke, drops its stages and fairing, and lands its booster on a drone ship; Crew Dragon rides
    // a Falcon 9 to the space station, comes home through the plasma of re-entry and splashes down under its parachutes;
    // Hubble unfolds over a small Earth and turns to its famous pictures; Voyager 1 flies its Grand Tour past Jupiter and
    // Saturn and out of the Sun's bubble. The scenes are built by the Orbit builder; this only poses them: Evaluate(t)
    // sets everything from the mission clock alone, so stages can be skipped and the editor can preview any moment.
    public sealed class HoloMission : MonoBehaviour
    {
        [Serializable] public class Show { public string id; public Transform root; public Renderer[] craft; public Material[] real; public Texture2D[] pictures; }

        public HoloDeck deck;
        [Tooltip("Speaks each stage's name (English voices only); the card shows its story.")] public OrbitAssistant narrator;
        [Tooltip("The Earth and Moon in the big window: they fade away while a NASA picture fills the window, and come back after.")] public OrbitEarth earth;
        public Show[] shows;
        [Header("Picture panel")]
        public CanvasGroup picturePanel;
        public RectTransform pictureCanvas;
        public RawImage picture;
        public TMP_Text pictureTitle, pictureCredit;
        public RectTransform[] pictureFrame; public Vector2[] pictureFrameGrow;
        public Vector2 pictureMax = new Vector2(920, 600);
        [Tooltip("When the narration runs past a stage, the scene slows to this speed so the words keep up.")]
        [Range(.1f, 1)] public float overrunSpeed = .3f;
        [Tooltip("The longest a stage waits for its narration (seconds), in case the voice stalls.")] public float overrunLimit = 20;
        [Tooltip("Editor previews only: show the craft in their real finishes.")] public bool previewRealistic;

        int playing = -1, beat = -1, pictureShown = -1;
        float time, overrun;
        bool jumped;
        int namedFor = -1;
        readonly Dictionary<string, Transform> named = new Dictionary<string, Transform>();
        readonly Dictionary<Transform, (Vector3 p, Quaternion r, Vector3 s)> rest = new Dictionary<Transform, (Vector3, Quaternion, Vector3)>();
        readonly Dictionary<ParticleSystem, float> rates = new Dictionary<ParticleSystem, float>();

        public bool Playing => playing >= 0;
        public OrbitMissions.Story Story => playing >= 0 ? OrbitMissions.Find(shows[playing].id) : null;
        public string StageTitle => Story != null && beat >= 0 ? Story.beats[beat].title : "";
        public bool Has(string id) => Array.Exists(shows, s => s.id == id && s.root);

        // ---- control (HoloDeck)
        public bool Play(string id)
        {
            int i = Array.FindIndex(shows, s => s.id == id && s.root);
            if (i < 0 || OrbitMissions.Find(id) == null) return false;
            if (playing >= 0) Close();
            playing = i; time = 0; beat = -1; overrun = 0;
            Index(i);
            shows[i].root.gameObject.SetActive(true);
            Reskin();
            // every stage name fetched now, so each is ready to speak the moment its stage begins
            if (narrator) narrator.Prefetch(Array.ConvertAll(OrbitMissions.Find(id).beats, b => b.title));
            foreach (var ps in shows[i].root.GetComponentsInChildren<ParticleSystem>(true)) { ps.Clear(true); ps.Play(true); }
            Evaluate(time);
            return true;
        }
        public void Stop()
        {
            if (playing < 0) return;
            Close(); playing = -1; beat = -1;
            if (deck) deck.MissionEnded();
        }
        // The arrows step through the stages: back goes to the start of this stage first, then the one before.
        public void Skip(int dir)
        {
            var st = Story; if (st == null) return;
            int b = Mathf.Max(beat, 0);
            if (dir > 0) { if (b + 1 >= st.beats.Length) { Stop(); return; } b++; }
            else if (time - st.beats[b].at < 2.5f) b = Mathf.Max(b - 1, 0);
            time = st.beats[b].at; beat = -1; overrun = 0; jumped = true;
            foreach (var ps in shows[playing].root.GetComponentsInChildren<ParticleSystem>(true)) ps.Clear(true);
            Evaluate(time);
        }
        // Craft in the deck's look: the real finishes or the cyan hologram.
        public void Reskin()
        {
            if (playing < 0 || !deck) return;
            var s = shows[playing]; bool real = deck.Realistic || (!Application.isPlaying && previewRealistic);
            for (int i = 0; i < s.craft.Length; i++) deck.Dress(s.craft[i], i < s.real.Length ? s.real[i] : null, false, real);
        }

        void Close()
        {
            var s = shows[playing];
            foreach (var ps in s.root.GetComponentsInChildren<ParticleSystem>(true)) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            RestorePose();
            s.root.gameObject.SetActive(false);
            Picture(null, -1, 0);
            if (narrator) narrator.StopNarration();
        }

        void Update()
        {
            if (playing < 0) return;
            var st = Story; float dt = Time.deltaTime;
            // narration that runs past its stage: slow the scene rather than cut the words off (the next stage's words
            // queue behind these, and the mission waits at its end for the last line)
            int next = beat + 1; float boundary = next < st.beats.Length ? st.beats[next].at : st.length;
            bool talking = narrator && narrator.Narrating && overrun < overrunLimit;
            if (talking && time + dt >= boundary) { dt *= overrunSpeed; overrun += Time.deltaTime; }
            time += dt;
            if (time >= st.length) { if (talking) { time = st.length; return; } Stop(); return; }
            Evaluate(time);
        }

        // ---- the editor's previews (the builder captures a few moments of each mission)
        public void PreviewAt(string id, float t, float particleSeconds = 1.5f)
        {
            int i = Array.FindIndex(shows, s => s.id == id && s.root); if (i < 0) return;
            if (playing >= 0 && playing != i) Close();
            playing = i; beat = -1; time = t;
            Index(i); shows[i].root.gameObject.SetActive(true); Reskin();
            Evaluate(t);
            foreach (var ps in shows[i].root.GetComponentsInChildren<ParticleSystem>(true)) ps.Simulate(particleSeconds, true, true);
        }
        public void EndPreview() { if (playing < 0) return; Close(); playing = -1; beat = -1; if (deck) deck.MissionText(null, null); }

        // ---- the mission at time t
        public void Evaluate(float t)
        {
            if (playing < 0) return;
            var s = shows[playing]; var st = OrbitMissions.Find(s.id);
            int b = st.BeatAt(t);
            if (b != beat)
            {
                beat = b; overrun = 0;
                var bt = st.beats[b];
                if (deck) deck.MissionText(bt.title, bt.text);
                // only the stage's name (the card tells its story): short enough to keep pace with the animation; a stage the
                // person skipped to replaces what was being said
                if (Application.isPlaying && narrator) narrator.Narrate(bt.title, jumped);
            }
            jumped = false;
            switch (s.id)
            {
                case "falcon-9": Falcon(t); break;
                case "crew-dragon": Dragon(t); break;
                case "hubble": Hubble(t); break;
                case "voyager-1": Voyager(t); break;
            }
            // this stage's picture, faded in after the stage starts and out before it ends
            var pb = st.beats[b]; float end = b + 1 < st.beats.Length ? st.beats[b + 1].at : st.length;
            int pic = pb.picture;
            float alpha = pic >= 0 ? Mathf.Min(Ramp(t, pb.at + .3f, pb.at + 1.1f), 1 - Ramp(t, end - .6f, end)) : 0;
            Picture(pic >= 0 && s.pictures != null && pic < s.pictures.Length ? s.pictures[pic] : null, pic, alpha, pb);
        }

        void Picture(Texture2D tex, int index, float alpha, OrbitMissions.Beat b = null)
        {
            if (!picturePanel) return;
            bool show = tex && alpha > .001f;
            if (earth && earth.visible < 1) earth.visible = 1;   // (the Earth no longer fades for a picture: the picture stands on the card)
            if (picturePanel.gameObject.activeSelf != show) picturePanel.gameObject.SetActive(show);
            if (!show) { pictureShown = -1; return; }
            picturePanel.alpha = alpha;
            OnCard();
            if (pictureShown == index + playing * 100) return;
            pictureShown = index + playing * 100;
            picture.texture = tex;
            if (pictureTitle) pictureTitle.text = b?.pictureTitle ?? "";
            if (pictureCredit) pictureCredit.text = b?.credit ?? "";
            LayoutPicture(tex);
        }

        // The picture stands on the deck card's top edge, wherever the card is and however it has sized itself (its
        // pivot is its bottom, so it grows upward and never covers the card), turned as the card is.
        void OnCard()
        {
            var card = deck ? deck.cardRect : null; if (!card || !pictureCanvas) return;
            var c = new Vector3[4]; card.GetWorldCorners(c);
            pictureCanvas.SetPositionAndRotation((c[1] + c[2]) / 2 + card.up * .05f, card.rotation);
        }

        // The picture as large as it fits, the title and credit under it, the frame round them.
        void LayoutPicture(Texture tex)
        {
            if (!pictureCanvas || !picture) return;
            float aspect = tex.width / (float)Mathf.Max(1, tex.height);
            var size = new Vector2(pictureMax.x, pictureMax.x / aspect);
            if (size.y > pictureMax.y) size = new Vector2(pictureMax.y * aspect, pictureMax.y);
            const float pad = 30, textH = 112;
            var panel = new Vector2(Mathf.Max(size.x, 600) + pad * 2, size.y + pad * 2 + textH);
            pictureCanvas.sizeDelta = panel;
            void Top(RectTransform r, float y, Vector2 sz) { r.anchorMin = r.anchorMax = new Vector2(.5f, 1); r.pivot = new Vector2(.5f, 1); r.anchoredPosition = new Vector2(0, y); r.sizeDelta = sz; }
            Top(picture.rectTransform, -pad, size);
            if (pictureTitle) Top(pictureTitle.rectTransform, -pad - size.y - 16, new Vector2(panel.x - pad * 2, 50));
            if (pictureCredit) Top(pictureCredit.rectTransform, -pad - size.y - 70, new Vector2(panel.x - pad * 2, 32));
            for (int i = 0; pictureFrame != null && i < pictureFrame.Length; i++)
            {
                var f = pictureFrame[i]; if (!f) continue;
                var grow = pictureFrameGrow != null && i < pictureFrameGrow.Length ? pictureFrameGrow[i] : Vector2.zero;
                f.anchorMin = f.anchorMax = new Vector2(.5f, 1); f.pivot = new Vector2(.5f, .5f);
                f.sizeDelta = panel + grow; f.anchoredPosition = new Vector2(0, -panel.y / 2 + (i == 1 ? -6 : 0));
            }
        }

        // ================================================================== the four missions
        // Times are seconds on the mission clock (OrbitMissions holds the stages that go with them). Positions are in the
        // scene's own frame: the table top at y 0, +Z away from the person at the deck.

        // The stack's flight: on the pad, then rising faster and faster, the view catching up with it (it settles back to
        // the middle while the pad drops away), and the gentle pitch downrange. An offset from the stack's place on the pad.
        static (Vector3 pos, Quaternion rot) Stack(float t, float liftoff)
        {
            float climb = Accel(t, liftoff, liftoff + 8) * 1.0f, follow = Ease(t, liftoff + 5, liftoff + 10);
            return (new Vector3(0, Mathf.Lerp(climb, .45f, follow), 0), Quaternion.Euler(0, 0, 18 * Ease(t, liftoff + 4, liftoff + 13)));
        }
        void Pad(float t, float liftoff)
        {
            Move("Pad", new Vector3(0, -.5f * Accel(t, liftoff + 6, liftoff + 10), 0), null, 1 - Ease(t, liftoff + 7, liftoff + 10));
            Fx("Pad smoke", Window(t, .5f, liftoff + 4, .6f));
            Fx("Trail", Window(t, liftoff, liftoff + 10, .6f));
        }

        void Falcon(float t)
        {
            const float L = 5;
            var (p, r) = Stack(t, L); var up = r * Vector3.up;
            Pad(t, L);
            Fx("Streaks", Window(t, 12, 30, 1));
            // the stages part at 21: pneumatic pushers, the booster drops back, the upper stage pulls ahead
            float sep = Ease(t, 21, 23);
            Move("Upper", p + up * (.07f * sep + 1.6f * Accel(t, 26, 36)), r, 1 - Ease(t, 34, 36));
            Fx("Upper flame", Window(t, 25.5f, 36, .4f));
            // the fairing halves hinge open and fall away
            float open = Ease(t, 30.5f, 32.5f), drift = Ramp(t, 30.5f, 35), gone = 1 - Ease(t, 33, 35);
            Move("Fairing left", new Vector3(-.05f, -.03f, 0) * drift, Quaternion.Euler(0, 0, 40 * open), gone);
            Move("Fairing right", new Vector3(.05f, -.03f, 0) * drift, Quaternion.Euler(0, 0, -40 * open), gone);
            // the booster: flip, grid fins, entry burn, and down onto the drone ship
            Vector3 bPos; Quaternion bRot;
            if (t < 21) { bPos = p; bRot = r; }
            else
            {
                var (ps, rs) = Stack(21, L);
                var start = ps - rs * Vector3.up * (.06f * sep);
                var hover = new Vector3(-.12f, .62f, 0); var above = new Vector3(0, .62f, 0); var land = Rest("Landing mark").p - Rest("Booster").p;
                bPos = t < 40 ? Vector3.Lerp(start, hover, Ease(t, 23, 40)) : t < 49 ? Vector3.Lerp(hover, above, Ease(t, 40, 49)) : Vector3.Lerp(above, land, Decel(t, 49, 58));
                bRot = Quaternion.Euler(0, 0, 18 + 182 * Ease(t, 36, 40) + 160 * Ease(t, 41, 46));
            }
            Move("Booster", bPos, bRot);
            Fx("Booster flame", Mathf.Max(t < 20.5f ? Ramp(t, .5f, 2.5f) : 0, Window(t, 45, 48, .3f), Window(t, 51, 58, .3f)));
            Fx("Booster puffs", Window(t, 36, 40, .2f));
            Size("Grid fins", Ease(t, 38, 39.5f));
            Size("Legs", Ease(t, 54, 55.5f));
            Size("Droneship", Ease(t, 46, 48.5f));
            Fx("Landing smoke", Window(t, 57.6f, 59.5f, .2f));
        }

        void Dragon(float t)
        {
            const float L = 5;
            var (p, r) = Stack(t, L); var up = r * Vector3.up;
            Pad(t, L);
            Move("Crew arm", Vector3.zero, Quaternion.Euler(0, -75 * Ease(t, 0, 2.5f), 0));
            Fx("Streaks", Window(t, 12, 27, 1));
            // the booster rides to 19, then drops back and fades
            float bsep = Ease(t, 19.5f, 21.5f);
            Move("Booster", p - up * (.08f * bsep + .5f * Accel(t, 21, 25)), r, 1 - Ease(t, 22, 25));
            Fx("Booster flame", t < 19 ? Ramp(t, .5f, 2.5f) : 0);
            // the upper stage burns to 31, then lets Dragon go and falls away
            float usep = Ease(t, 31, 33);
            Move("Upper", p + up * (.06f * bsep) - up * (.1f * usep + .4f * Accel(t, 32, 36)), r, 1 - Ease(t, 33, 36));
            Fx("Upper flame", Window(t, 23.5f, 31, .4f));
            // Dragon: on the stack, then free, and the view closes in on it (it grows to its close-up size mid-deck)
            var origin = Rest("Booster").p; var stacked = Rest("Dragon").p - origin;
            var ride = origin + p + r * stacked + up * (.06f * bsep + .02f * usep);
            var close = Rest("Dragon close"); float zoom = Ease(t, 32, 37);
            var pos = Vector3.Lerp(ride, close.p, zoom); var rot = Quaternion.Slerp(r, Quaternion.identity, zoom); float k = Mathf.Lerp(1, close.s.x, zoom);
            // up to the station's port and back, then down through the atmosphere to the sea
            pos = Vector3.Lerp(pos, Rest("Docking target").p, Ease(t, 41, 50) * (1 - Ease(t, 57, 60)));
            pos = Vector3.Lerp(pos, new Vector3(0, .55f, 0), Ease(t, 68, 76));
            pos = Vector3.Lerp(pos, Rest("Splash target").p, Decel(t, 76, 88));
            if (t > 88) pos.y += .006f * Mathf.Sin((t - 88) * 2.2f) * (1 - Ramp(t, 88, 96) * .5f);
            rot *= Quaternion.Euler(0, 0, 20 * Window(t, 68, 80, 3));
            Pose("Dragon", pos, rot, k);
            Move("Nosecone", Vector3.zero, Quaternion.Euler(110 * Ease(t, 36, 38) * (1 - Ease(t, 61, 62.5f)), 0, 0));
            float tj = Ramp(t, 64, 68);
            Move("Trunk", new Vector3(0, -6 * tj, 0), Quaternion.Euler(25 * tj, 0, 0), 1 - Ease(t, 66, 68));
            Fx("Dragon puffs", Mathf.Max(Mathf.Max(Window(t, 41, 43, .2f), Window(t, 45, 46, .2f)), Mathf.Max(Window(t, 48, 49, .2f), Window(t, 62, 64, .2f))));
            Fx("Plasma", Window(t, 69, 76, 1));
            Fx("Reentry streaks", Window(t, 68, 77, 1));
            // the station drifts in, and away again after undocking
            Move("ISS", new Vector3(0, .3f, .15f) * Ease(t, 59, 62), null, Ease(t, 39, 42) * (1 - Ease(t, 59, 62)));
            // two drogues, then four mains that unfold and fill; let go after splashdown
            Size("Drogues", Ease(t, 76, 77.5f) * (1 - Ease(t, 80, 80.6f)));
            float mains = Ease(t, 80, 81) * (1 - Ease(t, 89, 91)), fill = Mathf.Lerp(.25f, 1, Ease(t, 80.5f, 82.5f));
            SizeXYZ("Mains", new Vector3(fill, 1, fill) * mains);
            Size("Ocean", Ease(t, 78, 80));
            Fx("Splash", Window(t, 88, 89.3f, .1f));
        }

        static readonly float[] hubbleAt = { 19, 31, 43, 55 };
        static readonly Vector2[] hubbleAim = { new Vector2(0, 0), new Vector2(14, 6), new Vector2(-10, -8), new Vector2(8, -4) };
        void Hubble(float t)
        {
            var earth = Rest("Earth").p; var home = Rest("Hubble");
            Move("Earth", Vector3.zero, Quaternion.Euler(0, -t * 1.5f, 0));   // turning slowly, Africa and Europe toward the deck
            // deployment: the antennas swing out, the solar arrays unroll
            Size("Antennas", Ease(t, 1.5f, 4));
            float unroll = Mathf.Lerp(.04f, 1, Ease(t, 3.5f, 8));
            SizeXYZ("Array left", new Vector3(unroll, 1, 1)); SizeXYZ("Array right", new Vector3(unroll, 1, 1));
            // one orbit, then pictures, then on round again
            float orbit = Ease(t, 9, 19) + .5f * Ease(t, 67, 75), th = orbit * Mathf.PI * 2, radius = (home.p - earth).magnitude;
            var off = Quaternion.Euler(0, 0, 28.5f) * new Vector3(Mathf.Sin(th), 0, -Mathf.Cos(th)) * radius;
            // a small slew toward each target
            var aim = Vector2.zero;
            for (int i = 0; i < hubbleAt.Length; i++) aim = Vector2.Lerp(aim, hubbleAim[i], Ease(t, hubbleAt[i], hubbleAt[i] + 1.5f));
            aim = Vector2.Lerp(aim, Vector2.zero, Ease(t, 67, 69));
            Pose("Hubble", earth + off, Quaternion.Euler(aim.y, aim.x, 0) * home.r, 1);
        }

        // Voyager's path: Earth, Jupiter, Saturn, up out of the plane of the planets, through the heliopause. Path markers
        // set by the builder; where along it (0..4) at each moment, slow at each world it visits.
        static readonly Vector2[] voyagerKeys = { new Vector2(1, 0), new Vector2(15, 1), new Vector2(22, 1.08f), new Vector2(29, 1.15f), new Vector2(34, 2), new Vector2(41, 2.15f), new Vector2(46, 3), new Vector2(55, 3), new Vector2(63, 4) };
        void Voyager(float t)
        {
            var pts = new Vector3[5]; for (int i = 0; i < 5; i++) pts[i] = Rest("Path " + i).p;
            float u = Keyed(voyagerKeys, t);
            var pos = Spline(pts, u); var earth = Rest("Earth").p;
            // the high-gain dish always faces home
            Pose("Voyager", pos, Quaternion.FromToRotation(Vector3.up, (earth - pos).normalized), 1);
            Move("Jupiter", Vector3.zero, Quaternion.Euler(0, t * 25, 0));
            Move("Saturn", Vector3.zero, Quaternion.Euler(0, t * 20, 0));
            var trail = T("Trail"); if (trail && trail.TryGetComponent<LineRenderer>(out var line))
            {
                int n = Mathf.Max(2, Mathf.CeilToInt(u * 24) + 1); line.positionCount = n;
                for (int i = 0; i < n; i++) line.SetPosition(i, Spline(pts, u * i / (n - 1)));
            }
            Size("Heliopause", Ease(t, 53, 56) * (1 + .04f * Window(t, 58.5f, 61, .9f)));
            Fx("Sun glow", 1);
            Pose("Signal", Vector3.Lerp(pos, earth, Mathf.Repeat(t * .5f, 1)), Quaternion.identity, Window(t, 63, 74, .5f));
        }
        static float Keyed(Vector2[] keys, float t)
        {
            if (t <= keys[0].x) return keys[0].y;
            for (int i = 1; i < keys.Length; i++) if (t < keys[i].x) return Mathf.Lerp(keys[i - 1].y, keys[i].y, Mathf.SmoothStep(0, 1, Ramp(t, keys[i - 1].x, keys[i].x)) * .5f + Ramp(t, keys[i - 1].x, keys[i].x) * .5f);
            return keys[keys.Length - 1].y;
        }
        static Vector3 Spline(Vector3[] p, float u)
        {
            int n = p.Length - 1; u = Mathf.Clamp(u, 0, n); int i = Mathf.Min((int)u, n - 1); float f = u - i;
            Vector3 a = p[Mathf.Max(i - 1, 0)], b = p[i], c = p[i + 1], d = p[Mathf.Min(i + 2, n)];
            return .5f * (2 * b + (-a + c) * f + (2 * a - 5 * b + 4 * c - d) * f * f + (-a + 3 * b - 3 * c + d) * f * f * f);
        }

        // ================================================================== posing
        void Index(int i)
        {
            if (namedFor == i) return;
            named.Clear(); namedFor = i;
            foreach (var t in shows[i].root.GetComponentsInChildren<Transform>(true))
            {
                named[t.name] = t;
                if (!rest.ContainsKey(t)) rest[t] = (t.localPosition, t.localRotation, t.localScale);
            }
        }
        void RestorePose()
        {
            foreach (var kv in rest) if (kv.Key) { kv.Key.localPosition = kv.Value.p; kv.Key.localRotation = kv.Value.r; kv.Key.localScale = kv.Value.s; }
        }
        Transform T(string n) => named.TryGetValue(n, out var t) ? t : null;
        (Vector3 p, Quaternion r, Vector3 s) Rest(string n) { var t = T(n); return t && rest.TryGetValue(t, out var v) ? v : (Vector3.zero, Quaternion.identity, Vector3.one); }
        // Absolute pose in the parent's frame; a piece shrunk to nothing is switched off (one less thing to draw).
        void Pose(string n, Vector3 pos, Quaternion rot, float size) => Place(T(n), pos, rot, Vector3.one * size);
        void Place(Transform t, Vector3 pos, Quaternion rot, Vector3 size)
        {
            if (!t) return;
            var r = rest[t];
            t.localPosition = pos; t.localRotation = rot; t.localScale = Vector3.Scale(r.s, size);
            bool show = size.x > .001f && size.y > .001f && size.z > .001f;
            if (t.gameObject.activeSelf != show) t.gameObject.SetActive(show);
        }
        // Relative to the rest pose: an offset, a turn on top of it, a size.
        void Move(string n, Vector3 offset, Quaternion? turn = null, float size = 1)
        {
            var t = T(n); if (!t) return; var r = rest[t];
            Place(t, r.p + offset, turn.HasValue ? turn.Value * r.r : r.r, Vector3.one * size);
        }
        void Size(string n, float size) => Move(n, Vector3.zero, null, size);
        void SizeXYZ(string n, Vector3 size) { var t = T(n); if (!t) return; var r = rest[t]; Place(t, r.p, r.r, size); }
        // A particle effect at a strength (0 stops it emitting; what is in the air lives out its life).
        void Fx(string n, float strength)
        {
            var t = T(n); if (!t || !t.TryGetComponent<ParticleSystem>(out var ps)) return;
            if (!rates.TryGetValue(ps, out var rate)) rates[ps] = rate = ps.emission.rateOverTimeMultiplier;
            var e = ps.emission; e.rateOverTimeMultiplier = rate * Mathf.Max(0, strength);
            if (Application.isPlaying && !ps.isPlaying) ps.Play();
        }

        static float Ramp(float t, float a, float b) => Mathf.Clamp01((t - a) / (b - a));
        static float Ease(float t, float a, float b) { float x = Ramp(t, a, b); return x * x * (3 - 2 * x); }
        static float Accel(float t, float a, float b) { float x = Ramp(t, a, b); return x * x; }
        static float Decel(float t, float a, float b) { float x = 1 - Ramp(t, a, b); return 1 - x * x; }
        static float Window(float t, float a, float b, float fade) => Mathf.Min(Ramp(t, a, a + fade), 1 - Ramp(t, b - fade, b));
    }

    // The stages of each mission and what Orbit says about them (figures from NASA and SpaceX).
    public static class OrbitMissions
    {
        public sealed class Beat
        {
            public float at; public string title, text; public int picture = -1; public string pictureTitle, credit;
            public Beat(float at, string title, string text) { this.at = at; this.title = title; this.text = text; }
            public Beat(float at, string title, string text, int picture, string pictureTitle, string credit) : this(at, title, text) { this.picture = picture; this.pictureTitle = pictureTitle; this.credit = credit; }
        }
        public sealed class Story
        {
            public string id; public float length; public Beat[] beats;
            public int BeatAt(float t) { int b = 0; for (int i = 0; i < beats.Length; i++) if (t >= beats[i].at) b = i; return b; }
        }
        public static Story Find(string id) => Array.Find(All, s => s.id == id);

        public static readonly Story[] All =
        {
            new Story
            {
                id = "falcon-9", length = 66, beats = new[]
                {
                    new Beat(0, "Ignition", "Nine Merlin engines light three seconds before liftoff. Clamps hold the rocket down until the flight computers see full thrust."),
                    new Beat(5, "Liftoff", "Falcon 9 rises on about 7,600 kilonewtons of thrust, burning rocket-grade kerosene and liquid oxygen."),
                    new Beat(13, "Max Q", "About 70 seconds in, already faster than sound, the rocket passes Max Q: the moment the air pushes hardest on it."),
                    new Beat(20, "Stage separation", "Two and a half minutes up, the first stage engines cut off and pneumatic pushers split the stages apart."),
                    new Beat(25.5f, "Second stage", "One Merlin Vacuum engine, with a huge nozzle made for thin air, lights to carry the payload on to orbit."),
                    new Beat(30, "Fairing separation", "Above the thick air, the nose fairing splits in two and falls away. Each half steers under a parafoil and is caught to fly again."),
                    new Beat(36, "Flip and grid fins", "Meanwhile the booster turns itself around with cold-gas thrusters and unfolds four titanium grid fins to steer as it falls."),
                    new Beat(43, "Entry burn", "Three engines relight to slow the booster before it hits the thick lower atmosphere, so it does not burn up."),
                    new Beat(50, "Landing burn", "One engine relights for the landing burn, and four carbon-fibre legs swing down just before touchdown on a drone ship at sea."),
                    new Beat(58, "Touchdown", "The booster stands on the drone ship, ready to fly again. Some boosters have now flown more than thirty times."),
                },
            },
            new Story
            {
                id = "crew-dragon", length = 96, beats = new[]
                {
                    new Beat(0, "Ignition", "The crew access arm swings back, and nine Merlin engines light beneath Dragon and its crew."),
                    new Beat(5, "Liftoff", "Falcon 9 lifts Dragon off Launch Complex 39A in Florida, the pad that launched Apollo 11 and the Space Shuttle."),
                    new Beat(13, "Max Q", "About a minute in, the rocket passes Max Q, the greatest air pressure of the climb. The crew feel about three g."),
                    new Beat(19, "Stage separation", "The first stage shuts down and drops away, to land itself on a drone ship or back at the coast."),
                    new Beat(23.5f, "Second stage", "The second stage's single Merlin Vacuum engine pushes Dragon the rest of the way to orbit."),
                    new Beat(31, "Dragon separates", "About twelve minutes after liftoff Dragon flies free, then opens its nosecone to uncover the docking port and its guidance sensors."),
                    new Beat(39, "Rendezvous", "Dragon flies itself to the International Space Station, nudged along by sixteen small Draco thrusters, usually in about a day."),
                    new Beat(50, "Docked", "Dragon docks automatically to the station's port. After leak checks the hatch opens and the crew float aboard."),
                    new Beat(57, "Undocking", "Months later, the crew climb back in and Dragon backs away from the station."),
                    new Beat(62, "Deorbit and trunk", "The nosecone closes, the Dracos fire to drop Dragon out of orbit, and the trunk with its solar cells is let go."),
                    new Beat(68, "Re-entry", "Dragon hits the atmosphere at about 27,000 kilometres an hour. Its heat shield faces temperatures near 1,900 degrees Celsius."),
                    new Beat(76, "Parachutes", "Two drogue parachutes steady the capsule about five and a half kilometres up, then four main parachutes open."),
                    new Beat(88, "Splashdown", "Dragon splashes down at about 25 kilometres an hour, and a recovery ship lifts the crew aboard within the hour."),
                },
            },
            new Story
            {
                id = "hubble", length = 76, beats = new[]
                {
                    new Beat(0, "Deployment", "On 25 April 1990 the shuttle Discovery's robot arm let Hubble go, about 600 kilometres up. Its antennas swung out and its solar arrays unrolled."),
                    new Beat(9, "In orbit", "Hubble circles the Earth every 95 minutes at about 27,000 kilometres an hour, above the blur of the atmosphere."),
                    new Beat(19, "Pillars of Creation", "Towers of cold gas and dust in the Eagle Nebula, 6,500 light-years away, where new stars are forming. Hubble first photographed them in 1995.",
                        0, "Pillars of Creation, Eagle Nebula", "NASA, ESA and the Hubble Heritage Team (STScI/AURA)"),
                    new Beat(31, "Ultra Deep Field", "A speck of sky smaller than a grain of sand held at arm's length, filled with about 10,000 galaxies, some seen as they were 13 billion years ago.",
                        1, "Hubble Ultra Deep Field", "NASA, ESA, H. Teplitz and M. Rafelski, A. Koekemoer, R. Windhorst, Z. Levay"),
                    new Beat(43, "Mystic Mountain", "A three-light-year pillar in the Carina Nebula, 7,500 light-years away, with jets of gas firing from the young stars inside it. Taken for Hubble's 20th birthday.",
                        2, "Mystic Mountain, Carina Nebula", "NASA, ESA and M. Livio and the Hubble 20th Anniversary Team (STScI)"),
                    new Beat(55, "Crab Nebula", "What is left of a star that exploded in the year 1054, a blast Chinese astronomers saw in daylight. A neutron star at its heart spins 30 times a second.",
                        3, "Crab Nebula", "NASA, ESA, J. Hester and A. Loll (Arizona State University)"),
                    new Beat(67, "Still observing", "Astronauts upgraded Hubble on five shuttle missions between 1993 and 2009. It has made more than 1.6 million observations, and it is still at work."),
                },
            },
            new Story
            {
                id = "voyager-1", length = 74, beats = new[]
                {
                    new Beat(0, "Launch", "Voyager 1 left Earth on 5 September 1977 on a Titan IIIE-Centaur rocket, sixteen days after its twin, Voyager 2."),
                    new Beat(6, "Cruise", "On a faster path, it overtook Voyager 2 and crossed the asteroid belt on the way to Jupiter. These planets are not to scale."),
                    new Beat(15, "Jupiter", "Voyager 1 passed Jupiter in March 1979, and its pictures showed the Great Red Spot, a storm bigger than the Earth, churning in detail.",
                        0, "Jupiter's Great Red Spot", "NASA/JPL"),
                    new Beat(22, "Io's volcanoes", "It found volcanoes erupting on Jupiter's moon Io, the first active volcanoes ever seen beyond the Earth. Jupiter's gravity then flung it toward Saturn.",
                        1, "Jupiter with Io", "NASA/JPL"),
                    new Beat(29, "Saturn and Titan", "In November 1980 it passed Saturn and swung close by Titan to study its thick haze. That flyby bent its path up and out of the plane of the planets.",
                        2, "Saturn with Tethys and Dione", "NASA/JPL"),
                    new Beat(41, "The Pale Blue Dot", "On 14 February 1990, six billion kilometres out, Voyager 1 looked back and photographed the Earth: a single pale blue pixel in a sunbeam.",
                        3, "The Pale Blue Dot", "NASA/JPL-Caltech"),
                    new Beat(53, "Interstellar space", "On 25 August 2012 it crossed the heliopause, where the Sun's wind gives way to the gas between the stars, the first human-made object to do so."),
                    new Beat(63, "Still calling home", "It is still talking to us, on a radio about as strong as a fridge light. Its signals now take nearly a whole day to reach the Earth."),
                },
            },
        };
    }
}
