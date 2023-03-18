using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Gameplay.GameControllers.Enemies.Framework.IA;

[Serializable]
public class SpawnBehaviourConfig
{
	[FoldoutGroup("Spawn config", 0)]
	public bool dontWalk;

	[FoldoutGroup("Spawn config", 0)]
	public List<SpawnBehaviorFloatParam> floatParams;

	public float TryGetFloat(string n)
	{
		float result = -1f;
		if (floatParams != null)
		{
			result = floatParams.Find((SpawnBehaviorFloatParam x) => x.name == n).value;
		}
		return result;
	}
}
