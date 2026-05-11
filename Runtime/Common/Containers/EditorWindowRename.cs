using UnityEditor;
using UnityEngine;

namespace Foolish.Utils.Containers.Editor
{
    public class EditorWindowRename : EditorWindow
    {
        private ScriptableObject item;
        private IRebuilder editor;
        private string newName;

        public void Initialize(ScriptableObject item, IRebuilder editor)
        {
            this.item = item;
            this.editor = editor;
            newName = item.name;
        }

        private void OnGUI()
        {
            newName = EditorGUILayout.TextField("Name", newName);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Save"))
            {
                item.name = newName;
                EditorUtility.SetDirty(item);
                AssetDatabase.SaveAssets();

                editor.Rebuild();
                Close();
            }

            if (GUILayout.Button("Cancel"))
            {
                Close();
            }

            GUILayout.EndHorizontal();
        }
    }
}