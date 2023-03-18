using System;
using System.Collections.Generic;
using DamageEffect;
using DG.Tweening;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.Snake.Audio;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Snake;

public class Snake : Enemy, IDamageable, IPaintDamageableCollider
{
	public EnemyDamageArea TongueLeftOpenMouth;

	public EnemyDamageArea TongueRightOpenMouth;

	public EnemyDamageArea TongueLeftIdleMouth;

	public EnemyDamageArea TongueRightIdleMouth;

	public Collider2D GuardColliderLeftHead;

	public Collider2D GuardColliderRightHead;

	public Collider2D BarrierLeftHead;

	public Collider2D BarrierRightHead;

	public AnimationCurve SlowTimeCurve;

	public GameObject HeadLeft;

	public GameObject HeadRight;

	public GameObject Tail;

	public GameObject ChainLeft;

	public GameObject ChainRight;

	public List<SnakeSegmentVisualController> SnakeSegments;

	public SnakeSegmentsMovementController SnakeSegmentsMovementController;

	public SpriteRenderer HeadLeftSprite;

	public SpriteRenderer HeadRightSprite;

	public SpriteRenderer TongueLeftSprite;

	public SpriteRenderer TongueRightSprite;

	public DamageEffectScript TongueLeftDamageEffect;

	public DamageEffectScript TongueRightDamageEffect;

	public List<SpriteRenderer> ShadowMaskSprites = new List<SpriteRenderer>();

	[HideInInspector]
	public Animator TailAnimator;

	private SpriteRenderer TongueLeftOpenMouthRenderer;

	private SpriteRenderer TongueRightOpenMouthRenderer;

	private SpriteRenderer TongueLeftIdleMouthRenderer;

	private SpriteRenderer TongueRightIdleMouthRenderer;

	[SerializeField]
	[BoxGroup("Audio", true, false, 0)]
	[EventRef]
	private string FinalHit;

	private Vector2 baseGuardEffectOffset;

	public bool IsRightHeadVisible => HeadRightSprite.IsVisibleFrom(UnityEngine.Camera.main);

	public bool IsLeftHeadVisible => HeadLeftSprite.IsVisibleFrom(UnityEngine.Camera.main);

	public SnakeBehaviour Behaviour { get; private set; }

	public SnakeAudio Audio { get; private set; }

	public SnakeAnimatorInyector SnakeAnimatorInyector { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		Behaviour = GetComponent<SnakeBehaviour>();
		Audio = GetComponent<SnakeAudio>();
		SnakeAnimatorInyector = GetComponent<SnakeAnimatorInyector>();
		TailAnimator = Tail.GetComponent<Animator>();
		TongueLeftOpenMouthRenderer = TongueLeftOpenMouth.GetComponentInParent<SpriteRenderer>();
		TongueRightOpenMouthRenderer = TongueRightOpenMouth.GetComponentInParent<SpriteRenderer>();
		TongueLeftIdleMouthRenderer = TongueLeftIdleMouth.GetComponentInParent<SpriteRenderer>();
		TongueRightIdleMouthRenderer = TongueRightIdleMouth.GetComponentInParent<SpriteRenderer>();
		baseGuardEffectOffset = GuardEffectOffset;
		AttachShowScriptIfNeeded();
	}

	internal DamageArea GetActiveDamageArea()
	{
		if (IsCurrentlyDamageable())
		{
			if (Status.Orientation == EntityOrientation.Left)
			{
				if (TongueRightOpenMouth.DamageAreaCollider.enabled)
				{
					return TongueRightOpenMouth;
				}
				return TongueRightIdleMouth;
			}
			if (TongueLeftOpenMouth.DamageAreaCollider.enabled)
			{
				return TongueLeftOpenMouth;
			}
			return TongueLeftIdleMouth;
		}
		return null;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!Status.Dead)
		{
			bool attacking = ((bool)Behaviour.CurrentMeleeAttackLeftHead && Behaviour.CurrentMeleeAttackLeftHead.DealsDamage) || ((bool)Behaviour.CurrentMeleeAttackRightHead && Behaviour.CurrentMeleeAttackRightHead.DealsDamage);
			Collider2D guardColliderLeftHead = GuardColliderLeftHead;
			bool flag = !IsCurrentlyDamageable();
			GuardColliderRightHead.enabled = flag;
			guardColliderLeftHead.enabled = flag;
			EntityOrientation orientation = ((!IsLeftHeadVisible) ? EntityOrientation.Left : EntityOrientation.Right);
			SetOrientation(orientation, allowFlipRenderer: false);
			Sequence sequence = DOTween.Sequence();
			sequence.AppendInterval(0.2f);
			sequence.OnComplete(delegate
			{
				Collider2D barrierLeftHead = BarrierLeftHead;
				bool flag2 = !attacking;
				BarrierRightHead.enabled = flag2;
				barrierLeftHead.enabled = flag2;
			});
			sequence.Play();
		}
	}

	public bool IsCurrentlyDamageable()
	{
		return TongueLeftOpenMouth.DamageAreaCollider.enabled || TongueRightOpenMouth.DamageAreaCollider.enabled || TongueLeftIdleMouth.DamageAreaCollider.enabled || TongueRightIdleMouth.DamageAreaCollider.enabled;
	}

	public void AttachShowScriptIfNeeded()
	{
	}

	public float GetHpPercentage()
	{
		return Stats.Life.Current / Stats.Life.CurrentMax;
	}

	public void Damage(Hit hit)
	{
		if (hit.AttackingEntity.name.StartsWith("Snake") || Core.Logic.Penitent.Stats.Life.Current <= 0f || Status.Dead)
		{
			return;
		}
		base.IsGuarding = !IsCurrentlyDamageable();
		SetGuardEffectOffset(hit);
		if (!GuardHit(hit))
		{
			if (WillDieByHit(hit))
			{
				hit.HitSoundId = FinalHit;
			}
			TakeDamageByVisibleHead(hit);
			if (Status.Dead)
			{
				TongueLeftOpenMouth.DamageAreaCollider.enabled = false;
				TongueRightOpenMouth.DamageAreaCollider.enabled = false;
				TongueLeftIdleMouth.DamageAreaCollider.enabled = false;
				TongueRightIdleMouth.DamageAreaCollider.enabled = false;
				Behaviour.Death();
			}
			else
			{
				DamageFlash();
				Behaviour.Damage(hit);
				SleepTimeByHit(hit);
			}
		}
	}

	public Vector3 GetLeftDamagedPosition()
	{
		EnemyDamageArea enemyDamageArea = ((!TongueLeftOpenMouth.DamageAreaCollider.enabled) ? TongueLeftIdleMouth : TongueLeftOpenMouth);
		return enemyDamageArea.DamageAreaCollider.bounds.center;
	}

	public Vector3 GetRightDamagedPosition()
	{
		EnemyDamageArea enemyDamageArea = ((!TongueRightOpenMouth.DamageAreaCollider.enabled) ? TongueRightIdleMouth : TongueRightOpenMouth);
		return enemyDamageArea.DamageAreaCollider.bounds.center;
	}

	private void SetGuardEffectOffset(Hit hit)
	{
		GameObject gameObject = ((!HeadLeftSprite.IsVisibleFrom(UnityEngine.Camera.main)) ? HeadRight : HeadLeft);
		Vector3 vector = Vector3.Lerp(hit.AttackingEntity.transform.position, gameObject.transform.position, 0.3f);
		GuardEffectOffset = (Vector3)baseGuardEffectOffset + (vector - base.transform.position);
		GuardEffectOffset.x *= -1f;
	}

	private void DamageFlash()
	{
		TongueLeftDamageEffect.Blink(0f, 0.07f);
		TongueRightDamageEffect.Blink(0f, 0.07f);
	}

	private void TakeDamageByVisibleHead(Hit hit)
	{
		if (TongueLeftOpenMouthRenderer.IsVisibleFrom(UnityEngine.Camera.main))
		{
			TongueLeftOpenMouth.TakeDamage(hit);
		}
		else if (TongueRightOpenMouthRenderer.IsVisibleFrom(UnityEngine.Camera.main))
		{
			TongueRightOpenMouth.TakeDamage(hit);
		}
		else if (TongueLeftIdleMouthRenderer.IsVisibleFrom(UnityEngine.Camera.main))
		{
			TongueLeftIdleMouth.TakeDamage(hit);
		}
		else if (TongueRightIdleMouthRenderer.IsVisibleFrom(UnityEngine.Camera.main))
		{
			TongueRightIdleMouth.TakeDamage(hit);
		}
	}

	public Vector3 GetPosition()
	{
		throw new NotImplementedException();
	}

	public override EnemyAttack EnemyAttack()
	{
		throw new NotImplementedException();
	}

	public override EnemyBumper EnemyBumper()
	{
		throw new NotImplementedException();
	}

	public override EnemyFloorChecker EnemyFloorChecker()
	{
		throw new NotImplementedException();
	}

	protected override void EnablePhysics(bool enable = true)
	{
		throw new NotImplementedException();
	}
}
