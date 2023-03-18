using System.Collections.Generic;
using Framework.Managers;
using Framework.Map;

namespace Gameplay.UI.Console;

public class MapCommand : ConsoleCommand
{
	public override void Execute(string command, string[] parameters)
	{
		List<string> listParameters;
		string subcommand = GetSubcommand(parameters, out listParameters);
		if (command != null && command == "map")
		{
			ParseMap(subcommand, listParameters);
		}
	}

	public override List<string> GetNames()
	{
		List<string> list = new List<string>();
		list.Add("map");
		return list;
	}

	public override bool HasLowerParameters()
	{
		return false;
	}

	public override bool ToLowerAll()
	{
		return false;
	}

	private void ParseMap(string command, List<string> paramList)
	{
		string command2 = "map " + command;
		switch (command)
		{
		case "help":
			if (ValidateParams(command2, 0, paramList))
			{
				base.Console.Write("Available MAP commands:");
				base.Console.Write("map list: List all maps.");
				base.Console.Write("map set MAPID: Set current map.");
				base.Console.Write("map secrets: List currentmap secrets.");
				base.Console.Write("map secret SECRETID ON/OFF: Sets the secret SECRETID on or off in current map.");
				base.Console.Write("map reveal all: Reveal all map");
				base.Console.Write("map reveal DISTRICT: Reveal district DISTRICT");
				base.Console.Write("map reveal DISTRICT ZONE: Reveal zone ZONE in district DISTRICT");
				base.Console.Write("map unrevealed: Lists all unrevealed cells in current map");
			}
			break;
		case "list":
			if (!ValidateParams(command2, 0, paramList))
			{
				break;
			}
			base.Console.Write("Available MAPs:");
			{
				foreach (string allMap in Core.NewMapManager.GetAllMaps())
				{
					string text = allMap;
					if (Core.NewMapManager.GetCurrentMap() == allMap)
					{
						text += "  <--- Current";
					}
					base.Console.Write(text);
				}
				break;
			}
		case "set":
			if (ValidateParams(command2, 1, paramList))
			{
				WriteCommandResult("map set", Core.NewMapManager.SetCurrentMap(paramList[0]));
			}
			break;
		case "secrets":
			if (!ValidateParams(command2, 0, paramList))
			{
				break;
			}
			base.Console.Write("Available Secrets:");
			{
				foreach (SecretData allSecret in Core.NewMapManager.GetAllSecrets())
				{
					base.Console.Write(allSecret.Name + ", revealed: " + allSecret.Revealed);
				}
				break;
			}
		case "secret":
			if (ValidateParams(command2, 2, paramList))
			{
				string secretId = paramList[0];
				bool enable = ((paramList[1].ToUpper() == "ON" || paramList[1].ToUpper() == "TRUE") ? true : false);
				WriteCommandResult("map secret", Core.NewMapManager.SetSecret(secretId, enable));
			}
			break;
		case "unrevealed":
		{
			base.Console.WriteFormat("Map type: {0}", (!Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.NEW_GAME_PLUS)) ? "base" : "NG+");
			List<CellData> unrevealedCellsForCompletion = Core.NewMapManager.GetUnrevealedCellsForCompletion();
			base.Console.WriteFormat("Total unrevealed cells: {0}", unrevealedCellsForCompletion.Count);
			{
				foreach (CellData item in unrevealedCellsForCompletion)
				{
					base.Console.WriteFormat("   Cell: {0} {1}", item.ZoneId.GetKey(), item.CellKey);
				}
				break;
			}
		}
		case "reveal":
			if (paramList.Count != 1 && paramList.Count != 2)
			{
				base.Console.Write("The command map reveal needs 1 or 2 params. You passed " + paramList.Count);
			}
			else if (paramList.Count == 1)
			{
				if (paramList[0].ToUpper() == "ALL")
				{
					Core.NewMapManager.RevealAllMap();
				}
				else if (paramList[0].ToUpper() == "NG")
				{
					Core.NewMapManager.RevealAllNGMap();
				}
				else
				{
					Core.NewMapManager.RevealAllDistrict(paramList[0]);
				}
			}
			else
			{
				Core.NewMapManager.RevealAllZone(paramList[0], paramList[1]);
			}
			break;
		default:
			base.Console.Write("Command unknow, use map help");
			break;
		}
	}
}
