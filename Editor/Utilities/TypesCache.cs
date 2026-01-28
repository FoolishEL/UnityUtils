#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Foolish.Utils.Editor
{
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
                .Where(baseType.IsAssignableFrom)
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
}
#endif