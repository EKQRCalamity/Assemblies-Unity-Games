using System;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.RangedBoomerang.Attack;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.RangedBoomerang.IA;

public class RangedBoomerangBehaviour : EnemyBehaviour
{
	[FoldoutGroup("Activation Settings", true, 0)]
	public float ActivationDistance;

	[FoldoutGroup("Attack Settings", true, 0)]
	public float MaxVisibleHeight = 2f;

	[FoldoutGroup("Attack Settings", true, 0)]
	public float MinAttackDistance = 2f;

	[FoldoutGroup("Attack Settings", true, 0)]
	public float AttackCoolDown = 1.75f;

	public bool doPatrol = true;

	private float _currentAttackLapse = 1.25f;

	public RangedBoomerangAttack rangedBoomerangAttack;

	public EntityMotionChecker motionChecker;

	public LayerMask sightCollisionMask;

	public Vector2 sightOffset;

	public float visionAngle;

	public float debugLastAngle;

	private bool isExecuted;

	[FoldoutGroup("Activation Settings", true, 0)]
	public float DistanceToTarget { get; private set; }

	public RangedBoomerang RangedBoomerang { get; private set; }

	public bool Awaken { get; private set; }

	public override void OnAwake()
	{
		base.OnAwake();
		RangedBoomerang = (RangedBoomerang)Entity;
	}

	private void Update()
	{
		DistanceToTarget = Vector2.Distance(RangedBoomerang.transform.position, RangedBoomerang.Target.transform.position);
		if (!base.IsAttacking && CanSeeTarget())
		{
			_currentAttackLapse += Time.deltaTime;
		}
		if (DistanceToTarget <= ActivationDistance && !base.BehaviourTree.isRunning && !Awaken)
		{
			Awaken = true;
			base.BehaviourTree.StartBehaviour();
		}
	}

	public void OnBoomerangRecovered()
	{
		RangedBoomerang.Audio.StopThrow();
		RangedBoomerang.AnimatorInyector.Recover();
	}

	public override void Idle()
	{
		StopMovement();
	}

	public bool ShouldPatrol()
	{
		return doPatrol;
	}

	public bool CanSeeTarget()
	{
		return RangedBoomerang.VisionCone.CanSeeTarget(RangedBoomerang.Target.transform, "Penitent");
	}

	public bool CanAttack()
	{
		return _currentAttackLapse >= AttackCoolDown;
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
		if (targetPos.x > RangedBoomerang.transform.position.x)
		{
			if (RangedBoomerang.Status.Orientation != 0)
			{
				RangedBoomerang.SetOrientation(EntityOrientation.Right);
			}
		}
		else if (RangedBoomerang.Status.Orientation != EntityOrientation.Left)
		{
			RangedBoomerang.SetOrientation(EntityOrientation.Left);
		}
	}

	public void Chase(Vector3 position)
	{
	}

	public override void Damage()
	{
	}

	public void Death()
	{
		RangedBoomerang.Audio.StopThrow();
		StopMovement();
		base.BehaviourTree.StopBehaviour();
		RangedBoomerang.AnimatorInyector.Death();
	}

	public void ResetCoolDown()
	{
		if (_currentAttackLapse > 0f)
		{
			_currentAttackLapse = 0f;
		}
	}

	public override void Attack()
	{
		if (!base.IsAttacking)
		{
			StopMovement();
			Transform target = GetTarget();
			rangedBoomerangAttack.target = target;
			LookAtTarget(target.position);
			if (_currentAttackLapse >= AttackCoolDown)
			{
				_currentAttackLapse = 0f;
				RangedBoomerang.AnimatorInyector.Attack();
			}
		}
	}

	public override void StopMovement()
	{
		RangedBoomerang.AnimatorInyector.Stop();
		RangedBoomerang.Input.HorizontalInput = 0f;
	}

	public override void Wander()
	{
		if (base.IsAttacking || base.IsHurt)
		{
			StopMovement();
			return;
		}
		float num = ((Entity.Status.Orientation != 0) ? (-1f) : 1f);
		isBlocked = motionChecker.HitsBlock;
		bool hitsFloor = motionChecker.HitsFloor;
		if (isBlocked || !hitsFloor)
		{
			LookAtTarget(base.transform.position - num * Vector3.right);
			return;
		}
		RangedBoomerang.Input.HorizontalInput = num;
		RangedBoomerang.AnimatorInyector.Walk();
	}

	public override void Execution()
	{
		base.Execution();
		isExecuted = true;
		RangedBoomerang.gameObject.layer = LayerMask.NameToLayer("Default");
		RangedBoomerang.Audio.StopAll();
		RangedBoomerang.Animator.Play("Idle");
		StopMovement();
		StopBehaviour();
		RangedBoomerang.SpriteRenderer.enabled = false;
		Core.Logic.Penitent.Audio.PlaySimpleHitToEnemy();
		if (RangedBoomerang.EntExecution != null)
		{
			RangedBoomerang.EntExecution.InstantiateExecution();
			RangedBoomerang.EntExecution.enabled = true;
		}
	}

	public override void Alive()
	{
		base.Alive();
		isExecuted = false;
		RangedBoomerang.gameObject.layer = LayerMask.NameToLayer("Enemy");
		RangedBoomerang.SpriteRenderer.enabled = true;
		RangedBoomerang.Animator.Play("Idle");
		RangedBoomerang.CurrentLife = RangedBoomerang.Stats.Life.Base / 2f;
		RangedBoomerang.Attack.enabled = true;
		StartBehaviour();
		if (RangedBoomerang.EntExecution != null)
		{
			RangedBoomerang.EntExecution.enabled = false;
		}
	}

	public override void Chase(Transform targetPosition)
	{
		throw new NotImplementedException();
	}

	public override void ReadSpawnerConfig(SpawnBehaviourConfig config)
	{
		base.ReadSpawnerConfig(config);
		doPatrol = !config.dontWalk;
	}
}
