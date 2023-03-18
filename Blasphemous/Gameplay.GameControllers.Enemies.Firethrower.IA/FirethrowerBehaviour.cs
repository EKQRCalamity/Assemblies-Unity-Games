using System;
using Framework.FrameworkCore;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Firethrower.IA;

public class FirethrowerBehaviour : EnemyBehaviour
{
	private enum ATTACK_STATE
	{
		NOT_ATTACKING,
		CHARGING,
		THROWING_FIRE
	}

	[FoldoutGroup("Activation Settings", true, 0)]
	public float ActivationDistance;

	[FoldoutGroup("Attack Settings", true, 0)]
	public float MaxVisibleHeight = 2f;

	[FoldoutGroup("Attack Settings", true, 0)]
	public float MinAttackDistance = 2f;

	[FoldoutGroup("Attack Settings", true, 0)]
	public float AttackCoolDown = 2f;

	private float _currentAttackLapse;

	private ATTACK_STATE currentAttackState;

	public float chargeTime = 1f;

	public float attackDuration = 1f;

	private float currentChargeTime = -1f;

	private float currentAttackTime = -1f;

	public EntityMotionChecker motionChecker;

	[Header("TEMP ONLY")]
	public Transform attackPlaceholderParent;

	[FoldoutGroup("Activation Settings", true, 0)]
	public float DistanceToTarget { get; private set; }

	public Firethrower Firethrower { get; private set; }

	public bool Awaken { get; private set; }

	public override void OnAwake()
	{
		base.OnAwake();
		Firethrower = (Firethrower)Entity;
	}

	public override void OnStart()
	{
		base.OnStart();
		Firethrower.OnDeath += OnDeath;
	}

	private void OnDeath()
	{
		Firethrower.OnDeath -= OnDeath;
		BoxCollider2D[] componentsInChildren = Firethrower.Attack.GetComponentsInChildren<BoxCollider2D>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
	}

	private void Update()
	{
		if (!Firethrower.Status.Dead)
		{
			Firethrower.Target = GetTarget().gameObject;
			if (Firethrower.Target != null)
			{
				DistanceToTarget = Vector2.Distance(Firethrower.transform.position, Firethrower.Target.transform.position);
			}
			if (!base.IsAttacking)
			{
				_currentAttackLapse += Time.deltaTime;
			}
			CheckCounters();
			if (DistanceToTarget <= ActivationDistance && !base.BehaviourTree.isRunning && !Awaken)
			{
				Awaken = true;
				base.BehaviourTree.StartBehaviour();
			}
		}
	}

	private void CheckCounters()
	{
		if (currentChargeTime >= 0f && currentChargeTime < chargeTime)
		{
			currentChargeTime += Time.deltaTime;
		}
		else if (currentChargeTime >= chargeTime)
		{
			currentChargeTime = -1f;
			Firethrower.AnimatorInyector.Attack();
			currentAttackState = ATTACK_STATE.THROWING_FIRE;
			currentAttackTime = 0f;
		}
		if (currentAttackTime >= 0f && currentAttackTime < attackDuration)
		{
			currentAttackTime += Time.deltaTime;
		}
		else if (currentAttackTime >= attackDuration)
		{
			currentAttackTime = -1f;
			Firethrower.AnimatorInyector.StopAttack();
		}
	}

	public bool IsFreeToMove()
	{
		return currentAttackState == ATTACK_STATE.NOT_ATTACKING;
	}

	public override void Idle()
	{
		Debug.Log("Firethrower: IDLE");
		StopMovement();
	}

	public bool TargetCanBeVisible()
	{
		float num = Firethrower.Target.transform.position.y - Firethrower.transform.position.y;
		num = ((!(num > 0f)) ? (0f - num) : num);
		return num <= MaxVisibleHeight;
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
		if (targetPos.x > Firethrower.transform.position.x)
		{
			if (Firethrower.Status.Orientation != 0)
			{
				Firethrower.SetOrientation(EntityOrientation.Right, allowFlipRenderer: false);
				Firethrower.AnimatorInyector.TurnAround();
			}
		}
		else if (Firethrower.Status.Orientation != EntityOrientation.Left)
		{
			Firethrower.SetOrientation(EntityOrientation.Left, allowFlipRenderer: false);
			Firethrower.AnimatorInyector.TurnAround();
		}
	}

	public void Chase(Vector3 position)
	{
		if (base.IsAttacking || base.IsHurt)
		{
			StopMovement();
			return;
		}
		LookAtTarget(position);
		if (base.TurningAround)
		{
			StopMovement();
			return;
		}
		float num = ((Entity.Status.Orientation != 0) ? (-1f) : 1f);
		ChangePlaceholderDirection(num);
		Firethrower.Input.HorizontalInput = num;
		Firethrower.AnimatorInyector.Walk();
	}

	public override void Damage()
	{
	}

	public void Death()
	{
		StopMovement();
		base.BehaviourTree.StopBehaviour();
		Firethrower.AnimatorInyector.Death();
		currentAttackState = ATTACK_STATE.NOT_ATTACKING;
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
		if (!base.TurningAround)
		{
			StopMovement();
			if (_currentAttackLapse >= AttackCoolDown)
			{
				_currentAttackLapse = 0f;
				CheckAttackState();
			}
		}
	}

	private void CheckAttackState()
	{
		switch (currentAttackState)
		{
		case ATTACK_STATE.NOT_ATTACKING:
			currentAttackState = ATTACK_STATE.CHARGING;
			Firethrower.AnimatorInyector.Charge();
			currentChargeTime = 0f;
			break;
		}
	}

	public override void StopMovement()
	{
		Firethrower.AnimatorInyector.Stop();
		Firethrower.Input.HorizontalInput = 0f;
	}

	public override void Wander()
	{
		Debug.Log("Firethrower: WANDER");
		if (base.IsAttacking || base.IsHurt)
		{
			StopMovement();
			return;
		}
		if (base.TurningAround)
		{
			StopMovement();
			return;
		}
		float num = ((Entity.Status.Orientation != 0) ? (-1f) : 1f);
		ChangePlaceholderDirection(num);
		isBlocked = motionChecker.HitsBlock;
		bool hitsFloor = motionChecker.HitsFloor;
		if (isBlocked || !hitsFloor)
		{
			LookAtTarget(base.transform.position - num * Vector3.right);
			return;
		}
		Firethrower.Input.HorizontalInput = num;
		Firethrower.AnimatorInyector.Walk();
	}

	public void OnAttackAnimationFinished()
	{
		currentAttackState = ATTACK_STATE.NOT_ATTACKING;
		ResetCoolDown();
	}

	private void ChangePlaceholderDirection(float dir)
	{
		attackPlaceholderParent.localScale = new Vector3(dir, 1f, 1f);
	}

	public override void Chase(Transform targetPosition)
	{
		throw new NotImplementedException();
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.magenta;
	}
}
