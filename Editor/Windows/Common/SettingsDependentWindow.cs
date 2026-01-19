using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Foolish.Utils.Editor.Windows
{
    public abstract class SettingsDependentWindow : EditorWindow, IHasCustomMenu
    {
        public WindowsSettingsAsset windowsSettingsAsset;
        
        bool TryLoadSettingsInternal()
        {
            var asset = AssetDatabase.FindAssets($"t:{nameof(WindowsSettingsAsset)}")
                .Select(guid => AssetDatabase.LoadAssetAtPath<WindowsSettingsAsset>(AssetDatabase.GUIDToAssetPath(guid)))
                .FirstOrDefault();
            if (asset)
            {
                windowsSettingsAsset = asset;
            }
            return asset is not null;
        }

        void OnEnable()
        {
            if (windowsSettingsAsset is null)
            {
                if (!TryLoadSettingsInternal())
                {
                    GetWindow<WindowsSettingsInitializeWindow>("Initialize Settings");
                    EditorApplication.delayCall += Close;
                    return;
                }
            }
            OnEnableInternal();
        }
        
        void OnDisable()
        {
            if (windowsSettingsAsset is not null)
            {
                OnDisableInternal();
            }
        }
        
        void OnGUI()
        {
            if (windowsSettingsAsset is null)
            {
                DrawWarningIfNoSettings();
            }
            else
            {
                OnGUIInternal();
            }
        }
        
        protected abstract void OnEnableInternal();
        
        protected abstract void OnDisableInternal();

        protected abstract void OnGUIInternal();

        void DrawWarningIfNoSettings()
        {
            EditorGUILayout.HelpBox(
                "No Windows Settings Asset found. Please ensure that the settings asset is created and assigned.", 
                MessageType.Warning
            );

            if (GUILayout.Button("Open Initialize Settings Window"))
            {
                GetWindow<WindowsSettingsInitializeWindow>("Initialize Settings");
            }
            if (GUILayout.Button("Check asset in project"))
            {
                TryLoadSettingsInternal();
            }
        }

        void IHasCustomMenu.AddItemsToMenu(GenericMenu menu) => AddItemsToMenu(menu);

        protected virtual void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new("Show script"), false, ShowScript);
        }
        
        void ShowScript()
        {
            var assets = AssetDatabase.FindAssets($"t:Script {GetType().Name}");
            if (assets.Length == 1)
            {
                var path = AssetDatabase.GUIDToAssetPath(assets[0]);
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                if (asset)
                {
                    EditorGUIUtility.PingObject(asset);
                }
                else
                {
                    Debug.LogError("Could not find asset");
                }
            }
            else
            {
                Debug.LogError($"Could not find asset: assets with name \"{GetType().Name}\" with type Script more ore less then one!");
            }
        }
    }
}