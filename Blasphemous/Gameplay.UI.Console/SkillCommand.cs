using System.Collections.Generic;
using Framework.FrameworkCore;
using Framework.Managers;

namespace Gameplay.UI.Console;

public class SkillCommand : ConsoleCommand
{
	public override bool HasLowerParameters()
	{
		return false;
	}

	public override void Execute(string command, string[] parameters)
	{
		List<string> listParameters;
		string subcommand = GetSubcommand(parameters, out listParameters);
		if (command != null && command == "skill")
		{
			ParseSkill(subcommand, listParameters);
		}
	}

	public override List<string> GetNames()
	{
		List<string> list = new List<string>();
		list.Add("skill");
		return list;
	}

	private void ParseSkill(string command, List<string> paramList)
	{
		string text = "skill " + command;
		switch (command)
		{
		case "help":
			if (ValidateParams(text, 0, paramList))
			{
				base.Console.Write("Available SKILL commands:");
				base.Console.Write("skill list: List all skills and the current status");
				base.Console.Write("skill lock SKILLNAME: Locks the skill SKILLNAME");
				base.Console.Write("skill lockall: Locks all the skills");
				base.Console.Write("skill unlock SKILLNAME: Unlocks the skill SKILLNAME");
				base.Console.Write("skill unlockall: Unlocks all skills");
				base.Console.Write("skill showui: Show the unlock UI");
			}
			break;
		case "list":
		{
			if (!ValidateParams(text, 0, paramList))
			{
				break;
			}
			base.Console.Write("*** All skills:");
			for (int k = 0; k <= Core.SkillManager.GetMaxSkillsTier(); k++)
			{
				List<UnlockableSkill> skillsByTier3 = Core.SkillManager.GetSkillsByTier(k);
				if (skillsByTier3.Count <= 0)
				{
					continue;
				}
				base.Console.Write("Tier " + k);
				foreach (UnlockableSkill item in skillsByTier3)
				{
					string text2 = "  " + item.id + "  -- " + item.caption + " (";
					text2 += ((!item.unlocked) ? "Locked" : "Unlocked");
					text2 += ")";
					base.Console.Write(text2);
				}
			}
			break;
		}
		case "unlock":
			if (ValidateParams(text, 1, paramList))
			{
				WriteCommandResult(text, Core.SkillManager.UnlockSkill(paramList[0].ToUpper(), ignoreChecks: true));
			}
			break;
		case "unlockall":
		{
			if (!ValidateParams(text, 0, paramList))
			{
				break;
			}
			for (int i = 0; i <= Core.SkillManager.GetMaxSkillsTier(); i++)
			{
				List<UnlockableSkill> skillsByTier = Core.SkillManager.GetSkillsByTier(i);
				if (skillsByTier.Count <= 0)
				{
					continue;
				}
				foreach (UnlockableSkill item2 in skillsByTier)
				{
					if (!item2.unlocked)
					{
						Core.SkillManager.UnlockSkill(item2.id.ToUpper(), ignoreChecks: true);
					}
				}
			}
			base.Console.Write("All skills unlocked");
			break;
		}
		case "lock":
			if (ValidateParams(text, 1, paramList))
			{
				WriteCommandResult(text, Core.SkillManager.LockSkill(paramList[0].ToUpper()));
			}
			break;
		case "lockall":
		{
			if (!ValidateParams(text, 0, paramList))
			{
				break;
			}
			for (int j = 0; j <= Core.SkillManager.GetMaxSkillsTier(); j++)
			{
				List<UnlockableSkill> skillsByTier2 = Core.SkillManager.GetSkillsByTier(j);
				if (skillsByTier2.Count <= 0)
				{
					continue;
				}
				foreach (UnlockableSkill item3 in skillsByTier2)
				{
					if (!item3.unlocked)
					{
						Core.SkillManager.LockSkill(item3.id.ToUpper());
					}
				}
			}
			break;
		}
		case "showui":
			if (ValidateParams(text, 0, paramList))
			{
				UIController.instance.ShowUnlockSKill();
			}
			break;
		default:
			base.Console.Write("Command " + text + " is unknow, use skill help");
			break;
		}
	}
}
