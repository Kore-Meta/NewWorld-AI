using System.IO;
using UnityEngine;

namespace StudyEverywhere
{
    // Opt-in only: Android launch extra "skyprobe" = "1". Change sky-probe.txt
    // in persistentDataPath to isolate layers without rebuilding or restarting.
    [DefaultExecutionOrder(10000)]
    public sealed class SkyProbe : MonoBehaviour
    {
        string mode = "", path;
        float nextPoll;
        CloudCover[] clouds;
        TwinkleStars[] stars;
        Cubemap white;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (DevLaunch.Extra("skyprobe") != "1") return;
            var go = new GameObject("Sky diagnostics (opt-in)");
            DontDestroyOnLoad(go);
            go.AddComponent<SkyProbe>();
        }

        void Awake()
        {
            path = Path.Combine(Application.persistentDataPath, "sky-probe.txt");
            white = new Cubemap(1, TextureFormat.RGBA32, false);
            for (int face = 0; face < 6; face++) white.SetPixel((CubemapFace)face, 0, 0, Color.white);
            white.Apply();
            Debug.Log("[SkyProbe] Control file: " + path);
        }

        void LateUpdate()
        {
            if (Time.unscaledTime >= nextPoll)
            {
                nextPoll = Time.unscaledTime + 1;
                clouds = FindObjectsByType<CloudCover>();
                stars = FindObjectsByType<TwinkleStars>();
                try
                {
                    // Optional fixed time for repeatable captures, without changing saved preferences.
                    var timePath = Path.Combine(Application.persistentDataPath, "sky-time.txt");
                    var requestedTime = File.Exists(timePath) ? File.ReadAllText(timePath).Trim() : "";
                    if (requestedTime == "night" || requestedTime == "day")
                    {
                        var preset = requestedTime == "night" ? TimeOfDay.Preset.Night : TimeOfDay.Preset.Morning;
                        foreach (var clock in FindObjectsByType<TimeOfDay>())
                            if (clock.preset != preset) clock.SetPreset(preset);
                    }
                    string requested = File.Exists(path) ? File.ReadAllText(path).Trim().ToLowerInvariant() : "normal";
                    if (requested != mode && Valid(requested))
                    {
                        Restore();
                        mode = requested;
                        Report();
                    }
                }
                catch (IOException e) { Debug.LogWarning("[SkyProbe] " + e.Message); }
            }
            if (clouds == null || mode == "normal") return;
            foreach (var cloud in clouds)
            {
                if (!cloud || !cloud.dome) continue;
                var r = cloud.dome;
                r.forceRenderingOff = mode == "clouds-off" || mode == "layers-off";
                if (mode == "white-mask" || mode == "clouds-magenta")
                {
                    r.GetPropertyBlock(block);
                    if (mode == "white-mask") block.SetTexture("_Mask", white);
                    else { block.SetColor("_Lit", Color.magenta); block.SetColor("_Shade", Color.magenta); }
                    r.SetPropertyBlock(block);
                }
            }
            foreach (var star in stars)
            {
                if (!star || !star.TryGetComponent<Renderer>(out var r)) continue;
                r.forceRenderingOff = mode == "stars-off" || mode == "layers-off";
                if (mode == "white-mask")
                {
                    r.GetPropertyBlock(block); block.SetTexture("_Mask", white); r.SetPropertyBlock(block);
                }
            }
        }

        static bool Valid(string value) => value == "normal" || value == "clouds-off" || value == "stars-off"
            || value == "layers-off" || value == "white-mask" || value == "clouds-magenta";

        void RestoreRenderer(Renderer r)
        {
            if (!r) return;
            r.forceRenderingOff = false;
            if (mode != "white-mask") return;
            r.GetPropertyBlock(block);
            var mask = r.sharedMaterial.GetTexture("_Mask");
            if (mask) block.SetTexture("_Mask", mask);
            r.SetPropertyBlock(block);
        }

        void Restore()
        {
            if (clouds != null) foreach (var c in clouds) if (c) RestoreRenderer(c.dome);
            if (stars != null) foreach (var s in stars) if (s) RestoreRenderer(s.GetComponent<Renderer>());
        }

        void Report()
        {
            var cam = Camera.main;
            Debug.Log($"[SkyProbe] mode={mode}, graphics={SystemInfo.graphicsDeviceType}, " +
                $"sun={RenderSettings.sun}, camera={cam}, far={(cam ? cam.farClipPlane : 0)}");
            foreach (var c in clouds)
            {
                if (!c.dome) continue;
                c.dome.GetPropertyBlock(block);
                Debug.Log($"[SkyProbe] {c.name}: night={(c.time ? c.time.Evening : 0)}, " +
                    $"coverage={block.GetFloat("_Coverage")}, lit={block.GetColor("_Lit")}, shade={block.GetColor("_Shade")}");
            }
        }

        void OnDestroy() { Restore(); if (white) Destroy(white); }
    }
}
