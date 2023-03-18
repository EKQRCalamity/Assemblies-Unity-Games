using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Framework.Achievements;
using Framework.BossRush;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.Isidora;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using Gameplay.UI.Others.Buttons;
using Gameplay.UI.Others.MenuLogic;
using Gameplay.UI.Others.UIGameLogic;
using Gameplay.UI.Widgets;
using I2.Loc;
using Rewired;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gameplay.UI;

[RequireComponent(typeof(Canvas))]
[DefaultExecutionOrder(-1)]
public class UIController : SerializedMonoBehaviour
{
	public enum FullMensages
	{
		BossDefeated,
		ConfessorArea,
		EndBossDefeated
	}

	public enum PopupItemAction
	{
		GetObejct,
		GiveObject
	}

	public static UIController instance;

	[SerializeField]
	private DeadScreenWidget deadScreen;

	[SerializeField]
	public Image fade;

	[SerializeField]
	private ExtrasMenuWidget extrasMenu;

	[SerializeField]
	private NewInventoryWidget newInventoryMenu;

	[SerializeField]
	private PopUpWidget popUpWidget;

	[SerializeField]
	private BossHealth bossHealth;

	[SerializeField]
	private GameplayWidget gameplayWidget;

	[SerializeField]
	private FadeWidget fadeWidget;

	[SerializeField]
	private GlowWidget glowWidget;

	[SerializeField]
	private DialogWidget dialogWidget;

	[SerializeField]
	private GameObject loadWidget;

	[SerializeField]
	private GameObject loadWidgetDemake;

	[SerializeField]
	private NewMainMenu mainMenuWidget;

	[SerializeField]
	private GameObject content;

	[SerializeField]
	private SubtitleWidget subtitleWidget;

	[SerializeField]
	private GameObject tutorialWidget;

	[SerializeField]
	private GameObject unlockWidget;

	[SerializeField]
	private GameObject fullMessages;

	[SerializeField]
	private KneelPopUpWidget kneelMenuWidget;

	[SerializeField]
	private PauseWidget pauseWidget;

	[SerializeField]
	private PatchNotesWidget patchNotesWidget;

	[SerializeField]
	private UpgradeFlasksWidget upgradeFlasksWidget;

	[SerializeField]
	private ChoosePenitenceWidget choosePenitenceWidget;

	[SerializeField]
	private QuoteWidget quoteWidget;

	[SerializeField]
	private ConfirmationWidget confirmationWidget;

	[SerializeField]
	private AbandonPenitenceWidget abandonPenitenceWidget;

	[SerializeField]
	private AlmsWidget almsWidget;

	[SerializeField]
	private TeleportWidget teleportWidget;

	[SerializeField]
	private BossRushRankWidget bossRushRankWidget;

	[SerializeField]
	private PopupAchievementWidget popupAchievementWidget;

	[SerializeField]
	private IntroDemakeWidget introDemakeWidget;

	[SerializeField]
	private ModeUnlockedWidget modeUnlockedWidget;

	private List<BasicUIBlockingWidget> allUIBLockingWidgets = new List<BasicUIBlockingWidget>();

	public List<FontsByLanguage> fontsByLanguage;

	private bool _canEquipSwordHearts;

	public bool CanOpenInventory = true;

	[SerializeField]
	private Dictionary<FullMensages, GameObject> fullMessagesConfig = new Dictionary<FullMensages, GameObject>();

	[SerializeField]
	private float waitTimeToShowZone = 1f;

	[TutorialId]
	[SerializeField]
	[BoxGroup("Tutorial", true, false, 0)]
	private string TutorialPrayer;

	private bool paused;

	private Player rewired;

	private CanvasGroup fullMessageCanvas;

	[SerializeField]
	[BoxGroup("Sound", true, false, 0)]
	private string sfxOpenOptions = "event:/SFX/UI/ChangeTab";

	private bool firstRun = true;

	public bool CanEquipSwordHearts
	{
		get
		{
			return _canEquipSwordHearts;
		}
		set
		{
			_canEquipSwordHearts = value;
		}
	}

	public bool BossRushRetryPressed { get; private set; }

	public bool IsShowingMenu => newInventoryMenu.currentlyActive || pauseWidget.IsActive();

	public bool IsShowingInventory => newInventoryMenu.currentlyActive;

	public bool Paused => Math.Abs(Core.Logic.CurrentLevelConfig.TimeScale) < Mathf.Epsilon;

	private void Awake()
	{
		instance = this;
		int childCount = content.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			if (!(base.transform.name == "UNUSED"))
			{
				content.transform.GetChild(i).gameObject.SetActive(value: true);
			}
		}
		EnableCanvas(newInventoryMenu.gameObject, enabled: false);
		tutorialWidget.SetActive(value: false);
		if (unlockWidget != null)
		{
			unlockWidget.SetActive(value: false);
		}
		fullMessageCanvas = fullMessages.GetComponent<CanvasGroup>();
		fullMessageCanvas.alpha = 0f;
		fullMessages.SetActive(value: false);
		if ((bool)EventSystem.current)
		{
			EventSystem.current.SetSelectedGameObject(null);
		}
		patchNotesWidget.gameObject.SetActive(value: false);
		upgradeFlasksWidget.gameObject.SetActive(value: false);
		choosePenitenceWidget.gameObject.SetActive(value: false);
		quoteWidget.gameObject.SetActive(value: false);
		confirmationWidget.gameObject.SetActive(value: false);
		abandonPenitenceWidget.gameObject.SetActive(value: false);
		introDemakeWidget.gameObject.SetActive(value: false);
		modeUnlockedWidget.gameObject.SetActive(value: false);
		gameplayWidget.ShowPurgePoints();
		allUIBLockingWidgets.Clear();
		allUIBLockingWidgets.Add(pauseWidget);
		allUIBLockingWidgets.Add(almsWidget);
		allUIBLockingWidgets.Add(teleportWidget);
		allUIBLockingWidgets.Add(bossRushRankWidget);
		foreach (BasicUIBlockingWidget allUIBLockingWidget in allUIBLockingWidgets)
		{
			allUIBLockingWidget.InitializeWidget();
		}
		firstRun = true;
	}

	private void Start()
	{
		Canvas component = GetComponent<Canvas>();
		component.planeDistance = 1f;
		component.sortingLayerName = "Canvas UI";
		SpawnManager.OnPlayerSpawn += OnPenitentReady;
		LevelManager.OnLevelLoaded += OnLevelLoaded;
		LevelManager.OnBeforeLevelLoad += OnBeforeLevelLoad;
		rewired = ReInput.players.GetPlayer(0);
	}

	private void OnPenitentReady(Penitent penitent)
	{
		penitent.OnDead = (Core.SimpleEvent)Delegate.Combine(penitent.OnDead, new Core.SimpleEvent(OnDead));
	}

	private void OnBeforeLevelLoad(Level oldLevel, Level newLevel)
	{
		popUpWidget.HideAreaPopup();
	}

	private void OnLevelLoaded(Level oldLevel, Level newLevel)
	{
		if (firstRun)
		{
			firstRun = false;
			pauseWidget.ReadOptionConfigurations();
			Debug.LogWarning("<color=yellow>Removing initial input block</color>");
			Core.Input.SetBlocker("InitialBlocker", blocking: false);
		}
		if (fullMessages.activeInHierarchy)
		{
			StopCoroutine("ShowFullMessageCourrutine");
			fullMessages.SetActive(value: false);
		}
	}

	private void OnDestroy()
	{
		SpawnManager.OnPlayerSpawn -= OnPenitentReady;
		LevelManager.OnLevelLoaded -= OnLevelLoaded;
		LevelManager.OnBeforeLevelLoad -= OnBeforeLevelLoad;
		firstRun = false;
	}

	private void Update()
	{
		bool flag = false;
		if (rewired == null)
		{
			return;
		}
		if (dialogWidget.IsShowingDialog())
		{
			flag = true;
			if (rewired.GetButtonDown(39))
			{
				dialogWidget.DialogButtonPressed();
			}
		}
		if (IsTutorialActive() || CreditsWidget.instance != null || (ConsoleWidget.Instance != null && ConsoleWidget.Instance.IsEnabled()) || fullMessages.activeInHierarchy || patchNotesWidget.isOpen || modeUnlockedWidget.isOpen)
		{
			flag = true;
		}
		bool flag2 = rewired.GetButtonDown(51);
		if (!flag && flag2)
		{
			if (newInventoryMenu.currentlyActive)
			{
				StartCoroutine(SafeGoBackInventory());
				flag = true;
			}
			else if (extrasMenu.currentlyActive)
			{
				StartCoroutine(SafeGoBackExtras());
				flag = true;
			}
			else if (quoteWidget.IsOpen)
			{
				quoteWidget.Close();
			}
			else
			{
				foreach (BasicUIBlockingWidget allUIBLockingWidget in allUIBLockingWidgets)
				{
					if (allUIBLockingWidget.AutomaticBack() && allUIBLockingWidget.IsActive() && !allUIBLockingWidget.IsFading && !allUIBLockingWidget.GoBack())
					{
						allUIBLockingWidget.FadeHide();
					}
				}
			}
		}
		if (flag2 && (rewired.GetButtonDown(22) || rewired.GetButtonDown(10)))
		{
			flag2 = false;
		}
		if (!flag && pauseWidget.IsActive())
		{
			if (rewired.GetButtonDown(61))
			{
				flag = true;
				if (pauseWidget.ChangeToOptions() && !string.IsNullOrEmpty(sfxOpenOptions))
				{
					Core.Audio.PlayOneShot(sfxOpenOptions);
				}
			}
			if (rewired.GetButtonDown(60))
			{
				pauseWidget.CenterView();
				flag = true;
			}
			if (rewired.GetButtonDown(50))
			{
				pauseWidget.SubmitPressed();
				flag = true;
			}
			if (rewired.GetButtonDown(28))
			{
				pauseWidget.UITabLeft();
				flag = true;
			}
			if (rewired.GetButtonDown(29))
			{
				pauseWidget.UITabRight();
				flag = true;
			}
		}
		if (!flag && newInventoryMenu.currentlyActive)
		{
			if (rewired.GetButtonDown(64))
			{
				newInventoryMenu.ShowLore();
				flag = true;
			}
			if (rewired.GetButtonDown(28))
			{
				newInventoryMenu.SelectPreviousCategory();
			}
			if (rewired.GetButtonDown(29))
			{
				newInventoryMenu.SelectNextCategory();
			}
		}
		if (!flag && almsWidget.IsActive() && rewired.GetButtonDown(50))
		{
			almsWidget.SubmitPressed();
			flag = true;
		}
		if (!flag && rewired.GetButtonDown(22) && !flag2 && !Core.Input.HasBlocker("INTERACTABLE"))
		{
			bool flag3 = Core.Input.HasBlocker("PLAYER_LOGIC") || Core.Logic.IsSlowMode();
			if (!newInventoryMenu.currentlyActive && flag3)
			{
				flag = true;
			}
			else if (CanOpenBlockWidget() && !kneelMenuWidget.IsShowing && CanOpenInventory)
			{
				if (IsInIsidoraBossfight())
				{
					if (Core.Logic != null && (bool)Core.Logic.Penitent && !Core.Logic.Penitent.Dead)
					{
						ShowPopUp(ScriptLocalization.UI.ISIDORA_MENU_FORBIDDEN, string.Empty, 2f, blockPlayer: false);
					}
					flag = true;
				}
				else
				{
					EnableCanvas(newInventoryMenu.gameObject, enabled: true);
					newInventoryMenu.Show(!newInventoryMenu.currentlyActive);
					flag = true;
					if (!newInventoryMenu.currentlyActive)
					{
						CheckPrayerTutorial();
					}
				}
			}
		}
		if (flag || !rewired.GetButtonDown(10) || flag2 || Core.Input.HasBlocker("PLAYER_LOGIC"))
		{
			return;
		}
		flag = true;
		if (pauseWidget.IsActive() && !pauseWidget.OptionsWidget.controlsMenuScreen.enabled)
		{
			pauseWidget.FadeHide();
		}
		else
		{
			if (!CanOpenBlockWidget())
			{
				return;
			}
			if (IsInIsidoraBossfight())
			{
				if (Core.Logic != null && (bool)Core.Logic.Penitent && !Core.Logic.Penitent.Dead)
				{
					ShowPopUp(ScriptLocalization.UI.ISIDORA_MENU_FORBIDDEN, string.Empty, 2f, blockPlayer: false);
				}
				flag = true;
			}
			else
			{
				pauseWidget.InitialWidget = PauseWidget.ChildWidgets.MAP;
				pauseWidget.InitialMapMode = PauseWidget.MapModes.SHOW;
				pauseWidget.FadeShow(checkInput: true);
			}
		}
	}

	private bool IsInIsidoraBossfight()
	{
		return (Core.LevelManager.currentLevel.LevelName.Equals("D01BZ08S01") || Core.LevelManager.currentLevel.LevelName.Equals("D22Z01S18")) && (bool)UnityEngine.Object.FindObjectOfType<IsidoraBehaviour>();
	}

	internal void ShowJoysticksButtons()
	{
		throw new NotImplementedException();
	}

	internal void ShowKeyboardButtons()
	{
		throw new NotImplementedException();
	}

	private void OnDead()
	{
		foreach (BasicUIBlockingWidget allUIBLockingWidget in allUIBLockingWidgets)
		{
			allUIBLockingWidget.FadeHide();
		}
		newInventoryMenu.Show(p_active: false);
	}

	public void ShowKneelMenu(KneelPopUpWidget.Modes mode)
	{
		kneelMenuWidget.ShowPopUp(mode);
		CanEquipSwordHearts = true;
	}

	public void MakeKneelMenuInvisible()
	{
		kneelMenuWidget.HidePopUp();
	}

	public void HideKneelMenu()
	{
		kneelMenuWidget.HidePopUp();
		CanEquipSwordHearts = false;
	}

	public bool IsInventoryMenuPressed()
	{
		return rewired.GetButtonDown(22);
	}

	public bool IsStopKneelPressed()
	{
		return rewired.GetButtonDown(51);
	}

	public bool IsInventoryClosed()
	{
		return !newInventoryMenu.currentlyActive;
	}

	public void CloseInventory()
	{
		StartCoroutine(SafeGoBackInventory());
	}

	public void ToggleInventoryMenu()
	{
		EnableCanvas(newInventoryMenu.gameObject, enabled: true);
		newInventoryMenu.Show(!newInventoryMenu.currentlyActive);
	}

	public void SelectTab(NewInventoryWidget.TabType tab)
	{
		newInventoryMenu.SetTab(tab);
	}

	public bool IsTeleportMenuPressed()
	{
		return rewired.GetButtonDown(6);
	}

	public void ShowPopUp(string message, string eventSound = "", float timeToWait = 0f, bool blockPlayer = true)
	{
		popUpWidget.ShowPopUp(message, eventSound, timeToWait, blockPlayer);
	}

	public void ShowCherubPopUp(string message, string eventSound = "", float timeToWait = 0f, bool blockPlayer = true)
	{
		popUpWidget.ShowCherubPopUp(message, eventSound, timeToWait, blockPlayer);
	}

	public void ShowPopUpObjectUse(string itemName, string eventSound = "")
	{
		string valueWithParam = Core.Localization.GetValueWithParam(ScriptLocalization.UI_Inventory.TEXT_DOOR_USE_OBJECT, "object_caption", itemName);
		string eventSound2 = ((!(eventSound != string.Empty)) ? "event:/Key Event/UseQuestItem" : eventSound);
		ShowPopUp(valueWithParam, eventSound2);
	}

	public void ShowObjectPopUp(PopupItemAction action, string itemName, Sprite image, InventoryManager.ItemType objType, float timeToWait = 3f, bool blockPlayer = false)
	{
		string message = string.Empty;
		switch (action)
		{
		case PopupItemAction.GetObejct:
			message = ScriptLocalization.UI_Inventory.TEXT_ITEM_FOUND;
			break;
		case PopupItemAction.GiveObject:
			message = ScriptLocalization.UI_Inventory.TEXT_ITEM_GIVE;
			break;
		}
		popUpWidget.ShowItemGet(message, itemName, image, objType, timeToWait, blockPlayer);
	}

	public void ShowAreaPopUp(string area, float timeToWait = 3f, bool blockPlayer = false)
	{
		StartCoroutine(ShowPopUpDelayed(area, waitTimeToShowZone, timeToWait, blockPlayer));
	}

	public IEnumerator ShowPopUpDelayed(string area, float timeToShow = 1f, float timeToWait = 3f, bool blockPlayer = false)
	{
		popUpWidget.WaitingToShowArea = true;
		yield return new WaitForSeconds(timeToShow);
		popUpWidget.ShowAreaPopUp(area, timeToWait, blockPlayer);
	}

	public bool IsShowingPopUp()
	{
		return popUpWidget.IsShowing;
	}

	public void ShowUnlockSKill()
	{
		if (!newInventoryMenu.currentlyActive)
		{
			ConsoleWidget.Instance.SetEnabled(enabled: false);
			EnableCanvas(newInventoryMenu.gameObject, enabled: true);
			newInventoryMenu.ShowSkills(p_active: true);
		}
	}

	public void ShowTeleportUI()
	{
		if (!newInventoryMenu.currentlyActive)
		{
			ConsoleWidget.Instance.SetEnabled(enabled: false);
			teleportWidget.FadeShow(checkInput: false);
		}
	}

	public void HideInventory()
	{
		if (newInventoryMenu.currentlyActive)
		{
			newInventoryMenu.Show(p_active: false);
		}
	}

	public IEnumerator ShowUnlockSKillCourrutine()
	{
		ShowUnlockSKill();
		while (newInventoryMenu.currentlyActive)
		{
			yield return 0;
		}
	}

	public IEnumerator ShowAlmsWidgetCourrutine()
	{
		almsWidget.FadeShow(checkInput: false);
		while (almsWidget.IsActive())
		{
			yield return 0;
		}
	}

	public bool IsTutorialActive()
	{
		return tutorialWidget.activeSelf;
	}

	public GameObject GetTutorialRoot()
	{
		return tutorialWidget;
	}

	public bool IsUnlockActive()
	{
		return unlockWidget.activeSelf;
	}

	public GameObject GetUnlockRoot()
	{
		return unlockWidget;
	}

	public void ShowUnlockPopup(string unlockId)
	{
		if (unlockWidget != null)
		{
			StartCoroutine(ShowUnlock(unlockId));
		}
	}

	public IEnumerator ShowUnlock(string unlockId, bool blockPlayer = true)
	{
		if (blockPlayer)
		{
			Core.Input.SetBlocker("UNLOCK", blocking: true);
			Core.Logic.PauseGame();
		}
		GameObject uiroot = GetUnlockRoot();
		UnlockWidget widget = unlockWidget.GetComponent<UnlockWidget>();
		widget.Configurate(unlockId);
		widget.ShowInGame();
		ShowUnlock(unlockId);
		CanvasGroup gr = widget.GetComponentInChildren<CanvasGroup>();
		gr.alpha = 0f;
		uiroot.SetActive(value: true);
		DOTween.defaultTimeScaleIndependent = true;
		DOTween.To(() => gr.alpha, delegate(float x)
		{
			gr.alpha = x;
		}, 1f, 1f);
		while (!widget.WantToExit)
		{
			yield return null;
		}
		TweenerCore<float, float, FloatOptions> teen = DOTween.To(() => gr.alpha, delegate(float x)
		{
			gr.alpha = x;
		}, 0f, 1f);
		yield return new WaitForSecondsRealtime(0.5f);
		if (blockPlayer)
		{
			Core.Input.SetBlocker("UNLOCK", blocking: false);
			Core.Logic.ResumeGame();
		}
		uiroot.SetActive(value: false);
		DOTween.defaultTimeScaleIndependent = false;
		yield return null;
	}

	public void ShowFullMessage(FullMensages message, float totalTime, float fadeInTime, float fadeOutTime)
	{
		StartCoroutine(ShowFullMessageCourrutine(message, totalTime, fadeInTime, fadeOutTime));
	}

	public IEnumerator ShowFullMessageCourrutine(FullMensages message, float totalTime, float fadeInTime, float fadeOutTime)
	{
		fullMessageCanvas.alpha = 0f;
		foreach (KeyValuePair<FullMensages, GameObject> item in fullMessagesConfig)
		{
			item.Value.SetActive(item.Key == message);
		}
		fullMessages.SetActive(value: true);
		Tweener myTween2 = fullMessageCanvas.DOFade(1f, fadeInTime);
		yield return myTween2.WaitForCompletion();
		yield return new WaitForSeconds(totalTime);
		if (fadeOutTime >= 0f)
		{
			myTween2 = fullMessageCanvas.DOFade(0f, fadeOutTime);
			yield return myTween2.WaitForCompletion();
			fullMessages.SetActive(value: false);
		}
	}

	public void PlayBossRushRankAudio(bool complete)
	{
		StartCoroutine(bossRushRankWidget.PlayBossRushRankAudio(complete));
	}

	public IEnumerator ShowBossRushRanksAndWait(BossRushHighScore score, bool pauseGame, bool complete, bool unlockHard)
	{
		if (complete)
		{
			PlayBossRushRankAudio(complete: true);
			yield return ShowFullMessageCourrutine(FullMensages.EndBossDefeated, 4f, 1f, -1f);
			Core.UI.Fade.Fade(toBlack: true, 1f);
			yield return new WaitForSecondsRealtime(1f);
			fullMessages.SetActive(value: false);
		}
		bossRushRankWidget.ShowHighScore(score, pauseGame, complete, unlockHard);
		while (bossRushRankWidget.IsFading)
		{
			yield return 0;
		}
		if (Core.Input.HasBlocker("FADE"))
		{
			Core.UI.Fade.Fade(toBlack: false, 0.5f);
		}
		while (bossRushRankWidget.IsActive())
		{
			yield return 0;
		}
		BossRushRetryPressed = bossRushRankWidget.RetryPressed;
	}

	public void ShowPopupAchievement(Achievement achievement)
	{
		if (Core.AchievementsManager.ShowPopUp)
		{
			popupAchievementWidget.ShowPopup(achievement);
		}
	}

	public void ShowPatchNotes()
	{
		patchNotesWidget.Open();
	}

	public bool IsPatchNotesShowing()
	{
		return patchNotesWidget.isOpen;
	}

	public void ShowUpgradeFlasksWidget(float price, Action onUpgradeFlask, Action onContinueWithoutUpgrading)
	{
		upgradeFlasksWidget.Open(price, onUpgradeFlask, onContinueWithoutUpgrading);
	}

	public void ShowChoosePenitenceWidget(Action onChoosingPenitence, Action onContinueWithoutChoosingPenitence)
	{
		choosePenitenceWidget.Open(onChoosingPenitence, onContinueWithoutChoosingPenitence);
	}

	public void ShowAbandonPenitenceWidget(Action onAbandoningPenitence, Action onContinueWithoutAbandoningPenitence)
	{
		abandonPenitenceWidget.Open(onAbandoningPenitence, onContinueWithoutAbandoningPenitence);
	}

	public void ShowQuoteWidget(float fadeInTime, float timeActive, float fadeOutTime, Action onFinish)
	{
		quoteWidget.Open(fadeInTime, timeActive, fadeOutTime, onFinish);
	}

	public void ShowConfirmationWidget(string infoMessage, Action onAccept, Action onBack)
	{
		confirmationWidget.Open(infoMessage, onAccept, onBack);
	}

	public void ShowConfirmationWidget(string infoMessage, string acceptMessage, string dissentMessage, Action onAccept, Action onBack)
	{
		confirmationWidget.Open(infoMessage, acceptMessage, dissentMessage, onAccept, onBack);
	}

	public void ShowIntroDemakeWidget(Action onAccept)
	{
		introDemakeWidget.Open(onAccept);
	}

	public void HideIntroDemakeWidget()
	{
		introDemakeWidget.Close();
	}

	public void ShowModeUnlockedWidget(ModeUnlockedWidget.ModesToUnlock modeUnlocked)
	{
		modeUnlockedWidget.Open(modeUnlocked);
	}

	public void HideModeUnlockedWidget()
	{
		modeUnlockedWidget.Close();
	}

	public bool IsModeUnlockedShowing()
	{
		return modeUnlockedWidget.isOpen;
	}

	public void HideAllNotInGameUI()
	{
		HideInventory();
		HideBossHealth();
		HideMainMenu();
		HidePauseMenu();
		foreach (BasicUIBlockingWidget allUIBLockingWidget in allUIBLockingWidgets)
		{
			allUIBLockingWidget.FadeHide();
		}
	}

	public void ShowBossHealth(Entity entity)
	{
		bossHealth.gameObject.SetActive(value: true);
		bossHealth.SetTarget(entity.gameObject);
		string empty = string.Empty;
		if ((string)entity.displayName == (string)null)
		{
			empty = "NAME NOT SET, PLEASE FIX SCENE";
		}
		else
		{
			empty = entity.displayName.ToString();
			if (empty == string.Empty)
			{
				empty = "[!LOC_" + entity.displayName.mTerm.ToUpper() + "]";
			}
		}
		bossHealth.SetName(empty);
		bossHealth.Show();
	}

	public void HideBossHealth()
	{
		bossHealth.Hide();
		bossHealth.gameObject.SetActive(value: false);
	}

	public void ShowMainMenu(string newInitialScene = "")
	{
		mainMenuWidget.gameObject.SetActive(value: true);
		gameplayWidget.RestoreDefaultPanelsStatus();
		mainMenuWidget.ShowMenu(newInitialScene);
	}

	public IEnumerator ShowMainMenuToChooseBackground()
	{
		yield return new WaitForEndOfFrame();
		mainMenuWidget.gameObject.SetActive(value: true);
		mainMenuWidget.ShowChooseBackground();
	}

	public void HideMainMenu()
	{
		mainMenuWidget.gameObject.SetActive(value: false);
	}

	public GlowWidget GetGlow()
	{
		return glowWidget;
	}

	public void ShowGlow(Color haloColor, float haloDuration)
	{
		glowWidget.color = haloColor;
		glowWidget.Show(haloDuration);
	}

	public DialogWidget GetDialog()
	{
		return dialogWidget;
	}

	public void HidePauseMenu()
	{
		if (pauseWidget.IsActive())
		{
			pauseWidget.FadeHide();
		}
		HideKneelMenu();
	}

	public void ShowLoad(bool show, Color? background = null)
	{
		Color color = ((!background.HasValue) ? Color.black : background.Value);
		if (Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.DEMAKE))
		{
			loadWidgetDemake.gameObject.GetComponent<Image>().color = color;
			loadWidgetDemake.gameObject.SetActive(show);
			loadWidget.gameObject.SetActive(value: false);
		}
		else
		{
			loadWidget.gameObject.GetComponent<Image>().color = color;
			loadWidget.gameObject.SetActive(show);
			loadWidgetDemake.gameObject.SetActive(value: false);
		}
		if (show && fadeWidget.IsActive)
		{
			fadeWidget.SetOnColor(color);
			fadeWidget.Fade(toBlack: false, 0.2f, 0f, delegate
			{
				fadeWidget.ResetToBlack();
			});
		}
	}

	public void UpdatePurgePoints()
	{
		gameplayWidget.UpdatePurgePoints();
	}

	public void UpdateGuiltLevel(bool whenDead)
	{
		gameplayWidget.UpdateGuiltLevel(whenDead);
	}

	public void ShowPurgePoints()
	{
		gameplayWidget.ShowPurgePoints();
	}

	public void HidePurgePoints()
	{
		gameplayWidget.HidePurgePoints();
	}

	public void ShowBossRushTimer()
	{
		gameplayWidget.ShowBossRushTimer();
	}

	public void HideBossRushTimer()
	{
		gameplayWidget.HideBossRushTimer();
	}

	public void NotEnoughFervour()
	{
		gameplayWidget.NotEnoughFervour();
	}

	public void StartMiriamTimer()
	{
		gameplayWidget.StartMiriamTimer();
	}

	public void StopMiriamTimer()
	{
		gameplayWidget.StopMiriamTimer();
	}

	public void SetMiriamTimerTargetTime(float targetTime)
	{
		gameplayWidget.SetMiriamTimerTargetTime(targetTime);
	}

	public void ShowMiriamTimer()
	{
		gameplayWidget.ShowMiriamTimer();
	}

	public void HideMiriamTimer()
	{
		gameplayWidget.HideMiriamTimer();
	}

	public IEnumerator ShowOptions()
	{
		pauseWidget.InitialWidget = PauseWidget.ChildWidgets.OPTIONS;
		pauseWidget.FadeShow(checkInput: false);
		while (pauseWidget.IsActive())
		{
			yield return null;
		}
	}

	public IEnumerator ShowMapTeleport()
	{
		pauseWidget.InitialWidget = PauseWidget.ChildWidgets.MAP;
		pauseWidget.InitialMapMode = PauseWidget.MapModes.TELEPORT;
		pauseWidget.FadeShow(checkInput: false);
		while (pauseWidget.IsActive())
		{
			yield return null;
		}
	}

	public void ShowExtras()
	{
		EnableCanvas(extrasMenu.gameObject, enabled: true);
		extrasMenu.ShowExtras();
	}

	public SubtitleWidget GetSubtitleWidget()
	{
		return subtitleWidget;
	}

	public OptionsWidget GetOptionsWidget()
	{
		return pauseWidget.OptionsWidget;
	}

	public OptionsWidget.SCALING_STRATEGY GetScalingStrategy()
	{
		return pauseWidget.GetScalingStrategy();
	}

	public void ShowGameplayLeftPart()
	{
		gameplayWidget.ShowLeftPart();
	}

	public void HideGameplayLeftPart()
	{
		gameplayWidget.HideLeftPart();
	}

	public void ShowGameplayRightPart()
	{
		gameplayWidget.ShowRightPart();
	}

	public void HideGameplayRightPart()
	{
		gameplayWidget.HideRightPart();
	}

	private bool CanOpenBlockWidget()
	{
		bool flag = !newInventoryMenu.currentlyActive && !mainMenuWidget.currentlyActive;
		if (flag)
		{
			foreach (BasicUIBlockingWidget allUIBLockingWidget in allUIBLockingWidgets)
			{
				flag = flag && !allUIBLockingWidget.IsActive();
			}
		}
		bool flag2 = string.Equals(Core.LevelManager.currentLevel.LevelName, "d07z01s03", StringComparison.CurrentCultureIgnoreCase);
		bool flag3 = Core.Input.HasBlocker("INVENTORY");
		bool insideChangeLevel = Core.LevelManager.InsideChangeLevel;
		return flag && !flag2 && !flag3 && !insideChangeLevel;
	}

	private IEnumerator SafeGoBackInventory()
	{
		yield return new WaitForEndOfFrame();
		newInventoryMenu.GoBack();
		if (!newInventoryMenu.currentlyActive)
		{
			EventSystem.current.SetSelectedGameObject(null);
			EnableCanvas(newInventoryMenu.gameObject, enabled: false);
			CheckPrayerTutorial();
		}
	}

	private IEnumerator SafeGoBackExtras()
	{
		yield return new WaitForEndOfFrame();
		extrasMenu.GoBack();
	}

	private void EnableCanvas(GameObject widget, bool enabled)
	{
		CanvasGroup component = widget.GetComponent<CanvasGroup>();
		component.interactable = enabled;
		component.blocksRaycasts = enabled;
	}

	private void CheckPrayerTutorial()
	{
		if (Core.InventoryManager.GetPrayerInSlot(0) != null && !Core.TutorialManager.IsTutorialUnlocked(TutorialPrayer))
		{
			StartCoroutine(Core.TutorialManager.ShowTutorial(TutorialPrayer));
		}
	}
}
