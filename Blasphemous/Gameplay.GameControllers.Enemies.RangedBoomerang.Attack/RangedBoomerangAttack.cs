using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Projectiles;
using Gameplay.GameControllers.Enemies.RangedBoomerang.IA;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.RangedBoomerang.Attack;

public class RangedBoomerangAttack : EnemyAttack
{
	private Hit _weaponHit;

	public ProjectilePool pool;

	public Transform target;

	public GameObject projectilePrefab;

	public RangedBoomerangBehaviour ownerBehaviour;

	protected override void OnStart()
	{
		base.OnStart();
		PoolManager.Instance.CreatePool(projectilePrefab, 1);
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
		BoomerangProjectile component = PoolManager.Instance.ReuseObject(projectilePrefab, pool.transform.position, Quaternion.identity).GameObject.GetComponent<BoomerangProjectile>();
		component.Init(component.transform.position, target.position + Vector3.up * 1.25f, 12f);
		_weaponHit.AttackingEntity = component.gameObject;
		component.GetComponent<BoomerangBlade>().SetHit(_weaponHit);
		component.OnBackToOrigin += OnBoomerangBack;
	}

	private void OnBoomerangBack(BoomerangProjectile b)
	{
		b.GetComponent<BoomerangBlade>().Recycle();
		ownerBehaviour.OnBoomerangRecovered();
		b.OnBackToOrigin -= OnBoomerangBack;
	}
}
