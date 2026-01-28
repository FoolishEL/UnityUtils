#if UNITY_EDITOR && UNITY_AI_NAVIGATION
using UnityEngine;
using UnityEngine.AI;

namespace Foolish.Utils.Editor
{
    [UnityEditor.CustomPropertyDrawer(typeof(NavMeshAgentTypeAttribute))]
    public class NavMeshAgentTypeDrawer : UnityEditor.PropertyDrawer
    {
        public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != UnityEditor.SerializedPropertyType.Integer)
            {
                UnityEditor.EditorGUI.LabelField(position, label.text, "Use NavMeshAreaMask only on int!");
                return;
            }
            UnityEditor.EditorGUI.BeginProperty(position, label, property);

            var currentId = property.intValue;
            var indexes = GetIndexes();
            GUIContent[] names = new GUIContent[indexes.Length];
            for (var i = 0; i < indexes.Length; i++)
            {
                names[i] = new(NavMesh.GetSettingsNameFromID(indexes[i]));
            }
            var newValue = UnityEditor.EditorGUI.IntPopup(position, label, currentId, names, indexes);

            if (newValue != currentId)
            {
                property.intValue = newValue;
            }

            UnityEditor.EditorGUI.EndProperty();
        }

        private int[] GetIndexes()
        {
            int count = NavMesh.GetSettingsCount();
            int[] ids = new int[count];
            for (int i = 0; i < count; i++)
            {
                NavMeshBuildSettings settings = NavMesh.GetSettingsByIndex(i);
                ids[i] = settings.agentTypeID;
            }
            return ids;
        }
    }
}
#endif