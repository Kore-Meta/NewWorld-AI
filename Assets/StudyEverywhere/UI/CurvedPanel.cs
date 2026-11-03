using UnityEngine;
using UnityEngine.UI;

namespace StudyEverywhere
{
    // A world-space canvas bent gently round the viewer (see UICurve): every Graphic under it gets a UICurve, also
    // ones added later (call Apply after building new controls).
    [ExecuteAlways, DisallowMultipleComponent]
    public sealed class CurvedPanel : MonoBehaviour
    {
        [Tooltip("Radius of the cylinder the panel lies on, in canvas units (larger = flatter). About twice the panel's width reads as a gentle curve.")]
        public float radius = 2200;
        [Tooltip("Images are cut into columns no wider than this (canvas units) so they curve smoothly.")]
        public float segment = 40;

        void OnEnable() => Apply();

        public void Apply()
        {
            foreach (var g in GetComponentsInChildren<Graphic>(true))
            {
                if (!g.TryGetComponent(out UICurve c)) c = g.gameObject.AddComponent<UICurve>();
                c.panel = this;
                g.SetVerticesDirty();
            }
        }
    }
}
