using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Foolish.Utils.Editor.Windows
{
    public class ExternalScenesPopup : EditorWindow
    {
        List<string> externalScenes = new();
        Vector2 scrollPosition;
        Action<List<string>> onScenesModified;

        public static void ShowPopupWindow(Action<List<string>> onScenesModified, List<string> currentScenes)
        {
            ExternalScenesPopup window = GetWindow<ExternalScenesPopup>(true, "Manage External Scenes", true);
            window.externalScenes = currentScenes;
            window.onScenesModified = onScenesModified;
            window.minSize = new Vector2(300, 200);
        }

        void OnGUI()
        {
            Rect dropArea = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drop scenes here", EditorStyles.helpBox);

            HandleDragAndDrop(dropArea);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            for (int i = externalScenes.Count - 1; i >= 0; i--)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(externalScenes[i]);
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    externalScenes.RemoveAt(i);
                    onScenesModified.Invoke(externalScenes);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }

        void HandleDragAndDrop(Rect dropArea)
        {
            Event evt = Event.current;
            if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
            {
                if (!dropArea.Contains(evt.mousePosition)) return;
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (string path in DragAndDrop.paths)
                    {
                        if (path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!externalScenes.Contains(path))
                            {
                                externalScenes.Add(path);
                                onScenesModified.Invoke(externalScenes);
                            }
                        }
                    }
                }
                evt.Use();
            }
        }
    }

}