using Framework.Managers;
using Gameplay.GameControllers.Enemies.BellGhost;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Projectiles;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.CommonAttacks;

public class BossStraightProjectileAttack : EnemyAttack, IProjectileAttack
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

	public StraightProjectile Shoot(Vector2 dir, Vector2 position, Vector2 offset, Vector3 rotation, float hitStrength = 1f)
	{
		StraightProjectile straightProjectile = Shoot(dir);
		straightProjectile.transform.position = position;
		straightProjectile.transform.position += (Vector3)offset;
		Quaternion rotation2 = default(Quaternion);
		rotation2.eulerAngles = rotation;
		straightProjectile.transform.rotation = rotation2;
		straightProjectile.GetComponent<ProjectileWeapon>().SetDamageStrength(hitStrength);
		return straightProjectile;
	}

	public StraightProjectile Shoot(Vector2 dir, Vector2 position, Vector2 offset, float hitStrength = 1f)
	{
		StraightProjectile straightProjectile = Shoot(dir);
		straightProjectile.transform.position = position;
		straightProjectile.transform.position += (Vector3)offset;
		straightProjectile.GetComponent<ProjectileWeapon>().SetDamageStrength(hitStrength);
		return straightProjectile;
	}

	public StraightProjectile Shoot(Vector2 dir, Vector2 offset, Vector3 rotation, float hitStrength = 1f)
	{
		StraightProjectile straightProjectile = Shoot(dir);
		straightProjectile.transform.position += (Vector3)offset;
		Quaternion rotation2 = default(Quaternion);
		rotation2.eulerAngles = rotation;
		straightProjectile.transform.rotation = rotation2;
		straightProjectile.GetComponent<ProjectileWeapon>().SetDamageStrength(hitStrength);
		return straightProjectile;
	}

	public StraightProjectile Shoot(Vector2 dir, Vector2 offset, float hitStrength = 1f)
	{
		StraightProjectile straightProjectile = Shoot(dir);
		straightProjectile.transform.position += (Vector3)offset;
		straightProjectile.GetComponent<ProjectileWeapon>().SetDamageStrength(hitStrength);
		return straightProjectile;
	}

	public StraightProjectile Shoot(Vector2 dir)
	{
		base.CurrentWeaponAttack();
		GameObject gameObject = PoolManager.Instance.ReuseObject(projectilePrefab, projectileSource.position, Quaternion.identity).GameObject;
		StraightProjectile straightProjectile = null;
		if ((bool)gameObject)
		{
			straightProjectile = gameObject.GetComponent<StraightProjectile>();
			if (!straightProjectile)
			{
				return straightProjectile;
			}
			straightProjectile.OriginalDamage = ProjectileDamageAmount;
			SetProjectileWeaponDamage(straightProjectile, ProjectileDamageAmount);
			straightProjectile.Init(straightProjectile.transform.position, straightProjectile.transform.position + (Vector3)dir, projectileSpeed);
			if (!muzzleFlashPrefab)
			{
				return straightProjectile;
			}
			float z = Mathf.Atan2(dir.y, dir.x) * 57.29578f;
			PoolManager.Instance.ReuseObject(muzzleFlashPrefab, straightProjectile.transform.position, Quaternion.Euler(0f, 0f, z));
		}
		return straightProjectile;
	}

	public void SetProjectileWeaponDamage(int damage)
	{
		ProjectileDamageAmount = damage;
	}

	public void SetProjectileWeaponDamage(Projectile projectile, int damage)
	{
		SetProjectileWeaponDamage(damage);
		if ((bool)projectile)
		{
			ProjectileWeapon componentInChildren = projectile.GetComponentInChildren<ProjectileWeapon>();
			if ((bool)componentInChildren)
			{
				componentInChildren.SetDamage(damage);
			}
		}
	}
}
