using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace StudyEverywhere
{
    // A café pet's toy (the user, Oct 27): Bubble's ball, Émile's feather wand and felt mouse, Hori's big ball. The person
    // can pick it up (near or with the ray) and throw it (XRGrabInteractable, thrown with the hand's speed); its pet
    // watches for that (Released, ThrownAt) and plays with it. A pet can carry it in its mouth (CarryIn: it rides a mouth
    // point, the grab off meanwhile, so it can't be pulled out of the mouth half-way) and drop it, or knock it along
    // (Nudge). It never leaves the room: below the floor or outside its bounds it is put back where it lives.
    [RequireComponent(typeof(Rigidbody))]
    [DefaultExecutionOrder(1000)]   // after the pets' LateUpdate has placed their mouths
    public sealed class PetToy : MonoBehaviour
    {
        [Tooltip("Where it lives (and is put back when it gets out of the room).")] public Vector3 home;
        public Quaternion homeRotation = Quaternion.identity;
        [Tooltip("The room it stays in (world space).")] public Bounds room = new Bounds(Vector3.zero, Vector3.one * 100);

        [Tooltip("Optional: the floor's open areas (world space; walls without colliders round them). Loose, it stays inside one of them, its whole width clear of their edges, and bounces softly off an edge (revision 102828, the user: Hori's ball went into the corner, inside the wall).")] public Bounds[] keepIn;

        public Rigidbody Body { get; private set; }
        public XRGrabInteractable Grab { get; private set; }
        public bool Held => Grab && Grab.isSelected || simulateHeld;
        [System.NonSerialized] public bool simulateHeld;   // tooling (ToyPlayCheck): as if a hand held it
        // tooling: as if the person had thrown it from `from` at `velocity`
        public void SimulateThrow(Vector3 from, Vector3 velocity)
        {
            simulateHeld = false; Release(); transform.position = from; Body.position = from; Body.linearVelocity = velocity; ThrownAt = Time.time;
        }
        public bool Carried => carrier;
        public float ThrownAt { get; private set; } = -99;   // when the person last let go of it
        public float GrabbedAt { get; private set; } = -99;  // when the person last took it
        // lying still on something (not held, not carried, barely moving)
        public bool Resting => !Held && !Carried && Body && Body.linearVelocity.sqrMagnitude < .02f;
        public Vector3 Velocity => Body ? Body.linearVelocity : Vector3.zero;

        Transform carrier; Vector3 carryOffset;

        void Awake()
        {
            Body = GetComponent<Rigidbody>(); Grab = GetComponent<XRGrabInteractable>();
            if (home == Vector3.zero) { home = transform.position; homeRotation = transform.rotation; }
        }
        void OnEnable() { if (Grab) { Grab.selectEntered.AddListener(OnGrabbed); Grab.selectExited.AddListener(OnReleased); } }
        void OnDisable() { if (Grab) { Grab.selectEntered.RemoveListener(OnGrabbed); Grab.selectExited.RemoveListener(OnReleased); } }
        void OnGrabbed(SelectEnterEventArgs _) { GrabbedAt = Time.time; }
        void OnReleased(SelectExitEventArgs _) { ThrownAt = Time.time; }

        void LateUpdate()
        {
            if (carrier) { transform.SetPositionAndRotation(carrier.TransformPoint(carryOffset), carrier.rotation); return; }
            if (Held) return;
            var p = transform.position;
            if (!room.Contains(p) || float.IsNaN(p.x)) PutBack();
        }

        // Inside the open floor: on the level, its centre its own radius in from the edges of the area it was last in.
        // Past an edge it is set back on it and the speed that carried it out is turned back, halved.
        int area = -1;
        void FixedUpdate()
        {
            if (keepIn == null || keepIn.Length == 0 || carrier || Held || !Body) return;
            var p = Body.position; float r = Radius;
            bool In(Bounds b) => p.x >= b.min.x + r && p.x <= b.max.x - r && p.z >= b.min.z + r && p.z <= b.max.z - r;
            if (area >= 0 && area < keepIn.Length && In(keepIn[area])) return;
            for (int i = 0; i < keepIn.Length; i++) if (In(keepIn[i])) { area = i; return; }
            if (area < 0 || area >= keepIn.Length)
            {
                // never in one yet: the nearest
                float best = float.MaxValue;
                for (int i = 0; i < keepIn.Length; i++) { float d = (keepIn[i].ClosestPoint(new Vector3(p.x, keepIn[i].center.y, p.z)) - new Vector3(p.x, keepIn[i].center.y, p.z)).sqrMagnitude; if (d < best) { best = d; area = i; } }
            }
            var a = keepIn[area]; var v = Body.linearVelocity;
            float x = Mathf.Clamp(p.x, a.min.x + r, a.max.x - r), z = Mathf.Clamp(p.z, a.min.z + r, a.max.z - r);
            if (x != p.x && (x - p.x) * v.x < 0) v.x = -v.x * .5f;
            if (z != p.z && (z - p.z) * v.z < 0) v.z = -v.z * .5f;
            Body.position = new Vector3(x, p.y, z); Body.linearVelocity = v;
        }
        float Radius { get { if (radius < 0) { var s = GetComponent<SphereCollider>(); radius = s ? s.radius * transform.lossyScale.x : .02f; } return radius; } }
        float radius = -1;

        public void PutBack()
        {
            Release();
            Body.linearVelocity = Vector3.zero; Body.angularVelocity = Vector3.zero;
            transform.SetPositionAndRotation(home, homeRotation); Body.position = home; Body.rotation = homeRotation;
        }

        // In a pet's mouth: it follows the mouth point (offset in its frame), no physics, the person can't take it meanwhile
        public void CarryIn(Transform mouth, Vector3 offset = default)
        {
            if (Held) return;
            carrier = mouth; carryOffset = offset;
            Body.isKinematic = true; if (Grab) Grab.enabled = false;
        }
        // Dropped (from the mouth), at a point with a little speed, or just let go where it is
        public void Drop(Vector3? at = null, Vector3 velocity = default)
        {
            if (!carrier) return;
            Release();
            if (at.HasValue) { transform.position = at.Value; Body.position = at.Value; }
            Body.linearVelocity = velocity;
        }
        void Release() { carrier = null; if (Body) Body.isKinematic = false; if (Grab) Grab.enabled = true; }

        // Knocked along by a paw or a pounce
        public void Nudge(Vector3 velocityChange)
        {
            if (Held || Carried) return;
            Body.WakeUp(); Body.AddForce(velocityChange, ForceMode.VelocityChange);
        }
    }
}
