using System;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.ViciousDasher.AI;

public class ViciousDasherBehaviour : EnemyBehaviour
{
	public float DashDistance;

	public float CloseRange = 2f;

	public float AttackTime = 3f;

	private float _parryRecoverTime;

	private bool _targetIsDead;

	private static readonly int IsParried = UnityEngine.Animator.StringToHash("IS_PARRIED");

	protected ViciousDasher ViciousDasher { get; set; }

	public bool IsDashBlocked { get; private set; }

	public bool CanBeExecuted { get; private set; }

	public override void OnAwake()
	{
		base.OnAwake();
		ViciousDasher = (ViciousDasher)Entity;
	}

	public override void OnStart()
	{
		base.OnStart();
		_parryRecoverTime = 0f;
		Gameplay.GameControllers.Penitent.Penitent penitent = Core.Logic.Penitent;
		penitent.OnDead = (Core.SimpleEvent)Delegate.Combine(penitent.OnDead, new Core.SimpleEvent(OnPlayerDead));
	}

	private void LateUpdate()
	{
		IsDashBlocked = !ViciousDasher.MotionChecker.HitsFloor || ViciousDasher.MotionChecker.HitsBlock;
		if (IsDashBlocked)
		{
			ViciousDasher.MotionLerper.StopLerping();
		}
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		_parryRecoverTime -= Time.deltaTime;
		if (ViciousDasher.Target == null || ViciousDasher.IsAttacking)
		{
			return;
		}
		if (ViciousDasher.Status.Dead || base.GotParry || _parryRecoverTime > 0f)
		{
			ViciousDasher.StateMachine.SwitchState<ViciousDasherDeathState>();
		}
		else if (ViciousDasher.IsTargetVisible)
		{
			LookAtTarget(ViciousDasher.Target.transform.position);
			if (ViciousDasher.DistanceToTarget < DashDistance && !_targetIsDead)
			{
				ViciousDasher.StateMachine.SwitchState<ViciousDasherAttackState>();
			}
		}
		else
		{
			ViciousDasher.StateMachine.SwitchState<ViciousDasherIdleState>();
		}
	}

	public void Dash()
	{
		if (!ViciousDasher.MotionLerper.IsLerping && !IsDashBlocked)
		{
			ViciousDasher.MotionLerper.distanceToMove = ViciousDasher.DistanceToTarget - 1f;
			Vector2 vector = ((ViciousDasher.Status.Orientation != 0) ? Vector2.left : Vector2.right);
			ViciousDasher.MotionLerper.StartLerping(vector);
		}
	}

	private void OnPlayerDead()
	{
		_targetIsDead = true;
	}

	public void ResetParryRecover()
	{
		_parryRecoverTime = 1f;
	}

	public override void Parry()
	{
		base.Parry();
		base.GotParry = true;
		ViciousDasher.IsAttacking = false;
		ViciousDasher.AnimatorInjector.IsParried(isParried: true);
	}

	public override void Execution()
	{
		base.Execution();
		base.GotParry = true;
		CanBeExecuted = true;
		StopMovement();
		ViciousDasher.AnimatorInjector.StopAttack();
		ViciousDasher.gameObject.layer = LayerMask.NameToLayer("Default");
		ViciousDasher.SpriteRenderer.enabled = false;
		Core.Logic.Penitent.Audio.PlaySimpleHitToEnemy();
		ViciousDasher.EntExecution.InstantiateExecution();
		if (ViciousDasher.EntExecution != null)
		{
			ViciousDasher.EntExecution.enabled = true;
		}
	}

	public override void Alive()
	{
		base.Alive();
		ViciousDasher.gameObject.layer = LayerMask.NameToLayer("Enemy");
		base.GotParry = false;
		CanBeExecuted = false;
		ViciousDasher.SpriteRenderer.enabled = true;
		ViciousDasher.Animator.SetBool(IsParried, value: false);
		ViciousDasher.Animator.Play("Idle");
		ViciousDasher.CurrentLife = ViciousDasher.Stats.Life.Base / 2f;
		if (ViciousDasher.EntExecution != null)
		{
			ViciousDasher.EntExecution.enabled = false;
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
		throw new NotImplementedException();
	}

	public override void StopMovement()
	{
		if (ViciousDasher.MotionLerper.IsLerping)
		{
			ViciousDasher.MotionLerper.StopLerping();
		}
	}
}
