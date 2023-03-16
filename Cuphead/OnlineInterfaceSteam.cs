using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Steamworks;
using UnityEngine;

public class OnlineInterfaceSteam : OnlineInterface
{
	private SteamManager steamManager;

	private string SavePath
	{
		get
		{
			if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
			{
				return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Cuphead\\");
			}
			if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
			{
				return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Library/Application Support/unity.Studio MDHR.Cuphead/Cuphead/");
			}
			return string.Empty;
		}
	}

	public OnlineUser MainUser => null;

	public OnlineUser SecondaryUser => null;

	public bool CloudStorageInitialized => true;

	public bool SupportsMultipleUsers => false;

	public bool SupportsUserSignIn => false;

	public event SignInEventHandler OnUserSignedIn;

	public event SignOutEventHandler OnUserSignedOut;

	public void Init()
	{
		steamManager = new GameObject("SteamManager").AddComponent<SteamManager>();
		steamManager.transform.SetParent(Cuphead.Current.transform);
		if (SteamManager.Initialized)
		{
			SteamUserStats.RequestCurrentStats();
		}
	}

	public void Reset()
	{
	}

	public void SignInUser(bool silent, PlayerId player, ulong controllerId)
	{
		this.OnUserSignedIn(null);
	}

	public void SwitchUser(PlayerId player, ulong controllerId)
	{
	}

	public OnlineUser GetUserForController(ulong id)
	{
		return null;
	}

	public List<ulong> GetControllersForUser(PlayerId player)
	{
		return null;
	}

	public bool IsUserSignedIn(PlayerId player)
	{
		return false;
	}

	public OnlineUser GetUser(PlayerId player)
	{
		return null;
	}

	public void SetUser(PlayerId player, OnlineUser user)
	{
	}

	public Texture2D GetProfilePic(PlayerId player)
	{
		return null;
	}

	public void GetAchievement(PlayerId player, string id, AchievementEventHandler achievementRetrievedHandler)
	{
	}

	public void UnlockAchievement(PlayerId player, string id)
	{
		if (SteamManager.Initialized)
		{
			SteamUserStats.GetAchievement(id, out var pbAchieved);
			if (!pbAchieved)
			{
				SteamUserStats.SetAchievement(id);
				SteamUserStats.StoreStats();
			}
		}
	}

	public void SyncAchievementsAndStats()
	{
		if (SteamManager.Initialized)
		{
			SteamUserStats.StoreStats();
		}
	}

	public void SetStat(PlayerId player, string id, int value)
	{
		if (SteamManager.Initialized)
		{
			SteamUserStats.SetStat(id, value);
		}
	}

	public void SetStat(PlayerId player, string id, float value)
	{
		if (SteamManager.Initialized)
		{
			SteamUserStats.SetStat(id, value);
		}
	}

	public void SetStat(PlayerId player, string id, string value)
	{
	}

	public void IncrementStat(PlayerId player, string id, int value)
	{
		if (SteamManager.Initialized)
		{
			SteamUserStats.GetStat(id, out int pData);
			int num = pData + value;
			SteamUserStats.SetStat(id, num);
			if (id == "Parries" && (num == 20 || num == 100))
			{
				SteamUserStats.StoreStats();
			}
		}
	}

	public void SetRichPresence(PlayerId player, string id, bool active)
	{
	}

	public void SetRichPresenceActive(PlayerId player, bool active)
	{
	}

	public void InitializeCloudStorage(PlayerId player, InitializeCloudStoreHandler handler)
	{
		handler(success: true);
	}

	public void UninitializeCloudStorage()
	{
	}

	public void SaveCloudData(IDictionary<string, string> data, SaveCloudDataHandler handler)
	{
		string savePath = SavePath;
		if (!Directory.Exists(savePath))
		{
			Directory.CreateDirectory(savePath);
		}
		foreach (string key in data.Keys)
		{
			try
			{
				TextWriter textWriter = new StreamWriter(Path.Combine(savePath, key + ".sav"));
				textWriter.Write(data[key]);
				textWriter.Close();
			}
			catch
			{
				Cuphead.Current.StartCoroutine(saveFailed_cr(handler));
				return;
			}
		}
		handler(success: true);
	}

	private IEnumerator saveFailed_cr(SaveCloudDataHandler handler)
	{
		yield return new WaitForSeconds(0.25f);
		handler(success: false);
	}

	public void LoadCloudData(string[] keys, LoadCloudDataHandler handler)
	{
		string[] array = new string[keys.Length];
		string savePath = SavePath;
		for (int i = 0; i < array.Length; i++)
		{
			string path = Path.Combine(savePath, keys[i] + ".sav");
			if (File.Exists(path))
			{
				try
				{
					TextReader textReader = new StreamReader(Path.Combine(savePath, keys[i] + ".sav"));
					array[i] = textReader.ReadToEnd();
					textReader.Close();
				}
				catch
				{
					handler(array, CloudLoadResult.Failed);
				}
			}
			else
			{
				handler(array, CloudLoadResult.NoData);
			}
		}
		handler(array, CloudLoadResult.Success);
	}

	public void UpdateControllerMapping()
	{
	}

	public bool ControllerMappingChanged()
	{
		return false;
	}
}
