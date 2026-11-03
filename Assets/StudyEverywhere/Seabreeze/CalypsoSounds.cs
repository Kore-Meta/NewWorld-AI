using UnityEngine;

namespace StudyEverywhere
{
    // Calypso's two small sounds from the state language, made in code so there are no audio assets:
    //  - Chime: a soft rising two-note glass chime when she hears her name (she is listening).
    //  - Shimmer: a faint, seamless high shimmer, looped under her while she thinks.
    public static class CalypsoSounds
    {
        const int Rate = 44100;
        static AudioClip chime, shimmer;

        public static AudioClip Chime
        {
            get
            {
                if (chime) return chime;
                chime = Make("Calypso chime", .9f, t =>
                {
                    float v = Bell(t, 1318.5f, 0) * .55f + Bell(t, 1975.5f, .11f) * .45f;   // E6 then B6
                    return v * .5f;
                });
                return chime;
            }
        }

        public static AudioClip Shimmer
        {
            get
            {
                if (shimmer) return shimmer;
                // whole-hertz partials and a 0.5 Hz swell: every part completes its cycles in 2 s, so the loop has no seam
                shimmer = Make("Calypso shimmer", 2, t =>
                {
                    float swell = .6f + .4f * Mathf.Sin(2 * Mathf.PI * .5f * t);
                    float v = Mathf.Sin(2 * Mathf.PI * 2093 * t) * .5f + Mathf.Sin(2 * Mathf.PI * 2637 * t) * .3f * (.5f + .5f * Mathf.Sin(2 * Mathf.PI * 1.5f * t))
                            + Mathf.Sin(2 * Mathf.PI * 3136 * t) * .2f * (.5f + .5f * Mathf.Cos(2 * Mathf.PI * t));
                    return v * swell * .18f;
                });
                return shimmer;
            }
        }

        // A struck glass note: quick attack, long soft decay, a faint octave partial.
        static float Bell(float t, float hz, float start)
        {
            t -= start; if (t < 0) return 0;
            float env = Mathf.Min(1, t / .006f) * Mathf.Exp(-t * 5.5f);
            return env * (Mathf.Sin(2 * Mathf.PI * hz * t) + .25f * Mathf.Sin(2 * Mathf.PI * hz * 2.01f * t) * Mathf.Exp(-t * 6));
        }

        static AudioClip Make(string name, float seconds, System.Func<float, float> wave)
        {
            int n = Mathf.CeilToInt(seconds * Rate);
            var data = new float[n];
            for (int i = 0; i < n; i++) data[i] = wave(i / (float)Rate);
            var clip = AudioClip.Create(name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
