using System;
using UnityEngine;

namespace StudyEverywhere
{
    // The meadow's fireflies: always drifting over the grass, and every few minutes a few more rise for a while and
    // settle again. They can be switched off from the wrist menu (remembered); switching back on brings the whole
    // field back at once rather than letting it fill up slowly.
    public sealed class Fireflies : MonoBehaviour
    {
        public ParticleSystem swarm;
        [Tooltip("Fireflies born per second the rest of the time.")]
        public float steadyRate = 3;
        [Tooltip("Fireflies born per second at the height of a swell.")]
        public float swellRate = 8;
        [Tooltip("Seconds from one swell to the next.")]
        public float swellEvery = 300;
        [Tooltip("How long a swell lasts, rising and falling.")]
        public float swellSeconds = 50;

        const string Key = "StudyEverywhere.Fireflies";
        float swellStart;

        public event Action Changed;
        public bool On { get => PlayerPrefs.GetInt(Key, 1) == 1; set { PlayerPrefs.SetInt(Key, value ? 1 : 0); Apply(); Changed?.Invoke(); } }

        void Start()
        {
            swellStart = Time.time + swellEvery;
            Apply();
        }

        void Apply()
        {
            if (!swarm) return;
            if (On && !swarm.isPlaying) { swarm.Simulate(20, true, true); swarm.Play(); }
            else if (!On && swarm.isPlaying) swarm.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        void Update()
        {
            if (!swarm || !On) return;
            float t = Time.time - swellStart;
            if (t > swellSeconds) { swellStart += swellEvery; t = Time.time - swellStart; }
            float swell = t >= 0 && t <= swellSeconds ? Mathf.Pow(Mathf.Sin(Mathf.PI * t / swellSeconds), 2) : 0;
            var emission = swarm.emission;
            emission.rateOverTime = Mathf.Lerp(steadyRate, swellRate, swell);
        }

        void OnDisable() => PlayerPrefs.Save();
    }
}
