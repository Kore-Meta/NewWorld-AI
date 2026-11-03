using UnityEngine;

namespace StudyEverywhere
{
    // One reading light that goes where the person sits: when they take a seat (a chair's pop-up, or a place from the
    // menu) near one of the lamps listed here, this light moves into that lamp, so every seat's lamp really lights its
    // desk without paying for a light at every lamp. Put the light in TimeOfDay's lamps, so the menu's Lights switch
    // and the clock turn it on and off with the others.
    public sealed class SeatLamp : MonoBehaviour
    {
        public PlayerPlacement placement;
        public Light lamp;
        [Tooltip("Where the light sits for each lamp (inside its shade).")] public Transform[] spots;
        [Tooltip("A lamp lights the seat whose eye is within this distance (metres).")] public float reach = 1.6f;

        void OnEnable() { if (placement) placement.Changed += Move; Move(); }
        void OnDisable() { if (placement) placement.Changed -= Move; }

        void Move()
        {
            if (!lamp || spots == null || !placement || !placement.origin || !placement.origin.Camera) return;
            var eye = placement.origin.Camera.transform.position;
            Transform best = null; float d = reach;
            foreach (var s in spots) if (s) { float x = Vector3.Distance(s.position, eye); if (x < d) { d = x; best = s; } }
            lamp.gameObject.SetActive(best);
            if (best) lamp.transform.position = best.position;
        }

        // A seat may be taken by walking up or teleporting too: look again every second.
        float next;
        void Update() { if (Time.time >= next) { next = Time.time + 1; Move(); } }
    }
}
