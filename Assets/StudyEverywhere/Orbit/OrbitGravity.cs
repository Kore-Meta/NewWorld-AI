using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity;

namespace StudyEverywhere
{
    // Gravity on or off in the Orbit library (the menu's Gravity row), and a jetpack on the controllers.
    //
    // Gravity off: the person rises off the floor slowly (several seconds, easing in and out, never a jolt) and bobs
    // gently, and moves as astronauts do, by grabbing the air: close a hand (or hold a controller's grip) and pull, and
    // the body glides the other way, keeping its speed after the hand opens and slowing only slowly. Loose things (the
    // books and the desk things) lift off and tumble softly; a floating book can still be caught and held. XRI's own
    // gravity (GravityProvider) is paused meanwhile, or it would drag the rig back down every frame.
    // Gravity on: the person settles back to the floor where they are, as slowly, and things drift home.
    //
    // Jetpack: hold a controller's A or X button and the body is pushed the way that controller points, with a soft
    // rumble; let go and you coast. With gravity off you keep drifting; with gravity on you sink gently back down.
    // Always starts with gravity on.
    public sealed class OrbitGravity : MonoBehaviour
    {
        public XROrigin origin;
        [Tooltip("Hands' and controllers' Near-Far interactors: one holding something does not swim.")]
        public NearFarInteractor leftHand, rightHand, leftController, rightController;
        [Tooltip("Things that float when gravity is off.")] public Transform[] floaters;
        [Header("Room")]
        public Vector3 roomCentre;
        public float roomRadius = 6.3f, maxRise = 3.4f;
        [Header("Feel")]
        [Tooltip("How far the person lifts off the floor, and the bob on top of it (metres).")] public float lift = .3f, bob = .03f;
        [Tooltip("Seconds to rise into weightlessness, or settle back down.")] public float settleSeconds = 4.5f;
        [Tooltip("Speed kept per second after the hand lets go (1 = forever).")] [Range(.1f, 1)] public float keep = .6f;
        public float maxSpeed = 1.6f;
        [Tooltip("Curl ratio (fingertip to palm over palm length) below which a hand counts as closed, and above which open again.")]
        public float closeRatio = .75f, openRatio = .95f;
        [Header("Jetpack (A / X)")]
        public float thrust = 1.6f, jetpackMaxSpeed = 2.2f;
        [Tooltip("With gravity on, how fast a jetpack lift sinks back (m/s²).")] public float sink = 1.2f;

        public bool On { get; private set; } = true;
        public bool Thrusting { get; private set; }
        public event Action Changed;

        sealed class Floater
        {
            public Transform t; public Vector3 home; public Quaternion homeRot; public Rigidbody body; public bool wasKinematic;
            public XRGrabInteractable grab; public Vector3 offset, axis; public float rise, phase, spin, weight; public bool returning;
        }
        readonly List<Floater> things = new List<Floater>();
        static readonly List<XRHandSubsystem> subsystems = new List<XRHandSubsystem>();
        readonly List<GravityProvider> xriGravity = new List<GravityProvider>();
        Vector3 applied, lastSet, drift, velocity, lastLeft, lastRight;
        bool hasLast, leftClosed, rightClosed, leftWas, rightWas;
        float weightless;   // 0 with gravity, 1 floating; eased in Update

        void Awake()
        {
            foreach (var t in floaters)
            {
                if (!t) continue;
                var f = new Floater { t = t, home = t.position, homeRot = t.rotation, body = t.GetComponent<Rigidbody>(), grab = t.GetComponent<XRGrabInteractable>() };
                int k = things.Count;
                f.rise = .25f + .55f * Hash(k, 1); f.phase = Hash(k, 2) * 10; f.spin = 6 + 14 * Hash(k, 3);
                f.axis = new Vector3(Hash(k, 4) - .5f, Hash(k, 5) - .5f, Hash(k, 6) - .5f).normalized;
                things.Add(f);
            }
            if (origin) origin.GetComponentsInChildren(true, xriGravity);
        }
        static float Hash(int k, int salt) { unchecked { uint h = (uint)(k * 73856093 ^ salt * 19349663); h ^= h >> 13; h *= 0x5bd1e995; h ^= h >> 15; return (h & 0xffff) / 65535f; } }

        public void Set(bool on)
        {
            if (On == on) return;
            On = on;
            if (!on)
                foreach (var f in things)
                {
                    if (!f.t) continue;
                    f.home = f.t.position; f.homeRot = f.t.rotation; f.offset = Vector3.zero; f.weight = 0;
                    if (f.body) { f.wasKinematic = f.body.isKinematic; f.body.isKinematic = true; f.body.linearVelocity = Vector3.zero; f.body.angularVelocity = Vector3.zero; }
                }
            else { velocity = Vector3.zero; foreach (var f in things) f.returning = true; }
            Changed?.Invoke();
        }
        public void Toggle() => Set(!On);

        // Gravity back on at once, the person down on the floor where they are (no slow settle): for going to a place
        // from the menu while floating, so they arrive standing at the spot instead of drifting above it.
        public void Land()
        {
            if (origin && hasLast) origin.transform.position -= applied;
            applied = drift = velocity = Vector3.zero; weightless = 0; hasLast = false;
            foreach (var g in xriGravity) if (g) g.useGravity = true;
            if (On) return;
            On = true;
            foreach (var f in things) f.returning = true;
            Changed?.Invoke();
        }

        void Update()
        {
            if (!origin) return;
            float dt = Time.deltaTime;
            weightless = Mathf.MoveTowards(weightless, On ? 0 : 1, dt / Mathf.Max(.1f, settleSeconds));
            float ease = weightless * weightless * weightless * (weightless * (weightless * 6 - 15) + 10);   // smootherstep: no jolt at either end
            // XRI's gravity would pull the rig straight back down while it floats or flies
            bool airborne = ease > 0 || drift.y > .001f || Thrusting;
            foreach (var g in xriGravity) if (g && g.useGravity == airborne) g.useGravity = !airborne;
            MovePerson(dt, ease);
            MoveThings(dt, ease);
        }

        // ---- the person: the rig is carried by `applied` on top of wherever teleport or the menu put it
        void MovePerson(float dt, float ease)
        {
            var rig = origin.transform;
            // something else moved the rig (a teleport, a place, a seat): take its new spot as the base
            Vector3 basePos = hasLast && (rig.position - lastSet).sqrMagnitude < 1e-6f ? lastSet - applied : rig.position;
            var space = origin.CameraFloorOffsetObject ? origin.CameraFloorOffsetObject.transform : rig;
            if (!On) Swim(dt, space);
            Jetpack(dt, space);
            if (On)
            {
                // with gravity: a jetpack lift sinks slowly back to the floor, and sideways coasting fades
                if (!Thrusting) { velocity.y -= sink * dt; velocity.x *= Mathf.Pow(.35f, dt); velocity.z *= Mathf.Pow(.35f, dt); }
            }
            else velocity *= Mathf.Pow(keep, dt);
            drift += velocity * dt;
            // stay inside the rotunda and between floor and ceiling
            var flat = new Vector3(basePos.x + drift.x - roomCentre.x, 0, basePos.z + drift.z - roomCentre.z);
            if (flat.magnitude > roomRadius) { var clamped = flat.normalized * roomRadius; drift.x += clamped.x - flat.x; drift.z += clamped.z - flat.z; velocity.x *= .3f; velocity.z *= .3f; }
            float floor = -ease * lift;   // drift.y is measured above the floating height
            if (drift.y < floor) { drift.y = floor; if (velocity.y < 0) velocity.y = 0; }
            if (drift.y > maxRise) { drift.y = maxRise; if (velocity.y > 0) velocity.y = 0; }
            // on the ground with gravity and nothing moving: fold the sideways drift into where the rig stands
            if (On && ease <= 0 && drift.y <= 1e-4f && velocity.sqrMagnitude < 1e-4f && !Thrusting)
            { basePos += new Vector3(drift.x, 0, drift.z); drift = Vector3.zero; velocity = Vector3.zero; }
            float up = ease * (lift + bob * Mathf.Sin(Time.time * Mathf.PI * 2 / 7f)) + drift.y;
            applied = new Vector3(drift.x, Mathf.Max(0, up), drift.z);
            rig.position = basePos + applied;
            lastSet = rig.position; hasLast = true;
        }

        // Grab the air and pull: each closed hand's movement (in the rig's own space) pushes the body the other way.
        void Swim(float dt, Transform space)
        {
            Vector3 push = Vector3.zero; int pulling = 0;
            SubsystemManager.GetSubsystems(subsystems);
            XRHandSubsystem hands = null; foreach (var s in subsystems) if (s.running) { hands = s; break; }
            if (Hand(hands?.leftHand, XRNode.LeftHand, leftHand, leftController, ref leftClosed, out var l))
            { if (leftWas) { push -= space.TransformVector(l - lastLeft); pulling++; } lastLeft = l; leftWas = true; }
            else leftWas = false;
            if (Hand(hands?.rightHand, XRNode.RightHand, rightHand, rightController, ref rightClosed, out var r))
            { if (rightWas) { push -= space.TransformVector(r - lastRight); pulling++; } lastRight = r; rightWas = true; }
            else rightWas = false;
            if (pulling > 0 && dt > 0) velocity = Vector3.ClampMagnitude(push / pulling / dt, maxSpeed);   // while pulling, the body follows the hand
        }

        // A closed, free hand's position in tracking space: tracked-hand joints if there are any, else the controller with its grip.
        bool Hand(XRHand? hand, XRNode node, NearFarInteractor handInteractor, NearFarInteractor controllerInteractor, ref bool closed, out Vector3 position)
        {
            position = default;
            if (hand.HasValue && hand.Value.isTracked)
            {
                var h = hand.Value;
                if (handInteractor && handInteractor.hasSelection) { closed = false; return false; }
                if (!h.GetJoint(XRHandJointID.Palm).TryGetPose(out var palm) || !h.GetJoint(XRHandJointID.Wrist).TryGetPose(out var wrist) || !h.GetJoint(XRHandJointID.MiddleProximal).TryGetPose(out var knuckle)
                    || !h.GetJoint(XRHandJointID.MiddleTip).TryGetPose(out var middle) || !h.GetJoint(XRHandJointID.RingTip).TryGetPose(out var ring)) { closed = false; return false; }
                float length = Mathf.Max(Vector3.Distance(wrist.position, knuckle.position), .03f);
                float curl = (Vector3.Distance(middle.position, palm.position) + Vector3.Distance(ring.position, palm.position)) * .5f / length;
                closed = closed ? curl < openRatio : curl < closeRatio;
                position = palm.position;
                return closed;
            }
            if (controllerInteractor && controllerInteractor.hasSelection) { closed = false; return false; }
            var device = InputDevices.GetDeviceAtXRNode(node);
            if (!device.isValid || !device.TryGetFeatureValue(CommonUsages.gripButton, out bool grip) || !device.TryGetFeatureValue(CommonUsages.devicePosition, out position)) { closed = false; return false; }
            closed = grip;
            return grip;
        }

        // A or X held: push along that controller's pointing direction, with a light rumble.
        void Jetpack(float dt, Transform space)
        {
            Vector3 dir = Vector3.zero; bool any = false;
            foreach (var node in new[] { XRNode.LeftHand, XRNode.RightHand })
            {
                var device = InputDevices.GetDeviceAtXRNode(node);
                if (!device.isValid || !device.TryGetFeatureValue(CommonUsages.primaryButton, out bool pressed) || !pressed) continue;
                if (!device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation)) continue;
                dir += space.TransformDirection(rotation * Vector3.forward); any = true;
                if (device.TryGetHapticCapabilities(out var caps) && caps.supportsImpulse) device.SendHapticImpulse(0, .12f, .05f);
            }
            Thrusting = any;
            if (!any) return;
            velocity = Vector3.ClampMagnitude(velocity + dir.normalized * thrust * dt, jetpackMaxSpeed);
        }

        // ---- the things: each lifts to its own height and turns slowly about its own axis, drifting a little; held
        // ones are left to the hand, and when let go their new spot is where they float from
        void MoveThings(float dt, float ease)
        {
            foreach (var f in things)
            {
                if (!f.t) continue;
                bool held = f.grab && f.grab.isSelected;
                if (held) { f.weight = 0; continue; }
                if (On && ease <= 0 && !f.returning) continue;   // with gravity, things stay wherever they are put
                if (!On && f.weight == 0 && (f.t.position - Target(f, ease)).sqrMagnitude > .0004f)
                { f.home = f.t.position - Vector3.up * f.rise * ease - f.offset; f.homeRot = f.t.rotation; }
                f.weight = Mathf.MoveTowards(f.weight, 1, dt / .8f);
                if (On && ease <= 0) { RestHome(f); continue; }
                f.offset = new Vector3(Mathf.Sin(Time.time * .21f + f.phase) * .06f, Mathf.Sin(Time.time * .33f + f.phase * 2) * .05f, Mathf.Cos(Time.time * .17f + f.phase) * .06f) * ease;
                f.t.position = Vector3.Lerp(f.t.position, Target(f, ease), 1 - Mathf.Exp(-dt * 3 * f.weight));
                var tumble = Quaternion.AngleAxis(f.spin * Time.time * ease, f.axis) * f.homeRot;
                f.t.rotation = Quaternion.Slerp(f.t.rotation, ease > 0 ? tumble : f.homeRot, 1 - Mathf.Exp(-dt * 2));
            }
        }
        Vector3 Target(Floater f, float ease) => f.home + Vector3.up * f.rise * ease + f.offset;
        void RestHome(Floater f)
        {
            if ((f.t.position - f.home).sqrMagnitude > 1e-6f || Quaternion.Angle(f.t.rotation, f.homeRot) > .1f)
            {
                f.t.position = Vector3.Lerp(f.t.position, f.home, .2f);
                f.t.rotation = Quaternion.Slerp(f.t.rotation, f.homeRot, .2f);
                return;
            }
            if (f.body && f.body.isKinematic != f.wasKinematic) f.body.isKinematic = f.wasKinematic;
            f.returning = false;
        }
    }
}
