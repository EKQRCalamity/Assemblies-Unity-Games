using System;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.Quirce.Attack;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.AshCharger.AI;

public class AshChargerBehaviour : EnemyBehaviour
{
	public BossDashAttack dashAttack;

	public float MinAttackDistance = 2f;

	public float AttackCooldown = 1f;

	public ParticleSystem particles;

	protected AshCharger AshCharger { get; set; }

	public bool TargetIsDead { get; private set; }

	public bool CanSeeTarget => AshCharger.VisionCone.CanSeeTarget(AshCharger.Target.transform, "Penitent");

	public bool CanWalk => !AshCharger.MotionChecker.HitsBlock && AshCharger.MotionChecker.HitsFloor;

	public bool TargetIsOnAttackDistance
	{
		get
		{
			float num = Vector2.Distance(AshCharger.transform.position, AshCharger.Target.transform.position);
			return num < AshCharger.Behaviour.MinAttackDistance;
		}
	}

	public bool TargetIsInFront
	{
		get
		{
			EntityOrientation orientation = AshCharger.Status.Orientation;
			Vector3 position = AshCharger.transform.position;
			Vector3 position2 = AshCharger.Target.transform.position;
			bool flag = ((orientation != 0) ? (position.x > position2.x) : (position.x <= position2.x));
			return CanSeeTarget && flag;
		}
	}

	public override void OnStart()
	{
		base.OnStart();
		AshCharger = (AshCharger)Entity;
		AshCharger.IsGuarding = true;
		AshCharger.StateMachine.SwitchState<AshChargerAppearState>();
		AshCharger.OnDeath += OnDeath;
		Gameplay.GameControllers.Penitent.Penitent penitent = Core.Logic.Penitent;
		penitent.OnDead = (Core.SimpleEvent)Delegate.Combine(penitent.OnDead, new Core.SimpleEvent(TargetOnDead));
		AshCharger.Target = Core.Logic.Penitent.gameObject;
	}

	public override void Idle()
	{
		throw new NotImplementedException();
	}

	public override void Wander()
	{
	}

	public override void StopMovement()
	{
		AshCharger.Controller.PlatformCharacterPhysics.HSpeed = 0f;
		AshCharger.Inputs.HorizontalInput = 0f;
	}

	public override void Chase(Transform targetPosition)
	{
		throw new NotImplementedException();
	}

	private void OnDeath()
	{
		AshCharger.AnimatorInyector.SetSpeed(1f);
		dashAttack.StopDash(base.transform, launchFinishedCallback: false);
		dashAttack.OnDashFinishedEvent -= DashAttack_OnDashFinishedEvent;
		AshCharger.OnDeath -= OnDeath;
		AshCharger.StateMachine.enabled = false;
		AshCharger.AnimatorInyector.Death();
		StopMovement();
	}

	private void TargetOnDead()
	{
		if (Core.Logic.Penitent != null)
		{
			Gameplay.GameControllers.Penitent.Penitent penitent = Core.Logic.Penitent;
			penitent.OnDead = (Core.SimpleEvent)Delegate.Remove(penitent.OnDead, new Core.SimpleEvent(TargetOnDead));
		}
		AshCharger.StateMachine.SwitchState<AshChargerAppearState>();
		TargetIsDead = true;
	}

	private float GetDirFromOrientation()
	{
		return (Entity.Status.Orientation != 0) ? (-1f) : 1f;
	}

	public override void Attack()
	{
		SetParticlesOrientation();
		Vector2 direction = Vector2.right * GetDirFromOrientation();
		dashAttack.OnDashFinishedEvent += DashAttack_OnDashFinishedEvent;
		dashAttack.OnDashAdvancedEvent += DashAttack_OnDashAdvancedEvent;
		dashAttack.Dash(base.transform, direction, 12f);
		AshCharger.AnimatorInyector.Attack();
	}

	private void SetParticlesOrientation()
	{
		ParticleSystem.TextureSheetAnimationModule textureSheetAnimation = particles.textureSheetAnimation;
		textureSheetAnimation.flipU = ((GetDirFromOrientation() == 1f) ? 1 : 0);
	}

	private void DashAttack_OnDashAdvancedEvent(float value)
	{
		float a = 0.5f;
		float b = 3f;
		AshCharger.AnimatorInyector.SetSpeed(Mathf.Lerp(a, b, value));
	}

	private void DashAttack_OnDashFinishedEvent()
	{
		dashAttack.OnDashFinishedEvent -= DashAttack_OnDashFinishedEvent;
		AshCharger.Kill();
	}

	public override void Damage()
	{
		throw new NotImplementedException();
	}
}
