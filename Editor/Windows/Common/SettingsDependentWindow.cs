using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Foolish.Utils.Editor.Windows
{
    internal static class EditorWindowUI
    {
        const float PagePadding = 10f;

        public static void BeginPage()
        {
            GUILayout.Space(PagePadding);
            GUILayout.BeginHorizontal();
            GUILayout.Space(PagePadding);
            GUILayout.BeginVertical();
        }

        public static void EndPage()
        {
            GUILayout.EndVertical();
            GUILayout.Space(PagePadding);
            GUILayout.EndHorizontal();
            GUILayout.Space(PagePadding);
        }

        public static void Header(string title, string subtitle = null)
        {
            GUILayout.Label(title, EditorStyles.largeLabel);
            if (!string.IsNullOrEmpty(subtitle))
            {
                var style = new GUIStyle(EditorStyles.wordWrappedMiniLabel)
                {
                    normal =
                    {
                        textColor = EditorGUIUtility.isProSkin
                            ? new Color(0.72f, 0.72f, 0.72f)
                            : new Color(0.32f, 0.32f, 0.32f)
                    }
                };
                GUILayout.Label(subtitle, style);
            }
            GUILayout.Space(8f);
        }

        public static void Section(string title)
        {
            GUILayout.Space(4f);
            GUILayout.Label(title, EditorStyles.boldLabel);
        }

        public static bool PrimaryButton(string label, params GUILayoutOption[] options)
        {
            var style = new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold };
            return GUILayout.Button(label, style, options);
        }

        public static Rect DropArea(string title, string hint)
        {
            var rect = GUILayoutUtility.GetRect(0, 64, GUILayout.ExpandWidth(true));
            GUI.Box(rect, GUIContent.none, EditorStyles.helpBox);

            var titleRect = new Rect(rect.x + 10, rect.y + 12, rect.width - 20, 20);
            var hintRect = new Rect(rect.x + 10, rect.y + 32, rect.width - 20, 18);
            GUI.Label(titleRect, title, EditorStyles.boldLabel);
            GUI.Label(hintRect, hint, EditorStyles.miniLabel);
            return rect;
        }
    }

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
