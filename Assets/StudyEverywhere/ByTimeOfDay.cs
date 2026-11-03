using UnityEngine;

namespace StudyEverywhere
{
    // Shows things only at some times of day, by the clock's Evening (0 by day, 1 at night): the full moon at night,
    // butterflies and birds by day. Switches whole objects, so their scripts rest when hidden.
    public sealed class ByTimeOfDay : MonoBehaviour
    {
        public TimeOfDay time;
        public GameObject[] targets;
        [Tooltip("Shown while the clock's Evening is between these.")] public float from, to = 1;

        void Update() => Apply();

        // also called by builders and previews, where Update does not run (an editor capture showed the moon by day)
        public void Apply()
        {
            if (!time || targets == null) return;
            bool on = time.Evening >= from && time.Evening <= to;
            foreach (var t in targets) if (t && t.activeSelf != on) t.SetActive(on);
        }
    }
}
