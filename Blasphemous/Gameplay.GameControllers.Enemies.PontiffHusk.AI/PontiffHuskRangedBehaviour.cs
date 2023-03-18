using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.HighWills.Attack;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.PontiffHusk.Animator;
using Gameplay.GameControllers.Enemies.PontiffHusk.Attack;
using Gameplay.GameControllers.Entities;
using NodeCanvas.BehaviourTrees;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.PontiffHusk.AI;

public class PontiffHuskRangedBehaviour : EnemyBehaviour
{
	[Serializable]
	public struct ShootAndMove
	{
		[Title("~ Segment ~", null, TitleAlignments.Left, true, true)]
		public GameObject MinePrefabToShoot;

		public Vector2 Movement;

		public float HorMovementTime;

		public float VerMovementTime;

		public Ease HorMovementEase;

		public Ease VerMovementEase;

		public float WaitTimeAfterMovement;
	}

	public PontiffHuskRangedVariantAttack BulletAttack;

	public float MaxRndTimeAttack = 6f;

	public float MinRndTimeAttack = 3f;

	public float MinTargetDistance = 1f;

	public float Speed = 5f;

	public float AttackSpeed = 10f;

	public float MaxDistanceFromOrigin = 40f;

	public float BackDashDistance = 0.3f;

	public float BackDashTime = 0.2f;

	public List<ShootAndMove> ShootingSequence = new List<ShootAndMove>();

	private PontiffHuskRanged _PontiffHuskRanged;

	private Transform _target;

	private float _attackTime;

	private float _time;

	private Tween normalHorMove;

	private int numMinesShooted;

	private List<RangedMine> rangedMineInstances = new List<RangedMine>();

	private Tween attackingHorMove;

	private Tween attackingVerMove;

	private Vector3 origin;

	private bool originSetted;

	private GameObject currentMinePrefab;

	public bool IsAwake { get; set; }

	public bool IsAppear { get; set; }

	public bool IsDisappearing { get; set; }

	public bool Asleep { get; private set; }

	public bool IsRamming => false;

	public PontiffHuskAnimatorInyector AnimatorInyector { get; private set; }

	[Button(ButtonSizes.Small)]
	public void AddAnotherSegmentToShootingSequence()
	{
		ShootAndMove shootAndMove = default(ShootAndMove);
		shootAndMove.Movement = Vector2.right;
		shootAndMove.HorMovementTime = 1f;
		shootAndMove.VerMovementTime = 1f;
		shootAndMove.HorMovementEase = Ease.OutBack;
		shootAndMove.VerMovementEase = Ease.InOutQuad;
		shootAndMove.WaitTimeAfterMovement = 1f;
		ShootAndMove item = shootAndMove;
		ShootingSequence.Add(item);
	}

	public override void OnAwake()
	{
		base.OnAwake();
		_PontiffHuskRanged = (PontiffHuskRanged)Entity;
		AnimatorInyector = _PontiffHuskRanged.GetComponentInChildren<PontiffHuskAnimatorInyector>();
	}

	private void OnLerpStop()
	{
		if (_PontiffHuskRanged.IsAttacking)
		{
			_PontiffHuskRanged.IsAttacking = false;
		}
		_PontiffHuskRanged.GhostTrail.EnableGhostTrail = false;
		_PontiffHuskRanged.FloatingMotion.IsFloating = true;
	}

	public override void OnStart()
	{
		base.OnStart();
		base.BehaviourTree = _PontiffHuskRanged.GetComponent<BehaviourTreeOwner>();
		_PontiffHuskRanged.OnDeath += PontiffHuskRangedOnEntityDie;
		MotionLerper motionLerper = _PontiffHuskRanged.MotionLerper;
		motionLerper.OnLerpStop = (Core.SimpleEvent)Delegate.Combine(motionLerper.OnLerpStop, new Core.SimpleEvent(OnLerpStop));
		if (_PontiffHuskRanged.TargetDistance > _PontiffHuskRanged.ActivationRange)
		{
			Disappear(0f);
		}
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (_PontiffHuskRanged.Target == null || IsDisappearing || IsDead())
		{
			return;
		}
		if (_time < _attackTime && IsAppear)
		{
			_time += Time.deltaTime;
		}
		if (base.IsAttacking)
		{
			if (numMinesShooted > 0)
			{
				if (!AttackOnCooldown())
				{
					Shoot();
				}
				if (numMinesShooted == ShootingSequence.Count)
				{
					AnimatorInyector.StopShootingMines();
					_PontiffHuskRanged.IsAttacking = false;
					numMinesShooted = 0;
				}
			}
		}
		else if (ShouldReturnToOrigin())
		{
			ResetState();
		}
	}

	private bool ShouldReturnToOrigin()
	{
		return Vector2.Distance(origin, base.transform.position) > MaxDistanceFromOrigin;
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
		if (AnimatorInyector.IsFading || !_PontiffHuskRanged.gameObject.activeSelf || !_PontiffHuskRanged.Target || IsDisappearing || attackingHorMove != null || attackingVerMove != null)
		{
			return;
		}
		if (_PontiffHuskRanged.SpriteRenderer.color.a < 1f && !base.IsChasing)
		{
			AnimatorInyector.Fade(1f, 0.3f);
			_PontiffHuskRanged.Audio.Appear();
			AnimatorInyector.PlayAppear();
		}
		if (_target == null)
		{
			_target = _PontiffHuskRanged.Target.transform;
		}
		LookAtTarget(_target.position);
		base.IsChasing = true;
		if (normalHorMove == null)
		{
			int num = ((Entity.Status.Orientation == EntityOrientation.Left) ? 1 : (-1));
			float endValue = base.transform.position.x + (float)num * Speed;
			normalHorMove = base.transform.DOMoveX(endValue, 1f).SetEase(Ease.Linear).OnComplete(delegate
			{
				normalHorMove = null;
			});
		}
		_PontiffHuskRanged.Audio.UpdateFloatingPanning();
	}

	public void MoveAfterShooting()
	{
		if (attackingHorMove == null && attackingVerMove == null)
		{
			if (normalHorMove != null)
			{
				normalHorMove.Kill(complete: true);
			}
			int index = numMinesShooted - 1;
			ShootAndMove shootAndMove = ShootingSequence[index];
			float endValue = base.transform.position.x + shootAndMove.Movement.x;
			float horMovementTime = shootAndMove.HorMovementTime;
			Ease horMovementEase = shootAndMove.HorMovementEase;
			attackingHorMove = base.transform.DOMoveX(endValue, horMovementTime).SetEase(horMovementEase).OnComplete(delegate
			{
				attackingHorMove = null;
			});
			float endValue2 = base.transform.position.y + shootAndMove.Movement.y;
			float verMovementTime = shootAndMove.VerMovementTime;
			Ease verMovementEase = shootAndMove.VerMovementEase;
			attackingVerMove = base.transform.DOMoveY(endValue2, verMovementTime).SetEase(verMovementEase).OnComplete(delegate
			{
				attackingVerMove = null;
			});
			_attackTime = shootAndMove.WaitTimeAfterMovement + Mathf.Max(horMovementTime, verMovementTime);
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

	public bool CanSeePenitent()
	{
		return _PontiffHuskRanged.VisionCone.CanSeeTarget(Core.Logic.Penitent.transform, "Penitent");
	}

	public bool IsTargetInsideShootingRange()
	{
		return GetDistanceToTarget() < MinTargetDistance && CanSeePenitent();
	}

	public bool CanShoot()
	{
		return IsTargetInsideShootingRange() && !AttackOnCooldown();
	}

	public override void Attack()
	{
		if (!(_target == null))
		{
			BulletVariantAttack();
		}
	}

	private void BulletVariantAttack()
	{
		if (!base.IsAttacking)
		{
			_PontiffHuskRanged.Audio.StopFloating();
			_PontiffHuskRanged.FloatingMotion.IsFloating = true;
			LookAtTarget(_target.position);
			AnimatorInyector.StartShootingMines();
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
		if (!AnimatorInyector.IsFading)
		{
			SetColliderScale();
			if (!IsAppear)
			{
				IsAwake = true;
				AnimatorInyector.Appear(time);
				_PontiffHuskRanged.Audio.Appear();
				origin = base.transform.position;
				originSetted = true;
				numMinesShooted = 0;
				rangedMineInstances.Clear();
				currentMinePrefab = ShootingSequence[0].MinePrefabToShoot;
			}
		}
	}

	public void Disappear(float time)
	{
		if (!AnimatorInyector.IsFading)
		{
			IsDisappearing = true;
			IsAwake = false;
			AnimatorInyector.Disappear(time);
			_PontiffHuskRanged.Audio.Dissapear();
			StartCoroutine(AfterDisappear(time));
			_PontiffHuskRanged.BehaviourTree.enabled = false;
		}
	}

	private IEnumerator AfterDisappear(float disappearTime)
	{
		yield return new WaitForSeconds(disappearTime);
		IsDisappearing = false;
		yield return null;
		_PontiffHuskRanged.BehaviourTree.enabled = true;
	}

	public void HurtDisplacement(GameObject attackingEntity)
	{
		if (_PontiffHuskRanged.MotionLerper.IsLerping)
		{
			_PontiffHuskRanged.MotionLerper.StopLerping();
		}
		Vector3 position = attackingEntity.transform.position;
		if (attackingEntity.name.Equals(Entity.name))
		{
			position = Core.Logic.Penitent.GetPosition();
		}
		Vector2 vector = ((!(position.x >= base.transform.position.x)) ? Vector2.right : Vector2.left);
		_PontiffHuskRanged.GhostTrail.EnableGhostTrail = true;
		_PontiffHuskRanged.MotionLerper.distanceToMove = 3f;
		_PontiffHuskRanged.MotionLerper.TimeTakenDuringLerp = 0.5f;
		_PontiffHuskRanged.MotionLerper.StartLerping(vector);
	}

	public void Shoot()
	{
		_time = 0f;
		BulletAttack.MinePrefab = currentMinePrefab;
		rangedMineInstances.Add(BulletAttack.Shoot());
		RangedMine priorMine = null;
		foreach (RangedMine rangedMineInstance in rangedMineInstances)
		{
			rangedMineInstance.SetPriorMine(priorMine);
			priorMine = rangedMineInstance;
		}
		numMinesShooted++;
		if (numMinesShooted < ShootingSequence.Count)
		{
			currentMinePrefab = ShootingSequence[numMinesShooted].MinePrefabToShoot;
		}
		MoveAfterShooting();
	}

	private void PontiffHuskRangedOnEntityDie()
	{
		_PontiffHuskRanged.OnDeath -= PontiffHuskRangedOnEntityDie;
		if (_PontiffHuskRanged.AttackArea != null)
		{
			_PontiffHuskRanged.AttackArea.WeaponCollider.enabled = false;
		}
		_PontiffHuskRanged.EntityDamageArea.DamageAreaCollider.enabled = false;
		if (base.BehaviourTree.isRunning)
		{
			base.BehaviourTree.StopBehaviour();
		}
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
	}

	private void SetColliderScale()
	{
		_PontiffHuskRanged.EntityDamageArea.DamageAreaCollider.transform.localScale = new Vector3((_PontiffHuskRanged.Status.Orientation == EntityOrientation.Right) ? 1 : (-1), 1f, 1f);
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
	}

	private void OnDestroy()
	{
		if ((bool)_PontiffHuskRanged)
		{
			MotionLerper motionLerper = _PontiffHuskRanged.MotionLerper;
			motionLerper.OnLerpStop = (Core.SimpleEvent)Delegate.Remove(motionLerper.OnLerpStop, new Core.SimpleEvent(OnLerpStop));
		}
	}

	public void ResetState()
	{
		if (originSetted)
		{
			base.gameObject.SetActive(value: true);
			_PontiffHuskRanged.Stats.Life.SetToCurrentMax();
			_PontiffHuskRanged.Status.Dead = false;
			_PontiffHuskRanged.IsAttacking = false;
			AnimatorInyector.StopShootingMines();
			float num = 0.5f;
			Disappear(num);
			StartCoroutine(AfterDisappearResetState(num));
		}
	}

	private IEnumerator AfterDisappearResetState(float disappearTime)
	{
		yield return new WaitForSeconds(disappearTime);
		base.transform.DOKill(complete: true);
		base.transform.position = origin;
		AnimatorInyector.EntityAnimator.Play("IDLE");
	}
}
