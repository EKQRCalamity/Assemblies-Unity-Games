using System;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Enemies.TrinityMinion.AI;
using Gameplay.GameControllers.Enemies.TrinityMinion.Animator;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.TrinityMinion;

public class TrinityMinion : Enemy, IDamageable
{
	public TrinityMinionBehaviour Behaviour { get; private set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public TrinityMinionAnimatorInyector AnimatorInyector { get; private set; }

	public BoxCollider2D Collider { get; private set; }

	protected override void OnStart()
	{
		base.OnStart();
		Behaviour = GetComponent<TrinityMinionBehaviour>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		AnimatorInyector = GetComponentInChildren<TrinityMinionAnimatorInyector>();
		Collider = GetComponent<BoxCollider2D>();
	}

	public override EnemyFloorChecker EnemyFloorChecker()
	{
		throw new NotImplementedException();
	}

	public override EnemyAttack EnemyAttack()
	{
		throw new NotImplementedException();
	}

	public override EnemyBumper EnemyBumper()
	{
		throw new NotImplementedException();
	}

	protected override void EnablePhysics(bool enable = true)
	{
		throw new NotImplementedException();
	}

	public void Damage(Hit hit)
	{
		DamageArea.TakeDamage(hit);
		if (Status.Dead)
		{
			Behaviour.Death();
		}
		else
		{
			Behaviour.Damage();
		}
		SleepTimeByHit(hit);
	}

	public Vector3 GetPosition()
	{
		throw new NotImplementedException();
	}
}
