#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace Foolish.Utils.Editor.Windows
{
    public class ScriptableObjectCreator : SettingsDependentWindow
    {
        string ProjectName => windowsSettingsAsset.ProjectTitle;
        private const string EDITOR_PREFS_PATH = "ScriptableObjectCreator_{0}_Namespace_";
        string EDITOR_PREFS_PATH_FORMATED => string.Format(EDITOR_PREFS_PATH, ProjectName);
        
        private const string EDITOR_PREFS_PATH_IGNORED_NAMESPACES = "ScriptableObjectCreator_{0}_ignoredNamespaces";
        
        string EDITOR_PREFS_PATH_IGNORED_NAMESPACES_FORMATED => string.Format(EDITOR_PREFS_PATH_IGNORED_NAMESPACES, ProjectName);
        
        private static HashSet<Type> scriptableObjectTypes = new();
        private string searchString = "";
        private Vector2 typeScrollPos;
        private Vector2 previewScrollPos;
        private ScriptableObject previewObject;
        private string targetFolder;
        private Type selectedType;
        private Dictionary<string, List<Type>> namespaceGroups = new();
        private UnityEditor.Editor previewEditor;
        private string newAssetName = "New Asset";
        private bool? expandAllNamespaces = false;
        private bool expandAllNamespacesLastValue;
        private List<string> ignoredNamespaces = new();
        private Vector2 ignoredNamespacesScrollPos;

        [MenuItem("Tools/Developer/ScriptableObjectCreator", priority = -100)]
        [MenuItem("Assets/Create Scriptable Object", priority = -1000)]
        private static void ShowDialog()
        {
            var path = "Assets";
            var obj = Selection.activeObject;
            if (obj && AssetDatabase.Contains(obj))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!Directory.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                }
            }

            var window = CreateInstance<ScriptableObjectCreator>();
            window.name = "Scriptable Object Creator";
            window.expandAllNamespacesLastValue = false;
            window.ShowUtility();
            var position = window.position;
            position.center = new Rect(0, 0, Screen.currentResolution.width, Screen.currentResolution.height).center / 2f;
            position.width = 800;
            position.height = 500;
            window.position = position;
            window.titleContent = new(path);
            if (path != null)
            {
                window.targetFolder = path.Trim('/');
            }

            scriptableObjectTypes = new(
                AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(t => t.IsClass &&
                                typeof(ScriptableObject).IsAssignableFrom(t) &&
                                !typeof(EditorWindow).IsAssignableFrom(t) &&
                                !typeof(UnityEditor.Editor).IsAssignableFrom(t))
            );
            window.UpdateIgnoredNameSpaces();
            window.UpdateFilteredTypes();
        }

        protected override void OnEnableInternal()
        {
            
        }
        protected override void OnDisableInternal()
        {
            
        }
        protected override void OnGUIInternal()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope(GUILayout.Width(270)))
                {
                    EditorGUI.BeginChangeCheck();
                    searchString = EditorGUILayout.TextField(searchString, EditorStyles.toolbarSearchField);
                    if (EditorGUI.EndChangeCheck())
                    {
                        UpdateFilteredTypes();
                    }

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(expandAllNamespacesLastValue ? "Collapse All" : "Expand All", EditorStyles.miniButton))
                    {
                        expandAllNamespacesLastValue = !expandAllNamespacesLastValue;
                        expandAllNamespaces = expandAllNamespacesLastValue;
                    }

                    if (GUILayout.Button("Namespaces", EditorStyles.miniButton))
                    {
                        ShowIgnoredNamespacesWindow();
                    }
                    EditorGUILayout.EndHorizontal();

                    typeScrollPos = EditorGUILayout.BeginScrollView(typeScrollPos);
                    {
                        var isForceValue = expandAllNamespaces.HasValue;
                        foreach (var namespaceGroup in namespaceGroups.OrderByDescending(c => c.Key.Contains("NAN")).ThenBy(g => g.Key))
                        {
                            var namespaceName = namespaceGroup.Key;
                            var types = namespaceGroup.Value;

                            var foldoutStyle = new GUIStyle(EditorStyles.foldout)
                            {
                                fontStyle = FontStyle.Bold
                            };

                            var expanded = EditorPrefs.GetBool($"{EDITOR_PREFS_PATH_FORMATED}{namespaceName}", false);
                            if (isForceValue)
                            {
                                expanded = expandAllNamespaces.Value;
                            }
                            expanded = EditorGUILayout.Foldout(expanded, namespaceName, foldoutStyle);
                            EditorPrefs.SetBool($"{EDITOR_PREFS_PATH_FORMATED}{namespaceName}", expanded);

                            if (expanded)
                            {
                                EditorGUI.indentLevel++;
                                foreach (var type in types)
                                {
                                    var isSelected = selectedType == type;
                                    var newSelection = EditorGUILayout.ToggleLeft(ObjectNames.NicifyVariableName(type.Name), isSelected);
                                    if (newSelection != isSelected && newSelection)
                                    {
                                        selectedType = type;
                                        newAssetName = "New " + ObjectNames.NicifyVariableName(selectedType.Name);
                                        CreatePreviewObject();
                                    }
                                }
                                EditorGUI.indentLevel--;
                            }
                        }
                        if (isForceValue)
                        {
                            expandAllNamespaces = null;
                        }
                    }
                    EditorGUILayout.EndScrollView();
                }

                using (new EditorGUILayout.VerticalScope("Box"))
                {
                    if (previewObject != null)
                    {
                        previewScrollPos = EditorGUILayout.BeginScrollView(previewScrollPos);
                        {
                            previewEditor = UnityEditor.Editor.CreateEditor(previewObject);
                            previewEditor?.OnInspectorGUI();
                        }
                        EditorGUILayout.EndScrollView();

                        GUILayout.FlexibleSpace();

                        EditorGUILayout.Separator();
                        EditorGUILayout.LabelField("Asset Name", EditorStyles.boldLabel);
                        newAssetName = EditorGUILayout.TextField(newAssetName);

                        EditorGUILayout.Separator();
                        if (GUILayout.Button("Create Asset", GUILayout.Height(30)))
                        {
                            CreateAsset();
                        }
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Select a ScriptableObject type to preview", MessageType.Info);
                    }
                }
            }
        }

        private void UpdateIgnoredNameSpaces()
        {
            ignoredNamespaces.Clear();
            if (EditorPrefs.HasKey(EDITOR_PREFS_PATH_IGNORED_NAMESPACES_FORMATED) && false)
            {
                var namespacesString = EditorPrefs.GetString(EDITOR_PREFS_PATH_IGNORED_NAMESPACES_FORMATED);
                var namespacesArray = namespacesString.Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
                ignoredNamespaces.AddRange(namespacesArray);
            }
            else
            {
                ignoredNamespaces = new()
                    { "Unity", "UnityEditor", "Foolish.Utils", "Rider", };
                SaveIgnoredNamespaces();
            }
        }

        private void SaveIgnoredNamespaces()
        {
            EditorPrefs.SetString(EDITOR_PREFS_PATH_IGNORED_NAMESPACES_FORMATED, string.Join("+", ignoredNamespaces));
        }

        private void UpdateFilteredTypes()
        {
            namespaceGroups.Clear();

            var typesToShow = scriptableObjectTypes
                .Where(NotContainsFilteredNamespace)
                .Where(t => !t.IsAbstract)
                .OrderBy(t => t.Namespace)
                .ThenBy(t => t.Name);

            if (!string.IsNullOrEmpty(searchString))
            {
                typesToShow = typesToShow
                    .Where(t =>
                        t.Name.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        (t.Namespace != null &&
                         t.Namespace.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0))
                    .OrderBy(t => t.Namespace);
            }

            foreach (var type in typesToShow)
            {
                var namespaceKey = string.IsNullOrEmpty(type.Namespace) ? "<Global Namespace>" : type.Namespace;

                if (!namespaceGroups.ContainsKey(namespaceKey))
                {
                    namespaceGroups[namespaceKey] = new();
                }
                namespaceGroups[namespaceKey].Add(type);
            }
        }

        private bool NotContainsFilteredNamespace(Type type)
        {
            if (type.Namespace != null)
            {
                return !ignoredNamespaces.Any(ns => type.Namespace.Contains(ns));
            }
            return true;
        }

        private void ShowIgnoredNamespacesWindow()
        {
            var window = CreateInstance<IgnoredNamespacesWindow>();
            window.parentWindow = this;
            window.ignoredNamespaces = new(ignoredNamespaces);
            window.titleContent = new("Ignored Namespaces");

            var windowPosition = position;
            windowPosition.x += position.width;
            windowPosition.width = 300;
            windowPosition.height = 400;
            window.position = windowPosition;

            window.ShowUtility();
        }

        private void CreatePreviewObject()
        {
            if (previewObject && !AssetDatabase.Contains(previewObject))
            {
                DestroyImmediate(previewObject);
            }

            if (selectedType is { IsAbstract: false })
            {
                previewObject = ScriptableObject.CreateInstance(selectedType);
            }
        }

        private void CreateAsset()
        {
            if (previewObject)
            {
                var cleanName = newAssetName.Trim();
                if (string.IsNullOrEmpty(cleanName))
                {
                    cleanName = "New " + selectedType.Name;
                }

                foreach (var invalidChar in Path.GetInvalidFileNameChars())
                {
                    cleanName = cleanName.Replace(invalidChar.ToString(), "");
                }

                var dest = targetFolder + "/" + cleanName + ".asset";
                dest = AssetDatabase.GenerateUniqueAssetPath(dest);
                AssetDatabase.CreateAsset(previewObject, dest);
                AssetDatabase.Refresh();
                Selection.activeObject = previewObject;
                EditorApplication.delayCall += Close;
            }
        }

        public class IgnoredNamespacesWindow : EditorWindow
        {
            public ScriptableObjectCreator parentWindow;
            public List<string> ignoredNamespaces;
            private Vector2 scrollPos;
            private string newNamespace = "";

            private void OnGUI()
            {
                EditorGUILayout.LabelField("Ignored Namespaces", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Types from these namespaces won't appear in the creator window", MessageType.Info);

                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                {
                    for (var i = 0; i < ignoredNamespaces.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            ignoredNamespaces[i] = EditorGUILayout.TextField(ignoredNamespaces[i]);
                            if (GUILayout.Button("×", GUILayout.Width(20)))
                            {
                                ignoredNamespaces.RemoveAt(i);
                                i--;
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndScrollView();

                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                {
                    newNamespace = EditorGUILayout.TextField("Add Namespace:", newNamespace);
                    if (GUILayout.Button("Add", GUILayout.Width(60)) && !string.IsNullOrWhiteSpace(newNamespace))
                    {
                        if (!ignoredNamespaces.Contains(newNamespace))
                        {
                            ignoredNamespaces.Add(newNamespace);
                            newNamespace = "";
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                if (GUILayout.Button("Save"))
                {
                    parentWindow.ignoredNamespaces = ignoredNamespaces;
                    parentWindow.SaveIgnoredNamespaces();
                    parentWindow.UpdateFilteredTypes();
                    parentWindow.Repaint();
                    Close();
                }

                if (GUILayout.Button("Reset to Default"))
                {
                    ignoredNamespaces = new()
                        { "Unity", "UnityEditor" };
                }

                if (GUILayout.Button("Cancel"))
                {
                    Close();
                }
            }
        }
    }
}
#endif