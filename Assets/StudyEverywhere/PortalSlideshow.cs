using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StudyEverywhere
{
    // A lobby portal's view into its kind of place: pictures of each place there is (the libraries, the café, the rest
    // area) fading one into the next, the place's name across the middle.
    public sealed class PortalSlideshow : MonoBehaviour
    {
        public RawImage front, back;
        public TMP_Text title;
        public Texture2D[] pictures;
        public string[] names;
        public float hold = 5, fade = 1.2f;

        int index; float clock;

        void OnEnable() { index = 0; clock = 0; Show(0, 1); }

        void Update()
        {
            if (pictures == null || pictures.Length < 2) return;
            clock += Time.deltaTime;
            if (clock < hold) return;
            float k = Mathf.Clamp01((clock - hold) / fade);
            int next = (index + 1) % pictures.Length;
            Show(next, k);
            if (k >= 1) { index = next; clock = 0; }
        }

        // the current picture behind, the next fading in over it; the name follows the picture more than half shown
        void Show(int next, float k)
        {
            if (pictures == null || pictures.Length == 0) return;
            if (back) back.texture = pictures[index];
            if (front) { front.texture = pictures[next]; var c = front.color; c.a = k; front.color = c; }
            if (title && names != null && names.Length > 0) title.text = names[k > .5f ? next : index];
        }
    }
}
