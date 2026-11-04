using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace StudyEverywhere
{
    // In-headset side of connecting a Windows computer, with every display it shares (product direction 05).
    // Not paired: the source chooser. "Connect a computer" asks the IScreenSource for a short-lived code. Awaiting
    // approval: the user enters it in the desktop app, approves capture and picks the displays. Connecting: the streams
    // are set up. Connected: this card folds away and every display stands round the seat as a curved screen at its
    // real size (ScreenSurface), arranged the way the monitors stand on the desk, the user's − / + making them all
    // bigger or smaller together. A display whose stream stops delivering frames is marked paused on its own screen at
    // once (never left looking live) and the connection reads Reconnecting; if every display stays silent it gives up
    // to Disconnected, with Retry. Sharing can be stopped here (all displays, or one from its strip) or on the computer.
    public sealed class WorkspaceConnection : MonoBehaviour
    {
        public enum State { NotPaired, AwaitingApproval, Connecting, Connected, Reconnecting, Disconnected }

        // The panel's connection, for the focus status strip.
        public static WorkspaceConnection Active { get; private set; }
        public State Current => state;
        public int DisplayCount => displays.Count;
        public event Action<State> StateChanged;

        [Tooltip("Component implementing IScreenSource. On the headset the real WebRtcScreenSource is added at runtime in its place.")]
        public MonoBehaviour source;
        [Tooltip("Keep the simulated source in the Editor (no PC needed); untick to pair with the New World Desktop app from Play mode.")]
        public bool simulateInEditor = true;
        [Tooltip("Seconds without a new frame before a display is marked paused.")]
        public float staleAfter = 1f;
        [Tooltip("Seconds without a frame from any display before giving up.")]
        public float giveUpAfter = 15f;
        [Tooltip("Seconds to wait for the streams after approval.")]
        public float connectTimeout = 20f;
        [Tooltip("Where people get the New World Desktop app, as the panel shows it (no https://).")]
        public string desktopAppPage = "github.com/Kore-Meta/NewWorld-AI/releases";   // NewWorld AI: the installer is on the release

        [Header("Panel")]
        public TMP_Text eyebrow, subtitle;
        public Image well;
        [Tooltip("The well's colour while no computer picture shows (the place's menu theme; the café's tinted sage by default).")]
        public Color wellIdle = UIStyle.TintedSage;
        [Header("Not paired")]
        public GameObject chooser;
        public Button connect;
        [Header("Awaiting approval")]
        public GameObject pairing;
        public TMP_Text code, countdown;
        public Button cancelPairing;
        [Header("Connecting, Disconnected")]
        public GameObject message;
        public TMP_Text messageTitle, messageBody, backLabel;
        public RectTransform retryGroup, backGroup;
        public Button retry, back;
        [Header("Connected, Reconnecting")]
        public GameObject stream, controls;
        public DisplayView mainView;
        public Button stopSharing;
        [Header("Former per-display panels (unused since displays became curved screens; kept for the scene builders)")]
        public DisplayPanel landscapeTemplate;
        public DisplayPanel ultrawideTemplate, portraitTemplate;

        [Header("Shared screens")]
        [Tooltip("Distance from the seat to the screens at true size, metres (about a desk's depth).")]
        public float screenDistance = 0.8f;
        [Tooltip("Gap between neighbouring screens, metres (about two bezels).")]
        public float screenGap = 0.015f;

        sealed class Shown
        {
            public SharedDisplay info;
            public ScreenSurface surface;   // the display drawn round the seat at its real proportions
            public float lastFrame;
            public bool live = true;
        }

        // The user's screen size (100% by default), kept between sessions and places. 100% is 2.6 times the monitor's
        // true size: true size reads small in the headset, and testing on real 34" ultrawides settled on this as the
        // size that feels like the real monitor. Smaller and Bigger step from there.
        const string ScreenSizeKey = "NewWorld.ScreenSize";
        const float DefaultScale = 2.6f;
        static float ScreenSize
        {
            get => Mathf.Clamp(PlayerPrefs.GetFloat(ScreenSizeKey, 1f), 0.4f, 1.6f);
            set { PlayerPrefs.SetFloat(ScreenSizeKey, Mathf.Clamp(value, 0.4f, 1.6f)); PlayerPrefs.Save(); }
        }
        static float ScreenScale => DefaultScale * ScreenSize;
        ScreenControls bar;
        PanelWindow window;
        bool foldedForSharing, pivotSet, faceGaze;
        Vector3 pivot;
        float pivotYaw;

        IScreenSource screen;
        State state;
        float expiresAt, enteredAt;
        string reason, idleNote;
        readonly Dictionary<int, Shown> displays = new Dictionary<int, Shown>();
        int mainId;

        public string StatusLabel => state switch
        {
            State.Connected => displays.Count > 1 ? $"PC Connected · {displays.Count} displays" : "PC Connected",
            State.Reconnecting => "PC reconnecting",
            State.AwaitingApproval => "Pairing computer",
            State.Connecting => "Connecting PC",
            _ => "No computer",
        };

        void Awake()
        {
            // The scenes are built with the simulated source; everywhere but the Editor (unless asked) the real one
            // streams from the New World Desktop app instead. It is one for the whole app, so sharing carries on into
            // the next place that has a workspace.
            if (source is SimulatedScreenSource simulated && !(Application.isEditor && simulateInEditor))
            {
                simulated.enabled = false;
                source = WebRtcScreenSource.Shared;
            }
            screen = source as IScreenSource;
            window = GetComponent<PanelWindow>();
            if (screen == null) Debug.LogError("[WorkspaceConnection] The source does not implement IScreenSource.", this);
            if (ComingSoon) ShowComingSoon();
            else ShowInstallHint();
        }

        // Under Connect a computer: that it needs the desktop app installed on the PC, and where to get it (the user,
        // Nov 4: people must know to install it first). The hint line shrinks to fit a long address or translation.
        TMP_Text Hint => chooser ? chooser.GetComponentsInChildren<TMP_Text>(true).FirstOrDefault(t => t.name == "Connect hint") : null;
        void ShowInstallHint()
        {
            var hint = Hint;
            if (!hint) return;
            if (!hint.enableAutoSizing) { hint.fontSizeMax = hint.fontSize; hint.fontSizeMin = hint.fontSize * .7f; hint.enableAutoSizing = true; }
            hint.text = Localization.Format("Install the New World Desktop app on a Windows PC on this Wi-Fi: {0}", desktopAppPage);
        }

        // Until the real desktop app and stream exist, only the simulated source is wired in. In release builds the
        // panel says so: Connect a computer is greyed out like Browser and Document, its hint reads Coming soon, and
        // nothing pretends to pair (the store listing calls it coming soon). The Editor and development builds keep the
        // simulated flow for testing.
        bool ComingSoon => source is SimulatedScreenSource && !Application.isEditor && !Debug.isDebugBuild;
        void ShowComingSoon()
        {
            connect.interactable = false;
            var colours = connect.colors; colours.disabledColor = Color.Lerp(UIStyle.WarmIvory, UIStyle.LightBorder, .45f); connect.colors = colours;
            foreach (var t in connect.GetComponentsInChildren<TMP_Text>(true)) t.color = UIStyle.Secondary;
            var hint = Hint;
            if (hint) { hint.text = "COMING SOON"; hint.fontStyle = FontStyles.Bold; hint.color = UIStyle.Brass; }   // Localizer translates it
        }

        void OnEnable()
        {
            Active = this;
            if (screen != null)
            {
                screen.CodeIssued += OnCode;
                screen.Approved += OnApproved;
                screen.DisplayAdded += OnDisplayAdded;
                screen.DisplayRemoved += Remove;
                screen.Frame += OnFrame;
                screen.Ended += OnEnded;
            }
            connect.onClick.AddListener(Connect);
            retry.onClick.AddListener(Connect);
            cancelPairing.onClick.AddListener(Cancel);
            back.onClick.AddListener(Cancel);
            stopSharing.onClick.AddListener(StopSharing);
            Localization.Changed += OnLanguage;
            Enter(State.NotPaired);
            // Arriving from another place while sharing: take the screens over once the player stands at the seat.
            if (screen is WebRtcScreenSource shared)
            {
                shared.Claim(this);
                if (shared.IsSharing) Invoke(nameof(TakeOver), 1f);
            }
        }

        void TakeOver()
        {
            if (!(screen is WebRtcScreenSource shared) || !shared.IsSharing || state != State.NotPaired) return;
            Enter(State.Connecting);
            foreach (var d in shared.ShownDisplays.ToList()) OnDisplayAdded(d);
        }

        void OnDisable()
        {
            CancelInvoke(nameof(TakeOver));
            if (screen != null)
            {
                // Leaving the place: the shared source lets the next place's workspace take the screens over (and stops
                // by itself if none does); any other source stops here.
                if (screen is WebRtcScreenSource shared) shared.Release(this);
                else if (state != State.NotPaired && state != State.Disconnected) screen.Stop();
                screen.CodeIssued -= OnCode;
                screen.Approved -= OnApproved;
                screen.DisplayAdded -= OnDisplayAdded;
                screen.DisplayRemoved -= Remove;
                screen.Frame -= OnFrame;
                screen.Ended -= OnEnded;
            }
            connect.onClick.RemoveListener(Connect);
            retry.onClick.RemoveListener(Connect);
            cancelPairing.onClick.RemoveListener(Cancel);
            back.onClick.RemoveListener(Cancel);
            stopSharing.onClick.RemoveListener(StopSharing);
            Localization.Changed -= OnLanguage;
            ClearDisplays();
            if (Active == this) Active = null;
        }

        [ContextMenu("Connect a computer")]
        public void Connect() { if (ComingSoon) return; idleNote = null; screen?.Pair(); }

        // Cancel during pairing or connecting; Back from Disconnected.
        public void Cancel()
        {
            if (state == State.AwaitingApproval || state == State.Connecting) screen?.Cancel();
            idleNote = null;
            Enter(State.NotPaired);
        }

        [ContextMenu("Stop sharing")]
        public void StopSharing()
        {
            screen?.Stop();
            idleNote = "Sharing stopped. Your computer is no longer sending its screens.";
            Enter(State.NotPaired);
        }

        // Stop one display from its own panel; the others keep streaming.
        public void StopDisplay(int id)
        {
            screen?.StopDisplay(id);
            Remove(id);
        }

        void OnCode(string value, float lifetime)
        {
            code.text = value.Length == 6 ? value.Substring(0, 3) + " " + value.Substring(3) : value;
            expiresAt = Time.time + lifetime;
            Enter(State.AwaitingApproval);
        }

        void OnApproved() => Enter(State.Connecting);

        // Every display becomes a screen of its own round the seat; the primary one is the main screen, in front.
        void OnDisplayAdded(SharedDisplay d)
        {
            if (state != State.Connecting && state != State.Connected && state != State.Reconnecting) return;
            if (displays.TryGetValue(d.id, out var old)) Close(old);
            var shown = new Shown { info = d, lastFrame = Time.time };
            displays[d.id] = shown;
            shown.surface = ScreenSurface.Create(d.name, d.texture);
            if (mainId == 0 || d.primary && !displays[mainId].info.primary) mainId = d.id;
            if (!bar) bar = ScreenControls.Create(BarStyle(), () => Resize(-0.1f), () => Resize(0.1f), BringToMe, StopSharing, StopDisplay);
            if (state == State.Connecting) Enter(State.Connected);
            else Summarize();
            Arrange();
        }

        // A display stopped (here or on the computer): its screen goes; if it was the main one, the next one moves in.
        void Remove(int id)
        {
            if (!displays.TryGetValue(id, out var gone)) return;
            Close(gone);
            displays.Remove(id);
            if (id == mainId)
            {
                var next = displays.Values.OrderByDescending(s => s.info.primary).ThenBy(s => s.info.layout.x).FirstOrDefault();
                mainId = next?.info.id ?? 0;
            }
            if (displays.Count == 0) { OnEnded("Your computer is no longer sharing any displays."); return; }
            Summarize();
            Arrange();
        }

        // Smaller and Bigger step the size by 10% of the default.
        void Resize(float step)
        {
            ScreenSize = Mathf.Round((ScreenSize + step) * 10) / 10;
            Arrange();
        }

        // The screens (and the bar) come round to face wherever the user now sits and looks.
        void BringToMe()
        {
            pivotSet = false;
            faceGaze = true;
            Arrange();
        }

        // The workspace card's own styled pieces, cloned into the control bar so it matches the place's menus.
        ScreenControls.Style BarStyle() => new ScreenControls.Style
        {
            card = Named("Card") ? Named("Card") : well,
            trim = Named("Brass trim") ? Named("Brass trim") : well,
            dot = mainView.liveDot,
            label = eyebrow,
            pill = stopSharing,
            raycaster = GetComponents<Component>().FirstOrDefault(c => c.GetType().Name == "TrackedDeviceGraphicRaycaster")?.GetType(),
        };

        Image Named(string name) => GetComponentsInChildren<Image>(true).FirstOrDefault(i => i.name == name);

        // The bar's top line and display chips, in the current language.
        void RefreshBar()
        {
            if (!bar) return;
            int n = displays.Count;
            bool allLive = displays.Values.All(d => d.live);
            bar.SetSummary((n > 1 ? Localization.Format("Sharing {0} displays", n) : Localization.Tr("Sharing your screen")).ToUpperInvariant()
                + " · " + Localization.Tr(allLive ? "LIVE" : "PAUSED"), allLive);
            bar.SetSize(ScreenSize * 100);
            bar.SetDisplays(displays.Values.OrderBy(d => d.info.layout.x).Select(d => (d.info.id, d.info.name, d.live)).ToList());
        }

        static void Close(Shown s)
        {
            if (s.surface) Destroy(s.surface.gameObject);
            s.surface = null;
        }

        void OnFrame(int id)
        {
            if (!displays.TryGetValue(id, out var s)) return;
            s.lastFrame = Time.time;
        }

        void OnEnded(string why)
        {
            if (state == State.NotPaired) return;
            reason = why;
            Enter(State.Disconnected);
        }

        void Update()
        {
            switch (state)
            {
                case State.AwaitingApproval:
                    float left = expiresAt - Time.time;
                    // most often no desktop app answered at all: say what it needs and where to get it
                    if (left <= 0) { screen.Cancel(); OnEnded(Localization.Format("The code expired before the computer approved. Is the New World Desktop app installed and open on your PC? Get it at {0}, then Retry.", desktopAppPage)); break; }
                    int s = Mathf.CeilToInt(left);
                    countdown.text = Localization.Format("Code expires in {0}:{1:00}", s / 60, s % 60);
                    break;
                case State.Connecting:
                    if (Time.time - enteredAt > connectTimeout) { screen.Cancel(); OnEnded("Couldn't reach the computer. Check it is awake and on the same Wi-Fi."); break; }
                    messageTitle.text = Localization.Tr("Connecting") + new string('.', 1 + (int)((Time.time - enteredAt) * 2) % 3);
                    break;
                case State.Connected:
                case State.Reconnecting:
                    bool anyPaused = false, allGone = true;
                    foreach (var d in displays.Values)
                    {
                        float quiet = Time.time - d.lastFrame;
                        bool live = quiet <= staleAfter;
                        if (live != d.live)
                        {
                            d.live = live;
                            d.surface.Mark(live);
                            if (d.info.id == mainId) mainView.Mark(live);
                            RefreshBar();
                        }
                        if (live && d.info.id == mainId) mainView.Pulse(Time.time);
                        anyPaused |= !live;
                        allGone &= quiet > giveUpAfter;
                    }
                    if (bar && !anyPaused) bar.Pulse(Time.time);
                    if (allGone) { screen.Stop(); OnEnded("The streams stopped. Check the computer is awake and on the same Wi-Fi."); break; }
                    var want = anyPaused ? State.Reconnecting : State.Connected;
                    if (want != state) Enter(want);
                    break;
            }
        }

        void Enter(State next)
        {
            state = next;
            enteredAt = Time.time;
            bool showing = next == State.Connected || next == State.Reconnecting;
            if (!showing) ClearDisplays();
            // While sharing, the screens stand round the seat and this card folds away entirely: even its Open pill would
            // sit between the seat and the main screen, and the screens' own bar has everything the card offers then.
            // It opens again when sharing ends.
            if (window)
            {
                if (showing && !foldedForSharing) { foldedForSharing = true; window.Minimize(); window.openGroup.SetActive(false); }
                else if (!showing && foldedForSharing) { foldedForSharing = false; window.Restore(); }
            }
            chooser.SetActive(next == State.NotPaired);
            pairing.SetActive(next == State.AwaitingApproval);
            message.SetActive(next == State.Connecting || next == State.Disconnected);
            stream.SetActive(false);   // the main display is on its own screen now, not in this card
            controls.SetActive(showing);
            well.color = wellIdle;
            Summarize();

            if (next == State.Connecting)
            {
                messageTitle.text = Localization.Tr("Connecting") + ".";
                messageBody.text = "Your computer approved sharing. Setting up an encrypted stream for each display on your local network.";
                backLabel.text = "Cancel";
                retryGroup.gameObject.SetActive(false);
                backGroup.anchoredPosition = new Vector2(0, backGroup.anchoredPosition.y);
            }
            else if (next == State.Disconnected)
            {
                messageTitle.text = "Disconnected";
                messageBody.text = reason;
                backLabel.text = "Back";
                retryGroup.gameObject.SetActive(true);
                backGroup.anchoredPosition = new Vector2(-retryGroup.anchoredPosition.x, backGroup.anchoredPosition.y);
            }
            StateChanged?.Invoke(next);
        }

        // The header copy and the screens' labels are composed here, so they are written again in a new language.
        void OnLanguage()
        {
            if (!ComingSoon) ShowInstallHint();
            Summarize();
            RefreshBar();
        }

        // Header copy: which display this panel shows, and how many are shared.
        void Summarize()
        {
            int n = displays.Count;
            eyebrow.text = (state == State.Connected || state == State.Reconnecting) && displays.TryGetValue(mainId, out var main)
                ? main.info.name.ToUpperInvariant() + (main.info.primary ? " · " + Localization.Tr("MAIN") : "")
                : Localization.Tr("WORKSPACE");
            subtitle.text = state switch
            {
                State.AwaitingApproval => Localization.Tr("Pairing with your computer."),
                State.Connecting => Localization.Tr("Approved on your computer. Starting encrypted streams."),
                State.Connected => n > 1 ? Localization.Format("Sharing {0} displays. Only you can see these panels.", n) : Localization.Tr("Sharing your computer's screen. Only you can see this panel."),
                State.Reconnecting => Localization.Tr("A display paused. Reconnecting to your computer."),
                State.Disconnected => Localization.Tr("Your computer is no longer connected."),
                _ => Localization.Tr(idleNote ?? "Bring your computer's screens into your study space."),
            };
        }

        void ClearDisplays()
        {
            foreach (var s in displays.Values) Close(s);
            displays.Clear();
            if (bar) Destroy(bar.gameObject);
            bar = null;
            mainId = 0;
            pivotSet = false;
            if (mainView.picture) mainView.Clear();
        }

        // A display's size in metres: the monitor's own size when the computer reports it, otherwise its desktop size
        // at the main display's scale (or, failing that too, as tall as a typical 24-34" monitor), times the user's size.
        static Vector2 Metres(Shown s, Shown main)
        {
            if (s.info.sizeMm.x > 0 && s.info.sizeMm.y > 0) return s.info.sizeMm / 1000f * ScreenScale;
            var m = main.info;
            float perUnit = (m.sizeMm.y > 0 ? m.sizeMm.y / 1000f : 0.33f) / Mathf.Max(1, m.layout.height);
            return new Vector2(s.info.layout.width, s.info.layout.height) * perUnit * ScreenScale;
        }

        // Stands the screens round the seat the way the monitors stand on the desk: the main one straight ahead with its
        // top edge at eye level, displays beside it continuing the row left or right (in desktop order, edge to edge),
        // displays mostly above or below it stacked over or under it. All curve round the seat at a desk's depth, a
        // little further back as the user makes them bigger. The seat is fixed when sharing starts, so the screens stay
        // put when the head moves.
        void Arrange()
        {
            if (!displays.TryGetValue(mainId, out var main)) return;
            if (!pivotSet)
            {
                var head = Camera.main ? Camera.main.transform : transform;
                pivot = head.position;
                // First time: towards this workspace card (the place's study screen); after Bring to me: where the user looks.
                var ahead = Vector3.ProjectOnPlane(faceGaze ? head.forward : transform.position - head.position, Vector3.up);
                if (ahead.sqrMagnitude < 0.04f) ahead = Vector3.ProjectOnPlane(head.forward, Vector3.up);
                faceGaze = false;
                pivotYaw = Quaternion.LookRotation(ahead.sqrMagnitude > 1e-4f ? ahead.normalized : Vector3.forward).eulerAngles.y;
                pivotSet = true;
            }
            float radius = Mathf.Clamp(screenDistance + 0.3f * (ScreenScale - 1), 0.65f, 1.8f);
            var m = main.info.layout;
            var size = Metres(main, main);
            // The main screen's centre a little below the eye (a big screen's top would otherwise sit far above it).
            float mainY = -Mathf.Min(0.12f, size.y * 0.15f), perUnit = size.y / Mathf.Max(1, m.height);
            main.surface.Layout(pivot, pivotYaw, radius, 0, mainY, size.x, size.y);
            if (bar) { bar.Place(pivot, pivotYaw); RefreshBar(); }

            const float strip = 0.02f;   // the gap between stacked screens
            bool Stacked(RectInt r) => Mathf.Min(r.xMax, m.xMax) - Mathf.Max(r.xMin, m.xMin) >= 0.5f * Mathf.Min(r.width, m.width);
            float Lift(RectInt r) => Mathf.Clamp(-(r.center.y - m.center.y) * perUnit, -size.y / 2, size.y / 2);
            var others = displays.Values.Where(s => s != main).ToList();

            float above = mainY + size.y / 2, below = mainY - size.y / 2 - strip;
            foreach (var s in others.Where(s => Stacked(s.info.layout)))
            {
                var r = s.info.layout;
                var sz = Metres(s, main);
                float arc = (r.center.x - m.center.x) * perUnit;
                if (r.center.y < m.center.y) { above += strip + sz.y / 2; s.surface.Layout(pivot, pivotYaw, radius, arc, above, sz.x, sz.y); above += sz.y / 2; }
                else { below -= screenGap + sz.y / 2; s.surface.Layout(pivot, pivotYaw, radius, arc, below, sz.x, sz.y); below -= sz.y / 2 + strip; }
            }
            float right = size.x / 2;
            foreach (var s in others.Where(s => !Stacked(s.info.layout) && s.info.layout.center.x >= m.center.x).OrderBy(s => s.info.layout.center.x))
            {
                var sz = Metres(s, main);
                right += screenGap + sz.x / 2;
                s.surface.Layout(pivot, pivotYaw, radius, right, mainY + Lift(s.info.layout), sz.x, sz.y);
                right += sz.x / 2;
            }
            float left = -size.x / 2;
            foreach (var s in others.Where(s => !Stacked(s.info.layout) && s.info.layout.center.x < m.center.x).OrderByDescending(s => s.info.layout.center.x))
            {
                var sz = Metres(s, main);
                left -= screenGap + sz.x / 2;
                s.surface.Layout(pivot, pivotYaw, radius, left, mainY + Lift(s.info.layout), sz.x, sz.y);
                left -= sz.x / 2;
            }
        }
    }
}
