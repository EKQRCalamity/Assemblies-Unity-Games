using System;
using System.Collections.Generic;
using Gameplay.GameControllers.Enemies.ChasingHead;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.HeadThrower.Animator;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.HeadThrower.AI;

public class HeadThrowerBehaviour : EnemyBehaviour
{
	private readonly List<GameObject> _spawnedHeadList = new List<GameObject>();

	private float _currentSpawningLapse;

	private float _distanceToTarget;

	private HeadThrower _headThrower;

	[FoldoutGroup("Spawning Settings", true, 0)]
	public GameObject HeadPrefab;

	[FoldoutGroup("Spawning Settings", true, 0)]
	public float MaxHeadSpawned = 3f;

	[FoldoutGroup("Spawning Settings", true, 0)]
	public float MinDistanceBeforeSpawn = 16f;

	[FoldoutGroup("Spawning Settings", true, 0)]
	public float SpawningInterval = 3f;

	[FoldoutGroup("Spawning Settings", true, 0)]
	public EnemyRootPoint SpawningRootPoint;

	public HeadThrowerAnimatorInyector AnimatorInyector { get; private set; }

	public int SpawnedHeadAmount => _spawnedHeadList.Count;

	public override void OnStart()
	{
		base.OnStart();
		_headThrower = (HeadThrower)Entity;
		AnimatorInyector = _headThrower.GetComponentInChildren<HeadThrowerAnimatorInyector>();
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (!(_headThrower.Target == null))
		{
			float num = Vector2.Distance(_headThrower.transform.position, _headThrower.Target.transform.position);
			_currentSpawningLapse += Time.deltaTime;
			if (num <= MinDistanceBeforeSpawn && _currentSpawningLapse >= SpawningInterval && (float)SpawnedHeadAmount < MaxHeadSpawned)
			{
				_currentSpawningLapse = 0f;
				SpawnHead();
			}
		}
	}

	public override void Idle()
	{
		throw new NotImplementedException();
	}

	public override void Wander()
	{
		throw new NotImplementedException();
	}

	public override void Chase(Transform targetPosition)
	{
		throw new NotImplementedException();
	}

	public override void Attack()
	{
		throw new NotImplementedException();
	}

	public override void Damage()
	{
		_currentSpawningLapse = 0f;
		if (_headThrower.Status.Dead)
		{
			AnimatorInyector.Death();
		}
		else
		{
			AnimatorInyector.Damage();
		}
	}

	public override void StopMovement()
	{
		throw new NotImplementedException();
	}

	public void SpawnHead()
	{
		if (!(HeadPrefab == null))
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(HeadPrefab, SpawningRootPoint.transform.position, Quaternion.identity);
			if (gameObject != null)
			{
				AddSpawnedHeadToList(gameObject);
				Gameplay.GameControllers.Enemies.ChasingHead.ChasingHead component = gameObject.GetComponent<Gameplay.GameControllers.Enemies.ChasingHead.ChasingHead>();
				component.SetTarget(_headThrower.Target);
				component.OwnHeadThrower = _headThrower;
			}
		}
	}

	public void RemoveSpawnedHeadFromList(GameObject spawnedHead)
	{
		if (_spawnedHeadList.Contains(spawnedHead))
		{
			_spawnedHeadList.Remove(spawnedHead);
		}
	}

	public void AddSpawnedHeadToList(GameObject spawnedHead)
	{
		if (!_spawnedHeadList.Contains(spawnedHead))
		{
			_spawnedHeadList.Add(spawnedHead);
		}
	}
}
