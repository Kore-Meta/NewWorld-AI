using UnityEngine;

namespace StudyEverywhere
{
    // A few birds far off over the water or the garden: they wheel in a loose flock round a wide circle, flapping and
    // gliding by turns, now and then crossing over and coming back. Each bird is a body with two wing children hinged
    // at the shoulders.
    public sealed class Birds : MonoBehaviour
    {
        [System.Serializable] public class Bird { public Transform body, leftWing, rightWing; }
        public Bird[] birds;
        public Vector3 centre = new Vector3(-40, 22, 0);
        public float radius = 35, speed = 7, spread = 6;
        float[] phase;

        void Start() { phase = new float[birds.Length]; for (int i = 0; i < phase.Length; i++) phase[i] = Random.value * 100; }

        void Update()
        {
            for (int i = 0; i < birds.Length; i++)
            {
                var b = birds[i]; if (!b.body) continue;
                float t = Time.time + phase[i] * .05f, ang = t * speed / radius + i * .35f;
                // a wobbling circle, each bird off the line a little, rising and falling
                var p = centre + new Vector3(Mathf.Cos(ang) * radius, Mathf.Sin(t * .23f + i) * 3, Mathf.Sin(ang) * radius * .7f)
                    + new Vector3(Mathf.Sin(t * .31f + i * 2) * spread, 0, Mathf.Cos(t * .27f + i * 3) * spread);
                var dir = p - b.body.position;
                b.body.position = p;
                if (dir.sqrMagnitude > 1e-5f) b.body.rotation = Quaternion.Slerp(b.body.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 4);
                // flap for a few seconds, glide for a few
                float beat = Mathf.Sin(phase[i] + t * .4f) > -.2f ? Mathf.Sin(t * 9 + phase[i]) * 40 : 8;
                if (b.leftWing && !b.rightWing) b.leftWing.localScale = new Vector3(Mathf.Max(.35f, Mathf.Cos(beat * Mathf.Deg2Rad)), 1, 1);   // one mesh: the wings fold
                else { if (b.leftWing) b.leftWing.localRotation = Quaternion.Euler(0, 0, beat); if (b.rightWing) b.rightWing.localRotation = Quaternion.Euler(0, 0, -beat); }
            }
        }
    }
}
