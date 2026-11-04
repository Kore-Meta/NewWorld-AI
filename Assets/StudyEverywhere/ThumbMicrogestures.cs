using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

namespace StudyEverywhere
{
    // Legacy joint-based thumb gestures. This is NOT Meta's native microgesture recognition API.
    // Input is gated by XRI ownership, stable release, tap dwell, and a shared cooldown.
    // rest the thumb tip on the side of the index finger, then
    //   swipe it forward (toward the fingertip)  glide a metre ahead
    //   swipe it back                            step back
    //   swipe it left or right                   turn 30 degrees that way
    //   tap (touch and lift without moving)      teleport to where you are looking (a ring shows the spot while the
    //                                            thumb rests on the finger)
    // Glides stop short of walls and furniture; a teleport only lands on floor that teleporting allows.
    // The places are seated, so by default only turning is on, by a round arrow button on the thumb side of each index
    // finger's middle joint (FingerTurnButtons): the right hand's turns right, the left hand's left. It replaced the
    // invisible taps on the side of the finger (tapTurn: near the knuckle right, near the tip left), which were hard to hit
    // and still run with fingerButtons off. Swipes were hard to make; swipeTurn, swipeGlide and tapTeleport switch them on. With hands,
    // teleporting is the hand-teleport gesture alone (thumb and index out, pinch to aim, release to go).
    public sealed class ThumbMicrogestures : MonoBehaviour
    {
        public XROrigin origin;
        [Tooltip("A ring on the floor showing where a tap would teleport to.")] public Transform target;
        public float glide = 1f, back = .6f, turnDegrees = 30, glideSeconds = .35f, maxTeleport = 7;
        // Off by default: the hand teleport (thumb and index out, pinch, release) is the one way to teleport with hands; two
        // different teleports on one hand confused people. With this on, a thumb tap also teleports where you look.
        [Tooltip("A thumb tap teleports where you look (off: use the hand teleport instead).")]
        public bool tapTeleport;
        [Tooltip("Off by default (the places are seated): with this on, a thumb swipe along the finger glides forward or back.")]
        public bool swipeGlide;
        [Tooltip("A thumb tap on the side of the index finger turns: near the knuckle to the right, near the tip to the left.")] public bool tapTurn = true;
        [Tooltip("Off by default (taps turn now): a sideways thumb swipe across the finger turns.")] public bool swipeTurn;
        [Tooltip("Thumb tip within this of the index finger's side counts as touching (metres).")] public float touch = .018f, release = .028f;
        [Tooltip("Thumb travel along or across the finger that makes a swipe (metres).")] public float swipe = .02f;
        [Tooltip("A tap is a touch shorter than this (seconds) without a swipe.")] public float tapSeconds = .45f;
        [Min(.04f)] public float minimumTapSeconds = .08f;
        [Min(.05f)] public float releaseDwell = .12f;
        [Min(.1f)] public float actionCooldown = .45f;
        [Range(0, .25f), Tooltip("Ignore ambiguous taps around the middle of the index finger.")]
        public float directionDeadZone = .08f;

        [Tooltip("A round arrow button on the thumb side of each index finger's middle joint (right hand turns right, left hand left): press it with the thumb. On, it replaces the invisible taps.")]
        public bool fingerButtons = true;
        // NewWorld AI (Nov 3 review, item 41: snap turn was on both hands; the user: the right hand turns, the left moves).
        // Off: the left controller's thumbstick moves and never turns, and the left hand's old thumb taps are off. The finger
        // buttons are one a hand since Nov 4 (the left one turns left), so both hands keep theirs: without it, no left turn.
        [Tooltip("The left controller's thumbstick turns too, and the left hand's old thumb taps. Off: the left stick only moves. (Both hands' finger buttons work either way.)")]
        public bool leftHandTurn;
        [Tooltip("Button size across, and how far out from the middle of the finger it sits (metres).")] public float buttonSize = .015f, buttonOut = .0105f;
        [Tooltip("The thumb pad within this of a button's centre presses it; it must then move this far off before the next press (metres).")]
        public float pressRadius = .014f, pressRelease = .026f;
        [Min(0), Tooltip("How long the thumb rests on a button before it turns (seconds): enough to skip a brush past.")] public float pressDwell = .05f;
        [Range(20, 80), Tooltip("The buttons show, and work, only while the hand is within this many degrees of where you look.")] public float buttonView = 50;
        [Tooltip("Thumb tip this close to the index tip is a pinch, which disarms the buttons (metres).")] public float pinchGap = .02f;
        [Range(0, 1), Tooltip("A palm turned to the face past this (cosine) hides the button: that pose brings up Meta's own menu button.")]
        public float palmToFace = .55f;

        // A hand busy with something real (typing on a real keyboard, holding a cup) makes the same thumb-on-finger touches
        // as a tap: while this says so for a hand, that hand's gestures do nothing.
        public static System.Func<Handedness, bool> Paused;

        sealed class State { public readonly HandTapGate gate = new HandTapGate(); public Vector3 start; public FingerTurnButtons buttons; }
        readonly State left = new State(), right = new State();
        readonly HandInteractionGuard leftGuard = new HandInteractionGuard(), rightGuard = new HandInteractionGuard();
        static readonly List<XRHandSubsystem> subsystems = new List<XRHandSubsystem>();
        Vector3 glideFrom, glideTo; float glideT = 1;
        float nextAction;

        void OnEnable()
        {
            if (!origin) origin = GetComponentInParent<XROrigin>();
            var modality = origin ? origin.GetComponentInChildren<XRInputModalityManager>(true) : null;
            leftGuard.Bind(modality, Handedness.Left);
            rightGuard.Bind(modality, Handedness.Right);
            if (!leftHandTurn && origin)
            {
                // the controllers: the left thumbstick moves, it no longer turns too
                foreach (var t in origin.GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning.SnapTurnProvider>(true))
                    t.leftHandTurnInput.inputSourceMode = UnityEngine.XR.Interaction.Toolkit.Inputs.Readers.XRInputValueReader.InputSourceMode.Unused;
                foreach (var t in origin.GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning.ContinuousTurnProvider>(true))
                    t.leftHandTurnInput.inputSourceMode = UnityEngine.XR.Interaction.Toolkit.Inputs.Readers.XRInputValueReader.InputSourceMode.Unused;
            }
            ResetInput();
        }

        void OnDisable() => ResetInput();
        void OnDestroy() { left.buttons?.Destroy(); right.buttons?.Destroy(); }
        void OnApplicationFocus(bool focused) { if (!focused) ResetInput(); }
        void OnApplicationPause(bool paused) { if (paused) ResetInput(); }
        void ResetInput()
        {
            left.gate.Reset(); right.gate.Reset(); glideT = 1;
            left.buttons?.Hide(); right.buttons?.Hide();
            leftGuard.PutPointerAway(false); rightGuard.PutPointerAway(false);
            if (target) target.gameObject.SetActive(false);
        }

        void Update()
        {
            if (!origin || !origin.Camera || !Application.isFocused) { ResetInput(); return; }
            if (glideT < 1)
            {
                glideT = Mathf.MoveTowards(glideT, 1, Time.deltaTime / Mathf.Max(.1f, glideSeconds));
                float e = glideT * glideT * (3 - 2 * glideT);
                var rig = origin.transform; rig.position = Vector3.Lerp(glideFrom, glideTo, e);
            }
            SubsystemManager.GetSubsystems(subsystems);
            XRHandSubsystem hands = null; foreach (var s in subsystems) if (s.running) { hands = s; break; }
            bool aiming = false;
            if (hands != null)
            {
                if (fingerButtons)
                {
                    // one button a hand since Nov 4 (the left one turns left), so both stay on whatever leftHandTurn says
                    Buttons(hands.leftHand, Handedness.Left, left, leftGuard);
                    Buttons(hands.rightHand, Handedness.Right, right, rightGuard);
                }
                else { left.buttons?.Hide(); right.buttons?.Hide(); leftGuard.PutPointerAway(false); rightGuard.PutPointerAway(false); }
                // the older joint gestures, only when one of them is on
                if (tapTeleport || swipeGlide || swipeTurn || (tapTurn && !fingerButtons))
                { if (leftHandTurn) aiming |= Read(hands.leftHand, left, leftGuard); aiming |= Read(hands.rightHand, right, rightGuard); }
            }
            else ResetInput();
            if (target)
            {
                var spot = Vector3.zero;
                bool show = aiming && Gaze(out spot);
                target.gameObject.SetActive(show);
                if (show) target.position = spot + Vector3.up * .01f;
            }
        }

        // One hand: returns true while the thumb rests on the index finger (aiming a teleport).
        bool Read(XRHand hand, State s, HandInteractionGuard guard)
        {
            if (Time.unscaledTime < nextAction || guard.Busy || (Paused != null && Paused(hand.handedness))
                || !hand.isTracked || !hand.GetJoint(XRHandJointID.ThumbTip).TryGetPose(out var thumb) || !hand.GetJoint(XRHandJointID.IndexProximal).TryGetPose(out var knuckle)
                || !hand.GetJoint(XRHandJointID.IndexIntermediate).TryGetPose(out var middle) || !hand.GetJoint(XRHandJointID.IndexDistal).TryGetPose(out var distal)
                || !hand.GetJoint(XRHandJointID.IndexTip).TryGetPose(out var tip))
            { s.gate.Reset(); return false; }
            // a relaxed hand: the index finger curled (its tip near the palm side), so a pinch is not mistaken for this
            float curl = Vector3.Distance(tip.position, knuckle.position);
            float fingerLength = Vector3.Distance(knuckle.position, middle.position) + Vector3.Distance(middle.position, distal.position) + Vector3.Distance(distal.position, tip.position);
            var axis = (distal.position - knuckle.position); float len = axis.magnitude; if (len < 1e-3f) { s.gate.Reset(); return false; } axis /= len;
            // distance from the thumb tip to the index's middle segment
            var rel = thumb.position - knuckle.position; float along = Mathf.Clamp(Vector3.Dot(rel, axis), 0, len);
            float gap = Vector3.Distance(thumb.position, knuckle.position + axis * along);
            // Normalize to this hand: the old fixed 9 cm test accepted straight fingers on smaller hands.
            bool relaxed = fingerLength > .025f && curl / fingerLength < .85f;
            // Opening the entire hand cancels. Only separating the thumb completes a valid tap.
            if (!relaxed) { s.gate.Reset(); return false; }
            float at = along / len;
            int direction = tapTeleport ? 1 : at < .5f - directionDeadZone ? 1 : at > .5f + directionDeadZone ? -1 : 0;
            bool wasTouching = s.gate.Touching;
            int tapped = s.gate.Sample(true, gap < touch, gap > Mathf.Max(touch + .005f, release), direction,
                Time.unscaledTime, minimumTapSeconds, Mathf.Max(minimumTapSeconds, tapSeconds), releaseDwell);
            // Track thumb travel relative to the finger: moving the whole hand is not a swipe.
            var localThumb = Quaternion.Inverse(knuckle.rotation) * rel;
            if (!wasTouching && s.gate.Touching) s.start = localThumb;
            if (tapped != 0)
            {
                if (tapTeleport && Gaze(out var spot)) { AcceptAction(); Teleport(spot); }
                else if (!tapTeleport && tapTurn && !fingerButtons) { AcceptAction(); Turn(tapped); }
                return false;
            }
            if (s.gate.Touching && !s.gate.Consumed && Time.unscaledTime - s.gate.StartedAt >= minimumTapSeconds)
            {
                var move = knuckle.rotation * (localThumb - s.start);
                float forward = Vector3.Dot(move, axis), side = Vector3.Dot(move, Vector3.Cross(axis, middle.rotation * Vector3.up).normalized);
                if (Mathf.Abs(forward) > swipe && Mathf.Abs(forward) > Mathf.Abs(side)) { if (swipeGlide) { s.gate.Consume(); AcceptAction(); Glide(forward > 0 ? glide : -back); } }
                else if (swipeTurn && Mathf.Abs(side) > swipe) { s.gate.Consume(); AcceptAction(); Turn(hand.handedness == Handedness.Left ? -Mathf.Sign(side) : Mathf.Sign(side)); }
            }
            return tapTeleport && s.gate.Touching && !s.gate.Consumed;
        }

        // The arrow button on one index finger: shown while the hand is tracked, in front of the eyes, palm not turned to
        // the face (Meta's menu button pose) and not holding anything; a press turns. A ray resting on a menu does not hide
        // it (pressing the side of the finger is no pinch), but while the thumb is on it the ray is put away.
        void Buttons(XRHand hand, Handedness side, State s, HandInteractionGuard guard)
        {
            var space = origin.CameraFloorOffsetObject ? origin.CameraFloorOffsetObject.transform : origin.transform;
            if (s.buttons == null) s.buttons = new FingerTurnButtons(this, side, space);
            if (!hand.isTracked || (Paused != null && Paused(side))
                || !hand.GetJoint(XRHandJointID.IndexProximal).TryGetPose(out var knuckle) || !hand.GetJoint(XRHandJointID.IndexIntermediate).TryGetPose(out var middle)
                || !hand.GetJoint(XRHandJointID.IndexDistal).TryGetPose(out var distal) || !hand.GetJoint(XRHandJointID.IndexTip).TryGetPose(out var tip)
                || !hand.GetJoint(XRHandJointID.ThumbProximal).TryGetPose(out var thumbBase) || !hand.GetJoint(XRHandJointID.ThumbDistal).TryGetPose(out var thumbDistal)
                || !hand.GetJoint(XRHandJointID.ThumbTip).TryGetPose(out var thumb) || !hand.GetJoint(XRHandJointID.Palm).TryGetPose(out var palm))
            { s.buttons.Hide(); guard.PutPointerAway(false); return; }
            var head = origin.Camera.transform;
            var eye = space.InverseTransformPoint(head.position);
            var toHand = knuckle.position - eye;
            bool inView = toHand.magnitude < .8f && Vector3.Angle(space.InverseTransformDirection(head.forward), toHand) < buttonView;
            // joint +Y is the back of the hand, so the palm faces -Y
            bool palmToEyes = Vector3.Dot(palm.rotation * Vector3.down, (eye - palm.position).normalized) > palmToFace;
            int turn = s.buttons.Step(inView && !palmToEyes && !guard.Holding, Time.unscaledTime >= nextAction, knuckle, middle, distal, tip,
                thumbBase.position, thumbDistal.position, thumb.position, eye, space.InverseTransformDirection(head.up), Time.unscaledTime);
            guard.PutPointerAway(s.buttons.ThumbClose);
            if (turn != 0) { AcceptAction(); Turn(turn); }
        }

        void AcceptAction()
        {
            nextAction = Time.unscaledTime + Mathf.Max(.1f, actionCooldown);
            left.gate.Reset(); right.gate.Reset();
        }

        void Glide(float metres)
        {
            var head = origin.Camera.transform; var dir = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized * Mathf.Sign(metres);
            float dist = Mathf.Abs(metres);
            var from = head.position; from.y = Mathf.Max(origin.transform.position.y + .5f, from.y - .6f);   // chest height, so desks stop it too
            if (Physics.SphereCast(from, .2f, dir, out var hit, dist + .25f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) dist = Mathf.Max(0, hit.distance - .25f);
            if (dist < .05f) return;
            glideFrom = origin.transform.position; glideTo = glideFrom + dir * dist; glideT = 0;
        }

        void Turn(float sign) { var head = origin.Camera.transform; origin.transform.RotateAround(head.position, Vector3.up, turnDegrees * sign); }

        // Where the person is looking, on floor that allows teleporting.
        bool Gaze(out Vector3 spot)
        {
            spot = default;
            var head = origin.Camera.transform;
            if (!Physics.Raycast(head.position, head.forward, out var hit, maxTeleport, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) return false;
            if (!hit.collider.GetComponentInParent<TeleportationArea>() || hit.normal.y < .7f) return false;
            spot = hit.point; return true;
        }

        void Teleport(Vector3 spot)
        {
            var head = origin.Camera.transform; var rig = origin.transform;
            var offset = head.position - rig.position; offset.y = 0;
            rig.position = new Vector3(spot.x, spot.y, spot.z) - offset;
            glideT = 1;
        }
    }
}
