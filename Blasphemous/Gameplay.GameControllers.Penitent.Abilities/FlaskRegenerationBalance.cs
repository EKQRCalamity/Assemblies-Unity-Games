using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Abilities;

[CreateAssetMenu(fileName = "FlaskRegenerationBalance", menuName = "Blasphemous/Flask Regeneration", order = 0)]
public class FlaskRegenerationBalance : ScriptableObject
{
	[Serializable]
	public struct FlaskRegeneration
	{
		public int flaskLevel;

		public float regenerationTime;
	}

	[SerializeField]
	public List<FlaskRegeneration> regenerationByLevel;

	public float GetTimeByFlaskLevel(int flaskLevel)
	{
		foreach (FlaskRegeneration item in regenerationByLevel)
		{
			if (item.flaskLevel == flaskLevel)
			{
				return item.regenerationTime;
			}
		}
		Debug.LogErrorFormat("Can't find regeneration time for flask level {0}!. Using last in list: ({1})", flaskLevel, regenerationByLevel[regenerationByLevel.Count - 1].regenerationTime);
		return regenerationByLevel[regenerationByLevel.Count - 1].regenerationTime;
	}
}
