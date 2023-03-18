using System;
using System.Collections;
using CreativeSpore.SmartColliders;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Runner.AI;

public class RunnerBehaviour : EnemyBehaviour
{
	public float ChasingRemainTime = 2f;

	public float ChasingImpasse = 1f;

	private float _currentChasingTime;

	private const float MaxSpeed = 7f;

	public float DecelerationFactor = 2f;

	public float frenzyFactor = 1.4f;

	private bool frenzy;

	public ParticleSystem angryParticles;

	private bool isExecuted;

	private bool subscribedToEvents;

	public Runner Runner { get; private set; }

	public bool IsScreaming { get; set; }

	public bool CanChase => !Runner.MotionChecker.HitsBlock && Runner.MotionChecker.HitsFloor;

	public override void OnStart()
	{
		base.OnStart();
		Runner = (Runner)Entity;
		ContactDamage contactDamage = Runner.ContactDamage;
		contactDamage.OnContactDamage = (Core.GenericEvent)Delegate.Combine(contactDamage.OnContactDamage, new Core.GenericEvent(OnContactDamage));
		Runner.OnDeath += OnDeath;
		Gameplay.GameControllers.Penitent.Penitent penitent = Core.Logic.Penitent;
		penitent.OnDead = (Core.SimpleEvent)Delegate.Combine(penitent.OnDead, new Core.SimpleEvent(OnPlayerDead));
		subscribedToEvents = true;
	}

	private void OnPlayerDead()
	{
		Gameplay.GameControllers.Penitent.Penitent penitent = Core.Logic.Penitent;
		penitent.OnDead = (Core.SimpleEvent)Delegate.Remove(penitent.OnDead, new Core.SimpleEvent(OnPlayerDead));
		Runner.StateMachine.SwitchState<RunnerIdleState>();
		base.enabled = false;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (Runner.Status.Dead || isExecuted)
		{
			Runner.StateMachine.SwitchState<RunnerIdleState>();
			return;
		}
		if (Runner.VisionCone.CanSeeTarget(Runner.Target.transform, "Penitent"))
		{
			_currentChasingTime = ChasingRemainTime;
			Runner.StateMachine.SwitchState<RunnerChaseState>();
			return;
		}
		_currentChasingTime -= Time.deltaTime;
		if (_currentChasingTime < 0f)
		{
			Runner.StateMachine.SwitchState<RunnerIdleState>();
		}
	}

	public override void Chase(Transform targetPosition)
	{
		Runner.Controller.MaxWalkingSpeed = 7f;
		Runner.Attack.SetContactDamageType(DamageArea.DamageType.Heavy);
		Runner.Attack.SetContactDamage(Runner.Stats.Strength.Final);
		float horizontalInput = ((Runner.Status.Orientation != EntityOrientation.Left) ? 1f : (-1f));
		Runner.Input.HorizontalInput = horizontalInput;
		base.IsChasing = true;
		Runner.IsAttacking = true;
		Runner.AnimatorInjector.Run(base.IsChasing);
	}

	private bool LessThanHalfHP()
	{
		return Runner.Stats.Life.Current < Runner.Stats.Life.CurrentMax * 0.5f;
	}

	private void SetFrenzy()
	{
		frenzy = true;
		Runner.Animator.speed = 1f * frenzyFactor;
		PlatformCharacterController component = GetComponent<PlatformCharacterController>();
		float maxWalkingSpeed = component.MaxWalkingSpeed;
		component.MaxWalkingSpeed = maxWalkingSpeed * frenzyFactor;
		angryParticles.Play();
	}

	public void Scream()
	{
		if (!frenzy && LessThanHalfHP())
		{
			SetFrenzy();
		}
		Runner.AnimatorInjector.Scream();
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
		if (Runner.Status.Orientation == EntityOrientation.Right)
		{
			if (Runner.transform.position.x >= targetPos.x)
			{
				Runner.AnimatorInjector.TurnAround();
			}
		}
		else if (Runner.transform.position.x <= targetPos.x)
		{
			Runner.AnimatorInjector.TurnAround();
		}
	}

	private void OnContactDamage(UnityEngine.Object param)
	{
		Scream();
	}

	public override void StopMovement()
	{
		if (Runner.Controller.MaxWalkingSpeed >= 7f)
		{
			StartCoroutine(ReduceSpeed());
		}
		base.IsChasing = false;
		Runner.IsAttacking = false;
		Runner.Attack.SetContactDamageType(DamageArea.DamageType.Normal);
		Runner.Attack.SetContactDamage(Runner.Attack.ContactDamageAmount);
		Runner.AnimatorInjector.Run(base.IsChasing);
	}

	private IEnumerator ReduceSpeed()
	{
		float currentSpeed = Runner.Controller.MaxWalkingSpeed;
		while (currentSpeed > 0f)
		{
			if (CanChase)
			{
				currentSpeed -= Time.deltaTime * DecelerationFactor;
				Runner.Controller.MaxWalkingSpeed = currentSpeed;
				yield return null;
				continue;
			}
			Stop();
			yield break;
		}
		Stop();
	}

	public void Stop()
	{
		Runner.Input.HorizontalInput = 0f;
		Runner.Controller.PlatformCharacterPhysics.HSpeed = 0f;
		Runner.Controller.MaxWalkingSpeed = 0f;
	}

	public override void Execution()
	{
		base.Execution();
		isExecuted = true;
		Runner.gameObject.layer = LayerMask.NameToLayer("Default");
		Runner.Audio.StopScream();
		Runner.Animator.Play("Idle");
		Stop();
		Runner.SpriteRenderer.enabled = false;
		Core.Logic.Penitent.Audio.PlaySimpleHitToEnemy();
		Runner.Attack.enabled = false;
		Runner.EntExecution.InstantiateExecution();
		if (Runner.EntExecution != null)
		{
			Runner.EntExecution.enabled = true;
		}
	}

	public override void Alive()
	{
		base.Alive();
		isExecuted = false;
		Runner.gameObject.layer = LayerMask.NameToLayer("Enemy");
		Runner.SpriteRenderer.enabled = true;
		Runner.Animator.Play("Idle");
		Runner.CurrentLife = Runner.Stats.Life.Base / 2f;
		Runner.Attack.enabled = true;
		if (Runner.EntExecution != null)
		{
			Runner.EntExecution.enabled = false;
		}
	}

	private void OnDeath()
	{
		UnSubscribeEvents();
	}

	private void OnDestroy()
	{
		UnSubscribeEvents();
	}

	private void UnSubscribeEvents()
	{
		if ((bool)Runner && subscribedToEvents)
		{
			subscribedToEvents = false;
			Runner.OnDeath -= OnDeath;
			ContactDamage contactDamage = Runner.ContactDamage;
			contactDamage.OnContactDamage = (Core.GenericEvent)Delegate.Remove(contactDamage.OnContactDamage, new Core.GenericEvent(OnContactDamage));
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

	public override void Attack()
	{
		throw new NotImplementedException();
	}

	public override void Damage()
	{
		throw new NotImplementedException();
	}
}
