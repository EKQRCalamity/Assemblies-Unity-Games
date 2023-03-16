using System;
using System.Collections;
using RektTransform;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class LevelGameOverGUI : AbstractMonoBehaviour
{
	private enum State
	{
		Init,
		Ready,
		Exiting
	}

	[Serializable]
	public class TimelineObjects
	{
		public RectTransform timeline;

		public RectTransform line;

		[Header("Players")]
		public Image cuphead;

		public Image mugman;

		public Image chalice;

		[Header("Positions")]
		public Transform start;

		public Transform end;

		private LevelGameOverGUI gui;

		public void Setup(LevelGameOverGUI gui, Level.Timeline properties)
		{
			int num = 0;
			foreach (Level.Timeline.Event @event in properties.events)
			{
				RectTransform rectTransform = UnityEngine.Object.Instantiate(line);
				rectTransform.SetParent(line.parent, worldPositionStays: false);
				rectTransform.SetAsFirstSibling();
				rectTransform.name = "Line " + num++;
				Vector3 localPosition = Vector3.Lerp(end.localPosition, start.localPosition, @event.percentage);
				localPosition.y -= 7f;
				rectTransform.localPosition = localPosition;
			}
			line.gameObject.SetActive(value: false);
			Image image = (PlayerManager.GetPlayer(PlayerId.PlayerOne).stats.isChalice ? chalice : ((!PlayerManager.player1IsMugman) ? cuphead : mugman));
			float num2 = ((!PlayerManager.player1IsMugman) ? properties.cuphead : properties.mugman);
			gui.StartCoroutine(timelineIcon_cr(image, num2 / properties.health));
			Image image2 = null;
			if (PlayerManager.Multiplayer)
			{
				image2 = (PlayerManager.GetPlayer(PlayerId.PlayerTwo).stats.isChalice ? chalice : ((!PlayerManager.player1IsMugman) ? mugman : cuphead));
				float num3 = ((!PlayerManager.player1IsMugman) ? properties.mugman : properties.cuphead);
				gui.StartCoroutine(timelineIcon_cr(image2, num3 / properties.health));
			}
			cuphead.gameObject.SetActive(image == cuphead || image2 == cuphead);
			mugman.gameObject.SetActive(image == mugman || image2 == mugman);
			chalice.gameObject.SetActive(image == chalice || image2 == chalice);
		}

		private IEnumerator timelineIcon_cr(Image icon, float percent)
		{
			Color startColor = new Color(1f, 1f, 1f, 0f);
			Color endColor = new Color(1f, 1f, 1f, 1f);
			float t = 0f;
			Vector3 endPosition = Vector3.Lerp(start.localPosition, end.localPosition, percent);
			icon.rectTransform.localPosition = start.localPosition;
			while (t < 2f)
			{
				float val = t / 2f;
				Vector3 newPosition = Vector3.Lerp(start.localPosition, endPosition, EaseUtils.Ease(EaseUtils.EaseType.easeOutSine, 0f, 1f, val));
				icon.rectTransform.localPosition = newPosition;
				icon.color = Color.Lerp(startColor, endColor, val * 8f);
				t += Time.deltaTime;
				yield return null;
			}
			icon.rectTransform.localPosition = endPosition;
		}
	}

	public static LevelGameOverGUI Current;

	[SerializeField]
	private Image youDiedText;

	[Space(10f)]
	[SerializeField]
	private CanvasGroup cardCanvasGroup;

	[Space(10f)]
	[SerializeField]
	private CanvasGroup helpCanvasGroup;

	[Space(10f)]
	[SerializeField]
	private Image bossPortraitImage;

	[SerializeField]
	private Text bossQuoteText;

	[SerializeField]
	private LocalizationHelper bossQuoteLocalization;

	[Space(10f)]
	[SerializeField]
	private Text[] menuItems;

	[SerializeField]
	private TimelineObjects timeline;

	[SerializeField]
	private GameObject timelineObj;

	[SerializeField]
	private Sprite timelineSecret;

	[SerializeField]
	private LevelEquipUI equipUI;

	[SerializeField]
	private GameObject equipToolTip;

	private State state;

	private CupheadInput.AnyPlayerInput input;

	private CanvasGroup canvasGroup;

	private int selection;

	[SerializeField]
	private LocalizationHelper retryLocHelper;

	public static Color COLOR_SELECTED { get; private set; }

	public static Color COLOR_INACTIVE { get; private set; }

	public static Color COLOR_DESABLE { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		Current = this;
		canvasGroup = GetComponent<CanvasGroup>();
		base.gameObject.SetActive(value: false);
		input = new CupheadInput.AnyPlayerInput();
		cardCanvasGroup.alpha = 0f;
		helpCanvasGroup.alpha = 0f;
		ignoreGlobalTime = true;
		timeLayer = CupheadTime.Layer.UI;
		COLOR_SELECTED = menuItems[0].color;
		COLOR_INACTIVE = menuItems[menuItems.Length - 1].color;
		if (Level.IsTowerOfPower)
		{
			equipToolTip.SetActive(value: false);
			if (!TowerOfPowerLevelGameInfo.IsTokenLeft())
			{
				menuItems[0].gameObject.SetActive(value: false);
				selection = 1;
				UpdateSelection();
			}
			else
			{
				retryLocHelper.currentID = Localization.Find("OptionMenuRetryTowerBattle").id;
				retryLocHelper.ApplyTranslation();
			}
		}
		state = State.Init;
	}

	private void Start()
	{
		if (Level.Current != null && Level.Current.CurrentLevel == Levels.Airplane)
		{
			updateRotateControlsToggleVisualValue();
		}
	}

	private void OnDestroy()
	{
		Current = null;
		youDiedText = null;
		bossPortraitImage = null;
		timeline.cuphead = null;
		timeline.mugman = null;
		timeline = null;
	}

	private void Update()
	{
		if (state != State.Ready)
		{
			return;
		}
		if (selection == 2 && Level.Current != null && Level.Current.CurrentLevel == Levels.Airplane && (getButtonDown(CupheadButton.Accept) || getButtonDown(CupheadButton.MenuLeft) || getButtonDown(CupheadButton.MenuRight)))
		{
			AudioManager.Play("level_menu_card_down");
			toggleRotateControls();
			return;
		}
		int num = 0;
		if (getButtonDown(CupheadButton.Accept))
		{
			Select();
			AudioManager.Play("level_menu_select");
			state = State.Exiting;
		}
		if (!Level.IsTowerOfPower && getButtonDown(CupheadButton.EquipMenu))
		{
			ChangeEquipment();
		}
		if (getButtonDown(CupheadButton.MenuDown))
		{
			AudioManager.Play("level_menu_move");
			num++;
		}
		if (getButtonDown(CupheadButton.MenuUp))
		{
			AudioManager.Play("level_menu_move");
			num--;
		}
		selection += num;
		selection = Mathf.Clamp(selection, 0, menuItems.Length - 1);
		if (!menuItems[selection].gameObject.activeSelf)
		{
			selection -= num;
			selection = Mathf.Clamp(selection, 0, menuItems.Length - 1);
		}
		UpdateSelection();
	}

	private bool getButtonDown(CupheadButton button)
	{
		return input.GetButtonDown(button);
	}

	private void UpdateSelection()
	{
		for (int i = 0; i < menuItems.Length; i++)
		{
			Text text = menuItems[i];
			if (i == selection)
			{
				text.color = COLOR_SELECTED;
			}
			else
			{
				text.color = COLOR_INACTIVE;
			}
		}
	}

	private void Select()
	{
		if (!Level.IsGraveyard)
		{
			AudioManager.SnapshotReset(SceneLoader.SceneName, 2f);
			AudioManager.ChangeBGMPitch(1f, 2f);
		}
		if (Level.Current != null && Level.Current.CurrentLevel == Levels.Airplane)
		{
			SettingsData.Save();
			if (PlatformHelper.IsConsole)
			{
				SettingsData.SaveToCloud();
			}
		}
		switch (selection)
		{
		default:
			Retry();
			AudioManager.Play("level_menu_card_down");
			break;
		case 1:
			ExitToMap();
			AudioManager.Play("level_menu_card_down");
			break;
		case 2:
			QuitGame();
			AudioManager.Play("level_menu_card_down");
			break;
		}
	}

	private void Retry()
	{
		if (Level.IsDicePalaceMain || Level.IsDicePalace)
		{
			DicePalaceMainLevelGameInfo.CleanUpRetry();
		}
		SceneLoader.ReloadLevel();
	}

	private void ExitToMap()
	{
		SceneLoader.LoadLastMap();
	}

	private void QuitGame()
	{
		Level.IsGraveyard = false;
		PlayerManager.ResetPlayers();
		SceneLoader.LoadScene(Scenes.scene_title, SceneLoader.Transition.Fade, SceneLoader.Transition.Iris);
	}

	private void ChangeEquipment()
	{
		StartCoroutine(outforequip_cr());
	}

	public void ReactivateOnChangeEquipmentClosed()
	{
		StartCoroutine(inforequip_cr());
	}

	private void SetAlpha(float value)
	{
		canvasGroup.alpha = value;
	}

	private void SetTextAlpha(float value)
	{
		Color color = youDiedText.color;
		color.a = value;
		youDiedText.color = color;
	}

	private void SetCardValue(float value)
	{
		cardCanvasGroup.alpha = value;
		helpCanvasGroup.alpha = value;
		cardCanvasGroup.transform.SetLocalEulerAngles(null, null, Mathf.Lerp(30f, 4f, value));
	}

	private void SetCardValueEquipSwap(float value)
	{
		cardCanvasGroup.alpha = value;
		helpCanvasGroup.alpha = value;
		cardCanvasGroup.transform.SetLocalEulerAngles(null, null, Mathf.Lerp(30f, 4f, value));
		cardCanvasGroup.transform.SetLocalPosition(null, Mathf.Lerp(-720f, 0f, value));
	}

	public void In(bool secretTriggered)
	{
		base.gameObject.SetActive(value: true);
		bossPortraitImage.sprite = Level.Current.BossPortrait;
		if (secretTriggered)
		{
			cardCanvasGroup.GetComponent<Image>().sprite = timelineSecret;
			timelineObj.SetActive(value: false);
		}
		if (bossQuoteLocalization == null)
		{
			bossQuoteText.text = "\"" + Level.Current.BossQuote + "\"";
		}
		else
		{
			bossQuoteLocalization.ApplyTranslation(Localization.Find(Level.Current.BossQuote));
			if (Localization.language == Localization.Languages.Korean)
			{
				bossQuoteLocalization.textMeshProComponent.fontStyle = FontStyles.Bold;
			}
		}
		if (bossPortraitImage.sprite != null)
		{
			bossPortraitImage.rectTransform.SetSize(bossPortraitImage.sprite.rect.width, bossPortraitImage.sprite.rect.height);
		}
		StartCoroutine(in_cr());
	}

	private IEnumerator in_cr()
	{
		AudioManager.Play("level_menu_card_up");
		yield return TweenValue(0f, 1f, 0.05f, EaseUtils.EaseType.linear, SetAlpha);
		yield return new WaitForSeconds(1f);
		PlayerDeathEffect[] array = UnityEngine.Object.FindObjectsOfType<PlayerDeathEffect>();
		foreach (PlayerDeathEffect playerDeathEffect in array)
		{
			playerDeathEffect.GameOverUnpause();
		}
		PlanePlayerDeathPart[] array2 = UnityEngine.Object.FindObjectsOfType<PlanePlayerDeathPart>();
		foreach (PlanePlayerDeathPart planePlayerDeathPart in array2)
		{
			planePlayerDeathPart.GameOverUnpause();
		}
		yield return TweenValue(1f, 0f, 0.25f, EaseUtils.EaseType.linear, SetTextAlpha);
		yield return new WaitForSeconds(0.3f);
		if (!Level.IsGraveyard && !Level.IsChessBoss)
		{
			AudioManager.Play("player_die_vinylscratch");
			AudioManager.HandleSnapshot(AudioManager.Snapshots.Death.ToString(), 4f);
			AudioManager.ChangeBGMPitch(0.7f, 6f);
		}
		CupheadLevelCamera.Current.StartBlur();
		timeline.Setup(this, Level.Current.timeline);
		TweenValue(0f, 1f, 0.3f, EaseUtils.EaseType.easeOutCubic, SetCardValue);
		state = State.Ready;
		yield return null;
	}

	private IEnumerator outforequip_cr()
	{
		state = State.Init;
		equipUI.gameObject.SetActive(value: true);
		equipUI.Activate();
		yield return TweenValue(1f, 0f, 0.3f, EaseUtils.EaseType.easeOutCubic, SetCardValueEquipSwap);
	}

	private IEnumerator inforequip_cr()
	{
		yield return TweenValue(0f, 1f, 0.3f, EaseUtils.EaseType.easeOutCubic, SetCardValueEquipSwap);
		state = State.Ready;
	}

	private void toggleRotateControls()
	{
		SettingsData.Data.rotateControlsWithCamera = !SettingsData.Data.rotateControlsWithCamera;
		updateRotateControlsToggleVisualValue();
	}

	private void updateRotateControlsToggleVisualValue()
	{
		Text text = menuItems[2];
		text.GetComponent<LocalizationHelper>().ApplyTranslation(Localization.Find("CameraRotationControl"));
		text.text = string.Format(text.text, (!SettingsData.Data.rotateControlsWithCamera) ? "A" : "B");
	}
}
