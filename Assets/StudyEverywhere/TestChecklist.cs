using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace StudyEverywhere
{
    // Temporary, for headset testing only: while a place's wrist menu (WristPanel) is open, a "Test checks" pill hangs
    // just to its right; pressing it unfolds, beside the menu and never over it, this build's list of things to check in
    // the current scene, each with a tick box. Ticks are kept in persistentDataPath/test-checklist.json (and echoed to
    // the log as [Checklist]) for pulling over adb; anything left unticked is what we talk through next. Nothing leaves
    // the headset. The list is Resources/TestChecklist.json (per place by scene name, plus scene "*" for checks every
    // place carries, so the pill is in all of them); delete it and the whole thing stays away.
    public sealed class TestChecklist : MonoBehaviour
    {
        [Serializable] public class Item { public string id, text; }
        [Serializable] public class SceneList { public string scene; public Item[] items; }
        [Serializable] public class Data { public string build; public SceneList[] scenes; }
        [Serializable] public class Tick { public string scene, id, text; public bool done; public string at; }
        [Serializable] public class Saved { public string build; public List<Tick> ticks = new List<Tick>(); }

        const float Scale = .00035f, Width = 900, RowHeight = 96, Gap = .03f;
        const int PerPage = 6;
        static readonly Color Ink = new Color(.12f, .16f, .22f), Muted = new Color(.42f, .45f, .5f), Card = new Color(.97f, .96f, .93f),
            Accent = new Color(.3f, .42f, .53f), Done = new Color(.29f, .52f, .36f), Box = new Color(.86f, .87f, .84f);

        Data data;
        Saved saved;
        string path;
        WristPanel menu;
        Canvas canvas;
        RectTransform root, body;
        TMP_Text pillLabel, pageLabel;
        Button prev, next;
        readonly List<(Button box, Image fill, TMP_Text text)> rows = new List<(Button, Image, TMP_Text)>();
        Item[] items = Array.Empty<Item>();
        string scene;
        int page;
        bool expanded;
        TMP_FontAsset font;
        Sprite rounded;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            var json = Resources.Load<TextAsset>("TestChecklist");
            if (!json) return;
            var go = new GameObject("Test checklist (temporary)");
            DontDestroyOnLoad(go);
            var c = go.AddComponent<TestChecklist>();
            c.data = JsonUtility.FromJson<Data>(json.text);
        }

        void Awake()
        {
            path = Path.Combine(Application.persistentDataPath, "test-checklist.json");
            try { saved = File.Exists(path) ? JsonUtility.FromJson<Saved>(File.ReadAllText(path)) : null; } catch { saved = null; }
            SceneManager.sceneLoaded += (s, m) => Forget();
        }

        void Start()
        {
            if (saved == null || saved.build != data.build) { saved = new Saved { build = data.build }; Save(); }
            Debug.Log($"[Checklist] build '{data.build}', results in {path}");
        }

        void Forget() { if (root) Destroy(root.gameObject); root = null; menu = null; rows.Clear(); expanded = false; page = 0; }

        void LateUpdate()
        {
            if (!menu) menu = FindAnyObjectByType<WristPanel>();
            if (!menu) return;
            if (!root) Build();
            if (!root) return;
            bool show = menu.IsOpen;
            if (canvas.enabled != show) { canvas.enabled = show; if (!show) Expand(false); }
            if (!show) return;
            Place(menu.origin && menu.origin.Camera ? menu.origin.Camera.transform.position : (Vector3?)null);
        }

        // Beside the menu's right edge, its top level with the menu's top, turned to face the eyes.
        void Place(Vector3? head)
        {
            var m = (RectTransform)menu.transform;
            float half = m.rect.width * .5f * m.lossyScale.x, mine = Width * .5f * Scale;
            var at = m.position + m.right * (half + Gap + mine) + m.up * (m.rect.height * .5f * m.lossyScale.y) - m.forward * .08f;
            var face = Vector3.ProjectOnPlane(at - (head ?? at - m.forward), Vector3.up);
            root.SetPositionAndRotation(at, Quaternion.LookRotation(face.sqrMagnitude > 1e-4f ? face : m.forward, Vector3.up));
        }

        // For the editor's close-ups: the list unfolded beside a menu that is shown by hand.
        public static GameObject Preview(WristPanel beside, Vector3 head)
        {
            var json = Resources.Load<TextAsset>("TestChecklist"); if (!json) return null;
            var go = new GameObject("Test checklist (preview)");
            var c = go.AddComponent<TestChecklist>();
            c.data = JsonUtility.FromJson<Data>(json.text); c.saved = new Saved { build = c.data.build }; c.menu = beside;
            c.Build(); if (!c.root) { DestroyImmediate(go); return null; }
            c.root.SetParent(go.transform, true); c.canvas.enabled = true; c.Expand(true); c.Place(head);
            return go;
        }

        // ---- the panel, made in code from the menu's own font and rounded card
        void Build()
        {
            scene = SceneManager.GetActiveScene().name;
            // this place's own checks, then the ones every place carries (listed under scene "*")
            var lists = data.scenes ?? Array.Empty<SceneList>();
            var own = Array.Find(lists, s => s.scene == scene)?.items ?? Array.Empty<Item>();
            var every = Array.Find(lists, s => s.scene == "*")?.items ?? Array.Empty<Item>();
            var all = new List<Item>(own); all.AddRange(every); items = all.ToArray();
            if (items.Length == 0) return;
            var anyText = menu.GetComponentInChildren<TMP_Text>(true);
            font = anyText ? anyText.font : TMP_Settings.defaultFontAsset;
            foreach (var img in menu.GetComponentsInChildren<Image>(true)) if (img.sprite && img.type == Image.Type.Sliced) { rounded = img.sprite; break; }

            var go = new GameObject("Test checklist | beside the wrist menu", typeof(RectTransform));
            go.layer = menu.gameObject.layer;
            root = (RectTransform)go.transform;
            root.sizeDelta = new Vector2(Width, 100); root.pivot = new Vector2(.5f, 1); root.localScale = Vector3.one * Scale;
            canvas = go.AddComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace; canvas.sortingOrder = 55;
            go.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 3;
            go.AddComponent<TrackedDeviceGraphicRaycaster>();
            if (menu.origin && menu.origin.Camera) canvas.worldCamera = menu.origin.Camera;

            // the pill: always there while the menu is open
            var pill = Panel("Test checks pill", root, new Vector2(0, -40), new Vector2(Width, 80), Accent);
            var pb = pill.gameObject.AddComponent<Button>(); pb.targetGraphic = pill.GetComponent<Image>();
            pb.onClick.AddListener(() => Expand(!expanded));
            pillLabel = Text("Label", pill, Vector2.zero, new Vector2(Width - 40, 70), 30, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);

            // the list, below the pill
            float h = 70 + PerPage * RowHeight + 90;
            body = Panel("List", root, new Vector2(0, -90 - h / 2), new Vector2(Width, h), Card);
            Text("Heading", body, new Vector2(0, h / 2 - 40), new Vector2(Width - 80, 50), 24, Muted, TextAlignmentOptions.Left, FontStyles.Bold)
                .text = $"{scene.ToUpperInvariant()}  ·  {data.build}";
            for (int i = 0; i < PerPage; i++)
            {
                float y = h / 2 - 70 - RowHeight / 2 - i * RowHeight;
                var box = Panel($"Row {i + 1} box", body, new Vector2(-Width / 2 + 60, y), new Vector2(54, 54), Box);
                var fill = Panel("Tick", box, Vector2.zero, new Vector2(34, 34), Done).GetComponent<Image>();
                var b = box.gameObject.AddComponent<Button>(); b.targetGraphic = box.GetComponent<Image>();
                var text = Text($"Row {i + 1} text", body, new Vector2(45, y), new Vector2(Width - 150, RowHeight - 6), 27, Ink, TextAlignmentOptions.Left, FontStyles.Normal);
                text.textWrappingMode = TextWrappingModes.Normal;
                // the text is a big target too
                var tb = text.gameObject.AddComponent<Button>(); tb.transition = Selectable.Transition.None;
                int k = i; b.onClick.AddListener(() => Toggle(k)); tb.onClick.AddListener(() => Toggle(k));
                rows.Add((b, fill, text));
            }
            prev = Pill("Previous", new Vector2(-Width / 2 + 120, -h / 2 + 48), () => { page--; Draw(); });
            next = Pill("Next", new Vector2(Width / 2 - 120, -h / 2 + 48), () => { page++; Draw(); });
            pageLabel = Text("Page", body, new Vector2(0, -h / 2 + 48), new Vector2(240, 50), 22, Muted, TextAlignmentOptions.Center, FontStyles.Normal);
            canvas.enabled = false;
            Expand(false);
        }

        Button Pill(string label, Vector2 at, Action click)
        {
            var p = Panel(label, body, at, new Vector2(190, 60), Accent);
            var b = p.gameObject.AddComponent<Button>(); b.targetGraphic = p.GetComponent<Image>(); b.onClick.AddListener(() => click());
            Text("Label", p, Vector2.zero, new Vector2(180, 56), 22, Color.white, TextAlignmentOptions.Center, FontStyles.Bold).text = label;
            return b;
        }

        RectTransform Panel(string name, RectTransform parent, Vector2 at, Vector2 size, Color colour)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.layer = parent.gameObject.layer;
            var rt = (RectTransform)go.transform; rt.SetParent(parent, false); rt.anchoredPosition = at; rt.sizeDelta = size;
            var img = go.AddComponent<Image>(); img.color = colour;
            if (rounded) { img.sprite = rounded; img.type = Image.Type.Sliced; }
            return rt;
        }

        TMP_Text Text(string name, RectTransform parent, Vector2 at, Vector2 size, float fontSize, Color colour, TextAlignmentOptions align, FontStyles style)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.layer = parent.gameObject.layer;
            var rt = (RectTransform)go.transform; rt.SetParent(parent, false); rt.anchoredPosition = at; rt.sizeDelta = size;
            var t = go.AddComponent<TextMeshProUGUI>(); if (font) t.font = font;
            t.fontSize = fontSize; t.color = colour; t.alignment = align; t.fontStyle = style; t.textWrappingMode = TextWrappingModes.NoWrap;
            return t;
        }

        void Expand(bool on) { expanded = on; if (body) body.gameObject.SetActive(on); Draw(); }

        // ---- ticks
        Tick Find(Item item) => saved.ticks.Find(t => t.scene == scene && t.id == item.id);

        void Toggle(int row)
        {
            int i = page * PerPage + row; if (i >= items.Length) return;
            var item = items[i]; var t = Find(item);
            if (t == null) { t = new Tick { scene = scene, id = item.id, text = item.text }; saved.ticks.Add(t); }
            t.done = !t.done; t.at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Debug.Log($"[Checklist] {scene} / {item.id}: {(t.done ? "OK" : "not OK")} — {item.text}");
            Save(); Draw();
        }

        void Save()
        {
            try { File.WriteAllText(path, JsonUtility.ToJson(saved, true)); } catch (Exception e) { Debug.LogWarning("[Checklist] could not save: " + e.Message); }
        }

        void Draw()
        {
            if (!root) return;
            int pages = Mathf.Max(1, (items.Length + PerPage - 1) / PerPage); page = Mathf.Clamp(page, 0, pages - 1);
            int done = 0; foreach (var it in items) if (Find(it)?.done == true) done++;
            pillLabel.text = expanded ? $"Test checks  {done} / {items.Length}   ·  Hide" : $"Test checks  {done} / {items.Length}   ·  Show";
            for (int r = 0; r < rows.Count; r++)
            {
                int i = page * PerPage + r; bool has = i < items.Length;
                rows[r].box.gameObject.SetActive(has); rows[r].text.gameObject.SetActive(has);
                if (!has) continue;
                bool tick = Find(items[i])?.done == true;
                rows[r].fill.enabled = tick;
                rows[r].text.text = $"{i + 1}. {items[i].text}";
                rows[r].text.color = tick ? Muted : Ink;
            }
            prev.gameObject.SetActive(page > 0); next.gameObject.SetActive(page < pages - 1);
            pageLabel.text = pages > 1 ? $"{page + 1} of {pages}" : "";
        }
    }
}
