using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace StudyEverywhere
{
    // A café object that finds its own way back (the user, Oct 27: New York's shelf and counter pieces, picked up and
    // dropped on the floor, go back to their own spots). At home it rests still (kinematic, so nothing nudges it off a
    // shelf); let go, it falls and tumbles as anything would, and may be left on a table or the counter. Once it lies
    // still on the floor (or has fallen out of the room) it fades back to where it was made, shrinking away and growing
    // back in place.
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ReturnHome : MonoBehaviour
    {
        [Tooltip("Height of the floor it returns from (world y).")] public float floorY;
        [Tooltip("Lying lower than this above the floor counts as on the floor.")] public float floorBand = .25f;
        [Tooltip("Seconds it lies on the floor before it goes back.")] public float wait = 1.2f;
        [Tooltip("Shown only while it stands at home: its soft shadow painted on the wall and shelf.")] public Renderer[] homeShadows;

        Rigidbody body; XRGrabInteractable grab;
        Vector3 homePos, baseScale; Quaternion homeRot;
        float onFloorFor, fade = -1;

        void Awake()
        {
            body = GetComponent<Rigidbody>(); grab = GetComponent<XRGrabInteractable>();
            homePos = transform.position; homeRot = transform.rotation; baseScale = transform.localScale;
            body.isKinematic = true;
        }

        void OnEnable() { if (grab) grab.selectExited.AddListener(Released); }
        void OnDisable() { if (grab) grab.selectExited.RemoveListener(Released); }

        // the grab restores the body as it found it (still, at home): let go, it is free to fall
        void Released(SelectExitEventArgs args) { if (!grab.isSelected) { body.isKinematic = false; body.WakeUp(); onFloorFor = 0; } }

        void Update()
        {
            bool home = body.isKinematic && fade < 0 && !(grab && grab.isSelected);
            if (homeShadows != null) foreach (var r in homeShadows) if (r && r.enabled != home) r.enabled = home;
            if (fade >= 0)
            {
                // shrink away where it lies (0.25 s), then grow back at home (0.3 s)
                fade += Time.deltaTime;
                if (fade < .25f) transform.localScale = baseScale * (1 - fade / .25f);
                else
                {
                    if (transform.position != homePos) { body.isKinematic = true; transform.SetPositionAndRotation(homePos, homeRot); }
                    float k = Mathf.Clamp01((fade - .25f) / .3f); transform.localScale = baseScale * Mathf.SmoothStep(0, 1, k);
                    if (k >= 1) { fade = -1; transform.localScale = baseScale; }
                }
                return;
            }
            if (body.isKinematic || (grab && grab.isSelected)) { onFloorFor = 0; return; }
            bool fallenOut = transform.position.y < floorY - 2;
            bool onFloor = transform.position.y < floorY + floorBand && body.linearVelocity.sqrMagnitude < .01f;
            onFloorFor = onFloor ? onFloorFor + Time.deltaTime : 0;
            if (fallenOut || onFloorFor >= wait) { fade = 0; onFloorFor = 0; body.linearVelocity = Vector3.zero; body.angularVelocity = Vector3.zero; }
        }
    }
}
