using System;
using System.IO;
using System.Reflection;
using CricketUniverse.Data;
using CricketUniverse.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace CricketUniverse.Editor
{
    public static class CricketUniverseProjectSetup
    {
        private const string Root = "Assets/CricketUniverse";
        private const string ConfigPath = Root + "/Data/DemoMatchConfig.asset";
        private const string ScenePath = Root + "/Scenes/CricketUniverseDemo.unity";

        [MenuItem("Cricket Universe/Create Demo Scene")]
        public static void CreateDemoScene()
        {
            EnsureFolder(Root + "/Data");
            EnsureFolder(Root + "/Scenes");
            ConfigureInputSystem();

            MatchConfig config = AssetDatabase.LoadAssetAtPath<MatchConfig>(ConfigPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<MatchConfig>();
                config.oversPerInnings = 2;
                config.wicketsPerInnings = 3;
                config.demoTarget = 36;
                config.homeTeamName = "Harbor Hawks";
                config.awayTeamName = "Summit Strikers";
                AssetDatabase.CreateAsset(config, ConfigPath);
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject bootstrap = new GameObject("Cricket Universe");
            CricketUniverseBootstrap component = bootstrap.AddComponent<CricketUniverseBootstrap>();
            SerializedObject serialized = new SerializedObject(component);
            serialized.FindProperty("matchConfig").objectReferenceValue = config;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            EditorUtility.DisplayDialog("Cricket Universe", "Demo scene created. Open the scene and press Play.", "Let's play");
        }

        [MenuItem("Cricket Universe/Assign Selected URP Asset")]
        public static void AssignSelectedUrpAsset()
        {
            RenderPipelineAsset pipeline = Selection.activeObject as RenderPipelineAsset;
            if (pipeline == null)
            {
                EditorUtility.DisplayDialog("Cricket Universe", "Select the URP pipeline asset in the Project window first.", "OK");
                return;
            }
            GraphicsSettings.defaultRenderPipeline = pipeline;
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Cricket Universe", "URP assigned as the project render pipeline.", "OK");
        }

        private static void ConfigureInputSystem()
        {
            // Reflection keeps this small editor helper tolerant of Unity versions that rename the property.
            try
            {
                PropertyInfo inputSetting = typeof(PlayerSettings).GetProperty("activeInputHandler", BindingFlags.Public | BindingFlags.Static);
                if (inputSetting == null || !inputSetting.CanWrite) return;
                object both = Enum.Parse(inputSetting.PropertyType, "Both");
                inputSetting.SetValue(null, both);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not set Active Input Handling automatically. Choose Both in Project Settings > Player. {exception.Message}");
            }
        }

        private static void EnsureFolder(string path)
        {
            string absolute = Path.GetFullPath(path);
            if (Directory.Exists(absolute)) return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string leaf = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
