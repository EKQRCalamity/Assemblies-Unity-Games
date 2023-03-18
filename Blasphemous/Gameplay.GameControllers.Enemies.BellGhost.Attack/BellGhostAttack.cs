using System;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.BellGhost.Attack;

public class BellGhostAttack : EnemyAttack
{
	private AttackArea _attackArea;

	private BellGhost _bellGhost;

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
		_bellGhost = (BellGhost)base.EntityOwner;
		base.CurrentEnemyWeapon = base.EntityOwner.GetComponentInChildren<Weapon>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		_attackArea = base.EntityOwner.GetComponentInChildren<AttackArea>();
		if ((bool)_bellGhost)
		{
			MotionLerper obj = _bellGhost.MotionLerper;
			obj.OnLerpStop = (Core.SimpleEvent)Delegate.Combine(obj.OnLerpStop, new Core.SimpleEvent(OnLerpStop));
			MotionLerper obj2 = _bellGhost.MotionLerper;
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
			if (_bellGhost.MotionLerper.IsLerping && _attackArea != null)
			{
				EnableWeaponAreaCollider = false;
			}
		}
	}

	private void AttackAreaFloatingReposition()
	{
		if (!(_bellGhost.AttackArea == null))
		{
			Vector2 vector = _bellGhost.FloatingMotion.transform.localPosition;
			_bellGhost.AttackArea.transform.localPosition = vector;
		}
	}

	public override void CurrentWeaponAttack(DamageArea.DamageType damageType)
	{
		base.CurrentWeaponAttack(damageType);
		if ((bool)base.CurrentEnemyWeapon)
		{
			Hit hit = default(Hit);
			hit.AttackingEntity = _bellGhost.gameObject;
			hit.DamageType = damageType;
			hit.DamageAmount = _bellGhost.Stats.Strength.Final;
			hit.HitSoundId = HitSound;
			Hit weapondHit = hit;
			base.CurrentEnemyWeapon.Attack(weapondHit);
		}
	}

	private void OnLerpStart()
	{
		SetContactDamage(_bellGhost.Stats.Strength.Final);
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
		if ((bool)_bellGhost)
		{
			MotionLerper obj = _bellGhost.MotionLerper;
			obj.OnLerpStop = (Core.SimpleEvent)Delegate.Remove(obj.OnLerpStop, new Core.SimpleEvent(OnLerpStop));
			MotionLerper obj2 = _bellGhost.MotionLerper;
			obj2.OnLerpStart = (Core.SimpleEvent)Delegate.Remove(obj2.OnLerpStart, new Core.SimpleEvent(OnLerpStart));
		}
		if ((bool)_attackArea)
		{
			_attackArea.OnStay -= AttackAreaOnStay;
			_attackArea.OnExit -= AttackAreaOnExit;
		}
	}
}
