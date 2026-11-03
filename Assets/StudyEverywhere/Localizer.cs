using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace StudyEverywhere
{
    // Puts a scene's text into the chosen language (Localization): every TextMeshPro text whose English wording is in
    // the table is swapped, now and whenever the language changes. Text that code changes later (a button that flips
    // between Pause and Play, a card that fills in) is noticed on the next look round, once a second, and translated
    // too; the English it started from is remembered so switching back restores it.
    public sealed class Localizer : MonoBehaviour
    {
        readonly Dictionary<TMP_Text, string> english = new Dictionary<TMP_Text, string>();
        readonly Dictionary<TMP_Text, string> shown = new Dictionary<TMP_Text, string>();
        float next;

        void OnEnable() { Localization.Changed += ApplyAll; ApplyAll(); }
        void OnDisable() => Localization.Changed -= ApplyAll;

        void Update()
        {
            if (Time.unscaledTime < next) return;
            next = Time.unscaledTime + 1;
            Apply(false);
        }

        void ApplyAll() => Apply(true);

        void Apply(bool all)
        {
            foreach (var t in FindObjectsByType<TMP_Text>(FindObjectsInactive.Include))
            {
                if (!t) continue;
                var now = t.text;
                // new text, or text code changed since we last set it: that is the English to translate from
                if (!shown.TryGetValue(t, out var last) || last != now) { english[t] = now; if (!all && !Localization.Knows(now)) { shown[t] = now; continue; } }
                var src = english[t];
                var tr = Localization.Tr(src);
                if (tr != now) { FitButtonLabel(t); t.text = tr; }
                shown[t] = tr;
            }
        }

        // A button's label, cut to one line with an ellipsis, shrinks to fit a longer translation instead (down to 60%).
        readonly HashSet<TMP_Text> fitted = new HashSet<TMP_Text>();
        void FitButtonLabel(TMP_Text t)
        {
            if (t.enableAutoSizing || !fitted.Add(t) || !t.GetComponentInParent<UnityEngine.UI.Selectable>()) return;
            float size = t.fontSize; t.fontSizeMax = size; t.fontSizeMin = size * .6f; t.enableAutoSizing = true;
        }
    }
}
