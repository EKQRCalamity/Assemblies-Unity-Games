using System;
using System.Collections;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.PontiffHusk.Animator;
using Gameplay.GameControllers.Entities;
using NodeCanvas.BehaviourTrees;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.PontiffHusk.AI;

public class PontiffHuskMeleeBehaviour : EnemyBehaviour
{
	private float _attackTime;

	private PontiffHuskMelee _PontiffHuskMelee;

	private bool _chargingAttack;

	private Transform _target;

	private float _time;

	private Vector3 _velocity = Vector3.zero;

	private float _disappearedTime;

	public EnemyAttack bulletAttack;

	public float ChasingElongation = 0.5f;

	public float ElapseTimeBeforeRamming = 0.25f;

	public float MaxDistanceToOrigin = 10f;

	public float MaxRndTimeAttack = 6f;

	public float MinRndTimeAttack = 3f;

	public float MinTargetDistance = 1f;

	public float FleeDistance = 2f;

	public float MinTimeDisappeared = 2f;

	public float Speed = 5f;

	public float AttackSpeed = 10f;

	public float RammingDistance = 14f;

	public float ChaseHorizontalOffset = 8f;

	public float ChaseVerticalOffset = -2f;

	private Vector2 randomOffset;

	private Vector2 minMaxOffsetX;

	private Vector2 minMaxOffsetY;

	public bool IsAwake { get; set; }

	public bool IsAppear { get; set; }

	public bool Asleep { get; private set; }

	public Vector3 Origin { get; set; }

	public bool IsOutReach { get; set; }

	public bool IsBackToOrigin { get; set; }

	public bool IsRamming { get; set; }

	public PontiffHuskAnimatorInyector AnimatorInyector { get; private set; }

	public override void OnAwake()
	{
		base.OnAwake();
		_PontiffHuskMelee = (PontiffHuskMelee)Entity;
		AnimatorInyector = _PontiffHuskMelee.GetComponentInChildren<PontiffHuskAnimatorInyector>();
		minMaxOffsetX = new Vector2(-4f, 4f);
		minMaxOffsetY = new Vector2(-5f, 5f);
		randomOffset = new Vector2(UnityEngine.Random.Range(minMaxOffsetX.x, minMaxOffsetX.y), UnityEngine.Random.Range(minMaxOffsetY.x, minMaxOffsetY.y));
	}

	private void OnLerpStop()
	{
		if (_PontiffHuskMelee.IsAttacking)
		{
			_PontiffHuskMelee.IsAttacking = false;
		}
		_PontiffHuskMelee.FloatingMotion.IsFloating = true;
		IsBackToOrigin = false;
		IsRamming = false;
		_PontiffHuskMelee.Audio.StopAttack();
		_PontiffHuskMelee.Audio.StopChargeAttack();
	}

	public override void OnStart()
	{
		base.OnStart();
		SetRndTimeAttack();
		base.BehaviourTree = _PontiffHuskMelee.GetComponent<BehaviourTreeOwner>();
		_PontiffHuskMelee.OnDeath += PontiffHuskMeleeOnEntityDie;
		MotionLerper motionLerper = _PontiffHuskMelee.MotionLerper;
		motionLerper.OnLerpStop = (Core.SimpleEvent)Delegate.Combine(motionLerper.OnLerpStop, new Core.SimpleEvent(OnLerpStop));
		if (_PontiffHuskMelee.TargetDistance > _PontiffHuskMelee.ActivationRange)
		{
			Disappear(0f);
		}
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (_PontiffHuskMelee.Target == null)
		{
			return;
		}
		_chargingAttack = false;
		if (IsAwake)
		{
			IsOutReach = CheckDistanceToOrigin() >= MaxDistanceToOrigin;
			if (Core.Logic.CurrentState == LogicStates.PlayerDead && _PontiffHuskMelee.PontiffHuskMeleeAttack != null)
			{
				_PontiffHuskMelee.PontiffHuskMeleeAttack.EnableWeaponAreaCollider = false;
				SetAsleep();
			}
			_chargingAttack = AnimatorInyector.EntityAnimator.GetCurrentAnimatorStateInfo(0).IsName("ReadyAttack");
			if (!IsAppear && !AnimatorInyector.IsFading)
			{
				_disappearedTime += Time.deltaTime;
			}
		}
		else
		{
			_PontiffHuskMelee.PontiffHuskMeleeAttack.EnableWeaponAreaCollider = false;
		}
		if (base.IsChasing && !_PontiffHuskMelee.Status.Dead)
		{
			if (!IsAppear)
			{
				return;
			}
			_time += Time.deltaTime;
			if (!(_time >= _attackTime))
			{
				return;
			}
			SetRndTimeAttack();
			if (_PontiffHuskMelee.Status.IsVisibleOnCamera)
			{
				_PontiffHuskMelee.IsAttacking = true;
			}
			else if (_chargingAttack)
			{
				_PontiffHuskMelee.IsAttacking = true;
			}
			else
			{
				_PontiffHuskMelee.IsAttacking = false;
			}
		}
		if (AnimatorInyector.IsFading)
		{
			_PontiffHuskMelee.PontiffHuskMeleeAttack.EnableWeaponAreaCollider = false;
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
		if (!AnimatorInyector.IsFading && _PontiffHuskMelee.gameObject.activeSelf && (bool)_PontiffHuskMelee.Target)
		{
			if (_PontiffHuskMelee.SpriteRenderer.color.a < 1f && !base.IsChasing)
			{
				AnimatorInyector.Fade(1f, 0.3f);
				_PontiffHuskMelee.Audio.Appear();
			}
			if (_target == null)
			{
				_target = _PontiffHuskMelee.Target.transform;
			}
			base.IsChasing = true;
			float num = Vector2.Distance(base.transform.position, _target.position);
			if (num > MinTargetDistance)
			{
				float num2 = ((Entity.Status.Orientation != EntityOrientation.Left) ? (0f - ChaseHorizontalOffset) : ChaseHorizontalOffset);
				Vector2 vector = new Vector3(_target.position.x + num2, _target.position.y + ChaseVerticalOffset);
				base.transform.position = Vector3.SmoothDamp(base.transform.position, vector, ref _velocity, ChasingElongation, Speed);
			}
			_PontiffHuskMelee.Audio.UpdateFloatingPanning();
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
			RammingVariantAttack();
		}
	}

	private void RammingVariantAttack()
	{
		_PontiffHuskMelee.Audio.UpdateAttackPanning();
		if (!_PontiffHuskMelee.MotionLerper.IsLerping && !IsRamming)
		{
			_PontiffHuskMelee.Audio.PlayAttack();
			_PontiffHuskMelee.Audio.StopFloating();
			IsRamming = true;
			_PontiffHuskMelee.FloatingMotion.IsFloating = false;
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
		if (!(_disappearedTime < MinTimeDisappeared) || !IsAwake)
		{
			if (_PontiffHuskMelee.Target == null)
			{
				_PontiffHuskMelee.Target = Core.Logic.Penitent.gameObject;
			}
			if (_target == null)
			{
				_target = _PontiffHuskMelee.Target.transform;
			}
			LookAtTarget(_target.position, force: true);
			SetColliderScale();
			if (!IsAppear)
			{
				IsAppear = true;
				IsAwake = true;
				AnimatorInyector.Appear(time);
				_PontiffHuskMelee.Audio.Appear();
			}
		}
	}

	public void Disappear(float time)
	{
		IsAppear = false;
		IsAwake = false;
		AnimatorInyector.Disappear(time);
		_PontiffHuskMelee.Audio.Dissapear();
		_disappearedTime = 0f;
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
		float rammingDistance = RammingDistance;
		Vector2 vector = ((Entity.Status.Orientation != EntityOrientation.Left) ? Vector2.right : Vector2.left);
		_PontiffHuskMelee.MotionLerper.distanceToMove = rammingDistance;
		_PontiffHuskMelee.MotionLerper.TimeTakenDuringLerp = rammingDistance / AttackSpeed;
		_PontiffHuskMelee.MotionLerper.StartLerping(vector);
	}

	private void SetRndTimeAttack()
	{
		_time = 0f;
		_attackTime = UnityEngine.Random.Range(MinRndTimeAttack, MaxRndTimeAttack);
	}

	private void PontiffHuskMeleeOnEntityDie()
	{
		_PontiffHuskMelee.OnDeath -= PontiffHuskMeleeOnEntityDie;
		if (_PontiffHuskMelee.AttackArea != null)
		{
			_PontiffHuskMelee.AttackArea.WeaponCollider.enabled = false;
		}
		_PontiffHuskMelee.EntityDamageArea.DamageAreaCollider.enabled = false;
		if (base.BehaviourTree.isRunning)
		{
			base.BehaviourTree.StopBehaviour();
		}
	}

	public void LookAtTarget(Vector3 targetPos, bool force)
	{
		if (force || !CheckLookAtTargetConditions())
		{
			SetColliderScale();
			SetOrientation(targetPos);
		}
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
		if (CheckLookAtTargetConditions())
		{
			SetColliderScale();
			SetOrientation(targetPos);
		}
	}

	private bool CheckLookAtTargetConditions()
	{
		return !Entity.Status.Dead && !AnimatorInyector.IsFading && !base.IsAttacking && IsAppear;
	}

	public void SetOrientation(Vector3 targetPos)
	{
		if (Entity.transform.position.x >= targetPos.x + 1f)
		{
			if (Entity.Status.Orientation != EntityOrientation.Left)
			{
				_PontiffHuskMelee.SetOrientation(EntityOrientation.Left, allowFlipRenderer: false);
			}
		}
		else if (Entity.transform.position.x < targetPos.x - 1f && Entity.Status.Orientation != 0)
		{
			_PontiffHuskMelee.SetOrientation(EntityOrientation.Right, allowFlipRenderer: false);
		}
	}

	private void SetColliderScale()
	{
		_PontiffHuskMelee.EntityDamageArea.DamageAreaCollider.transform.localScale = new Vector3((_PontiffHuskMelee.Status.Orientation == EntityOrientation.Right) ? 1 : (-1), 1f, 1f);
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
		if ((bool)_PontiffHuskMelee)
		{
			MotionLerper motionLerper = _PontiffHuskMelee.MotionLerper;
			motionLerper.OnLerpStop = (Core.SimpleEvent)Delegate.Remove(motionLerper.OnLerpStop, new Core.SimpleEvent(OnLerpStop));
		}
	}
}
