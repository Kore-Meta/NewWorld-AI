using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StudyEverywhere
{
    // The hologram table in the Orbit library. Power raises one of four spacecraft (OrbitCraft) out of the table in
    // the projector's light (in its real finish, the default, or, with Wireframe, as a glowing cyan wireframe) and sets it turning
    // slowly; the arrows change craft; Parts spreads the craft into its labelled pieces
    // and Assemble puts it back. A card beside the model carries the craft's fact sheet, the focused part's story, and
    // Orbit's captions. HoloPart does each piece's grab-and-return; HoloGesture turns, scales and swipes it by hand;
    // OrbitAssistant answers questions and can drive the deck through Act().
    public sealed class HoloDeck : MonoBehaviour
    {
        [Serializable] public class Rig { public string id; public Transform root; public HoloPart[] parts; }

        [Header("Stage")]
        [Tooltip("Where the models hang; it turns. Models are children, one per craft.")] public Transform stage;
        public Rig[] rigs;
        [Tooltip("The faint cyan fill inside the wireframe, and brighter for a held part.")] public Material holo, holoHot;
        [Tooltip("The glowing wireframe lines (each model renderer has a child named \"Wireframe\" drawn with these).")] public Material wire, wireHot;
        [Tooltip("The table's top ring: lit when the deck is on.")] public Renderer glowRing;
        public Material ringOn, ringOff;
        public Light glowLight;
        [Tooltip("The faint cone of light from the ring up to the model; shown while the deck is on.")] public GameObject beam;
        public float spinDegreesPerSecond = 9, riseSeconds = .7f;
        public Vector3 stageHome;
        [Header("Rim panel")]
        [Tooltip("The look the deck starts in: the real finishes (the default), or off for the cyan wireframe.")] public bool startRealistic = true;
        [Tooltip("look: the Realistic pill; wireButton: the Wireframe pill. The two are an on/off pair: the lit one is the look shown.")]
        public Button power, previous, next, parts, ask, look, wireButton, missionButton;
        public TMP_Text powerLabel, partsLabel, lookLabel, missionLabel;
        [Tooltip("Between the arrows: the name of what is shown (craft, world, step, piece); pressing it opens the list.")]
        public Button itemButton; public TMP_Text itemLabel;
        [Tooltip("The list pop-up: every item of the mode, to jump straight to one.")] public GameObject dropdown; public Button[] dropdownItems;
        [Tooltip("The list's Back (to the console; in a world's close-up, back to the whole system too) and close.")] public Button listBack, listClose;
        [Tooltip("Slots that only some modes use (the whole pill, shadow and all): Mission for the craft, Calculator for Math.")]
        public GameObject missionSlot, calculatorSlot; public Button calculator; public TMP_Text calculatorLabel;
        [Tooltip("The console, the card and the pop-ups: the Menu size pills scale them all together (remembered).")]
        public Transform[] menus; public Button menuSmaller, menuLarger;
        [Tooltip("Mission mode: the craft's story played out in miniature (launch, staging, landing; pictures; the Grand Tour).")]
        public HoloMission mission;
        [Header("Modes: Spacecraft, Solar System, Universe (one at a time)")]
        public HoloSolar solar;
        public HoloUniverse universe;
        public HoloMath math;
        [Tooltip("The title band's mode pills: Spacecraft, Solar System, Universe.")] public Button craftMode, solarMode, universeMode, mathMode;
        [Header("Card")]
        public GameObject card;
        public TMP_Text title, maker, tagline, body, facts, partTitle, partBody, heard, caption, hint;
        public GameObject listening;
        [Tooltip("Tap-to-ask questions under the caption: each asks Orbit its label (works without the microphone).")]
        public Button[] chips;
        [Header("Card layout: it grows and shrinks to fit what it shows")]
        [Tooltip("The card's canvas; its top edge stays put and it grows downward.")] public RectTransform cardRect;
        [Tooltip("Shapes drawn behind the card (glow, shadow, edge, face) and how much larger than the card each is.")]
        public RectTransform[] cardFrame; public Vector2[] cardFrameGrow;
        [Tooltip("Rows from top to bottom; hidden rows take no room. Text rows take their text's height.")]
        public RectTransform[] stack;
        public float cardWidth = 1000, padding = 44, spacing = 16;
        [Tooltip("The card turns to face the viewer about this pivot (the stage).")] public bool cardFacesViewer = true;
        [Tooltip("The card keeps its middle at this height (metres), so long and short cards both sit at eye level.")] public float cardEyeHeight = 1.72f;

        int current;
        bool on, exploded, spinPaused, realistic;
        float stageScale, scaleVelocity;
        HoloPart focused;
        Coroutine typing;
        string missionTitle, missionBody;
        public enum Mode { Craft, Solar, Universe, Math }
        Mode mode;
        string infoKicker, infoTitle, infoTagline, infoBody; string[] infoFacts;
        public event Action Changed;
        public event Action AskPressed;
        public event Action<string> Asked;   // a question chip was tapped
        bool layoutDirty = true;

        public bool On => on;
        public bool Exploded => exploded;
        public bool Realistic => realistic;
        public bool MissionPlaying => mission && mission.Playing;
        public int Current => current;
        public OrbitCraft.Craft Craft => current >= 0 && current < rigs.Length ? OrbitCraft.Find(rigs[current].id) : null;
        public HoloPart Focused => focused;
        public bool SpinPaused { get => spinPaused; set => spinPaused = value; }
        public Mode CurrentMode => mode;
        // What the card shows in the Solar System, Universe and Math modes, and the name of whatever is on the deck now.
        public string InfoTitle => infoTitle;
        public string InfoBody => infoBody;
        public string CurrentName { get { var n = ItemNames(); int i = ItemIndex(); return i >= 0 && i < n.Length ? n[i] : null; } }

        void Awake()
        {
            if (stage) stageHome = stage.localPosition;
            foreach (var r in rigs) { if (r.root) { r.root.localScale = Vector3.zero; r.root.gameObject.SetActive(false); } foreach (var p in r.parts) if (p) p.deck = this; }
            foreach (var m in new[] { solar ? solar.root : null, universe ? universe.root : null, math ? math.root : null }) if (m) { m.localScale = Vector3.zero; m.gameObject.SetActive(false); }
            if (menus != null) { menuBase = new Vector3[menus.Length]; for (int i = 0; i < menus.Length; i++) if (menus[i]) menuBase[i] = menus[i].localScale; }
            try { menuStep = Mathf.Clamp(PlayerPrefs.GetInt(MenuSizeKey, 1), 0, MenuSizes.Length - 1); } catch { menuStep = 1; }
            ApplyMenuSize();
            realistic = startRealistic;   // the real finishes are the default look; Wireframe is the person's choice
            Apply();
        }
        // dress every model in the starting look once the modes have woken (their Reskin reads their own fields)
        void Start() => SetRealistic(realistic);

        // ---- modes: the spacecraft (with their parts and missions), the Solar System, the Universe size steps
        public void SetMode(Mode value)
        {
            if (!on) on = true;
            if (value == Mode.Solar && !solar || value == Mode.Universe && !universe || value == Mode.Math && !math) return;
            if (mode != value)
            {
                if (MissionPlaying) mission.Stop();
                exploded = false; Focus(null, false);
                mode = value; ResetPose(); if (dropdown) dropdown.SetActive(false);
                if (mode == Mode.Solar) solar.ShowSystem();
                if (mode == Mode.Universe) universe.Restart();
                if (mode == Mode.Math) math.Replay();
            }
            Apply(); Changed?.Invoke();
        }
        // What the card shows in the Solar System and Universe modes (they call this as the view changes).
        public void Info(string kicker, string title, string tagline, string body, System.Collections.Generic.IEnumerable<string> facts)
        {
            infoKicker = kicker; infoTitle = title; infoTagline = tagline; infoBody = body; infoFacts = facts?.ToArray();
            Apply(); Changed?.Invoke();
        }

        void OnEnable()
        {
            Localization.Changed += Refresh;   // labels and the card set from code follow the chosen language
            if (power) power.onClick.AddListener(TogglePower);
            if (previous) previous.onClick.AddListener(() => Step(-1));
            if (next) next.onClick.AddListener(() => Step(1));
            if (parts) parts.onClick.AddListener(PartsPressed);
            if (craftMode) craftMode.onClick.AddListener(() => SetMode(Mode.Craft));
            if (solarMode) solarMode.onClick.AddListener(() => SetMode(Mode.Solar));
            if (universeMode) universeMode.onClick.AddListener(() => SetMode(Mode.Universe));
            if (mathMode) mathMode.onClick.AddListener(() => SetMode(Mode.Math));
            if (itemButton) itemButton.onClick.AddListener(ToggleDropdown);
            if (listBack) listBack.onClick.AddListener(Back);
            if (listClose) listClose.onClick.AddListener(() => { if (dropdown) dropdown.SetActive(false); Apply(); });
            if (menuSmaller) menuSmaller.onClick.AddListener(() => ResizeMenus(-1));
            if (menuLarger) menuLarger.onClick.AddListener(() => ResizeMenus(1));
            if (dropdownItems != null) for (int i = 0; i < dropdownItems.Length; i++) { int k = i; if (dropdownItems[i]) dropdownItems[i].onClick.AddListener(() => ChooseItem(k)); }
            if (calculator) calculator.onClick.AddListener(() => { if (math) math.TogglePopup(); if (dropdown && math && math.PopupOpen) dropdown.SetActive(false); Apply(); });
            if (ask) ask.onClick.AddListener(() => AskPressed?.Invoke());
            if (look) look.onClick.AddListener(() => SetRealistic(wireButton ? true : !realistic));
            if (wireButton) wireButton.onClick.AddListener(() => SetRealistic(false));
            if (missionButton) missionButton.onClick.AddListener(ToggleMission);
            if (chips != null) foreach (var c in chips) if (c) { var label = c.GetComponentInChildren<TMP_Text>(); c.onClick.AddListener(() => Asked?.Invoke(label ? label.text : "")); }
        }
        void OnDisable()
        {
            Localization.Changed -= Refresh;
            foreach (var b in new[] { power, previous, next, parts, ask, look, wireButton, missionButton, craftMode, solarMode, universeMode, mathMode, itemButton, calculator, menuSmaller, menuLarger, listBack, listClose }) if (b) b.onClick.RemoveAllListeners();
            if (dropdownItems != null) foreach (var b in dropdownItems) if (b) b.onClick.RemoveAllListeners();
            if (chips != null) foreach (var c in chips) if (c) c.onClick.RemoveAllListeners();
        }

        // ---- controls (the rim panel, gestures and Orbit all come through here)
        public void TogglePower() => SetPower(!on);
        public void SetPower(bool value)
        {
            if (on == value) return;
            on = value;
            if (!on) { exploded = false; Focus(null, false); if (MissionPlaying) mission.Stop(); }
            Apply(); Changed?.Invoke();
        }
        // The arrows (and the sweep gesture) change craft; during a mission they step through its stages; in the Solar
        // System they go world by world, in the Universe step by step.
        public void Step(int dir)
        {
            if (!on) SetPower(true);
            if (mode == Mode.Solar) { solar.Step(dir); return; }
            if (mode == Mode.Universe) { universe.Next(dir); return; }
            if (mode == Mode.Math) { math.Step(dir); return; }
            if (rigs.Length == 0) return;
            if (MissionPlaying) { mission.Skip(dir); return; }
            Show((current + dir + rigs.Length) % rigs.Length);
        }
        // The Parts pill: Parts / Assemble for a craft, back to the whole Solar System, back to the start of the Universe.
        void PartsPressed()
        {
            if (mode == Mode.Solar) { if (!on) SetPower(true); solar.ShowSystem(); return; }
            if (mode == Mode.Universe) { if (!on) SetPower(true); universe.Restart(); return; }
            if (mode == Mode.Math) { if (!on) SetPower(true); math.Replay(); return; }
            SetExploded(!exploded);
        }
        public void Show(int index)
        {
            if (index < 0 || index >= rigs.Length) return;
            if (!on) on = true;
            if (mode != Mode.Craft) { mode = Mode.Craft; ResetPose(); }
            if (index != current) { exploded = false; Focus(null, false); current = index; if (MissionPlaying) mission.Stop(); }
            Apply(); Changed?.Invoke();
        }
        public bool Show(string craftId) { int i = Array.FindIndex(rigs, r => r.id == craftId); if (i < 0) return false; Show(i); return true; }
        public void SetExploded(bool value)
        {
            if (!on) SetPower(true);
            if (value && mode != Mode.Craft) SetMode(Mode.Craft);
            if (value && MissionPlaying) mission.Stop();
            if (exploded == value) return;
            exploded = value;
            if (!exploded) Focus(null, false);
            Apply(); Changed?.Invoke();
        }
        public bool FocusPart(string partId)
        {
            var part = rigs[current].parts.FirstOrDefault(p => p && p.id == partId);
            if (!part) return false;
            if (!exploded) SetExploded(true);
            Focus(part, true);
            return true;
        }

        // A model piece in the deck's look: the real finish, or the cyan wireframe (its child "Wireframe" lit, the piece
        // itself a faint fill), brighter while held. Pieces with no real finish stay wireframe.
        public const string WireName = "Wireframe";
        public static Renderer WireOf(Renderer r) { var t = r ? r.transform.Find(WireName) : null; return t ? t.GetComponent<Renderer>() : null; }
        public void Dress(Renderer r, Material real, bool hot, bool realLook)
        {
            if (!r) return;
            bool showReal = realLook && real;
            var m = showReal ? real : hot ? holoHot : holo;
            if (r.sharedMaterial != m) r.sharedMaterial = m;
            var w = WireOf(r); if (!w) return;
            w.enabled = !showReal;
            var wm = hot && wireHot ? wireHot : wire;
            if (wm && w.sharedMaterial != wm) w.sharedMaterial = wm;
        }

        // Realistic: every part in its real finish (paint, foil, solar cells), lit white; or back to the cyan wireframe.
        public void SetRealistic(bool value)
        {
            realistic = value;
            foreach (var r in rigs) foreach (var p in r.parts) if (p) p.Skin();
            if (mission) mission.Reskin();
            if (solar) solar.Reskin();
            if (universe) universe.Reskin();
            if (math) math.Reskin();
            if (glowLight) glowLight.color = realistic ? new Color(1, .97f, .92f) : new Color(.4f, .85f, 1);
            Apply(); Changed?.Invoke();
        }

        // Mission mode: play the current craft's story, or stop it and bring the craft back.
        public void ToggleMission() => SetMission(!MissionPlaying);
        public void SetMission(bool value)
        {
            if (!mission) return;
            if (value && !on) SetPower(true);
            if (value && mode != Mode.Craft) { mode = Mode.Craft; ResetPose(); }
            if (value == MissionPlaying) return;
            if (value) { exploded = false; Focus(null, false); if (!mission.Play(rigs[current].id)) return; }
            else mission.Stop();
            Apply(); Changed?.Invoke();
        }
        // The stage being played, on the card in place of the fact sheet (null: back to the fact sheet).
        public void MissionText(string title, string body) { missionTitle = title; missionBody = body; RefreshCard(); }
        public void MissionEnded() { missionTitle = missionBody = null; Apply(); Changed?.Invoke(); }

        // A hovered part shows its story on the card; a held one also lights up and stays focused when the hand leaves.
        public void Focus(HoloPart part, bool strong)
        {
            if (focused && focused != part && focused.Held) return;   // a held part keeps the card while something else is brushed past
            if (focused && focused != part) focused.SetHot(false);
            focused = part;
            if (focused) focused.SetHot(true);
            RefreshCard();
        }

        // Actions Orbit may send back: "power on|off", "show_craft <id>", "parts", "assemble", "focus_part <id>", "next", "previous",
        // "mission" (play the craft's mission), "stop_mission".
        public void Act(string action)
        {
            if (string.IsNullOrWhiteSpace(action)) return;
            var words = action.Trim().Split(' ', 2);
            switch (words[0])
            {
                case "power": SetPower(words.Length < 2 || words[1] != "off"); break;
                case "show_craft": if (words.Length > 1) Show(words[1].Trim()); break;
                case "parts": SetExploded(true); break;
                case "assemble": SetExploded(false); break;
                case "focus_part": if (words.Length > 1) FocusPart(words[1].Trim()); break;
                case "next": Step(1); break;
                case "previous": Step(-1); break;
                case "mission": case "play_mission": SetMission(true); break;
                case "stop_mission": SetMission(false); break;
                case "spacecraft": SetMode(Mode.Craft); break;
                case "solar_system": SetMode(Mode.Solar); break;
                case "universe": SetMode(Mode.Universe); break;
                case "math": SetMode(Mode.Math); break;
                case "math_preset": if (words.Length > 1 && math) { SetMode(Mode.Math); math.Preset(words[1].Trim()); } break;
                case "show_planet":
                    if (words.Length > 1 && solar) { int i = Array.FindIndex(OrbitSolar.Bodies, b => b.id == words[1].Trim()); if (i >= 0) { SetMode(Mode.Solar); solar.Pick(i); } }
                    break;
                case "universe_step":
                    if (words.Length > 1 && universe) { int i = Array.FindIndex(OrbitSolar.Steps, s => s.id == words[1].Trim()); if (i >= 0) { SetMode(Mode.Universe); universe.Go(i); } }
                    break;
            }
        }

        // What Orbit is told about the deck with each question.
        public string Describe()
        {
            if (!on) return "The deck is off (dark).";
            if (mode == Mode.Solar)
            {
                var b = solar.Body;
                return b == null ? "The deck shows the Solar System mode: an orrery of the Sun, the eight planets and Pluto (not to scale)."
                    : $"The deck shows the Solar System mode, with {b.name} ({b.id}) up close" + (b.moons.Length > 0 ? " and its moons " + string.Join(", ", b.moons.Select(m => m.name)) : "") + ".";
            }
            if (mode == Mode.Math) return $"The deck shows the Math mode: {math.Current} ({(math.Current == HoloMath.Kind.Custom ? "a graph the person typed" : "a black-hole visualisation")}).";
            if (mode == Mode.Universe) { var s = universe.Step; return $"The deck shows the Universe size comparison, step {universe.Current + 1} of {OrbitSolar.Steps.Length}: {s.name} ({s.id}), shown beside everything smaller."; }
            var c = Craft; if (c == null) return "The deck is on with nothing shown.";
            if (MissionPlaying) return $"The deck is playing {c.name}'s mission animation, at the stage \"{mission.StageTitle}\".";
            return $"The deck shows {c.name} ({c.id}), {(exploded ? "spread into its parts" : "assembled")}" + (focused ? $"; the person is holding or pointing at the {focused.name.ToLowerInvariant()} ({focused.id})." : ".");
        }

        // ---- gestures (HoloGesture)
        public void Turn(float yawDegrees, float pitchDegrees, Vector3 pitchAxis)
        {
            if (!stage || !on) return;
            stage.rotation = Quaternion.AngleAxis(yawDegrees, Vector3.up) * Quaternion.AngleAxis(pitchDegrees, pitchAxis) * stage.rotation;
        }
        public void ScaleBy(float factor)
        {
            if (!stage || !on) return;
            stageScale = Mathf.Clamp(stageScale * factor, .5f, 2.2f);
        }
        public void Nudge(Vector3 worldDelta)
        {
            if (!stage || !on || !stage.parent) return;
            var local = stage.localPosition + stage.parent.InverseTransformVector(worldDelta);
            var off = local - stageHome; off = Vector3.ClampMagnitude(off, .45f); if (stageHome.y + off.y < stageHome.y - .1f) off.y = -.1f;
            stage.localPosition = stageHome + off;
        }
        public void ResetPose() { if (stage) { stage.localPosition = stageHome; stage.localRotation = Quaternion.identity; } stageScale = 1; }

        // ---- captions (OrbitAssistant)
        public void Listening(bool value) { if (listening && listening.activeSelf != value) listening.SetActive(value); if (value) Status(Localization.Tr("Listening…")); }
        public void Heard(string text) { if (heard) { heard.text = string.IsNullOrEmpty(text) ? "" : "“" + text + "”"; layoutDirty = true; } }
        // A short line in the heard row: what Orbit is doing ("Listening…", "Thinking…") or a note for the person.
        public void Status(string text) { if (heard) { heard.text = text ?? ""; layoutDirty = true; } }
        public void Say(string text)
        {
            if (!caption) return;
            if (typing != null) StopCoroutine(typing);
            typing = StartCoroutine(TypeOut(text ?? ""));
        }
        IEnumerator TypeOut(string text)
        {
            if (hint) hint.gameObject.SetActive(false);
            // size the card for the whole reply first, so it does not grow line by line while typing
            caption.text = text; caption.gameObject.SetActive(true); layoutDirty = true; Layout();
            caption.text = "";
            float perChar = Mathf.Clamp(6f / Mathf.Max(text.Length, 1), .012f, .05f);
            for (int i = 1; i <= text.Length; i++) { caption.text = text.Substring(0, i); yield return new WaitForSeconds(perChar); }
            typing = null;
        }

        void Apply()
        {
            for (int i = 0; i < rigs.Length; i++) if (rigs[i].root && (i == current || !rigs[i].root.gameObject.activeSelf) && on && mode == Mode.Craft) rigs[i].root.gameObject.SetActive(i == current);
            if (!on) stageScale = 1;
            if (glowRing && ringOn && ringOff) glowRing.sharedMaterial = on ? ringOn : ringOff;
            if (beam && beam.activeSelf != on) beam.SetActive(on);   // the projector's blue light shines from below in both looks
            if (wireButton)
            {
                // the on/off pair: the look shown is lit
                if (lookLabel) lookLabel.text = Localization.Tr("Realistic");
                var wireLabel = wireButton.GetComponentInChildren<TMP_Text>(); if (wireLabel) wireLabel.text = Localization.Tr("Wireframe");
                MenuTheme.Holo.Mark(wireButton, !realistic, false);
                if (look) MenuTheme.Holo.Mark(look, realistic, false);
            }
            else if (lookLabel) lookLabel.text = Localization.Tr(realistic ? "Wireframe" : "Realistic");
            if (powerLabel) powerLabel.text = Localization.Tr(on ? "Off" : "Power");
            if (partsLabel) partsLabel.text = Localization.Tr(mode == Mode.Solar ? "System" : mode == Mode.Universe ? "Start" : mode == Mode.Math ? "Replay" : exploded ? "Assemble" : "Parts");
            if (missionLabel) missionLabel.text = Localization.Tr(MissionPlaying ? "Stop" : "Mission");
            if (missionSlot && missionSlot.activeSelf != (mode == Mode.Craft)) missionSlot.SetActive(mode == Mode.Craft);
            if (calculatorSlot && calculatorSlot.activeSelf != (mode == Mode.Math)) calculatorSlot.SetActive(mode == Mode.Math);
            if (calculatorLabel && math) calculatorLabel.text = Localization.Tr(math.PopupName);
            // between the arrows: what is shown, and the list to jump to any of it
            var names = ItemNames(); int at = ItemIndex();
            if (itemLabel) itemLabel.text = at >= 0 && at < names.Length ? Localization.Tr(names[at]) : "";
            if (dropdownItems != null) for (int i = 0; i < dropdownItems.Length; i++)
            {
                var b = dropdownItems[i]; if (!b) continue;
                var slot = b.transform.parent.gameObject; bool show = i < names.Length;
                if (slot.activeSelf != show) slot.SetActive(show);
                if (!show) continue;
                var l = b.GetComponentInChildren<TMP_Text>(); if (l) l.text = Localization.Tr(names[i]);
                MenuTheme.Holo.Mark(b, i == at, false);
            }
            if (dropdown && !on && dropdown.activeSelf) dropdown.SetActive(false);
            // the title band's mode pills: the mode shown is lit
            if (craftMode) MenuTheme.Holo.Mark(craftMode, mode == Mode.Craft, false);
            if (solarMode) MenuTheme.Holo.Mark(solarMode, mode == Mode.Solar, false);
            if (universeMode) MenuTheme.Holo.Mark(universeMode, mode == Mode.Universe, false);
            if (mathMode) MenuTheme.Holo.Mark(mathMode, mode == Mode.Math, false);
            foreach (var (b, k) in new[] { (craftMode, "Spacecraft"), (solarMode, "Solar System"), (universeMode, "Universe"), (mathMode, "Math") }) { var l = b ? b.GetComponentInChildren<TMP_Text>() : null; if (l) l.text = Localization.Tr(k); }
            if (solar && solar.root && on && mode == Mode.Solar && !solar.root.gameObject.activeSelf) solar.root.gameObject.SetActive(true);
            if (universe && universe.root && on && mode == Mode.Universe && !universe.root.gameObject.activeSelf) universe.root.gameObject.SetActive(true);
            if (math && math.root && on && mode == Mode.Math && !math.root.gameObject.activeSelf) math.root.gameObject.SetActive(true);
            if (math && math.panel && !(on && mode == Mode.Math) && math.panel.activeSelf) math.panel.SetActive(false);   // the pop-up: HoloMath opens it, the deck only closes it
            if (card && card.activeSelf != on) card.SetActive(on);
            RefreshCard();
        }

        // ---- the size of the menus: four steps, the console, the card and the pop-ups together
        static readonly float[] MenuSizes = { .8f, 1, 1.25f, 1.5f };
        const string MenuSizeKey = "orbit.menuSize";
        int menuStep = 1; Vector3[] menuBase;
        public void ResizeMenus(int dir)
        {
            menuStep = Mathf.Clamp(menuStep + dir, 0, MenuSizes.Length - 1);
            try { PlayerPrefs.SetInt(MenuSizeKey, menuStep); } catch { }
            ApplyMenuSize(); layoutDirty = true;
        }
        void ApplyMenuSize()
        {
            if (menus == null || menuBase == null) return;
            for (int i = 0; i < menus.Length && i < menuBase.Length; i++) if (menus[i]) menus[i].localScale = menuBase[i] * MenuSizes[menuStep];
            if (menuSmaller) menuSmaller.interactable = menuStep > 0;
            if (menuLarger) menuLarger.interactable = menuStep < MenuSizes.Length - 1;
        }

        // ---- the item pill and its list
        public string[] ItemNames()
        {
            switch (mode)
            {
                case Mode.Solar: return OrbitSolar.Bodies.Select(b => b.name).ToArray();
                case Mode.Universe: return OrbitSolar.Steps.Select(s => s.name).ToArray();
                case Mode.Math: return HoloMath.PieceNames;
                default: return rigs.Select(r => OrbitCraft.Find(r.id)?.name ?? r.id).ToArray();
            }
        }
        int ItemIndex()
        {
            switch (mode)
            {
                case Mode.Solar: return solar ? Mathf.Max(solar.Picked, 0) : 0;   // the orrery counts as the Sun's row
                case Mode.Universe: return universe ? universe.Current : 0;
                case Mode.Math: return math ? math.PieceIndex : 0;
                default: return current;
            }
        }
        // Back: put the list away and step out of a close-up (a world or a moon back to the whole Solar System)
        public void Back()
        {
            if (dropdown) dropdown.SetActive(false);
            if (mode == Mode.Solar && solar && solar.Picked >= 0) solar.ShowSystem();
            Apply(); Changed?.Invoke();
        }
        public void Refresh() => Apply();
        public void ToggleDropdown() { if (!dropdown) return; if (!on) SetPower(true); dropdown.SetActive(!dropdown.activeSelf); if (dropdown.activeSelf && math && math.PopupOpen) math.ShowPopup(false); Apply(); }   // one pop-up at a time: they share the spot above the console
        public void ChooseItem(int i)
        {
            if (dropdown) dropdown.SetActive(false);
            if (!on) SetPower(true);
            switch (mode)
            {
                case Mode.Solar: solar.Pick(i); break;
                case Mode.Universe: universe.Go(i); break;
                case Mode.Math: math.Piece(i); break;
                default: Show(i); break;
            }
            Apply(); Changed?.Invoke();
        }

        void RefreshCard()
        {
            if (mode != Mode.Craft)
            {
                if (maker) maker.text = Localization.Tr(infoKicker) ?? ""; if (title) title.text = Localization.Tr(infoTitle) ?? ""; if (tagline) tagline.text = Localization.Tr(infoTagline) ?? "";
                if (body) { body.gameObject.SetActive(true); body.text = Localization.Tr(infoBody) ?? ""; }
                if (facts) { facts.gameObject.SetActive(true); facts.text = infoFacts == null ? "" : string.Join("\n", infoFacts.Select(f => "·  " + Localization.Tr(f))); }
                if (partTitle) partTitle.gameObject.SetActive(false);
                if (partBody) partBody.gameObject.SetActive(false);
                layoutDirty = true;
                return;
            }
            var c = Craft;
            // in the chosen language (the table in Localization is keyed by the catalogue's English); Refresh runs again
            // when the language changes. The maker (NASA / JPL, SpaceX) stays in Latin letters.
            if (title) title.text = Localization.Tr(c?.name) ?? "";
            if (maker) maker.text = c?.maker.ToUpperInvariant() ?? "";
            if (tagline) tagline.text = Localization.Tr(c?.tagline) ?? "";
            bool missionView = !string.IsNullOrEmpty(missionTitle), partView = !missionView && focused && exploded;
            if (body) { body.gameObject.SetActive(!partView && !missionView); body.text = Localization.Tr(c?.summary) ?? ""; }
            if (facts) { facts.gameObject.SetActive(!partView && !missionView); facts.text = c == null ? "" : string.Join("\n", c.facts.Select(f => "·  " + Localization.Tr(f))); }
            if (partTitle) { partTitle.gameObject.SetActive(partView || missionView); partTitle.text = Localization.Tr(missionView ? missionTitle : focused ? focused.name : "") ?? ""; }
            if (partBody) { partBody.gameObject.SetActive(partView || missionView); partBody.text = Localization.Tr(missionView ? missionBody : focused && c != null ? (c.parts.FirstOrDefault(p => p.id == focused.id)?.summary ?? "") : "") ?? ""; }
            layoutDirty = true;
        }

        // Stack the visible rows from the top, each as tall as its text, and size the card (and its frame) to fit.
        public void Layout()
        {
            if (!cardRect || stack == null) return;
            layoutDirty = false;
            float inner = cardWidth - padding * 2, y = padding;
            foreach (var row in stack)
            {
                if (!row || !row.gameObject.activeSelf) continue;
                var text = row.GetComponent<TMP_Text>();
                float h = row.sizeDelta.y;
                if (text)
                {
                    if (string.IsNullOrEmpty(text.text)) continue;
                    // the height the text wants at this width; measured twice (the preferred size, then the laid-out mesh),
                    // since long rows with symbols from fallback fonts came out short and the rows below overlapped them
                    h = Mathf.Ceil(text.GetPreferredValues(text.text, inner, Mathf.Infinity).y) + 4;
                    row.sizeDelta = new Vector2(inner, h);
                    text.ForceMeshUpdate(true, true);
                    h = Mathf.Max(h, Mathf.Ceil(text.textBounds.size.y) + 6);
                    row.sizeDelta = new Vector2(inner, h);
                }
                row.anchorMin = row.anchorMax = new Vector2(.5f, 1); row.pivot = new Vector2(.5f, 1);
                row.anchoredPosition = new Vector2(0, -y);
                y += h + spacing;
            }
            float height = y - spacing + padding;
            cardRect.sizeDelta = new Vector2(cardWidth, height);
            for (int i = 0; cardFrame != null && i < cardFrame.Length; i++)
            {
                var f = cardFrame[i]; if (!f) continue;
                var grow = cardFrameGrow != null && i < cardFrameGrow.Length ? cardFrameGrow[i] : Vector2.zero;
                f.anchorMin = f.anchorMax = new Vector2(.5f, 1); f.pivot = new Vector2(.5f, .5f);
                f.sizeDelta = new Vector2(cardWidth, height) + grow;
                f.anchoredPosition = new Vector2(0, -height / 2 + (i == 1 ? -6 : 0));
            }
            // the card hangs with its middle at eye level, whatever its length
            if (cardEyeHeight > 0) { var p = cardRect.position; p.y = cardEyeHeight + height * cardRect.lossyScale.y / 2; cardRect.position = p; }
            var curved = cardRect.GetComponent<CurvedPanel>(); if (curved) curved.Apply();
        }

        void Update()
        {
            if (!stage) return;
            // the current craft grows out of the table; the others shrink away
            for (int i = 0; i < rigs.Length; i++)
            {
                var r = rigs[i].root; if (!r || !r.gameObject.activeSelf) continue;
                float target = on && mode == Mode.Craft && i == current && !MissionPlaying ? stageScale : 0;
                float s = Mathf.MoveTowards(r.localScale.x, target, Time.deltaTime / riseSeconds * Mathf.Max(1, stageScale));
                r.localScale = Vector3.one * s;
                if (s <= 0 && target <= 0) r.gameObject.SetActive(false);
            }
            // the Solar System and the Universe grow out of the table the same way
            Grow(solar ? solar.root : null, mode == Mode.Solar);
            Grow(universe ? universe.root : null, mode == Mode.Universe);
            Grow(math ? math.root : null, mode == Mode.Math);
            if (on && mode == Mode.Craft && !spinPaused && !MissionPlaying && !AnyHeld()) stage.Rotate(0, spinDegreesPerSecond * Time.deltaTime, 0, Space.World);
            if (glowLight) glowLight.intensity = Mathf.MoveTowards(glowLight.intensity, on ? (realistic ? 1f : 1.1f) : 0, Time.deltaTime * 2);
            if (layoutDirty && card && card.activeSelf) Layout();
            if (card && card.activeSelf && cardFacesViewer)
            {
                var head = Camera.main;
                if (head) { var away = card.transform.position - head.transform.position; away.y = 0; if (away.sqrMagnitude > 1e-3f) card.transform.rotation = Quaternion.Slerp(card.transform.rotation, Quaternion.LookRotation(away), Time.deltaTime * 4); }
            }
        }

        // ---- the part labels in the parts view
        // Laid out together, as callouts: two columns either side of the model (as the person sees it), each pill pushed
        // out just past what the model reaches at that pill's own height (a fin or an array lower down does not push it
        // out), so none sits on the craft and none floats far off it, stacked in each column with at least a pill's height and
        // a gap between, and joined to its part by its leader line. Sides alternate down the craft (top to bottom), so
        // neighbours never share a column; a part well out to one side when the view opens (a solar array, a boom) keeps
        // that side. Labels glide to their places as the parts spread and the craft turns.
        [Tooltip("Clearance between the model's reach and the near edge of a label, and between stacked labels (metres at full size).")]
        public float labelMargin = .03f, labelGap = .014f;
        float labelReach;
        void LateUpdate()
        {
            var head = Camera.main;
            bool craftShown = on && mode == Mode.Craft && exploded && !MissionPlaying;
            for (int i = 0; i < rigs.Length; i++)
                foreach (var p in rigs[i].parts)
                {
                    if (!p || !p.labelRoot) continue;
                    bool show = craftShown && i == current && !p.Held && rigs[i].root && rigs[i].root.gameObject.activeInHierarchy;
                    if (p.labelRoot.activeSelf != show) p.labelRoot.SetActive(show);
                    if (p.leader && p.leader.enabled != show) p.leader.enabled = show;
                    if (!show) p.labelPlaced = false;
                }
            if (craftShown && head && current >= 0 && current < rigs.Length) LayoutLabels(rigs[current], head.transform.position, false);
        }
        // Also called by the builder's previews (snap: straight to the places, no glide).
        public void LayoutLabels(Rig rig, Vector3 eye, bool snap)
        {
            if (rig == null || !rig.root) return;
            var shown = rig.parts.Where(p => p && p.labelRoot && p.labelRoot.activeSelf).ToList();
            if (shown.Count == 0) { labelReach = 0; return; }
            var origin = rig.root.position;
            var fwd = origin - eye; fwd.y = 0; if (fwd.sqrMagnitude < 1e-6f) fwd = rig.root.forward; fwd.Normalize();
            var right = Vector3.Cross(Vector3.up, fwd);
            // how far the model reaches out from its upright axis (a part in the hand does not count: it is wherever the hand is)
            float reach = 0;
            foreach (var p in rig.parts) if (p && !p.Held && p.renderers != null) foreach (var r in p.renderers)
            {
                if (!r) continue; var b = r.bounds;
                for (int k = 0; k < 4; k++) { var c = b.center + new Vector3((k & 1) == 0 ? -b.extents.x : b.extents.x, 0, (k & 2) == 0 ? -b.extents.z : b.extents.z) - origin; reach = Mathf.Max(reach, new Vector2(c.x, c.z).magnitude); }
            }
            labelReach = snap || labelReach <= 0 ? reach : Mathf.Lerp(labelReach, reach, 1 - Mathf.Exp(-Time.deltaTime * 4));
            float size = Mathf.Max(rig.root.lossyScale.x, 1e-3f), margin = labelMargin * size, gap = labelGap * size;
            // sides: chosen when the view opens, then kept, so labels never swap across the craft as it turns
            // (judged from where each part is going in the spread, not where it is while still flying out)
            Vector3 Goal(HoloPart p) => (p.transform.parent ? p.transform.parent.TransformPoint(p.spread) : p.transform.position) + (p.Centre - p.transform.position);
            var byHeight = shown.OrderByDescending(p => Goal(p).y).ToList();
            for (int k = 0; k < byHeight.Count; k++)
            {
                var p = byHeight[k]; if (p.labelPlaced && p.labelSide != 0) continue;
                float x = Vector3.Dot(Goal(p) - origin, right);
                p.labelSide = Mathf.Abs(x) > labelReach * .35f ? (x > 0 ? 1 : -1) : (k % 2 == 0 ? 1 : -1);
            }
            foreach (int side in new[] { -1, 1 })
            {
                var col = byHeight.Where(p => p.labelSide == side).ToList(); if (col.Count == 0) continue;
                var w = col.Select(p => { var rt = (RectTransform)p.labelRoot.transform; return rt.sizeDelta.x * rt.lossyScale.x; }).ToArray();
                var h = col.Select(p => { var rt = (RectTransform)p.labelRoot.transform; return rt.sizeDelta.y * rt.lossyScale.y; }).ToArray();
                var want = col.Select(p => Goal(p).y).ToArray(); var y = (float[])want.Clone();
                // top down: each at its part's height, or lower, a pill and a gap under the one above; then the column is
                // moved as a whole so it stays centred on its parts
                for (int k = 1; k < y.Length; k++) y[k] = Mathf.Min(y[k], y[k - 1] - (h[k - 1] + h[k]) / 2 - gap);
                float shift = 0; for (int k = 0; k < y.Length; k++) shift += want[k] - y[k]; shift /= y.Length;
                for (int k = 0; k < col.Count; k++)
                {
                    var p = col[k]; var t = p.labelRoot.transform; float at = y[k] + shift;
                    // out past the model at this height, and brought forward a little so the model's near side does not cover the pill
                    float local = SideReach(rig, origin, right * side, at - h[k] / 2 - gap, at + h[k] / 2 + gap);
                    var target = origin + right * side * (local + margin + w[k] / 2) - fwd * local * .5f;
                    target.y = at;
                    if (snap || !p.labelPlaced) { t.position = target; p.labelVelocity = Vector3.zero; p.labelPlaced = true; }
                    else t.position = Vector3.SmoothDamp(t.position, target, ref p.labelVelocity, .15f);
                    var away = t.position - eye; away.y = 0; if (away.sqrMagnitude > 1e-4f) t.rotation = Quaternion.LookRotation(away);
                    // the leader from the part's middle to the pill's inner edge
                    if (p.leader) { p.leader.SetPosition(0, p.Centre); p.leader.SetPosition(1, t.position - t.right * side * w[k] / 2); }
                }
            }
        }

        // How far the model reaches out toward one side (along dir, from its upright axis) within a band of heights; a part
        // in the hand does not count.
        static float SideReach(Rig rig, Vector3 origin, Vector3 dir, float lo, float hi)
        {
            float reach = 0;
            foreach (var p in rig.parts) if (p && !p.Held && p.renderers != null) foreach (var r in p.renderers)
            {
                if (!r) continue; var b = r.bounds; if (b.max.y < lo || b.min.y > hi) continue;
                for (int k = 0; k < 4; k++) { var c = b.center + new Vector3((k & 1) == 0 ? -b.extents.x : b.extents.x, 0, (k & 2) == 0 ? -b.extents.z : b.extents.z) - origin; reach = Mathf.Max(reach, Vector3.Dot(c, dir)); }
            }
            return reach;
        }

        void Grow(Transform r, bool wanted)
        {
            if (!r || !r.gameObject.activeSelf) return;
            float target = on && wanted ? stageScale : 0;
            float s = Mathf.MoveTowards(r.localScale.x, target, Time.deltaTime / riseSeconds * Mathf.Max(1, stageScale));
            r.localScale = Vector3.one * s;
            if (s <= 0 && target <= 0) r.gameObject.SetActive(false);
        }

        bool AnyHeld() { foreach (var p in rigs[current].parts) if (p && p.Held) return true; return false; }
    }
}
