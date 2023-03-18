using System;
using System.Collections;
using System.Collections.Generic;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.CommonAttacks;

public class BossEnemySpawn : EnemyAttack
{
	private Coroutine _currentCoroutine;

	[FoldoutGroup("Spawn settings", 0)]
	public GameObject enemyToSpawn;

	public int maxEnemies = 3;

	public List<GameObject> spawnedEnemies;

	public GameObject spawnFX;

	protected override void OnStart()
	{
		base.OnStart();
		if (spawnFX != null)
		{
			PoolManager.Instance.CreatePool(spawnFX, 1);
		}
		spawnedEnemies = new List<GameObject>();
	}

	public override void CurrentWeaponAttack()
	{
		base.CurrentWeaponAttack();
	}

	public void DestroyAllSpawned()
	{
		int count = spawnedEnemies.Count;
		for (int i = 0; i < count; i++)
		{
			spawnedEnemies[i].GetComponent<Enemy>().Kill();
		}
	}

	public Enemy Spawn(Vector2 origin, Vector2 dir, float spawnDelay = 0f, Action callback = null)
	{
		if (spawnedEnemies.Count >= maxEnemies)
		{
			return null;
		}
		if (spawnFX != null)
		{
			PoolManager.Instance.ReuseObject(spawnFX, origin, Quaternion.identity);
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(enemyToSpawn, origin, Quaternion.identity);
		spawnedEnemies.Add(gameObject);
		Enemy component = gameObject.GetComponent<Enemy>();
		component.SetOrientation((!(dir.x > 0f)) ? EntityOrientation.Left : EntityOrientation.Right);
		component.OnEntityDeath += OnEnemyDeath;
		gameObject.SetActive(value: false);
		StartCoroutine(DelayedActivation(gameObject, spawnDelay, callback));
		return component;
	}

	private IEnumerator DelayedActivation(GameObject go, float seconds, Action callback)
	{
		yield return new WaitForSeconds(seconds);
		go.SetActive(value: true);
		callback?.Invoke();
	}

	private void OnEnemyDeath(Entity e)
	{
		e.OnEntityDeath -= OnEnemyDeath;
		spawnedEnemies.Remove(e.gameObject);
	}

	private void DrawDebugCross(Vector2 point, Color c, float seconds)
	{
		float num = 0.6f;
		Debug.DrawLine(point - Vector2.up * num, point + Vector2.up * num, c, seconds);
		Debug.DrawLine(point - Vector2.right * num, point + Vector2.right * num, c, seconds);
	}
}
