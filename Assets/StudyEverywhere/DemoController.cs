using UnityEngine;

namespace StudyEverywhere
{
    // A controller model (XRI's universal controller) acting out the lobby's controller lessons: the trigger pulls, the
    // grip squeezes, the thumbstick tips forward or to the sides, a face button presses; the part in use glows gold on
    // the beat, so the person sees which one to use. Driven by LobbyWelcome.ActController, a two-second loop per lesson.
    public sealed class DemoController : MonoBehaviour
    {
        public Transform trigger, grip, thumbstick, menuButton, faceButton;
        public Material glow;
        [Tooltip("1 the right controller, -1 the left: which way it tips to turn.")] public int side = 1;

        Quaternion triggerRest, stickRest; Vector3 gripRest, menuRest, faceRest;
        Renderer[] parts; Material[][] own;
        bool ready;

        void Setup()
        {
            if (ready) return;
            if (trigger) triggerRest = trigger.localRotation;
            if (thumbstick) stickRest = thumbstick.localRotation;
            if (grip) gripRest = grip.localPosition;
            if (menuButton) menuRest = menuButton.localPosition;
            if (faceButton) faceRest = faceButton.localPosition;
            var ts = new[] { trigger, grip, thumbstick, menuButton, faceButton };
            parts = new Renderer[ts.Length]; own = new Material[ts.Length][];
            for (int i = 0; i < ts.Length; i++) if (ts[i] && ts[i].TryGetComponent(out parts[i])) own[i] = parts[i].sharedMaterials;
            ready = true;
        }

        // Rests everything, then moves and lights the part this lesson uses. t: seconds into the lesson.
        public void Act(string lesson, float t)
        {
            Setup();
            float beat = Mathf.Repeat(t, 2) / 2, pulse = Mathf.SmoothStep(0, 1, Mathf.PingPong(beat * 2, 1));
            if (trigger) trigger.localRotation = triggerRest;
            if (thumbstick) thumbstick.localRotation = stickRest;
            if (grip) grip.localPosition = gripRest;
            if (menuButton) menuButton.localPosition = menuRest;
            if (faceButton) faceButton.localPosition = faceRest;
            int lit = -1;
            switch (lesson)
            {
                case "pinch":      // pull the trigger
                    if (trigger) trigger.localRotation = triggerRest * Quaternion.Euler(-15 * pulse, 0, 0);
                    lit = 0; break;
                case "grab":       // squeeze the grip
                    if (grip) grip.localPosition = gripRest + Vector3.right * .0025f * pulse * side;
                    lit = 1; break;
                case "teleport":   // stick forward, hold, let go
                    if (thumbstick) thumbstick.localRotation = stickRest * Quaternion.Euler(-28 * pulse, 0, 0);
                    lit = 2; break;
                case "turn":       // flick right, then left
                    if (thumbstick) thumbstick.localRotation = stickRest * Quaternion.Euler(0, 0, (Mathf.Repeat(t, 4) < 2 ? -28 : 28) * pulse);
                    lit = 2; break;
                case "menu":       // press the menu button (left controller)
                    if (menuButton) menuButton.localPosition = menuRest + Vector3.down * .0015f * pulse;
                    if (faceButton) faceButton.localPosition = faceRest + Vector3.down * .0015f * pulse;
                    lit = side < 0 ? 3 : -1; if (lit >= 0 && !menuButton) lit = 4; break;
            }
            for (int i = 0; i < parts.Length; i++)
            {
                if (!parts[i]) continue;
                bool on = glow && pulse > .35f && (i == lit || (lesson == "menu" && side < 0 && i == 4));
                if (on == litNow[i]) continue;
                parts[i].sharedMaterials = on ? System.Array.ConvertAll(own[i], _ => glow) : own[i];
                litNow[i] = on;
            }
        }
        readonly bool[] litNow = new bool[5];
    }
}
