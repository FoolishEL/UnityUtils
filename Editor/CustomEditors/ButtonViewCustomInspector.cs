using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Foolish.Utils.UI.Editor
{
    using Utils.Editor.Extensions;
    using Editor = UnityEditor.Editor;

    [CustomEditor(typeof(ButtonView))]
    public class ButtonViewCustomInspector : Editor
    {
        private static ButtonView _script;

        private SerializedProperty _targetProperty;
        private static bool _showContent;
        private static GenericMenu _menu;
        private static List<(Type, MonoScript)> _typesWithMono;
        private static MonoScript _baseTypeMono;
        private static FieldInfo _fieldInfo;
        private static bool _isTypesSetsUp = false;

        private void OnEnable()
        {
            _script = (ButtonView)target;
            GetCommonStaticInfo();
            CreatePopupMenu();
            _targetProperty = serializedObject.FindProperty("_buttonHandlers");
            _showContent = EditorPrefs.GetBool(nameof(ButtonViewCustomInspector) + "_" + nameof(_showContent), false);
        }

        private void OnDisable()
        {
            EditorPrefs.SetBool(nameof(ButtonViewCustomInspector) + "_" + nameof(_showContent), _showContent);
        }

        public override void OnInspectorGUI()
        {
            this.DrawScriptInstance();
            serializedObject.Update();
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.PropertyField(serializedObject.FindProperty("button"));
            if (!(_targetProperty is null) && _targetProperty.isArray)
            {
                _showContent = EditorGUILayout.BeginFoldoutHeaderGroup(_showContent, new GUIContent("Button Handlers"));
                EditorGUILayout.EndFoldoutHeaderGroup();
                if (_showContent)
                    using (new EditorGUILayout.VerticalScope())
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
                                        if (GUILayout.Button(
                                                new GUIContent(EditorGUIUtility.FindTexture("Toolbar Minus")),
                                                GUILayout.Width(20)))
                                            elementToDelete = i;
                                    }
                                }
                            }

                        DrawPlusButton();
                        if (elementToDelete != -1)
                            _targetProperty.DeleteArrayElementAtIndex(elementToDelete);
                    }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void HandleScriptButton(int id)
        {
            var serializedPropertyElement = _targetProperty.GetArrayElementAtIndex(id);
            (Type, MonoScript) currentElement = default;
            //TODO: create solution for elder Unity Versions:
            //REASON: no getter in managedReferenceValue
            #if UNITY_2021_1_OR_NEWER
            try
            {
                currentElement = _typesWithMono.First(c =>
                    c.Item1 == serializedPropertyElement.managedReferenceValue.GetType());
                
            }
            catch
            {
            #endif
                currentElement = (_typesWithMono[id].Item1, _baseTypeMono);
#if UNITY_2021_1_OR_NEWER
            }
            finally
            {
#endif
                if (GUILayout.Button(
                        new GUIContent(EditorGUIUtility.FindTexture(currentElement.Item2 is {}
                                ? "cs Script Icon"
                                : "console.warnicon"),
                            currentElement.Item2 is {} ? "Ping script" : "MonoScript with this class not found!"),
                        GUILayout.Width(25), GUILayout.Height(20)))
                {
                    EditorGUIUtility.PingObject(currentElement.Item2 ?? _baseTypeMono);
                }
#if UNITY_2021_1_OR_NEWER
            }
#endif
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

        private static void CreatePopupMenu()
        {
            if (_isTypesSetsUp)
                return;

            _typesWithMono ??= new List<(Type, MonoScript)>();
            _typesWithMono.Clear();
            _menu = new GenericMenu();
            var monoScripts = new List<MonoScript>();
            monoScripts.AddRange(MonoImporter.GetAllRuntimeMonoScripts());
            _baseTypeMono = monoScripts.First(c => c.GetClass() == typeof(AbstractButtonHandler));
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                foreach (Type type in
                         assembly.GetTypes()
                             .Where(myType =>
                                 myType.IsClass && !myType.IsAbstract &&
                                 myType.IsSubclassOf(typeof(AbstractButtonHandler))))
                {
                    _typesWithMono.Add((type, monoScripts.FirstOrDefault(c => c.GetClass() == type)));
                }
            }

            for (int i = 0; i < _typesWithMono.Count; i++)
            {
                _menu.AddItem(new GUIContent(_typesWithMono.ElementAt(i).Item1.Name), false, HandlePopupMenuSelection,
                    i);
            }

            _isTypesSetsUp = true;
        }

        private static void HandlePopupMenuSelection(object parameter)
        {
            int id = (int)parameter;
            List<AbstractButtonHandler> filedData = (List<AbstractButtonHandler>)_fieldInfo.GetValue(_script);
            filedData.Add((AbstractButtonHandler)Activator.CreateInstance(_typesWithMono.ElementAt(id).Item1));
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            _isTypesSetsUp = false;
            CreatePopupMenu();
        }

        private static void GetCommonStaticInfo()
        {
            _fieldInfo = _script.GetType()
                .GetField("_buttonHandlers", BindingFlags.Instance | BindingFlags.NonPublic);
        }
    }
}