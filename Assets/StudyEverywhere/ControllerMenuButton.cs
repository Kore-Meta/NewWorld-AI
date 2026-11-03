using UnityEngine;
using UnityEngine.XR;

namespace StudyEverywhere
{
    // Opens and closes a place's wrist menu from the controllers, so nobody has to aim at the watch: press the left
    // controller's menu (≡) or Y button. The menu appears in front of the person, where the other hand's ray can reach.
    public sealed class ControllerMenuButton : MonoBehaviour
    {
        public WristPanel panel;
        bool was;

        void Update()
        {
            if (!panel) return;
            var left = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            bool pressed = left.isValid
                && ((left.TryGetFeatureValue(CommonUsages.menuButton, out bool menu) && menu)
                 || (left.TryGetFeatureValue(CommonUsages.secondaryButton, out bool y) && y));
            if (pressed && !was) panel.Toggle();
            was = pressed;
        }
    }
}
