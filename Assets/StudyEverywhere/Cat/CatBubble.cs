using TMPro;
using UnityEngine;

namespace StudyEverywhere
{
    // Speech bubbles over Émile: an ivory bubble (brass rim, tail pointing down at the cat) for what the cat says,
    // typed out as it speaks; and a smaller sage bubble for what it heard from the person, which shows animated dots
    // while they are still talking. Both face the viewer, float above the cat's head, and fade when done.
    public sealed class CatBubble : MonoBehaviour
    {
        public Transform anchor;            // the cat's head bone
        public Transform viewer;            // the person's head
        public RectTransform says, heard;
        public CanvasGroup saysGroup, heardGroup;
        public TMP_Text saysText, heardText;
        public float charsPerSecond = 32, height = .26f;

        string sayFull = "", heardFull = "";
        float sayShown, sayHold, heardHold, saysAlpha, heardAlpha;
        bool listening, intro;

        // The pet's introduction as the person arrives (the user, Oct 27: its name, how to talk to it, that it can be
        // turned off): held until the person starts talking to it (Listening, Heard) or it says something else.
        public const string IntroLine = "My name is {0}. Call my name to talk with me. You can turn me off in the wrist menu.";
        public void Intro(string petName)
        {
            Say(string.Format(Localization.Tr(IntroLine), Localization.Tr(petName)));
            sayHold = float.PositiveInfinity; intro = true;
        }
        public void EndIntro() { if (!intro) return; intro = false; sayHold = 0; }

        public void Say(string text)
        {
            intro = false;
            sayFull = text ?? ""; sayShown = 0;
            sayHold = 2.5f + sayFull.Length / 14f;          // time to read after it is fully typed
            saysText.text = "";
            Fit(says, saysText, sayFull, 520);
        }

        public void Heard(string text)
        {
            EndIntro(); listening = false; heardFull = text ?? ""; heardText.text = heardFull; heardHold = 3 + heardFull.Length / 16f;
            Fit(heard, heardText, heardFull, 440);
        }

        public void Listening(bool on)
        {
            listening = on;
            if (on) { EndIntro(); heardFull = ""; heardHold = 0; Fit(heard, heardText, "· · ·", 440); }
        }

        // Size the bubble to its text (up to maxWidth canvas units), keeping the padding.
        static void Fit(RectTransform bubble, TMP_Text text, string s, float maxWidth)
        {
            var pref = text.GetPreferredValues(s, maxWidth - 48, 0);
            var size = new Vector2(Mathf.Clamp(pref.x + 48, 120, maxWidth), Mathf.Max(64, text.GetPreferredValues(s, Mathf.Clamp(pref.x, 72, maxWidth - 48), 0).y + 36));
            bubble.sizeDelta = size;
            text.rectTransform.sizeDelta = size - new Vector2(40, 28);
        }

        void LateUpdate()
        {
            float dt = Time.deltaTime;
            if (anchor) transform.position = anchor.position + Vector3.up * height;
            if (viewer)
            {
                var to = transform.position - viewer.position;
                if (to.sqrMagnitude > 1e-4f) transform.rotation = Quaternion.LookRotation(to, Vector3.up);
                // stay readable: a little bigger with distance
                float d = to.magnitude; transform.localScale = Vector3.one * .00045f * Mathf.Clamp(d / 1.2f, .8f, 2.2f);
            }

            // What the cat says: typed out, held, then faded.
            if (sayShown < sayFull.Length)
            {
                sayShown = Mathf.Min(sayFull.Length, sayShown + dt * charsPerSecond);
                saysText.text = sayFull.Substring(0, Mathf.FloorToInt(sayShown));
            }
            else sayHold -= dt;
            float sayTarget = sayFull.Length > 0 && sayHold > 0 ? 1 : 0;
            saysAlpha = Mathf.MoveTowards(saysAlpha, sayTarget, dt * 4);
            saysGroup.alpha = saysAlpha; says.gameObject.SetActive(saysAlpha > 0);

            // What it heard: dots while listening, then the words.
            if (listening) heardText.text = new string('·', 1 + (int)(Time.time * 3) % 3).Replace("·", "· ").Trim();
            else heardHold -= dt;
            float heardTarget = listening || (heardFull.Length > 0 && heardHold > 0) ? 1 : 0;
            heardAlpha = Mathf.MoveTowards(heardAlpha, heardTarget, dt * 4);
            heardGroup.alpha = heardAlpha; heard.gameObject.SetActive(heardAlpha > 0);
            // stack: the heard bubble sits above the cat's reply while both show
            heard.anchoredPosition = new Vector2(-40, says.gameObject.activeSelf ? says.sizeDelta.y + 30 : 0);
        }
    }
}
