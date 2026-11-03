using System.Collections;
using UnityEngine;

namespace StudyEverywhere
{
    // Calypso's guide: her own droplets, used to lead the eye without labels or arrows. It uses a copy of her droplets'
    // particle system, in world space, so it looks like water leaving her.
    //  - Point: after she changes something the person might miss (the hatchlings), an arc of droplets runs from her orb
    //    to it, then rings ripple out on the spot (the "beacon").
    //  - Pulse: rings on a spot only, for a moment (the nest glowing as she names "hatch the turtles").
    //  - Thread: a quick straight run of droplets from one point to another (the light thread toward what she is
    //    changing while she acts, and the trail each spoken cue flows out along).
    // CalypsoCompanion adds this at start if the scene has none.
    public sealed class CalypsoGuide : MonoBehaviour
    {
        [Tooltip("Her orb (paths start here).")] public Transform from;
        [Tooltip("Her droplets: copied for the guide, in world space.")] public ParticleSystem template;
        public Color tint = new Color(1f, .93f, .78f, .95f);
        [Tooltip("Seconds the arc takes to reach the spot.")] public float draw = 1.4f;

        ParticleSystem fx;
        Coroutine pointing;

        public void Point(Vector3 target, float seconds = 6)
        {
            if (!from || !Fx()) return;
            if (pointing != null) StopCoroutine(pointing);
            pointing = StartCoroutine(Arc(target, seconds));
        }

        public void Pulse(Vector3 target, float seconds = 1.8f) { if (Fx()) StartCoroutine(Rings(target, seconds)); }

        public void Thread(Vector3 a, Vector3 b, float seconds, Color colour) { if (Fx()) StartCoroutine(Line(a, b, seconds, colour)); }

        ParticleSystem Fx()
        {
            if (fx) return fx;
            if (!template) return null;
            fx = Instantiate(template);   // at the scene root
            fx.name = "Calypso's guide | droplets leading the eye";
            fx.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity); fx.transform.localScale = Vector3.one;
            var main = fx.main; main.simulationSpace = ParticleSystemSimulationSpace.World; main.scalingMode = ParticleSystemScalingMode.Local; main.maxParticles = 800;
            var drag = fx.limitVelocityOverLifetime; drag.enabled = false;
            fx.Play();
            return fx;
        }

        IEnumerator Arc(Vector3 target, float seconds)
        {
            float t0 = Time.time, debt = 0, nextRing = 0;
            while (Time.time - t0 < seconds)
            {
                float k = Mathf.Clamp01((Time.time - t0) / draw);
                Vector3 a = from.position, b = target;
                var mid = Vector3.Lerp(a, b, .5f) + Vector3.up * (.2f + .08f * Vector3.Distance(a, b));
                // the path: droplets laid along the arc as far as it has reached, each lingering a moment
                debt += Time.deltaTime * 110;
                while (debt >= 1)
                {
                    debt -= 1; float u = Random.value * k;
                    Emit(Bezier(a, mid, b, u) + Random.insideUnitSphere * .01f, Vector3.up * .02f, Mathf.Lerp(.01f, .025f, u), 1.1f, tint);
                }
                if (k >= 1 && Time.time >= nextRing) { nextRing = Time.time + .9f; Ring(b); }
                yield return null;
            }
            pointing = null;
        }

        IEnumerator Rings(Vector3 target, float seconds)
        {
            float t0 = Time.time;
            while (Time.time - t0 < seconds) { Ring(target); yield return new WaitForSeconds(.6f); }
        }

        IEnumerator Line(Vector3 a, Vector3 b, float seconds, Color colour)
        {
            float t0 = Time.time, debt = 0, reach = Mathf.Min(.8f, seconds * .5f);
            float size = Mathf.Clamp(Vector3.Distance(a, b) * .02f, .004f, .02f);
            while (Time.time - t0 < seconds)
            {
                float k = Mathf.Clamp01((Time.time - t0) / reach);
                debt += Time.deltaTime * 70;
                while (debt >= 1)
                {
                    debt -= 1; float u = Random.value * k;
                    Emit(Vector3.Lerp(a, b, u) + Random.insideUnitSphere * size * .4f, (b - a).normalized * .03f, size * Random.Range(.7f, 1.2f), .7f, colour);
                }
                yield return null;
            }
        }

        void Ring(Vector3 at)
        {
            for (int i = 0; i < 18; i++)
            {
                float ang = i / 18f * Mathf.PI * 2; var d = new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang));
                Emit(at + Vector3.up * .03f + d * .06f, d * .25f, .03f, 1.2f, tint);
            }
        }

        void Emit(Vector3 at, Vector3 velocity, float size, float life, Color colour)
        {
            var e = new ParticleSystem.EmitParams { position = at, velocity = velocity, startSize = size, startLifetime = life, startColor = colour };
            fx.Emit(e, 1);
        }

        static Vector3 Bezier(Vector3 a, Vector3 m, Vector3 b, float u) => Vector3.Lerp(Vector3.Lerp(a, m, u), Vector3.Lerp(m, b, u), u);
    }
}
