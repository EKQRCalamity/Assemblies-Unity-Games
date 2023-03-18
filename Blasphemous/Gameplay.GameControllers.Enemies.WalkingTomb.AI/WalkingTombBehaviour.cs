using System;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.WalkingTomb.AI;

public class WalkingTombBehaviour : EnemyBehaviour
{
	public float MinAttackDistance = 2f;

	public float AttackCooldown = 1f;

	public bool isExecuted;

	protected WalkingTomb WalkingTomb { get; set; }

	public bool TargetIsDead { get; private set; }

	public bool CanSeeTarget => WalkingTomb.VisionCone.CanSeeTarget(WalkingTomb.Target.transform, "Penitent");

	public bool CanWalk => !WalkingTomb.MotionChecker.HitsBlock && WalkingTomb.MotionChecker.HitsFloor;

	public bool TargetIsOnAttackDistance
	{
		get
		{
			float num = Vector2.Distance(WalkingTomb.transform.position, WalkingTomb.Target.transform.position);
			return num < WalkingTomb.Behaviour.MinAttackDistance;
		}
	}

	public bool TargetIsInFront
	{
		get
		{
			EntityOrientation orientation = WalkingTomb.Status.Orientation;
			Vector3 position = WalkingTomb.transform.position;
			Vector3 position2 = WalkingTomb.Target.transform.position;
			bool flag = ((orientation != 0) ? (position.x > position2.x) : (position.x <= position2.x));
			return CanSeeTarget && flag;
		}
	}

	public override void OnStart()
	{
		base.OnStart();
		WalkingTomb = (WalkingTomb)Entity;
		WalkingTomb.IsGuarding = true;
		WalkingTomb.StateMachine.SwitchState<WalkingTombWalkState>();
		WalkingTomb.OnDeath += OnDeath;
		Gameplay.GameControllers.Penitent.Penitent penitent = Core.Logic.Penitent;
		penitent.OnDead = (Core.SimpleEvent)Delegate.Combine(penitent.OnDead, new Core.SimpleEvent(TargetOnDead));
	}

	public override void Idle()
	{
		throw new NotImplementedException();
	}

	public override void Wander()
	{
		float horizontalInput = ((WalkingTomb.Status.Orientation != 0) ? (-1f) : 1f);
		WalkingTomb.Inputs.HorizontalInput = horizontalInput;
		WalkingTomb.AnimatorInjector.Attack(attack: false);
		if (!CanWalk)
		{
			ReverseOrientation();
		}
	}

	public override void StopMovement()
	{
		WalkingTomb.Controller.PlatformCharacterPhysics.HSpeed = 0f;
		WalkingTomb.Inputs.HorizontalInput = 0f;
	}

	public override void Chase(Transform targetPosition)
	{
		throw new NotImplementedException();
	}

	private void OnDeath()
	{
		WalkingTomb.OnDeath -= OnDeath;
		WalkingTomb.StateMachine.enabled = false;
		StopMovement();
	}

	private void TargetOnDead()
	{
		if (Core.Logic.Penitent != null)
		{
			Gameplay.GameControllers.Penitent.Penitent penitent = Core.Logic.Penitent;
			penitent.OnDead = (Core.SimpleEvent)Delegate.Remove(penitent.OnDead, new Core.SimpleEvent(TargetOnDead));
		}
		WalkingTomb.StateMachine.SwitchState<WalkingTombWalkState>();
		TargetIsDead = true;
	}

	public override void Attack()
	{
		WalkingTomb.AnimatorInjector.Attack();
	}

	public override void Execution()
	{
		base.Execution();
		isExecuted = true;
		WalkingTomb.Animator.enabled = false;
		WalkingTomb.StateMachine.enabled = false;
		WalkingTomb.gameObject.layer = LayerMask.NameToLayer("Default");
		WalkingTomb.Audio.StopAttack();
		WalkingTomb.Animator.Play("Idle");
		StopMovement();
		WalkingTomb.SpriteRenderer.enabled = false;
		Core.Logic.Penitent.Audio.PlaySimpleHitToEnemy();
		WalkingTomb.Attack.enabled = false;
		WalkingTomb.EntExecution.InstantiateExecution();
		if (WalkingTomb.EntExecution != null)
		{
			WalkingTomb.EntExecution.enabled = true;
		}
	}

	public override void Alive()
	{
		base.Alive();
		isExecuted = false;
		WalkingTomb.Animator.enabled = true;
		WalkingTomb.StateMachine.enabled = true;
		WalkingTomb.gameObject.layer = LayerMask.NameToLayer("Enemy");
		WalkingTomb.SpriteRenderer.enabled = true;
		WalkingTomb.Animator.Play("Idle");
		WalkingTomb.CurrentLife = WalkingTomb.Stats.Life.Base / 2f;
		WalkingTomb.Attack.enabled = true;
		if (WalkingTomb.EntExecution != null)
		{
			WalkingTomb.EntExecution.enabled = false;
		}
	}

	public override void Damage()
	{
	}
}
