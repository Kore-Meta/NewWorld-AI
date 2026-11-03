using UnityEngine;
using UnityEngine.XR.Hands;

namespace StudyEverywhere
{
    // Two round arrow buttons on the thumb side of one index finger: one on the middle segment, near the fingertip, and one
    // on the first segment, near the knuckle. Press one with the thumb pad and the place snap-turns the way its arrow
    // points. The button nearer the fingertip is the outer one as the hands are held in front of you (fingers point a
    // little inward), so it turns left on the right hand and right on the left hand. The very tip is left alone: a thumb
    // there is a pinch, and a pinch disarms the buttons.
    // They show, and press, only while the hand is in front of the eyes, so a thumb resting on a finger in the lap does
    // nothing; a press needs the thumb to have been off both buttons for a moment first, and a short rest on the button.
    // The joints come in the XR Origin's floor-offset space, which is where the buttons live, so they follow every turn.
    public sealed class FingerTurnButtons
    {
        public const int Tip = 0, Knuckle = 1;
        // the thumb pad sits a little back from the thumb-tip joint, toward the last thumb joint (metres)
        const float PadBack = .007f;
        // how far past the press radius the button starts to brighten as the thumb comes (metres)
        const float HoverReach = .045f;
        static readonly Color Idle = new Color(1, 1, 1, .62f), Lit = new Color(1, 1, 1, 1), Flash = new Color(1, .82f, .42f, 1);
        static readonly int MainTexId = Shader.PropertyToID("_MainTex"), ColorId = Shader.PropertyToID("_Color"),
            RendererColorId = Shader.PropertyToID("_RendererColor"), FlipId = Shader.PropertyToID("_Flip");
        static Texture2D leftArrow, rightArrow; static Mesh quad; static Material material;

        readonly ThumbMicrogestures owner; readonly Handedness handedness;
        readonly Transform[] disc = new Transform[2]; readonly MeshRenderer[] look = new MeshRenderer[2];
        readonly float[] near = new float[2], flash = new float[2];
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        float shown; bool armed; float awaySince = -1, touchSince; int touching = -1;

        // The way a button turns: -1 left, 1 right.
        public int Turn(int button) => (button == Tip) == (handedness == Handedness.Right) ? -1 : 1;

        public FingerTurnButtons(ThumbMicrogestures owner, Handedness handedness, Transform space)
        {
            this.owner = owner; this.handedness = handedness;
            for (int i = 0; i < 2; i++)
            {
                var go = new GameObject((i == Tip ? "Turn button near the tip | " : "Turn button near the knuckle | ") + handedness + " hand");
                go.layer = space.gameObject.layer; go.transform.SetParent(space, false);
                go.AddComponent<MeshFilter>().sharedMesh = Quad();
                var r = go.AddComponent<MeshRenderer>(); r.sharedMaterial = Material();
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false;
                r.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off; r.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                r.enabled = false;
                disc[i] = go.transform; look[i] = r;
            }
        }

        public void Hide()
        {
            shown = 0; armed = false; awaySince = -1; touching = -1;
            foreach (var r in look) if (r && r.enabled) r.enabled = false;
        }

        public void Destroy()
        {
            foreach (var t in disc) if (t) Object.Destroy(t.gameObject);
        }

        // Where the buttons sit (centres) and which way each faces out of the finger's thumb side. Joints: the index
        // knuckle, middle and last joints, and the thumb's base knuckle (only to tell which side the thumb is on).
        public static void Place(Pose knuckle, Pose middle, Pose distal, Vector3 thumbBase, float outward,
            out Vector3 tipAt, out Vector3 knuckleAt, out Vector3 tipSide, out Vector3 knuckleSide)
        {
            knuckleSide = Side(knuckle.position, middle.position, knuckle.rotation, thumbBase - knuckle.position);
            tipSide = Side(middle.position, distal.position, middle.rotation, thumbBase - knuckle.position);
            // the first segment is a little thicker than the middle one
            knuckleAt = Vector3.Lerp(knuckle.position, middle.position, .32f) + knuckleSide * outward * 1.3f;
            tipAt = Vector3.Lerp(middle.position, distal.position, .6f) + tipSide * outward;
        }

        // Across the segment, at right angles to the back of the finger, on the thumb's side.
        static Vector3 Side(Vector3 from, Vector3 to, Quaternion joint, Vector3 towardThumb)
        {
            var axis = (to - from).normalized;
            var side = Vector3.Cross(axis, joint * Vector3.up);
            if (side.sqrMagnitude < 1e-6f) side = Vector3.ProjectOnPlane(towardThumb, axis);
            side.Normalize();
            return Vector3.Dot(side, towardThumb) < 0 ? -side : side;
        }

        // One frame. live: tracked, in front of the eyes and free, so the buttons show; canPress: also past the turn
        // cooldown. Eye and eye-up are in the joints' space. Returns -1 to turn left, 1 to turn right, 0 for nothing.
        public int Step(bool live, bool canPress, Pose knuckle, Pose middle, Pose distal, Pose indexTip,
            Vector3 thumbBase, Vector3 thumbDistal, Vector3 thumbTip, Vector3 eye, Vector3 eyeUp, float now)
        {
            Place(knuckle, middle, distal, thumbBase, owner.buttonOut, out var tipAt, out var knuckleAt, out var tipSide, out var knuckleSide);
            var pad = thumbTip + (thumbDistal - thumbTip).normalized * PadBack;
            float dTip = Vector3.Distance(pad, tipAt), dKnuckle = Vector3.Distance(pad, knuckleAt);
            int nearest = dTip <= dKnuckle ? Tip : Knuckle; float d = Mathf.Min(dTip, dKnuckle);
            bool pinch = Vector3.Distance(thumbTip, indexTip.position) < owner.pinchGap;
            int turned = 0;
            if (!live || pinch) { armed = false; awaySince = -1; touching = -1; }
            else if (!armed)
            {
                if (d > owner.pressRelease) { if (awaySince < 0) awaySince = now; armed = now - awaySince >= owner.releaseDwell; }
                else awaySince = -1;
                touching = -1;
            }
            else if (d < owner.pressRadius)
            {
                if (touching != nearest) { touching = nearest; touchSince = now; }
                if (canPress && now - touchSince >= owner.pressDwell)
                {
                    armed = false; awaySince = -1; touching = -1;
                    flash[nearest] = 1; turned = Turn(nearest);
                }
            }
            else touching = -1;

            float dt = Time.unscaledDeltaTime;
            shown = Mathf.MoveTowards(shown, live ? 1 : 0, dt * 6);
            for (int i = 0; i < 2; i++)
            {
                float di = i == Tip ? dTip : dKnuckle;
                near[i] = Mathf.MoveTowards(near[i], live && !pinch ? Mathf.Clamp01(1 - (di - owner.pressRadius) / HoverReach) : 0, dt * 10);
                flash[i] = Mathf.MoveTowards(flash[i], 0, dt * 2.5f);
                Draw(i, i == Tip ? tipAt : knuckleAt, i == Tip ? tipSide : knuckleSide, eye, eyeUp);
            }
            return turned;
        }

        void Draw(int i, Vector3 at, Vector3 side, Vector3 eye, Vector3 eyeUp)
        {
            var r = look[i]; bool on = shown > .01f;
            if (r.enabled != on) r.enabled = on;
            if (!on) return;
            Orient(at, side, eye, eyeUp, out var p, out var q);
            disc[i].localPosition = p; disc[i].localRotation = q;
            disc[i].localScale = Vector3.one * Scale(owner.buttonSize, near[i], flash[i]);
            var c = Tint(near[i], flash[i]); c.a *= shown;
            Paint(block, Turn(i) > 0, c);
            r.SetPropertyBlock(block);
        }

        // Out of the finger's side, turned partway to the eyes so the arrow reads; the arrows point the eyes' left and right.
        public static void Orient(Vector3 at, Vector3 side, Vector3 eye, Vector3 eyeUp, out Vector3 position, out Quaternion rotation)
        {
            var facing = (side + (eye - at).normalized * .9f).normalized;
            var up = Vector3.ProjectOnPlane(eyeUp, facing);
            if (up.sqrMagnitude < 1e-6f) up = Vector3.ProjectOnPlane(Vector3.up, facing);
            position = at + facing * .001f;
            rotation = Quaternion.LookRotation(-facing, up.normalized);
        }

        // near: 0 thumb far .. 1 thumb on it; flash: 1 just pressed, fading to 0.
        public static float Scale(float size, float near, float flash) => size * (1 + .16f * near + .22f * flash);
        public static Color Tint(float near, float flash) => Color.Lerp(Color.Lerp(Idle, Lit, near), Flash, flash);

        public static void Paint(MaterialPropertyBlock block, bool right, Color tint)
        {
            block.SetTexture(MainTexId, Arrow(right)); block.SetColor(ColorId, tint);
            block.SetColor(RendererColorId, Color.white); block.SetVector(FlipId, Vector4.one);
        }

        // ---- looks, made once: a unit quad facing the viewer along +Z, and Sprites/Default (always in the build:
        // unlit, two-sided, blended, no depth write)

        public static Mesh Quad()
        {
            if (quad) return quad;
            quad = new Mesh { name = "Finger turn button" };
            quad.vertices = new[] { new Vector3(-.5f, -.5f), new Vector3(-.5f, .5f), new Vector3(.5f, .5f), new Vector3(.5f, -.5f) };
            quad.uv = new[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };
            quad.colors = new[] { Color.white, Color.white, Color.white, Color.white };
            quad.normals = new[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back };
            quad.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            return quad;
        }

        public static Material Material()
        {
            if (!material) material = new Material(Shader.Find("Sprites/Default")) { name = "Finger turn button", renderQueue = 3100 };
            return material;
        }

        // A see-through dark disc with a white rim and a white arrow, drawn 4x4 supersampled so the edges stay soft.
        public static Texture2D Arrow(bool right)
        {
            ref var cached = ref right ? ref rightArrow : ref leftArrow;
            if (cached) return cached;
            const int N = 128; const float R = 61, Rim = 5.5f;
            var px = new Color[N * N];
            for (int y = 0; y < N; y++)
                for (int x = 0; x < N; x++)
                {
                    float disc = 0, rim = 0, arrow = 0;
                    for (int sy = 0; sy < 4; sy++)
                        for (int sx = 0; sx < 4; sx++)
                        {
                            float u = x + (sx + .5f) / 4 - N / 2f, v = y + (sy + .5f) / 4 - N / 2f;
                            float rr = Mathf.Sqrt(u * u + v * v);
                            if (rr <= R) { disc++; if (rr > R - Rim) rim++; }
                            if (InArrow(right ? u : -u, v)) arrow++;
                        }
                    disc /= 16; rim /= 16; arrow /= 16;
                    // fill, then rim, then arrow, each laid over the last
                    var c = new Color(.06f, .09f, .15f, .58f * disc);
                    c = Over(c, new Color(1, 1, 1, .95f * rim));
                    c = Over(c, new Color(1, 1, 1, arrow));
                    px[y * N + x] = c;
                }
            cached = new Texture2D(N, N, TextureFormat.RGBA32, true) { name = right ? "Turn right arrow" : "Turn left arrow",
                wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Trilinear, anisoLevel = 4 };
            cached.SetPixels(px); cached.Apply(true, true);
            return cached;
        }

        // A right-pointing arrow centred on the disc: a rounded shaft and a broad head.
        static bool InArrow(float u, float v)
        {
            const float HeadBase = 2, HeadTip = 31, HeadHalf = 24, ShaftFrom = -29, ShaftHalf = 7.5f;
            if (u >= HeadBase && u <= HeadTip && Mathf.Abs(v) <= HeadHalf * (HeadTip - u) / (HeadTip - HeadBase)) return true;
            if (u >= ShaftFrom && u <= HeadBase + 1 && Mathf.Abs(v) <= ShaftHalf) return true;
            float du = u - ShaftFrom; return du * du + v * v <= ShaftHalf * ShaftHalf;
        }

        static Color Over(Color under, Color top)
        {
            float a = top.a + under.a * (1 - top.a);
            if (a <= 0) return new Color(0, 0, 0, 0);
            var rgb = ((Vector4)top * top.a + (Vector4)under * under.a * (1 - top.a)) / a;
            return new Color(rgb.x, rgb.y, rgb.z, a);
        }
    }
}
