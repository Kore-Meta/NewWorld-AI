using UnityEngine;

namespace StudyEverywhere
{
    // Little lives that keep the hours: butterflies over the beds by day, fireflies once it is dark. Follows a place's
    // TimeOfDay (its Evening: 0 by day, about .4 at sunset, 1 at night), starting and stopping the particle systems.
    public sealed class DayAndNightLife : MonoBehaviour
    {
        public TimeOfDay time;
        [Tooltip("Played while the evening is below dayUntil.")]
        public ParticleSystem[] day;
        [Tooltip("Played while the evening is above nightFrom.")]
        public ParticleSystem[] night;
        [Range(0, 1)] public float dayUntil = .3f, nightFrom = .55f;

        bool? wasDay, wasNight;

        void OnEnable() => Refresh();
        // Starts or stops the effects for the clock as it is now (also used by the builders' preview renders).
        public void Refresh() { wasDay = wasNight = null; Update(); }

        void Update()
        {
            if (!time) return;
            float e = time.Evening;
            bool d = e < dayUntil, n = e > nightFrom;
            if (wasDay != d) { Set(day, d); wasDay = d; }
            if (wasNight != n) { Set(night, n); wasNight = n; }
        }

        static void Set(ParticleSystem[] systems, bool on)
        {
            if (systems == null) return;
            foreach (var ps in systems)
            {
                if (!ps) continue;
                if (on && !ps.isPlaying) { ps.Simulate(8, true, true); ps.Play(true); }
                else if (!on && ps.isPlaying) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }
}
