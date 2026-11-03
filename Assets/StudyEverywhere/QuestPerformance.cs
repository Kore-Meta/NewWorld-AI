using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR.Features.Extensions.PerformanceSettings;
#if UNITY_ANDROID
using UnityEngine.XR.OpenXR.Features.Meta;   // Android-only assembly
#endif

namespace StudyEverywhere
{
    // Holds Quest at or above the target frame rate.
    // Once XR is running: request the display rate, hint sustained-high CPU/GPU clocks, enable foveation.
    // Then a governor watches frame rate and GPU time every half second and trades resolution for frames:
    // foveation first (free to change), render scale second; quality comes back only after sustained headroom.
    // Development builds show a focus status strip (New World UI) under the view and log to logcat ("adb logcat -s Unity | findstr QuestPerformance").
    public sealed class QuestPerformance : MonoBehaviour
    {
        [Tooltip("Frame rate to hold. The closest supported display rate at or above it is requested.")]
        public float targetRefreshRate = 72;
        [Range(0, 1)] public float baseFoveation = 0.5f;
        [Range(0.5f, 1)] public float minRenderScale = 0.7f;
        [Tooltip("Show the focus status strip in release builds too. Development builds always show it.")]
        public bool showFps;
        [Tooltip("Rounded UI sprite and font for the status strip; the builder assigns them.")]
        public Sprite uiSprite;
        public TMP_FontAsset uiFont;

        // Each level costs less than the one before it. Scales are relative to the asset's own render scale (1.2 on Quest:
        // supersampled eye buffers keep text and fine edges crisp); 0 drops to minRenderScale.
        static readonly (float foveation, float scale)[] Levels = { (-1, 1f), (1, 1f), (1, 0.9f), (1, 0.8f), (1, 0) };
        const float Window = 0.5f;

        readonly List<XRDisplaySubsystem> displays = new List<XRDisplaySubsystem>();
        readonly FrameTiming[] timings = new FrameTiming[1];
        XRDisplaySubsystem display;
        UniversalRenderPipelineAsset urp;
        float originalScale = -1, elapsed, logTimer;
        int frames, level, overWindows, headroomWindows;
        bool governs;
        TMP_Text statusLine;
        Image statusDot;

        void Update()
        {
            FrameTimingManager.CaptureFrameTimings();
            if (display == null && !TrySetup()) return;

            frames++;
            elapsed += Time.unscaledDeltaTime;
            if (elapsed < Window) return;
            float fps = frames / elapsed, gpu = FrameTimingManager.GetLatestTimings(1, timings) > 0 ? (float)timings[0].gpuFrameTime : 0;
            frames = 0; elapsed = 0;

            float target = Mathf.Max(targetRefreshRate, 1), budget = 1000 / target;
            if (governs)
            {
                // Over budget for 1 s: step down. Comfortable for 5 s: step up.
                bool over = fps < target * 0.97f || gpu > budget * 0.92f;
                bool headroom = fps >= target * 0.99f && gpu > 0 && gpu < budget * 0.7f;
                overWindows = over ? overWindows + 1 : 0;
                headroomWindows = headroom ? headroomWindows + 1 : 0;
                if (overWindows >= 2 && level < Levels.Length - 1) { SetLevel(level + 1); overWindows = 0; }
                else if (headroomWindows >= 10 && level > 0) { SetLevel(level - 1); headroomWindows = 0; }
            }

            string line = $"{fps:0} fps  ·  GPU {gpu:0.0} / {budget:0.0} ms  ·  L{level}";
            if (statusLine) ShowStatus(line);
            if ((logTimer += Window) >= 5 && Debug.isDebugBuild) { logTimer = 0; Debug.Log("[QuestPerformance] " + line); }
        }

        bool TrySetup()
        {
            SubsystemManager.GetSubsystems(displays);
            foreach (var d in displays) if (d.running) { display = d; break; }
            if (display == null) return false;

            // The governor changes the active URP asset, so it only runs on the headset build, never on Editor assets.
            governs = Application.platform == RuntimePlatform.Android;
            urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urp) originalScale = urp.renderScale;
            if (governs)
            {
                RequestRefreshRate();
                XrPerformanceSettingsFeature.SetPerformanceLevelHint(PerformanceDomain.Cpu, PerformanceLevelHint.SustainedHigh);
                XrPerformanceSettingsFeature.SetPerformanceLevelHint(PerformanceDomain.Gpu, PerformanceLevelHint.SustainedHigh);
            }
            display.foveatedRenderingFlags = XRDisplaySubsystem.FoveatedRenderingFlags.GazeAllowed;
            SetLevel(0);
            if (showFps || Debug.isDebugBuild) CreateStatusStrip();
            return true;
        }

        void RequestRefreshRate()
        {
#if UNITY_ANDROID
            if (!display.TryGetSupportedDisplayRefreshRates(Allocator.Temp, out var rates)) return;
            float pick = 0, highest = 0;
            foreach (float r in rates)
            {
                highest = Mathf.Max(highest, r);
                if (r >= targetRefreshRate - 0.5f && (pick == 0 || r < pick)) pick = r;
            }
            rates.Dispose();
            if (pick == 0) pick = highest;
            if (pick > 0 && display.TryRequestDisplayRefreshRate(pick)) targetRefreshRate = Mathf.Max(targetRefreshRate, pick);
#endif
        }

        void SetLevel(int value)
        {
            level = value;
            var (foveation, scale) = Levels[level];
            // Foveation only on the headset build: over Link the PC has the headroom, and it shows as soft, shimmering edges.
            display.foveatedRenderingLevel = !governs ? 0 : foveation < 0 ? baseFoveation : foveation;
            if (governs && urp) urp.renderScale = scale == 0 ? minRenderScale : Mathf.Max(minRenderScale, scale * Mathf.Max(originalScale, 1));
        }

        // The strip leads with the workspace computer connection ("PC Connected", "No computer"); its dot is green while
        // the picture is live, brass while pairing or reconnecting, grey with no computer.
        void ShowStatus(string performance)
        {
            var link = WorkspaceConnection.Active;
            var state = link ? link.Current : WorkspaceConnection.State.NotPaired;
            statusLine.text = (link ? link.StatusLabel : "No computer") + "  ·  " + performance;
            statusDot.color = state == WorkspaceConnection.State.Connected ? UIStyle.Live
                : state == WorkspaceConnection.State.NotPaired || state == WorkspaceConnection.State.Disconnected ? UIStyle.LightBorder : UIStyle.Brass;
        }

        // Focus status strip: dark translucent pill, connection dot, ivory text, a little below the line of sight.
        void CreateStatusStrip()
        {
            var cam = Camera.main;
            if (!cam) return;
            var go = new GameObject("Focus status strip", typeof(RectTransform), typeof(Canvas));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 100;
            var rt = (RectTransform)go.transform;
            rt.SetParent(cam.transform, false);
            rt.localPosition = new Vector3(0, -0.28f, 0.9f);
            rt.localRotation = Quaternion.Euler(18, 0, 0);
            rt.localScale = Vector3.one * 0.0006f;
            rt.sizeDelta = new Vector2(640, 56);
            Part<Image>("Pill", rt, Vector2.zero, rt.sizeDelta, UIStyle.SurfaceDark, 28);
            statusDot = Part<Image>("Connection dot", rt, new Vector2(-292, 0), new Vector2(12, 12), UIStyle.LightBorder, 6);
            statusLine = Part<TextMeshProUGUI>("Status line", rt, new Vector2(14, 0), new Vector2(580, 48), UIStyle.WarmIvory, 0);
            if (uiFont) statusLine.font = uiFont;
            statusLine.fontSize = 22;
            statusLine.alignment = TextAlignmentOptions.Left;
            statusLine.textWrappingMode = TextWrappingModes.NoWrap;
        }

        T Part<T>(string name, RectTransform parent, Vector2 pos, Vector2 size, Color color, float radius) where T : Graphic
        {
            var r = (RectTransform)new GameObject(name, typeof(RectTransform), typeof(T)).transform;
            r.SetParent(parent, false);
            r.anchoredPosition = pos;
            r.sizeDelta = size;
            var g = r.GetComponent<T>();
            g.color = color;
            g.raycastTarget = false;
            if (g is Image img && uiSprite) { img.sprite = uiSprite; img.type = Image.Type.Sliced; img.pixelsPerUnitMultiplier = UIStyle.CornerMultiplier(radius); }
            return g;
        }

        void OnDisable()
        {
            if (urp && originalScale > 0) urp.renderScale = originalScale;
            if (statusLine) Destroy(statusLine.canvas.gameObject);
            display = null;
        }
    }
}
