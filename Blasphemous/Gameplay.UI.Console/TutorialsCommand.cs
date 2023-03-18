using System.Collections.Generic;
using Framework.FrameworkCore;
using Framework.Managers;
using Framework.Util;
using Sirenix.Utilities;
using UnityEngine;

namespace Gameplay.UI.Console;

public class TutorialsCommand : ConsoleCommand
{
	public override bool HasLowerParameters()
	{
		return false;
	}

	public override List<string> GetNames()
	{
		List<string> list = new List<string>();
		list.Add("tutorial");
		return list;
	}

	public override void Execute(string command, string[] parameters)
	{
		List<string> listParameters;
		string subcommand = GetSubcommand(parameters, out listParameters);
		Tutorial[] array = Resources.FindObjectsOfTypeAll<Tutorial>();
		array.Sort((Tutorial t1, Tutorial t2) => (t1.order >= t2.order) ? 1 : (-1));
		switch (subcommand)
		{
		case "list":
		{
			base.Console.Write("Available tutorials:");
			Tutorial[] array3 = array;
			foreach (Tutorial tutorial2 in array3)
			{
				base.Console.Write($"{tutorial2.id}: {tutorial2.description}");
			}
			break;
		}
		case "show":
		{
			if (!ValidateParams("tutorial list", 1, listParameters))
			{
				break;
			}
			string text = null;
			Tutorial[] array2 = array;
			foreach (Tutorial tutorial in array2)
			{
				if (tutorial.id.ToUpper().StartsWith(listParameters[0].ToUpper()))
				{
					text = tutorial.id;
					break;
				}
			}
			if (text != null)
			{
				Singleton<Core>.Instance.StartCoroutine(Core.TutorialManager.ShowTutorial(text));
			}
			else
			{
				base.Console.Write("Unknown tutorial: " + listParameters[0]);
			}
			break;
		}
		default:
			base.Console.Write("Available " + command + " commands:");
			base.Console.Write(command + " show TUTORIAL_ID: Show TUTORIAL_ID tutorial");
			base.Console.Write(command + " list: Show tutorials list");
			break;
		}
	}
}
