using System;
using UnityEngine;

namespace StudyEverywhere
{
    // The Starry Meadow's night: drives the Night Sky shader (moon and its phase, clouds, Milky Way, shooting stars)
    // and the moonlight and ambient light, which follow how much of the moon is lit and how high it is. The moon can
    // stay still, rise and set very slowly, or be switched off for stars alone. The phase, motion and clouds are
    // chosen from the wrist menu, eased in rather than switched, and remembered between visits.
    [ExecuteAlways]
    public sealed class NightSky : MonoBehaviour
    {
        public enum MoonPhase { New, WaxingCrescent, FirstQuarter, WaxingGibbous, Full, WaningGibbous, LastQuarter, WaningCrescent }
        public enum Clouds { Clear, Few, Cloudy }
        public enum MoonMotion { Still, RiseAndSet, Off }

        public static readonly string[] MotionNames = { "Still", "Rise and set", "Off" };
        public static readonly string[] PhaseNames = { "New moon", "Waxing crescent", "First quarter", "Waxing gibbous", "Full moon", "Waning gibbous", "Last quarter", "Waning crescent" };

        [Header("Moon")]
        public MoonPhase phase = MoonPhase.WaxingGibbous;
        [Tooltip("Degrees clockwise from the arrival view (world +Z).")]
        public float moonAzimuth = 18;
        public float moonElevation = 34;
        [Tooltip("Apparent radius in degrees (the real moon is 0.26; a little larger reads better in a headset).")]
        public float moonRadius = 1.1f;
        [Tooltip("Tilt of the lit side, in degrees.")]
        public float terminatorTilt = 28;
        [Tooltip("Still: the moon stays where it is set above. Rise and set: it arcs slowly across the sky through that spot, sinks behind the trees, and rises again later. Off: stars only.")]
        public MoonMotion motion = MoonMotion.Still;
        [Tooltip("Real minutes the moon is up, from rising to setting, when it moves.")]
        [Min(1)] public float minutesUp = 60;
        [Tooltip("Real minutes it stays below the horizon before rising again.")]
        [Min(0)] public float minutesDown = 10;
        [Header("Clouds")]
        public Clouds clouds = Clouds.Few;
        [Tooltip("Cloud drift in noise tiles per second along world X and Z.")]
        public Vector2 cloudWind = new Vector2(0.0022f, 0.0009f);
        [Header("Milky Way")]
        [Tooltip("Where its bright core sits low over the horizon (azimuth, elevation in degrees).")]
        public Vector2 milkyWayCore = new Vector2(-48, 8);
        [Tooltip("A second point the band passes through, which sets how it arches over the sky.")]
        public Vector2 milkyWayThrough = new Vector2(95, 62);
        [Header("Shooting stars")]
        [Tooltip("Seconds between shooting stars, at random within this range.")]
        public Vector2 meteorInterval = new Vector2(7, 22);
        [Header("Scene")]
        public Light moonlight;
        public float moonlightNew = 0.12f, moonlightFull = 0.8f;
        public Color ambientSky = new Color(.1f, .14f, .2f), ambientEquator = new Color(.07f, .12f, .1f), ambientGround = new Color(.02f, .035f, .02f);
        [Tooltip("Seconds to ease to a new phase or cloud cover.")]
        public float transitionSeconds = 3;

        const string PhaseKey = "StudyEverywhere.MoonPhase", CloudsKey = "StudyEverywhere.NightClouds", MotionKey = "StudyEverywhere.MoonMotion";
        static readonly int MoonDirId = Shader.PropertyToID("_NightMoonDir"), MoonPhaseId = Shader.PropertyToID("_NightMoonPhase"), CloudsId = Shader.PropertyToID("_NightClouds"),
            MWNormalId = Shader.PropertyToID("_NightMWNormal"), MWCoreId = Shader.PropertyToID("_NightMWCore"), MeteorAId = Shader.PropertyToID("_NightMeteorA"), MeteorBId = Shader.PropertyToID("_NightMeteorB");

        float shownPhase = -1, shownCover = -1, shownMoon = -1;
        float arc = .5f;   // 0 rising, .5 at the home spot, 1 setting; past 1 it is below the horizon until it rises again
        Vector2 cloudOffset;
        float nextMeteor = -1, meteorStart, meteorDuration;
        Vector3 meteorA, meteorB;
        bool meteorActive;

        public event Action Changed;

        // 0 at new moon, 1 at full.
        public float LitFraction => 0.5f * (1 - Mathf.Cos(PhaseAngle(shownPhase < 0 ? (float)phase : shownPhase)));
        static float PhaseAngle(float p) => p / 8f * Mathf.PI * 2;
        static float Cover(Clouds c) => c switch { Clouds.Clear => 0, Clouds.Cloudy => .62f, _ => .36f };

        void Awake()
        {
            if (!Application.isPlaying) return;
            if (PlayerPrefs.HasKey(PhaseKey)) phase = (MoonPhase)Mathf.Clamp(PlayerPrefs.GetInt(PhaseKey), 0, 7);
            if (PlayerPrefs.HasKey(CloudsKey)) clouds = (Clouds)Mathf.Clamp(PlayerPrefs.GetInt(CloudsKey), 0, 2);
            if (PlayerPrefs.HasKey(MotionKey)) motion = (MoonMotion)Mathf.Clamp(PlayerPrefs.GetInt(MotionKey), 0, 2);
        }

        void OnEnable() { shownPhase = -1; shownCover = -1; shownMoon = -1; Apply(); }

        public void SetPhase(MoonPhase p) { phase = p; if (Application.isPlaying) PlayerPrefs.SetInt(PhaseKey, (int)p); Changed?.Invoke(); }
        public void StepPhase(int delta) => SetPhase((MoonPhase)(((int)phase + delta + 8) % 8));
        public void SetClouds(Clouds c) { clouds = c; if (Application.isPlaying) PlayerPrefs.SetInt(CloudsKey, (int)c); Changed?.Invoke(); }
        public void SetMotion(MoonMotion m) { motion = m; if (Application.isPlaying) PlayerPrefs.SetInt(MotionKey, (int)m); Changed?.Invoke(); }

        static Vector3 Dir(float azimuth, float elevation)
        {
            float a = azimuth * Mathf.Deg2Rad, e = elevation * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(a) * Mathf.Cos(e), Mathf.Sin(e), Mathf.Cos(a) * Mathf.Cos(e));
        }

        // Where the moon is now. Its rising-and-setting arc runs from 95 degrees left of the home spot to 95 degrees
        // right of it, peaking at the home spot, and dips just under the horizon at both ends (behind the trees).
        public float MoonAzimuthNow => moonAzimuth + Mathf.Lerp(-95, 95, Mathf.Clamp01(arc));
        public float MoonElevationNow => arc <= 1 ? (moonElevation + 4) * Mathf.Sin(Mathf.PI * Mathf.Clamp01(arc)) - 4 : -4;
        public Vector3 MoonDirection => Dir(MoonAzimuthNow, MoonElevationNow);

        void Update()
        {
            float dt = Application.isPlaying ? Time.deltaTime : 0;
            cloudOffset += cloudWind * dt;
            if (cloudOffset.x > 1000) cloudOffset = Vector2.zero;
            if (Application.isPlaying) { UpdateMeteor(); UpdateMoonArc(dt); }
            Apply(dt);
        }

        // Rise and set walks the arc (and below the horizon for a while); Still glides back to the home spot; Off
        // leaves it where it is, unseen.
        void UpdateMoonArc(float dt)
        {
            if (motion == MoonMotion.RiseAndSet)
            {
                arc += dt / (minutesUp * 60);
                if (arc > 1 + minutesDown / minutesUp) arc = 0;
            }
            else if (motion == MoonMotion.Still && !Mathf.Approximately(arc, .5f))
            {
                if (arc > 1) arc = Mathf.Min(arc, 1);   // still below the trees: come back up from the setting side
                arc = Mathf.MoveTowards(arc, .5f, dt / Mathf.Max(.5f, transitionSeconds * 2));
            }
        }

        // Pushes everything to the shader and lights; dt 0 jumps straight to the chosen phase and clouds.
        public void Apply(float dt = 0)
        {
            float k = dt > 0 && transitionSeconds > 0 ? dt / transitionSeconds * 8 : 8;
            // Ease the phase the short way round the cycle.
            float target = (float)phase;
            if (shownPhase < 0) shownPhase = target;
            float diff = Mathf.Repeat(target - shownPhase + 4, 8) - 4;
            shownPhase = Mathf.Repeat(shownPhase + Mathf.Clamp(diff, -k, k), 8);
            float coverTarget = Cover(clouds);
            if (shownCover < 0) shownCover = coverTarget;
            shownCover = Mathf.MoveTowards(shownCover, coverTarget, k / 8);

            // The moon fades out when switched off (and back in), and its light fades as it sinks to the horizon.
            float showTarget = motion == MoonMotion.Off ? 0 : 1;
            if (shownMoon < 0) shownMoon = showTarget;
            shownMoon = Mathf.MoveTowards(shownMoon, showTarget, k / 8);
            float aboveTrees = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(-3, 6, MoonElevationNow));

            float angle = PhaseAngle(shownPhase), lit = 0.5f * (1 - Mathf.Cos(angle));
            var moon = MoonDirection;
            Shader.SetGlobalVector(MoonDirId, new Vector4(moon.x, moon.y, moon.z, moonRadius * Mathf.Deg2Rad));
            Shader.SetGlobalVector(MoonPhaseId, new Vector4(angle, lit, terminatorTilt * Mathf.Deg2Rad, shownMoon));
            lit *= shownMoon * aboveTrees;   // from here on: how much moonlight actually reaches the meadow
            Shader.SetGlobalVector(CloudsId, new Vector4(shownCover, cloudOffset.x, cloudOffset.y, 0));
            var core = Dir(milkyWayCore.x, milkyWayCore.y);
            var normal = Vector3.Cross(core, Dir(milkyWayThrough.x, milkyWayThrough.y)).normalized;
            Shader.SetGlobalVector(MWNormalId, normal);
            Shader.SetGlobalVector(MWCoreId, Vector3.Cross(normal, Vector3.Cross(core, normal)).normalized);
            if (!meteorActive) Shader.SetGlobalVector(MeteorBId, Vector4.zero);

            float light = Mathf.Lerp(moonlightNew, moonlightFull, lit) * (1 - 0.45f * shownCover);
            if (moonlight)
            {
                // Never from below: a low or set moon still lights from a little above the horizon (at starlight strength).
                moonlight.transform.rotation = Quaternion.LookRotation(-Dir(MoonAzimuthNow, Mathf.Max(MoonElevationNow, 12)));
                moonlight.intensity = light;
            }
            float amb = Mathf.Lerp(.75f, 1.25f, lit);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = ambientSky * amb; RenderSettings.ambientEquatorColor = ambientEquator * amb; RenderSettings.ambientGroundColor = ambientGround * amb;
        }

        // ---- shooting stars: a short bright streak high in the sky, now and then, travelling downwards
        void UpdateMeteor()
        {
            float now = Time.time;
            if (nextMeteor < 0) nextMeteor = now + UnityEngine.Random.Range(3f, 8f);
            if (!meteorActive && now >= nextMeteor)
            {
                float az = UnityEngine.Random.Range(-150f, 150f), el = UnityEngine.Random.Range(35f, 75f);
                float heading = UnityEngine.Random.Range(0f, 360f), length = UnityEngine.Random.Range(9f, 18f);
                ShowMeteor(Dir(az, el), Dir(az + Mathf.Sin(heading * Mathf.Deg2Rad) * length, el - Mathf.Abs(Mathf.Cos(heading * Mathf.Deg2Rad)) * length * 0.8f - 4), UnityEngine.Random.Range(.55f, 1.1f));
            }
            if (!meteorActive) return;
            float t = (now - meteorStart) / meteorDuration;
            if (t >= 1) { meteorActive = false; nextMeteor = now + UnityEngine.Random.Range(meteorInterval.x, meteorInterval.y); Shader.SetGlobalVector(MeteorBId, Vector4.zero); return; }
            SetMeteor(t);
        }

        // Starts a shooting star from a to b (directions) lasting the given seconds.
        public void ShowMeteor(Vector3 a, Vector3 b, float seconds)
        {
            meteorA = a.normalized; meteorB = b.normalized; meteorDuration = Mathf.Max(.1f, seconds); meteorStart = Time.time; meteorActive = true;
            SetMeteor(0);
        }

        // For previews: the streak at progress t, without the timer.
        public void SetMeteor(float t)
        {
            float brightness = Mathf.Sin(Mathf.PI * Mathf.Clamp01(t)) * (1 - 0.3f * shownCover);
            Shader.SetGlobalVector(MeteorAId, new Vector4(meteorA.x, meteorA.y, meteorA.z, Mathf.Clamp01(t)));
            Shader.SetGlobalVector(MeteorBId, new Vector4(meteorB.x, meteorB.y, meteorB.z, Mathf.Max(brightness, 1e-3f)));
        }
        public void PreviewMeteor(Vector3 a, Vector3 b, float t) { meteorA = a.normalized; meteorB = b.normalized; meteorActive = true; SetMeteor(t); }
        public void ClearMeteor() { meteorActive = false; Shader.SetGlobalVector(MeteorBId, Vector4.zero); }

        void OnDisable() => Shader.SetGlobalVector(MeteorBId, Vector4.zero);
    }
}
