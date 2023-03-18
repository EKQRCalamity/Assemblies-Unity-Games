using System;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.ReekLeader.Attack;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.ReekLeader.AI;

public class ReekLeaderBehaviour : EnemyBehaviour
{
	[FoldoutGroup("Attack Settings", true, 0)]
	public float SpawningDistanceToPlayer = 4f;

	[FoldoutGroup("Attack Settings", true, 0)]
	public float DefensiveDistance = 4f;

	[FoldoutGroup("Attack Settings", true, 0)]
	public float SummoningLapse = 5f;

	private float _currentSummoningLapse;

	[FoldoutGroup("Attack Settings", true, 0)]
	public int DefensiveModeMaxReekSummoned = 1;

	[FoldoutGroup("Attack Settings", true, 0)]
	public int OffensiveModeMaxReekSummoned = 3;

	public ReekLeader ReekLeader { get; private set; }

	public ReekSpawner ReekSpawner { get; private set; }

	public bool IsDefensiveMode { get; private set; }

	public override void OnStart()
	{
		base.OnStart();
		ReekLeader = (ReekLeader)Entity;
		ReekSpawner = GetComponentInChildren<ReekSpawner>();
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (!ReekLeader.Status.Dead)
		{
			_currentSummoningLapse += Time.deltaTime;
			float num = Vector2.Distance(base.transform.position, ReekLeader.Target.transform.position);
			if (num <= DefensiveDistance && !IsDefensiveMode)
			{
				IsDefensiveMode = true;
			}
			if (_currentSummoningLapse >= SummoningLapse)
			{
				_currentSummoningLapse = 0f;
				SummonReek();
			}
		}
	}

	public override void Idle()
	{
	}

	public override void Wander()
	{
	}

	public override void Chase(Transform targetPosition)
	{
	}

	public override void Attack()
	{
	}

	public void SummonReek()
	{
		int summonedReekAmount = ReekSpawner.SummonedReekAmount;
		if (IsDefensiveMode)
		{
			if (summonedReekAmount <= DefensiveModeMaxReekSummoned)
			{
				ReekSpawnPoint spawnPoint = GetSpawnPoint();
				if (!(spawnPoint == null))
				{
					spawnPoint.SpawnedEntityId = ReekSpawner.InstanceReek(spawnPoint.transform.position).GetInstanceID();
					ReekLeader.AnimatorInyector.Spawn();
					ReekLeader.Audio.StopIdle();
				}
			}
			else
			{
				_currentSummoningLapse = SummoningLapse - 1f;
			}
		}
		else if (summonedReekAmount < OffensiveModeMaxReekSummoned)
		{
			ReekSpawnPoint spawnPoint2 = GetSpawnPoint();
			if (!(spawnPoint2 == null))
			{
				spawnPoint2.SpawnedEntityId = ReekSpawner.InstanceReek(spawnPoint2.transform.position).GetInstanceID();
				ReekLeader.AnimatorInyector.Spawn();
				ReekLeader.Audio.StopIdle();
			}
		}
		else
		{
			_currentSummoningLapse = SummoningLapse - 1f;
		}
	}

	private ReekSpawnPoint GetSpawnPoint()
	{
		return (!IsDefensiveMode) ? ReekSpawner.GetPlayerClosestReekSpawnPoint() : ReekSpawner.GetNearestReekSpawnPoint();
	}

	public override void Damage()
	{
		if (!(ReekLeader == null))
		{
			ReekLeader.AnimatorInyector.Hurt();
			ReekLeader.Audio.StopIdle();
			ReekLeader.Audio.StopCall();
		}
	}

	public void Death()
	{
		if (!(ReekLeader == null))
		{
			ReekLeader.AnimatorInyector.Death();
		}
	}

	public override void StopMovement()
	{
		throw new NotImplementedException();
	}
}
