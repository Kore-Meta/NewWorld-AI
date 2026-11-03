using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace StudyEverywhere
{
    // Life on Seabreeze's beach, on the clock (TimeOfDay):
    // - Green turtle hatchlings. In the cool of the evening, through the night and at first light (when real hatchlings
    //   leave the nest; by day the sand is too hot), a few dig out of a nest high on the beach one after another and row
    //   down to the lagoon, heading for its bright horizon: a burst of crawling, a rest, on again, a little wobble in the
    //   line. The wash may tumble one back a little before it gets in; then it swims off and is gone. After a while
    //   another clutch comes up.
    // - Ghost crabs by day. Each lives in a burrow on the upper beach: it comes up, dashes sideways across the sand in
    //   short bursts, freezes, forages down at the wet sand, and goes home. Come too close (head or hand) and it runs for
    //   its burrow and drops in.
    public sealed class BeachCritters : MonoBehaviour
    {
        public enum Kind { Hatchlings, GhostCrabs }
        public Kind kind;
        public TimeOfDay time;
        [Tooltip("Clock hours they are about: from, to (wrapping past midnight when from > to).")]
        public Vector2 hours = new Vector2(19, 10.5f);
        public Renderer[] bodies;
        [Tooltip("Hatchlings: the nest. Crabs: one burrow per crab.")]
        public Vector3[] homes;
        public XROrigin origin;
        public Transform leftController, rightController;
        [Tooltip("Hatchlings: seconds between one clutch reaching the sea and the next coming up.")]
        public Vector2 restBetween = new Vector2(30, 70);

        enum State { Waiting, Emerging, Moving, Resting, Swimming, Gone, Hidden, Fleeing, Sinking }
        sealed class Critter
        {
            public Renderer r; public State state; public float seed, timer, stroke, swim, heading, speed, wobble, travelled, rise;
            public Vector2 pos, target; public int moves; public bool towardHome;
        }

        readonly List<Critter> critters = new List<Critter>();
        readonly List<Vector3> tips = new List<Vector3>();
        MaterialPropertyBlock block;
        static readonly int StrokeId = Shader.PropertyToID("_Stroke"), SwimId = Shader.PropertyToID("_Swim");
        float clutchTimer;
        bool active, forced;

        // Hatchlings, on request (Calypso): a clutch digs out now, whatever the hour; once it has reached the sea the
        // hatchlings keep their own hours again.
        public void HatchNow()
        {
            if (kind != Kind.Hatchlings) return;
            forced = active = true;
            Reset(false);
            for (int i = 0; i < critters.Count; i++) critters[i].timer = .4f + i * Random.Range(.7f, 1.6f);
        }
        // Hatchlings out of the nest and on their way (for Calypso's description of the beach).
        public bool Hatching => active && critters.Exists(c => c.state == State.Emerging || c.state == State.Moving || c.state == State.Resting || c.state == State.Swimming);

        void Awake()
        {
            block = new MaterialPropertyBlock();
            foreach (var r in bodies) if (r) critters.Add(new Critter { r = r, seed = critters.Count * 2.39f });
            Reset(true);
        }

        bool InHours()
        {
            if (!time) return true;
            float h = time.Current < 0 ? TimeOfDay.PresetHour(time.preset) : time.Current;
            return hours.x <= hours.y ? h >= hours.x && h < hours.y : h >= hours.x || h < hours.y;
        }

        void Reset(bool first)
        {
            for (int i = 0; i < critters.Count; i++)
            {
                var c = critters[i];
                c.state = kind == Kind.Hatchlings ? State.Waiting : State.Hidden;
                c.timer = kind == Kind.Hatchlings ? i * Random.Range(2f, 5f) + Random.Range(0, 2f) : Random.Range(first ? 1f : 4f, 14f);
                c.r.gameObject.SetActive(false);
                c.swim = 0; c.travelled = 0; c.moves = 0;
            }
        }

        static float Sand(Vector2 p) => SeabreezeBeach.Sand(p.x, p.y);

        void Place(Critter c, float lift, float bodyHalf)
        {
            var n = SeabreezeBeach.Normal(c.pos.x, c.pos.y);
            var fwd = new Vector3(Mathf.Sin(c.heading), 0, Mathf.Cos(c.heading));
            var rot = Quaternion.LookRotation(Vector3.ProjectOnPlane(fwd, n), n);
            c.r.transform.SetPositionAndRotation(new Vector3(c.pos.x, Sand(c.pos) + bodyHalf + lift, c.pos.y), rot);
            block.SetFloat(StrokeId, c.stroke); block.SetFloat(SwimId, c.swim);
            c.r.SetPropertyBlock(block);
        }

        void Update()
        {
            bool on = InHours() || forced;
            if (on != active) { active = on; Reset(false); }
            if (!on) return;
            if (kind == Kind.Hatchlings) Hatchlings(); else Crabs();
        }

        // ---- hatchlings
        void Hatchlings()
        {
            float dt = Time.deltaTime, level = SeaShore.Level;
            bool any = false;
            foreach (var c in critters)
            {
                switch (c.state)
                {
                    case State.Waiting:
                        any = true;
                        if ((c.timer -= dt) > 0) break;
                        var nest = homes.Length > 0 ? homes[0] : Vector3.zero;
                        c.pos = new Vector2(nest.x, nest.z) + Random.insideUnitCircle * .12f;
                        c.heading = Random.Range(-.5f, .5f); c.rise = 0; c.stroke = Random.value * 6;
                        c.state = State.Emerging; c.r.gameObject.SetActive(true);
                        break;
                    case State.Emerging:
                        any = true;
                        // digging up out of the sand, flippers going
                        c.rise = Mathf.MoveTowards(c.rise, 1, dt / 2.5f); c.stroke += dt * 9;
                        Place(c, (c.rise - 1) * .045f, .011f);
                        if (c.rise >= 1) { c.state = State.Moving; c.timer = Random.Range(1.5f, 4f); c.speed = Random.Range(.07f, .12f); }
                        break;
                    case State.Moving:
                    case State.Resting:
                        any = true;
                        float depth = level - Sand(c.pos);
                        // toward the sea's bright horizon: down the beach, with a slow wander
                        c.wobble += dt * Random.Range(.2f, .6f);
                        var down = SeabreezeBeach.Normal(c.pos.x, c.pos.y); float seaward = Mathf.Atan2(down.x, down.z);   // straight down the beach
                        float want = seaward + Mathf.Sin(c.wobble + c.seed) * .35f;
                        c.heading = Mathf.LerpAngle(c.heading * Mathf.Rad2Deg, want * Mathf.Rad2Deg, dt * .8f) * Mathf.Deg2Rad;
                        if (c.state == State.Moving)
                        {
                            var step = new Vector2(Mathf.Sin(c.heading), Mathf.Cos(c.heading)) * c.speed * dt;
                            // the wash can carry a little one back up the beach
                            if (depth > .002f && depth < .02f && SeaShore.Rising) step = new Vector2(0, -.1f * dt);
                            c.pos += step; c.stroke += dt * 7;
                            if ((c.timer -= dt) <= 0) { c.state = State.Resting; c.timer = Random.Range(.6f, 2.2f); }
                        }
                        else if ((c.timer -= dt) <= 0) { c.state = State.Moving; c.timer = Random.Range(1.5f, 4.5f); c.speed = Random.Range(.07f, .12f); }
                        Place(c, 0, .011f);
                        if (depth > .025f) { c.state = State.Swimming; c.travelled = 0; }
                        break;
                    case State.Swimming:
                        any = true;
                        // off into the lagoon, sinking as it swims; gone a few metres out
                        c.swim = Mathf.MoveTowards(c.swim, 1, dt * 2);
                        c.heading = Mathf.LerpAngle(c.heading * Mathf.Rad2Deg, 0, dt * .5f) * Mathf.Deg2Rad;
                        c.pos += new Vector2(Mathf.Sin(c.heading), Mathf.Cos(c.heading)) * .2f * dt; c.travelled += .2f * dt; c.stroke += dt * 5;
                        float y = Mathf.Max(Sand(c.pos) + .012f, level - .015f - c.travelled * .05f);
                        Place(c, y - Sand(c.pos) - .011f, .011f);
                        if (c.travelled > 3) { c.state = State.Gone; c.r.gameObject.SetActive(false); }
                        break;
                }
            }
            if (any) clutchTimer = Random.Range(restBetween.x, restBetween.y);
            else if (forced) forced = false;   // the asked-for clutch is all in the sea: back to the hatchlings' own hours
            else if ((clutchTimer -= Time.deltaTime) <= 0) Reset(false);
        }

        // ---- ghost crabs
        bool Threatened(Vector2 p)
        {
            var head = Camera.main ? Camera.main.transform.position : Vector3.up * 100;
            if (Vector2.Distance(new Vector2(head.x, head.z), p) < 1.1f && head.y - Sand(p) < 1.3f) return true;
            SeaShore.ReadTips(origin, leftController, rightController, tips, indexOnly: true);
            foreach (var t in tips) if (Vector2.Distance(new Vector2(t.x, t.z), p) < .45f && t.y - Sand(p) < .4f) return true;
            return false;
        }

        void Crabs()
        {
            float dt = Time.deltaTime;
            for (int i = 0; i < critters.Count; i++)
            {
                var c = critters[i]; var home3 = homes[Mathf.Min(i, homes.Length - 1)]; var home = new Vector2(home3.x, home3.z);
                switch (c.state)
                {
                    case State.Hidden:
                        if ((c.timer -= dt) > 0 || Threatened(home)) break;
                        c.pos = home; c.rise = 0; c.moves = Random.Range(3, 8); c.towardHome = false; c.heading = Random.Range(0, 6.28f);
                        c.state = State.Emerging; c.r.gameObject.SetActive(true);
                        break;
                    case State.Emerging:
                        c.rise = Mathf.MoveTowards(c.rise, 1, dt / .8f);
                        Place(c, (c.rise - 1) * .05f, .014f);
                        if (c.rise >= 1) { c.state = State.Resting; c.timer = Random.Range(.5f, 2f); }
                        break;
                    case State.Resting:
                        Place(c, 0, .014f);
                        if (Threatened(c.pos)) { Flee(c, home); break; }
                        if ((c.timer -= dt) > 0) break;
                        if (c.moves-- <= 0) { c.target = home; c.towardHome = true; }
                        else
                        {
                            // now and then down to the wet sand to forage; otherwise a dash somewhere near
                            var t = Random.value < .35f ? new Vector2(c.pos.x + Random.Range(-1.5f, 1.5f), SeabreezeBeach.Shore(c.pos.x) - Random.Range(.3f, .9f)) : c.pos + Random.insideUnitCircle * Random.Range(.6f, 2.2f);
                            t.y = Mathf.Min(t.y, SeabreezeBeach.Shore(t.x) - .25f);
                            c.target = t;
                        }
                        c.speed = Random.Range(.5f, .95f); c.timer = Random.Range(.3f, 1.2f); c.state = State.Moving;
                        // a crab runs sideways: it faces across its path
                        var dir = c.target - c.pos; if (dir.sqrMagnitude > 1e-4) c.heading = Mathf.Atan2(dir.x, dir.y) + (Random.value < .5f ? 1 : -1) * Mathf.PI / 2;
                        break;
                    case State.Moving:
                    case State.Fleeing:
                        if (c.state == State.Moving && Threatened(c.pos)) { Flee(c, home); break; }
                        var to = c.target - c.pos; float d = to.magnitude, step = c.speed * dt;
                        c.pos += d > step ? to / d * step : to; c.stroke += dt * 28;
                        Place(c, 0, .014f);
                        if (d <= step || (c.state == State.Moving && (c.timer -= dt) <= 0))
                        {
                            if (c.towardHome && Vector2.Distance(c.pos, home) < .02f) { c.state = State.Sinking; c.rise = 1; }
                            else { c.state = State.Resting; c.timer = Random.Range(.4f, 3f); }
                        }
                        break;
                    case State.Sinking:
                        c.rise = Mathf.MoveTowards(c.rise, 0, dt / .4f);
                        Place(c, (c.rise - 1) * .05f, .014f);
                        if (c.rise <= 0) { c.state = State.Hidden; c.timer = Random.Range(6f, 25f); c.r.gameObject.SetActive(false); }
                        break;
                }
            }
        }

        void Flee(Critter c, Vector2 home)
        {
            c.target = home; c.towardHome = true; c.speed = 1.6f; c.state = State.Fleeing;
            var dir = home - c.pos; if (dir.sqrMagnitude > 1e-4) c.heading = Mathf.Atan2(dir.x, dir.y) + Mathf.PI / 2;
        }
    }
}
