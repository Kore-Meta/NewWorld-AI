using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace StudyEverywhere
{
    // Hand gestures for the hologram, in the Minority Report manner. Standing near the deck with it on: pinch the air
    // and drag to turn the craft (sideways turns it, up and down tips it); pinch with both hands and pull them apart or
    // together to grow or shrink it, or move both to carry it; sweep an open hand across to go to the next or previous
    // craft. A pinch that lands on something (a part, a button, a panel) is an ordinary XR Interaction Toolkit select
    // and is left alone, so the two never fight. Controllers do the same through the parts' grab handles and the rim
    // panel; these gestures are for tracked hands only.
    public sealed class HoloGesture : MonoBehaviour
    {
        public HoloDeck deck;
        public XROrigin origin;
        [Tooltip("The hands' Near-Far interactors (left, right): a hand that hovers or holds something is not gesturing.")]
        public NearFarInteractor leftInteractor, rightInteractor;
        [Tooltip("Gestures work within this distance of the deck's stage (metres).")] public float reach = 1.6f;
        [Tooltip("Thumb to index tip: a pinch closes below this and opens above the second.")] public float pinchClose = .022f, pinchOpen = .04f;
        public float turnDegreesPerMetre = 220, tipDegreesPerMetre = 140;
        [Tooltip("An open hand moving sideways faster than this (m/s) over at least swipeTravel metres is a swipe.")] public float swipeSpeed = 1.1f, swipeTravel = .22f;

        sealed class Hand
        {
            public bool tracked, pinching, open, gesturing, ready, busy;
            public Vector3 pinch, palm, lastPinch, swipeStart;
            public float swipeSince = -1;
            public readonly Queue<(float t, Vector3 p)> trail = new Queue<(float, Vector3)>();
        }
        readonly Hand left = new Hand(), right = new Hand();
        static readonly List<XRHandSubsystem> subsystems = new List<XRHandSubsystem>();
        float lastSpread, swipeCooldown;
        bool twoHanded;
        void OnDisable() => Release();
        void OnApplicationFocus(bool focused) { if (!focused) Release(); }

        void Update()
        {
            if (!Application.isFocused || !deck || !deck.On || !deck.stage) { Release(); return; }
            SubsystemManager.GetSubsystems(subsystems);
            XRHandSubsystem running = null;
            foreach (var s in subsystems) if (s.running) { running = s; break; }
            if (running == null) { Release(); return; }
            var space = origin && origin.CameraFloorOffsetObject ? origin.CameraFloorOffsetObject.transform : transform.root;
            Read(running.leftHand, left, leftInteractor, space);
            Read(running.rightHand, right, rightInteractor, space);
            var head = Camera.main;
            var right_ = head ? head.transform.right : Vector3.right;

            bool l = left.gesturing, r = right.gesturing;
            if (l && r)
            {
                float spread = Vector3.Distance(left.pinch, right.pinch);
                Vector3 mid = (left.pinch + right.pinch) / 2, lastMid = (left.lastPinch + right.lastPinch) / 2;
                if (twoHanded && lastSpread > .02f) { deck.ScaleBy(spread / lastSpread); deck.Nudge(mid - lastMid); }
                lastSpread = spread; twoHanded = true;
            }
            else
            {
                twoHanded = false;
                var h = l ? left : r ? right : null;
                if (h != null)
                {
                    var d = h.pinch - h.lastPinch;
                    float side = Vector3.Dot(d, right_), up = d.y;
                    deck.Turn(-side * turnDegreesPerMetre, up * tipDegreesPerMetre, right_);
                }
            }
            deck.SpinPaused = l || r;
            left.lastPinch = left.pinch; right.lastPinch = right.pinch;

            // swipes: an open hand, not gesturing, crossing in front at speed
            swipeCooldown -= Time.deltaTime;
            if (swipeCooldown <= 0) { if (!Swipe(left, right_)) Swipe(right, right_); }
        }

        void Read(XRHand hand, Hand h, NearFarInteractor interactor, Transform space)
        {
            if (!hand.isTracked || !hand.GetJoint(XRHandJointID.IndexTip).TryGetPose(out var index) || !hand.GetJoint(XRHandJointID.ThumbTip).TryGetPose(out var thumb)
                || !hand.GetJoint(XRHandJointID.Palm).TryGetPose(out var palm) || !hand.GetJoint(XRHandJointID.MiddleTip).TryGetPose(out var middle) || !hand.GetJoint(XRHandJointID.Wrist).TryGetPose(out var wrist))
            { ResetHand(h); return; }
            h.tracked = true;
            h.pinch = space.TransformPoint((index.position + thumb.position) / 2);
            h.palm = space.TransformPoint(palm.position);
            float gap = Vector3.Distance(index.position, thumb.position);
            bool wasPinching = h.pinching;
            h.pinching = wasPinching ? gap < pinchOpen : gap < pinchClose;
            float palmLength = Mathf.Max(Vector3.Distance(wrist.position, palm.position), .03f);
            h.open = !h.pinching && Vector3.Distance(middle.position, palm.position) / palmLength > 1.6f;
            var aim = hand.handedness == Handedness.Left ? MetaAimHand.left : MetaAimHand.right;
            bool busy = !interactor || !interactor.isActiveAndEnabled || interactor.hasSelection || interactor.hasHover || OverUI(interactor)
                || (aim != null && (aim.aimFlags.ReadValue() & (int)(MetaAimFlags.SystemGesture | MetaAimFlags.MenuPressed)) != 0)
                || (ThumbMicrogestures.Paused != null && ThumbMicrogestures.Paused(hand.handedness));
            h.busy = busy;
            if (busy) { h.ready = h.gesturing = false; h.trail.Clear(); return; }
            if (!h.pinching) h.ready = true;
            bool near = Vector3.Distance(h.pinch, deck.stage.position) < reach;
            // a gesture starts on a fresh pinch in clear air near the deck, and lasts until the pinch opens
            if (!h.gesturing) { if (h.ready && h.pinching && !wasPinching && near) { h.gesturing = true; h.ready = false; h.lastPinch = h.pinch; } }
            else if (!h.pinching) h.gesturing = false;
        }

        static bool OverUI(NearFarInteractor interactor) => interactor.TryGetUIModel(out var model) && model.currentRaycast.isValid;

        bool Swipe(Hand h, Vector3 right_)
        {
            if (!h.tracked || h.busy || !h.open || h.gesturing || Vector3.Distance(h.palm, deck.stage.position) > reach) { h.trail.Clear(); return false; }
            h.trail.Enqueue((Time.time, h.palm));
            while (h.trail.Count > 0 && Time.time - h.trail.Peek().t > .3f) h.trail.Dequeue();
            if (h.trail.Count < 3) return false;
            var first = h.trail.Peek(); float dt = Time.time - first.t; if (dt < .08f) return false;
            float travel = Vector3.Dot(h.palm - first.p, right_);
            if (Mathf.Abs(travel) < swipeTravel || Mathf.Abs(travel) / dt < swipeSpeed) return false;
            deck.Step(travel > 0 ? 1 : -1);
            h.trail.Clear(); swipeCooldown = 1f;
            return true;
        }

        static void ResetHand(Hand h) { h.tracked = h.pinching = h.open = h.gesturing = h.ready = h.busy = false; h.trail.Clear(); }
        void Release() { ResetHand(left); ResetHand(right); twoHanded = false; if (deck) deck.SpinPaused = false; }
    }
}
