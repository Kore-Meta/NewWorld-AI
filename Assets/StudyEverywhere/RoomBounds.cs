using System;
using UnityEngine;

namespace StudyEverywhere
{
    // Keeps the person inside a place whatever moves them: teleport, continuous movement, a recenter, or walking the
    // real room. Each frame the head's position is checked against the allowed zones (rectangles and discs, in plan);
    // outside them, the rig slides back by the shortest way, so the head rests on the nearest edge. Colliders still do
    // the day-to-day stopping; this is the guarantee behind them (a window reveal, a gap in a wall, a step offset).
    // It also brings back someone who walked their real room out through a wall: the thumbstick's capsule is put under
    // the head when it moves, so out there the walls would hold them outside for good.
    [DefaultExecutionOrder(990)]   // after the locomotion and teleport providers have moved the rig this frame (they run in Update)
    public sealed class RoomBounds : MonoBehaviour
    {
        [Serializable]
        public struct Zone
        {
            public string name;
            public Vector2 centre;
            [Tooltip("Half size for a rectangle; ignored when radius is set.")]
            public Vector2 halfSize;
            [Tooltip("Set for a disc.")]
            public float radius;
        }

        public Unity.XR.CoreUtils.XROrigin origin;
        public Zone[] zones;
        [Tooltip("Places inside the zones the head may not enter (Seoul's pond): it is pushed out to the nearest edge.")]
        public Zone[] keepOut;

        void LateUpdate()
        {
            if (!origin || !origin.Camera || zones == null || zones.Length == 0) return;
            var head = origin.Camera.transform.position;
            var p = new Vector2(head.x, head.z);
            float best = float.MaxValue; Vector2 nearest = p;
            foreach (var z in zones)
            {
                Vector2 q;
                if (z.radius > 0) { var d = p - z.centre; q = d.magnitude <= z.radius ? p : z.centre + d.normalized * z.radius; }
                else q = new Vector2(Mathf.Clamp(p.x, z.centre.x - z.halfSize.x, z.centre.x + z.halfSize.x), Mathf.Clamp(p.y, z.centre.y - z.halfSize.y, z.centre.y + z.halfSize.y));
                float dist = (q - p).sqrMagnitude;
                if (dist < best) { best = dist; nearest = q; }
                if (best == 0) break;
            }
            // then out of any keep-out: the nearest way out of a rectangle (its closest side), or of a disc
            if (keepOut != null)
                foreach (var z in keepOut)
                {
                    var d = nearest - z.centre;
                    if (z.radius > 0) { if (d.magnitude < z.radius) nearest = z.centre + (d.sqrMagnitude > 1e-8f ? d.normalized : Vector2.right) * z.radius; continue; }
                    float ox = z.halfSize.x - Mathf.Abs(d.x), oz = z.halfSize.y - Mathf.Abs(d.y);
                    if (ox <= 0 || oz <= 0) continue;
                    if (ox < oz) nearest.x = z.centre.x + Mathf.Sign(d.x == 0 ? 1 : d.x) * z.halfSize.x;
                    else nearest.y = z.centre.y + Mathf.Sign(d.y == 0 ? 1 : d.y) * z.halfSize.y;
                }
            best = (nearest - p).sqrMagnitude;
            if (best < 1e-6f) return;
            var slide = nearest - p;
            origin.transform.position += new Vector3(slide.x, 0, slide.y);
            Physics.SyncTransforms();   // the rig's capsule (CharacterController) starts the next glide from here
        }
    }
}
