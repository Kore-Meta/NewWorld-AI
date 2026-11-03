using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StudyEverywhere
{
    // The Language row on a menu: a pill naming the current language with a little arrow; press it and the list of
    // languages drops open beside it (each written in its own language), pick one and it closes. The whole app switches
    // at once (Localization, Localizer).
    public sealed class LanguagePicker : MonoBehaviour
    {
        public Button toggle;
        public TMP_Text current;
        public GameObject list;
        public Button[] options;   // same order as Localization.Codes

        void OnEnable()
        {
            if (toggle) toggle.onClick.AddListener(Toggle);
            for (int i = 0; i < options.Length; i++) { int k = i; if (options[i]) options[i].onClick.AddListener(() => Pick(k)); }
            Localization.Changed += Refresh;
            if (list) list.SetActive(false);
            Refresh();
        }
        void OnDisable()
        {
            if (toggle) toggle.onClick.RemoveListener(Toggle);
            foreach (var o in options) if (o) o.onClick.RemoveAllListeners();
            Localization.Changed -= Refresh;
        }

        void Toggle() { if (list) { var p = list.transform.localPosition; p.z = -30; list.transform.localPosition = p; list.SetActive(!list.activeSelf); } }
        void Pick(int i) { Localization.Current = i; if (list) list.SetActive(false); Refresh(); }
        void Refresh() { if (current) current.text = Localization.Names[Localization.Current]; }   // the chevron is drawn beside it (the fonts have no ▾)
    }
}
