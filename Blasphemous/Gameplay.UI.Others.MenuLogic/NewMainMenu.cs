using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using FullSerializer;
using I2.Loc;
using Rewired;
using Sirenix.OdinInspector;
using Tools.Audio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gameplay.UI.Others.MenuLogic;

public class NewMainMenu : MonoBehaviour
{
	private enum MenuState
	{
		OFF,
		MENU,
		SLOT,
		FADEOUT,
		CONFIRM,
		OPTIONS,
		CHOOSE_BACKGROUND,
		BOSSRUSH
	}

	private enum SequenceButtons
	{
		LEFT,
		RIGHT,
		UP,
		DOWN,
		DUMMY
	}

	public static Core.SimpleEvent OnMenuOpen;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	[EventRef]
	protected string trackIdentifier;

	[BoxGroup("Audio Settings", true, false, 0)]
	[SerializeField]
	[EventRef]
	private string soundOnAccept = "event:/SFX/UI/ChangeTab";

	[BoxGroup("Audio Settings", true, false, 0)]
	[SerializeField]
	[EventRef]
	private string soundOnBack = "event:/SFX/UI/ChangeTab";

	[BoxGroup("Audio Settings", true, false, 0)]
	[SerializeField]
	[EventRef]
	private string soundOnMove = "event:/SFX/UI/ChangeTab";

	[BoxGroup("Config", true, false, 0)]
	public string initialSceneName;

	[BoxGroup("Widgets", true, false, 0)]
	[SerializeField]
	private SelectSaveSlots selectSaveSlotsWidget;

	[BoxGroup("Widgets", true, false, 0)]
	[SerializeField]
	private BossRushWidget bossRushWidget;

	[BoxGroup("Widgets", true, false, 0)]
	[SerializeField]
	private KeepFocus mainMenuKeepFocus;

	[BoxGroup("Widgets", true, false, 0)]
	[SerializeField]
	private List<GameObject> choosingBackgroundOptions;

	[BoxGroup("Widgets", true, false, 0)]
	[SerializeField]
	private List<GameObject> mainMenuOptions;

	[BoxGroup("Widgets Background", true, false, 0)]
	[SerializeField]
	private Sprite[] availableBackgrounds;

	[BoxGroup("Widgets Background", true, false, 0)]
	[SerializeField]
	private Animator backgroundAnimator;

	[BoxGroup("Widgets Background", true, false, 0)]
	[SerializeField]
	private Text backgroundLabel;

	[BoxGroup("Widgets Background", true, false, 0)]
	[SerializeField]
	private Image background;

	[BoxGroup("Widgets", true, false, 0)]
	[SerializeField]
	private Button BossRushButton;

	[BoxGroup("Widgets", true, false, 0)]
	[SerializeField]
	private List<Button> AllButtons;

	private const string APP_SETTINGS_MAIN_MENU_BG_KEY = "main_menu_background";

	private const string APP_SETTINGS_LATEST_BACKGROUND_INDEX_KEY = "latest_background_index";

	private const string ANIMATOR_STATE = "STATUS";

	private const int LATEST_BACKGROUND_INDEX = 3;

	private bool ShowBossRush;

	private MenuState currentState;

	private Player rewiredPlayer;

	private Animator animator;

	private float timeWaiting;

	private string sceneName;

	private bool mustConvertToNewgamePlus;

	private bool isChoosingBackground;

	private bool isContinue;

	public int bgIndex;

	private bool menuButtonsNavEnabled = true;

	[ShowInInspector]
	private List<SequenceButtons> skinSequence = new List<SequenceButtons>
	{
		SequenceButtons.UP,
		SequenceButtons.UP,
		SequenceButtons.DOWN,
		SequenceButtons.DOWN,
		SequenceButtons.LEFT,
		SequenceButtons.RIGHT,
		SequenceButtons.LEFT,
		SequenceButtons.RIGHT
	};

	[ShowInInspector]
	private List<SequenceButtons> currentSequence = new List<SequenceButtons>();

	[EventRef]
	public string SkinUnlockedSFX = "event:/SFX/DEMAKE/DSkinItem";

	[EventRef]
	public string SkinAlreadyUnlockedSFX = "event:/SFX/DEMAKE/DPlatformCollapse";

	public bool currentlyActive => currentState != MenuState.OFF;

	private void Awake()
	{
		if (Application.runInBackground)
		{
			Debug.LogWarning("Run in background was true! Correcting.");
		}
		Application.runInBackground = false;
		animator = GetComponent<Animator>();
		selectSaveSlotsWidget.gameObject.SetActive(value: true);
		selectSaveSlotsWidget.Clear();
		SetState(MenuState.OFF);
		bossRushWidget.InitializeWidget();
		string pathAppSettings = GetPathAppSettings();
		if (!File.Exists(pathAppSettings))
		{
			File.CreateText(pathAppSettings).Close();
		}
		else
		{
			ReadFileSelectedBackground(pathAppSettings);
		}
		rewiredPlayer = ReInput.players.GetPlayer(0);
		SetBackgroundSpriteAndAnimation();
		ShowMainMenuOptions();
		HideChoosingBackgroundOptions();
		UpdateBackgroundLabelText();
	}

	private void Update()
	{
		if (!currentlyActive)
		{
			return;
		}
		if (menuButtonsNavEnabled && (UIController.instance.IsPatchNotesShowing() || UIController.instance.IsModeUnlockedShowing()))
		{
			DisableMenuButtonsNav();
		}
		else if (!menuButtonsNavEnabled && !UIController.instance.IsPatchNotesShowing() && !UIController.instance.IsModeUnlockedShowing())
		{
			EnableMenuButtonsNav();
		}
		if (isChoosingBackground)
		{
			if (rewiredPlayer.GetButtonDown(50))
			{
				ProcessSubmitInput();
				return;
			}
			if (rewiredPlayer.GetButtonDown(51))
			{
				ProcessBackInput();
				return;
			}
			float axisPrev = rewiredPlayer.GetAxisPrev(48);
			if (axisPrev < 0.3f && axisPrev > -0.3f)
			{
				if (rewiredPlayer.GetAxisRaw(48) > 0.3f)
				{
					ProcessMoveInput(movingRight: true);
				}
				else if (rewiredPlayer.GetAxisRaw(48) < -0.3f)
				{
					ProcessMoveInput(movingRight: false);
				}
			}
		}
		else
		{
			CheckSequenceButtons();
		}
	}

	private void CheckSequenceButtons()
	{
		SequenceButtons sequenceButtons = SequenceButtons.DUMMY;
		if (CheckRight())
		{
			sequenceButtons = SequenceButtons.RIGHT;
		}
		if (sequenceButtons == SequenceButtons.DUMMY && CheckLeft())
		{
			sequenceButtons = SequenceButtons.LEFT;
		}
		if (sequenceButtons == SequenceButtons.DUMMY && CheckUp())
		{
			sequenceButtons = SequenceButtons.UP;
		}
		if (sequenceButtons == SequenceButtons.DUMMY && CheckDown())
		{
			sequenceButtons = SequenceButtons.DOWN;
		}
		if (sequenceButtons != SequenceButtons.DUMMY)
		{
			UpdateSequence(sequenceButtons);
		}
	}

	private bool CheckRight()
	{
		float axisPrev = rewiredPlayer.GetAxisPrev(48);
		return axisPrev < 0.3f && axisPrev > -0.3f && rewiredPlayer.GetAxisRaw(48) > 0.3f;
	}

	private bool CheckLeft()
	{
		float axisPrev = rewiredPlayer.GetAxisPrev(48);
		return axisPrev < 0.3f && axisPrev > -0.3f && rewiredPlayer.GetAxisRaw(48) < -0.3f;
	}

	private bool CheckUp()
	{
		float axisPrev = rewiredPlayer.GetAxisPrev(49);
		return axisPrev < 0.3f && axisPrev > -0.3f && rewiredPlayer.GetAxisRaw(49) > 0.3f;
	}

	private bool CheckDown()
	{
		float axisPrev = rewiredPlayer.GetAxisPrev(49);
		return axisPrev < 0.3f && axisPrev > -0.3f && rewiredPlayer.GetAxisRaw(49) < -0.3f;
	}

	private void UpdateSequence(SequenceButtons currentSequenceButton)
	{
		if (currentSequence.Count < skinSequence.Count)
		{
			SequenceButtons sequenceButtons = skinSequence[currentSequence.Count];
			if (sequenceButtons == currentSequenceButton)
			{
				currentSequence.Add(currentSequenceButton);
			}
			else
			{
				currentSequence.Clear();
			}
		}
		if (currentSequence.Count == skinSequence.Count)
		{
			currentSequence.Clear();
			if (Core.ColorPaletteManager.IsColorPaletteUnlocked("PENITENT_KONAMI"))
			{
				Core.Audio.PlayOneShot(SkinAlreadyUnlockedSFX);
				return;
			}
			Core.Audio.PlayOneShot(SkinUnlockedSFX);
			Core.ColorPaletteManager.UnlockBossKonamiColorPalette();
		}
	}

	public void OptionCampain()
	{
		if (currentState == MenuState.MENU)
		{
			SetState(MenuState.SLOT);
			selectSaveSlotsWidget.SetAllData(this, SelectSaveSlots.SlotsModes.Normal);
			StartCoroutine(ShowSlotSaveWidget());
		}
	}

	public void OptionBossRush()
	{
		if (currentState == MenuState.MENU)
		{
			StartCoroutine(ShowWidgetForBossRush());
		}
	}

	public void OptionOptions()
	{
		if (currentState == MenuState.MENU)
		{
			StartCoroutine(ShowOptionsFromMap());
		}
	}

	public void OptionExtras()
	{
		if (currentState == MenuState.MENU)
		{
			ShowExtras();
		}
	}

	public void OptionExitGame()
	{
		if (currentState == MenuState.MENU)
		{
			Application.Quit();
		}
	}

	public void SetConfirmationDeleteFromSlot()
	{
		SetState(MenuState.CONFIRM);
	}

	public void SetNormalModeFromConfirmation()
	{
		SetState(MenuState.SLOT);
	}

	public void ShowMenu(string newInitialScene = "")
	{
		PersistentManager.ResetAutomaticSlot();
		sceneName = ((!(newInitialScene != string.Empty)) ? initialSceneName : newInitialScene);
		if (OnMenuOpen != null)
		{
			OnMenuOpen();
		}
		Core.Audio.Ambient.SetSceneParams(trackIdentifier, string.Empty, new AudioParam[0], string.Empty);
		SetState(MenuState.MENU);
		SetBackgroundSpriteAndAnimation();
		Core.GameModeManager.ChangeMode(GameModeManager.GAME_MODES.MENU);
		ShowMainMenuOptions();
	}

	public IEnumerator ShowOptionsFromMap()
	{
		SetState(MenuState.OPTIONS);
		yield return UIController.instance.ShowOptions();
		SetState(MenuState.MENU);
	}

	public void ShowExtras()
	{
		SetState(MenuState.OFF);
		UIController.instance.ShowExtras();
	}

	public void ShowChooseBackground()
	{
		if (OnMenuOpen != null)
		{
			OnMenuOpen();
		}
		Core.Audio.Ambient.SetSceneParams(trackIdentifier, string.Empty, new AudioParam[0], string.Empty);
		ShowChoosingBackgroundOptions();
		HideMainMenuOptions();
		SetState(MenuState.CHOOSE_BACKGROUND);
	}

	public IEnumerator ShowSlotSaveWidget()
	{
		while (selectSaveSlotsWidget.IsShowing)
		{
			yield return new WaitForEndOfFrame();
		}
		if (selectSaveSlotsWidget.SelectedSlot >= 0)
		{
			PersistentManager.SetAutomaticSlot(selectSaveSlotsWidget.SelectedSlot);
			isContinue = selectSaveSlotsWidget.CanLoadSelectedSlot;
			mustConvertToNewgamePlus = selectSaveSlotsWidget.MustConvertToNewgamePlus;
			if (isContinue)
			{
				if (mustConvertToNewgamePlus)
				{
					Log.Trace("Continue pressed and new game plus, starting the game...");
				}
				else
				{
					Log.Trace("Continue pressed, starting the game...");
				}
			}
			else
			{
				Log.Trace("Play pressed, starting the game...");
			}
			EventSystem.current.SetSelectedGameObject(null);
			SetState(MenuState.OFF);
			Core.Audio.Ambient.StopCurrent();
			InternalPlay();
		}
		else
		{
			SetState(MenuState.MENU);
		}
		yield return null;
	}

	public IEnumerator ShowWidgetForBossRush()
	{
		mainMenuKeepFocus.enabled = false;
		EventSystem.current.SetSelectedGameObject(null);
		SetState(MenuState.MENU);
		currentState = MenuState.BOSSRUSH;
		bossRushWidget.FadeShow(checkInput: false, pauseGame: false);
		while (bossRushWidget.IsActive())
		{
			yield return new WaitForEndOfFrame();
		}
		bool returnToMenu = true;
		if (bossRushWidget.IsAllSelected)
		{
			InternalBossRush();
			returnToMenu = false;
		}
		if (returnToMenu)
		{
			mainMenuKeepFocus.enabled = true;
			SetState(MenuState.MENU);
		}
		yield return null;
	}

	private bool IsAnySlotForBossRush()
	{
		Core.BossRushManager.CheckCoursesUnlockBySlots();
		return Core.BossRushManager.IsAnyCourseUnlocked();
	}

	private void InternalPlay()
	{
		Core.SpawnManager.InitialScene = sceneName;
		Core.LevelManager.ActivatePrecachedScene();
		UIController.instance.HideMainMenu();
		if (mustConvertToNewgamePlus)
		{
			Core.Persistence.LoadGameWithOutRespawn(PersistentManager.GetAutomaticSlot());
			Core.GameModeManager.ConvertCurrentGameToPlus();
			Core.Persistence.SaveGame(fullSave: false);
			Core.SpawnManager.FirstSpanw = true;
			Core.SpawnManager.SetInitialSpawn(sceneName);
			Core.LevelManager.ChangeLevel(sceneName);
		}
		else if (isContinue)
		{
			Core.Persistence.LoadGame(PersistentManager.GetAutomaticSlot());
		}
		else
		{
			Core.Logic.ResetAllData();
			Core.Persistence.DeleteSaveGame(PersistentManager.GetAutomaticSlot());
			Core.SpawnManager.FirstSpanw = true;
			Core.GameModeManager.ChangeMode(GameModeManager.GAME_MODES.NEW_GAME);
			Core.SpawnManager.SetInitialSpawn(sceneName);
			Core.LevelManager.ChangeLevel(sceneName);
		}
	}

	private void InternalBossRush()
	{
		SetState(MenuState.OFF);
		mainMenuKeepFocus.enabled = true;
		UIController.instance.HideMainMenu();
		Core.BossRushManager.StartCourse(bossRushWidget.SelectedCourse, bossRushWidget.SelectedMode, bossRushWidget.CurrentSlot);
	}

	private void SetState(MenuState state)
	{
		currentState = state;
		animator.SetInteger("STATUS", (int)currentState);
	}

	private void ProcessSubmitInput()
	{
		if (soundOnAccept != string.Empty)
		{
			Core.Audio.PlayOneShot(soundOnAccept);
		}
		string pathAppSettings = GetPathAppSettings();
		WriteFileAppSettings(pathAppSettings);
		HideChoosingBackgroundOptions();
		ShowMainMenuOptions();
		SetState(MenuState.MENU);
	}

	public void PlaySoundOnBack()
	{
		if (soundOnBack != string.Empty)
		{
			Core.Audio.PlayOneShot(soundOnBack);
		}
	}

	private void ProcessBackInput()
	{
		PlaySoundOnBack();
		string pathAppSettings = GetPathAppSettings();
		ReadFileSelectedBackground(pathAppSettings);
		SetBackgroundSpriteAndAnimation();
		HideChoosingBackgroundOptions();
		ShowMainMenuOptions();
		SetState(MenuState.MENU);
	}

	private void ProcessMoveInput(bool movingRight)
	{
		if (soundOnMove != string.Empty)
		{
			Core.Audio.PlayOneShot(soundOnMove);
		}
		if (movingRight)
		{
			bgIndex++;
			if (bgIndex > 3)
			{
				bgIndex = 0;
			}
		}
		else
		{
			bgIndex--;
			if (bgIndex < 0)
			{
				bgIndex = 3;
			}
		}
		SetBackgroundSpriteAndAnimation();
		UpdateBackgroundLabelText();
	}

	private void ShowChoosingBackgroundOptions()
	{
		isChoosingBackground = true;
		choosingBackgroundOptions.ForEach(delegate(GameObject x)
		{
			x.SetActive(value: true);
		});
		UpdateBackgroundLabelText();
	}

	private void HideChoosingBackgroundOptions()
	{
		isChoosingBackground = false;
		choosingBackgroundOptions.ForEach(delegate(GameObject x)
		{
			x.SetActive(value: false);
		});
	}

	private void ShowMainMenuOptions()
	{
		mainMenuOptions.ForEach(delegate(GameObject x)
		{
			x.SetActive(value: true);
		});
		ShowBossRush = IsAnySlotForBossRush();
		if (ShowBossRush)
		{
			BossRushButton.gameObject.SetActive(value: true);
			Selectable selectOnUp = BossRushButton.navigation.selectOnUp;
			Navigation navigation = selectOnUp.navigation;
			navigation.selectOnDown = BossRushButton;
			selectOnUp.navigation = navigation;
			Selectable selectOnDown = BossRushButton.navigation.selectOnDown;
			Navigation navigation2 = selectOnDown.navigation;
			navigation2.selectOnUp = BossRushButton;
			selectOnDown.navigation = navigation2;
			UIController.instance.ShowModeUnlockedWidget(ModeUnlockedWidget.ModesToUnlock.BossRush);
		}
		else
		{
			Selectable selectOnUp2 = BossRushButton.navigation.selectOnUp;
			Selectable selectOnDown2 = BossRushButton.navigation.selectOnDown;
			Navigation navigation3 = selectOnUp2.navigation;
			navigation3.selectOnDown = selectOnDown2;
			selectOnUp2.navigation = navigation3;
			Navigation navigation4 = selectOnDown2.navigation;
			navigation4.selectOnUp = selectOnUp2;
			selectOnDown2.navigation = navigation4;
			BossRushButton.gameObject.SetActive(value: false);
		}
	}

	private void HideMainMenuOptions()
	{
		mainMenuOptions.ForEach(delegate(GameObject x)
		{
			x.SetActive(value: false);
		});
	}

	private void SetBackgroundSpriteAndAnimation()
	{
		background.sprite = availableBackgrounds[bgIndex];
		backgroundAnimator.SetInteger("BG_INDEX", bgIndex);
	}

	private void UpdateBackgroundLabelText()
	{
		switch (bgIndex)
		{
		case 0:
			backgroundLabel.text = ScriptLocalization.UI_Extras.BACKGROUND_0_LABEL;
			break;
		case 1:
			backgroundLabel.text = ScriptLocalization.UI_Extras.BACKGROUND_1_LABEL;
			break;
		case 2:
			backgroundLabel.text = ScriptLocalization.UI_Extras.BACKGROUND_2_LABEL;
			break;
		case 3:
			backgroundLabel.text = ScriptLocalization.UI_Extras.BACKGROUND_3_LABEL;
			break;
		}
	}

	private void DisableMenuButtonsNav()
	{
		menuButtonsNavEnabled = false;
		AllButtons.ForEach(delegate(Button x)
		{
			Navigation navigation = x.navigation;
			navigation.mode = Navigation.Mode.None;
			x.navigation = navigation;
			x.interactable = false;
		});
	}

	private void EnableMenuButtonsNav()
	{
		menuButtonsNavEnabled = true;
		AllButtons.ForEach(delegate(Button x)
		{
			Navigation navigation = x.navigation;
			navigation.mode = Navigation.Mode.Explicit;
			x.navigation = navigation;
			x.interactable = true;
		});
	}

	private static string GetPathAppSettings()
	{
		return PersistentManager.GetPathAppSettings("/app_settings");
	}

	private void ReadFileSelectedBackground(string filePath)
	{
		bgIndex = 3;
		fsData data = new fsData();
		if (PersistentManager.TryToReadFile(filePath, out var fileData))
		{
			byte[] bytes = Convert.FromBase64String(fileData);
			string @string = Encoding.UTF8.GetString(bytes);
			fsResult fsResult = fsJsonParser.Parse(@string, out data);
			if (fsResult.Failed && !fsResult.FormattedMessages.Equals("No input"))
			{
				Debug.LogError("Parsing error: " + fsResult.FormattedMessages);
			}
			else if (data != null)
			{
				Dictionary<string, fsData> asDictionary = data.AsDictionary;
				bool flag = false;
				if (asDictionary.ContainsKey("latest_background_index") && asDictionary["latest_background_index"].IsInt64)
				{
					int num = (int)asDictionary["latest_background_index"].AsInt64;
					if (num != 3)
					{
						flag = true;
					}
				}
				else
				{
					flag = true;
				}
				if (flag)
				{
					bgIndex = 3;
				}
				else if (asDictionary.ContainsKey("main_menu_background") && asDictionary["main_menu_background"].IsInt64)
				{
					int value = (int)asDictionary["main_menu_background"].AsInt64;
					bgIndex = Mathf.Clamp(value, 0, 3);
				}
			}
		}
		bgIndex = Mathf.Clamp(bgIndex, 0, availableBackgrounds.Length - 1);
		background.sprite = availableBackgrounds[bgIndex];
	}

	private void WriteFileAppSettings(string filePath)
	{
		fsData fsData = PersistentManager.ReadAppSettings(filePath);
		if (!(fsData == null) && fsData.IsDictionary)
		{
			fsData.AsDictionary["main_menu_background"] = new fsData(bgIndex);
			fsData.AsDictionary["latest_background_index"] = new fsData(3L);
			string s = fsJsonPrinter.CompressedJson(fsData);
			byte[] bytes = Encoding.UTF8.GetBytes(s);
			string contents = Convert.ToBase64String(bytes);
			File.WriteAllText(filePath, contents);
		}
	}
}
