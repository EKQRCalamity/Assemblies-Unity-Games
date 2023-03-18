using System.Collections.Generic;
using Framework.Managers;

namespace Gameplay.UI.Console;

public class GuiltCommand : ConsoleCommand
{
	public override bool HasLowerParameters()
	{
		return false;
	}

	public override void Execute(string command, string[] parameters)
	{
		List<string> listParameters;
		string subcommand = GetSubcommand(parameters, out listParameters);
		if (command != null && command == "guilt")
		{
			ParseGuilt(subcommand, listParameters);
		}
	}

	public override List<string> GetNames()
	{
		List<string> list = new List<string>();
		list.Add("guilt");
		return list;
	}

	private void ParseGuilt(string command, List<string> paramList)
	{
		string command2 = "guilt " + command;
		switch (command)
		{
		case "help":
			if (ValidateParams(command2, 0, paramList))
			{
				base.Console.Write("Available GUILT commands:");
				base.Console.Write("guilt get: Get the level of guilt");
				base.Console.Write("guilt reset: Reset guilt to 0");
				base.Console.Write("guilt add: Add guilt to current position");
			}
			break;
		case "get":
			if (ValidateParams(command2, 0, paramList))
			{
				base.Console.Write("Current guilt: " + Core.GuiltManager.GetDropsCount());
			}
			break;
		case "reset":
			if (ValidateParams(command2, 0, paramList))
			{
				Core.GuiltManager.ResetGuilt(restoreDropTears: true);
				base.Console.Write("Guilt reset to 0.");
			}
			break;
		case "add":
			if (ValidateParams(command2, 0, paramList))
			{
				Core.GuiltManager.AddGuilt();
				base.Console.Write("Guilt added.");
			}
			break;
		default:
			base.Console.Write("Command unknow, use guilt help");
			break;
		}
	}
}
