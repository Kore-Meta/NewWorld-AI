using System;
using UnityEngine;

namespace StudyEverywhere
{
    // The meadow's sounds, all kept very soft: a faint night-air bed, crickets hidden in the grass that chirp only now and
    // then (each on its own clock, sometimes resting a while), and a breeze every so often. While a breeze blows, the
    // grass bends with it: _RestGust follows the gust's swell for the Meadow Grass shader. One volume (wrist menu)
    // covers all of it and is remembered.
    public sealed class RestAmbience : MonoBehaviour
    {
        public AudioSource bed, breeze;
        [Tooltip("Spatial sources placed around the meadow; each plays the chirps.")]
        public AudioSource[] crickets;
        public AudioClip[] chirps;
        [Range(0, 1)] public float defaultVolume = 0.7f;
        [Range(0, 1)] public float bedLevel = 0.12f, cricketLevel = 0.12f, breezeLevel = 0.12f;
        [Tooltip("Seconds between one cricket's chirps.")]
        public Vector2 chirpInterval = new Vector2(36, 120);
        [Tooltip("Seconds between breezes.")]
        public Vector2 breezeInterval = new Vector2(30, 70);
        [Tooltip("Grass sway between gusts (0..1 of a full gust).")]
        [Range(0, 1)] public float calm = 0.08f;
        // set by Seabreeze's SeaShore: the surf bed fades out while Calypso holds the waves calm (1: as usual)
        [NonSerialized] public float bedScale = 1;

        const string VolumeKey = "StudyEverywhere.NatureVolume";
        static readonly int GustId = Shader.PropertyToID("_RestGust");
        float[] nextChirp;
        float nextBreeze, gust;

        public event Action Changed;
        public float Volume { get => PlayerPrefs.GetFloat(VolumeKey, defaultVolume); set { PlayerPrefs.SetFloat(VolumeKey, Mathf.Clamp01(value)); Changed?.Invoke(); } }

        void Start()
        {
            nextChirp = new float[crickets.Length];
            for (int i = 0; i < nextChirp.Length; i++) nextChirp[i] = Time.time + UnityEngine.Random.Range(15f, 180f);
            nextBreeze = Time.time + UnityEngine.Random.Range(4f, 10f);
            if (bed) { bed.loop = true; bed.Play(); }
        }

        void Update()
        {
            float v = Volume, now = Time.time;
            if (bed) bed.volume = bedLevel * v * bedScale;
            for (int i = 0; i < crickets.Length; i++)
            {
                if (!crickets[i] || chirps == null || chirps.Length == 0 || now < nextChirp[i]) continue;
                var c = crickets[i];
                c.pitch = UnityEngine.Random.Range(.95f, 1.05f);
                c.PlayOneShot(chirps[UnityEngine.Random.Range(0, chirps.Length)], cricketLevel * v * UnityEngine.Random.Range(.6f, 1f));
                // Sparse: now and then a chirp, and often a long silence.
                nextChirp[i] = now + (UnityEngine.Random.value < .5f ? UnityEngine.Random.Range(135f, 360f) : UnityEngine.Random.Range(chirpInterval.x, chirpInterval.y));
            }
            float target = calm;
            if (breeze && breeze.clip)
            {
                if (!breeze.isPlaying && now >= nextBreeze)
                {
                    breeze.pitch = UnityEngine.Random.Range(.9f, 1.1f);
                    breeze.panStereo = UnityEngine.Random.Range(-.4f, .4f);
                    breeze.Play();
                    nextBreeze = now + breeze.clip.length / breeze.pitch + UnityEngine.Random.Range(breezeInterval.x, breezeInterval.y);
                }
                breeze.volume = breezeLevel * v;
                // The clip swells as sin^2 over its length; the grass follows it, a little ahead so it moves as you hear it.
                if (breeze.isPlaying) target = Mathf.Max(calm, Mathf.Pow(Mathf.Sin(Mathf.PI * Mathf.Clamp01(breeze.time / breeze.clip.length + .05f)), 2));
            }
            gust = Mathf.MoveTowards(gust, target, Time.deltaTime * .8f);
            Shader.SetGlobalFloat(GustId, gust);
        }

        void OnDisable() { Shader.SetGlobalFloat(GustId, 0); PlayerPrefs.Save(); }
    }
}
