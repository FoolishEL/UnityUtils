using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Utils.UI.Editor
{
    using Utils.Editor.Extensions;
    using Editor = UnityEditor.Editor;

    [CustomEditor(typeof(ButtonView))]
    public class ButtonViewCustomInspector : Editor
    {
        private ButtonView _script;

        private SerializedProperty _targetProperty;
        private static bool _showContent;
        private GenericMenu _menu;
        private List<(Type,MonoScript)> _typesWithMono;
        private MonoScript _baseTypeMono;
        private FieldInfo _fieldInfo;
        private void OnEnable()
        {
            _script = (ButtonView)target;
            CreatePopupMenu();
            _fieldInfo = _script.GetType()
                .GetField("_buttonHandlers", BindingFlags.Instance | BindingFlags.NonPublic);
            _targetProperty = serializedObject.FindProperty("_buttonHandlers");
        }

        public override void OnInspectorGUI()
        {
            this.DrawScriptInstance();
            serializedObject.Update();
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.PropertyField(serializedObject.FindProperty("button"));
            if (_targetProperty is not null && _targetProperty.isArray)
            {
                _showContent = EditorGUILayout.BeginFoldoutHeaderGroup(_showContent, new GUIContent("Button Handlers"));
                if (_showContent)
                {
                    int elementToDelete = -1;
                    if (_targetProperty.arraySize != 0)
                        using (new EditorGUILayout.VerticalScope())
                        {
                            for (int i = 0; i < _targetProperty.arraySize; i++)
                            {
                                using (new EditorGUILayout.HorizontalScope("box"))
                                {
                                    GUILayout.Space(10f);
                                    DrawPropertyWithType(_targetProperty.GetArrayElementAtIndex(i));
                                    GUILayout.Space(10f);
                                    HandleScriptButton(i);
                                    if (GUILayout.Button(new GUIContent(EditorGUIUtility.FindTexture("Toolbar Minus")),
                                            GUILayout.Width(20)))
                                        elementToDelete = i;
                                }
                            }
                        }


                    DrawPlusButton();

                    if (elementToDelete != -1)
                        _targetProperty.DeleteArrayElementAtIndex(elementToDelete);
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void HandleScriptButton(int id)
        {
            var serializedPropertyElement = _targetProperty.GetArrayElementAtIndex(id);
            var currentElement = _typesWithMono.First(c =>
                c.Item1 == serializedPropertyElement.managedReferenceValue.GetType());
            if (GUILayout.Button(
                    new GUIContent(EditorGUIUtility.FindTexture(currentElement.Item2 is not null 
                            ? "cs Script Icon"
                            : "console.warnicon"),
                        currentElement.Item2 is not null ? "Ping script" : "MonoScript with this class not found!"),
                    GUILayout.Width(25), GUILayout.Height(20)))
            {
                EditorGUIUtility.PingObject(currentElement.Item2 is not null
                    ? currentElement.Item2
                    : _baseTypeMono);
            }
        }

        private void DrawPropertyWithType(SerializedProperty property)
        {
            string propertyName = property.managedReferenceFullTypename;
            int lastPointPosition = propertyName.LastIndexOf('.') + 1;
            propertyName = propertyName.Substring(lastPointPosition,
                propertyName.Length - lastPointPosition);
            EditorGUILayout.PropertyField(property, new GUIContent(propertyName), true);
        }

        private void DrawPlusButton(bool isExpandWidth = false)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var rect = GUILayoutUtility.GetRect(
                    new GUIContent(EditorGUIUtility.FindTexture("Toolbar Plus")), GUIStyle.none);
                if (EditorGUILayout.DropdownButton(new GUIContent(EditorGUIUtility.FindTexture("Toolbar Plus")),
                        FocusType.Passive, isExpandWidth ? GUILayout.ExpandWidth(true) : GUILayout.Width(40)))
                {
                    _menu.DropDown(rect);
                }
            }
        }

        private void CreatePopupMenu()
        {
            _typesWithMono ??= new();
            _typesWithMono.Clear();
            _menu = new GenericMenu();
            var monoScripts = new List<MonoScript>();
            monoScripts.AddRange(MonoImporter.GetAllRuntimeMonoScripts());
            _baseTypeMono = monoScripts.First(c => c.GetClass() == typeof(AbstractButtonHandler));
            foreach (Type type in
                     Assembly.GetAssembly(typeof(AbstractButtonHandler)).GetTypes()
                         .Where(myType =>
                             myType.IsClass && !myType.IsAbstract &&
                             myType.IsSubclassOf(typeof(AbstractButtonHandler))))
            {
                _typesWithMono.Add((type, monoScripts.FirstOrDefault(c => c.GetClass() == type)));
            }

            for (int i = 0; i < _typesWithMono.Count; i++)
            {
                _menu.AddItem(new GUIContent(_typesWithMono.ElementAt(i).Item1.Name), false, HandlePopupMenuSelection, i);
            }
        }

        private void HandlePopupMenuSelection(object parameter)
        {
            int id = (int)parameter;
            List<AbstractButtonHandler> filedData = (List<AbstractButtonHandler>)_fieldInfo.GetValue(_script);
            filedData.Add((AbstractButtonHandler)Activator.CreateInstance(_typesWithMono.ElementAt(id).Item1));
        }
    }
}