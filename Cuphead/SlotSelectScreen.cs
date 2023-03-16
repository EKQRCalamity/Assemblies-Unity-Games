using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotSelectScreen : AbstractMonoBehaviour
{
	public enum State
	{
		InitializeStorage,
		MainMenu,
		AchievementsMenu,
		OptionsMenu,
		DLC,
		SlotSelect,
		ConfirmDelete,
		PlayerSelect
	}

	public enum MainMenuItem
	{
		Start,
		Achievements,
		Options,
		DLC,
		Exit
	}

	public enum ConfirmDeleteItem
	{
		Yes,
		No
	}

	private enum SaveDataStatus
	{
		Uninitialized,
		Received,
		Initialized
	}

	private State state;

	[SerializeField]
	private RectTransform LoadingChild;

	[SerializeField]
	private RectTransform mainMenuChild;

	[SerializeField]
	private RectTransform slotSelectChild;

	[SerializeField]
	private RectTransform confirmDeleteChild;

	[SerializeField]
	private Text[] mainMenuItems;

	[SerializeField]
	private SlotSelectScreenSlot[] slots;

	[SerializeField]
	private Text[] confirmDeleteItems;

	[SerializeField]
	private RectTransform playerProfiles;

	[SerializeField]
	private RectTransform confirmPrompt;

	[SerializeField]
	private RectTransform confirmGlyph;

	[SerializeField]
	private RectTransform confirmSpacer;

	[SerializeField]
	private RectTransform backPrompt;

	[SerializeField]
	private RectTransform backGlyph;

	[SerializeField]
	private RectTransform backSpacer;

	[SerializeField]
	private RectTransform storePrompt;

	[SerializeField]
	private RectTransform storeGlyph;

	[SerializeField]
	private RectTransform storeSpacer;

	[SerializeField]
	private RectTransform deletePrompt;

	[SerializeField]
	private RectTransform deleteGlyph;

	[SerializeField]
	private RectTransform deleteSpacer;

	[SerializeField]
	private RectTransform prompts;

	[SerializeField]
	private Color mainMenuSelectedColor;

	[SerializeField]
	private Color mainMenuUnselectedColor;

	[SerializeField]
	private Color confirmDeleteSelectedColor;

	[SerializeField]
	private Color confirmDeleteUnselectedColor;

	[SerializeField]
	private OptionsGUI optionsPrefab;

	[SerializeField]
	private RectTransform optionsRoot;

	[SerializeField]
	private AchievementsGUI achievementsPrefab;

	[SerializeField]
	private RectTransform achievementsRoot;

	[SerializeField]
	private DLCGUI dlcMenuPrefab;

	[SerializeField]
	private RectTransform dlcMenuRoot;

	[SerializeField]
	private TMP_Text confirmDeleteSlotTitle;

	[SerializeField]
	private TMP_Text confirmDeleteSlotSeparator;

	[SerializeField]
	private TMP_Text confirmDeleteSlotPercentage;

	private OptionsGUI options;

	private AchievementsGUI achievements;

	private DLCGUI dlcMenu;

	private int _slotSelection;

	private int _mainMenuSelection;

	private MainMenuItem[] _availableMainMenuItems;

	private int _confirmDeleteSelection;

	private CupheadInput.AnyPlayerInput input;

	private bool isConsole;

	private const string PATH = "Audio/TitleScreenAudio";

	private float timeSinceStart;

	private SaveDataStatus dataStatus;

	private bool RespondToDeadPlayer => true;

	protected override void Awake()
	{
		base.Awake();
		Cuphead.Init();
		input = new CupheadInput.AnyPlayerInput();
		isConsole = PlatformHelper.IsConsole;
		PlayerData.inGame = false;
		List<Text> list = new List<Text>(mainMenuItems);
		List<MainMenuItem> list2 = new List<MainMenuItem>((MainMenuItem[])Enum.GetValues(typeof(MainMenuItem)));
		if (isConsole)
		{
			mainMenuItems[4].gameObject.SetActive(value: false);
			list.RemoveAt(4);
			list2.RemoveAt(4);
		}
		if (!PlatformHelper.ShowDLCMenuItem)
		{
			mainMenuItems[3].gameObject.SetActive(value: false);
			list.RemoveAt(3);
			list2.RemoveAt(3);
		}
		if (!PlatformHelper.ShowAchievements)
		{
			mainMenuItems[1].gameObject.SetActive(value: false);
			list.RemoveAt(1);
			list2.RemoveAt(1);
		}
		mainMenuItems = list.ToArray();
		_availableMainMenuItems = list2.ToArray();
	}

	private void Update()
	{
		if (dataStatus == SaveDataStatus.Received)
		{
			dataStatus = SaveDataStatus.Initialized;
			StartCoroutine(allDataLoaded_cr());
		}
		timeSinceStart += Time.deltaTime;
		switch (state)
		{
		case State.MainMenu:
			UpdateMainMenu();
			break;
		case State.AchievementsMenu:
			UpdateAchievementsMenu();
			break;
		case State.OptionsMenu:
			UpdateOptionsMenu();
			break;
		case State.DLC:
			UpdateDLCMenu();
			break;
		case State.SlotSelect:
			UpdateSlotSelect();
			break;
		case State.ConfirmDelete:
			UpdateConfirmDelete();
			break;
		case State.PlayerSelect:
			UpdatePlayerSelect();
			break;
		}
	}

	private void Start()
	{
		if (StartScreenAudio.Instance == null)
		{
			UnityEngine.Object.Instantiate(Resources.Load("Audio/TitleScreenAudio"));
			SceneLoader.OnLoaderCompleteEvent += PlayMusic;
		}
		CupheadLevelCamera.Current.StartSmoothShake(8f, 3f, 2);
		SetState(State.InitializeStorage);
		PlayerData.Init(OnPlayerDataInitialized);
	}

	private void PlayMusic()
	{
		AudioManager.PlayBGMPlaylistManually(goThroughPlaylistAfter: true);
	}

	private void OnDestroy()
	{
		SceneLoader.OnLoaderCompleteEvent -= PlayMusic;
	}

	private void SetState(State state)
	{
		this.state = state;
		mainMenuChild.gameObject.SetActive(state == State.MainMenu);
		LoadingChild.gameObject.SetActive(state == State.InitializeStorage);
		slotSelectChild.gameObject.SetActive(state == State.SlotSelect || state == State.ConfirmDelete || state == State.PlayerSelect);
		confirmDeleteChild.gameObject.SetActive(state == State.ConfirmDelete);
		confirmPrompt.gameObject.SetActive(state == State.MainMenu || state == State.OptionsMenu || state == State.SlotSelect || state == State.ConfirmDelete || state == State.PlayerSelect);
		confirmGlyph.gameObject.SetActive(confirmPrompt.gameObject.activeSelf);
		confirmSpacer.gameObject.SetActive(confirmPrompt.gameObject.activeSelf);
		backPrompt.gameObject.SetActive(state == State.OptionsMenu || state == State.SlotSelect || state == State.ConfirmDelete || state == State.PlayerSelect || state == State.AchievementsMenu || state == State.DLC);
		backGlyph.gameObject.SetActive(backPrompt.gameObject.activeSelf);
		backSpacer.gameObject.SetActive(backPrompt.gameObject.activeSelf);
		deletePrompt.gameObject.SetActive(state == State.SlotSelect);
		deleteGlyph.gameObject.SetActive(deletePrompt.gameObject.activeSelf);
		deleteSpacer.gameObject.SetActive(deletePrompt.gameObject.activeSelf);
		storePrompt.gameObject.SetActive(state == State.DLC && DLCManager.CanRedirectToStore() && !DLCManager.DLCEnabled());
		storeGlyph.gameObject.SetActive(storePrompt.gameObject.activeSelf);
		storeSpacer.gameObject.SetActive(storePrompt.gameObject.activeSelf);
		playerProfiles.gameObject.SetActive(state == State.SlotSelect || state == State.MainMenu);
		PlayerManager.SetPlayerCanSwitch(PlayerId.PlayerOne, state == State.SlotSelect || state == State.MainMenu);
	}

	private void UpdateMainMenu()
	{
		if (timeSinceStart < 0.75f)
		{
			return;
		}
		if (GetButtonDown(CupheadButton.MenuDown))
		{
			AudioManager.Play("level_menu_move");
			_mainMenuSelection = (_mainMenuSelection + 1) % mainMenuItems.Length;
		}
		if (GetButtonDown(CupheadButton.MenuUp))
		{
			AudioManager.Play("level_menu_move");
			_mainMenuSelection--;
			if (_mainMenuSelection < 0)
			{
				_mainMenuSelection = mainMenuItems.Length - 1;
			}
		}
		for (int i = 0; i < mainMenuItems.Length; i++)
		{
			mainMenuItems[i].color = ((_mainMenuSelection != i) ? mainMenuUnselectedColor : mainMenuSelectedColor);
		}
		if (!GetButtonDown(CupheadButton.Accept))
		{
			return;
		}
		AudioManager.Play("level_menu_select");
		switch (_availableMainMenuItems[_mainMenuSelection])
		{
		case MainMenuItem.Start:
		{
			SetState(State.SlotSelect);
			for (int j = 0; j < 3; j++)
			{
				slots[j].Init(j);
			}
			break;
		}
		case MainMenuItem.Achievements:
			SetState(State.AchievementsMenu);
			achievements.ShowAchievements();
			break;
		case MainMenuItem.Options:
			SetState(State.OptionsMenu);
			options.ShowMainOptionMenu();
			break;
		case MainMenuItem.DLC:
			SetState(State.DLC);
			dlcMenu.ShowDLCMenu();
			break;
		case MainMenuItem.Exit:
			Application.Quit();
			break;
		}
	}

	private void UpdateOptionsMenu()
	{
		prompts.gameObject.SetActive(!Cuphead.Current.controlMapper.isOpen);
		if (!options.optionMenuOpen && !options.justClosed)
		{
			SetState(State.MainMenu);
		}
	}

	private void UpdateAchievementsMenu()
	{
		if (!achievements.achievementsMenuOpen && !achievements.justClosed)
		{
			SetState(State.MainMenu);
		}
	}

	private void UpdateDLCMenu()
	{
		if (!dlcMenu.dlcMenuOpen && !dlcMenu.justClosed)
		{
			SetState(State.MainMenu);
		}
	}

	private void UpdatePlayerSelect()
	{
		if (PlayerData.inGame)
		{
			return;
		}
		if (GetButtonDown(CupheadButton.MenuLeft) || GetButtonDown(CupheadButton.MenuRight))
		{
			AudioManager.Play("level_menu_move");
			slots[_slotSelection].SwapSprite();
		}
		else if (GetButtonDown(CupheadButton.Cancel))
		{
			AudioManager.Play("level_menu_select");
			for (int i = 0; i < slots.Length; i++)
			{
				if (i != _slotSelection)
				{
					StartCoroutine(activate_noise_cr(i));
				}
			}
			slots[_slotSelection].StopSelectingPlayer();
			SetState(State.SlotSelect);
		}
		else if (GetButtonDown(CupheadButton.Accept))
		{
			AudioManager.Play("ui_menu_confirm");
			slots[_slotSelection].PlayAnimation(_slotSelection);
			StartCoroutine(game_start_cr());
		}
	}

	private void UpdateSlotSelect()
	{
		if (PlayerData.inGame)
		{
			return;
		}
		if (GetButtonDown(CupheadButton.MenuDown))
		{
			AudioManager.Play("ui_saveslot_move");
			_slotSelection = (_slotSelection + 1) % 3;
		}
		if (GetButtonDown(CupheadButton.MenuUp))
		{
			AudioManager.Play("ui_saveslot_move");
			_slotSelection--;
			if (_slotSelection < 0)
			{
				_slotSelection = 2;
			}
		}
		for (int i = 0; i < 3; i++)
		{
			slots[i].SetSelected(_slotSelection == i);
		}
		if (GetButtonDown(CupheadButton.Accept))
		{
			AudioManager.Play("level_select");
			for (int j = 0; j < slots.Length; j++)
			{
				if (j != _slotSelection)
				{
					slots[j].noise.gameObject.SetActive(value: false);
				}
			}
			slots[_slotSelection].EnterSelectMenu();
			SetState(State.PlayerSelect);
		}
		else if (GetButtonDown(CupheadButton.Cancel))
		{
			AudioManager.Play("level_menu_select");
			SetState(State.MainMenu);
		}
		else if (!slots[_slotSelection].IsEmpty && GetButtonDown(CupheadButton.EquipMenu))
		{
			AudioManager.Play("level_menu_select");
			SetState(State.ConfirmDelete);
			_confirmDeleteSelection = 1;
			confirmDeleteSlotTitle.text = slots[_slotSelection].GetSlotTitle();
			confirmDeleteSlotTitle.font = slots[_slotSelection].GetSlotTitleFont();
			confirmDeleteSlotSeparator.text = slots[_slotSelection].GetSlotSeparator();
			confirmDeleteSlotSeparator.font = slots[_slotSelection].GetSlotSeparatorFont();
			confirmDeleteSlotPercentage.text = slots[_slotSelection].GetSlotPercentage() + "?";
			confirmDeleteSlotPercentage.font = slots[_slotSelection].GetSlotPercentageFont();
		}
	}

	private IEnumerator game_start_cr()
	{
		PlayerData.inGame = true;
		for (int i = 0; i < 45; i++)
		{
			yield return null;
		}
		EnterGame();
	}

	private IEnumerator activate_noise_cr(int index)
	{
		for (int i = 0; i < 10; i++)
		{
			yield return null;
		}
		slots[index].noise.gameObject.SetActive(value: true);
		yield return null;
	}

	private void EnterGame()
	{
		DLCManager.RefreshDLC();
		PlayerManager.SetPlayerCanSwitch(PlayerId.PlayerOne, canSwitch: false);
		PlayerData.CurrentSaveFileIndex = _slotSelection;
		PlayerManager.player1IsMugman = slots[_slotSelection].isPlayer1Mugman;
		PlayerData.GetDataForSlot(_slotSelection).isPlayer1Mugman = PlayerManager.player1IsMugman;
		if (!DLCManager.DLCEnabled())
		{
			PlayerData data = PlayerData.Data;
			for (int i = 0; i < 2; i++)
			{
				PlayerId player = (PlayerId)i;
				PlayerData.PlayerLoadouts.PlayerLoadout playerLoadout = data.Loadouts.GetPlayerLoadout(player);
				if (Array.IndexOf(PlayerData.WeaponsDLC, playerLoadout.secondaryWeapon) >= 0)
				{
					playerLoadout.secondaryWeapon = Weapon.None;
				}
				if (Array.IndexOf(PlayerData.WeaponsDLC, playerLoadout.primaryWeapon) >= 0)
				{
					playerLoadout.primaryWeapon = Weapon.level_weapon_peashot;
					if (playerLoadout.secondaryWeapon == Weapon.level_weapon_peashot)
					{
						playerLoadout.secondaryWeapon = Weapon.None;
					}
				}
				if (Array.IndexOf(PlayerData.CharmsDLC, playerLoadout.charm) >= 0)
				{
					playerLoadout.charm = Charm.None;
				}
			}
		}
		Level.ResetPreviousLevelInfo();
		if (!slots[_slotSelection].IsEmpty)
		{
			if (!DLCManager.DLCEnabled() && PlayerData.Data.CurrentMap == Scenes.scene_map_world_DLC)
			{
				PlayerData.Data.CurrentMap = Scenes.scene_map_world_1;
				PlayerData.Data.GetMapData(Scenes.scene_map_world_1).sessionStarted = false;
			}
			SceneLoader.LoadScene(PlayerData.Data.CurrentMap, SceneLoader.Transition.Fade, SceneLoader.Transition.Iris);
		}
		else
		{
			PlayerData.Data.CurrentMap = Scenes.scene_map_world_1;
			PlayerData.Data.GetMapData(Scenes.scene_map_world_1).sessionStarted = false;
			Cutscene.Load(Scenes.scene_level_house_elder_kettle, Scenes.scene_cutscene_intro, SceneLoader.Transition.Fade, SceneLoader.Transition.Fade);
		}
		PlayerData.inGame = true;
		if (StartScreenAudio.Instance != null)
		{
			UnityEngine.Object.Destroy(StartScreenAudio.Instance.gameObject);
		}
	}

	private void UpdateConfirmDelete()
	{
		if (GetButtonDown(CupheadButton.MenuDown))
		{
			AudioManager.Play("level_menu_move");
			_confirmDeleteSelection = (_confirmDeleteSelection + 1) % 2;
		}
		if (GetButtonDown(CupheadButton.MenuUp))
		{
			AudioManager.Play("level_menu_move");
			_confirmDeleteSelection--;
			if (_confirmDeleteSelection < 0)
			{
				_confirmDeleteSelection = 1;
			}
		}
		for (int i = 0; i < 2; i++)
		{
			confirmDeleteItems[i].color = ((_confirmDeleteSelection != i) ? confirmDeleteUnselectedColor : confirmDeleteSelectedColor);
		}
		if (GetButtonDown(CupheadButton.Accept))
		{
			switch ((ConfirmDeleteItem)_confirmDeleteSelection)
			{
			case ConfirmDeleteItem.Yes:
				AudioManager.Play("level_menu_select");
				PlayerData.ClearSlot(_slotSelection);
				slots[_slotSelection].Init(_slotSelection);
				SetState(State.SlotSelect);
				break;
			case ConfirmDeleteItem.No:
				AudioManager.Play("level_menu_select");
				SetState(State.SlotSelect);
				break;
			}
		}
		if (GetButtonDown(CupheadButton.Cancel))
		{
			AudioManager.Play("level_menu_select");
			SetState(State.SlotSelect);
		}
	}

	private void OnPlayerDataInitialized(bool success)
	{
		if (!success)
		{
			PlayerData.Init(OnPlayerDataInitialized);
		}
		else if (PlatformHelper.IsConsole && !PlatformHelper.PreloadSettingsData)
		{
			SettingsData.LoadFromCloud(OnSettingsDataLoaded);
		}
		else
		{
			dataStatus = SaveDataStatus.Received;
		}
	}

	private void OnSettingsDataLoaded(bool success)
	{
		if (!success)
		{
			SettingsData.LoadFromCloud(OnSettingsDataLoaded);
			return;
		}
		SettingsData.ApplySettingsOnStartup();
		StartCoroutine(allDataLoaded_cr());
	}

	private IEnumerator allDataLoaded_cr()
	{
		yield return null;
		SetState(State.MainMenu);
		for (int i = 0; i < 3; i++)
		{
			slots[i].Init(i);
		}
		ControllerDisconnectedPrompt.Instance.allowedToShow = true;
		options = optionsPrefab.InstantiatePrefab<OptionsGUI>();
		options.rectTransform.SetParent(optionsRoot, worldPositionStays: false);
		options.Init(checkIfDead: false);
		if (PlatformHelper.ShowAchievements)
		{
			achievements = achievementsPrefab.InstantiatePrefab<AchievementsGUI>();
			achievements.rectTransform.SetParent(achievementsRoot, worldPositionStays: false);
			achievements.Init(checkIfDead: false);
		}
		if (PlatformHelper.ShowDLCMenuItem)
		{
			dlcMenu = dlcMenuPrefab.InstantiatePrefab<DLCGUI>();
			dlcMenu.rectTransform.SetParent(dlcMenuRoot, worldPositionStays: false);
			dlcMenu.Init(checkIfDead: false);
		}
		if (PlatformHelper.IsConsole)
		{
			PlayerManager.LoadControllerMappings(PlayerId.PlayerOne);
		}
		SetRichPresence();
	}

	protected bool GetButtonDown(CupheadButton button)
	{
		if (input.GetButtonDown(button))
		{
			return true;
		}
		return false;
	}

	private void SetRichPresence()
	{
		OnlineManager.Instance.Interface.SetRichPresence(PlayerId.Any, "SlotSelect", active: true);
	}
}
