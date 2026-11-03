using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Meta.WitAi.TTS.Utilities;
using Oculus.Voice;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace StudyEverywhere
{
    // Calypso, Seabreeze's companion: a warm, calm voice of the beach whose form is a bubble of moving water (CalypsoOrb)
    // with her see-through card under it, both just in front of the person and drifting after their gaze; the orb answers
    // her voice as she talks and the person's as they speak. Speech-to-text runs on Wit.ai through Meta's Voice SDK (as Orbit); a sentence that names
    // Calypso, or anything said just after the card's Talk pill, goes with the beach's state to our relay (Tools/CatServer,
    // /pet/calypso/talk), where the AI chosen on the wrist menu answers and may act on the beach: hatch the turtles, set the
    // time of day or run the day's cycle, calm the waves or let them run, put the fire out or light it. The reply is spoken
    // through the Voice SDK's text-to-speech and written on the card, with what was heard and what was done. Without the
    // relay she still carries out plain commands ("hatch the turtles", "sunset", "stop the waves", "fire off") herself.
    // Kept to the privacy policy as the pets are: what is said is logged only in the Editor and development builds.
    public sealed class CalypsoCompanion : MonoBehaviour
    {
        [Header("Voice SDK")]
        public AppVoiceExperience voice;
        public TTSSpeaker speaker;
        [Tooltip("Wit text-to-speech voice (a warm, calm woman's voice) and pitch.")] public string voiceName = "Charlie";
        [Range(25, 200)] public int pitch = 96;
        [Header("Relay server (Tools/CatServer); empty: Resources/VoiceRelay.json")]
        public string serverUrl = "", appToken = "";
        [Header("The beach")]
        public TimeOfDay time;
        public SeaShore shore;
        public BeachCritters hatchlings;
        public Transform user;
        [Header("Her form: the orb, and the card under it")]
        [Tooltip("The orb and card together (the orb's centre, in the card's plane).")] public Transform card;
        public CalypsoOrb orb;
        public CanvasGroup cardGroup;
        public TMP_Text status, heard, done, reply;
        public Button talk;
        [Tooltip("Where she floats: metres ahead (past the wrist menu, within reach), degrees to the side (negative: left), metres below the eyes.")]
        public float cardDistance = .6f, cardYaw = 0, cardDrop = .05f;
        [Tooltip("Words that address Calypso.")] public string[] wakeWords = { "calypso", "calipso", "kalypso" };
        [Tooltip("After the Talk pill, the next sentence is for Calypso even without her name (seconds).")] public float askWindow = 8;

        bool busy;
        float retryAt, busySince, requestSince = -1, askedAt = -100, lastActive = -100;
        readonly List<(string role, string text)> history = new List<(string, string)>();

        void Awake()
        {
            if (speaker && speaker.GetComponent<Meta.Voice.Audio.IAudioPlayer>() == null) speaker.gameObject.AddComponent<WholeClipAudioPlayer>();
            var tts = speaker ? speaker.TTSService : null;
            if (tts && tts.RuntimeCacheHandler == null)
            {
                var cache = tts.gameObject.AddComponent<Meta.WitAi.TTS.Integrations.TTSRuntimeLRUCache>();
                cache.ClipLimit = false; cache.RamLimit = true; cache.RamCapacity = 16000;
                tts.RuntimeCacheHandler = cache;
            }
        }

        void OnEnable()
        {
            if (talk) talk.onClick.AddListener(OnTalk);
            if (!voice) return;
            voice.VoiceEvents.OnPartialTranscription.AddListener(OnPartial);
            voice.VoiceEvents.OnFullTranscription.AddListener(OnFull);
            voice.VoiceEvents.OnError.AddListener(OnError);
            voice.VoiceEvents.OnMicLevelChanged.AddListener(OnMicLevel);
        }
        void OnDisable()
        {
            if (talk) talk.onClick.RemoveListener(OnTalk);
            if (!voice) return;
            if (voice.IsRequestActive) voice.Deactivate();
            voice.VoiceEvents.OnPartialTranscription.RemoveListener(OnPartial);
            voice.VoiceEvents.OnFullTranscription.RemoveListener(OnFull);
            voice.VoiceEvents.OnError.RemoveListener(OnError);
            voice.VoiceEvents.OnMicLevelChanged.RemoveListener(OnMicLevel);
            busy = false;
        }

        void Start()
        {
            if (speaker) SpokenLines.Voice(speaker, voiceName, pitch);
            Idle();
            if (card && user) PlaceCard(true);
            Debug.Log($"Calypso: voice {(Configured ? "configured" : "NOT configured (no Wit configuration)")}, relay {(string.IsNullOrEmpty(VoiceRelay.Url(serverUrl)) ? "not set" : "set")}.");
        }

        bool Configured => voice && voice.RuntimeConfiguration != null && voice.RuntimeConfiguration.witConfiguration;

        void Update()
        {
            if (card && user) PlaceCard(false);
            if (orb) orb.thinking = busy && !(speaker && speaker.IsActive);
            // her captions come up while she is listening, thinking or talking, and fade away once she has been quiet a while
            // (Nov 3 review: no card; her name stays in her orb)
            if (cardGroup) cardGroup.alpha = Mathf.MoveTowards(cardGroup.alpha, Time.time - lastActive < 12 ? 1 : 0, Time.deltaTime * 1.5f);
            if (!Configured) return;
            if (busy) { if (Time.time - busySince > 60) { Debug.LogWarning("Calypso: a reply took too long; listening again."); busy = false; Idle(); } return; }
            if (speaker && speaker.IsActive) { if (voice.IsRequestActive) voice.Deactivate(); requestSince = -1; return; }   // never hear herself
            if (voice.IsRequestActive)
            {
                if (requestSince < 0) requestSince = Time.time;
                else if (Time.time - requestSince > 20) { voice.Deactivate(); requestSince = -1; }
                return;
            }
            requestSince = -1;
            if (Time.time >= retryAt) voice.Activate();   // she is the whole beach's voice: always in earshot
        }

        // Her form drifts after the person's gaze, just in front of them and a little below the eyes, beyond where the wrist
        // menu opens (0.5 m) so the menu comes up in front of her without the two passing through each other, and near
        // enough to touch the card: it only moves once the head has turned well away from it, and then glides round. While
        // the wrist menu is open she holds still, so she never drifts into it.
        Vector3 cardTarget; bool cardMoving;
        WristPanel menu; bool menuLooked;
        void PlaceCard(bool snap)
        {
            if (!menuLooked) { menu = FindAnyObjectByType<WristPanel>(); menuLooked = true; }
            if (!snap && menu && menu.IsOpen) { cardMoving = false; return; }
            var f = Vector3.ProjectOnPlane(user.forward, Vector3.up); if (f.sqrMagnitude < 1e-4f) f = Vector3.forward;
            var want = user.position + Quaternion.Euler(0, cardYaw, 0) * f.normalized * cardDistance + Vector3.down * cardDrop;
            var toCard = Vector3.ProjectOnPlane(card.position - user.position, Vector3.up);
            if (snap || Vector3.Angle(toCard, Quaternion.Euler(0, cardYaw, 0) * f) > 32 || Mathf.Abs(card.position.y - want.y) > .25f) cardMoving = true;
            if (cardMoving) cardTarget = want;
            card.position = snap ? want : Vector3.Lerp(card.position, cardTarget, Time.deltaTime * 3);
            if (cardMoving && Vector3.Distance(card.position, cardTarget) < .01f) cardMoving = false;
            var face = card.position - user.position; face.y *= .5f;
            if (face.sqrMagnitude > 1e-4f) card.rotation = Quaternion.LookRotation(face, Vector3.up);
        }

        void Show(string statusLine, string heardLine = null, string doneLine = null, string replyLine = null)
        {
            lastActive = Time.time;
            if (status) status.text = statusLine;
            if (heardLine != null && heard) heard.text = heardLine;
            if (doneLine != null && done) done.text = doneLine;
            if (replyLine != null && reply) reply.text = replyLine;
        }
        string IdleLine => Configured ? Localization.Tr("Say “Calypso” to talk, or tap Talk.") : Localization.Tr("Calypso's microphone isn't set up yet.");
        void Idle() { if (status) status.text = IdleLine; }

        // The Talk pill: the next sentence is for Calypso even without her name.
        void OnTalk()
        {
            if (busy) return;
            if (!Configured) { Show(Localization.Tr("Calypso's microphone isn't set up yet.")); return; }
            if (speaker && speaker.IsActive) speaker.Stop();
            askedAt = Time.time;
            Show(Localization.Tr("Listening…"), "", "");
            if (!voice.IsRequestActive) voice.Activate();
            StartCoroutine(GiveUp());
        }
        IEnumerator GiveUp()
        {
            float at = askedAt;
            yield return new WaitForSeconds(askWindow);
            if (!busy && askedAt == at) { askedAt = -100; Show(Localization.Tr("I didn't catch that. Say “Calypso”, then what you'd like.")); }
        }

        bool Addressed(string text)
        {
            if (Time.time - askedAt < askWindow) return true;
            if (string.IsNullOrEmpty(text)) return false;
            var t = text.ToLowerInvariant();
            foreach (var w in wakeWords) if (Regex.IsMatch(t, @"\b" + Regex.Escape(w) + @"\b")) return true;
            return false;
        }

        void OnPartial(string text) { if (!busy && Addressed(text)) { if (orb) orb.Addressed(); Show(Localization.Tr("Listening…"), "“" + text + "”"); } }
        void OnMicLevel(float level) { if (orb && !(speaker && speaker.IsActive)) orb.Heard(level); }
        void OnError(string error, string message) { Debug.LogWarning($"Calypso: {error} {message}"); retryAt = Time.time + 2; }
        void OnFull(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || busy || !Addressed(text)) return;
            askedAt = -100;
            StartCoroutine(Converse(text));
        }

        IEnumerator Converse(string text)
        {
            busy = true; busySince = Time.time;
            try
            {
                if (voice && voice.IsRequestActive) voice.Deactivate();
                Show(Localization.Tr("Thinking…"), "“" + text + "”", "", "");
                VoiceRelay.Transcript($"Calypso heard \"{text}\"");
                string answer = null; string[] actions = null;
                var url = VoiceRelay.Url(serverUrl);
                if (!string.IsNullOrEmpty(url))
                {
                    var past = new List<Line>(); foreach (var (role, said) in history) past.Add(new Line { role = role, text = said });
                    var body = JsonUtility.ToJson(new Request { text = text, state = Describe(), now = DateTime.Now.ToString("dddd d MMMM yyyy, HH:mm"), history = past.ToArray() });
                    using var req = new UnityWebRequest(url + "/pet/calypso/talk", "POST") { uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)), downloadHandler = new DownloadHandlerBuffer(), timeout = 40 };
                    VoiceRelay.Prepare(req, appToken);
                    UnityWebRequestAsyncOperation sending = null;
                    try { sending = req.SendWebRequest(); } catch (Exception ex) { Debug.LogWarning($"Calypso relay: {ex.Message}"); }
                    if (sending != null) yield return sending;
                    if (sending != null && req.result == UnityWebRequest.Result.Success)
                    {
                        var res = JsonUtility.FromJson<Response>(req.downloadHandler.text);
                        answer = res?.reply; actions = res?.actions;
                    }
                    else if (sending != null) Debug.LogWarning($"Calypso relay: {req.responseCode} {req.error}");
                }
                if (string.IsNullOrEmpty(answer) && (actions == null || actions.Length == 0)) (answer, actions) = Offline(text);
                var labels = new List<string>();
                if (actions != null) foreach (var a in actions) { var l = Act(a); if (!string.IsNullOrEmpty(l)) labels.Add(l); }
                if (string.IsNullOrEmpty(answer)) answer = labels.Count > 0 ? Localization.Tr("There you go.") : Localization.Tr("Mm, I'm here.");
                history.Add(("user", text)); history.Add(("assistant", answer));
                while (history.Count > 12) history.RemoveAt(0);
                VoiceRelay.Transcript($"Calypso: \"{answer}\" {string.Join(", ", labels)}");
                Show(IdleLine, null, string.Join("   ·   ", labels), answer);
                yield return Speak(answer);
            }
            finally { busy = false; Idle(); lastActive = Time.time; }
        }

        IEnumerator Speak(string text)
        {
            // the Wit voices speak English: in other languages Calypso answers on the card only
            if (!speaker || string.IsNullOrEmpty(text) || Localization.Current != 0) yield break;
            if (speaker.IsActive) speaker.Stop();
            foreach (var part in SpokenLines.Sentences(text)) speaker.SpeakQueued(part);
            float until = Time.time + 10 + text.Length * .12f;
            yield return new WaitForSeconds(.3f);
            while (speaker.IsActive && Time.time < until) { lastActive = Time.time; yield return null; }
            if (speaker.IsActive) speaker.Stop();
        }

        // ---- the beach

        static readonly string[] PresetNames = { "morning", "afternoon", "sunset", "night" };

        // What the beach is doing now, for the relay's [beach] line.
        public string Describe()
        {
            var parts = new List<string>();
            if (time)
            {
                if (time.preset == TimeOfDay.Preset.Automatic) parts.Add($"The day's cycle is running; it is about {Clock(time.Current)} on the beach.");
                else parts.Add($"Time of day: {PresetNames[(int)time.preset]} (held, not cycling).");
                parts.Add(time.LampsOn ? "The fire in the fire bowl is lit." : "The fire is out.");
            }
            if (shore) parts.Add(shore.Calm ? "The waves are calmed: the lagoon lies still." : "Small waves are running up the beach.");
            if (hatchlings) parts.Add(hatchlings.Hatching ? "Turtle hatchlings are crawling down to the sea right now." : "No hatchlings are out of the nest at the moment.");
            return string.Join(" ", parts);
        }
        static string Clock(float h) { if (h < 0) h = 0; int hh = Mathf.FloorToInt(h) % 24, mm = Mathf.FloorToInt((h - Mathf.Floor(h)) * 60); return $"{hh:00}:{mm:00}"; }

        // One action from the relay (or from Offline), done on the beach; returns what the card shows for it.
        public string Act(string action)
        {
            var a = (action ?? "").Trim().ToLowerInvariant().Split(' ');
            bool On() => a.Length < 2 || a[1] != "off";
            switch (a[0])
            {
                case "hatch":
                    if (!hatchlings) return null;
                    hatchlings.HatchNow(); return Localization.Tr("Turtles hatching");
                case "time":
                    if (!time || a.Length < 2) return null;
                    int p = Array.IndexOf(PresetNames, a[1]); if (p < 0) return null;
                    time.SetPreset((TimeOfDay.Preset)p); return Localization.Tr(char.ToUpperInvariant(a[1][0]) + a[1].Substring(1));
                case "cycle":
                    if (!time) return null;
                    if (On()) { time.SetPreset(TimeOfDay.Preset.Automatic); return Localization.Tr("Day cycle on"); }
                    // holding the clock: the preset nearest the hour it has reached
                    if (time.preset == TimeOfDay.Preset.Automatic)
                    {
                        int best = 0; float gap = 99;
                        for (int i = 0; i < PresetNames.Length; i++) { float d = Mathf.Abs(Mathf.Repeat(TimeOfDay.PresetHour((TimeOfDay.Preset)i) - time.Current + 12, 24) - 12); if (d < gap) { gap = d; best = i; } }
                        time.SetPreset((TimeOfDay.Preset)best);
                    }
                    return Localization.Tr("Day cycle paused");
                case "waves":
                    if (!shore) return null;
                    shore.Calm = !On(); return Localization.Tr(shore.Calm ? "Waves calmed" : "Waves rolling");
                case "fire":
                    if (!time) return null;
                    time.LampsOn = On(); return Localization.Tr(time.LampsOn ? "Fire lit" : "Fire out");
            }
            return null;
        }

        // Without the relay: the plain commands, done straight away, and a short friendly line.
        (string, string[]) Offline(string text)
        {
            var t = (text ?? "").ToLowerInvariant();
            bool Has(string w) => Regex.IsMatch(t, @"\b" + w + @"\b");
            bool off = Has("off") || Has("out") || Has("stop") || Has("calm") || Has("still") || Has("pause") || Has("put out") || Has("no more");
            var acts = new List<string>();
            if (Has("hatch\\w*") || Has("turtles?")) acts.Add("hatch");
            foreach (var n in PresetNames) if (Has(n)) acts.Add("time " + n);
            if (Has("dawn") && !acts.Contains("time morning")) acts.Add("time morning");
            if (Has("cycle") || Has("time pass\\w*")) acts.Add(off ? "cycle off" : "cycle on");
            if (Has("waves?") || Has("sea") || Has("lagoon")) acts.Add(off ? "waves off" : "waves on");
            if (Has("fire") || Has("campfire") || Has("flames?")) acts.Add(off ? "fire off" : "fire on");
            string say = acts.Count > 0 ? Localization.Tr("There you go. I'll be here if you need anything else.")
                : Localization.Tr("I can't reach my thoughts right now, but I can still hatch the turtles, change the time, calm the waves or put out the fire.");
            return (say, acts.ToArray());
        }

        [Serializable] class Line { public string role, text; }
        [Serializable] class Request { public string text, state, now; public Line[] history; }
        [Serializable] class Response { public string reply; public string[] actions; }
    }
}
