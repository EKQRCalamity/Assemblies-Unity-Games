using Framework.Managers;
using Gameplay.GameControllers.Enemies.BellGhost;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Projectiles;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.CommonAttacks;

public class BossCurvedProjectileAttack : EnemyAttack, IProjectileAttack
{
	public GameObject muzzleFlashPrefab;

	public GameObject projectilePrefab;

	public Transform projectileSource;

	public int ProjectileDamageAmount;

	public int poolSize = 3;

	public float projectileSpeed;

	public Vector2 targetOffset;

	protected override void OnStart()
	{
		base.OnStart();
		PoolManager.Instance.CreatePool(projectilePrefab, poolSize);
		if (muzzleFlashPrefab != null)
		{
			PoolManager.Instance.CreatePool(muzzleFlashPrefab, poolSize);
		}
	}

	public void Clear()
	{
	}

	public CurvedProjectile Shoot(Vector2 target)
	{
		base.CurrentWeaponAttack();
		GameObject gameObject = PoolManager.Instance.ReuseObject(projectilePrefab, projectileSource.position, Quaternion.identity).GameObject;
		CurvedProjectile curvedProjectile = null;
		if ((bool)gameObject)
		{
			curvedProjectile = gameObject.GetComponent<CurvedProjectile>();
			if (!curvedProjectile)
			{
				return curvedProjectile;
			}
			curvedProjectile.OriginalDamage = ProjectileDamageAmount;
			SetProjectileWeaponDamage(curvedProjectile, ProjectileDamageAmount);
			curvedProjectile.Init(curvedProjectile.transform.position, target, projectileSpeed);
			if (!muzzleFlashPrefab)
			{
				return curvedProjectile;
			}
			Vector2 vector = target - (Vector2)curvedProjectile.transform.position;
			float z = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
			PoolManager.Instance.ReuseObject(muzzleFlashPrefab, curvedProjectile.transform.position, Quaternion.Euler(0f, 0f, z));
		}
		return curvedProjectile;
	}

	public void SetProjectileWeaponDamage(int damage)
	{
		if (damage > 0)
		{
			ProjectileDamageAmount = damage;
		}
	}

	public void SetProjectileWeaponDamage(Projectile projectile, int damage)
	{
		SetProjectileWeaponDamage(damage);
		if (damage > 0 && (bool)projectile)
		{
			ProjectileWeapon componentInChildren = projectile.GetComponentInChildren<ProjectileWeapon>();
			if ((bool)componentInChildren)
			{
				componentInChildren.SetDamage(damage);
			}
		}
	}
}
