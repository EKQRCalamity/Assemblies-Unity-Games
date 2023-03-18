using System;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.ShieldMaiden.Attack;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.ShieldMaiden.IA;

public class ShieldMaidenBehaviour : EnemyBehaviour
{
	[FoldoutGroup("Activation Settings", true, 0)]
	public float ActivationDistance;

	[FoldoutGroup("Attack Settings", true, 0)]
	public float MaxVisibleHeight = 2f;

	[FoldoutGroup("Attack Settings", true, 0)]
	public float MinAttackDistance = 2f;

	[FoldoutGroup("Attack Settings", true, 0)]
	public float AttackCoolDown = 2f;

	[FoldoutGroup("Attack Settings", true, 0)]
	public float maxParryTime = 2f;

	public ShieldMaidenAttack ShieldMaidenAttack;

	[FoldoutGroup("Traits", true, 0)]
	public EntityMotionChecker motionChecker;

	private float _currentAttackLapse;

	private float _parryTime;

	private float _waitPeriod;

	private float _lastDirectionChange;

	[FoldoutGroup("Activation Settings", true, 0)]
	public float DistanceToTarget { get; private set; }

	public ShieldMaiden ShieldMaiden { get; private set; }

	public bool Awaken { get; private set; }

	public override void OnAwake()
	{
		base.OnAwake();
		ShieldMaiden = (ShieldMaiden)Entity;
	}

	public override void OnStart()
	{
		base.OnStart();
	}

	private void Update()
	{
		if (_waitPeriod > 0f)
		{
			_waitPeriod -= Time.deltaTime;
		}
		DistanceToTarget = Vector2.Distance(ShieldMaiden.transform.position, ShieldMaiden.Target.transform.position);
		if (!base.IsAttacking)
		{
			_currentAttackLapse += Time.deltaTime;
		}
		CheckParryCounter();
		if (DistanceToTarget <= ActivationDistance && !base.BehaviourTree.isRunning && !Awaken)
		{
			Awaken = true;
			base.BehaviourTree.StartBehaviour();
		}
	}

	private void CheckParryCounter()
	{
		if (_parryTime > 0f)
		{
			_parryTime -= Time.deltaTime;
		}
		else if (_parryTime < 0f)
		{
			_parryTime = 0f;
			_currentAttackLapse = 0f;
			ShieldMaiden.AnimatorInyector.ParryRecover();
		}
	}

	public bool IsOnWaitingPeriod()
	{
		return _waitPeriod > 0f;
	}

	private void SetRandomWait()
	{
		_waitPeriod = UnityEngine.Random.Range(0.3f, 0.5f);
	}

	public override void Idle()
	{
		StopMovement();
	}

	public bool TargetCanBeVisible()
	{
		float num = ShieldMaiden.Target.transform.position.y - ShieldMaiden.transform.position.y;
		num = ((!(num > 0f)) ? (0f - num) : num);
		return num <= MaxVisibleHeight;
	}

	public bool CanSeePenitent()
	{
		return ShieldMaiden.VisionCone.CanSeeTarget(GetTarget(), "Penitent");
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
		if (targetPos.x > ShieldMaiden.transform.position.x)
		{
			if (ShieldMaiden.Status.Orientation != 0)
			{
				ShieldMaiden.SetOrientation(EntityOrientation.Right);
			}
		}
		else if (ShieldMaiden.Status.Orientation != EntityOrientation.Left)
		{
			ShieldMaiden.SetOrientation(EntityOrientation.Left);
		}
	}

	public void OnBouncedBackByOverlapping()
	{
		if (ShieldMaiden.Input.HorizontalInput != 0f && CanSeePenitent() && CanSeePenitent())
		{
			_currentAttackLapse = UnityEngine.Random.Range(0f, AttackCoolDown / 2f);
		}
	}

	public void Chase(Vector3 position)
	{
		if (base.IsAttacking)
		{
			StopMovement();
			return;
		}
		LookAtTarget(position);
		float horizontalInput = ((Entity.Status.Orientation != 0) ? (-1f) : 1f);
		if (!motionChecker.HitsFloor)
		{
			StopMovement();
			return;
		}
		ShieldMaiden.Input.HorizontalInput = horizontalInput;
		ShieldMaiden.AnimatorInyector.Walk();
	}

	public override void Damage()
	{
	}

	public void OnShieldHit()
	{
		_currentAttackLapse = 0f;
	}

	public bool CanAttack()
	{
		return _currentAttackLapse >= AttackCoolDown;
	}

	public void ToggleShield(bool active)
	{
		ShieldMaiden.IsGuarding = active;
	}

	public void Death()
	{
		StopMovement();
		base.BehaviourTree.StopBehaviour();
		ShieldMaiden.AnimatorInyector.Death();
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
		if (!base.IsAttacking && !IsBeingParried())
		{
			StopMovement();
			Transform target = GetTarget();
			ShieldMaidenAttack.target = target;
			LookAtTarget(target.position);
			if (_currentAttackLapse >= AttackCoolDown)
			{
				_currentAttackLapse = GetRandomAttackLapse();
				ShieldMaiden.AnimatorInyector.Attack();
			}
		}
	}

	private float GetRandomAttackLapse()
	{
		return UnityEngine.Random.Range(-0.3f, 0f);
	}

	private bool IsBeingParried()
	{
		return _parryTime > 0f;
	}

	public void OnParry()
	{
		_currentAttackLapse = GetRandomAttackLapse();
		ShieldMaiden.AnimatorInyector.Parry();
		Core.Logic.CameraManager.ProCamera2DShake.ShakeUsingPreset("Parry");
		_parryTime = maxParryTime;
	}

	public override void StopMovement()
	{
		ShieldMaiden.AnimatorInyector.Stop();
		ShieldMaiden.Input.HorizontalInput = 0f;
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
		bool hitsPatrolBlock = motionChecker.HitsPatrolBlock;
		if (isBlocked || !hitsFloor || hitsPatrolBlock)
		{
			if (Time.time - _lastDirectionChange > 0.5f)
			{
				LookAtTarget(base.transform.position - num * Vector3.right);
				_lastDirectionChange = Time.time;
				return;
			}
			SetRandomWait();
		}
		ShieldMaiden.Input.HorizontalInput = num;
		ShieldMaiden.AnimatorInyector.Walk();
	}

	public override void Chase(Transform targetPosition)
	{
		throw new NotImplementedException();
	}

	public override void Execution()
	{
		base.Execution();
		ShieldMaiden.gameObject.layer = LayerMask.NameToLayer("Default");
		StopMovement();
		StopBehaviour();
		ShieldMaiden.SpriteRenderer.enabled = false;
		ShieldMaiden.EntExecution.InstantiateExecution();
		if (ShieldMaiden.EntExecution != null)
		{
			ShieldMaiden.EntExecution.enabled = true;
		}
	}

	public override void Alive()
	{
		base.Alive();
		if (!ShieldMaiden.Status.Dead)
		{
			StartBehaviour();
			ShieldMaiden.SpriteRenderer.enabled = true;
			ShieldMaiden.CurrentLife = ShieldMaiden.Stats.Life.Base / 2f;
			ShieldMaiden.gameObject.layer = LayerMask.NameToLayer("Enemy");
			if (ShieldMaiden.EntExecution != null)
			{
				ShieldMaiden.EntExecution.enabled = false;
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.magenta;
	}
}
