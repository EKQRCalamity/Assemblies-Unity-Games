using System;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.WheelCarrier.IA;

public class WheelCarrierBehaviour : EnemyBehaviour
{
	[FoldoutGroup("Attack Settings", true, 0)]
	protected float AttackLapse;

	[FoldoutGroup("Activation Settings", true, 0)]
	public float ActivationDistance;

	[FoldoutGroup("Attack Settings", true, 0)]
	public float AttackCoolDown = 2f;

	private bool isExecuted;

	[FoldoutGroup("Attack Settings", true, 0)]
	public float MaxVisibleHeight = 2f;

	[FoldoutGroup("Attack Settings", true, 0)]
	public float MinAttackDistance = 2f;

	public bool canWalk = true;

	[FoldoutGroup("Activation Settings", true, 0)]
	public float DistanceToTarget { get; private set; }

	public WheelCarrier WheelCarrier { get; private set; }

	public override void OnAwake()
	{
		base.OnAwake();
		WheelCarrier = (WheelCarrier)Entity;
	}

	public override void OnStart()
	{
		base.OnStart();
		if (Core.Logic.Penitent != null)
		{
			Core.Logic.Penitent.OnDeath += PlayerOnDeath;
		}
	}

	private void Update()
	{
		if (IsDead() || base.GotParry)
		{
			return;
		}
		DistanceToTarget = Vector2.Distance(WheelCarrier.transform.position, WheelCarrier.Target.transform.position);
		if (DistanceToTarget < ActivationDistance && TargetCanBeVisible())
		{
			if (DistanceToTarget > MinAttackDistance)
			{
				Vector3 position = WheelCarrier.Target.transform.position;
				Chase(position);
			}
			else
			{
				Attack();
			}
		}
		else
		{
			Idle();
		}
	}

	public override void Idle()
	{
		StopMovement();
	}

	public bool TargetCanBeVisible()
	{
		return WheelCarrier.VisionCone.CanSeeTarget(WheelCarrier.Target.transform, "Penitent");
	}

	public void StartVulnerablePeriod()
	{
		WheelCarrier.VulnerablePeriod.StartVulnerablePeriod(WheelCarrier);
	}

	public void Chase(Vector3 position)
	{
		if (base.IsAttacking || base.IsHurt || !WheelCarrier.MotionChecker.HitsFloor)
		{
			StopMovement();
			return;
		}
		LookAtTarget(position);
		float horizontalInput = ((Entity.Status.Orientation != 0) ? (-1f) : 1f);
		if (canWalk)
		{
			WheelCarrier.Input.HorizontalInput = horizontalInput;
			WheelCarrier.AnimatorInyector.Walk();
		}
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
		if (WheelCarrier.transform.position.x < targetPos.x)
		{
			if (WheelCarrier.Status.Orientation != 0)
			{
				WheelCarrier.SetOrientation(EntityOrientation.Right);
			}
		}
		else if (WheelCarrier.Status.Orientation != EntityOrientation.Left)
		{
			WheelCarrier.SetOrientation(EntityOrientation.Left);
		}
	}

	public override void Damage()
	{
	}

	public void Death()
	{
		StopMovement();
		WheelCarrier.AnimatorInyector.Death();
	}

	private void PlayerOnDeath()
	{
	}

	public void ResetCoolDown()
	{
		if (AttackLapse > 0f)
		{
			AttackLapse = 0f;
		}
	}

	public override void Attack()
	{
		StopMovement();
		AttackLapse += Time.deltaTime;
		if (AttackLapse >= AttackCoolDown)
		{
			AttackLapse = 0f;
			WheelCarrier.AnimatorInyector.Attack();
		}
	}

	public override void Parry()
	{
		base.Parry();
		base.GotParry = true;
		WheelCarrier.AnimatorInyector.ParryReaction();
	}

	public override void Execution()
	{
		base.Execution();
		isExecuted = true;
		base.GotParry = true;
		StopMovement();
		WheelCarrier.gameObject.layer = LayerMask.NameToLayer("Default");
		WheelCarrier.SpriteRenderer.enabled = false;
		Core.Logic.Penitent.Audio.PlaySimpleHitToEnemy();
		WheelCarrier.EntExecution.InstantiateExecution();
		WheelCarrier.Attack.gameObject.SetActive(value: false);
		if (WheelCarrier.EntExecution != null)
		{
			WheelCarrier.EntExecution.enabled = true;
		}
	}

	public override void Alive()
	{
		base.Alive();
		WheelCarrier.gameObject.layer = LayerMask.NameToLayer("Enemy");
		base.GotParry = false;
		WheelCarrier.SpriteRenderer.enabled = true;
		WheelCarrier.Attack.gameObject.SetActive(value: true);
		WheelCarrier.Animator.Play("Idle");
		WheelCarrier.CurrentLife = WheelCarrier.Stats.Life.Base / 2f;
		if (WheelCarrier.EntExecution != null)
		{
			WheelCarrier.EntExecution.enabled = false;
		}
	}

	public override void StopMovement()
	{
		WheelCarrier.AnimatorInyector.Stop();
		WheelCarrier.Input.HorizontalInput = 0f;
	}

	private void OnDestroy()
	{
		Core.Logic.Penitent.OnDeath -= PlayerOnDeath;
	}

	public override void Wander()
	{
		throw new NotImplementedException();
	}

	public override void Chase(Transform targetPosition)
	{
		throw new NotImplementedException();
	}

	public override void ReadSpawnerConfig(SpawnBehaviourConfig config)
	{
		base.ReadSpawnerConfig(config);
		canWalk = !config.dontWalk;
	}
}
