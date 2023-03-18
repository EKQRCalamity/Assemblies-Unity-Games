using System.Collections.Generic;
using Framework.Managers;

namespace Gameplay.UI.Console;

public class DebugUICommand : ConsoleCommand
{
	public override bool HasLowerParameters()
	{
		return false;
	}

	public override void Execute(string command, string[] parameters)
	{
		List<string> listParameters;
		string subcommand = GetSubcommand(parameters, out listParameters);
		if (command != null && command == "show_debug_ui")
		{
			ParseShowUI(subcommand, listParameters);
		}
	}

	public override List<string> GetNames()
	{
		List<string> list = new List<string>();
		list.Add("show_debug_ui");
		return list;
	}

	private void ParseShowUI(string command, List<string> paramList)
	{
		string command2 = "show_debug_ui " + command;
		switch (command)
		{
		case "help":
			if (ValidateParams(command2, 0, paramList))
			{
				base.Console.Write("Available show_debug_ui commands:");
				base.Console.Write("show_debug_ui off: Turn off DEBUG UI");
				base.Console.Write("show_debug_ui on: Turn on DEBUG UI");
				base.Console.Write("show_debug_ui current: Show the current option");
			}
			break;
		case "current":
			if (ValidateParams(command2, 0, paramList))
			{
				base.Console.Write("*** Current Debug UI: " + Core.UI.ConsoleShowDebugUI);
			}
			break;
		case "on":
			if (ValidateParams(command2, 0, paramList))
			{
				Core.UI.ConsoleShowDebugUI = true;
			}
			break;
		case "off":
			if (ValidateParams(command2, 0, paramList))
			{
				Core.UI.ConsoleShowDebugUI = false;
			}
			break;
		default:
			base.Console.Write("Command unknow, use show_debug_ui help");
			break;
		}
	}
}
