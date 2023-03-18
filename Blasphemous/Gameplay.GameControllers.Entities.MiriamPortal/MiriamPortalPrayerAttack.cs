using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using UnityEngine;

namespace Gameplay.GameControllers.Entities.MiriamPortal;

public class MiriamPortalPrayerAttack : Attack, IDirectAttack, IPaintAttackCollider
{
	[EventRef]
	public string HitSound;

	private Hit miriamPortalHit;

	private MiriamPortalPrayerWeapon miriamPortalWeapon;

	private bool dealsDamage;

	[HideInInspector]
	public bool DealsDamage
	{
		get
		{
			return dealsDamage;
		}
		set
		{
			dealsDamage = value;
			if (dealsDamage)
			{
				miriamPortalWeapon.StartAttacking(miriamPortalHit);
			}
			else
			{
				miriamPortalWeapon.StopAttacking();
			}
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		miriamPortalWeapon = GetComponentInChildren<MiriamPortalPrayerWeapon>();
		AttachShowScriptIfNeeded();
	}

	protected override void OnStart()
	{
		base.OnStart();
		CreateHit();
	}

	public void CreateHit()
	{
		miriamPortalHit = new Hit
		{
			DamageAmount = base.EntityOwner.Stats.Strength.Final * Core.Logic.Penitent.Stats.DamageMultiplier.Final * 0.5f,
			AttackingEntity = base.EntityOwner.gameObject,
			DamageElement = DamageArea.DamageElement.Normal,
			DamageType = DamageArea.DamageType.OptionalStunt,
			DestroysProjectiles = true,
			HitSoundId = HitSound
		};
	}

	public void SetDamage(int damage)
	{
		if (damage >= 0)
		{
			miriamPortalHit.DamageAmount = damage;
		}
	}

	public bool IsCurrentlyDealingDamage()
	{
		return DealsDamage;
	}

	public void AttachShowScriptIfNeeded()
	{
	}
}
