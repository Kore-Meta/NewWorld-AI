using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StudyEverywhere
{
    // Haven's settings, opened from the watch (see WristPanel), in the café menu's two-page layout: music volume and
    // track, the time of day (Morning, Afternoon, Sunset, Night, or Cycle: the clock runs through the whole day), the
    // reading lamps on or off, view height with recenter on the left; where to be (the solo desk, the window seat, the
    // lounge, standing in the hall) and which wrist wears the watch on the right. After dusk the menu turns dark, as in
    // the café. Locations swaps the right page for a PlaceChooser (the lobby and every kind of place); its Back returns here.
    public sealed class HavenMenu : WristPanel
    {
        public ParisMusic music;
        public PlayerPlacement placement;
        public TimeOfDay time;
        [Header("Controls")]
        public Slider musicVolume;
        public TMP_Text musicValue, trackName, heightValue;
        [Tooltip("Optional: the recording's credit (performer, licence), small under the track name; empty when none is needed.")]
        public TMP_Text trackCredit;
        public Button trackPrevious, trackNext, lampsOn, lampsOff;
        [Tooltip("Morning, Afternoon, Sunset, Night, Cycle (TimeOfDay.Preset order).")]
        public Button[] times;
        public Button[] places;
        public Button heightDown, heightUp, recenter, close;
        [Tooltip("Orbit only: the Gravity row (On, Off).")] public OrbitGravity gravity;
        public Button gravityOn, gravityOff;
        [Tooltip("Places with live clouds: the Clouds row (Clear, Some, Cloudy).")] public CloudCover clouds;
        public Button[] cloudPills;
        [Tooltip("Seabreeze: smooths away the writing in the sand.")] public SandWriting sand;
        public Button smoothSand;
        [Tooltip("A place with a listening character (New York's Bubble): its voice link, off when the person turns listening off.")]
        public Behaviour petVoice;
        public Button petListenOn, petListenOff;
        [Tooltip("PlayerPrefs key of that switch (1 listens, 0 not).")] public string petListenKey = "StudyEverywhere.NewYork.PetListens";
        bool PetListens => PlayerPrefs.GetInt(petListenKey, 1) == 1;
        void SetPetListens(bool on) { PlayerPrefs.SetInt(petListenKey, on ? 1 : 0); PlayerPrefs.Save(); if (petVoice) petVoice.enabled = on; Refresh(); }
        [Tooltip("Nature sounds (Seoul's frogs, and any place's ambience that follows StudyEverywhere.NatureVolume): Off sets it to 0, On brings back the level it had.")]
        public Button natureOn, natureOff;
        const string NatureKey = "StudyEverywhere.NatureVolume", NatureBeforeKey = "StudyEverywhere.NatureVolumeBeforeOff";
        static bool NatureOn => PlayerPrefs.GetFloat(NatureKey, .7f) > 0;
        void SetNature(bool on)
        {
            if (on == NatureOn) return;
            if (on) PlayerPrefs.SetFloat(NatureKey, Mathf.Max(.2f, PlayerPrefs.GetFloat(NatureBeforeKey, .7f)));
            else { PlayerPrefs.SetFloat(NatureBeforeKey, PlayerPrefs.GetFloat(NatureKey, .7f)); PlayerPrefs.SetFloat(NatureKey, 0); }
            PlayerPrefs.Save(); Refresh();
        }
        [Header("Travel")]
        public GameObject settingsPage;
        public PlaceChooser travel;
        public Button locations;

        protected override void Awake()
        {
            base.Awake();
            ShowPlaces(false);
            if (petVoice) petVoice.enabled = PetListens;   // the person's choice holds from the start: off, the microphone never opens
        }

        void OnEnable()
        {
            if (locations) locations.onClick.AddListener(OpenPlaces);
            if (travel) travel.Exited += ClosePlaces;
            musicVolume.onValueChanged.AddListener(SetMusicVolume);
            if (trackPrevious) trackPrevious.onClick.AddListener(() => music.Step(-1));
            if (trackNext) trackNext.onClick.AddListener(() => music.Step(1));
            if (lampsOn) lampsOn.onClick.AddListener(() => time.LampsOn = true);
            if (lampsOff) lampsOff.onClick.AddListener(() => time.LampsOn = false);
            for (int i = 0; i < times.Length; i++) { int k = i; times[i].onClick.AddListener(() => time.SetPreset((TimeOfDay.Preset)k)); }
            for (int i = 0; i < places.Length; i++) { int k = i; places[i].onClick.AddListener(() => { if (gravity && !gravity.On) gravity.Land(); placement.GoTo(k); Refresh(); }); }   // Orbit: floating, a place puts you down there with gravity on
            heightDown.onClick.AddListener(() => placement.AdjustHeight(-placement.heightStep));
            heightUp.onClick.AddListener(() => placement.AdjustHeight(placement.heightStep));
            recenter.onClick.AddListener(placement.Recenter);
            close.onClick.AddListener(Close);
            if (gravityOn) gravityOn.onClick.AddListener(() => gravity.Set(true));
            if (gravityOff) gravityOff.onClick.AddListener(() => gravity.Set(false));
            if (gravity) gravity.Changed += Refresh;
            if (cloudPills != null) for (int i = 0; i < cloudPills.Length; i++) { int k = i; if (cloudPills[i]) cloudPills[i].onClick.AddListener(() => { if (clouds) clouds.Current = (CloudCover.Amount)k; Refresh(); }); }
            if (smoothSand && sand) smoothSand.onClick.AddListener(sand.Smooth);
            if (petListenOn) petListenOn.onClick.AddListener(() => SetPetListens(true));
            if (petListenOff) petListenOff.onClick.AddListener(() => SetPetListens(false));
            if (natureOn) natureOn.onClick.AddListener(() => SetNature(true));
            if (natureOff) natureOff.onClick.AddListener(() => SetNature(false));
            if (music) music.Changed += Refresh;
            if (placement) placement.Changed += Refresh;
            if (time) time.Changed += Refresh;
            Refresh();
        }

        void OnDisable()
        {
            musicVolume.onValueChanged.RemoveAllListeners();
            foreach (var b in new[] { trackPrevious, trackNext, lampsOn, lampsOff }) if (b) b.onClick.RemoveAllListeners();
            foreach (var b in times) b.onClick.RemoveAllListeners();
            foreach (var b in places) b.onClick.RemoveAllListeners();
            heightDown.onClick.RemoveAllListeners(); heightUp.onClick.RemoveAllListeners(); recenter.onClick.RemoveAllListeners(); close.onClick.RemoveAllListeners();
            foreach (var b in new[] { gravityOn, gravityOff, smoothSand, petListenOn, petListenOff, natureOn, natureOff }) if (b) b.onClick.RemoveAllListeners();
            if (gravity) gravity.Changed -= Refresh;
            if (cloudPills != null) foreach (var b in cloudPills) if (b) b.onClick.RemoveAllListeners();
            if (music) music.Changed -= Refresh;
            if (placement) placement.Changed -= Refresh;
            if (time) time.Changed -= Refresh;
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

        protected override void OnShown() { SetDark(Night); Refresh(); }

        // After dusk the menu turns dark like the room (it follows the clock, also while open).
        bool Night => time && time.Evening > .5f;
        void Update() { if (Themed && Night != dark) { SetDark(Night); Refresh(); } }

        void SetMusicVolume(float v) { if (music) { music.Volume = v; if (music.Muted && v > 0) music.Muted = false; } }

        void Refresh()
        {
            if (music) { musicVolume.SetValueWithoutNotify(music.Volume); musicValue.text = Mathf.RoundToInt(music.Volume * 100) + "%"; }
            if (music && trackName) trackName.text = string.IsNullOrEmpty(music.Title) ? "Music" : music.Title;
            if (music && trackCredit) trackCredit.text = music.Credit;
            if (time)
            {
                for (int i = 0; i < times.Length; i++) Mark(times[i], (int)time.preset == i);
                if (lampsOn) Mark(lampsOn, time.LampsOn);
                if (lampsOff) Mark(lampsOff, !time.LampsOn);
            }
            for (int i = 0; i < places.Length; i++) Mark(places[i], placement && placement.Current == i);
            if (gravity && gravityOn) Mark(gravityOn, gravity.On);
            if (gravity && gravityOff) Mark(gravityOff, !gravity.On);
            if (petVoice && petListenOn) Mark(petListenOn, PetListens);
            if (petVoice && petListenOff) Mark(petListenOff, !PetListens);
            if (natureOn) Mark(natureOn, NatureOn);
            if (natureOff) Mark(natureOff, !NatureOn);
            if (clouds && cloudPills != null) for (int i = 0; i < cloudPills.Length; i++) if (cloudPills[i]) Mark(cloudPills[i], (int)clouds.Current == i);
            if (placement) heightValue.text = (placement.HeightAdjust >= 0 ? "+" : "-") + Mathf.RoundToInt(Mathf.Abs(placement.HeightAdjust) * 100) + " cm";
        }
    }
}
