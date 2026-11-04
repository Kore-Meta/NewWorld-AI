using System;
using System.Collections;
using System.Linq;
using System.Text;
using Meta.WitAi.TTS.Utilities;
using Oculus.Voice;
using UnityEngine;
using UnityEngine.Networking;

namespace StudyEverywhere
{
    // Orbit, the library's guide: the voice of the holodeck. Speech-to-text runs on Wit.ai through Meta's Voice SDK
    // (as the café cats); a sentence that names Orbit, or anything said just after the Ask pill, goes with the deck's
    // state to our relay server (Tools/CatServer, /orbit/talk), where Claude answers from the craft catalogue and may
    // act on the deck (show a craft, spread its parts, point at one); the reply is spoken in a calm, friendly voice
    // through the Voice SDK's text-to-speech and typed out on the deck's card, which also shows what was heard.
    // Without the server, Orbit still answers from the catalogue itself so the deck is never mute.
    public sealed class OrbitAssistant : MonoBehaviour
    {
        [Header("Voice SDK")]
        public AppVoiceExperience voice;
        public TTSSpeaker speaker;
        [Tooltip("Wit text-to-speech voice (a friendly, calm female voice) and pitch.")] public string voiceName = "Rebecca";
        [Range(25, 200)] public int pitch = 100;
        [Header("Relay server (Tools/CatServer)")]
        public string serverUrl = "", appToken = "";
        [Header("Deck")]
        public HoloDeck deck;
        public Transform user;
        [Tooltip("The microphone stays open while the person is within this distance of the deck (metres).")] public float listenRange = 4.5f;
        [Tooltip("Words that address Orbit.")] public string[] wakeWords = { "orbit" };
        [Tooltip("After the Ask pill, the next sentence is for Orbit even without her name (seconds).")] public float askWindow = 8;

        bool busy, waitingForWords, serverWarned;
        float retryAt, busySince, requestSince = -1, askedAt = -100, lastPartialLog;

        // Lines played whole rather than streamed (see WholeClipAudioPlayer). The builder adds it; this covers a scene built
        // before it did. TTSSpeaker looks for its player in Start, after every Awake.
        void Awake()
        {
            if (speaker && speaker.GetComponent<Meta.Voice.Audio.IAudioPlayer>() == null) speaker.gameObject.AddComponent<WholeClipAudioPlayer>();
            // a runtime clip cache: without one every line is fetched again each time (the SDK gives each request a new id),
            // and lines fetched ahead (Prefetch) could not be found when they are spoken
            var tts = speaker ? speaker.TTSService : null;
            if (tts && tts.RuntimeCacheHandler == null)
            {
                var cache = tts.gameObject.AddComponent<Meta.WitAi.TTS.Integrations.TTSRuntimeLRUCache>();
                cache.ClipLimit = false; cache.RamLimit = true; cache.RamCapacity = 16000;
                tts.RuntimeCacheHandler = cache;
            }
        }

        // Lines fetched from Wit ahead of time (a mission's stage names when it starts), so each plays the moment it is
        // asked for instead of a second or two later.
        public void Prefetch(System.Collections.Generic.IEnumerable<string> lines)
        {
            if (!speaker || Localization.Current != 0 || !speaker.TTSService) return;
            foreach (var line in lines)
                if (!string.IsNullOrWhiteSpace(line))
                    foreach (var part in SpokenLines.Sentences(line)) speaker.TTSService.Load(part, speaker.VoiceSettings);
        }

        void OnEnable()
        {
            if (deck) { deck.AskPressed += OnAsk; deck.Asked += OnChip; }
            if (!voice) return;
            voice.VoiceEvents.OnPartialTranscription.AddListener(OnPartial);
            voice.VoiceEvents.OnFullTranscription.AddListener(OnFull);
            voice.VoiceEvents.OnError.AddListener(OnError);
            // what the voice service is doing, in the device log (adb logcat -s Unity), to see where a request stalls
            voice.VoiceEvents.OnStartListening.AddListener(LogStart);
            voice.VoiceEvents.OnStoppedListening.AddListener(LogStop);
            voice.VoiceEvents.OnMicLevelChanged.AddListener(LogLevel);
        }
        void OnDisable()
        {
            if (deck) { deck.AskPressed -= OnAsk; deck.Asked -= OnChip; }
            if (!voice) return;
            voice.VoiceEvents.OnStartListening.RemoveListener(LogStart);
            voice.VoiceEvents.OnStoppedListening.RemoveListener(LogStop);
            voice.VoiceEvents.OnMicLevelChanged.RemoveListener(LogLevel);
            if (voice.IsRequestActive) voice.Deactivate();
            voice.VoiceEvents.OnPartialTranscription.RemoveListener(OnPartial);
            voice.VoiceEvents.OnFullTranscription.RemoveListener(OnFull);
            voice.VoiceEvents.OnError.RemoveListener(OnError);
            busy = false;
        }

        void Start()
        {
            if (speaker) SpokenLines.Voice(speaker, voiceName, pitch);
            Debug.Log($"Orbit: voice {(Configured ? "configured" : "NOT configured (no Wit configuration)")}, relay {(string.IsNullOrEmpty(VoiceRelay.Url(serverUrl)) ? "not set" : "set")}.");
        }
        float loudest; float loudestAt;
        void LogStart() => Debug.Log("Orbit: microphone listening.");
        void LogStop() => Debug.Log($"Orbit: microphone stopped (loudest level {loudest:F2}).");
        void LogLevel(float level) { if (level > loudest || Time.time - loudestAt > 5) { loudest = level; loudestAt = Time.time; } }

        bool Configured => voice && voice.RuntimeConfiguration != null && voice.RuntimeConfiguration.witConfiguration;
        bool Near => user && deck && deck.stage && Vector3.Distance(user.position, deck.stage.position) < listenRange;

        void Update()
        {
            if (!Configured) return;
            if (busy) { if (Time.time - busySince > 60) { Debug.LogWarning("Orbit: a reply took too long; listening again."); busy = false; } return; }
            // not while she is narrating a mission: the microphone would hear her (the stages say "orbit", her wake word,
            // which started a conversation that then cut the narration off), and opening it mid-line upsets playback
            if (speaker && speaker.IsActive) { if (voice.IsRequestActive) voice.Deactivate(); requestSince = -1; return; }
            if (voice.IsRequestActive)
            {
                if (requestSince < 0) requestSince = Time.time;
                else if (Time.time - requestSince > 20) { Debug.LogWarning("Orbit: listening stuck; restarting the microphone."); voice.Deactivate(); requestSince = -1; }
                return;
            }
            requestSince = -1;
            if (Time.time < retryAt) return;
            if (Near) voice.Activate();
        }

        // The Ask pill: Orbit answers at once (a short spoken greeting, so the button never seems dead), then listens:
        // the next sentence is for her even without her name. Without a microphone she says how to ask instead.
        void OnAsk()
        {
            if (busy) return;
            if (deck && !deck.On) deck.SetPower(true);
            StartCoroutine(Greet());
        }
        IEnumerator Greet()
        {
            busy = true; busySince = Time.time;
            try
            {
                if (voice && voice.IsRequestActive) voice.Deactivate();   // so she does not hear herself
                var shown = deck ? deck.CurrentName : null;
                string line = Configured
                    ? Localization.Format("I'm listening. Ask me anything about {0}, or about space.", string.IsNullOrEmpty(shown) ? Localization.Tr("these spacecraft") : Localization.Tr(shown))
                    : Localization.Tr("My microphone isn't set up yet, so I can't hear you. The card shows everything about what is on the deck.");
                if (deck) { deck.Heard(""); deck.Say(line); }
                yield return Speak(line);
            }
            finally { busy = false; }
            if (!Configured) yield break;
            askedAt = Time.time; waitingForWords = true;
            if (deck) deck.Listening(true);
            voice.Activate();
            yield return new WaitForSeconds(askWindow);
            if (waitingForWords && !busy && deck) { deck.Listening(false); deck.Status(Localization.Tr("I didn't catch that. Try again, say “Orbit” first, or tap a question.")); }
            waitingForWords = false;
        }

        // A question chip: asked as if spoken (no microphone needed).
        void OnChip(string question)
        {
            if (busy || string.IsNullOrWhiteSpace(question)) return;
            if (question.StartsWith("Show the parts")) { deck?.SetExploded(true); }
            else if (question.StartsWith("Next craft")) { deck?.Step(1); question = "Introduce this craft briefly."; }
            StartCoroutine(Converse(question));
        }

        IEnumerator Speak(string text)
        {
            // the Wit voices speak English: in other languages Orbit answers in captions only
            if (!speaker || string.IsNullOrEmpty(text) || Localization.Current != 0) yield break;
            if (speaker.IsActive) speaker.Stop();   // a question asked mid-mission: the answer replaces the narration
            foreach (var part in SpokenLines.Sentences(text)) speaker.SpeakQueued(part);
            float until = Time.time + 10 + text.Length * .12f;
            yield return new WaitForSeconds(.3f);
            while (speaker.IsActive && Time.time < until) yield return null;
            if (speaker.IsActive) speaker.Stop();
        }

        // The deck's missions: Orbit reads out each stage (English voices only; not over a conversation in progress).
        // Stages queue behind the one being read, so a line is never cut off by the next stage; `interrupt` (a stage
        // the person skipped to) drops what is still being said first.
        public bool Narrating => speaker && speaker.IsActive && !busy;
        public void Narrate(string text, bool interrupt = false)
        {
            if (!speaker || busy || string.IsNullOrEmpty(text) || Localization.Current != 0) return;
            if (interrupt && speaker.IsActive) speaker.Stop();
            foreach (var part in SpokenLines.Sentences(text)) speaker.SpeakQueued(part);
        }
        public void StopNarration() { if (speaker && !busy && speaker.IsActive) speaker.Stop(); }

        bool Addressed(string text)
        {
            if (Time.time - askedAt < askWindow) return true;
            if (string.IsNullOrEmpty(text)) return false;
            var t = text.ToLowerInvariant();
            foreach (var w in wakeWords) if (System.Text.RegularExpressions.Regex.IsMatch(t, @"\b" + System.Text.RegularExpressions.Regex.Escape(w) + @"\b")) return true;
            return false;
        }

        void OnPartial(string text)
        {
            if (Time.time - lastPartialLog > 1) { Debug.Log($"Orbit: hearing \"{text}\""); lastPartialLog = Time.time; }
            if (!busy && deck && Addressed(text)) { deck.Listening(true); deck.Heard(text); }
        }
        void OnError(string error, string message) { Debug.LogWarning($"Orbit: {error} {message}"); retryAt = Time.time + 2; if (deck) deck.Listening(false); }
        void OnFull(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || busy) return;
            if (!Addressed(text)) { if (deck) deck.Listening(false); return; }
            askedAt = -100; waitingForWords = false;
            StartCoroutine(Converse(text));
        }

        IEnumerator Converse(string text)
        {
            busy = true; busySince = Time.time;
            try
            {
                if (voice && voice.IsRequestActive) voice.Deactivate();   // so she does not hear herself answer
                if (deck) { deck.Listening(false); if (!deck.On) deck.SetPower(true); deck.Status("“" + text + "”   ·   " + Localization.Tr("thinking…")); }
                Debug.Log($"Orbit: heard \"{text}\"");
                bool offline = true;
                string reply = null; string[] actions = null;
                // the relay's address and token from Resources/VoiceRelay.json (VoiceRelay), as the pets and Calypso: never saved in the scene
                var url = VoiceRelay.Url(serverUrl);
                yield return VoiceRelay.Check(url, appToken);   // no relay: the fact sheet answers at once
                if (!string.IsNullOrEmpty(url) && VoiceRelay.Up)
                {
                    var body = JsonUtility.ToJson(new Request { text = text, state = deck ? deck.Describe() : "", lang = Localization.LanguageName });
                    using var req = new UnityWebRequest(url + "/orbit/talk", "POST") { uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)), downloadHandler = new DownloadHandlerBuffer(), timeout = 40 };
                    VoiceRelay.Prepare(req, appToken);
                    UnityWebRequestAsyncOperation sending = null;
                    try { sending = req.SendWebRequest(); } catch (Exception ex) { Debug.LogWarning($"Orbit server: {ex.Message}"); }
                    if (sending != null) yield return sending;
                    if (sending != null && req.result == UnityWebRequest.Result.Success)
                    {
                        var res = JsonUtility.FromJson<Response>(req.downloadHandler.text);
                        reply = res?.reply; actions = res?.actions; offline = false;
                    }
                    else if (sending != null) Debug.LogWarning($"Orbit server: {req.responseCode} {req.error} {req.downloadHandler?.text}");
                }
                if (string.IsNullOrEmpty(reply)) reply = Offline(text);
                if (deck) deck.Status("“" + text + "”" + (offline ? "   ·   " + Localization.Tr("from the fact sheet (Orbit's relay is offline)") : ""));
                if (offline && !serverWarned) { serverWarned = true; Debug.LogWarning("Orbit: relay unreachable; answering from the catalogue."); }
                Debug.Log($"Orbit: \"{reply}\"");
                if (actions != null && deck) foreach (var a in actions) deck.Act(a);
                if (deck) deck.Say(reply);
                yield return Speak(reply);
            }
            finally { busy = false; }
        }

        // Without the relay, Orbit answers from the catalogues: a planet or craft named, a part named, a fun fact, the parts,
        // else what is on the deck.
        // The reply is in the chosen language: the catalogue's English is looked up in Localization's table.
        string Offline(string text)
        {
            var t = (text ?? "").ToLowerInvariant();
            string Tr(string s) => Localization.Tr(s);
            string Pair(string name, string about) => Localization.Format("{0}: {1}", Tr(name), Tr(about));
            bool english = Localization.Current == 0;
            // a phrase that follows a colon mid-sentence: lower-case in English (and Spanish and French), as it is in other scripts
            string Mid(string s) { s = Tr(s) ?? ""; return english ? s.ToLowerInvariant() : Localization.Current >= 3 && s.Length > 0 ? char.ToLowerInvariant(s[0]) + s.Substring(1) : s; }
            string Plain(string name) => name.ToLowerInvariant().Replace("the ", "").Trim();
            bool Has(string words) => System.Text.RegularExpressions.Regex.IsMatch(t, @"\b" + System.Text.RegularExpressions.Regex.Escape(words) + @"\b");   // whole words: "io" is not in "motion"
            // the Solar System's worlds and their moons
            foreach (var b in OrbitSolar.Bodies)
            {
                foreach (var m in b.moons)
                    if (m.name == "The Moon" ? Has("our moon") || Has("the moon") : Has(Plain(m.name)))
                        return Localization.Format("{0}, a moon of {1}: {2} It is {3} km across.", Tr(m.name), english ? b.name.Replace("The ", "the ") : Tr(b.name), Tr(m.about), (m.radiusKm * 2).ToString("N0"));
                if (Has(Plain(b.name))) return Pair(b.name, b.summary);
            }
            // the Universe mode's steps (the most specific name first: "observable universe" before "universe"); the size
            // line is composed in English, so other languages leave it out
            foreach (var s in OrbitSolar.Steps.OrderByDescending(x => x.name.Length))
                if (Has(Plain(s.name)) || Has(s.id.Replace('-', ' ')))
                    return english ? $"{s.name}: {s.about} It is {OrbitSolar.SizeLine(s).ToLowerInvariant()}." : Pair(s.name, s.about);
            var planet = OrbitPlanets.All.FirstOrDefault(pl => t.Contains(pl.name.ToLowerInvariant()));
            if (planet != null) return Pair(planet.name, planet.summary);
            // the black-hole mathematics
            if (t.Contains("aretakis")) return Tr("The Aretakis instability: on an extremal black hole, one whose charge or spin is as large as it can be, a wave's slope at the horizon never fades. It is conserved, and it makes the next derivative grow without end, even though the wave itself dies away. Stefanos Aretakis proved it in 2011. Open the Math mode's Aretakis piece to watch it happen.");
            if (t.Contains("penrose")) return Tr("A Penrose diagram fits all of a spacetime on one page: time runs up, light always travels at 45 degrees, and infinity is squeezed onto the edges. For a black hole it shows why nothing that crosses the horizon can get out: every future path inside ends at the singularity.");
            if (t.Contains("black hole") || t.Contains("event horizon")) return Tr("A black hole is where so much mass is packed so tightly that not even light can climb out. Its edge, the event horizon, is not a surface but a point of no return: for a hole like our Sun's mass it would be about 3 km in radius. The Math mode shows the curved space round one, and its Penrose diagram.");
            if (t.Contains("universe")) { var u = OrbitSolar.Steps[OrbitSolar.Steps.Length - 1]; return Pair(u.name, u.about); }
            if (t.Contains("galaxy") || t.Contains("galaxies")) { var g = Array.Find(OrbitSolar.Steps, s => s.id == "milky-way"); return Pair(g.name, g.about); }
            // nothing named: what the deck is showing now, outside the spacecraft
            if (deck && deck.CurrentMode != HoloDeck.Mode.Craft && !string.IsNullOrEmpty(deck.InfoTitle)) return Pair(deck.InfoTitle, deck.InfoBody);
            var craft = OrbitCraft.All.FirstOrDefault(c => t.Contains(c.name.ToLowerInvariant().Split(' ')[0])) ?? deck?.Craft ?? OrbitCraft.All[0];
            if (deck && deck.Craft != craft) deck.Show(craft.id);
            var part = craft.parts.FirstOrDefault(p => p.name.ToLowerInvariant().Split(' ').Any(w => w.Length > 4 && t.Contains(w)));
            if (part != null) { deck?.FocusPart(part.id); return Pair(part.name, part.summary); }
            if (t.Contains("fun fact") || t.Contains("fact")) return Localization.Format("Here's one about {0}: {1}.", Tr(craft.name), Tr(craft.facts[UnityEngine.Random.Range(0, craft.facts.Length)]));
            if (t.Contains("part")) { deck?.SetExploded(true); return Localization.Format("Here are the parts of {0}. Pinch or grab any piece to look closer; it floats back when you let go.", Tr(craft.name)); }
            if (t.Contains("introduce") || t == "hello") return Localization.Format("This is {0}, by {1}: {2}. Tap Parts to open it up.", Tr(craft.name), craft.maker, Mid(craft.tagline));
            return Pair(craft.name, craft.summary);
        }

        [Serializable] class Request { public string text, state, lang; }
        [Serializable] class Response { public string reply; public string[] actions; }
    }
}
