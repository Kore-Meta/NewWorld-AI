using System;
using TMPro;
using UnityEngine;

namespace StudyEverywhere
{
    // The holodeck's Universe mode: a size comparison, one step at a time. Each step brings in something bigger at the
    // same size in the middle of the deck, and everything before it shrinks to true scale beside it (a star beside the
    // last star, the Sun dwindling to a dot, then whole systems drawn round the one before), from the Moon to the
    // observable universe (OrbitSolar.Steps). The zoom between steps runs on a logarithmic scale, so a step of a
    // thousand times feels like a step of ten. Arrows (or a sweep) go on or back; the Parts pill (here "Start") goes back
    // to the Moon. Each model is a unit-radius thing built by the builder; this only places and sizes them.
    public sealed class HoloUniverse : MonoBehaviour
    {
        public HoloDeck deck;
        [Tooltip("Everything in this mode; the deck grows and shrinks it.")] public Transform root;
        [Tooltip("One per OrbitSolar.Steps, each a unit-radius model about its own centre.")] public Transform[] things;
        [Tooltip("Every model renderer, with its real finish (null: always wireframe) and whether it glows like a star.")]
        public Renderer[] renderers; public Material[] real; public bool[] glowing;
        [Tooltip("The step's own thing is shown at this radius (metres).")] public float displayRadius = .17f;
        public float stepSeconds = 2.6f;
        [Tooltip("Smaller than this (metres) and a thing is not drawn.")] public float smallest = .0012f;
        [Tooltip("Things drawn larger than this radius, or farther out than this, are not drawn (so nothing grows over the card).")] public float largest = .45f, reach = .8f;
        [Tooltip("Each thing's name, a small canvas at its middle, turned to the viewer (one per step).")] public TMP_Text[] names;
        [Tooltip("What the mode is about to show, over the deck until the first step on.")] public GameObject intro;
        [Tooltip("Names are shown for this step and this many before it (more would crowd).")] public int namesBack = 3;
        bool started;

        int step, from;
        float t = 1;

        public int Current => step;
        public OrbitSolar.Step Step => OrbitSolar.Steps[step];

        void OnEnable() { Localization.Changed += Relabel; Layout(); Tell(); }
        void OnDisable() => Localization.Changed -= Relabel;
        // the card again in the new language (only while this mode is the one on the deck)
        void Relabel() { if (deck && deck.CurrentMode == HoloDeck.Mode.Universe) Tell(); }
        int told = -1;

        public void Go(int index)
        {
            index = Mathf.Clamp(index, 0, OrbitSolar.Steps.Length - 1);
            if (index == step && t >= 1) { Tell(); return; }
            from = Shown(); step = index; t = 0;
            if (index > 0) started = true;
            // the card changes when the zoom is halfway (Update), so it never describes one thing while showing another
        }
        public void Next(int dir) => Go(step + dir);
        // Straight to a step with no zoom (the editor's previews).
        public void Snap(int index) { started = index > 0; step = from = Mathf.Clamp(index, 0, OrbitSolar.Steps.Length - 1); t = 1; Layout(); Tell(); }
        public void Restart() { started = false; Go(0); }
        int Shown() => t < .5f ? from : step;
        public void Reskin()
        {
            if (!deck) return;
            for (int i = 0; i < renderers.Length; i++)
                deck.Dress(renderers[i], real != null && i < real.Length ? real[i] : null, glowing != null && i < glowing.Length && glowing[i], deck.Realistic);
        }

        void Tell()
        {
            if (!deck) return;
            var steps = OrbitSolar.Steps; var s = steps[step];
            var facts = new System.Collections.Generic.List<string> { OrbitSolar.SizeLine(s, true) };
            if (step > 0)
            {
                var p = steps[step - 1]; double k = s.radiusKm / p.radiusKm;
                string times = k < 10 ? k.ToString("0.0") : k.ToString("N0"), before = Localization.Current == 0 ? Lower(p.name) : Localization.Tr(p.name);
                facts.Add(Localization.Format(s.estimate || p.estimate ? "{0} times as wide as {1} (roughly)" : "{0} times as wide as {1}", times, before));
            }
            facts.Add(step < steps.Length - 1 ? Localization.Format("Next: {0}. Arrows or a sweep to go on or back.", Localization.Tr(steps[step + 1].name)) : Localization.Tr("The end of what we can see. Start goes back to the Moon."));
            deck.Info(Localization.Format("UNIVERSE · STEP {0} OF {1}", step + 1, steps.Length), Localization.Tr(s.name), Localization.Tr(s.kind), Localization.Tr(s.about), facts);
            told = step;
        }
        static string Lower(string name) => name.StartsWith("The ") ? "t" + name.Substring(1) : name;

        void Update()
        {
            if (t < 1) { t = Mathf.MoveTowards(t, 1, Time.deltaTime / stepSeconds); }
            if (t >= .5f && told != step) Tell();
            Layout();
            var head = Camera.main;
            // each shown name on its dark plate (refitted when a translation changes its length), turned to the viewer
            if (names != null) foreach (var n in names) if (n && n.transform.parent.gameObject.activeInHierarchy)
            {
                HoloLabel.Fit(n);
                if (head) { var c = n.transform.parent; var away = c.position - head.transform.position; if (away.sqrMagnitude > 1e-4f) c.rotation = Quaternion.LookRotation(away); }
            }
            // the intro panel too, upright, so its lines stay square to the person wherever the deck has been turned
            if (head && intro && intro.activeInHierarchy) { var away = intro.transform.position - head.transform.position; away.y = 0; if (away.sqrMagnitude > 1e-4f) intro.transform.rotation = Quaternion.LookRotation(away); }
        }

        // Every thing at true size relative to the others, the zoom eased (in log scale) between the last step and this
        // one, the view sliding from the one to the other. Small things sit in a row to the left, touching; a system
        // is drawn round the thing before it.
        void Layout()
        {
            if (intro && intro.activeSelf != (!started && step == 0)) intro.SetActive(!started && step == 0);
            var steps = OrbitSolar.Steps; int n = Mathf.Min(steps.Length, things.Length);
            float e = t * t * (3 - 2 * t);
            double logA = Math.Log(displayRadius / steps[from].radiusKm), logB = Math.Log(displayRadius / steps[step].radiusKm);
            double scale = Math.Exp(logA + (logB - logA) * e);   // metres per km
            int last = Mathf.Max(from, step);
            var x = new double[n];
            for (int i = 0; i < n; i++)
            {
                double r = steps[i].radiusKm * scale;
                if (i == 0 || steps[i].nested) x[i] = i == 0 ? 0 : x[i - 1];
                else x[i] = x[i - 1] + steps[i - 1].radiusKm * scale + r * 1.15;   // each to the right of the last, so earlier ones line up leftward
            }
            double centre = x[from] + (x[step] - x[from]) * e;
            for (int i = 0; i < n; i++)
            {
                var th = things[i]; if (!th) continue;
                double r = steps[i].radiusKm * scale, pos = x[i] - centre;
                bool show = i <= last && r > smallest && r < largest && Math.Abs(pos) - r < reach;
                if (th.gameObject.activeSelf != show) th.gameObject.SetActive(show);
                // its name in its middle, on the side facing the person (for this step and a few before it)
                var label = names != null && i < names.Length && names[i] ? names[i].transform.parent : null;
                bool named = show && r > .015f && i <= Shown() && i >= Shown() - namesBack;
                if (label && label.gameObject.activeSelf != named) label.gameObject.SetActive(named);
                if (!show) continue;
                th.localPosition = new Vector3((float)pos, 0, 0);
                th.localScale = Vector3.one * (float)r;
                if (named) label.localPosition = new Vector3((float)pos, 0, -(float)(steps[i].nested ? Math.Min(r * .1, .03) : r * 1.04) - .005f);
            }
        }
    }
}
