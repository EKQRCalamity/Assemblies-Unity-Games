using System.Collections.Generic;
using Framework.Managers;

namespace Gameplay.UI.Console;

public class ShowUICommand : ConsoleCommand
{
	public override bool HasLowerParameters()
	{
		return false;
	}

	public override void Execute(string command, string[] parameters)
	{
		List<string> listParameters;
		string subcommand = GetSubcommand(parameters, out listParameters);
		if (command != null && command == "showui")
		{
			ParseShowUI(subcommand, listParameters);
		}
	}

	public override List<string> GetNames()
	{
		List<string> list = new List<string>();
		list.Add("showui");
		return list;
	}

	private void ParseShowUI(string command, List<string> paramList)
	{
		string command2 = "showui " + command;
		switch (command)
		{
		case "help":
			if (ValidateParams(command2, 0, paramList))
			{
				base.Console.Write("Available SHOWUI commands:");
				base.Console.Write("showui off: Turn off UI");
				base.Console.Write("showui on: Turn on UI");
				base.Console.Write("showui current: Show the current option");
			}
			break;
		case "current":
			if (ValidateParams(command2, 0, paramList))
			{
				base.Console.Write("*** Current ShowUI: " + Core.UI.ShowGamePlayUIForDebug);
			}
			break;
		case "on":
			if (ValidateParams(command2, 0, paramList))
			{
				Core.UI.ShowGamePlayUIForDebug = true;
			}
			break;
		case "off":
			if (ValidateParams(command2, 0, paramList))
			{
				Core.UI.ShowGamePlayUIForDebug = false;
			}
			break;
		default:
			base.Console.Write("Command unknow, use showui help");
			break;
		}
	}
}
