using System;
using System.Collections.Generic;
using Framework.FrameworkCore;
using Framework.Util;
using Rewired;
using Tools.DataContainer;
using UnityEngine;

namespace Framework.Managers;

public class InputManager : GameSystem
{
	private readonly List<string> inputBlockers = new List<string>();

	public readonly bool UseDebugControllers;

	public ControllerType debugControllerType;

	public JoystickType debugJoystickType;

	private ControllerType _activeControllerType;

	public bool InputBlocked { get; private set; }

	private Controller Keyboard { get; set; }

	private Controller PrevKeyboard { get; set; }

	private Controller Joystick { get; set; }

	private Controller PrevJoystick { get; set; }

	public ControllerType ActiveControllerType
	{
		get
		{
			return (!UseDebugControllers) ? _activeControllerType : debugControllerType;
		}
		set
		{
			_activeControllerType = value;
		}
	}

	private bool JoystickConnected => ReInput.controllers.Joysticks.Count > 0;

	public JoystickType ActiveJoystickModel
	{
		get
		{
			if (UseDebugControllers)
			{
				return debugJoystickType;
			}
			IList<Joystick> joysticks = ReInput.controllers.Joysticks;
			Joystick device = null;
			if (JoystickConnected)
			{
				Joystick joystick = (Joystick)ReInput.players.GetPlayer(0).controllers.GetLastActiveController(ControllerType.Joystick);
				Joystick joystick2 = ((ReInput.players.GetPlayer(0).controllers.joystickCount <= 0) ? null : ReInput.players.GetPlayer(0).controllers.Joysticks[0]);
				device = joystick ?? joystick2;
			}
			return GetJoystickType(device);
		}
	}

	public Controller ActiveController
	{
		get
		{
			switch (ActiveControllerType)
			{
			case ControllerType.Keyboard:
				return Keyboard;
			case ControllerType.Joystick:
			{
				Joystick joystick = (Joystick)ReInput.players.GetPlayer(0).controllers.GetLastActiveController(ControllerType.Joystick);
				Joystick joystick2 = ((ReInput.players.GetPlayer(0).controllers.joystickCount <= 0) ? null : ReInput.players.GetPlayer(0).controllers.Joysticks[0]);
				Joystick = joystick ?? joystick2 ?? Keyboard;
				return Joystick;
			}
			default:
				return Keyboard;
			}
		}
	}

	private bool AxisTouched
	{
		get
		{
			Joystick joystick = (Joystick)ReInput.controllers.GetLastActiveController(ControllerType.Joystick);
			if (joystick != null)
			{
				IList<Controller.Axis> axes = joystick.Axes;
				for (int i = 0; i < axes.Count; i++)
				{
					if (axes[i].value != 0f)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public event Core.SimpleEvent OnInputLocked;

	public event Core.SimpleEvent OnInputUnlocked;

	public event Core.SimpleEvent KeyboardPressed;

	public event Core.SimpleEvent JoystickPressed;

	public JoystickType GetJoystickType(Joystick device)
	{
		if (device == null)
		{
			return JoystickType.None;
		}
		if (device.name.ToLower().Contains("xinput") || device.hardwareTypeGuid == new Guid("19002688-7406-4f4a-8340-8d25335406c8") || device.hardwareTypeGuid == new Guid("d74a350e-fe8b-4e9e-bbcd-efff16d34115"))
		{
			return JoystickType.XBOX;
		}
		if (device.name.ToLower().Contains("dualshock") || device.hardwareTypeGuid == new Guid("71dfe6c8-9e81-428f-a58e-c7e664b7fbed") || device.hardwareTypeGuid == new Guid("cd9718bf-a87a-44bc-8716-60a0def28a9f") || device.hardwareTypeGuid == new Guid("c3ad3cad-c7cf-4ca8-8c2e-e3df8d9960bb"))
		{
			return JoystickType.PlayStation;
		}
		if (device.name.ToLower().Contains("switch") || device.hardwareTypeGuid == new Guid("7bf3154b-9db8-4d52-950f-cd0eed8a5819") || device.hardwareTypeGuid == new Guid("521b808c-0248-4526-bc10-f1d16ee76bf1") || device.hardwareTypeGuid == new Guid("1fbdd13b-0795-4173-8a95-a2a75de9d204"))
		{
			return JoystickType.Switch;
		}
		return JoystickType.Generic;
	}

	public override void Start()
	{
		PlayMakerGUI.LockCursor = false;
		PlayMakerGUI.HideCursor = true;
		Debug.LogWarning("BLOCKING INPUT until main menu is fully shown");
		SetBlocker("InitialBlocker", blocking: true);
		ActiveControllerType = (JoystickConnected ? ControllerType.Joystick : ControllerType.Keyboard);
		Keyboard = ReInput.players.GetPlayer(0).controllers.Keyboard;
		IList<Joystick> joysticks = ReInput.players.GetPlayer(0).controllers.Joysticks;
		Joystick = ((joysticks.Count <= 0) ? null : joysticks[0]);
		SendInputChangedEvent();
	}

	public override void Update()
	{
		if (ReInput.isReady && Core.ControlRemapManager.ListeningForInputDone)
		{
			UpdateMainInput();
		}
	}

	private void UpdateMainInput()
	{
		Joystick joystick = (Joystick)ReInput.players.GetPlayer(0).controllers.GetLastActiveController(ControllerType.Joystick);
		if (PrevJoystick == null)
		{
			PrevJoystick = Joystick ?? joystick;
		}
		if (PrevKeyboard == null)
		{
			PrevKeyboard = Keyboard;
		}
		if (ReInput.controllers.Keyboard.GetAnyButton() || ReInput.controllers.Mouse.GetAnyButton())
		{
			SetMainInput(ControllerType.Keyboard, Keyboard.id);
			PrevKeyboard = Keyboard;
		}
		else if (JoystickConnected && joystick != null && (joystick.GetAnyButton() || AxisTouched))
		{
			SetMainInput(ControllerType.Joystick, PrevJoystick.id);
			PrevJoystick = Joystick;
		}
	}

	public void ResetManager()
	{
		SendInputChangedEvent();
		RemoveBlockers();
	}

	public void SetBlocker(string name, bool blocking)
	{
		if (!inputBlockers.Contains(name) && blocking)
		{
			inputBlockers.Add(name);
			if (this.OnInputLocked != null)
			{
				this.OnInputLocked();
			}
			Log.Trace("Input", "The input blocker <color=green>" + name + "</color> has been enabled.");
		}
		if (inputBlockers.Contains(name) && !blocking)
		{
			inputBlockers.Remove(name);
			if (this.OnInputUnlocked != null)
			{
				this.OnInputUnlocked();
			}
			Log.Trace("Input", "The input blocker <color=yellow>" + name + "</color> has been disabled.");
		}
		InputBlocked = inputBlockers.Count > 0;
	}

	public bool HasBlocker(string name)
	{
		return inputBlockers.Contains(name);
	}

	private void RemoveBlockers()
	{
		inputBlockers.Clear();
		Log.Trace("Input", "All blockers have been removed.");
	}

	private void SetMainInput(ControllerType type, int controllerId)
	{
		if (ActiveControllerType != type || ActiveController.id != controllerId)
		{
			ActiveControllerType = type;
			SendInputChangedEvent();
			Log.Trace("Input", "Main input is " + ActiveControllerType.ToString().ToUpper() + ".");
		}
	}

	private void SendInputChangedEvent()
	{
		if (ActiveControllerType == ControllerType.Joystick && this.JoystickPressed != null)
		{
			this.JoystickPressed();
		}
		if (ActiveControllerType == ControllerType.Keyboard && this.KeyboardPressed != null)
		{
			this.KeyboardPressed();
		}
	}

	public void ApplyRumble(RumbleData rumble)
	{
		SingletonSerialized<RumbleSystem>.Instance.ApplyRumble(rumble);
	}

	public void StopRumble(string id)
	{
		SingletonSerialized<RumbleSystem>.Instance.StopRumble(id);
	}

	public void StopAllRumbles()
	{
		SingletonSerialized<RumbleSystem>.Instance.StopAllRumbles();
	}

	public List<string> AppliedRumbles()
	{
		return SingletonSerialized<RumbleSystem>.Instance.AppliedRumbles();
	}

	public override void OnGUI()
	{
		DebugResetLine();
		DebugDrawTextLine("******    Input");
		DebugDrawTextLine("Blockers");
		DebugDrawTextLine("------------------------");
		foreach (string inputBlocker in inputBlockers)
		{
			DebugDrawTextLine(inputBlocker);
		}
	}
}
