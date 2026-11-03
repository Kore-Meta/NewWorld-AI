using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace StudyEverywhere
{
    // The ring round Calypso's orb while she acts: a faint full circle with an amber arc filling round it over the time
    // the beach takes to change, then fading. CalypsoCompanion adds this at start if the scene has none.
    public sealed class CalypsoRing : MonoBehaviour
    {
        [Tooltip("Her orb (the ring is centred on it, in her plane).")] public Transform from;
        [Tooltip("Orb radius in metres; the ring sits just outside it.")] public float radius = .06f;
        [Tooltip("Metres per canvas unit.")] public float scale = .0005f;
        public Color colour = new Color(1f, .82f, .6f, 1f);

        RectTransform root; CanvasGroup group; Image arc;
        Coroutine running;

        public void Run(float seconds)
        {
            if (!from) return;
            if (!root) Build();
            if (running != null) StopCoroutine(running);
            running = StartCoroutine(Fill(seconds));
        }

        void Build()
        {
            var parent = from.parent ? from.parent : from;
            var go = new GameObject("Calypso's ring | while she acts", typeof(RectTransform));
            var canvas = go.AddComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace; canvas.sortingOrder = 39;
            go.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 4;
            group = go.AddComponent<CanvasGroup>(); group.alpha = 0; group.blocksRaycasts = false; group.interactable = false;
            root = (RectTransform)go.transform; root.SetParent(parent, false);
            float d = (radius + .018f) * 2 / scale;
            root.sizeDelta = new Vector2(d, d); root.localScale = Vector3.one * scale; root.localRotation = Quaternion.identity;
            var c = parent.InverseTransformPoint(from.position); c.z -= .005f; root.localPosition = c;
            Image Layer(string n, Color col)
            {
                var img = new GameObject(n, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
                var r = (RectTransform)img.transform; r.SetParent(root, false);
                r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero;
                img.sprite = CalypsoIcons.Ring(); img.color = col; img.raycastTarget = false;
                return img;
            }
            Layer("Track", new Color(1, 1, 1, .22f));
            arc = Layer("Arc", colour);
            arc.type = Image.Type.Filled; arc.fillMethod = Image.FillMethod.Radial360; arc.fillOrigin = (int)Image.Origin360.Top; arc.fillClockwise = true; arc.fillAmount = 0;
        }

        IEnumerator Fill(float seconds)
        {
            arc.fillAmount = 0;
            for (float t = 0; t < seconds; t += Time.deltaTime)
            {
                group.alpha = Mathf.MoveTowards(group.alpha, 1, Time.deltaTime * 4);
                arc.fillAmount = Mathf.SmoothStep(0, 1, t / seconds);
                yield return null;
            }
            arc.fillAmount = 1;
            for (float a = group.alpha; a > 0; a -= Time.deltaTime / .5f) { group.alpha = a; yield return null; }
            group.alpha = 0; running = null;
        }
    }
}
