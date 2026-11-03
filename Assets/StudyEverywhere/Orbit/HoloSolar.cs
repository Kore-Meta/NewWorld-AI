using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StudyEverywhere
{
    // The holodeck's Solar System mode. The overview is an orrery over the table: the Sun in the middle and the eight
    // planets and Pluto going round it on their orbit lines, the inner ones fastest, each turning on its own tilted
    // axis (sizes, distances and speeds are not to scale; the card says so). Pick a planet (poke or pinch it, or step
    // with the arrows) and the orrery folds away and that world rises large in the middle with its major moons circling
    // it; pick a moon for its story. The deck's Parts pill (here "System") goes back to the orrery. The builder makes
    // the models (OrbitBuilder, SolarBuilder); OrbitSolar holds what the card says.
    public sealed class HoloSolar : MonoBehaviour
    {
        [Serializable] public class Orbiter
        {
            [Tooltip("Turns about its Y axis: carries the body round its orbit.")] public Transform pivot;
            [Tooltip("Turns about its own Y axis: the body's day.")] public Transform spin;
            public float lapSeconds, daySeconds;
        }
        [Serializable] public class Item
        {
            public string id;
            public Orbiter orbit;
            [Tooltip("The large model shown when this body is picked, with its moons.")] public Transform focus;
            public Orbiter focusGlobe;
            public Orbiter[] moons;
        }

        public HoloDeck deck;
        [Tooltip("Everything in this mode; the deck grows and shrinks it.")] public Transform root;
        [Tooltip("The orrery.")] public Transform overview;
        [Tooltip("Sun first, then the planets in order, then Pluto (as OrbitSolar.Bodies).")] public Item[] items;
        [Tooltip("Every model renderer, with its real finish (null: always wireframe) and whether it is lit like the Sun.")]
        public Renderer[] renderers; public Material[] real; public bool[] glowing;
        [Tooltip("Name labels: kept facing the viewer.")] public Transform[] labels;
        public float foldSeconds = .6f;

        int picked = -1, moon = -1;
        float overviewShown = 1;
        float[] focusShown;

        public int Picked => picked;
        public OrbitSolar.Body Body => picked >= 0 && picked < OrbitSolar.Bodies.Length ? OrbitSolar.Bodies[picked] : null;

        void Awake()
        {
            focusShown = new float[items.Length];
            foreach (var it in items) if (it.focus) { it.focus.localScale = Vector3.zero; it.focus.gameObject.SetActive(false); }
        }
        void OnEnable() { Localization.Changed += Relabel; Tell(); }
        void OnDisable() => Localization.Changed -= Relabel;
        // the card again in the new language (only while this mode is the one on the deck)
        void Relabel() { if (deck && deck.CurrentMode == HoloDeck.Mode.Solar) Tell(); }
        static string T(string english) => Localization.Tr(english);

        // ---- control (HoloDeck, HoloPick)
        public void Pick(int body, int moonIndex = -1)
        {
            if (body < -1 || body >= items.Length) return;
            picked = body; moon = body >= 0 ? moonIndex : -1;
            Tell();
        }
        public void ShowSystem() => Pick(-1);
        // Straight to the orrery (-1) or a world up close, with no folding (the editor's previews).
        public void Snap(int body)
        {
            if (focusShown == null || focusShown.Length != items.Length) focusShown = new float[items.Length];
            Pick(body);
            overviewShown = body < 0 ? 1 : 0; Size(overview, overviewShown);
            for (int i = 0; i < items.Length; i++) { focusShown[i] = i == body ? 1 : 0; Size(items[i].focus, focusShown[i]); }
        }
        // The arrows: from the orrery, the Sun; then out planet by planet, and round again.
        public void Step(int dir)
        {
            int n = items.Length;
            Pick(picked < 0 ? (dir > 0 ? 0 : n - 1) : (picked + dir + n) % n);
        }
        public void Reskin()
        {
            if (!deck) return;
            for (int i = 0; i < renderers.Length; i++)
                deck.Dress(renderers[i], real != null && i < real.Length ? real[i] : null, glowing != null && i < glowing.Length && glowing[i], deck.Realistic);
        }

        // What the card says now.
        void Tell()
        {
            if (!deck) return;
            var b = Body;
            if (b == null)
            {
                deck.Info(T("SOLAR SYSTEM"), T("The Solar System"), T("The Sun, eight planets and the dwarf planet Pluto"),
                    T("Everything here goes round the Sun, the inner planets fastest. Poke or pinch any world, or use the arrows, to bring it up close with its biggest moons."),
                    new[] { T("Sizes, distances and speeds are not to scale: true to scale, the Earth would be a grain of sand 10 metres from a beach-ball Sun."), T("Figures from NASA's planetary fact sheets and NASA/JPL.") });
                return;
            }
            if (moon >= 0 && moon < b.moons.Length)
            {
                var m = b.moons[moon];
                string planet = Localization.Current == 0 ? b.name.Replace("The ", "") : T(b.name);
                deck.Info(Localization.Format("MOON OF {0}", planet).ToUpperInvariant(), T(m.name), Localization.Format("{0:N0} km across", m.radiusKm * 2),
                    T(m.about),
                    new[] { Localization.Format("Distance from {0}: {1:N0} km", T(b.name), m.orbitKm), Localization.Format(m.periodDays < 0 ? "Goes round every {0} days, backwards" : "Goes round every {0} days", Mathf.Abs(m.periodDays).ToString("0.##")), T("Pick the planet to go back to it.") });
                return;
            }
            var facts = new List<string>(b.facts.Select(T));
            if (b.moons.Length > 0) facts.Add(Localization.Format("Shown circling it: {0}", string.Join(Localization.Code == "ja" ? "、" : ", ", b.moons.Select(x => T(x.name)))));
            if (!string.IsNullOrEmpty(b.explorers)) facts.Add(Localization.Format("Explorers: {0}", T(b.explorers)));
            facts.Add(T("Moon sizes true to the planet; their distances not to scale."));
            int k = Array.IndexOf(OrbitSolar.Bodies, b);
            string kicker = b.id == "sun" ? T("SOLAR SYSTEM · OUR STAR") : b.id == "pluto" ? T("SOLAR SYSTEM · DWARF PLANET") : Localization.Format("SOLAR SYSTEM · PLANET {0} FROM THE SUN", k);
            deck.Info(kicker, T(b.name), T(b.kind), T(b.summary), facts);
        }

        void Update()
        {
            float dt = Time.deltaTime;
            // the orrery keeps turning (even while folded, so it comes back where it would be)
            foreach (var it in items) Turn(it.orbit, dt);
            // fold one view away before the next opens
            bool anyFocus = false; for (int i = 0; i < focusShown.Length; i++) if (focusShown[i] > .02f && i != picked) anyFocus = true;
            overviewShown = Mathf.MoveTowards(overviewShown, picked < 0 && !anyFocus ? 1 : 0, dt / foldSeconds);
            Size(overview, overviewShown);
            for (int i = 0; i < items.Length; i++)
            {
                var it = items[i]; if (!it.focus) continue;
                bool want = i == picked && overviewShown < .02f && !OtherFocus(i);
                focusShown[i] = Mathf.MoveTowards(focusShown[i], want ? 1 : 0, dt / foldSeconds);
                Size(it.focus, focusShown[i]);
                if (focusShown[i] > 0) { Turn(it.focusGlobe, dt); if (it.moons != null) foreach (var m in it.moons) Turn(m, dt); }
            }
            var head = Camera.main;
            if (head && labels != null) foreach (var l in labels) if (l && l.gameObject.activeInHierarchy) { var away = l.position - head.transform.position; if (away.sqrMagnitude > 1e-4f) l.rotation = Quaternion.LookRotation(away); }
        }
        bool OtherFocus(int i) { for (int k = 0; k < focusShown.Length; k++) if (k != i && focusShown[k] > .02f) return true; return false; }
        static void Turn(Orbiter o, float dt)
        {
            if (o == null) return;
            if (o.pivot && o.lapSeconds != 0) o.pivot.Rotate(0, 360 * dt / o.lapSeconds, 0, Space.Self);
            if (o.spin && o.daySeconds != 0) o.spin.Rotate(0, 360 * dt / o.daySeconds, 0, Space.Self);
        }
        static void Size(Transform t, float shown)
        {
            if (!t) return;
            float e = shown * shown * (3 - 2 * shown);
            t.localScale = Vector3.one * e;
            bool on = shown > 0;
            if (t.gameObject.activeSelf != on) t.gameObject.SetActive(on);
        }
    }
}
