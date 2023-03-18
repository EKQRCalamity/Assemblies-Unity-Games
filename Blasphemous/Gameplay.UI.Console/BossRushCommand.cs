using System;
using System.Collections.Generic;
using Framework.Managers;

namespace Gameplay.UI.Console;

public class BossRushCommand : ConsoleCommand
{
	public override bool HasLowerParameters()
	{
		return false;
	}

	public override void Execute(string command, string[] parameters)
	{
		List<string> listParameters;
		string subcommand = GetSubcommand(parameters, out listParameters);
		if (command != null && command == "bossrush")
		{
			ParseBossRush(subcommand, listParameters);
		}
	}

	public override List<string> GetNames()
	{
		List<string> list = new List<string>();
		list.Add("bossrush");
		return list;
	}

	private void ParseBossRush(string command, List<string> paramList)
	{
		string command2 = "bossrush " + command;
		switch (command)
		{
		case "help":
			if (ValidateParams(command2, 0, paramList))
			{
				base.Console.Write("Available BOSS RUSH commands:");
				base.Console.Write("start course_x y: starts the x boss rush course with the difficulty y.");
				base.Console.Write("hub: loads the hub scene.");
				base.Console.Write("next: loads the next course scene.");
				base.Console.Write("golast: goes to last boss.");
				base.Console.Write("end: ends the current boss rush run if any, showing the results.");
				base.Console.Write("printscore: prints in the console the current score.");
				base.Console.Write("unlock course_x: unlocks course_x.");
			}
			break;
		case "start":
			if (ValidateParams(command2, 2, paramList))
			{
				string value = paramList[0];
				string value2 = paramList[1];
				BossRushManager.BossRushCourseId courseId2 = (BossRushManager.BossRushCourseId)Enum.Parse(typeof(BossRushManager.BossRushCourseId), value, ignoreCase: true);
				BossRushManager.BossRushCourseMode courseMode = (BossRushManager.BossRushCourseMode)Enum.Parse(typeof(BossRushManager.BossRushCourseMode), value2, ignoreCase: true);
				Core.BossRushManager.StartCourse(courseId2, courseMode);
			}
			break;
		case "hub":
			if (ValidateParams(command2, 0, paramList))
			{
				Core.BossRushManager.LoadHub();
			}
			break;
		case "next":
			if (ValidateParams(command2, 0, paramList))
			{
				Core.BossRushManager.LoadCourseNextScene();
			}
			break;
		case "golast":
			if (ValidateParams(command2, 0, paramList))
			{
				Core.BossRushManager.LoadLastScene();
			}
			break;
		case "end":
			if (ValidateParams(command2, 0, paramList))
			{
				Core.BossRushManager.EndCourse(completed: true);
			}
			break;
		case "printscore":
			if (ValidateParams(command2, 0, paramList))
			{
				Core.BossRushManager.LogHighScoreObtained();
			}
			break;
		case "unlock":
			if (ValidateParams(command2, 1, paramList))
			{
				string courseId = paramList[0].ToUpper();
				Core.BossRushManager.DEBUGUnlockCourse(courseId);
			}
			break;
		default:
			base.Console.Write("Command unknow, use bossrush help");
			break;
		}
	}
}
