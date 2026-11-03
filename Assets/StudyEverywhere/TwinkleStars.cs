using UnityEngine;
using UnityEngine.Rendering;

namespace StudyEverywhere
{
    // Stars that sparkle, over any place's baked sky (StudyEverywhere/Twinkle Stars). Builds a field of small sprites on
    // a far sphere round this object, more of them dim than bright, in the warm-white-blue of real stars, and only
    // above `minElevation` (a horizon, trees or a lake can hide the rest). With a TimeOfDay it shows them only at night
    // (its Evening, from dusk), fading in and out with the clock; without one they always show (Orbit, the meadow).
    [ExecuteAlways]
    public sealed class TwinkleStars : MonoBehaviour
    {
        public TimeOfDay time;
        public Material material;
        public int count = 700;
        public float radius = 1200;
        [Tooltip("Degrees above the horizon below which no stars are placed.")] public float minElevation = -90;
        [Tooltip("Apparent size of the brightest stars, in degrees.")] public float size = .16f;
        public int seed = 20261017;
        [Tooltip("Fade-in by the clock's Evening (0 day, 1 night): stars appear above this.")] public float showFrom = .55f;

        static readonly int Visible = Shader.PropertyToID("_Visible");
        MaterialPropertyBlock block;
        MeshRenderer mr;

        void OnEnable()
        {
            // (not ?? : a missing component is Unity's fake null)
            if (!TryGetComponent(out MeshFilter mf)) mf = gameObject.AddComponent<MeshFilter>();
            if (!TryGetComponent(out mr)) mr = gameObject.AddComponent<MeshRenderer>();
            mr.sharedMaterial = material; mr.shadowCastingMode = ShadowCastingMode.Off; mr.receiveShadows = false;
            mr.lightProbeUsage = LightProbeUsage.Off; mr.reflectionProbeUsage = ReflectionProbeUsage.Off;
            if (!mf.sharedMesh || mf.sharedMesh.vertexCount != count * 4) mf.sharedMesh = Build();
            block ??= new MaterialPropertyBlock();
            Fade();
        }

        public Mesh Build()
        {
            var rnd = new System.Random(seed);
            float R() => (float)rnd.NextDouble();
            var verts = new Vector3[count * 4]; var uv = new Vector2[count * 4]; var star = new Vector4[count * 4]; var col = new Color[count * 4]; var tris = new int[count * 6];
            float sinMin = Mathf.Sin(minElevation * Mathf.Deg2Rad);
            for (int i = 0; i < count; i++)
            {
                float y = Mathf.Lerp(sinMin, 1, R()), a = R() * Mathf.PI * 2, r = Mathf.Sqrt(Mathf.Max(0, 1 - y * y));
                var p = new Vector3(Mathf.Cos(a) * r, y, Mathf.Sin(a) * r) * radius;
                float bright = Mathf.Pow(R(), 3.5f);                          // most stars faint, a few bright
                float metres = radius * Mathf.Tan(size * Mathf.Deg2Rad) * (.45f + .55f * bright);
                float t = R(); var c = t < .2f ? new Color(1, .82f, .62f) : t < .65f ? new Color(1, .97f, .92f) : new Color(.75f, .85f, 1);
                var s = new Vector4(metres, R() * 20, .8f + R() * 2.6f, .25f + .9f * bright);
                for (int k = 0; k < 4; k++) { verts[i * 4 + k] = p; star[i * 4 + k] = s; col[i * 4 + k] = c; }
                uv[i * 4] = new Vector2(-1, -1); uv[i * 4 + 1] = new Vector2(-1, 1); uv[i * 4 + 2] = new Vector2(1, 1); uv[i * 4 + 3] = new Vector2(1, -1);
                int v = i * 4, q = i * 6; tris[q] = v; tris[q + 1] = v + 1; tris[q + 2] = v + 2; tris[q + 3] = v + 2; tris[q + 4] = v + 3; tris[q + 5] = v;
            }
            var m = new Mesh { name = "Twinkling stars", indexFormat = count * 4 > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
            m.vertices = verts; m.uv = uv; m.SetUVs(1, star); m.colors = col; m.triangles = tris;
            m.bounds = new Bounds(Vector3.zero, Vector3.one * radius * 2.2f);
            return m;
        }

        void Update() => Fade();
        void Fade()
        {
            if (!mr) return;
            float v = time ? Mathf.InverseLerp(showFrom, 1, time.Evening) : 1;
            v = v * v * (3 - 2 * v);
            mr.enabled = v > .001f;
            block ??= new MaterialPropertyBlock();
            mr.GetPropertyBlock(block); block.SetFloat(Visible, v); mr.SetPropertyBlock(block);
        }
    }
}
