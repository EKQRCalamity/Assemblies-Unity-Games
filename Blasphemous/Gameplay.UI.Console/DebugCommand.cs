using System.Collections.Generic;
using Framework.Managers;
using Framework.Util;

namespace Gameplay.UI.Console;

public class DebugCommand : ConsoleCommand
{
	public override void Execute(string command, string[] parameters)
	{
		List<string> listParameters;
		string subcommand = GetSubcommand(parameters, out listParameters);
		if (command != null && command == "debug")
		{
			ParseDebug(subcommand, listParameters);
		}
	}

	public override List<string> GetNames()
	{
		List<string> list = new List<string>();
		list.Add("debug");
		return list;
	}

	private void ParseDebug(string command, List<string> paramList)
	{
		string command2 = "debug " + command;
		switch (command)
		{
		case "help":
			if (ValidateParams(command2, 0, paramList))
			{
				base.Console.Write("Available DEBUG commands:");
				base.Console.Write("debug list: List all debug systems");
				base.Console.Write("debug on SYSTEM: Turn on debug for this system");
				base.Console.Write("debug off SYSTEM: Turn off debug for this system");
			}
			break;
		case "list":
			if (!ValidateParams(command2, 0, paramList))
			{
				break;
			}
			base.Console.Write("All systems ID:");
			{
				foreach (string item in Singleton<Core>.Instance.GetSystemsId())
				{
					base.Console.Write(item);
				}
				break;
			}
		case "on":
			if (ValidateParams(command2, 1, paramList))
			{
				Singleton<Core>.Instance.SetDebug(paramList[0], value: true);
				base.Console.Write("Setting on " + paramList[0]);
			}
			break;
		case "off":
			if (ValidateParams(command2, 1, paramList))
			{
				Singleton<Core>.Instance.SetDebug(paramList[0], value: false);
				base.Console.Write("Setting off " + paramList[0]);
			}
			break;
		default:
			base.Console.Write("Command unknow, use debug help");
			break;
		}
	}
}
