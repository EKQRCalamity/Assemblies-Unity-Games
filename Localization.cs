using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "LocalizationAsset", menuName = "Localization Asset", order = 1)]
public class Localization : ScriptableObject, ISerializationCallbackReceiver
{
	[SerializeField]
	public enum Languages
	{
		English,
		French,
		Italian,
		German,
		SpanishSpain,
		SpanishAmerica,
		Korean,
		Russian,
		Polish,
		PortugueseBrazil,
		Japanese,
		SimplifiedChinese
	}

	[SerializeField]
	public enum Categories
	{
		NoCategory,
		LevelSelectionName,
		LevelSelectionIn,
		LevelSelectionStage,
		LevelSelectionDifficultyHeader,
		LevelSelectionDifficultys,
		EquipCategoryNames,
		EquipWeaponNames,
		EquipCategoryBackName,
		EquipCategoryBackTitle,
		EquipCategoryBackSubtitle,
		EquipCategoryBackDescription,
		ChecklistTitle,
		ChecklistWorldNames,
		ChecklistContractHeaders,
		ChecklistContracts,
		PauseMenuItems,
		DeathMenuQuote,
		DeathMenuItems,
		ResultsMenuTitle,
		ResultsMenuCategories,
		ResultsMenuGrade,
		ResultsMenuNewRecord,
		ResultsMenuTryNormal,
		IntroEndingText,
		IntroEndingAction,
		CutScenesText,
		SpeechBalloons,
		WorldMapTitles,
		Glyphs,
		TitleScreenSelection,
		Notifications,
		Tutorials,
		OptionMenu,
		RemappingMenu,
		RemappingButton,
		XboxNotification,
		AttractScreen,
		JoinPrompt,
		ConfirmMenu,
		DifficultyMenu,
		ShopElement,
		StageTitles,
		NintendoSwitchNotification,
		Achievements
	}

	[Serializable]
	public struct Translation
	{
		[SerializeField]
		public bool hasImage;

		[SerializeField]
		public string text;

		[SerializeField]
		public CategoryLanguageFont fonts;

		[SerializeField]
		public Sprite image;

		[SerializeField]
		public string spriteAtlasName;

		[SerializeField]
		public string spriteAtlasImageName;

		public bool hasSpriteAtlasImage => spriteAtlasName != null && spriteAtlasName.Length > 0 && spriteAtlasImageName != null && spriteAtlasImageName.Length > 0;

		public bool hasCustomFont => fonts.fontType != FontLoader.FontType.None;

		public bool hasCustomFontAsset => fonts.tmpFontType != FontLoader.TMPFontType.None;

		public string SanitizedText()
		{
			return text.Replace("\\n", "\n");
		}
	}

	[Serializable]
	public class CategoryLanguageFont
	{
		public int fontSize;

		public FontLoader.FontType fontType;

		public float fontAssetSize;

		public FontLoader.TMPFontType tmpFontType;

		public float charSpacing;

		public Font font => FontLoader.GetFont(fontType);

		public TMP_FontAsset fontAsset => FontLoader.GetTMPFont(tmpFontType);
	}

	[Serializable]
	public struct CategoryLanguageFonts
	{
		[SerializeField]
		public CategoryLanguageFont[] fonts;

		public CategoryLanguageFont this[int index]
		{
			get
			{
				return fonts[index];
			}
			set
			{
				fonts[index] = value;
			}
		}
	}

	public delegate void LanguageChanged();

	public const int LanguagesEnumSize = 12;

	public const string PATH = "LocalizationAsset";

	private static string[] csvKeys = new string[16]
	{
		"id", "key", "category", "description", "|lang|_text", "|lang|_cuphead_text", "|lang|_mugman_text", "|lang|_image", "|lang|_spriteAtlasName", "|lang|_spriteAtlasImageName",
		"|lang|_cuphead_image", "|lang|_mugman_image", "|lang|_font", "|lang|_fontSize", "|lang|_fontAsset", "|lang|_fontAssetSize"
	};

	private static Localization _instance;

	public static Languages language1 = Languages.English;

	public static Languages language2 = Languages.French;

	[SerializeField]
	private List<TranslationElement> m_TranslationElements = new List<TranslationElement>();

	[SerializeField]
	public CategoryLanguageFonts[] m_Fonts;

	public static Localization Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = Resources.Load<Localization>("LocalizationAsset");
			}
			return _instance;
		}
	}

	public static Languages language
	{
		get
		{
			if (SettingsData.Data.language == -1)
			{
				SettingsData.Data.language = (int)DetectLanguage.GetDefaultLanguage();
			}
			return (Languages)SettingsData.Data.language;
		}
		set
		{
			SettingsData.Data.language = (int)value;
			if (Localization.OnLanguageChangedEvent != null)
			{
				Localization.OnLanguageChangedEvent();
			}
		}
	}

	[SerializeField]
	public CategoryLanguageFonts[] fonts
	{
		get
		{
			if (m_Fonts == null)
			{
				int num = Enum.GetNames(typeof(Languages)).Length;
				int num2 = Enum.GetNames(typeof(Categories)).Length;
				m_Fonts = new CategoryLanguageFonts[num];
				for (int i = 0; i < num; i++)
				{
					m_Fonts[i].fonts = new CategoryLanguageFont[num2];
				}
			}
			return m_Fonts;
		}
		set
		{
			m_Fonts = value;
		}
	}

	[SerializeField]
	public List<TranslationElement> translationElements
	{
		get
		{
			return m_TranslationElements;
		}
		set
		{
			m_TranslationElements = value;
		}
	}

	public static event LanguageChanged OnLanguageChangedEvent;

	public static Translation Translate(string key)
	{
		if (Parser.IntTryParse(key, out var result))
		{
			return Translate(result);
		}
		Translation result2 = default(Translation);
		for (int i = 0; i < Instance.m_TranslationElements.Count; i++)
		{
			if (_instance.m_TranslationElements[i].key == key)
			{
				TranslationElement translationElement = _instance.m_TranslationElements[i];
				result2 = translationElement.translation;
			}
		}
		return result2;
	}

	public static Translation Translate(int id)
	{
		Translation result = default(Translation);
		for (int i = 0; i < Instance.m_TranslationElements.Count; i++)
		{
			if (_instance.m_TranslationElements[i].id == id)
			{
				TranslationElement translationElement = _instance.m_TranslationElements[i];
				result = translationElement.translation;
			}
		}
		return result;
	}

	public static TranslationElement Find(string key)
	{
		for (int i = 0; i < Instance.m_TranslationElements.Count; i++)
		{
			if (_instance.m_TranslationElements[i].key == key)
			{
				return _instance.m_TranslationElements[i];
			}
		}
		return null;
	}

	public static TranslationElement Find(int id)
	{
		for (int i = 0; i < Instance.m_TranslationElements.Count; i++)
		{
			if (_instance.m_TranslationElements[i].id == id)
			{
				return _instance.m_TranslationElements[i];
			}
		}
		return null;
	}

	public static void ExportCsv(string path)
	{
		string text = "|lang|";
		string text2 = "|lang|_cuphead";
		string text3 = "|lang|_mugman";
		char value = '@';
		string value2 = "\r\n";
		StringBuilder stringBuilder = new StringBuilder();
		int num = Enum.GetNames(typeof(Languages)).Length;
		int num2 = Enum.GetNames(typeof(Categories)).Length;
		for (int i = 0; i < csvKeys.Length; i++)
		{
			if (csvKeys[i].Contains(text))
			{
				string value3 = csvKeys[i].Replace(text, string.Empty);
				for (int j = 0; j < num; j++)
				{
					if (i > 0)
					{
						stringBuilder.Append(value);
					}
					Languages languages = (Languages)j;
					stringBuilder.Append(languages.ToString());
					stringBuilder.Append(value3);
				}
			}
			else
			{
				if (i > 0)
				{
					stringBuilder.Append(value);
				}
				stringBuilder.Append(csvKeys[i]);
			}
		}
		stringBuilder.Append(value2);
		string empty = string.Empty;
		for (int k = 0; k < Instance.m_TranslationElements.Count; k++)
		{
			TranslationElement translationElement = _instance.m_TranslationElements[k];
			if (translationElement.depth == -1)
			{
				continue;
			}
			for (int l = 0; l < csvKeys.Length; l++)
			{
				if (csvKeys[l].Contains(text))
				{
					for (int m = 0; m < num; m++)
					{
						if (l > 0)
						{
							stringBuilder.Append(value);
						}
						empty = string.Empty;
						string text4;
						Translation translation;
						if (csvKeys[l].Contains(text2))
						{
							text4 = csvKeys[l].Replace(text2, string.Empty);
							translation = ((translationElement.translationsCuphead != null && translationElement.translationsCuphead.Length != 0) ? translationElement.translationsCuphead[m] : default(Translation));
						}
						else if (csvKeys[l].Contains(text3))
						{
							text4 = csvKeys[l].Replace(text3, string.Empty);
							translation = ((translationElement.translationsMugman != null && translationElement.translationsMugman.Length != 0) ? translationElement.translationsMugman[m] : default(Translation));
						}
						else
						{
							text4 = csvKeys[l].Replace(text, string.Empty);
							translation = translationElement.translations[m];
						}
						switch (text4)
						{
						case "_text":
							empty = translation.text;
							if (!string.IsNullOrEmpty(empty))
							{
								empty = empty.Replace('\n'.ToString(), '\\' + "n");
							}
							break;
						case "_image":
							if (translation.image != null)
							{
								empty = translation.image.name;
							}
							break;
						case "_spriteAtlasName":
							empty = translation.spriteAtlasName;
							break;
						case "_spriteAtlasImageName":
							empty = translation.spriteAtlasImageName;
							break;
						case "_font":
							if (translation.fonts.fontType != 0)
							{
								empty = FontLoader.GetFilename(translation.fonts.fontType);
							}
							break;
						case "_fontSize":
							empty = ((translation.fonts.fontSize <= 0) ? string.Empty : translation.fonts.fontSize.ToString());
							break;
						case "_fontAsset":
							if (translation.fonts.tmpFontType != 0)
							{
								empty = FontLoader.GetFilename(translation.fonts.tmpFontType);
							}
							break;
						case "_fontAssetSize":
							empty = ((!(translation.fonts.fontAssetSize > 0f)) ? string.Empty : translation.fonts.fontAssetSize.ToString());
							break;
						}
						if (empty != null)
						{
							stringBuilder.Append(empty);
						}
					}
				}
				else
				{
					if (l > 0)
					{
						stringBuilder.Append(value);
					}
					empty = string.Empty;
					switch (csvKeys[l])
					{
					case "id":
						empty = translationElement.id.ToString();
						break;
					case "key":
						empty = translationElement.key;
						break;
					case "category":
						empty = translationElement.category.ToString();
						break;
					case "description":
						empty = translationElement.description;
						break;
					}
					if (empty != null)
					{
						stringBuilder.Append(empty);
					}
				}
			}
			stringBuilder.Append(value2);
		}
		Encoding encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
		byte[] bytes = encoding.GetBytes(stringBuilder.ToString());
		FileStream fileStream = new FileStream(path, FileMode.Create);
		byte[] preamble = encoding.GetPreamble();
		fileStream.Write(preamble, 0, preamble.Length);
		fileStream.Write(bytes, 0, bytes.Length);
		fileStream.Dispose();
	}

	public static void ImportCsv(string path)
	{
		char c = '@';
		string text = "\r\n";
		Encoding encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
		FileStream fileStream = new FileStream(path, FileMode.Open);
		byte[] preamble = encoding.GetPreamble();
		byte[] array = new byte[preamble.Length];
		fileStream.Read(array, 0, preamble.Length);
		bool flag = true;
		for (int i = 0; i < preamble.Length; i++)
		{
			if (preamble[i] != array[i])
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			array = new byte[fileStream.Length - preamble.Length];
			fileStream.Read(array, 0, array.Length);
		}
		else
		{
			array = new byte[fileStream.Length];
			fileStream.Position = 0L;
			fileStream.Read(array, 0, (int)fileStream.Length);
		}
		fileStream.Dispose();
		string @string = encoding.GetString(array);
		string[] array2 = @string.Split(new string[1] { text }, StringSplitOptions.RemoveEmptyEntries);
		string[] headers = array2[0].Split(c);
		processImportedLines(headers, array2, c);
	}

	private static void processImportedLines(string[] headers, string[] lines, char separator)
	{
		string text = "_cuphead";
		string text2 = "_mugman";
		string[] names = Enum.GetNames(typeof(Languages));
		string[] names2 = Enum.GetNames(typeof(Categories));
		Dictionary<string, Font> dictionary = new Dictionary<string, Font>();
		Dictionary<string, TMP_FontAsset> dictionary2 = new Dictionary<string, TMP_FontAsset>();
		Instance.m_TranslationElements.Clear();
		TranslationElement item = new TranslationElement("Root", -1, 0);
		_instance.m_TranslationElements.Add(item);
		for (int i = 1; i < lines.Length; i++)
		{
			string[] array = lines[i].Split(separator);
			if (array.Length != headers.Length)
			{
				if (!(lines[i] != string.Empty))
				{
				}
				continue;
			}
			item = Instance.AddKey();
			for (int j = 0; j < array.Length; j++)
			{
				if (string.IsNullOrEmpty(array[j]))
				{
					continue;
				}
				string text3 = headers[j];
				switch (text3)
				{
				case "id":
					item.id = Parser.IntParse(array[j]);
					continue;
				case "key":
					item.key = array[j];
					continue;
				case "category":
				{
					int category = -1;
					for (int k = 0; k < names2.Length; k++)
					{
						if (names2[k] == array[j])
						{
							category = k;
						}
					}
					item.category = (Categories)category;
					continue;
				}
				case "description":
					item.description = array[j];
					continue;
				}
				for (int l = 0; l < names.Length; l++)
				{
					if (!text3.Contains(names[l]))
					{
						continue;
					}
					text3 = text3.Replace(names[l], string.Empty);
					bool flag = false;
					bool flag2 = false;
					Translation translation;
					if (text3.Contains(text))
					{
						flag = true;
						text3 = text3.Replace(text, string.Empty);
						if (item.translationsCuphead == null || item.translationsCuphead.Length == 0)
						{
							item.translationsCuphead = new Translation[names.Length];
							item.translationsMugman = new Translation[names.Length];
						}
						translation = item.translationsCuphead[l];
					}
					else if (text3.Contains(text2))
					{
						flag2 = true;
						text3 = text3.Replace(text2, string.Empty);
						if (item.translationsCuphead == null || item.translationsCuphead.Length == 0)
						{
							item.translationsCuphead = new Translation[names.Length];
							item.translationsMugman = new Translation[names.Length];
						}
						translation = item.translationsMugman[l];
					}
					else
					{
						translation = item.translations[l];
					}
					if (translation.fonts == null)
					{
						translation.fonts = new CategoryLanguageFont();
					}
					switch (text3)
					{
					case "_text":
						translation.text = array[j];
						break;
					case "_image":
						if (string.IsNullOrEmpty(array[j]))
						{
							goto end_IL_04ae;
						}
						break;
					case "_spriteAtlasName":
						translation.spriteAtlasName = array[j];
						break;
					case "_spriteAtlasImageName":
						translation.spriteAtlasImageName = array[j];
						break;
					case "_font":
						if (string.IsNullOrEmpty(array[j]))
						{
							goto end_IL_04ae;
						}
						break;
					case "_fontSize":
						if (!string.IsNullOrEmpty(array[j]))
						{
							int num2 = Convert.ToInt32(array[j]);
							if (num2 == 0)
							{
								goto end_IL_04ae;
							}
							translation.fonts.fontSize = num2;
						}
						break;
					case "_fontAsset":
						if (string.IsNullOrEmpty(array[j]))
						{
							goto end_IL_04ae;
						}
						break;
					case "_fontAssetSize":
						if (!string.IsNullOrEmpty(array[j]))
						{
							float num = Convert.ToSingle(array[j]);
							if (num == 0f)
							{
								goto end_IL_04ae;
							}
							translation.fonts.fontAssetSize = num;
						}
						break;
					}
					if (flag)
					{
						item.translationsCuphead[l] = translation;
					}
					else if (flag2)
					{
						item.translationsMugman[l] = translation;
					}
					else
					{
						item.translations[l] = translation;
					}
					break;
					continue;
					end_IL_04ae:
					break;
				}
			}
		}
		if (Localization.OnLanguageChangedEvent != null)
		{
			Localization.OnLanguageChangedEvent();
		}
	}

	public TranslationElement AddKey()
	{
		int num = -1;
		for (int i = 0; i < m_TranslationElements.Count; i++)
		{
			if (m_TranslationElements[i].id > num)
			{
				num = m_TranslationElements[i].id;
			}
		}
		num++;
		TranslationElement translationElement = new TranslationElement("Key" + num, Categories.NoCategory, string.Empty, string.Empty, string.Empty, 0, num);
		m_TranslationElements.Add(translationElement);
		return translationElement;
	}

	private void Awake()
	{
		if (m_TranslationElements.Count == 0)
		{
			m_TranslationElements = new List<TranslationElement>(1);
			TranslationElement item = new TranslationElement("Root", -1, 0);
			m_TranslationElements.Add(item);
		}
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		bool flag = false;
		int num = Enum.GetNames(typeof(Languages)).Length;
		if (fonts.Length < num)
		{
			flag = true;
		}
		int num2 = Enum.GetNames(typeof(Categories)).Length;
		if (fonts[0].fonts.Length < num2)
		{
			flag = true;
		}
		if (flag)
		{
			fonts = GrowFonts(fonts, num, num2);
		}
	}

	private CategoryLanguageFonts[] GrowFonts(CategoryLanguageFonts[] oldFonts, int newLanguagesLength, int newCategoriesLength)
	{
		CategoryLanguageFonts[] array = new CategoryLanguageFonts[newLanguagesLength];
		for (int i = 0; i < newLanguagesLength; i++)
		{
			array[i].fonts = new CategoryLanguageFont[newCategoriesLength];
		}
		for (int j = 0; j < oldFonts.Length; j++)
		{
			for (int k = 0; k < oldFonts[j].fonts.Length; k++)
			{
				array[j][k] = oldFonts[j][k];
			}
		}
		return array;
	}
}
