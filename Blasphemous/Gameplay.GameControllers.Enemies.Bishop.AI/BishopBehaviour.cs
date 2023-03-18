using System;
using DG.Tweening;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Bishop.AI;

public class BishopBehaviour : EnemyBehaviour
{
	[FoldoutGroup("Patrol", 0)]
	[Tooltip("Time wandering before Idle")]
	public float WanderingTime;

	private float _currentWanderingTime;

	[FoldoutGroup("Patrol", 0)]
	[Tooltip("Idle time before starts wandering")]
	public float IdleTime;

	[FoldoutGroup("Patrol", 0)]
	[Tooltip("Min distance to ground")]
	public float GroundDistance;

	[FoldoutGroup("Motion", 0)]
	[Tooltip("Chasing Speed")]
	public float MaxSpeed = 3f;

	private bool _isMaxSpeed;

	[FoldoutGroup("Motion", 0)]
	[Tooltip("Wander Speed")]
	public float MinSpeed = 1f;

	private bool _isMinSpeed;

	[FoldoutGroup("Attack", 0)]
	[Tooltip("Wander Speed")]
	public float MinDistanceToTarget = 2f;

	[FoldoutGroup("Hurt", 0)]
	[Tooltip("Displacement when the enemy is hit")]
	public float HurtDisplacement = 1f;

	private bool isExecuted;

	public const float HurtAnimDuration = 0.55f;

	public bool IsExecuted => isExecuted;

	public Bishop Bishop { get; private set; }

	public bool RequestTurning { get; private set; }

	public bool IsBlock
	{
		get
		{
			if (Bishop == null)
			{
				return false;
			}
			return Bishop.MotionChecker.HitsBlock || !Bishop.MotionChecker.HitsFloor;
		}
	}

	public bool CanTurn => _currentWanderingTime >= WanderingTime;

	public override void OnStart()
	{
		base.OnStart();
		Bishop = (Bishop)Entity;
		Bishop.OnDeath += OnDeath;
	}

	public override void Idle()
	{
		throw new NotImplementedException();
	}

	public override void Wander()
	{
		if (!Bishop)
		{
			return;
		}
		if (IsTurningAround())
		{
			StopMovement();
			return;
		}
		Bishop.AnimatorInyector.Chasing(isChasing: false);
		if (!_isMinSpeed)
		{
			_isMinSpeed = true;
			_isMaxSpeed = false;
			DOTween.To(delegate(float x)
			{
				Bishop.Controller.MaxWalkingSpeed = x;
			}, Bishop.Controller.MaxWalkingSpeed, MinSpeed, 1f);
		}
		if (base.IsChasing)
		{
			base.IsChasing = false;
		}
		_currentWanderingTime += Time.deltaTime;
		float horizontalInput = ((Bishop.Status.Orientation != 0) ? (-1f) : 1f);
		Bishop.Inputs.HorizontalInput = horizontalInput;
	}

	public override void Chase(Transform targetPosition)
	{
		if (!Bishop || Bishop.Status.Dead)
		{
			StopMovement();
			return;
		}
		if (IsBlock)
		{
			Bishop.Controller.MaxWalkingSpeed = 0f;
			Bishop.AnimatorInyector.Idle();
			return;
		}
		Bishop.AnimatorInyector.Chasing(isChasing: true);
		if (!_isMaxSpeed)
		{
			_isMaxSpeed = true;
			_isMinSpeed = false;
			DOTween.To(delegate(float x)
			{
				Bishop.Controller.MaxWalkingSpeed = x;
			}, Bishop.Controller.MaxWalkingSpeed, MaxSpeed, UnityEngine.Random.value * 5f + 0.5f);
		}
		if (!base.IsChasing)
		{
			base.IsChasing = true;
		}
		float horizontalInput = ((Bishop.Status.Orientation != 0) ? (-1f) : 1f);
		Bishop.Inputs.HorizontalInput = horizontalInput;
	}

	public override void Attack()
	{
		Bishop.AnimatorInyector.Chasing(isChasing: false);
		if (RequestTurning)
		{
			TurnAround();
		}
		else if (!base.TurningAround && !isExecuted)
		{
			Bishop.AnimatorInyector.Attack();
		}
	}

	public override void Damage()
	{
		PauseBehaviour();
		StopMovement();
		Bishop.AnimatorInyector.Damage();
	}

	public override void StopMovement()
	{
		Bishop.Inputs.HorizontalInput = 0f;
		Bishop.Controller.PlatformCharacterPhysics.HSpeed = 0f;
		Bishop.Controller.MaxWalkingSpeed = 0f;
		_isMaxSpeed = (_isMinSpeed = false);
	}

	public void HitDisplacement(Vector3 attakingEntityPos)
	{
		float num = ((!(Entity.transform.position.x >= attakingEntityPos.x)) ? (0f - HurtDisplacement) : HurtDisplacement);
		Bishop.transform.DOMoveX(Bishop.transform.position.x + num, 0.55f);
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
		RequestTurning = false;
		if (Entity.transform.position.x >= targetPos.x)
		{
			if (Entity.Status.Orientation != EntityOrientation.Left)
			{
				RequestTurning = true;
				TurnAround();
			}
		}
		else if (Entity.transform.position.x < targetPos.x && Entity.Status.Orientation != 0)
		{
			RequestTurning = true;
			TurnAround();
		}
	}

	public void TurnAround()
	{
		_currentWanderingTime = 0f;
		StopMovement();
		Bishop.AnimatorInyector.TurnAround();
	}

	public override void Execution()
	{
		base.Execution();
		isExecuted = true;
		Bishop.gameObject.layer = LayerMask.NameToLayer("Default");
		Bishop.Audio.StopAll();
		Bishop.Animator.Play("Idle");
		StopMovement();
		StopBehaviour();
		Bishop.SpriteRenderer.enabled = false;
		Core.Logic.Penitent.Audio.PlaySimpleHitToEnemy();
		Bishop.Attack.enabled = false;
		Bishop.EntExecution.InstantiateExecution();
		if (Bishop.EntExecution != null)
		{
			Bishop.EntExecution.enabled = true;
		}
	}

	public override void Alive()
	{
		base.Alive();
		isExecuted = false;
		Bishop.gameObject.layer = LayerMask.NameToLayer("Enemy");
		Bishop.SpriteRenderer.enabled = true;
		Bishop.Animator.Play("Idle");
		Bishop.CurrentLife = Bishop.Stats.Life.Base / 2f;
		Bishop.Attack.enabled = true;
		StartBehaviour();
		if (Bishop.EntExecution != null)
		{
			Bishop.EntExecution.enabled = false;
		}
	}

	private void OnDeath()
	{
		StopMovement();
		StopBehaviour();
	}
}
