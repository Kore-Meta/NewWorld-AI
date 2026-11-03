using UnityEngine;

namespace StudyEverywhere
{
    // Calypso's form: a soap-bubble orb with water swirling inside (shader StudyEverywhere/Calypso Orb), above her card.
    // It follows her voice: as she talks, the waves inside swell with each syllable, rings run out from the centre, the
    // shell quivers and droplets spin off it; as the person speaks, rings come in from their side and droplets drift in
    // toward her, so she is seen to be listening. While she thinks, the water turns a little faster. CalypsoCompanion
    // tells it what is heard (Heard, Addressed) and whether she is thinking; her speech is read off her voice's AudioSource.
    //
    // State language (Nov 3 design review): one colour and one motion per state, so the orb alone says what she is doing.
    //   Idle       soft blue, slow breathing (the shader's own)        Speaking   blue, pulses with each word
    //   Listening  aqua, leans in, rings come in from the person         Thinking   violet, the water turns faster
    //   Acting     amber, the water races while the beach changes        Unsure     grey, dims and tilts like a head tilt
    // CalypsoCompanion sets `mood` (Idle, Thinking or Acting) and calls Unsure(); Listening and Speaking come from what the
    // orb hears, so they always win over the mood she was told.
    public sealed class CalypsoOrb : MonoBehaviour
    {
        public enum Mood { Idle, Listening, Thinking, Speaking, Acting, Unsure }

        public Renderer body;
        public ParticleSystem droplets;
        [Tooltip("Calypso's text-to-speech AudioSource.")] public AudioSource speech;
        [Tooltip("Orb radius in metres (the droplets start and end on it).")] public float radius = .06f;
        [Tooltip("Microphone level below which the room is taken as quiet (Voice SDK levels run 0 to 1).")] public float quiet = .06f;
        [HideInInspector] public bool thinking;
        [HideInInspector] public Mood mood;

        [Header("State colours (deep water, shallow water); idle and speaking keep the material's own")]
        public Color listenDeep = new Color(.04f, .6f, .58f), listenShallow = new Color(.55f, .96f, .9f);
        public Color thinkDeep = new Color(.45f, .27f, .95f), thinkShallow = new Color(.76f, .66f, 1f);
        public Color actDeep = new Color(.95f, .48f, .14f), actShallow = new Color(1f, .82f, .52f);
        public Color unsureDeep = new Color(.42f, .47f, .56f), unsureShallow = new Color(.74f, .77f, .82f);

        // What the orb is showing now, after its own signals are taken into account.
        public Mood Shown { get; private set; }

        static readonly int FlowId = Shader.PropertyToID("_Flow"), SpeakId = Shader.PropertyToID("_Speak"), ListenId = Shader.PropertyToID("_Listen"), ThinkId = Shader.PropertyToID("_Think");
        static readonly int DeepId = Shader.PropertyToID("_Deep"), ShallowId = Shader.PropertyToID("_Shallow"), ClarityId = Shader.PropertyToID("_Clarity");
        readonly float[] samples = new float[256];
        Material mat;
        float flow, speak, listen, think, act, unsure, mic, addressedUntil = -1, unsureUntil = -1, emitDebt, clarity;
        Color idleDeep, idleShallow, deepNow, shallowNow;
        Quaternion rest = Quaternion.identity;

        void Awake()
        {
            if (body) { mat = body.material; rest = body.transform.localRotation; }
            if (mat)
            {
                idleDeep = deepNow = mat.GetColor(DeepId); idleShallow = shallowNow = mat.GetColor(ShallowId);
                clarity = mat.GetFloat(ClarityId);
            }
        }
        void OnDestroy() { if (mat) Destroy(mat); }

        // The microphone's level (any speech in the room, a gentle response).
        public void Heard(float level) => mic = Mathf.Max(mic, Mathf.Clamp01((level - quiet) / .3f));
        // The person is talking to Calypso: she turns to them fully for a moment.
        public void Addressed() => addressedUntil = Time.time + .6f;
        // She didn't catch what was said: a short head tilt in grey.
        public void Unsure() => unsureUntil = Time.time + 1.8f;

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
            bool isUnsure = Time.time < unsureUntil;
            Shown = isUnsure ? Mood.Unsure
                : speak > .05f ? Mood.Speaking
                : mood == Mood.Acting ? Mood.Acting
                : listen > .35f ? Mood.Listening
                : thinking ? Mood.Thinking
                : mood;
            think = Mathf.MoveTowards(think, Shown == Mood.Thinking ? 1 : 0, dt * 1.5f);
            act = Mathf.MoveTowards(act, Shown == Mood.Acting ? 1 : 0, dt * 2);
            unsure = Mathf.MoveTowards(unsure, isUnsure ? 1 : 0, dt * 3);
            flow += dt * (1 + 2.2f * speak + .9f * think + .6f * listen + 1.6f * act);

            if (mat)
            {
                mat.SetFloat(FlowId, flow); mat.SetFloat(SpeakId, speak);
                mat.SetFloat(ListenId, listen); mat.SetFloat(ThinkId, think);
                Tint(dt);
            }
            // the head tilt: a slow sway of the bubble while she is unsure
            if (body) body.transform.localRotation = rest * Quaternion.Euler(0, 0, unsure * 10 * Mathf.Sin(Time.time * 3.2f));
            Emit(dt);
        }

        // The water takes the state's colour over about a quarter of a second, so changes read as a mood, not a switch.
        void Tint(float dt)
        {
            Color deep = idleDeep, shallow = idleShallow;
            switch (Shown)
            {
                case Mood.Listening: deep = listenDeep; shallow = listenShallow; break;
                case Mood.Thinking: deep = thinkDeep; shallow = thinkShallow; break;
                case Mood.Acting: deep = actDeep; shallow = actShallow; break;
                case Mood.Unsure: deep = unsureDeep; shallow = unsureShallow; break;
            }
            float k = 1 - Mathf.Exp(-dt * 4);
            deepNow = Color.Lerp(deepNow, deep, k); shallowNow = Color.Lerp(shallowNow, shallow, k);
            mat.SetColor(DeepId, deepNow); mat.SetColor(ShallowId, shallowNow);
            mat.SetFloat(ClarityId, Mathf.Lerp(clarity, Mathf.Min(1, clarity + .3f), unsure));   // dimmer while unsure
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
