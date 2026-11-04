using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace StudyEverywhere
{
    // How the AI characters (Orbit, the café cats and later pets) reach the relay server (Tools/CatServer), kept to
    // what the privacy policy promises:
    // - The address and app token come from Resources/VoiceRelay.json, written by the builders from the git-ignored
    //   Tools/CatServer/unity-client.json, so they are never saved into a scene or committed. A component's own
    //   serverUrl/appToken, if set, still wins (for testing).
    // - Each launch makes a random session id, held in memory only and sent with every request, so the relay keeps
    //   each person's conversation apart and never mixes it with anyone else's. It identifies nothing about the person.
    // - Release builds only talk to an https relay ("encrypted connections to our servers"); plain http to a PC on the
    //   local Wi-Fi works only in the Editor and development builds.
    // - What people say is logged only in the Editor and development builds (Transcript), never in a release build.
    public static class VoiceRelay
    {
        [Serializable] class Config { public string serverUrl = "", appToken = ""; }
        static Config config;
        static Config Loaded => config ??= Load();
        static Config Load()
        {
            var file = Resources.Load<TextAsset>("VoiceRelay");
            try { return file ? JsonUtility.FromJson<Config>(file.text) ?? new Config() : new Config(); }
            catch (Exception) { return new Config(); }
        }

        public static readonly string Session = Guid.NewGuid().ToString("N");

        // The relay address to use, or "" when there is none this build may use (the character then answers offline).
        public static string Url(string own = "")
        {
            var url = (string.IsNullOrEmpty(own) ? Loaded.serverUrl : own ?? "").Trim().TrimEnd('/');
            // A release build speaks plain http only to a relay on the local network (the PC beside the headset, as the
            // showcase runs); anywhere else it must be https.
            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !Application.isEditor && !Debug.isDebugBuild && !OnLocalNetwork(url)) return "";
            return url;
        }

        static bool OnLocalNetwork(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return false;
            var host = uri.Host;
            if (host == "localhost" || host == "127.0.0.1" || host.EndsWith(".local", StringComparison.OrdinalIgnoreCase)) return true;
            if (!System.Net.IPAddress.TryParse(host, out var ip) || ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork) return false;
            var b = ip.GetAddressBytes();
            return b[0] == 10 || (b[0] == 192 && b[1] == 168) || (b[0] == 172 && b[1] >= 16 && b[1] <= 31);
        }

        public static void Prepare(UnityWebRequest request, string ownToken = "")
        {
            request.SetRequestHeader("Content-Type", "application/json");
            var token = string.IsNullOrEmpty(ownToken) ? Loaded.appToken : ownToken;
            if (!string.IsNullOrEmpty(token)) request.SetRequestHeader("X-Cat-Token", token);
            request.SetRequestHeader("X-Session", Session);
            request.SetRequestHeader("X-AI-Provider", AIProvider.Id);   // the wrist menu's AI choice
        }

        // The relay on Render's free plan sleeps after a quiet quarter hour, and the first reply then took 15-20 s (Nov 3
        // review: Hori's "come to me"). So each place wakes it as it loads, before anyone speaks, and every ten minutes while
        // the app is open: a bare GET /health, with no token, no session and nothing anyone said.
        const float WakeEvery = 600;
        static float lastWake = -WakeEvery;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void WakeOnLoad()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += (s, m) => Wake(true);
            var keeper = new GameObject("Relay wake (every ten minutes)") { hideFlags = HideFlags.HideAndDontSave };
            UnityEngine.Object.DontDestroyOnLoad(keeper);
            keeper.AddComponent<Waker>();
            Wake(true);
        }
        public static void Wake(bool now = false)
        {
            var url = Url();
            if (string.IsNullOrEmpty(url) || (!now && Time.realtimeSinceStartup - lastWake < WakeEvery)) return;
            lastWake = Time.realtimeSinceStartup;
            var req = UnityWebRequest.Get(url + "/health"); req.timeout = 90;
            req.SendWebRequest().completed += _ => { Heard(req); req.Dispose(); };
        }
        sealed class Waker : MonoBehaviour { void Update() => Wake(); }

        // Whether the relay is running (New World's Nov 3 check, so a character doesn't sit on "Thinking…" for its whole
        // talk timeout when nothing answers). Here the relay is hosted and may be waking (15-20 s), so a slow answer is not
        // a missing relay: only a connection that fails counts as down, the check waits as long as a wake can take, and
        // any answer (the wakes' included) holds for ten minutes. Muse is never skipped while the relay can still answer.
        public static bool Up { get; private set; } = true;
        static float checkedAt = -999;
        static void Heard(UnityWebRequest request)
        {
            Up = request.result != UnityWebRequest.Result.ConnectionError;
            checkedAt = Time.realtimeSinceStartup;
            if (!Up) Debug.LogWarning($"Relay: nothing answers at {Url()} ({request.error}).");
        }
        public static IEnumerator Check(string url, string ownToken = "")
        {
            if (string.IsNullOrEmpty(url) || Time.realtimeSinceStartup - checkedAt < (Up ? WakeEvery : 3)) yield break;
            using var request = UnityWebRequest.Get(url + "/health");
            request.timeout = 30;
            UnityWebRequestAsyncOperation sending = null;
            try { sending = request.SendWebRequest(); } catch (Exception) { }   // an http address a release build refuses
            if (sending != null) yield return sending;
            if (sending == null) { Up = false; checkedAt = Time.realtimeSinceStartup; }
            else Heard(request);
        }

        // For lines that contain what someone said or what a character replied: compiled out of release builds.
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Transcript(string line) => Debug.Log(line);
    }
}
