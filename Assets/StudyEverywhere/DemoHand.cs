using System;
using UnityEngine;

namespace StudyEverywhere
{
    // A hand model (the XR Hands sample's rigged hand) posed by code, to show a gesture: each finger curls by an amount
    // (0 open, 1 closed) and the thumb can reach for a point (a pinch, a slide along the index finger). The curl turns
    // each finger joint about the line across the knuckles, toward the palm; which way is "toward the palm" is found
    // from the model itself when it starts, so left and right hands and any rig orientation work the same. The thumb
    // reaches by a few passes of cyclic coordinate descent. Blends smoothly toward whatever pose is asked for.
    public sealed class DemoHand : MonoBehaviour
    {
        static readonly string[][] FingerJoints =
        {
            new[] { "ThumbMetacarpal", "ThumbProximal", "ThumbDistal" },
            new[] { "IndexProximal", "IndexIntermediate", "IndexDistal" },
            new[] { "MiddleProximal", "MiddleIntermediate", "MiddleDistal" },
            new[] { "RingProximal", "RingIntermediate", "RingDistal" },
            new[] { "LittleProximal", "LittleIntermediate", "LittleDistal" },
        };
        static readonly string[] Tips = { "ThumbTip", "IndexTip", "MiddleTip", "RingTip", "LittleTip" };
        static readonly float[] CurlDegrees = { 70, 95, 65 };

        [Tooltip("Curl of thumb, index, middle, ring, little: 0 open, 1 closed.")] public float[] curl = new float[5];
        [Tooltip("Where the thumb tip reaches (world), and how far toward it (0 = not at all).")] public Vector3 thumbTarget; public float thumbReach;
        public float blendSpeed = 8;
        [Tooltip("1 a right hand, -1 a left hand, 0 from the object's name (Right / Left).")] public int hand;
        float Side() => hand != 0 ? Mathf.Sign(hand) : name.IndexOf("Left", StringComparison.OrdinalIgnoreCase) >= 0 ? -1 : 1;

        Transform wrist, palm;
        Transform[][] joints; Transform[] tips;
        Quaternion[][] rest; Vector3[][] axis;
        float[] now = new float[5]; float reachNow; Vector3 palmSide;
        bool ready;

        void Awake() => Setup();
        public void Setup()
        {
            if (ready) return;
            var all = GetComponentsInChildren<Transform>(true);
            Transform Find(string suffix) => Array.Find(all, t => t.name.EndsWith(suffix, StringComparison.Ordinal) && !t.name.EndsWith("_" + suffix + "_end", StringComparison.Ordinal));
            wrist = Find("Wrist"); palm = Find("Palm");
            joints = new Transform[5][]; tips = new Transform[5]; rest = new Quaternion[5][]; axis = new Vector3[5][];
            for (int f = 0; f < 5; f++)
            {
                joints[f] = Array.ConvertAll(FingerJoints[f], Find); tips[f] = Find(Tips[f]);
                rest[f] = Array.ConvertAll(joints[f], j => j ? j.localRotation : Quaternion.identity);
            }
            if (!wrist || joints[1][0] == null || joints[4][0] == null || joints[2][0] == null) { Debug.LogWarning("DemoHand: joints not found on " + name); return; }
            // the palm's side, from which hand this is: with the fingers along `along` and `across` from the little finger to
            // the index, a right palm faces Cross(along, across) and a left palm the other way. (The thumb's base is not a
            // safe guide: on the right model it sits a touch to the back of the knuckles' plane, and the hand came out
            // turned over, its fingers curling backwards.)
            Vector3 across = (joints[1][0].position - joints[4][0].position).normalized, along = (joints[2][0].position - wrist.position).normalized;
            Vector3 normal = Vector3.Cross(along, across).normalized * Side();
            palmSide = normal;
            // each finger joint's curl axis, in its own rest space, signed so a positive curl moves the tip palmward
            for (int f = 1; f < 5; f++)
            {
                axis[f] = new Vector3[3];
                for (int k = 0; k < 3; k++)
                {
                    var j = joints[f][k]; if (!j) continue;
                    var a = j.InverseTransformDirection(across);
                    var tip = tips[f] ? tips[f] : j;
                    var before = tip.position; j.localRotation = rest[f][k] * Quaternion.AngleAxis(10, a); var after = tip.position; j.localRotation = rest[f][k];
                    axis[f][k] = Vector3.Dot(after - before, normal) >= 0 ? a : -a;
                }
            }
            ready = true;
        }

        // Put the hand so its fingers point along `forward` and the back of the hand faces `back`.
        public void Aim(Vector3 forward, Vector3 back)
        {
            Setup(); if (!ready) return;
            Vector3 along = (joints[2][0].position - wrist.position).normalized, across = (joints[1][0].position - joints[4][0].position).normalized;
            var dorsal = -Vector3.Cross(along, across).normalized * Side();   // opposite the palm (Setup)
            var have = Quaternion.LookRotation(along, dorsal); var want = Quaternion.LookRotation(forward, back);
            transform.rotation = want * Quaternion.Inverse(have) * transform.rotation;
        }

        public void Pose(float thumb, float index, float middle, float ring, float little) { curl[0] = thumb; curl[1] = index; curl[2] = middle; curl[3] = ring; curl[4] = little; }
        public Transform Wrist => wrist;
        public Transform Tip(int finger) => tips != null && finger < tips.Length ? tips[finger] : null;
        public Transform Joint(int finger, int k) => joints != null ? joints[finger][k] : null;

        void LateUpdate() => Apply(Time.deltaTime);
        public void Apply(float dt)
        {
            if (!ready) return;
            float b = dt <= 0 ? 1 : 1 - Mathf.Exp(-dt * blendSpeed);
            for (int f = 0; f < 5; f++) now[f] = Mathf.Lerp(now[f], Mathf.Clamp01(curl[f]), b);
            reachNow = Mathf.Lerp(reachNow, Mathf.Clamp01(thumbReach), b);
            for (int f = 1; f < 5; f++)
                for (int k = 0; k < 3; k++) { var j = joints[f][k]; if (j) j.localRotation = rest[f][k] * Quaternion.AngleAxis(now[f] * CurlDegrees[k], axis[f][k]); }
            // the thumb: back to rest, folded in by its curl toward the palm's middle, then reaching for the target
            for (int k = 0; k < 3; k++) if (joints[0][k]) joints[0][k].localRotation = rest[0][k];
            var inward = (joints[2][0].position + joints[2][1].position) * .5f;
            Reach(Vector3.Lerp(tips[0].position, inward, now[0] * .8f), 1);
            if (reachNow > .001f) Reach(thumbTarget, reachNow);
        }
        void Reach(Vector3 target, float weight)
        {
            var tip = tips[0]; if (!tip) return;
            for (int pass = 0; pass < 3; pass++)
                for (int k = 2; k >= 0; k--)
                {
                    var j = joints[0][k]; if (!j) continue;
                    Vector3 toTip = tip.position - j.position, toTarget = target - j.position;
                    if (toTip.sqrMagnitude < 1e-8f || toTarget.sqrMagnitude < 1e-8f) continue;
                    var turn = Quaternion.FromToRotation(toTip, toTarget);
                    j.rotation = Quaternion.Slerp(Quaternion.identity, turn, weight * .6f) * j.rotation;
                }
        }
    }
}
