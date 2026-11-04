using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Meta.WitAi.TTS.Integrations;
using Meta.WitAi.TTS.Utilities;
using Oculus.Voice;
using UnityEngine;
using UnityEngine.Networking;

namespace StudyEverywhere
{
    // A café pet's voice (New York's puppy Bubble, Seoul's tiger cub Hori): they talk as the Paris café's Émile does.
    // Speech-to-text runs on Wit.ai through Meta's Voice SDK while the person is within listenRange of the pet (and the
    // menu's "listens" switch is on: turning it off disables this component, which closes the microphone at once); a
    // finished sentence goes, with what the pet is doing (IPetCompanion.Describe), to our relay (Tools/CatServer,
    // POST /pet/<petId>/talk), where Claude answers as the pet and may act (sit, a chuff, a play bow…); the reply is spoken
    // in the pet's voice through the Voice SDK's text-to-speech, typed into its speech bubble, and its mouth follows the
    // sound.
    // Kept to the privacy policy through VoiceRelay: the relay's address and token come from Resources/VoiceRelay.json
    // (never saved in the scene), each launch's random session id keeps this person's conversation apart from anyone
    // else's, a release build talks only https (or to the PC beside it on the local network), and what was said is logged
    // only in the Editor and development builds. The last few lines are held in memory so a relay restart keeps the thread;
    // they are forgotten after 30 minutes of quiet or when this is turned off.
    public sealed class PetVoiceLink : MonoBehaviour
    {
        [Header("Voice SDK")]
        public AppVoiceExperience voice;
        public TTSSpeaker speaker;
        [Header("The pet")]
        [Tooltip("Its companion script (an IPetCompanion: PuppyCompanion, TigerCompanion).")] public MonoBehaviour pet;
        [Tooltip("The relay's route for him: /pet/<id>/talk.")] public string petId = "bubble";
        [Tooltip("The person's head (the main camera).")] public Transform user;
        [Tooltip("The microphone stays open while the person is within this distance of the pet (metres).")] public float listenRange = 4f;
        [Header("Its voice (a Wit preset)")]
        public string voiceName = "Cam";
        [Range(25, 200)] public int voicePitch = 140;
        [Tooltip("Wit style: default, soft, formal, fast or projected.")] public string voiceStyle = "fast";
        [Tooltip("How the voice should say the pet's name, when the plain spelling is misread (Hori said \"hoh-rye\": \"Horee\"). Only the spoken words change; the bubble keeps the real name. Empty: as written.")]
        public string spokenName = "";
        [Header("Relay (Tools/CatServer); empty = Resources/VoiceRelay.json")]
        public string serverUrl = "", appToken = "";

        IPetCompanion Pet => pet as IPetCompanion;
        string Who => Pet != null ? Pet.DisplayName : "Pet";
        bool busy;
        float retryAt, busySince, requestSince = -1, lastExchange = -1;
        readonly List<Line> history = new List<Line>();
        const int HistoryLines = 12;
        const float ForgetAfter = 30 * 60;

        void Awake()
        {
            // lines played whole, and a runtime clip cache (as the composers and Orbit)
            if (speaker && speaker.GetComponent<Meta.Voice.Audio.IAudioPlayer>() == null) speaker.gameObject.AddComponent<WholeClipAudioPlayer>();
            var tts = speaker ? speaker.TTSService : null;
            if (tts && tts.RuntimeCacheHandler == null)
            {
                var cache = tts.gameObject.AddComponent<TTSRuntimeLRUCache>();
                cache.ClipLimit = false; cache.RamLimit = true; cache.RamCapacity = 16000;
                tts.RuntimeCacheHandler = cache;
            }
        }

        // Each time the person comes into the café: the pet's bubble introduces it (its name, call it to talk, the wrist
        // menu's switch) until they start talking to it. Only while it can listen: turned off, it says nothing of the sort.
        IEnumerator Start()
        {
            yield return new WaitForSeconds(1.5f);   // once the person is in and looking about
            if (isActiveAndEnabled && Configured && Pet?.Bubble) Pet.Bubble.Intro(Who);
        }

        void OnEnable()
        {
            if (!voice) return;
            voice.VoiceEvents.OnPartialTranscription.AddListener(OnPartial);
            voice.VoiceEvents.OnFullTranscription.AddListener(OnFull);
            voice.VoiceEvents.OnError.AddListener(OnError);
        }

        // Turned off (the menu's switch, or leaving): the microphone closes now, no "listening" dots linger, and the
        // conversation is forgotten.
        void OnDisable()
        {
            if (voice)
            {
                if (voice.IsRequestActive) voice.DeactivateAndAbortRequest();
                voice.VoiceEvents.OnPartialTranscription.RemoveListener(OnPartial);
                voice.VoiceEvents.OnFullTranscription.RemoveListener(OnFull);
                voice.VoiceEvents.OnError.RemoveListener(OnError);
            }
            if (speaker && speaker.IsActive) speaker.Stop();
            if (Pet?.Bubble) { Pet.Bubble.Listening(false); Pet.Bubble.EndIntro(); }
            busy = false; history.Clear();
        }

        bool Configured => voice && voice.RuntimeConfiguration != null && voice.RuntimeConfiguration.witConfiguration;
        bool Near => Pet != null && user && (pet.transform.position - user.position).magnitude < listenRange;

        void Update()
        {
            // The microphone stays open while the pet is near and no reply is pending; the Voice SDK closes it after each
            // sentence (or a stretch of quiet), so it is reopened here. Watchdogs free a reply or a listening request that
            // never finishes, so he can't go deaf.
            if (!Configured) return;
            if (busy)
            {
                if (Time.time - busySince > 60) { Debug.LogWarning($"{Who}'s voice: a reply took too long; listening again."); busy = false; }
                return;
            }
            if (voice.IsRequestActive)
            {
                if (!Near) { voice.Deactivate(); requestSince = -1; return; }   // walked away: stop listening
                if (requestSince < 0) requestSince = Time.time;
                else if (Time.time - requestSince > 20) { Debug.LogWarning($"{Who}'s voice: listening stuck; restarting the microphone."); voice.Deactivate(); requestSince = -1; }
                return;
            }
            requestSince = -1;
            if (Time.time < retryAt) return;
            if (Near) voice.Activate();
        }

        void OnPartial(string text)
        {
            if (busy || !Near || string.IsNullOrWhiteSpace(text)) return;
            if (Pet.Bubble) Pet.Bubble.Listening(true);
            Pet.Attend();
        }

        void OnError(string error, string message) { Debug.LogWarning($"{Who}'s voice: {error} {message}"); retryAt = Time.time + 2; }

        void OnFull(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || busy) return;   // still answering: a new reply would cut him off
            if (!Near) { if (Pet?.Bubble) Pet.Bubble.Listening(false); return; }
            StartCoroutine(Converse(text.Trim()));
        }

        IEnumerator Converse(string text)
        {
            busy = true; busySince = Time.time;
            try
            {
                Pet.Attend(20);
                if (Pet.Bubble) { Pet.Bubble.Listening(false); Pet.Bubble.Heard(text); }
                VoiceRelay.Transcript($"{Who}: heard \"{text}\"");
                if (lastExchange >= 0 && Time.time - lastExchange > ForgetAfter) history.Clear();
                string url = VoiceRelay.Url(serverUrl), reply = null; string[] actions = null;
                yield return VoiceRelay.Check(url, appToken);
                if (!string.IsNullOrEmpty(url) && VoiceRelay.Up)
                {
                    // the headset's local date and time, so he knows what "today" is
                    var now = DateTime.Now.ToString("dddd d MMMM yyyy, HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                    var body = JsonUtility.ToJson(new Request { text = text, state = Pet.Describe(), now = now, history = history.ToArray() });
                    using var req = new UnityWebRequest(url + "/pet/" + petId + "/talk", "POST")
                    {
                        uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)),
                        downloadHandler = new DownloadHandlerBuffer(),
                        timeout = 45,   // a reply with a web search takes longer
                    };
                    VoiceRelay.Prepare(req, appToken);
                    UnityWebRequestAsyncOperation sending = null;
                    try { sending = req.SendWebRequest(); } catch (Exception ex) { Debug.LogWarning($"{Who}'s relay: {ex.Message}"); }   // an http address a release build refuses
                    if (sending != null) yield return sending;
                    if (sending != null && req.result == UnityWebRequest.Result.Success)
                    {
                        try { var res = JsonUtility.FromJson<Response>(req.downloadHandler.text); reply = res?.reply; actions = res?.actions; } catch (Exception) { reply = null; }
                    }
                    else if (sending != null) Debug.LogWarning($"{Who}'s relay: {req.responseCode} {req.error}");
                }
                if (string.IsNullOrWhiteSpace(reply))
                {
                    // say so, rather than fall silent: the pet's own sound and a line in its bubble
                    Pet.Do(Pet.UnreachableAction);
                    if (Pet.Bubble) Pet.Bubble.Say(Localization.Tr(Pet.Unreachable));
                    yield break;
                }
                reply = reply.Trim();
                Remember("user", text); Remember("assistant", reply);
                lastExchange = Time.time;
                VoiceRelay.Transcript($"{Who}: \"{reply}\" {(actions != null ? string.Join(", ", actions) : "")}");
                if (Pet.Bubble) Pet.Bubble.Say(reply);
                if (actions != null) foreach (var a in actions) Pet.Do(a);
                // spoken in the pet's voice (the Wit voices speak English; in other languages it answers in its bubble only)
                if (speaker && Localization.Current == 0)
                {
                    speaker.SetVoiceOverride(new TTSWitVoiceSettings { voice = voiceName, pitch = voicePitch, style = string.IsNullOrEmpty(voiceStyle) ? "default" : voiceStyle });
                    Pet.Speech = speaker.AudioSource;
                    var spoken = string.IsNullOrEmpty(spokenName) ? reply : System.Text.RegularExpressions.Regex.Replace(reply, $@"\b{System.Text.RegularExpressions.Regex.Escape(Who)}\b", spokenName);
                    foreach (var part in SpokenLines.Sentences(spoken)) speaker.SpeakQueued(part);
                    // the microphone stays shut until the pet has finished, so it does not hear itself; never longer than the
                    // reply could take (a voice that never starts must not keep it shut)
                    float until = Time.time + 10 + reply.Length * .12f;
                    yield return new WaitForSeconds(.3f);
                    while (speaker.IsActive && Time.time < until) yield return null;
                    if (speaker.IsActive) { Debug.LogWarning($"{Who}'s voice: speech did not finish; stopping it."); speaker.Stop(); }
                }
            }
            finally { busy = false; }
        }

        void Remember(string role, string text)
        {
            history.Add(new Line { role = role, text = text.Length > 600 ? text.Substring(0, 600) : text });
            while (history.Count > HistoryLines) history.RemoveAt(0);
        }

        [Serializable] class Line { public string role, text; }
        [Serializable] class Request { public string text, state, now; public Line[] history; }
        [Serializable] class Response { public string reply; public string[] actions; }
    }
}
