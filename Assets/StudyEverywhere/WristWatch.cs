using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Hands;

namespace StudyEverywhere
{
    // A wristwatch worn in every place: a softly rounded gold case and black screen on a cream strap that loops round
    // the wrist. It rides the wrist rigidly, like a real watch, lying on the back of the wrist with its top across the
    // wrist, and moves with every turn of the hand. The screen turns from the Kore Meta logo, to MENU / Press (Press
    // glowing now and then), to the local time under the date, and round again; a soft gold halo swells round the
    // watch every few seconds so it is easy to notice. Tapping the screen (poke it with the other hand's finger, or
    // point and click) is what opens the place's settings menu (WristPanel); tapping again puts it away. It is worn
    // on the left wrist unless the menu moves it to the right (remembered). Tracked hands place it from the wrist
    // joint, in the same space the hand visualizer draws the hands in; with controllers it sits where the wrist is
    // behind the controller.
    [RequireComponent(typeof(Canvas))]
    public sealed class WristWatch : MonoBehaviour
    {
        public enum Hand { Left, Right }

        public XROrigin origin;
        [Tooltip("The space the hand visualizer draws the hands in (joint poses are relative to it). Empty: the origin's camera offset.")]
        public Transform handSpace;
        public Transform leftController, rightController;
        public WristPanel panel;
        public Button press;
        [Header("Faces")]
        public CanvasGroup logoFace;
        public CanvasGroup menuFace;
        public CanvasGroup timeFace;
        public TMP_Text time, date;
        [Tooltip("A soft gold glow behind Press that pulses while the Menu face shows, and the pill it lights.")]
        public Image pressGlow, pressPill;
        public float glowSeconds = 1.5f;
        [Tooltip("A soft gold halo round the whole watch that swells now and then, so the person notices it is there.")]
        public Image halo;
        [Tooltip("Seconds between the halo's swells, and how long each lasts.")]
        public float haloEvery = 5, haloSeconds = 1.8f;
        [Range(0, 1)] public float haloStrength = .55f;
        public float logoSeconds = 3, menuSeconds = 3, timeSeconds = 6, fadeSeconds = 0.4f;
        [Header("Placement")]
        [Tooltip("Height of the screen above the wrist joint (the middle of the wrist), in metres.")]
        public float lift = 0.034f;
        [Tooltip("How far up the forearm from the wrist joint, in metres.")]
        public float back = 0.03f;

        const string HandKey = "StudyEverywhere.WatchHand";
        public static event Action WornChanged;
        public static Hand Worn
        {
            get => PlayerPrefs.GetInt(HandKey, 0) == 1 ? Hand.Right : Hand.Left;
            set { PlayerPrefs.SetInt(HandKey, value == Hand.Right ? 1 : 0); PlayerPrefs.Save(); WornChanged?.Invoke(); }
        }

        static readonly Color PillDim = new Color(.2f, .16f, .1f), PillLit = new Color(.52f, .4f, .2f);
        static readonly List<XRHandSubsystem> handSubsystems = new List<XRHandSubsystem>();
        Canvas canvas;
        Collider[] colliders;
        float started;
        int shownMinute = -1;
        bool visible = true;

        void Awake()
        {
            canvas = GetComponent<Canvas>();
            colliders = GetComponentsInChildren<Collider>(true);
            started = Time.time;
        }

        void OnEnable() { if (press) press.onClick.AddListener(Press); Localization.Changed += Redraw; }
        void OnDisable() { if (press) press.onClick.RemoveListener(Press); Localization.Changed -= Redraw; }
        void Redraw() => shownMinute = -1;   // the date in the new language at the next frame

        void Press()
        {
            if (panel) panel.Toggle();
            started = Time.time - logoSeconds - menuSeconds;   // after a press, show the time
        }

        void LateUpdate()
        {
            if (!origin || !origin.Camera) return;
            bool worn = TryWrist(out var position, out var outward, out var along);
            SetVisible(worn);
            if (!worn) return;
            // Fixed to the wrist: the screen faces out of the back of the wrist, its right towards the fingers on the
            // left wrist (towards the elbow on the right), so its top runs across the wrist and the strap round it.
            var right = Worn == Hand.Left ? along : -along;
            transform.SetPositionAndRotation(position, Quaternion.LookRotation(-outward, Vector3.Cross(right, outward)));
            UpdateFaces();
        }

        // The back of the chosen wrist: tracked hand first, else that hand's controller.
        bool TryWrist(out Vector3 position, out Vector3 outward, out Vector3 along)
        {
            position = outward = along = default;
            bool left = Worn == Hand.Left;
            SubsystemManager.GetSubsystems(handSubsystems);
            foreach (var hands in handSubsystems)
            {
                var hand = left ? hands.leftHand : hands.rightHand;
                if (!hands.running || !hand.isTracked) continue;
                if (!hand.GetJoint(XRHandJointID.Wrist).TryGetPose(out var local)) continue;
                var space = handSpace ? handSpace : origin.CameraFloorOffsetObject.transform;
                var rotation = space.rotation * local.rotation;
                along = rotation * Vector3.forward;   // towards the fingers
                outward = rotation * Vector3.up;      // out of the back of the hand
                position = space.TransformPoint(local.position) + outward * lift - along * back;
                return true;
            }
            var controller = left ? leftController : rightController;
            if (!controller || !controller.gameObject.activeInHierarchy) return false;
            along = controller.forward; outward = controller.up;
            position = controller.position - along * 0.11f - outward * 0.02f + outward * lift;   // the wrist is behind and below the grip
            return true;
        }

        void SetVisible(bool value)
        {
            if (visible == value) return;
            visible = value;
            canvas.enabled = value;
            foreach (var c in colliders) if (c) c.enabled = value;
            foreach (var r in GetComponentsInChildren<Renderer>(true)) r.enabled = value;
        }

        // Logo, then Menu, then the time, each easing into the next.
        void UpdateFaces()
        {
            float total = logoSeconds + menuSeconds + timeSeconds, t = Mathf.Repeat(Time.time - started, total);
            float Weight(float from, float length)
            {
                float local = Mathf.Repeat(t - from + fadeSeconds, total) - fadeSeconds;   // fades in before its start
                float fadeIn = fadeSeconds > 0 ? Mathf.Clamp01((local + fadeSeconds) / fadeSeconds) : 1;
                float fadeOut = fadeSeconds > 0 ? Mathf.Clamp01((length - local) / fadeSeconds) : 1;
                return local > length ? 0 : Mathf.Min(fadeIn, fadeOut);
            }
            if (logoFace) logoFace.alpha = Weight(0, logoSeconds);
            if (menuFace) menuFace.alpha = Weight(logoSeconds, menuSeconds);
            if (timeFace) timeFace.alpha = Weight(logoSeconds + menuSeconds, timeSeconds);
            // Press breathes: a glow swells and fades about every glowSeconds, and the pill warms with it.
            float pulse = Mathf.Pow(Mathf.Sin(Mathf.PI * Mathf.Repeat(Time.time, glowSeconds) / glowSeconds), 2);
            if (pressGlow) { var c = pressGlow.color; c.a = 0.15f + 0.85f * pulse; pressGlow.color = c; }
            if (pressPill) pressPill.color = Color.Lerp(PillDim, PillLit, pulse);
            // The halo: dark most of the time, then a slow swell and fade (sin squared) every haloEvery seconds.
            if (halo)
            {
                float h = Mathf.Repeat(Time.time - started, haloEvery) / Mathf.Max(.1f, haloSeconds);
                var c = halo.color; c.a = h < 1 ? haloStrength * Mathf.Pow(Mathf.Sin(Mathf.PI * h), 2) : 0; halo.color = c;
            }
            var now = DateTime.Now;
            if (now.Minute == shownMinute) return;
            shownMinute = now.Minute;
            if (time) time.text = now.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.Contains("H") ? "H:mm" : "h:mm", CultureInfo.InvariantCulture);
            if (date) date.text = DateText(now);
        }

        // The date in the chosen language (Localization): a small table rather than CultureInfo, whose data a Quest
        // build may not carry. English WED APR 1, Korean 4월 1일 (수), Japanese 4月1日(水), Spanish MIÉ 1 ABR, French MER 1 AVR.
        static readonly string[][] Weekdays =
        {
            new[] { "SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT" },
            new[] { "일", "월", "화", "수", "목", "금", "토" },
            new[] { "日", "月", "火", "水", "木", "金", "土" },
            new[] { "DOM", "LUN", "MAR", "MIÉ", "JUE", "VIE", "SÁB" },
            new[] { "DIM", "LUN", "MAR", "MER", "JEU", "VEN", "SAM" },
        };
        static readonly string[][] Months =
        {
            new[] { "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC" },
            null, null,
            new[] { "ENE", "FEB", "MAR", "ABR", "MAY", "JUN", "JUL", "AGO", "SEP", "OCT", "NOV", "DIC" },
            new[] { "JANV", "FÉVR", "MARS", "AVR", "MAI", "JUIN", "JUIL", "AOÛT", "SEPT", "OCT", "NOV", "DÉC" },
        };
        static string DateText(DateTime d)
        {
            int l = Localization.Current, w = (int)d.DayOfWeek, m = d.Month - 1;
            switch (l)
            {
                case 1: return $"{d.Month}월 {d.Day}일 ({Weekdays[1][w]})";
                case 2: return $"{d.Month}月{d.Day}日({Weekdays[2][w]})";
                case 3:
                case 4: return $"{Weekdays[l][w]} {d.Day} {Months[l][m]}";
                default: return $"{Weekdays[0][w]} {Months[0][m]} {d.Day}";
            }
        }
    }
}
