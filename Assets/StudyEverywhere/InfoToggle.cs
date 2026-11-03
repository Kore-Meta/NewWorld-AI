using UnityEngine;
using UnityEngine.UI;

namespace StudyEverywhere
{
    // A round "i" button that shows a small panel of sources and credits over its card, and Close (or the "i" again)
    // that puts it away. The panel starts hidden.
    public sealed class InfoToggle : MonoBehaviour
    {
        public Button open, close;
        public GameObject panel;

        void Awake() { if (panel) panel.SetActive(false); }
        void OnEnable() { if (open) open.onClick.AddListener(Toggle); if (close) close.onClick.AddListener(Hide); }
        void OnDisable() { if (open) open.onClick.RemoveListener(Toggle); if (close) close.onClick.RemoveListener(Hide); if (panel) panel.SetActive(false); }

        public void Toggle() { if (panel) panel.SetActive(!panel.activeSelf); }
        public void Hide() { if (panel) panel.SetActive(false); }
    }
}
