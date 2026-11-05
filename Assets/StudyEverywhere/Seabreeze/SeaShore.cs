using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Hands;

namespace StudyEverywhere
{
    // The lagoon's edge at Seabreeze. Every few seconds a small wave runs up the beach and slides back: the sea level
    // (_SeaLevel, read by the water, the sand and the creatures) rises quickly and ebbs slowly, each wave a little
    // different. The sand stays dark and glossy where the last waves reached (_SeaWet), drying slowly. A fingertip (or a
    // controller's tip) dipped into the water sends rings across it (_SeaRipples), so the person can touch the sea.
    public sealed class SeaShore : MonoBehaviour
    {
        [Tooltip("Sea level between waves (m).")] public float rest = -.012f;
        [Tooltip("How high the biggest wave lifts the water (m); the beach face turns this into a run-up of about 15 times as far.")]
        public float runUp = .045f;
        [Tooltip("Seconds from one wave to the next.")] public float period = 9;
        [Tooltip("How fast the wet sand dries back down (m of height per second).")] public float drying = .0025f;
        public XROrigin origin;
        public Transform leftController, rightController;

        public static float Level { get; private set; }
        public static float Wet { get; private set; }
        public static bool Rising { get; private set; }

        // Calm (Calypso, on request): the waves die down over calmSeconds and the water lies still at its resting level, until
        // they are let run again. All of the sea goes quiet with it (headset review, Nov 4: "stop the waves" barely showed,
        // and the surf went on): the run-up, the lagoon's swell and ripples (_SeaCalm, Seabreeze Water) and the surf sound.
        public bool Calm { get; set; }
        [Tooltip("Seconds for the waves to die down, or build up again.")] public float calmSeconds = 4;
        [Tooltip("The surf sound (the ambience bed) fades out as the sea calms (found in the scene when empty).")] public RestAmbience surf;
        float calm;

        static readonly int LevelId = Shader.PropertyToID("_SeaLevel"), WetId = Shader.PropertyToID("_SeaWet"), RipplesId = Shader.PropertyToID("_SeaRipples"), CalmId = Shader.PropertyToID("_SeaCalm");
        readonly Vector4[] ripples = new Vector4[4];
        readonly List<Vector3> tips = new List<Vector3>();
        readonly Dictionary<int, (Vector3 at, float time)> lastRipple = new Dictionary<int, (Vector3, float)>();
        int nextRipple;
        float wet = -1, previous;

        static float Hash(int k) { unchecked { uint h = (uint)k * 2654435761u; h ^= h >> 15; h *= 2246822519u; h ^= h >> 13; return (h & 0xffff) / 65535f; } }

        // One wave's shape over its period: a quick rise as it runs up, a long slow slide back.
        public float LevelAt(float t)
        {
            float x = t / period; int k = Mathf.FloorToInt(x); float p = x - k;
            float amp = .45f + .55f * Hash(k * 7 + 3);
            float shape = p < .18f ? Mathf.SmoothStep(0, 1, p / .18f) : Mathf.Pow(1 - Mathf.SmoothStep(0, 1, (p - .18f) / .82f), 1.4f);
            return rest + runUp * amp * shape;
        }

        void OnEnable() { Shader.SetGlobalVectorArray(RipplesId, ripples); Apply(rest, rest); Shader.SetGlobalFloat(CalmId, 0); if (!surf) surf = FindAnyObjectByType<RestAmbience>(); }
        void OnDisable() { Shader.SetGlobalFloat(LevelId, rest); Shader.SetGlobalFloat(WetId, rest); Shader.SetGlobalFloat(CalmId, 0); if (surf) surf.bedScale = 1; }

        void Apply(float level, float wetLevel)
        {
            Level = level; Wet = wetLevel;
            Shader.SetGlobalFloat(LevelId, level); Shader.SetGlobalFloat(WetId, wetLevel);
        }

        void Update()
        {
            calm = Mathf.MoveTowards(calm, Calm ? 1 : 0, Time.deltaTime / Mathf.Max(.01f, calmSeconds));
            float still = Mathf.SmoothStep(0, 1, calm);
            float t = Time.timeSinceLevelLoad, level = Mathf.Lerp(LevelAt(t), rest, still);
            Shader.SetGlobalFloat(CalmId, still);
            if (surf) surf.bedScale = 1 - still;
            Rising = level > previous; previous = level;
            wet = Mathf.Max(level, wet < -0.5f ? level : wet - drying * Time.deltaTime);
            Apply(level, wet);

            // Fingers in the water: a ring where each tip goes in, and more as it moves through.
            ReadTips(origin, leftController, rightController, tips);
            for (int i = 0; i < tips.Count; i++)
            {
                var p = tips[i];
                float sand = SeabreezeBeach.Sand(p.x, p.z);
                bool inWater = p.y < level + .004f && level > sand + .003f && p.y > sand - .05f;
                if (!inWater) { lastRipple.Remove(i); continue; }
                bool fresh = !lastRipple.TryGetValue(i, out var last);
                if (fresh || Vector3.Distance(last.at, p) > .04f || t - last.time > .45f)
                {
                    ripples[nextRipple] = new Vector4(p.x, p.z, t, fresh ? 1 : .6f);
                    nextRipple = (nextRipple + 1) % ripples.Length;
                    lastRipple[i] = (p, t);
                    Shader.SetGlobalVectorArray(RipplesId, ripples);
                }
            }
        }

        static readonly List<XRHandSubsystem> hands = new List<XRHandSubsystem>();
        static readonly XRHandJointID[] Fingers = { XRHandJointID.IndexTip, XRHandJointID.MiddleTip, XRHandJointID.ThumbTip };

        // World positions of the fingertips that can touch things: index, middle and thumb of each tracked hand, or the
        // tip of a controller held instead.
        public static void ReadTips(XROrigin origin, Transform leftController, Transform rightController, List<Vector3> into, bool indexOnly = false)
        {
            into.Clear();
            SubsystemManager.GetSubsystems(hands);
            XRHandSubsystem running = null;
            foreach (var s in hands) if (s.running) { running = s; break; }
            var space = origin && origin.CameraFloorOffsetObject ? origin.CameraFloorOffsetObject.transform : null;
            bool left = false, right = false;
            if (running != null && space)
            {
                foreach (var (hand, isLeft) in new[] { (running.leftHand, true), (running.rightHand, false) })
                {
                    if (!hand.isTracked) continue;
                    foreach (var id in Fingers)
                    {
                        if (indexOnly && id != XRHandJointID.IndexTip) continue;
                        if (hand.GetJoint(id).TryGetPose(out var pose)) { into.Add(space.TransformPoint(pose.position)); if (isLeft) left = true; else right = true; }
                    }
                }
            }
            // A controller's tip: a few centimetres ahead of its grip, where a pointing finger would be.
            if (!left && leftController && leftController.gameObject.activeInHierarchy) into.Add(leftController.position + leftController.forward * .07f);
            if (!right && rightController && rightController.gameObject.activeInHierarchy) into.Add(rightController.position + rightController.forward * .07f);
        }
    }
}
