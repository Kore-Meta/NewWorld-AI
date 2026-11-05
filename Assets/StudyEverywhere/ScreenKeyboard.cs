using System;
using UnityEngine;

namespace StudyEverywhere
{
    // Typing on the shared computer from the headset: the bar's Keyboard opens the Quest's own system keyboard (the
    // overlay keyboard Horizon OS shows for Android text entry; AndroidSystemKeyboard declares it in the manifest), and
    // what is typed there goes on to the computer as it is typed, into the window in focus on it (WebRtcScreenSource:
    // Typing). The keyboard's field starts empty each time, so each change is sent as new text or as Backspaces; its
    // Return is Enter on the computer.
    public sealed class ScreenKeyboard : MonoBehaviour
    {
        public WebRtcScreenSource source;
        [Tooltip("Tells the person why typing can't start (the bar shows it).")] public Action<string> note;

        TouchScreenKeyboard keyboard;
        string sent = "";

        public bool Showing => keyboard != null;

        public void Toggle()
        {
            if (keyboard != null) { Close(); return; }
            if (!source || !source.CanType)
            {
                note?.Invoke(Localization.Tr("To type on your computer, update New World Desktop there and turn on “Let the headset type”."));
                return;
            }
            sent = "";
            keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, true, false, false, Localization.Tr("Type on your computer"));
            if (keyboard == null) note?.Invoke(Localization.Tr("The headset's keyboard didn't open. Try again."));
        }

        void Update()
        {
            if (keyboard == null) return;
            if (!source || !source.CanType) { Close(); return; }
            var now = keyboard.text ?? "";
            if (now != sent)
            {
                int same = 0;
                while (same < now.Length && same < sent.Length && now[same] == sent[same]) same++;
                if (sent.Length > same) source.TypeKey("Backspace", sent.Length - same);
                if (now.Length > same) source.TypeText(now.Substring(same));
                sent = now;
            }
            if (keyboard.status != TouchScreenKeyboard.Status.Visible) keyboard = null;   // done or dismissed
        }

        void Close()
        {
            if (keyboard != null) keyboard.active = false;
            keyboard = null;
        }

        void OnDisable() => Close();
    }
}
