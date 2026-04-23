using UnityEngine;

namespace Foolish.Utils.Containers
{
    public abstract class AbstractContainer<T> : ScriptableObject where T : ScriptableObject
    {
        [SerializeField]
        protected T[] containedData;
    }
}