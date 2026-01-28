#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Foolish.Utils.Editor
{
    [CustomPropertyDrawer(typeof(NestedObjectInspector))]
    public class NestedObjectInspectorDrawer : PropertyDrawer
    {
        bool showNestedObject = false;
        UnityEditor.Editor EditorInstance;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.boxedValue == null || property.boxedValue is null)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            Rect buttonRect = new Rect(position.x, position.y, 10, EditorGUI.GetPropertyHeight(property));
            Rect propertyRect = new Rect(position.x + 15, position.y, position.width - 15,
                EditorGUI.GetPropertyHeight(property));
            showNestedObject = EditorGUI.Toggle(buttonRect, "", showNestedObject,
                EditorStyles.foldout);
            EditorGUI.PropertyField(propertyRect, property, GUIContent.none, true);
            if (showNestedObject)
            {
                if (property.boxedValue != null || property.boxedValue is not null)
                {
                    EditorGUILayout.Space(5f);
                    Rect rect = EditorGUILayout.BeginVertical();

                    DrawUIBox(rect, Color.Lerp(Color.white, Color.gray, .5f), Color.Lerp(Color.black, Color.gray, .5f));
                    EditorGUILayout.Space(10f);

                    EditorGUI.indentLevel++;
                    using (new EditorGUILayout.VerticalScope())
                    {

                        if (EditorInstance is null or null)
                        {
                            CreateGui(property);
                        }

                        if (EditorInstance is not null and not null)
                        {
                            EditorInstance.OnInspectorGUI();
                        }
                        EditorGUILayout.Space(10f);
                    }
                    EditorGUILayout.Space(10f);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();
                }
            }

            EditorGUI.EndProperty();
        }

        void DrawUIBox(Rect rect, Color borderColor, Color backgroundColor, int width = 2)
        {
            Rect outer = new Rect(rect);
            Rect inner = new Rect(rect.x + width, rect.y + width, rect.width - width * 2, rect.height - width * 2);
            EditorGUI.DrawRect(outer, borderColor);
            EditorGUI.DrawRect(inner, backgroundColor);
        }

        void CreateGui(SerializedProperty property)
        {
            EditorInstance = UnityEditor.Editor.CreateEditor(property.objectReferenceValue);
        }
    }

}
#endif