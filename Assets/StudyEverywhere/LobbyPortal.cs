using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace StudyEverywhere
{
    // A location button in the lobby's wrist menu (PlaceMenu). Clicking it loads that place's scene, whose own rig and
    // placement take over; a location without a scene is built with its button disabled (Coming soon).
    public sealed class LobbyPortal : MonoBehaviour
    {
        [Tooltip("Scene to load (must be in the build settings); empty for a place that is not open yet.")]
        public string scene;
        public Button enter;
        public TMP_Text label;

        bool loading;

        // NewWorld AI (the hackathon build): only the lobby and the three free places open; every premium place keeps its
        // card and its crown but is locked, whichever button or menu leads to it.
        public static readonly string[] OpenScenes = { "Lobby", "Orbit", "Seoul", "Seabreeze" };
        public static bool IsOpen(string scene) => System.Array.IndexOf(OpenScenes, scene) >= 0;

        void OnEnable()
        {
            if (enter) enter.onClick.AddListener(Enter);
            if (!string.IsNullOrEmpty(scene) && !IsOpen(scene))
            {
                if (enter) enter.interactable = false;
                if (label) label.text = Localization.Tr("Premium");
            }
        }
        void OnDisable() { if (enter) enter.onClick.RemoveListener(Enter); }

        public void Enter()
        {
            if (loading || string.IsNullOrEmpty(scene) || !IsOpen(scene)) return;
            StartCoroutine(Load());
        }

        IEnumerator Load()
        {
            loading = true;
            if (enter) enter.interactable = false;
            string before = label ? label.text : null;
            if (label) label.text = "Opening…";
            var op = SceneManager.LoadSceneAsync(scene);
            if (op == null)
            {
                Debug.LogWarning($"New World: scene '{scene}' is not in the build settings.");
                loading = false;
                if (enter) enter.interactable = true;
                if (label) label.text = before;
                yield break;
            }
            while (!op.isDone) yield return null;
        }
    }
}
