using System;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Nun.IA;

public class NunBehaviour : EnemyBehaviour
{
	[FoldoutGroup("Activation Settings", true, 0)]
	public float ActivationDistance;

	[FoldoutGroup("Attack Settings", true, 0)]
	public float MaxVisibleHeight = 2f;

	[FoldoutGroup("Attack Settings", true, 0)]
	public float MinAttackDistance = 2f;

	[FoldoutGroup("Attack Settings", true, 0)]
	public float AttackCoolDown = 2f;

	private float _currentAttackLapse;

	[FoldoutGroup("Activation Settings", true, 0)]
	public float DistanceToTarget { get; private set; }

	public Nun Nun { get; private set; }

	public bool Awaken { get; private set; }

	public override void OnAwake()
	{
		base.OnAwake();
		Nun = (Nun)Entity;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (!Nun.Dead)
		{
			DistanceToTarget = Vector2.Distance(Nun.transform.position, GetTarget().position);
			if (!base.IsAttacking)
			{
				_currentAttackLapse += Time.deltaTime;
			}
			if (DistanceToTarget <= ActivationDistance && !base.BehaviourTree.isRunning && !Awaken && !base.GotParry)
			{
				Awaken = true;
				base.BehaviourTree.StartBehaviour();
			}
		}
	}

	public override void Idle()
	{
		StopMovement();
	}

	public bool TargetCanBeVisible()
	{
		float num = Nun.Target.transform.position.y - Nun.transform.position.y;
		num = ((!(num > 0f)) ? (0f - num) : num);
		return num <= MaxVisibleHeight;
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
		if (targetPos.x > Nun.transform.position.x)
		{
			if (Nun.Status.Orientation != 0)
			{
				Nun.SetOrientation(EntityOrientation.Right, allowFlipRenderer: false);
				Nun.AnimatorInyector.TurnAround();
			}
		}
		else if (Nun.Status.Orientation != EntityOrientation.Left)
		{
			Nun.SetOrientation(EntityOrientation.Left, allowFlipRenderer: false);
			Nun.AnimatorInyector.TurnAround();
		}
	}

	public void Chase(Vector3 position)
	{
		LookAtTarget(position);
		if (base.IsHurt || base.TurningAround || base.IsAttacking || Nun.Status.Dead)
		{
			StopMovement();
			return;
		}
		float horizontalInput = ((Entity.Status.Orientation != 0) ? (-1f) : 1f);
		Nun.Input.HorizontalInput = horizontalInput;
		Nun.AnimatorInyector.Walk();
	}

	public override void Damage()
	{
	}

	public override void Chase(Transform targetPosition)
	{
	}

	public override void Wander()
	{
		throw new NotImplementedException();
	}

	public override void Execution()
	{
		base.Execution();
		base.GotParry = true;
		base.BehaviourTree.StopBehaviour();
		Nun.IsAttacking = false;
		StopMovement();
		Nun.gameObject.layer = LayerMask.NameToLayer("Default");
		Nun.SpriteRenderer.enabled = false;
		Nun.Attack.AttackArea.WeaponCollider.enabled = false;
		Nun.Animator.Play("Idle");
		Core.Logic.Penitent.Audio.PlaySimpleHitToEnemy();
		Nun.EntExecution.InstantiateExecution();
		if (Nun.EntExecution != null)
		{
			Nun.EntExecution.enabled = true;
		}
	}

	public override void Alive()
	{
		base.Alive();
		base.GotParry = false;
		base.BehaviourTree.StartBehaviour();
		Nun.gameObject.layer = LayerMask.NameToLayer("Enemy");
		Nun.SpriteRenderer.enabled = true;
		Nun.Attack.AttackArea.WeaponCollider.enabled = true;
		Nun.Animator.Play("Idle");
	}

	public void Death()
	{
		base.BehaviourTree.StopBehaviour();
		Nun.AnimatorInyector.Death();
		StopMovement();
		Nun.Attack.AttackArea.gameObject.SetActive(value: false);
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
				Nun.AnimatorInyector.Attack();
			}
		}
	}

	public override void StopMovement()
	{
		Nun.AnimatorInyector.Stop();
		Nun.Input.HorizontalInput = 0f;
	}
}
