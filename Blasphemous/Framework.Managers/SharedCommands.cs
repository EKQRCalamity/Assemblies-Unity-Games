using System.Collections.Generic;
using System.Linq;
using Gameplay.UI.Widgets;
using Tools.DataContainer;
using UnityEngine;

namespace Framework.Managers;

public class SharedCommands : GameSystem
{
	private const string COMMAND_DIRECTORY = "SharedCommands/";

	private Dictionary<string, SharedCommand> Commands = new Dictionary<string, SharedCommand>();

	public override void Initialize()
	{
		LoadAllCommands();
	}

	public void RefreshCommands()
	{
		LoadAllCommands();
	}

	public List<SharedCommand> GetAllCommands()
	{
		return Commands.Values.ToList();
	}

	public bool ExecuteCommand(string Id)
	{
		SharedCommand commandFromName = GetCommandFromName(Id);
		if ((bool)commandFromName)
		{
			List<string> list = new List<string>(commandFromName.commands.Split('\n'));
			foreach (string item in list)
			{
				string text = item.Replace("\r", string.Empty);
				if (!text.StartsWith("//") && text.Length > 0)
				{
					ConsoleWidget.Instance.ProcessCommand(text);
				}
			}
		}
		return commandFromName != null;
	}

	private void LoadAllCommands()
	{
		Commands.Clear();
		SharedCommand[] array = Resources.LoadAll<SharedCommand>("SharedCommands/");
		SharedCommand[] array2 = array;
		foreach (SharedCommand sharedCommand in array2)
		{
			sharedCommand.Id = sharedCommand.name.ToUpper();
			Commands[sharedCommand.Id] = sharedCommand;
		}
	}

	private SharedCommand GetCommandFromName(string id)
	{
		SharedCommand sharedCommand = null;
		string idUpper = id.ToUpper();
		if (Commands.ContainsKey(idUpper))
		{
			sharedCommand = Commands[idUpper];
		}
		if (!sharedCommand)
		{
			sharedCommand = (from e in Commands
				where e.Key.StartsWith(idUpper)
				select e into kv
				select kv.Value).FirstOrDefault();
		}
		if (!sharedCommand)
		{
			sharedCommand = (from e in Commands
				where e.Key.Contains(idUpper)
				select e into kv
				select kv.Value).FirstOrDefault();
		}
		return sharedCommand;
	}
}
