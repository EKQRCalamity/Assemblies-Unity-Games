using System;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.PontiffHusk.Attack;

public class PontiffHuskMeleeAttack : EnemyAttack
{
	private AttackArea _attackArea;

	private PontiffHuskMelee _PontiffHuskMelee;

	public bool EntityAttacked { get; set; }

	public bool EnableWeaponAreaCollider
	{
		get
		{
			return _attackArea.WeaponCollider.enabled;
		}
		set
		{
			_attackArea.WeaponCollider.enabled = value;
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		_PontiffHuskMelee = (PontiffHuskMelee)base.EntityOwner;
		base.CurrentEnemyWeapon = base.EntityOwner.GetComponentInChildren<Weapon>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		_attackArea = base.EntityOwner.GetComponentInChildren<AttackArea>();
		if ((bool)_PontiffHuskMelee)
		{
			MotionLerper obj = _PontiffHuskMelee.MotionLerper;
			obj.OnLerpStop = (Core.SimpleEvent)Delegate.Combine(obj.OnLerpStop, new Core.SimpleEvent(OnLerpStop));
			MotionLerper obj2 = _PontiffHuskMelee.MotionLerper;
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
			EntityAttacked = false;
		}
	}

	private void AttackAreaOnStay(object sender, Collider2DParam e)
	{
		if (!base.EntityOwner.Status.Dead && !EntityAttacked)
		{
			EntityAttacked = true;
			CurrentWeaponAttack(DamageType);
			if (_PontiffHuskMelee.MotionLerper.IsLerping && _attackArea != null)
			{
				EnableWeaponAreaCollider = false;
			}
		}
	}

	private void AttackAreaFloatingReposition()
	{
		if (!(_PontiffHuskMelee.AttackArea == null))
		{
			Vector2 vector = _PontiffHuskMelee.FloatingMotion.transform.localPosition;
			_PontiffHuskMelee.AttackArea.transform.localPosition = vector;
		}
	}

	public override void CurrentWeaponAttack(DamageArea.DamageType damageType)
	{
		base.CurrentWeaponAttack(damageType);
		if ((bool)base.CurrentEnemyWeapon)
		{
			Hit hit = default(Hit);
			hit.AttackingEntity = _PontiffHuskMelee.gameObject;
			hit.DamageType = damageType;
			hit.DamageAmount = _PontiffHuskMelee.Stats.Strength.Final;
			hit.Force = Force;
			hit.HitSoundId = HitSound;
			Hit weapondHit = hit;
			base.CurrentEnemyWeapon.Attack(weapondHit);
		}
	}

	private void OnLerpStart()
	{
		SetContactDamage(_PontiffHuskMelee.Stats.Strength.Final);
		if (EntityAttacked)
		{
			EntityAttacked = false;
		}
		if ((bool)_attackArea && !EnableWeaponAreaCollider)
		{
			EnableWeaponAreaCollider = true;
		}
	}

	private void OnLerpStop()
	{
		SetContactDamage(ContactDamageAmount);
		if ((bool)_attackArea && !EnableWeaponAreaCollider)
		{
			EnableWeaponAreaCollider = false;
		}
	}

	private void OnDestroy()
	{
		if ((bool)_PontiffHuskMelee)
		{
			MotionLerper obj = _PontiffHuskMelee.MotionLerper;
			obj.OnLerpStop = (Core.SimpleEvent)Delegate.Remove(obj.OnLerpStop, new Core.SimpleEvent(OnLerpStop));
			MotionLerper obj2 = _PontiffHuskMelee.MotionLerper;
			obj2.OnLerpStart = (Core.SimpleEvent)Delegate.Remove(obj2.OnLerpStart, new Core.SimpleEvent(OnLerpStart));
		}
		if ((bool)_attackArea)
		{
			_attackArea.OnStay -= AttackAreaOnStay;
			_attackArea.OnExit -= AttackAreaOnExit;
		}
	}
}
