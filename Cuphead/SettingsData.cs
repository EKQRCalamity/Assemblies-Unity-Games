using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SettingsData
{
	public delegate void SettingsDataLoadFromCloudHandler(bool success);

	public const string KEY = "cuphead_settings_data_v1";

	private static SettingsDataLoadFromCloudHandler _loadFromCloudHandler;

	private static SettingsData _data;

	public bool hasBootedUpGame;

	public float overscan;

	public float chromaticAberration;

	public int screenWidth;

	public int screenHeight;

	public int vSyncCount;

	public bool fullScreen;

	public bool forceOriginalTitleScreen;

	public float masterVolume;

	public float sFXVolume;

	public float musicVolume;

	private static bool originalAudioValuesInitialized;

	private static float originalMasterVolume;

	private static float originalsFXVolume;

	private static float originalMusicVolume;

	public bool canVibrate = true;

	public bool rotateControlsWithCamera;

	public int language = -1;

	public bool chromaticAberrationEffect;

	public bool noiseEffect;

	public bool subtleBlurEffect;

	[SerializeField]
	private float brightness;

	public static SettingsData Data
	{
		get
		{
			if (_data == null)
			{
				if (!originalAudioValuesInitialized)
				{
					originalAudioValuesInitialized = true;
					originalMasterVolume = AudioManager.masterVolume;
					originalsFXVolume = AudioManager.sfxOptionsVolume;
					originalMusicVolume = AudioManager.bgmOptionsVolume;
				}
				if (hasKey())
				{
					try
					{
						_data = JsonUtility.FromJson<SettingsData>(PlayerPrefs.GetString("cuphead_settings_data_v1"));
					}
					catch (ArgumentException)
					{
						_data = new SettingsData();
						Save();
					}
				}
				else
				{
					_data = new SettingsData();
					Save();
				}
				if (_data == null)
				{
					return null;
				}
				ApplySettings();
			}
			return _data;
		}
	}

	public bool vintageAudioEnabled
	{
		get
		{
			if (!PlayerData.inGame)
			{
				return false;
			}
			return PlayerData.Data.vintageAudioEnabled;
		}
	}

	public BlurGamma.Filter filter
	{
		get
		{
			if (!PlayerData.inGame)
			{
				return BlurGamma.Filter.None;
			}
			return PlayerData.Data.filter;
		}
	}

	public float Brightness
	{
		get
		{
			ClampBrightness();
			return brightness;
		}
		set
		{
			brightness = value;
			ClampBrightness();
		}
	}

	public static event Action OnSettingsAppliedEvent;

	public SettingsData()
	{
		overscan = 0f;
		chromaticAberration = 1f;
		screenWidth = Screen.currentResolution.width;
		screenHeight = Screen.currentResolution.height;
		fullScreen = Screen.fullScreen;
		vSyncCount = QualitySettings.vSyncCount;
		masterVolume = originalMasterVolume;
		sFXVolume = originalsFXVolume;
		musicVolume = originalMusicVolume;
		hasBootedUpGame = false;
		SetCameraEffectDefaults();
	}

	public static void Save()
	{
		string value = JsonUtility.ToJson(_data);
		PlayerPrefs.SetString("cuphead_settings_data_v1", value);
		PlayerPrefs.Save();
	}

	public static void LoadFromCloud(SettingsDataLoadFromCloudHandler handler)
	{
		_loadFromCloudHandler = handler;
		if (OnlineManager.Instance.Interface.CloudStorageInitialized)
		{
			OnlineManager.Instance.Interface.LoadCloudData(new string[1] { "cuphead_settings_data_v1" }, OnLoadedCloudData);
		}
	}

	public static void SaveToCloud()
	{
		if (OnlineManager.Instance.Interface.CloudStorageInitialized)
		{
			string value = JsonUtility.ToJson(_data);
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary["cuphead_settings_data_v1"] = value;
			OnlineManager.Instance.Interface.SaveCloudData(dictionary, OnSavedCloudData);
		}
	}

	private static void OnSavedCloudData(bool success)
	{
	}

	private static void OnLoadedCloudData(string[] data, CloudLoadResult result)
	{
		if (result == CloudLoadResult.Failed)
		{
			LoadFromCloud(_loadFromCloudHandler);
			return;
		}
		try
		{
			if (result == CloudLoadResult.NoData)
			{
				if (hasKey())
				{
					try
					{
						_data = JsonUtility.FromJson<SettingsData>(PlayerPrefs.GetString("cuphead_settings_data_v1"));
					}
					catch (ArgumentException)
					{
						_data = new SettingsData();
					}
				}
				else
				{
					_data = new SettingsData();
				}
				SaveToCloud();
			}
			else
			{
				_data = JsonUtility.FromJson<SettingsData>(data[0]);
			}
		}
		catch (ArgumentException)
		{
		}
		if (_loadFromCloudHandler != null)
		{
			_loadFromCloudHandler(success: true);
			_loadFromCloudHandler = null;
		}
	}

	public static void Reset()
	{
		_data = new SettingsData();
		Save();
	}

	public static void ApplySettings()
	{
		if (SettingsData.OnSettingsAppliedEvent != null)
		{
			SettingsData.OnSettingsAppliedEvent();
		}
		Save();
	}

	public static void ApplySettingsOnStartup()
	{
		if (Screen.width < 320 || Screen.height < 240)
		{
			Data.screenWidth = 640;
			Data.screenHeight = 480;
			Data.fullScreen = false;
			Screen.SetResolution(Data.screenWidth, Data.screenHeight, Data.fullScreen);
		}
		QualitySettings.vSyncCount = Data.vSyncCount;
		AudioManager.masterVolume = Data.masterVolume;
		AudioManager.sfxOptionsVolume = Data.sFXVolume;
		AudioManager.bgmOptionsVolume = Data.musicVolume;
	}

	private static bool hasKey()
	{
		return PlayerPrefs.HasKey("cuphead_settings_data_v1");
	}

	private void SetCameraEffectDefaults()
	{
		chromaticAberrationEffect = true;
		noiseEffect = true;
		subtleBlurEffect = true;
		brightness = 0f;
	}

	private void ClampBrightness()
	{
		if (brightness < -1f)
		{
			brightness = -1f;
		}
		if (brightness > 1f)
		{
			brightness = 1f;
		}
	}
}
