using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StudyEverywhere
{
    // Calypso's cues: small icons with the words to say, flowing out of her orb along a trail of her droplets, holding a
    // moment and fading, so the person learns what to say without a menu. Everything is still done by voice; the cues
    // only show the words.
    //  - ShowAbilities: her five abilities fan out round her one by one as she names them; the newest lights up and the
    //    earlier ones dim, and the companion previews each in the world (her first hello, or "what can you do?").
    //  - ShowSteps: the icons of a request with several actions, in order in a row under her (the echo back); the
    //    companion lights each as it is done and releases them.
    //  - Idle hints: until she has been asked for anything once, every so often one full phrase drifts out of her
    //    ("Calypso, calm the waves"), teaching her name and an ability together. They stop for good after first use.
    // CalypsoCompanion adds this beside her orb at start if the scene has none, and sets `quiet` while she is busy.
    public sealed class CalypsoCues : MonoBehaviour
    {
        [Tooltip("Where cues come out from: her orb.")] public Transform from;
        public TMP_FontAsset font;
        [Tooltip("Her droplets, for the trail each cue flows out along (optional).")] public CalypsoGuide guide;
        [Tooltip("Metres per canvas unit (her name uses the same).")] public float scale = .0005f;
        [Tooltip("How far out from her centre a cue settles, in metres.")] public float reach = .11f;
        [Tooltip("Until she has been asked for anything: seconds between idle hints (0: none).")] public float hintEvery = 60;

        public struct Cue
        {
            public string say; public Color tint; public Vector2 at; public bool hasIcon; public CalypsoIcons.Kind icon;
            // `at`: where it settles, in metres from her centre, in her plane (x right, y up)
            public Cue(string say, Color tint, Vector2 at, CalypsoIcons.Kind icon) { this.say = say; this.tint = tint; this.at = at; hasIcon = true; this.icon = icon; }
            public Cue(string say, Color tint, Vector2 at) { this.say = say; this.tint = tint; this.at = at; hasIcon = false; icon = default; }
        }

        // A cue on show: dim it, make it pop, or let it go.
        public sealed class Handle
        {
            internal GameObject go; internal float level = 1, pop; internal bool released;
            public bool Alive => go;
            public void SetLevel(float l) => level = Mathf.Clamp01(l);
            public void Pop() => pop = 1;
            public void Release() => released = true;
        }

        // The relay's actions, as the person would say them; each settles on its own side of her.
        public static readonly string[] AbilityActions = { "time sunset", "cycle on", "hatch", "waves off", "fire off" };
        static readonly string[] AbilityWords = { "“make it sunset”", "“start the day cycle”", "“hatch the turtles”", "“calm the waves”", "“put out the fire”" };
        static readonly Vector2[] AbilityDirs = { new Vector2(-1, -.15f), new Vector2(-.75f, .65f), new Vector2(0, 1), new Vector2(.75f, .65f), new Vector2(1, -.15f) };
        static readonly string[] Hints = { "“Calypso, calm the waves”", "“Calypso, hatch the turtles”", "“Calypso, make it sunset”", "“Calypso, light the fire”" };
        static readonly string[] HintActions = { "waves off", "hatch", "time sunset", "fire on" };

        // The icon and colour of an action ("hatch", "time sunset", "waves off"...), shared with the echo back.
        public static bool Look(string action, out CalypsoIcons.Kind icon, out Color tint)
        {
            var a = (action ?? "").Trim().ToLowerInvariant();
            icon = default; tint = Color.white;
            if (a.StartsWith("hatch")) { icon = CalypsoIcons.Kind.Turtle; tint = new Color(.82f, 1f, .87f); return true; }
            if (a.StartsWith("time")) { icon = CalypsoIcons.Kind.Sun; tint = new Color(1f, .87f, .68f); return true; }
            if (a.StartsWith("cycle")) { icon = CalypsoIcons.Kind.Cycle; tint = new Color(1f, .91f, .76f); return true; }
            if (a.StartsWith("waves")) { icon = CalypsoIcons.Kind.Waves; tint = new Color(.84f, .94f, 1f); return true; }
            if (a.StartsWith("fire")) { icon = CalypsoIcons.Kind.Fire; tint = new Color(1f, .82f, .7f); return true; }
            return false;
        }

        const string UsedKey = "Calypso.used";
        public static bool Used => PlayerPrefs.GetInt(UsedKey, 0) == 1;
        public static void MarkUsed() { if (Used) return; PlayerPrefs.SetInt(UsedKey, 1); PlayerPrefs.Save(); }

        [HideInInspector] public bool quiet;   // CalypsoCompanion: true while she listens, thinks or talks
        float nextHint; int hint;

        void Start() => nextHint = Time.time + hintEvery;

        void Update()
        {
            if (hintEvery <= 0 || Used || !from) return;
            if (quiet) { nextHint = Mathf.Max(nextHint, Time.time + hintEvery * .5f); return; }
            if (Time.time < nextHint) return;
            nextHint = Time.time + hintEvery;
            int i = hint++ % Hints.Length;
            Look(HintActions[i], out var icon, out _);
            Show(new Cue(Localization.Tr(Hints[i]), Color.white, new Vector2(-.85f, -.55f) * reach, icon), 5);
        }

        // The five abilities, fanning out one by one `gap` seconds apart; `named(i)` is called as each appears (for the
        // world's preview). They all leave together `hold` seconds after the last.
        public void ShowAbilities(float startDelay = 0, float gap = 1.1f, float hold = 5, Action<int> named = null) => StartCoroutine(Abilities(startDelay, gap, hold, named));

        IEnumerator Abilities(float startDelay, float gap, float hold, Action<int> named)
        {
            if (startDelay > 0) yield return new WaitForSeconds(startDelay);
            var shown = new List<Handle>();
            for (int i = 0; i < AbilityActions.Length; i++)
            {
                Look(AbilityActions[i], out var icon, out var tint);
                var d = AbilityDirs[i].normalized;
                foreach (var h in shown) h.SetLevel(.6f);   // the one she is naming is the bright one
                var cue = Show(new Cue(Localization.Tr(AbilityWords[i]), tint, new Vector2(d.x * reach, d.y * reach * .8f), icon), -1);
                cue.Pop(); shown.Add(cue);
                named?.Invoke(i);
                yield return new WaitForSeconds(gap);
            }
            foreach (var h in shown) h.SetLevel(1);
            yield return new WaitForSeconds(hold);
            foreach (var h in shown) h.Release();
        }

        // The steps of a request, as icons in a row under her, in order, all dim until the companion lights them.
        public Handle[] ShowSteps(IList<string> actions)
        {
            var hs = new Handle[actions.Count];
            for (int i = 0; i < actions.Count; i++)
            {
                if (!Look(actions[i], out var icon, out var tint)) { icon = CalypsoIcons.Kind.Sun; }
                var at = new Vector2((i - (actions.Count - 1) * .5f) * .075f, -reach * .95f);
                hs[i] = Show(new Cue("", tint, at, icon), -1, i * .35f);
            }
            return hs;
        }

        // Show one cue; hold < 0 keeps it until its handle is released.
        public Handle Show(Cue cue, float hold, float delay = 0)
        {
            var h = new Handle();
            StartCoroutine(Run(cue, hold, delay, h));
            return h;
        }

        IEnumerator Run(Cue cue, float hold, float delay, Handle h)
        {
            if (delay > 0) yield return new WaitForSeconds(delay);
            if (!from || h.released) yield break;
            var parent = from.parent ? from.parent : from;
            var go = h.go = new GameObject("Calypso's cue | " + (string.IsNullOrEmpty(cue.say) ? cue.icon.ToString() : cue.say), typeof(RectTransform));
            var canvas = go.AddComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace; canvas.sortingOrder = 42;
            go.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 4;
            var group = go.AddComponent<CanvasGroup>(); group.alpha = 0; group.blocksRaycasts = false; group.interactable = false;
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false); rt.localRotation = Quaternion.identity; rt.localScale = Vector3.one * scale;
            Layout(rt, cue);

            // out of the centre of the orb, a little in front of it, along a trail of her droplets, to its resting place
            var centre = parent.InverseTransformPoint(from.position); centre.z -= .01f;
            var end = centre + new Vector3(cue.at.x, cue.at.y, 0);
            if (guide) guide.Thread(from.position, parent.TransformPoint(end), 1f, new Color(cue.tint.r, cue.tint.g, cue.tint.b, .9f));
            for (float a = 0; a < 1; a += Time.deltaTime / .8f)
            {
                if (!go) yield break;
                float e = 1 - (1 - a) * (1 - a);
                rt.localPosition = Vector3.Lerp(centre, end, e); group.alpha = e * h.level;
                yield return null;
            }
            rt.localPosition = end;
            float until = hold >= 0 ? Time.time + hold : float.MaxValue;
            while (!h.released && Time.time < until)
            {
                if (!go) yield break;
                group.alpha = Mathf.MoveTowards(group.alpha, h.level, Time.deltaTime * 2.5f);
                h.pop = Mathf.MoveTowards(h.pop, 0, Time.deltaTime * 2);
                rt.localScale = Vector3.one * scale * (1 + .18f * h.pop * h.pop);
                yield return null;
            }
            for (float a = group.alpha; a > 0; a -= Time.deltaTime / .6f) { if (!go) yield break; group.alpha = a; yield return null; }
            Destroy(go);
        }

        // Icon on the side nearest her, the words beyond it; above or below her, the icon nearest and the words past it.
        void Layout(RectTransform rt, Cue cue)
        {
            const float icon = 52, gap = 10, width = 480, line = 60;
            bool words = !string.IsNullOrEmpty(cue.say);
            bool left = cue.at.x < -.02f, right = cue.at.x > .02f, above = cue.at.y >= 0;
            if (!words) { rt.pivot = new Vector2(.5f, .5f); rt.sizeDelta = new Vector2(icon, icon); }
            else if (left) { rt.pivot = new Vector2(1, .5f); rt.sizeDelta = new Vector2(width, line); }
            else if (right) { rt.pivot = new Vector2(0, .5f); rt.sizeDelta = new Vector2(width, line); }
            else { rt.pivot = new Vector2(.5f, above ? 0 : 1); rt.sizeDelta = new Vector2(width, icon + gap + line); }

            if (cue.hasIcon)
            {
                var img = new GameObject("Icon", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
                var ir = (RectTransform)img.transform; ir.SetParent(rt, false); ir.sizeDelta = new Vector2(icon, icon);
                Vector2 anchor = !words ? new Vector2(.5f, .5f) : left ? new Vector2(1, .5f) : right ? new Vector2(0, .5f) : new Vector2(.5f, above ? 0 : 1);
                ir.anchorMin = ir.anchorMax = ir.pivot = anchor; ir.anchoredPosition = Vector2.zero;
                img.sprite = CalypsoIcons.Get(cue.icon); img.color = cue.tint; img.raycastTarget = false; img.preserveAspect = true;
            }
            if (!words) return;
            var t = new GameObject("Say", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            var tr = (RectTransform)t.transform; tr.SetParent(rt, false);
            tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero;
            float inset = cue.hasIcon ? icon + gap : 0;
            if (left) tr.offsetMax = new Vector2(-inset, 0);
            else if (right) tr.offsetMin = new Vector2(inset, 0);
            else if (above) tr.offsetMin = new Vector2(0, inset);
            else tr.offsetMax = new Vector2(0, -inset);
            if (font) { t.font = font; t.fontSharedMaterial = font.material; }
            t.text = cue.say; t.fontSize = 26; t.fontStyle = FontStyles.Italic; t.color = cue.tint; t.raycastTarget = false;
            t.alignment = left ? TextAlignmentOptions.Right : right ? TextAlignmentOptions.Left : TextAlignmentOptions.Center;
            t.textWrappingMode = TextWrappingModes.NoWrap; t.overflowMode = TextOverflowModes.Overflow;
            t.outlineWidth = .2f; t.outlineColor = new Color32(14, 52, 62, 210);   // as her name: legible over sky and sea
        }
    }
}
