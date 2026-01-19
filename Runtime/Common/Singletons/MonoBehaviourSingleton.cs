using UnityEngine;

namespace Foolish.Utils.Common.Singletons
{
    /// <summary>
    /// Generic temporary Singleton for MonoBehaviour
    /// </summary>
    public abstract class MonoBehaviourSingleton<T> : MonoBehaviour where T : Component
    {
        public static T Instance { get; protected set; }

        [SerializeField] private bool needUnparentSelf = true;

        protected virtual void Awake()
        {
            if (Instance != null)
                Destroy(Instance);
            Instance = this as T;
            if (needUnparentSelf)
                transform.parent = null;
            Init();
        }

        protected virtual void Init()
        {
        }
    }
}