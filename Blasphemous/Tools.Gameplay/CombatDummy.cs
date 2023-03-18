using System;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Tools.Gameplay;

public class CombatDummy : Enemy, IDamageable
{
	private EnemyDamageArea dmgArea;

	private void Start()
	{
		dmgArea = GetComponentInChildren<EnemyDamageArea>();
	}

	public void Damage(Hit hit)
	{
		dmgArea.TakeDamage(hit);
		switch (hit.DamageType)
		{
		case DamageArea.DamageType.Normal:
			Core.Audio.PlaySfxOnCatalog("PenitentSimpleEnemyHit");
			break;
		case DamageArea.DamageType.Heavy:
			Core.Audio.PlaySfxOnCatalog("PenitentHeavyEnemyHit");
			break;
		case DamageArea.DamageType.Critical:
			Core.Audio.PlaySfxOnCatalog("PenitentCriticalEnemyHit");
			break;
		}
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	public override EnemyFloorChecker EnemyFloorChecker()
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

	protected override void EnablePhysics(bool enable)
	{
		throw new NotImplementedException();
	}
}
