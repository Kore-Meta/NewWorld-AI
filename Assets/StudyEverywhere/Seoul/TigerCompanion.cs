using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StudyEverywhere
{
    // Hori, the Seoul café's tiger cub, on her quilted mat between the floor chairs. She lives a cub's day as
    // docs/seoul-cafe/tiger-character/TIGER_RESEARCH.md describes it, a felid's and not a dog's:
    //   - asleep when you arrive (a loaf nap, the head low, 20 breaths a minute); she wakes with a slow stretch, front then
    //     back, and stays up a few minutes, sitting, lying like a sphinx or standing; then her lids grow heavy and she
    //     settles down again;
    //   - awake she grooms (paw licks, then a face-wash), sniffs the floor (sometimes lifting her head in the flehmen lip
    //     curl after), and now and then plays: a slow stalk, head level and still, and a pounce with a butt wiggle; she
    //     greets the person with a chuff (a soft nasal breath, mouth shut, head bobbing), never a wag or a bark;
    //   - her face is layered on top of the body clips with a cat's manners: a full blink only every 20-40 s with softer
    //     half-blinks between, glances at the person of 1-3 s (a long stare is a challenge to a cat), and if the person
    //     keeps staring at her she answers with a slow blink (revision 102929: and turns to face them, GazeUpdate); ear twitches; whiskers and ears forward when
    //     she is intent; her long tail sways slowly with a tip flick now and then (never a side-to-side "happy wag", which
    //     in a felid means unease), turned about the vertical so a tail lying on the floor never dips into it.
    // Every change of pose goes through its own clip (whose first and last frames are the poses either side, all solved
    // onto the floor in Blender), chained through Sleep - Lie - Sit - Stand, so a cross-fade never blends two heights.
    // She talks as Bubble does (PetVoiceLink, /pet/hori): spoken to, she wakes and turns to the person, her reply is
    // spoken in her own calm voice with her mouth following it, and the actions in it run here.
    public sealed class TigerCompanion : MonoBehaviour, IPetCompanion
    {
        public Transform viewer;
        [Header("Rig (found by name when empty)")]
        public Transform head, neck, thoracic, earL, earR, eyeL, eyeR, lidL, lidR;
        public Transform[] tail;
        public SkinnedMeshRenderer face;
        [Tooltip("Degrees the lid bones turn about their X axis to close (from the Blender rig).")]
        public float lidCloseL = 100, lidCloseR = 100;
        [Tooltip("The FBX import turns local-X rotations round: the lids close on negative X in Unity.")]
        public float xSign = -1;
        [Header("Sounds (none sourced yet: she is quiet until they are)")]
        public AudioSource voice;
        public AudioClip[] chuffs, mews, yawns, sighs;
        [Header("Talking (PetVoiceLink)")]
        public string displayName = "Hori";
        [Tooltip("Her speaking voice (the link's TTSSpeaker): the mouth follows its sound.")] public AudioSource speech;
        [Tooltip("The speech bubble over her head (set by the builder).")] public CatBubble bubble;
        public bool Engaged => Time.time < engagedUntil;
        // The roamer (shared with Bubble) walks her round the café with the Walk, Trot, Turn.L and Turn.R clips: while it
        // does, her own day pauses (no actions or head turns of her own), and her breathing, blinks, ears and tail go on.
        public bool Roaming { get; private set; }
        [Tooltip("Walking her round the tea room and the courtyard (found on this object when empty).")] public PetRoamer roamer;
        // Her ball (the user, Oct 27: a toy for each pet): a big tough ball, as zoos give tiger cubs. Thrown, she fetches it
        // (below); now and then by herself she runs to it, stalks it and pounces, knocking it on, two or three rounds, then settles.
        [Tooltip("Her ball (a PetToy).")] public PetToy ball;
        int playRounds; float lastBallThrow = -99, ballPlaySince; bool goingForBall, ballPlay;   // ballPlay: a game is on (her day's moods wait: a groom cut into it, ToyPlayCheck)
        // Fetch (revision 102828, the user: Hori too takes her ball in her mouth and brings it to the person, as Bubble does).
        // Thrown, she runs to it, dips her head and takes it in her jaws, trots it back and lets it go at the person's feet,
        // then sits and waits for the next throw. On her own she still stalks and pounces on it. She carries it at a point
        // on her tongue bone, just ahead of her front teeth (her own rig's bones follow her face, unlike Bubble's scan).
        enum Fetch { None, Chase, Carry, Return, Wait }
        Fetch fetch; float fetchSince, dipT = -1, mouthWideUntil, chaseRetry; int fetchTries; Vector3 chasedAt; Transform mouthPoint;
        bool Carrying => fetch == Fetch.Carry || fetch == Fetch.Return;
        float Reach => mouthPoint ? Mathf.Max(.1f, Vector3.ProjectOnPlane(mouthPoint.position - transform.position, Vector3.up).magnitude) : .4f;
        System.Action pendingRoam;   // a walk asked for while she was lying or sitting: she gets up first
        public void BeginRoam() { steps.Clear(); current = null; oneShot = null; Roaming = true; }
        public void EndRoam(float settleSeconds = 30) { Roaming = false; mode = Mode.Stand; Play("Stand", .25f); modeUntil = Time.time + settleSeconds; nextAct = Time.time + Random.Range(4f, 9f); stillSince = Time.time; }
        // She never stays in one place longer than this (the user, Oct 27): then she gets up and walks on, unless the person
        // is talking with her or has just asked her for something. Her naps are as short.
        [Tooltip("Seconds in one place before she moves on.")] public float stayAtMost = 30;
        float stillSince;

        enum Mode { Sleep, Lie, Sit, Stand }
        struct Step { public string clip; public Mode end; public System.Action start; }
        static readonly Dictionary<(Mode, Mode), string> Moves = new Dictionary<(Mode, Mode), string>
        {
            { (Mode.Lie, Mode.Sit), "LieToSit" }, { (Mode.Sit, Mode.Lie), "SitToLie" }, { (Mode.Lie, Mode.Sleep), "LieToSleep" }, { (Mode.Sleep, Mode.Lie), "SleepToLie" },
            { (Mode.Sit, Mode.Stand), "SitToStand" }, { (Mode.Stand, Mode.Sit), "StandToSit" }, { (Mode.Stand, Mode.Lie), "StandToLie" }, { (Mode.Lie, Mode.Stand), "LieToStand" },
        };
        Animator animator;
        readonly Dictionary<string, float> lengths = new Dictionary<string, float>();
        readonly Queue<Step> steps = new Queue<Step>();
        Step? current; float stepUntil;
        Mode mode = Mode.Sleep;
        string oneShot; float oneShotAt;   // the one-shot playing (Groom, Sniff, Stalk, Pounce, Stretch) and when it began
        float modeUntil, nextAct, sleepy, engagedUntil, breathPhase, breathFast;
        // the face
        float blinkAt, halfAt, blinkT = -1, halfT = -1, halfDepth, slowT = -1, lidNow, chuffT = -1, mewT = -1, flehT = -1, yawnT = -1, talkOpen;
        float glanceUntil, nextGlance, stareFor, lookW, nextEar, earTw, earSide, earFwd, tailPhase, flickT = -1, nextFlick;
        Vector2 eyeAim, headAim; float lookSide = 1;
        int sChuff = -1, sOpen = -1, sYawn = -1, sTongue = -1, sSmile = -1, sLip = -1, sBrowL = -1, sBrowR = -1;
        readonly float[] samples = new float[256];

        void Start()
        {
            animator = GetComponentInChildren<Animator>();
            if (animator && animator.runtimeAnimatorController) foreach (var c in animator.runtimeAnimatorController.animationClips) lengths[c.name] = c.length;
            var byName = new Dictionary<string, Transform>();
            foreach (var t in GetComponentsInChildren<Transform>(true)) if (!byName.ContainsKey(t.name)) byName[t.name] = t;
            Transform F(Transform t, string n) => t ? t : byName.TryGetValue(n, out var x) ? x : null;
            head = F(head, "Head"); neck = F(neck, "Neck"); thoracic = F(thoracic, "Thoracic"); earL = F(earL, "Ear.L"); earR = F(earR, "Ear.R");
            eyeL = F(eyeL, "Eye.L"); eyeR = F(eyeR, "Eye.R"); lidL = F(lidL, "Lid.L"); lidR = F(lidR, "Lid.R");
            if (tail == null || tail.Length == 0) tail = Enumerable.Range(1, 12).Select(i => byName.TryGetValue("Tail" + i, out var t) ? t : null).Where(t => t).ToArray();
            if (!face) face = GetComponentsInChildren<SkinnedMeshRenderer>(true).FirstOrDefault(s => s.sharedMesh && s.sharedMesh.blendShapeCount > 0);
            if (face)
            {
                int I(string n) => face.sharedMesh.GetBlendShapeIndex(n);
                sChuff = I("Chuff"); sOpen = I("MouthOpen"); sYawn = I("Yawn"); sTongue = I("TongueOut"); sSmile = I("Smile"); sLip = I("LipCurl"); sBrowL = I("BrowUp_L"); sBrowR = I("BrowUp_R");
            }
            if (!roamer) roamer = GetComponent<PetRoamer>();
            if (roamer) { roamer.Arrived += OnArrived; roamer.Faced += OnFaced; }
            // where she holds her ball: ahead of the tongue's root along the jaw, past the front teeth (the rig: tongue root
            // 4.5 cm behind the jaw's tip, 5.5 cm ahead of its hinge, full size), its centre a little beyond them
            if (byName.TryGetValue("Jaw", out var jaw) && byName.TryGetValue("Tongue", out var tongue) && ball)
            {
                var along = tongue.position - jaw.position; float unit = along.magnitude / .0554f;   // her size from the bones
                var col = ball.GetComponent<SphereCollider>(); float r = col ? col.radius * ball.transform.lossyScale.x : .08f;
                mouthPoint = new GameObject("Mouth | carries her ball").transform; mouthPoint.SetParent(tongue, false);
                // and half its radius down from the tongue, between the jaws (on the tongue's line it rode over her nose: ToyPlayCheck)
                var down = Vector3.Cross(transform.right, along).normalized;
                mouthPoint.SetPositionAndRotation(tongue.position + along.normalized * (.045f * unit + r * .45f) + down * r * .55f, transform.rotation);
            }
            blinkAt = Time.time + Random.Range(20f, 40f); halfAt = Time.time + Random.Range(5f, 10f); nextGlance = Time.time + 3; nextEar = Time.time + 2; nextFlick = Time.time + Random.Range(8f, 20f);
            if (awakeOnArrival)
            {
                // awake when you arrive (headset review, Oct 25: she should greet you, in view, facing you): lying with her head
                // up, she looks over and chuffs a moment after you appear, then goes about her day
                mode = Mode.Lie; Play("Lie", 0); modeUntil = Time.time + Random.Range(90f, 180f); lidNow = 0; nextAct = Time.time + Random.Range(14f, 22f);
                greetAt = Time.time + 2.5f;
                return;
            }
            // asleep when you arrive; never in step with anything else
            mode = Mode.Sleep; Play("Sleep", 0); modeUntil = Time.time + Random.Range(25f, 70f); lidNow = 1;
        }
        [Tooltip("Awake and greeting the person when they arrive (lying, head up), rather than asleep.")] public bool awakeOnArrival = true;
        float greetAt = -1;

        // ---- the body: loops, changes of pose, one-shots
        void Play(string state, float fade = .12f) { if (animator) animator.CrossFadeInFixedTime(state, fade); }
        float Len(string clip) => lengths.TryGetValue(clip, out var l) ? l : 1f;
        static string Loop(Mode m) => m.ToString();
        Mode Planned => steps.Count > 0 ? steps.Last().end : current.HasValue ? current.Value.end : mode;
        bool Busy => current.HasValue || steps.Count > 0;
        // the shortest chain of changes from one pose to another (Sleep - Lie - Sit/Stand, Sit - Stand)
        static List<string> Path(Mode from, Mode to)
        {
            var prev = new Dictionary<Mode, (Mode, string)>(); var q = new Queue<Mode>(); q.Enqueue(from); var seen = new HashSet<Mode> { from };
            while (q.Count > 0)
            {
                var m = q.Dequeue(); if (m == to) break;
                foreach (var kv in Moves) if (kv.Key.Item1 == m && seen.Add(kv.Key.Item2)) { prev[kv.Key.Item2] = (m, kv.Value); q.Enqueue(kv.Key.Item2); }
            }
            var path = new List<string>(); var at = to;
            while (at != from && prev.ContainsKey(at)) { path.Insert(0, prev[at].Item2); at = prev[at].Item1; }
            return path;
        }
        void GoTo(Mode m)
        {
            var from = Planned;
            foreach (var clip in Path(from, m)) { var key = Moves.First(kv => kv.Value == clip).Key; steps.Enqueue(new Step { clip = clip, end = key.Item2 }); }
        }
        void Act(string clip, Mode from, Mode end, System.Action start = null)
        {
            GoTo(from); steps.Enqueue(new Step { clip = clip, end = end, start = start });
        }
        void Settle(Mode m, float seconds) { GoTo(m); modeUntil = Time.time + seconds; nextAct = Time.time + Random.Range(6f, 14f); }

        void Update()
        {
            float now = Time.time;
            if (greetAt > 0 && now >= greetAt) { greetAt = -1; if (viewer && !Roaming && !Busy) { Attend(5); Chuff(); } }
            // her ball thrown: once it slows (or a second after it left the hand) she is off after it, waking if she slept
            // (revision 102828: she fetches it, as Bubble does)
            if (ball && ball.ThrownAt > lastBallThrow && !ball.Held && !ball.Carried && (fetch == Fetch.None || fetch == Fetch.Wait) && (ball.Velocity.magnitude < 1.5f || now - ball.ThrownAt > 1))
            { lastBallThrow = ball.ThrownAt; if (now - ball.ThrownAt < 10) { sleepy = 0; StartFetch(); } }
            FetchUpdate(now);
            GazeUpdate(now);
            if (Roaming) return;
            // the queue: each clip plays whole, then the next; when it runs dry she rests in the loop of where she ended
            if (current.HasValue && now >= stepUntil) { mode = current.Value.end; current = null; if (steps.Count == 0) { Play(Loop(mode), .1f); oneShot = null; } }
            if (!current.HasValue && steps.Count > 0)
            {
                var s = steps.Dequeue(); current = s; Play(s.clip, .08f); stepUntil = now + Len(s.clip) - .02f; s.start?.Invoke();
                oneShot = Moves.ContainsValue(s.clip) ? null : s.clip; oneShotAt = now;
                if (s.end == Mode.Sleep) Say(sighs, .4f);
            }
            if (Busy) return;
            if (ballPlay && now - ballPlaySince > 60) ballPlay = false;   // (never stuck in a game)
            if ((ballPlay || fetch != Fetch.None) && pendingRoam == null) return;
            // a walk waiting on her getting up
            if (pendingRoam != null) { if (mode == Mode.Stand) { var go = pendingRoam; pendingRoam = null; BeginRoam(); go(); } else GoTo(Mode.Stand); return; }
            // the day: asleep most of it; awake a few minutes; heavy lids, then sleep
            if (Engaged && modeUntil < now + 20) modeUntil = now + 20;
            if (now >= modeUntil)
            {
                if (mode == Mode.Sleep) { sleepy = 0; Stretch(); modeUntil = now + Random.Range(120f, 300f); return; }
                sleepy += Time.deltaTime * .15f;
                if (sleepy >= 1 && roamer && !roamer.AtHome) { Roam(() => roamer.GoHome()); return; }   // she sleeps in her own spot: back there first
                if (sleepy >= 1) { sleepy = 0; Settle(Mode.Sleep, Random.Range(stayAtMost * .7f, stayAtMost)); return; }
            }
            // long enough here: off somewhere else (a run now and then, else a stroll, sometimes back to her spot)
            if (roamer && mode != Mode.Sleep && !Engaged && now - stillSince > stayAtMost)
            {
                stillSince = now;
                if (Random.value < .4f) { breathFast = now + 30; Roam(() => roamer.RunAbout(Random.Range(2, 4))); }   // runs often (the user, Oct 27, twice)
                else if (!roamer.AtHome && Random.value < .3f) Roam(() => roamer.GoHome()); else Roam(() => roamer.Wander(Random.value < .15f));
                nextAct = now + Random.Range(9f, 20f); return;
            }
            if (mode == Mode.Sleep || now < nextAct || Engaged) return;
            // now and then a game with her ball by herself
            if (ball && ball.Resting && Random.value < .15f) { playRounds = Random.Range(1, 3); GoForBall(); nextAct = now + Random.Range(9f, 20f); return; }
            // now and then off for a slow wander round the room or out to the courtyard, or back to her spot
            if (roamer && Random.value < (roamer.AtHome ? .15f : .35f))
            {
                // now and then a playful run about the room and the yard, spot to spot without stopping (the user, Oct 27)
                if (Random.value < .4f) { breathFast = now + 30; Roam(() => roamer.RunAbout(Random.Range(2, 4))); }   // runs often (the user, Oct 27, twice)
                else if (!roamer.AtHome && Random.value < .5f) Roam(() => roamer.GoHome()); else { bool trot = Random.value < .15f; Roam(() => roamer.Wander(trot)); }
                nextAct = now + Random.Range(9f, 20f); return;
            }
            float r = Random.value;
            if (r < .22f) Settle(mode == Mode.Sit ? (Random.value < .6f ? Mode.Lie : Mode.Stand) : mode == Mode.Lie ? Mode.Sit : (Random.value < .5f ? Mode.Sit : Mode.Lie), modeUntil - now);
            else if (r < .40f) Groom();
            else if (r < .55f) Sniff();
            else if (r < .65f) { Stalk(); if (Random.value < .6f) Pounce(); }   // a little play: a creep, mostly ending in a pounce
            else if (r < .72f) Yawn();
            else if (r < .80f && viewer) { Attend(4); Chuff(); }
            nextAct = now + Random.Range(9f, 20f);
        }
        void Say(AudioClip[] set, float volume = 1)
        {
            if (!voice || set == null || set.Length == 0) return;
            voice.pitch = Random.Range(.97f, 1.03f); voice.PlayOneShot(set[Random.Range(0, set.Length)], volume);
        }
        void Stretch() { Act("Stretch", Mode.Lie, Mode.Stand, () => { yawnT = 0; Say(yawns, .45f); }); }
        void Groom() { Act("Groom", Mode.Sit, Mode.Sit); }
        void Sniff() { Act("Sniff", Mode.Stand, Mode.Stand); if (Random.value < .3f) steps.Enqueue(new Step { clip = "Stand", end = Mode.Stand, start = () => flehT = 0 }); }
        void Stalk() { Act("Stalk", Mode.Stand, Mode.Stand); }
        void Pounce() { Act("Pounce", Mode.Stand, Mode.Stand, () => breathFast = Time.time + 25); }
        void Yawn() { yawnT = 0; Say(yawns, .45f); }
        void Chuff() { chuffT = 0; Say(chuffs, .5f); earFwd = 1; }
        void SlowBlink() { slowT = 0; }
        // a walk (the roamer's): from standing; she hands her body over until she arrives (OnArrived)
        void Roam(System.Action go)
        {
            if (!roamer) return;
            if (Roaming) { go(); return; }
            pendingRoam = go; if (Planned != Mode.Stand) GoTo(Mode.Stand);
        }
        // ---- looked at (revision 102929, the user, Oct 29: when the person looks at her, she turns to face them). The
        // person's gaze resting on her (PetRoamer.Gazed: their head's line of sight through her body) for a third of a
        // second: her eyes go to them and her head follows (the look in LateUpdate, held on them while they look); held a
        // second, with them more than 40 degrees off her nose, she gets up (through her own clips: Lie - Stand), turns round
        // on the spot to face them, and settles back into the pose she was in (a cat repositions; it doesn't spin lying
        // down). Asleep, a steady look of 2.5 s wakes her (her head comes up). Never in the middle of something: not while
        // she runs after, carries or brings back her ball, plays with it, walks somewhere, or is part-way through a clip
        // (a groom, a sniff, a change of pose); once turned, not again for 4 s. Waiting for a throw, she does turn.
        float gazeFor, faceBlockUntil; Mode poseBeforeFace; bool facingPerson;
        void GazeUpdate(float now)
        {
            if (!viewer || !head) return;
            bool seen = PetRoamer.Gazed(viewer, transform.position + Vector3.up * .2f, head.position, .25f);
            gazeFor = seen ? gazeFor + Time.deltaTime : Mathf.Max(0, gazeFor - Time.deltaTime * 2);
            if (gazeFor < .35f) return;
            bool busy = Roaming || pendingRoam != null || Busy || ballPlay || goingForBall || (fetch != Fetch.None && fetch != Fetch.Wait) || now < faceBlockUntil;
            if (Planned == Mode.Sleep)
            {
                if (gazeFor > 2.5f && !busy) { sleepy = 0; Settle(Mode.Lie, Random.Range(60f, 150f)); earFwd = 1; glanceUntil = now + 4; faceBlockUntil = now + 2; }
                return;
            }
            glanceUntil = Mathf.Max(glanceUntil, now + 1.5f); nextGlance = Mathf.Max(nextGlance, glanceUntil + 2);   // eyes, then head
            if (busy || gazeFor < 1.1f || !roamer || sleepy >= 1) return;
            var to = viewer.position - transform.position; to.y = 0;
            if (to.sqrMagnitude < .01f || Vector3.Angle(Vector3.ProjectOnPlane(transform.forward, Vector3.up), to) < 40) return;
            poseBeforeFace = mode; facingPerson = true; stillSince = now; faceBlockUntil = now + 4;
            var at = viewer.position; Roam(() => roamer.Face(at));
        }
        // turned to the person: back down into the pose she was in, facing them, her eyes on them a while
        void OnFaced()
        {
            if (!facingPerson) return;
            facingPerson = false; EndRoam(Mathf.Max(modeUntil - Time.time, 30));
            if (poseBeforeFace != Mode.Stand) Settle(poseBeforeFace == Mode.Sleep ? Mode.Lie : poseBeforeFace, Mathf.Max(modeUntil - Time.time, 30));
            glanceUntil = Time.time + 3; faceBlockUntil = Time.time + 4; nextAct = Mathf.Max(nextAct, Time.time + 4);
        }

        // ---- her ball
        void GoForBall()
        {
            if (!ball || !roamer || ball.Held || ball.Carried) { ballPlay = false; return; }
            goingForBall = true; stillSince = Time.time;
            if (!ballPlay) ballPlaySince = Time.time; ballPlay = true;
            var at = ball.transform.position;
            Roam(() => roamer.GoToward(at, .55f, "ball", true, run: true));
        }
        System.Collections.IEnumerator PounceBall()
        {
            stillSince = Time.time;
            Stalk(); Pounce();
            for (float t = 0; t < 8 && oneShot != "Pounce"; t += Time.deltaTime) yield return null;   // the stalk, then the pounce begins
            yield return new WaitForSeconds(Len("Pounce") * .45f);   // her forepaws come down on it
            if (ball && !ball.Held && !ball.Carried)
            {
                var to = ball.transform.position - transform.position; to.y = 0;
                if (to.magnitude < 1f) ball.Nudge((to.sqrMagnitude > 1e-4f ? to.normalized : transform.forward) * Random.Range(1.4f, 2.4f) + Vector3.up * Random.Range(.3f, .8f));
            }
            for (float t = 0; t < 8 && Busy; t += Time.deltaTime) yield return null;
            stillSince = Time.time;
            if (!ballPlay) yield break;   // the game was called off meanwhile (asked for something else)
            if (--playRounds > 0 && !Engaged) { yield return new WaitForSeconds(Random.Range(.6f, 1.5f)); GoForBall(); }
            else { ballPlay = false; Settle(Random.value < .5f ? Mode.Sit : Mode.Lie, 20); }
        }

        // ---- fetch (revision 102828)
        void StartFetch()
        {
            if (!ball || !roamer || ball.Held || ball.Carried) return;
            if (!mouthPoint) { playRounds = Random.Range(2, 4); GoForBall(); return; }   // (no mouth to carry it in: the pounce game)
            ballPlay = false; goingForBall = false; playRounds = 0;
            fetch = Fetch.Chase; fetchTries = 0; fetchSince = Time.time; stillSince = Time.time; GoBall();
        }
        void GoBall()
        {
            chasedAt = ball.transform.position; chaseRetry = Time.time + .6f;
            Roam(() => roamer.GoToward(chasedAt, Reach, "ball", true, run: true));
        }
        void FetchUpdate(float now)
        {
            if (fetch == Fetch.None || !ball || !roamer) return;
            // the person took the ball back while she ran for it: she stops and sits, waiting for the throw
            if (ball.Held && fetch == Fetch.Chase) { fetch = Fetch.Wait; fetchSince = now; pendingRoam = null; if (Roaming) roamer.Stop(); Settle(Mode.Sit, 60); }
            // still rolling where she was heading: after it
            if (fetch == Fetch.Chase && Roaming && now > chaseRetry && (ball.transform.position - chasedAt).magnitude > .35f) GoBall();
            // the ball at the person's feet: she waits a while for the next throw, then goes about her day
            if (fetch == Fetch.Wait && now - fetchSince > 25) EndFetch();
            if (fetch != Fetch.None && fetch != Fetch.Wait && now - fetchSince > 60) EndFetch();   // (never stuck in a game)
        }
        bool FetchArrived(string place)
        {
            if (fetch == Fetch.Wait) { EndRoam(60); return true; }   // (stopped on the way: the person has the ball again)
            if (fetch == Fetch.Chase)
            {
                EndRoam(20);
                var off = ball.transform.position - mouthPoint.position; off.y = 0;
                if (off.magnitude < Reach * .5f + .2f && !ball.Held) { StartCoroutine(TakeBall()); return true; }
                if (++fetchTries < 4 && !ball.Held) { GoBall(); return true; }
                EndFetch(); return true;
            }
            if (fetch == Fetch.Return)
            {
                // at the person's feet (or wherever the way ran out): her jaws open and the ball rolls out in front of her
                EndRoam(60); fetch = Fetch.Wait; fetchSince = Time.time; mouthWideUntil = Time.time + .4f;
                ball.Drop(mouthPoint.position + transform.forward * .03f, transform.forward * .3f);
                Settle(Mode.Sit, 60);
                if (place == "by you") { Attend(6); Chuff(); }
                return true;
            }
            return false;
        }
        System.Collections.IEnumerator TakeBall()
        {
            fetch = Fetch.Carry; dipT = 0; mouthWideUntil = Time.time + .55f;
            yield return new WaitForSeconds(.35f);   // her head down at the ball, jaws wide
            if (ball.Held || fetch != Fetch.Carry) { EndFetch(); yield break; }
            ball.CarryIn(mouthPoint);
            yield return new WaitForSeconds(.6f);
            if (fetch != Fetch.Carry) yield break;
            fetch = Fetch.Return; breathFast = Time.time + 30;
            Roam(() => roamer.ComeTo(true));
        }
        void EndFetch()
        {
            if (ball && ball.Carried) ball.Drop(null, transform.forward * .2f);
            fetch = Fetch.None; dipT = -1; nextAct = Time.time + Random.Range(4f, 9f);
        }

        void OnArrived(string place)
        {
            if (fetch != Fetch.None && FetchArrived(place)) return;
            if (goingForBall)
            {
                goingForBall = false; EndRoam(20);
                var to = ball ? ball.transform.position - transform.position : Vector3.zero; to.y = 0;
                if (ball && to.magnitude < 1.1f) { StartCoroutine(PounceBall()); return; }
                if (--playRounds > 0) { GoForBall(); return; }
                ballPlay = false; return;
            }
            if (sleepy >= 1 && roamer.AtHome) { EndRoam(0); return; }   // home to sleep: the day settles her down (Update)
            EndRoam(place == "home" ? 90 : 40);
            if (place == "by you") { Settle(Mode.Sit, 60); Attend(6); Chuff(); }
            else if (place == "home") Settle(Random.value < .6f ? Mode.Lie : Mode.Sit, 90);
            else if (Random.value < .4f) Sniff();
            else Settle(Random.value < .5f ? Mode.Sit : Mode.Lie, 40);
        }

        // ---- talking (PetVoiceLink)
        string IPetCompanion.DisplayName => displayName;
        CatBubble IPetCompanion.Bubble => bubble;
        AudioSource IPetCompanion.Speech { get => speech; set => speech = value; }
        string IPetCompanion.Unreachable => "Mm, I can't reach my thoughts right now. Try me again in a little while.";
        string IPetCompanion.UnreachableAction => "slow_blink";

        // Spoken to: she wakes if asleep (her head comes up), turns to the person and keeps them in view while they talk.
        public void Attend(float seconds = 12)
        {
            engagedUntil = Mathf.Max(engagedUntil, Time.time + seconds);
            if (Planned == Mode.Sleep && !Busy) { sleepy = 0; Settle(Mode.Lie, Random.Range(90f, 240f)); }
            glanceUntil = Time.time + seconds; nextGlance = glanceUntil + 4; earFwd = Mathf.Max(earFwd, .7f);
        }

        // What she is doing, for the relay (no words of the person's: only her own state and how far away they are).
        public string Describe()
        {
            string where = roamer ? roamer.Where() : "on her spot";
            string doing = Roaming || pendingRoam != null ? (roamer ? where : "walking about the café") : oneShot switch
            {
                "Groom" => "washing her face with a paw", "Sniff" => "sniffing the floor", "Stalk" => "stalking something in play", "Pounce" => "pouncing in play", "Stretch" => "stretching",
                _ => (mode switch { Mode.Sleep => "asleep", Mode.Sit => "sitting", Mode.Stand => "standing", _ => "lying with her head up" }) + " " + where,
            };
            string near = viewer ? $", {Vector3.Distance(viewer.position, transform.position):0.0} m from you" : "";
            if (goingForBall || playRounds > 0) doing = "playing with her ball";
            if (fetch == Fetch.Chase) doing = "running after her ball"; else if (Carrying) doing = "bringing her ball back in her mouth"; else if (fetch == Fetch.Wait) doing = "waiting for the person to throw her ball again";
            return $"{displayName} the tiger cub is {doing}{near}. The person can throw her big ball for her to fetch. She can: play_ball, come_here, go_to ({(roamer ? string.Join(", ", roamer.SpotNames()) : "")}), go_to_spot, walk, trot, run_around, wake, sit, lie_down, stand, sleep, chuff, slow_blink, groom, sniff, flehmen, stalk, pounce, yawn, stretch, mew.";
        }

        // One action from the relay (the tool names the server gives her).
        public void Do(string command)
        {
            var c = (command ?? "").Trim().ToLowerInvariant();
            stillSince = Time.time;   // asked for something: she gives it her half-minute before wandering off
            if (c != "play" && c != "play_ball" && c != "chuff" && c != "slow_blink" && c != "mew" && c != "look_at_user") { ballPlay = false; playRounds = 0; goingForBall = false; if (fetch != Fetch.None) EndFetch(); }   // asked for something else: the game is over
            string arg = ""; int sp = c.IndexOf(' '); if (sp > 0) { arg = c.Substring(sp + 1).Trim(); c = c.Substring(0, sp); }
            // asked to walk somewhere: the roamer takes her (from wherever she is, getting up first)
            switch (c)
            {
                case "come_here": if (roamer) { if (Planned == Mode.Sleep) sleepy = 0; Roam(() => roamer.ComeTo(false)); Attend(); } return;
                case "go_to": { var spot = roamer ? roamer.Find(arg) : null; if (spot != null) { if (Planned == Mode.Sleep) sleepy = 0; Roam(() => roamer.GoTo(spot)); } else if (roamer && !roamer.AtHome && (arg.Contains("spot") || arg.Contains("mat") || arg == "home")) Roam(() => roamer.GoHome()); return; }
                case "go_to_spot": case "go_home": if (roamer && !roamer.AtHome) Roam(() => roamer.GoHome()); return;
                case "walk": if (roamer) { if (Planned == Mode.Sleep) sleepy = 0; Roam(() => roamer.Wander(false)); } return;
                case "trot": if (roamer) { if (Planned == Mode.Sleep) sleepy = 0; Roam(() => roamer.Wander(true)); } return;
                // asked to run around (the user, Oct 25: she said she couldn't): a playful run from place to place, three legs
                // at her fastest gait without stopping, then she settles where she ends up
                case "play": case "play_ball": if (ball) { if (Planned == Mode.Sleep) sleepy = 0; playRounds = 3; GoForBall(); } return;
                case "run_around": case "run": case "zoomies": if (roamer) { if (Planned == Mode.Sleep) sleepy = 0; breathFast = Time.time + 30; Roam(() => roamer.RunAbout(3)); } return;
            }
            // asked to do something on the spot while walking: she stops where she is and does it (the face's actions carry on as she walks)
            bool faceOnly = c == "chuff" || c == "slow_blink" || c == "mew" || c == "look_at_user";
            if (!faceOnly && (Roaming || pendingRoam != null)) { pendingRoam = null; if (Roaming) { roamer.Stop(); EndRoam(30); } }
            if (c == "sleep" && roamer && !roamer.AtHome) { engagedUntil = 0; sleepy = 1; modeUntil = Time.time; return; }   // back to her spot, then asleep (Update)
            void Up() { if (Planned == Mode.Sleep) { sleepy = 0; Settle(Mode.Lie, 120); } }
            switch (c)
            {
                case "wake": if (Planned == Mode.Sleep) { sleepy = 0; Stretch(); modeUntil = Time.time + 180; } break;
                case "sleep": sleepy = 0; engagedUntil = 0; Settle(Mode.Sleep, Random.Range(60f, 120f)); break;   // asked to: a longer nap than her own
                case "sit": Up(); Settle(Mode.Sit, Random.Range(60f, 180f)); break;
                case "lie_down": Up(); Settle(Mode.Lie, Random.Range(60f, 180f)); break;
                case "stand": Up(); Settle(Mode.Stand, Random.Range(30f, 90f)); break;
                case "chuff": Up(); Chuff(); break;
                case "slow_blink": SlowBlink(); break;
                case "groom": Up(); Groom(); break;
                case "sniff": Up(); Sniff(); break;
                case "flehmen": Up(); flehT = 0; break;
                case "stalk": Up(); Stalk(); break;
                case "pounce": Up(); Pounce(); break;
                case "yawn": Yawn(); break;
                case "stretch": if (Planned == Mode.Sleep) { sleepy = 0; } Stretch(); break;
                case "mew": mewT = 0; Say(mews, .5f); break;
                case "look_at_user": Attend(); break;
            }
        }

        // ---- the face and the procedural layers, on top of the clips
        static float Bump(float t, float up, float hold, float down) => t < 0 ? 0 : t < up ? Mathf.SmoothStep(0, 1, t / up) : t < up + hold ? 1 : t < up + hold + down ? Mathf.SmoothStep(1, 0, (t - up - hold) / down) : 0;
        void LateUpdate()
        {
            float now = Time.time, dt = Time.deltaTime; bool asleep = mode == Mode.Sleep && !current.HasValue;
            string shot = oneShot; float st = now - oneShotAt;
            bool headBusy = Carrying || Roaming || shot == "Groom" || shot == "Sniff" || shot == "Stalk" || shot == "Pounce" || shot == "Stretch";

            // ---- breathing: 20 a minute asleep, 28 awake, 40 for a while after play (the belly and chest, the mouth shut)
            breathPhase += dt * (asleep ? 20f : now < breathFast ? 40f : 28f) / 60f * Mathf.PI * 2;
            if (thoracic) thoracic.localRotation *= Quaternion.Euler(Mathf.Sin(breathPhase) * (asleep ? 1.3f : .8f), 0, 0);

            // ---- lids: a full blink every 20-40 s (60 ms down, 40 held, 80 up); a softer half-blink every 5-12 s; the slow
            // blink (two half-blinks, narrow to 70%, hold, open); heavy lids when sleepy; shut asleep; squeezed in a yawn
            if (!asleep && now >= blinkAt) { blinkT = 0; blinkAt = now + Random.Range(20f, 40f); }
            if (!asleep && now >= halfAt) { halfT = 0; halfDepth = Random.Range(.4f, .6f); halfAt = now + Random.Range(5f, 12f); }
            float lid = asleep ? 1 : sleepy * .6f;
            if (blinkT >= 0) { blinkT += dt; lid = Mathf.Max(lid, Bump(blinkT, .06f, .04f, .08f)); if (blinkT > .2f) blinkT = -1; }
            if (halfT >= 0) { halfT += dt; lid = Mathf.Max(lid, halfDepth * Bump(halfT, .15f, .05f, .18f)); if (halfT > .4f) halfT = -1; }
            if (slowT >= 0)
            {
                slowT += dt; float s = slowT;
                float v = s < .7f ? .5f * Bump(s, .15f, .05f, .15f) + .5f * Bump(s - .35f, .15f, .05f, .15f) : .7f * Bump(s - .7f, .4f, 1.2f, .5f);
                lid = Mathf.Max(lid, v); if (s > 2.9f) slowT = -1;
            }
            if (shot == "Groom") lid = Mathf.Max(lid, .45f);   // eyes half-closed while she washes
            if (yawnT >= 0) lid = Mathf.Max(lid, Bump(yawnT, .9f, .7f, .5f));
            if (flehT >= 0) lid = Mathf.Max(lid, .5f * Bump(flehT, .4f, 2.0f, .5f));
            lidNow = Mathf.MoveTowards(lidNow, lid, dt * (asleep ? .6f : 14f));
            if (lidL) lidL.localRotation *= Quaternion.Euler(xSign * lidNow * lidCloseL, 0, 0);
            if (lidR) lidR.localRotation *= Quaternion.Euler(xSign * lidNow * lidCloseR, 0, 0);

            // ---- look, with a cat's manners: glances at the person of 1-3 s, then looking away 6-20 s; a person who keeps
            // staring at her (4 s) gets a slow blink and a look away. Eyes lead (15 deg), the head follows 0.25 s behind.
            if (viewer && head)
            {
                var toHer = head.position - viewer.position;
                bool stared = Vector3.Angle(viewer.forward, toHer) < 10 && toHer.magnitude < 4;
                stareFor = stared && !asleep ? stareFor + dt : 0;
                // (revision 102929, the user, Oct 29: looked at, she turns to face the person, GazeUpdate; the slow blink stays,
                // a cat's friendly answer, but she no longer looks away after it: she keeps her face to them while they look)
                if (stareFor > 4 && slowT < 0) { SlowBlink(); stareFor = -6; }
                if (!asleep && !Engaged && now >= nextGlance) { glanceUntil = now + Random.Range(1f, 3f); nextGlance = glanceUntil + Random.Range(6f, 20f); }
                var to = viewer.position - head.position; var fwd = transform.forward;
                // a person behind her: the angle to them sat on the +-180 seam and flipped side every frame, so the clamped turn
                // swung from 70 degrees one way to 70 the other (measured, HoriPlayCheck: 324 reversals in 5 s, the vibration
                // as she sat, the user, Oct 27). Near the seam the side she last turned to holds, and past 115 degrees she
                // doesn't crane round at all (a cat turns its ears, then its body)
                float raw = Vector3.SignedAngle(Vector3.ProjectOnPlane(fwd, Vector3.up), Vector3.ProjectOnPlane(to, Vector3.up), Vector3.up);
                if (Mathf.Abs(raw) > 150) raw = Mathf.Abs(raw) * lookSide; else if (Mathf.Abs(raw) > 1) lookSide = Mathf.Sign(raw);
                bool behind = Mathf.Abs(raw) > 115;
                lookW = Mathf.MoveTowards(lookW, !asleep && !headBusy && !behind && (now < glanceUntil || Engaged) ? 1 : 0, dt * 2.5f);
                float yaw = Mathf.Clamp(raw, -70, 70);
                // the person's height above or below her eye line (measured against her forward, it swung wildly whenever the
                // person stood beside her, where her forward is edge-on to that plane: the vibration as she sat side-on, the
                // user, Oct 27)
                float pitch = Mathf.Clamp(Mathf.Atan2(to.y, new Vector2(to.x, to.z).magnitude) * Mathf.Rad2Deg, -50, 40);
                var want = new Vector2(yaw, pitch) * lookW;
                eyeAim = Vector2.Lerp(eyeAim, new Vector2(Mathf.Clamp(want.x, -15, 15), Mathf.Clamp(want.y, -12, 12)), 1 - Mathf.Exp(-dt * 16));
                headAim = Vector2.Lerp(headAim, want - eyeAim * .5f, 1 - Mathf.Exp(-dt * 4));
                if (neck) neck.rotation = Quaternion.AngleAxis(headAim.x * .55f, Vector3.up) * neck.rotation;
                head.rotation = Quaternion.AngleAxis(headAim.x * .45f, Vector3.up) * Quaternion.AngleAxis(-headAim.y, transform.right) * head.rotation;
                foreach (var e in new[] { eyeL, eyeR }) if (e) e.rotation = Quaternion.AngleAxis(eyeAim.x - headAim.x * .45f, Vector3.up) * Quaternion.AngleAxis(-eyeAim.y, transform.right) * e.rotation;
            }

            // ---- fetch (revision 102828): her head dips to the floor to take the ball (down 0.35 s, held, up 0.35 s); the mouth
            // point rides her tongue bone, so it goes down with her head and the ball comes up in her jaws
            if (dipT >= 0 && head)
            {
                dipT += dt; float d = dipT < .35f ? Mathf.SmoothStep(0, 1, dipT / .35f) : dipT < .6f ? 1 : Mathf.SmoothStep(1, 0, (dipT - .6f) / .35f);
                if (neck) neck.rotation = Quaternion.AngleAxis(32 * d, transform.right) * neck.rotation;
                head.rotation = Quaternion.AngleAxis(28 * d, transform.right) * head.rotation;
                if (dipT > .95f) dipT = -1;
            }

            // ---- the chuff: 3 soft nasal pulses 0.3 s apart, the whisker pads puffing and the head bobbing down-forward
            float chuffAmt = 0, bob = 0;
            if (chuffT >= 0)
            {
                chuffT += dt;
                for (int k = 0; k < 3; k++) { float p = Bump(chuffT - k * .3f, .08f, .04f, .14f); chuffAmt = Mathf.Max(chuffAmt, p); bob = Mathf.Max(bob, p); }
                if (chuffT > 1.4f) { chuffT = -1; if (Random.value < .5f) SlowBlink(); }
            }
            // sniffing: quick nostril bursts while the nose is down
            if (shot == "Sniff" && st > .5f && st < Len("Sniff") - .4f) chuffAmt = Mathf.Max(chuffAmt, .35f * Mathf.Clamp01(Mathf.Sin(st * Mathf.PI * 2 * 5)));
            if (head && bob > 0) head.rotation = Quaternion.AngleAxis(5 * bob, transform.right) * head.rotation;
            // the flehmen: the head lifts and holds still, the upper lip curls, the mouth a little open, the eyes narrowed
            float flehAmt = 0;
            if (flehT >= 0) { flehT += dt; flehAmt = Bump(flehT, .4f, 2.0f, .5f); if (flehT > 3) flehT = -1; if (head) head.rotation = Quaternion.AngleAxis(-18 * flehAmt, transform.right) * head.rotation; }
            // the yawn: 1 s up (the head lifting), 0.7 s at its widest (45 deg at most, in the shape), 0.5 s closing
            float yawnAmt = 0;
            if (yawnT >= 0) { yawnT += dt; yawnAmt = Bump(yawnT, 1f, .7f, .5f); if (yawnT > 2.4f) yawnT = -1; if (head) head.rotation = Quaternion.AngleAxis(-12 * yawnAmt, transform.right) * head.rotation; }
            // a mew: the jaw a little open, the head lifting
            float mewAmt = 0;
            if (mewT >= 0) { mewT += dt; mewAmt = Bump(mewT, .12f, .2f, .25f); if (mewT > .6f) mewT = -1; }

            // ---- ears: a quick swivel toward a sound now and then; forward when intent (chuff, sniff, stalk, pounce, being
            // talked to); drifting sideways when sleepy or asleep. Never folded back: the white spots never face the person.
            if (!asleep && now >= nextEar) { earTw = 1; earSide = Random.value < .5f ? -1 : 1; nextEar = now + Random.Range(3f, 12f); }
            earTw = Mathf.Max(0, earTw - dt / .12f);
            bool intent = shot == "Sniff" || shot == "Stalk" || shot == "Pounce" || chuffT >= 0 || Engaged;
            earFwd = Mathf.MoveTowards(earFwd, intent ? 1 : 0, dt * 2);
            float drift = asleep ? 16 : sleepy * 12;
            if (earL) earL.localRotation *= Quaternion.Euler(-drift * .4f + earFwd * 6, 0, -drift + earFwd * 5 + (earSide > 0 ? earTw * 14 : 0));
            if (earR) earR.localRotation *= Quaternion.Euler(-drift * .4f + earFwd * 6, 0, drift - earFwd * 5 - (earSide < 0 ? earTw * 14 : 0));

            // ---- the tail: a slow sway of its outer half (0.15 Hz, a few degrees a joint) and a tip flick every 10-30 s;
            // both turned about the vertical, so a tail resting on the floor slides over it, never into it. Still while she
            // stalks (the clip holds it low) and asleep it barely moves.
            if (tail != null && tail.Length >= 4 && shot != "Pounce")
            {
                tailPhase += dt * .15f * Mathf.PI * 2;
                float amp = asleep ? .6f : shot == "Stalk" ? 0 : 2.2f;
                if (!asleep && now >= nextFlick) { flickT = 0; nextFlick = now + Random.Range(10f, 30f); }
                float flick = 0; if (flickT >= 0) { flickT += dt; flick = Mathf.Sin(Mathf.Clamp01(flickT / .45f) * Mathf.PI) * 14; if (flickT > .45f) flickT = -1; }
                for (int i = tail.Length / 2; i < tail.Length; i++)
                {
                    float k = (i - tail.Length / 2 + 1f) / (tail.Length - tail.Length / 2);
                    float a = amp * Mathf.Sin(tailPhase - i * .5f) + (i >= tail.Length - 2 ? flick * k : 0);
                    tail[i].rotation = Quaternion.AngleAxis(a, Vector3.up) * tail[i].rotation;
                }
            }

            // ---- mouth: talking (the jaw opens a little with the loudness of her voice), the chuff's pads, the yawn, the
            // flehmen, a mew, and the tongue while she licks her paw (2.5 strokes a second in the groom's first 1.6 s)
            float loud = 0;
            if (speech && speech.isPlaying)
            {
                speech.GetOutputData(samples, 0); float sum = 0;
                foreach (var v in samples) sum += v * v;
                loud = Mathf.Clamp01(Mathf.Sqrt(sum / samples.Length) * 12); engagedUntil = Mathf.Max(engagedUntil, now + 4);
            }
            // the jaw follows each syllable: quick to open, a little slower to close, and wide enough to read from the chair
            // (headset review, Oct 25: at most 60% of MouthOpen it was hard to tell she was speaking)
            talkOpen = Mathf.Lerp(talkOpen, loud * 100, 1 - Mathf.Exp(-dt * (loud * 100 > talkOpen ? 30 : 14)));
            if (head && talkOpen > 1) head.rotation = Quaternion.AngleAxis(-2.5f * talkOpen / 100, transform.right) * head.rotation;   // the head lifts a touch on each
            float lick = shot == "Groom" && st > .4f && st < 2.0f ? Mathf.Clamp01(Mathf.Sin((st - .4f) * Mathf.PI * 2 * 2.5f)) : 0;
            if (face)
            {
                void W(int i, float w) { if (i >= 0) face.SetBlendShapeWeight(i, Mathf.Clamp(w, 0, 100)); }
                // (revision 102828: her jaws round the ball while she carries it, wide to take it or let it go)
                W(sOpen, Mathf.Max(now < mouthWideUntil ? 85 : Carrying ? 60 : 0, Mathf.Max(talkOpen, Mathf.Max(30 * flehAmt, Mathf.Max(35 * mewAmt, 45 * lick)))));
                W(sYawn, Mathf.Max(100 * yawnAmt, .3f * talkOpen));   // a little of the yawn's wider jaw on the loudest syllables
                W(sTongue, Mathf.Max(100 * lick, 30 * yawnAmt));
                W(sChuff, 100 * chuffAmt);
                W(sLip, 100 * flehAmt);
                W(sSmile, asleep ? 30 : talkOpen > 5 ? 25 : 0);
                W(sBrowL, 0); W(sBrowR, 0);
            }
        }
    }
}
