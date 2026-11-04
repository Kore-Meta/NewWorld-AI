using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace StudyEverywhere
{
    // Read XRI's ownership of each hand instead of recognizing an independent gesture over UI/grabs.
    public sealed class HandInteractionGuard
    {
        GameObject hand;
        NearFarInteractor nearFar;
        XRBaseInteractor[] interactors;
        Handedness handedness;

        public void Bind(XRInputModalityManager modality, Handedness side)
        {
            handedness = side;
            hand = modality ? (side == Handedness.Left ? modality.leftHand : modality.rightHand) : null;
            nearFar = hand ? hand.GetComponentInChildren<NearFarInteractor>(true) : null;
            interactors = hand ? hand.GetComponentsInChildren<XRBaseInteractor>(true) : null;
        }

        public bool Busy
        {
            get
            {
                // Disabled Near-Far means teleport, poke, system gesture, or another modality owns input.
                // Missing wiring fails closed, avoiding invisible gestures on a controller-only rig.
                if (!hand || !hand.activeInHierarchy || !nearFar || !nearFar.isActiveAndEnabled) return true;
                var aim = handedness == Handedness.Left ? MetaAimHand.left : MetaAimHand.right;
                if (aim != null && (aim.aimFlags.ReadValue() & (int)(MetaAimFlags.SystemGesture | MetaAimFlags.MenuPressed)) != 0) return true;
                if (nearFar.TryGetUIModel(out var ui) && ui.currentRaycast.isValid) return true;
                foreach (var interactor in interactors)
                    if (interactor && interactor.isActiveAndEnabled && (interactor.hasSelection || interactor.hasHover)) return true;
                return false;
            }
        }

        // Narrower than Busy, for buttons you can see: only a hand holding something, teleporting, making the system
        // gesture or on another modality. A ray resting on a menu or an object does not count.
        public bool Holding
        {
            get
            {
                if (!hand || !hand.activeInHierarchy || !nearFar || !nearFar.isActiveAndEnabled) return true;
                var aim = handedness == Handedness.Left ? MetaAimHand.left : MetaAimHand.right;
                if (aim != null && (aim.aimFlags.ReadValue() & (int)(MetaAimFlags.SystemGesture | MetaAimFlags.MenuPressed)) != 0) return true;
                foreach (var interactor in interactors)
                    if (interactor && interactor.isActiveAndEnabled && interactor.hasSelection) return true;
                return false;
            }
        }

        // The hand's pointing ray (Near-Far far casting: the line and its dot) put away while the thumb is on the finger
        // turn button, and brought back after. Only undoes what it did itself.
        bool pointerAway;
        public void PutPointerAway(bool away)
        {
            if (!nearFar || away == pointerAway) return;
            if (away && (!nearFar.enableFarCasting || nearFar.hasSelection)) return;
            nearFar.enableFarCasting = !away; pointerAway = away;
        }
    }
}
