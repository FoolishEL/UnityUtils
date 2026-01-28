#if UNITY_EDITOR && UNITY_AI_NAVIGATION

using UnityEngine;
using UnityEngine.AI;

namespace Foolish.Utils.Editor
{
    [UnityEditor.CustomPropertyDrawer(typeof(NavMeshAreaAttribute))]
    public class NavMeshAreaDrawer : UnityEditor.PropertyDrawer
    {
        const int MAX_DISPLAY_NAMES = 5;
        private string[] names;
        private int[] areaIndices;

        private void Init()
        {
#if UNITY_2023_OR_NEWER
            var areaNames = NavMesh.GetAreaNames();
#else
            var areaNames = UnityEditor.GameObjectUtility.GetNavMeshAreaNames();
#endif
            int count = areaNames.Length;

            names = new string[count];
            areaIndices = new int[count];

            for (int i = 0; i < count; i++)
            {
                names[i] = areaNames[i];
                areaIndices[i] = NavMesh.GetAreaFromName(areaNames[i]);
                areaIndices[i] = 1 << areaIndices[i];
            }

        }

        public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != UnityEditor.SerializedPropertyType.Integer)
            {
                UnityEditor.EditorGUI.LabelField(position, label.text, "Use NavMeshArea only on int!");
                return;
            }

            Init();

            var lineHeight = UnityEditor.EditorGUIUtility.singleLineHeight;
            float labelWidth = UnityEditor.EditorGUIUtility.labelWidth;
            float buttonWidth = position.width - labelWidth - UnityEditor.EditorGUIUtility.standardVerticalSpacing;
            var labelRect = new Rect(position.x, position.y, labelWidth, lineHeight);
            var buttonRect = new Rect(position.x + labelWidth + UnityEditor.EditorGUIUtility.standardVerticalSpacing, position.y, buttonWidth, lineHeight);

            UnityEditor.EditorGUI.BeginProperty(position, label, property);

            int currentAreaIndex = property.intValue;
            var checkResult = DummyCheck(currentAreaIndex);
            if (checkResult != currentAreaIndex)
            {
                currentAreaIndex = checkResult;
                property.intValue = currentAreaIndex;
            }
            var displayName = GetFullName(currentAreaIndex);
            UnityEditor.EditorGUI.LabelField(labelRect, label);
            if (GUI.Button(buttonRect, new GUIContent(displayName, UnityEditor.EditorGUIUtility.IconContent("overlays/d_searchoverlay@2x").image)))
            {
                UnityEditor.GenericMenu menu = new();
                for (var i = 0; i < names.Length; i++)
                {
                    var name = names[i];
                    var value = areaIndices[i];
                    var on = (currentAreaIndex & value) != 0;
                    var valueIfTrue = currentAreaIndex & ~value;
                    var valueIfFalse = currentAreaIndex | value;
                    menu.AddItem(
                        new(name),
                        on,
                        () =>
                        {
                            SetNewValue(property, on ? valueIfTrue : valueIfFalse);
                            GUI.changed = true;
                        });
                }
                menu.AddSeparator("");
                menu.AddItem(new("Nothing"), false, () => SetNewValue(property, 0));
                menu.AddItem(new("Everything"), false, () => AddAll(property));
                menu.ShowAsContext();
            }
            UnityEditor.EditorGUI.EndProperty();
        }

        void AddAll(UnityEditor.SerializedProperty property)
        {
            var newResult = 0;
            for (var i = 0; i < names.Length; i++)
            {
                var value = areaIndices[i];
                newResult |= value;
            }
            SetNewValue(property, newResult);
        }

        int DummyCheck(int currentAreaIndex)
        {
            var newResult = 0;
            for (var i = 0; i < names.Length; i++)
            {
                var value = areaIndices[i];
                var on = (currentAreaIndex & value) != 0;
                if (on)
                {
                    newResult |= value;
                }
            }
            return newResult;
        }
        string GetFullName(int currentAreaIndex)
        {
            System.Collections.Generic.List<string> allNames = new();
            for (var i = 0; i < names.Length; i++)
            {
                var value = areaIndices[i];
                var on = (currentAreaIndex & value) != 0;
                if (on)
                {
                    allNames.Add(names[i]);
                }
            }
            switch (allNames.Count)
            {
                case 0:
                    return "None";
                case > MAX_DISPLAY_NAMES:
                    return "Mixed";
            }
            System.Text.StringBuilder sb = new();
            sb.AppendJoin(',', allNames);
            return sb.ToString();
        }
        void SetNewValue(UnityEditor.SerializedProperty property, int newValue)
        {
            property.intValue = newValue;
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif