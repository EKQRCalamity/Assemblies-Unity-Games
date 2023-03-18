using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Projectiles;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent.Gizmos;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.BellGhost.Attack;

public class BellGhostVariantAttack : EnemyAttack
{
	public ProjectilePool pPool;

	public SpawnPoint bulletSpawnPoint;

	public Entity owner;

	public Transform target;

	public GameObject projectilePrefab;

	public float projectileSpeed;

	public RootMotionDriver rootMotion;

	private const int MAX_PROJECTILES_POOLED = 2;

	protected override void OnStart()
	{
		base.OnStart();
		PoolManager.Instance.CreatePool(projectilePrefab, 2);
		rootMotion = bulletSpawnPoint.GetComponent<RootMotionDriver>();
	}

	public void ShootProjectileAtPlayer()
	{
		Debug.Log("SHOOTING AT PLAYER");
		Vector3 vector = ((!base.EntityOwner.SpriteRenderer.flipX) ? rootMotion.transform.position : rootMotion.ReversePosition);
		StraightProjectile component = PoolManager.Instance.ReuseObject(projectilePrefab.gameObject, vector, Quaternion.identity).GameObject.GetComponent<StraightProjectile>();
		(component as TargetedProjectile).Init(vector, target.position + Vector3.up * 1.5f, projectileSpeed);
		ProjectileWeapon componentInChildren = component.GetComponentInChildren<ProjectileWeapon>();
		componentInChildren.SetOwner(owner.gameObject);
		componentInChildren.SetDamage((int)owner.Stats.Strength.Final);
	}

	public override void CurrentWeaponAttack()
	{
		base.CurrentWeaponAttack();
		ShootProjectileAtPlayer();
	}
}
