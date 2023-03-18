using System;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.PontiffHusk.Attack;

public class PontiffHuskRangedAttack : EnemyAttack
{
	private AttackArea _attackArea;

	private PontiffHuskRanged _PontiffHuskRanged;

	public bool EntityAttacked { get; set; }

	public bool EnableWeaponAreaCollider
	{
		get
		{
			return _attackArea.WeaponCollider.enabled;
		}
		set
		{
			Collider2D weaponCollider = _attackArea.WeaponCollider;
			weaponCollider.enabled = value;
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		_PontiffHuskRanged = (PontiffHuskRanged)base.EntityOwner;
		base.CurrentEnemyWeapon = base.EntityOwner.GetComponentInChildren<Weapon>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		_attackArea = base.EntityOwner.GetComponentInChildren<AttackArea>();
		if ((bool)_PontiffHuskRanged)
		{
			MotionLerper obj = _PontiffHuskRanged.MotionLerper;
			obj.OnLerpStop = (Core.SimpleEvent)Delegate.Combine(obj.OnLerpStop, new Core.SimpleEvent(OnLerpStop));
			MotionLerper obj2 = _PontiffHuskRanged.MotionLerper;
			obj2.OnLerpStart = (Core.SimpleEvent)Delegate.Combine(obj2.OnLerpStart, new Core.SimpleEvent(OnLerpStart));
		}
		if ((bool)_attackArea)
		{
			_attackArea.OnStay += AttackAreaOnStay;
			_attackArea.OnExit += AttackAreaOnExit;
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		AttackAreaFloatingReposition();
	}

	private void AttackAreaOnExit(object sender, Collider2DParam e)
	{
		if (EntityAttacked)
		{
			EntityAttacked = !EntityAttacked;
		}
	}

	private void AttackAreaOnStay(object sender, Collider2DParam e)
	{
		if (!base.EntityOwner.Status.Dead && !EntityAttacked)
		{
			EntityAttacked = true;
			CurrentWeaponAttack(DamageArea.DamageType.Normal);
			if (_PontiffHuskRanged.MotionLerper.IsLerping && _attackArea != null)
			{
				EnableWeaponAreaCollider = false;
			}
		}
	}

	private void AttackAreaFloatingReposition()
	{
		if (!(_PontiffHuskRanged.AttackArea == null))
		{
			Vector2 vector = _PontiffHuskRanged.FloatingMotion.transform.localPosition;
			_PontiffHuskRanged.AttackArea.transform.localPosition = vector;
		}
	}

	public override void CurrentWeaponAttack(DamageArea.DamageType damageType)
	{
		base.CurrentWeaponAttack(damageType);
		if ((bool)base.CurrentEnemyWeapon)
		{
			Hit hit = default(Hit);
			hit.AttackingEntity = _PontiffHuskRanged.gameObject;
			hit.DamageType = damageType;
			hit.DamageAmount = _PontiffHuskRanged.Stats.Strength.Final;
			hit.HitSoundId = HitSound;
			Hit weapondHit = hit;
			base.CurrentEnemyWeapon.Attack(weapondHit);
		}
	}

	private void OnLerpStart()
	{
		SetContactDamage(_PontiffHuskRanged.Stats.Strength.Final);
		if (EntityAttacked)
		{
			EntityAttacked = !EntityAttacked;
		}
	}

	private void OnLerpStop()
	{
		SetContactDamage(ContactDamageAmount);
		if ((bool)_attackArea && !EnableWeaponAreaCollider)
		{
			EnableWeaponAreaCollider = true;
		}
	}

	private void OnDestroy()
	{
		if ((bool)_PontiffHuskRanged)
		{
			MotionLerper obj = _PontiffHuskRanged.MotionLerper;
			obj.OnLerpStop = (Core.SimpleEvent)Delegate.Remove(obj.OnLerpStop, new Core.SimpleEvent(OnLerpStop));
			MotionLerper obj2 = _PontiffHuskRanged.MotionLerper;
			obj2.OnLerpStart = (Core.SimpleEvent)Delegate.Remove(obj2.OnLerpStart, new Core.SimpleEvent(OnLerpStart));
		}
		if ((bool)_attackArea)
		{
			_attackArea.OnStay -= AttackAreaOnStay;
			_attackArea.OnExit -= AttackAreaOnExit;
		}
	}
}
