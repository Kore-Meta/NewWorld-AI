using System.IO;
using System.Xml;
using UnityEditor;
using UnityEditor.Android;

namespace StudyEverywhere.EditorTools
{
    // The cats talk to the cat server on the local network over plain http (Tools/CatServer). Android refuses cleartext
    // traffic unless the app opts in, and Unity's "Allow downloads over HTTP" setting alone does not add the opt-in to
    // the manifest. So while that setting is Always allowed, every Android build marks its application
    // android:usesCleartextTraffic="true" in the generated Gradle project.
    sealed class AndroidCleartext : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 0;

        public void OnPostGenerateGradleAndroidProject(string unityLibrary)
        {
            if (PlayerSettings.insecureHttpOption != InsecureHttpOption.AlwaysAllowed) return;
            const string android = "http://schemas.android.com/apk/res/android";
            foreach (var manifest in new[] { Path.Combine(unityLibrary, "src/main/AndroidManifest.xml"), Path.Combine(unityLibrary, "../launcher/src/main/AndroidManifest.xml") })
            {
                if (!File.Exists(manifest)) continue;
                var doc = new XmlDocument(); doc.Load(manifest);
                if (doc.SelectSingleNode("/manifest/application") is not XmlElement app) continue;
                app.SetAttribute("usesCleartextTraffic", android, "true");
                doc.Save(manifest);
            }
        }
    }
}
