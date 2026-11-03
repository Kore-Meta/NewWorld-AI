using UnityEngine;

namespace StudyEverywhere
{
    // Keeps the coffee level in the world while the cup is carried or tilted, and lets it slosh: the cup's acceleration
    // tips the surface on a damped spring. The shader keeps it level at the fill height by itself; this sends the tilt
    // (StudyEverywhere/Coffee _PlaneNormal) through a property block.
    // Also lets a few thin wisps of steam rise, steadily, while the cup stands upright.
    public sealed class CoffeeLiquid : MonoBehaviour
    {
        public Renderer liquid;
        public float spring = 60f, damping = 5f, slosh = 0.035f, maxTilt = 0.3f;
        [Header("Steam")]
        public ParticleSystem steam;
        [Tooltip("Wisps per second while upright.")]
        public float steamRate = 2.2f;

        static readonly int PlaneNormal = Shader.PropertyToID("_PlaneNormal");
        MaterialPropertyBlock block;
        Vector3 lastPosition, lastVelocity;
        Vector2 tilt, tiltVelocity;
        float steamLevel = -1;

        void OnEnable()
        {
            block ??= new MaterialPropertyBlock();
            lastPosition = transform.position;
            lastVelocity = Vector3.zero;
            steamLevel = -1;
        }

        void LateUpdate()
        {
            float dt = Time.deltaTime;
            if (dt > 0)
            {
                var velocity = (transform.position - lastPosition) / dt;
                var accel = (velocity - lastVelocity) / dt;
                lastPosition = transform.position;
                lastVelocity = velocity;
                // The surface lags behind the cup: pushed against its acceleration, pulled back level by the spring.
                tiltVelocity += (new Vector2(-accel.x, -accel.z) * slosh - tilt * spring) * dt;
                tiltVelocity *= Mathf.Exp(-damping * dt);
                tilt = Vector2.ClampMagnitude(tilt + tiltVelocity * dt, maxTilt);
            }
            block.SetVector(PlaneNormal, new Vector3(tilt.x, 1, tilt.y).normalized);
            liquid.SetPropertyBlock(block);
            UpdateSteam(dt);
        }

        // Steady, faint wisps; they fade out (rather than stop dead) when the cup is tipped or lifted at an angle.
        void UpdateSteam(float dt)
        {
            if (!steam) return;
            float target = Vector3.Dot(transform.up, Vector3.up) > 0.85f ? 1 : 0;
            float level = steamLevel < 0 ? target : Mathf.MoveTowards(steamLevel, target, dt * 2);
            if (Mathf.Approximately(level, steamLevel)) return;
            steamLevel = level;
            var emission = steam.emission;
            emission.rateOverTime = steamRate * level;
        }
    }
}
