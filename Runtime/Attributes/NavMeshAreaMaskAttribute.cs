#if UNITY_AI_NAVIGATION
using System;
using UnityEngine;

namespace Foolish.Utils
{

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class NavMeshAreaMaskAttribute : PropertyAttribute
    {
    }

#if UNITY_EDITOR

    [UnityEditor.CustomPropertyDrawer(typeof(NavMeshAreaMaskAttribute))]
    public class NavMeshAreaMaskDrawer : UnityEditor.PropertyDrawer
    {
        public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != UnityEditor.SerializedPropertyType.Integer)
            {
                UnityEditor.EditorGUI.LabelField(position, label.text, "Use NavMeshAreaMask only on int!");
                return;
            }
            UnityEditor.EditorGUI.BeginProperty(position, label, property);

            var mask = property.intValue;
#if UNITY_2023_OR_NEWER
            var names = UnityEngine.AI.NavMesh.GetAreaNames();
#else
            var names = UnityEditor.GameObjectUtility.GetNavMeshAreaNames();
#endif

            var newMask = UnityEditor.EditorGUI.MaskField(position, label, mask, names);

            if (newMask != mask)
            {
                property.intValue = newMask;
            }

            UnityEditor.EditorGUI.EndProperty();
        }
    }

#endif
}
#endif