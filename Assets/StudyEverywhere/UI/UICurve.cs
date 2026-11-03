using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StudyEverywhere
{
    // Bends a world-space panel gently round the viewer, like a curved monitor: every vertex is pushed toward the
    // viewer by how far it is from the panel's vertical centre line, on a cylinder of `radius` canvas units. Put
    // CurvedPanel on the canvas; it adds this to every Graphic under it. Images are cut into narrow columns first so
    // their edges curve smoothly; text glyphs are small and bend as they are (TextMeshPro meshes are bent after each
    // rebuild). The curve is mild, so pointer hits on the flat canvas still land on the right control.
    [ExecuteAlways, DisallowMultipleComponent]
    public sealed class UICurve : BaseMeshEffect
    {
        public CurvedPanel panel;
        static readonly List<UIVertex> stream = new List<UIVertex>(), output = new List<UIVertex>();
        TMP_Text tmp;

        protected override void OnEnable()
        {
            base.OnEnable();
            tmp = GetComponent<TMP_Text>();
            if (tmp) { TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged); tmp.ForceMeshUpdate(); BendText(); }
        }
        protected override void OnDisable()
        {
            if (tmp) TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
            base.OnDisable();
        }
        void OnTextChanged(Object obj) { if (obj == tmp) BendText(); }

        // Canvas-space sag (toward the viewer, -z) for a point given in this graphic's local space.
        Vector3 Bend(Vector3 local, Transform root, float radius)
        {
            var p = root.InverseTransformPoint(transform.TransformPoint(local));
            float x = Mathf.Clamp(p.x, -radius * .95f, radius * .95f);
            float sag = radius - Mathf.Sqrt(radius * radius - x * x);
            return local + transform.InverseTransformVector(root.TransformVector(new Vector3(0, 0, -sag)));
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || !panel || tmp) return;
            var root = panel.transform; float radius = panel.radius, step = panel.segment;
            stream.Clear(); output.Clear();
            vh.GetUIVertexStream(stream);
            // the stream is triangles; quads arrive as (0,1,2)(2,3,0): cut each into columns no wider than `step`
            for (int i = 0; i + 5 < stream.Count; i += 6)
            {
                UIVertex a = stream[i], b = stream[i + 1], c = stream[i + 2], d = stream[i + 4];
                bool quad = stream[i + 3].position == c.position && stream[i + 5].position == a.position;
                if (!quad) { for (int k = 0; k < 6; k++) output.Add(stream[i + k]); continue; }
                // a, b, c, d run round the quad; find its left and right edges by x
                float minX = Mathf.Min(Mathf.Min(a.position.x, b.position.x), Mathf.Min(c.position.x, d.position.x));
                float maxX = Mathf.Max(Mathf.Max(a.position.x, b.position.x), Mathf.Max(c.position.x, d.position.x));
                int cols = Mathf.Clamp(Mathf.CeilToInt((maxX - minX) * root.InverseTransformVector(transform.TransformVector(Vector3.right)).magnitude / step), 1, 32);
                for (int k = 0; k < cols; k++)
                {
                    float t0 = k / (float)cols, t1 = (k + 1) / (float)cols;
                    // a quad's corners in order: a (bottom-left), b (top-left), c (top-right), d (bottom-right)
                    UIVertex bl = Lerp(a, d, t0), tl = Lerp(b, c, t0), tr = Lerp(b, c, t1), br = Lerp(a, d, t1);
                    output.Add(bl); output.Add(tl); output.Add(tr); output.Add(tr); output.Add(br); output.Add(bl);
                }
            }
            for (int i = 0; i < output.Count; i++) { var v = output[i]; v.position = Bend(v.position, root, radius); output[i] = v; }
            vh.Clear();
            vh.AddUIVertexTriangleStream(output);
        }

        static UIVertex Lerp(UIVertex p, UIVertex q, float t) => new UIVertex
        {
            position = Vector3.Lerp(p.position, q.position, t), color = Color32.Lerp(p.color, q.color, t), normal = p.normal, tangent = p.tangent,
            uv0 = Vector4.Lerp(p.uv0, q.uv0, t), uv1 = Vector4.Lerp(p.uv1, q.uv1, t), uv2 = Vector4.Lerp(p.uv2, q.uv2, t), uv3 = Vector4.Lerp(p.uv3, q.uv3, t),
        };

        void BendText()
        {
            if (!tmp || !panel || !isActiveAndEnabled) return;
            var info = tmp.textInfo; if (info == null) return;
            var root = panel.transform; float radius = panel.radius;
            for (int m = 0; m < info.meshInfo.Length; m++)
            {
                var verts = info.meshInfo[m].vertices; if (verts == null) continue;
                for (int i = 0; i < verts.Length; i++) verts[i] = Bend(verts[i], root, radius);
            }
            tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
        }
    }
}
