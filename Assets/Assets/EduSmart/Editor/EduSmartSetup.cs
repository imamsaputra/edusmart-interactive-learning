#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Otomatis membuat 5 scene + mengisi Build Settings saat paket pertama kali di-import.
// Bisa dijalankan ulang lewat menu: EduSmart > Setup Scenes & Build Settings
public static class EduSmartSetup {
    const string Dir = "Assets/EduSmart/Scenes";
    static readonly string[] Names = { Nav.S1, Nav.S2, Nav.S3, Nav.S4, Nav.S5 };
    static readonly System.Type[] Types = { typeof(WelcomeScene), typeof(MainMenuScene), typeof(MateriScene), typeof(QuizScene), typeof(LeaderboardScene) };

    [InitializeOnLoadMethod]
    static void Auto() {
        EditorApplication.delayCall += () => {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!File.Exists(Dir + "/" + Names[0] + ".unity")) Build();
        };
    }

    [MenuItem("EduSmart/Setup Scenes & Build Settings")]
    public static void Build() {
        Directory.CreateDirectory(Dir);
        var list = new List<EditorBuildSettingsScene>();
        for (int i = 0; i < Names.Length; i++) {
            string path = Dir + "/" + Names[i] + ".unity";
            var sc = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cam = new GameObject("Main Camera") { tag = "MainCamera" };
            var c = cam.AddComponent<Camera>(); c.clearFlags = CameraClearFlags.SolidColor; c.backgroundColor = new Color(.96f, .97f, .99f);
            cam.AddComponent<AudioListener>();
            new GameObject("SceneController").AddComponent(Types[i]);
            EditorSceneManager.SaveScene(sc, path);
            list.Add(new EditorBuildSettingsScene(path, true));
        }
        EditorBuildSettings.scenes = list.ToArray();
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(Dir + "/" + Names[0] + ".unity");
        Debug.Log("EduSmart: 5 scene dibuat & Build Settings terisi. Tekan Play pada Scene_01_Welcome.");
    }
}
#endif
