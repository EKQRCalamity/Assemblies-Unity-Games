using System;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.WallEnemy.Attack;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.WallEnemy.AI;

public class WallEnemyBehaviour : EnemyBehaviour
{
	private float _currentAttackLapse;

	[FoldoutGroup("Attack Settings", true, 0)]
	public float AttackCooldown = 2f;

	[FoldoutGroup("Attack Settings", true, 0)]
	public float AttackDelay = 2f;

	public CollisionSensor CollisionSensor { get; private set; }

	public EnemyAttack WallEnemyAttack { get; protected set; }

	public override void OnStart()
	{
		base.OnStart();
		CollisionSensor = Entity.GetComponentInChildren<CollisionSensor>();
		CollisionSensor.SensorTriggerEnter += EnemyOnRange;
		CollisionSensor.SensorTriggerExit += EnemyOutOfRange;
		WallEnemyAttack = Entity.GetComponentInChildren<WallEnemyAttack>();
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (!Entity.Status.Dead)
		{
			_currentAttackLapse += Time.deltaTime;
			if (Entity.Animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
			{
				_currentAttackLapse = 0f;
			}
			if (base.PlayerSeen && _currentAttackLapse >= AttackCooldown)
			{
				_currentAttackLapse = 0f;
				Attack();
			}
		}
	}

	private void OnDestroy()
	{
		CollisionSensor.SensorTriggerEnter -= EnemyOnRange;
		CollisionSensor.SensorTriggerExit -= EnemyOutOfRange;
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
		Entity.Animator.SetTrigger("ATTACK");
	}

	public override void Damage()
	{
		if (Entity.Status.Dead)
		{
			Entity.Animator.SetTrigger("DEATH");
		}
	}

	public override void StopMovement()
	{
		throw new NotImplementedException();
	}

	private void EnemyOnRange(Collider2D objectcollider)
	{
		if (!base.PlayerSeen)
		{
			base.PlayerSeen = true;
		}
	}

	private void EnemyOutOfRange(Collider2D objectcollider)
	{
		if (base.PlayerSeen)
		{
			base.PlayerSeen = !base.PlayerSeen;
		}
	}
}
