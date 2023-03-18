using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Level.Actionables;

public class TriggerTrapManager : MonoBehaviour
{
	public List<TriggerBasedTrap> traps;

	[SerializeField]
	[FoldoutGroup("Trap configuration", 0)]
	private float firstTrapLapse = 2f;

	[SerializeField]
	[FoldoutGroup("Trap configuration", 0)]
	private float loopTrapLapse = 5f;

	[Button(ButtonSizes.Small)]
	public void LinkToSceneTraps()
	{
		traps = new List<TriggerBasedTrap>(Object.FindObjectsOfType<TriggerBasedTrap>());
	}

	public float GetSceneTrapLapse()
	{
		return loopTrapLapse;
	}

	public float GetFirstTrapLapse()
	{
		return firstTrapLapse;
	}

	public void Trigger(string id)
	{
		List<TriggerBasedTrap> list = traps.FindAll((TriggerBasedTrap x) => x.triggerID == id);
		foreach (TriggerBasedTrap item in list)
		{
			item.Use();
		}
	}
}
