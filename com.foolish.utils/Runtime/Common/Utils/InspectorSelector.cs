using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Foolish.Utils
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class InspectorSelector : PropertyAttribute
	{
		public Type AbstractType { get; }

		public InspectorSelector(Type abstractType) => AbstractType = abstractType;
	}

#if UNITY_EDITOR
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

			var derivedTypes = TypesCache.GetDerivedTypes(abstractType).Where(t => !t.IsAbstract
				&& !t.IsInterface
				&& !t.IsGenericTypeDefinition
				&& !t.ContainsGenericParameters).ToArray();
			var typeNames = derivedTypes
				.Select(t => string.IsNullOrEmpty(t.FullName) ? t.Name : t.FullName.Replace('.', '/'))
				.ToArray();

			var currentValue = property.managedReferenceValue;
			var currentIndex = currentValue != null
				? Array.IndexOf(derivedTypes, currentValue.GetType())
				: -1;
			var fieldName = label.text;
			if (typeNames.Length == 0)
			{
				typeNames = new[]
				{
					"None"
				};
				currentIndex = 0;
			}
			else
			{
				if (currentIndex >= 0)
				{
					fieldName = typeNames[currentIndex];
					var lastIndex = fieldName.LastIndexOf('/');
					if (lastIndex != -1)
					{
						fieldName = fieldName.Substring(lastIndex + 1);
					}
				}
			}
			var newIndex = EditorGUI.Popup(
				new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
				fieldName,
				currentIndex,
				typeNames);

			if (newIndex != currentIndex)
			{
				var isNone = derivedTypes.Length == 0;
				if (newIndex >= 0 && newIndex < derivedTypes.Length && !isNone)
				{
					var selectedType = derivedTypes[newIndex];
					property.managedReferenceValue = Activator.CreateInstance(selectedType);
				}
				else
				{
					property.managedReferenceValue = null;
				}
			}

			if (currentValue != null || newIndex >= 0)
			{
				var rect = new Rect(position.x,
					position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
					position.width,
					position.height - EditorGUIUtility.standardVerticalSpacing * 2);
				if(property.isExpanded)
					DrawUIBox(rect, Color.Lerp(Color.white, Color.black, .6f), Color.Lerp(Color.black, Color.gray, .5f));
				rect.width -= EditorGUIUtility.standardVerticalSpacing * 2;
				EditorGUI.PropertyField(
					rect,
					property,
					GUIContent.none,
					true);
				if (property.isExpanded)
				{
					EditorGUILayout.Space(18);
				}
			}
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

			return height;
		}
	}

	public static class TypesCache
	{
		private static Dictionary<Type, Type[]> cache = new();
		private static bool initialized;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Initialize()
		{
			cache.Clear();
			initialized = false;
			AppDomain.CurrentDomain.DomainUnload += OnDomainUnload;
		}

		private static void OnDomainUnload(object sender, EventArgs e)
		{
			cache.Clear();
			initialized = false;
			AppDomain.CurrentDomain.DomainUnload -= OnDomainUnload;
		}

		public static Type[] GetDerivedTypes(Type baseType)
		{
			if (!initialized)
			{
				RefreshAllTypes();
				initialized = true;
			}

			if (cache.TryGetValue(baseType, out var types))
			{
				return types;
			}

			var derivedTypes = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly => assembly.GetTypes())
				.Where(t => !t.IsAbstract && !t.IsInterface && baseType.IsAssignableFrom(t))
				.OrderBy(t => t.FullName)
				.ToArray();

			cache[baseType] = derivedTypes;
			return derivedTypes;
		}

		private static void RefreshAllTypes()
		{
			cache.Clear();
		}
	}

#endif
}
