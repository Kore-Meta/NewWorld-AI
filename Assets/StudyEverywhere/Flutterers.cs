using UnityEngine;

namespace StudyEverywhere
{
    // Butterflies and dragonflies outside the windows by day. Butterflies wander in soft loops round the flowers,
    // wings beating slowly with pauses; dragonflies dart, hover, and dart again, wings a blur. Each one is a body
    // with wing children hinged along the body (two for a butterfly, four for a dragonfly).
    public sealed class Flutterers : MonoBehaviour
    {
        public enum Kind { Butterfly, Dragonfly }
        [System.Serializable] public class Flyer { public Kind kind; public Transform body; public Transform[] wings; public Vector3 home; public float range = 1.5f; }
        public Flyer[] flyers;
        float[] seed; Vector3[] target, vel; float[] retarget;

        void Start()
        {
            int n = flyers.Length; seed = new float[n]; target = new Vector3[n]; vel = new Vector3[n]; retarget = new float[n];
            for (int i = 0; i < n; i++) { seed[i] = Random.value * 100; target[i] = flyers[i].home; }
        }

        void Update()
        {
            float dt = Time.deltaTime;
            for (int i = 0; i < flyers.Length; i++)
            {
                var f = flyers[i]; if (!f.body) continue; float t = Time.time + seed[i];
                if (Time.time > retarget[i])
                {
                    target[i] = f.home + new Vector3(Random.Range(-1f, 1f), Random.Range(-.4f, .6f), Random.Range(-1f, 1f)) * f.range;
                    retarget[i] = Time.time + (f.kind == Kind.Dragonfly ? Random.Range(.6f, 2.2f) : Random.Range(1.5f, 4f));
                }
                var to = target[i] - f.body.position;
                if (f.kind == Kind.Dragonfly)
                {
                    // dart: quick acceleration, then a stop and a hover
                    vel[i] = Vector3.Lerp(vel[i], to.magnitude > .08f ? to.normalized * 2.2f : Vector3.zero, dt * 5);
                    f.body.position += vel[i] * dt + Vector3.up * Mathf.Sin(t * 7) * .002f;
                    if (vel[i].sqrMagnitude > .01f) f.body.rotation = Quaternion.Slerp(f.body.rotation, Quaternion.LookRotation(new Vector3(vel[i].x, 0, vel[i].z) + Vector3.forward * 1e-4f), dt * 8);
                    if (f.wings.Length == 1) { if (f.wings[0]) f.wings[0].localScale = new Vector3(.75f + .25f * Mathf.Sin(t * 60), 1, 1); }   // one mesh: a shimmer
                    else for (int w = 0; w < f.wings.Length; w++) if (f.wings[w]) f.wings[w].localRotation = Quaternion.Euler(0, 0, (w % 2 == 0 ? 1 : -1) * (12 + Mathf.Sin(t * 60 + w) * 18));
                }
                else
                {
                    // flutter: wandering, bobbing, slow wingbeats with rests (wings held up)
                    vel[i] = Vector3.Lerp(vel[i], to.normalized * .45f + new Vector3(Mathf.Sin(t * 2.3f), Mathf.Sin(t * 3.1f) * .8f, Mathf.Cos(t * 1.7f)) * .25f, dt * 2);
                    f.body.position += vel[i] * dt;
                    if (vel[i].sqrMagnitude > .001f) f.body.rotation = Quaternion.Slerp(f.body.rotation, Quaternion.LookRotation(new Vector3(vel[i].x, 0, vel[i].z) + Vector3.forward * 1e-4f), dt * 3);
                    float beat = Mathf.Sin(t * .9f) > .7f ? 80 : 45 + Mathf.Sin(t * 14) * 40;
                    if (f.wings.Length == 1) { if (f.wings[0]) f.wings[0].localScale = new Vector3(Mathf.Max(.12f, Mathf.Cos(beat * Mathf.Deg2Rad)), 1, 1); }   // one mesh: wings fold in and open
                    else if (f.wings.Length > 1) { if (f.wings[0]) f.wings[0].localRotation = Quaternion.Euler(0, 0, beat); if (f.wings[1]) f.wings[1].localRotation = Quaternion.Euler(0, 0, -beat); }
                }
            }
        }
    }
}
