using UnityEngine;
using UnityEngine.XR.Hands;

namespace StudyEverywhere
{
    // One round arrow button on the thumb side of an index finger, at its middle joint: the right hand's turns right, the
    // left hand's turns left. Press it with the thumb pad and the place snap-turns that way. It sits well back from the
    // tip, where a thumb is a pinch (a pinch disarms it), and it is pressed with the hand held as for a handshake or palm
    // down: a palm turned to the face hides it, because that is the pose that brings up Meta's own menu button.
    // It shows, and presses, only while the hand is in front of the eyes, so a thumb resting on a finger in the lap does
    // nothing; a press needs the thumb to have been off it for a moment first, and a short rest on it.
    // While the thumb is on or just by it, that hand's pointing ray is put away (ThumbMicrogestures does that), so the
    // ray and its dot do not swing about while you press.
    // The joints come in the XR Origin's floor-offset space, which is where the button lives, so it follows every turn.
    public sealed class FingerTurnButtons
    {
        // the thumb pad sits a little back from the thumb-tip joint, toward the last thumb joint (metres)
        const float PadBack = .007f;
        // how far past the press radius the button starts to brighten as the thumb comes (metres)
        const float HoverReach = .045f;
        // thumb pad nearer than this puts the pointing ray away, and farther than QuietEnd brings it back (metres)
        const float QuietStart = .03f, QuietEnd = .045f;
        static readonly Color Idle = new Color(1, 1, 1, .62f), Lit = new Color(1, 1, 1, 1), Flash = new Color(1, .82f, .42f, 1);
        static readonly int MainTexId = Shader.PropertyToID("_MainTex"), ColorId = Shader.PropertyToID("_Color"),
            RendererColorId = Shader.PropertyToID("_RendererColor"), FlipId = Shader.PropertyToID("_Flip");
        static Texture2D leftArrow, rightArrow; static Mesh quad; static Material material;

        readonly ThumbMicrogestures owner; readonly Handedness handedness;
        readonly Transform disc; readonly MeshRenderer look;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        float shown, near, flash; bool armed, touching; float awaySince = -1, touchSince;

        // The way this hand's button turns: -1 left, 1 right.
        public int Turn => handedness == Handedness.Right ? 1 : -1;
        // The thumb is on or just by the button: the pointing ray should keep out of the way.
        public bool ThumbClose { get; private set; }

        public FingerTurnButtons(ThumbMicrogestures owner, Handedness handedness, Transform space)
        {
            this.owner = owner; this.handedness = handedness;
            var go = new GameObject("Turn button | " + handedness + " index finger");
            go.layer = space.gameObject.layer; go.transform.SetParent(space, false);
            go.AddComponent<MeshFilter>().sharedMesh = Quad();
            look = go.AddComponent<MeshRenderer>(); look.sharedMaterial = Material();
            look.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; look.receiveShadows = false;
            look.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off; look.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            look.enabled = false;
            disc = go.transform;
        }

        public void Hide()
        {
            shown = 0; armed = touching = ThumbClose = false; awaySince = -1;
            if (look && look.enabled) look.enabled = false;
        }

        public void Destroy() { if (disc) Object.Destroy(disc.gameObject); }

        // Where the button sits (its centre, on the side of the middle joint) and which way it faces out of the finger's
        // thumb side. Joints: the index knuckle, middle and last joints, and the thumb's base knuckle (only to tell which
        // side the thumb is on).
        public static void Place(Pose knuckle, Pose middle, Pose distal, Vector3 thumbBase, float outward, out Vector3 at, out Vector3 side)
        {
            var towardThumb = thumbBase - knuckle.position;
            side = (Side(knuckle.position, middle.position, knuckle.rotation, towardThumb) + Side(middle.position, distal.position, middle.rotation, towardThumb)).normalized;
            // the middle joint is the thickest part of the finger
            at = middle.position + side * outward * 1.2f;
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

        // One frame. live: tracked, in front of the eyes, palm not to the face and free, so the button shows; canPress:
        // also past the turn cooldown. Eye and eye-up are in the joints' space. Returns -1 to turn left, 1 right, 0 nothing.
        public int Step(bool live, bool canPress, Pose knuckle, Pose middle, Pose distal, Pose indexTip,
            Vector3 thumbBase, Vector3 thumbDistal, Vector3 thumbTip, Vector3 eye, Vector3 eyeUp, float now)
        {
            Place(knuckle, middle, distal, thumbBase, owner.buttonOut, out var at, out var side);
            var pad = thumbTip + (thumbDistal - thumbTip).normalized * PadBack;
            float d = Vector3.Distance(pad, at);
            bool pinch = Vector3.Distance(thumbTip, indexTip.position) < owner.pinchGap;
            int turned = 0;
            if (!live || pinch) { armed = touching = false; awaySince = -1; }
            else if (!armed)
            {
                if (d > owner.pressRelease) { if (awaySince < 0) awaySince = now; armed = now - awaySince >= owner.releaseDwell; }
                else awaySince = -1;
                touching = false;
            }
            else if (d < owner.pressRadius)
            {
                if (!touching) { touching = true; touchSince = now; }
                if (canPress && now - touchSince >= owner.pressDwell)
                {
                    armed = touching = false; awaySince = -1;
                    flash = 1; turned = Turn;
                }
            }
            else touching = false;
            ThumbClose = live && !pinch && d < (ThumbClose ? QuietEnd : QuietStart);

            float dt = Time.unscaledDeltaTime;
            shown = Mathf.MoveTowards(shown, live ? 1 : 0, dt * 6);
            near = Mathf.MoveTowards(near, live && !pinch ? Mathf.Clamp01(1 - (d - owner.pressRadius) / HoverReach) : 0, dt * 10);
            flash = Mathf.MoveTowards(flash, 0, dt * 2.5f);
            bool on = shown > .01f;
            if (look.enabled != on) look.enabled = on;
            if (on)
            {
                Orient(at, side, eye, eyeUp, out var p, out var q);
                disc.localPosition = p; disc.localRotation = q;
                disc.localScale = Vector3.one * Scale(owner.buttonSize, near, flash);
                var c = Tint(near, flash); c.a *= shown;
                Paint(block, Turn > 0, c);
                look.SetPropertyBlock(block);
            }
            return turned;
        }

        // Out of the finger's side, turned partway to the eyes so the arrow reads; the arrow points the eyes' left or right.
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
