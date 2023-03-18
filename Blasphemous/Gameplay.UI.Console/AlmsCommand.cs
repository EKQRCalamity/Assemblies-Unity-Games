using System.Collections.Generic;
using Framework.Managers;

namespace Gameplay.UI.Console;

public class AlmsCommand : ConsoleCommand
{
	public override void Execute(string command, string[] parameters)
	{
		List<string> listParameters;
		string subcommand = GetSubcommand(parameters, out listParameters);
		if (command != null && command == "alms")
		{
			ParseAlms(subcommand, listParameters);
		}
	}

	public override List<string> GetNames()
	{
		List<string> list = new List<string>();
		list.Add("alms");
		return list;
	}

	private void ParseAlms(string command, List<string> paramList)
	{
		int resultValue = 0;
		string command2 = "alms " + command;
		switch (command)
		{
		case "help":
			if (ValidateParams(command2, 0, paramList))
			{
				base.Console.Write("Available ALMS commands:");
				base.Console.Write("alms current: Get current alms and tier");
				base.Console.Write("alms list: List all tiers requeriments");
				base.Console.Write("alms consume NUMBER: Consume NUMBER of tears");
				base.Console.Write("alms set NUMBER: Set the current alms");
			}
			break;
		case "current":
			if (ValidateParams(command2, 0, paramList))
			{
				base.Console.Write("Current tears given: " + Core.Alms.TearsGiven);
				base.Console.Write("Current tier: " + Core.Alms.CurentTier);
				base.Console.Write("Altar level: " + Core.Alms.GetAltarLevel());
				base.Console.Write("PrieDieu level: " + Core.Alms.GetPrieDieuLevel());
			}
			break;
		case "list":
		{
			if (!ValidateParams(command2, 0, paramList))
			{
				break;
			}
			base.Console.Write("Tears to reach tier:");
			int num = 1;
			{
				foreach (int tears in Core.Alms.Config.GetTearsList())
				{
					base.Console.Write("Tier " + num + ": " + tears + " tears needed.");
					num++;
				}
				break;
			}
		}
		case "consume":
			if (ValidateParams(command2, 1, paramList) && ValidateParam(paramList[0], out resultValue, 0, 99999))
			{
				if (!Core.Alms.CanConsumeTears(resultValue))
				{
					base.Console.Write("Error can't consume " + resultValue + " tears");
					break;
				}
				Core.Alms.ConsumeTears(resultValue);
				base.Console.Write("Tears consumed.");
			}
			break;
		case "set":
			if (ValidateParams(command2, 1, paramList) && ValidateParam(paramList[0], out resultValue, 0, 99999))
			{
				Core.Alms.DEBUG_SetTearsGiven(resultValue);
				base.Console.Write("Alms setted");
			}
			break;
		default:
			base.Console.Write("Command unknow, use audio help");
			break;
		}
	}
}
