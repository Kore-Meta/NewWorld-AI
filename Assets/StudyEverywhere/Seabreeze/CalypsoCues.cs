using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StudyEverywhere
{
    // Calypso's cues: short spoken phrases that flow out of her orb, hold a moment and fade, so the person learns what to
    // say without a menu. Everything is still done by voice; the cues only show the words. Two uses:
    //  - ShowAbilities: the five things she can do fan out round her, one after another (her first hello, or when asked
    //    "what can you do?").
    //  - Idle hints: until she has been asked for anything once, every so often one full phrase drifts out of her
    //    ("Calypso, calm the waves"), teaching her name and an ability together. They stop for good after first use.
    // CalypsoCompanion adds this beside her orb at start if the scene has none, and sets `quiet` while she is busy.
    public sealed class CalypsoCues : MonoBehaviour
    {
        [Tooltip("Where cues come out from: her orb.")] public Transform from;
        public TMP_FontAsset font;
        [Tooltip("Metres per canvas unit (her name uses the same).")] public float scale = .0005f;
        [Tooltip("How far out from her centre a cue settles, in metres.")] public float reach = .11f;
        [Tooltip("Until she has been asked for anything: seconds between idle hints (0: none).")] public float hintEvery = 60;

        public struct Cue
        {
            public string say; public Color tint; public Vector2 dir;
            public Cue(string say, Color tint, Vector2 dir) { this.say = say; this.tint = tint; this.dir = dir; }
        }

        // The relay's actions, as the person would say them; each settles on its own side of her.
        public static readonly Cue[] Abilities =
        {
            new Cue("“make it sunset”", new Color(1f, .87f, .68f), new Vector2(-1, -.15f)),
            new Cue("“start the day cycle”", new Color(1f, .91f, .76f), new Vector2(-.75f, .65f)),
            new Cue("“hatch the turtles”", new Color(.82f, 1f, .87f), new Vector2(0, 1)),
            new Cue("“calm the waves”", new Color(.84f, .94f, 1f), new Vector2(.75f, .65f)),
            new Cue("“put out the fire”", new Color(1f, .82f, .7f), new Vector2(1, -.15f)),
        };
        static readonly string[] Hints = { "“Calypso, calm the waves”", "“Calypso, hatch the turtles”", "“Calypso, make it sunset”", "“Calypso, light the fire”" };

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
            Show(new Cue(Localization.Tr(Hints[hint++ % Hints.Length]), Color.white, new Vector2(-.85f, -.55f)), 0, 5);
        }

        // The five abilities, fanning out one by one; they all leave together.
        public void ShowAbilities(float startDelay = 0, float gap = 1.1f, float hold = 5)
        {
            for (int i = 0; i < Abilities.Length; i++)
            {
                var c = Abilities[i]; c.say = Localization.Tr(c.say);
                Show(c, startDelay + i * gap, hold + (Abilities.Length - 1 - i) * gap);
            }
        }

        public void Show(Cue cue, float delay, float hold) => StartCoroutine(Run(cue, delay, hold));

        IEnumerator Run(Cue cue, float delay, float hold)
        {
            if (delay > 0) yield return new WaitForSeconds(delay);
            if (!from) yield break;
            var parent = from.parent ? from.parent : from;
            var go = new GameObject("Calypso's cue | " + cue.say, typeof(RectTransform));
            var canvas = go.AddComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace; canvas.sortingOrder = 42;
            go.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 4;
            var group = go.AddComponent<CanvasGroup>(); group.alpha = 0; group.blocksRaycasts = false; group.interactable = false;
            var rt = (RectTransform)go.transform;
            var dir = cue.dir.sqrMagnitude > 1e-4f ? cue.dir.normalized : Vector2.up;
            // a cue to her left ends at its point, one to her right starts at it, one above or below is centred on it
            float px = dir.x < -.3f ? 1 : dir.x > .3f ? 0 : .5f;
            rt.pivot = new Vector2(px, .5f); rt.sizeDelta = new Vector2(440, 56); rt.localScale = Vector3.one * scale;
            rt.SetParent(parent, false); rt.localRotation = Quaternion.identity;

            var t = new GameObject("Say", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            var tr = (RectTransform)t.transform; tr.SetParent(rt, false);
            tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero;
            if (font) { t.font = font; t.fontSharedMaterial = font.material; }
            t.text = cue.say; t.fontSize = 26; t.fontStyle = FontStyles.Italic; t.color = cue.tint; t.raycastTarget = false;
            t.alignment = px > .9f ? TextAlignmentOptions.Right : px < .1f ? TextAlignmentOptions.Left : TextAlignmentOptions.Center;
            t.textWrappingMode = TextWrappingModes.NoWrap; t.overflowMode = TextOverflowModes.Overflow;
            t.outlineWidth = .2f; t.outlineColor = new Color32(14, 52, 62, 210);   // as her name: legible over sky and sea

            // out of the centre of the orb, a little in front of it, to its resting place
            var centre = parent.InverseTransformPoint(from.position); centre.z -= .01f;
            var end = centre + new Vector3(dir.x * reach, dir.y * reach * .8f, 0);
            for (float a = 0; a < 1; a += Time.deltaTime / .8f)
            {
                if (!go) yield break;
                float e = 1 - (1 - a) * (1 - a);
                rt.localPosition = Vector3.Lerp(centre, end, e); group.alpha = e;
                yield return null;
            }
            rt.localPosition = end; group.alpha = 1;
            yield return new WaitForSeconds(hold);
            for (float a = 1; a > 0; a -= Time.deltaTime / .6f) { if (!go) yield break; group.alpha = a; yield return null; }
            Destroy(go);
        }
    }
}
