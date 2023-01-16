using UnityEngine;

namespace Utils.Common.Singletons
{
	/// <summary>
	/// Generic Singleton for MonoBehaviour
	/// </summary>
	public abstract class MonoBehaviourSingletonPersistent<T> : MonoBehaviour where T : Component
	{
		public static T Instance { get; protected set; }

		protected virtual void Awake()
		{
			if (Instance == null)
			{
				Instance = this as T;
				transform.SetParent(null);
				DontDestroyOnLoad(this);
			}
			else
			{
				Destroy(this);
			}
		}
	}
}