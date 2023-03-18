using Framework.Managers;

namespace Gameplay.UI.Console;

public class Invincible : ConsoleCommand
{
	private static bool enabled;

	public override void Execute(string command, string[] parameters)
	{
		enabled = !enabled;
		base.Console.Write("Player invulnerability: " + ((!enabled) ? "DISABLED" : "ENABLED"));
		Core.SpawnManager.AutomaticRespawn = enabled;
	}

	public override void Update()
	{
		if (Core.Logic.Penitent != null && enabled)
		{
			Core.Logic.Penitent.Stats.Life.SetToCurrentMax();
		}
	}

	public override string GetName()
	{
		return "invincible";
	}
}
