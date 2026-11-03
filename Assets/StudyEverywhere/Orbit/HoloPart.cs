using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace StudyEverywhere
{
    // One part of a hologram on the deck. It floats at its place in the assembled craft or, in the parts view, at its
    // spread position with a label on a leader line. Pinch or grab it (hand or controller) and it comes along, lit
    // brighter, and its card shows what it is; let go and it drifts back to where it belongs. Hovering alone focuses it.
    [RequireComponent(typeof(XRGrabInteractable))]
    public sealed class HoloPart : MonoBehaviour
    {
        public HoloDeck deck;
        public string id;
        [Tooltip("Where the part sits in the assembled craft (local to the craft root).")] public Vector3 home;
        [Tooltip("Where it floats in the parts view (local to the craft root).")] public Vector3 spread;
        public Renderer[] renderers;
        [Tooltip("The part in its real finish (white paint, foil, solar cells...), shown in the deck's Realistic mode.")] public Material real;
        public TMP_Text label;
        [Tooltip("The label's canvas (shown in the parts view and placed on its leader line).")] public GameObject labelRoot;
        public LineRenderer leader;
        [Tooltip("No longer used for placing: the deck sets every label beside the model (HoloDeck.LayoutLabels). Kept for old scenes.")] public Vector3 labelOffset = new Vector3(0, .06f, 0);
        public float returnSeconds = .45f;

        XRGrabInteractable grab;
        Vector3 velocity;
        Vector3 centreLocal;   // the renderers' centre in this part's own frame
        bool held, hot;

        public bool Held => held;
        public Vector3 Target => deck && deck.Exploded ? spread : home;

        void Awake()
        {
            grab = GetComponent<XRGrabInteractable>();
            var b = new Bounds(transform.position, Vector3.zero); bool any = false;
            foreach (var r in renderers) if (r) { if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds); }
            centreLocal = any ? transform.InverseTransformPoint(b.center) : Vector3.zero;
        }

        void OnEnable()
        {
            grab.selectEntered.AddListener(OnGrab); grab.selectExited.AddListener(OnRelease);
            grab.hoverEntered.AddListener(OnHover);
        }
        void OnDisable()
        {
            grab.selectEntered.RemoveListener(OnGrab); grab.selectExited.RemoveListener(OnRelease);
            grab.hoverEntered.RemoveListener(OnHover);
            held = false;
        }

        void OnGrab(SelectEnterEventArgs _) { held = true; if (deck) deck.Focus(this, true); }
        void OnRelease(SelectExitEventArgs _) { held = false; velocity = Vector3.zero; }
        void OnHover(HoverEnterEventArgs _) { if (deck) deck.Focus(this, false); }

        public void SetHot(bool on)
        {
            if (hot == on || !deck) return;
            hot = on;
            Skin();
        }

        // Hologram (cyan, brighter when held) or the real finish, as the deck shows.
        public void Skin()
        {
            if (!deck) return;
            foreach (var r in renderers) deck.Dress(r, real, hot, deck.Realistic);
        }

        // The part's middle in the world now (its renderers' bounds; also right in the editor, where Awake has not run).
        public Vector3 Centre
        {
            get
            {
                if (Application.isPlaying && renderers != null && renderers.Length == 1 && renderers[0]) return transform.TransformPoint(centreLocal);
                var b = new Bounds(transform.position, Vector3.zero); bool any = false;
                if (renderers != null) foreach (var r in renderers) if (r) { if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds); }
                return b.center;
            }
        }
        // The deck lays the labels out together (HoloDeck.LayoutLabels), so they clear the model and one another; this
        // is the state it keeps for this part's label: which side of the model it is on, and its glide.
        [System.NonSerialized] public int labelSide;
        [System.NonSerialized] public bool labelPlaced;
        [System.NonSerialized] public Vector3 labelVelocity;

        void LateUpdate()
        {
            if (!deck) return;
            if (!held)
            {
                transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Target, ref velocity, returnSeconds);
                transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, Time.deltaTime * 6);
            }
        }
    }
}
