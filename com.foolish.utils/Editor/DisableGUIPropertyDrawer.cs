using UnityEditor;
using UnityEngine;

namespace Foolish.Utils.UI.Editor
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(DisableGUI))]
    public class DisableGUIPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var prevState = GUI.enabled;
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label);
            GUI.enabled = prevState;
        }
    }
#endif
}
