using UnityEngine;
using UnityEngine.Rendering;

namespace StudyEverywhere
{
    // Time of day for the Paris destination: drives the Paris Sky shader (sky, clouds, water, city day/night),
    // the sun/moon light, ambient light and the café pendants. The baked city is lit for late afternoon from
    // the left, so the sun keeps that azimuth and only its height changes; other times tint that lighting.
    // Weather (Sunny, Cloudy, Rain) blends cloud cover, a softer greyer sky, dimmer sun and city on the same clock;
    // Rain exposes how wet it is (0..1) for the rain particles and sound.
    [ExecuteAlways]
    public sealed class ParisSky : MonoBehaviour
    {
        public enum Preset { Morning, Afternoon, Sunset, Night, Automatic }

        [Header("Time")]
        public Preset preset = Preset.Afternoon;
        [Tooltip("Clock hour used by Automatic. Advances while playing unless frozen.")]
        [Range(0, 24)] public float hour = 17.8f;
        [Tooltip("Real minutes for one full day in Automatic mode.")]
        [Min(1)] public float minutesPerDay = 30;
        [Tooltip("Seconds to blend when the preset changes. 0 switches instantly.")]
        [Min(0)] public float transitionSeconds = 4;
        [Tooltip("Stops the clock and suppresses transitions.")]
        public bool freeze;

        public enum Weather { Sunny, Cloudy, Rain }

        [Header("Clouds and weather")]
        public Weather weather = Weather.Sunny;
        [Tooltip("Cloud cover on a sunny day; Cloudy and Rain thicken it.")]
        [Range(0, 1)] public float cloudCover = 0.45f;
        [Tooltip("Cloud drift in noise tiles per second along world X and Z.")]
        public Vector2 cloudWind = new Vector2(0.0035f, 0.0012f);

        [Header("Scene")]
        public Light sun;
        public Light[] lamps;
        public float lampDayIntensity = 0.8f, lampNightIntensity = 2.0f;
        [Tooltip("Lights that come on only after dusk (the sign's spotlight, the lantern over the door); off by day.")]
        public Light[] nightLights;
        public float nightLightIntensity = 1.6f;
        [Tooltip("Each night light's own full intensity (overrides nightLightIntensity when set): far spots need more.")]
        public float[] nightLightLevels;
        [Tooltip("Lamp glass that glows while the night lights are on (its material's emission, scaled per renderer).")]
        public Renderer[] nightGlow;
        public Color glowColor = new Color(1f, .78f, .45f);
        [Tooltip("Painting canvases lit by their picture lamps, day and night, more after dusk (their own image as emission).")]
        public Renderer[] paintings;
        [Range(0, 1.5f)] public float paintingLight = .85f;
        [Tooltip("How much of paintingLight the picture lamps give by day (they stay on; the room is brighter then).")]
        [Range(0, 1)] public float paintingDay = .6f;

        [Header("Eiffel Tower at night")]
        [Tooltip("Strength of the slowly turning beacon beams (kept soft; 1 is the designed level).")]
        [Range(0, 1)] public float beacon = 1;
        [Tooltip("The tower sparkles for sparkleMinutes out of every sparkleEveryMinutes of real time, at night.")]
        [Min(1)] public float sparkleEveryMinutes = 10;
        [Min(0)] public float sparkleMinutes = 5;
        // The beacon and the sparkle together, switched from the wrist menu and remembered.
        const string TowerKey = "StudyEverywhere.TowerLights";
        public bool TowerLights { get => PlayerPrefs.GetInt(TowerKey, 1) == 1; set { PlayerPrefs.SetInt(TowerKey, value ? 1 : 0); dirty = true; } }

        // 0 by day, 1 at night: how far the evening lights are on (candles follow it too).
        public float Evening { get; private set; }

        const float SunAzimuth = -110, MoonAzimuth = 60, MoonElevation = 32;
        float current = -1, overcast = -1;
        bool dirty = true;
        MaterialPropertyBlock glowBlock, paintBlock;
        float nightNow, nightSince = -1;   // when night last began: each night opens with the sparkle
        static readonly int SparkleId = Shader.PropertyToID("_ParisSparkle");
        static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

        // 0 on a clear day, 1 in steady rain.
        public float Rain => Mathf.InverseLerp(0.6f, 1f, Mathf.Max(0, overcast));
        static float WeatherTarget(Weather w) => w switch { Weather.Cloudy => 0.6f, Weather.Rain => 1f, _ => 0f };

        public static float PresetHour(Preset p) => p switch { Preset.Morning => 9f, Preset.Sunset => 20.6f, Preset.Night => 23.5f, _ => 17.8f };  // 17:48 matches the baked sun (24 degrees)
        // Summer day: sunrise 06:00, sunset 21:00, the sun peaks at 40 degrees over this street.
        public static float SunElevation(float h)
        {
            h = Mathf.Repeat(h, 24);
            if (h >= 6 && h <= 21) return 40 * Mathf.Sin(Mathf.PI * (h - 6) / 15);
            float n = h > 21 ? h - 21 : h + 3;
            return -18 * Mathf.Sin(Mathf.PI * n / 9);
        }

        void OnEnable() { current = -1; overcast = -1; dirty = true; Apply(); }
        void OnValidate() => dirty = true;

        void Update()
        {
            if (preset == Preset.Automatic && Application.isPlaying && !freeze) { hour = Mathf.Repeat(hour + Time.deltaTime * 24 / (minutesPerDay * 60), 24); dirty = true; }
            if (Application.isPlaying && !freeze && transitionSeconds > 0 && preset != Preset.Automatic && current >= 0)
            {
                float delta = Mathf.Repeat(Target() - current + 12, 24) - 12;
                if (Mathf.Abs(delta) > 1e-3f) { current = Mathf.Repeat(current + delta * (1 - Mathf.Exp(-Time.deltaTime * 3 / transitionSeconds)), 24); dirty = true; }
            }
            if (Application.isPlaying && !freeze && transitionSeconds > 0 && overcast >= 0)
            {
                float delta = WeatherTarget(weather) - overcast;
                if (Mathf.Abs(delta) > 1e-3f) { overcast += delta * (1 - Mathf.Exp(-Time.deltaTime * 2 / transitionSeconds)); dirty = true; }
            }
            if (dirty) Apply();
            // The sparkle: on for the first few minutes of each cycle (counted from nightfall), easing in and out over five seconds.
            float sparkle = 0;
            if (Application.isPlaying && sparkleMinutes > 0 && TowerLights)
            {
                if (nightNow > .5f && nightSince < 0) nightSince = Time.time; else if (nightNow < .3f) nightSince = -1;
                float s = nightSince < 0 ? 0 : Mathf.Repeat(Time.time - nightSince, sparkleEveryMinutes * 60), on = sparkleMinutes * 60;
                sparkle = Mathf.Clamp01(Mathf.Min(s, on - s) / 5f) * Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.3f, .9f, nightNow));
            }
            Shader.SetGlobalFloat(SparkleId, sparkle);
        }

        float Target() => preset == Preset.Automatic ? hour : PresetHour(preset);

        public void Apply()
        {
            dirty = false;
            if (current < 0 || freeze || transitionSeconds <= 0 || !Application.isPlaying || preset == Preset.Automatic) current = Target();
            if (overcast < 0 || freeze || transitionSeconds <= 0 || !Application.isPlaying) overcast = WeatherTarget(weather);
            float wet = overcast;
            float el = SunElevation(current);
            float night = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(2, -8, el));
            float dusk = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(-6, 2, el)) * (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(4, 16, el)));
            Vector3 sunDir = Dir(SunAzimuth, el), moonDir = Dir(MoonAzimuth, MoonElevation);

            Color Blend(Color day, Color duskC, Color nightC) => Color.Lerp(Color.Lerp(day, duskC, dusk), nightC, night);
            Color Sky(Color clear, Color grey) => Color.Lerp(clear, grey, wet);
            Shader.SetGlobalVector("_ParisSunDir", sunDir);
            Shader.SetGlobalVector("_ParisMoonDir", moonDir);
            Shader.SetGlobalFloat("_ParisNight", night);
            nightNow = night;
            Shader.SetGlobalFloat("_ParisBeacon", (Application.isPlaying && !TowerLights ? 0 : beacon) * Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.3f, .9f, night)));
            Shader.SetGlobalFloat("_ParisDusk", dusk);
            Shader.SetGlobalColor("_ParisZenith", Sky(Blend(new Color(.06f, .18f, .55f), new Color(.1f, .12f, .3f), new Color(.004f, .007f, .02f)), Blend(new Color(.3f, .34f, .4f), new Color(.17f, .16f, .2f), new Color(.004f, .005f, .01f))));
            Shader.SetGlobalColor("_ParisHorizon", Sky(Blend(new Color(.62f, .66f, .72f), new Color(.95f, .5f, .22f), new Color(.03f, .035f, .06f)), Blend(new Color(.56f, .58f, .61f), new Color(.38f, .33f, .33f), new Color(.025f, .027f, .035f))));
            Shader.SetGlobalColor("_ParisSunColor", Blend(new Color(1, .93f, .82f), new Color(1, .55f, .25f), Color.black) * Mathf.Clamp01((el + 3) / 4) * (1 - 0.85f * wet));
            Shader.SetGlobalColor("_ParisCityTint", Color.Lerp(Color.white, new Color(.85f, .6f, .47f), dusk) * Color.Lerp(Color.white, new Color(.66f, .69f, .74f), wet));
            Shader.SetGlobalColor("_ParisCloudLit", Sky(Blend(new Color(1, .98f, .95f), new Color(1, .6f, .42f), new Color(.1f, .11f, .16f)), Blend(new Color(.66f, .68f, .72f), new Color(.42f, .38f, .4f), new Color(.05f, .055f, .07f))));
            Shader.SetGlobalColor("_ParisCloudShade", Sky(Blend(new Color(.42f, .47f, .56f), new Color(.33f, .24f, .3f), new Color(.02f, .025f, .04f)), Blend(new Color(.36f, .38f, .42f), new Color(.22f, .2f, .23f), new Color(.015f, .017f, .025f))));
            Shader.SetGlobalColor("_ParisWaterDeep", Blend(new Color(.012f, .028f, .024f), new Color(.02f, .018f, .02f), new Color(.002f, .003f, .004f)));
            Shader.SetGlobalFloat("_ParisCloudCover", Mathf.Lerp(cloudCover * 0.6f, 0.97f, wet));
            Shader.SetGlobalVector("_ParisCloudWind", cloudWind);

            if (sun)
            {
                bool moon = night > .5f;
                Vector3 d = moon ? moonDir : sunDir;
                sun.transform.rotation = Quaternion.LookRotation(-d);
                sun.color = moon ? new Color(.62f, .72f, 1) : Blend(new Color(1, .85f, .66f), new Color(1, .55f, .3f), Color.black);
                sun.intensity = (moon ? .08f * night : 1.45f * Mathf.SmoothStep(0, 1, Mathf.InverseLerp(-2, 10, el))) * (1 - 0.65f * wet);
                sun.shadowStrength = Mathf.Lerp(.8f, .3f, wet);
            }
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = Sky(Blend(new Color(.62f, .69f, .8f), new Color(.45f, .38f, .42f), new Color(.05f, .06f, .1f)), Blend(new Color(.55f, .58f, .63f), new Color(.38f, .35f, .37f), new Color(.045f, .05f, .07f)));
            RenderSettings.ambientEquatorColor = Blend(new Color(.6f, .55f, .46f), new Color(.6f, .42f, .3f), new Color(.06f, .05f, .05f));
            RenderSettings.ambientGroundColor = Blend(new Color(.27f, .24f, .2f), new Color(.2f, .15f, .12f), new Color(.03f, .025f, .02f));
            RenderSettings.reflectionIntensity = Mathf.Lerp(.55f, .12f, night);
            // Keep task lighting readable after dark.
            if (lamps != null) foreach (var l in lamps) if (l) l.intensity = Mathf.Lerp(lampDayIntensity, lampNightIntensity, Mathf.Max(night, dusk * .5f));
            // The sign's spotlight and the door lantern fade in through dusk; switched off by day so they cost nothing.
            float evening = Evening = Mathf.Max(night, dusk * .5f);
            if (nightLights != null) for (int i = 0; i < nightLights.Length; i++) { var l = nightLights[i]; if (!l) continue; float full = nightLightLevels != null && i < nightLightLevels.Length ? nightLightLevels[i] : nightLightIntensity; l.intensity = full * evening; l.enabled = evening > .01f; }
            if (nightGlow != null && nightGlow.Length > 0)
            {
                glowBlock ??= new MaterialPropertyBlock();
                glowBlock.SetColor(EmissionId, glowColor * (.7f + 1.65f * evening));   // lamps stay lit by day (the picture lamps are always on)
                foreach (var r in nightGlow) if (r) r.SetPropertyBlock(glowBlock);
            }
            if (paintings != null && paintings.Length > 0)
            {
                paintBlock ??= new MaterialPropertyBlock();
                paintBlock.SetColor(EmissionId, new Color(1f, .9f, .78f) * (paintingLight * Mathf.Lerp(paintingDay, 1, evening)));
                foreach (var r in paintings) if (r) r.SetPropertyBlock(paintBlock);
            }
        }

        static Vector3 Dir(float azimuth, float elevation)
        {
            float a = azimuth * Mathf.Deg2Rad, e = elevation * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(a) * Mathf.Cos(e), Mathf.Sin(e), Mathf.Cos(a) * Mathf.Cos(e));
        }
    }
}
