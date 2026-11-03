using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StudyEverywhere
{
    // Quick settings in the café, opened from the watch (see WristPanel and WristWatch): music volume, time
    // of day, weather, places to go in the café, view height with recenter, which wrist wears the watch, and whether
    // the café cat is there and whether it listens (Off hides it, or keeps it but stops it listening; both remembered
    // between sessions). Locations
    // swaps the settings for a PlaceChooser (the lobby and every kind of place, then their locations); its Back returns to the settings.
    public sealed class WristMenu : WristPanel
    {
        public ParisMusic music;
        public ParisSky sky;
        public PlayerPlacement placement;
        [Header("Controls")]
        public Slider volume;
        public TMP_Text volumeValue, heightValue;
        public Button[] times, weathers, places;
        public Button heightDown, heightUp, recenter, close;
        [Header("Travel")]
        public GameObject settingsPage;
        public PlaceChooser travel;
        public Button locations;
        [Header("Café cat")]
        public Button petOn, petOff, listenOn, listenOff;
        [Header("Eiffel Tower")]
        [Tooltip("The tower's night lights: the turning beacon and the sparkle.")]
        public Button towerOn, towerOff;
        [Tooltip("The cats (their bubbles go with them).")]
        public GameObject[] pets;
        [Tooltip("The cats' voice (listening and replies), off while the cat is away.")]
        public Behaviour petVoice;

        const string PetKey = "StudyEverywhere.PetOn", ListenKey = "StudyEverywhere.PetListens";
        public bool PetOn => PlayerPrefs.GetInt(PetKey, 1) == 1;
        public bool PetListens => PlayerPrefs.GetInt(ListenKey, 1) == 1;

        static readonly ParisSky.Preset[] TimeOrder = { ParisSky.Preset.Morning, ParisSky.Preset.Afternoon, ParisSky.Preset.Sunset, ParisSky.Preset.Night };

        protected override void Awake()
        {
            base.Awake();
            ShowPlaces(false);
        }

        void Start() => ApplyPet();

        void SetPet(bool on) { PlayerPrefs.SetInt(PetKey, on ? 1 : 0); ApplyPet(); Refresh(); }
        void SetListening(bool on) { PlayerPrefs.SetInt(ListenKey, on ? 1 : 0); ApplyPet(); Refresh(); }
        void ApplyPet()
        {
            bool on = PetOn;
            if (pets != null) foreach (var p in pets) if (p) p.SetActive(on);
            if (petVoice) petVoice.enabled = on && PetListens;   // a cat that is away does not listen either
        }

        void OnEnable()
        {
            if (locations) locations.onClick.AddListener(OpenPlaces);
            if (travel) travel.Exited += ClosePlaces;
            if (petOn) petOn.onClick.AddListener(() => SetPet(true));
            if (petOff) petOff.onClick.AddListener(() => SetPet(false));
            if (listenOn) listenOn.onClick.AddListener(() => SetListening(true));
            if (listenOff) listenOff.onClick.AddListener(() => SetListening(false));
            if (towerOn) towerOn.onClick.AddListener(() => { if (sky) sky.TowerLights = true; Refresh(); });
            if (towerOff) towerOff.onClick.AddListener(() => { if (sky) sky.TowerLights = false; Refresh(); });
            volume.onValueChanged.AddListener(SetVolume);
            for (int i = 0; i < times.Length; i++) { int k = i; times[i].onClick.AddListener(() => SetTime(k)); }
            for (int i = 0; i < weathers.Length; i++) { int k = i; weathers[i].onClick.AddListener(() => SetWeather(k)); }
            for (int i = 0; i < places.Length; i++) { int k = i; places[i].onClick.AddListener(() => { placement.GoTo(k); Refresh(); }); }
            heightDown.onClick.AddListener(() => placement.AdjustHeight(-placement.heightStep));
            heightUp.onClick.AddListener(() => placement.AdjustHeight(placement.heightStep));
            recenter.onClick.AddListener(placement.Recenter);
            close.onClick.AddListener(Unpin);
            if (music) music.Changed += Refresh;
            if (placement) placement.Changed += Refresh;
            Refresh();
        }

        void OnDisable()
        {
            volume.onValueChanged.RemoveAllListeners();
            foreach (var b in times) b.onClick.RemoveAllListeners();
            foreach (var b in weathers) b.onClick.RemoveAllListeners();
            foreach (var b in places) b.onClick.RemoveAllListeners();
            heightDown.onClick.RemoveAllListeners(); heightUp.onClick.RemoveAllListeners(); recenter.onClick.RemoveAllListeners(); close.onClick.RemoveAllListeners();
            if (music) music.Changed -= Refresh;
            if (placement) placement.Changed -= Refresh;
            if (locations) locations.onClick.RemoveListener(OpenPlaces);
            if (travel) travel.Exited -= ClosePlaces;
            if (petOn) petOn.onClick.RemoveAllListeners();
            if (petOff) petOff.onClick.RemoveAllListeners();
            if (listenOn) listenOn.onClick.RemoveAllListeners();
            if (listenOff) listenOff.onClick.RemoveAllListeners();
            if (towerOn) towerOn.onClick.RemoveAllListeners();
            if (towerOff) towerOff.onClick.RemoveAllListeners();
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

        // After dusk the menu turns dark like the café (it follows the sky, also while open).
        bool Night => sky && sky.Evening > .5f;
        void Update() { if (Themed && Night != dark) { SetDark(Night); Refresh(); } }

        void SetVolume(float v) { if (music) { music.Volume = v; if (music.Muted && v > 0) music.Muted = false; } }
        void SetTime(int i) { if (sky) { sky.preset = TimeOrder[i]; Refresh(); } }
        void SetWeather(int i) { if (sky) { sky.weather = (ParisSky.Weather)i; Refresh(); } }

        void Refresh()
        {
            if (music) { volume.SetValueWithoutNotify(music.Volume); volumeValue.text = Mathf.RoundToInt(music.Volume * 100) + "%"; }
            for (int i = 0; i < times.Length; i++) Mark(times[i], sky && sky.preset == TimeOrder[i], dark);
            for (int i = 0; i < weathers.Length; i++) Mark(weathers[i], sky && (int)sky.weather == i, dark);
            for (int i = 0; i < places.Length; i++) Mark(places[i], placement && placement.Current == i, dark);
            if (petOn) Mark(petOn, PetOn, dark);
            if (petOff) Mark(petOff, !PetOn, dark);
            if (listenOn) Mark(listenOn, PetListens, dark);
            if (listenOff) Mark(listenOff, !PetListens, dark);
            if (towerOn && sky) Mark(towerOn, sky.TowerLights, dark);
            if (towerOff && sky) Mark(towerOff, !sky.TowerLights, dark);
            if (placement) heightValue.text = (placement.HeightAdjust >= 0 ? "+" : "-") + Mathf.RoundToInt(Mathf.Abs(placement.HeightAdjust) * 100) + " cm";
        }

        // Selected pills are sage with ivory text; the others ivory with ink text (or, in dark mode, slate with ivory text).
        public static void Mark(Button b, bool on, bool dark = false)
        {
            var c = b.colors;
            c.normalColor = on ? UIStyle.Sage : dark ? UIStyle.NightPill : UIStyle.WarmIvory;
            b.colors = c;
            var label = b.GetComponentInChildren<TMP_Text>();
            if (label) label.color = on ? UIStyle.WarmIvory : dark ? UIStyle.NightText : UIStyle.Ink;
        }
    }
}
