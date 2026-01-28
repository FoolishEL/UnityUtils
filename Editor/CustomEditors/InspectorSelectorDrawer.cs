#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Foolish.Utils.Editor
{
    [CustomPropertyDrawer(typeof(InspectorSelector))]
    public class InspectorSelectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var selectorAttribute = (InspectorSelector)attribute;
            var abstractType = selectorAttribute.AbstractType;

            if (!abstractType.IsAbstract && !abstractType.IsInterface)
            {
                EditorGUI.HelpBox(position,
                    $"Type {abstractType.Name} must be abstract or interface",
                    MessageType.Error);
                return;
            }
            var derivedTypes = TypesCache.GetDerivedTypes(abstractType)
                .Where(t => !t.IsAbstract && !t.IsInterface && !t.IsGenericTypeDefinition && !t.ContainsGenericParameters)
                .ToArray();

            var typeNames = derivedTypes
                .Select(t => string.IsNullOrEmpty(t.FullName) ? t.Name : t.FullName.Replace('.', '/'))
                .ToArray();

            var currentValue = property.managedReferenceValue;
            var currentIndex = currentValue != null
                ? Array.IndexOf(derivedTypes, currentValue.GetType())
                : -1;

            var lineHeight = EditorGUIUtility.singleLineHeight;


            float buttonWidth = EditorGUIUtility.labelWidth + 7;
            float labelWidth = position.width - buttonWidth;
            var labelRect = new Rect(position.x, position.y, labelWidth, lineHeight);
            var buttonRect = new Rect(position.x + labelWidth, position.y, buttonWidth, lineHeight);

            string instanceName = "None";
            bool isNone = true;
            if (currentIndex >= 0 && currentIndex < typeNames.Length)
            {
                instanceName = typeNames[currentIndex];
                var lastSlash = instanceName.LastIndexOf('/');
                if (lastSlash != -1)
                    instanceName = instanceName.Substring(lastSlash + 1);
                isNone = false;
            }
            if (!isNone)
            {
                EditorGUI.LabelField(labelRect, instanceName);
            }
            else
            {
                buttonRect = position;
            }
            if (GUI.Button(buttonRect, new GUIContent(isNone ? "Select type" : "Change type", EditorGUIUtility.IconContent("overlays/d_searchoverlay@2x").image)))
            {
                var menu = new GenericMenu();

                menu.AddItem(new("None"), currentIndex == -1, () => SetNewType(property, -1, derivedTypes));

                menu.AddSeparator("");

                for (int i = 0; i < derivedTypes.Length; i++)
                {
                    int index = i;
                    string displayName = typeNames[i];
                    bool selected = currentIndex == index;

                    menu.AddItem(new(displayName), selected, () => SetNewType(property, index, derivedTypes));
                }

                menu.DropDown(buttonRect);
            }

            if (currentValue != null)
            {
                var contentRect = new Rect(
                    position.x,
                    position.y + lineHeight + EditorGUIUtility.standardVerticalSpacing,
                    position.width,
                    position.height - lineHeight - EditorGUIUtility.standardVerticalSpacing);

                if (property.isExpanded)
                    DrawUIBox(contentRect, Color.Lerp(Color.white, Color.black, 0.6f), Color.Lerp(Color.black, Color.gray, 0.5f));

                contentRect.x += 2;
                contentRect.width -= 4;

                EditorGUI.PropertyField(contentRect, property, GUIContent.none, true);
            }
        }

        private static void SetNewType(SerializedProperty property, int newIndex, Type[] derivedTypes)
        {
            if (newIndex < 0)
            {
                property.managedReferenceValue = null;
            }
            else if (newIndex < derivedTypes.Length)
            {
                var selectedType = derivedTypes[newIndex];
                property.managedReferenceValue = Activator.CreateInstance(selectedType);
            }

            property.serializedObject.ApplyModifiedProperties();
        }

        private void DrawUIBox(Rect rect, Color borderColor, Color backgroundColor, int width = 2)
        {
            var outer = new Rect(rect);
            var inner = new Rect(rect.x + width, rect.y + width, rect.width - width * 2, rect.height - width * 2);
            EditorGUI.DrawRect(outer, borderColor);
            EditorGUI.DrawRect(inner, backgroundColor);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var selectorAttribute = (InspectorSelector)attribute;
            var abstractType = selectorAttribute.AbstractType;

            if (!abstractType.IsAbstract && !abstractType.IsInterface)
            {
                return EditorGUIUtility.singleLineHeight * 2;
            }

            var height = EditorGUIUtility.singleLineHeight;

            if (property.managedReferenceValue != null)
            {
                height += EditorGUI.GetPropertyHeight(property, true) + EditorGUIUtility.standardVerticalSpacing;
            }
            if (property.isExpanded)
            {
                height += EditorGUIUtility.singleLineHeight;
            }

            return height;
        }
    }

}

#endif