using UnityEngine;
using UnityEngine.SceneManagement;

namespace StudyEverywhere
{
    // For profiling only: launched from adb with a scene name, the app goes straight there instead of the lobby
    //   adb shell am start -n com.KoreMeta.NewWorld/com.unity3d.player.UnityPlayerGameActivity -e scene Solace -e perftour 1
    // and, with -e time Night (Morning, Afternoon, Sunset, Night), at that time of day, to check a place's night on the
    // headset without the menu. A normal launch (no extras) is untouched.
    public static class DevLaunch
    {
        public static string Extra(string key)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                using var intent = activity.Call<AndroidJavaObject>("getIntent");
                return intent.Call<string>("getStringExtra", key);
            }
            catch { return null; }
#else
            return null;
#endif
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void GoStraightThere()
        {
            var scene = Extra("scene");
            if (string.IsNullOrEmpty(scene) || SceneManager.GetActiveScene().name == scene) return;
            if (!LobbyPortal.IsOpen(scene)) { Debug.Log("DevLaunch: " + scene + " is a premium place, locked in this build."); return; }
            Debug.Log("DevLaunch: going straight to " + scene);
            if (System.Enum.TryParse<TimeOfDay.Preset>(Extra("time"), true, out var preset))
            {
                void Arrived(Scene s, LoadSceneMode mode)
                {
                    if (s.name != scene) return;
                    SceneManager.sceneLoaded -= Arrived;
                    var time = Object.FindAnyObjectByType<TimeOfDay>(); if (time) { time.SetPreset(preset); Debug.Log("DevLaunch: time " + preset); }
                }
                SceneManager.sceneLoaded += Arrived;
            }
            SceneManager.LoadScene(scene);
        }
    }
}
