using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace StudyEverywhere
{
    // Lets a floating panel be carried by its grab bar, with a hand (pinch or grab) or a controller (grip, near or by
    // ray). The panel keeps where it was taken hold of and turns to face the person while it moves, so it always lands
    // readable. The grab bar's collider is the only one the interactable sees, so the card itself stays clickable UI.
    [RequireComponent(typeof(XRGrabInteractable))]
    public sealed class PanelGrab : MonoBehaviour
    {
        XRGrabInteractable grab;

        void Awake() => grab = GetComponent<XRGrabInteractable>();

        void LateUpdate()
        {
            if (!grab.isSelected) return;
            var head = Camera.main;
            if (!head) return;
            var away = transform.position - head.transform.position;
            if (away.sqrMagnitude > 1e-4f) transform.rotation = Quaternion.LookRotation(away, Vector3.up);   // canvas forward points away from the viewer
        }
    }
}
