using UnityEngine;

namespace StudyEverywhere
{
    // Calypso's form: a soap-bubble orb with water swirling inside (shader StudyEverywhere/Calypso Orb), above her card.
    // It follows her voice: as she talks, the waves inside swell with each syllable, rings run out from the centre, the
    // shell quivers and droplets spin off it; as the person speaks, rings come in from their side and droplets drift in
    // toward her, so she is seen to be listening. While she thinks, the water turns a little faster. CalypsoCompanion
    // tells it what is heard (Heard, Addressed) and whether she is thinking; her speech is read off her voice's AudioSource.
    public sealed class CalypsoOrb : MonoBehaviour
    {
        public Renderer body;
        public ParticleSystem droplets;
        [Tooltip("Calypso's text-to-speech AudioSource.")] public AudioSource speech;
        [Tooltip("Orb radius in metres (the droplets start and end on it).")] public float radius = .06f;
        [Tooltip("Microphone level below which the room is taken as quiet (Voice SDK levels run 0 to 1).")] public float quiet = .06f;
        [HideInInspector] public bool thinking;

        static readonly int FlowId = Shader.PropertyToID("_Flow"), SpeakId = Shader.PropertyToID("_Speak"), ListenId = Shader.PropertyToID("_Listen"), ThinkId = Shader.PropertyToID("_Think");
        readonly float[] samples = new float[256];
        Material mat;
        float flow, speak, listen, think, mic, addressedUntil = -1, emitDebt;

        void Awake() { if (body) mat = body.material; }
        void OnDestroy() { if (mat) Destroy(mat); }

        // The microphone's level (any speech in the room, a gentle response).
        public void Heard(float level) => mic = Mathf.Max(mic, Mathf.Clamp01((level - quiet) / .3f));
        // The person is talking to Calypso: she turns to them fully for a moment.
        public void Addressed() => addressedUntil = Time.time + .6f;

        void Update()
        {
            float dt = Time.deltaTime;
            float loud = 0;
            if (speech && speech.isPlaying)
            {
                speech.GetOutputData(samples, 0); float sum = 0;
                foreach (var v in samples) sum += v * v;
                loud = Mathf.Clamp01(Mathf.Sqrt(sum / samples.Length) * 9);
            }
            // quick to rise with a syllable, slower to settle
            speak = Mathf.Lerp(speak, loud, 1 - Mathf.Exp(-dt * (loud > speak ? 22 : 6)));
            float hearing = Mathf.Max(mic * .55f, Time.time < addressedUntil ? 1 : 0);
            if (loud > .02f) hearing = 0;   // never "listens" to herself
            listen = Mathf.Lerp(listen, hearing, 1 - Mathf.Exp(-dt * (hearing > listen ? 10 : 2.5f)));
            mic = Mathf.MoveTowards(mic, 0, dt * 2.5f);
            think = Mathf.MoveTowards(think, thinking ? 1 : 0, dt * 1.5f);
            flow += dt * (1 + 2.2f * speak + .9f * think + .6f * listen);

            if (mat)
            {
                mat.SetFloat(FlowId, flow); mat.SetFloat(SpeakId, speak);
                mat.SetFloat(ListenId, listen); mat.SetFloat(ThinkId, think);
            }
            Emit(dt);
        }

        // Droplets: spun off the shell as she talks, drifting in to her from the person's side as they speak.
        void Emit(float dt)
        {
            if (!droplets) return;
            float rate = speak * 70 + listen * 40;
            emitDebt += rate * dt;
            var cam = Camera.main;
            var toUser = cam ? transform.InverseTransformDirection(cam.transform.position - transform.position).normalized : Vector3.back;
            float r = radius / Mathf.Max(1e-4f, transform.lossyScale.x);
            while (emitDebt >= 1)
            {
                emitDebt -= 1;
                bool outward = Random.value * (speak + listen) < speak;
                var e = new ParticleSystem.EmitParams();
                if (outward)
                {
                    var d = Random.onUnitSphere;
                    var spin = Vector3.Cross(Vector3.up, d) * Random.Range(.5f, 1.2f);
                    e.position = d * r * 1.02f;
                    e.velocity = (d * Random.Range(.6f, 1.4f) + spin) * r * 1.6f;
                    e.startLifetime = Random.Range(.6f, 1.1f);
                    e.startColor = Color.Lerp(new Color(.62f, .82f, 1f, .9f), new Color(.98f, .97f, .92f, .9f), Random.value);
                }
                else
                {
                    // from a cap on the person's side, a little out, in toward the centre
                    var d = (toUser + Random.insideUnitSphere * .75f).normalized;
                    e.position = d * r * Random.Range(1.6f, 2.1f);
                    e.startLifetime = Random.Range(.7f, 1f);
                    e.velocity = -d * r * .75f / e.startLifetime;
                    e.startColor = new Color(.8f, .92f, 1f, .75f);
                }
                e.startSize = radius * Random.Range(.05f, .11f);
                droplets.Emit(e, 1);
            }
        }
    }
}
