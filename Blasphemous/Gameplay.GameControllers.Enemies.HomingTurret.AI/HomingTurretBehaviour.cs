using System;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.HomingTurret.Attack;
using Gameplay.GameControllers.Entities.StateMachine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.HomingTurret.AI;

public class HomingTurretBehaviour : EnemyBehaviour
{
	private enum TurretStates
	{
		Idle,
		Attack,
		Dead
	}

	[FoldoutGroup("Attack Settings", 0)]
	public float DetectionTime;

	private float currentDetectionTime;

	[FoldoutGroup("Attack Settings", 0)]
	public float ReadyAttackTime;

	[FoldoutGroup("Attack Settings", 0)]
	public float AttackCooldown;

	private TurretStates CurrentState;

	private int hitStunCounter;

	public int maxHitsInHitstun = 3;

	private HomingTurret homingTurret { get; set; }

	public HomingTurretAttack TurretAttack { get; set; }

	private StateMachine StateMachine { get; set; }

	private bool CanSeeTarget
	{
		get
		{
			if (!Core.Logic.Penitent)
			{
				return false;
			}
			return homingTurret.Vision.CanSeeTarget(Core.Logic.Penitent.transform, "Penitent");
		}
	}

	public override void OnAwake()
	{
		base.OnAwake();
		StateMachine = GetComponent<StateMachine>();
		TurretAttack = StateMachine.GetComponentInChildren<HomingTurretAttack>();
	}

	public override void OnStart()
	{
		base.OnStart();
		homingTurret = (HomingTurret)Entity;
		SwitchToState(TurretStates.Idle);
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		switch (CurrentState)
		{
		case TurretStates.Idle:
			Idle();
			break;
		case TurretStates.Attack:
			Attack();
			break;
		case TurretStates.Dead:
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public override void Idle()
	{
		if (CanSeeTarget)
		{
			currentDetectionTime += Time.deltaTime;
			if (currentDetectionTime >= DetectionTime)
			{
				SwitchToState(TurretStates.Attack);
			}
		}
		else
		{
			currentDetectionTime = 0f;
		}
		if (Entity.Status.Dead)
		{
			SwitchToState(TurretStates.Dead);
		}
	}

	public void ChargeAttack()
	{
		homingTurret.AnimationInyector.ChargeAttack();
	}

	public void ReleaseAttack()
	{
		homingTurret.AnimationInyector.ReleaseAttack();
		hitStunCounter = 0;
	}

	public override void Attack()
	{
		currentDetectionTime = 0f;
		if (!CanSeeTarget)
		{
			SwitchToState(TurretStates.Idle);
		}
		if (Entity.Status.Dead)
		{
			SwitchToState(TurretStates.Dead);
		}
	}

	public void Dead()
	{
		homingTurret.AnimationInyector.Death();
	}

	public void Spawn()
	{
		homingTurret.SetOrientation((GetOrientationToTarget() != 1) ? EntityOrientation.Left : EntityOrientation.Right);
		homingTurret.AnimationInyector.Spawn();
	}

	private int GetOrientationToTarget()
	{
		if (!Core.Logic.Penitent)
		{
			return 0;
		}
		return (int)Mathf.Sign((Core.Logic.Penitent.transform.position - base.transform.position).x);
	}

	private void SwitchToState(TurretStates targetState)
	{
		CurrentState = targetState;
		switch (targetState)
		{
		case TurretStates.Idle:
			StateMachine.SwitchState<HomingTurretIdleState>();
			break;
		case TurretStates.Attack:
			StateMachine.SwitchState<HomingTurretAttackState>();
			break;
		case TurretStates.Dead:
			StateMachine.SwitchState<HomingTurretDeadState>();
			break;
		}
	}

	public override void Damage()
	{
		if (hitStunCounter < maxHitsInHitstun)
		{
			hitStunCounter++;
			homingTurret.AnimationInyector.Damage();
		}
	}

	public override void Wander()
	{
	}

	public override void Chase(Transform targetPosition)
	{
	}

	public override void StopMovement()
	{
	}
}
