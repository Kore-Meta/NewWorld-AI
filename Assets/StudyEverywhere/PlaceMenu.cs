using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StudyEverywhere
{
    // The lobby's menu, opened from the watch (see WristPanel and WristWatch): where to go (a PlaceChooser: kinds of
    // place, then locations) with view height, recenter and the watch's wrist below, as in the café's menu.
    public sealed class PlaceMenu : WristPanel
    {
        public PlayerPlacement placement;
        public PlaceChooser places;
        [Header("Controls")]
        public TMP_Text heightValue;
        public Button heightDown, heightUp, recenter, close;
        [Tooltip("Brings back the lobby's welcome and its lessons.")] public Button tutorial;
        public LobbyWelcome welcome;
        [Header("Music (optional)")]
        [Tooltip("The place's own music (the lobby's song): its volume slider.")]
        public ParisMusic music;
        public Slider musicVolume;
        public TMP_Text musicValue;

        void OnEnable()
        {
            if (musicVolume && music) { musicVolume.onValueChanged.AddListener(v => { music.Volume = v; if (music.Muted && v > 0) music.Muted = false; }); music.Changed += Refresh; }
            heightDown.onClick.AddListener(() => placement.AdjustHeight(-placement.heightStep));
            heightUp.onClick.AddListener(() => placement.AdjustHeight(placement.heightStep));
            recenter.onClick.AddListener(placement.Recenter);
            close.onClick.AddListener(Unpin);
            if (tutorial && welcome) tutorial.onClick.AddListener(() => { welcome.Reopen(); Close(); });
            if (placement) placement.Changed += Refresh;
            Refresh();
        }

        void OnDisable()
        {
            heightDown.onClick.RemoveAllListeners(); heightUp.onClick.RemoveAllListeners(); recenter.onClick.RemoveAllListeners(); close.onClick.RemoveAllListeners();
            if (tutorial) tutorial.onClick.RemoveAllListeners();
            if (placement) placement.Changed -= Refresh;
            if (musicVolume) musicVolume.onValueChanged.RemoveAllListeners();
            if (music) music.Changed -= Refresh;
        }

        protected override void OnShown() => Refresh();

        void Refresh()
        {
            if (placement && heightValue) heightValue.text = (placement.HeightAdjust >= 0 ? "+" : "-") + Mathf.RoundToInt(Mathf.Abs(placement.HeightAdjust) * 100) + " cm";
            if (music && musicVolume) { musicVolume.SetValueWithoutNotify(music.Volume); if (musicValue) musicValue.text = Mathf.RoundToInt(music.Volume * 100) + "%"; }
        }
    }
}
