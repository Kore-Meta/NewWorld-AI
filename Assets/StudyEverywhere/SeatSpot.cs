using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace StudyEverywhere
{
    // A chair you can sit in. Pointing at it with a controller or hand ray (its trigger volume sits on the Seat layer,
    // which only the far rays see) pops a round chair icon up above it; clicking or pinching the icon seats you there:
    // eye at seated height, facing the table. The icon stays while the ray moves from the chair up to it, and the
    // chair you are already sitting in offers nothing but, when it has one, a Stand up button in front of it.
    public sealed class SeatSpot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public PlayerPlacement placement;
        [Tooltip("Seated eye point; its forward faces the table.")]
        public Transform eye;
        public XRSimpleInteractable chair;
        public Collider volume;
        public RectTransform icon;
        public Button sit;
        public Image face, glyph;
        [Header("Stand up (optional)")]
        [Tooltip("Shown in front of the seat while the person sits in it.")]
        public GameObject standGroup;
        public Button stand;
        [Tooltip("Where the person stands after getting up; its forward is the view direction.")]
        public Transform standSpot;
        [Tooltip("Seconds the icon lingers after the ray leaves, so it can be reached.")]
        public float linger = 0.6f;

        float shownFor, hiddenIn, pop;
        bool overIcon;

        void OnEnable()
        {
            if (sit) sit.onClick.AddListener(Sit);
            if (stand) stand.onClick.AddListener(Stand);
            if (standGroup) standGroup.SetActive(false);
            if (icon) icon.gameObject.SetActive(false);
        }

        void OnDisable()
        {
            if (sit) sit.onClick.RemoveListener(Sit);
            if (stand) stand.onClick.RemoveListener(Stand);
            overIcon = false;
        }

        public void OnPointerEnter(PointerEventData _) => overIcon = true;
        public void OnPointerExit(PointerEventData _) => overIcon = false;

        public bool Occupied
        {
            get
            {
                if (!placement || !placement.origin || !placement.origin.Camera) return false;
                if (placement.Seat == eye) return true;
                Vector3 d = placement.origin.Camera.transform.position - eye.position;
                d.y = 0;
                return d.sqrMagnitude < 0.35f * 0.35f;
            }
        }

        void Update()
        {
            bool occupied = Occupied;
            bool seatedHere = placement && placement.Seat == eye;
            if (standGroup && standGroup.activeSelf != seatedHere) standGroup.SetActive(seatedHere);
            if (volume) volume.enabled = !occupied;
            bool pointed = !occupied && ((chair && chair.isHovered) || overIcon);
            if (pointed) hiddenIn = linger;
            else hiddenIn -= Time.deltaTime;
            bool show = hiddenIn > 0 && !occupied;
            if (!icon) return;
            if (show != icon.gameObject.activeSelf) { icon.gameObject.SetActive(show); pop = 0; if (!show) overIcon = false; }
            if (!show) return;

            // Pop in, face the viewer, and swell a little while the ray is on the icon itself.
            pop = Mathf.Min(1, pop + Time.deltaTime / 0.18f);
            swell = Mathf.Lerp(swell, overIcon ? 1.15f : 1, 1 - Mathf.Exp(-Time.deltaTime * 16));
            icon.localScale = Vector3.one * baseScale * swell * (1 - Mathf.Pow(1 - pop, 3));
            var cam = placement && placement.origin ? placement.origin.Camera : Camera.main;
            if (cam) icon.rotation = Quaternion.LookRotation(icon.position - cam.transform.position, Vector3.up);
            if (face) face.color = overIcon ? UIStyle.Sage : UIStyle.WarmIvory;
            if (glyph) glyph.color = overIcon ? UIStyle.WarmIvory : UIStyle.Sage;
        }

        float baseScale, swell = 1;
        void Awake()
        {
            if (icon) baseScale = icon.localScale.x;
            LetRaysSeeSeats(volume ? volume.gameObject.layer : gameObject.layer);
        }

        // The rig's far rays come from the XR prefab, which ignores the Seat layer and passes through triggers; the
        // builder's edits to them are not kept as prefab overrides, so the chair volumes are made visible to them here.
        static void LetRaysSeeSeats(int layer)
        {
            foreach (var caster in FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.Interactors.Casters.CurveInteractionCaster>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                caster.raycastMask |= 1 << layer;
                caster.raycastTriggerInteraction = QueryTriggerInteraction.Collide;
            }
        }

        public void Sit()
        {
            if (!placement || !eye) return;
            placement.SitAt(eye);
            hiddenIn = 0;
            overIcon = false;
            if (icon) icon.gameObject.SetActive(false);
        }

        public void Stand()
        {
            if (placement && standSpot) placement.StandAt(standSpot);
        }
    }
}
