using TMPro;
using UnityEngine;

namespace StudyEverywhere
{
    // A heading drawn bold when the chosen language is Korean or Japanese: their thin fallback glyphs (Noto Sans) look
    // faint beside the Latin serif the heading was set in. Back to its own style in the other languages.
    [RequireComponent(typeof(TMP_Text))]
    public sealed class BoldInScript : MonoBehaviour
    {
        TMP_Text text; FontStyles own; bool known;

        void OnEnable() { Localization.Changed += Apply; Apply(); }
        void OnDisable() => Localization.Changed -= Apply;

        void Apply()
        {
            if (!text) text = GetComponent<TMP_Text>();
            if (!known) { own = text.fontStyle; known = true; }
            bool cjk = Localization.Code == "ko" || Localization.Code == "ja";
            text.fontStyle = cjk ? own | FontStyles.Bold : own;
        }
    }
}
