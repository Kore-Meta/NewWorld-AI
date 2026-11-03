using UnityEngine;

namespace StudyEverywhere
{
    // The shape of Seabreeze's beach, shared by the builder (the sand and water meshes) and the life on it (hatchlings,
    // crabs, the writing patch, the hands in the water). The sea lies toward +Z; at rest the waterline curves round a
    // shallow bay, its middle at z = 1 in front of the shore spawn. Landward the sand rises up the beach face and levels
    // into the dry upper beach where the pavilion stands; seaward it shelves gently into the lagoon, whose floor levels at 4 m.
    public static class SeabreezeBeach
    {
        // z of the waterline at rest (sand height 0) at x: the bay's curve, the headlands coming forward on both sides.
        public static float Shore(float x) => 1f + .0055f * x * x + .0000035f * x * x * x * x;

        // Sand height (m) at a point; negative under the lagoon.
        public static float Sand(float x, float z)
        {
            float d = z - Shore(x);
            if (d >= 0) return Mathf.Max(-4, -.06f * d - .0035f * d * d);   // the lagoon floor levels out at 4 m
            float up = -d;
            float h = .03f * up + .175f * (1 - Mathf.Exp(-up / 5));
            // the upper beach is a little uneven (old wind ripples and footprints smoothed out)
            h += .07f * Mathf.SmoothStep(0, 1, Mathf.InverseLerp(6, 12, up)) * (Mathf.PerlinNoise(x * .21f + 3.7f, z * .21f + 1.3f) - .5f);
            return h;
        }

        public static Vector3 OnSand(float x, float z) => new Vector3(x, Sand(x, z), z);

        // Normal of the sand at a point (for setting things down on it).
        public static Vector3 Normal(float x, float z)
        {
            const float e = .05f;
            return new Vector3(Sand(x - e, z) - Sand(x + e, z), 2 * e, Sand(x, z - e) - Sand(x, z + e)).normalized;
        }
    }
}
