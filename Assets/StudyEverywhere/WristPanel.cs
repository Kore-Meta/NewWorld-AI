using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace StudyEverywhere
{
    // The settings menu of every place, opened only by tapping the watch's screen (WristWatch). It opens in front of
    // the person and stays anchored where it opened, so it does not follow the hands; tapping the watch again, or
    // Close, puts it away. Every menu also chooses which wrist
    // wears the watch (watchLeft / watchRight). Subclasses fill it (WristMenu in the café, PlaceMenu in the lobby,
    // RestMenu in the meadow). It springs out of the watch when it opens and is drawn back into it when it closes;
    // every button ticks when pressed, and opening and closing give a soft ping (sounds made here, no audio files).
    [RequireComponent(typeof(Canvas))]
    public abstract class WristPanel : MonoBehaviour
    {
        public Unity.XR.CoreUtils.XROrigin origin;
        public Transform leftController;
        [Header("Placement")]
        [Tooltip("How far in front of the eyes the menu opens, in metres.")]
        public float distance = 0.5f;
        [Tooltip("How far below eye level it opens, in metres.")]
        public float drop = 0.12f;
        [Header("Watch")]
        public Button watchLeft, watchRight;
        [Header("Feel")]
        [Tooltip("Seconds the menu takes to spring out of the watch, and to be drawn back in.")]
        public float openSeconds = 0.3f, closeSeconds = 0.2f;
        [Range(0, 1)] public float soundVolume = 0.5f;
        [Tooltip("Night colours (slate cards, ivory text) for places at night.")]
        public bool dark;
        [Tooltip("The place's own menu colours (see MenuTheme); off: the café's, as the kit is drawn.")]
        public bool customTheme;
        public MenuTheme theme;
        public MenuTheme Theme => customTheme ? theme : MenuTheme.Cafe;
        // Marks a selection in this menu's colours (as WristMenu.Mark does in the café's).
        protected void Mark(Button b, bool on) => Theme.Mark(b, on, dark);

        Canvas canvas;
        bool shown;
        Vector3 fullScale;
        Coroutine motion;
        AudioSource sfx;
        static AudioClip tick, pingOpen, pingClose;

        public bool IsOpen => shown;

        protected virtual void Awake()
        {
            canvas = GetComponent<Canvas>();
            fullScale = transform.localScale;
            sfx = gameObject.AddComponent<AudioSource>();
            sfx.playOnAwake = false; sfx.spatialBlend = .3f; sfx.volume = 1;   // soundVolume is applied once, per sound (Play)
            MakeSounds();
            foreach (var b in GetComponentsInChildren<Button>(true)) b.onClick.AddListener(() => Play(tick));
            if (watchLeft) watchLeft.onClick.AddListener(() => { WristWatch.Worn = WristWatch.Hand.Left; MarkWatch(); });
            if (watchRight) watchRight.onClick.AddListener(() => { WristWatch.Worn = WristWatch.Hand.Right; MarkWatch(); });
            SetShown(false);
        }

        // ---- day and night colours. The first switch remembers every element's day colour; after that, night maps
        // them through UIStyle.ToNight and day puts them back. Subclasses re-mark their selections afterwards.
        readonly System.Collections.Generic.List<(Graphic g, Color c)> dayGraphics = new System.Collections.Generic.List<(Graphic, Color)>();
        readonly System.Collections.Generic.List<(Button b, ColorBlock c)> dayButtons = new System.Collections.Generic.List<(Button, ColorBlock)>();
        protected bool Themed { get; private set; }
        public void SetDark(bool on)
        {
            if (Themed && on == dark) return;
            if (!Themed)
            {
                foreach (var g in GetComponentsInChildren<Graphic>(true)) dayGraphics.Add((g, g.color));
                foreach (var b in GetComponentsInChildren<Button>(true)) dayButtons.Add((b, b.colors));
                Themed = true;
            }
            var t = Theme;
            foreach (var (g, c) in dayGraphics) if (g) g.color = on ? t.ToNight(c, g is TMPro.TMP_Text) : c;
            foreach (var (b, c) in dayButtons)
            {
                if (!b) continue;
                var cb = c;
                if (on && UIStyle.Near(c.normalColor, t.surface)) { cb.normalColor = t.nightPill; cb.highlightedColor = cb.selectedColor = t.nightHover; cb.pressedColor = t.nightEdge; }
                // a greyed-out pill (where you are, not open yet) is light by day; at night it must go dark too, or its
                // light night text vanishes against it
                if (on && c.disabledColor.grayscale > .45f) cb.disabledColor = Color.Lerp(t.nightPill, t.nightSurface, .5f);
                b.colors = cb;
            }
            dark = on;
            MarkWatch();
        }

        // Called each time the menu appears, to bring its controls up to date.
        protected virtual void OnShown() { }

        public void Open()
        {
            if (origin && origin.Camera)
            {
                var head = origin.Camera.transform;
                var ahead = Vector3.ProjectOnPlane(head.forward, Vector3.up);
                if (ahead.sqrMagnitude < 1e-4f) ahead = Vector3.ProjectOnPlane(head.up, Vector3.up);
                var point = head.position + ahead.normalized * distance + Vector3.down * drop;
                transform.SetPositionAndRotation(point, Quaternion.LookRotation(point - head.position, Vector3.up));
            }
            bool wasShown = shown && motion == null;   // still being drawn in counts as closed: spring back out
            SetShown(true);
            if (wasShown) return;
            Play(pingOpen);
            Animate(true);
        }
        // An open menu comes along when the person is moved (a teleport, a place from the menu, a seat): it keeps its spot
        // relative to them, so it is never left behind across the room.
        Vector3 rigWas; Quaternion rigTurnWas; bool rigKnown;
        void LateUpdate()
        {
            if (!origin) return;
            var rig = origin.transform;
            if (shown && motion == null && rigKnown && ((rig.position - rigWas).sqrMagnitude > 1e-6f || Quaternion.Angle(rig.rotation, rigTurnWas) > .01f))
            {
                var turn = rig.rotation * Quaternion.Inverse(rigTurnWas);
                transform.SetPositionAndRotation(rig.position + turn * (transform.position - rigWas), turn * transform.rotation);
            }
            rigWas = rig.position; rigTurnWas = rig.rotation; rigKnown = true;
        }

        public void Close()
        {
            if (!shown) return;
            Play(pingClose);
            Animate(false);
        }
        public void Toggle() { if (shown) Close(); else Open(); }
        // Kept for the menus' Close buttons.
        public void Unpin() => Close();

        void SetShown(bool value)
        {
            if (!canvas) canvas = GetComponent<Canvas>();
            if (shown == value && canvas.enabled == value) return;
            shown = value;
            canvas.enabled = value;
            foreach (var c in GetComponentsInChildren<Collider>(true)) c.enabled = value;
            if (value) { MarkWatch(); OnShown(); }
        }

        // ---- springing out of the watch and back in
        void Animate(bool opening)
        {
            if (motion != null) StopCoroutine(motion);
            if (!isActiveAndEnabled) { transform.localScale = fullScale; if (!opening) SetShown(false); return; }
            motion = StartCoroutine(opening ? SpringOut() : DrawIn());
        }

        // Where the watch is now, or (no watch showing) a little below the menu.
        Vector3 WatchPoint()
        {
            var watch = FindAnyObjectByType<WristWatch>();
            if (watch && watch.isActiveAndEnabled && watch.GetComponent<Canvas>().enabled) return watch.transform.position;
            return transform.position + Vector3.down * .25f;
        }

        IEnumerator SpringOut()
        {
            Vector3 to = transform.position, from = WatchPoint();
            for (float t = 0; t < openSeconds; t += Time.deltaTime)
            {
                float u = t / openSeconds, k = 1 + 2.2f * Mathf.Pow(u - 1, 3) + 1.2f * Mathf.Pow(u - 1, 2);   // ease out with a little overshoot
                transform.position = Vector3.LerpUnclamped(from, to, k);
                transform.localScale = fullScale * Mathf.LerpUnclamped(.02f, 1, k);
                yield return null;
            }
            transform.position = to; transform.localScale = fullScale; motion = null;
        }

        IEnumerator DrawIn()
        {
            Vector3 from = transform.position;
            for (float t = 0; t < closeSeconds; t += Time.deltaTime)
            {
                float u = t / closeSeconds, k = u * u * u;   // ease in: slow, then gone
                transform.position = Vector3.Lerp(from, WatchPoint(), k);
                transform.localScale = fullScale * Mathf.Lerp(1, .02f, k);
                yield return null;
            }
            SetShown(false);
            transform.position = from; transform.localScale = fullScale; motion = null;
        }

        // ---- sounds: a soft tick for buttons, a soft bell-like ping, higher to open and lower to close
        void Play(AudioClip clip) { if (clip && sfx && soundVolume > 0) sfx.PlayOneShot(clip, soundVolume); }

        static void MakeSounds()
        {
            if (tick) return;
            const int rate = 44100;
            AudioClip Make(string name, float seconds, System.Func<float, float, float> sample)
            {
                int n = (int)(rate * seconds); var data = new float[n];
                for (int i = 0; i < n; i++) data[i] = sample(i / (float)rate, i / (float)n);
                var clip = AudioClip.Create(name, n, 1, rate, false); clip.SetData(data, 0); return clip;
            }
            tick = Make("Menu tick", .06f, (t, u) => (Mathf.Sin(2 * Mathf.PI * 1650 * t) + .35f * Mathf.Sin(2 * Mathf.PI * 3300 * t)) * Mathf.Exp(-t / .012f) * .23f);
            // A clear tone with a soft bell partial and a lower body, a 5 ms rounded attack and a gentle fade: no click, no hiss.
            AudioClip Ping(string name, float hz) => Make(name, .6f, (t, u) =>
            {
                float attack = Mathf.SmoothStep(0, 1, t / .005f), tail = 1 - Mathf.SmoothStep(.85f, 1, u);
                float tone = Mathf.Sin(2 * Mathf.PI * hz * t) + .3f * Mathf.Sin(2 * Mathf.PI * hz * .5f * t) + .2f * Mathf.Sin(2 * Mathf.PI * hz * 2.01f * t) * Mathf.Exp(-t / .08f);
                return tone * attack * tail * Mathf.Exp(-t / .2f) * .55f;
            });
            pingOpen = Ping("Menu open", 1318.5f); pingClose = Ping("Menu close", 987.8f);   // E6 up, B5 down
        }

        void MarkWatch()
        {
            if (watchLeft) Mark(watchLeft, WristWatch.Worn == WristWatch.Hand.Left);
            if (watchRight) Mark(watchRight, WristWatch.Worn == WristWatch.Hand.Right);
        }
    }
}
