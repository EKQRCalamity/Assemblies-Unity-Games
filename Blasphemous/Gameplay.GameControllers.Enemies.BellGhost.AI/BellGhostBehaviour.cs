using System;
using System.Collections;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.BellGhost.Animator;
using Gameplay.GameControllers.Enemies.BellGhost.Attack;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Entities;
using NodeCanvas.BehaviourTrees;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.BellGhost.AI;

public class BellGhostBehaviour : EnemyBehaviour
{
	private float _attackTime;

	private BellGhost _bellGhost;

	private bool _chargingAttack;

	private Transform _target;

	private float _time;

	private Vector3 _velocity = Vector3.zero;

	public BELL_GHOST_TYPES bellGhostVariant;

	public EnemyAttack bulletAttack;

	public float ChasingElongation = 0.5f;

	public float ElapseTimeBeforeRamming = 0.25f;

	public float MaxDistanceToOrigin = 10f;

	public float MaxRndTimeAttack = 6f;

	public float MinRndTimeAttack = 3f;

	public float MinTargetDistance = 1f;

	public float FleeDistance = 2f;

	public float Speed = 5f;

	public float AttackSpeed = 10f;

	private Vector2 randomOffset;

	private Vector2 minMaxOffsetX;

	private Vector2 minMaxOffsetY;

	public bool IsAwake { get; set; }

	public bool IsAppear { get; private set; }

	public bool Asleep { get; private set; }

	public Vector3 Origin { get; set; }

	public bool IsOutReach { get; set; }

	public bool IsBackToOrigin { get; set; }

	public bool IsRamming { get; set; }

	public BellGhostAnimatorInyector AnimatorInyector { get; private set; }

	public override void OnAwake()
	{
		base.OnAwake();
		_bellGhost = (BellGhost)Entity;
		AnimatorInyector = _bellGhost.GetComponentInChildren<BellGhostAnimatorInyector>();
		minMaxOffsetX = new Vector2(-4f, 4f);
		minMaxOffsetY = new Vector2(-5f, 5f);
		randomOffset = new Vector2(UnityEngine.Random.Range(minMaxOffsetX.x, minMaxOffsetX.y), UnityEngine.Random.Range(minMaxOffsetY.x, minMaxOffsetY.y));
	}

	private void OnLerpStop()
	{
		if (_bellGhost.IsAttacking)
		{
			_bellGhost.IsAttacking = false;
		}
		_bellGhost.GhostTrail.EnableGhostTrail = false;
		_bellGhost.FloatingMotion.IsFloating = true;
		IsBackToOrigin = false;
		IsRamming = false;
		if (bellGhostVariant == BELL_GHOST_TYPES.RAMMING)
		{
			_bellGhost.Audio.StopAttack();
			_bellGhost.Audio.StopChargeAttack();
		}
	}

	public override void OnStart()
	{
		base.OnStart();
		SetRndTimeAttack();
		base.BehaviourTree = _bellGhost.GetComponent<BehaviourTreeOwner>();
		_bellGhost.OnDeath += BellGhostOnEntityDie;
		MotionLerper motionLerper = _bellGhost.MotionLerper;
		motionLerper.OnLerpStop = (Core.SimpleEvent)Delegate.Combine(motionLerper.OnLerpStop, new Core.SimpleEvent(OnLerpStop));
		if (_bellGhost.TargetDistance > _bellGhost.ActivationRange)
		{
			Disappear(0f);
		}
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (_bellGhost.Target == null)
		{
			return;
		}
		_chargingAttack = false;
		if (IsAwake)
		{
			IsOutReach = CheckDistanceToOrigin() >= MaxDistanceToOrigin;
			if (Core.Logic.CurrentState == LogicStates.PlayerDead && _bellGhost.BellGhostAttack != null)
			{
				_bellGhost.BellGhostAttack.EnableWeaponAreaCollider = false;
				SetAsleep();
			}
			if (bellGhostVariant == BELL_GHOST_TYPES.RAMMING)
			{
				_chargingAttack = AnimatorInyector.EntityAnimator.GetCurrentAnimatorStateInfo(0).IsName("ReadyAttack");
			}
		}
		if (bellGhostVariant == BELL_GHOST_TYPES.BULLET)
		{
			if (_time < _attackTime)
			{
				_time += Time.deltaTime;
			}
			if (base.IsAttacking)
			{
				MoveWhileAttacking();
			}
		}
		else
		{
			if (!base.IsChasing || _bellGhost.Status.Dead)
			{
				return;
			}
			_time += Time.deltaTime;
			if (_time >= _attackTime)
			{
				SetRndTimeAttack();
				if (_bellGhost.Status.IsVisibleOnCamera)
				{
					_bellGhost.IsAttacking = true;
				}
				else if (_chargingAttack)
				{
					_bellGhost.IsAttacking = true;
				}
				else
				{
					_bellGhost.IsAttacking = false;
				}
			}
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
		if (!AnimatorInyector.IsFading && _bellGhost.gameObject.activeSelf && (bool)_bellGhost.Target)
		{
			if (_bellGhost.SpriteRenderer.color.a < 1f && !base.IsChasing)
			{
				AnimatorInyector.Fade(1f, 0.3f);
				_bellGhost.Audio.Appear();
			}
			if (_target == null)
			{
				_target = _bellGhost.Target.transform;
			}
			if (bellGhostVariant == BELL_GHOST_TYPES.BULLET)
			{
				LookAtTarget(_target.position);
			}
			base.IsChasing = true;
			float num = Vector2.Distance(base.transform.position, _target.position);
			if (num > MinTargetDistance)
			{
				float num2 = Mathf.Sign(_target.position.x - base.transform.position.x);
				Vector2 vector = ((bellGhostVariant != BELL_GHOST_TYPES.BULLET) ? ((Vector2)new Vector3(_target.position.x + num2, _target.position.y + 5f)) : ((Vector2)new Vector3(_target.position.x + num2 * 8f + randomOffset.x, _target.position.y + 2f + randomOffset.y)));
				base.transform.position = Vector3.SmoothDamp(base.transform.position, vector, ref _velocity, ChasingElongation, Speed);
			}
			else if (num < FleeDistance && bellGhostVariant == BELL_GHOST_TYPES.BULLET)
			{
				int num3 = ((Entity.Status.Orientation == EntityOrientation.Left) ? 1 : (-1));
				Vector3 target = new Vector3(_target.position.x + (float)(num3 * 8) + randomOffset.x, _target.position.y + 1f + randomOffset.y);
				base.transform.position = Vector3.SmoothDamp(base.transform.position, target, ref _velocity, ChasingElongation, Speed * 0.5f);
			}
			_bellGhost.Audio.UpdateFloatingPanning();
		}
	}

	private void MoveWhileAttacking()
	{
		float num = Vector2.Distance(base.transform.position, _target.position);
		if (num < FleeDistance * 3f)
		{
			int num2 = ((Entity.Status.Orientation == EntityOrientation.Left) ? 1 : (-1));
			Vector3 target = new Vector3(_target.position.x + (float)(num2 * 8), _target.position.y + 1f);
			base.transform.position = Vector3.SmoothDamp(base.transform.position, target, ref _velocity, ChasingElongation, Speed * 0.75f);
		}
	}

	public float GetDistanceToTarget()
	{
		_target = GetTarget();
		if (_target == null)
		{
			return 1000f;
		}
		return Vector3.Distance(_target.position, base.transform.position);
	}

	public bool IsTargetInsideShootingRange()
	{
		return GetDistanceToTarget() < MinTargetDistance;
	}

	public bool CanShoot()
	{
		return IsTargetInsideShootingRange() && !AttackOnCooldown();
	}

	public float CheckDistanceToOrigin()
	{
		return Vector3.Distance(base.transform.position, Origin);
	}

	public override void Attack()
	{
		if (!(_target == null))
		{
			switch (bellGhostVariant)
			{
			case BELL_GHOST_TYPES.RAMMING:
				RammingVariantAttack();
				break;
			case BELL_GHOST_TYPES.BULLET:
				BulletVariantAttack();
				break;
			}
		}
	}

	private void RammingVariantAttack()
	{
		_bellGhost.Audio.UpdateAttackPanning();
		if (!_bellGhost.MotionLerper.IsLerping && !IsRamming)
		{
			_bellGhost.Audio.PlayAttack();
			_bellGhost.Audio.StopFloating();
			IsRamming = true;
			_bellGhost.FloatingMotion.IsFloating = false;
			LookAtTarget(_target.position);
			AnimatorInyector.Attack();
		}
	}

	private void BulletVariantAttack()
	{
		if (!base.IsAttacking)
		{
			_bellGhost.Audio.StopFloating();
			_bellGhost.FloatingMotion.IsFloating = true;
			LookAtTarget(_target.position);
			AnimatorInyector.Attack();
		}
	}

	public bool AttackOnCooldown()
	{
		return _time < _attackTime;
	}

	public override void Damage()
	{
		throw new NotImplementedException();
	}

	public override void StopMovement()
	{
		throw new NotImplementedException();
	}

	public void Appear(float time)
	{
		SetColliderScale();
		if (!IsAppear)
		{
			IsAppear = true;
			IsAwake = true;
			AnimatorInyector.Appear(time);
			_bellGhost.Audio.Appear();
		}
	}

	public void Disappear(float time)
	{
		IsAppear = false;
		IsAwake = false;
		AnimatorInyector.Disappear(time);
		_bellGhost.Audio.Dissapear();
	}

	public void MoveToOrigin()
	{
		if (!IsBackToOrigin)
		{
			IsBackToOrigin = true;
			base.IsChasing = false;
			SetRndTimeAttack();
			StartCoroutine(MoveToOriginCoroutine());
		}
	}

	public void HurtDisplacement(GameObject attackingEntity)
	{
		if (_bellGhost.MotionLerper.IsLerping)
		{
			_bellGhost.MotionLerper.StopLerping();
		}
		Vector2 vector = ((!(attackingEntity.transform.position.x >= base.transform.position.x)) ? Vector2.right : (-Vector2.right));
		_bellGhost.GhostTrail.EnableGhostTrail = true;
		_bellGhost.MotionLerper.distanceToMove = 3f;
		_bellGhost.MotionLerper.TimeTakenDuringLerp = 0.5f;
		_bellGhost.MotionLerper.StartLerping(vector);
	}

	private IEnumerator MoveToOriginCoroutine()
	{
		float disappearTime = 1f;
		Disappear(disappearTime);
		yield return new WaitForSeconds(disappearTime);
		base.transform.position = Origin;
		IsOutReach = false;
		IsAppear = false;
	}

	public void Ramming()
	{
		LookAtTarget(_target.position);
		_bellGhost.GhostTrail.EnableGhostTrail = true;
		float num = Vector2.Distance(base.transform.position, _target.position) * 1.5f;
		Vector3 normalized = (_target.position - base.transform.position).normalized;
		_bellGhost.MotionLerper.distanceToMove = num;
		_bellGhost.MotionLerper.TimeTakenDuringLerp = num / AttackSpeed;
		_bellGhost.MotionLerper.StartLerping(normalized);
	}

	public void Shoot()
	{
		((BellGhostVariantAttack)bulletAttack).target = _target;
		bulletAttack.CurrentWeaponAttack();
		OnBulletShot();
	}

	private void OnBulletShot()
	{
		_bellGhost.FloatingMotion.IsFloating = true;
		IsBackToOrigin = false;
		SetRndTimeAttack();
	}

	private void SetRndTimeAttack()
	{
		_time = 0f;
		_attackTime = UnityEngine.Random.Range(MinRndTimeAttack, MaxRndTimeAttack);
	}

	private void BellGhostOnEntityDie()
	{
		_bellGhost.OnDeath -= BellGhostOnEntityDie;
		if (_bellGhost.AttackArea != null)
		{
			_bellGhost.AttackArea.WeaponCollider.enabled = false;
		}
		_bellGhost.EntityDamageArea.DamageAreaCollider.enabled = false;
		if (base.BehaviourTree.isRunning)
		{
			base.BehaviourTree.StopBehaviour();
		}
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
		if (Entity.Status.Dead || AnimatorInyector.IsTurning)
		{
			return;
		}
		SetColliderScale();
		if (Entity.transform.position.x >= targetPos.x + 1f)
		{
			if (Entity.Status.Orientation != EntityOrientation.Left)
			{
				if (OnTurning != null)
				{
					OnTurning();
				}
				_bellGhost.SetOrientation(EntityOrientation.Left, allowFlipRenderer: false);
			}
		}
		else if (Entity.transform.position.x < targetPos.x - 1f && Entity.Status.Orientation != 0)
		{
			if (OnTurning != null)
			{
				OnTurning();
			}
			_bellGhost.SetOrientation(EntityOrientation.Right, allowFlipRenderer: false);
		}
	}

	private void SetColliderScale()
	{
		if (bellGhostVariant == BELL_GHOST_TYPES.BULLET)
		{
			_bellGhost.EntityDamageArea.DamageAreaCollider.transform.localScale = new Vector3((_bellGhost.Status.Orientation == EntityOrientation.Right) ? 1 : (-1), 1f, 1f);
		}
	}

	public void SetAsleep()
	{
		if (!Asleep)
		{
			Asleep = true;
		}
		if (base.BehaviourTree.isRunning)
		{
			base.BehaviourTree.StopBehaviour();
		}
		MoveToOrigin();
	}

	private void OnDestroy()
	{
		if ((bool)_bellGhost)
		{
			MotionLerper motionLerper = _bellGhost.MotionLerper;
			motionLerper.OnLerpStop = (Core.SimpleEvent)Delegate.Remove(motionLerper.OnLerpStop, new Core.SimpleEvent(OnLerpStop));
		}
	}
}
