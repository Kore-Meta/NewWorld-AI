using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace StudyEverywhere
{
    // Desk music panel: volume slider, previous / play-pause / next and mute for ParisMusic.
    // A world-space uGUI canvas, so the XR rig's ray and poke interactors drive it like any other UI.
    public sealed class MusicPanel : MonoBehaviour
    {
        public ParisMusic music;
        public Slider volume;
        public Button previous, playPause, next, mute;
        public TMP_Text title, volumeValue, playPauseLabel, muteLabel;
        [Tooltip("Optional: the recording's credit (performer, licence), small under the title; empty when none is needed.")]
        public TMP_Text credit;
        [Tooltip("Optional \"i\" button that shows the place's music credits (wired by MusicCredits).")]
        public Button info;
        [Tooltip("Record icon that turns while music plays.")]
        public RectTransform vinyl;
        public float rpm = 33.3f;

        void Update()
        {
            if (vinyl && music && music.Playing) vinyl.Rotate(0, 0, -rpm * 6 * Time.deltaTime);   // 6 degrees per second per rpm, clockwise
        }

        void Awake()
        {
            // A long title stays on one line under NOW PLAYING: it shrinks to fit (never wrapping onto a second line).
            if (!title) return;
            title.textWrappingMode = TextWrappingModes.NoWrap;
            title.enableAutoSizing = true;
            title.fontSizeMax = title.fontSize; title.fontSizeMin = 16;
            title.overflowMode = TextOverflowModes.Ellipsis;
        }

        void OnEnable()
        {
            if (!music) return;
            music.Changed += Refresh;
            volume.onValueChanged.AddListener(SetVolume);
            previous.onClick.AddListener(music.Previous);
            playPause.onClick.AddListener(music.TogglePause);
            next.onClick.AddListener(music.Next);
            mute.onClick.AddListener(music.ToggleMute);
            Refresh();
        }

        void OnDisable()
        {
            if (!music) return;
            music.Changed -= Refresh;
            volume.onValueChanged.RemoveListener(SetVolume);
            previous.onClick.RemoveListener(music.Previous);
            playPause.onClick.RemoveListener(music.TogglePause);
            next.onClick.RemoveListener(music.Next);
            mute.onClick.RemoveListener(music.ToggleMute);
        }

        void SetVolume(float v) => music.Volume = v;

        void Refresh()
        {
            volume.SetValueWithoutNotify(music.Volume);
            volumeValue.text = music.Muted ? "Muted" : Mathf.RoundToInt(music.Volume * 100) + "%";
            title.text = string.IsNullOrEmpty(music.Title) ? "Paris café music" : music.Title;
            if (credit) credit.text = music.Credit;
            playPauseLabel.text = music.Paused ? "Play" : "Pause";
            muteLabel.text = music.Muted ? "Unmute" : "Mute";
        }
    }
}
