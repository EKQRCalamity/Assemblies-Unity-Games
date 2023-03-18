using System.Collections.Generic;
using System.Linq;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Audio;

public class SceneAudioModifer : MonoBehaviour
{
	private enum CheckEnemies
	{
		DONT_CHECK,
		ANY_IN_SCENE,
		IN_SPANW_POINTS
	}

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	private AudioParamInitialized[] parameters = new AudioParamInitialized[0];

	[SerializeField]
	[BoxGroup("Global", true, false, 0)]
	private bool changeReverb;

	[SerializeField]
	[BoxGroup("Global", true, false, 0)]
	[EventRef]
	[ShowIf("changeReverb", true)]
	private string idReverb;

	[SerializeField]
	[BoxGroup("Checks", true, false, 0)]
	private CheckEnemies checkEnemies;

	[SerializeField]
	[ShowIf("ShowSpawnList", true)]
	[BoxGroup("Checks", true, false, 0)]
	private List<string> SpawnPoints = new List<string>();

	private bool isActive;

	private bool ShowSpawnList()
	{
		return checkEnemies == CheckEnemies.IN_SPANW_POINTS;
	}

	private void Awake()
	{
		LevelManager.OnLevelLoaded += OnLevelLoaded;
	}

	private void OnDestroy()
	{
		Core.Logic.EnemySpawner.OnConsumedSpanwPoint -= OnConsumedSpanwPoint;
		LevelManager.OnLevelLoaded -= OnLevelLoaded;
	}

	public void OnLevelLoaded(Framework.FrameworkCore.Level oldLevel, Framework.FrameworkCore.Level newLevel)
	{
		Core.Logic.EnemySpawner.OnConsumedSpanwPoint += OnConsumedSpanwPoint;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Penitent") && CheckCondition())
		{
			isActive = true;
			string newReverb = ((!changeReverb) ? string.Empty : idReverb);
			Core.Audio.Ambient.StartModifierParams(base.name, newReverb, parameters);
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.CompareTag("Penitent"))
		{
			StopModifier();
		}
	}

	private void OnConsumedSpanwPoint()
	{
		if (isActive && checkEnemies != 0 && !CheckCondition())
		{
			StopModifier();
		}
	}

	private bool CheckCondition()
	{
		bool result = true;
		switch (checkEnemies)
		{
		case CheckEnemies.ANY_IN_SCENE:
			result = Core.Logic.EnemySpawner.IsAnySpawnerLeft();
			break;
		case CheckEnemies.IN_SPANW_POINTS:
			result = SpawnPoints.Any((string p) => !Core.Logic.EnemySpawner.IsSpawnerConsumed(p));
			break;
		}
		return result;
	}

	private void StopModifier()
	{
		if (isActive)
		{
			Core.Audio.Ambient.StopModifierParams(base.name);
			isActive = false;
		}
	}
}
