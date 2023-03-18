using Gameplay.UI.Widgets;

namespace Gameplay.UI.Console;

public class ExitCommand : ConsoleCommand
{
	public override void Execute(string command, string[] parameters)
	{
		ConsoleWidget.Instance.SetEnabled(enabled: false);
	}

	public override string GetName()
	{
		return "exit";
	}
}
