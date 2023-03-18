using System.Collections.Generic;
using Framework.Managers;

namespace Gameplay.UI.Console;

public class SaveGameCommand : ConsoleCommand
{
	public override bool HasLowerParameters()
	{
		return false;
	}

	public override void Execute(string command, string[] parameters)
	{
		List<string> listParameters;
		string subcommand = GetSubcommand(parameters, out listParameters);
		if (command != null && command == "savegame")
		{
			ParseSaveGame(subcommand, listParameters);
		}
	}

	public override List<string> GetNames()
	{
		List<string> list = new List<string>();
		list.Add("savegame");
		return list;
	}

	private void ParseSaveGame(string command, List<string> paramList)
	{
		string command2 = "savegame " + command;
		int resultValue = 0;
		switch (command)
		{
		case "help":
			if (ValidateParams(command2, 0, paramList))
			{
				base.Console.Write("Available SAVEGAME commands:");
				base.Console.Write("savegame load SLOT: Loads the current game to slot");
				base.Console.Write("savegame save SLOT: Save the current game to slot");
				base.Console.Write("savegame enablenewgameplus: Enable new game plus in this slot.");
			}
			break;
		case "load":
			if (ValidateParams(command2, 1, paramList) && ValidateParam(paramList[0], out resultValue, 0, 10))
			{
				if (Core.Persistence.ExistSlot(resultValue))
				{
					Core.Persistence.LoadGame(resultValue);
				}
				else
				{
					base.Console.Write("Slot " + resultValue + " not found");
				}
			}
			break;
		case "save":
			if (ValidateParams(command2, 1, paramList) && ValidateParam(paramList[0], out resultValue, 0, 10))
			{
				Core.Persistence.SaveGame(resultValue);
			}
			break;
		case "enablenewgameplus":
			if (ValidateParams(command2, 0, paramList))
			{
				Core.Persistence.HACK_EnableNewGamePlusInCurrent();
			}
			break;
		default:
			base.Console.Write("Command unknow, use savegame help");
			break;
		}
	}
}
