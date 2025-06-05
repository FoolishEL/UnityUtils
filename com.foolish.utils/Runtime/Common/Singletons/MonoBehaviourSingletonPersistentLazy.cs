using UnityEngine;

namespace Foolish.Utils.Common.Singletons
{

    /// <summary>
    /// Generic Lazy Singleton for MonoBehaviour
    /// </summary>
    public abstract class MonoBehaviourSingletonPersistentLazy<T> : MonoBehaviour where T : Component
    {
        protected static T instance = null;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameObject(typeof(T).Name).AddComponent<T>();
                    DontDestroyOnLoad(instance);
                    (instance as MonoBehaviourSingletonPersistentLazy<T>).Init();
                }
                return instance;
            }
        }

        public virtual void Awake()
        {
            if (instance == null)
            {
                transform.parent = null;
                instance = this as T;
                DontDestroyOnLoad(this);
                Init();
            }
            else
            {
                if (instance != this)
                    Destroy(gameObject);
            }
        }
        protected abstract void Init();
    }
}