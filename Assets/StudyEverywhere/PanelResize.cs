using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace StudyEverywhere
{
    // Corner grip on a panel: drag it with the controller ray (or the mouse in the Editor) to resize the panel about its
    // centre. The pointer is projected onto the panel plane, so the drag keeps working after the ray leaves the card.
    public sealed class PanelResize : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        public PanelWindow window;
        float startScale, startDistance;

        public void OnBeginDrag(PointerEventData e)
        {
            startDistance = TryPoint(e, out var p) ? Vector3.Distance(p, window.transform.position) : 0;
            startScale = window.UserScale;
        }

        public void OnDrag(PointerEventData e)
        {
            if (startDistance < 0.01f || !TryPoint(e, out var p)) return;
            window.UserScale = startScale * Vector3.Distance(p, window.transform.position) / startDistance;
        }

        bool TryPoint(PointerEventData e, out Vector3 point)
        {
            point = default;
            Ray ray;
            if (e is TrackedDeviceEventData t && t.rayPoints != null && t.rayPoints.Count >= 2) ray = new Ray(t.rayPoints[0], t.rayPoints[1] - t.rayPoints[0]);
            else if (e.pressEventCamera) ray = e.pressEventCamera.ScreenPointToRay(e.position);
            else return false;
            var plane = new Plane(window.transform.forward, window.transform.position);
            if (!plane.Raycast(ray, out float d)) return false;
            point = ray.GetPoint(d);
            return true;
        }
    }
}
