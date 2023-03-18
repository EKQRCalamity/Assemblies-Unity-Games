using System;
using System.Collections;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.IA;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.JarThrower.AI;

public class JarThrowerBehaviour : EnemyBehaviour
{
	public float AttackDistance;

	public float ThrowingDistance;

	public float HealingLapse = 2f;

	[Range(1f, 100f)]
	public float HealingRatio = 25f;

	public float AttackCooldown = 2f;

	public float TimeChasing = 2f;

	public float WalkSpeed = 1.25f;

	public float RunSpeed = 2.5f;

	public float JarProjectileSpeed = 10f;

	public JarThrower JarThrower { get; private set; }

	public bool IsHealing { get; set; }

	public bool TargetIsDead { get; private set; }

	public bool TargetSeen => JarThrower.VisionCone.CanSeeTarget(JarThrower.Target.transform, "Penitent");

	public bool CanWalk => !JarThrower.MotionChecker.HitsBlock && JarThrower.MotionChecker.HitsFloor;

	public override void OnStart()
	{
		base.OnStart();
		TargetIsDead = false;
		JarThrower = (JarThrower)Entity;
		JarThrower.StateMachine.SwitchState<JarThrowerWanderState>();
		JarThrower.OnDeath += OnDeath;
		Core.Logic.Penitent.OnDeath += PenitentOnDeath;
	}

	public void Jump(Vector3 target)
	{
		JarThrower.AnimatorInjector.Jump();
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
		if (JarThrower.IsFalling)
		{
			return;
		}
		if (targetPos.x > JarThrower.transform.position.x)
		{
			if (JarThrower.Status.Orientation != 0)
			{
				JarThrower.SetOrientation(EntityOrientation.Right);
			}
		}
		else if (JarThrower.Status.Orientation != EntityOrientation.Left)
		{
			JarThrower.SetOrientation(EntityOrientation.Left);
		}
	}

	public override void Idle()
	{
		throw new NotImplementedException();
	}

	public override void Wander()
	{
		Move(WalkSpeed);
		JarThrower.AnimatorInjector.Walk();
		if (!CanWalk)
		{
			ReverseOrientation();
		}
	}

	public void Move(float speed)
	{
		float num = ((JarThrower.Status.Orientation != EntityOrientation.Left) ? 1f : (-1f));
		JarThrower.Inputs.HorizontalInput = ((!JarThrower.IsRunLanding && JarThrower.Status.IsGrounded) ? num : 0f);
		JarThrower.Controller.MaxWalkingSpeed = speed;
	}

	public override void Chase(Transform targetPosition)
	{
		LookAtTarget(targetPosition.position);
		JarThrower.AnimatorInjector.Run();
		Move(RunSpeed);
	}

	public override void Attack()
	{
		JarThrower.IsAttacking = true;
		JarThrower.AnimatorInjector.Jump();
	}

	public IEnumerator JumpAttackCoroutine()
	{
		JarThrower.Inputs.Jump = true;
		yield return new WaitForSeconds(1f);
		JarThrower.Inputs.Jump = false;
	}

	public void Death()
	{
		JarThrower.StartCoroutine(CallAnimatorInyectorDeath());
	}

	private IEnumerator CallAnimatorInyectorDeath()
	{
		while (base.enabled)
		{
			JarThrower.AnimatorInjector.Death();
			yield return new WaitForSeconds(1f);
			if (base.enabled)
			{
				JarThrower.AnimatorInjector.EntityAnimator.Play("Death");
			}
		}
	}

	public void Healing()
	{
		if (JarThrower.Stats.Life.Current < JarThrower.Stats.Life.CurrentMax * 0.5f)
		{
			IsHealing = true;
			JarThrower.AnimatorInjector.Healing();
			StopMovement();
			JarThrower.Stats.Life.Current += JarThrower.Stats.Life.Base * HealingRatio / 100f;
		}
	}

	public override void Damage()
	{
	}

	public override void StopMovement()
	{
		JarThrower.Controller.PlatformCharacterPhysics.HSpeed = 0f;
		JarThrower.Inputs.HorizontalInput = 0f;
		JarThrower.AnimatorInjector.Walk(walk: false);
		JarThrower.AnimatorInjector.Run(run: false);
	}

	private void OnDeath()
	{
		JarThrower.OnDeath -= OnDeath;
		JarThrower.StateMachine.enabled = false;
		StopMovement();
		Death();
	}

	private void PenitentOnDeath()
	{
		Core.Logic.Penitent.OnDeath -= PenitentOnDeath;
		JarThrower.StateMachine.SwitchState<JarThrowerWanderState>();
		TargetIsDead = true;
	}
}
