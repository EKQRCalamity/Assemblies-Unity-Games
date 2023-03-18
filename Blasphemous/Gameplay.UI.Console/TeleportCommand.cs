using System.Collections.Generic;
using Framework.FrameworkCore;
using Framework.Managers;

namespace Gameplay.UI.Console;

public class TeleportCommand : ConsoleCommand
{
	public override bool HasLowerParameters()
	{
		return false;
	}

	public override void Execute(string command, string[] parameters)
	{
		List<string> listParameters;
		string subcommand = GetSubcommand(parameters, out listParameters);
		if (command != null && command == "teleport")
		{
			ParseTeleport(subcommand, listParameters);
		}
	}

	public override List<string> GetNames()
	{
		List<string> list = new List<string>();
		list.Add("teleport");
		return list;
	}

	private void ParseTeleport(string command, List<string> paramList)
	{
		string command2 = "teleport " + command;
		switch (command)
		{
		case "help":
			if (ValidateParams(command2, 0, paramList))
			{
				base.Console.Write("Available TELEPORT commands:");
				base.Console.Write("teleport list: List all teleports ID");
				base.Console.Write("teleport go IDTELEPORT: Teleport to this teleport");
				base.Console.Write("teleport showui: Show the teleport UI");
				base.Console.Write("teleport unlock [ID|ALL]: Unlocks the teleport ID or all");
			}
			break;
		case "list":
			if (!ValidateParams(command2, 0, paramList))
			{
				break;
			}
			base.Console.Write("*** All teleports:");
			{
				foreach (TeleportDestination allTeleport in Core.SpawnManager.GetAllTeleports())
				{
					string message = allTeleport.id + ": " + allTeleport.caption + "  (" + allTeleport.sceneName + ", " + allTeleport.teleportName + ")";
					base.Console.Write(message);
				}
				break;
			}
		case "go":
			if (ValidateParams(command2, 1, paramList))
			{
				Core.SpawnManager.Teleport(paramList[0].ToUpper());
			}
			break;
		case "showui":
			if (ValidateParams(command2, 0, paramList))
			{
				UIController.instance.ShowTeleportUI();
			}
			break;
		case "unlock":
			if (ValidateParams(command2, 1, paramList))
			{
				UnlockTeleport(paramList[0]);
			}
			break;
		default:
			base.Console.Write("Command unknown, use teleport help");
			break;
		}
	}

	private static void UnlockTeleport(string teleportID)
	{
		string text = teleportID.ToUpperInvariant();
		if (text != null && text == "ALL")
		{
			{
				foreach (TeleportDestination allTeleport in Core.SpawnManager.GetAllTeleports())
				{
					if (allTeleport.useInUI)
					{
						Core.SpawnManager.SetTeleportActive(allTeleport.id, active: true);
					}
				}
				return;
			}
		}
		Core.SpawnManager.SetTeleportActive(teleportID.ToUpperInvariant(), active: true);
	}
}
