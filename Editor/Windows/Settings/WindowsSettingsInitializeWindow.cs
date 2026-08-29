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
            EditorWindowUI.BeginPage();
            EditorWindowUI.Header("Initialize Windows Settings",
                "Create the shared settings asset used by the developer tools.");

            if (EditorWindowUI.PrimaryButton("Create Settings Asset", GUILayout.Height(30)))
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

            GUILayout.Space(4);
            if (GUILayout.Button("Close"))
            {
                Close();
            }
            EditorWindowUI.EndPage();
        }
    }
}
