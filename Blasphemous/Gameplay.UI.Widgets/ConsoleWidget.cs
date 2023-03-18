using System.Collections.Generic;
using System.Linq;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using Gameplay.UI.Console;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gameplay.UI.Widgets;

public class ConsoleWidget : UIWidget
{
	public const int MAX_LINES = 200;

	public const int LINES_SHOW = 17;

	public InputField input;

	public RectTransform content;

	public ScrollRect scrollRect;

	public GameObject elements;

	public int scrollSize = 4;

	public bool forceScrollPosition;

	[Range(0f, 1f)]
	public float forceScrollPositionValue;

	private static List<string> previousCommands = new List<string>();

	private static int currentInputCommand = -1;

	public static ConsoleWidget Instance;

	private List<ConsoleCommand> commands = new List<ConsoleCommand>();

	private bool isEnabled;

	private float scrollZero = 189f;

	private float elementSize = 22.115f;

	private bool mirrorToDebugLog;

	private SharedCommandsCommand SharedCommand;

	private bool scrollToBottom;

	public int LineAmount => content.childCount;

	private void Awake()
	{
		SpawnManager.OnPlayerSpawn += OnPlayerSpawn;
	}

	private void OnDestroy()
	{
		SpawnManager.OnPlayerSpawn -= OnPlayerSpawn;
	}

	private void OnPlayerSpawn(Penitent penitent)
	{
		InitializeCommands();
		for (int i = 0; i < commands.Count; i++)
		{
			commands[i].Initialize(this);
			commands[i].Start();
		}
		isEnabled = false;
		elements.SetActive(isEnabled);
		Instance = this;
	}

	public bool IsEnabled()
	{
		return isEnabled;
	}

	private void OnEnable()
	{
		EventSystem.current.SetSelectedGameObject(input.gameObject, null);
		input.OnPointerClick(new PointerEventData(EventSystem.current));
		input.ActivateInputField();
	}

	private void OnDisable()
	{
		if (EventSystem.current != null)
		{
			EventSystem.current.SetSelectedGameObject(null);
		}
		EraseInput();
		ConsoleIndexToStart();
	}

	private void Update()
	{
		for (int i = 0; i < commands.Count; i++)
		{
			commands[i].Update();
		}
		if (Input.GetKeyDown(KeyCode.F1) && Debug.isDebugBuild)
		{
			SetEnabled(!isEnabled);
		}
		if (isEnabled)
		{
			if (scrollToBottom)
			{
				EnsureVisible();
			}
			CheckConsoleKeys();
		}
		if (base.enabled && forceScrollPosition)
		{
			scrollRect.verticalNormalizedPosition = forceScrollPositionValue;
		}
	}

	public List<string> GetAllNames()
	{
		List<string> list = new List<string>();
		foreach (ConsoleCommand command in commands)
		{
			foreach (string name in command.GetNames())
			{
				list.Add(name);
			}
		}
		list.Sort();
		return list;
	}

	public void SetEnabled(bool enabled)
	{
		isEnabled = enabled;
		elements.SetActive(isEnabled);
		Core.Input.SetBlocker("CONSOLE", isEnabled);
		if (enabled)
		{
			OnEnable();
		}
		else
		{
			OnDisable();
		}
	}

	public void ProcessCommand(string rawText)
	{
		string text = rawText.Replace("\r", string.Empty);
		if (text.Trim().Length == 0)
		{
			return;
		}
		string[] array = text.ToLower().Split(' ');
		string[] source = text.Split(' ');
		string text2 = array[0];
		string[] array2 = array.Skip(1).ToArray();
		string[] parameters = source.Skip(1).ToArray();
		ConsoleCommand result = null;
		string commandFromName = GetCommandFromName(text2, out result);
		if (result != null)
		{
			if (result.HasLowerParameters())
			{
				string[] parameters2 = array2.Select((string s) => s.ToLowerInvariant()).ToArray();
				result.Execute(commandFromName, parameters2);
			}
			else if (result.ToLowerAll())
			{
				result.Execute(commandFromName, array2);
			}
			else
			{
				result.Execute(commandFromName, parameters);
			}
		}
		else if (!SharedCommand.ExecuteIfIsCommand(text2))
		{
			Write("Command not found. Use Help for more information.");
		}
	}

	public void Submit()
	{
		if (!input.text.IsNullOrWhitespace())
		{
			previousCommands.Add(input.text);
			ConsoleIndexToStart();
			Write("> " + input.text);
			string text = input.text;
			if (!ProcessInternalCommand(text))
			{
				ProcessCommand(text);
			}
			EraseInput();
			input.ActivateInputField();
			scrollToBottom = true;
		}
	}

	public void Write(string message)
	{
		if (LineAmount > 200)
		{
			ClearLines(1);
		}
		GameObject gameObject = new GameObject();
		Text text = gameObject.AddComponent<Text>();
		text.fontSize = 22;
		text.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
		text.text = message;
		gameObject.transform.SetParent(content);
		gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
		if (mirrorToDebugLog)
		{
			Debug.Log($"[CONSOLE] - {message}");
		}
	}

	public void WriteFormat(string str, params object[] format)
	{
		Write(string.Format(str, format));
	}

	public void EraseInput()
	{
		input.text = string.Empty;
	}

	public void ClearLines(int amount = -1)
	{
		float num = ((amount <= 0 || amount >= content.childCount) ? content.childCount : amount);
		for (int i = 0; (float)i < num; i++)
		{
			Object.Destroy(content.GetChild(i).gameObject);
		}
	}

	private void InitializeCommands()
	{
		commands.Clear();
		commands.Add(new Invincible());
		commands.Add(new FervourRefill());
		commands.Add(new Kill());
		commands.Add(new Help());
		commands.Add(new Restart());
		commands.Add(new LoadLevel());
		commands.Add(new LanguageCommand());
		commands.Add(new InventoryCommand());
		commands.Add(new StatsCommand());
		commands.Add(new BonusCommand());
		commands.Add(new SendEvent());
		commands.Add(new MaxFervour());
		commands.Add(new DialogCommand());
		commands.Add(new ExitCommand());
		commands.Add(new Graybox());
		commands.Add(new SaveGameCommand());
		commands.Add(new SkillCommand());
		commands.Add(new TimescaleCommand());
		commands.Add(new TeleportCommand());
		commands.Add(new ShowUICommand());
		commands.Add(new ExecutionCommand());
		commands.Add(new DebugCommand());
		commands.Add(new AudioCommand());
		commands.Add(new FlagCommand());
		commands.Add(new GuiltCommand());
		commands.Add(new TestPlanCommand());
		commands.Add(new AchievementCommand());
		commands.Add(new SkinCommand());
		commands.Add(new DebugUICommand());
		commands.Add(new MapCommand());
		commands.Add(new ShowCursor());
		commands.Add(new GameModeCommand());
		commands.Add(new PenitenceCommand());
		SharedCommand = new SharedCommandsCommand();
		commands.Add(SharedCommand);
		commands.Add(new AlmsCommand());
		commands.Add(new TutorialsCommand());
		commands.Add(new BossRushCommand());
		commands.Add(new MiriamCommand());
		commands.Add(new DemakeCommand());
		commands.Add(new CameraCommand());
		commands.Add(new CompletionCommand());
	}

	private void CheckConsoleKeys()
	{
		string text = string.Empty;
		if (Input.GetKeyDown(KeyCode.Return))
		{
			Submit();
		}
		if (Input.GetKeyUp(KeyCode.DownArrow))
		{
			text = NextCommand();
		}
		if (Input.GetKeyUp(KeyCode.UpArrow))
		{
			text = PreviousCommand();
		}
		if (!text.IsNullOrWhitespace())
		{
			input.text = text;
		}
		if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow))
		{
			CursorToEnd();
		}
		if (Input.GetKeyUp(KeyCode.PageDown))
		{
			Scroll(-0.5f);
		}
		if (Input.GetKeyUp(KeyCode.PageUp))
		{
			Scroll(0.5f);
		}
	}

	private string PreviousCommand()
	{
		if (previousCommands.Count == 0 || currentInputCommand == 0)
		{
			return string.Empty;
		}
		currentInputCommand--;
		string storedCommand = GetStoredCommand();
		Log.Trace("Console", "Current: " + currentInputCommand + " Count: " + previousCommands.Count);
		return storedCommand;
	}

	private string NextCommand()
	{
		if (currentInputCommand < previousCommands.Count - 1)
		{
			currentInputCommand++;
		}
		if (previousCommands.Count == 0 || currentInputCommand >= previousCommands.Count)
		{
			return string.Empty;
		}
		Log.Trace("Console", "Current: " + currentInputCommand + " Count: " + previousCommands.Count);
		return GetStoredCommand();
	}

	private void ConsoleIndexToStart()
	{
		currentInputCommand = previousCommands.Count;
	}

	private void CursorToEnd()
	{
		input.caretPosition = input.text.Length;
	}

	private string GetStoredCommand()
	{
		return previousCommands[currentInputCommand];
	}

	private void ResetPreviousCommands()
	{
		previousCommands.Clear();
		currentInputCommand = -1;
	}

	private void KeepFocus()
	{
		if (isEnabled && !input.isFocused)
		{
			EventSystem.current.SetSelectedGameObject(input.gameObject, null);
			input.OnPointerClick(new PointerEventData(EventSystem.current));
		}
	}

	private void Scroll(float amount)
	{
		float height = scrollRect.viewport.rect.height;
		float height2 = scrollRect.content.rect.height;
		float num = height / height2;
		scrollRect.verticalNormalizedPosition += amount * num;
	}

	private bool ProcessInternalCommand(string cmd)
	{
		bool result = true;
		switch (cmd.ToUpper())
		{
		case "CLEAR":
		case "CLS":
			ClearLines();
			scrollRect.verticalNormalizedPosition = 1f;
			break;
		case "MIRRORLOG":
			mirrorToDebugLog = !mirrorToDebugLog;
			WriteFormat("Mirror to Unity log: {0}", mirrorToDebugLog);
			break;
		default:
			result = false;
			break;
		}
		return result;
	}

	private void EnsureVisible()
	{
		scrollRect.verticalNormalizedPosition = 0f;
		scrollToBottom = false;
	}

	private string GetCommandFromName(string id, out ConsoleCommand result)
	{
		string idUpper = id.ToUpper();
		result = null;
		string text = string.Empty;
		foreach (ConsoleCommand command in commands)
		{
			text = command.GetNames().Find((string name) => name.ToUpper() == idUpper);
			if (text != null && text != string.Empty)
			{
				result = command;
				break;
			}
		}
		if (result == null)
		{
			foreach (ConsoleCommand command2 in commands)
			{
				text = command2.GetNames().Find((string name) => name.ToUpper().StartsWith(idUpper));
				if (text != null && text != string.Empty)
				{
					result = command2;
					return text;
				}
			}
			return text;
		}
		return text;
	}
}
