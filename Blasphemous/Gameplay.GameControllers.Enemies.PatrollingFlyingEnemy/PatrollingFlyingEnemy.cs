using System;
using BezierSplines;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Enemies.PatrollingFlyingEnemy.AI;
using Gameplay.GameControllers.Enemies.PatrollingFlyingEnemy.Animator;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.PatrollingFlyingEnemy;

public class PatrollingFlyingEnemy : Enemy, IDamageable
{
	public PatrollingFlyingEnemyBehaviour Behaviour { get; private set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public PatrollingFlyingEnemyAnimatorInyector AnimatorInyector { get; private set; }

	protected override void OnStart()
	{
		base.OnStart();
		Behaviour = GetComponent<PatrollingFlyingEnemyBehaviour>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		AnimatorInyector = GetComponentInChildren<PatrollingFlyingEnemyAnimatorInyector>();
	}

	public void SetConfig(BezierSpline spline, AnimationCurve curve, float secondsToFoolLoop)
	{
		Behaviour = GetComponent<PatrollingFlyingEnemyBehaviour>();
		Behaviour.SetPath(spline);
		Behaviour.currentCurve = curve;
		Behaviour.secondsToFullLoop = secondsToFoolLoop;
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
