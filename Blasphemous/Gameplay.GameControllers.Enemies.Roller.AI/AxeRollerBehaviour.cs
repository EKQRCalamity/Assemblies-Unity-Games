using System;
using DG.Tweening;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Roller.Attack;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Roller.AI;

public class AxeRollerBehaviour : EnemyBehaviour
{
	[FoldoutGroup("Activation Settings", true, 0)]
	public float EscapeDistance;

	[FoldoutGroup("Activation Settings", true, 0)]
	public float FirstProjectileWaitTime = 1f;

	[FoldoutGroup("Activation Settings", true, 0)]
	public float ProjectileCoolDown = 1f;

	[FoldoutGroup("Activation Settings", true, 0)]
	public float MaxRollingTime;

	[SerializeField]
	[FoldoutGroup("Design settings", 0)]
	private LayerMask keepRollingLayerMask;

	[FoldoutGroup("Hurt", 0)]
	[Tooltip("Displacement when the enemy is hit")]
	public float HurtDisplacement = 1f;

	[FoldoutGroup("Hurt", 0)]
	[Tooltip("Max number of hit reactions")]
	public int MaxHitReactions = 3;

	public const float HurtAnimDuration = 0.55f;

	private const float BackwardsRangeBlockDetection = -6f;

	private float currentCoolDown;

	private bool isExecuted;

	private float currentRollingTime;

	private int currentHitReactionsDone;

	private RaycastHit2D[] results;

	private bool hasAlreadyAttackedBefore;

	private float normalRangeBlockDetection;

	public AxeRoller Roller { get; private set; }

	[FoldoutGroup("Activation Settings", true, 0)]
	public float DistanceToTarget { get; private set; }

	public bool IsRolling { get; private set; }

	public bool IsChargingAttack { get; set; }

	public bool IsHurting { get; set; }

	public override void OnStart()
	{
		base.OnStart();
		Roller = (AxeRoller)Entity;
		results = new RaycastHit2D[2];
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (Roller == null || Roller.Status.Dead || isExecuted || IsHurting)
		{
			return;
		}
		DistanceToTarget = Vector2.Distance(Entity.transform.position, GetTarget().position);
		if (DistanceToTarget <= EscapeDistance && !IsRolling && !IsChargingAttack && currentCoolDown <= 0f && CanSeePenitent() && CanRoll())
		{
			AnticipateRoll();
			return;
		}
		if (IsRolling)
		{
			currentRollingTime += Time.deltaTime;
			if (HasReachedMaxTimeRolling() || !CanRoll())
			{
				if (HasAnotherRollerBehind())
				{
					RollBackwards();
				}
				else
				{
					StopMovement();
					IsChargingAttack = false;
					LookAtTarget(GetTarget().position);
				}
			}
		}
		if (IsChargingAttack || CanSeePenitent())
		{
			currentCoolDown += Time.deltaTime;
			if (currentCoolDown >= GetProjectileCoolDown())
			{
				currentCoolDown = 0f;
				IsChargingAttack = false;
				Roller.IsAttacking = true;
				Roller.AnimatorInjector.Attack();
				hasAlreadyAttackedBefore = true;
			}
		}
		if (CanSeePenitent() && !IsRolling)
		{
			LookAtTarget(GetTarget().position);
		}
	}

	private float GetProjectileCoolDown()
	{
		return (!hasAlreadyAttackedBefore) ? FirstProjectileWaitTime : ProjectileCoolDown;
	}

	private bool HasAnotherRollerBehind()
	{
		return Physics2D.RaycastNonAlloc(base.transform.position, Vector2.down, results, 0.1f, keepRollingLayerMask) > 1;
	}

	private bool CanRoll()
	{
		return !Roller.MotionChecker.HitsBlock && Roller.MotionChecker.HitsFloor;
	}

	public bool CanSeePenitent()
	{
		return Roller.VisionCone.CanSeeTarget(GetTarget(), "Penitent");
	}

	public void AnticipateRoll()
	{
		if (!(Roller.Input == null) && !IsRolling)
		{
			Roller.Audio.PlayAnticipateRoll();
			IsRolling = true;
			Roller.AnimatorInjector.Rolling(isRolling: true);
		}
	}

	public void Roll()
	{
		if (!(Roller.Input == null))
		{
			Roller.GhostTrailGenerator.EnableGhostTrail = true;
			AxeRollerMeleeAttack rollingAttack = Roller.RollingAttack;
			rollingAttack.OnAttackGuarded = (Core.SimpleEvent)Delegate.Combine(rollingAttack.OnAttackGuarded, new Core.SimpleEvent(RollBackwards));
			Roller.Input.HorizontalInput = ((Roller.Status.Orientation != 0) ? (-1f) : 1f);
			Roller.Audio.PlayRolling();
			Roller.DamageByContact = false;
			Roller.RollingAttack.damageOnEnterArea = true;
			Roller.RollingAttack.CurrentWeaponAttack();
		}
	}

	private void RollBackwards()
	{
		currentRollingTime = MaxRollingTime - 0.5f + UnityEngine.Random.Range(-0.1f, 0.1f);
		Roller.Input.HorizontalInput = ((Roller.Status.Orientation != 0) ? 1f : (-1f));
		if (Roller.MotionChecker.RangeBlockDetection != -6f)
		{
			normalRangeBlockDetection = Roller.MotionChecker.RangeBlockDetection;
			Roller.MotionChecker.RangeBlockDetection = -6f;
		}
	}

	public void HitDisplacement(Vector3 attakingEntityPos)
	{
		if (!IsRolling && !base.IsAttacking && !HasReachedMaxHitReactions())
		{
			IsHurting = true;
			currentHitReactionsDone++;
			Roller.AnimatorInjector.Damage();
			float num = ((!(Entity.transform.position.x >= attakingEntityPos.x)) ? (0f - HurtDisplacement) : HurtDisplacement);
			Roller.transform.DOMoveX(Roller.transform.position.x + num, 0.55f).OnComplete(delegate
			{
				IsHurting = false;
			});
		}
	}

	public void RollIfCantSeePenitent()
	{
		if (!IsRolling && !base.IsAttacking && !IsHurting && !CanSeePenitent())
		{
			if (!CanRoll())
			{
				Roller.SetOrientation((Roller.Status.Orientation == EntityOrientation.Right) ? EntityOrientation.Left : EntityOrientation.Right);
			}
			AnticipateRoll();
		}
	}

	private bool HasReachedMaxHitReactions()
	{
		return currentHitReactionsDone >= MaxHitReactions;
	}

	public override void StopMovement()
	{
		AxeRollerMeleeAttack rollingAttack = Roller.RollingAttack;
		rollingAttack.OnAttackGuarded = (Core.SimpleEvent)Delegate.Remove(rollingAttack.OnAttackGuarded, new Core.SimpleEvent(RollBackwards));
		IsRolling = false;
		currentRollingTime = 0f;
		Roller.Input.HorizontalInput = 0f;
		Roller.Controller.PlatformCharacterPhysics.Velocity = Vector3.zero;
		Roller.AnimatorInjector.Rolling(isRolling: false);
		Roller.Audio.StopRolling();
		Roller.GhostTrailGenerator.EnableGhostTrail = false;
		Roller.DamageByContact = true;
		Roller.RollingAttack.damageOnEnterArea = false;
		if (Roller.MotionChecker.RangeBlockDetection == -6f)
		{
			Roller.MotionChecker.RangeBlockDetection = normalRangeBlockDetection;
		}
	}

	public void OnDisable()
	{
		if ((bool)Roller && (bool)Roller.Audio)
		{
			Roller.Audio.StopRolling();
		}
	}

	public void StopAttackIfPenitentIsTooFar()
	{
		if (!CanSeePenitent())
		{
			Roller.IsAttacking = false;
			Roller.AnimatorInjector.StopAttack();
		}
	}

	private bool HasReachedMaxTimeRolling()
	{
		return currentRollingTime >= MaxRollingTime;
	}

	public override void Execution()
	{
		base.Execution();
		isExecuted = true;
		Roller.gameObject.layer = LayerMask.NameToLayer("Default");
		Roller.Audio.StopRolling();
		Roller.Animator.Play("Idle");
		StopMovement();
		Roller.SpriteRenderer.enabled = false;
		Core.Logic.Penitent.Audio.PlaySimpleHitToEnemy();
		Roller.AxeAttack.enabled = false;
		Roller.RollingAttack.enabled = false;
		Roller.EntExecution.InstantiateExecution();
		if (Roller.EntExecution != null)
		{
			Roller.EntExecution.enabled = true;
		}
	}

	public override void Alive()
	{
		base.Alive();
		isExecuted = false;
		Roller.gameObject.layer = LayerMask.NameToLayer("Enemy");
		Roller.SpriteRenderer.enabled = true;
		Roller.Animator.Play("Idle");
		Roller.CurrentLife = Roller.Stats.Life.Base / 2f;
		Roller.AxeAttack.enabled = true;
		Roller.RollingAttack.enabled = true;
		StartBehaviour();
		if (Roller.EntExecution != null)
		{
			Roller.EntExecution.enabled = false;
		}
	}

	public override void Idle()
	{
	}

	public override void Wander()
	{
	}

	public override void Chase(Transform targetPosition)
	{
	}

	public override void Damage()
	{
		throw new NotImplementedException();
	}

	public override void Attack()
	{
		throw new NotImplementedException();
	}
}
