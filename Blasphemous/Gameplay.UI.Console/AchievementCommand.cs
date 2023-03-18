using System.Collections.Generic;
using Framework.Managers;
using Steamworks;

namespace Gameplay.UI.Console;

public class AchievementCommand : ConsoleCommand
{
	public override bool HasLowerParameters()
	{
		return false;
	}

	public override void Execute(string command, string[] parameters)
	{
		List<string> listParameters;
		string subcommand = GetSubcommand(parameters, out listParameters);
		if (command != null && command == "achievement")
		{
			ParseAchievement(subcommand, listParameters);
		}
	}

	public override List<string> GetNames()
	{
		List<string> list = new List<string>();
		list.Add("achievement");
		return list;
	}

	private void ParseAchievement(string command, List<string> paramList)
	{
		string command2 = "achievement " + command;
		switch (command)
		{
		case "help":
			if (ValidateParams(command2, 0, paramList))
			{
				base.Console.Write("Available ACHIEVEMENT commands:");
				base.Console.Write("enablepopup: Show the popup when achievement is granted");
				base.Console.Write("disablepopup: Dont show the popup when achievement is granted");
				base.Console.Write("check acxx: checks whether the achievement with acxx as its ID is granted or not.");
				base.Console.Write("grant acxx: grants the achievement with acxx as its ID.");
				base.Console.Write("clear acxx: clears the progress of the achievement with acxx as its ID.");
				base.Console.Write("clearsteam acxx: clears the progress of the achievement with acxx on Steam.");
				base.Console.Write("clearall: clears the progress of all the achievements.");
				base.Console.Write("clearsteamall: clears the progress of all the achievements on Steam.");
				base.Console.Write("addprogress acxx progress: adds progress (from 0f to 100f) to the achievement with acxx as its ID.");
				base.Console.Write("checkprogress acxx: checks the progress of the achievement with acxx as its ID.");
			}
			break;
		case "enablepopup":
			if (ValidateParams(command2, 0, paramList))
			{
				Core.AchievementsManager.ShowPopUp = true;
				base.Console.Write("Popup is enabled");
			}
			break;
		case "disablepopup":
			if (ValidateParams(command2, 0, paramList))
			{
				Core.AchievementsManager.ShowPopUp = false;
				base.Console.Write("Popup is disabled");
			}
			break;
		case "check":
			if (ValidateParams(command2, 1, paramList))
			{
				if (Core.AchievementsManager.CheckAchievementGranted(paramList[0]))
				{
					base.Console.Write("Achievement: " + paramList[0] + " is granted.");
				}
				else
				{
					base.Console.Write("Achievement: " + paramList[0] + " is not granted.");
				}
			}
			break;
		case "grant":
			if (!ValidateParams(command2, 1, paramList))
			{
				break;
			}
			if (Core.AchievementsManager.CheckAchievementGranted(paramList[0]))
			{
				base.Console.Write("Achievement: " + paramList[0] + " is already granted.");
				break;
			}
			Core.AchievementsManager.GrantAchievement(paramList[0]);
			if (Core.AchievementsManager.CheckAchievementGranted(paramList[0]))
			{
				base.Console.Write("Achievement: " + paramList[0] + " has been granted.");
			}
			else
			{
				base.Console.Write("Achievement: " + paramList[0] + " hasn't been granted. Something went wrong.");
			}
			break;
		case "clear":
			if (ValidateParams(command2, 1, paramList))
			{
				Core.AchievementsManager.DebugResetAchievement(paramList[0]);
				if (Core.AchievementsManager.CheckAchievementGranted(paramList[0]))
				{
					base.Console.Write("Achievement: " + paramList[0] + " hasn't been cleared. Something went wrong.");
				}
				else
				{
					base.Console.Write("Achievement: " + paramList[0] + " has been cleared.");
				}
			}
			break;
		case "clearsteam":
			if (!ValidateParams(command2, 1, paramList))
			{
				break;
			}
			if (SteamManager.Initialized)
			{
				bool flag2 = SteamUserStats.ClearAchievement(paramList[0]);
				if (!SteamUserStats.StoreStats())
				{
					base.Console.Write("Achievement: " + paramList[0] + " hasn't been cleared from Steam for user " + SteamUser.GetHSteamUser().m_HSteamUser + ". Something went wrong.");
				}
				else
				{
					base.Console.Write("Achievement: " + paramList[0] + " has been cleared from Steam for user " + SteamUser.GetHSteamUser().m_HSteamUser);
				}
			}
			else
			{
				base.Console.Write("STEAM is not initialized!");
			}
			break;
		case "clearall":
			if (ValidateParams(command2, 0, paramList))
			{
				Core.AchievementsManager.DebugReset();
			}
			break;
		case "clearsteamall":
			if (!ValidateParams(command2, 0, paramList))
			{
				break;
			}
			if (SteamManager.Initialized)
			{
				bool flag = false;
				foreach (string key in Core.AchievementsManager.Achievements.Keys)
				{
					flag |= SteamUserStats.ClearAchievement(key);
				}
				if (!SteamUserStats.StoreStats())
				{
					base.Console.Write("Achievements hasn't been cleared from Steam. Something went wrong.");
				}
				else
				{
					base.Console.Write("Achievements has been cleared from Steam for user " + SteamUser.GetHSteamUser().m_HSteamUser);
				}
			}
			else
			{
				base.Console.Write("STEAM is not initialized!");
			}
			break;
		case "addprogress":
			if (ValidateParams(command2, 2, paramList))
			{
				Core.AchievementsManager.AddAchievementProgress(paramList[0], float.Parse(paramList[1]));
				base.Console.Write("Achievement: " + paramList[0] + " has been added a progress of: " + paramList[1]);
			}
			break;
		case "checkprogress":
			if (ValidateParams(command2, 1, paramList))
			{
				float num = Core.AchievementsManager.CheckAchievementProgress(paramList[0]);
				base.Console.Write("Achievement: " + paramList[0] + " has a progress of: " + num);
			}
			break;
		default:
			base.Console.Write("Command unknow, use achievement help");
			break;
		}
	}
}
