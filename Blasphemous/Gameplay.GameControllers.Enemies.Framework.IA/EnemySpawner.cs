using System.Collections.Generic;
using System.Linq;
using Framework.FrameworkCore;
using Framework.Managers;
using Tools.Level.Layout;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Framework.IA;

public class EnemySpawner
{
	private EnemySpawnPoint[] _enemySpawnPoints;

	private readonly List<Vector3> consumedSpawns = new List<Vector3>();

	public event Core.SimpleEvent OnEnemiesRespawn;

	public event Core.SimpleEvent OnConsumedSpanwPoint;

	public void SpawnEnemiesOnLoad()
	{
		Log.Trace("Spawn", "Spawning enemies on level.");
		EnemySpawnPoint[] array = Object.FindObjectsOfType<EnemySpawnPoint>();
		EnemySpawnPoint[] array2 = array;
		foreach (EnemySpawnPoint enemySpawnPoint in array2)
		{
			enemySpawnPoint.CreateEnemy();
		}
	}

	public void RespawnDeadEnemies()
	{
		Log.Trace("Spawn", "Respawning dead enemies on level.");
		EnemySpawnPoint[] array = Object.FindObjectsOfType<EnemySpawnPoint>();
		consumedSpawns.Clear();
		EnemySpawnPoint[] array2 = array;
		foreach (EnemySpawnPoint enemySpawnPoint in array2)
		{
			enemySpawnPoint.CreateEnemy();
		}
		if (this.OnEnemiesRespawn != null)
		{
			this.OnEnemiesRespawn();
		}
	}

	public void Reset()
	{
		consumedSpawns.Clear();
	}

	public void AddConsumedSpawner(EnemySpawnPoint spawnPoint)
	{
		if (!(spawnPoint == null) && !consumedSpawns.Contains(spawnPoint.transform.position))
		{
			consumedSpawns.Add(spawnPoint.transform.position);
			if (this.OnConsumedSpanwPoint != null)
			{
				this.OnConsumedSpanwPoint();
			}
		}
	}

	public bool IsSpawnerConsumed(string spawnPointName)
	{
		EnemySpawnPoint[] source = Object.FindObjectsOfType<EnemySpawnPoint>();
		return IsSpawnerConsumed(source.First((EnemySpawnPoint p) => p.gameObject.name == spawnPointName));
	}

	public bool IsSpawnerConsumed(EnemySpawnPoint spawnPoint)
	{
		return spawnPoint != null && consumedSpawns.Contains(spawnPoint.transform.position);
	}

	public bool AreAllSpawnersConsumed()
	{
		EnemySpawnPoint[] source = Object.FindObjectsOfType<EnemySpawnPoint>();
		return source.All((EnemySpawnPoint p) => IsSpawnerConsumed(p));
	}

	public bool IsAnySpawnerLeft()
	{
		EnemySpawnPoint[] source = Object.FindObjectsOfType<EnemySpawnPoint>();
		return source.Any((EnemySpawnPoint p) => !IsSpawnerConsumed(p));
	}
}
