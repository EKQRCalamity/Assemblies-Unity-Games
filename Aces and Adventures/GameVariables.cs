using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Preferences/GameVariables")]
public class GameVariables : ScriptableObject
{
	[Serializable]
	public class Main
	{
		[Range(0.5f, 2f)]
		public float enemyExperienceMultiplier = 1f;
	}

	[Serializable]
	public class LevelUpVariables
	{
		[Range(1f, 5f)]
		public float vialToPlantPourTime = 3f;

		[Range(1f, 5f)]
		public float fillVialTime = 3f;
	}

	private static ResourceBlueprint<GameVariables> _Values = "GameState/GameVariables";

	public Main main;

	public LevelUpVariables levelUp;

	public static GameVariables Values => _Values;
}
