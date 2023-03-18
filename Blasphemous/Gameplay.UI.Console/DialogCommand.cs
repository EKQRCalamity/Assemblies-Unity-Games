using System.Collections.Generic;
using Framework.Dialog;
using Framework.Managers;

namespace Gameplay.UI.Console;

public class DialogCommand : ConsoleCommand
{
	public override bool HasLowerParameters()
	{
		return false;
	}

	public override void Execute(string command, string[] parameters)
	{
		List<string> listParameters;
		string subcommand = GetSubcommand(parameters, out listParameters);
		if (command != null && command == "dialog")
		{
			ParseDialog(subcommand, listParameters);
		}
	}

	public override List<string> GetNames()
	{
		List<string> list = new List<string>();
		list.Add("dialog");
		return list;
	}

	private void ParseDialog(string command, List<string> paramList)
	{
		string command2 = "dialog " + command;
		switch (command)
		{
		case "help":
			if (ValidateParams(command2, 0, paramList))
			{
				base.Console.Write("Available DIALOG commands:");
				base.Console.Write("dialog list: List all dialogs");
				base.Console.Write("dialog start IDDIALOG: Start a dialog");
			}
			break;
		case "list":
			if (!ValidateParams(command2, 0, paramList))
			{
				break;
			}
			base.Console.Write("All dialogs ID:");
			{
				foreach (DialogObject allDialog in Core.Dialog.GetAllDialogs())
				{
					base.Console.Write(allDialog.id.ToString() + " - " + allDialog.sortDescription);
				}
				break;
			}
		case "start":
			if (ValidateParams(command2, 1, paramList))
			{
				WriteCommandResult(command2, Core.Dialog.StartConversation(paramList[0], modal: true, useOnlyLast: false));
			}
			break;
		default:
			base.Console.Write("Command unknow, use dialog help");
			break;
		}
	}
}
