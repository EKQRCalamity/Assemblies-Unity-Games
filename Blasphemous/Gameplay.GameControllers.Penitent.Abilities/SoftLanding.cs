using System.Collections.Generic;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Abilities;

public class SoftLanding : Ability
{
	private bool _attack;

	public bool Active;

	public AttackArea HardLandingAttackArea;

	private List<IDamageable> _damageableEntities;

	protected override void OnStart()
	{
		base.OnStart();
		_damageableEntities = new List<IDamageable>();
		EnableVerticalAttackCollider(enable: false);
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (base.Animator.GetCurrentAnimatorStateInfo(0).IsName("HardLanding") && !base.Casting)
		{
			Cast();
		}
		if (!base.Animator.GetCurrentAnimatorStateInfo(0).IsName("HardLanding") && base.Casting)
		{
			StopCast();
		}
	}

	protected override void OnCastStart()
	{
		base.OnCastStart();
		EnableVerticalAttackCollider();
		Hit hardLandingHit = GetHardLandingHit();
		AttackDamageableEntities(hardLandingHit);
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: true);
	}

	protected override void OnCastEnd(float castingTime)
	{
		base.OnCastEnd(castingTime);
		if (_attack)
		{
			_attack = !_attack;
		}
		EnableVerticalAttackCollider(enable: false);
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: false);
	}

	private List<IDamageable> GetDamageableEntities()
	{
		GameObject[] array = HardLandingAttackArea.OverlappedEntities();
		int num = array.Length;
		for (byte b = 0; b < num; b = (byte)(b + 1))
		{
			IDamageable componentInParent = array[b].GetComponentInParent<IDamageable>();
			_damageableEntities.Add(componentInParent);
		}
		return _damageableEntities;
	}

	private void AttackDamageableEntities(Hit hardLandingHit)
	{
		List<IDamageable> damageableEntities = GetDamageableEntities();
		int count = damageableEntities.Count;
		if (count > 0)
		{
			for (byte b = 0; b < count; b = (byte)(b + 1))
			{
				_damageableEntities[b].Damage(hardLandingHit);
			}
			_damageableEntities.Clear();
		}
	}

	private Hit GetHardLandingHit()
	{
		Hit result = default(Hit);
		result.AttackingEntity = base.EntityOwner.gameObject;
		result.DamageType = DamageArea.DamageType.Normal;
		result.DamageAmount = base.EntityOwner.Stats.Strength.Final;
		return result;
	}

	private void EnableVerticalAttackCollider(bool enable = true)
	{
		if (enable)
		{
			if (!HardLandingAttackArea.WeaponCollider.enabled)
			{
				HardLandingAttackArea.WeaponCollider.enabled = true;
			}
			if (!HardLandingAttackArea.enabled)
			{
				HardLandingAttackArea.enabled = true;
			}
		}
		else
		{
			if (HardLandingAttackArea.WeaponCollider.enabled)
			{
				HardLandingAttackArea.WeaponCollider.enabled = false;
			}
			if (HardLandingAttackArea.enabled)
			{
				HardLandingAttackArea.enabled = false;
			}
		}
	}
}
