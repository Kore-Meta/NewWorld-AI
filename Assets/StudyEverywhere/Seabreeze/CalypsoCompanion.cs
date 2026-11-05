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
        [Tooltip("Wit text-to-speech voice: a soft, smooth woman's voice that flows like water (Nov 3: Rebecca, soft, a touch low and unhurried).")] public string voiceName = "Rebecca";
        [Range(25, 200)] public int pitch = 92;
        [Tooltip("Wit style: default, soft, formal, fast or projected.")] public string voiceStyle = "soft";
        [Tooltip("Speaking speed (100: normal); a little slower reads as calm and flowing.")] [Range(50, 200)] public int voiceSpeed = 88;
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
        [Tooltip("Words that address Calypso (with the ways speech-to-text tends to mishear her name).")]
        public string[] wakeWords = { "calypso", "calipso", "kalypso", "kalipso", "calypsa", "collapso", "clypso", "galipso" };
        [Tooltip("After the Talk pill, the next sentence is for Calypso even without her name (seconds).")] public float askWindow = 8;
        [Header("Voice-first (Nov 3 design review): the orb is the interface")]
        [Tooltip("Show her words as captions beside her orb. Off: voice only. They always show when she can't speak the language.")]
        public bool captions;
        [Tooltip("Icons and spoken phrases that flow out of her orb (added at start if empty).")] public CalypsoCues cues;
        [Tooltip("Her droplets leading the eye (added at start if empty).")] public CalypsoGuide guide;
        [Tooltip("The ring round her orb while she acts (added at start if empty).")] public CalypsoRing ring;
        [Tooltip("Seconds she shows Acting for each change to the beach (she stays quiet meanwhile).")] public float actingFor = 2.2f;
        [Tooltip("After she echoes a request with several actions back, seconds the person has to say “wait”.")] public float waitWindow = 2.5f;
        [Tooltip("Seconds she drifts aside after pointing at something, so it isn't behind her.")] public float asideFor = 8;
        [Tooltip("Degrees further out she drifts when aside, and when docked at rest (headset review, Nov 4: kept small, as far out it hurt to look at her).")]
        public float asideYaw = 10, dockYaw = 6;
        [Tooltip("Seconds of quiet before she docks small at the edge of view (0: never).")] public float dockAfter = 25;
        [Tooltip("Her size when docked, against her full size.")] [Range(.3f, 1)] public float dockScale = .6f;
        [Tooltip("Seconds she takes to appear when the place opens.")] public float arriveFor = 1.6f;
        [Range(0, 1)] public float chimeVolume = .45f, shimmerVolume = .12f;

        bool busy;
        float retryAt, busySince, requestSince = -1, askedAt = -100, lastActive = -100, actingUntil = -1;
        const string MetKey = "Calypso.met";
        static readonly Regex AsksAbilities = new Regex(@"\bwhat (else )?can you do\b|\bwhat do you do\b|\bwhat can i (say|ask)\b|\bhelp\b", RegexOptions.IgnoreCase);
        static readonly Regex WaitWords = new Regex(@"\b(wait|stop|hold on|cancel|never ?mind|no)\b", RegexOptions.IgnoreCase);
        static readonly Regex UndoWords = new Regex(@"\b(change|put|turn|set) it back\b|\bundo\b|\bback (to )?(how|the way) it was\b", RegexOptions.IgnoreCase);

        // the voice-first extras: sounds, the "wait" window, undo, her place (aside, docked) and her arrival
        AudioSource sfx, hum;
        float waitUntil = -1, chimedAt = -100, asideUntil = -1, aside, dock, arrive = 1;
        bool waitHeard;
        Vector3 baseScale = Vector3.one;
        struct Snapshot { public bool valid, onlyHatch; public TimeOfDay.Preset preset; public bool lamps, calm; }
        Snapshot before;
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
            if (speaker) speaker.SetVoiceOverride(new Meta.WitAi.TTS.Integrations.TTSWitVoiceSettings { voice = voiceName, pitch = pitch, style = string.IsNullOrEmpty(voiceStyle) ? "default" : voiceStyle, speed = voiceSpeed });
            Idle();
            if (card && user) PlaceCard(true);
            AddCuesAndGuide();
            AddSounds();
            if (card) { baseScale = card.localScale; StartCoroutine(Arrive()); }
            StartCoroutine(Greet());
            Debug.Log($"Calypso: voice {(Configured ? "configured" : "NOT configured (no Wit configuration)")}, relay {(string.IsNullOrEmpty(VoiceRelay.Url(serverUrl)) ? "not set" : "set")}.");
        }

        bool Configured => voice && voice.RuntimeConfiguration != null && voice.RuntimeConfiguration.witConfiguration;

        void Update()
        {
            if (card && user) PlaceCard(false);
            bool talking = speaker && speaker.IsActive;
            if (orb)
            {
                orb.thinking = busy && !talking;
                orb.mood = Time.time < actingUntil ? CalypsoOrb.Mood.Acting : busy && !talking ? CalypsoOrb.Mood.Thinking : CalypsoOrb.Mood.Idle;
            }
            if (cues) cues.quiet = busy || talking || Time.time - lastActive < 4;
            // thinking has its faint shimmer; acting has the world's own sounds; speaking is her voice
            if (hum) hum.volume = Mathf.MoveTowards(hum.volume, orb && orb.Shown == CalypsoOrb.Mood.Thinking ? shimmerVolume : 0, Time.deltaTime * .4f);
            // her place: aside for a while after pointing at something, docked small at the edge of view when long quiet
            aside = Mathf.MoveTowards(aside, Time.time < asideUntil ? 1 : 0, Time.deltaTime * .8f);
            bool resting = dockAfter > 0 && !busy && !talking && Time.time - lastActive > dockAfter;
            dock = Mathf.MoveTowards(dock, resting ? 1 : 0, Time.deltaTime * (resting ? .35f : 1.5f));
            if (card) card.localScale = baseScale * (arrive * Mathf.Lerp(1, dockScale, dock));
            // voice first: her captions come up only when asked for (or when she can't speak the language), while she is
            // listening, thinking or talking, and fade away once she has been quiet a while. Her name stays in her orb.
            bool showCaptions = captions || Localization.Current != 0;
            if (cardGroup) cardGroup.alpha = Mathf.MoveTowards(cardGroup.alpha, showCaptions && Time.time - lastActive < 12 ? 1 : 0, Time.deltaTime * 1.5f);
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
        Vector3 cardTarget; bool cardMoving; float placedOutward;
        WristPanel menu; bool menuLooked;
        void PlaceCard(bool snap)
        {
            if (!menuLooked) { menu = FindAnyObjectByType<WristPanel>(); menuLooked = true; }
            if (!snap && menu && menu.IsOpen) { cardMoving = false; return; }
            var f = Vector3.ProjectOnPlane(user.forward, Vector3.up); if (f.sqrMagnitude < 1e-4f) f = Vector3.forward;
            // aside after pointing, and docked at rest, she moves further out on her own side, out of the way
            float outward = (cardYaw >= 0 ? 1 : -1) * Mathf.Max(aside * asideYaw, dock * dockYaw);
            float yaw = cardYaw + outward;
            var want = user.position + Quaternion.Euler(0, yaw, 0) * f.normalized * cardDistance + Vector3.down * cardDrop;
            var toCard = Vector3.ProjectOnPlane(card.position - user.position, Vector3.up);
            // she comes along when the person walks or teleports about the beach (headset review, Nov 4: she stayed
            // behind), not only when they turn: further than a few steps off, she glides over; far off, she is there at once
            float off = Vector3.ProjectOnPlane(card.position - want, Vector3.up).magnitude;
            if (off > 3) snap = true;
            if (snap || off > .3f || Mathf.Abs(outward - placedOutward) > 2 || Vector3.Angle(toCard, Quaternion.Euler(0, yaw, 0) * f) > 32 || Mathf.Abs(card.position.y - want.y) > .25f) { cardMoving = true; placedOutward = outward; }
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
            Chime();
            Show(Localization.Tr("Listening…"), "", "");
            if (!voice.IsRequestActive) voice.Activate();
            StartCoroutine(GiveUp());
        }
        // Nothing came: the grey head tilt and a short "Sorry?".
        IEnumerator GiveUp()
        {
            float at = askedAt;
            yield return new WaitForSeconds(askWindow);
            if (!busy && askedAt == at)
            {
                askedAt = -100;
                if (orb) orb.Unsure();
                Show(Localization.Tr("I didn't catch that. Say “Calypso”, then what you'd like."));
                yield return Speak(Localization.Tr("Sorry?"));
            }
        }

        // ---- the voice-first extras (Nov 3 design board, frames 01 to 08)

        // Her cues, guide and ring sit beside her orb; scenes built before them get them here, so no scene change is needed.
        void AddCuesAndGuide()
        {
            if (!orb) return;
            var host = card ? card.gameObject : orb.gameObject;
            if (!guide)
            {
                guide = host.GetComponent<CalypsoGuide>();   // (not ??: Unity's missing components aren't C# null)
                if (!guide) guide = host.AddComponent<CalypsoGuide>();
                guide.from = orb.transform; guide.template = orb.droplets;
            }
            if (!cues)
            {
                cues = host.GetComponent<CalypsoCues>();
                if (!cues) cues = host.AddComponent<CalypsoCues>();
                cues.from = orb.transform;
                var any = reply ? reply : heard;   // her captions' font (Plus Jakarta Sans), as the rest of New World
                if (any) cues.font = any.font;
            }
            if (!cues.guide) cues.guide = guide;
            if (!ring)
            {
                ring = host.GetComponent<CalypsoRing>();
                if (!ring) ring = host.AddComponent<CalypsoRing>();
                ring.from = orb.transform; ring.radius = orb.radius;
            }
        }

        // Her two small sounds, from her orb: the chime when she hears her name, the shimmer while she thinks.
        void AddSounds()
        {
            if (!orb) return;
            sfx = orb.gameObject.AddComponent<AudioSource>(); Spatial(sfx);
            hum = orb.gameObject.AddComponent<AudioSource>(); Spatial(hum);
            hum.clip = CalypsoSounds.Shimmer; hum.loop = true; hum.volume = 0; hum.Play();
        }
        static void Spatial(AudioSource s)
        {
            s.playOnAwake = false; s.spatialBlend = 1; s.dopplerLevel = 0;
            s.rolloffMode = AudioRolloffMode.Linear; s.minDistance = .3f; s.maxDistance = 6;
        }
        void Chime()
        {
            if (sfx && Time.time - chimedAt > 4) sfx.PlayOneShot(CalypsoSounds.Chime, chimeVolume);
            chimedAt = Time.time;   // one chime per sentence, however many partial transcripts it brings
        }

        // 01: she grows into view when the place opens.
        IEnumerator Arrive()
        {
            arrive = .01f;
            yield return new WaitForSeconds(.4f);
            for (float t = 0; t < 1; t += Time.deltaTime / Mathf.Max(.01f, arriveFor)) { arrive = Mathf.SmoothStep(.01f, 1, t); yield return null; }
            arrive = 1;
        }

        // 01 and 02: the first time someone comes to Seabreeze she says hello, and her abilities flow out of her as she
        // names them, each previewed in the world for a moment.
        IEnumerator Greet()
        {
            if (PlayerPrefs.GetInt(MetKey, 0) == 1) yield break;
            yield return new WaitForSeconds(2.5f);
            while (busy) yield return null;
            PlayerPrefs.SetInt(MetKey, 1); PlayerPrefs.Save();
            var hello = Localization.Tr("Hi, I'm Calypso, the voice of this beach. Say my name, then what you'd like. I can change the time of day, hatch the turtles, calm the waves or put out the fire.");
            Show(IdleLine, "", "", hello);
            if (cues) cues.ShowAbilities(3.2f, 1.1f, 5, Preview);
            yield return Speak(hello);
            lastActive = Time.time;
        }

        // 02: the world previews an ability as she names it: the sky warms, the waves still, the nest and the fire glow.
        static readonly Color Warm = new Color(1f, .84f, .6f, .9f);
        int changes;   // counts real changes, so a preview never undoes one made meanwhile
        void Preview(int i)
        {
            var action = CalypsoCues.AbilityActions[i];
            var at = Target(action);
            if (action.StartsWith("hatch") || action.StartsWith("fire")) { if (guide && at.HasValue) guide.Pulse(at.Value, 1.8f); }
            else if (action.StartsWith("waves")) { if (shore && !shore.Calm) StartCoroutine(StillWaves(1.8f)); }
            else if (action.StartsWith("time") && time && time.preset != TimeOfDay.Preset.Sunset && time.preset != TimeOfDay.Preset.Automatic) StartCoroutine(WarmSky(1.8f));
            else if (guide && orb && at.HasValue) guide.Thread(orb.transform.position, at.Value, 1.2f, Warm);   // toward the sun
        }
        IEnumerator StillWaves(float seconds)
        {
            int c = changes; shore.Calm = true;
            yield return new WaitForSeconds(seconds);
            if (changes == c && shore) shore.Calm = false;
        }
        IEnumerator WarmSky(float seconds)
        {
            // the preset eases in over the sky's transition time, so a short preview is a warming, not a jump
            int c = changes; var was = time.preset; time.SetPreset(TimeOfDay.Preset.Sunset);
            yield return new WaitForSeconds(seconds);
            if (changes == c && time) time.SetPreset(was);
        }

        // Where an action happens: the nest, the sun, the waterline in front of her, the fire bowl.
        Vector3? Target(string action)
        {
            var a = (action ?? "").Trim().ToLowerInvariant();
            var o = orb ? orb.transform.position : card ? card.position : transform.position;
            if (a.StartsWith("hatch"))
            {
                if (!hatchlings || hatchlings.homes == null || hatchlings.homes.Length == 0) return null;
                var n = hatchlings.homes[0]; return SeabreezeBeach.OnSand(n.x, n.z);
            }
            if (a.StartsWith("time") || a.StartsWith("cycle")) return time && time.sun ? o - time.sun.transform.forward * 3 : o + Vector3.up * 2.5f;
            if (a.StartsWith("waves")) return SeabreezeBeach.OnSand(o.x, SeabreezeBeach.Shore(o.x));
            if (a.StartsWith("fire")) { var bowl = FindAnyObjectByType<FireBowl>(); return bowl ? bowl.transform.position + Vector3.up * .3f : (Vector3?)null; }
            return null;
        }

        // 04: a request with several actions, said back in order ("Sunset, then the turtles.").
        static string EchoLine(List<string> todo)
        {
            var parts = new List<string>();
            foreach (var t in todo)
            {
                var a = t.Trim().ToLowerInvariant().Split(' ');
                bool on = a.Length < 2 || a[1] != "off";
                switch (a[0])
                {
                    case "time": parts.Add(a.Length > 1 ? a[1] : "the time"); break;
                    case "cycle": parts.Add(on ? "the day cycle" : "pausing the day"); break;
                    case "hatch": parts.Add("the turtles"); break;
                    case "waves": parts.Add(on ? "the waves back" : "calm waves"); break;
                    case "fire": parts.Add(on ? "the fire" : "the fire out"); break;
                }
            }
            var s = string.Join(", then ", parts) + ".";
            return Localization.Tr(char.ToUpperInvariant(s[0]) + s.Substring(1));
        }

        // 06: what she can put back. Hatchlings can't be called back.
        Snapshot Take(List<string> todo) => new Snapshot
        {
            valid = true, onlyHatch = todo.TrueForAll(a => a.Trim().ToLowerInvariant().StartsWith("hatch")),
            preset = time ? time.preset : TimeOfDay.Preset.Afternoon, lamps = time && time.LampsOn, calm = shore && shore.Calm,
        };
        string Undo()
        {
            if (!before.valid) return null;
            var was = before; before.valid = false;
            if (was.onlyHatch) return Localization.Tr("The little ones are already on their way to the sea. I can't call them back.");
            changes++;
            actingUntil = Time.time + actingFor; if (ring) ring.Run(actingFor);
            if (time) { time.SetPreset(was.preset); time.LampsOn = was.lamps; }
            if (shore) shore.Calm = was.calm;
            return Localization.Tr("There, back as it was.");
        }

        bool Addressed(string text)
        {
            if (Time.time - askedAt < askWindow) return true;
            if (string.IsNullOrEmpty(text)) return false;
            var t = text.ToLowerInvariant();
            foreach (var w in wakeWords) if (Regex.IsMatch(t, @"\b" + Regex.Escape(w) + @"\b")) return true;
            return false;
        }

        // While the "wait" window is open, anything heard only counts as "wait" (or not).
        bool InWaitWindow(string text)
        {
            if (Time.time >= waitUntil) return false;
            if (!string.IsNullOrEmpty(text) && WaitWords.IsMatch(text)) waitHeard = true;
            return true;
        }

        void OnPartial(string text)
        {
            if (InWaitWindow(text)) return;
            if (!busy && Addressed(text)) { if (orb) orb.Addressed(); Chime(); Show(Localization.Tr("Listening…"), "“" + text + "”"); }
        }
        void OnMicLevel(float level) { if (orb && !(speaker && speaker.IsActive)) orb.Heard(level); }
        void OnError(string error, string message) { Debug.LogWarning($"Calypso: {error} {message}"); retryAt = Time.time + 2; }
        void OnFull(string text)
        {
            if (InWaitWindow(text)) return;
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

                // 06: "change it back" is hers to do, straight away
                if (UndoWords.IsMatch(text))
                {
                    var back = Undo();
                    if (back != null)
                    {
                        history.Add(("user", text)); history.Add(("assistant", back));
                        Show(IdleLine, null, "", back);
                        yield return new WaitForSeconds(Time.time < actingUntil ? actingFor : 0);
                        yield return Speak(back);
                        yield break;
                    }
                }

                string answer = null; string[] actions = null;
                var url = VoiceRelay.Url(serverUrl);
                yield return VoiceRelay.Check(url, appToken);   // no relay: she does what she can herself at once
                if (!string.IsNullOrEmpty(url) && VoiceRelay.Up)
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

                var todo = new List<string>();
                if (actions != null) foreach (var a in actions) if (CalypsoCues.Look(a, out _, out _)) todo.Add(a);
                Debug.Log($"Calypso: actions [{string.Join(", ", todo)}]");   // what she does to the beach only, never what was said

                // 04: several actions are echoed back in order, with a moment to say "wait"; one goes straight ahead
                CalypsoCues.Handle[] steps = null;
                waitHeard = false;
                if (todo.Count >= 2)
                {
                    if (cues) steps = cues.ShowSteps(todo);
                    yield return Speak(EchoLine(todo));
                    waitUntil = Time.time + waitWindow;
                    if (Configured && !voice.IsRequestActive) voice.Activate();
                    while (Time.time < waitUntil && !waitHeard) yield return null;
                    waitUntil = -1;
                    if (voice && voice.IsRequestActive) voice.Deactivate();
                }

                // 05: one change at a time: amber, the ring filling, a thread of light toward it, the next step waiting
                // dimly; she stays quiet, the beach changing is the confirmation
                var labels = new List<string>(); Vector3? lookAt = null;
                if (waitHeard) answer = Localization.Tr("Okay, I'll wait. Just ask when you're ready.");
                else if (todo.Count > 0)
                {
                    before = Take(todo);
                    for (int i = 0; i < todo.Count; i++)
                    {
                        if (steps != null) for (int j = 0; j < steps.Length; j++) { steps[j].SetLevel(j == i ? 1 : j < i ? .3f : .45f); if (j == i) steps[j].Pop(); }
                        actingUntil = Time.time + actingFor;
                        if (ring) ring.Run(actingFor);
                        var at = Target(todo[i]);
                        if (guide && orb && at.HasValue) guide.Thread(orb.transform.position, at.Value, actingFor, Warm);
                        var l = Act(todo[i]); if (!string.IsNullOrEmpty(l)) labels.Add(l);
                        if (todo[i].Trim().ToLowerInvariant().StartsWith("hatch") && at.HasValue) lookAt = at;
                        yield return new WaitForSeconds(actingFor);
                    }
                }
                if (steps != null) foreach (var s in steps) s.Release();
                if (labels.Count > 0) CalypsoCues.MarkUsed();

                // 06: she drifts aside and leads the eye to what could be missed, and says where to look
                if (lookAt.HasValue)
                {
                    asideUntil = Time.time + asideFor;
                    if (guide) guide.Point(lookAt.Value, asideFor * .75f);
                    if (string.IsNullOrEmpty(answer) || answer.IndexOf("nest", StringComparison.OrdinalIgnoreCase) < 0)
                        answer = (string.IsNullOrEmpty(answer) ? "" : answer + " ") + Localization.Tr("Look down by the nest.");
                }
                if (cues && AsksAbilities.IsMatch(text)) cues.ShowAbilities(1.2f, 1.1f, 5, Preview);
                if (string.IsNullOrEmpty(answer)) answer = labels.Count > 0 ? Localization.Tr("There you go.") : Localization.Tr("Mm, I'm here.");
                history.Add(("user", text)); history.Add(("assistant", answer));
                while (history.Count > 12) history.RemoveAt(0);
                VoiceRelay.Transcript($"Calypso: \"{answer}\" {string.Join(", ", labels)}");
                Show(IdleLine, null, string.Join("   ·   ", labels), answer);
                yield return Speak(answer);
            }
            finally { busy = false; waitUntil = -1; Idle(); lastActive = Time.time; }
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
            changes++;
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

        // Without the relay: the plain commands, done straight away, and a short friendly line. Each clause ("light the
        // fire and calm the waves") is read on its own, so an "off" word in one doesn't turn the other off too, and the
        // everyday ways of saying a time ("make it dark", "golden hour", "daytime") count as well as the preset names.
        static readonly (string words, string preset)[] TimeWords =
        {
            (@"morning|sunrise|dawn|daybreak|early", "morning"),
            (@"afternoon|daytime|day ?light|midday|noon|bright", "afternoon"),
            (@"sunset|sun ?down|dusk|evening|golden hour", "sunset"),
            (@"night|nighttime|midnight|dark|stars", "night"),
        };
        static readonly Regex Clauses = new Regex(@"\s*(?:,|;|\band then\b|\bthen\b|\band\b|\balso\b)\s*", RegexOptions.IgnoreCase);
        (string, string[]) Offline(string text)
        {
            var acts = new List<string>();
            void Add(string a) { if (!acts.Contains(a)) acts.Add(a); }
            foreach (var clause in Clauses.Split((text ?? "").ToLowerInvariant()))
            {
                var t = clause;
                bool Has(string w) => Regex.IsMatch(t, @"\b(?:" + w + @")\b");
                bool off = Has(@"off|out|stop\w*|calm\w*|still|quiet|settle|smooth|pause\w*|freeze|hold|extinguish|no more|end");
                if (Has(@"hatch\w*|turtles?|hatchlings?|babies|nest")) Add("hatch");
                if (Has(@"cycle|time pass\w*|time go\w*|day go\w*")) Add(off ? "cycle off" : "cycle on");
                else foreach (var (words, preset) in TimeWords) if (Has(words)) { Add("time " + preset); break; }
                if (Has(@"waves?|sea|lagoon|ocean|water|tide")) Add(off ? "waves off" : "waves on");
                if (Has(@"fire|campfire|flames?|bonfire|fire ?bowl")) Add(off ? "fire off" : "fire on");
            }
            string say = acts.Count > 0 ? Localization.Tr("There you go. I'll be here if you need anything else.")
                : Localization.Tr("My connection is resting right now, so I can't chat, but I can still change the time, hatch the turtles, calm the waves or put out the fire.");
            return (say, acts.ToArray());
        }

        [Serializable] class Line { public string role, text; }
        [Serializable] class Request { public string text, state, now; public Line[] history; }
        [Serializable] class Response { public string reply; public string[] actions; }
    }
}
