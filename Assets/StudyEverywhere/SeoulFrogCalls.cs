using UnityEngine;

namespace StudyEverywhere
{
    // Now and then, a tree frog (청개구리, Dryophytes japonicus) in the garden calls: a short run of its quick
    // "kkaek-kkaek" notes, sometimes a second or third run after a breath, then a long silence. One frog is never two:
    // a bout keeps one caller, one place and one pitch, and each clip waits for the last to finish, so it reads as a
    // single male, never a paddy chorus. The sound comes from one of the frogs (whose throat puffs while it calls) or,
    // without frogs, from one of the garden points, through a single 3D source that fades out within a few metres, so
    // it is heard in the courtyard and not across the café. It follows the nature volume every other place uses
    // (StudyEverywhere.NatureVolume, set from the wrist menu): at zero it stays silent. A little busier after dark when
    // a TimeOfDay is given. With no clips it does nothing and says so once: only the real species, never a stand-in.
    public sealed class SeoulFrogCalls : MonoBehaviour
    {
        [Tooltip("Recordings of Dryophytes japonicus only: whole runs of notes, or single notes (then raise maxPerBout and shorten boutPause). Empty: no calls, one warning.")] public AudioClip[] calls;
        [Tooltip("The frogs to call from (preferred). Sitting frogs only; the caller's throat puffs.")] public SeoulFrogs frogs;
        [Tooltip("Garden points to call from when there are no frogs (or none is sitting).")] public Transform[] points;
        [Tooltip("Optional: a preset 3D source (mixer group etc.). Made here when empty.")] public AudioSource source;
        [Tooltip("Optional: calls come a little more often towards night.")] public TimeOfDay time;

        [Tooltip("Seconds of silence between bouts (random in this range).")] public Vector2 interval = new Vector2(70, 240);
        [Tooltip("Before the nature volume.")] [Range(0, 1)] public float volume = .25f;
        [Tooltip("Each bout's pitch is a random multiple in this range (kept for the whole bout: one frog, one voice).")] public Vector2 pitchJitter = new Vector2(.97f, 1.03f);
        [Tooltip("Chance of one more clip in a bout, asked again after each.")] [Range(0, 1)] public float repeatChance = .35f;
        [Tooltip("Most clips in one bout.")] [Min(1)] public int maxPerBout = 3;
        [Tooltip("Seconds between the end of one clip and the start of the next within a bout.")] public Vector2 boutPause = new Vector2(.8f, 2.5f);
        [Tooltip("At night the gaps shrink to this share.")] [Range(.2f, 1)] public float nightGap = .6f;
        [Tooltip("Heard fully within this distance (metres).")] public float minDistance = .6f;
        [Tooltip("Silent beyond this distance (metres): keep it inside the courtyard.")] public float maxDistance = 9;

        const string VolumeKey = "StudyEverywhere.NatureVolume";
        const float DefaultNature = .7f;   // as RestAmbience and EvercrestAmbience
        float next, pitch, level; int left, caller = -1, point = -1;

        void Start()
        {
            if (calls == null || calls.Length == 0) { Debug.LogWarning("SeoulFrogCalls: no frog call clips assigned, so the garden stays silent. Add licensed Dryophytes japonicus recordings.", this); enabled = false; return; }
            if (!frogs && (points == null || points.Length == 0)) { Debug.LogWarning("SeoulFrogCalls: no frogs and no garden points to call from.", this); enabled = false; return; }
            if (!source)
            {
                var go = new GameObject("Frog call"); go.transform.SetParent(transform, false);
                source = go.AddComponent<AudioSource>();
            }
            source.playOnAwake = false; source.loop = false;
            source.spatialBlend = 1; source.dopplerLevel = 0; source.spread = 0;
            // logarithmic, but cut to nothing at maxDistance rather than lingering at a whisper across the room
            source.rolloffMode = AudioRolloffMode.Custom; source.minDistance = minDistance; source.maxDistance = maxDistance;
            source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, Rolloff(minDistance / maxDistance));
            next = Time.time + Random.Range(interval.x * .3f, interval.y);
        }

        void Update()
        {
            float now = Time.time;
            if (now < next) return;
            float nature = PlayerPrefs.GetFloat(VolumeKey, DefaultNature);
            if (left <= 0)
            {
                // a new bout: how many clips, which frog, its voice and how near it sounds
                left = 1; while (left < maxPerBout && Random.value < repeatChance) left++;
                caller = point = -1;
                pitch = Random.Range(pitchJitter.x, pitchJitter.y); level = Random.Range(.75f, 1f);
            }
            float length = 0;
            if (nature > 0 && Place())
            {
                var clip = calls[Random.Range(0, calls.Length)];
                if (clip)
                {
                    source.pitch = pitch; length = clip.length / pitch;
                    source.PlayOneShot(clip, volume * nature * level);
                    if (caller >= 0) frogs.Calling(caller, length);
                }
            }
            // within a bout, the next run follows once this one is done; after the last, a long silence
            if (--left > 0) next = now + length + Random.Range(boutPause.x, boutPause.y);
            else { float g = Random.Range(interval.x, interval.y); if (time) g *= Mathf.Lerp(1, nightGap, Mathf.Clamp01(time.Evening)); next = now + length + g; }
        }

        // Puts the source on the caller, the same one for the whole bout: a sitting frog, else a garden point.
        bool Place()
        {
            if (caller < 0 && point < 0 && frogs && frogs.Count > 0)
            {
                int start = Random.Range(0, frogs.Count);
                for (int k = 0; k < frogs.Count; k++) { int i = (start + k) % frogs.Count; if (frogs.Root(i) && frogs.Sitting(i)) { caller = i; break; } }
            }
            if (caller >= 0) { source.transform.position = frogs.Root(caller).position + Vector3.up * .03f; return true; }
            if (points == null || points.Length == 0) return false;
            if (point < 0) point = Random.Range(0, points.Length);
            var p = points[point]; if (!p) return false;
            source.transform.position = p.position; return true;
        }

        // 1/distance from the near edge, eased to zero at the far edge (distance as a share of maxDistance).
        static AnimationCurve Rolloff(float near)
        {
            near = Mathf.Clamp(near, .01f, .9f);
            var c = new AnimationCurve();
            for (int i = 0; i <= 8; i++)
            {
                float x = Mathf.Lerp(near, 1, i / 8f);
                float v = near / x * (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.6f, 1, x)));
                c.AddKey(x, v);
            }
            c.AddKey(0, 1);
            return c;
        }
    }
}
