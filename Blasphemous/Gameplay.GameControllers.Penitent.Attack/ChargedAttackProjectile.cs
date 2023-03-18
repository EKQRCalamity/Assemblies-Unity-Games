using System;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Attack;

public class ChargedAttackProjectile : Weapon
{
	[FoldoutGroup("Motion Settings", true, 0)]
	public float Speed;

	[FoldoutGroup("Motion Settings", true, 0)]
	public float MaxSpeed;

	[FoldoutGroup("Motion Settings", true, 0)]
	public float Acceleration;

	[FoldoutGroup("Motion Settings", true, 0)]
	public float Range;

	[FoldoutGroup("Motion Settings", true, 0)]
	public float Lifetime;

	private float _currentLifeTime;

	[FoldoutGroup("Hit Settings", true, 0)]
	public LayerMask EnemyLayer;

	[FoldoutGroup("Hit Settings", true, 0)]
	public LayerMask BlockLayer;

	[SerializeField]
	[FoldoutGroup("Audio", 0)]
	[EventRef]
	protected string HitSound;

	[SerializeField]
	[FoldoutGroup("Audio", 0)]
	[EventRef]
	protected string VanishSound;

	[SerializeField]
	[FoldoutGroup("Audio", 0)]
	[EventRef]
	protected string FlightSound;

	private SpriteRenderer _spriteRenderer;

	private UnityEngine.Animator _animator;

	private Vector2 _dir;

	private bool _vanishing;

	private Hit _chargedAttackHit;

	private bool _hasImpact;

	public MotionLerper MotionLerper { get; private set; }

	public Entity Owner
	{
		get
		{
			return WeaponOwner;
		}
		set
		{
			WeaponOwner = value;
		}
	}

	public AttackArea AttackArea { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		_spriteRenderer = GetComponent<SpriteRenderer>();
		_animator = GetComponent<UnityEngine.Animator>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		_chargedAttackHit = GetHit();
		_dir = ((WeaponOwner.Status.Orientation != 0) ? (-Vector2.right) : Vector2.right);
		MotionLerper = GetComponentInChildren<MotionLerper>();
		AttackArea = GetComponentInChildren<AttackArea>();
		AttackArea.OnEnter += AttackAreaOnEnter;
		AttackArea.Entity = WeaponOwner;
		MotionLerper motionLerper = MotionLerper;
		motionLerper.OnLerpStop = (Core.SimpleEvent)Delegate.Combine(motionLerper.OnLerpStop, new Core.SimpleEvent(OnLerpStop));
		SetSpriteOrientation();
		LinearMotion();
	}

	private void AttackAreaOnEnter(object sender, Collider2DParam e)
	{
		if ((EnemyLayer.value & (1 << e.Collider2DArg.gameObject.layer)) > 0)
		{
			if (!MotionLerper.IsLerping)
			{
				return;
			}
			Impact();
			MotionLerper.StopLerping();
			Attack(_chargedAttackHit);
		}
		if ((BlockLayer.value & (1 << e.Collider2DArg.gameObject.layer)) > 0 && MotionLerper.IsLerping)
		{
			Impact();
			MotionLerper.StopLerping();
		}
	}

	private void SetSpriteOrientation()
	{
		if (_spriteRenderer != null)
		{
			_spriteRenderer.flipX = WeaponOwner.Status.Orientation == EntityOrientation.Left;
		}
	}

	public void LinearMotion()
	{
		if (!(MotionLerper == null))
		{
			MotionLerper.StartLerping(_dir);
		}
	}

	private void OnLerpStop()
	{
		if (!_hasImpact)
		{
			Vanish();
		}
	}

	public override void Attack(Hit weapondHit)
	{
		GetDamageableEntities();
		AttackDamageableEntities(weapondHit);
	}

	public override void OnHit(Hit weaponHit)
	{
	}

	public void Vanish()
	{
		_animator.SetTrigger("VANISH");
	}

	public void Impact()
	{
		_hasImpact = true;
		_animator.SetTrigger("IMPACT");
	}

	public void Dispose()
	{
		Destroy();
	}

	public Hit GetHit()
	{
		float damageAmount = Core.Logic.Penitent.Stats.Strength.Final * Core.Logic.Penitent.PenitentAttack.HeavyAttackMultiplier;
		Hit result = default(Hit);
		result.AttackingEntity = Owner.gameObject;
		result.DamageType = DamageArea.DamageType.Heavy;
		result.DamageAmount = damageAmount;
		result.Force = 0f;
		result.HitSoundId = HitSound;
		return result;
	}

	public override void OnObjectReuse()
	{
		base.OnObjectReuse();
		_hasImpact = false;
		if (WeaponOwner == null)
		{
			WeaponOwner = Core.Logic.Penitent;
		}
		_dir = ((WeaponOwner.Status.Orientation != 0) ? (-Vector2.right) : Vector2.right);
		SetSpriteOrientation();
		LinearMotion();
	}
}
