using System;
using System.Collections;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Penitent;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Jumper.AI;

public class JumperBehaviour : EnemyBehaviour
{
	[FoldoutGroup("Activation Settings", true, 0)]
	public float ActivationDistance;

	[FoldoutGroup("Activation Settings", true, 0)]
	public float LongJumpRange;

	[FoldoutGroup("Motion Settings", true, 0)]
	public float LongJumpSpeed = 10f;

	private float _defaultJumpSpeed;

	public const float LongJumpLapse = 1f;

	public const float ShortJumpLapse = 0.01f;

	private WaitForSeconds _shortLapseWaiting;

	private WaitForSeconds _longTimeJumpWaiting;

	public float DistanceToTarget { get; private set; }

	public Jumper Jumper { get; set; }

	public bool IsPlayerDead { get; set; }

	public bool IsJumping { get; set; }

	private bool TargetIsOnSight
	{
		get
		{
			Transform target = GetTarget();
			float num = target.position.y - base.transform.position.y;
			num = ((!(num > 0f)) ? (0f - num) : num);
			return num <= 2f;
		}
	}

	public override void OnStart()
	{
		base.OnStart();
		Jumper = (Jumper)Entity;
		_shortLapseWaiting = new WaitForSeconds(0.01f);
		_longTimeJumpWaiting = new WaitForSeconds(1f);
		Gameplay.GameControllers.Penitent.Penitent penitent = Core.Logic.Penitent;
		penitent.OnDead = (Core.SimpleEvent)Delegate.Combine(penitent.OnDead, new Core.SimpleEvent(OnDead));
		Jumper.OnDeath += OnDeathJumper;
		_defaultJumpSpeed = Jumper.Controller.JumpingSpeed;
	}

	private void OnDead()
	{
		IsPlayerDead = true;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (!IsPlayerDead && !Jumper.Status.Dead && TargetIsOnSight)
		{
			DistanceToTarget = Vector2.Distance(Jumper.transform.position, GetTarget().position);
			Jumper.Status.IsGrounded = Jumper.Controller.IsGrounded;
			if (DistanceToTarget <= ActivationDistance && !IsJumping)
			{
				IsJumping = true;
				Jumper.Animator.Play("JumpReady");
			}
		}
	}

	public override void StopMovement()
	{
		Jumper.Inputs.HorizontalInput = 0f;
	}

	public void Jump()
	{
		if (!(Jumper == null))
		{
			bool flag = DistanceToTarget > LongJumpRange;
			Jumper.Controller.JumpingSpeed = ((!flag) ? _defaultJumpSpeed : LongJumpSpeed);
			StartCoroutine(JumpPress(flag));
		}
	}

	private IEnumerator JumpPress(bool isLongPress)
	{
		Jumper.Inputs.Jump = true;
		yield return (!isLongPress) ? _shortLapseWaiting : _longTimeJumpWaiting;
		Jumper.Inputs.Jump = false;
	}

	private void OnDeathJumper()
	{
		Jumper.Attack.AttackArea.WeaponCollider.enabled = false;
		Jumper.Controller.enabled = false;
		Jumper.DamageByContact = false;
		Jumper.AnimatorInjector.Death();
	}

	private void OnDestroy()
	{
		if ((bool)Core.Logic.Penitent)
		{
			Gameplay.GameControllers.Penitent.Penitent penitent = Core.Logic.Penitent;
			penitent.OnDead = (Core.SimpleEvent)Delegate.Remove(penitent.OnDead, new Core.SimpleEvent(OnDead));
		}
		if (Jumper != null)
		{
			Jumper.OnDeath -= OnDeathJumper;
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
