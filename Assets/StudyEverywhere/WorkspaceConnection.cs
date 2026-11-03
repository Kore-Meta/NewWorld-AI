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
    // are set up. Connected: the main display fills this panel's well and each other display gets its own panel, placed
    // where that monitor stands relative to the main one and curved around the seat. A display whose stream stops
    // delivering frames is marked paused on its own panel at once (never left looking live) and the connection reads
    // Reconnecting; if every display stays silent it gives up to Disconnected, with Retry. Sharing can be stopped here
    // (all displays, or one from its panel) or on the computer.
    public sealed class WorkspaceConnection : MonoBehaviour
    {
        public enum State { NotPaired, AwaitingApproval, Connecting, Connected, Reconnecting, Disconnected }

        // The panel's connection, for the focus status strip.
        public static WorkspaceConnection Active { get; private set; }
        public State Current => state;
        public int DisplayCount => displays.Count;
        public event Action<State> StateChanged;

        [Tooltip("Component implementing IScreenSource (SimulatedScreenSource until the real transport exists).")]
        public MonoBehaviour source;
        [Tooltip("Seconds without a new frame before a display is marked paused.")]
        public float staleAfter = 1f;
        [Tooltip("Seconds without a frame from any display before giving up.")]
        public float giveUpAfter = 15f;
        [Tooltip("Seconds to wait for the streams after approval.")]
        public float connectTimeout = 20f;
        [Tooltip("Space between neighbouring display panels, in metres.")]
        public float panelGap = 0.04f;

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
        [Header("Other displays: inactive templates cloned per display, chosen by aspect")]
        public DisplayPanel landscapeTemplate;
        public DisplayPanel ultrawideTemplate, portraitTemplate;

        sealed class Shown
        {
            public SharedDisplay info;
            public DisplayPanel panel;   // null for the main display, which is drawn in this panel's well
            public float lastFrame;
            public bool live = true;
            public string where;   // where its panel stands relative to the main display (English, for Detail)
        }

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
            screen = source as IScreenSource;
            if (screen == null) Debug.LogError("[WorkspaceConnection] The source does not implement IScreenSource.", this);
            if (ComingSoon) ShowComingSoon();
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
            var hint = chooser ? chooser.GetComponentsInChildren<TMP_Text>(true).FirstOrDefault(t => t.name == "Connect hint") : null;
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
        }

        void OnDisable()
        {
            if (screen != null)
            {
                if (state != State.NotPaired && state != State.Disconnected) screen.Stop();
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

        // The primary display takes the main panel; every other display opens a panel of its own.
        void OnDisplayAdded(SharedDisplay d)
        {
            if (state != State.Connecting && state != State.Connected && state != State.Reconnecting) return;
            var shown = new Shown { info = d, lastFrame = Time.time };
            displays[d.id] = shown;
            if (mainId == 0 || d.primary && !displays[mainId].info.primary)
            {
                if (mainId != 0) OpenPanel(displays[mainId]);
                mainId = d.id;
                mainView.Show(d.texture);
                mainView.Mark(true);
            }
            else OpenPanel(shown);
            if (state == State.Connecting) Enter(State.Connected);
            else Summarize();
            Arrange();
        }

        // A display stopped (here or on the computer): close its panel; if it was the main display, the next one moves in.
        void Remove(int id)
        {
            if (!displays.TryGetValue(id, out var gone)) return;
            ClosePanel(gone);
            displays.Remove(id);
            if (id == mainId)
            {
                mainId = 0;
                mainView.Clear();
                var next = displays.Values.OrderByDescending(s => s.info.primary).ThenBy(s => s.info.layout.x).FirstOrDefault();
                if (next != null)
                {
                    ClosePanel(next);
                    mainId = next.info.id;
                    mainView.Show(next.info.texture);
                    mainView.Mark(next.live);
                }
            }
            if (displays.Count == 0) { OnEnded("Your computer is no longer sharing any displays."); return; }
            Summarize();
            Arrange();
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
                    if (left <= 0) { screen.Cancel(); OnEnded("The code expired before the computer approved. Retry for a new code."); break; }
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
                        if (live != d.live) { d.live = live; View(d).Mark(live); }
                        if (live) View(d).Pulse(Time.time);
                        anyPaused |= !live;
                        allGone &= quiet > giveUpAfter;
                    }
                    if (allGone) { screen.Stop(); OnEnded("The streams stopped. Check the computer is awake and on the same Wi-Fi."); break; }
                    var want = anyPaused ? State.Reconnecting : State.Connected;
                    if (want != state) Enter(want);
                    break;
            }
        }

        DisplayView View(Shown s) => s.panel ? s.panel.view : mainView;

        void Enter(State next)
        {
            state = next;
            enteredAt = Time.time;
            bool showing = next == State.Connected || next == State.Reconnecting;
            if (!showing) ClearDisplays();
            chooser.SetActive(next == State.NotPaired);
            pairing.SetActive(next == State.AwaitingApproval);
            message.SetActive(next == State.Connecting || next == State.Disconnected);
            stream.SetActive(showing);
            controls.SetActive(showing);
            well.color = showing ? UIStyle.Ink : wellIdle;
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

        // The header copy and the panels' placement notes are composed here, so they are written again in a new language.
        void OnLanguage()
        {
            Summarize();
            foreach (var s in displays.Values) if (s.panel && s.where != null) Detail(s);
        }

        static void Detail(Shown s) => s.panel.detail.text = $"{s.info.layout.width} × {s.info.layout.height} · {Localization.Tr(s.where)}";

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

        void OpenPanel(Shown s)
        {
            var layout = s.info.layout;
            float aspect = layout.width / (float)Mathf.Max(1, layout.height);
            var template = aspect < 1 ? portraitTemplate : aspect > 1.9f ? ultrawideTemplate : landscapeTemplate;
            var panel = Instantiate(template);
            panel.name = "Display panel | " + s.info.name;
            panel.title.text = s.info.name.ToUpperInvariant();
            int id = s.info.id;
            panel.stop.onClick.AddListener(() => StopDisplay(id));
            panel.view.Show(s.info.texture);
            panel.view.Mark(s.live);
            s.panel = panel;
            panel.gameObject.SetActive(true);
        }

        static void ClosePanel(Shown s)
        {
            if (s.panel) Destroy(s.panel.gameObject);
            s.panel = null;
        }

        void ClearDisplays()
        {
            foreach (var s in displays.Values) ClosePanel(s);
            displays.Clear();
            mainId = 0;
            if (mainView.picture) mainView.Clear();
        }

        // Places every other display's panel where that monitor stands relative to the main one: displays beside it
        // continue the row to the left or right (in desktop order, edge to edge with a small gap), curved around the
        // seat so each faces the user; displays mostly above or below it stack over or under the workspace panel.
        // Desktop units become metres at the scale the main display is drawn at.
        void Arrange()
        {
            if (!displays.TryGetValue(mainId, out var main)) return;
            var m = main.info.layout;
            Vector3 scale = transform.lossyScale;
            Vector2 card = ((RectTransform)transform).sizeDelta, area = ((RectTransform)mainView.picture.transform.parent).rect.size;
            float mainW = card.x * scale.x, mainH = card.y * scale.y;
            float metresPerUnit = Mathf.Min(area.x, area.y * m.width / (float)m.height) * scale.x / m.width;

            var head = Camera.main ? Camera.main.transform.position : transform.position - transform.forward;
            var pivot = new Vector3(head.x, transform.position.y, head.z);
            if (Vector3.Distance(pivot, transform.position) < 0.3f) pivot = transform.position - Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            float radius = Vector3.Distance(pivot, transform.position);

            void Place(Shown s, float arc, float up, string where)
            {
                var rot = Quaternion.Euler(0, arc / radius * Mathf.Rad2Deg, 0);
                s.panel.transform.SetPositionAndRotation(pivot + rot * (transform.position - pivot) + Vector3.up * up, rot * transform.rotation);
                s.where = where; Detail(s);
            }
            Vector2 Size(Shown s) => ((RectTransform)s.panel.transform).sizeDelta * s.panel.transform.lossyScale.x;
            bool Stacked(RectInt r) => Mathf.Min(r.xMax, m.xMax) - Mathf.Max(r.xMin, m.xMin) >= 0.5f * Mathf.Min(r.width, m.width);
            float RowLift(RectInt r) => Mathf.Clamp(-(r.center.y - m.center.y) * metresPerUnit, -mainH / 2, mainH / 2);

            var others = displays.Values.Where(s => s.panel).ToList();
            float above = mainH / 2, below = -mainH / 2;
            foreach (var s in others.Where(s => Stacked(s.info.layout)))
            {
                var r = s.info.layout;
                float h = Size(s).y, arc = (r.center.x - m.center.x) * metresPerUnit;
                if (r.center.y < m.center.y) { above += panelGap + h / 2; Place(s, arc, above, "above main"); above += h / 2; }
                else { below -= panelGap + h / 2; Place(s, arc, below, "below main"); below -= h / 2; }
            }
            float right = mainW / 2;
            foreach (var s in others.Where(s => !Stacked(s.info.layout) && s.info.layout.center.x >= m.center.x).OrderBy(s => s.info.layout.center.x))
            {
                float w = Size(s).x;
                right += panelGap + w / 2;
                Place(s, right, RowLift(s.info.layout), "right of main");
                right += w / 2;
            }
            float left = -mainW / 2;
            foreach (var s in others.Where(s => !Stacked(s.info.layout) && s.info.layout.center.x < m.center.x).OrderByDescending(s => s.info.layout.center.x))
            {
                float w = Size(s).x;
                left -= panelGap + w / 2;
                Place(s, left, RowLift(s.info.layout), "left of main");
                left -= w / 2;
            }
        }
    }
}
