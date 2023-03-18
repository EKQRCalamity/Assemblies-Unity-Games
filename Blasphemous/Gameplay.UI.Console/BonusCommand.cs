using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Framework.FrameworkCore.Attributes.Logic;
using Gameplay.GameControllers.Entities;

namespace Gameplay.UI.Console;

public class BonusCommand : ConsoleCommand
{
	public override bool HasLowerParameters()
	{
		return false;
	}

	public override void Execute(string command, string[] parameters)
	{
		List<string> listParameters;
		switch (GetSubcommand(parameters, out listParameters))
		{
		case "help":
			base.Console.Write("Available " + command + " commands:");
			base.Console.Write(command + " help: Show this help");
			base.Console.Write(command + " list: List all bonuses");
			break;
		case "list":
			base.Console.Write("Current bonuses:");
			{
				foreach (EntityStats.StatsTypes value in Enum.GetValues(typeof(EntityStats.StatsTypes)))
				{
					Framework.FrameworkCore.Attributes.Logic.Attribute byType = base.Penitent.Stats.GetByType(value);
					ReadOnlyCollection<RawBonus> rawBonus = byType.GetRawBonus();
					if (rawBonus.Count <= 0 && !(byType.PermanetBonus > 0f))
					{
						continue;
					}
					base.Console.Write(value.ToString() + " stat, permanet " + byType.PermanetBonus);
					foreach (RawBonus item in rawBonus)
					{
						base.Console.Write("...Base:" + item.Base + "  Multyplier:" + item.Multiplier);
					}
				}
				break;
			}
		default:
			base.Console.Write("Command unknow, use " + command + " help");
			break;
		}
	}

	public override List<string> GetNames()
	{
		List<string> list = new List<string>();
		list.Add("bonus");
		return list;
	}
}
