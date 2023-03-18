using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.PontiffHusk;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Environment.MovingPlatforms;
using Sirenix.OdinInspector;
using Tools.Level.Layout;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PontiffHuskiesCombatTrigger : MonoBehaviour
{
	[BoxGroup("Camera Settings", true, false, 0)]
	public bool RemovesCameraInfluenceDuringCombat;

	[BoxGroup("Camera Settings", true, false, 0)]
	[HideInInspector]
	public bool InCombat;

	[BoxGroup("Enemies Spawns Settings", true, false, 0)]
	public EnemySpawnPoint[] Enemies;

	[BoxGroup("Moving Platforms To Use", true, false, 0)]
	public List<GameObject> MovingPlatformGameobjects = new List<GameObject>();

	[BoxGroup("Penitent Trigger Mask", true, false, 0)]
	public LayerMask TriggerMask;

	public const string AppearFx = "event:/Key Event/HordeAppear";

	private List<StraightMovingPlatform> movingPlatformSs = new List<StraightMovingPlatform>();

	private List<WaypointsMovingPlatform> movingPlatformWs = new List<WaypointsMovingPlatform>();

	private bool penitentEnteredTrigger;

	private bool allEnemiesDied;

	private List<PontiffHuskRanged> spawnedEnemies = new List<PontiffHuskRanged>();

	private void Awake()
	{
		foreach (GameObject movingPlatformGameobject in MovingPlatformGameobjects)
		{
			StraightMovingPlatform component = movingPlatformGameobject.GetComponent<StraightMovingPlatform>();
			if ((bool)component)
			{
				movingPlatformSs.Add(component);
				continue;
			}
			WaypointsMovingPlatform component2 = movingPlatformGameobject.GetComponent<WaypointsMovingPlatform>();
			if ((bool)component2)
			{
				movingPlatformWs.Add(component2);
			}
			else
			{
				Debug.LogError("Gameobject: " + movingPlatformGameobject.name + " doesn't have a platform!");
			}
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!penitentEnteredTrigger)
		{
			penitentEnteredTrigger = CheckTriggerMask(collision);
			if (penitentEnteredTrigger)
			{
				StartCoroutine(CombatSequence());
			}
		}
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		if (!penitentEnteredTrigger)
		{
			penitentEnteredTrigger = CheckTriggerMask(collision);
			if (penitentEnteredTrigger)
			{
				StartCoroutine(CombatSequence());
			}
		}
	}

	private bool CheckTriggerMask(Collider2D collision)
	{
		return (TriggerMask.value & (1 << collision.gameObject.layer)) > 0;
	}

	[BoxGroup("Friendly Reminder", true, false, 0)]
	[InfoBox("This Button only serves debugging purposes.", InfoMessageType.Info, null)]
	[Button(ButtonSizes.Small)]
	public void ResetState()
	{
		Debug.LogError("ResetState!");
		if (Application.isPlaying)
		{
			spawnedEnemies.ForEach(delegate(PontiffHuskRanged x)
			{
				x.Behaviour.ResetState();
			});
			spawnedEnemies.Clear();
			movingPlatformSs.ForEach(delegate(StraightMovingPlatform x)
			{
				x.Reset();
			});
			movingPlatformWs.ForEach(delegate(WaypointsMovingPlatform x)
			{
				x.ResetPlatform();
			});
			penitentEnteredTrigger = false;
			allEnemiesDied = false;
			InCombat = false;
		}
	}

	private IEnumerator CombatSequence()
	{
		InCombat = true;
		EnemySpawnPoint[] shuffleArray = ShuffleEnemiesArray();
		foreach (EnemySpawnPoint curSpawnPoint in shuffleArray)
		{
			if (curSpawnPoint == null)
			{
				continue;
			}
			yield return new WaitForSeconds(GetSpawnPointDelayInstantiation(curSpawnPoint));
			if ((bool)curSpawnPoint.SpawnedEnemy)
			{
				if (curSpawnPoint.SpawnOnArena)
				{
					curSpawnPoint.SpawnEnemyOnArena();
					Core.Audio.PlaySfx("event:/Key Event/HordeAppear");
				}
				else if (!curSpawnPoint.SpawnEnabledEnemy)
				{
					curSpawnPoint.SpawnedEnemy.gameObject.SetActive(value: true);
				}
				spawnedEnemies.Add(curSpawnPoint.SpawnedEnemy as PontiffHuskRanged);
			}
		}
		SetEnemiesLife();
		SuscribeToEnemiesDeaths();
		yield return new WaitUntil(() => allEnemiesDied);
		movingPlatformSs.ForEach(delegate(StraightMovingPlatform x)
		{
			x.Use();
		});
		movingPlatformWs.ForEach(delegate(WaypointsMovingPlatform x)
		{
			x.Use();
		});
		yield return new WaitForSeconds(1f);
		InCombat = false;
	}

	private EnemySpawnPoint[] ShuffleEnemiesArray()
	{
		EnemySpawnPoint[] enemies = Enemies;
		System.Random rnd = new System.Random();
		return enemies.OrderBy((EnemySpawnPoint x) => rnd.Next()).ToArray();
	}

	private void SetEnemiesLife()
	{
		foreach (PontiffHuskRanged spawnedEnemy in spawnedEnemies)
		{
			spawnedEnemy.Status.Dead = false;
			spawnedEnemy.Stats.Life.Current = spawnedEnemy.Stats.LifeBase;
		}
	}

	private void SuscribeToEnemiesDeaths()
	{
		foreach (PontiffHuskRanged spawnedEnemy in spawnedEnemies)
		{
			spawnedEnemy.OnEntityDeath += CheckForAliveEnemies;
		}
	}

	private void CheckForAliveEnemies(Entity lastDeadEnemy)
	{
		lastDeadEnemy.OnEntityDeath -= CheckForAliveEnemies;
		PontiffHuskRanged item = lastDeadEnemy as PontiffHuskRanged;
		spawnedEnemies.Remove(item);
		allEnemiesDied = spawnedEnemies.Count == 0;
	}

	private float GetSpawnPointDelayInstantiation(EnemySpawnPoint spawnPoint)
	{
		float result = 0f;
		if (spawnPoint.SpawnEnabledEnemy || spawnPoint.SpawnOnArena)
		{
			result = UnityEngine.Random.Range(0.1f, 0.5f);
		}
		return result;
	}
}
