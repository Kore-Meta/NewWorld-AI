using UnityEngine;

namespace StudyEverywhere
{
    // One shared display drawn as a frameless screen curved around the seat, at the monitor's real size times the
    // user's screen size. The stream is copied into a mipmapped texture through a light sharpening pass before it is
    // drawn: the desktop has more detail than the headset can show, and sampling it without mipmaps makes small text
    // shimmer and smear, while the sharpening wins back a little of the crispness the headset's optics soften.
    // The controls for all screens live on one ScreenControls bar.
    public sealed class ScreenSurface : MonoBehaviour
    {
        const float Sharpen = 0.3f;

        static Material sharpen;
        Material material;
        MeshFilter meshFilter;
        RenderTexture sharp;
        Texture source;
        int lastCopy = -1;
        bool logged;

        public static ScreenSurface Create(string displayName, Texture source)
        {
            var go = new GameObject("Shared screen | " + displayName);
            var s = go.AddComponent<ScreenSurface>();
            s.source = source;
            s.meshFilter = go.AddComponent<MeshFilter>();
            s.meshFilter.sharedMesh = new Mesh { name = "Curved screen" };
            s.material = new Material(Shader.Find("Universal Render Pipeline/Unlit")) { name = "Shared screen" };
            s.material.SetFloat("_Cull", 0);
            go.AddComponent<MeshRenderer>().sharedMaterial = s.material;
            if (!sharpen)
            {
                var shader = Resources.Load<Shader>("ScreenSharpen");
                if (shader) sharpen = new Material(shader) { name = "Screen sharpen" };
            }
            return s;
        }

        // Live: full colour; paused: greyed.
        public void Mark(bool live) => material.color = live ? Color.white : new Color(.45f, .45f, .45f);

        // Places the screen on a cylinder of the given radius round the pivot (the seated head, eye height):
        // arcCentre is the screen's centre along the arc and centreY its centre height relative to the eye, in metres.
        public void Layout(Vector3 pivot, float yaw, float radius, float arcCentre, float centreY, float width, float height)
        {
            transform.SetPositionAndRotation(pivot, Quaternion.Euler(0, yaw, 0));
            float a0 = (arcCentre - width / 2) / radius, a1 = (arcCentre + width / 2) / radius;
            int segments = Mathf.Max(8, Mathf.CeilToInt((a1 - a0) * Mathf.Rad2Deg / 2.5f));
            var vertices = new Vector3[(segments + 1) * 2];
            var uv = new Vector2[vertices.Length];
            var triangles = new int[segments * 6];
            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments, a = Mathf.Lerp(a0, a1, t);
                var flat = new Vector3(Mathf.Sin(a) * radius, 0, Mathf.Cos(a) * radius);
                vertices[i * 2] = flat + Vector3.up * (centreY - height / 2);
                vertices[i * 2 + 1] = flat + Vector3.up * (centreY + height / 2);
                uv[i * 2] = new Vector2(t, 0);
                uv[i * 2 + 1] = new Vector2(t, 1);
                if (i == segments) break;
                int v = i * 2, k = i * 6;
                triangles[k] = v; triangles[k + 1] = v + 1; triangles[k + 2] = v + 2;
                triangles[k + 3] = v + 1; triangles[k + 4] = v + 3; triangles[k + 5] = v + 2;
            }
            var mesh = meshFilter.sharedMesh;
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
        }

        void LateUpdate()
        {
            if (!source) return;
            if (!sharp || sharp.width != source.width || sharp.height != source.height)
            {
                if (sharp) sharp.Release();
                sharp = new RenderTexture(source.width, source.height, 0, RenderTextureFormat.ARGB32)
                {
                    name = "Shared screen (mipmapped)", useMipMap = true, autoGenerateMips = true,
                    filterMode = FilterMode.Trilinear, anisoLevel = 2, wrapMode = TextureWrapMode.Clamp,
                };
                sharp.Create();
                material.mainTexture = sharp;
                if (!logged) { logged = true; Debug.Log($"[Screens] {name}: stream {source.width}×{source.height}"); }
            }
            // Desktop streams run at 30 fps, so every other frame is enough.
            if (Time.frameCount - lastCopy < 2) return;
            lastCopy = Time.frameCount;
            if (sharpen) { sharpen.SetFloat("_Amount", Sharpen); Graphics.Blit(source, sharp, sharpen); }
            else Graphics.Blit(source, sharp);
        }

        void OnDestroy()
        {
            if (sharp) sharp.Release();
            if (material) Destroy(material);
            if (meshFilter && meshFilter.sharedMesh) Destroy(meshFilter.sharedMesh);
        }
    }
}
