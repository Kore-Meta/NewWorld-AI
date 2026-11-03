using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace StudyEverywhere
{
    // Writing in the sand by the water at Seabreeze: a patch of beach round the shore spot where a fingertip (or a
    // controller's tip) pressed into the sand leaves a groove with soft lips, as in real damp sand. The marks live in a
    // small half-float map (_WriteMap over _WriteRect, read by the Seabreeze Sand shader); the waves that run over the
    // patch smooth them away bit by bit, and Smooth() (the menu) clears it all.
    public sealed class SandWriting : MonoBehaviour
    {
        [Tooltip("x, z of the patch's corner (world).")] public Vector2 corner = new Vector2(-1.5f, -1.5f);
        [Min(.5f)] public float size = 3;
        public int resolution = 512;
        [Tooltip("A fingertip this close above the sand already marks it (m).")] public float reach = .012f;
        [Tooltip("Groove half-width (m).")] public float radius = .008f;
        public Shader stampShader;
        public XROrigin origin;
        public Transform leftController, rightController;

        static readonly int MapId = Shader.PropertyToID("_WriteMap"), RectId = Shader.PropertyToID("_WriteRect"),
            StampRect = Shader.PropertyToID("_Rect"), FromId = Shader.PropertyToID("_From"), ToId = Shader.PropertyToID("_To"), RadiusId = Shader.PropertyToID("_Radius");
        RenderTexture front, back;
        Material stamp;
        readonly List<Vector3> tips = new List<Vector3>();
        readonly Vector4[] from = new Vector4[4], to = new Vector4[4];
        readonly Vector3[] last = new Vector3[4];
        readonly bool[] touching = new bool[4];
        float lowest;   // the patch's lowest sand: the wash only runs when the sea is above it
        readonly List<(Vector3 a, Vector3 b, float s)> queued = new List<(Vector3, Vector3, float)>();

        void OnEnable()
        {
            var format = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RHalf) ? RenderTextureFormat.RHalf : RenderTextureFormat.ARGBHalf;
            RenderTexture Make() { var r = new RenderTexture(resolution, resolution, 0, format) { name = "Sand writing", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear }; r.Create(); return r; }
            front = Make(); back = Make();
            if (!stampShader) stampShader = Shader.Find("Hidden/StudyEverywhere/Sand Stamp");
            stamp = new Material(stampShader) { hideFlags = HideFlags.HideAndDontSave };
            lowest = float.MaxValue;
            for (int i = 0; i <= 8; i++) for (int j = 0; j <= 8; j++) lowest = Mathf.Min(lowest, SeabreezeBeach.Sand(corner.x + size * i / 8, corner.y + size * j / 8));
            Smooth();
        }

        void OnDisable()
        {
            if (front) front.Release(); if (back) back.Release();
            if (stamp) Destroy(stamp);
            Shader.SetGlobalTexture(MapId, Texture2D.blackTexture);
        }

        public void Smooth()
        {
            foreach (var r in new[] { front, back }) { var was = RenderTexture.active; RenderTexture.active = r; GL.Clear(false, true, Color.clear); RenderTexture.active = was; }
            Publish();
        }

        void Publish()
        {
            Shader.SetGlobalTexture(MapId, front);
            Shader.SetGlobalVector(RectId, new Vector4(corner.x, corner.y, size, 1f / resolution));
        }

        // A stroke from code (previews): a to b, both on the sand.
        public void Stroke(Vector3 a, Vector3 b, float strength = 1) => queued.Add((a, b, strength));

        bool Inside(Vector3 p) => p.x > corner.x && p.z > corner.y && p.x < corner.x + size && p.z < corner.y + size;

        void Update()
        {
            int n = 0;
            for (int i = 0; i < 4; i++) from[i] = to[i] = Vector4.zero;
            SeaShore.ReadTips(origin, leftController, rightController, tips, indexOnly: true);
            for (int i = 0; i < 4; i++)
            {
                if (i >= tips.Count) { touching[i] = false; continue; }
                var p = tips[i];
                float above = p.y - SeabreezeBeach.Sand(p.x, p.z);
                bool down = Inside(p) && above < reach && above > -.08f;
                if (down && n < 4)
                {
                    // Pressing deeper makes a deeper groove (up to about a centimetre).
                    float strength = Mathf.Clamp01((reach - above) / (reach + .008f)) * .9f + .1f;
                    var a = touching[i] && Vector3.Distance(last[i], p) < .15f ? last[i] : p;
                    from[n] = new Vector4(a.x, a.z, 0, strength); to[n] = new Vector4(p.x, p.z, 0, 0); n++;
                }
                touching[i] = down; last[i] = p;
            }
            foreach (var (a, b, s) in queued) { if (n >= 4) break; from[n] = new Vector4(a.x, a.z, 0, s); to[n] = new Vector4(b.x, b.z, 0, 0); n++; }
            queued.Clear();

            bool washing = SeaShore.Level > lowest;
            if (n == 0 && !washing) return;
            stamp.SetVector(StampRect, new Vector4(corner.x, corner.y, size, Time.deltaTime));
            stamp.SetVectorArray(FromId, from); stamp.SetVectorArray(ToId, to);
            stamp.SetFloat(RadiusId, radius);
            Graphics.Blit(front, back, stamp);
            (front, back) = (back, front);
            Publish();
        }
    }
}
