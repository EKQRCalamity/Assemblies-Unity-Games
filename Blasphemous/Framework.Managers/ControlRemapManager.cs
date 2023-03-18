using System;
using System.Collections.Generic;
using System.IO;
using I2.Loc;
using Rewired;
using Rewired.Data;
using Rewired.Data.Mapping;
using Tools;
using UnityEngine;

namespace Framework.Managers;

public class ControlRemapManager : GameSystem
{
	public delegate void InputMappedDelegate(string newButtonName = "", int newActionElementMapId = -1);

	private const string DEFAULT_CATEGORY = "Default";

	private const string MENU_CATEGORY = "Menu";

	private const string DEFAULT_LAYOUT = "Default";

	private const string OLD_CONTROLS_SETTINGS_PATH = "/controls_settings";

	private const string CONTROLS_SETTINGS_PATH = "/keybinding_settings";

	private readonly Dictionary<string, List<string>> conflictingActions = new Dictionary<string, List<string>>();

	private readonly Dictionary<string, List<string>> exceptionsWithoutConflictActions = new Dictionary<string, List<string>>();

	private readonly InputMapper keyboardAndJoystickInputMapper = new InputMapper();

	private readonly InputMapper mouseInputMapper = new InputMapper();

	private readonly List<int> userUnassignableActionIds = new List<int>();

	private readonly List<string> consoleButtonsNotAllowed = new List<string> { "TOUCH PAD", "LEFT STICK UP", "LEFT STICK DOWN", "LEFT STICK LEFT", "LEFT STICK RIGHT", "PS BUTTON" };

	private int currentActionElementMapId;

	private bool definedConflictingActions;

	private ControllerMap prevControllerMap;

	private bool readControlsSetting;

	private Dictionary<string, string> joystickSettingsCache = new Dictionary<string, string>();

	private const int ListeningCooldown = 30;

	private int timeSinceStoppingListening;

	public bool listeningForInput { get; private set; }

	public bool ListeningForInputDone => !listeningForInput && timeSinceStoppingListening == 0;

	private Player player => ReInput.players.GetPlayer(0);

	private ControllerMap controllerMap
	{
		get
		{
			if (controller == null)
			{
				return null;
			}
			return player.controllers.maps.GetMap(controller.type, controller.id, "Default", "Default");
		}
	}

	private ControllerMap mouseControllerMap
	{
		get
		{
			if (controller == null)
			{
				return null;
			}
			return player.controllers.maps.GetMap(ControllerType.Mouse, player.controllers.Mouse.id, "Default", "Default");
		}
	}

	private Controller controller => Core.Input.ActiveController;

	public static event InputMappedDelegate InputMappedEvent;

	public override void Initialize()
	{
		keyboardAndJoystickInputMapper.options.timeout = 0f;
		mouseInputMapper.options.timeout = 0f;
		keyboardAndJoystickInputMapper.options.ignoreMouseXAxis = true;
		keyboardAndJoystickInputMapper.options.ignoreMouseYAxis = true;
		mouseInputMapper.options.ignoreMouseXAxis = false;
		mouseInputMapper.options.ignoreMouseYAxis = false;
		keyboardAndJoystickInputMapper.options.allowAxes = true;
		mouseInputMapper.options.allowAxes = true;
		keyboardAndJoystickInputMapper.options.allowKeyboardModifierKeyAsPrimary = true;
		keyboardAndJoystickInputMapper.options.allowKeyboardKeysWithModifiers = false;
		keyboardAndJoystickInputMapper.options.isElementAllowedCallback = OnIsElementAllowed;
		mouseInputMapper.options.isElementAllowedCallback = OnIsElementAllowed;
		keyboardAndJoystickInputMapper.InputMappedEvent += OnInputMapped;
		keyboardAndJoystickInputMapper.StoppedEvent += OnStopped;
		keyboardAndJoystickInputMapper.ConflictFoundEvent += OnConflictFound;
		mouseInputMapper.InputMappedEvent += OnInputMapped;
		mouseInputMapper.StoppedEvent += OnStopped;
		mouseInputMapper.ConflictFoundEvent += OnConflictFound;
		Core.Input.JoystickPressed += ActiveInputChanged;
		Core.Input.KeyboardPressed += ActiveInputChanged;
		ReInput.ControllerConnectedEvent += OnControllerConnected;
	}

	public override void Update()
	{
		if (controllerMap != null && controllerMap.AllMaps.Count != 0)
		{
			if (prevControllerMap == null)
			{
				prevControllerMap = controllerMap;
			}
			if (!listeningForInput && timeSinceStoppingListening > 0)
			{
				timeSinceStoppingListening--;
			}
			if (!definedConflictingActions)
			{
				DefineConflictingActions();
				definedConflictingActions = true;
			}
			if (!readControlsSetting)
			{
				DeleteDeprecatedControlsSettingsFiles();
				LoadKeyboardAndMouseControlsSettingsFromFile();
				InitialLoadJoystickControlsSettingsFromFile();
				readControlsSetting = true;
			}
			player.controllers.hasMouse = true;
		}
	}

	private void InitialLoadJoystickControlsSettingsFromFile()
	{
		if (ReInput.players == null || ReInput.players.playerCount == 0)
		{
			return;
		}
		Player player = ReInput.players.GetPlayer(0);
		if (player.controllers == null || player.controllers.joystickCount <= 0)
		{
			return;
		}
		foreach (Joystick joystick in player.controllers.Joysticks)
		{
			LoadJoystickControlsSettingsFromFile(joystick);
		}
	}

	private void DefineConflictingActions()
	{
		InitializeUserUnassignableActions();
		InitializeExceptionsWithoutConflict();
		InitializeConflictingActions();
		foreach (string key in conflictingActions.Keys)
		{
			foreach (string key2 in conflictingActions.Keys)
			{
				if (!key2.Equals(key) && (!exceptionsWithoutConflictActions.ContainsKey(key2) || !exceptionsWithoutConflictActions[key2].Contains(key)))
				{
					conflictingActions[key].Add(key2);
				}
			}
		}
	}

	private void InitializeUserUnassignableActions()
	{
		userUnassignableActionIds.Add(39);
		userUnassignableActionIds.Add(28);
		userUnassignableActionIds.Add(64);
		userUnassignableActionIds.Add(29);
		userUnassignableActionIds.Add(45);
		userUnassignableActionIds.Add(43);
		userUnassignableActionIds.Add(51);
		userUnassignableActionIds.Add(52);
		userUnassignableActionIds.Add(48);
		userUnassignableActionIds.Add(50);
		userUnassignableActionIds.Add(49);
	}

	private void InitializeExceptionsWithoutConflict()
	{
		string name = ReInput.mapping.GetAction(65).name;
		string name2 = ReInput.mapping.GetAction(57).name;
		string name3 = ReInput.mapping.GetAction(7).name;
		exceptionsWithoutConflictActions[name] = new List<string>();
		exceptionsWithoutConflictActions[name].Add(name2);
		exceptionsWithoutConflictActions[name].Add(name3);
		exceptionsWithoutConflictActions[name2] = new List<string>();
		exceptionsWithoutConflictActions[name2].Add(name);
		exceptionsWithoutConflictActions[name3] = new List<string>();
		exceptionsWithoutConflictActions[name3].Add(name);
	}

	private void InitializeConflictingActions()
	{
		List<ActionElementMap> list = new List<ActionElementMap>(mouseControllerMap.AllMaps);
		list.AddRange(controllerMap.AllMaps);
		foreach (ActionElementMap item in list)
		{
			if (!userUnassignableActionIds.Contains(item.actionId))
			{
				string actionNameWithPolarity = GetActionNameWithPolarity(item);
				if (!conflictingActions.ContainsKey(actionNameWithPolarity))
				{
					conflictingActions[actionNameWithPolarity] = new List<string>();
				}
			}
		}
	}

	public List<string> GetAllActionNamesInOrder()
	{
		List<string> list = new List<string>();
		List<int> list2 = new List<int>();
		list2.Add(4);
		list2.Add(0);
		list2.Add(5);
		list2.Add(6);
		list2.Add(8);
		list2.Add(7);
		list2.Add(38);
		list2.Add(57);
		list2.Add(65);
		list2.Add(23);
		list2.Add(25);
		list2.Add(22);
		list2.Add(10);
		list2.Add(20);
		list2.Add(21);
		foreach (int item in list2)
		{
			InputAction action = ReInput.mapping.GetAction(item);
			string actionNameWithPolarity = GetActionNameWithPolarity(action);
			list.Add(actionNameWithPolarity);
			if (actionNameWithPolarity.Contains("Pos"))
			{
				actionNameWithPolarity = actionNameWithPolarity.Replace("Pos", "Neg");
				list.Add(actionNameWithPolarity);
			}
		}
		return list;
	}

	public Dictionary<string, ActionElementMap> GetRebindeableAemsByActionName(List<string> actionNames)
	{
		Dictionary<string, ActionElementMap> dictionary = new Dictionary<string, ActionElementMap>();
		foreach (string actionName in actionNames)
		{
			dictionary[actionName] = null;
		}
		List<ActionElementMap> list = new List<ActionElementMap>();
		list.AddRange(controllerMap.AllMaps);
		if (controllerMap.controllerType == ControllerType.Keyboard)
		{
			IList<ActionElementMap> allMaps = mouseControllerMap.AllMaps;
			foreach (ActionElementMap item in allMaps)
			{
				if (item.actionId != 51 && item.actionId != 52 && item.actionId != 48 && item.actionId != 50 && item.actionId != 49)
				{
					list.Add(item);
				}
			}
		}
		list.Sort((ActionElementMap x, ActionElementMap y) => x.elementIndex.CompareTo(y.elementIndex));
		foreach (ActionElementMap item2 in list)
		{
			if (!userUnassignableActionIds.Contains(item2.actionId))
			{
				string actionNameWithPolarity = GetActionNameWithPolarity(item2);
				dictionary[actionNameWithPolarity] = item2;
			}
		}
		return dictionary;
	}

	public void StartListeningInput(int actionElementMapId)
	{
		ActionElementMap firstElementMapMatch = controllerMap.GetFirstElementMapMatch((ActionElementMap x) => x.id == actionElementMapId);
		if (firstElementMapMatch == null && controllerMap.controllerType == ControllerType.Keyboard && player.controllers.hasMouse)
		{
			firstElementMapMatch = mouseControllerMap.GetFirstElementMapMatch((ActionElementMap x) => x.id == actionElementMapId);
		}
		if (firstElementMapMatch != null)
		{
			if (!listeningForInput)
			{
				listeningForInput = true;
				currentActionElementMapId = firstElementMapMatch.id;
				AxisRange actionRange = ((firstElementMapMatch.axisContribution == Pole.Positive) ? AxisRange.Positive : AxisRange.Negative);
				keyboardAndJoystickInputMapper.Start(new InputMapper.Context
				{
					actionId = firstElementMapMatch.actionId,
					controllerMap = controllerMap,
					actionRange = actionRange,
					actionElementMapToReplace = controllerMap.GetElementMap(firstElementMapMatch.id)
				});
				if (controllerMap.controllerType == ControllerType.Keyboard && player.controllers.hasMouse)
				{
					ControllerMap map = player.controllers.maps.GetMap(ControllerType.Mouse, player.controllers.Mouse.id, "Default", "Default");
					mouseInputMapper.Start(new InputMapper.Context
					{
						actionId = firstElementMapMatch.actionId,
						controllerMap = map,
						actionRange = actionRange,
						actionElementMapToReplace = map.GetElementMap(firstElementMapMatch.id)
					});
				}
			}
		}
		else
		{
			Debug.Log("Found no action element map assigned to action element map id: " + actionElementMapId);
		}
	}

	public void StopListeningInput()
	{
		keyboardAndJoystickInputMapper.Stop();
		mouseInputMapper.Stop();
	}

	public void RestoreDefaultMaps()
	{
		switch (controller.type)
		{
		case ControllerType.Joystick:
			player.controllers.maps.LoadDefaultMaps(ControllerType.Joystick);
			break;
		case ControllerType.Keyboard:
			player.controllers.maps.LoadDefaultMaps(ControllerType.Keyboard);
			player.controllers.maps.LoadDefaultMaps(ControllerType.Mouse);
			break;
		}
	}

	public ActionElementMap FindLastElementMapByInputAction(InputAction inputAction, AxisRange axisRange, Controller controller)
	{
		string actionNameWithPolarity = GetActionNameWithPolarity(inputAction, axisRange);
		ControllerMap map = player.controllers.maps.GetMap(controller.type, controller.id, "Default", "Default");
		return FindLastElementMapByActionName(actionNameWithPolarity, map, mouseControllerMap);
	}

	private ActionElementMap FindLastElementMapByActionName(string actionName, ControllerMap controllerMap, ControllerMap mouseControllerMap)
	{
		ActionElementMap actionElementMap = null;
		Predicate<ActionElementMap> predicate = null;
		predicate = (actionName.Contains("Pos") ? ((Predicate<ActionElementMap>)((ActionElementMap aem) => GetActionNameWithPolarity(aem).Equals(actionName) && aem.axisContribution == Pole.Positive)) : ((!actionName.Contains("Neg")) ? ((Predicate<ActionElementMap>)((ActionElementMap aem) => GetActionNameWithPolarity(aem).Equals(actionName))) : ((Predicate<ActionElementMap>)((ActionElementMap aem) => GetActionNameWithPolarity(aem).Equals(actionName) && aem.axisContribution == Pole.Negative))));
		ControllerMap map = player.controllers.maps.GetMap(controller.type, controller.id, "Menu", "Default");
		if (controllerMap.controllerType == ControllerType.Keyboard && player.controllers.hasMouse)
		{
			actionElementMap = FindLastElementMapMatch(mouseControllerMap, predicate);
		}
		if (actionElementMap == null)
		{
			actionElementMap = ((!actionName.StartsWith("UI")) ? FindLastElementMapMatch(controllerMap, predicate) : FindLastElementMapMatch(map, predicate));
		}
		if (actionElementMap == null && player.controllers.hasMouse)
		{
			Debug.Log("FindFirstElementMapByActionName: actionElementMap not found! actionName: " + actionName);
		}
		return actionElementMap;
	}

	private List<ActionElementMap> FindAllElementMapsByActionName(string actionName, ControllerMap controllerMap, ControllerMap mouseControllerMap)
	{
		List<ActionElementMap> list = new List<ActionElementMap>();
		Predicate<ActionElementMap> predicate = null;
		predicate = (actionName.Contains("Pos") ? ((Predicate<ActionElementMap>)((ActionElementMap aem) => GetActionNameWithPolarity(aem).Equals(actionName) && aem.axisContribution == Pole.Positive)) : ((!actionName.Contains("Neg")) ? ((Predicate<ActionElementMap>)((ActionElementMap aem) => GetActionNameWithPolarity(aem).Equals(actionName))) : ((Predicate<ActionElementMap>)((ActionElementMap aem) => GetActionNameWithPolarity(aem).Equals(actionName) && aem.axisContribution == Pole.Negative))));
		ControllerMap map = player.controllers.maps.GetMap(controller.type, controller.id, "Menu", "Default");
		if (controllerMap.controllerType == ControllerType.Keyboard && player.controllers.hasMouse)
		{
			list = FindAllElementMapMatch(mouseControllerMap, predicate);
		}
		if (list.Count == 0)
		{
			list = ((!actionName.StartsWith("UI")) ? FindAllElementMapMatch(controllerMap, predicate) : FindAllElementMapMatch(map, predicate));
		}
		if (list.Count == 0 && player.controllers.hasMouse)
		{
			Debug.Log("actionElementMap not found! actionName: " + actionName);
		}
		return list;
	}

	public int CountConflictingActions()
	{
		return CountConflictingActions(controllerMap, mouseControllerMap);
	}

	private int CountConflictingActions(ControllerMap controllerMap)
	{
		return CountConflictingActions(controllerMap, mouseControllerMap);
	}

	private int CountConflictingActions(ControllerMap controllerMap, ControllerMap mouseControllerMap)
	{
		if (controllerMap == null)
		{
			Debug.Log("CountConflictingActions: controllerMap is null!");
		}
		else if (controllerMap.controllerType == ControllerType.Keyboard && player.controllers.hasMouse && mouseControllerMap == null)
		{
			Debug.Log("CountConflictingActions: mouseControllerMap is null!");
		}
		int num = 0;
		foreach (string actionName in conflictingActions.Keys)
		{
			List<ActionElementMap> list = FindAllElementMapsByActionName(actionName, controllerMap, mouseControllerMap);
			List<string> currentElementMapsIdentifierNames = new List<string>();
			foreach (ActionElementMap item in list)
			{
				currentElementMapsIdentifierNames.Add(item.elementIdentifierName);
			}
			Predicate<ActionElementMap> predicate = (ActionElementMap aem) => currentElementMapsIdentifierNames.Contains(aem.elementIdentifierName) && conflictingActions[actionName].Contains(GetActionNameWithPolarity(aem));
			List<ActionElementMap> list2 = new List<ActionElementMap>();
			controllerMap.GetElementMapMatches(predicate, list2);
			if (list2.Count > 0)
			{
				Debug.Log("CountConflictingActions: Conflict! actionName: " + actionName);
				num++;
			}
			else if (controllerMap.controllerType == ControllerType.Keyboard && player.controllers.hasMouse)
			{
				mouseControllerMap.GetElementMapMatches(predicate, list2);
				if (list2.Count > 0)
				{
					Debug.Log("CountConflictingActions: Conflict! actionName: " + actionName);
					num++;
				}
			}
		}
		return num;
	}

	public Dictionary<int, string> GetAllCurrentConflictingButtonsByAemId()
	{
		Dictionary<int, string> dictionary = new Dictionary<int, string>();
		foreach (string actionName in conflictingActions.Keys)
		{
			List<ActionElementMap> list = FindAllElementMapsByActionName(actionName, controllerMap, mouseControllerMap);
			List<string> currentElementMapsIdentifierNames = new List<string>();
			foreach (ActionElementMap item in list)
			{
				currentElementMapsIdentifierNames.Add(item.elementIdentifierName);
			}
			Predicate<ActionElementMap> predicate = delegate(ActionElementMap aem)
			{
				if (currentElementMapsIdentifierNames.Contains(aem.elementIdentifierName))
				{
				}
				return currentElementMapsIdentifierNames.Contains(aem.elementIdentifierName) && conflictingActions[actionName].Contains(GetActionNameWithPolarity(aem));
			};
			List<ActionElementMap> list2 = new List<ActionElementMap>();
			controllerMap.GetElementMapMatches(predicate, list2);
			if (list2.Count == 0 && controllerMap.controllerType == ControllerType.Keyboard && player.controllers.hasMouse)
			{
				mouseControllerMap.GetElementMapMatches(predicate, list2);
			}
			if (list2.Count <= 0)
			{
				continue;
			}
			Debug.Log("Conflict! actionName: " + actionName + ", searchResults.Count: " + list2.Count);
			foreach (ActionElementMap item2 in list)
			{
				dictionary[item2.id] = item2.elementIdentifierName;
			}
		}
		return dictionary;
	}

	private void ActiveInputChanged()
	{
		if (controllerMap == null || prevControllerMap == null)
		{
			return;
		}
		int num = CountConflictingActions(prevControllerMap);
		if (prevControllerMap.controllerType == ControllerType.Keyboard)
		{
			if (num > 0)
			{
				player.controllers.maps.LoadDefaultMaps(ControllerType.Keyboard);
				player.controllers.maps.LoadDefaultMaps(ControllerType.Mouse);
			}
			Controller mouse = player.controllers.Mouse;
			player.controllers.RemoveController(mouse);
		}
		else if (prevControllerMap.controllerType != 0)
		{
			if (num > 0)
			{
				player.controllers.maps.LoadDefaultMaps(ControllerType.Joystick);
			}
			if (!player.controllers.hasMouse)
			{
				Controller mouse2 = player.controllers.Mouse;
				player.controllers.AddController(mouse2, removeFromOtherPlayers: false);
			}
		}
		prevControllerMap = controllerMap;
	}

	private string GetActionNameWithPolarity(InputAction inputAction)
	{
		string result = string.Empty;
		if (inputAction != null)
		{
			result = ((inputAction.type != InputActionType.Button) ? (inputAction.name + " Pos") : inputAction.name);
		}
		else
		{
			Debug.LogError("GetActionNameWithPolarity: inputAction is null!");
		}
		return result;
	}

	private string GetActionNameWithPolarity(InputAction inputAction, AxisRange axisRange)
	{
		string result = string.Empty;
		if (inputAction != null)
		{
			result = ((inputAction.type == InputActionType.Button) ? inputAction.name : ((axisRange != AxisRange.Negative) ? (inputAction.name + " Pos") : (inputAction.name + " Neg")));
		}
		else
		{
			Debug.LogError("GetActionNameWithPolarity: inputAction is null!");
		}
		return result;
	}

	private string GetActionNameWithPolarity(ActionElementMap actionElementMap)
	{
		string result = string.Empty;
		if (actionElementMap == null)
		{
			Debug.LogError("GetActionNameWithPolarity: actionElementMap is null!");
		}
		else
		{
			InputAction action = ReInput.mapping.GetAction(actionElementMap.actionId);
			result = ((action.type == InputActionType.Button) ? action.name : ((actionElementMap.axisContribution != 0) ? (action.name + " Neg") : (action.name + " Pos")));
		}
		return result;
	}

	public string LocalizeActionName(string actionName)
	{
		string empty = string.Empty;
		string text = "UI_Controls/" + actionName.Replace(" ", "_").ToUpperInvariant();
		string text2 = ScriptLocalization.Get(text);
		if (string.IsNullOrEmpty(text2))
		{
			empty = text;
			Debug.LogError("Action Name: '" + text + "' has no localization term!");
		}
		else
		{
			empty = text2;
		}
		return empty;
	}

	private ActionElementMap FindLastElementMapMatch(ControllerMap controllerMap, Predicate<ActionElementMap> predicate)
	{
		ActionElementMap result = null;
		List<ActionElementMap> list = new List<ActionElementMap>();
		controllerMap.GetElementMapMatches(predicate, list);
		if (list.Count > 0)
		{
			result = list[list.Count - 1];
		}
		return result;
	}

	private List<ActionElementMap> FindAllElementMapMatch(ControllerMap controllerMap, Predicate<ActionElementMap> predicate)
	{
		List<ActionElementMap> list = new List<ActionElementMap>();
		controllerMap.GetElementMapMatches(predicate, list);
		return list;
	}

	private void OnInputMapped(InputMapper.InputMappedEventData data)
	{
		ActionElementMap actionElementMap = data.actionElementMap;
		Debug.Log("Button " + actionElementMap.elementIdentifierName + " is now assigned to the Action " + ReInput.mapping.GetAction(actionElementMap.actionId).name);
		Debug.Log("It has been assigned with pole: " + actionElementMap.axisContribution);
		if (ControlRemapManager.InputMappedEvent != null)
		{
			ControlRemapManager.InputMappedEvent(actionElementMap.elementIdentifierName, actionElementMap.id);
			ControlRemapManager.InputMappedEvent = null;
		}
	}

	private void OnStopped(InputMapper.StoppedEventData data)
	{
		if (data.inputMapper.Equals(keyboardAndJoystickInputMapper))
		{
			if (mouseInputMapper.status == InputMapper.Status.Listening)
			{
				mouseInputMapper.Stop();
			}
		}
		else
		{
			keyboardAndJoystickInputMapper.Stop();
		}
		currentActionElementMapId = -1;
		ControlRemapManager.InputMappedEvent = null;
		listeningForInput = false;
		timeSinceStoppingListening = 30;
	}

	private void OnConflictFound(InputMapper.ConflictFoundEventData data)
	{
		Debug.Log("OnConflictFound: data.assignment.action.name: " + data.assignment.action.name);
		Debug.Log("OnConflictFound: data.conflicts[0].action.name: " + data.conflicts[0].action.name);
		if (data.isProtected)
		{
			data.responseCallback(InputMapper.ConflictResponse.Cancel);
		}
		else
		{
			data.responseCallback(InputMapper.ConflictResponse.Add);
		}
	}

	private bool OnIsElementAllowed(ControllerPollingInfo info)
	{
		bool flag = true;
		if (info.controllerType == ControllerType.Mouse)
		{
			string text = info.elementIdentifierName.ToUpper();
		}
		else if (info.controllerType == ControllerType.Keyboard)
		{
			if (info.keyboardKey == KeyCode.Escape || info.keyboardKey == KeyCode.KeypadEnter || info.keyboardKey == KeyCode.Return)
			{
				flag = false;
			}
		}
		else
		{
			string item = info.elementIdentifierName.ToUpper();
			if (consoleButtonsNotAllowed.Contains(item))
			{
				flag = false;
			}
		}
		if (flag)
		{
			if (controllerMap.GetFirstElementMapMatch((ActionElementMap aem) => aem.id == currentActionElementMapId) != null)
			{
				controllerMap.DeleteElementMap(currentActionElementMapId);
			}
			else
			{
				mouseControllerMap.DeleteElementMap(currentActionElementMapId);
			}
		}
		return flag;
	}

	private string GetPathControlsSettings()
	{
		return PersistentManager.GetPathAppSettings("/keybinding_settings");
	}

	private string GetPathOldControlsSettings()
	{
		return PersistentManager.GetPathAppSettings("/controls_settings");
	}

	private void DeleteDeprecatedControlsSettingsFiles()
	{
		Rewired.InputManager inputManager = UnityEngine.Object.FindObjectOfType<Rewired.InputManager>();
		ControllerDataFiles controllerDataFiles = null;
		if (inputManager != null)
		{
			controllerDataFiles = inputManager.dataFiles;
		}
		List<ControllerMap> list = new List<ControllerMap>(player.controllers.maps.GetAllMapsInCategory(0));
		foreach (ControllerMap item in list)
		{
			string empty = string.Empty;
			if (item.controllerType == ControllerType.Joystick)
			{
				if (controllerDataFiles == null)
				{
					continue;
				}
				HardwareJoystickMap hardwareJoystickMap = controllerDataFiles.GetHardwareJoystickMap(item.hardwareGuid);
				if (hardwareJoystickMap == null)
				{
					continue;
				}
				empty = GetPathOldControlsSettings() + "_" + hardwareJoystickMap.ControllerName.ToLowerInvariant().Replace(' ', '_') + ".xml";
			}
			else
			{
				empty = GetPathOldControlsSettings() + "_" + item.controllerType.ToString().ToLowerInvariant() + ".xml";
			}
			if (File.Exists(empty))
			{
				File.Delete(empty);
			}
		}
	}

	public void LoadKeyboardAndMouseControlsSettingsFromFile()
	{
		List<ControllerMap> list = new List<ControllerMap>(player.controllers.maps.GetAllMapsInCategory(0));
		foreach (ControllerMap item in list)
		{
			if (item.controllerType == ControllerType.Joystick)
			{
				continue;
			}
			string text = GetPathControlsSettings() + "_" + item.controllerType.ToString().ToLowerInvariant() + ".xml";
			if (File.Exists(text))
			{
				string text2 = File.ReadAllText(text);
				if (string.IsNullOrEmpty(text2))
				{
					Debug.LogError("LoadKeyboardAndMouseControlsSettingsFromFile: there is no data inside the controls settings file with path: " + text);
					continue;
				}
				ControllerMap map = ControllerMap.CreateFromXml(item.controllerType, text2);
				switch (item.controllerType)
				{
				case ControllerType.Keyboard:
					player.controllers.maps.AddMap<KeyboardMap>(item.controllerId, map);
					break;
				case ControllerType.Mouse:
					player.controllers.maps.AddMap<MouseMap>(item.id, map);
					break;
				}
			}
			else
			{
				Debug.Log("LoadKeyboardAndMouseControlsSettingsFromFile: file not found, loading deafult maps");
				player.controllers.maps.LoadDefaultMaps(item.controllerType);
			}
		}
		bool flag = FixOldActions(ControllerType.Keyboard, 64);
		if (flag | FixOldActions(ControllerType.Keyboard, 65))
		{
			Core.ControlRemapManager.WriteControlsSettingsToFile();
		}
	}

	public void OnControllerConnected(ControllerStatusChangedEventArgs args)
	{
		if (args.controllerType == ControllerType.Joystick)
		{
			Joystick joystick = (Joystick)player.controllers.GetController(ControllerType.Joystick, args.controllerId);
			LoadJoystickControlsSettingsFromFile(joystick);
		}
	}

	public void LoadJoystickControlsSettingsFromFile(Joystick joystick)
	{
		string text = joystick.name.ToLowerInvariant().Replace(' ', '_');
		if (joystickSettingsCache.ContainsKey(text))
		{
			ControllerMap map = ControllerMap.CreateFromXml(ControllerType.Joystick, joystickSettingsCache[text]);
			player.controllers.maps.AddMap<JoystickMap>(joystick.id, map);
		}
		else
		{
			string text2 = GetPathControlsSettings() + "_" + text + ".xml";
			if (File.Exists(text2))
			{
				Debug.Log("Loading mapping definitions for :" + text);
				string text3 = File.ReadAllText(text2);
				if (string.IsNullOrEmpty(text3))
				{
					Debug.LogError("LoadJoystickControlsSettingsFromFile: there is no data inside the controls settings file with path: " + text2);
				}
				else
				{
					ControllerMap map2 = ControllerMap.CreateFromXml(ControllerType.Joystick, text3);
					player.controllers.maps.AddMap<JoystickMap>(joystick.id, map2);
					joystickSettingsCache.Add(text, text3);
				}
			}
			else
			{
				Debug.Log("LoadJoystickControlsSettingsFromFile: file not found, loading deafult maps. path: " + text2);
				player.controllers.maps.LoadDefaultMaps(ControllerType.Joystick);
			}
		}
		bool flag = FixOldActions(ControllerType.Joystick, 64);
		if (flag | FixOldActions(ControllerType.Joystick, 65))
		{
			Core.ControlRemapManager.WriteControlsSettingsToFile();
		}
	}

	private bool FixOldActions(ControllerType mapType, int requiredAction)
	{
		bool flag = false;
		foreach (ControllerMap allMap in player.controllers.maps.GetAllMaps(mapType))
		{
			if (allMap.ContainsAction(requiredAction))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			Debug.LogWarningFormat("Missing action: {0}, restoring default controls for {1}", requiredAction, mapType);
			player.controllers.maps.LoadDefaultMaps(mapType);
		}
		return !flag;
	}

	public void WriteControlsSettingsToFile()
	{
		Rewired.InputManager inputManager = UnityEngine.Object.FindObjectOfType<Rewired.InputManager>();
		ControllerDataFiles controllerDataFiles = null;
		if (inputManager != null)
		{
			controllerDataFiles = inputManager.dataFiles;
		}
		foreach (ControllerMap allMap in player.controllers.maps.GetAllMaps())
		{
			if (allMap.categoryId == 4)
			{
				continue;
			}
			switch (allMap.controllerType)
			{
			case ControllerType.Joystick:
			{
				if (controllerDataFiles == null)
				{
					break;
				}
				HardwareJoystickMap hardwareJoystickMap = controllerDataFiles.GetHardwareJoystickMap(allMap.hardwareGuid);
				if (!(hardwareJoystickMap == null))
				{
					string text = player.controllers.GetController<Joystick>(allMap.controllerId).name.ToLowerInvariant().Replace(' ', '_');
					string path2 = GetPathControlsSettings() + "_" + text + ".xml";
					if (!File.Exists(path2))
					{
						File.CreateText(path2).Close();
					}
					string text2 = allMap.ToXmlString();
					FileTools.SaveSecure(path2, text2);
					joystickSettingsCache[text] = text2;
				}
				break;
			}
			case ControllerType.Keyboard:
			case ControllerType.Mouse:
			case ControllerType.Custom:
			{
				string path = GetPathControlsSettings() + "_" + allMap.controllerType.ToString().ToLowerInvariant() + ".xml";
				if (!File.Exists(path))
				{
					File.CreateText(path).Close();
				}
				string encryptedData = allMap.ToXmlString();
				FileTools.SaveSecure(path, encryptedData);
				break;
			}
			}
		}
	}
}
