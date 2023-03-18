using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Framework.FrameworkCore;
using Framework.Util;
using Gameplay.UI;
using I2.Loc;
using Rewired;
using Sirenix.Utilities;
using Steamworks;
using TMPro;
using Tools.DataContainer;
using Tools.UI;
using UnityEngine;

namespace Framework.Managers;

public class LocalizationManager : GameSystem
{
	public delegate void OnLocalizeCallback(string languageKey);

	private static string currentId = string.Empty;

	private static TextMeshProUGUI currentTextMeshProFont;

	private List<string> AudioLanguages = new List<string> { "English", "Spanish" };

	public static List<string> AudioLanguagesKeys = new List<string> { "EN", "ES" };

	private int currentAudioLanguageIndex;

	private static TMP_SpriteAsset _cachedIconData;

	public int CurrentAudioLanguageIndex
	{
		get
		{
			return currentAudioLanguageIndex;
		}
		set
		{
			if (value >= 0 && value <= AudioLanguages.Count)
			{
				currentAudioLanguageIndex = value;
				if (LocalizationManager.OnLocalizeAudioEvent != null)
				{
					LocalizationManager.OnLocalizeAudioEvent(GetCurrentAudioLanguageCode());
				}
			}
		}
	}

	public static event OnLocalizeCallback OnLocalizeAudioEvent;

	public override void Initialize()
	{
		Singleton<Core>.Instance.StartCoroutine(WaitForCoreAnContinue());
	}

	private IEnumerator WaitForCoreAnContinue()
	{
		yield return new WaitUntil(() => Core.ready);
		if (!UIController.instance.GetOptionsWidget().ReadOptionsFromFile())
		{
			SteamLanguageChange();
		}
	}

	private void SteamLanguageChange()
	{
		if (SteamManager.Initialized)
		{
			Debug.Log("LocalizationManager::Initialize: SteamManager is Initialized!");
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("spanish", "es");
			dictionary.Add("english", "en");
			dictionary.Add("french", "fr");
			dictionary.Add("german", "de");
			dictionary.Add("italian", "it");
			dictionary.Add("schinese", "zh");
			dictionary.Add("tchinese", "zh");
			dictionary.Add("russian", "ru");
			dictionary.Add("japanese", "ja");
			dictionary.Add("brazilian", "pt-BR");
			string currentGameLanguage = SteamApps.GetCurrentGameLanguage();
			Debug.Log("LocalizationManager::Initialize: SteamApps.GetCurrentGameLanguage() returns the following value: " + currentGameLanguage);
			try
			{
				string text = dictionary[currentGameLanguage];
				Debug.Log("LocalizationManager::Initialize: gameLanCode has the following value: " + text);
				int indexByLanguageCode = Core.Localization.GetIndexByLanguageCode(text);
				Debug.Log("LocalizationManager::Initialize: Core.Localization.GetIndexByLanguageCode returns the following value: " + indexByLanguageCode);
				Core.Localization.SetLanguageByIdx(indexByLanguageCode);
				Debug.Log("LocalizationManager::Initialize: Language setted!");
				return;
			}
			catch (KeyNotFoundException)
			{
				Debug.Log("LocalizationManager::Initialize: Language not found!");
				return;
			}
		}
		Debug.Log("LocalizationManager::Initialize: SteamManager is not Initialized!");
	}

	public static LanguageSource GetMainLanguageSource()
	{
		string currentLanguage = I2.Loc.LocalizationManager.CurrentLanguage;
		return I2.Loc.LocalizationManager.Sources[0];
	}

	public void SetNextLanguage()
	{
		string currentLanguageCode = I2.Loc.LocalizationManager.CurrentLanguageCode;
		LanguageSource lang = I2.Loc.LocalizationManager.Sources[0];
		int idx = lang.GetLanguageIndexFromCode(currentLanguageCode);
		idx++;
		if (idx >= lang.mLanguages.Count)
		{
			idx = 0;
		}
		while (!GetAllEnabledLanguages().Exists((LanguageData x) => lang.mLanguages[idx].Code == x.Code))
		{
			idx++;
			if (idx >= lang.mLanguages.Count)
			{
				idx = 0;
			}
		}
		currentLanguageCode = lang.mLanguages[idx].Code;
		I2.Loc.LocalizationManager.CurrentLanguageCode = currentLanguageCode;
	}

	public void SetLanguageByIdx(int idx)
	{
		if (idx >= GetAllEnabledLanguages().Count)
		{
			idx = 0;
		}
		I2.Loc.LocalizationManager.CurrentLanguageCode = GetAllEnabledLanguages()[idx].Code;
	}

	public int GetCurrentLanguageIndex()
	{
		string currentLanguageCode = I2.Loc.LocalizationManager.CurrentLanguageCode;
		return GetIndexByLanguageCode(currentLanguageCode);
	}

	public string GetCurrentLanguageCode()
	{
		return I2.Loc.LocalizationManager.CurrentLanguageCode;
	}

	public string GetLanguageCodeByIndex(int index)
	{
		return GetAllEnabledLanguages()[index].Code;
	}

	public string GetLanguageNameByIndex(int index)
	{
		return GetAllEnabledLanguages()[index].Name;
	}

	public int GetIndexByLanguageCode(string languageCode)
	{
		return GetAllEnabledLanguages().FindIndex((LanguageData x) => x.Code == languageCode);
	}

	public Font GetFontByLanguageName(string languageName)
	{
		Font result = null;
		LanguageSource languageSource = I2.Loc.LocalizationManager.Sources[0];
		string value = GameConstants.DefaultFont;
		if (GameConstants.FontByLanguages.ContainsKey(languageName))
		{
			value = GameConstants.FontByLanguages[languageName];
		}
		foreach (Font item in languageSource.Assets.FilterCast<Font>())
		{
			if (item.name.StartsWith(value))
			{
				return item;
			}
		}
		return result;
	}

	public List<string> GetAllEnabledLanguagesNames()
	{
		List<string> list = new List<string>();
		foreach (LanguageData allEnabledLanguage in GetAllEnabledLanguages())
		{
			list.Add(allEnabledLanguage.Name);
		}
		return list;
	}

	public List<LanguageData> GetAllEnabledLanguages()
	{
		List<LanguageData> list = new List<LanguageData>();
		LanguageSource languageSource = I2.Loc.LocalizationManager.Sources[0];
		foreach (LanguageData mLanguage in languageSource.mLanguages)
		{
			if (mLanguage.IsEnabled())
			{
				list.Add(mLanguage);
			}
		}
		return list;
	}

	public void AddLanguageSource(string SourceName)
	{
		GameObject asset = ResourceManager.pInstance.GetAsset<GameObject>(SourceName);
		LanguageSource languageSource = ((!asset) ? null : asset.GetComponent<LanguageSource>());
		if ((bool)languageSource && !I2.Loc.LocalizationManager.Sources.Contains(languageSource))
		{
			I2.Loc.LocalizationManager.AddSource(languageSource);
		}
	}

	public string Get(string key)
	{
		string translation = I2.Loc.LocalizationManager.GetTranslation(key);
		if (translation.IsNullOrWhitespace())
		{
			return "[!LOC_" + key.ToUpper() + "]";
		}
		return translation;
	}

	public static string ParseMeshPro(string localizedText, string idString, TextMeshProUGUI textMesh = null)
	{
		Regex regex = new Regex("\\[(.*?)\\]");
		currentId = idString;
		currentTextMeshProFont = textMesh;
		return regex.Replace(localizedText, ProcessTag);
	}

	public List<string> GetAllAudioLanguagesNames()
	{
		return AudioLanguages;
	}

	public string GetCurrentAudioLanguageCode()
	{
		return AudioLanguagesKeys[currentAudioLanguageIndex];
	}

	public string GetCurrentAudioLanguage()
	{
		return AudioLanguages[currentAudioLanguageIndex];
	}

	public string GetCurrentAudioLanguageByIndex(int index)
	{
		return AudioLanguages[index];
	}

	private static string ProcessTag(Match m)
	{
		string value = m.Groups[1].Value;
		string result = value;
		string[] array = value.Split(':');
		if (array.Length != 2)
		{
			Debug.LogWarning("Localization PARSE error ID : " + currentId + " Tag element different 2");
		}
		else
		{
			string text = array[0].ToUpper();
			string text2 = array[1];
			string text3 = ((!(I2.Loc.LocalizationManager.CurrentLanguageCode == "zh")) ? string.Empty : " ");
			switch (text)
			{
			case "ICON":
			{
				string spriteName = "ICON_" + text2;
				TMP_Sprite spriteData = GetSpriteData(spriteName);
				result = $"<size={spriteData.height}><sprite name=\"{spriteData.name}\"></size>";
				break;
			}
			case "ACT":
				result = text3 + ParseAction(text2);
				break;
			case "VAR":
				Debug.LogWarning("Localization PARSE error ID : " + currentId + " VAR NOT IMPLEMENTED");
				break;
			default:
				Debug.LogWarning("Localization PARSE error ID : " + currentId + " Unknow prefix " + text);
				break;
			}
		}
		return result;
	}

	private static string ParseAction(string action)
	{
		string result = "[" + action + "]";
		JoystickType activeJoystickModel = Core.Input.ActiveJoystickModel;
		ControllerType activeControllerType = Core.Input.ActiveControllerType;
		string text = "KB_";
		if (activeControllerType != 0)
		{
			switch (activeJoystickModel)
			{
			case JoystickType.XBOX:
			case JoystickType.Generic:
				text = "XBOX_";
				break;
			case JoystickType.PlayStation:
				text = "PS_";
				break;
			case JoystickType.Switch:
				text = "SWITCH_";
				break;
			}
		}
		InputIcon.AxisCheck axisCheck = InputIcon.AxisCheck.None;
		if (action.EndsWith("+"))
		{
			axisCheck = InputIcon.AxisCheck.Positive;
			action = action.Remove(action.Length - 1);
		}
		else if (action.EndsWith("-"))
		{
			axisCheck = InputIcon.AxisCheck.Negative;
			action = action.Remove(action.Length - 1);
		}
		Player player = ReInput.players.GetPlayer(0);
		InputAction action2 = ReInput.mapping.GetAction(action);
		ActionElementMap actionElementMap = null;
		if (action2 != null)
		{
			AxisRange axisRange = ((axisCheck == InputIcon.AxisCheck.Positive) ? AxisRange.Positive : AxisRange.Negative);
			actionElementMap = Core.ControlRemapManager.FindLastElementMapByInputAction(action2, axisRange, Core.Input.ActiveController);
		}
		if (actionElementMap == null)
		{
			Debug.LogWarning("Localization PARSE error ID : " + currentId + " Action not found: " + action);
		}
		else if (activeControllerType == ControllerType.Keyboard)
		{
			string name = InputIcon.GetButtonDescriptionByButtonKey(actionElementMap.elementIdentifierName).icon.name;
			if (name.Equals("KB_BLANK"))
			{
				result = GetKeyboardIconRtfByLanguage(actionElementMap.keyCode.ToString());
			}
			else
			{
				TMP_Sprite spriteData = GetSpriteData(name);
				result = $"<size={spriteData.height}><sprite name=\"{name}\"></size>";
			}
		}
		else
		{
			string text2 = actionElementMap.elementIdentifierName.ToUpper();
			if (text2.StartsWith("BUTTON "))
			{
				result = GetJoystickIconRtfByLanguage(text2[text2.Length - 1] + string.Empty);
			}
			else
			{
				if (activeJoystickModel == JoystickType.Generic)
				{
					if (actionElementMap.elementIdentifierName.ToUpper().Equals("RIGHT TRIGGER"))
					{
						text2 = "START";
					}
					else if (actionElementMap.elementIdentifierName.ToUpper().Equals("LEFT TRIGGER"))
					{
						text2 = "BACK";
					}
				}
				string text3 = text + text2;
				TMP_Sprite spriteData2 = GetSpriteData(text3);
				string arg = ((spriteData2 != null) ? $"<size={spriteData2.height}>" : string.Empty);
				string arg2 = ((spriteData2 != null) ? "</size>" : string.Empty);
				result = $"{arg}<sprite name=\"{text3}\">{arg2}";
			}
		}
		return result;
	}

	private static string GetKeyboardIconRtfByLanguage(string keycode)
	{
		LocalizationSpacingData[] array = Resources.LoadAll<LocalizationSpacingData>("UI");
		int num = -22;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		float num5 = 0f;
		int num6 = 22;
		int num7 = 26;
		float num8 = 0f;
		bool flag = true;
		TMP_Sprite spriteData = GetSpriteData("KB_BLANK");
		if (spriteData != null)
		{
			num6 = (int)spriteData.width;
			num7 = (int)spriteData.xAdvance;
		}
		LocalizationSpacingData[] array2 = array;
		foreach (LocalizationSpacingData localizationSpacingData in array2)
		{
			if (!(localizationSpacingData.Language != I2.Loc.LocalizationManager.CurrentLanguage))
			{
				num2 = localizationSpacingData.extraSpacing;
				num4 = localizationSpacingData.extraAfterSpacing;
				num8 = ((!localizationSpacingData.addCharacterWidth) ? 0f : 1f);
				num5 = localizationSpacingData.verticalSpacing;
				break;
			}
		}
		if ((bool)currentTextMeshProFont)
		{
			int num9 = (int)currentTextMeshProFont.GetPreferredValues(keycode).x;
			num = -num6 + num2;
			num3 = (int)((float)(num7 - num6 + num4) + num8 * (float)num9 / 2f);
		}
		if (keycode.Length > 1)
		{
			flag = false;
			num = 0;
			num3 = 0;
		}
		return string.Format("<size={5}>{6}</size><space={0}><voffset={1}px><color={4}>{2}</color><space={3}></voffset>", num, num5, keycode, num3, "#F8E4C7FF", spriteData.height, (!flag) ? string.Empty : "<sprite name=\"KB_BLANK\">");
	}

	private static string GetJoystickIconRtfByLanguage(string keycode)
	{
		string text = "<sprite name=\"CONSOLE_BLANK\">";
		string text2 = "<space=-12>";
		string text3 = " ";
		string text4 = string.Empty;
		string text5 = "</voffset>";
		string text6 = "<voffset=4px>";
		switch (I2.Loc.LocalizationManager.CurrentLanguageCode)
		{
		case "ru":
			text2 = "<space=-11>";
			break;
		case "zh":
		case "ja":
			text4 = "<voffset=-2px>";
			text2 = "<space=-13>";
			text3 = "<space=10></voffset>";
			break;
		}
		return text6 + text + text5 + text2 + text4 + keycode + text3;
	}

	private static TMP_Sprite GetSpriteData(string spriteName)
	{
		int num = -1;
		if (!_cachedIconData)
		{
			_cachedIconData = Resources.Load<TMP_SpriteAsset>("Input/all_platform_buttons");
		}
		if ((bool)_cachedIconData)
		{
			num = _cachedIconData.GetSpriteIndexFromName(spriteName);
		}
		return (num < 0) ? null : _cachedIconData.spriteInfoList[num];
	}

	public string GetValueWithParam(string scriptKey, string key, string value)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add(key, value);
		Dictionary<string, string> parameters = dictionary;
		return GetValueWithParams(scriptKey, parameters);
	}

	public string GetValueWithParams(string scriptKey, Dictionary<string, string> parameters)
	{
		string text = scriptKey;
		foreach (KeyValuePair<string, string> parameter in parameters)
		{
			text = text.Replace("{[" + parameter.Key + "]}", parameter.Value);
		}
		return text;
	}
}
