using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapEventNotification : AbstractMonoBehaviour
{
	public enum Type
	{
		SoulContract,
		Super,
		Coin,
		ThreeCoins,
		Blueprint,
		Tooltip,
		TooltipEquip,
		DLCAvailable,
		AirplaneIngredient,
		RumIngredient,
		OldManIngredient,
		SnowIngredient,
		CowboyIngredient,
		CoinVariable,
		Djimmi,
		DjimmiFreed
	}

	[SerializeField]
	private Image background;

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private LocalizationHelper localizationHelper;

	[SerializeField]
	private LocalizationHelper notificationLocalizationHelper;

	[SerializeField]
	private RectTransform sparkleTransformContract;

	[SerializeField]
	private RectTransform sparkleTransformCoin1;

	[SerializeField]
	private RectTransform sparkleTransformCoin2;

	[SerializeField]
	private RectTransform sparkleTransformCoin3;

	[SerializeField]
	private GameObject sparklePrefab;

	[SerializeField]
	private CanvasGroup glyphCanvasGroup;

	[SerializeField]
	private GameObject coin2;

	[SerializeField]
	private GameObject coin3;

	[SerializeField]
	private GameObject coinVariable;

	[SerializeField]
	private Text coinVariableText;

	[SerializeField]
	private GameObject super1;

	[SerializeField]
	private GameObject super2;

	[SerializeField]
	private GameObject super3;

	[SerializeField]
	private GameObject curseCharm;

	[SerializeField]
	private GameObject ingredientStarburst;

	[SerializeField]
	private GameObject airplaneIngred;

	[SerializeField]
	private GameObject rumIngred;

	[SerializeField]
	private GameObject oldManIngred;

	[SerializeField]
	private GameObject snowCultIngred;

	[SerializeField]
	private GameObject cowboyIngred;

	[SerializeField]
	private GameObject confirmGlyph;

	[SerializeField]
	private GameObject dlcUIPrefab;

	[SerializeField]
	private Transform dlcUIRoot;

	[Header("Tooltips")]
	[SerializeField]
	private CanvasGroup tooltipCanvasGroup;

	[SerializeField]
	private Image tooltipPortrait;

	[SerializeField]
	private LocalizationHelper tooltipLocalizationHelper;

	[SerializeField]
	private GameObject tooltipEquipGlyph;

	[SerializeField]
	private Sprite TurtleSprite;

	[SerializeField]
	private Sprite CanteenSprite;

	[SerializeField]
	private Sprite ShopkeepSprite;

	[SerializeField]
	private Sprite ForkSprite;

	[SerializeField]
	private Sprite KingDiceSprite;

	[SerializeField]
	private Sprite MausoleumSprite;

	[SerializeField]
	private Sprite SaltbakerSpriteA;

	[SerializeField]
	private Sprite SaltbakerSpriteB;

	[SerializeField]
	private Sprite ChaliceSprite;

	[SerializeField]
	private Sprite ChaliceFanSprite;

	[SerializeField]
	private Sprite BoatmanSprite;

	private CanvasGroup canvasGroup;

	private bool sparkling;

	private bool coinShowing;

	private bool tooltipShowing;

	private bool tooltipEquipShowing;

	private bool superShowing;

	private bool dlcAvailableShowing;

	private bool ingredientShowing;

	private bool djimmiShowing;

	private int coinVariableCount;

	private Animator[] sparkleAnimatorsContract = new Animator[3];

	private Animator[] sparkleAnimatorsCoin1 = new Animator[3];

	private Animator[] sparkleAnimatorsCoin2 = new Animator[3];

	private Animator[] sparkleAnimatorsCoin3 = new Animator[3];

	private float timeBeforeNextSparkleContract = 0.2f;

	private float timeBeforeNextSparkleCoin1 = 0.2f;

	private float timeBeforeNextSparkleCoin2 = 0.2f;

	private float timeBeforeNextSparkleCoin3 = 0.2f;

	[SerializeField]
	private float timeBetweenSparkle = 0.3f;

	private CupheadInput.AnyPlayerInput input;

	public Queue<Action> EventQueue = new Queue<Action>();

	private MapDLCUI dlcUI;

	public static MapEventNotification Current { get; private set; }

	public bool showing { get; set; }

	protected override void Awake()
	{
		base.Awake();
		Current = this;
		input = new CupheadInput.AnyPlayerInput();
		canvasGroup = GetComponent<CanvasGroup>();
		canvasGroup.alpha = 0f;
		for (int i = 0; i < sparkleAnimatorsContract.Length; i++)
		{
			sparkleAnimatorsContract[i] = UnityEngine.Object.Instantiate(sparklePrefab, sparkleTransformContract).GetComponent<Animator>();
		}
		for (int j = 0; j < sparkleAnimatorsCoin1.Length; j++)
		{
			sparkleAnimatorsCoin1[j] = UnityEngine.Object.Instantiate(sparklePrefab, sparkleTransformCoin1).GetComponent<Animator>();
		}
		for (int k = 0; k < sparkleAnimatorsCoin2.Length; k++)
		{
			sparkleAnimatorsCoin2[k] = UnityEngine.Object.Instantiate(sparklePrefab, sparkleTransformCoin2).GetComponent<Animator>();
		}
		for (int l = 0; l < sparkleAnimatorsCoin3.Length; l++)
		{
			sparkleAnimatorsCoin3[l] = UnityEngine.Object.Instantiate(sparklePrefab, sparkleTransformCoin3).GetComponent<Animator>();
		}
		dlcUI = UnityEngine.Object.Instantiate(dlcUIPrefab, dlcUIRoot).GetComponent<MapDLCUI>();
		dlcUI.Init(checkIfDead: false);
		base.gameObject.SetActive(value: false);
	}

	private void OnDestroy()
	{
		if (Current == this)
		{
			Current = null;
		}
	}

	private void Update()
	{
		if (superShowing)
		{
			if (input.GetAnyButtonDown())
			{
				StartCoroutine(tweenOut_cr());
				base.animator.SetTrigger("hide_super");
				superShowing = false;
			}
			timeBeforeNextSparkleCoin1 -= CupheadTime.Delta;
			for (int i = 0; i < sparkleAnimatorsCoin1.Length; i++)
			{
				if (!(timeBeforeNextSparkleCoin1 > 0f) && sparkleAnimatorsCoin1[i].GetCurrentAnimatorStateInfo(0).IsName("Empty"))
				{
					timeBeforeNextSparkleCoin1 = timeBetweenSparkle;
					sparkleAnimatorsCoin1[i].transform.position = new Vector3(sparkleTransformCoin1.position.x + UnityEngine.Random.Range(sparkleTransformCoin1.sizeDelta.x * -0.5f, sparkleTransformCoin1.sizeDelta.x * 0.5f), sparkleTransformCoin1.position.y + UnityEngine.Random.Range(sparkleTransformCoin1.sizeDelta.y * -0.5f, sparkleTransformCoin1.sizeDelta.y * 0.5f), 101f);
					sparkleAnimatorsCoin1[i].SetTrigger(UnityEngine.Random.Range(0, 4).ToStringInvariant());
				}
			}
		}
		if (tooltipShowing && input.GetAnyButtonDown())
		{
			StartCoroutine(tweenOut_cr());
			base.animator.SetTrigger("hide_tooltip");
			tooltipShowing = false;
		}
		if (tooltipEquipShowing && input.GetButtonDown(CupheadButton.EquipMenu))
		{
			StartCoroutine(tweenOut_cr(0.5f));
			base.animator.SetTrigger("hide_tooltip");
			tooltipShowing = false;
		}
		if (coinShowing)
		{
			if (input.GetAnyButtonDown())
			{
				StartCoroutine(tweenOut_cr());
				base.animator.SetTrigger("hide_coin");
				coinShowing = false;
			}
			timeBeforeNextSparkleCoin1 -= CupheadTime.Delta;
			timeBeforeNextSparkleCoin2 -= CupheadTime.Delta;
			timeBeforeNextSparkleCoin3 -= CupheadTime.Delta;
			for (int j = 0; j < sparkleAnimatorsCoin1.Length; j++)
			{
				if (!(timeBeforeNextSparkleCoin1 > 0f) && sparkleAnimatorsCoin1[j].GetCurrentAnimatorStateInfo(0).IsName("Empty"))
				{
					timeBeforeNextSparkleCoin1 = timeBetweenSparkle;
					sparkleAnimatorsCoin1[j].transform.position = new Vector3(sparkleTransformCoin1.position.x + UnityEngine.Random.Range(sparkleTransformCoin1.sizeDelta.x * -0.5f, sparkleTransformCoin1.sizeDelta.x * 0.5f), sparkleTransformCoin1.position.y + UnityEngine.Random.Range(sparkleTransformCoin1.sizeDelta.y * -0.5f, sparkleTransformCoin1.sizeDelta.y * 0.5f), 101f);
					sparkleAnimatorsCoin1[j].SetTrigger(UnityEngine.Random.Range(0, 4).ToStringInvariant());
				}
			}
			for (int k = 0; k < sparkleAnimatorsCoin2.Length; k++)
			{
				if (!(timeBeforeNextSparkleCoin2 > 0f) && sparkleAnimatorsCoin2[k].GetCurrentAnimatorStateInfo(0).IsName("Empty"))
				{
					timeBeforeNextSparkleCoin2 = timeBetweenSparkle;
					sparkleAnimatorsCoin2[k].transform.position = new Vector3(sparkleTransformCoin2.position.x + UnityEngine.Random.Range(sparkleTransformCoin2.sizeDelta.x * -0.5f, sparkleTransformCoin2.sizeDelta.x * 0.5f), sparkleTransformCoin2.position.y + UnityEngine.Random.Range(sparkleTransformCoin2.sizeDelta.y * -0.5f, sparkleTransformCoin2.sizeDelta.y * 0.5f), 101f);
					sparkleAnimatorsCoin2[k].SetTrigger(UnityEngine.Random.Range(0, 4).ToStringInvariant());
				}
			}
			for (int l = 0; l < sparkleAnimatorsCoin3.Length; l++)
			{
				if (!(timeBeforeNextSparkleCoin3 > 0f) && sparkleAnimatorsCoin3[l].GetCurrentAnimatorStateInfo(0).IsName("Empty"))
				{
					timeBeforeNextSparkleCoin3 = timeBetweenSparkle;
					sparkleAnimatorsCoin3[l].transform.position = new Vector3(sparkleTransformCoin3.position.x + UnityEngine.Random.Range(sparkleTransformCoin3.sizeDelta.x * -0.5f, sparkleTransformCoin3.sizeDelta.x * 0.5f), sparkleTransformCoin3.position.y + UnityEngine.Random.Range(sparkleTransformCoin3.sizeDelta.y * -0.5f, sparkleTransformCoin3.sizeDelta.y * 0.5f), 101f);
					sparkleAnimatorsCoin3[l].SetTrigger(UnityEngine.Random.Range(0, 4).ToStringInvariant());
				}
			}
		}
		if (sparkling)
		{
			if (input.GetAnyButtonDown())
			{
				StartCoroutine(tweenOut_cr());
				base.animator.SetTrigger("hide");
				sparkling = false;
			}
			timeBeforeNextSparkleContract -= CupheadTime.Delta;
			for (int m = 0; m < sparkleAnimatorsContract.Length; m++)
			{
				if (!(timeBeforeNextSparkleContract > 0f) && sparkleAnimatorsContract[m].GetCurrentAnimatorStateInfo(0).IsName("Empty"))
				{
					timeBeforeNextSparkleContract = timeBetweenSparkle;
					sparkleAnimatorsContract[m].transform.position = new Vector3(sparkleTransformContract.position.x + UnityEngine.Random.Range(sparkleTransformContract.sizeDelta.x * -0.5f, sparkleTransformContract.sizeDelta.x * 0.5f), sparkleTransformContract.position.y + UnityEngine.Random.Range(sparkleTransformContract.sizeDelta.y * -0.5f, sparkleTransformContract.sizeDelta.y * 0.5f), 101f);
					sparkleAnimatorsContract[m].SetTrigger(UnityEngine.Random.Range(0, 4).ToStringInvariant());
				}
			}
		}
		if (dlcAvailableShowing && !dlcUI.visible)
		{
			StartCoroutine(tweenOut_cr(0.25f));
			dlcAvailableShowing = false;
		}
		if (ingredientShowing && input.GetAnyButtonDown())
		{
			StartCoroutine(tweenOut_cr());
			base.animator.SetTrigger("hide_ingred");
			ingredientShowing = false;
		}
		if (djimmiShowing && input.GetAnyButtonDown())
		{
			base.animator.SetTrigger("hide_djimmi");
			djimmiShowing = false;
		}
	}

	public void SparkleStart()
	{
		sparkling = true;
		StartCoroutine(showGlyphs_cr());
	}

	protected IEnumerator showGlyphs_cr()
	{
		yield return new WaitForSeconds(0.5f);
		float t = 0f;
		while (t < 0.2f)
		{
			float val = t / 0.2f;
			glyphCanvasGroup.alpha = Mathf.Lerp(0f, 1f, val);
			t += Time.deltaTime;
			yield return null;
		}
		glyphCanvasGroup.alpha = 1f;
		while (!input.GetButtonDown(CupheadButton.Accept))
		{
			yield return null;
		}
		base.animator.SetTrigger("hide");
		yield return null;
		yield return base.animator.WaitForAnimationToEnd(this, "anim_map_ui_contract_end", 0);
		base.gameObject.SetActive(value: false);
	}

	public void DebugShowContract(Levels level)
	{
		base.gameObject.SetActive(value: true);
		super1.SetActive(value: false);
		super2.SetActive(value: false);
		super3.SetActive(value: false);
		coin2.SetActive(value: false);
		coin3.SetActive(value: false);
		coinVariable.SetActive(value: false);
		coinVariableText.enabled = false;
		curseCharm.SetActive(value: false);
		airplaneIngred.SetActive(value: false);
		rumIngred.SetActive(value: false);
		oldManIngred.SetActive(value: false);
		snowCultIngred.SetActive(value: false);
		cowboyIngred.SetActive(value: false);
		InterruptingPrompt.SetCanInterrupt(canInterrupt: true);
		tooltipEquipGlyph.SetActive(value: false);
		AudioManager.Play("world_map_soul_contract_open");
		AudioManager.PlayLoop("world_map_soul_contract_stamp_shimmer_loop");
		base.animator.SetTrigger("show");
		TranslationElement translationElement = Localization.Find(level.ToString());
		localizationHelper.ApplyTranslation(translationElement);
		localizationHelper.textMeshProComponent.text = localizationHelper.textMeshProComponent.text.ToUpper().Replace("\\N", "\\n");
		string newValue = ((Localization.language != Localization.Languages.Japanese) ? " " : string.Empty);
		notificationLocalizationHelper.ApplyTranslation(Localization.Find("UnlockContract"), new LocalizationHelper.LocalizationSubtext[1]
		{
			new LocalizationHelper.LocalizationSubtext("CONTRACT", translationElement.translation.text.Replace("\\n", newValue))
		});
		showing = true;
		canvasGroup.alpha = 1f;
	}

	public void DebugShowEvent(Levels level)
	{
		DebugShowContract(level);
	}

	public IEnumerator HideContract()
	{
		showing = true;
		base.animator.SetTrigger("hide");
		yield return null;
		base.gameObject.SetActive(value: false);
	}

	public void ShowEvent(Type eventType)
	{
		EventQueue.Enqueue(delegate
		{
			InternalShowEvent(eventType);
		});
	}

	public void ShowVariableCoinEvent(int coinCount)
	{
		if (coinCount > 1)
		{
			coinVariableCount = coinCount;
			EventQueue.Enqueue(delegate
			{
				InternalShowEvent(Type.CoinVariable);
			});
		}
		else
		{
			EventQueue.Enqueue(delegate
			{
				InternalShowEvent(Type.Coin);
			});
		}
	}

	private void InternalShowEvent(Type eventType)
	{
		base.gameObject.SetActive(value: true);
		super1.SetActive(value: false);
		super2.SetActive(value: false);
		super3.SetActive(value: false);
		coin2.SetActive(value: false);
		coin3.SetActive(value: false);
		coinVariable.SetActive(value: false);
		coinVariableText.enabled = false;
		curseCharm.SetActive(value: false);
		airplaneIngred.SetActive(value: false);
		rumIngred.SetActive(value: false);
		oldManIngred.SetActive(value: false);
		snowCultIngred.SetActive(value: false);
		cowboyIngred.SetActive(value: false);
		InterruptingPrompt.SetCanInterrupt(canInterrupt: true);
		switch (eventType)
		{
		case Type.SoulContract:
		{
			confirmGlyph.SetActive(value: true);
			tooltipEquipGlyph.SetActive(value: false);
			AudioManager.Play("world_map_soul_contract_open");
			AudioManager.PlayLoop("world_map_soul_contract_stamp_shimmer_loop");
			base.animator.SetTrigger("show");
			TranslationElement translationElement = Localization.Find(Level.PreviousLevel.ToString());
			localizationHelper.ApplyTranslation(translationElement);
			localizationHelper.textMeshProComponent.text = localizationHelper.textMeshProComponent.text.ToUpper().Replace("\\N", "\\n");
			string newValue = ((Localization.language != Localization.Languages.Japanese) ? " " : string.Empty);
			notificationLocalizationHelper.ApplyTranslation(Localization.Find("UnlockContract"), new LocalizationHelper.LocalizationSubtext[1]
			{
				new LocalizationHelper.LocalizationSubtext("CONTRACT", translationElement.translation.text.Replace("\\n", newValue))
			});
			break;
		}
		case Type.Super:
			confirmGlyph.SetActive(value: true);
			base.animator.SetTrigger("show_super");
			AudioManager.Stop("world_level_bridge_building_poof");
			AudioManager.Play("world_map_super_open");
			AudioManager.PlayLoop("world_map_super_loop");
			StartCoroutine(SuperInRoutine());
			notificationLocalizationHelper.ApplyTranslation(Localization.Find("UnlockSuper"));
			switch (PlayerData.Data.CurrentMap)
			{
			case Scenes.scene_map_world_1:
				super1.SetActive(value: true);
				break;
			case Scenes.scene_map_world_2:
				super2.SetActive(value: true);
				break;
			case Scenes.scene_map_world_3:
				super3.SetActive(value: true);
				break;
			}
			break;
		case Type.Coin:
			confirmGlyph.SetActive(value: true);
			AudioManager.Play("world_map_coin_open");
			StartCoroutine(CoinInRoutine());
			notificationLocalizationHelper.ApplyTranslation(Localization.Find("GotACoin"));
			base.animator.SetTrigger("show_coin");
			break;
		case Type.ThreeCoins:
			confirmGlyph.SetActive(value: true);
			coin2.SetActive(value: true);
			coin3.SetActive(value: true);
			AudioManager.Play("world_map_coin_open");
			StartCoroutine(CoinInRoutine());
			notificationLocalizationHelper.ApplyTranslation(Localization.Find("GotThreeCoins"));
			base.animator.SetTrigger("show_coin");
			break;
		case Type.Tooltip:
			confirmGlyph.SetActive(value: true);
			tooltipEquipGlyph.SetActive(value: false);
			StartCoroutine(TooltipInRoutine());
			base.animator.SetTrigger("show_tooltip");
			AudioManager.Play("menu_cardup");
			break;
		case Type.TooltipEquip:
			confirmGlyph.SetActive(value: false);
			tooltipEquipGlyph.SetActive(value: true);
			StartCoroutine(TooltipEquipInRoutine());
			base.animator.SetTrigger("show_tooltip");
			AudioManager.Play("menu_cardup");
			break;
		case Type.DLCAvailable:
			GetComponent<Animator>().enabled = false;
			base.transform.Find("Darker").gameObject.SetActive(value: false);
			base.transform.Find("Background").gameObject.SetActive(value: false);
			base.transform.Find("Text").gameObject.SetActive(value: false);
			base.transform.Find("LetterboxTop").gameObject.SetActive(value: false);
			base.transform.Find("LetterboxBottom").gameObject.SetActive(value: false);
			confirmGlyph.SetActive(value: true);
			notificationLocalizationHelper.textComponent.text = string.Empty;
			dlcUI.ShowMenu();
			StartCoroutine(DLCAvailableRoutine());
			break;
		case Type.AirplaneIngredient:
		{
			confirmGlyph.SetActive(value: true);
			airplaneIngred.SetActive(value: true);
			StartCoroutine(IngredientRoutine());
			base.animator.SetTrigger("show_ingred_airplane");
			AudioManager.Play("sfx_dlc_worldmap_ingredient");
			TranslationElement translationElement = Localization.Find("AirplaneIngredient");
			localizationHelper.ApplyTranslation(translationElement);
			localizationHelper.textMeshProComponent.text = localizationHelper.textMeshProComponent.text.ToUpper().Replace("\\N", "\\n");
			string newValue = ((Localization.language != Localization.Languages.Japanese) ? " " : string.Empty);
			notificationLocalizationHelper.ApplyTranslation(Localization.Find("UnlockIngredient"), new LocalizationHelper.LocalizationSubtext[1]
			{
				new LocalizationHelper.LocalizationSubtext("INGREDIENT", translationElement.translation.text.Replace("\\n", newValue))
			});
			break;
		}
		case Type.RumIngredient:
		{
			confirmGlyph.SetActive(value: true);
			rumIngred.SetActive(value: true);
			StartCoroutine(IngredientRoutine());
			base.animator.SetTrigger("show_ingred_rum");
			TranslationElement translationElement = Localization.Find("RumIngredient");
			AudioManager.Play("sfx_dlc_worldmap_ingredient");
			localizationHelper.ApplyTranslation(translationElement);
			localizationHelper.textMeshProComponent.text = localizationHelper.textMeshProComponent.text.ToUpper().Replace("\\N", "\\n");
			string newValue = ((Localization.language != Localization.Languages.Japanese) ? " " : string.Empty);
			notificationLocalizationHelper.ApplyTranslation(Localization.Find("UnlockIngredient"), new LocalizationHelper.LocalizationSubtext[1]
			{
				new LocalizationHelper.LocalizationSubtext("INGREDIENT", translationElement.translation.text.Replace("\\n", newValue))
			});
			break;
		}
		case Type.OldManIngredient:
		{
			confirmGlyph.SetActive(value: true);
			oldManIngred.SetActive(value: true);
			StartCoroutine(IngredientRoutine());
			base.animator.SetTrigger("show_ingred_oldman");
			AudioManager.Play("sfx_dlc_worldmap_ingredient");
			TranslationElement translationElement = Localization.Find("OldManIngredient");
			localizationHelper.ApplyTranslation(translationElement);
			localizationHelper.textMeshProComponent.text = localizationHelper.textMeshProComponent.text.ToUpper().Replace("\\N", "\\n");
			string newValue = ((Localization.language != Localization.Languages.Japanese) ? " " : string.Empty);
			notificationLocalizationHelper.ApplyTranslation(Localization.Find("UnlockIngredient"), new LocalizationHelper.LocalizationSubtext[1]
			{
				new LocalizationHelper.LocalizationSubtext("INGREDIENT", translationElement.translation.text.Replace("\\n", newValue))
			});
			break;
		}
		case Type.SnowIngredient:
		{
			confirmGlyph.SetActive(value: true);
			snowCultIngred.SetActive(value: true);
			StartCoroutine(IngredientRoutine());
			base.animator.SetTrigger("show_ingred_snowcult");
			AudioManager.Play("sfx_dlc_worldmap_ingredient");
			TranslationElement translationElement = Localization.Find("SnowCultIngredient");
			localizationHelper.ApplyTranslation(translationElement);
			localizationHelper.textMeshProComponent.text = localizationHelper.textMeshProComponent.text.ToUpper().Replace("\\N", "\\n");
			string newValue = ((Localization.language != Localization.Languages.Japanese) ? " " : string.Empty);
			notificationLocalizationHelper.ApplyTranslation(Localization.Find("UnlockIngredient"), new LocalizationHelper.LocalizationSubtext[1]
			{
				new LocalizationHelper.LocalizationSubtext("INGREDIENT", translationElement.translation.text.Replace("\\n", newValue))
			});
			break;
		}
		case Type.CowboyIngredient:
		{
			confirmGlyph.SetActive(value: true);
			cowboyIngred.SetActive(value: true);
			StartCoroutine(IngredientRoutine());
			base.animator.SetTrigger("show_ingred_cowboy");
			AudioManager.Play("sfx_dlc_worldmap_ingredient");
			TranslationElement translationElement = Localization.Find("CowboyIngredient");
			localizationHelper.ApplyTranslation(translationElement);
			localizationHelper.textMeshProComponent.text = localizationHelper.textMeshProComponent.text.ToUpper().Replace("\\N", "\\n");
			string newValue = ((Localization.language != Localization.Languages.Japanese) ? " " : string.Empty);
			notificationLocalizationHelper.ApplyTranslation(Localization.Find("UnlockIngredient"), new LocalizationHelper.LocalizationSubtext[1]
			{
				new LocalizationHelper.LocalizationSubtext("INGREDIENT", translationElement.translation.text.Replace("\\n", newValue))
			});
			break;
		}
		case Type.CoinVariable:
			confirmGlyph.SetActive(value: true);
			coinVariable.SetActive(value: true);
			coinVariableText.text = "x" + coinVariableCount;
			coinVariableText.enabled = true;
			AudioManager.Play("world_map_coin_open");
			StartCoroutine(CoinInRoutine());
			notificationLocalizationHelper.ApplyTranslation(Localization.Find("GotACoin"));
			base.animator.SetTrigger("show_coinvariable");
			break;
		case Type.Djimmi:
		{
			AudioManager.Play("sfx_worldmap_djimmi_open");
			TranslationElement translationElement = Localization.Find("GameDjimmi_Tooltip_Wish" + (3 - PlayerData.Data.djimmiWishes));
			notificationLocalizationHelper.ApplyTranslation(translationElement);
			base.animator.SetTrigger("show_djimmi");
			StartCoroutine(DjimmiRoutine());
			break;
		}
		case Type.DjimmiFreed:
		{
			AudioManager.Play("sfx_worldmap_djimmi_open");
			TranslationElement translationElement = Localization.Find("GameDjimmi_Tooltip_Freed");
			notificationLocalizationHelper.ApplyTranslation(translationElement);
			base.animator.SetTrigger("show_djimmi");
			StartCoroutine(DjimmiRoutine());
			break;
		}
		}
		showing = true;
		StartCoroutine(tweenIn_cr());
	}

	public void ShowTooltipEvent(TooltipEvent tooltipEvent)
	{
		InterruptingPrompt.SetCanInterrupt(canInterrupt: true);
		switch (tooltipEvent)
		{
		case TooltipEvent.Turtle:
			tooltipPortrait.sprite = TurtleSprite;
			tooltipLocalizationHelper.ApplyTranslation(Localization.Find("Pacifist_Tooltip_NewAudioVisMode"));
			ShowEvent(Type.Tooltip);
			break;
		case TooltipEvent.Canteen:
			tooltipPortrait.sprite = CanteenSprite;
			tooltipLocalizationHelper.ApplyTranslation(Localization.Find("Canteen_Tooltip_ShmupWeapons"));
			ShowEvent(Type.Tooltip);
			break;
		case TooltipEvent.ShopKeep:
			tooltipPortrait.sprite = ShopkeepSprite;
			tooltipLocalizationHelper.ApplyTranslation(Localization.Find("Shopkeeper_Tooltip_NewPurchase"));
			ShowEvent(Type.TooltipEquip);
			break;
		case TooltipEvent.Professional:
			tooltipPortrait.sprite = ForkSprite;
			tooltipLocalizationHelper.ApplyTranslation(Localization.Find("Professional_Tooltip_SuperEquip"));
			ShowEvent(Type.Tooltip);
			break;
		case TooltipEvent.KingDice:
			tooltipPortrait.sprite = KingDiceSprite;
			tooltipLocalizationHelper.ApplyTranslation(Localization.Find("KingDice_Tooltip_RegularSoulContracts"));
			ShowEvent(Type.Tooltip);
			break;
		case TooltipEvent.Mausoleum:
			tooltipPortrait.sprite = MausoleumSprite;
			tooltipLocalizationHelper.ApplyTranslation(Localization.Find("Chalice_Tooltip_NewSuperEquip"));
			ShowEvent(Type.TooltipEquip);
			break;
		case TooltipEvent.Boatman:
			tooltipPortrait.sprite = BoatmanSprite;
			tooltipLocalizationHelper.ApplyTranslation(Localization.Find("Boatman_Tooltip_UpgradedSave"));
			ShowEvent(Type.Tooltip);
			break;
		case TooltipEvent.SimpleIngredient:
			tooltipPortrait.sprite = SaltbakerSpriteA;
			tooltipLocalizationHelper.ApplyTranslation(Localization.Find("Saltbaker_Tooltip_SimpleIngredient"));
			ShowEvent(Type.Tooltip);
			break;
		case TooltipEvent.Chalice:
			tooltipPortrait.sprite = SaltbakerSpriteB;
			tooltipLocalizationHelper.ApplyTranslation(Localization.Find("Chalice_Tooltip_CharmEquip"));
			ShowEvent(Type.TooltipEquip);
			break;
		case TooltipEvent.ChaliceTutorialEquipCharm:
			tooltipPortrait.sprite = SaltbakerSpriteA;
			tooltipLocalizationHelper.ApplyTranslation(Localization.Find((!PlayerManager.Multiplayer) ? "Saltbaker_Tooltip_ChaliceTutorialSingle" : "Saltbaker_Tooltip_ChaliceTutorialMulti"));
			ShowEvent(Type.TooltipEquip);
			break;
		case TooltipEvent.BackToKitchen:
			tooltipPortrait.sprite = ChaliceSprite;
			tooltipLocalizationHelper.ApplyTranslation(Localization.Find("Chalice_Tooltip_GotAllIngredients"));
			ShowEvent(Type.Tooltip);
			break;
		case TooltipEvent.ChaliceFan:
			tooltipPortrait.sprite = ChaliceFanSprite;
			tooltipLocalizationHelper.ApplyTranslation(Localization.Find("ChaliceFan_Tooltip_NewFilter"));
			ShowEvent(Type.Tooltip);
			break;
		default:
			tooltipPortrait.sprite = null;
			tooltipLocalizationHelper.ApplyTranslation(Localization.Find("Shopkeeper_Tooltip_NewPurchase"));
			ShowEvent(Type.Tooltip);
			break;
		}
	}

	protected IEnumerator CoinInRoutine()
	{
		yield return new WaitForSeconds(1f);
		coinShowing = true;
	}

	protected IEnumerator TooltipInRoutine()
	{
		yield return new WaitForSeconds(1f);
		tooltipShowing = true;
	}

	protected IEnumerator TooltipEquipInRoutine()
	{
		yield return new WaitForSeconds(1f);
		tooltipEquipShowing = true;
	}

	protected IEnumerator SuperInRoutine()
	{
		yield return new WaitForSeconds(1f);
		superShowing = true;
	}

	protected IEnumerator DLCAvailableRoutine()
	{
		yield return new WaitForSeconds(1f);
		dlcAvailableShowing = true;
	}

	protected IEnumerator IngredientRoutine()
	{
		ingredientStarburst.SetActive(value: true);
		yield return new WaitForSeconds(1f);
		ingredientShowing = true;
	}

	private void AniEvent_DjimmiAppear()
	{
		AudioManager.Play("sfx_worldmap_djimmi_entrance");
	}

	private void AniEvent_DjimmiLaugh()
	{
		AudioManager.Play("sfx_worldmap_djimmi_laugh");
	}

	private void AniEvent_DjimmiMagicLoop()
	{
		AudioManager.PlayLoop("sfx_worldmap_djimmi_magic");
		AudioManager.FadeSFXVolumeLinear("sfx_worldmap_djimmi_magic", 0.5f, 0.5f);
	}

	protected IEnumerator DjimmiRoutine()
	{
		yield return new WaitForSeconds(1f);
		djimmiShowing = true;
		yield return base.animator.WaitForAnimationToStart(this, "anim_map_djimmi_out");
		AudioManager.Play("sfx_worldmap_djimmi_disappear");
		AudioManager.Stop("sfx_worldmap_djimmi_magic");
		StartCoroutine(tweenOut_cr());
	}

	protected IEnumerator tweenIn_cr()
	{
		float t = 0f;
		while (t < 0.2f)
		{
			float val = t / 0.2f;
			canvasGroup.alpha = Mathf.Lerp(0f, 1f, val);
			t += Time.deltaTime;
			yield return null;
		}
		canvasGroup.alpha = 1f;
	}

	protected IEnumerator tweenOut_cr(float time = 1.5f)
	{
		AudioManager.FadeSFXVolume("world_map_soul_contract_stamp_shimmer_loop", 0f, 5f);
		AudioManager.FadeSFXVolume("world_map_super_loop", 0f, 5f);
		yield return new WaitForSeconds(time);
		float t = 0f;
		while (t < 0.2f)
		{
			float val = t / 0.2f;
			canvasGroup.alpha = Mathf.Lerp(1f, 0f, val);
			t += Time.deltaTime;
			yield return null;
		}
		canvasGroup.alpha = 0f;
		while (InterruptingPrompt.IsInterrupting())
		{
			yield return null;
		}
		showing = false;
		base.gameObject.SetActive(value: false);
	}

	private IEnumerator text_cr()
	{
		yield return StartCoroutine(textScale_cr(0.9f, 1.1f, 0.5f));
		yield return StartCoroutine(textScale_cr(1.1f, 0.9f, 0.5f));
		while (!input.GetButtonDown(CupheadButton.Accept))
		{
			yield return null;
		}
		showing = false;
		base.gameObject.SetActive(value: false);
	}

	protected IEnumerator textScale_cr(float start, float end, float time)
	{
		float t = 0f;
		while (t < time)
		{
			float val = t / time;
			text.transform.localScale = Vector3.one * EaseUtils.Ease(EaseUtils.EaseType.easeInOutSine, start, end, val);
			t += Time.deltaTime;
			yield return null;
		}
		text.transform.localScale = Vector3.one * end;
		yield return null;
	}
}
