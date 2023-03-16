using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapDifficultySelectStartUI : AbstractMapSceneStartUI
{
	private const int KoreanUpscaleSize = 100;

	private const float AsianImageScale = 0.9f;

	private const float KoreanBossTitleScale = 1.2f;

	private const int KoreanDifficultyFontSize = 29;

	private const int KoreanDifficultyOptionsFontSize = 37;

	[SerializeField]
	private Image inAnimated;

	[SerializeField]
	private Image bossTitleImage;

	[SerializeField]
	private Image bossNameImage;

	[SerializeField]
	private Image difficultyImage;

	[SerializeField]
	private Sprite[] inSprites;

	[SerializeField]
	private Image[] separatorsAnimated;

	[SerializeField]
	private Sprite[] separatorsSprites;

	[SerializeField]
	private RectTransform cursor;

	[Header("Options")]
	[SerializeField]
	private RectTransform easy;

	[SerializeField]
	private RectTransform normal;

	[SerializeField]
	private RectTransform normalSeparator;

	[SerializeField]
	private RectTransform hard;

	[SerializeField]
	private RectTransform hardSeparator;

	[SerializeField]
	private RectTransform box;

	[SerializeField]
	private Color selectedColor;

	[SerializeField]
	private Color unselectedColor;

	[Header("Stage")]
	[SerializeField]
	private LocalizationHelper bossImage;

	[SerializeField]
	private LocalizationHelper bossName;

	[SerializeField]
	private LocalizationHelper difficultyText;

	[SerializeField]
	private Material bossCardWhiteMaterial;

	[SerializeField]
	private Image difficultySelectionText;

	[SerializeField]
	private Text inText;

	[Header("Glow")]
	[SerializeField]
	private GlowText glowScript;

	[SerializeField]
	private LocalizationHelper bossGlow;

	[SerializeField]
	private Image asianGlow;

	private TMP_Text[] difficulyTexts;

	private Level.Mode[] options;

	private int index = 1;

	private float cursorY;

	private int initialMaxFontSize;

	private Vector2 initialinImagePosX;

	private Vector2 initialinImagePosY;

	private Vector2 initialinDifficultyPos;

	private Vector2 initialDifficultyPos;

	private Vector2 initialBossNamePos;

	public static MapDifficultySelectStartUI Current { get; protected set; }

	public static Level.Mode Mode { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		Current = this;
		switch (Level.CurrentMode)
		{
		case Level.Mode.Easy:
			index = 0;
			break;
		case Level.Mode.Normal:
			index = 1;
			break;
		case Level.Mode.Hard:
			index = 2;
			break;
		}
		options = new Level.Mode[3]
		{
			Level.Mode.Easy,
			Level.Mode.Normal,
			Level.Mode.Hard
		};
		SetDifficultyAvailability();
		difficulyTexts = new TMP_Text[3];
		difficulyTexts[0] = easy.GetComponent<TMP_Text>();
		difficulyTexts[1] = normal.GetComponent<TMP_Text>();
		difficulyTexts[2] = hard.GetComponent<TMP_Text>();
		if (bossImage != null && bossImage.textComponent != null)
		{
			initialMaxFontSize = bossImage.textComponent.resizeTextMaxSize;
		}
		initialinImagePosX = inAnimated.rectTransform.offsetMin;
		initialinImagePosY = inAnimated.rectTransform.offsetMax;
		initialinDifficultyPos = difficultyImage.rectTransform.anchoredPosition;
		initialDifficultyPos = difficultySelectionText.rectTransform.anchoredPosition;
		initialBossNamePos = bossNameImage.rectTransform.anchoredPosition;
	}

	private void SetDifficultyAvailability()
	{
		if (PlayerData.Data.CurrentMap == Scenes.scene_map_world_4)
		{
			if (!PlayerData.Data.IsHardModeAvailable)
			{
				options = new Level.Mode[1] { Level.Mode.Normal };
				hard.gameObject.SetActive(value: false);
				hardSeparator.gameObject.SetActive(value: false);
			}
			else
			{
				options = new Level.Mode[2]
				{
					Level.Mode.Normal,
					Level.Mode.Hard
				};
			}
			index = Mathf.Max(0, index - 1);
			easy.gameObject.SetActive(value: false);
			normalSeparator.gameObject.SetActive(value: false);
		}
		else if (PlayerData.Data.CurrentMap == Scenes.scene_map_world_DLC)
		{
			if (level == "Saltbaker")
			{
				if (!PlayerData.Data.IsHardModeAvailableDLC)
				{
					options = new Level.Mode[1] { Level.Mode.Normal };
				}
				else
				{
					options = new Level.Mode[2]
					{
						Level.Mode.Normal,
						Level.Mode.Hard
					};
				}
			}
			else if (!PlayerData.Data.IsHardModeAvailableDLC)
			{
				options = new Level.Mode[2]
				{
					Level.Mode.Easy,
					Level.Mode.Normal
				};
			}
			else
			{
				options = new Level.Mode[3]
				{
					Level.Mode.Easy,
					Level.Mode.Normal,
					Level.Mode.Hard
				};
			}
			easy.gameObject.SetActive(level != "Saltbaker");
			normalSeparator.gameObject.SetActive(level != "Saltbaker");
			hard.gameObject.SetActive(PlayerData.Data.IsHardModeAvailableDLC);
			hardSeparator.gameObject.SetActive(PlayerData.Data.IsHardModeAvailableDLC);
		}
		else
		{
			if (!PlayerData.Data.IsHardModeAvailable)
			{
				options = new Level.Mode[2]
				{
					Level.Mode.Easy,
					Level.Mode.Normal
				};
			}
			hard.gameObject.SetActive(PlayerData.Data.IsHardModeAvailable);
			hardSeparator.gameObject.SetActive(PlayerData.Data.IsHardModeAvailable);
		}
	}

	public new void In(MapPlayerController playerController)
	{
		base.In(playerController);
		if (Level.CurrentMode == Level.Mode.Easy && PlayerData.Data.CurrentMap == Scenes.scene_map_world_4)
		{
			Level.SetCurrentMode(Level.Mode.Normal);
			switch (Level.CurrentMode)
			{
			case Level.Mode.Easy:
				index = 0;
				break;
			case Level.Mode.Normal:
				index = 1;
				break;
			case Level.Mode.Hard:
				index = 2;
				break;
			}
		}
		if (PlayerData.Data.CurrentMap == Scenes.scene_map_world_DLC)
		{
			SetDifficultyAvailability();
			if (level == "Saltbaker" && Level.CurrentMode == Level.Mode.Easy)
			{
				Level.SetCurrentMode(Level.Mode.Normal);
			}
		}
		if (base.animator != null)
		{
			base.animator.SetTrigger("ZoomIn");
			AudioManager.Play("world_map_level_menu_open");
		}
		InWordSetup();
		difficultyImage.enabled = Localization.language == Localization.Languages.Japanese;
		difficultyImage.rectTransform.anchoredPosition = initialinDifficultyPos;
		for (int i = 0; i < separatorsAnimated.Length; i++)
		{
			separatorsAnimated[i].sprite = separatorsSprites[Random.Range(0, separatorsSprites.Length)];
		}
		bool flag = Localization.language == Localization.Languages.Korean || Localization.language == Localization.Languages.SimplifiedChinese || Localization.language == Localization.Languages.Japanese;
		bossTitleImage.enabled = Localization.language == Localization.Languages.English || flag || PlayerData.Data.CurrentMap == Scenes.scene_map_world_DLC;
		glowScript.StopGlow();
		glowScript.DisableTMPText();
		glowScript.DisableImages();
		if (Localization.language == Localization.Languages.SimplifiedChinese)
		{
			difficultySelectionText.rectTransform.anchoredPosition = new Vector2(difficultySelectionText.rectTransform.anchoredPosition.x, -70f);
		}
		else
		{
			difficultySelectionText.rectTransform.anchoredPosition = initialDifficultyPos;
		}
		TranslationElement translationElement = Localization.Find(level + "Selection");
		if (bossImage != null && translationElement != null)
		{
			bossImage.ApplyTranslation(translationElement);
			if (bossImage.textComponent != null)
			{
				if (Localization.language == Localization.Languages.Korean)
				{
					bossImage.textComponent.resizeTextMaxSize = 100;
				}
				else
				{
					bossImage.textComponent.resizeTextMaxSize = initialMaxFontSize;
				}
			}
			if (flag)
			{
				SetupAsianBossCard(translationElement, bossTitleImage);
			}
			else
			{
				bossImage.transform.localScale = Vector3.one;
				bossImage.transform.localPosition = Vector3.zero;
				bossTitleImage.rectTransform.offsetMax = new Vector2(bossTitleImage.rectTransform.offsetMax.x, 0.5f);
				bossTitleImage.rectTransform.offsetMin = new Vector2(bossTitleImage.rectTransform.offsetMin.x, 0.5f);
				inAnimated.rectTransform.offsetMin = initialinImagePosX;
				inAnimated.rectTransform.offsetMax = initialinImagePosY;
				inText.fontStyle = FontStyle.Italic;
			}
		}
		TranslationElement translationElement2 = Localization.Find(level + "WorldMap");
		if (translationElement2 != null)
		{
			bossName.ApplyTranslation(translationElement2);
			if (bossName.textComponent != null && bossName.textComponent.enabled)
			{
				bossName.textComponent.font = FontLoader.GetFont(FontLoader.FontType.CupheadHenriette_A_merged);
			}
			bossNameImage.transform.localScale = Vector3.one;
			bossNameImage.rectTransform.anchoredPosition = initialBossNamePos;
			if (flag)
			{
				bossNameImage.material = bossCardWhiteMaterial;
				if (Localization.language == Localization.Languages.Korean || Localization.language == Localization.Languages.Japanese)
				{
					bossNameImage.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
					if (Localization.language == Localization.Languages.Japanese)
					{
						bossNameImage.rectTransform.anchoredPosition = new Vector2(0f, 214.2f);
					}
				}
			}
			bossName.gameObject.SetActive(Localization.language != 0 && !flag && PlayerData.Data.CurrentMap != Scenes.scene_map_world_DLC);
		}
		TranslationElement translationElement3 = Localization.Find(level + "Glow");
		if (Localization.language != 0)
		{
			if (translationElement3 != null && flag)
			{
				bossGlow.ApplyTranslation(translationElement3);
			}
			else
			{
				glowScript.InitTMPText(bossImage.textMeshProComponent, bossName.textComponent);
				glowScript.BeginGlow();
			}
		}
		bossGlow.gameObject.SetActive(flag && PlayerData.Data.CurrentMap != Scenes.scene_map_world_DLC);
		for (int j = 0; j < difficulyTexts.Length; j++)
		{
			difficulyTexts[j].color = unselectedColor;
		}
		difficulyTexts[(int)Level.CurrentMode].color = selectedColor;
	}

	private void OnDestroy()
	{
		bossNameImage.sprite = null;
		bossTitleImage.sprite = null;
		asianGlow.sprite = null;
		if (Current == this)
		{
			Current = null;
		}
	}

	private void Update()
	{
		UpdateCursor();
		if (base.CurrentState == State.Active)
		{
			CheckInput();
		}
	}

	private void CheckInput()
	{
		if (base.Able)
		{
			if (GetButtonDown(CupheadButton.MenuLeft))
			{
				Next(-1);
			}
			if (GetButtonDown(CupheadButton.MenuRight))
			{
				Next(1);
			}
			if (GetButtonDown(CupheadButton.Cancel))
			{
				Out();
			}
			if (GetButtonDown(CupheadButton.Accept))
			{
				LoadLevel();
			}
		}
	}

	private void Next(int direction)
	{
		if ((index != options.Length - 1 && direction != -1) || (index != 0 && direction != 1))
		{
			AudioManager.Play("world_map_level_difficulty_hover");
		}
		index = Mathf.Clamp(index + direction, 0, options.Length - 1);
		Level.SetCurrentMode(options[index]);
		UpdateCursor();
		for (int i = 0; i < difficulyTexts.Length; i++)
		{
			difficulyTexts[i].color = unselectedColor;
		}
		difficulyTexts[(int)Level.CurrentMode].color = selectedColor;
	}

	private void UpdateCursor()
	{
		Vector3 position = cursor.transform.position;
		position.y = normal.position.y;
		Level.Mode mode = Level.CurrentMode;
		if (PlayerData.Data.CurrentMap == Scenes.scene_map_world_4 && mode == Level.Mode.Easy)
		{
			mode = Level.Mode.Normal;
		}
		switch (mode)
		{
		case Level.Mode.Easy:
			position.x = easy.position.x;
			cursor.sizeDelta = new Vector2(easy.sizeDelta.x + 30f, easy.sizeDelta.y + 20f);
			break;
		case Level.Mode.Normal:
			position.x = normal.position.x;
			cursor.sizeDelta = new Vector2(normal.sizeDelta.x + 30f, normal.sizeDelta.y + 20f);
			break;
		case Level.Mode.Hard:
			position.x = hard.position.x;
			cursor.sizeDelta = new Vector2(hard.sizeDelta.x + 30f, hard.sizeDelta.y + 20f);
			break;
		}
		cursor.transform.position = position;
	}

	private void SetupAsianBossCard(TranslationElement translation, Image image)
	{
		image.material = bossCardWhiteMaterial;
		image.rectTransform.offsetMax = new Vector2(image.rectTransform.offsetMax.x, 0f);
		image.rectTransform.offsetMin = new Vector2(image.rectTransform.offsetMin.x, 0f);
		SetupAsianDifficulty();
		image.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
		if (Localization.language == Localization.Languages.Korean)
		{
			if (PlayerData.Data.CurrentMap != Scenes.scene_map_world_DLC)
			{
				image.rectTransform.offsetMax = new Vector2(image.rectTransform.offsetMax.x, 40f);
				image.rectTransform.offsetMin = new Vector2(image.rectTransform.offsetMin.x, 40f);
			}
			SetupKoreanInWord();
		}
		else if (Localization.language == Localization.Languages.SimplifiedChinese)
		{
			inAnimated.rectTransform.offsetMax = new Vector2(inAnimated.rectTransform.offsetMax.x, -140f);
			if (level.Equals("FlyingBlimp"))
			{
				image.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
				image.rectTransform.offsetMax = new Vector2(image.rectTransform.offsetMax.x, 40f);
				image.rectTransform.offsetMin = new Vector2(image.rectTransform.offsetMin.x, 40f);
				inAnimated.rectTransform.offsetMax = new Vector2(inAnimated.rectTransform.offsetMax.x, -100f);
			}
		}
		else if (Localization.language == Localization.Languages.Japanese)
		{
			if (level.Equals("Flower") || level.Equals("FlyingBird") || level.Equals("Mouse") || level.Equals("SallyStagePlay"))
			{
				image.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
				image.rectTransform.offsetMax = new Vector2(image.rectTransform.offsetMax.x, -90f);
			}
			else if (level.Equals("Train"))
			{
				image.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
				image.rectTransform.offsetMax = new Vector2(image.rectTransform.offsetMax.x, 70f);
			}
			else if (level.Equals("Bee"))
			{
				image.transform.localScale = Vector3.one;
				image.rectTransform.offsetMax = new Vector2(image.rectTransform.offsetMax.x, -60f);
			}
			else if (level.Equals("DicePalaceMain"))
			{
				image.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
				image.rectTransform.offsetMax = new Vector2(image.rectTransform.offsetMax.x, -70f);
			}
			else
			{
				image.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
				image.rectTransform.offsetMax = new Vector2(image.rectTransform.offsetMax.x, 55f);
			}
			difficultyImage.rectTransform.anchoredPosition = new Vector2(0f, -70f);
			SetupJapaneseInWord();
		}
	}

	private void SetupAsianDifficulty()
	{
		difficultyText.textComponent.fontSize = 29;
		easy.gameObject.GetComponent<TMP_Text>().fontSize = 37f;
		normal.gameObject.GetComponent<TMP_Text>().fontSize = 37f;
		hard.gameObject.GetComponent<TMP_Text>().fontSize = 37f;
	}

	private void SetupJapaneseInWord()
	{
		if (PlayerData.Data.CurrentMap == Scenes.scene_map_world_DLC)
		{
			inAnimated.enabled = false;
			inText.enabled = false;
			return;
		}
		inAnimated.preserveAspect = true;
		if (level.Equals("Flower") || level.Equals("FlyingBird") || level.Equals("Mouse") || level.Equals("SallyStagePlay") || level.Equals("Bee") || level.Equals("DicePalaceMain"))
		{
			inAnimated.rectTransform.offsetMax = new Vector2(inAnimated.rectTransform.offsetMax.x, initialinImagePosY.y - 15.9f);
			inAnimated.rectTransform.offsetMin = new Vector2(inAnimated.rectTransform.offsetMin.x, initialinImagePosX.y - 15.9f);
		}
		else
		{
			inAnimated.rectTransform.offsetMax = new Vector2(inAnimated.rectTransform.offsetMax.x, initialinImagePosY.y + 6.5f);
			inAnimated.rectTransform.offsetMin = new Vector2(inAnimated.rectTransform.offsetMin.x, initialinImagePosX.y + 6.5f);
		}
	}

	private void SetupKoreanInWord()
	{
		if (PlayerData.Data.CurrentMap == Scenes.scene_map_world_DLC)
		{
			inAnimated.enabled = false;
			inText.enabled = false;
			return;
		}
		inText.fontStyle = FontStyle.Normal;
		if (level.Equals("Bird") || level.Equals("Dragon") || level.Equals("Devil"))
		{
			inAnimated.rectTransform.offsetMax = new Vector2(inAnimated.rectTransform.offsetMax.x, -160f);
		}
		else if (level.Equals("Flower"))
		{
			inAnimated.rectTransform.offsetMax = new Vector2(inAnimated.rectTransform.offsetMax.x, -140f);
		}
		else if (level.Equals("Bee"))
		{
			inAnimated.rectTransform.offsetMax = new Vector2(inAnimated.rectTransform.offsetMax.x, -130f);
		}
		else if (level.Equals("KingDiceTop"))
		{
			inAnimated.rectTransform.offsetMax = new Vector2(inAnimated.rectTransform.offsetMax.x, -155f);
		}
		else if (level.Equals("Frogs") || level.Equals("FlyingBlimp") || level.Equals("Baroness") || level.Equals("FlyingGenie") || level.Equals("Clown") || level.Equals("SallyStagePlay") || level.Equals("FlyingMermaid"))
		{
			inAnimated.rectTransform.offsetMax = new Vector2(inAnimated.rectTransform.offsetMax.x, -110f);
		}
		else
		{
			inAnimated.rectTransform.offsetMax = new Vector2(inAnimated.rectTransform.offsetMax.x, -150f);
		}
	}

	private void InWordSetup()
	{
		if (PlayerData.Data.CurrentMap == Scenes.scene_map_world_DLC)
		{
			inAnimated.enabled = false;
			inText.enabled = false;
			return;
		}
		if (Localization.language == Localization.Languages.English)
		{
			inAnimated.sprite = inSprites[Random.Range(0, inSprites.Length)];
		}
		inAnimated.enabled = Localization.language != Localization.Languages.Korean && Localization.language != Localization.Languages.SimplifiedChinese && PlayerData.Data.CurrentMap != Scenes.scene_map_world_DLC;
		inAnimated.transform.localScale = Vector3.one;
		if (Localization.language == Localization.Languages.French)
		{
			inAnimated.transform.localScale = Vector3.one * 1.5f;
		}
		else if (Localization.language == Localization.Languages.PortugueseBrazil || Localization.language == Localization.Languages.SpanishSpain || Localization.language == Localization.Languages.SpanishAmerica)
		{
			inAnimated.transform.localScale = Vector3.one * 1.2f;
		}
		else if (Localization.language == Localization.Languages.Russian)
		{
			inAnimated.transform.localScale = Vector3.one * 2f;
		}
	}
}
