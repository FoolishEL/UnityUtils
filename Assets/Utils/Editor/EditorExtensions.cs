using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace Utils.Editor.Extensions
{
    /// <summary>
    /// Class for helpful methods for creating Custom editors. 
    /// </summary>
    public static class EditorExtensions
    {
        /// <summary>
        /// Draw script instance for custom inspector.
        /// Can be called only from UnityEditor.Editor.OnInspectorGUI method!
        /// </summary>
        public static void DrawScriptInstance(this UnityEditor.Editor editor,
            [CallerMemberName] string callerName = default!) 
        {
            if(!CallerCheckerOnInspectorGUI(callerName))
                return;
            
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)editor.target),
                    editor.GetType(), false);
        }

        /// <summary>
        /// Draw listed property.
        /// Can be called only from UnityEditor.Editor.OnInspectorGUI method!
        /// </summary>
        public static void DrawEditorList(SerializedProperty listedProperty,
            [CallerMemberName] string callerName = default!)
        {
            if(!CallerCheckerOnInspectorGUI(callerName))
                return;
            if (listedProperty is null)
            {
                Debug.LogError($"Property is null!");
                return;
            }

            EditorGUILayout.PropertyField(listedProperty);
        }

        private static bool CallerCheckerOnInspectorGUI(string callerName)
        {
            if (callerName != nameof(UnityEditor.Editor.OnInspectorGUI))
            {
                Debug.LogWarning(
                    $"Extension method {nameof(DrawScriptInstance)} can only be called from {nameof(UnityEditor.Editor.OnInspectorGUI)} method only");
                return false;
            }
            return true;
        }
    }
}