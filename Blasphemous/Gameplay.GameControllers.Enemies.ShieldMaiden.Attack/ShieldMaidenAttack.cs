using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.ShieldMaiden.IA;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.ShieldMaiden.Attack;

public class ShieldMaidenAttack : EnemyAttack
{
	private Hit _weaponHit;

	public Transform target;

	public ShieldMaidenBehaviour ownerBehaviour;

	protected override void OnAwake()
	{
		base.OnAwake();
		base.CurrentEnemyWeapon = GetComponentInChildren<ShieldMaidenWeapon>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		_weaponHit = new Hit
		{
			AttackingEntity = base.EntityOwner.gameObject,
			DamageAmount = base.EntityOwner.Stats.Strength.Final,
			DamageType = DamageType,
			Force = Force,
			HitSoundId = HitSound,
			Unnavoidable = false
		};
	}

	public override void CurrentWeaponAttack()
	{
		base.CurrentWeaponAttack();
		Debug.Log("ShieldMaiden ATTACK with current weapon");
		base.CurrentEnemyWeapon.Attack(_weaponHit);
	}
}
