using Rewired;

public class CupheadInput
{
	public enum InputDevice
	{
		Keyboard,
		Controller_1,
		Controller_2
	}

	public enum InputSymbols
	{
		XBOX_NONE,
		XBOX_A,
		XBOX_B,
		XBOX_X,
		XBOX_Y,
		XBOX_RB,
		XBOX_LB
	}

	public class AnyPlayerInput
	{
		private Player[] players;

		public bool checkIfDead;

		public AnyPlayerInput(bool checkIfDead = false)
		{
			this.checkIfDead = checkIfDead;
			players = new Player[2]
			{
				PlayerManager.GetPlayerInput(PlayerId.PlayerOne),
				PlayerManager.GetPlayerInput(PlayerId.PlayerTwo)
			};
		}

		public bool GetButton(CupheadButton button)
		{
			Player[] array = players;
			foreach (Player player in array)
			{
				if (player.GetButton((int)button) && (!checkIfDead || !IsDead(player)))
				{
					return true;
				}
			}
			return false;
		}

		public bool GetButtonDown(CupheadButton button)
		{
			if (InterruptingPrompt.IsInterrupting())
			{
				return false;
			}
			Player[] array = players;
			foreach (Player player in array)
			{
				if (player.GetButtonDown((int)button) && (!checkIfDead || !IsDead(player)))
				{
					return true;
				}
			}
			return false;
		}

		public bool GetActionButtonDown()
		{
			if (InterruptingPrompt.IsInterrupting())
			{
				return false;
			}
			Player[] array = players;
			foreach (Player player in array)
			{
				if ((player.GetButtonDown(13) || player.GetButtonDown(14) || player.GetButtonDown(7) || player.GetButtonDown(15) || player.GetButtonDown(2) || player.GetButtonDown(6) || player.GetButtonDown(8) || player.GetButtonDown(3) || player.GetButtonDown(4) || player.GetButtonDown(5)) && (!checkIfDead || !IsDead(player)))
				{
					return true;
				}
			}
			return false;
		}

		public bool GetAnyButtonDown()
		{
			if (InterruptingPrompt.IsInterrupting())
			{
				return false;
			}
			Player[] array = players;
			foreach (Player player in array)
			{
				foreach (Controller controller in player.controllers.Controllers)
				{
					if (controller.GetAnyButtonDown() && (!checkIfDead || !IsDead(player)))
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool GetAnyButtonHeld()
		{
			if (InterruptingPrompt.IsInterrupting())
			{
				return false;
			}
			Player[] array = players;
			foreach (Player player in array)
			{
				foreach (Controller controller in player.controllers.Controllers)
				{
					if (controller.GetAnyButton() && (!checkIfDead || !IsDead(player)))
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool GetButtonUp(CupheadButton button)
		{
			Player[] array = players;
			foreach (Player player in array)
			{
				if (player.GetButtonUp((int)button) && (!checkIfDead || !IsDead(player)))
				{
					return true;
				}
			}
			return false;
		}

		private bool IsDead(Player player)
		{
			PlayerId id = ((player != players[0]) ? PlayerId.PlayerTwo : PlayerId.PlayerOne);
			AbstractPlayerController player2 = PlayerManager.GetPlayer(id);
			return player2 == null || player2.IsDead;
		}
	}

	public class Pair
	{
		public readonly InputSymbols symbol;

		public readonly string first;

		public readonly string second;

		public Pair(InputSymbols symbol, string first, string second)
		{
			this.symbol = symbol;
			this.first = first;
			this.second = second;
		}
	}

	public static readonly Pair[] pairs = new Pair[4]
	{
		new Pair(InputSymbols.XBOX_A, "<sprite=0>", "<sprite=1>"),
		new Pair(InputSymbols.XBOX_B, "<sprite=2>", "<sprite=3>"),
		new Pair(InputSymbols.XBOX_X, "<sprite=4>", "<sprite=5>"),
		new Pair(InputSymbols.XBOX_Y, "<sprite=6>", "<sprite=7>")
	};

	public static Localization.Translation InputDisplayForButton(CupheadButton button, int rewiredPlayerId = 0)
	{
		ActionElementMap actionElementMap = null;
		Player.ControllerHelper controllers = ReInput.players.GetPlayer(rewiredPlayerId).controllers;
		if (controllers != null && controllers.joystickCount > 0)
		{
			ControllerType controllerType = ControllerType.Joystick;
			Controller lastActiveController = ReInput.players.GetPlayer(rewiredPlayerId).controllers.GetLastActiveController();
			if (lastActiveController != null)
			{
				controllerType = lastActiveController.type;
			}
			actionElementMap = controllers.maps.GetFirstElementMapWithAction(controllerType, (int)button, skipDisabledMaps: true);
		}
		else
		{
			if (PlatformHelper.IsConsole)
			{
				return default(Localization.Translation);
			}
			actionElementMap = ReInput.players.GetPlayer(rewiredPlayerId).controllers.maps.GetFirstElementMapWithAction((int)button, skipDisabledMaps: true);
		}
		if (actionElementMap == null)
		{
			Localization.Translation result = default(Localization.Translation);
			result.text = string.Empty;
			return result;
		}
		string text = actionElementMap.elementIdentifierName;
		if (button == CupheadButton.EquipMenu && text.Contains("Shift"))
		{
			text = "Shift";
		}
		string text2 = handleCustomGlyphs(text, rewiredPlayerId);
		Localization.Translation result2 = Localization.Translate(text);
		if (text2 == null)
		{
			if (!string.IsNullOrEmpty(result2.text))
			{
				text = result2.text;
			}
		}
		else
		{
			text = text2;
		}
		text = text.ToUpper();
		text = text.Replace(" SHOULDER", "B");
		text = text.Replace(" BUMPER", "B");
		text = text.Replace(" TRIGGER", "T");
		text = text.Replace("LEFT", "L");
		text = text.Replace("RIGHT", "R");
		text = text.Replace("R SHIFT", "SHIFT");
		text = text.Replace("L SHIFT", "SHIFT");
		text = text.Replace(" +", string.Empty);
		text = text.Replace(" -", string.Empty);
		result2.text = text;
		return result2;
	}

	private static string handleCustomGlyphs(string input, int rewiredPlayerId)
	{
		return null;
	}

	public static InputSymbols InputSymbolForButton(CupheadButton button)
	{
		InputSymbols inputSymbols = InputSymbols.XBOX_NONE;
		return button switch
		{
			CupheadButton.Accept => InputSymbols.XBOX_A, 
			CupheadButton.Jump => InputSymbols.XBOX_A, 
			CupheadButton.Cancel => InputSymbols.XBOX_B, 
			CupheadButton.Super => InputSymbols.XBOX_B, 
			CupheadButton.Shoot => InputSymbols.XBOX_X, 
			CupheadButton.Dash => InputSymbols.XBOX_Y, 
			CupheadButton.Lock => InputSymbols.XBOX_RB, 
			CupheadButton.SwitchWeapon => InputSymbols.XBOX_LB, 
			_ => InputSymbols.XBOX_NONE, 
		};
	}

	public static string DialogueStringFromButton(CupheadButton button)
	{
		return string.Concat(" {", button, "} ");
	}

	public static Joystick CheckForUnconnectedControllerPress()
	{
		foreach (Joystick joystick in ReInput.controllers.Joysticks)
		{
			if (ReInput.controllers.IsJoystickAssigned(joystick) || !joystick.GetAnyButtonDown())
			{
				continue;
			}
			return joystick;
		}
		return null;
	}

	public static Joystick CheckForControllerPress(long systemID)
	{
		foreach (Joystick joystick in ReInput.controllers.Joysticks)
		{
			long? systemId = joystick.systemId;
			if (systemId.GetValueOrDefault() == systemID && systemId.HasValue && joystick.GetAnyButtonDown())
			{
				return joystick;
			}
		}
		return null;
	}

	public static bool AutoAssignController(int rewiredPlayerId)
	{
		foreach (Joystick joystick in ReInput.controllers.Joysticks)
		{
			if (!ReInput.controllers.IsJoystickAssigned(joystick))
			{
				Player player = ReInput.players.GetPlayer(rewiredPlayerId);
				if (player != null && player.controllers.joystickCount <= 0)
				{
					player.controllers.AddController(joystick, removeFromOtherPlayers: true);
					return true;
				}
			}
		}
		return false;
	}
}
