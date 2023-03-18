using BezierSplines;
using Gameplay.GameControllers.Enemies.PatrollingFlyingEnemy;
using Gameplay.GameControllers.Entities;
using Tools.Level.Layout;
using UnityEngine;

namespace Tools.Level.Utils;

public class CherubCaptorSpawnConfigurator : EnemySpawnConfigurator
{
	public BezierSpline path;

	public AnimationCurve curve;

	public CherubCaptorPersistentObject cherubPersistentObject;

	public float secondsToCompletePatrol = 2f;

	private Enemy cherub;

	protected override void OnAwake()
	{
		base.OnAwake();
	}

	public void DisableCherubSpawn()
	{
		Debug.Log($"<color=red>CHERUB OF ID:{cherubPersistentObject.cherubId} already destroyed. Won't spawn.</color>");
		EnemySpawnPoint component = GetComponent<EnemySpawnPoint>();
		component.EnemySpawnDisabled = true;
	}

	public void DestroySpawnedCherub()
	{
		EnemySpawnPoint component = GetComponent<EnemySpawnPoint>();
		if (component.HasEnemySpawned)
		{
			Object.Destroy(cherub.gameObject);
		}
	}

	protected override void OnSpawn(Enemy e)
	{
		base.OnSpawn(e);
		Debug.Log($"<color=red>SPAWNING CHERUB OF ID:{cherubPersistentObject.cherubId} </color>");
		(e as PatrollingFlyingEnemy).SetConfig(path, curve, secondsToCompletePatrol);
		e.OnDeath += OnCherubDeath;
		cherub = e;
	}

	private void OnCherubDeath()
	{
		Debug.Log($"<color=red>CHERUB OF ID:{cherubPersistentObject.cherubId} DIED</color>");
		cherubPersistentObject.OnCherubKilled();
	}
}
