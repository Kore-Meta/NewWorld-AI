using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace StudyEverywhere.EditorTools
{
    // NewWorld AI's Quest build: the lobby and the three open places (Orbit, Seoul, Seabreeze), already built and saved,
    // into Builds/NewWorld-AI.apk. Its own app id and name, so it installs beside the full New World on the same headset.
    //   Unity.exe -batchmode -quit -projectPath . -executeMethod StudyEverywhere.EditorTools.NewWorldAIBuild.BuildQuestApk
    public static class NewWorldAIBuild
    {
        public static readonly string[] Scenes = { "Assets/Scenes/Lobby.unity", "Assets/Scenes/Orbit.unity", "Assets/Scenes/Seoul.unity", "Assets/Scenes/Seabreeze.unity" };

        // Without Meta Quest Support the APK has no VR launch category: the Quest opens it as a flat app in Home. Switching
        // build profiles can turn it and the Quest controller profiles off, so every build switches them back on.
        static readonly string[] QuestFeatures = { "MetaQuestFeature", "OculusTouchControllerProfile", "MetaQuestTouchPlusControllerProfile", "MetaQuestTouchProControllerProfile" };

        [MenuItem("New World/Build Quest APK")]
        public static void BuildQuestApk()
        {
            EditorBuildSettings.scenes = Scenes.Select(s => new EditorBuildSettingsScene(s, true)).ToArray();
            PlayerSettings.companyName = "Kore Meta"; PlayerSettings.productName = "NewWorld AI";
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, "com.KoreMeta.NewWorldAI");
            var android = UnityEngine.XR.OpenXR.OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            if (android == null) throw new InvalidOperationException("No OpenXR settings for Android; enable OpenXR under Project Settings > XR Plug-in Management > Android.");
            foreach (var f in android.GetFeatures())
                if (Array.IndexOf(QuestFeatures, f.GetType().Name) >= 0 && !f.enabled) { f.enabled = true; EditorUtility.SetDirty(f); }
            EditorUtility.SetDirty(android); AssetDatabase.SaveAssets();
            if (!Resources.Load<TextAsset>("VoiceRelay")) Debug.LogWarning("NewWorld AI: no Resources/VoiceRelay.json, so the characters can't reach the relay (see README).");

            Directory.CreateDirectory("Builds");
            EditorUserBuildSettings.buildAppBundle = false;
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = Scenes, locationPathName = "Builds/NewWorld-AI.apk", target = BuildTarget.Android, targetGroup = BuildTargetGroup.Android, options = BuildOptions.None });
            var size = File.Exists(report.summary.outputPath) ? new FileInfo(report.summary.outputPath).Length / 1048576f : 0;
            Debug.Log($"NewWorld AI: Quest APK build {report.summary.result}, {size:F0} MB ({report.summary.outputPath}), {report.summary.totalErrors} errors.");
            if (Application.isBatchMode && report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) EditorApplication.Exit(1);
        }
    }
}
