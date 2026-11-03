using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace StudyEverywhere.EditorTools
{
    // Calypso's water orb in the saved Seabreeze scene (AI review item 45: the sphere was missing). It replaces her old
    // rectangular card with the form New World has: a 12 cm bubble of moving water at the person's lower right, "Calypso"
    // lettered inside it (tap the orb to talk), and her captions beside it only while she listens, thinks or talks. The
    // rest of the beach is left as it is. The orb's mesh and materials are the ones in Seabreeze/Meshes and /Materials.
    // Unity.exe -batchmode -quit -projectPath . -executeMethod StudyEverywhere.EditorTools.NewWorldAICalypso.Rebuild
    public static class NewWorldAICalypso
    {
        const string Scene = "Assets/Scenes/Seabreeze.unity", Root = "Assets/StudyEverywhere/Seabreeze";
        // 0.55 m out, 32 degrees to the right and 30 cm below the eyes: clear of the wrist menu and within reach.
        const float Distance = .55f, Yaw = 32, Drop = .3f, Radius = .06f, Scale = .0005f;
        static readonly Vector2 Captions = new Vector2(640, 330);
        static TMP_FontAsset serif, sans;

        public static void Rebuild()
        {
            var scene = EditorSceneManager.OpenScene(Scene, OpenSceneMode.Single);
            var calypso = Object.FindAnyObjectByType<CalypsoCompanion>();
            if (!calypso) throw new System.InvalidOperationException("Seabreeze has no Calypso.");
            serif = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/StudyEverywhere/Paris/UI/Lora SDF.asset");
            sans = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/StudyEverywhere/Paris/UI/Plus Jakarta Sans SDF.asset");
            var head = calypso.user ? calypso.user.GetComponent<Camera>() : Camera.main;

            // her voice moves to the orb, so it is in front of the person wherever they look; the old card goes
            var speaker = calypso.speaker;
            if (speaker) speaker.transform.SetParent(null, true);
            if (calypso.card) Object.DestroyImmediate(calypso.card.gameObject);

            var form = new GameObject("Calypso's form | the water orb and her captions, lower right").transform;
            var eye = SeabreezeBeach.OnSand(.1f, -.15f) + Vector3.up * .78f; var fwd = Quaternion.Euler(0, 8, 0) * Vector3.forward;
            form.position = eye + Quaternion.Euler(0, Yaw, 0) * fwd * Distance + Vector3.down * Drop;
            form.rotation = Quaternion.LookRotation(form.position - eye, Vector3.up);
            var orb = BuildOrb(form);
            var group = BuildName(head, form, out var heard, out var done, out var reply, out var talk);
            if (speaker)
            {
                speaker.transform.SetParent(orb.transform, false); speaker.transform.localPosition = Vector3.zero;
                orb.speech = speaker.GetComponent<AudioSource>();
            }

            calypso.card = form; calypso.orb = orb; calypso.cardGroup = group;
            calypso.cardDistance = Distance; calypso.cardYaw = Yaw; calypso.cardDrop = Drop;
            calypso.status = null; calypso.heard = heard; calypso.done = done; calypso.reply = reply; calypso.talk = talk;
            EditorUtility.SetDirty(calypso);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Capture();
        }

        // (from the scene as saved: canvases built in this same pass do not draw their text into a camera render yet)
        public static void Capture()
        {
            EditorSceneManager.OpenScene(Scene, OpenSceneMode.Single);
            var calypso = Object.FindAnyObjectByType<CalypsoCompanion>();
            Capture(calypso, SeabreezeBeach.OnSand(.1f, -.15f) + Vector3.up * .78f);
        }

        static CalypsoOrb BuildOrb(Transform form)
        {
            var go = new GameObject("Calypso's orb | water in a bubble"); go.transform.SetParent(form, false);
            go.transform.localPosition = new Vector3(0, 0, .04f);
            var ball = new GameObject("Bubble"); ball.transform.SetParent(go.transform, false);
            ball.transform.localScale = Vector3.one * Radius * 2;
            ball.AddComponent<MeshFilter>().sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(Root + "/Meshes/Calypso orb sphere.asset");
            var mr = ball.AddComponent<MeshRenderer>();
            mr.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/Calypso orb.mat");
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; mr.receiveShadows = false; mr.sortingOrder = 30;
            mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off; mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes;

            var fx = new GameObject("Calypso's droplets"); fx.transform.SetParent(go.transform, false);
            var ps = fx.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main; main.loop = true; main.playOnAwake = true; main.maxParticles = 90; main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.startSpeed = 0; main.startLifetime = 1; main.startSize = .005f; main.gravityModifier = 0; main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            var em = ps.emission; em.rateOverTime = 0;   // CalypsoOrb emits each one itself
            var sh = ps.shape; sh.enabled = false;
            var col = ps.colorOverLifetime; col.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) }, new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, .2f), new GradientAlphaKey(.8f, .6f), new GradientAlphaKey(0, 1) });
            col.color = g;
            var size = ps.sizeOverLifetime; size.enabled = true; size.size = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(new Keyframe(0, .5f), new Keyframe(.3f, 1), new Keyframe(1, .7f)));
            var drag = ps.limitVelocityOverLifetime; drag.enabled = true; drag.drag = 1.5f; drag.limit = 1;
            var pr = fx.GetComponent<ParticleSystemRenderer>();
            pr.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/Calypso droplet.mat");
            pr.renderMode = ParticleSystemRenderMode.Billboard; pr.sortingOrder = 31;
            pr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; pr.receiveShadows = false;

            var orb = go.AddComponent<CalypsoOrb>();
            orb.body = mr; orb.droplets = ps; orb.radius = Radius;
            return orb;
        }

        // Her name inside the orb (a round, clear Talk button over it), and her captions to its left with no pane behind them.
        static CanvasGroup BuildName(Camera head, Transform form, out TMP_Text heard, out TMP_Text done, out TMP_Text reply, out Button talk)
        {
            var nameGO = new GameObject("Calypso's name | inside her orb, tap to talk", typeof(RectTransform));
            var nc = nameGO.AddComponent<Canvas>(); nc.renderMode = RenderMode.WorldSpace; nc.sortingOrder = 41; nc.worldCamera = head;
            nameGO.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 4; nameGO.AddComponent<TrackedDeviceGraphicRaycaster>();
            var nrt = (RectTransform)nameGO.transform; float d = Radius * 2 / Scale; nrt.sizeDelta = new Vector2(d, d); nrt.localScale = Vector3.one * Scale;
            nrt.SetParent(form, false); nrt.localPosition = new Vector3(0, 0, .038f); nrt.localRotation = Quaternion.identity;
            var hit = new GameObject("Tap", typeof(RectTransform), typeof(Image)); hit.transform.SetParent(nrt, false);
            ((RectTransform)hit.transform).sizeDelta = new Vector2(d, d); var hitImage = hit.GetComponent<Image>(); hitImage.color = new Color(1, 1, 1, 0); hitImage.raycastTarget = true;
            talk = hit.AddComponent<Button>(); talk.transition = Selectable.Transition.None;
            var name = Text("Name", nrt, "Calypso", Vector2.zero, new Vector2(d * .86f, 60), serif, 34, new Color(1, 1, 1, .95f), TextAlignmentOptions.Center);
            name.enableAutoSizing = true; name.fontSizeMin = 20; name.fontSizeMax = 34;
            name.outlineWidth = .18f; name.outlineColor = new Color32(18, 70, 84, 200);   // a deep sea-teal edge: legible over the bright water

            var go = new GameObject("Calypso's captions | beside her orb", typeof(RectTransform));
            var canvas = go.AddComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace; canvas.sortingOrder = 40; canvas.worldCamera = head;
            go.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 3;
            var group = go.AddComponent<CanvasGroup>(); group.alpha = 0; group.blocksRaycasts = false; group.interactable = false;
            var rt = (RectTransform)go.transform; rt.sizeDelta = Captions; rt.localScale = Vector3.one * Scale;
            rt.SetParent(form, false);
            rt.localPosition = new Vector3(-(Radius + .015f + Captions.x * Scale / 2), 0, 0); rt.localRotation = Quaternion.identity;
            float w = Captions.x - 10;
            TMP_Text Line(string n, Vector2 at, float h, int fs, Color c)
            {
                var t = Text(n, rt, "", at, new Vector2(w, h), sans, fs, c, TextAlignmentOptions.Right);
                t.outlineWidth = .2f; t.outlineColor = new Color32(14, 52, 62, 210); return t;
            }
            heard = Line("Heard", new Vector2(0, 112), 34, 19, new Color(.92f, .97f, .98f)); heard.fontStyle = FontStyles.Italic;
            done = Line("Done", new Vector2(0, 80), 28, 16, new Color(.62f, .93f, .9f)); done.fontStyle = FontStyles.Bold; done.characterSpacing = 2;
            reply = Line("Reply", new Vector2(0, -20), 170, 22, Color.white);
            reply.textWrappingMode = TextWrappingModes.Normal; reply.verticalAlignment = VerticalAlignmentOptions.Top;
            foreach (var t in new[] { heard, done }) t.textWrappingMode = TextWrappingModes.NoWrap;
            return group;
        }

        static TextMeshProUGUI Text(string name, RectTransform parent, string s, Vector2 pos, Vector2 size, TMP_FontAsset f, int fs, Color c, TextAlignmentOptions a)
        {
            var g = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)); g.transform.SetParent(parent, false);
            var r = (RectTransform)g.transform; r.anchoredPosition = pos; r.sizeDelta = size;
            var t = g.GetComponent<TextMeshProUGUI>();
            if (f) { t.font = f; t.fontSharedMaterial = f.material; }
            t.text = s; t.fontSize = fs; t.color = c; t.alignment = a; t.overflowMode = TextOverflowModes.Ellipsis; t.raycastTarget = false;
            return t;
        }

        // The view from the shore and a close look, with a sample exchange (not saved), into Builds/calypso-*.png.
        static void Capture(CalypsoCompanion calypso, Vector3 eye)
        {
            calypso.cardGroup.alpha = 1;
            calypso.heard.text = "“Calypso, can you hatch the turtles at sunset?”";
            calypso.done.text = "Turtles hatching   ·   Sunset";
            calypso.reply.text = "Of course. The sun's going down now, and look, the first little ones are digging out of the nest.";
            foreach (var m in Object.FindObjectsByType<WristPanel>(FindObjectsInactive.Exclude)) m.GetComponent<Canvas>().enabled = false;
            var mat = calypso.orb.body.sharedMaterial; mat.SetFloat("_Flow", 3);
            Canvas.ForceUpdateCanvases();
            Directory.CreateDirectory("Builds");
            var cam = new GameObject("Preview camera").AddComponent<Camera>(); cam.nearClipPlane = .02f;
            var form = calypso.card.position;
            void Shot(string n, Vector3 from, Vector3 at, float fov)
            {
                cam.transform.SetPositionAndRotation(from, Quaternion.LookRotation(at - from)); cam.fieldOfView = fov;
                var rt = new RenderTexture(1600, 1000, 24) { antiAliasing = 4 }; cam.targetTexture = rt; cam.Render();
                RenderTexture.active = rt; var tex = new Texture2D(1600, 1000, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, 1600, 1000), 0, 0); tex.Apply();
                File.WriteAllBytes("Builds/calypso-" + n + ".png", tex.EncodeToPNG());
                RenderTexture.active = null; cam.targetTexture = null; Object.DestroyImmediate(rt); Object.DestroyImmediate(tex);
            }
            Shot("view", eye, eye + Quaternion.Euler(0, 8, 0) * Vector3.forward * 3 + Vector3.down * .5f, 80);
            var close = form - calypso.card.forward * .42f + Vector3.down * .06f;
            Shot("close", close, form + Vector3.down * .08f, 50);
            mat.SetFloat("_Flow", 0);
            Object.DestroyImmediate(cam.gameObject);
        }
    }
}
