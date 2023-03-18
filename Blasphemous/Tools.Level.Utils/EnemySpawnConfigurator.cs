using Framework.FrameworkCore;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Entities;
using Tools.Level.Layout;
using UnityEngine;

namespace Tools.Level.Utils;

public class EnemySpawnConfigurator : MonoBehaviour
{
	private EnemySpawnPoint spawnPoint;

	public SpawnBehaviourConfig configPackage;

	public bool facingLeft;

	private void Awake()
	{
		spawnPoint = GetComponent<EnemySpawnPoint>();
		spawnPoint.OnEnemySpawned += OnEnemySpawn;
		OnAwake();
	}

	private void OnEnemySpawn(EnemySpawnPoint sp, Enemy e)
	{
		if (facingLeft)
		{
			e.SetOrientation(EntityOrientation.Left);
		}
		e.GetComponent<EnemyBehaviour>().ReadSpawnerConfig(configPackage);
		OnSpawn(e);
	}

	protected virtual void OnAwake()
	{
	}

	protected virtual void OnSpawn(Enemy e)
	{
	}

	protected void OnDestroy()
	{
		spawnPoint.OnEnemySpawned -= OnEnemySpawn;
	}
}
