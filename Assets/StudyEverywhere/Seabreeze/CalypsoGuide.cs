using System.Collections;
using UnityEngine;

namespace StudyEverywhere
{
    // Calypso's guide: after she changes something the person might miss (the hatchlings coming out of the nest behind
    // them), a path of her own droplets runs from her orb over to it, then rings of droplets ripple out on the spot, so
    // the eye follows her without a label or an arrow. It uses a copy of her droplets' particle system, so it looks like
    // water leaving her. CalypsoCompanion adds this at start if the scene has none.
    public sealed class CalypsoGuide : MonoBehaviour
    {
        [Tooltip("Her orb (the path starts here).")] public Transform from;
        [Tooltip("Her droplets: copied for the path, in world space.")] public ParticleSystem template;
        public Color tint = new Color(1f, .93f, .78f, .95f);
        [Tooltip("Seconds the path takes to reach the spot.")] public float draw = 1.4f;

        ParticleSystem fx;
        Coroutine running;

        public void Point(Vector3 target, float seconds = 5)
        {
            if (!from || !template) return;
            if (running != null) StopCoroutine(running);
            running = StartCoroutine(Run(target, seconds));
        }

        ParticleSystem Fx()
        {
            if (fx) return fx;
            fx = Instantiate(template, null);
            fx.name = "Calypso's guide | droplets leading the eye";
            fx.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity); fx.transform.localScale = Vector3.one;
            var main = fx.main; main.simulationSpace = ParticleSystemSimulationSpace.World; main.scalingMode = ParticleSystemScalingMode.Local; main.maxParticles = 600;
            var drag = fx.limitVelocityOverLifetime; drag.enabled = false;
            fx.Play();
            return fx;
        }

        IEnumerator Run(Vector3 target, float seconds)
        {
            var ps = Fx();
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
                    Emit(ps, Bezier(a, mid, b, u) + Random.insideUnitSphere * .01f, Vector3.up * .02f, Mathf.Lerp(.01f, .025f, u), 1.1f);
                }
                // on the spot: a ring of droplets running out across the sand, again and again
                if (k >= 1 && Time.time >= nextRing)
                {
                    nextRing = Time.time + .9f;
                    for (int i = 0; i < 18; i++)
                    {
                        float ang = i / 18f * Mathf.PI * 2; var d = new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang));
                        Emit(ps, b + Vector3.up * .03f + d * .06f, d * .25f, .03f, 1.2f);
                    }
                }
                yield return null;
            }
            running = null;
        }

        void Emit(ParticleSystem ps, Vector3 at, Vector3 velocity, float size, float life)
        {
            var e = new ParticleSystem.EmitParams { position = at, velocity = velocity, startSize = size, startLifetime = life, startColor = tint };
            ps.Emit(e, 1);
        }

        static Vector3 Bezier(Vector3 a, Vector3 m, Vector3 b, float u) => Vector3.Lerp(Vector3.Lerp(a, m, u), Vector3.Lerp(m, b, u), u);
    }
}
