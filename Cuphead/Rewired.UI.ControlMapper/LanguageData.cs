using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rewired.UI.ControlMapper;

public class LanguageData : ScriptableObject
{
	[Serializable]
	private class CustomEntry
	{
		public string key;

		public string value;

		public CustomEntry()
		{
		}

		public CustomEntry(string key, string value)
		{
			this.key = key;
			this.value = value;
		}

		public static Dictionary<string, string> ToDictionary(CustomEntry[] array)
		{
			if (array == null)
			{
				return new Dictionary<string, string>();
			}
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != null && !string.IsNullOrEmpty(array[i].key) && !string.IsNullOrEmpty(array[i].value))
				{
					if (dictionary.ContainsKey(array[i].key))
					{
						UnityEngine.Debug.LogError("Key \"" + array[i].key + "\" is already in dictionary!");
					}
					else
					{
						dictionary.Add(array[i].key, array[i].value);
					}
				}
			}
			return dictionary;
		}
	}

	[SerializeField]
	private string _yes = "Yes";

	[SerializeField]
	private string _no = "No";

	[SerializeField]
	private string _add = "Add";

	[SerializeField]
	private string _replace = "Replace";

	[SerializeField]
	private string _remove = "Remove";

	[SerializeField]
	private string _cancel = "Cancel";

	[SerializeField]
	private string _none = "None";

	[SerializeField]
	private string _okay = "Okay";

	[SerializeField]
	private string _done = "Done";

	[SerializeField]
	private string _default = "Default";

	[SerializeField]
	private string _assignControllerWindowTitle = "Choose Controller";

	[SerializeField]
	private string _assignControllerWindowMessage = "Press any button or move an axis on the controller you would like to use.";

	[SerializeField]
	private string _controllerAssignmentConflictWindowTitle = "Controller Assignment";

	[SerializeField]
	[Tooltip("{0} = Joystick Name\n{1} = Other Player Name\n{2} = This Player Name")]
	private string _controllerAssignmentConflictWindowMessage = "{0} is already assigned to {1}. Do you want to assign this controller to {2} instead?";

	[SerializeField]
	private string _elementAssignmentPrePollingWindowMessage = "First center or zero all sticks and axes and press any button or wait for the timer to finish.";

	[SerializeField]
	[Tooltip("{0} = Action Name")]
	private string _joystickElementAssignmentPollingWindowMessage = "Now press a button or move an axis to assign it to {0}.";

	[SerializeField]
	[Tooltip("This text is only displayed when split-axis fields have been disabled and the user clicks on the full-axis field. Button/key/D-pad input cannot be assigned to a full-axis field.\n{0} = Action Name")]
	private string _joystickElementAssignmentPollingWindowMessage_fullAxisFieldOnly = "Now move an axis to assign it to {0}.";

	[SerializeField]
	[Tooltip("{0} = Action Name")]
	private string _keyboardElementAssignmentPollingWindowMessage = "Press a key to assign it to {0}. Modifier keys may also be used. To assign a modifier key alone, hold it down for 1 second.";

	[SerializeField]
	[Tooltip("{0} = Action Name")]
	private string _mouseElementAssignmentPollingWindowMessage = "Press a mouse button or move an axis to assign it to {0}.";

	[SerializeField]
	[Tooltip("This text is only displayed when split-axis fields have been disabled and the user clicks on the full-axis field. Button/key/D-pad input cannot be assigned to a full-axis field.\n{0} = Action Name")]
	private string _mouseElementAssignmentPollingWindowMessage_fullAxisFieldOnly = "Move an axis to assign it to {0}.";

	[SerializeField]
	private string _elementAssignmentConflictWindowMessage = "Assignment Conflict";

	[SerializeField]
	[Tooltip("{0} = Element Name")]
	private string _elementAlreadyInUseBlocked = "{0} is already in use cannot be replaced.";

	[SerializeField]
	[Tooltip("{0} = Element Name")]
	private string _elementAlreadyInUseCanReplace = "{0} is already in use. Do you want to replace it?";

	[SerializeField]
	[Tooltip("{0} = Element Name")]
	private string _elementAlreadyInUseCanReplace_conflictAllowed = "{0} is already in use. Do you want to replace it? You may also choose to add the assignment anyway.";

	[SerializeField]
	private string _mouseAssignmentConflictWindowTitle = "Mouse Assignment";

	[SerializeField]
	[Tooltip("{0} = Other Player Name\n{1} = This Player Name")]
	private string _mouseAssignmentConflictWindowMessage = "The mouse is already assigned to {0}. Do you want to assign the mouse to {1} instead?";

	[SerializeField]
	private string _calibrateControllerWindowTitle = "Calibrate Controller";

	[SerializeField]
	private string _calibrateAxisStep1WindowTitle = "Calibrate Zero";

	[SerializeField]
	[Tooltip("{0} = Axis Name")]
	private string _calibrateAxisStep1WindowMessage = "Center or zero {0} and press any button or wait for the timer to finish.";

	[SerializeField]
	private string _calibrateAxisStep2WindowTitle = "Calibrate Range";

	[SerializeField]
	[Tooltip("{0} = Axis Name")]
	private string _calibrateAxisStep2WindowMessage = "Move {0} through its entire range then press any button or wait for the timer to finish.";

	[SerializeField]
	private string _inputBehaviorSettingsWindowTitle = "Sensitivity Settings";

	[SerializeField]
	private string _restoreDefaultsWindowTitle = "Restore Defaults";

	[SerializeField]
	[Tooltip("Message for a single player game.")]
	private string _restoreDefaultsWindowMessage_onePlayer = "This will restore the default input configuration. Are you sure you want to do this?";

	[SerializeField]
	[Tooltip("Message for a multi-player game.")]
	private string _restoreDefaultsWindowMessage_multiPlayer = "This will restore the default input configuration for all players. Are you sure you want to do this?";

	[SerializeField]
	private string _actionColumnLabel = "Actions";

	[SerializeField]
	private string _keyboardColumnLabel = "Keyboard";

	[SerializeField]
	private string _mouseColumnLabel = "Mouse";

	[SerializeField]
	private string _controllerColumnLabel = "Controller";

	[SerializeField]
	private string _removeControllerButtonLabel = "Remove";

	[SerializeField]
	private string _calibrateControllerButtonLabel = "Calibrate";

	[SerializeField]
	private string _assignControllerButtonLabel = "Assign Controller";

	[SerializeField]
	private string _inputBehaviorSettingsButtonLabel = "Sensitivity";

	[SerializeField]
	private string _doneButtonLabel = "Done";

	[SerializeField]
	private string _restoreDefaultsButtonLabel = "Restore Defaults";

	[SerializeField]
	private string _playersGroupLabel = "Players:";

	[SerializeField]
	private string _controllerSettingsGroupLabel = "Controller:";

	[SerializeField]
	private string _assignedControllersGroupLabel = "Assigned Controllers:";

	[SerializeField]
	private string _settingsGroupLabel = "Settings:";

	[SerializeField]
	private string _mapCategoriesGroupLabel = "Categories:";

	[SerializeField]
	private string _calibrateWindow_deadZoneSliderLabel = "Dead Zone:";

	[SerializeField]
	private string _calibrateWindow_zeroSliderLabel = "Zero:";

	[SerializeField]
	private string _calibrateWindow_sensitivitySliderLabel = "Sensitivity:";

	[SerializeField]
	private string _calibrateWindow_invertToggleLabel = "Invert";

	[SerializeField]
	private string _calibrateWindow_calibrateButtonLabel = "Calibrate";

	[SerializeField]
	private CustomEntry[] _customEntries;

	private bool _initialized;

	private Dictionary<string, string> customDict;

	public string yes => Localization.Translate(_yes).text;

	public string no => Localization.Translate(_no).text;

	public string add => Localization.Translate(_add).text;

	public string replace => Localization.Translate(_replace).text;

	public string remove => Localization.Translate(_remove).text;

	public string cancel => Localization.Translate(_cancel).text;

	public string none => Localization.Translate(_none).text;

	public string okay => Localization.Translate(_okay).text;

	public string done => Localization.Translate(_done).text;

	public string default_ => Localization.Translate(_default).text;

	public string assignControllerWindowTitle => Localization.Translate(_assignControllerWindowTitle).text;

	public string assignControllerWindowMessage => Localization.Translate(_assignControllerWindowMessage).text;

	public string controllerAssignmentConflictWindowTitle => Localization.Translate(_controllerAssignmentConflictWindowTitle).text;

	public string elementAssignmentPrePollingWindowMessage => Localization.Translate(_elementAssignmentPrePollingWindowMessage).text;

	public string elementAssignmentConflictWindowMessage => Localization.Translate(_elementAssignmentConflictWindowMessage).text;

	public string mouseAssignmentConflictWindowTitle => Localization.Translate(_mouseAssignmentConflictWindowTitle).text;

	public string calibrateControllerWindowTitle => Localization.Translate(_calibrateControllerWindowTitle).text;

	public string calibrateAxisStep1WindowTitle => Localization.Translate(_calibrateAxisStep1WindowTitle).text;

	public string calibrateAxisStep2WindowTitle => Localization.Translate(_calibrateAxisStep2WindowTitle).text;

	public string inputBehaviorSettingsWindowTitle => Localization.Translate(_inputBehaviorSettingsWindowTitle).text;

	public string restoreDefaultsWindowTitle => Localization.Translate(_restoreDefaultsWindowTitle).text;

	public string actionColumnLabel => Localization.Translate(_actionColumnLabel).text;

	public string keyboardColumnLabel => Localization.Translate(_keyboardColumnLabel).text;

	public string mouseColumnLabel => Localization.Translate(_mouseColumnLabel).text;

	public string controllerColumnLabel => Localization.Translate(_controllerColumnLabel).text;

	public string removeControllerButtonLabel => Localization.Translate(_removeControllerButtonLabel).text;

	public string calibrateControllerButtonLabel => Localization.Translate(_calibrateControllerButtonLabel).text;

	public string assignControllerButtonLabel => Localization.Translate(_assignControllerButtonLabel).text;

	public string inputBehaviorSettingsButtonLabel => Localization.Translate(_inputBehaviorSettingsButtonLabel).text;

	public string doneButtonLabel => Localization.Translate(_doneButtonLabel).text;

	public string restoreDefaultsButtonLabel => Localization.Translate(_restoreDefaultsButtonLabel).text;

	public string controllerSettingsGroupLabel => Localization.Translate(_controllerSettingsGroupLabel).text;

	public string playersGroupLabel => Localization.Translate(_playersGroupLabel).text;

	public string assignedControllersGroupLabel => Localization.Translate(_assignedControllersGroupLabel).text;

	public string settingsGroupLabel => Localization.Translate(_settingsGroupLabel).text;

	public string mapCategoriesGroupLabel => Localization.Translate(_mapCategoriesGroupLabel).text;

	public string restoreDefaultsWindowMessage
	{
		get
		{
			if (ReInput.players.playerCount > 1)
			{
				return Localization.Translate(_restoreDefaultsWindowMessage_multiPlayer).text;
			}
			return Localization.Translate(_restoreDefaultsWindowMessage_onePlayer).text;
		}
	}

	public string calibrateWindow_deadZoneSliderLabel => Localization.Translate(_calibrateWindow_deadZoneSliderLabel).text;

	public string calibrateWindow_zeroSliderLabel => Localization.Translate(_calibrateWindow_zeroSliderLabel).text;

	public string calibrateWindow_sensitivitySliderLabel => Localization.Translate(_calibrateWindow_sensitivitySliderLabel).text;

	public string calibrateWindow_invertToggleLabel => Localization.Translate(_calibrateWindow_invertToggleLabel).text;

	public string calibrateWindow_calibrateButtonLabel => Localization.Translate(_calibrateWindow_calibrateButtonLabel).text;

	public void Initialize()
	{
		if (!_initialized)
		{
			customDict = CustomEntry.ToDictionary(_customEntries);
			_initialized = true;
		}
	}

	public string GetCustomEntry(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return string.Empty;
		}
		if (!customDict.TryGetValue(key, out var value))
		{
			return string.Empty;
		}
		return value;
	}

	public bool ContainsCustomEntryKey(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return false;
		}
		return customDict.ContainsKey(key);
	}

	public string GetControllerAssignmentConflictWindowMessage(string joystickName, string otherPlayerName, string currentPlayerName)
	{
		return string.Format(Localization.Translate(_controllerAssignmentConflictWindowMessage).text, joystickName, otherPlayerName, currentPlayerName);
	}

	public string GetJoystickElementAssignmentPollingWindowMessage(string actionName)
	{
		return string.Format(Localization.Translate(_joystickElementAssignmentPollingWindowMessage).text, actionName);
	}

	public string GetJoystickElementAssignmentPollingWindowMessage_FullAxisFieldOnly(string actionName)
	{
		return string.Format(_joystickElementAssignmentPollingWindowMessage_fullAxisFieldOnly, actionName);
	}

	public string GetKeyboardElementAssignmentPollingWindowMessage(string actionName)
	{
		return string.Format(Localization.Translate(_keyboardElementAssignmentPollingWindowMessage).text, actionName);
	}

	public string GetMouseElementAssignmentPollingWindowMessage(string actionName)
	{
		return string.Format(Localization.Translate(_mouseElementAssignmentPollingWindowMessage).text, actionName);
	}

	public string GetMouseElementAssignmentPollingWindowMessage_FullAxisFieldOnly(string actionName)
	{
		return string.Format(_mouseElementAssignmentPollingWindowMessage_fullAxisFieldOnly, actionName);
	}

	public string GetElementAlreadyInUseBlocked(string elementName)
	{
		return string.Format(Localization.Translate(_elementAlreadyInUseBlocked).text, elementName);
	}

	public string GetElementAlreadyInUseCanReplace(string elementName, bool allowConflicts)
	{
		if (!allowConflicts)
		{
			return string.Format(Localization.Translate(_elementAlreadyInUseCanReplace).text, elementName);
		}
		return string.Format(Localization.Translate(_elementAlreadyInUseCanReplace_conflictAllowed).text, elementName);
	}

	public int GetElementAlreadyInUseCanReplaceFontSize(bool allowConflicts)
	{
		if (!allowConflicts)
		{
			return Localization.Translate(_elementAlreadyInUseCanReplace).fonts.fontSize;
		}
		return Localization.Translate(_elementAlreadyInUseCanReplace_conflictAllowed).fonts.fontSize;
	}

	public string GetMouseAssignmentConflictWindowMessage(string otherPlayerName, string thisPlayerName)
	{
		return string.Format(Localization.Translate(_mouseAssignmentConflictWindowMessage).text, otherPlayerName, thisPlayerName);
	}

	public string GetCalibrateAxisStep1WindowMessage(string axisName)
	{
		return string.Format(Localization.Translate(_calibrateAxisStep1WindowMessage).text, axisName);
	}

	public string GetCalibrateAxisStep2WindowMessage(string axisName)
	{
		return string.Format(Localization.Translate(_calibrateAxisStep2WindowMessage).text, axisName);
	}
}
