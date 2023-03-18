using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent.Gizmos;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.WallEnemy.Attack;

public class WallEnemyRangedAttack : EnemyAttack
{
	public GameObject Projectile;

	private Hit _wallEnemyHit;

	public WallEnemyWeapon Weapon { get; private set; }

	public RootMotionDriver FiringPosition { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		base.CurrentEnemyWeapon = GetComponentInChildren<WallEnemyWeapon>();
		FiringPosition = base.EntityOwner.GetComponentInChildren<RootMotionDriver>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		Weapon = (WallEnemyWeapon)base.CurrentEnemyWeapon;
		_wallEnemyHit = new Hit
		{
			AttackingEntity = base.EntityOwner.gameObject,
			DamageAmount = base.EntityOwner.Stats.Strength.Final,
			DamageType = DamageArea.DamageType.Normal,
			Force = Force,
			HitSoundId = HitSound,
			Unnavoidable = false
		};
		if ((bool)Projectile)
		{
			PoolManager.Instance.CreatePool(Projectile, 2);
		}
	}

	public override void CurrentWeaponAttack()
	{
		base.CurrentWeaponAttack();
		base.CurrentEnemyWeapon.Attack(_wallEnemyHit);
		FireProjectile();
	}

	private void FireProjectile()
	{
		if ((bool)Projectile)
		{
			Vector2 fireProjectilePosition = GetFireProjectilePosition();
			PoolManager.ObjectInstance objectInstance = PoolManager.Instance.ReuseObject(Projectile, fireProjectilePosition, Quaternion.identity);
			WallEnemyProjectile componentInChildren = objectInstance.GameObject.GetComponentInChildren<WallEnemyProjectile>();
			if ((bool)componentInChildren)
			{
				componentInChildren.SetOwner(base.EntityOwner);
			}
		}
	}

	private Vector2 GetFireProjectilePosition()
	{
		return FiringPosition.transform.position;
	}
}
