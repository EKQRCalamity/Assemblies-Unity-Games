using Framework.FrameworkCore;
using Framework.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Entities;

public abstract class Attack : Trait
{
	public Core.GenericEvent OnEntityAttack;

	[SerializeField]
	protected Animator animator;

	[SerializeField]
	protected int attack1Damage;

	[SerializeField]
	protected int attack2Damage;

	[SerializeField]
	protected int attack3Damage;

	protected MotionLerper motionLerper;

	public AnimationCurve speedCurve;

	[SerializeField]
	[BoxGroup("Attack settings", true, false, 0)]
	public DamageArea.DamageType DamageType;

	[SerializeField]
	[BoxGroup("Attack settings", true, false, 0)]
	protected DamageArea.DamageElement DamageElement;

	protected bool isAttacking;

	protected bool isRangeAttacking;

	protected bool isWeaponBlowUp;

	protected bool isHeavyAttacking;

	protected bool isEnemyHit;

	public bool IsEnemyHit
	{
		get
		{
			return isEnemyHit;
		}
		set
		{
			isEnemyHit = value;
		}
	}

	public bool IsAttacking
	{
		get
		{
			return isAttacking;
		}
		set
		{
			isAttacking = value;
		}
	}

	public bool IsRangeAttacking
	{
		get
		{
			return isRangeAttacking;
		}
		set
		{
			isRangeAttacking = value;
		}
	}

	public bool IsWeaponBlowingUp
	{
		get
		{
			return isWeaponBlowUp;
		}
		set
		{
			isWeaponBlowUp = value;
		}
	}

	public bool IsHeavyAttacking
	{
		get
		{
			return isHeavyAttacking;
		}
		set
		{
			isHeavyAttacking = value;
		}
	}

	public virtual void ContactAttack(IDamageable damageable)
	{
	}

	public virtual void CurrentWeaponAttack()
	{
	}

	public virtual void CurrentWeaponAttack(DamageArea.DamageType damageType)
	{
	}

	public virtual void CurrentWeaponAttack(DamageArea.DamageType damageType, bool applyDamageTypeMultipliers)
	{
	}

	public virtual void OnAttack(Hit hit)
	{
	}
}
