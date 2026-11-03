using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StudyEverywhere
{
    // The lobby's welcome, at the person's chest in the middle of the round room as the person arrives: the greeting types
    // itself out (welcome to New World, made for you by Kore Meta, and what the libraries, cafés and rest areas are for),
    // then a short lesson in getting about, for hands or for controllers (the person chooses), one step at a time and
    // only when they press Next. With hands, a pair of grey, see-through hands (DemoHand) acts each step out: point and
    // pinch, touch, grab, teleport, turn, and the watch that opens the menu; with controllers, a pair of controller models
    // (DemoController) shows the trigger, grip, thumbstick and buttons in use. Done, Skip or the X puts it all away; it
    // comes back on the next arrival, or from Tutorial on the lobby's wrist menu (Reopen). It stays where it is when the
    // person moves about the room. The lessons describe exactly what the rig does (see ParisInteraction and the XRI rig).
    public sealed class LobbyWelcome : MonoBehaviour
    {
        [System.Serializable] public class Lesson { public string id, title, body; }

        [Tooltip("Typed out one after another.")] public TMP_Text[] typed;
        public TMP_Text lessonTitle, lessonBody, lessonCount;
        public Image[] lessonDots;
        public GameObject lessonGroup;
        public Button gotIt, next, back, handsChoice, controllersChoice, exit;
        public TMP_Text nextLabel;
        public DemoHand right, left;
        public DemoController rightController, leftController;
        [Tooltip("The demonstration hands' and controllers' stage, shown with the lessons.")] public GameObject demoStage;
        [Tooltip("A small gold box riding the left hand's wrist: the watch the Menu lesson taps.")] public Transform watch;
        public Lesson[] handLessons, controllerLessons;
        public float charactersPerSecond = 40, startDelay = 1.2f;
        public Color dotOn = Color.white, dotOff = new Color(1, 1, 1, .3f);
        public MenuTheme theme;
        // (revision 102929, the user, Oct 29, from the headset: lower, to the chest, not at the eye. The builder's 1.2 m was
        // a seated eye for someone sitting in their real room, since the lobby keeps the person's own height (standingEye
        // 0); so the height now comes from the head itself: for a moment after each arrival or Reopen, once tracking has
        // started and PlayerPlacement has placed the rig, the panel's centre goes belowEye under the eye (never under
        // lowest), tipped back to face an eye eyeDistance away as a lectern is, and the demo hands' stage follows it down.
        // Then it stays put, as before, when the person moves about.)
        [Tooltip("The head the welcome sits below; empty: the main camera.")] public Transform head;
        [Tooltip("Metres from the eye down to the panel's centre: chest height.")] public float belowEye = .33f;
        [Tooltip("Horizontal metres from the arrival's eye to the panel, for its tilt back toward the eye.")] public float eyeDistance = 1.3f;
        [Tooltip("The panel's centre never goes below this world height (m).")] public float lowest = .75f;
        const float FitSeconds = 1.5f;
        float fitClock;

        float clock, lessonClock; int lesson, typing; bool typed_, controllers, skipTyping;
        Lesson[] Lessons => controllers ? controllerLessons : handLessons;

        void OnEnable()
        {
            if (gotIt) gotIt.onClick.AddListener(Close);
            if (exit) exit.onClick.AddListener(Close);
            if (next) next.onClick.AddListener(() => { if (lesson >= Lessons.Length - 1) Close(); else Show(lesson + 1); });
            if (back) back.onClick.AddListener(() => Show(lesson - 1));
            if (handsChoice) handsChoice.onClick.AddListener(() => Choose(false));
            if (controllersChoice) controllersChoice.onClick.AddListener(() => Choose(true));
            Localization.Changed += Relabel;
            clock = 0; typing = 0; typed_ = false; fitClock = 0;   // (revision 102929: fit to the eye again on each arrival or Reopen)
            if (typed != null) foreach (var t in typed) if (t) t.maxVisibleCharacters = skipTyping ? 99999 : 0;
            if (lessonGroup) lessonGroup.SetActive(false);
            SetDemos();
            if (skipTyping) Typed();
            skipTyping = false;
        }
        void OnDisable()
        {
            foreach (var b in new[] { gotIt, next, back, handsChoice, controllersChoice, exit }) if (b) b.onClick.RemoveAllListeners();
            Localization.Changed -= Relabel;
        }
        void Relabel() { if (typed_) Show(lesson); }

        public void Close() { gameObject.SetActive(false); SetDemos(); }
        // Back from the lobby menu's Tutorial: the greeting already written out, the lessons from the first.
        public void Reopen()
        {
            if (gameObject.activeSelf) { Choose(controllers); return; }
            skipTyping = true; gameObject.SetActive(true);
        }
        // the hands for the hand lessons, the controllers for the controller lessons, nothing before the lessons start
        void SetDemos()
        {
            bool on = isActiveAndEnabled && typed_;
            void Set(Component c, bool v) { if (c && c.gameObject.activeSelf != v) c.gameObject.SetActive(v); }
            Set(right, on && !controllers); Set(left, on && !controllers);
            Set(rightController, on && controllers); Set(leftController, on && controllers);
            if (demoStage && demoStage.activeSelf != on) demoStage.SetActive(on);
        }

        public void Choose(bool useControllers)
        {
            controllers = useControllers;
            if (handsChoice) theme.Mark(handsChoice, !controllers, false);
            if (controllersChoice) theme.Mark(controllersChoice, controllers, false);
            SetDemos();
            Show(0);
        }

        public void Show(int i)
        {
            var list = Lessons; if (list == null || list.Length == 0) return;
            lesson = Mathf.Clamp(i, 0, list.Length - 1); lessonClock = 0;
            if (lessonTitle) lessonTitle.text = Localization.Tr(list[lesson].title);
            if (lessonBody) lessonBody.text = Localization.Tr(list[lesson].body);
            if (lessonCount) lessonCount.text = $"{lesson + 1} / {list.Length}";
            for (int k = 0; lessonDots != null && k < lessonDots.Length; k++)
                if (lessonDots[k]) { lessonDots[k].gameObject.SetActive(k < list.Length); lessonDots[k].color = k == lesson ? dotOn : dotOff; }
            if (back) back.interactable = lesson > 0;
            if (nextLabel) nextLabel.text = Localization.Tr(lesson >= list.Length - 1 ? "Done" : "Next");
        }

        // (revision 102929) Chest height under the real eye: run in LateUpdate for FitSeconds after tracking starts, so the
        // rig's first placement (PlayerPlacement, in its Update) and a recenter in those frames are both caught; only the
        // height and the tilt change, the panel keeps its place and its heading on the floor.
        void LateUpdate() { if (fitClock < FitSeconds) FitToEye(); }
        void FitToEye()
        {
            var h = head ? head : Camera.main ? Camera.main.transform : null;
            if (!h || h.localPosition == Vector3.zero) return;   // tracking has not started: no real eye height yet
            fitClock += Time.deltaTime;
            var level = Vector3.ProjectOnPlane(transform.forward, Vector3.up); if (level.sqrMagnitude < 1e-4f) return; level.Normalize();
            float y = Mathf.Max(lowest, h.position.y - belowEye), dy = y - transform.position.y;
            transform.SetPositionAndRotation(transform.position + Vector3.up * dy, Quaternion.LookRotation(level * eyeDistance + Vector3.down * Mathf.Max(0, h.position.y - y)));
            if (demoStage) demoStage.transform.position += Vector3.up * dy;   // the hands and controllers stay just below the lessons
        }

        void Update()
        {
            float dt = Time.deltaTime; clock += dt;
            if (!typed_)
            {
                // each line types in turn
                if (typed == null || typing >= typed.Length) { Typed(); return; }
                var t = typed[typing];
                if (!t) { typing++; return; }
                int n = Mathf.Max(0, Mathf.FloorToInt((clock - startDelay) * charactersPerSecond));
                t.maxVisibleCharacters = n;
                // the line's length, measured now if its mesh has not been built yet (it may not be in view); a line that
                // has not finished in a few seconds is shown whole, so the greeting can never be left blank
                if (t.textInfo == null || t.textInfo.characterCount == 0) t.ForceMeshUpdate();
                int count = t.textInfo != null ? t.textInfo.characterCount : 0;
                if (n >= count || clock - startDelay > count / charactersPerSecond + 3) { t.maxVisibleCharacters = 99999; typing++; clock = startDelay - .25f; }
                return;
            }
            lessonClock += dt;
            if (!controllers) Act(Lessons[lesson].id, lessonClock);
            else ActController(Lessons[lesson].id, lessonClock);
        }
        // The controllers act out the lesson: the right one for pointing, grabbing, teleporting and turning, the left
        // one for the menu (its menu button and Y).
        public void ActController(string which, float t)
        {
            if (rightController) rightController.Act(which == "menu" ? "" : which, t);
            if (leftController) leftController.Act(which, t);
        }
        void Typed() { if (typed != null) foreach (var t in typed) if (t) t.maxVisibleCharacters = 99999; typed_ = true; if (lessonGroup) lessonGroup.SetActive(true); Choose(controllers); }

        // The hands act out the lesson, each looping a two-second beat (they wait for Next; nothing moves on by itself).
        public void Act(string which, float t)
        {
            if (!right || !left) return;
            float beat = Mathf.Repeat(t, 2) / 2, pulse = Mathf.SmoothStep(0, 1, Mathf.PingPong(beat * 2, 1));   // 0 → 1 → 0 every two seconds
            left.thumbReach = 0; right.thumbReach = 0; bool palmUp = false;
            var relaxed = new[] { .3f, .3f, .35f, .4f, .45f };
            left.Pose(relaxed[0], relaxed[1], relaxed[2], relaxed[3], relaxed[4]);
            switch (which)
            {
                case "pinch":   // an open hand, the thumb meeting the index tip on the beat
                    right.Pose(0, .25f, .3f, .35f, .4f);
                    if (right.Tip(1)) { right.thumbTarget = right.Tip(1).position; right.thumbReach = pulse; }
                    break;
                case "touch":   // point, and press forward on the beat
                    right.Pose(.7f, 0, 1, 1, 1);
                    break;
                case "grab":    // open, close round something, open
                    right.Pose(pulse * .8f, pulse, pulse, pulse, pulse);
                    left.Pose(pulse * .8f, pulse, pulse, pulse, pulse);
                    break;
                case "teleport":   // palm up, thumb and index out, the rest curled; pinch to aim, then let go
                    right.Pose(0, 0, 1, 1, 1);
                    palmUp = true;
                    var mid = right.Joint(1, 1);
                    if (mid) { right.thumbTarget = mid.position; right.thumbReach = pulse; }
                    break;
                case "turn":    // the thumb taps the side of the index finger: by the knuckle (right), then by the tip (left)
                    right.Pose(.2f, .55f, .7f, .8f, .85f);
                    var a = right.Joint(1, Mathf.Repeat(t, 4) < 2 ? 0 : 2);
                    if (a) { right.thumbTarget = a.position; right.thumbReach = pulse; }
                    break;
                default:        // the menu: the right index taps the watch on the left wrist
                    left.Pose(.2f, .2f, .25f, .3f, .35f);
                    right.Pose(.7f, 0, 1, 1, 1);
                    break;
            }
            // the press (touch) and the tap (menu) move the right hand itself; the rest of the time it sits at home
            if (!homeSet) { rightHome = right.transform.localPosition; homeSet = true; }
            var push = Vector3.zero;
            if (which == "touch") push = right.transform.parent.InverseTransformDirection(transform.forward) * .06f * pulse;
            if (which == "menu" && watch && right.Tip(1))
            {
                // where the fingertip would be with the hand at home, and the move that brings it onto the watch
                var tipAtHome = right.Tip(1).position - right.transform.parent.TransformVector(lastPush);
                push = right.transform.parent.InverseTransformVector(watch.position - tipAtHome) * pulse;
            }
            right.transform.localPosition = rightHome + push; lastPush = push;
            // the teleport gesture is made with the palm turned up: roll the hand over for it
            if (!turnSet) { rightTurn = right.transform.localRotation; turnSet = true; }
            right.transform.localRotation = rightTurn;
            if (palmUp && right.Wrist && right.Joint(2, 0)) right.transform.RotateAround(right.Wrist.position, (right.Joint(2, 0).position - right.Wrist.position).normalized, 180);   // rolled over about the wrist
            if (watch) watch.gameObject.SetActive(which == "menu");
        }
        Vector3 rightHome, lastPush, start; bool homeSet, startKnown; Quaternion rightTurn; bool turnSet;
    }
}
