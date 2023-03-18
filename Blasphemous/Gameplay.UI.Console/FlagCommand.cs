using System.Collections.Generic;
using Framework.Managers;

namespace Gameplay.UI.Console;

public class FlagCommand : ConsoleCommand
{
	public override bool HasLowerParameters()
	{
		return false;
	}

	public override void Execute(string command, string[] parameters)
	{
		List<string> listParameters;
		string subcommand = GetSubcommand(parameters, out listParameters);
		if (command != null && command == "flag")
		{
			ParseFlag(subcommand, listParameters);
		}
	}

	public override List<string> GetNames()
	{
		List<string> list = new List<string>();
		list.Add("flag");
		return list;
	}

	private void ParseFlag(string command, List<string> paramList)
	{
		string command2 = "dialog " + command;
		switch (command)
		{
		case "help":
			if (ValidateParams(command2, 0, paramList))
			{
				base.Console.Write("Available FLAG commands:");
				base.Console.Write("flag set IDFLAG: Set flag to true");
				base.Console.Write("flag clear IDFLAG: Set flag to false");
				base.Console.Write("flag test IDFLAG: Outputs the current value of flag");
			}
			break;
		case "set":
			if (ValidateParams(command2, 1, paramList))
			{
				Core.Events.SetFlag(paramList[0].Trim(), b: true);
				base.Console.Write("Flag: " + paramList[0].Trim() + " set to true.");
			}
			break;
		case "clear":
			if (ValidateParams(command2, 1, paramList))
			{
				Core.Events.SetFlag(paramList[0].Trim(), b: false);
				base.Console.Write("Flag: " + paramList[0].Trim() + " set to false.");
			}
			break;
		case "test":
			if (ValidateParams(command2, 1, paramList))
			{
				if (Core.Events.GetFlag(paramList[0].Trim()))
				{
					base.Console.Write("Flag: " + paramList[0].Trim() + " is: true.");
				}
				else
				{
					base.Console.Write("Flag: " + paramList[0].Trim() + " is: false.");
				}
			}
			break;
		default:
			base.Console.Write("Command unknow, use flag help");
			break;
		}
	}
}
