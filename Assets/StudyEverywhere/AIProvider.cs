using System;
using UnityEngine;

namespace StudyEverywhere
{
    // Which AI answers the AI characters (the café cats, the pets, Orbit, the composers): Claude, Muse (Meta) or ChatGPT.
    // The person picks it on the wrist menu (AIPicker); it is remembered between sessions and sent with every relay
    // request (VoiceRelay.Prepare, the X-AI-Provider header). The relay holds the keys: a provider it has no key for
    // answers with its default instead.
    public static class AIProvider
    {
        public static readonly string[] Ids = { "muse" };   // NewWorld AI: Muse only
        public static readonly string[] Names = { "Muse" };
        const string Key = "NewWorld.AIProvider";

        public static event Action Changed;
        public static int Current
        {
            get => Mathf.Clamp(PlayerPrefs.GetInt(Key, 0), 0, Ids.Length - 1);
            set { int v = Mathf.Clamp(value, 0, Ids.Length - 1); if (v == Current) return; PlayerPrefs.SetInt(Key, v); PlayerPrefs.Save(); Changed?.Invoke(); }
        }
        public static string Id => Ids[Current];
        public static string Name => Names[Current];
    }
}
