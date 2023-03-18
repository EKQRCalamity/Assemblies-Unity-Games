using Framework.Managers;
using Gameplay.GameControllers.Enemies.BellGhost;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Projectiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.HomingTurret.Attack;

public class HomingTurretAttack : EnemyAttack
{
	[FoldoutGroup("Projectile Settings", 0)]
	public ProjectileWeapon ProjectileWeapon;

	[FoldoutGroup("Projectile Settings", 0)]
	public Vector3 OffsetPosition;

	[Tooltip("This vector is added to the direction to TPO in order to calculate the initital direction of the projectile.")]
	[FoldoutGroup("Projectile Settings", 0)]
	public Vector2 InitialDirectionAddendum;

	[FoldoutGroup("Projectile Settings", 0)]
	public bool UseEntityPosition = true;

	[FoldoutGroup("Projectile Settings", 0)]
	public bool UseEntityOrientation = true;

	[HideIf("UseEntityPosition", true)]
	[FoldoutGroup("Projectile Settings", 0)]
	public Transform ShootingPoint;

	protected override void OnStart()
	{
		base.OnStart();
		InitPool();
	}

	private void InitPool()
	{
		if ((bool)ProjectileWeapon)
		{
			PoolManager.Instance.CreatePool(ProjectileWeapon.gameObject, 5);
		}
	}

	private void SetupProjectile(ProjectileWeapon projectileWeapon)
	{
		if ((bool)projectileWeapon)
		{
			projectileWeapon.WeaponOwner = base.EntityOwner;
			projectileWeapon.SetOwner(base.EntityOwner.gameObject);
			projectileWeapon.SetDamage((int)base.EntityOwner.Stats.Strength.Final);
		}
	}

	public HomingProjectile FireProjectileToPenitent()
	{
		return FireProjectileToTarget(Core.Logic.Penitent.GetPosition());
	}

	public HomingProjectile FireProjectileToTarget(Vector3 target)
	{
		if (!ProjectileWeapon)
		{
			return null;
		}
		Transform transform = ((!UseEntityPosition) ? ShootingPoint : base.EntityOwner.transform);
		Vector3 vector = transform.position + OffsetPosition;
		if (UseEntityOrientation)
		{
			vector.x += ((base.EntityOwner.Status.Orientation != 0) ? (-1) : 0);
		}
		PoolManager.ObjectInstance objectInstance = PoolManager.Instance.ReuseObject(ProjectileWeapon.gameObject, vector, Quaternion.identity);
		ProjectileWeapon component = objectInstance.GameObject.GetComponent<ProjectileWeapon>();
		SetupProjectile(component);
		HomingProjectile component2 = objectInstance.GameObject.GetComponent<HomingProjectile>();
		component2.currentDirection = ((Vector2)(target - vector).normalized + InitialDirectionAddendum).normalized;
		return component2;
	}
}
