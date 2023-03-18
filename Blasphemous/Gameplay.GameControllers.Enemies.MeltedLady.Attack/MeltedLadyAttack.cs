using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.MeltedLady.IA;
using Gameplay.GameControllers.Enemies.Projectiles;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.MeltedLady.Attack;

public class MeltedLadyAttack : EnemyAttack
{
	private Hit _weaponHit;

	public MeltedLadyBehaviour ownerBehaviour;

	public ProjectilePool pool;

	public GameObject projectilePrefab;

	public Transform target;

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
	}
}
