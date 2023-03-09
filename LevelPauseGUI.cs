using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LevelPauseGUI : AbstractPauseGUI
{
	private enum MenuItems
	{
		Unpause,
		Restart,
		Achievements,
		Options,
		Player2Leave,
		ExitToMap,
		ExitToTitle,
		ExitToDesktop
	}

	[SerializeField]
	private Text[] menuItems;

	private OptionsGUI options;

	private AchievementsGUI achievements;

	private RestartTowerConfirmGUI restartTowerConfirm;

	private float _selectionTimer;

	private const float _SELECTION_TIME = 0.15f;

	[SerializeField]
	private LocalizationHelper retryLocHelper;

	private int _selection;

	private bool forceDisablePause;

	public static Color COLOR_SELECTED { get; private set; }

	public static Color COLOR_INACTIVE { get; private set; }

	private int selection
	{
		get
		{
			return _selection;
		}
		set
		{
			bool flag = value > _selection;
			int num = (int)Mathf.Repeat(value, menuItems.Length);
			while (!menuItems[num].gameObject.activeSelf)
			{
				num = ((!flag) ? (num - 1) : (num + 1));
				num = (int)Mathf.Repeat(num, menuItems.Length);
			}
			_selection = num;
			UpdateSelection();
		}
	}

	protected override bool CanPause => Level.Current.Started && !Level.Current.Ending && PauseManager.state != PauseManager.State.Paused && !SceneLoader.CurrentlyLoading && !forceDisablePause;

	public static event Action OnPauseEvent;

	public static event Action OnUnpauseEvent;

	private void OnEnable()
	{
		Localization.OnLanguageChangedEvent += onLanguageChangedEventHandler;
	}

	private void OnDisable()
	{
		Localization.OnLanguageChangedEvent -= onLanguageChangedEventHandler;
	}

	protected override void Awake()
	{
		base.Awake();
		COLOR_SELECTED = menuItems[0].color;
		COLOR_INACTIVE = menuItems[menuItems.Length - 1].color;
	}

	public override void Init(bool checkIfDead, OptionsGUI options, AchievementsGUI achievements)
	{
		Init(checkIfDead, options, achievements, null);
	}

	public override void Init(bool checkIfDead, OptionsGUI options, AchievementsGUI achievements, RestartTowerConfirmGUI restartTowerConfirm)
	{
		base.Init(checkIfDead, options, achievements);
		this.options = options;
		this.achievements = achievements;
		this.restartTowerConfirm = restartTowerConfirm;
		if (PlatformHelper.IsConsole && menuItems.Length > 7)
		{
			menuItems[7].gameObject.SetActive(value: false);
		}
		if (Level.Current != null && Level.Current.CurrentLevel == Levels.Airplane)
		{
			menuItems[2].gameObject.SetActive(value: true);
			updateRotateControlsToggleVisualValue();
		}
		else if (!PlatformHelper.ShowAchievements && menuItems.Length > 2)
		{
			menuItems[2].gameObject.SetActive(value: false);
		}
		if (Level.IsTowerOfPower)
		{
			ReplaceRestartWRestartTowerOfPower();
		}
		options.Init(checkIfDead);
		if (achievements != null)
		{
			achievements.Init(checkIfDead);
		}
		if (restartTowerConfirm != null)
		{
			restartTowerConfirm.Init(checkIfDead);
		}
	}

	public void ForceDisablePause(bool value)
	{
		forceDisablePause = value;
	}

	protected override void OnPause()
	{
		base.OnPause();
		if (CupheadLevelCamera.Current != null)
		{
			CupheadLevelCamera.Current.StartBlur();
		}
		else
		{
			CupheadMapCamera.Current.StartBlur();
		}
		PlayerManager.SetPlayerCanSwitch(PlayerId.PlayerOne, PlatformHelper.CanSwitchUserFromPause);
		PlayerManager.SetPlayerCanSwitch(PlayerId.PlayerTwo, PlatformHelper.CanSwitchUserFromPause);
		PlayerManager.SetPlayerCanJoin(PlayerId.PlayerTwo, canJoin: false, promptBeforeJoin: false);
		menuItems[4].gameObject.SetActive(PlayerManager.Multiplayer);
		if (LevelPauseGUI.OnPauseEvent != null)
		{
			LevelPauseGUI.OnPauseEvent();
		}
		selection = 0;
	}

	protected override void OnUnpause()
	{
		base.OnUnpause();
		if (CupheadLevelCamera.Current != null)
		{
			CupheadLevelCamera.Current.EndBlur();
		}
		else
		{
			CupheadMapCamera.Current.EndBlur();
		}
		if (Level.Current != null && Level.Current.CurrentLevel == Levels.Airplane)
		{
			SettingsData.Save();
			if (PlatformHelper.IsConsole)
			{
				SettingsData.SaveToCloud();
			}
		}
		PlayerManager.SetPlayerCanSwitch(PlayerId.PlayerOne, canSwitch: false);
		PlayerManager.SetPlayerCanSwitch(PlayerId.PlayerTwo, canSwitch: false);
		PlayerManager.SetPlayerCanJoin(PlayerId.PlayerTwo, canJoin: true, promptBeforeJoin: true);
		if (LevelPauseGUI.OnUnpauseEvent != null)
		{
			LevelPauseGUI.OnUnpauseEvent();
		}
	}

	private void OnDestroy()
	{
		PauseManager.Unpause();
	}

	protected override void Update()
	{
		base.Update();
		if (base.state != State.Paused || options.optionMenuOpen || options.justClosed || (achievements != null && (achievements.achievementsMenuOpen || achievements.justClosed)) || (restartTowerConfirm != null && (restartTowerConfirm.restartTowerConfirmMenuOpen || restartTowerConfirm.justClosed)))
		{
			return;
		}
		if (GetButtonDown(CupheadButton.Pause) || GetButtonDown(CupheadButton.Cancel))
		{
			Unpause();
		}
		else if (Level.Current != null && Level.Current.CurrentLevel == Levels.Airplane && selection == 2 && (GetButtonDown(CupheadButton.Accept) || GetButtonDown(CupheadButton.MenuLeft) || GetButtonDown(CupheadButton.MenuRight)))
		{
			MenuSelectSound();
			ToggleRotateControls();
		}
		else if (GetButtonDown(CupheadButton.Accept))
		{
			MenuSelectSound();
			Select();
		}
		else if (_selectionTimer >= 0.15f)
		{
			if (GetButton(CupheadButton.MenuUp))
			{
				MenuMoveSound();
				selection--;
			}
			if (GetButton(CupheadButton.MenuDown))
			{
				MenuMoveSound();
				selection++;
			}
		}
		else
		{
			_selectionTimer += Time.deltaTime;
		}
	}

	private void Select()
	{
		switch (selection)
		{
		case 0:
			Unpause();
			break;
		case 1:
			Restart();
			break;
		case 2:
			Achievements();
			break;
		case 3:
			Options();
			break;
		case 4:
			Player2Leave();
			break;
		case 5:
			Exit();
			break;
		case 6:
			ExitToTitle();
			break;
		case 7:
			ExitToDesktop();
			break;
		}
	}

	protected override void OnUnpauseSound()
	{
		base.OnUnpauseSound();
	}

	private void UpdateSelection()
	{
		_selectionTimer = 0f;
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

	private void Restart()
	{
		if (Level.IsTowerOfPower)
		{
			RestartTowerConfirm();
			return;
		}
		OnUnpauseSound();
		base.state = State.Animating;
		PlayerManager.SetPlayerCanSwitch(PlayerId.PlayerOne, canSwitch: false);
		PlayerManager.SetPlayerCanSwitch(PlayerId.PlayerTwo, canSwitch: false);
		SceneLoader.ReloadLevel();
		Dialoguer.EndDialogue();
		if (Level.IsDicePalaceMain || Level.IsDicePalace)
		{
			DicePalaceMainLevelGameInfo.CleanUpRetry();
		}
	}

	private void ReplaceRestartWRestartTowerOfPower()
	{
		retryLocHelper.currentID = Localization.Find("OptionMenuRestartTower").id;
		retryLocHelper.ApplyTranslation();
	}

	private void Exit()
	{
		base.state = State.Animating;
		PlayerManager.SetPlayerCanSwitch(PlayerId.PlayerOne, canSwitch: false);
		PlayerManager.SetPlayerCanSwitch(PlayerId.PlayerTwo, canSwitch: false);
		Dialoguer.EndDialogue();
		if (Level.IsDicePalaceMain || Level.IsDicePalace)
		{
			DicePalaceMainLevelGameInfo.CleanUpRetry();
		}
		SceneLoader.LoadLastMap();
	}

	private void Player2Leave()
	{
		PlayerManager.PlayerLeave(PlayerId.PlayerTwo);
		Unpause();
	}

	private void ExitToTitle()
	{
		base.state = State.Animating;
		PlayerManager.ResetPlayers();
		Dialoguer.EndDialogue();
		SceneLoader.LoadScene(Scenes.scene_title, SceneLoader.Transition.Fade, SceneLoader.Transition.Fade);
	}

	private void ExitToDesktop()
	{
		Dialoguer.EndDialogue();
		Application.Quit();
	}

	private void Options()
	{
		StartCoroutine(in_options_cr());
	}

	private IEnumerator in_options_cr()
	{
		HideImmediate();
		options.ShowMainOptionMenu();
		PlayerManager.SetPlayerCanSwitch(PlayerId.PlayerOne, canSwitch: false);
		PlayerManager.SetPlayerCanSwitch(PlayerId.PlayerTwo, canSwitch: false);
		while (options.optionMenuOpen)
		{
			yield return null;
		}
		PlayerManager.SetPlayerCanSwitch(PlayerId.PlayerOne, PlatformHelper.CanSwitchUserFromPause);
		PlayerManager.SetPlayerCanSwitch(PlayerId.PlayerTwo, PlatformHelper.CanSwitchUserFromPause);
		selection = 0;
		ShowImmediate();
		yield return null;
	}

	private void RestartTowerConfirm()
	{
		StartCoroutine(in_restarttowerconfirm_cr());
	}

	private IEnumerator in_restarttowerconfirm_cr()
	{
		HideImmediate();
		restartTowerConfirm.ShowMenu();
		PlayerManager.SetPlayerCanSwitch(PlayerId.PlayerOne, canSwitch: false);
		PlayerManager.SetPlayerCanSwitch(PlayerId.PlayerTwo, canSwitch: false);
		while (restartTowerConfirm.restartTowerConfirmMenuOpen)
		{
			yield return null;
		}
		PlayerManager.SetPlayerCanSwitch(PlayerId.PlayerOne, PlatformHelper.CanSwitchUserFromPause);
		PlayerManager.SetPlayerCanSwitch(PlayerId.PlayerTwo, PlatformHelper.CanSwitchUserFromPause);
		selection = 0;
		ShowImmediate();
		yield return null;
	}

	private void Achievements()
	{
		StartCoroutine(in_achievements_cr());
	}

	private IEnumerator in_achievements_cr()
	{
		HideImmediate();
		achievements.ShowAchievements();
		PlayerManager.SetPlayerCanSwitch(PlayerId.PlayerOne, canSwitch: false);
		PlayerManager.SetPlayerCanSwitch(PlayerId.PlayerTwo, canSwitch: false);
		while (achievements.achievementsMenuOpen)
		{
			yield return null;
		}
		PlayerManager.SetPlayerCanSwitch(PlayerId.PlayerOne, PlatformHelper.CanSwitchUserFromPause);
		PlayerManager.SetPlayerCanSwitch(PlayerId.PlayerTwo, PlatformHelper.CanSwitchUserFromPause);
		selection = 0;
		ShowImmediate();
		yield return null;
	}

	private void ToggleRotateControls()
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

	private void onLanguageChangedEventHandler()
	{
		if (Level.Current != null && Level.Current.CurrentLevel == Levels.Airplane)
		{
			StartCoroutine(changeRotationToggleLanguage_cr());
		}
	}

	private IEnumerator changeRotationToggleLanguage_cr()
	{
		yield return null;
		yield return null;
		yield return null;
		updateRotateControlsToggleVisualValue();
	}

	protected override void InAnimation(float i)
	{
	}

	protected override void OutAnimation(float i)
	{
	}
}
