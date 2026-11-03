using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

namespace StudyEverywhere
{
    // A café pet walking about the room, as the Paris café's Émile does (the user, Oct 25: Bubble and Hori roam the café).
    // For pets whose body is played by an Animator (PuppyCompanion, TigerCompanion): it plays their in-place Walk and Trot
    // clips and moves them over the floor at the speed their planted paws slide back in those clips, so the paws don't
    // skate (the clip's speed scales with the animator's). Routes run over a small waypoint graph in the aisles, with a
    // sphere cast at chest height steering round furniture and the person; the home (a bed, a mat) is raised, so the pet
    // hops off and on it from a point beside it. Spots are named places it can be sent to (the water bowl, the window).
    // The companion decides when to go and what to do on arrival (Arrived); while Moving, its own moods wait.
    public sealed class PetRoamer : MonoBehaviour
    {
        [Serializable]
        public class Spot
        {
            public string name;
            public Vector3 position;   // where it stands (floor)
            public float yaw;          // facing when it settles there
        }
        [Header("The room")]
        public Vector3 home;              // on its bed or mat (the pet's position there)
        public float homeYaw;
        public Vector3 homeStep;          // the floor point beside home that it hops down to and up from (home itself, for a home on the floor)
        public string homeName = "bed", homeAt = "on";   // "on his bed", "in her spot"
        public Spot[] spots;
        public Vector3[] waypoints;
        public Vector2Int[] links;
        public Transform viewer;          // the person's head, for come-here
        [Header("Gait (measured from the clips, at this pet's scale)")]
        public string walkState = "Walk", trotState = "Trot";
        [Tooltip("Ground speed (m/s) of the Walk clip at animator speed 1.")] public float walkClipSpeed = .13f;
        [Tooltip("Ground speed (m/s) of the Trot clip at animator speed 1.")] public float trotClipSpeed = .37f;
        [Tooltip("Animator speed when walking and trotting (the ground speed follows).")] public float walkRate = 1.35f, trotRate = 1.15f;
        [Tooltip("his or her, for Where().")] public string pronoun = "his";
        [Tooltip("Walking turns (Hori's Turn.L / Turn.R: the walk curving at turnClipRate degrees a second, the legs leaning into it). Empty: the walk is simply yawed as it goes (Bubble).")]
        public string turnLeftState = "", turnRightState = "";
        public float turnClipRate = 45;
        public float turnSpeed = 160, bodyRadius = .08f, chestHeight = .12f;
        [Tooltip("Follow the floor's height (Seoul: the courtyard is a step below the room): walkable floors (teleport areas) never block, and the pet steps up and down with them.")]
        public bool followGround;
        [Tooltip("With followGround: a step of more than 7 cm just ahead is jumped, up or down, rather than eased over.")]
        public bool hopSteps = true;
        public event Action Started;                   // it is about to move (the companion hands its body over)
        [Tooltip("Sits this far beside the person when called over.")] public float personGap = .55f;

        public bool Moving { get; private set; }
        public bool AtHome { get; private set; } = true;
        public string Heading { get; private set; }   // where it is going, while Moving
        public string Place { get; private set; } = "home";   // where it is: "home", a spot's name, "by you", "floor"
        public event Action<string> Arrived;           // the place it got to (or "floor" when it gave up on the way)

        Animator animator;
        Coroutine route;
        float speed;

        void Awake() { animator = GetComponentInChildren<Animator>(); }
        void OnDisable() { if (animator) animator.speed = 1; Moving = false; }

        public IEnumerable<string> SpotNames() { foreach (var s in spots) yield return s.name; }
        public Spot Find(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            name = name.Replace('_', ' ').Trim();
            foreach (var s in spots) if (s.name.Equals(name, StringComparison.OrdinalIgnoreCase)) return s;
            foreach (var s in spots) if (s.name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf(s.name, StringComparison.OrdinalIgnoreCase) >= 0) return s;
            return null;
        }

        public void GoTo(Spot s, bool trot = false) { if (s != null) Go(Route(s.position, s.yaw, s.name, trot)); }
        public void GoHome(bool trot = false) { if (!AtHome) Go(Route(homeStep, homeYaw, "home", trot)); }
        public void Stop()
        {
            if (route != null) StopCoroutine(route); route = null;
            if (hopping) { transform.position = hopTo; AtHome = hopTo == home; hopping = false; }   // never left in mid-air
            if (!AtHome && !Facing) Place = "floor";   // (stopped while turning on the spot: still where it was)
            Heading = null; Halt();
        }
        // to the person: beside them (the right, else the left, behind, in front: the first free floor), facing them
        public void ComeTo(bool trot = true)
        {
            if (!viewer) return;
            var f = viewer.forward; f.y = 0; f = f.sqrMagnitude > 1e-4f ? f.normalized : Vector3.forward;
            var r = Vector3.Cross(Vector3.up, f); var foot = viewer.position; foot.y = homeStep.y;
            var at = foot + r * personGap;
            foreach (var c in new[] { r * personGap + f * .1f, -r * personGap + f * .1f, -f * personGap, f * personGap })
                if (!Physics.CheckSphere(foot + c + Vector3.up * chestHeight, bodyRadius, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) { at = foot + c; break; }
            var face = foot - at; face.y = 0;
            Go(Route(at, Quaternion.LookRotation(face.sqrMagnitude > 1e-4f ? face : f).eulerAngles.y, "by you", trot));
        }
        // a stroll to somewhere it isn't
        public void Wander(bool trot = false)
        {
            var list = new List<Spot>(); foreach (var s in spots) if (s.name != Place) list.Add(s);
            if (list.Count > 0) GoTo(list[UnityEngine.Random.Range(0, list.Count)], trot);
        }
        // a run about the place: trotting (its fastest gait) from spot to spot, legs times without stopping, then settling
        // at the last (asked to "run around": the user, Oct 25, after Hori said she couldn't)
        public void RunAbout(int legs = 3) => Go(Run(Mathf.Max(1, legs)));
        [Tooltip("A run about is the trot played this much faster, and as much faster over the ground (a real run, not a jog: the user, Oct 27).")]
        public float runBoost = 1.35f;
        bool running;
        IEnumerator Run(int legs)
        {
            running = true;
            string last = Place;
            for (int i = 0; i < legs; i++)
            {
                var list = new List<Spot>(); foreach (var s in spots) if (s.name != last) list.Add(s);
                if (list.Count == 0) break;
                var spot = list[UnityEngine.Random.Range(0, list.Count)]; last = spot.name;
                yield return Route(spot.position, spot.yaw, spot.name, true, i == legs - 1);
            }
            running = false;
            if (Moving) { Halt(); Heading = null; route = null; Arrived?.Invoke(Place); }
        }

        // to a thing on the floor (a toy): gap short of it, facing it; run: at a run (the trot played faster, as RunAbout)
        public void GoToward(Vector3 target, float gap, string name, bool trot, bool run = false)
        {
            var d = target - transform.position; d.y = 0;
            var dir = d.sqrMagnitude > 1e-4f ? d.normalized : transform.forward;
            var at = target - dir * Mathf.Min(gap, d.magnitude);
            Go(Toward(at, Quaternion.LookRotation(dir).eulerAngles.y, name, trot || run, run));
        }
        IEnumerator Toward(Vector3 at, float yaw, string name, bool trot, bool run) { running = run; yield return Route(at, yaw, name, trot); running = false; }

        // ---- turning to face the person (revision 102929, the user, Oct 29: when the person looks at Hori or Bubble, they
        // turn to face them). A pivot on the spot (small steps round, the walk at a slow rate, as when settling at a spot),
        // never a hop: on its bed it turns on its bed. Moving meanwhile (the companion's moods wait, as for a walk), but it
        // isn't going anywhere: Place and AtHome stay as they were, Where() says where it is, and Faced (not Arrived) tells
        // the companion it has turned, so arriving's own business (a bark, sitting down by you) doesn't run.
        public event Action Faced;
        public bool Facing { get; private set; }
        [Tooltip("Degrees a second it turns round to face the person (slower than turning on a walk: an unhurried shuffle).")] public float faceSpeed = 70;
        public void Face(Vector3 point)
        {
            var d = point - transform.position; d.y = 0;
            if (d.sqrMagnitude < 1e-4f) return;
            // (Facing set before the coroutine starts: already facing, it finishes, and clears it, before StartCoroutine returns)
            if (route != null) StopCoroutine(route);
            Facing = true; route = StartCoroutine(FaceRoute(Quaternion.LookRotation(d).eulerAngles.y));
        }
        IEnumerator FaceRoute(float yaw)
        {
            Moving = true; Heading = null; Started?.Invoke();
            yield return Turn(yaw, true, faceSpeed);
            Halt(); route = null;
            Faced?.Invoke();
        }
        // The person looking at a pet (the head's forward: the headset has no eye tracking here, so the cone is generous, the
        // pet's own width plus 9 degrees round the line of sight): any of three points along its body, from a (its middle)
        // to b (its head), within reach of the line, in front of the person and no further than range.
        public static bool Gazed(Transform viewer, Vector3 a, Vector3 b, float radius, float range = 6)
        {
            if (!viewer) return false;
            Vector3 o = viewer.position, f = viewer.forward;
            for (int i = 0; i < 3; i++)
            {
                var p = Vector3.Lerp(a, b, i * .5f) - o; float along = Vector3.Dot(p, f);
                if (along < .2f || along > range) continue;
                if ((p - f * along).magnitude <= radius + along * .158f) return true;   // (tan 9 degrees)
            }
            return false;
        }

        void Go(IEnumerator r) { if (route != null) StopCoroutine(route); Facing = false; route = StartCoroutine(r); }

        IEnumerator Route(Vector3 goal, float yaw, string name, bool trot, bool settle = true)
        {
            Moving = true; Heading = name; Started?.Invoke();
            if (AtHome) { yield return Hop(homeStep); AtHome = false; }
            var path = PathTo(Nearest(transform.position), Nearest(goal));
            // the floor straight there is clear (and level): straight there, not round the aisles' waypoints (running after
            // a ball 1.7 m away, Bubble went the long way round: 20 s, ToyPlayCheck, Oct 27)
            var line = goal - transform.position; float ly = line.y; line.y = 0;
            if (line.magnitude > .05f && Mathf.Abs(ly) < .05f && Clear(line.normalized, line.magnitude)) path.Clear();
            // skip a first waypoint that is behind (closer to the goal than to us along the way)
            if (path.Count > 1 && (path[1] - transform.position).sqrMagnitude < (path[1] - path[0]).sqrMagnitude) path.RemoveAt(0);
            foreach (var p in path) yield return WalkTo(p, trot, .2f);
            yield return WalkTo(goal, trot, settle ? .1f : .3f);
            Place = name;
            if (!settle) yield break;   // running on: no stop, no turn, straight on to the next
            if (name == "home") { yield return Hop(home); AtHome = true; }
            // settling: a short pivot to face the spot's way, and only when it is well off (walked round on the turning clips
            // at 45 degrees a second she circled the spot before she sat: the user, Oct 27)
            if (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, yaw)) > 45) yield return Turn(yaw, true);
            Halt(); Heading = null; route = null;
            Arrived?.Invoke(name);
        }

        void Halt() { running = false; speed = 0; Moving = false; Facing = false; curving = 0; pending = null; if (animator) animator.speed = 1; }
        bool Turns => !string.IsNullOrEmpty(turnLeftState) && !string.IsNullOrEmpty(turnRightState);
        void Gait(bool trot) => State(trot ? trotState : walkState);
        // Changes of clip: one at a time, each held a moment. Asked for another while a cross-fade was still running (a
        // walk wavering about the curving threshold, a steer flicking left and right round an obstacle), the animator
        // restarted fade after fade, and the legs jittered (headset review, Oct 25: Hori's intermittent jitter).
        // A clip asked for while that can't be is kept and started as soon as it can (Update).
        string pending; float stateSince = -10;
        void State(string st) { if (animator) { pending = st; TryState(); } }
        void TryState()
        {
            var st = pending; if (st == null) return;
            bool fading = animator.IsInTransition(0);
            if (fading ? animator.GetNextAnimatorStateInfo(0).IsName(st) : animator.GetCurrentAnimatorStateInfo(0).IsName(st)) { pending = null; return; }
            if (fading || Time.time - stateSince < .45f) return;   // let the fade finish, and hold the clip a moment
            animator.CrossFadeInFixedTime(st, .3f); stateSince = Time.time; pending = null;
        }
        void Update() { if (pending != null && Moving) TryState(); }
        int curving;   // -1 curving left, 1 right, 0 straight (with hysteresis: in past 12 degrees, out under 4)
        Vector3 steer;   // the eased heading (WalkTo)

        IEnumerator WalkTo(Vector3 goal, bool trot, float near)
        {
            goal.y = transform.position.y;
            float best = float.MaxValue, sinceCloser = 0, stuck = 0, detourFor = 0; Vector3 detour = default;
            float rate = (trot ? trotRate : walkRate) * (running ? runBoost : 1), full = (trot ? trotClipSpeed : walkClipSpeed) * rate;
            Gait(trot); steer = transform.forward;
            while (true)
            {
                var to = goal - transform.position; to.y = 0; float d = to.magnitude;
                if (d < near) break;
                if (d < best - .04f) { best = d; sinceCloser = 0; } else if ((sinceCloser += Time.deltaTime) > 4) break;   // not getting closer: settle here
                Vector3 dir;
                if (detourFor > 0 && Clear(detour, .3f)) { dir = detour; detourFor -= Time.deltaTime; }
                else { dir = Steer(to / d, Mathf.Min(d, .4f)); detourFor = 0; if (dir != Vector3.zero && Vector3.Angle(dir, to / d) > 20) { detour = dir; detourFor = .6f; } }
                if (dir == Vector3.zero) { speed = 0; if ((stuck += Time.deltaTime) > 2.5f) break; if (animator) animator.speed = .3f; yield return null; continue; }
                stuck = 0;
                // the heading eased toward the steer's (a steer flicking between two clear ways frame by frame wobbled her
                // body left and right: the user, Oct 26 morning)
                steer = Vector3.Slerp(steer, dir, 1 - Mathf.Exp(-Time.deltaTime * 8));
                float angle = Vector3.SignedAngle(transform.forward, steer, Vector3.up);
                // with walking-turn clips: a sharp turn close by is made on the spot first (curving at the clip's rate it would
                // circle the goal); otherwise the walk curves (Turn.L / Turn.R) at that rate and full pace, as the clips are made
                if (!trot && Turns && Mathf.Abs(angle) > 100 && d < .9f) { speed = 0; yield return Turn(transform.eulerAngles.y + angle, true); Gait(false); continue; }
                // close to the goal the walk no longer curves on the turning clips: at their 45 degrees a second she can't
                // turn tighter than a third of a metre, and orbited a goal just off to one side (the circling, Oct 27)
                if (!trot && Turns && d > .6f)
                {
                    int side = angle < 0 ? -1 : 1;
                    if (curving == 0 ? Mathf.Abs(angle) > 12 : Mathf.Abs(angle) < 4 || side != curving && Mathf.Abs(angle) > 12) curving = Mathf.Abs(angle) < 4 ? 0 : side;
                    State(curving < 0 ? turnLeftState : curving > 0 ? turnRightState : walkState);
                }
                else if (curving != 0) { curving = 0; State(walkState); }
                bool bending = curving != 0;
                float maxTurn = (bending ? turnClipRate * rate : turnSpeed) * Time.deltaTime;
                transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y + Mathf.Clamp(angle, -maxTurn, maxTurn), 0);
                float facing = bending ? 1 : Mathf.Clamp01(1 - Mathf.Abs(angle) / 110);   // slows to turn
                float want = full * Mathf.Lerp(.35f, 1, facing) * Mathf.Clamp01(d / .2f + .4f);
                speed = Mathf.MoveTowards(speed, want, full * 3 * Time.deltaTime);
                // the paws keep pace with the ground: the clip plays at the speed it is moving (eased, so a wavering steer
                // doesn't flutter the legs)
                if (animator) animator.speed = Mathf.Lerp(animator.speed, rate * Mathf.Max(.3f, speed / Mathf.Max(1e-4f, full)), 1 - Mathf.Exp(-Time.deltaTime * 8));
                // a step up or down just ahead (Seoul's veranda to the yard, 40 cm): a little jump down or up onto it, paws
                // together, rather than the body drifting down with a paw in the air (it floated: the user, Oct 27)
                if (followGround && hopSteps)
                {
                    var ahead = transform.position + transform.forward * .14f;
                    if (Ground(ahead, out float ay) && Mathf.Abs(ay - transform.position.y) > .07f)
                    {
                        var land = transform.position + transform.forward * (.2f + Mathf.Abs(ay - transform.position.y) * .6f);
                        if (Ground(land, out float ly)) { land.y = ly; speed = 0; yield return Hop(land); Gait(trot); continue; }
                    }
                }
                transform.position += transform.forward * Mathf.Min(speed * Time.deltaTime, d);
                // the floor's height, eased and ignoring a few millimetres (two floor pieces a hair apart, stepped between frame
                // by frame, shook her up and down: the user, Oct 26 morning); a real step still takes a third of a second
                if (followGround && Ground(out float gy) && Mathf.Abs(gy - transform.position.y) > .004f) { var p = transform.position; p.y = Mathf.Lerp(p.y, gy, 1 - Mathf.Exp(-Time.deltaTime * 10)); transform.position = p; }
                yield return null;
            }
        }

        // off or onto the raised home: a short puppy hop, the body arcing over the rim
        bool hopping; Vector3 hopTo;
        IEnumerator Hop(Vector3 to)
        {
            if ((to - transform.position).sqrMagnitude < .0025f) { transform.position = to; yield break; }   // a home on the floor: no hop
            hopping = true; hopTo = to;
            Gait(true); if (animator) animator.speed = trotRate;
            var from = transform.position; var flat = to - from; flat.y = 0;
            if (flat.sqrMagnitude > 1e-4f) yield return Turn(Quaternion.LookRotation(flat).eulerAngles.y);
            float dur = Mathf.Clamp(flat.magnitude / .9f, .35f, .8f), rise = .1f + Mathf.Max(0, to.y - from.y);
            for (float t = 0; t < dur; t += Time.deltaTime)
            {
                float u = Mathf.SmoothStep(0, 1, t / dur);
                var p = Vector3.Lerp(from, to, u); p.y += rise * 4 * u * (1 - u);
                transform.position = p; yield return null;
            }
            transform.position = to; hopping = false;
        }

        // pivot: small quick steps round on the spot at turnSpeed (the walk at a slow rate), never the walking-turn clips
        // (revision 102929: at a rate of its own when given, the steps slowed to match, so turning to face the person is an
        // unhurried shuffle round rather than a spin)
        IEnumerator Turn(float yaw, bool pivot = false, float pace = 0)
        {
            bool stepping = false; bool curve = Turns && !pivot;
            float stepRate = pace > 0 ? Mathf.Clamp(pace / Mathf.Max(1, turnSpeed * .6f), .35f, 1) : 1;
            while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, yaw)) > 3)
            {
                float left = Mathf.DeltaAngle(transform.eulerAngles.y, yaw);
                if (!stepping) { if (curve) State(left < 0 ? turnLeftState : turnRightState); else Gait(false); if (animator) animator.speed = curve ? walkRate : walkRate * .6f * stepRate; stepping = true; }   // small steps round on the spot
                float rate = curve ? turnClipRate * walkRate : pace > 0 ? pace : turnSpeed * .6f;
                transform.rotation = Quaternion.Euler(0, Mathf.MoveTowardsAngle(transform.eulerAngles.y, yaw, rate * Time.deltaTime), 0);
                yield return null;
            }
        }

        // ---- not bumping into things: solid colliders (furniture, walls, its own home's rim excepted when hopping)
        readonly RaycastHit[] hits = new RaycastHit[16];
        bool Clear(Vector3 dir, float dist)
        {
            var o = transform.position + Vector3.up * chestHeight;
            if (!followGround) return !(Physics.SphereCast(o, bodyRadius, dir, out var hit, dist, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) && !hit.transform.IsChildOf(transform) && !hit.collider.GetComponentInParent<PetToy>());
            int n = Physics.SphereCastNonAlloc(o, bodyRadius, dir, hits, dist, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < n; i++) if (!hits[i].transform.IsChildOf(transform) && !hits[i].collider.GetComponent<TeleportationArea>() && !hits[i].collider.GetComponentInParent<PetToy>()) return false;   // a floor or a step to walk onto doesn't block, nor a toy (it is what it is going for)
            return true;
        }
        // the walkable floor under it (the highest teleport area within reach of its paws: a step of up to 60 cm)
        bool Ground(out float y) => Ground(transform.position, out y);
        bool Ground(Vector3 at, out float y)
        {
            y = 0; bool found = false;
            int n = Physics.RaycastNonAlloc(at + Vector3.up * .6f, Vector3.down, hits, 1.4f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < n; i++) if (hits[i].collider.GetComponent<TeleportationArea>() && (!found || hits[i].point.y > y)) { y = hits[i].point.y; found = true; }
            return found;
        }
        Vector3 Steer(Vector3 want, float look)
        {
            if (Clear(want, look)) return want;
            foreach (float a in new[] { 25f, -25f, 50f, -50f, 75f, -75f, 100f, -100f, 125f, -125f })
            {
                var d = Quaternion.Euler(0, a, 0) * want;
                if (Clear(d, look)) return d;
            }
            return Vector3.zero;
        }

        int Nearest(Vector3 p)
        {
            int best = 0; float bd = float.MaxValue;
            for (int i = 0; i < waypoints.Length; i++) { var d = waypoints[i] - p; d.y = 0; if (d.sqrMagnitude < bd) { bd = d.sqrMagnitude; best = i; } }
            return best;
        }
        // breadth-first over the waypoint graph (a handful of nodes in the aisles)
        List<Vector3> PathTo(int from, int to)
        {
            var path = new List<Vector3>(); if (waypoints == null || waypoints.Length == 0) return path;
            var prev = new int[waypoints.Length]; for (int i = 0; i < prev.Length; i++) prev[i] = -2;
            var q = new Queue<int>(); q.Enqueue(from); prev[from] = -1;
            while (q.Count > 0)
            {
                int n = q.Dequeue(); if (n == to) break;
                foreach (var l in links)
                {
                    int m = l.x == n ? l.y : l.y == n ? l.x : -1;
                    if (m >= 0 && prev[m] == -2) { prev[m] = n; q.Enqueue(m); }
                }
            }
            if (prev[to] == -2) return path;
            for (int n = to; n >= 0; n = prev[n]) path.Insert(0, waypoints[n]);
            return path;
        }

        // Where it is, in words, for the companion's Describe().
        public string Where() => Moving && !Facing ? (Heading == "home" ? $"on {pronoun} way back to {pronoun} {homeName}" : Heading == "by you" ? "coming over to you" : $"on {pronoun} way to the {Heading}")
                               : AtHome ? $"{homeAt} {pronoun} {homeName}" : Place == "by you" ? "on the floor by you" : Place == "floor" ? "on the café floor" : $"by the {Place}";
    }
}
