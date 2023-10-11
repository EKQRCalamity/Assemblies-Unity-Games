using Steamworks;
using UnityEngine;

public class SteamPump : MonoBehaviour
{
	private static SteamPump _Instance;

	private void Awake()
	{
		if ((bool)_Instance)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		_Instance = this;
		Object.DontDestroyOnLoad(base.gameObject);
		Object.DontDestroyOnLoad(this);
	}

	private void Update()
	{
		if (Steam.Enabled)
		{
			SteamAPI.RunCallbacks();
		}
	}

	private void OnDestroy()
	{
		if (_Instance == this && Steam.Enabled)
		{
			SteamAPI.Shutdown();
		}
	}
}
