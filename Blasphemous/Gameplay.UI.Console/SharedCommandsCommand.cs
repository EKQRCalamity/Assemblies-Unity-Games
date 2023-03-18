using System.Collections.Generic;
using Framework.Managers;
using Tools.DataContainer;

namespace Gameplay.UI.Console;

public class SharedCommandsCommand : ConsoleCommand
{
	public override bool HasLowerParameters()
	{
		return false;
	}

	public bool ExecuteIfIsCommand(string command)
	{
		return Core.SharedCommands.ExecuteCommand(command);
	}

	public override void Execute(string command, string[] parameters)
	{
		List<string> listParameters;
		string subcommand = GetSubcommand(parameters, out listParameters);
		if (command != null && command == "command")
		{
			ParseCommand(subcommand, listParameters);
		}
	}

	public override List<string> GetNames()
	{
		List<string> list = new List<string>();
		list.Add("command");
		return list;
	}

	private void ParseCommand(string command, List<string> paramList)
	{
		string command2 = "command " + command;
		switch (command)
		{
		case "help":
			if (ValidateParams(command2, 0, paramList))
			{
				base.Console.Write("Available COMMAND subcommands:");
				base.Console.Write("command help: This help");
				base.Console.Write("command list: Show all commands");
				base.Console.Write("command refresh: Reload all commands");
				base.Console.Write("command IDCOMMAND: Execute IDCOMMAND command");
			}
			break;
		case "list":
			if (!ValidateParams(command2, 0, paramList))
			{
				break;
			}
			base.Console.Write("All shared commands");
			{
				foreach (SharedCommand allCommand in Core.SharedCommands.GetAllCommands())
				{
					base.Console.Write(allCommand.Id + " :" + allCommand.Description);
				}
				break;
			}
		case "refresh":
			if (ValidateParams(command2, 0, paramList))
			{
				Core.SharedCommands.RefreshCommands();
				base.Console.Write("Commands refreshed");
			}
			break;
		default:
			WriteCommandResult("Executing " + command, Core.SharedCommands.ExecuteCommand(command));
			break;
		}
	}
}
