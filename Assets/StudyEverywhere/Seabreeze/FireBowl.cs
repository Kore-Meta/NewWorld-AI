using UnityEngine;

namespace StudyEverywhere
{
    // Seabreeze's fire bowl: flames, embers and a soft crackle, and a warm light that flickers with them. It burns while
    // the clock's lights are on (the menu's Fire row is TimeOfDay.LampsOn); TimeOfDay sets the light's strength for the
    // hour (faint by day, carrying the beach at night) and this only makes it flicker.
    public sealed class FireBowl : MonoBehaviour
    {
        public TimeOfDay time;
        public ParticleSystem[] flames;
        public AudioSource crackle;
        public Light glow;
        [Range(0, 1)] public float flicker = .22f;
        [Range(0, 1)] public float crackleLevel = .35f;
        bool lit = true;
        float seed;

        void Start() { seed = Random.value * 100; Set(time ? time.LampsOn : true, true); }

        void Set(bool on, bool force = false)
        {
            if (on == lit && !force) return;
            lit = on;
            foreach (var f in flames) if (f) { if (on) f.Play(true); else f.Stop(true, ParticleSystemStopBehavior.StopEmitting); }
            if (crackle) { if (on) { if (!crackle.isPlaying) crackle.Play(); } else crackle.Stop(); }
        }

        void LateUpdate()
        {
            Set(time ? time.LampsOn : true);
            if (crackle) crackle.volume = crackleLevel * PlayerPrefs.GetFloat("StudyEverywhere.NatureVolume", .7f);
            if (glow && lit)
            {
                float t = Time.time * 7 + seed;
                glow.intensity *= 1 - flicker * (.5f + .5f * Mathf.PerlinNoise(t, seed) * Mathf.PerlinNoise(t * 2.3f, seed + 7));
            }
        }
    }
}
