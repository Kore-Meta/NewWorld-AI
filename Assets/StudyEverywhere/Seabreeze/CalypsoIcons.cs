using System.Collections.Generic;
using UnityEngine;

namespace StudyEverywhere
{
    // The little line icons for Calypso's cues (time of day, day cycle, turtles, waves, fire) and the ring round her orb
    // while she acts, drawn once in code into white sprites (tinted by the Image), so no texture assets are needed. The
    // strokes are the design board's 24-unit icons, anti-aliased, with mipmaps so they stay smooth at a distance.
    public static class CalypsoIcons
    {
        public enum Kind { Sun, Cycle, Turtle, Waves, Fire }

        const int Size = 96;            // pixels per icon
        const float Stroke = 1.8f;      // in the 24-unit design grid
        static readonly Dictionary<Kind, Sprite> cache = new Dictionary<Kind, Sprite>();
        static Sprite ring;

        public static Sprite Get(Kind k)
        {
            if (cache.TryGetValue(k, out var s) && s) return s;
            s = Draw("Calypso icon " + k, Strokes(k), Stroke);
            cache[k] = s;
            return s;
        }

        // A thin full circle (Image type Filled, Radial 360, for the progress ring).
        public static Sprite Ring()
        {
            if (ring) return ring;
            var lines = new List<Vector2[]> { Arc(new Vector2(12, 12), 11, 11, 0, 360) };
            ring = Draw("Calypso ring", lines, .9f);
            return ring;
        }

        // ---- the icons, as polylines in the 24-unit grid (x right, y down, as in the SVGs on the board)

        static List<Vector2[]> Strokes(Kind k)
        {
            var l = new List<Vector2[]>();
            switch (k)
            {
                case Kind.Sun:   // the sun on the horizon
                    l.Add(Seg(3, 18, 21, 18));
                    l.Add(Arc(new Vector2(12, 18), 5, 5, 180, 360));
                    l.Add(Seg(12, 8, 12, 5)); l.Add(Seg(5.6f, 11.6f, 3.8f, 9.8f)); l.Add(Seg(18.4f, 11.6f, 20.2f, 9.8f));
                    break;
                case Kind.Cycle: // two arrows chasing round a small sun
                    l.Add(Arc(new Vector2(12, 12), 8, 8, 200, 340)); l.AddRange(Head(new Vector2(12, 12), 8, 340));
                    l.Add(Arc(new Vector2(12, 12), 8, 8, 20, 160)); l.AddRange(Head(new Vector2(12, 12), 8, 160));
                    l.Add(Arc(new Vector2(12, 12), 2.5f, 2.5f, 0, 360));
                    break;
                case Kind.Turtle:
                    l.Add(Arc(new Vector2(12, 13.5f), 6, 5, 0, 360));
                    l.Add(Arc(new Vector2(12, 5.5f), 2, 2, 0, 360));
                    l.Add(Seg(6.6f, 10.5f, 4, 8.5f)); l.Add(Seg(17.4f, 10.5f, 20, 8.5f));
                    l.Add(Seg(7.4f, 17.5f, 5.5f, 20)); l.Add(Seg(16.6f, 17.5f, 18.5f, 20));
                    break;
                case Kind.Waves:
                    l.Add(Wave(9)); l.Add(Wave(15));
                    break;
                case Kind.Fire:  // a flame with its inner tongue
                    l.Add(new[] { V(12, 3), V(13.2f, 5.6f), V(15.2f, 8.2f), V(16, 11), V(16, 13), V(15, 15.3f), V(12, 16.5f), V(9, 15.3f), V(8, 13), V(8.5f, 10.5f), V(10, 8.5f), V(10.6f, 10), V(12, 11.5f), V(12.4f, 9), V(11.8f, 6), V(12, 3) });
                    break;
            }
            return l;
        }

        static Vector2 V(float x, float y) => new Vector2(x, y);
        static Vector2[] Seg(float x0, float y0, float x1, float y1) => new[] { V(x0, y0), V(x1, y1) };
        static Vector2[] Arc(Vector2 c, float rx, float ry, float fromDeg, float toDeg)
        {
            int n = Mathf.Max(8, Mathf.CeilToInt(Mathf.Abs(toDeg - fromDeg) / 6));
            var p = new Vector2[n + 1];
            for (int i = 0; i <= n; i++) { float a = Mathf.Lerp(fromDeg, toDeg, i / (float)n) * Mathf.Deg2Rad; p[i] = c + new Vector2(Mathf.Cos(a) * rx, Mathf.Sin(a) * ry); }
            return p;
        }
        // an arrowhead at the end of an arc (travelling toward increasing angle)
        static IEnumerable<Vector2[]> Head(Vector2 c, float r, float atDeg)
        {
            float a = atDeg * Mathf.Deg2Rad;
            var tip = c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r;
            var along = new Vector2(-Mathf.Sin(a), Mathf.Cos(a));
            var outward = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
            yield return new[] { tip, tip - along * 2.6f + outward * 1.8f };
            yield return new[] { tip, tip - along * 2.6f - outward * 1.8f };
        }
        static Vector2[] Wave(float y)
        {
            var p = new Vector2[41];
            for (int i = 0; i <= 40; i++) { float x = 2 + i * .5f; p[i] = V(x, y - (1 - Mathf.Cos(2 * Mathf.PI * (x - 2) / 10))); }
            return p;
        }

        // ---- rasterising: each pixel's distance to the nearest stroke, anti-aliased over a pixel
        static Sprite Draw(string name, List<Vector2[]> lines, float strokeUnits)
        {
            float px = Size / 24f, half = strokeUnits * px * .5f;
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, true) { name = name, wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Trilinear };
            var pixels = new Color32[Size * Size];
            for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
            {
                // the pixel's centre in grid units, y flipped (textures run bottom-up)
                var p = new Vector2((x + .5f) / px, 24 - (y + .5f) / px);
                float d = float.MaxValue;
                foreach (var line in lines)
                    for (int i = 0; i + 1 < line.Length; i++) d = Mathf.Min(d, Dist(p, line[i], line[i + 1]));
                float a = Mathf.Clamp01(half - d * px + .5f);
                pixels[y * Size + x] = new Color32(255, 255, 255, (byte)(a * 255));
            }
            tex.SetPixels32(pixels); tex.Apply(true, true);
            var s = Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(.5f, .5f), 100);
            s.name = name;
            return s;
        }
        static float Dist(Vector2 p, Vector2 a, Vector2 b)
        {
            var ab = b - a; float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Mathf.Max(1e-6f, ab.sqrMagnitude));
            return Vector2.Distance(p, a + ab * t);
        }
    }
}
