using System.Collections.Generic;
using Framework.Managers;

namespace Gameplay.UI.Console;

public class GameModeCommand : ConsoleCommand
{
	public override bool HasLowerParameters()
	{
		return false;
	}

	public override void Execute(string command, string[] parameters)
	{
		List<string> listParameters;
		string subcommand = GetSubcommand(parameters, out listParameters);
		if (command != null && command == "gamemode")
		{
			ParseGameMode(subcommand, listParameters);
		}
	}

	public override List<string> GetNames()
	{
		List<string> list = new List<string>();
		list.Add("gamemode");
		return list;
	}

	private void ParseGameMode(string command, List<string> paramList)
	{
		string command2 = "gamemode " + command;
		switch (command)
		{
		case "help":
			if (ValidateParams(command2, 0, paramList))
			{
				base.Console.Write("Available GAMEMODE commands:");
				base.Console.Write("list: get all the possible game modes.");
				base.Console.Write("current: get the currently active game mode.");
				base.Console.Write("set GAME_MODE: sets the currently active game mode to a given value.");
			}
			break;
		case "list":
			if (!ValidateParams(command2, 0, paramList))
			{
				break;
			}
			{
				foreach (string allGameModesName in GameModeManager.GetAllGameModesNames())
				{
					base.Console.Write("Game Mode: " + allGameModesName);
				}
				break;
			}
		case "current":
			if (ValidateParams(command2, 0, paramList))
			{
				base.Console.Write("Current Game Mode: " + Core.GameModeManager.GetCurrentGameModeName());
			}
			break;
		case "set":
			if (ValidateParams(command2, 1, paramList))
			{
				if (Core.GameModeManager.GameModeExists(paramList[0]))
				{
					Core.GameModeManager.ChangeMode(paramList[0]);
					base.Console.Write("Game Mode has been changed to: " + paramList[0].ToUpper());
				}
				else
				{
					base.Console.Write("A Game Mode with name: " + paramList[0].ToUpper() + " doesn't exist!");
				}
			}
			break;
		default:
			base.Console.Write("Command unknow, use gamemode help");
			break;
		}
	}
}
