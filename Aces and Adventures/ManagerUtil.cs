using System;
using UnityEngine;

public class ManagerUtil : MonoBehaviour
{
	private static ManagerUtil Instance;

	static ManagerUtil()
	{
		UnityEngine.Object.DontDestroyOnLoad(Instance = new GameObject("ManagerUtil").AddComponent<ManagerUtil>());
	}

	public static T GetSingletonInstance<T>(ref T instance, bool createSeparateGameObject = false, Action<T> initialize = null, bool dontDestroyOnLoad = true) where T : Component
	{
		if ((bool)(UnityEngine.Object)instance)
		{
			return instance;
		}
		if (!Application.isPlaying || !Instance)
		{
			return null;
		}
		instance = (createSeparateGameObject ? new GameObject(typeof(T).Name) : Instance.gameObject).AddComponent<T>();
		initialize?.Invoke(instance);
		if (dontDestroyOnLoad)
		{
			UnityEngine.Object.DontDestroyOnLoad(instance.gameObject);
		}
		return instance;
	}
}
