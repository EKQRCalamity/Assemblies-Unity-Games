using System.Collections.Generic;
using System.Linq;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.ExplodingEnemy;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.ReekLeader.Attack;

public class ReekSpawner : Trait
{
	public GameObject Reek;

	public List<ReekSpawnPoint> ReekSpawnPoints = new List<ReekSpawnPoint>();

	private List<GameObject> _activedReeks = new List<GameObject>();

	public int SummonedReekAmount => _activedReeks.Count((GameObject x) => x.activeSelf);

	protected override void OnStart()
	{
		base.OnStart();
		SetSpawnPoints();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
	}

	public GameObject InstanceReek(Vector3 position)
	{
		if (!Reek)
		{
			return null;
		}
		GameObject gameObject = Object.Instantiate(Reek, position, Quaternion.identity);
		Gameplay.GameControllers.Enemies.ExplodingEnemy.ExplodingEnemy componentInChildren = gameObject.GetComponentInChildren<Gameplay.GameControllers.Enemies.ExplodingEnemy.ExplodingEnemy>();
		SetStatsByGameMode(componentInChildren);
		componentInChildren.IsSummoned = true;
		componentInChildren.ReekLeader = (ReekLeader)base.EntityOwner;
		_activedReeks.Add(gameObject);
		return gameObject;
	}

	public void DisposeReek(GameObject reekGo)
	{
		Gameplay.GameControllers.Enemies.ExplodingEnemy.ExplodingEnemy componentInChildren = reekGo.GetComponentInChildren<Gameplay.GameControllers.Enemies.ExplodingEnemy.ExplodingEnemy>();
		if ((bool)componentInChildren)
		{
			_activedReeks.Remove(reekGo);
			componentInChildren.Destroy();
		}
	}

	private static void SetStatsByGameMode(Enemy enemy)
	{
		Core.Logic.CurrentLevelConfig.EnemyStatsImporter?.SetEnemyStats(enemy);
	}

	private void SetSpawnPoints()
	{
		ReekSpawnPoint[] array = Object.FindObjectsOfType<ReekSpawnPoint>();
		ReekSpawnPoint[] array2 = array;
		foreach (ReekSpawnPoint item in array2)
		{
			ReekSpawnPoints.Add(item);
		}
	}

	public ReekSpawnPoint GetNearestReekSpawnPoint()
	{
		Vector3 position = base.transform.position;
		return GetNearestReekSpawnPoint(position);
	}

	public ReekSpawnPoint GetPlayerClosestReekSpawnPoint()
	{
		Vector3 position = Core.Logic.Penitent.transform.position;
		return GetNearestReekSpawnPoint(position);
	}

	private ReekSpawnPoint GetNearestReekSpawnPoint(Vector3 position)
	{
		ReekSpawnPoint result = null;
		float num = float.PositiveInfinity;
		foreach (ReekSpawnPoint reekSpawnPoint in ReekSpawnPoints)
		{
			float num2 = Vector3.Distance(reekSpawnPoint.transform.position, position);
			if (num2 < num && reekSpawnPoint.SpawnedEntityId == 0)
			{
				result = reekSpawnPoint;
				num = num2;
			}
		}
		return result;
	}

	public void ResetSpawnPoint(int id)
	{
		ReekSpawnPoint reekSpawnPoint = ReekSpawnPoints.First((ReekSpawnPoint x) => x.SpawnedEntityId == id);
		if (reekSpawnPoint != null)
		{
			reekSpawnPoint.SpawnedEntityId = 0;
		}
	}
}
