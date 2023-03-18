using Framework.Managers;

namespace Gameplay.UI.Console;

public class FervourRefill : ConsoleCommand
{
	private static bool enabled;

	public override void Execute(string command, string[] parameters)
	{
		enabled = !enabled;
		base.Console.Write("Fervour Refill: " + ((!enabled) ? "DISABLED" : "ENABLED"));
	}

	public override void Update()
	{
		if (Core.Logic.Penitent != null && enabled)
		{
			Core.Logic.Penitent.Stats.Fervour.SetToCurrentMax();
		}
	}

	public override string GetName()
	{
		return "fervourrefill";
	}
}
