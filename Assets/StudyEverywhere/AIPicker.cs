using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace StudyEverywhere
{
    // The AI row on a menu, as the Language row: a pill naming the AI that answers the characters, with a little arrow;
    // press it and the list (Claude, Muse, ChatGPT) drops open beside it, pick one and it closes (AIProvider). Each time it
    // shows, it asks the relay which providers it has keys for (GET /ai/providers) and greys out the others.
    public sealed class AIPicker : MonoBehaviour
    {
        public Button toggle;
        public TMP_Text current;
        public GameObject list;
        public Button[] options;   // same order as AIProvider.Ids

        void OnEnable()
        {
            if (toggle) toggle.onClick.AddListener(Toggle);
            for (int i = 0; i < options.Length; i++) { int k = i; if (options[i]) options[i].onClick.AddListener(() => Pick(k)); }
            AIProvider.Changed += Refresh;
            if (list) list.SetActive(false);
            Refresh();
            StartCoroutine(AskRelay());
        }
        void OnDisable()
        {
            if (toggle) toggle.onClick.RemoveListener(Toggle);
            foreach (var o in options) if (o) o.onClick.RemoveAllListeners();
            AIProvider.Changed -= Refresh;
        }

        void Toggle() { if (list) { var p = list.transform.localPosition; p.z = -30; list.transform.localPosition = p; list.SetActive(!list.activeSelf); } }
        void Pick(int i) { AIProvider.Current = i; if (list) list.SetActive(false); Refresh(); }
        void Refresh() { if (current) current.text = AIProvider.Name; }

        [System.Serializable] class Providers { public string @default = ""; public string[] available = new string[0]; }

        // Without a relay (or when it can't be reached) every option stays open: the relay falls back to its default anyway.
        IEnumerator AskRelay()
        {
            var url = VoiceRelay.Url();
            if (string.IsNullOrEmpty(url)) yield break;
            using var request = UnityWebRequest.Get(url + "/ai/providers");
            VoiceRelay.Prepare(request);
            request.timeout = 5;
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success) yield break;
            Providers p;
            try { p = JsonUtility.FromJson<Providers>(request.downloadHandler.text); } catch (System.Exception) { yield break; }
            if (p?.available == null || p.available.Length == 0) yield break;
            for (int i = 0; i < options.Length && i < AIProvider.Ids.Length; i++)
                if (options[i]) options[i].interactable = System.Array.IndexOf(p.available, AIProvider.Ids[i]) >= 0;
            // the saved choice has no key on this relay: show the one that will really answer
            if (System.Array.IndexOf(p.available, AIProvider.Id) < 0)
            {
                int d = System.Array.IndexOf(AIProvider.Ids, p.@default);
                if (d >= 0) AIProvider.Current = d;
            }
        }
    }
}
