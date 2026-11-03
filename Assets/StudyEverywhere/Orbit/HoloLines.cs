using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace StudyEverywhere
{
    // Glowing lines at run time, for the Holo Wire shader: each segment is a quad whose vertices carry their own end,
    // the other end (UV1) and their side (UV0.x), and the shader opens the quad toward the eye. Buffers are kept, so
    // redrawing every frame makes no garbage. Add segments, then Apply to a mesh.
    public sealed class HoloLines
    {
        readonly List<Vector3> verts = new List<Vector3>(), other = new List<Vector3>();
        readonly List<Vector2> uv = new List<Vector2>();
        readonly List<int> idx = new List<int>();
        Bounds bounds; bool any;

        public int Count => verts.Count / 4;
        public void Clear() { verts.Clear(); other.Clear(); uv.Clear(); idx.Clear(); any = false; }
        public void Add(Vector3 a, Vector3 b)
        {
            int s = verts.Count;
            verts.Add(a); other.Add(b); uv.Add(new Vector2(-1, 0));
            verts.Add(a); other.Add(b); uv.Add(new Vector2(1, 0));
            verts.Add(b); other.Add(a); uv.Add(new Vector2(-1, 1));
            verts.Add(b); other.Add(a); uv.Add(new Vector2(1, 1));
            idx.Add(s); idx.Add(s + 1); idx.Add(s + 2); idx.Add(s); idx.Add(s + 2); idx.Add(s + 3);
            if (!any) { bounds = new Bounds(a, Vector3.zero); any = true; }
            bounds.Encapsulate(a); bounds.Encapsulate(b);
        }
        // A polyline through the points.
        public void Add(IList<Vector3> pts, bool closed = false)
        {
            for (int i = 1; i < pts.Count; i++) Add(pts[i - 1], pts[i]);
            if (closed && pts.Count > 2) Add(pts[pts.Count - 1], pts[0]);
        }
        public void Apply(Mesh m)
        {
            m.Clear();
            m.indexFormat = verts.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            m.SetVertices(verts); m.SetUVs(0, uv); m.SetUVs(1, other); m.SetTriangles(idx, 0, false);
            if (any) { var b = bounds; b.Expand(.02f); m.bounds = b; }
        }
    }

    // A hologram label (the builder's small canvas: a dark rounded "Plate" behind a bold outlined name) kept fitted to
    // its words: whenever the text changes (a translation, a live number) the plate is sized to it again, so the words
    // always sit on a dark backing and read over a bright planet, a glowing wireframe or the Earth in the window.
    public static class HoloLabel
    {
        public const string PlateName = "Plate";
        const float PadX = 22, PadY = 10, MinHeight = 60;
        static readonly Dictionary<TMP_Text, (string text, Vector2 size)> fitted = new Dictionary<TMP_Text, (string, Vector2)>();
        // Fits the plate and returns its size in the canvas's own units (multiply by the canvas's scale for metres).
        public static Vector2 Fit(TMP_Text t)
        {
            if (!t) return Vector2.zero;
            var s = t.text ?? "";
            if (fitted.TryGetValue(t, out var have) && have.text == s) return have.size;
            var pref = s.Length == 0 ? Vector2.zero : t.GetPreferredValues(s);
            var size = new Vector2(s.Length == 0 ? 0 : Mathf.Ceil(pref.x) + PadX * 2, Mathf.Max(MinHeight, Mathf.Ceil(pref.y) + PadY * 2));
            var plate = t.transform.parent ? t.transform.parent.Find(PlateName) as RectTransform : null;
            if (plate) { plate.sizeDelta = size; plate.anchoredPosition = t.rectTransform.anchoredPosition; if (plate.gameObject.activeSelf != s.Length > 0) plate.gameObject.SetActive(s.Length > 0); }
            fitted[t] = (s, size);
            return size;
        }
    }
}
