using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Foolish.Utils.Editor.Windows
{
    public class SceneBrowserWindow : SettingsDependentWindow
    {
        
        const string PLAYER_PREFS_KEY = "ExternalScenes_Data{0}";
        string ProjectName => windowsSettingsAsset.ProjectTitle;
        string PlayerPrefsKeyFormated => string.Format(PLAYER_PREFS_KEY, ProjectName);
        List<string> externalScenesPaths = new();

        string previousScenePath;
        bool shouldReturnToPreviousScene;
        GUIStyle BoxStyle
        {
            get
            {
                boxStyle ??= new("box");
                return boxStyle;
            }
        }
        GUIStyle boxStyle;

        bool isLocked;
        Vector2 scrollPosition;

        [MenuItem("Tools/Developer/Scene Browser", priority = 10)]
        public static void ShowWindow()
        {
            GetWindow<SceneBrowserWindow>("Scene Browser");
        }

        [MenuItem("Tools/Developer/Scene Browser on front", priority = 10)]
        public static void ShowWindowOnFront()
        {
            var window = GetWindow<SceneBrowserWindow>("Scene Browser");
            window.ShowUtility();
        }

        protected override void OnEnableInternal()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayMode;
            isLocked = EditorApplication.isPlaying;
            LoadData();
        }

        protected override void OnDisableInternal()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged -= OnPlayMode;
            SaveData();
        }
        
        void OnPlayMode(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode)
            {
                isLocked = true;
            }
            if (change == PlayModeStateChange.ExitingPlayMode)
            {
                isLocked = false;
            }
        }

        protected override void OnGUIInternal()
        {
            GUILayout.Label("Scenes in Build Settings", EditorStyles.boldLabel);
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            Vector2 currentSize = position.size;
            bool isHorizontal = currentSize.x > 300;
            Action<string, float> drawMethode = isHorizontal ? DrawSceneNormal : DrawNarrow;
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            foreach (var scene in scenes)
            {
                if (scene is null)
                    continue;
                var path = scene.path;
                drawMethode(path, currentSize.x);
            }
            if (externalScenesPaths.Count > 0)
            {
                EditorGUILayout.Space();
                GUILayout.Label("External Scenes", "box", GUILayout.ExpandWidth(true));
                EditorGUILayout.Space();
            }

            foreach (var scenePath in externalScenesPaths)
            {
                drawMethode(scenePath, currentSize.x);
            }

            GUILayout.EndScrollView();
        }

        void DrawSceneNormal(string scenePath, float maxWidth)
        {
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            GUILayout.BeginHorizontal(BoxStyle);

            GUILayout.Label(sceneName, GUILayout.Width(90));

            if (isLocked)
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button("Open", GUILayout.Width(100)) && !isLocked)
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                }
            }

            if (GUILayout.Button("Play", GUILayout.Width(100)) && !isLocked)
            {
                previousScenePath = EditorSceneManager.GetActiveScene().path;
                shouldReturnToPreviousScene = true;
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    EditorApplication.isPlaying = true;
                }
            }
            if (isLocked)
            {
                GUI.enabled = true;
            }

            GUILayout.EndHorizontal();
        }

        void DrawNarrow(string scenePath, float maxWidth)
        {
            bool isSuperNarrow = maxWidth < 200;

            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            GUILayout.BeginVertical(BoxStyle);

            GUILayout.Label(sceneName, GUILayout.Width(90));

            if (isLocked)
            {
                GUI.enabled = false;
            }
            if (!isSuperNarrow)
                GUILayout.BeginHorizontal();

            if (GUILayout.Button("Open", GUILayout.Width(100)) && !isLocked)
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                }
            }

            if (GUILayout.Button("Play", GUILayout.Width(100)) && !isLocked)
            {
                previousScenePath = EditorSceneManager.GetActiveScene().path;
                shouldReturnToPreviousScene = true;
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    EditorApplication.isPlaying = true;
                }
            }
            if (isLocked)
            {
                GUI.enabled = true;
            }
            if (!isSuperNarrow)
                GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode && shouldReturnToPreviousScene)
            {
                shouldReturnToPreviousScene = false;

                if (!string.IsNullOrEmpty(previousScenePath))
                {
                    EditorSceneManager.OpenScene(previousScenePath, OpenSceneMode.Single);
                }
            }
        }

        protected override void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new("Manage external scenes"), false, AddExternalScenes);
            base.AddItemsToMenu(menu);
        }

        void AddExternalScenes()
        {
            ExternalScenesPopup.ShowPopupWindow(OnScenesAdded, externalScenesPaths);
        }

        void OnScenesAdded(List<string> newScenesPaths)
        {
            externalScenesPaths = new(newScenesPaths);
            Repaint();
        }

        void SaveData()
        {
            EditorPrefs.SetString(PlayerPrefsKeyFormated, string.Join(";", externalScenesPaths));
        }

        void LoadData()
        {
            ClearData();
            string data = EditorPrefs.GetString(PlayerPrefsKeyFormated, string.Empty);
            if (string.IsNullOrEmpty(data))
                return;

            var paths = data.Split(';');
            List<string> corruptedPaths = new();
            foreach (var path in paths)
            {
                if (externalScenesPaths.Contains(path))
                    continue;

                SceneAsset asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                if (!asset)
                {
                    corruptedPaths.Add(path);
                    continue;
                }
                externalScenesPaths.Add(path);
            }

            foreach (var corruptPath in corruptedPaths)
            {
                externalScenesPaths.Remove(corruptPath);
            }
        }

        void ClearData()
        {
            externalScenesPaths = new();
        }
    }
}