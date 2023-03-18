using System;
using Gameplay.GameControllers.Combat;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Enemies.FlyingPortrait.AI;
using Gameplay.GameControllers.Enemies.FlyingPortrait.Animator;
using Gameplay.GameControllers.Enemies.FlyingPortrait.Attack;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.StateMachine;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.FlyingPortrait;

public class FlyingPortrait : Enemy, IDamageable
{
	public StateMachine StateMachine { get; private set; }

	public FlyingPortraitBehaviour Behaviour { get; private set; }

	public FlyingPortraitAnimator AnimatorInjector { get; private set; }

	public FlyingPortraitAttack Attack { get; private set; }

	public EntityMotionChecker MotionChecker { get; private set; }

	public ColorFlash ColorFlash { get; private set; }

	public DamageArea DamageArea { get; private set; }

	public new EntityExecution Execution { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		StateMachine = GetComponent<StateMachine>();
		Behaviour = GetComponentInChildren<FlyingPortraitBehaviour>();
		AnimatorInjector = GetComponentInChildren<FlyingPortraitAnimator>();
		Attack = GetComponentInChildren<FlyingPortraitAttack>();
		MotionChecker = GetComponentInChildren<EntityMotionChecker>();
		ColorFlash = GetComponentInChildren<ColorFlash>();
		DamageArea = GetComponentInChildren<DamageArea>();
		Execution = GetComponentInChildren<EntityExecution>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		DamageArea.DamageAreaCollider.enabled = false;
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
		if (Execution(hit))
		{
			GetStun(hit);
			return;
		}
		DamageArea.TakeDamage(hit);
		if (Status.Dead)
		{
			AnimatorInjector.Death();
			return;
		}
		if (Behaviour.GotParry)
		{
			Behaviour.GotParry = false;
		}
		ColorFlash.TriggerColorFlash();
		SleepTimeByHit(hit);
	}

	public override void Parry()
	{
		base.Parry();
		Behaviour.Parry();
	}

	public override void GetStun(Hit hit)
	{
		base.GetStun(hit);
		if (!base.IsStunt)
		{
			Behaviour.Execution();
		}
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}
}
