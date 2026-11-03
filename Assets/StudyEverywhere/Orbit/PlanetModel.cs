using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace StudyEverywhere
{
    // A planet on a pillar round the hologram deck. It turns slowly on its own axis. Touch it, point at it, or pinch
    // it (hand or controller) and its card pops up above it with its name and facts (OrbitPlanets); pinch again to keep
    // it open. It folds away a few seconds after the hand or ray moves off, unless kept. Only one card is open at a time.
    [RequireComponent(typeof(XRSimpleInteractable))]
    public sealed class PlanetModel : MonoBehaviour
    {
        public string id;
        [Tooltip("The globe that spins (and the ring, for Saturn).")] public Transform spinner;
        public float secondsPerTurn = 30;
        [Tooltip("The info card above the planet (hidden until touched).")] public GameObject card;
        public TMP_Text title, kind, body, facts;
        public float lingerSeconds = 4, popSeconds = .25f;

        static PlanetModel open;
        XRSimpleInteractable touch;
        float hideAt = -1, shown;
        bool kept, hovered;
        Vector3 cardScale = Vector3.one;

        void Awake()
        {
            touch = GetComponent<XRSimpleInteractable>();
            Fill();
            if (card) { cardScale = card.transform.localScale; card.SetActive(false); card.transform.localScale = Vector3.zero; }
        }
        // The card's words, in the chosen language (again whenever it changes).
        void Fill()
        {
            var p = OrbitPlanets.Find(id);
            if (p == null) return;
            if (title) title.text = Localization.Tr(p.name);
            if (kind) kind.text = Localization.Tr(p.kind).ToUpperInvariant();
            if (body) body.text = Localization.Tr(p.summary);
            if (facts) facts.text = string.Join("\n", p.facts.Select(f => "·  " + Localization.Tr(f)));
        }
        void OnEnable()
        {
            touch.hoverEntered.AddListener(OnHover); touch.hoverExited.AddListener(OnUnhover); touch.selectEntered.AddListener(OnSelect);
            Localization.Changed += Fill; Fill();
        }
        void OnDisable()
        {
            touch.hoverEntered.RemoveListener(OnHover); touch.hoverExited.RemoveListener(OnUnhover); touch.selectEntered.RemoveListener(OnSelect);
            Localization.Changed -= Fill;
        }

        void OnHover(HoverEnterEventArgs _) { hovered = true; Show(); }
        void OnUnhover(HoverExitEventArgs _) { hovered = touch.isHovered; if (!hovered && !kept) hideAt = Time.time + lingerSeconds; }
        void OnSelect(SelectEnterEventArgs _) { if (kept && open == this) { kept = false; hideAt = Time.time; } else { Show(); kept = true; } }

        void Show()
        {
            if (open && open != this) open.Hide();
            open = this; hideAt = -1;
            if (card && !card.activeSelf) card.SetActive(true);
        }
        void Hide() { kept = false; hideAt = Time.time; }

        void Update()
        {
            if (spinner && secondsPerTurn > 0) spinner.Rotate(0, 360 * Time.deltaTime / secondsPerTurn, 0, Space.Self);
            if (!card) return;
            bool want = card.activeSelf && (hideAt < 0 || Time.time < hideAt || hovered || kept);
            shown = Mathf.MoveTowards(shown, want ? 1 : 0, Time.deltaTime / popSeconds);
            float e = shown * shown * (3 - 2 * shown);
            card.transform.localScale = cardScale * e;
            if (shown <= 0 && !want && card.activeSelf) { card.SetActive(false); if (open == this) open = null; }
            if (card.activeSelf)
            {
                var head = Camera.main;
                if (head) { var away = card.transform.position - head.transform.position; away.y = 0; if (away.sqrMagnitude > 1e-3f) card.transform.rotation = Quaternion.LookRotation(away); }
            }
        }
    }
}
