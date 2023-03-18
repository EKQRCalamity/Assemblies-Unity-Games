using Framework.Managers;

namespace Gameplay.UI.Console;

public class Restart : ConsoleCommand
{
	public override void Execute(string command, string[] parameters)
	{
		base.Console.Write("Reloading current level...");
		Core.LevelManager.ChangeLevel(Core.LevelManager.currentLevel.LevelName);
		Core.Persistence.ResetCurrent();
	}

	public override string GetName()
	{
		return "restart";
	}
}
