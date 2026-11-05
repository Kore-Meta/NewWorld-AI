using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace StudyEverywhere
{
    // The one control bar for every shared screen, in the place's own card style (cloned from the workspace card): it
    // floats low in front of the seat, tilted up like a tablet above the keyboard, so it stays in reach however big the
    // screens are. Top row: what is shared and whether it is live, then Smaller / size / Bigger, Keyboard (typing on the
    // computer, ScreenKeyboard), Bring to me and Stop sharing. Bottom row: one chip per display, live or paused, each with its own Stop.
    public sealed class ScreenControls : MonoBehaviour
    {
        // Styled pieces of the workspace card, cloned so the bar matches the place's menus.
        public struct Style
        {
            public Image card, trim;   // ivory card and its brass trim
            public Image dot;          // live dot
            public TMP_Text label;     // eyebrow text
            public Button pill;        // ivory pill button with a TMP label
            public Type raycaster;     // the card canvas's XR raycaster
        }

        const float CanvasScale = 0.0005f;   // metres per canvas unit, as the place's panels
        static readonly Vector2 Size = new Vector2(1800, 230);

        Style style;
        RectTransform root, chipRow;
        Image dot;
        TMP_Text summary, sizeLabel, note;
        float noteUntil = -1;
        readonly Dictionary<int, (Image dot, TMP_Text label)> chips = new Dictionary<int, (Image, TMP_Text)>();
        Action<int> stopOne;

        public static ScreenControls Create(Style style, UnityAction smaller, UnityAction bigger, UnityAction keyboard, UnityAction bringHere, UnityAction stopAll, Action<int> stopOne)
        {
            var go = new GameObject("Shared screens | controls", typeof(RectTransform));
            var c = go.AddComponent<ScreenControls>();
            c.style = style;
            c.stopOne = stopOne;
            c.root = (RectTransform)go.transform;
            c.root.localScale = Vector3.one * CanvasScale;
            c.root.sizeDelta = Size;
            go.AddComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            go.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 3;
            if (style.raycaster != null) go.AddComponent(style.raycaster);

            c.Clone(style.trim, "Brass trim", Vector2.zero, Size);
            c.Clone(style.card, "Card", Vector2.zero, Size - new Vector2(8, 8));

            // Top row, right to left: Stop sharing, Bring to me, Keyboard, Bigger, the size, Smaller.
            float right = Size.x / 2 - 24, top = 46;
            right = c.Pill("Stop sharing", 250, right, top, stopAll);
            right = c.Pill("Bring to me", 230, right - 18, top, bringHere);
            if (keyboard != null) right = c.Pill("Keyboard", 210, right - 18, top, keyboard);   // (none: no keyboard in this build)
            right = c.Pill("+  Bigger", 200, right - 18, top, bigger);
            c.sizeLabel = c.Text("Size 100%", new Vector2(right - 75, top), new Vector2(150, 50), 24, UIStyle.Ink, TextAlignmentOptions.Center);
            right = c.Pill("–  Smaller", 210, right - 150, top, smaller);

            float left = -Size.x / 2 + 30;
            c.dot = c.Clone(style.dot, "Live dot", new Vector2(left + 8, top), new Vector2(16, 16));
            c.summary = c.Text("SHARING", new Vector2((left + 30 + right - 20) / 2, top), new Vector2(right - 20 - (left + 30), 50), 24, UIStyle.Sage, TextAlignmentOptions.MidlineLeft);
            c.summary.fontStyle = FontStyles.Bold;

            c.chipRow = new GameObject("Displays", typeof(RectTransform)).GetComponent<RectTransform>();
            c.chipRow.SetParent(c.root, false);
            c.chipRow.anchoredPosition = new Vector2(0, -52);
            c.chipRow.sizeDelta = new Vector2(Size.x - 48, 80);
            c.note = c.Text("", new Vector2(0, -52), new Vector2(Size.x - 60, 70), 24, UIStyle.Ink, TextAlignmentOptions.Center);
            c.note.textWrappingMode = TextWrappingModes.Normal;
            c.note.gameObject.SetActive(false);
            return c;
        }

        // Low in front of the seat, a little above the keyboard, tilted up towards the eyes.
        public void Place(Vector3 pivot, float yaw)
        {
            var rot = Quaternion.Euler(0, yaw, 0);
            root.SetPositionAndRotation(pivot + rot * new Vector3(0, -0.3f, 0.5f), rot * Quaternion.Euler(32, 0, 0));
        }

        public void SetSummary(string text, bool live)
        {
            summary.text = text;
            dot.color = live ? UIStyle.Live : UIStyle.Brass;
        }

        public void Pulse(float time)
        {
            var c = UIStyle.Live;
            c.a = 0.55f + 0.45f * Mathf.Cos(time * 3);
            dot.color = c;
        }

        // A short message in place of the display chips for a few seconds (why the keyboard can't type, say).
        public void Note(string text, float seconds = 6)
        {
            note.text = text;
            note.gameObject.SetActive(true);
            chipRow.gameObject.SetActive(false);
            noteUntil = Time.time + seconds;
        }

        void Update()
        {
            if (noteUntil < 0 || Time.time < noteUntil) return;
            noteUntil = -1;
            note.gameObject.SetActive(false);
            chipRow.gameObject.SetActive(true);
        }

        public void SetSize(float percent) => sizeLabel.text = Localization.Format("Size {0}%", Mathf.RoundToInt(percent));

        // One chip per display, left to right in the order given: its dot, its name and LIVE / PAUSED, and Stop.
        public void SetDisplays(IList<(int id, string name, bool live)> displays)
        {
            foreach (Transform child in chipRow) Destroy(child.gameObject);
            chips.Clear();
            if (displays.Count == 0) return;
            float width = Mathf.Min(520, (chipRow.sizeDelta.x - 16 * (displays.Count - 1)) / displays.Count);
            float x = -chipRow.sizeDelta.x / 2 + width / 2;
            foreach (var d in displays)
            {
                var chip = new GameObject("Display chip | " + d.name, typeof(RectTransform)).GetComponent<RectTransform>();
                chip.SetParent(chipRow, false);
                chip.anchoredPosition = new Vector2(x, 0);
                chip.sizeDelta = new Vector2(width, 66);
                var bg = Instantiate(style.trim, chip);
                bg.name = "Chip"; bg.raycastTarget = false;
                bg.color = new Color(UIStyle.Brass.r, UIStyle.Brass.g, UIStyle.Brass.b, .18f);
                Fill(bg.rectTransform);
                var dDot = Instantiate(style.dot, chip);
                dDot.name = "Live dot"; dDot.raycastTarget = false;
                dDot.rectTransform.sizeDelta = new Vector2(14, 14);
                dDot.rectTransform.anchoredPosition = new Vector2(-width / 2 + 24, 0);
                int id = d.id;
                var stop = Instantiate(style.pill, chip);
                stop.name = "Stop display";
                stop.gameObject.SetActive(true);
                stop.interactable = true;
                var srt = (RectTransform)stop.transform;
                srt.sizeDelta = new Vector2(120, 50);
                srt.anchoredPosition = new Vector2(width / 2 - 68, 0);
                Label(stop, "Stop", 20);
                stop.onClick = new Button.ButtonClickedEvent();
                stop.onClick.AddListener(() => stopOne(id));
                var label = Instantiate(style.label, chip);
                label.name = "Display name";
                Dress(label, 21, UIStyle.Ink, TextAlignmentOptions.MidlineLeft);
                label.rectTransform.sizeDelta = new Vector2(width - 40 - 150, 50);
                label.rectTransform.anchoredPosition = new Vector2(-width / 2 + 40 + label.rectTransform.sizeDelta.x / 2, 0);
                chips[id] = (dDot, label);
                Mark(id, d.name, d.live);
                x += width + 16;
            }
        }

        public void Mark(int id, string name, bool live)
        {
            if (!chips.TryGetValue(id, out var chip)) return;
            chip.dot.color = live ? UIStyle.Live : UIStyle.Brass;
            chip.label.text = name + " · " + Localization.Tr(live ? "Live" : "Paused");
        }

        // ---- Building ----------------------------------------------------------------------------------------------

        float Pill(string text, float width, float right, float y, UnityAction onClick)
        {
            var b = Instantiate(style.pill, root);
            b.name = "Button | " + text;
            b.gameObject.SetActive(true);
            b.interactable = true;
            var rt = (RectTransform)b.transform;
            rt.sizeDelta = new Vector2(width, 64);
            rt.anchoredPosition = new Vector2(right - width / 2, y);
            Label(b, text, 24);
            b.onClick = new Button.ButtonClickedEvent();
            b.onClick.AddListener(onClick);
            return right - width;
        }

        static void Label(Button b, string text, float size)
        {
            var t = b.GetComponentInChildren<TMP_Text>(true);
            if (!t) return;
            t.text = text;
            t.fontSize = size;
            t.enableAutoSizing = false;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.rectTransform.anchoredPosition = Vector2.zero;
            t.rectTransform.sizeDelta = ((RectTransform)b.transform).sizeDelta;
        }

        TMP_Text Text(string text, Vector2 pos, Vector2 size, float fontSize, Color colour, TextAlignmentOptions align)
        {
            var t = Instantiate(style.label, root);
            t.name = "Text | " + text;
            Dress(t, fontSize, colour, align);
            t.text = text;
            t.rectTransform.anchoredPosition = pos;
            t.rectTransform.sizeDelta = size;
            return t;
        }

        static void Dress(TMP_Text t, float fontSize, Color colour, TextAlignmentOptions align)
        {
            t.fontSize = fontSize;
            t.enableAutoSizing = false;
            t.color = colour;
            t.alignment = align;
            t.fontStyle = FontStyles.Normal;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.overflowMode = TextOverflowModes.Ellipsis;
            t.raycastTarget = false;
        }

        Image Clone(Image template, string name, Vector2 pos, Vector2 size)
        {
            var i = Instantiate(template, root);
            i.name = name;
            i.gameObject.SetActive(true);
            i.raycastTarget = false;
            i.rectTransform.anchorMin = i.rectTransform.anchorMax = new Vector2(.5f, .5f);
            i.rectTransform.anchoredPosition = pos;
            i.rectTransform.sizeDelta = size;
            foreach (Transform child in i.transform) Destroy(child.gameObject);
            return i;
        }

        static void Fill(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.anchoredPosition = Vector2.zero; rt.sizeDelta = Vector2.zero;
            foreach (Transform child in rt) Destroy(child.gameObject);
        }
    }
}
