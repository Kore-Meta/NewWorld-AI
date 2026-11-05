using System.IO;
using System.Xml;
using UnityEditor.Android;

namespace StudyEverywhere.EditorTools
{
    // Typing on a shared computer (ScreenKeyboard) uses the Quest's own system keyboard. Horizon OS only shows its
    // overlay keyboard to an immersive app that declares the oculus.software.overlay_keyboard feature (what Meta's Core
    // SDK adds for "Require System Keyboard"; this project uses Unity's OpenXR Meta instead), so every Android build adds
    // it to the generated Gradle project's manifest.
    sealed class AndroidSystemKeyboard : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 1;

        public void OnPostGenerateGradleAndroidProject(string unityLibrary)
        {
            const string android = "http://schemas.android.com/apk/res/android", feature = "oculus.software.overlay_keyboard";
            var manifest = Path.Combine(unityLibrary, "src/main/AndroidManifest.xml");
            if (!File.Exists(manifest)) return;
            var doc = new XmlDocument(); doc.Load(manifest);
            if (doc.DocumentElement is not XmlElement root) return;
            foreach (XmlElement e in root.SelectNodes("uses-feature"))
                if (e.GetAttribute("name", android) == feature) return;
            var add = doc.CreateElement("uses-feature");
            add.SetAttribute("name", android, feature);
            add.SetAttribute("required", android, "false");
            root.AppendChild(add);
            doc.Save(manifest);
        }
    }
}
