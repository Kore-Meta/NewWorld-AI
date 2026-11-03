using UnityEngine;
using UnityEngine.UI;

namespace StudyEverywhere
{
    // Window behaviour for a floating New World panel: minimize folds it into a single "Open" pill that
    // glows every few seconds so it stays discoverable; maximize (optional) toggles a larger size around the panel
    // centre; a PanelResize grip sets the user's own size, which maximize multiplies.
    public sealed class PanelWindow : MonoBehaviour
    {
        public GameObject window;
        public Button minimize;
        [Tooltip("Optional: panels without a maximize button leave this empty.")]
        public Button maximize;
        [Tooltip("Shown while minimized: the Open pill and its glow.")]
        public GameObject openGroup;
        public Button open;
        public Image glow;
        [Min(1)] public float maximizeScale = 1.4f;
        [Tooltip("Smallest and largest size the resize grip allows, relative to the built size.")]
        public Vector2 resizeRange = new Vector2(0.6f, 1.6f);
        [Tooltip("Seconds between glow pulses while minimized.")]
        [Min(1)] public float glowEvery = 5f;
        [Tooltip("Open as the minimized pill the first time (New York's workspace: the Open Workspace pill beside the music).")]
        public bool startMinimized;
        const float PulseSeconds = 1.6f;

        Vector3 baseScale;
        bool maximized;
        float userScale = 1, clock;
        bool shownOnce;

        // The size chosen with the resize grip; kept across minimize and maximize.
        public float UserScale
        {
            get => userScale;
            set { userScale = Mathf.Clamp(value, resizeRange.x, resizeRange.y); if (window.activeSelf) ApplyScale(); }
        }

        void Awake() => baseScale = transform.localScale;

        void OnEnable()
        {
            minimize.onClick.AddListener(Minimize);
            if (maximize) maximize.onClick.AddListener(ToggleMaximize);
            open.onClick.AddListener(Restore);
            Show(shownOnce || !startMinimized);
            shownOnce = true;
        }

        void OnDisable()
        {
            minimize.onClick.RemoveListener(Minimize);
            if (maximize) maximize.onClick.RemoveListener(ToggleMaximize);
            open.onClick.RemoveListener(Restore);
        }

        public void Minimize() => Show(false);
        public void Restore() => Show(true);
        public void ToggleMaximize()
        {
            maximized = !maximized;
            ApplyScale();
        }

        void ApplyScale() => transform.localScale = baseScale * userScale * (maximized ? maximizeScale : 1);

        // The Open pill always appears at the built size, so minimizing also leaves the maximized state.
        void Show(bool shown)
        {
            maximized = false;
            transform.localScale = shown ? baseScale * userScale : baseScale;
            window.SetActive(shown);
            openGroup.SetActive(!shown);
            clock = glowEvery - PulseSeconds;   // first pulse soon after minimizing
        }

        void Update()
        {
            if (!openGroup.activeSelf) return;
            clock += Time.deltaTime;
            float p = Mathf.Repeat(clock, glowEvery) / PulseSeconds, a = p < 1 ? Mathf.Sin(p * Mathf.PI) : 0;
            if (glow) { var c = UIStyle.Brass; c.a = 0.8f * a; glow.color = c; }
            open.transform.localScale = Vector3.one * (1 + 0.05f * a);
        }
    }
}
