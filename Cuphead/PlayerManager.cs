using System;
using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PlayerManager
{
	private class PlayerSlot
	{
		public enum JoinState
		{
			NotJoining,
			JoinPromptDisplayed,
			JoinRequested,
			Joined,
			Leaving
		}

		public enum ControllerState
		{
			NoController,
			UsingController,
			Disconnected,
			ReconnectPromptDisplayed,
			WaitingForReconnect
		}

		public bool canJoin;

		public JoinState joinState;

		public ControllerState controllerState;

		public bool canSwitch;

		public bool requestedSwitch;

		public bool promptBeforeJoin;

		public int controllerId;

		public bool shouldAssignController;

		public bool controllerDisconnectFromPlm;

		public ControllerType lastController = ControllerType.Custom;
	}

	public delegate void PlayerChangedDelegate(PlayerId playerId);

	private const float SINGLE_PLAYER_DAMAGE_MULTIPLIER = 1f;

	private const float MULTIPLAYER_DAMAGE_MULTIPLIER = 0.5f;

	private static PlayerSlot[] playerSlots = new PlayerSlot[2]
	{
		new PlayerSlot(),
		new PlayerSlot()
	};

	public static bool Multiplayer;

	private static bool shouldGoToSlotSelect = false;

	private static bool shouldGoToStartScreen = false;

	private static bool pausedDueToPlm = false;

	public static int player1DisconnectedControllerId;

	public static bool player1IsMugman;

	public static bool[] playerWasChalice = new bool[2];

	private static Dictionary<int, Player> playerInputs;

	private static Dictionary<int, AbstractPlayerController> players;

	private static PlayerId currentId;

	public static bool ShouldShowJoinPrompt => playerSlots[1].joinState == PlayerSlot.JoinState.JoinPromptDisplayed;

	public static AbstractPlayerController Current => GetPlayer(currentId);

	public static int Count
	{
		get
		{
			int num = 0;
			foreach (int key in players.Keys)
			{
				if (DoesPlayerExist((PlayerId)key) && !GetPlayer((PlayerId)key).IsDead)
				{
					num++;
				}
			}
			return num;
		}
	}

	public static Vector2 Center
	{
		get
		{
			if (!Multiplayer || Count < 2)
			{
				return GetFirst().center;
			}
			return (players[0].center + players[1].center) / 2f;
		}
	}

	public static Vector2 CameraCenter
	{
		get
		{
			if (!Multiplayer || Count < 2)
			{
				return GetFirst().CameraCenter;
			}
			return (players[0].center + players[1].CameraCenter) / 2f;
		}
	}

	public static Vector2 TopPlayerPosition
	{
		get
		{
			if (!Multiplayer || Count < 2)
			{
				return GetFirst().transform.position;
			}
			float y = Mathf.Max(players[0].transform.position.y, players[1].transform.position.y);
			return new Vector2((players[0].transform.position.x + players[0].transform.position.x) / 2f, y);
		}
	}

	public static float DamageMultiplier
	{
		get
		{
			if (Count > 1)
			{
				return 0.5f;
			}
			return 1f;
		}
	}

	public static event PlayerChangedDelegate OnPlayerJoinedEvent;

	public static event PlayerChangedDelegate OnPlayerLeaveEvent;

	public static event Action OnControlsChanged;

	public static void Awake()
	{
		Multiplayer = false;
		players = new Dictionary<int, AbstractPlayerController>();
		players.Add(0, null);
		players.Add(1, null);
		playerInputs = new Dictionary<int, Player>();
		playerInputs.Add(0, ReInput.players.GetPlayer(0));
		playerInputs.Add(1, ReInput.players.GetPlayer(1));
	}

	public static void Init()
	{
		OnlineManager.Instance.Interface.OnUserSignedIn += OnUserSignedIn;
		OnlineManager.Instance.Interface.OnUserSignedOut += OnUserSignedOut;
		ReInput.ControllerConnectedEvent += OnControllerConnected;
		ReInput.ControllerDisconnectedEvent += OnControllerDisconnected;
		PlmManager.Instance.Interface.OnUnconstrained += OnUnconstrained;
		PlmManager.Instance.Interface.OnResume += OnResume;
		PlmManager.Instance.Interface.OnSuspend += OnSuspend;
	}

	public static void SetPlayerCanJoin(PlayerId player, bool canJoin, bool promptBeforeJoin)
	{
		PlayerSlot playerSlot = ((player != 0) ? playerSlots[1] : playerSlots[0]);
		playerSlot.canJoin = canJoin;
		playerSlot.promptBeforeJoin = promptBeforeJoin;
		if (!canJoin && playerSlot.joinState == PlayerSlot.JoinState.JoinPromptDisplayed)
		{
			playerSlot.joinState = PlayerSlot.JoinState.NotJoining;
		}
	}

	public static void ClearJoinPrompt()
	{
		for (int i = 0; i < 2; i++)
		{
			if (playerSlots[i].joinState == PlayerSlot.JoinState.JoinPromptDisplayed)
			{
				playerSlots[i].joinState = PlayerSlot.JoinState.NotJoining;
			}
		}
	}

	public static void SetPlayerCanSwitch(PlayerId player, bool canSwitch)
	{
		PlayerSlot playerSlot = ((player != 0) ? playerSlots[1] : playerSlots[0]);
		playerSlot.canSwitch = canSwitch;
		playerSlot.requestedSwitch = false;
	}

	public static void PlayerLeave(PlayerId player)
	{
		PlayerSlot playerSlot = ((player != 0) ? playerSlots[1] : playerSlots[0]);
		playerSlot.joinState = PlayerSlot.JoinState.Leaving;
	}

	public static void OnChaliceCharmUnequipped(PlayerId player)
	{
		playerWasChalice[(int)player] = false;
	}

	private static void OnControllerConnected(ControllerStatusChangedEventArgs args)
	{
	}

	public static void Update()
	{
		if (InterruptingPrompt.IsInterrupting())
		{
			for (int i = 0; i < playerSlots.Length; i++)
			{
				if (playerSlots[i].joinState == PlayerSlot.JoinState.Joined && playerSlots[i].controllerState == PlayerSlot.ControllerState.ReconnectPromptDisplayed)
				{
					PlayerId playerId = ((i != 0) ? PlayerId.PlayerTwo : PlayerId.PlayerOne);
					Joystick joystick = CupheadInput.CheckForUnconnectedControllerPress();
					Player playerInput = GetPlayerInput(playerId);
					if (joystick != null)
					{
						playerSlots[i].controllerState = PlayerSlot.ControllerState.UsingController;
						playerSlots[i].controllerId = joystick.id;
						playerSlots[i].controllerDisconnectFromPlm = false;
						playerSlots[i].lastController = ControllerType.Joystick;
						playerInput.controllers.AddController(joystick, removeFromOtherPlayers: true);
						ReInput.userDataStore.LoadControllerData(playerInputs[(int)playerId].id, ControllerType.Joystick, playerSlots[i].controllerId);
						ControlsChanged();
					}
					if (!PlatformHelper.IsConsole && playerInput.GetAnyButtonDown())
					{
						playerSlots[i].controllerState = PlayerSlot.ControllerState.NoController;
						playerSlots[i].controllerDisconnectFromPlm = false;
						ControlsChanged();
						playerSlots[i].lastController = ControllerType.Keyboard;
					}
				}
			}
			return;
		}
		for (int j = 0; j < playerSlots.Length; j++)
		{
			if (!playerSlots[j].canJoin || playerSlots[j].joinState == PlayerSlot.JoinState.Joined)
			{
				continue;
			}
			PlayerId playerId2 = ((j != 0) ? PlayerId.PlayerTwo : PlayerId.PlayerOne);
			bool flag = false;
			Joystick joystick2 = CupheadInput.CheckForUnconnectedControllerPress();
			Player playerInput2 = GetPlayerInput(playerId2);
			if (joystick2 != null)
			{
				flag = true;
				playerSlots[j].controllerState = PlayerSlot.ControllerState.UsingController;
				playerSlots[j].controllerId = joystick2.id;
			}
			else if (!PlatformHelper.IsConsole)
			{
				bool num;
				if (SceneManager.GetActiveScene().name == "scene_title")
				{
					if (!playerInput2.controllers.Keyboard.GetAnyButtonDown())
					{
						goto IL_0218;
					}
					num = playerSlots[j].joinState == PlayerSlot.JoinState.NotJoining;
				}
				else
				{
					num = playerInput2.GetAnyButtonDown();
				}
				if (num)
				{
					flag = true;
					playerSlots[j].controllerState = PlayerSlot.ControllerState.NoController;
				}
			}
			goto IL_0218;
			IL_0218:
			if (!flag)
			{
				continue;
			}
			if (playerSlots[j].joinState == PlayerSlot.JoinState.NotJoining && playerSlots[j].promptBeforeJoin)
			{
				playerSlots[j].joinState = PlayerSlot.JoinState.JoinPromptDisplayed;
				continue;
			}
			bool flag2 = false;
			playerSlots[j].joinState = PlayerSlot.JoinState.JoinRequested;
			if (OnlineManager.Instance.Interface.SupportsMultipleUsers)
			{
				ulong value = (ulong)joystick2.systemId.Value;
				OnlineUser userForController = OnlineManager.Instance.Interface.GetUserForController(value);
				if (userForController != null && ((j == 0 && !userForController.Equals(OnlineManager.Instance.Interface.SecondaryUser)) || (j == 1 && !userForController.Equals(OnlineManager.Instance.Interface.MainUser))))
				{
					OnlineManager.Instance.Interface.SetUser(playerId2, userForController);
					flag2 = true;
				}
				else
				{
					OnlineManager.Instance.Interface.SignInUser(silent: false, playerId2, value);
				}
			}
			else if (OnlineManager.Instance.Interface.SupportsUserSignIn && playerId2 == PlayerId.PlayerOne)
			{
				OnlineManager.Instance.Interface.SignInUser(silent: false, playerId2, 0uL);
			}
			else
			{
				flag2 = true;
			}
			if (flag2)
			{
				if (joystick2 != null)
				{
					playerInput2.controllers.AddController(joystick2, removeFromOtherPlayers: true);
					ReInput.userDataStore.LoadControllerData(playerInputs[(int)playerId2].id, ControllerType.Joystick, playerSlots[j].controllerId);
				}
				playerSlots[j].joinState = PlayerSlot.JoinState.Joined;
				if (playerId2 == PlayerId.PlayerTwo)
				{
					Multiplayer = true;
				}
				PlayerManager.OnPlayerJoinedEvent(playerId2);
				AudioManager.Play("player_spawn");
			}
		}
		for (int k = 0; k < playerSlots.Length; k++)
		{
			if (!OnlineManager.Instance.Interface.SupportsUserSignIn || !playerSlots[k].canSwitch || playerSlots[k].joinState != PlayerSlot.JoinState.Joined || (!OnlineManager.Instance.Interface.SupportsMultipleUsers && k == 1))
			{
				continue;
			}
			PlayerId playerId3 = ((k != 0) ? PlayerId.PlayerTwo : PlayerId.PlayerOne);
			Player playerInput3 = GetPlayerInput(playerId3);
			if (playerInput3.GetButtonDown(11))
			{
				playerSlots[k].requestedSwitch = true;
				playerSlots[(k + 1) % 2].requestedSwitch = false;
				ulong controllerId = 0uL;
				if (playerInput3.controllers.joystickCount > 0)
				{
					controllerId = (ulong)playerInput3.controllers.Joysticks[0].systemId.Value;
				}
				OnlineManager.Instance.Interface.SwitchUser(playerId3, controllerId);
			}
		}
		for (int l = 0; l < playerSlots.Length; l++)
		{
			if (SceneLoader.CurrentlyLoading)
			{
				break;
			}
			if (playerSlots[l].joinState == PlayerSlot.JoinState.Leaving)
			{
				PlayerId playerId4 = ((l != 0) ? PlayerId.PlayerTwo : PlayerId.PlayerOne);
				Player playerInput4 = GetPlayerInput(playerId4);
				playerInput4.controllers.ClearControllersOfType<Joystick>();
				playerSlots[l].joinState = PlayerSlot.JoinState.NotJoining;
				if (playerId4 == PlayerId.PlayerTwo)
				{
					Multiplayer = false;
				}
				OnlineManager.Instance.Interface.SetRichPresenceActive(playerId4, active: false);
				OnlineManager.Instance.Interface.SetUser(playerId4, null);
				if (playerId4 == PlayerId.PlayerOne)
				{
					shouldGoToStartScreen = true;
				}
				else if (PlayerManager.OnPlayerLeaveEvent != null)
				{
					PlayerManager.OnPlayerLeaveEvent(playerId4);
					AudioManager.Play("player_despawn");
				}
			}
		}
		for (int m = 0; m < playerSlots.Length; m++)
		{
			if (playerSlots[m].shouldAssignController)
			{
				PlayerId playerId5 = ((m != 0) ? PlayerId.PlayerTwo : PlayerId.PlayerOne);
				Player playerInput5 = GetPlayerInput(playerId5);
				playerInput5.controllers.AddController<Joystick>(playerSlots[m].controllerId, removeFromOtherPlayers: true);
				ReInput.userDataStore.LoadControllerData(playerInputs[(int)playerId5].id, ControllerType.Joystick, playerSlots[m].controllerId);
				playerSlots[m].shouldAssignController = false;
			}
		}
		if (ControllerDisconnectedPrompt.Instance != null && !ControllerDisconnectedPrompt.Instance.Visible && ControllerDisconnectedPrompt.Instance.allowedToShow)
		{
			for (int n = 0; n < 2; n++)
			{
				PlayerId playerId6 = ((n != 0) ? PlayerId.PlayerTwo : PlayerId.PlayerOne);
				if (IsControllerDisconnected(playerId6, countWaitingForReconnectAsDisconnected: false))
				{
					ControllerDisconnectedPrompt.Instance.Show(playerId6);
					break;
				}
			}
		}
		if (PlmManager.Instance.Interface.IsConstrained())
		{
			if (InterruptingPrompt.CanInterrupt() && PauseManager.state != PauseManager.State.Paused)
			{
				PauseManager.Pause();
				pausedDueToPlm = true;
			}
		}
		else if (pausedDueToPlm)
		{
			PauseManager.Unpause();
			pausedDueToPlm = false;
		}
		if (shouldGoToSlotSelect)
		{
			goToSlotSelect();
			shouldGoToSlotSelect = false;
		}
		if (shouldGoToStartScreen)
		{
			goToStartScreen();
			shouldGoToStartScreen = false;
		}
		for (int num2 = 0; num2 < 2; num2++)
		{
			PlayerId id = ((num2 != 0) ? PlayerId.PlayerTwo : PlayerId.PlayerOne);
			Controller lastActiveController = GetPlayerInput(id).controllers.GetLastActiveController();
			if (lastActiveController != null && lastActiveController.type != playerSlots[num2].lastController)
			{
				playerSlots[num2].lastController = lastActiveController.type;
				ControlsChanged();
			}
		}
	}

	public static void ControllerRemapped(PlayerId playerId, bool usingController, int controllerId)
	{
		int num = ((playerId != 0) ? 1 : 0);
		playerSlots[num].controllerState = (usingController ? PlayerSlot.ControllerState.UsingController : PlayerSlot.ControllerState.NoController);
		playerSlots[num].controllerId = controllerId;
	}

	public static void ControlsChanged()
	{
		if (PlayerManager.OnControlsChanged != null)
		{
			PlayerManager.OnControlsChanged();
		}
	}

	private static void OnUserSignedIn(OnlineUser user)
	{
		for (int i = 0; i < playerSlots.Length; i++)
		{
			if (!playerSlots[i].canJoin || playerSlots[i].joinState != PlayerSlot.JoinState.JoinRequested)
			{
				continue;
			}
			OnlineManager.Instance.Interface.UpdateControllerMapping();
			if (user == null || (i == 0 && user.Equals(OnlineManager.Instance.Interface.SecondaryUser)) || (i == 1 && user.Equals(OnlineManager.Instance.Interface.MainUser)))
			{
				playerSlots[i].joinState = PlayerSlot.JoinState.NotJoining;
				continue;
			}
			PlayerId playerId = ((i != 0) ? PlayerId.PlayerTwo : PlayerId.PlayerOne);
			OnlineManager.Instance.Interface.SetUser(playerId, user);
			if (playerSlots[i].controllerState == PlayerSlot.ControllerState.UsingController)
			{
				playerSlots[i].shouldAssignController = true;
			}
			playerSlots[i].joinState = PlayerSlot.JoinState.Joined;
			if (playerId == PlayerId.PlayerTwo)
			{
				Multiplayer = true;
			}
			PlayerManager.OnPlayerJoinedEvent(playerId);
		}
		for (int j = 0; j < playerSlots.Length; j++)
		{
			if (!playerSlots[j].canSwitch || !playerSlots[j].requestedSwitch || playerSlots[j].joinState != PlayerSlot.JoinState.Joined)
			{
				continue;
			}
			OnlineManager.Instance.Interface.UpdateControllerMapping();
			playerSlots[j].requestedSwitch = false;
			PlayerId player = ((j != 0) ? PlayerId.PlayerTwo : PlayerId.PlayerOne);
			if (user != null && !user.Equals(OnlineManager.Instance.Interface.MainUser) && !user.Equals(OnlineManager.Instance.Interface.SecondaryUser))
			{
				OnlineManager.Instance.Interface.SetUser(player, user);
				if (j == 0)
				{
					shouldGoToSlotSelect = true;
				}
			}
		}
	}

	private static void OnUserSignedOut(PlayerId player, string name)
	{
		if (!PlmManager.Instance.Interface.IsConstrained())
		{
			PlayerSlot playerSlot = ((player != 0) ? playerSlots[1] : playerSlots[0]);
			if (!playerSlot.requestedSwitch)
			{
				PlayerLeave(player);
			}
		}
	}

	private static void OnControllerDisconnected(ControllerStatusChangedEventArgs args)
	{
		if (PlmManager.Instance.Interface.IsConstrained())
		{
			return;
		}
		for (int i = 0; i < playerSlots.Length; i++)
		{
			PlayerId playerId = ((i != 0) ? PlayerId.PlayerTwo : PlayerId.PlayerOne);
			if (playerSlots[i].controllerState == PlayerSlot.ControllerState.UsingController && playerSlots[i].controllerId == args.controllerId && playerSlots[i].joinState == PlayerSlot.JoinState.Joined)
			{
				playerInputs[(int)playerId].controllers.RemoveController<Joystick>(args.controllerId);
				playerSlots[i].controllerState = PlayerSlot.ControllerState.Disconnected;
				if (playerId == PlayerId.PlayerOne)
				{
					player1DisconnectedControllerId = args.controllerId;
				}
			}
		}
	}

	private static void OnSuspend()
	{
	}

	private static void OnResume()
	{
	}

	private static void OnCloudStorageInitialized(bool success)
	{
		if (!success)
		{
			OnlineManager.Instance.Interface.InitializeCloudStorage(PlayerId.PlayerOne, OnCloudStorageInitialized);
		}
	}

	private static void OnUnconstrained()
	{
		CheckForPairingsChanges();
	}

	private static void CheckForPairingsChanges()
	{
		bool flag = OnlineManager.Instance.Interface.ControllerMappingChanged();
		for (int i = 0; i < playerSlots.Length; i++)
		{
			PlayerId playerId = ((i != 0) ? PlayerId.PlayerTwo : PlayerId.PlayerOne);
			if (playerSlots[i].joinState != PlayerSlot.JoinState.Joined)
			{
				continue;
			}
			if (!OnlineManager.Instance.Interface.IsUserSignedIn(playerId))
			{
				PlayerLeave(playerId);
				if (playerId == PlayerId.PlayerOne)
				{
					PlayerLeave(PlayerId.PlayerTwo);
				}
				continue;
			}
			if (!flag)
			{
				if (playerSlots[i].controllerState == PlayerSlot.ControllerState.UsingController && playerInputs[(int)playerId].controllers.joystickCount == 0)
				{
					playerInputs[(int)playerId].controllers.AddController<Joystick>(playerSlots[i].controllerId, removeFromOtherPlayers: true);
					ReInput.userDataStore.LoadControllerData(playerInputs[(int)playerId].id, ControllerType.Joystick, playerSlots[i].controllerId);
				}
				continue;
			}
			List<ulong> controllersForUser = OnlineManager.Instance.Interface.GetControllersForUser(playerId);
			if (controllersForUser == null || controllersForUser.Count != 1)
			{
				playerInputs[(int)playerId].controllers.ClearControllersOfType<Joystick>();
				playerSlots[i].controllerState = PlayerSlot.ControllerState.Disconnected;
				playerSlots[i].controllerDisconnectFromPlm = true;
				continue;
			}
			ulong num = controllersForUser[0];
			foreach (Joystick joystick in ReInput.controllers.Joysticks)
			{
				if (joystick.systemId.Value == (long)num)
				{
					if (playerInputs[(int)playerId].controllers.joystickCount > 0)
					{
					}
					playerInputs[(int)playerId].controllers.ClearControllersOfType<Joystick>();
					playerInputs[(int)playerId].controllers.AddController(joystick, removeFromOtherPlayers: true);
					ReInput.userDataStore.LoadControllerData(playerInputs[(int)playerId].id, ControllerType.Joystick, playerSlots[i].controllerId);
					playerSlots[i].controllerId = joystick.id;
					break;
				}
			}
		}
	}

	public static void LoadControllerMappings(PlayerId player)
	{
		int num = ((player != 0) ? 1 : 0);
		ReInput.userDataStore.LoadControllerData(playerInputs[(int)player].id, ControllerType.Joystick, playerSlots[num].controllerId);
	}

	public static bool IsControllerDisconnected(PlayerId playerId, bool countWaitingForReconnectAsDisconnected = true)
	{
		int num = ((playerId != 0) ? 1 : 0);
		return playerSlots[num].joinState == PlayerSlot.JoinState.Joined && (playerSlots[num].controllerState == PlayerSlot.ControllerState.Disconnected || playerSlots[num].controllerState == PlayerSlot.ControllerState.ReconnectPromptDisplayed || (countWaitingForReconnectAsDisconnected && playerSlots[num].controllerState == PlayerSlot.ControllerState.WaitingForReconnect));
	}

	public static void OnDisconnectPromptDisplayed(PlayerId playerId)
	{
		int num = ((playerId != 0) ? 1 : 0);
		playerSlots[num].controllerState = PlayerSlot.ControllerState.ReconnectPromptDisplayed;
	}

	private static void goToSlotSelect()
	{
		Cuphead.Current.controlMapper.Close(save: true);
		playerSlots[0].canSwitch = false;
		playerSlots[0].requestedSwitch = false;
		playerSlots[0].canJoin = false;
		GetPlayerInput(PlayerId.PlayerTwo).controllers.ClearControllersOfType<Joystick>();
		playerSlots[1] = new PlayerSlot();
		Multiplayer = false;
		OnlineManager.Instance.Interface.SetUser(PlayerId.PlayerTwo, null);
		SceneLoader.LoadScene(Scenes.scene_slot_select, SceneLoader.Transition.Iris, SceneLoader.Transition.Iris);
	}

	private static void goToStartScreen()
	{
		Cuphead.Current.controlMapper.Close(save: true);
		ResetPlayers();
		if (StartScreenAudio.Instance != null)
		{
			UnityEngine.Object.Destroy(StartScreenAudio.Instance.gameObject);
		}
		SceneLoader.LoadScene(Scenes.scene_title, SceneLoader.Transition.Fade, SceneLoader.Transition.Fade);
	}

	public static void ResetPlayers()
	{
		playerSlots[0] = new PlayerSlot();
		playerSlots[1] = new PlayerSlot();
		GetPlayerInput(PlayerId.PlayerOne).controllers.ClearControllersOfType<Joystick>();
		GetPlayerInput(PlayerId.PlayerTwo).controllers.ClearControllersOfType<Joystick>();
		Multiplayer = false;
		if (OnlineManager.Instance.Interface.SupportsMultipleUsers)
		{
			OnlineManager.Instance.Interface.SetUser(PlayerId.PlayerOne, null);
			OnlineManager.Instance.Interface.SetUser(PlayerId.PlayerTwo, null);
		}
	}

	public static Player GetPlayerInput(PlayerId id)
	{
		return playerInputs[(int)id];
	}

	public static void SetPlayer(PlayerId id, AbstractPlayerController player)
	{
		players[(int)id] = player;
	}

	public static void ClearPlayer(PlayerId id)
	{
		players[(int)id] = null;
	}

	public static void ClearPlayers()
	{
		currentId = PlayerId.PlayerOne;
		players[0] = null;
		players[1] = null;
	}

	public static AbstractPlayerController GetPlayer(PlayerId id)
	{
		return players[(int)id];
	}

	public static T GetPlayer<T>(PlayerId id) where T : AbstractPlayerController
	{
		return GetPlayer(id) as T;
	}

	public static AbstractPlayerController GetRandom()
	{
		if (!Multiplayer || !DoesPlayerExist(PlayerId.PlayerTwo))
		{
			return players[0];
		}
		return GetPlayer(EnumUtils.Random<PlayerId>());
	}

	public static AbstractPlayerController GetNext()
	{
		if (!Multiplayer || !DoesPlayerExist(PlayerId.PlayerTwo))
		{
			return players[0];
		}
		if (!DoesPlayerExist(PlayerId.PlayerOne))
		{
			return players[1];
		}
		AbstractPlayerController current = Current;
		PlayerId playerId = currentId;
		if (playerId == PlayerId.PlayerOne || playerId != PlayerId.PlayerTwo)
		{
			currentId = PlayerId.PlayerTwo;
		}
		else
		{
			currentId = PlayerId.PlayerOne;
		}
		return current;
	}

	public static bool DoesPlayerExist(PlayerId player)
	{
		if (players[(int)player] == null)
		{
			return false;
		}
		if (players[(int)player].IsDead)
		{
			return false;
		}
		return true;
	}

	public static bool BothPlayersActive()
	{
		return DoesPlayerExist(PlayerId.PlayerOne) && DoesPlayerExist(PlayerId.PlayerTwo);
	}

	public static AbstractPlayerController GetFirst()
	{
		if (!DoesPlayerExist(PlayerId.PlayerOne))
		{
			return players[1];
		}
		return players[0];
	}

	public static Dictionary<int, AbstractPlayerController>.ValueCollection GetAllPlayers()
	{
		return players.Values;
	}
}
