using System;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Projectiles;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.CommonAttacks;

public class BossBoomerangProjectileAttack : EnemyAttack, IProjectileAttack
{
	[FoldoutGroup("Boomerang Attack settings", 0)]
	public float damage;

	[FoldoutGroup("Boomerang Attack settings", 0)]
	public bool unavoidable;

	private Hit _weaponHit;

	public GameObject projectilePrefab;

	public Transform projectileSource;

	public int poolSize = 3;

	public event Action OnBoomerangReturnEvent;

	protected override void OnStart()
	{
		base.OnStart();
		PoolManager.Instance.CreatePool(projectilePrefab, poolSize);
	}

	public void CreateHit()
	{
		_weaponHit = new Hit
		{
			AttackingEntity = base.EntityOwner.gameObject,
			DamageAmount = damage,
			DamageType = DamageType,
			DamageElement = DamageElement,
			Force = Force,
			HitSoundId = HitSound,
			Unnavoidable = unavoidable
		};
	}

	public void SetDamage(float damage)
	{
		this.damage = damage;
		CreateHit();
	}

	public void Shoot(Transform target)
	{
		base.CurrentWeaponAttack();
		Shoot(target.position);
	}

	public void Shoot(Vector2 target)
	{
		base.CurrentWeaponAttack();
		BoomerangProjectile component = PoolManager.Instance.ReuseObject(projectilePrefab, projectileSource.position, Quaternion.identity).GameObject.GetComponent<BoomerangProjectile>();
		component.Init(component.transform.position, (Vector3)target + Vector3.up * 1.25f, 12f);
		_weaponHit.AttackingEntity = component.gameObject;
		SetProjectileWeaponDamage(component, (int)damage);
		component.OnBackToOrigin += OnBoomerangBack;
	}

	private void OnBoomerangBack(BoomerangProjectile b)
	{
		if (this.OnBoomerangReturnEvent != null)
		{
			this.OnBoomerangReturnEvent();
		}
		b.OnBackToOrigin -= OnBoomerangBack;
	}

	public void SetProjectileWeaponDamage(int damage)
	{
		SetDamage(damage);
	}

	public void SetProjectileWeaponDamage(Projectile projectile, int damage)
	{
		SetDamage(damage);
		BoomerangBlade component = projectile.GetComponent<BoomerangBlade>();
		if (component != null)
		{
			component.SetHit(_weaponHit);
		}
	}
}
