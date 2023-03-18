using Framework.Managers;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.UI.Console;

public class ExecutionCommand : ConsoleCommand
{
	public const string ErrorMessage = "Parameter must be Y/n";

	public const string SuccessMessage = "Debug execution {0}";

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
		return "execution";
	}

	private void RunningCommand(string parameter)
	{
		switch (parameter)
		{
		case "y":
			EnableDebugExecution();
			base.Console.Write(string.Format("Debug execution {0}", "enabled."));
			break;
		case "n":
			EnableDebugExecution(enable: false);
			base.Console.Write(string.Format("Debug execution {0}", "disabled."));
			break;
		default:
			base.Console.Write("Parameter must be Y/n");
			break;
		}
	}

	public static void EnableDebugExecution(bool enable = true)
	{
		Enemy[] array = Object.FindObjectsOfType<Enemy>();
		Enemy[] array2 = array;
		foreach (Enemy enemy in array2)
		{
			enemy.DebugExecutionActive = enable;
		}
		Core.Logic.DebugExecutionEnabled = enable;
	}
}
