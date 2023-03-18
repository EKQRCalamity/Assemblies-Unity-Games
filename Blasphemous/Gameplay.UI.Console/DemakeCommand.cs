using System.Collections.Generic;
using Framework.Managers;

namespace Gameplay.UI.Console;

public class DemakeCommand : ConsoleCommand
{
	public override bool HasLowerParameters()
	{
		return false;
	}

	public override void Execute(string command, string[] parameters)
	{
		List<string> listParameters;
		string subcommand = GetSubcommand(parameters, out listParameters);
		if (command != null && command == "demake")
		{
			ParseDemake(subcommand, listParameters);
		}
	}

	public override List<string> GetNames()
	{
		List<string> list = new List<string>();
		list.Add("demake");
		return list;
	}

	private void ParseDemake(string command, List<string> paramList)
	{
		string command2 = "demake " + command;
		switch (command)
		{
		case "help":
			if (ValidateParams(command2, 0, paramList))
			{
				base.Console.Write("Available DEMAKE commands:");
				base.Console.Write("enter: enter the demake.");
			}
			break;
		case "enter":
			if (ValidateParams(command2, 0, paramList))
			{
				Core.DemakeManager.StartDemakeRun();
			}
			break;
		default:
			base.Console.Write("Command unknow, use demake help");
			break;
		}
	}
}
