using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace StudyEverywhere
{
    // A world in the Solar System mode that can be picked: poke it, pinch it or point and select with a controller, and
    // the deck brings it up (a planet) or tells its story (a moon).
    [RequireComponent(typeof(XRSimpleInteractable))]
    public sealed class HoloPick : MonoBehaviour
    {
        public HoloSolar solar;
        public int body = -1, moon = -1;
        XRSimpleInteractable touch;

        void Awake() => touch = GetComponent<XRSimpleInteractable>();
        void OnEnable() => touch.selectEntered.AddListener(OnSelect);
        void OnDisable() => touch.selectEntered.RemoveListener(OnSelect);
        void OnSelect(SelectEnterEventArgs _) { if (solar) solar.Pick(body, moon); }
    }
}
