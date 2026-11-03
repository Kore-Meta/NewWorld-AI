using Unity.XR.CoreUtils;
using UnityEngine;

namespace StudyEverywhere
{
    // Keeps the person's feet on the ground on land that rises and falls (Evercrest's shelf, Seabreeze's beach), whatever
    // moves them: the thumbstick, a teleport, walking the real room. The hands rig's gravity is off and a glide only moves
    // level, so on a slope it floated out over the hollow going down and ran into the hillside going up. Each frame, after
    // the movement, a ray goes down under the head onto the walkable ground (the teleport surfaces, nothing else): the rig
    // rises at once to meet it and settles quickly onto it going down, so it is never under the ground nor above it.
    // Where there is no walkable ground under the head (past the area's edge, over water, too steep) the rig slides back
    // so the head is where it last stood on good ground. Choosing a place (PlayerPlacement) drops or lifts the rig to the
    // ground there at once; a seated place keeps the eye where it put it, a standing one keeps the person's own height.
    // Turning is never stopped; while lying back (a reclined rig) it waits.
    [DefaultExecutionOrder(1010)]   // after the locomotion providers (Update), RoomBounds (990) and WaterKeepOut (1000)
    public sealed class GroundFollow : MonoBehaviour
    {
        public XROrigin origin;
        public PlayerPlacement placement;
        [Tooltip("The surfaces one may stand on; nothing else counts as ground.")] public Collider[] ground;
        [Tooltip("Degrees; steeper ground is not walked onto.")] public float slopeLimit = 35;
        [Tooltip("Metres a second the feet settle onto lower ground (higher ground is met at once).")] public float settle = 6;
        [Tooltip("A drop bigger than this (a teleport, a place) is taken in one go.")] public float snap = .75f;

        Vector3 lastGood; bool known;

        void OnEnable() { if (placement) placement.Changed += Placed; }
        void OnDisable() { if (placement) placement.Changed -= Placed; }

        // The highest walkable ground under a point, looking down from a little over it.
        public bool Ground(Vector3 at, out float y)
        {
            y = 0; bool found = false; float best = float.MaxValue;
            if (ground == null) return false;
            var ray = new Ray(new Vector3(at.x, at.y + .5f, at.z), Vector3.down);
            foreach (var c in ground)
            {
                if (!c || !c.enabled || !c.gameObject.activeInHierarchy || !c.Raycast(ray, out var hit, 60) || hit.distance >= best) continue;
                best = hit.distance; found = Mathf.Abs(hit.normal.y) >= Mathf.Cos(slopeLimit * Mathf.Deg2Rad); y = hit.point.y;
            }
            return found;
        }

        bool Upright => Vector3.Angle(origin.transform.up, Vector3.up) < 1;

        void LateUpdate()
        {
            if (!origin || !origin.Camera) return;
            if (!Upright) { known = false; return; }
            var rig = origin.transform;
            var head = origin.Camera.transform.position;
            if (Ground(head, out float y)) { lastGood = head; known = true; Settle(y, false, false); return; }
            if (!known) return;
            // back to where the head last stood on good ground, keeping the way the person now faces
            rig.position += new Vector3(lastGood.x - head.x, 0, lastGood.z - head.z);
            if (Ground(origin.Camera.transform.position, out y)) Settle(y, false, false);
            Physics.SyncTransforms();
        }

        void Settle(float y, bool keepEye, bool now)
        {
            var rig = origin.transform; var p = rig.position; float dy = y - p.y;
            if (Mathf.Abs(dy) < 1e-4f) return;
            if (dy < 0 && -dy < snap && !now) dy = Mathf.MoveTowards(p.y, y, settle * Time.deltaTime) - p.y;
            rig.position = p + Vector3.up * dy;
            // a place sets the eye by the floor offset over the rig: move it the other way so the eye stays on the place
            if (keepEye) origin.CameraFloorOffsetObject.transform.localPosition -= Vector3.up * dy;
            Physics.SyncTransforms();   // the rig's capsule (CharacterController) starts the next glide from here
        }

        void Placed()
        {
            if (!origin || !origin.Camera) return;
            if (!Upright) { known = false; return; }
            var head = origin.Camera.transform.position;
            if (!Ground(head, out float y)) return;
            lastGood = head; known = true;
            Settle(y, placement.Seated, true);
        }
    }
}
