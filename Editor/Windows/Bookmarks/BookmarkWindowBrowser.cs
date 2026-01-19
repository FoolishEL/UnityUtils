using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace Foolish.Utils.Editor.Windows
{
    public class BookmarkWindowBrowser : SettingsDependentWindow
    {
        const string PLAYER_PREFS_KEY = "AssetListWindow_Data{0}";
        const string PLAYER_PREFS_KEY_GROUP = "AssetListWindow_Data_Group{0}";
        string ProjectName => windowsSettingsAsset.ProjectTitle;
        
        const float ITEM_BUTTON_WIDTH = 50f;
        string PlayerPrefsKeyFormated => string.Format(PLAYER_PREFS_KEY, ProjectName);
        string PlayerPrefsKeyGroupFormated => string.Format(PLAYER_PREFS_KEY_GROUP, ProjectName);
        
        List<string> assetPaths = new();
        List<string> assetsSubGroups = new();
        Dictionary<string, bool> expandedGroupStatus = new();
        Dictionary<string, List<int>> groupsData = new();
        List<Object> loadedObjects = new();
        Vector2 scrollPosition;
        string newGroupName = "";
        string renameGroupName = "";
        string groupToRename = "";

        [MenuItem("Tools/Developer/Bookmark Window Browser",priority = 0)]
        public static void ShowWindow()
        {
            GetWindow<BookmarkWindowBrowser>("Bookmarks");
        }

        protected override void OnEnableInternal() => LoadData();

        protected override void OnDisableInternal() => SaveData();

        protected override void OnGUIInternal()
        {
            EditorGUILayout.LabelField("Drag and drop assets below:", EditorStyles.boldLabel);

            Rect dropArea = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drag assets here", EditorStyles.helpBox);
            using (new EditorGUILayout.HorizontalScope("Box"))
            {
                if (GUILayout.Button("Save List"))
                {
                    SaveData();
                }
                if (GUILayout.Button("Load List"))
                {
                    LoadData();
                }
                if (GUILayout.Button("Clear List"))
                {
                    ClearData();
                }
            }

            EditorGUILayout.BeginHorizontal();
            newGroupName = EditorGUILayout.TextField("New Group Name", newGroupName);
            if (GUILayout.Button("Create Group", GUILayout.Width(100)))
            {
                if (!string.IsNullOrEmpty(newGroupName) && !groupsData.ContainsKey(newGroupName))
                {
                    groupsData.Add(newGroupName, new());
                    newGroupName = "";
                }
            }
            EditorGUILayout.EndHorizontal();

            HandleDragAndDrop(dropArea);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            var groupsList = groupsData.ToList();
            foreach (var (groupName, assetsIndexes) in groupsList)
            {
                DrawGroup(groupName, assetsIndexes);
            }
            EditorGUILayout.EndScrollView();
        }

        void DrawGroup(string groupName, List<int> assetIndexes)
        {
            EditorGUILayout.BeginVertical("Box");
            if (!expandedGroupStatus.TryGetValue(groupName, out var status))
            {
                status = false;
                expandedGroupStatus[groupName] = false;
            }
            bool isExpanded = EditorGUILayout.Foldout(status, groupName);
            if (isExpanded)
            {
                EditorGUILayout.BeginHorizontal("Box");
                GUILayout.FlexibleSpace();
                if (groupName != "Common")
                {
                    if (GUILayout.Button("Rename", GUILayout.Width(60)))
                    {
                        groupToRename = groupName;
                        renameGroupName = groupName;
                    }
                    if (GUILayout.Button("Delete", GUILayout.Width(60)))
                    {
                        DeleteGroup(groupName);
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();

                if (groupToRename == groupName)
                {
                    EditorGUILayout.BeginHorizontal();
                    renameGroupName = EditorGUILayout.TextField(renameGroupName);
                    if (GUILayout.Button("Save", GUILayout.Width(60)))
                    {
                        RenameGroup(groupName, renameGroupName);
                        groupToRename = "";
                    }
                    if (GUILayout.Button("Cancel", GUILayout.Width(60)))
                    {
                        groupToRename = "";
                    }
                    EditorGUILayout.EndHorizontal();
                }

                for (int i = 0; i < assetIndexes.Count; i++)
                {
                    int index = assetIndexes[i];
                    DrawAssetRow(index);
                    EditorGUILayout.Space(20);
                }
            }
            expandedGroupStatus[groupName] = isExpanded;
            EditorGUILayout.EndVertical();
        }

        void DrawAssetRow(int index)
        {
            EditorGUILayout.BeginHorizontal();

            var asset = GetAssetByIndex(index);

            if (asset != null)
            {
                GUI.enabled = false;
                EditorGUILayout.ObjectField(asset, typeof(Object), false, GUILayout.Height(40));
                GUI.enabled = true;
                using (new EditorGUILayout.VerticalScope())
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Ping", GUILayout.Width(ITEM_BUTTON_WIDTH)))
                        {
                            EditorGUIUtility.PingObject(asset);
                        }

                        if (GUILayout.Button("Open", GUILayout.Width(ITEM_BUTTON_WIDTH)))
                        {
                            bool isFolder = false;
                            if (asset is DefaultAsset defaultAsset)
                            {
                                string path = AssetDatabase.GetAssetPath(defaultAsset);
                                if (AssetDatabase.IsValidFolder(path))
                                {
                                    AssetDatabase.OpenAsset(defaultAsset);
                                    isFolder = true;
                                }
                            }
                            if (!isFolder)
                                AssetDatabase.OpenAsset(asset);
                        }
                    }
                    using (new EditorGUILayout.HorizontalScope())
                    {

                        if (GUILayout.Button("Move", GUILayout.Width(ITEM_BUTTON_WIDTH)))
                        {
                            ShowGroupPopup(index);
                        }

                        if (GUILayout.Button("Delete", GUILayout.Width(ITEM_BUTTON_WIDTH)))
                        {
                            RemoveAsset(index);
                        }
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("Missing asset", GUILayout.ExpandWidth(true));
                if (GUILayout.Button("Delete", GUILayout.Width(ITEM_BUTTON_WIDTH)))
                {
                    RemoveAsset(index);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        void ShowGroupPopup(int assetIndex)
        {
            GenericMenu menu = new GenericMenu();
            foreach (var groupName in groupsData.Keys)
            {
                menu.AddItem(new(groupName), false, () => MoveAssetToGroup(assetIndex, groupName));
            }
            menu.ShowAsContext();
        }

        void MoveAssetToGroup(int assetIndex, string newGroup)
        {
            string oldGroup = assetsSubGroups[assetIndex];
            groupsData[oldGroup].Remove(assetIndex);
            assetsSubGroups[assetIndex] = newGroup;
            groupsData[newGroup].Add(assetIndex);
        }

        void DeleteGroup(string groupName)
        {
            if (groupName == "Common")
                return;

            var assetsToMove = groupsData[groupName];
            foreach (var assetIndex in assetsToMove)
            {
                assetsSubGroups[assetIndex] = "Common";
                groupsData["Common"].Add(assetIndex);
            }

            groupsData.Remove(groupName);
            expandedGroupStatus.Remove(groupName);
        }

        void RenameGroup(string oldGroupName, string renameNewGroupName)
        {
            if (oldGroupName == "Common" || groupsData.ContainsKey(renameNewGroupName))
                return;

            var assetsIndexes = groupsData[oldGroupName];
            groupsData.Remove(oldGroupName);
            groupsData.Add(renameNewGroupName, assetsIndexes);

            for (int i = 0; i < assetsSubGroups.Count; i++)
            {
                if (assetsSubGroups[i] == oldGroupName)
                {
                    assetsSubGroups[i] = renameNewGroupName;
                }
            }

            expandedGroupStatus.Remove(oldGroupName);
            expandedGroupStatus[renameNewGroupName] = true;
        }

        void HandleDragAndDrop(Rect dropArea)
        {
            Event evt = Event.current;

            if (!dropArea.Contains(evt.mousePosition))
                return;

            if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        string path = AssetDatabase.GetAssetPath(draggedObject);
                        if (!string.IsNullOrEmpty(path))
                        {
                            AddAsset(path, assetPaths.Count);
                        }
                    }
                }

                evt.Use();
            }
        }

        void SaveData()
        {
            EditorPrefs.SetString(PlayerPrefsKeyFormated, string.Join(";", assetPaths));
            EditorPrefs.SetString(PlayerPrefsKeyGroupFormated, string.Join(";", assetsSubGroups));
        }

        void LoadData()
        {
            TryMigrate();
            ClearData();
            string data = EditorPrefs.GetString(PlayerPrefsKeyFormated, string.Empty);
            string groupData = EditorPrefs.GetString(PlayerPrefsKeyGroupFormated, string.Empty);
            groupsData.Add("Common", new());
            if (!string.IsNullOrEmpty(data))
            {
                var paths = data.Split(';');
                string[] groups;
                if (!string.IsNullOrEmpty(groupData))
                {
                    groups = groupData.Split(';');
                }
                else
                {
                    groups = new string[paths.Length];
                    for (int i = 0; i < paths.Length; i++)
                    {
                        groups[i] = "Common";
                    }

                }
                for (int i = 0; i < paths.Length; i++)
                {
                    string path = paths[i];
                    AddAsset(path, i, groups[i]);
                }
            }
        }

        void TryMigrate()
        {
            var unFormated = string.Format(PLAYER_PREFS_KEY, "");
            var unFormatedGroups = string.Format(PLAYER_PREFS_KEY_GROUP, "");

            if (EditorPrefs.HasKey(unFormated))
            {
                var data = EditorPrefs.GetString(unFormated, string.Empty);
                var data2 = EditorPrefs.GetString(unFormatedGroups, string.Empty);

                EditorPrefs.SetString(PlayerPrefsKeyFormated, data);
                EditorPrefs.SetString(PlayerPrefsKeyGroupFormated, data2);
                EditorPrefs.DeleteKey(unFormated);
                EditorPrefs.DeleteKey(unFormatedGroups);
            }
        }

        void AddAsset(string path, int index, string group = "Common")
        {
            if (assetPaths.Contains(path))
                return;
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            loadedObjects.Add(asset);
            assetPaths.Add(path);
            assetsSubGroups.Add(group);
            if (groupsData.ContainsKey(group))
            {
                groupsData[group].Add(index);
            }
            else
            {
                groupsData.Add(group, new()
                {
                    index
                });
            }
        }

        void RemoveAsset(int index)
        {
            assetPaths.RemoveAt(index);
            loadedObjects.RemoveAt(index);
            var group = assetsSubGroups[index];
            assetsSubGroups.RemoveAt(index);
            groupsData[group].Remove(index);
            foreach (var g in groupsData.Values)
            {
                for (int i = 0; i < g.Count; i++)
                {
                    var item = g[i];
                    if (item > index)
                    {
                        g[i]--;
                    }
                }
            }
        }

        Object GetAssetByIndex(int index)
        {
            return loadedObjects[index];
        }

        void ClearData()
        {
            assetPaths.Clear();
            loadedObjects.Clear();
            assetsSubGroups.Clear();
            groupsData.Clear();
        }
    }
}