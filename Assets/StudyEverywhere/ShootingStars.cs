using UnityEngine;
using UnityEngine.Rendering;

namespace StudyEverywhere
{
    // Now and then a shooting star over a place's baked night sky (StudyEverywhere/Shooting Star; the user, Oct 26: Seoul's
    // starry nights with shooting stars). One ribbon, far away, is sent across the sky at a time: it starts high (25-70
    // degrees up), runs 7-15 degrees along a great circle tipped down toward the horizon in 0.5-1.1 s, its head bright and
    // its tail fading behind it, and goes out. The shader clips it with the sky mask, so it never crosses a roof or a tree.
    // With a TimeOfDay it only flies at night, as the stars show; nothing moves on the CPU but two points a frame.
    [ExecuteAlways]
    public sealed class ShootingStars : MonoBehaviour
    {
        public TimeOfDay time;
        public Material material;
        public float radius = 1100;
        [Tooltip("Seconds between shooting stars, at random within this range.")] public Vector2 interval = new Vector2(6, 20);
        [Tooltip("Fade-in by the clock's Evening (0 day, 1 night), as the stars.")] public float showFrom = .55f;

        static readonly int HeadId = Shader.PropertyToID("_Head"), TailId = Shader.PropertyToID("_Tail"), CentreId = Shader.PropertyToID("_Centre"), LifeId = Shader.PropertyToID("_Life");
        MeshRenderer mr; MaterialPropertyBlock block;
        float next = -1, start, duration, length;
        Vector3 from, axis;

        void OnEnable()
        {
            if (!TryGetComponent(out MeshFilter mf)) mf = gameObject.AddComponent<MeshFilter>();
            if (!TryGetComponent(out mr)) mr = gameObject.AddComponent<MeshRenderer>();
            mr.sharedMaterial = material; mr.shadowCastingMode = ShadowCastingMode.Off; mr.receiveShadows = false;
            mr.lightProbeUsage = LightProbeUsage.Off; mr.reflectionProbeUsage = ReflectionProbeUsage.Off;
            if (!mf.sharedMesh) mf.sharedMesh = Ribbon();
            block ??= new MaterialPropertyBlock();
            mr.enabled = false;
        }

        // the ribbon: 12 steps from tail (u 0) to head (u 1), two sides (v -1, 1); the shader places it each frame
        Mesh Ribbon()
        {
            const int n = 12; var v = new Vector3[(n + 1) * 2]; var uv = new Vector2[v.Length]; var t = new int[n * 6];
            for (int i = 0; i <= n; i++) { uv[i * 2] = new Vector2(i / (float)n, -1); uv[i * 2 + 1] = new Vector2(i / (float)n, 1); }
            for (int i = 0; i < n; i++) { int a = i * 2, q = i * 6; t[q] = a; t[q + 1] = a + 1; t[q + 2] = a + 3; t[q + 3] = a; t[q + 4] = a + 3; t[q + 5] = a + 2; }
            return new Mesh { name = "Shooting star ribbon", vertices = v, uv = uv, triangles = t, bounds = new Bounds(Vector3.zero, Vector3.one * radius * 2.2f) };
        }

        void Update()
        {
            if (!mr || !Application.isPlaying) return;   // in the editor only Preview shows one
            float night = time ? Mathf.InverseLerp(showFrom, 1, time.Evening) : 1, now = Time.time;
            if (next < 0) next = now + Random.Range(interval.x, interval.y) * .5f;
            if (duration <= 0 || now >= start + duration)
            {
                mr.enabled = false;
                if (night < .3f) { next = Mathf.Max(next, now + interval.x * .5f); return; }   // by day: none, and none the moment night comes
                if (now < next) return;
                Launch(now);
            }
            Show((now - start) / duration, night);
        }

        void Launch(float now)
        {
            float el = Random.Range(25f, 70f) * Mathf.Deg2Rad, az = Random.Range(0, Mathf.PI * 2);
            from = new Vector3(Mathf.Cos(el) * Mathf.Cos(az), Mathf.Sin(el), Mathf.Cos(el) * Mathf.Sin(az));
            // its way across: sideways along the sky, tipped 20-60 degrees down toward the horizon
            var east = Vector3.Cross(Vector3.up, from).normalized; if (Random.value < .5f) east = -east;
            var down = Vector3.Cross(from, east).normalized; if (down.y > 0) down = -down;
            float tip = Random.Range(20f, 60f) * Mathf.Deg2Rad; var dir = east * Mathf.Cos(tip) + down * Mathf.Sin(tip);
            axis = Vector3.Cross(from, dir).normalized;
            length = Random.Range(7f, 15f); duration = Random.Range(.5f, 1.1f); start = now;
            next = now + duration + Random.Range(interval.x, interval.y);
        }

        // life 0..1: the head runs the whole way, the tail grows to a third of it behind and catches up at the end
        void Show(float life, float night)
        {
            float head = length * Mathf.SmoothStep(0, 1, life), tail = Mathf.Max(0, head - length * .35f * Mathf.Sin(Mathf.PI * Mathf.Min(1, life * 1.15f)));
            Vector3 P(float deg) => transform.position + Quaternion.AngleAxis(deg, axis) * from * radius;
            mr.enabled = true;
            mr.GetPropertyBlock(block);
            block.SetVector(HeadId, P(head)); block.SetVector(TailId, P(tail)); block.SetVector(CentreId, transform.position);
            block.SetFloat(LifeId, Mathf.Sin(Mathf.PI * life) * night);
            mr.SetPropertyBlock(block);
        }

        // a capture's shooting star, held still: from (elevation, azimuth in degrees), part way along
        public void Preview(float elevation, float azimuth, float life = .6f)
        {
            float el = elevation * Mathf.Deg2Rad, az = azimuth * Mathf.Deg2Rad;
            from = new Vector3(Mathf.Cos(el) * Mathf.Cos(az), Mathf.Sin(el), Mathf.Cos(el) * Mathf.Sin(az));
            var east = Vector3.Cross(Vector3.up, from).normalized; var down = Vector3.Cross(from, east).normalized; if (down.y > 0) down = -down;
            axis = Vector3.Cross(from, east * .8f + down * .6f).normalized; length = 12; duration = 1; start = 0;
            if (!mr) OnEnable();
            Show(life, 1);
        }
        public void ClearPreview() { duration = 0; if (mr) mr.enabled = false; }
    }
}
