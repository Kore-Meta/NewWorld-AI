using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StudyEverywhere
{
    // The Starry Meadow's settings, opened from the watch (see WristPanel): music volume and track, nature sounds volume, fireflies on or off, the moon's phase and motion (still, rising and setting, or off)
    // (previous / next through all eight), clouds, where to be (standing, sitting or lying back in the grass), view
    // height with recenter, and which wrist wears the watch. Locations swaps the settings for a PlaceChooser (the
    // lobby and every kind of place); its Back returns to the settings. As in the café's WristMenu.
    public sealed class RestMenu : WristPanel
    {
        public NightSky sky;
        public RestAmbience ambience;
        public ParisMusic music;
        public Fireflies fireflies;
        public PlayerPlacement placement;
        [Header("Controls")]
        public Slider volume, musicVolume;
        public TMP_Text volumeValue, musicValue, heightValue, phaseName, trackName;
        public Button trackPrevious, trackNext, firefliesOn, firefliesOff;
        public Button phasePrevious, phaseNext;
        public Button[] clouds, places, moonMotion;
        public Button heightDown, heightUp, recenter, close;
        [Header("Travel")]
        public GameObject settingsPage;
        public PlaceChooser travel;
        public Button locations;

        protected override void Awake()
        {
            base.Awake();
            ShowPlaces(false);
        }

        void OnEnable()
        {
            if (locations) locations.onClick.AddListener(OpenPlaces);
            if (travel) travel.Exited += ClosePlaces;
            volume.onValueChanged.AddListener(SetVolume);
            if (musicVolume) musicVolume.onValueChanged.AddListener(SetMusicVolume);
            if (trackPrevious) trackPrevious.onClick.AddListener(() => music.Step(-1));
            if (trackNext) trackNext.onClick.AddListener(() => music.Step(1));
            if (firefliesOn) firefliesOn.onClick.AddListener(() => fireflies.On = true);
            if (firefliesOff) firefliesOff.onClick.AddListener(() => fireflies.On = false);
            phasePrevious.onClick.AddListener(() => sky.StepPhase(-1));
            phaseNext.onClick.AddListener(() => sky.StepPhase(1));
            for (int i = 0; i < clouds.Length; i++) { int k = i; clouds[i].onClick.AddListener(() => sky.SetClouds((NightSky.Clouds)k)); }
            if (moonMotion != null) for (int i = 0; i < moonMotion.Length; i++) { int k = i; moonMotion[i].onClick.AddListener(() => sky.SetMotion((NightSky.MoonMotion)k)); }
            for (int i = 0; i < places.Length; i++) { int k = i; places[i].onClick.AddListener(() => { placement.GoTo(k); Refresh(); }); }
            heightDown.onClick.AddListener(() => placement.AdjustHeight(-placement.heightStep));
            heightUp.onClick.AddListener(() => placement.AdjustHeight(placement.heightStep));
            recenter.onClick.AddListener(placement.Recenter);
            close.onClick.AddListener(Close);
            if (sky) sky.Changed += Refresh;
            if (ambience) ambience.Changed += Refresh;
            if (music) music.Changed += Refresh;
            if (fireflies) fireflies.Changed += Refresh;
            if (placement) placement.Changed += Refresh;
            Refresh();
        }

        void OnDisable()
        {
            volume.onValueChanged.RemoveAllListeners();
            if (musicVolume) musicVolume.onValueChanged.RemoveAllListeners();
            foreach (var b in new[] { trackPrevious, trackNext, firefliesOn, firefliesOff }) if (b) b.onClick.RemoveAllListeners();
            phasePrevious.onClick.RemoveAllListeners(); phaseNext.onClick.RemoveAllListeners();
            foreach (var b in clouds) b.onClick.RemoveAllListeners();
            if (moonMotion != null) foreach (var b in moonMotion) b.onClick.RemoveAllListeners();
            foreach (var b in places) b.onClick.RemoveAllListeners();
            heightDown.onClick.RemoveAllListeners(); heightUp.onClick.RemoveAllListeners(); recenter.onClick.RemoveAllListeners(); close.onClick.RemoveAllListeners();
            if (sky) sky.Changed -= Refresh;
            if (ambience) ambience.Changed -= Refresh;
            if (music) music.Changed -= Refresh;
            if (fireflies) fireflies.Changed -= Refresh;
            if (placement) placement.Changed -= Refresh;
            if (locations) locations.onClick.RemoveListener(OpenPlaces);
            if (travel) travel.Exited -= ClosePlaces;
        }

        void OpenPlaces() => ShowPlaces(true);
        void ClosePlaces() => ShowPlaces(false);
        void ShowPlaces(bool on)
        {
            if (!travel || !settingsPage) return;
            settingsPage.SetActive(!on);
            travel.gameObject.SetActive(on);
            if (!on) Refresh();
        }

        protected override void OnShown() => Refresh();

        void SetVolume(float v) { if (ambience) ambience.Volume = v; }
        void SetMusicVolume(float v) { if (music) { music.Volume = v; if (music.Muted && v > 0) music.Muted = false; } }

        void Refresh()
        {
            if (ambience) { volume.SetValueWithoutNotify(ambience.Volume); volumeValue.text = Mathf.RoundToInt(ambience.Volume * 100) + "%"; }
            if (music && musicVolume) { musicVolume.SetValueWithoutNotify(music.Volume); musicValue.text = Mathf.RoundToInt(music.Volume * 100) + "%"; }
            if (music && trackName) trackName.text = string.IsNullOrEmpty(music.Title) ? "Music" : music.Title;
            if (fireflies) { if (firefliesOn) WristMenu.Mark(firefliesOn, fireflies.On, dark); if (firefliesOff) WristMenu.Mark(firefliesOff, !fireflies.On, dark); }
            if (sky && phaseName) phaseName.text = NightSky.PhaseNames[(int)sky.phase];
            for (int i = 0; i < clouds.Length; i++) WristMenu.Mark(clouds[i], sky && (int)sky.clouds == i, dark);
            if (moonMotion != null) for (int i = 0; i < moonMotion.Length; i++) WristMenu.Mark(moonMotion[i], sky && (int)sky.motion == i, dark);
            for (int i = 0; i < places.Length; i++) WristMenu.Mark(places[i], placement && placement.Current == i, dark);
            if (placement) heightValue.text = (placement.HeightAdjust >= 0 ? "+" : "-") + Mathf.RoundToInt(Mathf.Abs(placement.HeightAdjust) * 100) + " cm";
        }
    }
}
