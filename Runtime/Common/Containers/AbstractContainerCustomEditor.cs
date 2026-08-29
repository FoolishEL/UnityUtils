#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Foolish.Utils.Containers.Editor
{
    public abstract class AbstractContainerCustomEditor<T, TV> : UnityEditor.Editor, IRebuilder where T : AbstractContainer<TV> where TV : ScriptableObject
    {
        const string CONTAINER_NAME = "containedData";
        
        private Type[] allTypes;

        private VisualElement root;
        SerializedProperty containedDataProperty;
        
        protected abstract bool AllowSameContainersType { get; }

        private void OnEnable()
        {
            RefreshSerializedProperty();
        }

        private void RefreshSerializedProperty()
        {
            serializedObject.Update();
            containedDataProperty = serializedObject.FindProperty(CONTAINER_NAME);
        }

        public override VisualElement CreateInspectorGUI()
        {
            root = new();

            InitTypes();
            BuildUI();

            return root;
        }

        private void BuildUI()
        {
            root.Clear();

            root.style.paddingLeft = 4;
            root.style.paddingRight = 4;
            root.style.paddingTop = 4;
            root.style.paddingBottom = 4;

            serializedObject.Update();

            var iterator = serializedObject.GetIterator();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                if (iterator.name != CONTAINER_NAME)
                {
                    var field = new PropertyField(iterator);
                    // Unity's script reference is informational. All user-authored
                    // serialized properties must remain editable in this inspector.
                    field.SetEnabled(iterator.name != "m_Script");
                    root.Add(field);
                }
                enterChildren = false;
            }

            root.Add(CreateSubDataList());

            var createButton = new Button(ShowTypeMenu)
            {
                text = "Add Element",
                tooltip = "Create a new sub-asset and add it to this container",
            };

            createButton.style.height = 28;
            createButton.style.marginTop = 6;
            createButton.style.unityFontStyleAndWeight = FontStyle.Bold;

            root.Add(createButton);

            serializedObject.ApplyModifiedProperties();
        }

        private void InitTypes()
        {
            var baseType = typeof(TV);

            allTypes = TypeCache.GetTypesDerivedFrom<TV>()
                .Where(t => t != baseType && isValidType(t))
                .ToArray();
            
            if (isValidType(baseType))
            {
                allTypes = allTypes.Append(baseType).ToArray();
            }

            bool isValidType(Type type) => !type.IsAbstract && !type.ContainsGenericParameters;
        }

        private VisualElement CreateSubDataList()
        {
            var container = new VisualElement();

            var header = new Label("Elements")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 13,
                    marginTop = 8,
                    marginBottom = 4,
                },
            };
            container.Add(header);

            RefreshSerializedProperty();

            if (containedDataProperty is not { isArray: true, } ||
                containedDataProperty.arraySize == 0)
            {
                container.Add(new HelpBox("No sub-data created yet", HelpBoxMessageType.Info));
                return container;
            }

            for (int i = 0; i < containedDataProperty.arraySize; i++)
            {
                var elementProp = containedDataProperty.GetArrayElementAtIndex(i);
                var item = elementProp.objectReferenceValue as TV;

                if (item == null) continue;

                container.Add(CreateItemElement(item));
            }

            return container;
        }

        private VisualElement CreateItemElement(TV item)
        {
            var box = new VisualElement();
            ApplyBoxStyle(box);

            var foldout = new Foldout
            {
                text = item.name,
                value = true,
                style =
                {
                    marginLeft = 5,
                },
            };

            var label = foldout.Q<Label>();
            label.style.unityFontStyleAndWeight = FontStyle.Bold;

            var renameBtn = new Button(() => StartRename(item))
            {
                text = "Rename",
                tooltip = $"Rename {item.name}",
                style = { marginLeft = 4, },
            };
            var deleteBtn = new Button(() => DeleteItem(item))
            {
                text = "Delete",
                tooltip = $"Delete {item.name}",
                style = { marginLeft = 2, },
            };


            label.parent.Add(renameBtn);
            label.parent.Add(deleteBtn);

            var inspector = new InspectorElement(item)
            {
                style = { marginLeft = 0, },
            };
            foldout.Add(inspector);

            box.Add(foldout);

            return box;
        }

        private void StartRename(TV item)
        {
            var renameInspector = CreateInstance<EditorWindowRename>();
            renameInspector.Initialize(item,this);
            renameInspector.ShowUtility();
            
        }

        private void DeleteItem(TV item)
        {
            if (!EditorUtility.DisplayDialog("Delete", $"Delete {item.name}?", "Yes", "No"))
                return;

            serializedObject.Update();

            for (int i = containedDataProperty.arraySize - 1; i >= 0; i--)
            {
                var element = containedDataProperty.GetArrayElementAtIndex(i);

                if (element.objectReferenceValue == item)
                {
                    containedDataProperty.DeleteArrayElementAtIndex(i);
                    break;
                }
            }

            serializedObject.ApplyModifiedProperties();

            DestroyImmediate(item, true);

            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();

            BuildUI();
        }

        private void ShowTypeMenu()
        {
            if (AllowSameContainersType)
                ShowTypeMenuAll();
            else
                ShowTypeMenuOncePerType();
        }

        private void ShowTypeMenuAll()
        {
            var menu = new GenericMenu();
            foreach (var type in allTypes)
                menu.AddItem(new(type.Name), false, () => CreateSubData(type));
            menu.ShowAsContext();
        }

        private void ShowTypeMenuOncePerType()
        {
            var menu = new GenericMenu();
            var existingTypes = new HashSet<Type>();
            for (int i = 0; i < containedDataProperty.arraySize; i++)
            {
                var element = containedDataProperty.GetArrayElementAtIndex(i);
                if (element.objectReferenceValue is TV sp)
                    existingTypes.Add(sp.GetType());
            }
            foreach (var type in allTypes)
            {
                if (existingTypes.Contains(type))
                    continue;
                menu.AddItem(new(type.Name), false,
                    () => CreateSubData(type));
            }

            menu.ShowAsContext();
        }

        private void CreateSubData(Type type)
        {
            var mainData = (T)target;

            var newData = (TV)CreateInstance(type);
            newData.name = $"{type.Name}_{Guid.NewGuid().ToString("N").Substring(0, 4)}";

            AssetDatabase.AddObjectToAsset(newData, mainData);
            EditorUtility.SetDirty(newData);

            serializedObject.Update();
            int index = containedDataProperty.arraySize;
            containedDataProperty.arraySize++;

            containedDataProperty.GetArrayElementAtIndex(index).objectReferenceValue = newData;

            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(mainData);
            AssetDatabase.SaveAssets();

            BuildUI();
        }

        void IRebuilder.Rebuild() => BuildUI();

        void ApplyBoxStyle(VisualElement ve)
        {
            var borderWidth = 1;
#if UNITY_6000_0_OR_NEWER
            var borderColor = Color.gray6;
#else
            var borderColor = Color.grey;
#endif
            var paddings = 6;
            var margins = 3;

            ve.style.marginRight = margins;
            ve.style.marginTop = margins;
            ve.style.marginBottom = margins;

            ve.style.borderTopWidth = borderWidth;
            ve.style.borderBottomWidth = borderWidth;
            ve.style.borderLeftWidth = borderWidth;
            ve.style.borderRightWidth = borderWidth;
            ve.style.borderTopColor = borderColor;
            ve.style.borderBottomColor = borderColor;
            ve.style.borderLeftColor = borderColor;
            ve.style.borderRightColor = borderColor;

            ve.style.paddingLeft = paddings;
            ve.style.paddingRight = paddings;
            ve.style.paddingTop = paddings;
            ve.style.paddingBottom = paddings;

        }
    }
    
    public interface IRebuilder
    {
        void Rebuild();
    }
}
#endif
