using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

namespace StudyEverywhere
{
    // Direct hand grab for tracked hands. The XRI hand rig selects only on a pinch, so closing the hand around a cup
    // did nothing and only the ray (pinch at a distance) could take things. This sits in front of the hand's
    // Near-Far interactor select input: a pinch still selects as before, and a grasp (middle and ring fingers curled
    // into the palm) selects too when a grabbable is within reach of the palm, so the near caster picks it up.
    // The grasp holds until the hand opens again.
    public sealed class HandGrabAssist : MonoBehaviour, IXRInputButtonReader
    {
        public NearFarInteractor interactor;
        public Handedness handedness = Handedness.Left;
        public XROrigin origin;
        [Tooltip("Curl ratio (fingertip to palm over palm length) below which the hand counts as closed.")]
        public float closeRatio = 0.75f;
        [Tooltip("Curl ratio above which a held grasp lets go.")]
        public float openRatio = 0.95f;
        [Tooltip("Curl ratio a hand must be seen above, after a grasp, before it can grasp again.")]
        public float readyRatio = 0.85f;
        [Tooltip("Reach around the palm in which a grabbable can be grasped (metres).")]
        public float reach = 0.09f;
        [Tooltip("Editor testing: acts as a closed hand.")]
        public bool forceGrasp;

        XRInputButtonReader pinch;
        readonly XRInputButtonReader wrapper = new XRInputButtonReader("Select (pinch or grasp)", null, false, XRInputButtonReader.InputSourceMode.ObjectReference);
        static readonly List<XRHandSubsystem> hands = new List<XRHandSubsystem>();
        readonly List<IXRInteractable> targets = new List<IXRInteractable>();
        readonly List<Collider> nearHits = new List<Collider>();
        int frame = -1;
        bool held, selected, wasSelected, graspReady, pinchReady;
        float selectedValue;

        public bool Grasping => held;
        public float Curl { get; private set; } = 1;

        void OnEnable()
        {
            if (!interactor) interactor = GetComponent<NearFarInteractor>();
            if (!interactor || interactor.selectInput == wrapper) return;
            pinch = interactor.selectInput;
            wrapper.SetObjectReference(this);
            interactor.selectInput = wrapper;
            frame = -1;
            held = selected = wasSelected = graspReady = pinchReady = false;
        }

        void OnDisable()
        {
            if (interactor && interactor.selectInput == wrapper && pinch != null) interactor.selectInput = pinch;
            held = selected = wasSelected = graspReady = pinchReady = false;
            frame = -1;
        }

        void Refresh()
        {
            if (frame == Time.frameCount) return;
            frame = Time.frameCount;
            wasSelected = selected;
            bool closed = forceGrasp, open = !forceGrasp;
            bool tracked = forceGrasp;
            Vector3 palm = transform.position;
            if (!forceGrasp && TryReadHand(out palm, out float curl))
            {
                tracked = true;
                Curl = curl;
                closed = curl < closeRatio;
                open = curl > openRatio;
            }
            else if (!forceGrasp) { Curl = 1; graspReady = false; }
            // ready to grasp once the hand has been seen open enough (revision 102929: a relaxed hand reaching for Haven's
            // hourglass seldom reads the full openRatio, so the grasp never armed and the hourglass could not be turned)
            if (tracked && Curl >= readyRatio) graspReady = true;
            if (held) held = !open;
            else if ((graspReady || forceGrasp) && closed && GrabbableNear(forceGrasp ? (interactor.nearInteractionCaster?.castOrigin ? interactor.nearInteractionCaster.castOrigin.position : transform.position) : palm))
            { held = true; graspReady = false; }
            bool rawPinch = pinch != null && pinch.ReadIsPerformed();
            if (!tracked) pinchReady = false;
            else if (!rawPinch) pinchReady = true;
            bool pinching = tracked && pinchReady && rawPinch;
            selected = held || pinching;
            selectedValue = held ? 1 : tracked && pinchReady && pinch != null ? pinch.ReadValue() : 0;
        }

        bool TryReadHand(out Vector3 palmWorld, out float curl)
        {
            palmWorld = default; curl = 1;
            SubsystemManager.GetSubsystems(hands);
            XRHandSubsystem running = null;
            foreach (var s in hands) if (s.running) { running = s; break; }
            if (running == null) return false;
            var hand = handedness == Handedness.Left ? running.leftHand : running.rightHand;
            if (!hand.isTracked) return false;
            if (!hand.GetJoint(XRHandJointID.Palm).TryGetPose(out var palm) || !hand.GetJoint(XRHandJointID.Wrist).TryGetPose(out var wrist)
                || !hand.GetJoint(XRHandJointID.MiddleProximal).TryGetPose(out var knuckle)
                || !hand.GetJoint(XRHandJointID.MiddleTip).TryGetPose(out var middle) || !hand.GetJoint(XRHandJointID.RingTip).TryGetPose(out var ring))
                return false;
            float length = Mathf.Max(Vector3.Distance(wrist.position, knuckle.position), 0.03f);
            curl = (Vector3.Distance(middle.position, palm.position) + Vector3.Distance(ring.position, palm.position)) * 0.5f / length;
            // Joint poses are in tracking space, which the XR Origin's camera offset carries into the world.
            var space = origin && origin.CameraFloorOffsetObject ? origin.CameraFloorOffsetObject.transform : transform.root;
            palmWorld = space.TransformPoint(palm.position);
            return true;
        }

        bool GrabbableNear(Vector3 at)
        {
            if (!interactor || !interactor.isActiveAndEnabled || !interactor.enableNearCasting) return false;
            // Allow a pinch-held near object to transfer to a grasp without dropping/reselecting it.
            if (interactor.hasSelection)
            {
                if (interactor.selectionRegion.Value != NearFarInteractor.Region.Near
                    || !(interactor.interactablesSelected[0] is XRGrabInteractable selectedGrab)) return false;
                foreach (var collider in selectedGrab.colliders)
                    if (collider && (collider.ClosestPoint(at) - at).sqrMagnitude <= reach * reach) return true;
                return false;
            }
            if (interactor.TryGetUIModel(out var ui) && ui.currentRaycast.isValid) return false;
            var manager = interactor.interactionManager;
            if (!manager || interactor.nearInteractionCaster == null) return false;
            // Only grasp the target XRI would actually select, and only if its near caster sees it.
            // A cup beside the palm must never authorize selecting a remote button or object.
            interactor.GetValidTargets(targets);
            if (targets.Count == 0 || !(targets[0] is XRGrabInteractable grab) || !interactor.CanSelect((IXRSelectInteractable)grab)) return false;
            nearHits.Clear();
            if (!interactor.nearInteractionCaster.TryGetColliderTargets(manager, nearHits)) return false;
            foreach (var hit in nearHits)
            {
                if (hit && manager.TryGetInteractableForCollider(hit, out var found) && ReferenceEquals(found, grab)
                    && (hit.ClosestPoint(at) - at).sqrMagnitude <= reach * reach) return true;
            }
            return false;
        }

        public bool ReadIsPerformed() { Refresh(); return selected; }
        public bool ReadWasPerformedThisFrame() { Refresh(); return selected && !wasSelected; }
        public bool ReadWasCompletedThisFrame() { Refresh(); return !selected && wasSelected; }
        public float ReadValue() { Refresh(); return selectedValue; }
        public bool TryReadValue(out float value) { value = ReadValue(); return held || (pinch != null && pinch.TryReadValue(out _)); }
    }
}
