using System;
using UnityEngine;
using UnityEngine.UI;

namespace StudyEverywhere
{
    // Where to go, inside a wrist menu: a home page of kinds of place (Library, Café, Rest Area, and the Lobby when away
    // from it), each opening a page of that kind's locations. Every location is a button whose LobbyPortal loads its
    // scene; the current place and places not open yet are disabled. Used by the lobby's PlaceMenu and, behind its
    // Locations button, by the café's WristMenu (whose Back on the home page returns to its settings).
    public sealed class PlaceChooser : MonoBehaviour
    {
        [Serializable]
        public struct Category
        {
            public string name;
            public Button open;      // on the home page
            public GameObject page;  // its locations
            public Button back;      // on its page, back to the home page
        }

        public GameObject home;
        public Category[] categories;
        [Tooltip("Optional Back on the home page: leaves the chooser.")]
        public Button exit;

        public event Action Exited;

        void OnEnable()
        {
            for (int i = 0; i < categories.Length; i++)
            {
                int k = i;
                categories[i].open.onClick.AddListener(() => Show(k));
                categories[i].back.onClick.AddListener(() => Show(-1));
            }
            if (exit) exit.onClick.AddListener(Exit);
            Show(-1);
        }

        void OnDisable()
        {
            foreach (var c in categories) { c.open.onClick.RemoveAllListeners(); c.back.onClick.RemoveAllListeners(); }
            if (exit) exit.onClick.RemoveListener(Exit);
        }

        void Exit() => Exited?.Invoke();

        // -1 shows the kinds of place; otherwise that category's locations.
        public void Show(int category)
        {
            if (home) home.SetActive(category < 0);
            for (int i = 0; i < categories.Length; i++) if (categories[i].page) categories[i].page.SetActive(i == category);
        }
    }
}
