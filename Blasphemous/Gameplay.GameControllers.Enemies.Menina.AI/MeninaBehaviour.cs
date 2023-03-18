using System;
using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Menina.AI;

public class MeninaBehaviour : EnemyBehaviour
{
	private float originalSpeed;

	private bool isExecuted;

	public bool playOnHurt;

	[EventRef]
	public string OnHurt;

	public float AttackCooldown;

	public float repeatSmashProbability = 0.5f;

	public float AwaitBeforeBackward;

	public Menina Menina { get; private set; }

	public float CurrentChasingTime { get; set; }

	public float CurrentAttackLapse { get; set; }

	public bool IsAwake { get; set; }

	public override void OnStart()
	{
		base.OnStart();
		Menina = (Menina)Entity;
		originalSpeed = Menina.Controller.MaxWalkingSpeed;
		Menina.OnDeath += OnDeath;
		Menina.StateMachine.SwitchState<MeninaStateBackwards>();
	}

	private void OnDeath()
	{
		Menina.OnDeath -= OnDeath;
		NormalSpeed();
		Menina.AnimatorInyector.Death();
		Menina.SpriteRenderer.GetComponent<BoxCollider2D>().enabled = false;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (IsDead() || isExecuted)
		{
			StopMovement();
		}
		else if (!IsAwake && base.PlayerHeard)
		{
			Menina.StateMachine.SwitchState<MeninaStateChase>();
		}
	}

	public override void Idle()
	{
		throw new NotImplementedException();
	}

	public override void Wander()
	{
	}

	public void StepBackwards()
	{
		NormalSpeed();
		CurrentChasingTime = 0f;
		Menina.AnimatorInyector.Step(forward: false);
	}

	public override void Chase(Transform targetPosition)
	{
		NormalSpeed();
		CurrentChasingTime = 0f;
		Menina.AnimatorInyector.Step();
	}

	public override void Attack()
	{
		NormalSpeed();
		CurrentAttackLapse = 0f;
		Menina.IsAttacking = true;
		Menina.AnimatorInyector.Attack();
	}

	public void ResetAttackCoolDown()
	{
		CurrentAttackLapse = 0f;
	}

	public override void Damage()
	{
		if (playOnHurt)
		{
			Core.Audio.PlaySfx(OnHurt);
		}
	}

	public void SingleStep(bool forward)
	{
		Menina.AnimatorInyector.OnStepFinished += OnStepFinished;
		Menina.AnimatorInyector.Step(forward);
		SpeedUp(forward);
	}

	private void SpeedUp(bool changeContactDamage = false)
	{
		if (changeContactDamage)
		{
			Menina.Attack.ContactDamageType = DamageArea.DamageType.Heavy;
		}
		float num = 1.5f;
		Menina.Animator.speed = 1.5f;
		Menina.Controller.MaxWalkingSpeed = originalSpeed * num;
	}

	public bool ShouldRepeatSmash()
	{
		return UnityEngine.Random.Range(0f, 1f) < repeatSmashProbability;
	}

	private void NormalSpeed()
	{
		Menina.Attack.ContactDamageType = DamageArea.DamageType.Normal;
		Menina.Animator.speed = 1f;
		Menina.Controller.MaxWalkingSpeed = originalSpeed;
	}

	private void OnStepFinished()
	{
		Menina.AnimatorInyector.OnStepFinished -= OnStepFinished;
		NormalSpeed();
		StopMovement();
		CurrentAttackLapse = AttackCooldown;
	}

	public override void StopMovement()
	{
		if (!(Menina == null))
		{
			Menina.Inputs.HorizontalInput = 0f;
			Menina.AnimatorInyector.Stop();
		}
	}

	public override void Execution()
	{
		base.Execution();
		isExecuted = true;
		Menina.gameObject.layer = LayerMask.NameToLayer("Default");
		Menina.Audio.StopAttack();
		Menina.Animator.Play("Idle");
		Menina.StateMachine.SwitchState<MeninaStateBackwards>();
		Menina.SpriteRenderer.enabled = false;
		Core.Logic.Penitent.Audio.PlaySimpleHitToEnemy();
		Menina.Attack.StopAttack();
		Menina.AnimatorInyector.gameObject.SetActive(value: false);
		if ((bool)Menina.EntExecution)
		{
			Menina.EntExecution.InstantiateExecution();
			Menina.EntExecution.enabled = true;
		}
	}

	public override void Alive()
	{
		base.Alive();
		isExecuted = false;
		Menina.gameObject.layer = LayerMask.NameToLayer("Enemy");
		Menina.SpriteRenderer.enabled = true;
		Menina.AnimatorInyector.gameObject.SetActive(value: true);
		Menina.Animator.Play("Idle");
		Menina.CurrentLife = Menina.Stats.Life.Base / 2f;
		if (Menina.EntExecution != null)
		{
			Menina.EntExecution.enabled = false;
		}
	}

	private void OnDrawGizmos()
	{
	}
}
