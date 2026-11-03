using System;
using UnityEngine;
using UnityEngine.UI;

namespace StudyEverywhere
{
    // A place's music credits: every track of its playlist with its performer, source and licence, on a card laid over
    // a page of the place's menu (built with the place, from the music folder's CREDITS.txt). Its "i" buttons (beside
    // the track name in the menu, on the desk's music panel) open the menu and show the card; Close, or the menu being
    // put away, hides it again. A small dot marks the track that is playing.
    public sealed class MusicCredits : MonoBehaviour
    {
        public ParisMusic music;
        [Tooltip("The menu the card sits in; opened when an \"i\" button outside it is pressed.")] public WristPanel menu;
        [Tooltip("The card, laid over the page; hidden until shown.")] public GameObject card;
        public Button close;
        [Tooltip("The \"i\" buttons that show the card.")] public Button[] openers;
        [Tooltip("One row per playlist track, in playlist order.")] public RectTransform[] rows;
        [Tooltip("Marks the row of the track playing (moved to it, colour untouched so day and night colours hold).")] public RectTransform marker;
        bool wasOpen;

        void Awake() { if (card) card.SetActive(false); }

        void OnEnable()
        {
            if (close) close.onClick.AddListener(Hide);
            if (openers != null) foreach (var b in openers) if (b) b.onClick.AddListener(Show);
            if (music) music.Changed += Mark;
            Mark();
        }

        void OnDisable()
        {
            if (close) close.onClick.RemoveListener(Hide);
            if (openers != null) foreach (var b in openers) if (b) b.onClick.RemoveListener(Show);
            if (music) music.Changed -= Mark;
        }

        public void Show()
        {
            if (menu && !menu.IsOpen) menu.Open();
            if (card) card.SetActive(true);
            Mark();
        }

        public void Hide() { if (card) card.SetActive(false); }

        // the menu put away (its Close, the watch): the card goes with it, so the menu opens on its settings next time
        void Update()
        {
            bool open = !menu || menu.IsOpen;
            if (wasOpen && !open) Hide();
            wasOpen = open;
        }

        void Mark()
        {
            if (!marker || !music || music.playlist == null || rows == null) return;
            int i = Array.FindIndex(music.playlist, c => c && c.name == music.Title);
            bool on = i >= 0 && i < rows.Length && rows[i];
            marker.gameObject.SetActive(on);
            if (on) marker.anchoredPosition = new Vector2(marker.anchoredPosition.x, rows[i].anchoredPosition.y);
        }
    }
}
