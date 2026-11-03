using UnityEngine;

namespace StudyEverywhere
{
    // Two little Korean tree frogs (청개구리) on the stones round the Seoul café's garden pond. They sit, mostly:
    // the throat pumps, and now and then the head (or the whole frog, if it has no head bone) turns a few degrees and
    // back. Very occasionally one hops to another of its stones: a quick turn to face it, a short arc with the hind legs
    // kicked out and tucked again, then it settles to that stone's facing and sits a long while. Each frog keeps its own
    // random stream and clock, and no two hops ever come closer together than minHopSpacing, so they never move as a
    // pair. No physics and nothing allocated per frame; the idle life stops when the frog is out of sight or far away.
    public sealed class SeoulFrogs : MonoBehaviour
    {
        [System.Serializable] public struct Spot
        {
            [Tooltip("World position the frog's root sits at (on top of a pond-edge stone).")] public Vector3 position;
            [Tooltip("Yaw in degrees the frog faces while sitting here.")] public float facing;
            [Tooltip("A place in the pond: the frog floats here (position at the water line), legs trailing, and soon leaps back out.")] public bool water;
        }

        [System.Serializable] public class Frog
        {
            [Tooltip("Moved and turned: its origin is where the frog sits.")] public Transform root;
            [Tooltip("Pitched during a hop (nose up at take-off, down on landing). Optional.")] public Transform body;
            [Tooltip("Scaled for the throat pulse. Optional: without it the body breathes very slightly instead.")] public Transform throat;
            [Tooltip("Turned for the small looks round. Optional: without it the root turns instead.")] public Transform head;
            [Tooltip("Hind legs: swung back about their local X while hopping. Optional.")] public Transform leftLeg, rightLeg;
            [Tooltip("Optional pair of leg poses: folded under the body (sitting), stretched straight back (in the air and afloat). With them the legs swap pose instead of swinging.")] public GameObject legsFolded, legsExtended;
            [Tooltip("Optional ripple ring (a flat ring, transparent material) spreading where it enters or leaves the water.")] public Renderer ripple;
            [Tooltip("This frog's stones and water places; it only ever hops between these.")] public Spot[] spots;
            public int startSpot;
        }

        public Frog[] frogs;

        [Header("Waiting")]
        [Tooltip("Seconds a frog sits between hops (random in this range, its own clock).")] public Vector2 idleGap = new Vector2(25, 90);
        [Tooltip("No two hops (by any frogs) start closer together than this, in seconds.")] public float minHopSpacing = 12;
        [Tooltip("Seconds between small looks round (random in this range).")] public Vector2 lookGap = new Vector2(8, 30);
        [Tooltip("Largest look round, degrees either way.")] public float lookAngle = 14;

        [Header("Hop")]
        [Tooltip("Only places this far apart (metres, flat) are hopped between.")] public Vector2 hopDistance = new Vector2(.15f, .7f);

        [Header("Water")]
        [Tooltip("Seconds a frog floats before leaping back onto a stone (random in this range).")] public Vector2 waterStay = new Vector2(8, 25);
        [Tooltip("Chance that a hop from a stone goes into the water, when a water place is in range.")] [Range(0, 1)] public float waterChance = .35f;
        [Tooltip("How far the floating frog bobs (metres) and how slowly (seconds a bob).")] public float bob = .003f, bobPeriod = 3.2f;
        [Tooltip("The ripple: seconds it spreads, and its final diameter (metres).")] public float rippleTime = 1.6f, rippleSize = .5f;
        [Tooltip("Seconds in the air.")] public float hopTime = .35f;
        [Tooltip("Arc height: this much plus hopArcPerMetre times the distance.")] public float hopArc = .04f, hopArcPerMetre = .25f;
        [Tooltip("Degrees the hind legs swing back at full stretch.")] public float legKick = 70;
        [Tooltip("Body pitch in degrees at take-off (nose up) and landing (nose down).")] public float hopPitch = 18;

        [Header("Throat")]
        [Tooltip("Throat pulses a second.")] public float throatRate = 1.6f;
        [Tooltip("How much the throat swells on each pulse (share of its size).")] public float throatSwell = .12f;

        [Header("Cost")]
        [Tooltip("Beyond this distance (metres) from the head the idle life stops; hops still happen.")] public float liveDistance = 12;
        [Tooltip("Seed for the random streams (0: a new one each session).")] public int seed;

        enum Mode { Sit, Turn, Hop, Settle }
        struct State
        {
            public Mode mode; public System.Random rng; public int spot, target; public float t0, nextHop, nextLook, lookFrom, lookTo, lookT0, callUntil, breath;
            public Vector3 from; public float yawFrom, yawTo;
            public Quaternion legL, legR, body, head; public Vector3 throat, rootScale; public Renderer renderer;
            public float rippleT0; public Vector3 rippleAt;
        }
        MaterialPropertyBlock block;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        State[] s;
        float lastHop = -999;
        Transform eye;
        // The clock (Time.time; an editor test may run the pond on its own clock to watch a minute of hops in seconds).
        public static System.Func<float> clock;
        static float Now => clock != null ? clock() : Time.time;

        const float TurnTime = .3f, SettleTime = .45f, LookTime = .6f;

        // For the calls: where frog i is, and whether it is sitting still (not mid-hop).
        public int Count => frogs == null ? 0 : frogs.Length;
        public Transform Root(int i) => frogs[i].root;
        public bool Sitting(int i) => s != null && s[i].mode == Mode.Sit;
        // Puffs the throat harder for a while, as if calling (a tree frog calls with one sac under the chin, and
        // this is it).
        public void Calling(int i, float seconds) { if (s != null) s[i].callUntil = Now + seconds; }

        void Start()
        {
            int n = Count; s = new State[n];
            int baseSeed = seed != 0 ? seed : System.Environment.TickCount;
            for (int i = 0; i < n; i++)
            {
                var f = frogs[i]; ref var st = ref s[i];
                st.rng = new System.Random(baseSeed + 7919 * (i + 1));
                st.spot = f.spots != null && f.spots.Length > 0 ? Mathf.Clamp(f.startSpot, 0, f.spots.Length - 1) : -1;
                if (st.spot >= 0) Place(f.root, f.spots[st.spot]);
                if (f.leftLeg) st.legL = f.leftLeg.localRotation;
                if (f.rightLeg) st.legR = f.rightLeg.localRotation;
                if (f.body) st.body = f.body.localRotation;
                if (f.head) st.head = f.head.localRotation;
                st.throat = f.throat ? f.throat.localScale : Vector3.one;
                st.rootScale = (f.body ? f.body : f.root).localScale;
                st.renderer = f.root.GetComponentInChildren<Renderer>();
                // stagger the first hops well apart: one may go in the first minute, the other not until much later
                st.nextHop = Now + Range(ref st, idleGap.x * .5f, idleGap.y) + i * minHopSpacing;
                st.nextLook = Now + Range(ref st, lookGap.x, lookGap.y);
                st.lookT0 = -99; st.breath = Range(ref st, 0, 10); st.rippleT0 = -99;
                if (st.spot >= 0) Pose(f, f.spots[st.spot].water);
                if (f.ripple) f.ripple.enabled = false;
                if (st.spot >= 0 && !HasNeighbour(f, st.spot)) Debug.LogWarning($"SeoulFrogs: frog {i} has no stone {hopDistance.x}-{hopDistance.y} m from spot {st.spot}; it will never hop from there.", this);
            }
        }

        void Update()
        {
            float now = Now;
            if (!eye) { var cam = Camera.main; eye = cam ? cam.transform : null; }
            Vector3 head = eye ? eye.position : transform.position;
            float live2 = liveDistance * liveDistance;
            for (int i = 0; i < s.Length; i++)
            {
                var f = frogs[i]; if (!f.root || s[i].spot < 0) continue;
                ref var st = ref s[i];
                switch (st.mode)
                {
                    case Mode.Sit:
                        if (now >= st.nextHop) TryHop(f, ref st, now);
                        break;
                    case Mode.Turn:
                    {
                        float k = Mathf.Clamp01((now - st.t0) / TurnTime);
                        f.root.rotation = Quaternion.Euler(0, Mathf.LerpAngle(st.yawFrom, st.yawTo, Smooth(k)), 0);
                        Legs(f, st, k * .25f);   // a slight crouch, legs gathering
                        if (k >= 1)
                        {
                            st.mode = Mode.Hop; st.t0 = now; st.from = f.root.position; lastHop = now;
                            if (f.spots[st.spot].water) Ripple(ref st, st.from, now);   // leaving the water
                        }
                        break;
                    }
                    case Mode.Hop:
                    {
                        float k = Mathf.Clamp01((now - st.t0) / hopTime);
                        var to = f.spots[st.target].position;
                        float dist = Vector2.Distance(new Vector2(st.from.x, st.from.z), new Vector2(to.x, to.z));
                        var p = Vector3.Lerp(st.from, to, k); p.y += 4 * k * (1 - k) * (hopArc + hopArcPerMetre * dist);
                        f.root.position = p;
                        // legs shoot out at take-off, then tuck up through the second half of the arc
                        Legs(f, st, k < .2f ? k / .2f : 1 - Smooth((k - .2f) / .8f) * .85f);
                        Pose(f, (k > .05f && k < .85f) || f.spots[st.target].water);   // stretched out in the air, and on into the water
                        if (f.body) f.body.localRotation = st.body * Quaternion.Euler(Mathf.Lerp(-hopPitch, hopPitch, k), 0, 0);
                        if (k >= 1)
                        {
                            st.mode = Mode.Settle; st.t0 = now; st.spot = st.target; st.yawFrom = st.yawTo; st.yawTo = f.spots[st.spot].facing;
                            if (f.spots[st.spot].water) Ripple(ref st, to, now);   // the splash ring where it lands
                        }
                        break;
                    }
                    case Mode.Settle:
                    {
                        float k = Mathf.Clamp01((now - st.t0) / SettleTime);
                        f.root.rotation = Quaternion.Euler(0, Mathf.LerpAngle(st.yawFrom, st.yawTo, Smooth(k)), 0);
                        Legs(f, st, (1 - k) * .15f);
                        if (f.body) f.body.localRotation = Quaternion.Slerp(st.body * Quaternion.Euler(hopPitch, 0, 0), st.body, Smooth(k));
                        if (k >= 1)
                        {
                            st.mode = Mode.Sit; Legs(f, st, 0); Pose(f, f.spots[st.spot].water);
                            st.nextHop = now + (f.spots[st.spot].water ? Range(ref st, waterStay.x, waterStay.y) : Range(ref st, idleGap.x, idleGap.y));
                            st.nextLook = now + Range(ref st, lookGap.x, lookGap.y);
                        }
                        break;
                    }
                }
                UpdateRipple(f, st, now);
                if (st.mode != Mode.Sit) continue;
                // afloat: a slow bob on the water line
                if (f.spots[st.spot].water) { var p = f.spots[st.spot].position; p.y += bob * Mathf.Sin((now + st.breath) * Mathf.PI * 2 / bobPeriod); f.root.position = p; }
                // idle life, only where it can be seen
                if ((f.root.position - head).sqrMagnitude > live2 || (st.renderer && !st.renderer.isVisible)) continue;
                Idle(f, ref st, now);
            }
        }

        void Idle(Frog f, ref State st, float now)
        {
            // throat: a quick swell and slow ease, harder (and a little slower) while calling
            bool calling = now < st.callUntil;
            float w = Mathf.Sin((now + st.breath) * throatRate * (calling ? .8f : 1) * Mathf.PI * 2);
            float swell = Mathf.Max(0, w); swell *= swell * (calling ? throatSwell * 3 : throatSwell);
            if (f.throat) f.throat.localScale = new Vector3(st.throat.x * (1 + swell), st.throat.y * (1 + swell * .6f), st.throat.z * (1 + swell * .4f));
            else (f.body ? f.body : f.root).localScale = new Vector3(st.rootScale.x * (1 + swell * .15f), st.rootScale.y, st.rootScale.z);

            // a small look round: turn, hold, and back
            if (now >= st.nextLook)
            {
                st.lookFrom = st.lookTo;
                st.lookTo = Mathf.Abs(st.lookFrom) > .1f ? 0 : Range(ref st, -lookAngle, lookAngle);
                st.lookT0 = now; st.nextLook = now + (st.lookTo != 0 ? Range(ref st, 1.5f, 4) : Range(ref st, lookGap.x, lookGap.y));
            }
            float lk = (now - st.lookT0) / LookTime;
            if (lk >= 0 && lk <= 1.05f)
            {
                float a = Mathf.Lerp(st.lookFrom, st.lookTo, Smooth(Mathf.Clamp01(lk)));
                if (f.head) f.head.localRotation = st.head * Quaternion.Euler(0, a, 0);
                else f.root.rotation = Quaternion.Euler(0, f.spots[st.spot].facing + a, 0);
            }
        }

        void TryHop(Frog f, ref State st, float now)
        {
            // keep hops apart: if another frog hopped lately, wait a little longer than the spacing
            if (now - lastHop < minHopSpacing) { st.nextHop = lastHop + minHopSpacing + Range(ref st, 2, 15); return; }
            int pick = PickNeighbour(f, ref st);
            if (pick < 0) { st.nextHop = now + Range(ref st, idleGap.x, idleGap.y); return; }
            st.target = pick; st.mode = Mode.Turn; st.t0 = now; lastHop = now;
            var d = f.spots[pick].position - f.root.position;
            st.yawFrom = f.root.eulerAngles.y; st.yawTo = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
            st.lookTo = 0; st.lookFrom = 0; if (f.head) f.head.localRotation = st.head;
            if (f.throat) f.throat.localScale = st.throat; else (f.body ? f.body : f.root).localScale = st.rootScale;
        }

        // A random place within hop range of the current one (two passes, nothing allocated): from the water always back
        // onto a stone; from a stone into the water now and then (waterChance), otherwise to another stone.
        int PickNeighbour(Frog f, ref State st)
        {
            bool fromWater = f.spots[st.spot].water;
            bool wantWater = !fromWater && st.rng.NextDouble() < waterChance && CountIn(f, st.spot, true) > 0;
            int count = CountIn(f, st.spot, wantWater);
            if (count == 0) return -1;
            int k = st.rng.Next(count);
            for (int j = 0; j < f.spots.Length; j++) if (InRange(f, st.spot, j) && f.spots[j].water == wantWater && k-- == 0) return j;
            return -1;
        }
        int CountIn(Frog f, int from, bool water) { int n = 0; for (int j = 0; j < f.spots.Length; j++) if (InRange(f, from, j) && f.spots[j].water == water) n++; return n; }

        // Folded or stretched hind legs, when the frog has the two poses.
        static void Pose(Frog f, bool extended)
        {
            if (!f.legsFolded || !f.legsExtended) return;
            if (f.legsFolded.activeSelf == extended) f.legsFolded.SetActive(!extended);
            if (f.legsExtended.activeSelf != extended) f.legsExtended.SetActive(extended);
        }

        void Ripple(ref State st, Vector3 at, float now) { st.rippleT0 = now; st.rippleAt = at; }
        // A ring spreading from where the frog met the water, fading as it grows.
        void UpdateRipple(Frog f, in State st, float now)
        {
            if (!f.ripple) return;
            float k = (now - st.rippleT0) / rippleTime;
            bool on = k >= 0 && k < 1;
            if (f.ripple.enabled != on) f.ripple.enabled = on;
            if (!on) return;
            var t = f.ripple.transform; t.position = st.rippleAt; t.localScale = Vector3.one * Mathf.Lerp(.06f, rippleSize, 1 - (1 - k) * (1 - k));
            block ??= new MaterialPropertyBlock();
            f.ripple.GetPropertyBlock(block); block.SetColor(BaseColor, new Color(1, 1, 1, .5f * (1 - k) * (1 - k))); f.ripple.SetPropertyBlock(block);
        }

        bool HasNeighbour(Frog f, int from) { for (int j = 0; j < f.spots.Length; j++) if (InRange(f, from, j)) return true; return false; }

        bool InRange(Frog f, int a, int b)
        {
            if (a == b) return false;
            var d = f.spots[b].position - f.spots[a].position; d.y = 0;
            float m = d.magnitude; return m >= hopDistance.x && m <= hopDistance.y;
        }

        void Legs(Frog f, in State st, float kick)
        {
            if (f.legsFolded && f.legsExtended) return;   // the poses carry the legs
            var r = Quaternion.Euler(kick * legKick, 0, 0);
            if (f.leftLeg) f.leftLeg.localRotation = st.legL * r;
            if (f.rightLeg) f.rightLeg.localRotation = st.legR * r;
        }

        static void Place(Transform root, Spot spot) { root.SetPositionAndRotation(spot.position, Quaternion.Euler(0, spot.facing, 0)); }
        static float Smooth(float k) => k * k * (3 - 2 * k);
        static float Range(ref State st, float a, float b) => a + (float)st.rng.NextDouble() * (b - a);
    }
}
