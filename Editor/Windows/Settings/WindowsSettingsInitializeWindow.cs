using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Foolish.Utils.Editor.Windows
{
    public class WindowsSettingsInitializeWindow : EditorWindow
    {
        [InitializeOnLoadMethod]
        static void OpenIfNoSettingsAsset()
        {
            if (!AssetDatabase.FindAssets($"t:{nameof(WindowsSettingsAsset)}").Any())
            {
                GetWindow<WindowsSettingsInitializeWindow>("Initialize Settings");
            }
        }

        void OnGUI()
        {
            GUILayout.Label("Initialize Windows Settings", EditorStyles.boldLabel);

            if (GUILayout.Button("Create Settings Asset"))
            {
                string path = EditorUtility.SaveFilePanelInProject(
                    "Save Windows Settings Asset",
                    "WindowsSettingsAsset",
                    "asset",
                    "Please specify the file name for the Windows Settings Asset"
                );
                
                if (!string.IsNullOrEmpty(path))
                {
                    var settingsAsset = CreateInstance<WindowsSettingsAsset>();
                    settingsAsset.ProjectTitle = "Default Title";
                    
                    AssetDatabase.CreateAsset(settingsAsset, path);
                    AssetDatabase.SaveAssets();
                    
                    EditorUtility.DisplayDialog("Success", "Windows Settings Asset created successfully!", "OK");
                    
                    Close();
                }
            }

            if (GUILayout.Button("Close"))
            {
                Close();
            }
        }
    }
}
