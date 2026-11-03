using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace StudyEverywhere
{
    // The picture of one shared display: letterboxed to its aspect in an ink well, with a live indicator.
    // A stalled display is greyed under a "Picture paused" overlay and its indicator reads PAUSED, so it never looks live.
    [Serializable]
    public sealed class DisplayView
    {
        public RawImage picture;
        public AspectRatioFitter fit;
        public GameObject stalled;
        public Image liveDot;
        public TMP_Text liveLabel;

        public void Show(Texture texture)
        {
            picture.texture = texture;
            if (texture && texture.height > 0) fit.aspectRatio = texture.width / (float)texture.height;
        }

        public void Clear() => picture.texture = null;

        public void Mark(bool live)
        {
            stalled.SetActive(!live);
            picture.color = live ? Color.white : new Color(.45f, .45f, .45f);
            liveDot.color = live ? UIStyle.Live : UIStyle.Brass;
            liveLabel.text = live ? "LIVE" : "PAUSED";
        }

        public void Pulse(float time)
        {
            var c = UIStyle.Live;
            c.a = 0.55f + 0.45f * Mathf.Cos(time * 3);
            liveDot.color = c;
        }
    }

    // A floating panel for one additional shared display (the main display stays on the workspace panel).
    // WorkspaceConnection clones it from a builder-made template per display and places it like the physical monitor.
    public sealed class DisplayPanel : MonoBehaviour
    {
        public DisplayView view;
        public TMP_Text title, detail;
        public Button stop;
    }
}
