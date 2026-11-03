using UnityEngine.InputSystem.XR;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit.Interactors;


#if XR_HANDS_1_5_OR_NEWER
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.Gestures;
#endif

namespace UnityEngine.XR.Interaction.Toolkit.XRHandLocomotion
{
    /// <summary>
    /// Evaluates the hand joints/bones in order to control the state of the XRRayInteractor and perform further joystick pinch input that controls the teleport reticle
    /// </summary>
    [DefaultExecutionOrder(XRInteractionUpdateOrder.k_InteractionManager - 1)]
    public class TeleportHandGesture : MonoBehaviour
    {
#if XR_HANDS_1_5_OR_NEWER
        /// <summary>
        /// The XRHandshape used to activate the display of the teleport visuals
        /// </summary>
        [Header("Activate Teleport Mode")]
        [SerializeField]
        XRHandShape m_ActivateHandShape;

        /// <summary>
        /// Detect an activate gesture based on any user defined inspector conditions applied to the ActivateHandShape
        /// </summary>
        [SerializeField]
        XRHandRelativeOrientation m_ActivateHandOrientation;

        /// <summary>
        /// The XRHandshape used to deactivate the display of the teleport visuals and teleport mode
        /// </summary>
        [Header("Cancel Teleport Mode")]
        [SerializeField]
        XRHandShape m_CancelHandShape;
#endif

        /// <summary>
        /// The controller that handles the joystick input mode enabled when pinching while the teleport visuals are activated
        /// This controller handles the rotation targeting of the teleport ray reticle
        /// </summary>
        [SerializeField]
        JoystickPinchHandGesture m_JoystickPinchHandGesture;

        /// <summary>
        /// The teleport XRRayInteractor that will be activated and deactivated by the hand gesture. This interactor drives the teleport ray visuals.
        /// </summary>
        [Header("References")]
        [SerializeField]
        [Tooltip("The teleport XRRayInteractor that will be activated and deactivated by the hand gesture.")]
        XRRayInteractor m_TeleportInteractor;

        /// <summary>
        /// The TrackedPoseDriver used to drive the pose of the teleport ray visuals
        /// </summary>
        [SerializeField]
        TrackedPoseDriver m_TrackedPoseDriver;

        /// <summary>
        /// The hand's NearFarInteractor that is disabled when teleport mode is active, and reactivated when teleport mode is deactivated
        /// </summary>
        [SerializeField, FormerlySerializedAs("m_nearFarInteractor")]
        NearFarInteractor m_NearFarInteractor;

#if XR_HANDS_1_5_OR_NEWER
        XRHandJointsUpdatedEventArgs m_LatestHandData;
#endif
        [SerializeField, Min(0.05f)] float m_ActivationDwell = 0.18f;
        bool m_Active, m_NeutralSeen, m_NearFarWasEnabled;
        float m_ActivationSince = -1f;
        float m_LastJointUpdate = -1f;

        void Awake()
        {
            if (m_TeleportInteractor.rotateReferenceFrame != null)
            {
                Debug.LogWarning("The Teleport Hand Gesture creates a dynamic reference frame for rotation, so the assigned one will be ignored.");
            }

            m_TeleportInteractor.rotateReferenceFrame = new GameObject("Dynamic Teleport Anchor Rotation Reference Frame").transform;
            m_TeleportInteractor.rotateReferenceFrame.SetParent(transform, false);
        }

        void OnEnable()
        {
            m_NeutralSeen = false;
            m_ActivationSince = m_LastJointUpdate = -1f;
            m_TeleportInteractor.gameObject.SetActive(false);
            m_TeleportInteractor.selectEntered.AddListener(OnTeleportSelectEntered);
            m_TeleportInteractor.selectExited.AddListener(OnTeleportSelectExited);
        }

        void OnDisable()
        {
            CancelTeleport();
            m_TeleportInteractor.selectEntered.RemoveListener(OnTeleportSelectEntered);
            m_TeleportInteractor.selectExited.RemoveListener(OnTeleportSelectExited);
        }

#if XR_HANDS_1_5_OR_NEWER
        void OnActivateDetected()
        {
            m_Active = true;
            m_NearFarWasEnabled = m_NearFarInteractor.enabled;
            m_NearFarInteractor.enabled = false;
            m_TeleportInteractor.gameObject.SetActive(true);
        }

        void OnCancelDetected()
        {
            CancelTeleport();
        }
#endif

        void OnApplicationFocus(bool focused) { if (!focused) CancelTeleport(); }
        void OnApplicationPause(bool paused) { if (paused) CancelTeleport(); }

        void CancelTeleport()
        {
            // Explicit cancellation makes XRI's TeleportationArea ignore the select-exit.
            // Tracking loss, disabling, and an open-hand cancel must never teleport the user.
            if (m_TeleportInteractor)
            {
                if (m_TeleportInteractor.interactionManager)
                    m_TeleportInteractor.interactionManager.CancelInteractorSelection((IXRSelectInteractor)m_TeleportInteractor);
                m_TeleportInteractor.gameObject.SetActive(false);
            }
            if (m_JoystickPinchHandGesture) m_JoystickPinchHandGesture.StopJoystickPinchHandGestureTracking();
            if (m_TrackedPoseDriver) m_TrackedPoseDriver.enabled = true;
            if (m_Active && m_NearFarInteractor) m_NearFarInteractor.enabled = m_NearFarWasEnabled;
            m_Active = m_NeutralSeen = false;
            m_ActivationSince = -1f;
        }

        void OnTeleportSelectEntered(SelectEnterEventArgs eventArgs)
        {
            m_JoystickPinchHandGesture.StartJoystickPinchGestureTracking();
        }

        void OnTeleportSelectExited(SelectExitEventArgs eventArgs)
        {
            m_JoystickPinchHandGesture.StopJoystickPinchHandGestureTracking();
        }

#if XR_HANDS_1_5_OR_NEWER
        /// <summary>
        /// Receives joint data from the XRHandTrackingEvents component for use in gesture detection.
        /// </summary>
        /// <param name="jointEventArgs">Event args containing the joint data.</param>
        public void OnJointsUpdated(XRHandJointsUpdatedEventArgs jointEventArgs)
        {
            m_LatestHandData = jointEventArgs;
            m_LastJointUpdate = Time.unscaledTime;
        }
#endif

        void Update()
        {
#if XR_HANDS_1_5_OR_NEWER
            bool valid = Application.isFocused && m_LatestHandData != null && m_LatestHandData.subsystem.running
                && m_LatestHandData.hand.isTracked && Time.unscaledTime - m_LastJointUpdate < .2f;
            if (valid)
            {
                var aim = m_LatestHandData.hand.handedness == Handedness.Left ? MetaAimHand.left : MetaAimHand.right;
                valid = aim == null || (aim.aimFlags.ReadValue() & (int)(MetaAimFlags.SystemGesture | MetaAimFlags.MenuPressed)) == 0;
            }
            if (!valid) { CancelTeleport(); return; }
            bool cancel = m_CancelHandShape.CheckConditions(m_LatestHandData);
            if (m_Active)
            {
                if (cancel) { OnCancelDetected(); return; }
                m_TrackedPoseDriver.enabled = !m_TeleportInteractor.hasSelection;
                return;
            }
            bool busy = !m_NearFarInteractor.isActiveAndEnabled || m_NearFarInteractor.hasSelection || m_NearFarInteractor.hasHover
                || (m_NearFarInteractor.TryGetUIModel(out var ui) && ui.currentRaycast.isValid);
            if (busy) { m_NeutralSeen = false; m_ActivationSince = -1f; return; }
            bool activate = m_ActivateHandShape.CheckConditions(m_LatestHandData)
                && m_ActivateHandOrientation.CheckConditions(m_LatestHandData.hand.rootPose, m_LatestHandData.hand.handedness);
            if (!activate)
            {
                m_NeutralSeen = true;
                m_ActivationSince = -1f;
                return;
            }
            if (!m_NeutralSeen) return;
            if (m_ActivationSince < 0) m_ActivationSince = Time.unscaledTime;
            if (Time.unscaledTime - m_ActivationSince >= m_ActivationDwell) OnActivateDetected();
#endif
        }
    }
}
