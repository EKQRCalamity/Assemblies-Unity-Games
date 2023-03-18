using System.Collections.Generic;
using System.Collections.ObjectModel;
using Framework.Managers;

namespace Gameplay.UI.Console;

public class MiriamCommand : ConsoleCommand
{
	public override bool HasLowerParameters()
	{
		return false;
	}

	public override void Execute(string command, string[] parameters)
	{
		List<string> listParameters;
		string subcommand = GetSubcommand(parameters, out listParameters);
		if (command != null && command == "miriam")
		{
			ParseMiriam(subcommand, listParameters);
		}
	}

	public override List<string> GetNames()
	{
		List<string> list = new List<string>();
		list.Add("miriam");
		return list;
	}

	private void ParseMiriam(string command, List<string> paramList)
	{
		string command2 = "miriam " + command;
		switch (command)
		{
		case "":
		case "help":
			if (ValidateParams(command2, 0, paramList))
			{
				base.Console.Write("Available MIRIAM commands:");
				base.Console.Write("miriam status: Write the quest status");
				base.Console.Write("miriam start: Start miriam quest");
				base.Console.Write("miriam end: End miriam quest");
				base.Console.Write("miriam activateportal: Start the next miriam portal and teleport");
				base.Console.Write("miriam deactivateportal: End the current portal and teleport outside");
			}
			break;
		case "status":
		{
			if (!ValidateParams(command2, 0, paramList))
			{
				break;
			}
			base.Console.Write("Miriam started: " + Core.Events.IsMiriamQuestStarted);
			base.Console.Write("Miriam finished: " + Core.Events.IsMiriamQuestFinished);
			base.Console.Write("Miriam current portal: " + Core.Events.MiriamCurrentScenePortal);
			ReadOnlyCollection<string> miriamClosedPortals = Core.Events.GetMiriamClosedPortals();
			base.Console.Write("Miriam closed portals: " + miriamClosedPortals.Count);
			{
				foreach (string item in miriamClosedPortals)
				{
					base.Console.Write("   Portal from scene " + item);
				}
				break;
			}
		}
		case "start":
			if (ValidateParams(command2, 0, paramList))
			{
				WriteCommandResult("miriam start", Core.Events.StartMiriamQuest());
			}
			break;
		case "end":
			if (ValidateParams(command2, 0, paramList))
			{
				WriteCommandResult("miriam end", Core.Events.FinishMiriamQuest());
			}
			break;
		case "activateportal":
			if (ValidateParams(command2, 0, paramList))
			{
				WriteCommandResult("miriam activateportal", Core.Events.ActivateMiriamPortalAndTeleport());
			}
			break;
		case "deactivateportal":
			if (ValidateParams(command2, 0, paramList))
			{
				WriteCommandResult("miriam deactivateportal", Core.Events.EndMiriamPortalAndReturn());
			}
			break;
		case "gotogoal":
			if (ValidateParams(command2, 0, paramList))
			{
				WriteCommandResult("miriam gotogoal", Core.Events.TeleportPenitentToGoal());
			}
			break;
		default:
			base.Console.Write("Command unknow, use miriam help");
			break;
		}
	}
}
