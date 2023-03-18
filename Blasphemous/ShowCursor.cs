using Gameplay.UI.Console;
using Gameplay.UI.Widgets;
using UnityEngine;

public class ShowCursor : ConsoleCommand
{
	public const string ErrorMessage = "Parameter must be Y/n";

	public const string SuccessMessage = "Show Cursor {0}";

	public override void Execute(string command, string[] parameters)
	{
		base.Execute(command, parameters);
		if (parameters.Length != 1)
		{
			base.Console.Write("Parameter must be Y/n");
			return;
		}
		string parameter = parameters[0].ToLower();
		RunningCommand(parameter);
	}

	public override string GetName()
	{
		return "showcursor";
	}

	private void RunningCommand(string parameter)
	{
		switch (parameter)
		{
		case "y":
			ShowMouseCursor();
			base.Console.Write(string.Format("Show Cursor {0}", "enabled."));
			break;
		case "n":
			ShowMouseCursor(enable: false);
			base.Console.Write(string.Format("Show Cursor {0}", "disabled."));
			break;
		default:
			base.Console.Write("Parameter must be Y/n");
			break;
		}
	}

	public static void ShowMouseCursor(bool enable = true)
	{
		DebugInformation debugInformation = Object.FindObjectOfType<DebugInformation>();
		if ((bool)debugInformation)
		{
			debugInformation.showCursor = enable;
		}
	}
}
