using System.Collections.Generic;
using Framework.FrameworkCore.Attributes.Logic;

namespace Gameplay.UI.Console;

public class StatsCommand : ConsoleCommand
{
	public override bool HasLowerParameters()
	{
		return false;
	}

	public override void Execute(string command, string[] parameters)
	{
		List<string> listParameters;
		string subcommand = GetSubcommand(parameters, out listParameters);
		Attribute attribute = null;
		VariableAttribute variableAttribute = null;
		switch (command)
		{
		case "health":
			variableAttribute = base.Penitent.Stats.Life;
			break;
		case "flask":
			variableAttribute = base.Penitent.Stats.Flask;
			break;
		case "fervour":
			variableAttribute = base.Penitent.Stats.Fervour;
			break;
		case "purge":
			variableAttribute = base.Penitent.Stats.Purge;
			break;
		case "meaculpa":
			attribute = base.Penitent.Stats.MeaCulpa;
			break;
		case "strength":
			attribute = base.Penitent.Stats.Strength;
			break;
		case "flaskhealth":
			attribute = base.Penitent.Stats.FlaskHealth;
			break;
		}
		if (attribute == null)
		{
			attribute = variableAttribute;
		}
		if (subcommand == "help")
		{
			base.Console.Write("Available " + command + " commands:");
			base.Console.Write(command + " current: Show current value");
			base.Console.Write(command + " set VALUE: Set current value");
			base.Console.Write(command + " reset: Reset the upgrades");
			base.Console.Write(command + " upgrade: Upgrade stat");
			base.Console.Write(command + " upgradeto VALUE: Upgrade stat until final value is less that NUMBER");
			if (attribute.IsVariable())
			{
				base.Console.Write(command + " fill: Fill to max value");
				base.Console.Write(command + " setmax VALUE: Set max value");
			}
			return;
		}
		float resultValue2;
		int resultValue;
		if (!attribute.IsVariable())
		{
			switch (subcommand)
			{
			case "current":
				base.Console.Write("Current: " + attribute.Final + " (perma:" + attribute.PermanetBonus + ")");
				break;
			case "set":
				if (ValidateParams(command + " set", 1, listParameters) && ValidateParam(listParameters[0], out resultValue2, 0f, 99999f))
				{
					attribute.ConsoleSet(resultValue2);
					WriteCommandResult(command + " set", result: true);
				}
				break;
			case "upgrade":
				attribute.Upgrade();
				base.Console.Write("Upgraded, new value: " + attribute.Final + " (perma:" + attribute.PermanetBonus + ")");
				break;
			case "reset":
				attribute.ResetUpgrades();
				base.Console.Write("Reset, new value: " + attribute.Final + " (perma:" + attribute.PermanetBonus + ")");
				break;
			case "upgradeto":
				if (ValidateParams(command + " upgradeto", 1, listParameters) && ValidateParam(listParameters[0], out resultValue, 0, 9999))
				{
					while (attribute.Final < (float)resultValue)
					{
						attribute.Upgrade();
					}
					base.Console.Write("Upgraded, new value: " + attribute.Final + " (perma:" + attribute.PermanetBonus + ")");
				}
				break;
			default:
				base.Console.Write("Command unknow, use " + command + " help");
				break;
			}
			return;
		}
		switch (subcommand)
		{
		case "current":
			base.Console.Write("Current: " + variableAttribute.Current + ", ATTR_MAX:" + variableAttribute.Final + " (perma:" + variableAttribute.PermanetBonus + "), MAX:" + variableAttribute.MaxValue);
			break;
		case "fill":
			variableAttribute.SetToCurrentMax();
			WriteCommandResult(command + " fill", result: true);
			break;
		case "set":
			if (ValidateParams(command + " set", 1, listParameters) && ValidateParam(listParameters[0], out resultValue2, 0f, 99999f))
			{
				variableAttribute.Current = resultValue2;
				WriteCommandResult(command + " set", result: true);
			}
			break;
		case "setmax":
			if (ValidateParams(command + " setmax", 1, listParameters) && ValidateParam(listParameters[0], out resultValue2, 0f, 99999f))
			{
				variableAttribute.SetPermanentBonus(0f);
				variableAttribute.MaxValue = resultValue2;
				WriteCommandResult(command + " setmax", result: true);
			}
			break;
		case "upgrade":
			variableAttribute.Upgrade();
			base.Console.Write("Upgraded, new value: " + attribute.Final + " (perma:" + attribute.PermanetBonus + ")");
			break;
		case "reset":
			attribute.ResetUpgrades();
			base.Console.Write("Reset, new value: " + attribute.Final + " (perma:" + attribute.PermanetBonus + ")");
			break;
		case "upgradeto":
			if (ValidateParams(command + " upgradeto", 1, listParameters) && ValidateParam(listParameters[0], out resultValue, 0, 9999))
			{
				while (attribute.Final < (float)resultValue)
				{
					attribute.Upgrade();
				}
				base.Console.Write("Upgraded, new value: " + attribute.Final + " (perma:" + attribute.PermanetBonus + ")");
			}
			break;
		default:
			base.Console.Write("Command unknow, use " + command + " help");
			break;
		}
	}

	public override List<string> GetNames()
	{
		List<string> list = new List<string>();
		list.Add("health");
		list.Add("flask");
		list.Add("fervour");
		list.Add("purge");
		list.Add("meaculpa");
		list.Add("strength");
		list.Add("flaskhealth");
		return list;
	}
}
