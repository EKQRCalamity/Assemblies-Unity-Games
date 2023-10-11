using System;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
	private static ResourceBlueprint<GameObject> _HelpMenuBlueprint = "UI/Help Menu";

	private static readonly HashSet<object> _BlockInputRequests = new HashSet<object>();

	public Canvas canvas;

	public CanvasInputFocus inputFocus;

	public RectTransform container;

	public BoolEvent onShowChange;

	public StringEvent onVersionChange;

	[Header("Visibility Events")]
	public BoolEvent onEndAdventureChange;

	public BoolEvent onShowEndAdventureChange;

	public BoolEvent onShowGearIconChange;

	public BoolEvent onInputEnabledChange;

	private bool _paused;

	private GameObject _optionsMenu;

	private GameObject _helpMenu;

	private bool _inputEnabled = true;

	public static PauseMenu Instance { get; private set; }

	public bool paused
	{
		get
		{
			return _paused;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _paused, value))
			{
				_OnPausedChange();
			}
		}
	}

	public bool inputEnabled
	{
		get
		{
			return _inputEnabled;
		}
		private set
		{
			if (SetPropertyUtility.SetStruct(ref _inputEnabled, value))
			{
				onInputEnabledChange?.Invoke(value);
			}
		}
	}

	public bool optionsActive => _optionsMenu;

	private GameState gameState => GameState.Instance;

	public static void Prewarm()
	{
		Instance?.Show();
		Instance?.Options();
		Instance?._optionsMenu?.GetComponent<UIPopupControl>().Close();
		Instance?.Hide();
	}

	public static void BlockInput(object blockRequestedBy)
	{
		if (_BlockInputRequests.Add(blockRequestedBy) && (bool)Instance)
		{
			Instance.inputEnabled = false;
		}
	}

	public static void UnblockInput(object blockRequestedBy)
	{
		if (_BlockInputRequests.Remove(blockRequestedBy) && (bool)Instance && _BlockInputRequests.Count == 0)
		{
			Instance.inputEnabled = true;
		}
	}

	private void _OnPausedChange()
	{
		AudioListener.pause = paused;
		Time.timeScale = paused.ToInt(0, 1);
		onShowChange?.Invoke(paused);
		if (paused)
		{
			BoolEvent boolEvent = onShowEndAdventureChange;
			if (boolEvent != null)
			{
				GameState obj = gameState;
				boolEvent.Invoke(obj != null && obj.parameters.adventureStarted && !gameState.parameters.adventureEnded);
			}
			BoolEvent boolEvent2 = onEndAdventureChange;
			if (boolEvent2 != null)
			{
				GameState obj2 = gameState;
				boolEvent2.Invoke(obj2 != null && obj2.stack.activeStep?.canSafelyCancelStack == true);
			}
		}
	}

	private void _EndAdventure()
	{
		gameState.stack.Cancel();
		if (gameState.experience > 0)
		{
			gameState.stack.Push(new GameStepGroupLossMedia());
		}
		gameState.stack.Push(new GameStepGroupRewards(AdventureEndType.Forfeit));
		gameState.stack.Push(new GameStepAnimateGameStateClear());
		gameState.stack.Push(new GameStepTransitionToNewGameState());
		Hide();
	}

	private void _OnOptionsClose()
	{
		ProfileManager.Profile.SaveOptions(applyChanges: true);
		_optionsMenu = null;
	}

	private void Awake()
	{
		onVersionChange?.Invoke(IOUtil.VersionString);
		onShowGearIconChange?.Invoke(ProfileManager.options.game.preferences.showGearIcon);
		inputEnabled = _BlockInputRequests.Count == 0;
	}

	private void OnEnable()
	{
		Instance = this;
	}

	private void Update()
	{
		if (inputEnabled && InputManager.I[KeyAction.Pause][KState.Clicked] && inputFocus.HasFocusPermissive())
		{
			Toggle();
		}
		else if (InputManager.I[KeyAction.Back][KState.Clicked] && inputFocus.HasFocusPermissive())
		{
			Hide();
		}
	}

	private void OnDisable()
	{
		Instance = ((Instance == this) ? null : Instance);
	}

	public void Show()
	{
		paused = true;
	}

	public void Hide()
	{
		paused = false;
	}

	public void Toggle()
	{
		paused = !paused;
	}

	public void Resume()
	{
		Hide();
	}

	public void Options()
	{
		string title = MessageData.UIPopupTitle.Options.GetTitle().Localize();
		GameObject mainContent = UIUtil.CreateReflectedObject(ProfileManager.options, 1280f, 720f, persistUI: true);
		Transform parent = canvas.transform;
		Action onClose = _OnOptionsClose;
		_optionsMenu = UIUtil.CreatePopup(title, mainContent, null, null, null, null, null, onClose, true, true, null, null, null, parent, null, null);
	}

	public void RefreshOptionsMenu()
	{
		GameObject optionsMenu = _optionsMenu;
		if ((object)optionsMenu != null)
		{
			optionsMenu.GetComponent<UIPopupControl>().Close();
			UnityEngine.Object.DestroyImmediate(optionsMenu);
		}
		Options();
	}

	public void EndAdventure()
	{
		UIUtil.CreatePopup(MessageData.UIPopupTitle.EndAdventure.GetTitle().Localize(), UIUtil.CreateMessageBox(MessageData.UIPopupMessage.EndAdventure.GetMessage().Localize()), null, parent: canvas.transform, buttons: new string[2]
		{
			MessageData.UIPopupButton.Cancel.GetButton().Localize(),
			MessageData.UIPopupButton.EndAdventure.GetButton().Localize()
		}, size: null, centerReferece: null, center: null, pivot: null, onClose: null, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: delegate(string s)
		{
			if (s == MessageData.UIPopupButton.EndAdventure.GetButton().Localize())
			{
				_EndAdventure();
			}
		});
	}

	public void OpenHelpMenu()
	{
		if (!_helpMenu)
		{
			_helpMenu = UnityEngine.Object.Instantiate(_HelpMenuBlueprint.value, container, worldPositionStays: false);
		}
		_helpMenu.SetActive(value: true);
	}

	public void ExitGame()
	{
		GameUtil.ExitApplicationPopup(MessageData.UIPopupMessage.ExitGame.GetMessage().Localize(), canvas.transform, MessageData.UIPopupTitle.ExitGame.GetTitle().Localize(), MessageData.UIPopupButton.ExitGame.GetButton().Localize(), MessageData.UIPopupButton.Cancel.GetButton().Localize());
	}

	public void JoinDiscord()
	{
		UIUtil.JoinDiscord(canvas.transform);
	}

	public void OpenStorePage()
	{
		UIUtil.OpenStorePage(canvas.transform);
	}
}
