using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.UI.Console;

public class Npcoff : ConsoleCommand
{
	private bool enabled;

	public override void Execute(string command, string[] parameters)
	{
		enabled = !enabled;
		DamageArea[] array = Object.FindObjectsOfType<DamageArea>();
		DamageArea[] array2 = array;
		foreach (DamageArea damageArea in array2)
		{
			damageArea.enabled = enabled;
		}
	}

	public override string GetName()
	{
		return "npcoff";
	}
}
