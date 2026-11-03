using Unity.XR.CoreUtils;
using UnityEngine;

namespace StudyEverywhere
{
    // Keeps the person out of a lake (Evercrest's tarn): a grid of cells over the water and a little of its shore, baked
    // when the scene is built. Whenever the head comes over a water cell, by gliding on the thumbstick, by teleporting
    // or by walking in the room, the rig is put back where it last stood on dry ground. Turning is never stopped.
    [DefaultExecutionOrder(1000)]   // after the locomotion and teleport providers have moved the rig this frame
    public sealed class WaterKeepOut : MonoBehaviour
    {
        public XROrigin origin;
        [Tooltip("World x, z of the grid's first cell corner.")] public Vector2 corner;
        public float cell = .5f;
        public int columns, rows;
        [Tooltip("One per cell, row by row: 1 where the person may not stand.")] public byte[] water;

        Vector3 lastGood; bool known;

        public bool Wet(Vector3 p)
        {
            if (water == null || columns <= 0) return false;
            int i = Mathf.FloorToInt((p.x - corner.x) / cell), j = Mathf.FloorToInt((p.z - corner.y) / cell);
            return i >= 0 && j >= 0 && i < columns && j < rows && water[j * columns + i] != 0;
        }

        void LateUpdate()
        {
            if (!origin || !origin.Camera) return;
            var rig = origin.transform;
            if (!Wet(origin.Camera.transform.position)) { lastGood = rig.position; known = true; return; }
            if (!known) return;
            // back to dry ground: the rig where it was, keeping the way it now faces (turning about the head moves the rig a little)
            var head = origin.Camera.transform.position;
            rig.position = lastGood;
            if (Wet(origin.Camera.transform.position)) rig.position += new Vector3(lastGood.x - head.x, 0, lastGood.z - head.z).normalized * cell;
            Physics.SyncTransforms();   // the rig's capsule (CharacterController) starts the next glide from here; GroundFollow then sets the feet
        }
    }
}
