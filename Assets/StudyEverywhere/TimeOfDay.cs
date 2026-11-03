using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace StudyEverywhere
{
    // Day and night for a place with a baked sky (Haven, and the libraries and cafés after it), on the café's clock:
    // the same presets as ParisSky (Morning 9:00, Afternoon 17:48, Sunset 20:36, Night 23:30, or Automatic, where the
    // clock runs a whole day in minutesPerDay). Each key is one baked cubemap with its sun (or moon), ambient light and
    // how far the room's lamps are up; between keys the Blended Sky shader cross-fades the two skies and everything else
    // is interpolated, so a new preset eases in over transitionSeconds and Automatic glides through dusk and dawn.
    // The reading lamps can also be switched off from the menu (remembered between sessions under lampKey).
    [ExecuteAlways]
    public sealed class TimeOfDay : MonoBehaviour
    {
        public enum Preset { Morning, Afternoon, Sunset, Night, Automatic }

        [Serializable]
        public struct Key
        {
            public string name;
            [Range(0, 24)] public float hour;
            public Cubemap sky;
            [Tooltip("Degrees above the horizon and azimuth (from +Z toward +X) of the sun, or the moon at night: as baked into the sky.")]
            public float elevation, azimuth;
            public Color sunColor;
            public float sunIntensity;
            public Color ambientSky, ambientEquator, ambientGround;
            [Range(0, 1)] public float reflection;
            [Tooltip("Lamps and fills as a share of their full intensity.")]
            [Range(0, 1)] public float lamps, fills;
            [Tooltip("0 by day, 1 at night (dark menus, glowing lamps).")]
            [Range(0, 1)] public float night;
        }

        [Header("Time")]
        public Preset preset = Preset.Afternoon;
        [Tooltip("Clock hour used by Automatic. Advances while playing unless frozen.")]
        [Range(0, 24)] public float hour = 17.8f;
        [Min(1)] public float minutesPerDay = 30;
        [Min(0)] public float transitionSeconds = 4;
        public bool freeze;
        [Tooltip("In order of hour; the last wraps round to the first.")]
        public Key[] keys;

        [Header("Scene")]
        [Tooltip("A StudyEverywhere/Blended Sky material.")]
        public Material sky;
        public Light sun;
        public Light[] fills;
        public float fillIntensity = .8f;
        public Light[] lamps;
        public float lampIntensity = 1.3f;
        [Tooltip("The lamps' glowing shades: lit material while on, the unlit one while off.")]
        public Renderer[] lampGlass;
        public Material glassOn, glassOff;
        public string lampKey = "StudyEverywhere.Lamps";
        [Tooltip("With the lights on, the fills are at least this bright (times fillIntensity) and this is added to the ambient light: the room itself is lit, not only the lamps.")]
        public float roomFill;
        public Color roomAmbient = Color.black;
        [Tooltip("Lights that light the room itself (overhead, unseen; Solace's rotunda): on with the lamps at roomLightIntensity, never below roomLightMin of it by day, full at night.")]
        public Light[] roomLights;
        public float roomLightIntensity = 1, roomLightMin = .5f;
        [Tooltip("Soft pools of lamp light (additive quads on desks, floors and walls): shown with the lamps, as bright as the lamps are.")]
        public Renderer[] lampPools;
        public Color poolColour = new Color(1, .7f, .4f, 1);
        [Tooltip("Lamp shades and lanterns with their own lit and unlit materials (each renderer swaps between its pair).")]
        public LampSkin[] skins;
        [Serializable] public struct LampSkin { public Renderer renderer; public Material on, off; }

        public static float PresetHour(Preset p) => ParisSky.PresetHour((ParisSky.Preset)(int)p);
        public float Evening { get; private set; }
        public float Current => current;
        // The pair of keys the clock is between and how far from the first (for things lit to match, such as Orbit's Earth).
        public int KeyFrom { get; private set; }
        public int KeyTo { get; private set; }
        public float KeyBlend { get; private set; }
        public event Action Changed;
        public bool LampsOn { get => PlayerPrefs.GetInt(lampKey, 1) == 1; set { PlayerPrefs.SetInt(lampKey, value ? 1 : 0); Apply(); Changed?.Invoke(); } }

        float current = -1, speed;
        Preset applied;
        Material runtimeSky;
        static readonly int SkyA = Shader.PropertyToID("_SkyA"), SkyB = Shader.PropertyToID("_SkyB"), BlendId = Shader.PropertyToID("_Blend"),
            SunDirId = Shader.PropertyToID("_SunDir"), NightId = Shader.PropertyToID("_Night"), GlitterId = Shader.PropertyToID("_Glitter"), PoolColourId = Shader.PropertyToID("_BaseColor");
        MaterialPropertyBlock poolBlock;

        void OnEnable()
        {
            // In play the sky is a copy, so the clock never edits the material asset.
            if (Application.isPlaying && sky) { runtimeSky = new Material(sky); RenderSettings.skybox = runtimeSky; }
            current = -1; Apply();
        }

        void OnDisable() { if (runtimeSky) Destroy(runtimeSky); runtimeSky = null; }

        float Target() => preset == Preset.Automatic ? hour : PresetHour(preset);

        public void SetPreset(Preset p)
        {
            if (p == Preset.Automatic && preset != Preset.Automatic && current >= 0) hour = current;   // the clock runs on from now
            preset = p; Changed?.Invoke();
        }

        void Update()
        {
            if (!Application.isPlaying) { Apply(); return; }
            if (preset == Preset.Automatic && !freeze) hour = Mathf.Repeat(hour + Time.deltaTime * 24 / (minutesPerDay * 60), 24);
            float delta = Mathf.Repeat(Target() - current + 12, 24) - 12;   // the short way round the clock
            if (preset != applied) { speed = Mathf.Abs(delta) / Mathf.Max(.01f, transitionSeconds); applied = preset; }
            if (preset == Preset.Automatic || freeze || transitionSeconds <= 0) current = Target();
            else if (Mathf.Abs(delta) > 1e-4f) current = Mathf.Repeat(current + Mathf.Clamp(delta, -speed * Time.deltaTime, speed * Time.deltaTime), 24);
            Evaluate(current);
        }

        // Jumps straight to the preset's hour (builders, previews, and the first frame).
        public void Apply()
        {
            if (current < 0 || !Application.isPlaying) { current = Target(); applied = preset; }
            Evaluate(current);
        }

        void Evaluate(float h)
        {
            if (keys == null || keys.Length == 0) return;
            int i1 = 0;
            while (i1 < keys.Length && keys[i1].hour <= h) i1++;
            int i0 = (i1 - 1 + keys.Length) % keys.Length; i1 %= keys.Length;
            Key a = keys[i0], b = keys[i1];
            float span = Mathf.Repeat(b.hour - a.hour, 24), t = span > 0 ? Mathf.SmoothStep(0, 1, Mathf.Repeat(h - a.hour, 24) / span) : 0;
            KeyFrom = i0; KeyTo = i1; KeyBlend = t;

            var m = runtimeSky ? runtimeSky : sky;
            if (m)
            {
                m.SetTexture(SkyA, a.sky); m.SetTexture(SkyB, b.sky); m.SetFloat(BlendId, t);
                if (RenderSettings.skybox != m) RenderSettings.skybox = m;
            }
            RenderSettings.ambientMode = AmbientMode.Trilight;
            var room = LampsOn ? roomAmbient : Color.black;
            RenderSettings.ambientSkyColor = Color.Lerp(a.ambientSky, b.ambientSky, t) + room;
            RenderSettings.ambientEquatorColor = Color.Lerp(a.ambientEquator, b.ambientEquator, t) + room;
            RenderSettings.ambientGroundColor = Color.Lerp(a.ambientGround, b.ambientGround, t) + room * .6f;
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
            var reflect = t < .5f ? a.sky : b.sky; if (reflect && RenderSettings.customReflectionTexture != reflect) RenderSettings.customReflectionTexture = reflect;
            RenderSettings.reflectionIntensity = Mathf.Lerp(a.reflection, b.reflection, t);
            if (sun)
            {
                // The light points away from the sun: pitch by its height, yaw opposite its azimuth.
                sun.transform.rotation = Quaternion.Slerp(Quaternion.Euler(a.elevation, a.azimuth + 180, 0), Quaternion.Euler(b.elevation, b.azimuth + 180, 0), t);
                sun.color = Color.Lerp(a.sunColor, b.sunColor, t);
                sun.intensity = Mathf.Lerp(a.sunIntensity, b.sunIntensity, t);
                RenderSettings.sun = sun;
            }
            Evening = Mathf.Lerp(a.night, b.night, t);
            float f = Mathf.Max(Mathf.Lerp(a.fills, b.fills, t), LampsOn ? roomFill : 0) * fillIntensity, l = LampsOn ? Mathf.Lerp(a.lamps, b.lamps, t) * lampIntensity : 0;
            if (fills != null) foreach (var x in fills) if (x) x.intensity = f;
            if (lamps != null) foreach (var x in lamps) if (x) { x.intensity = l; x.enabled = l > 0; }
            float roomLight = LampsOn ? Mathf.Max(Mathf.Lerp(a.lamps, b.lamps, t), roomLightMin) * roomLightIntensity : 0;
            if (roomLights != null) foreach (var x in roomLights) if (x) { x.intensity = roomLight; x.enabled = roomLight > 0; }
            if (lampGlass != null) foreach (var r in lampGlass) if (r) { var g = LampsOn ? glassOn : glassOff; if (g && r.sharedMaterial != g) r.sharedMaterial = g; }
            if (skins != null) foreach (var k in skins) if (k.renderer) { var g = LampsOn ? k.on : k.off; if (g && k.renderer.sharedMaterial != g) k.renderer.sharedMaterial = g; }
            if (lampPools != null)
            {
                poolBlock ??= new MaterialPropertyBlock();
                float pool = LampsOn ? Mathf.Lerp(a.lamps, b.lamps, t) : 0;
                foreach (var r in lampPools) if (r) { r.enabled = pool > .01f; r.GetPropertyBlock(poolBlock); poolBlock.SetColor(PoolColourId, poolColour * pool); r.SetPropertyBlock(poolBlock); }
            }
            // a sky with live water (Haven) glitters with the sun or the moon, and calms at night
            if (m && m.HasProperty(SunDirId) && sun) { m.SetVector(SunDirId, -sun.transform.forward); m.SetFloat(NightId, Evening); m.SetColor(GlitterId, Color.Lerp(sun.color * 1.6f, new Color(.9f, .95f, 1.1f), Evening)); }
        }
    }
}
