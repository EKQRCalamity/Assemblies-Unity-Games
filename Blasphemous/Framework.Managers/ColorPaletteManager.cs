using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FullSerializer;
using Gameplay.GameControllers.Effects.Player.Recolor;
using Gameplay.UI;
using Tools;
using UnityEngine;

namespace Framework.Managers;

public class ColorPaletteManager : GameSystem
{
	private const string PALETTE_PATH = "Color Palettes/AVAILABLE_COLOR_PALETTES";

	private const string DEFAULT_SKIN = "PENITENT_DEFAULT";

	private const string BACKER_SKIN = "PENITENT_BACKER";

	private const string DELUXE_SKIN = "PENITENT_DELUXE";

	private const string ALMS_SKIN = "PENITENT_ALMS";

	private const string BOSSRUSH_SKIN = "PENITENT_BOSSRUSH";

	private const string BOSSRUSH_S_SKIN = "PENITENT_BOSSRUSH_S";

	private const string KONAMI_SKIN = "PENITENT_KONAMI";

	public const string DEMAKE1_SKIN = "PENITENT_DEMAKE";

	public const string DEMAKE2_SKIN = "PENITENT_GAMEBOY";

	private const string CURRENT_SKIN_KEY = "CURRENT_SKIN";

	private const string SKIN_SETTINGS_PATH = "/app_settings";

	private const string BACKER_DLC_NAME = "BACKER_DLC";

	private const string DELUXE_DLC_NAME = "DIGITAL_DELUXE_DLC";

	private string currentColorPaletteId = string.Empty;

	private Dictionary<string, bool> dlcPalettes;

	private List<string> dlcPalettesIds;

	private List<string> dlcs;

	private ColorPaletteDictionary palettes;

	private Dictionary<string, string> palettesByDlc;

	private Dictionary<string, bool> palettesStates;

	private void InitializeDlcPalettes()
	{
		palettesByDlc = new Dictionary<string, string>();
		palettesByDlc.Add("BACKER_DLC", "PENITENT_BACKER");
		palettesByDlc.Add("DIGITAL_DELUXE_DLC", "PENITENT_DELUXE");
		dlcPalettesIds = new List<string> { "PENITENT_BACKER", "PENITENT_DELUXE" };
		dlcs = new List<string> { "BACKER_DLC", "DIGITAL_DELUXE_DLC" };
		dlcPalettes = new Dictionary<string, bool>();
		foreach (string dlc in dlcs)
		{
			bool value = Core.DLCManager.IsDLCDownloaded(dlc);
			dlcPalettes.Add(palettesByDlc[dlc], value);
		}
	}

	public override void Initialize()
	{
		string pathSkinSettings = GetPathSkinSettings();
		InitializeDlcPalettes();
		palettes = Resources.Load<ColorPaletteDictionary>("Color Palettes/AVAILABLE_COLOR_PALETTES");
		if (!File.Exists(pathSkinSettings) || FileIsEmpty(pathSkinSettings))
		{
			File.CreateText(pathSkinSettings).Close();
			currentColorPaletteId = "PENITENT_DEFAULT";
			palettesStates = new Dictionary<string, bool>();
			palettesStates.Add("PENITENT_DEFAULT", value: true);
			SetCurrentSkinToSkinSettings("PENITENT_DEFAULT");
		}
		else
		{
			Dictionary<string, fsData> skinSettings = ParseCurrentSkinSettings(pathSkinSettings);
			palettesStates = GetAllPalettesStatesFromDictionary(skinSettings);
			currentColorPaletteId = GetCurrentSkinFromDictionary(skinSettings);
		}
	}

	private string GetPathSkinSettings()
	{
		return PersistentManager.GetPathAppSettings("/app_settings");
	}

	private bool FileIsEmpty(string path)
	{
		string s = File.ReadAllText(path);
		try
		{
			byte[] bytes = Convert.FromBase64String(s);
			string @string = Encoding.UTF8.GetString(bytes);
			fsData data;
			fsResult fsResult = fsJsonParser.Parse(@string, out data);
			if (fsResult.Failed && !fsResult.FormattedMessages.Equals("No input"))
			{
				Debug.LogError("ParseCurrentSkinSettings: parsing error: " + fsResult.FormattedMessages);
				return true;
			}
			if (data != null)
			{
				Dictionary<string, fsData> asDictionary = data.AsDictionary;
				if (asDictionary.Keys.Count > 0)
				{
					return false;
				}
			}
			return true;
		}
		catch (Exception ex)
		{
			Debug.Log("Something went wrong! File is not empty but probably not base64. >> Exception: " + ex.Message);
			return true;
		}
	}

	private Dictionary<string, fsData> ParseCurrentSkinSettings(string path)
	{
		Dictionary<string, fsData> result = new Dictionary<string, fsData>();
		if (File.Exists(path))
		{
			string s = File.ReadAllText(path);
			byte[] bytes = Convert.FromBase64String(s);
			string @string = Encoding.UTF8.GetString(bytes);
			fsData data;
			fsResult fsResult = fsJsonParser.Parse(@string, out data);
			if (fsResult.Failed && !fsResult.FormattedMessages.Equals("No input"))
			{
				Debug.LogError("ParseCurrentSkinSettings: parsing error: " + fsResult.FormattedMessages);
			}
			else if (data != null)
			{
				result = data.AsDictionary;
				result = CleanOldSaveFileFormat(result);
			}
		}
		return result;
	}

	private Dictionary<string, fsData> CleanOldSaveFileFormat(Dictionary<string, fsData> skinSettings)
	{
		foreach (string dlcPalettesId in dlcPalettesIds)
		{
			if (skinSettings.ContainsKey(dlcPalettesId))
			{
				skinSettings.Remove(dlcPalettesId);
			}
			string key = dlcPalettesId + "_UNLOCKED";
			if (skinSettings.ContainsKey(key))
			{
				skinSettings.Remove(key);
			}
		}
		return skinSettings;
	}

	private string GetCurrentSkinFromDictionary(Dictionary<string, fsData> skinSettings)
	{
		if (skinSettings.ContainsKey("CURRENT_SKIN"))
		{
			string asString = skinSettings["CURRENT_SKIN"].AsString;
			if (IsColorPaletteUnlocked(asString))
			{
				return asString;
			}
		}
		return "PENITENT_DEFAULT";
	}

	private Dictionary<string, bool> GetAllPalettesStatesFromDictionary(Dictionary<string, fsData> skinSettings)
	{
		Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
		dictionary.Add("PENITENT_DEFAULT", value: true);
		foreach (string key in skinSettings.Keys)
		{
			if (key.EndsWith("_UNLOCKED") && !key.Equals("PENITENT_DEFAULT_UNLOCKED"))
			{
				dictionary.Add(key.Replace("_UNLOCKED", string.Empty), skinSettings[key].AsBool);
			}
		}
		return dictionary;
	}

	public void InitializeSkinFile(string currentSkin)
	{
		string pathSkinSettings = GetPathSkinSettings();
		if (!File.Exists(pathSkinSettings))
		{
			File.CreateText(pathSkinSettings);
		}
		fsData fsData = fsData.CreateDictionary();
		foreach (string allId in palettes.GetAllIds())
		{
			fsData.AsDictionary[allId] = ((!(currentSkin == allId)) ? fsData.False : fsData.True);
			fsData.AsDictionary[allId + "_UNLOCKED"] = ((!IsColorPaletteUnlocked(allId)) ? fsData.False : fsData.True);
		}
		fsData.AsDictionary["CURRENT_SKIN"] = new fsData(currentSkin);
		string s = fsJsonPrinter.CompressedJson(fsData);
		byte[] bytes = Encoding.UTF8.GetBytes(s);
		string encryptedData = Convert.ToBase64String(bytes);
		FileTools.SaveSecure(pathSkinSettings, encryptedData);
	}

	public void SetCurrentSkinToSkinSettings(string currentSkin)
	{
		InitializeSkinFile(currentSkin);
		SetCurrentColorPaletteId(currentSkin);
	}

	public void UnlockColorPalette(string colorPaletteId, bool showPopup = true)
	{
		if (!palettesStates.ContainsKey(colorPaletteId))
		{
			palettesStates.Add(colorPaletteId, value: false);
		}
		if (!palettesStates[colorPaletteId])
		{
			palettesStates[colorPaletteId] = true;
			if (showPopup)
			{
				UIController.instance.ShowUnlockPopup(colorPaletteId);
			}
			SetCurrentSkinToSkinSettings(currentColorPaletteId);
		}
	}

	public void LockColorPalette(string colorPaletteId)
	{
		if (palettesStates.ContainsKey(colorPaletteId))
		{
			palettesStates[colorPaletteId] = false;
			if (currentColorPaletteId == colorPaletteId)
			{
				SetCurrentSkinToSkinSettings("PENITENT_DEFAULT");
			}
		}
	}

	public void UnlockAlmsColorPalette()
	{
		UnlockColorPalette("PENITENT_ALMS");
	}

	public void UnlockBossRushColorPalette()
	{
		UnlockColorPalette("PENITENT_BOSSRUSH");
	}

	public void UnlockBossRushRankSColorPalette()
	{
		UnlockColorPalette("PENITENT_BOSSRUSH_S");
	}

	public void UnlockBossKonamiColorPalette()
	{
		UnlockColorPalette("PENITENT_KONAMI");
	}

	public void UnlockDemake1ColorPalette()
	{
		UnlockColorPalette("PENITENT_DEMAKE");
	}

	public void UnlockDemake2ColorPalette()
	{
		UnlockColorPalette("PENITENT_GAMEBOY");
	}

	public bool IsColorPaletteUnlocked(string colorPaletteId)
	{
		if (colorPaletteId.Equals("PENITENT_DEFAULT"))
		{
			return true;
		}
		if (dlcPalettesIds.Contains(colorPaletteId))
		{
			return dlcPalettes[colorPaletteId];
		}
		if (palettesStates != null && palettesStates.ContainsKey(colorPaletteId))
		{
			return palettesStates[colorPaletteId];
		}
		return false;
	}

	public List<string> GetAllUnlockedColorPalettesId()
	{
		List<string> list = new List<string>();
		foreach (string key in palettesStates.Keys)
		{
			if (palettesStates[key])
			{
				list.Add(key);
			}
		}
		foreach (string dlcPalettesId in dlcPalettesIds)
		{
			if (dlcPalettes[dlcPalettesId])
			{
				list.Add(dlcPalettesId);
			}
		}
		return list;
	}

	public List<string> GetAllColorPalettesId()
	{
		return palettes.GetAllIds();
	}

	public string GetCurrentColorPaletteId()
	{
		return currentColorPaletteId;
	}

	public Sprite GetCurrentColorPaletteSprite()
	{
		return palettes.GetPalette(currentColorPaletteId);
	}

	public Sprite GetPaletteSpritePreview(string id)
	{
		return palettes.GetPreview(id);
	}

	public Sprite GetColorPaletteById(string id)
	{
		return palettes.GetPalette(id);
	}

	public void SetCurrentColorPaletteId(string colorPaletteId)
	{
		currentColorPaletteId = colorPaletteId;
	}
}
