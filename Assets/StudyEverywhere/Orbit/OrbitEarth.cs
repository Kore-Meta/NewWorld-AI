using UnityEngine;

namespace StudyEverywhere
{
    // The Earth in Orbit's big window: a real globe (NASA's Blue Marble, Black Marble city lights and a cloud layer,
    // StudyEverywhere/Orbit Earth) turning on its tilted axis, with the clouds drifting a little faster, inside a shell
    // of air (StudyEverywhere/Orbit Atmosphere). It is lit by the true sun for each time of day, blended with the same
    // keys and weights as the library's TimeOfDay, so it agrees with the baked sky: half-lit in the morning, full in the
    // afternoon, a crescent at sunset, and at night the sun behind it, its cities glittering inside a ring of light.
    [ExecuteAlways]
    public sealed class OrbitEarth : MonoBehaviour
    {
        public TimeOfDay time;
        [Tooltip("The globe (spins) and the air shell (does not).")] public Renderer globe, atmosphere;
        [Tooltip("The Moon: lit by the same sun (it does not turn; its near side faces us).")] public Renderer moon;
        [Tooltip("Toward the sun at each of TimeOfDay's keys, as azimuth / elevation in degrees (same order as its keys).")]
        public Vector2[] sunAzEl = { new Vector2(-100, 15), new Vector2(178, 12), new Vector2(62, 4), new Vector2(4, -3) };
        [Tooltip("The Earth's north pole, in the world.")] public Vector3 axis = new Vector3(.32f, .93f, -.18f);
        [Tooltip("Seconds per turn: the real Earth takes a day; here it turns gently enough to watch.")] public float secondsPerTurn = 240;
        [Tooltip("The clouds gain this fraction of a turn per turn of the ground.")] public float cloudDrift = .08f;
        [Tooltip("Longitude facing the library at the start (0 = Africa and Europe).")] public float startLongitude = 15;
        [Tooltip("1 shown, 0 faded away (the holodeck fades the Earth and the Moon while a NASA picture fills the window).")]
        [Range(0, 1)] public float visible = 1;

        static readonly int SunDir = Shader.PropertyToID("_SunDir"), CloudShift = Shader.PropertyToID("_CloudShift"), Exposure = Shader.PropertyToID("_Exposure"), Strength = Shader.PropertyToID("_Strength");
        MaterialPropertyBlock block;
        float turn;   // degrees

        // The longitude facing the library is -turn; turn grows, so the face we see moves west, as the real Earth turns east.
        void OnEnable() { block ??= new MaterialPropertyBlock(); turn = -startLongitude; Refresh(); }
        // Pose and light now (the builder's previews, or after the clock changes outside Play mode).
        public void Refresh() { Pose(); Light(); }

        void Update()
        {
            if (Application.isPlaying && secondsPerTurn > 0) turn = Mathf.Repeat(turn + 360 * Time.deltaTime / secondsPerTurn, 360);
            Refresh();
        }

        // Tilt the globe's up onto the axis, then turn it about that axis; the mesh's longitude 0 faces -Z.
        void Pose()
        {
            if (!globe) return;
            var tilt = Quaternion.FromToRotation(Vector3.up, axis.normalized);
            globe.transform.rotation = tilt * Quaternion.AngleAxis(-turn, Vector3.up);
        }

        void Light()
        {
            var sun = Sun();
            block ??= new MaterialPropertyBlock();
            if (globe)
            {
                globe.GetPropertyBlock(block);
                block.SetVector(SunDir, sun);
                block.SetFloat(CloudShift, Mathf.Repeat(-turn / 360f * cloudDrift, 1));
                block.SetFloat(Exposure, Base(globe, Exposure) * visible);
                globe.SetPropertyBlock(block);
                Show(globe);
            }
            if (moon)
            {
                moon.GetPropertyBlock(block);
                block.SetVector(SunDir, sun);
                block.SetFloat(Exposure, Base(moon, Exposure) * visible);
                moon.SetPropertyBlock(block);
                Show(moon);
            }
            if (atmosphere)
            {
                atmosphere.GetPropertyBlock(block);
                block.SetVector(SunDir, sun);
                block.SetFloat(Strength, Base(atmosphere, Strength) * visible);
                atmosphere.SetPropertyBlock(block);
                Show(atmosphere);
            }
        }

        // the material's own value, scaled by `visible`; faded right out, the renderer is switched off so the stars show through
        static float Base(Renderer r, int id) => r.sharedMaterial && r.sharedMaterial.HasProperty(id) ? r.sharedMaterial.GetFloat(id) : 1;
        void Show(Renderer r) { bool on = visible > .01f; if (r.enabled != on) r.enabled = on; }

        Vector3 Sun()
        {
            if (!time || sunAzEl == null || sunAzEl.Length == 0) return Dir(sunAzEl != null && sunAzEl.Length > 0 ? sunAzEl[0] : new Vector2(160, 35));
            int a = Mathf.Clamp(time.KeyFrom, 0, sunAzEl.Length - 1), b = Mathf.Clamp(time.KeyTo, 0, sunAzEl.Length - 1);
            return Vector3.Slerp(Dir(sunAzEl[a]), Dir(sunAzEl[b]), time.KeyBlend).normalized;
        }
        static Vector3 Dir(Vector2 azEl)
        {
            float az = azEl.x * Mathf.Deg2Rad, el = azEl.y * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(az) * Mathf.Cos(el), Mathf.Sin(el), Mathf.Cos(az) * Mathf.Cos(el));
        }
    }
}
