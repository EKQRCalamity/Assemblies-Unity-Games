using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class LocalizationHelper : MonoBehaviour
{
	public struct LocalizationSubtext
	{
		public string key;

		public string value;

		public bool dontTranslate;

		public LocalizationSubtext(string key, string value, bool dontTranslate = false)
		{
			this.key = key;
			this.value = value;
			this.dontTranslate = dontTranslate;
		}
	}

	public bool existingKey;

	public int currentID = -1;

	public Localization.Languages currentLanguage = (Localization.Languages)(-1);

	public Localization.Categories currentCategory;

	public bool currentCustomFont;

	public Text textComponent;

	public Image imageComponent;

	public SpriteRenderer spriteRendererComponent;

	public TMP_Text textMeshProComponent;

	private int initialFontSize;

	private float initialFontAssetSize;

	private bool isInit;

	private LocalizationSubtext[] subTranslations;

	private bool hasOverride;

	private LocalizationHelperPlatformOverride platformOverride;

	private void Init()
	{
		if (textComponent != null)
		{
			initialFontSize = textComponent.fontSize;
		}
		if (textMeshProComponent != null)
		{
			initialFontAssetSize = textMeshProComponent.fontSize;
		}
		isInit = true;
	}

	private void Awake()
	{
		platformOverride = GetComponent<LocalizationHelperPlatformOverride>();
		hasOverride = platformOverride != null;
	}

	private void Start()
	{
		Localization.OnLanguageChangedEvent += ApplyTranslation;
	}

	private void OnDestroy()
	{
		Localization.OnLanguageChangedEvent -= ApplyTranslation;
	}

	private void OnEnable()
	{
		ApplyTranslation();
	}

	public void ApplyTranslation()
	{
		int id = currentID;
		if (hasOverride && platformOverride.HasOverrideForCurrentPlatform(out var newID))
		{
			id = newID;
		}
		ApplyTranslation(Localization.Find(id));
	}

	public void ApplyTranslation(TranslationElement translationElement, LocalizationSubtext[] subTranslations = null)
	{
		this.subTranslations = subTranslations;
		ApplyTranslation(translationElement);
	}

	private void ApplyTranslation(TranslationElement translationElement)
	{
		if (!isInit)
		{
			Init();
		}
		currentLanguage = Localization.language;
		if (currentLanguage == (Localization.Languages)(-1) || translationElement == null || string.IsNullOrEmpty(translationElement.key))
		{
			return;
		}
		Localization.Translation translation = translationElement.translation;
		if (string.IsNullOrEmpty(translation.text))
		{
			translation = Localization.Translate(translationElement.key);
		}
		string text = translation.text;
		if (text != null)
		{
			text = text.Replace("\\n", "\n");
		}
		if (text != null && text.Contains("{") && text.Contains("}"))
		{
			if (subTranslations != null)
			{
				bool flag = true;
				while (flag)
				{
					flag = false;
					for (int i = 0; i < subTranslations.Length; i++)
					{
						if (text.Contains("{" + subTranslations[i].key + "}"))
						{
							flag = true;
							if (subTranslations[i].dontTranslate)
							{
								text = text.Replace("{" + subTranslations[i].key + "}", subTranslations[i].value);
								continue;
							}
							Localization.Translation translation2 = Localization.Translate(subTranslations[i].value);
							text = ((!string.IsNullOrEmpty(translation2.text)) ? text.Replace("{" + subTranslations[i].key + "}", translation2.text) : text.Replace("{" + subTranslations[i].key + "}", subTranslations[i].value));
						}
					}
				}
			}
			string[] array = text.Split('{');
			if (array.Length > 1)
			{
				string[] array2 = array[1].Split('}');
				if (array2.Length > 1)
				{
					string text2 = array2[0];
					Localization.Translation translation3 = Localization.Translate(text2);
					if (!string.IsNullOrEmpty(translation3.text))
					{
						text = text.Replace("{" + text2 + "}", translation3.text);
					}
				}
			}
		}
		if (textComponent != null)
		{
			textComponent.text = text;
			textComponent.enabled = !string.IsNullOrEmpty(text);
			if (translation.hasCustomFont)
			{
				textComponent.font = translation.fonts.font;
			}
			else if (Localization.Instance.fonts[(int)currentLanguage][(int)translationElement.category].fontType != 0)
			{
				textComponent.font = Localization.Instance.fonts[(int)currentLanguage][(int)translationElement.category].font;
			}
			textComponent.fontSize = ((translation.fonts.fontSize <= 0) ? initialFontSize : translation.fonts.fontSize);
		}
		if (textMeshProComponent != null)
		{
			textMeshProComponent.text = text;
			textMeshProComponent.enabled = !string.IsNullOrEmpty(text);
			textMeshProComponent.characterSpacing = translation.fonts.charSpacing;
			if (translation.hasCustomFontAsset)
			{
				textMeshProComponent.font = translation.fonts.fontAsset;
			}
			else
			{
				textMeshProComponent.font = Localization.Instance.fonts[(int)currentLanguage][(int)translationElement.category].fontAsset;
			}
			textMeshProComponent.fontSize = ((!(translation.fonts.fontAssetSize > 0f)) ? initialFontAssetSize : translation.fonts.fontAssetSize);
		}
		if (spriteRendererComponent != null)
		{
			Sprite sprite = null;
			if (translation.hasSpriteAtlasImage)
			{
				SpriteAtlas cachedAsset = AssetLoader<SpriteAtlas>.GetCachedAsset(translation.spriteAtlasName);
				sprite = cachedAsset.GetSprite(translation.spriteAtlasImageName);
			}
			else
			{
				sprite = translation.image;
			}
			spriteRendererComponent.sprite = sprite;
			spriteRendererComponent.enabled = false;
			spriteRendererComponent.enabled = sprite != null;
		}
		if (imageComponent != null)
		{
			Sprite sprite2 = null;
			if (translation.hasSpriteAtlasImage)
			{
				SpriteAtlas cachedAsset2 = AssetLoader<SpriteAtlas>.GetCachedAsset(translation.spriteAtlasName);
				sprite2 = cachedAsset2.GetSprite(translation.spriteAtlasImageName);
			}
			else
			{
				sprite2 = translation.image;
			}
			imageComponent.sprite = sprite2;
			imageComponent.enabled = false;
			imageComponent.enabled = sprite2 != null;
		}
	}
}
