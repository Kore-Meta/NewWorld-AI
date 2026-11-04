using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;

namespace StudyEverywhere
{
    // Puts the person where they belong in the café, whatever their real room looks like: on first tracking, after a
    // headset recenter, and on request, the XR Origin turns and slides so the head lands on the chosen place's eye
    // point, facing the way the place faces. Seated places also set the eye height (the floor offset lifts or lowers
    // the view), so someone sitting at a real desk ends up in the chair, not beside the table. The person's own height
    // adjustment (wrist menu) is added on top and remembered.
    public sealed class PlayerPlacement : MonoBehaviour
    {
        [Serializable]
        public struct Place
        {
            public string name;
            public Transform anchor;   // eye point; its forward is the view direction
            public bool seated;
            [Tooltip("Degrees the view tips back about the head (lying in the grass looking up); 0 stays upright.")]
            public float recline;
        }

        public XROrigin origin;
        public Place[] places;
        [Tooltip("Metres the height buttons move the view.")]
        public float heightStep = 0.05f;
        public float maxHeightAdjust = 0.4f;

        const string HeightKey = "StudyEverywhere.HeightAdjust";
        readonly List<XRInputSubsystem> inputs = new List<XRInputSubsystem>();
        int current, pendingFrames = -1;
        Transform seat;   // a chair chosen from its seat icon (SeatSpot); null while at one of the named places
        Transform stand;  // a standing spot (getting up from a seat); null while at a seat or a named place
        bool placed;

        public int Current => seat || stand ? -1 : current;
        public Transform Seat => seat;
        public bool Seated { get; private set; }   // the last place set the eye height (sitting, lying), not the person's own
        [Tooltip("A place of its own (e.g. \"Orbit\") remembers its own view height, starting from defaultHeight; empty: the shared height.")]
        public string heightPlace = ""; public float defaultHeight;
        [Tooltip("Floor levels of a place with more than one storey (Solace's rotunda: 0 and 5.4). Moving to a place stands the rig on the highest floor below its eye; empty: the rig keeps its height.")]
        public float[] floors;
        [Tooltip("Standing places lift the eye to at least this height above the floor (m), so someone sitting in their real room stands at a standing person's height; 0 keeps the person's own height (the cafés: 1.6).")]
        public float standingEye;
        string Key => string.IsNullOrEmpty(heightPlace) ? HeightKey : HeightKey + "." + heightPlace;
        public float HeightAdjust => PlayerPrefs.GetFloat(Key, string.IsNullOrEmpty(heightPlace) ? 0 : defaultHeight);
        public event Action Changed;

        void OnEnable()
        {
            SubsystemManager.GetSubsystems(inputs);
            foreach (var s in inputs) s.trackingOriginUpdated += OnRecentered;
        }

        void OnDisable()
        {
            foreach (var s in inputs) s.trackingOriginUpdated -= OnRecentered;
            inputs.Clear();
        }

        // XR Origin reacts to a recenter in the same callback; place again a couple of frames later so ours wins.
        void OnRecentered(XRInputSubsystem _) => pendingFrames = 2;

        void Update()
        {
            if (!origin || !origin.Camera) return;
            if (!placed && origin.Camera.transform.localPosition != Vector3.zero) { placed = true; Recenter(); return; }
            if (pendingFrames >= 0 && pendingFrames-- == 0) Recenter();
            // Left a seat without its Stand up (a teleport, a glide, a few real steps): the person is standing now, so the
            // eye goes to standing height where they are (it kept the seat's, and in Seoul's floor chairs that was a
            // crouch: the user, Oct 27, for New York, Seoul and Paris)
            if (Seated && SeatEye && leaveSeatDistance > 0)
            {
                var d = origin.Camera.transform.position - SeatEye.position; d.y = 0;
                if (d.sqrMagnitude > leaveSeatDistance * leaveSeatDistance) StandHere();
            }
        }

        [Tooltip("Metres the head may move from a seat's eye before the person counts as up and standing (0: never).")]
        public float leaveSeatDistance = .6f;
        Transform SeatEye => seat ? seat : places != null && current >= 0 && current < places.Length && places[current].seated ? places[current].anchor : null;
        Transform freeSpot;   // where the person got up by moving away from a seat, kept for a recenter
        void StandHere()
        {
            if (!freeSpot) freeSpot = new GameObject("Standing spot | where you got up").transform;
            var head = origin.Camera.transform; var f = Vector3.ProjectOnPlane(head.forward, Vector3.up);
            // The spot's eye is a standing eye over the floor the rig stands on now, not the head itself: the head still
            // has the seat's eye height (about 1.2 m), and MoveTo looks for a floor 1.5 m under a standing eye, so in
            // Solace's rotunda a teleport or glide away from an upstairs couch (or from the desk up to the music floor)
            // found the ground floor 5.4 m down and dropped the person to it.
            var at = new Vector3(head.position.x, origin.transform.position.y + 1.6f, head.position.z);
            freeSpot.SetPositionAndRotation(at, Quaternion.LookRotation(f.sqrMagnitude > 1e-4f ? f : Vector3.forward));
            StandAt(freeSpot);
        }

        public void GoTo(int index)
        {
            if (places == null || index < 0 || index >= places.Length || !places[index].anchor || !origin) return;
            current = index;
            seat = null;
            stand = null;
            MoveTo(places[index]);
        }

        // Sits the person in a chair: the eye lands on the seat's eye point, facing the way it faces (the table).
        public void SitAt(Transform eye)
        {
            if (!eye || !origin) return;
            seat = eye;
            stand = null;
            MoveTo(new Place { name = eye.name, anchor = eye, seated = true });
        }

        // Gets up from a seat: the eye moves over the spot at the person's own standing height, facing the way it faces.
        public void StandAt(Transform spot)
        {
            if (!spot || !origin) return;
            seat = null;
            stand = spot;
            MoveTo(new Place { name = spot.name, anchor = spot, seated = false });
        }

        void MoveTo(Place place)
        {
            var head = origin.Camera.transform;
            var rig = origin.transform;
            Seated = place.seated;
            // Level the rig first (a previous place may have tipped it back), then face the place's direction about the head.
            var e = rig.eulerAngles;
            if (Mathf.Abs(Mathf.DeltaAngle(e.x, 0)) > .01f || Mathf.Abs(Mathf.DeltaAngle(e.z, 0)) > .01f) rig.RotateAround(head.position, Vector3.Cross(rig.up, Vector3.up).normalized, Vector3.Angle(rig.up, Vector3.up));
            Vector3 look = Vector3.ProjectOnPlane(head.forward, Vector3.up), want = Vector3.ProjectOnPlane(place.anchor.forward, Vector3.up);
            if (look.sqrMagnitude > 1e-4f && want.sqrMagnitude > 1e-4f) rig.RotateAround(head.position, Vector3.up, Vector3.SignedAngle(look, want, Vector3.up));
            // Another storey (or a stage on it): the rig stands on the floor under the place. A seated eye is about 1.2 m
            // above its floor, a standing one about 1.6, so the highest floor 1.0 / 1.5 m below the eye is the one.
            if (floors != null && floors.Length > 0)
            {
                float floor = float.NegativeInfinity, clear = place.seated ? 1.0f : 1.5f;
                foreach (var f in floors) if (f <= place.anchor.position.y - clear && f > floor) floor = f;
                if (!float.IsNegativeInfinity(floor)) rig.position = new Vector3(rig.position.x, floor, rig.position.z);
            }
            // Eye height: seated places put the eye at the anchor; standing places keep the person's real height (raised to
            // standingEye when they are lower: sitting in their real room).
            var offset = origin.CameraFloorOffsetObject.transform;
            float eyeAboveOffset = head.position.y - offset.position.y;
            float lift = place.seated ? place.anchor.position.y - rig.position.y - eyeAboveOffset : Mathf.Max(0, standingEye - eyeAboveOffset);
            offset.localPosition = new Vector3(offset.localPosition.x, lift + HeightAdjust, offset.localPosition.z);
            // Slide so the head is over the anchor.
            var slide = place.anchor.position - head.position;
            slide.y = 0;
            rig.position += slide;
            // Lying back: tip the whole rig about the head, so looking straight ahead (relaxed, head on the ground) shows the sky.
            if (place.recline > 0) rig.RotateAround(head.position, Vector3.Cross(Vector3.up, want.sqrMagnitude > 1e-4f ? want.normalized : Vector3.forward), -place.recline);
            Physics.SyncTransforms();
            Changed?.Invoke();
        }

        public void Recenter() { if (seat) SitAt(seat); else if (stand) StandAt(stand); else GoTo(current); }

        public void AdjustHeight(float delta)
        {
            float before = HeightAdjust, after = Mathf.Clamp(before + delta, -maxHeightAdjust, maxHeightAdjust);
            PlayerPrefs.SetFloat(Key, after);
            if (origin) origin.CameraFloorOffsetObject.transform.localPosition += Vector3.up * (after - before);
            Changed?.Invoke();
        }
    }
}
