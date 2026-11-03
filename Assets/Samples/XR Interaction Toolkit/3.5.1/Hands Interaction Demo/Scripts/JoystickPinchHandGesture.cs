using System.Collections;
using Unity.Mathematics;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

#if XR_HANDS_1_5_OR_NEWER
using UnityEngine.XR.Hands;
#endif

namespace UnityEngine.XR.Interaction.Toolkit.XRHandLocomotion
{
    /// <summary>
    /// Handles the joystick input mode enabled when pinching while the teleport visuals are activated
    /// Rotates the teleport ray reticle based on hand position
    /// </summary>
    public class JoystickPinchHandGesture : MonoBehaviour
    {
#if XR_HANDS_1_5_OR_NEWER
        /// <summary>
        /// The XRHandJointID used to retrieve the pose of the targeted joint
        /// </summary>
        [SerializeField]
        XRHandJointID m_JoystickAttachInputJointID = XRHandJointID.IndexTip;
#endif

        /// <summary>
        /// The radius used to normalize the projected planar pose data
        /// </summary>
        [SerializeField]
        float m_Radius = 0.02f;

        /// <summary>
        /// The amount by which the reticle rotation is smoothed.  Prevents input jitter
        /// </summary>
        [SerializeField]
        float m_RotationSmoothingValue = 0.2f;
        // Study Everywhere: deliberate direction adjustment, with a centre dead zone.
        [SerializeField, Min(0.01f)] float m_RotationDeadZone = 0.035f;

        /// <summary>
        /// The XRInteractorLineVisual that draws the teleport line and reticle visuals
        /// </summary>
        [SerializeField]
        XRInteractorLineVisual m_XRInteractorLineVisual;

        /// <summary>
        /// XRRayInteractor whose selectEntered and selectExited state is controlled by the TeleportHandGesture controller
        /// </summary>
        [SerializeField]
        XRRayInteractor m_XRRayInteractor;

        /// <summary>
        /// The target transform that is rotated and offset from the joystick center position and used to position the LookAt target position used for reticle rotation
        /// </summary>
        [SerializeField]
        Transform m_AttachTransformRotationTarget;

        /// <summary>
        /// Transform that is the child of the stabilized aim origin; used to perform the final LookAt operation after processing the initial position and rotation changes
        /// </summary>
        [SerializeField]
        Transform m_AttachTransformForRotation;

        /// <summary>
        /// The UI visuals displayed above the hand performing reticle rotation
        /// </summary>
        [SerializeField]
        Transform m_NavigatorUI;

        /// <summary>
        /// The XROrigin transform used to source the position of the navigator UI visuals when enabling joystick input
        /// </summary>
        [SerializeField]
        Transform m_XROrigin;

#if XR_HANDS_1_5_OR_NEWER
        XRHandJointsUpdatedEventArgs m_LatestHandData;
        Coroutine m_ActivateGestureDetection;
        Pose m_CenterInTrackingSpace;
        Vector3 m_SmoothedRotationCurrent;
        Vector3 m_SmoothedRotationVelocity;
        bool m_JoystickCenterSet;
#endif

        Coroutine m_HoldGestureDetection;
        Transform m_ReticleTransform;

        void OnEnable()
        {
            m_XRRayInteractor.selectEntered.AddListener(OnSelectEntered);

            m_NavigatorUI.gameObject.SetActive(false);
        }

        void OnDisable()
        {
            StopJoystickPinchHandGestureTracking();
            m_XRRayInteractor.selectEntered.RemoveListener(OnSelectEntered);
        }

        /// <summary>
        /// Called when the TeleportHandGesture's XRRayInteractor's selectEntered event is called
        /// </summary>
        public void StartJoystickPinchGestureTracking()
        {
            if (m_HoldGestureDetection != null)
                StopCoroutine(m_HoldGestureDetection);

            if (m_ReticleTransform == null)
            {
                if (!m_XRInteractorLineVisual.reticle) return;
                m_ReticleTransform = m_XRInteractorLineVisual.reticle.transform;
            }
#if XR_HANDS_1_5_OR_NEWER
            m_JoystickCenterSet = false;
            m_SmoothedRotationCurrent = m_SmoothedRotationVelocity = Vector3.zero;
#endif

            m_HoldGestureDetection = StartCoroutine(MaintainJoystickMode());
        }

        /// <summary>
        /// Called when the TeleportHandGesture's XRRayInteractor's selectExited event is called
        /// </summary>
        public void StopJoystickPinchHandGestureTracking()
        {
            if (m_HoldGestureDetection != null) StopCoroutine(m_HoldGestureDetection);
            m_HoldGestureDetection = null;

#if XR_HANDS_1_5_OR_NEWER
            m_JoystickCenterSet = false;
            m_SmoothedRotationCurrent = m_SmoothedRotationVelocity = Vector3.zero;
#endif

            if (m_AttachTransformForRotation) m_AttachTransformForRotation.localRotation = quaternion.identity;

            if (m_NavigatorUI) m_NavigatorUI.gameObject.SetActive(false);
        }

#if XR_HANDS_1_5_OR_NEWER
        /// <summary>
        /// Receives joint data from the XRHandTrackingEvents component for use in gesture detection.
        /// </summary>
        /// <param name="jointEventArgs">Event args containing the joint data.</param>
        public void OnJointsUpdated(XRHandJointsUpdatedEventArgs jointEventArgs)
        {
            m_LatestHandData = jointEventArgs;
        }
#endif

        void OnSelectEntered(SelectEnterEventArgs args)
        {
            // Check if the selected object is a TeleportationAnchor
            var selectingTeleportationAnchor = args.interactableObject.transform.TryGetComponent<TeleportationAnchor>(out _);
            m_NavigatorUI.gameObject.SetActive(!selectingTeleportationAnchor);
        }

        IEnumerator MaintainJoystickMode()
        {
#if XR_HANDS_1_5_OR_NEWER
            float rotationThresholdSquared = m_RotationDeadZone * m_RotationDeadZone;
            var initialHandPosePosition = Vector3.zero;
            var rotationProcessingThresholdExceeded = false;

            while (isActiveAndEnabled)
            {
                // Check that the hand subsystem is still running to guard against accessing a destroyed joints
                // array due to the hand subsystem being destroyed between the time of the joints updated callback
                // and this coroutine executes.
                if (m_LatestHandData != null && m_LatestHandData.subsystem.running &&
                    m_LatestHandData.hand.isTracked && m_JoystickAttachInputJointID != XRHandJointID.Invalid &&
                    m_LatestHandData.hand.GetJoint(m_JoystickAttachInputJointID).TryGetPose(out var pose))
                {
                    // Store the initial palm-down rotation
                    var initialRotation = pose.rotation;

                    // Calculate and apply the palm-up rotation to invert the cylindrical scrubbing direction as opposed to palm-up
                    Quaternion palmUpRotation = Quaternion.Inverse(initialRotation);
                    pose.rotation = palmUpRotation;

                    var joystickForward = m_LatestHandData.hand.rootPose.forward;
                    joystickForward.y = 0f;
                    if (joystickForward.sqrMagnitude < .0001f) joystickForward = Vector3.forward;

                    var planeNormal = Vector3.up;
                    if (!m_JoystickCenterSet)
                    {
                        m_CenterInTrackingSpace.position = pose.position;
                        m_CenterInTrackingSpace.rotation = Quaternion.LookRotation(joystickForward, planeNormal);
                        m_JoystickCenterSet = true;

                        initialHandPosePosition = pose.position;

                        var navigatorWorldPosition = m_XROrigin.TransformPoint(pose.position);
                        m_NavigatorUI.position = navigatorWorldPosition;

                        var currentDelta = pose.position - m_CenterInTrackingSpace.position;
                        var projectedDelta = Vector3.ProjectOnPlane(currentDelta, planeNormal);
                        var normalizedInputValue = projectedDelta / Mathf.Max(.01f, m_Radius);
                        var rotatedInputValue = Quaternion.Inverse(m_CenterInTrackingSpace.rotation) * normalizedInputValue;

                        // set the initial smoothed rotation value so the reticle has a gradual initial rotation starting by pointing forward
                        m_SmoothedRotationCurrent = rotatedInputValue;
                    }
                    else if (rotationProcessingThresholdExceeded)
                    {
                        var currentDelta = pose.position - m_CenterInTrackingSpace.position;
                        var projectedDelta = Vector3.ProjectOnPlane(currentDelta, planeNormal);
                        // Remove the centre dead zone and cap outlier motion before smoothing.
                        float distance = projectedDelta.magnitude;
                        // Allow values beyond one so the target can cross behind the reticle (180-degree facing).
                        var normalizedInputValue = Vector3.ClampMagnitude(projectedDelta.normalized * Mathf.Max(0, distance - m_RotationDeadZone) / Mathf.Max(.01f, m_Radius), 2f);
                        var rotatedInputValue = Quaternion.Inverse(m_CenterInTrackingSpace.rotation) * normalizedInputValue;

                        m_SmoothedRotationCurrent = Vector3.SmoothDamp(m_SmoothedRotationCurrent, rotatedInputValue, ref m_SmoothedRotationVelocity, m_RotationSmoothingValue);

                        var forwardOffsetPosition = m_ReticleTransform.TransformPoint(Vector3.forward * 0.5f);
                        m_AttachTransformRotationTarget.position = forwardOffsetPosition;
                        m_AttachTransformRotationTarget.localPosition -= m_SmoothedRotationCurrent * 0.5f;

                        if ((m_AttachTransformRotationTarget.position - m_AttachTransformForRotation.position).sqrMagnitude > .000001f)
                            m_AttachTransformForRotation.LookAt(m_AttachTransformRotationTarget.position);
                        m_NavigatorUI.rotation = m_AttachTransformForRotation.rotation;
                    }
                    else
                    {
                        // Allow for rotation processing after the hand has translated greater than a minimum distance, preventing initial jerky movement
                        // This also facilitates the ability to quickly snap/pinch quickly & repeatedly while maintaining a forward direction
                        rotationProcessingThresholdExceeded = Vector3.ProjectOnPlane(initialHandPosePosition - pose.position, Vector3.up).sqrMagnitude > rotationThresholdSquared;
                    }
                }

                yield return null;
            }
#else
            yield return null;
#endif
        }
    }
}
