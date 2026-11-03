using System;
using UnityEngine;

namespace StudyEverywhere
{
    // A place's soundtrack (the Paris café, the Starry Meadow): plays the playlist in order and loops it while the scene is loaded.
    // Volume and mute are shared with the desk music panel and remembered between sessions.
    [RequireComponent(typeof(AudioSource))]
    public sealed class ParisMusic : MonoBehaviour
    {
        public AudioClip[] playlist;
        [Tooltip("Tracks that loop until another is chosen (an ambience such as Orbit's space hum) instead of moving on.")]
        public AudioClip[] loopTracks;
        [Tooltip("A credit for each playlist track (performer, licence), shown under the track name; empty when none is needed.")]
        public string[] credits;
        [Range(0, 1)] public float defaultVolume = 0.6f;
        [Min(0)] public float fadeSeconds = 2;
        // Scales the music down while something else should be heard (Solace Garden's instruments being played).
        [NonSerialized] public float duck = 1;
        [Tooltip("Seconds the music takes to fade out when another sound claims MusicFocus, and to come back after.")] public float focusFadeSeconds = 1;
        float focus = 1;

        const string VolumeKey = "StudyEverywhere.MusicVolume", MuteKey = "StudyEverywhere.MusicMuted";
        AudioSource source;
        int index;
        float fade;
        bool paused, started;

        public event Action Changed;
        public float Volume { get => PlayerPrefs.GetFloat(VolumeKey, defaultVolume); set { PlayerPrefs.SetFloat(VolumeKey, Mathf.Clamp01(value)); Changed?.Invoke(); } }
        public bool Muted { get => PlayerPrefs.GetInt(MuteKey, 0) == 1; set { PlayerPrefs.SetInt(MuteKey, value ? 1 : 0); Changed?.Invoke(); } }
        public bool Paused => paused;
        public bool Playing => started && !paused;
        public string Title => source && source.clip ? source.clip.name : "";
        public string Credit => started && credits != null && index < credits.Length ? credits[index] ?? "" : "";

        void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0;
        }

        void Start() { if (playlist != null && playlist.Length > 0) Play(0); }

        void Update()
        {
            if (!started) return;
            fade = fadeSeconds > 0 ? Mathf.MoveTowards(fade, 1, Time.unscaledDeltaTime / fadeSeconds) : 1;
            // another sound (a Play or Listen button) has the music's attention: fade right out, and back once it is done
            focus = Mathf.MoveTowards(focus, MusicFocus.Busy ? 0 : 1, Time.unscaledDeltaTime / Mathf.Max(.05f, focusFadeSeconds));
            source.volume = Muted ? 0 : Volume * fade * duck * focus;
            // A finished clip stops and rewinds to 0; a system pause keeps its position, so it is not mistaken for the end.
            if (!paused && !source.isPlaying && source.time == 0) Next();
        }

        public void Play(int i)
        {
            if (playlist == null || playlist.Length == 0) return;
            index = (i % playlist.Length + playlist.Length) % playlist.Length;
            source.clip = playlist[index];
            source.loop = loopTracks != null && Array.IndexOf(loopTracks, source.clip) >= 0;
            source.time = 0;
            source.Play();
            paused = false; started = true; fade = 0;
            Changed?.Invoke();
        }
        public void Next() => Play(index + 1);
        // For a track picker: straight to the neighbouring track either way.
        public void Step(int delta) => Play(index + delta);
        // Like a music player: restart the track, or go back one if it has only just started.
        public void Previous() => Play(source.time > 3 ? index : index - 1);
        public void TogglePause()
        {
            if (!started) return;
            if (paused) source.UnPause(); else source.Pause();
            paused = !paused;
            Changed?.Invoke();
        }
        public void ToggleMute() => Muted = !Muted;

        void OnDisable() => PlayerPrefs.Save();
    }
}
