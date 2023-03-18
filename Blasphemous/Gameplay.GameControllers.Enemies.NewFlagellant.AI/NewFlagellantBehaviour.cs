using System;
using DG.Tweening;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.NewFlagellant.AI;

public class NewFlagellantBehaviour : EnemyBehaviour
{
	public float DashDistance;

	public float CloseRange = 2f;

	public float attackCooldown = 3f;

	private float _parryRecoverTime;

	private bool _targetIsDead;

	private float _attackCD;

	private float _waitTimer;

	public bool flinchDuringAttacks = true;

	public float maxParryTime = 2f;

	public float maxPlayerRememberTime = 2f;

	private float rememberTime;

	protected NewFlagellant NewFlagellant { get; set; }

	public bool IsDashBlocked { get; private set; }

	public float _currentHurtTime { get; private set; }

	public void UpdateHurtTime()
	{
		_currentHurtTime += Time.deltaTime;
	}

	public void ResetHurtTime()
	{
		_currentHurtTime = 0f;
	}

	private void UpdateRememberTime()
	{
		if (rememberTime > 0f)
		{
			rememberTime -= Time.deltaTime;
		}
	}

	public void ResetRememberTime()
	{
		rememberTime = maxPlayerRememberTime;
	}

	public bool StillRemembersPlayer()
	{
		return rememberTime > 0f;
	}

	public override void OnAwake()
	{
		base.OnAwake();
		NewFlagellant = (NewFlagellant)Entity;
	}

	public override void OnStart()
	{
		base.OnStart();
		_parryRecoverTime = 0f;
		Gameplay.GameControllers.Penitent.Penitent penitent = Core.Logic.Penitent;
		penitent.OnDead = (Core.SimpleEvent)Delegate.Combine(penitent.OnDead, new Core.SimpleEvent(OnPlayerDead));
		NewFlagellant.StateMachine.SwitchState<NewFlagellantIdleState>();
	}

	public bool CanSeeTarget()
	{
		return NewFlagellant.Target != null && CanSeeTarget(NewFlagellant.Target.transform);
	}

	public bool CanSeeTarget(Transform t)
	{
		return NewFlagellant.VisionCone.CanSeeTarget(t, "Penitent");
	}

	public bool CanReachPlayer()
	{
		return NewFlagellant.Target != null && CanReachPosition(NewFlagellant.Target.transform.position);
	}

	public bool CanReachPosition(Vector2 p)
	{
		int num = 10;
		float num2 = 4f;
		if (Mathf.Abs(p.y - base.transform.position.y) > num2)
		{
			return false;
		}
		Vector2 a = (Vector2)base.transform.position + Vector2.up * 0.25f;
		Vector2 b = p;
		b.y = a.y;
		for (int i = 0; i < num; i++)
		{
			Vector2 pos = Vector2.Lerp(a, b, (float)i / (float)num);
			if (!NewFlagellant.MotionChecker.HitsFloorInPosition(pos, 1f, out var _, show: true))
			{
				return false;
			}
		}
		return true;
	}

	public bool CanAttack()
	{
		return _attackCD <= 0f;
	}

	public bool IsTargetInsideAttackRange()
	{
		return NewFlagellant.DistanceToTarget < 3f;
	}

	private void CheckParryCounter()
	{
		if (_parryRecoverTime > 0f)
		{
			_parryRecoverTime -= Time.deltaTime;
		}
		else if (_parryRecoverTime < 0f)
		{
			_parryRecoverTime = 0f;
			NewFlagellant.AnimatorInyector.IsParried(isParried: false);
			NewFlagellant.StateMachine.SwitchState<NewFlagellantIdleState>();
		}
	}

	public void OnBouncedBackByOverlapping()
	{
	}

	private void LateUpdate()
	{
	}

	public void CheckFall()
	{
		Vector2 hitPoint;
		bool flag = NewFlagellant.MotionChecker.HitsFloorInPosition(base.transform.position + Vector3.up * 0.2f + Vector3.right * 0.5f, 1f, out hitPoint);
		if (!NewFlagellant.MotionChecker.HitsFloorInPosition(base.transform.position + Vector3.up * 0.2f - Vector3.right * 0.5f, 1f, out hitPoint) && !flag)
		{
			NewFlagellant.StateMachine.SwitchState<NewFlagellantFallingState>();
		}
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		CheckParryCounter();
		UpdateRememberTime();
		if (_waitTimer > 0f)
		{
			_waitTimer -= Time.deltaTime;
		}
		if (_attackCD > 0f)
		{
			_attackCD -= Time.deltaTime;
		}
		if (!(NewFlagellant.Target == null) && !NewFlagellant.IsAttacking && (NewFlagellant.Status.Dead || base.GotParry || _parryRecoverTime > 0f))
		{
			NewFlagellant.StateMachine.SwitchState<NewFlagellantDeathState>();
		}
	}

	public void ResetCooldown()
	{
		_attackCD = attackCooldown;
	}

	public void Dash()
	{
		if (!NewFlagellant.MotionLerper.IsLerping && !IsDashBlocked)
		{
			NewFlagellant.MotionLerper.distanceToMove = NewFlagellant.DistanceToTarget - 1f;
			Vector2 vector = ((NewFlagellant.Status.Orientation != 0) ? Vector2.left : Vector2.right);
			NewFlagellant.MotionLerper.StartLerping(vector);
		}
	}

	public bool CanGoDownToReachPlayer()
	{
		bool result = false;
		Vector2 vector = ((NewFlagellant.Status.Orientation != 0) ? Vector2.left : Vector2.right);
		Vector2 vector2 = Core.Logic.Penitent.transform.position;
		if (Math.Sign((vector2 - (Vector2)base.transform.position).x) != Math.Sign(vector.x) || vector2.y > base.transform.position.y)
		{
			return false;
		}
		Vector2 pos = (Vector2)base.transform.position + vector;
		if (NewFlagellant.MotionChecker.HitsFloorInPosition(pos, 2f, out var _))
		{
			result = true;
		}
		return result;
	}

	public void Chase()
	{
		if (IsWaiting())
		{
			StopMovement();
			NewFlagellant.AnimatorInyector.Run(run: false);
			return;
		}
		if (NewFlagellant.MotionChecker.HitsBlock || !NewFlagellant.MotionChecker.HitsFloor || !CanReachPlayer())
		{
			StartWait(0.5f);
			StopMovement();
			return;
		}
		if (NewFlagellant.MotionChecker.HitsPatrolBlock)
		{
			StartWait(0.5f);
			return;
		}
		NewFlagellant.AnimatorInyector.Run(run: true);
		NewFlagellant.SetMovementSpeed(NewFlagellant.MAX_SPEED);
		Vector2 vector = ((NewFlagellant.Status.Orientation != 0) ? Vector2.left : Vector2.right);
		NewFlagellant.Input.HorizontalInput = vector.x;
	}

	public bool IsWaiting()
	{
		return _waitTimer > 0f;
	}

	private void StartWait(float seconds)
	{
		_waitTimer = seconds;
	}

	public void AttackDisplacement()
	{
		Vector2 vector = ((NewFlagellant.Status.Orientation != 0) ? Vector2.left : Vector2.right);
		NewFlagellant.EntityDisplacement.Move(1.2f * vector.x, 0.25f, Ease.OutCubic);
	}

	public void Patrol()
	{
		if (NewFlagellant.MotionChecker.HitsBlock || !NewFlagellant.MotionChecker.HitsFloor || NewFlagellant.MotionChecker.HitsPatrolBlock)
		{
			ReverseOrientation();
		}
		NewFlagellant.SetMovementSpeed(NewFlagellant.MIN_SPEED);
		Vector2 vector = ((NewFlagellant.Status.Orientation != 0) ? Vector2.left : Vector2.right);
		NewFlagellant.Input.HorizontalInput = vector.x;
		NewFlagellant.AnimatorInyector.Walk(walk: true);
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
		NewFlagellant.IsAttacking = false;
		NewFlagellant.AnimatorInyector.IsParried(isParried: true);
		Core.Logic.CameraManager.ProCamera2DShake.ShakeUsingPreset("Parry");
		_parryRecoverTime = maxParryTime;
	}

	public override void Execution()
	{
		base.Execution();
		NewFlagellant.IsAttacking = false;
		base.GotParry = true;
		StopMovement();
		NewFlagellant.gameObject.layer = LayerMask.NameToLayer("Default");
		NewFlagellant.SpriteRenderer.enabled = false;
		Core.Logic.Penitent.Audio.PlaySimpleHitToEnemy();
		NewFlagellant.EntExecution.InstantiateExecution();
		if (NewFlagellant.EntExecution != null)
		{
			NewFlagellant.EntExecution.enabled = true;
		}
	}

	public override void Alive()
	{
		base.Alive();
		NewFlagellant.gameObject.layer = LayerMask.NameToLayer("Enemy");
		base.GotParry = false;
		NewFlagellant.SpriteRenderer.enabled = true;
		NewFlagellant.Animator.SetBool("IS_PARRIED", value: false);
		NewFlagellant.Animator.Play("Idle");
		NewFlagellant.CurrentLife = NewFlagellant.Stats.Life.Base / 2f;
		if (NewFlagellant.EntExecution != null)
		{
			NewFlagellant.EntExecution.enabled = false;
		}
		NewFlagellant.StateMachine.SwitchState<NewFlagellantIdleState>();
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
		if (!base.IsAttacking || flinchDuringAttacks)
		{
			Hurt();
		}
	}

	private void Hurt()
	{
		LookAtPenitent();
		NewFlagellant.AnimatorInyector.Hurt();
		NewFlagellant.StateMachine.SwitchState<NewFlagellantHurtState>();
		ResetHurtTime();
	}

	public void LookAtPenitent()
	{
		if (NewFlagellant.Target != null)
		{
			LookAtTarget(NewFlagellant.Target.transform.position);
		}
	}

	public override void StopMovement()
	{
		if (NewFlagellant.MotionLerper.IsLerping)
		{
			NewFlagellant.MotionLerper.StopLerping();
		}
		NewFlagellant.Input.HorizontalInput = 0f;
	}
}
