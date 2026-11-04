using Meta.Voice.Audio;
using UnityEngine;

namespace StudyEverywhere
{
    // Plays each text-to-speech line as one finished AudioClip instead of the Voice SDK's streamed one. The SDK's own
    // player (UnityAudioPlayer) loops a one-second streamed clip that pulls samples as they download; when the download
    // falls behind playback (a slow network moment on the Quest), the stream hands back nothing and the clip replays its
    // last block, so the voice sticks on a syllable, and as it times the line by the clock rather than by the audio it
    // stops before the words have all played. Here a line waits until Wit has sent all of it (a second or so; the next
    // lines download while this one plays), then plays from memory, start to end. TTSSpeaker picks up an IAudioPlayer on
    // its own GameObject, so this only has to sit beside it.
    [RequireComponent(typeof(AudioSource))]
    public sealed class WholeClipAudioPlayer : BaseAudioPlayer, IAudioSourceProvider
    {
        [SerializeField] AudioSource source;
        // Timed from the last audio that arrived, not from the start: Wit streams a line at about the pace it is spoken, so
        // a long line (a composer's 15 s sentence) can take longer than this to arrive whole, and timing from the start cut
        // it off in the middle.
        [Tooltip("If a line stops arriving for this many seconds, play what has arrived.")] public float maxWait = 12;

        AudioClip clip;
        bool waiting, paused;
        float waitSince;
        int arrived;

        public AudioSource AudioSource => source;
        public override bool IsPlaying => ClipStream != null && !paused && (waiting || source && source.isPlaying);
        public override bool CanSetElapsedSamples => true;
        public override int ElapsedSamples => !waiting && source && clip && source.clip == clip ? source.timeSamples * clip.channels : 0;

        public override void Init()
        {
            if (!source) source = GetComponent<AudioSource>();
            if (source) { source.playOnAwake = false; source.loop = false; }
        }
        public override string GetPlaybackErrors() => source ? string.Empty : "Audio source is missing";

        protected override void Play(int offsetSamples)
        {
            if (!source) Init();
            paused = false; waiting = true; waitSince = Time.unscaledTime; arrived = 0;
            Update();
        }

        void Update()
        {
            if (!waiting || paused || ClipStream == null) return;
            var s = ClipStream;
            if (s.AddedSamples > arrived) { arrived = s.AddedSamples; waitSince = Time.unscaledTime; }
            bool timedOut = Time.unscaledTime - waitSince > maxWait;
            if (!s.IsComplete && !timedOut) return;
            waiting = false;
            int channels = Mathf.Max(1, s.Channels), frames = s.AddedSamples / channels;
            if (frames <= 0) return;   // nothing arrived: IsPlaying goes false and the speaker moves on
            if (timedOut) Debug.LogWarning("Voice: a line stopped downloading; playing what arrived.");
            var data = new float[frames * channels];
            s.ReadSamples(0, data);
            if (clip) Destroy(clip);
            clip = AudioClip.Create("Orbit line", frames, channels, s.SampleRate, false);
            clip.SetData(data, 0);
            source.clip = clip; source.loop = false; source.timeSamples = 0;
            source.Play();
        }

        public override void Pause()
        {
            if (paused) return;
            paused = true;
            if (source && source.isPlaying) source.Pause();
        }
        public override void Resume()
        {
            if (!paused) return;
            paused = false;
            if (!waiting && source && source.clip == clip && clip) source.UnPause();
        }
        public override void Stop()
        {
            waiting = paused = false;
            if (source) { source.Stop(); if (source.clip == clip) source.clip = null; }
            base.Stop();
        }

        void OnDestroy() { if (clip) Destroy(clip); }
    }
}
