using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.PostProcessing;

public class WinScreen : AbstractMonoBehaviour
{
	private static readonly int[] TryRegularCurveValues = new int[12]
	{
		0, 62, 48, 57, 65, 74, 58, 36, 54, 72,
		83, 27
	};

	private static readonly int[] TryRegularCurveOffsets = new int[12]
	{
		0, 32, 18, 27, 35, 45, 28, 7, 24, 42,
		50, -3
	};

	private static readonly int[] TryRegularCurveValuesDLC = new int[12]
	{
		62, 62, 48, 57, 65, 74, 58, 36, 54, 72,
		83, 27
	};

	private static readonly int[] TryRegularCurveOffsetsDLC = new int[12]
	{
		32, 32, 18, 27, 35, 45, 28, 7, 24, 42,
		50, -3
	};

	private const float BOB_FRAME_TIME = 1f / 24f;

	[Header("Delays")]
	[SerializeField]
	private float introDelay = 10f;

	[SerializeField]
	private float talliesDelay = 0.5f;

	[SerializeField]
	private float gradeDelay = 0.7f;

	[SerializeField]
	private float advanceDelay = 10f;

	[SerializeField]
	private WinScreenTicker timeTicker;

	[SerializeField]
	private WinScreenTicker hitsTicker;

	[SerializeField]
	private WinScreenTicker parriesTicker;

	[SerializeField]
	private WinScreenTicker superMeterTicker;

	[SerializeField]
	private WinScreenTicker difficultyTicker;

	[SerializeField]
	private LocalizationHelper spiritStockLabelLocalizationHelper;

	[SerializeField]
	private WinScreenGradeDisplay gradeDisplay;

	[SerializeField]
	private GameObject continuePrompt;

	private bool player1IsChalice;

	private bool player2IsChalice;

	[Header("UI Scoring")]
	[SerializeField]
	private GameObject scoring;

	[SerializeField]
	private TextMeshProUGUI gradeLabel;

	[Header("Try Text")]
	[SerializeField]
	private GameObject tryRegular;

	[SerializeField]
	private TMP_Text tryRegularText;

	[SerializeField]
	private SpriteRenderer tryRegularEnglishBackground;

	[Header("Glow effect")]
	[SerializeField]
	private GameObject glowingText;

	[SerializeField]
	private GlowText glowScript;

	[SerializeField]
	private PostProcessingBehaviour postProcessingScript;

	[SerializeField]
	public PostProcessingProfile asianProfile;

	[SerializeField]
	private GameObject tryExpert;

	[SerializeField]
	private TMP_Text tryExpertText;

	[Header("Background")]
	[SerializeField]
	private Transform Background;

	[Header("DifferentLayouts")]
	[SerializeField]
	private GameObject OnePlayerCuphead;

	[SerializeField]
	private GameObject OnePlayerMugman;

	[SerializeField]
	private Transform OnePlayerUIRoot;

	[SerializeField]
	private Animator OnePlayerTitleCuphead;

	[SerializeField]
	private Animator OnePlayerTitleMugman;

	[Space(10f)]
	[SerializeField]
	private GameObject TwoPlayerCupheadMugman;

	[SerializeField]
	private GameObject TwoPlayerMugmanCuphead;

	[SerializeField]
	private Transform TwoPlayerCupheadMugmanUIRoot;

	[SerializeField]
	private Transform TwoPlayerMugmanCupheadUIRoot;

	[SerializeField]
	private Animator TwoPlayerTitleCuphead;

	[SerializeField]
	private Animator TwoPlayerTitleMugman;

	[Space(10f)]
	[SerializeField]
	private GameObject OnePlayerChalice;

	[SerializeField]
	private GameObject TwoPlayerChaliceCuphead;

	[SerializeField]
	private GameObject TwoPlayerCupheadChalice;

	[SerializeField]
	private GameObject TwoPlayerMugmanChalice;

	[SerializeField]
	private GameObject TwoPlayerChaliceMugman;

	[SerializeField]
	private Transform OnePlayerChaliceUIRoot;

	[SerializeField]
	private Transform TwoPlayerChaliceCupheadUIRoot;

	[SerializeField]
	private Transform TwoPlayerCupheadChaliceUIRoot;

	[SerializeField]
	private Transform TwoPlayerMugmanChaliceUIRoot;

	[SerializeField]
	private Transform TwoPlayerChaliceMugmanUIRoot;

	[SerializeField]
	private Animator OnePlayerTitleChalice;

	[SerializeField]
	private Animator TwoPlayerTitleChaliceCuphead;

	[SerializeField]
	private Animator TwoPlayerTitleCupheadChalice;

	[SerializeField]
	private Animator TwoPlayerTitleMugmanChalice;

	[SerializeField]
	private Animator TwoPlayerTitleChaliceMugman;

	[Space(10f)]
	[SerializeField]
	private SpriteRenderer[] studioMHDRSubtitles;

	[SerializeField]
	private Transform[] resultsTitles;

	[SerializeField]
	private Vector3 playerOneOffCenterTitleRoot;

	[SerializeField]
	private Vector3 japaneseTitleRoot;

	[SerializeField]
	private Vector3 koreanTitleRoot;

	[SerializeField]
	private Vector3 chineseTitleRoot;

	[Space(10f)]
	[SerializeField]
	private Vector3 chaliceTitleOffset1P;

	[SerializeField]
	private Vector3 chaliceTitleOffset2P;

	[Space(10f)]
	[SerializeField]
	private Canvas results;

	[Header("BannerCurve")]
	[SerializeField]
	private MinMax textWidthRange;

	[SerializeField]
	private MinMax curveScaleRange;

	[SerializeField]
	private float yOffsetDelta;

	private CupheadInput.AnyPlayerInput input;

	private const float BG_NORMAL_SPEED = 50f;

	private const float BG_FAST_SPEED = 150f;

	private bool isDLCLevel;

	protected override void Awake()
	{
		base.Awake();
		OnePlayerCuphead.SetActive(value: false);
		TwoPlayerCupheadMugman.SetActive(value: false);
		Cuphead.Init();
		Animator animator = null;
		LevelScoringData scoringData = Level.ScoringData;
		if (scoringData != null)
		{
			player1IsChalice = scoringData.player1IsChalice;
			player2IsChalice = scoringData.player2IsChalice;
		}
		if (!PlayerManager.Multiplayer)
		{
			player2IsChalice = false;
		}
		if (Localization.language != 0)
		{
			DisableEnglishMDHRSubtitles();
		}
		if (Localization.language == Localization.Languages.Japanese)
		{
			CenterResultTitles(japaneseTitleRoot);
		}
		else if (Localization.language == Localization.Languages.Korean)
		{
			CenterResultTitles(koreanTitleRoot);
		}
		else if (Localization.language == Localization.Languages.SimplifiedChinese || Localization.language == Localization.Languages.German || Localization.language == Localization.Languages.SpanishSpain || Localization.language == Localization.Languages.SpanishAmerica || Localization.language == Localization.Languages.Russian || Localization.language == Localization.Languages.PortugueseBrazil)
		{
			CenterResultTitles(chineseTitleRoot);
		}
		if (PlayerManager.Multiplayer)
		{
			if (PlayerManager.player1IsMugman)
			{
				if (player1IsChalice)
				{
					animator = TwoPlayerTitleChaliceCuphead;
					TwoPlayerChaliceCuphead.SetActive(value: true);
					results.transform.position = TwoPlayerChaliceCupheadUIRoot.transform.position;
				}
				else if (player2IsChalice)
				{
					animator = TwoPlayerTitleMugmanChalice;
					TwoPlayerMugmanChalice.SetActive(value: true);
					results.transform.position = TwoPlayerMugmanChaliceUIRoot.transform.position;
				}
				else
				{
					animator = TwoPlayerTitleMugman;
					TwoPlayerMugmanCuphead.SetActive(value: true);
					results.transform.position = TwoPlayerMugmanCupheadUIRoot.transform.position;
				}
			}
			else if (player1IsChalice)
			{
				animator = TwoPlayerTitleChaliceMugman;
				TwoPlayerChaliceMugman.SetActive(value: true);
				results.transform.position = TwoPlayerChaliceMugmanUIRoot.transform.position;
			}
			else if (player2IsChalice)
			{
				animator = TwoPlayerTitleCupheadChalice;
				TwoPlayerCupheadChalice.SetActive(value: true);
				results.transform.position = TwoPlayerCupheadChaliceUIRoot.transform.position;
			}
			else
			{
				animator = TwoPlayerTitleCuphead;
				TwoPlayerCupheadMugman.SetActive(value: true);
				results.transform.position = TwoPlayerCupheadMugmanUIRoot.transform.position;
			}
			if (Localization.language == Localization.Languages.Polish || Localization.language == Localization.Languages.Italian || Localization.language == Localization.Languages.French)
			{
				CenterResultTitles(japaneseTitleRoot);
			}
			if (Localization.language == Localization.Languages.English)
			{
				animator.SetBool("pickedA", Rand.Bool());
			}
			animator.SetTrigger(GetTriggerName(Localization.language));
		}
		else
		{
			if (player1IsChalice)
			{
				animator = OnePlayerTitleChalice;
				OnePlayerChalice.SetActive(value: true);
			}
			else if (PlayerManager.player1IsMugman)
			{
				animator = OnePlayerTitleMugman;
				OnePlayerMugman.SetActive(value: true);
			}
			else
			{
				animator = OnePlayerTitleCuphead;
				OnePlayerCuphead.SetActive(value: true);
			}
			results.transform.position = OnePlayerUIRoot.transform.position;
			if (Localization.language == Localization.Languages.Polish || Localization.language == Localization.Languages.Italian || Localization.language == Localization.Languages.French || Localization.language == Localization.Languages.SimplifiedChinese || Localization.language == Localization.Languages.Japanese)
			{
				CenterResultTitles(playerOneOffCenterTitleRoot);
			}
			if (Localization.language == Localization.Languages.English)
			{
				animator.SetBool("pickedA", Rand.Bool());
			}
			animator.SetTrigger(GetTriggerName(Localization.language));
		}
		StartCoroutine(main_cr());
		continuePrompt.SetActive(value: false);
		input = new CupheadInput.AnyPlayerInput();
		StartCoroutine(rotate_bg_cr());
	}

	private void DisableEnglishMDHRSubtitles()
	{
		SpriteRenderer[] array = studioMHDRSubtitles;
		foreach (SpriteRenderer spriteRenderer in array)
		{
			spriteRenderer.enabled = false;
		}
	}

	private void CenterResultTitles(Vector3 rootPosition)
	{
		if (player1IsChalice)
		{
			rootPosition += ((!PlayerManager.Multiplayer) ? chaliceTitleOffset1P : chaliceTitleOffset2P);
		}
		Transform[] array = resultsTitles;
		foreach (Transform transform in array)
		{
			transform.localPosition = rootPosition;
		}
	}

	private IEnumerator main_cr()
	{
		LevelScoringData data = Level.ScoringData;
		if (Localization.language == Localization.Languages.Korean)
		{
			TextMeshProUGUI[] componentsInChildren = scoring.GetComponentsInChildren<TextMeshProUGUI>();
			foreach (TextMeshProUGUI textMeshProUGUI in componentsInChildren)
			{
				textMeshProUGUI.fontStyle = FontStyles.Bold;
			}
			gradeLabel.fontStyle = FontStyles.Bold;
		}
		if (data.difficulty == Level.Mode.Easy && Level.PreviousDifficulty == Level.Mode.Easy && Level.PreviousLevelType == Level.Type.Battle && !Level.IsDicePalace && !Level.IsDicePalaceMain && Level.PreviousLevel != Levels.Devil && Level.PreviousLevel != Levels.Saltbaker)
		{
			if (Array.IndexOf(Level.worldDLCBossLevels, Level.PreviousLevel) >= 0)
			{
				isDLCLevel = true;
			}
			else
			{
				isDLCLevel = false;
			}
			Localization.Translation translation = ((!isDLCLevel) ? Localization.Translate("ResultsTryRegular") : Localization.Translate("WinScreen_Tooltip_SimpleIngredient"));
			tryRegular.SetActive(value: true);
			if ((translation.image == null && !translation.hasSpriteAtlasImage) || isDLCLevel)
			{
				tryRegular.GetComponent<SpriteRenderer>().enabled = false;
				tryRegularEnglishBackground.enabled = false;
				tryRegularText.text = translation.text;
				tryRegularText.font = translation.fonts.fontAsset;
				tryRegularText.fontSize = ((translation.fonts.fontAssetSize != 0f) ? translation.fonts.fontAssetSize : tryRegularText.fontSize);
				tryRegularText.outlineWidth = ((Localization.language != Localization.Languages.Korean) ? tryRegularText.outlineWidth : 0.07f);
				AlignBannerText(tryRegularText.gameObject);
				if (Localization.language == Localization.Languages.Korean || Localization.language == Localization.Languages.Japanese)
				{
					postProcessingScript.profile = asianProfile;
				}
				AlignBannerText(glowingText);
				glowScript.InitTMPText(tryRegularText);
				if (Localization.language != 0 || isDLCLevel)
				{
					glowScript.BeginGlow();
				}
			}
			else
			{
				tryRegularText.enabled = false;
			}
		}
		if (data == null)
		{
			yield break;
		}
		timeTicker.TargetValue = (int)data.time;
		timeTicker.MaxValue = (int)data.goalTime;
		hitsTicker.TargetValue = Mathf.Clamp(data.finalHP, 0, 3);
		hitsTicker.MaxValue = 3;
		parriesTicker.TargetValue = Mathf.Min(data.numParries, (int)Cuphead.Current.ScoringProperties.parriesForHighestGrade);
		parriesTicker.MaxValue = (int)Cuphead.Current.ScoringProperties.parriesForHighestGrade;
		superMeterTicker.TargetValue = Mathf.Min(data.superMeterUsed, (int)Cuphead.Current.ScoringProperties.superMeterUsageForHighestGrade);
		superMeterTicker.MaxValue = (int)Cuphead.Current.ScoringProperties.superMeterUsageForHighestGrade;
		if (data.useCoinsInsteadOfSuperMeter)
		{
			superMeterTicker.TargetValue = data.coinsCollected;
			superMeterTicker.MaxValue = 5;
			spiritStockLabelLocalizationHelper.currentID = Localization.Find("ResultsMenuCoins").id;
		}
		difficultyTicker.TargetValue = ((data.difficulty != 0) ? ((data.difficulty == Level.Mode.Normal) ? 1 : 2) : 0);
		gradeDisplay.Grade = Level.Grade;
		gradeDisplay.Difficulty = data.difficulty;
		yield return new WaitForSeconds(introDelay);
		WinScreenTicker[] tickers = new WinScreenTicker[5] { timeTicker, hitsTicker, parriesTicker, superMeterTicker, difficultyTicker };
		WinScreenTicker[] array = tickers;
		foreach (WinScreenTicker ticker in array)
		{
			ticker.StartCounting();
			while (!ticker.FinishedCounting)
			{
				yield return null;
			}
			if (ticker.TargetValue != 0)
			{
				yield return new WaitForSeconds(talliesDelay);
			}
		}
		InterruptingPrompt.SetCanInterrupt(canInterrupt: true);
		float timer2 = 0f;
		while (timer2 < gradeDelay && !input.GetAnyButtonDown())
		{
			if (!InterruptingPrompt.IsInterrupting())
			{
				timer2 += Time.deltaTime;
			}
			yield return null;
		}
		gradeDisplay.Show();
		while (!gradeDisplay.FinishedGrading)
		{
			yield return null;
		}
		timer2 = 0f;
		continuePrompt.SetActive(value: true);
		while (timer2 < advanceDelay && !input.GetActionButtonDown())
		{
			if (!InterruptingPrompt.IsInterrupting())
			{
				timer2 += Time.deltaTime;
			}
			yield return null;
		}
		if (Level.PreviousLevel == Levels.Devil)
		{
			Cutscene.Load(Scenes.scene_title, Scenes.scene_cutscene_outro, SceneLoader.Transition.Iris, SceneLoader.Transition.Fade);
		}
		else if (Level.PreviousLevel == Levels.Saltbaker)
		{
			Cutscene.Load(Scenes.scene_map_world_DLC, Scenes.scene_cutscene_dlc_ending, SceneLoader.Transition.Iris, SceneLoader.Transition.Fade);
		}
		else
		{
			SceneLoader.LoadLastMap();
		}
	}

	private void AlignBannerText(GameObject bannerText)
	{
		bannerText.GetComponent<TextMeshCurveAndJitter>().CurveScale = ((!isDLCLevel) ? TryRegularCurveValues[(int)Localization.language] : TryRegularCurveValuesDLC[(int)Localization.language]);
		Vector3 localPosition = bannerText.transform.localPosition;
		localPosition.y = ((!isDLCLevel) ? TryRegularCurveOffsets[(int)Localization.language] : TryRegularCurveOffsetsDLC[(int)Localization.language]);
		bannerText.transform.localPosition = localPosition;
	}

	private IEnumerator rotate_bg_cr()
	{
		float frameTime = 0f;
		float normalTime = 0f;
		float speed = 50f;
		while (true)
		{
			frameTime += (float)CupheadTime.Delta;
			while (frameTime > 1f / 24f)
			{
				frameTime -= 1f / 24f;
				Background.Rotate(0f, 0f, speed * (float)CupheadTime.Delta);
				yield return null;
			}
			if (gradeDisplay.Celebration && speed < 150f)
			{
				normalTime += (float)CupheadTime.Delta;
				speed = Mathf.Lerp(50f, 150f, normalTime / 0.5f);
			}
			yield return null;
		}
	}

	private string GetTriggerName(Localization.Languages language)
	{
		return language switch
		{
			Localization.Languages.French => "useFrench", 
			Localization.Languages.Italian => "useItalian", 
			Localization.Languages.German => "useGerman", 
			Localization.Languages.Polish => "usePolish", 
			Localization.Languages.PortugueseBrazil => "usePortuguese", 
			Localization.Languages.Russian => "useRussian", 
			Localization.Languages.SpanishSpain => "useSpanishSpain", 
			Localization.Languages.SpanishAmerica => "useSpanishAmerica", 
			Localization.Languages.Japanese => "useJapanese", 
			Localization.Languages.Korean => "useKorean", 
			Localization.Languages.SimplifiedChinese => "useChinese", 
			_ => "useEnglish", 
		};
	}
}
