using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StudyEverywhere
{
    // A floating card before one of the lobby's entrances (Library, Café, Rest Area). Enter turns it to a carousel of
    // that kind's locations, one at a time: previous / next step through them, Go takes the person there (through the
    // LobbyPortal on the Go button), Back returns to the card. Locations without a scene say Coming soon; premium ones
    // show a gold crown (greyed out only while UnlockAllForCompetition is off).
    public sealed class PlaceCard : MonoBehaviour
    {
        // (revision 102929, the user, Oct 29: unlock every place for the competition. Premium places with a scene open
        // like the free ones, Go and all; the crown stays, as a mark, not a lock. The wrist menus (BuildPlaceChooser:
        // PlaceChooser, PlaceMenu, the café menu) already let premium places with a scene be entered; this card was the
        // only lock. Set false to grey premium places out again; read at runtime, so no scene rebuild either way.)
        public static readonly bool UnlockAllForCompetition = false;   // NewWorld AI: premium places locked (LobbyPortal.OpenScenes); static readonly, not const: no unreachable-code warning
        static bool Locked(Location l) => l.premium && !UnlockAllForCompetition;

        [Serializable]
        public struct Location
        {
            public string name, detail;
            [Tooltip("Scene to load; empty while the location is not open yet.")]
            public string scene;
            [Tooltip("Premium: shown with a gold crown; greyed out and not selectable unless PlaceCard.UnlockAllForCompetition.")]
            public bool premium;
        }

        public Location[] locations;
        public GameObject front, chooser;
        public Button enter, back, previous, next, go;
        public TMP_Text placeName, placeDetail, counter, goLabel;
        public GameObject crown;
        public LobbyPortal portal;

        int index;

        void Awake() => ShowChooser(false);

        void OnEnable()
        {
            enter.onClick.AddListener(() => ShowChooser(true));
            back.onClick.AddListener(() => ShowChooser(false));
            previous.onClick.AddListener(() => Step(-1));
            next.onClick.AddListener(() => Step(1));
        }

        void OnDisable()
        {
            enter.onClick.RemoveAllListeners(); back.onClick.RemoveAllListeners();
            previous.onClick.RemoveAllListeners(); next.onClick.RemoveAllListeners();
        }

        public void ShowChooser(bool on)
        {
            if (front) front.SetActive(!on);
            if (chooser) chooser.SetActive(on);
            if (on) Refresh();
        }

        void Step(int delta)
        {
            if (locations == null || locations.Length == 0) return;
            index = (index + delta + locations.Length) % locations.Length;
            Refresh();
        }

        void Refresh()
        {
            if (locations == null || locations.Length == 0) return;
            var l = locations[index];
            bool locked = Locked(l), open = !string.IsNullOrEmpty(l.scene) && !locked;   // (revision 102929: premium no longer locks)
            placeName.text = l.name;
            placeName.color = locked ? UIStyle.Secondary : UIStyle.Ink;
            if (crown) crown.SetActive(l.premium);
            placeDetail.text = l.detail;
            counter.text = $"{index + 1} / {locations.Length}";
            portal.scene = open ? l.scene : "";
            go.interactable = open;
            goLabel.text = locked ? "Premium" : open ? "Go" : "Coming soon";
            previous.interactable = next.interactable = locations.Length > 1;
        }
    }
}
