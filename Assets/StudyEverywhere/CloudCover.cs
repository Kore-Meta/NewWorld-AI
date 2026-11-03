using System;
using UnityEngine;

namespace StudyEverywhere
{
    // How cloudy a place's sky is (the menu's Clouds row: Clear, Some, Cloudy), drawn by the Cloud Layer dome over the
    // baked sky, and lit from the clock: the clouds take the light's colour (sun by day, moon by night) on their bright
    // side and the ambient sky's underneath. Remembered per place.
    [ExecuteAlways]
    public sealed class CloudCover : MonoBehaviour
    {
        public enum Amount { Clear, Some, Cloudy }
        public TimeOfDay time;
        public Renderer dome;
        public string key = "StudyEverywhere.Clouds";
        public Amount fallback = Amount.Some;
        [Tooltip("Coverage for Clear, Some and Cloudy.")] public float[] coverage = { 0, .6f, .9f };
        public float fadeSeconds = 3;
        [Tooltip("By moonlight, at full night: the clouds' lit side and their underside.")] public Color nightLit = new Color(.42f, .46f, .56f), nightShade = new Color(.08f, .09f, .12f);

        static readonly int Coverage = Shader.PropertyToID("_Coverage"), SunDir = Shader.PropertyToID("_SunDir"), Lit = Shader.PropertyToID("_Lit"), Shade = Shader.PropertyToID("_Shade");
        MaterialPropertyBlock block;
        float shown = -1;
        public event Action Changed;
        // While set (weather such as Evercrest's rain), the sky shows this instead of the menu's choice.
        public Amount? Hold { get; set; }

        public Amount Current
        {
            get => (Amount)Mathf.Clamp(PlayerPrefs.GetInt(key, (int)fallback), 0, 2);
            set { PlayerPrefs.SetInt(key, (int)value); Changed?.Invoke(); }
        }

        void Update()
        {
            if (!dome) return;
            float target = coverage[(int)(Hold ?? Current)];
            shown = shown < 0 || !Application.isPlaying ? target : Mathf.MoveTowards(shown, target, Time.deltaTime / Mathf.Max(.1f, fadeSeconds));
            block ??= new MaterialPropertyBlock();
            dome.GetPropertyBlock(block);
            block.SetFloat(Coverage, shown);
            // Prefer this place's light; RenderSettings can temporarily belong to another
            // scene during travel. Always write colours, even when no light is assigned.
            var sun = time && time.sun ? time.sun : RenderSettings.sun;
            block.SetVector(SunDir, sun ? -sun.transform.forward : Vector3.up);
            float night = time ? Mathf.Clamp01(time.Evening) : 0;
            // clouds are white lit by the sun, a little of its warmth in them; never brighter than white (a full deck glared
            // yellow once the noise was read right)
            var dayLit = sun ? Color.Lerp(new Color(.97f, .97f, .98f), sun.color, .35f) * Mathf.Clamp(sun.intensity, .4f, 1f) : RenderSettings.ambientSkyColor;
            var lit = Color.Lerp(dayLit, nightLit, night);
            var shade = Color.Lerp(RenderSettings.ambientSkyColor * 1.1f + new Color(.12f, .12f, .14f), nightShade, night);
            lit.a = shade.a = 1;
            block.SetColor(Lit, lit); block.SetColor(Shade, shade);
            dome.SetPropertyBlock(block);
            dome.enabled = shown > .001f;
        }
    }
}
