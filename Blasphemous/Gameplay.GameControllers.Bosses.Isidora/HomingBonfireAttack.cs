using System.Collections.Generic;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.BellGhost;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Projectiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Isidora;

public class HomingBonfireAttack : EnemyAttack
{
	[FoldoutGroup("Projectile Settings", 0)]
	public ProjectileWeapon ProjectileWeapon;

	[FoldoutGroup("Projectile Settings", 0)]
	public int NumProjectiles = 1;

	[FoldoutGroup("Projectile Settings", 0)]
	public Vector2 OffsetPosition;

	[FoldoutGroup("Projectile Settings", 0)]
	public bool UseCastingPosition;

	[FoldoutGroup("Projectile Settings", 0)]
	public Vector2 CastingPosition;

	[FoldoutGroup("Projectile Settings", 0)]
	public float HorizontalSpacingFactor = 1f;

	[FoldoutGroup("Projectile Settings", 0)]
	public float VerticalSpacingFactor = 1f;

	[FoldoutGroup("Other", 0)]
	public int PoolSize = 20;

	[FoldoutGroup("Other", 0)]
	public bool ChargesIsidora;

	[FoldoutGroup("Other", 0)]
	public IsidoraBehaviour isidoraBehaviour;

	private bool prevOffestWasPositive = true;

	private List<HomingProjectile> homingProjectiles = new List<HomingProjectile>();

	protected override void OnStart()
	{
		base.OnStart();
		InitPool();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		foreach (HomingProjectile homingProjectile in homingProjectiles)
		{
			if (ChargesIsidora)
			{
				if (!isidoraBehaviour)
				{
					break;
				}
				if (homingProjectile.ChangeTargetToAlternative(isidoraBehaviour.transform, 2f, 1f, 3f))
				{
					homingProjectile.OnLifeEndedEvent -= HomingProjectile_OnLifeEndedEvent;
					homingProjectile.OnLifeEndedEvent += HomingProjectile_OnLifeEndedEvent;
				}
			}
			else
			{
				homingProjectile.ChangeTargetToPenitent(changeTargetOnlyIfInactive: true);
			}
		}
	}

	public bool IsAnyProjectileActive()
	{
		foreach (HomingProjectile homingProjectile in homingProjectiles)
		{
			if (homingProjectile.gameObject.activeInHierarchy)
			{
				return true;
			}
		}
		return false;
	}

	public void ClearAll()
	{
		foreach (HomingProjectile homingProjectile in homingProjectiles)
		{
			homingProjectile.SetTTL(0f);
		}
	}

	private void InitPool()
	{
		if ((bool)ProjectileWeapon)
		{
			PoolManager.Instance.CreatePool(ProjectileWeapon.gameObject, PoolSize);
		}
	}

	private void SetupProjectileWeapon(ProjectileWeapon projectileWeapon)
	{
		if ((bool)projectileWeapon)
		{
			projectileWeapon.WeaponOwner = base.EntityOwner;
			projectileWeapon.SetOwner(base.EntityOwner.gameObject);
			projectileWeapon.SetDamage((int)base.EntityOwner.Stats.Strength.Final);
		}
	}

	private void SetupHomingProjectile(HomingProjectile homingProjectile, Vector2 currentDirection, float targetOffsetFactor, Vector2 targetOffset)
	{
		homingProjectile.currentDirection = currentDirection;
		homingProjectile.TargetOffsetFactor = targetOffsetFactor;
		homingProjectile.TargetOffset = targetOffset;
	}

	public void FireProjectile()
	{
		if (!ProjectileWeapon)
		{
			return;
		}
		for (int i = 1; i <= NumProjectiles; i++)
		{
			Vector2 vector = ((!UseCastingPosition) ? ((Vector2)base.EntityOwner.transform.position) : CastingPosition);
			vector += OffsetPosition;
			int num = 0;
			int num2 = NumProjectiles;
			if (NumProjectiles % 2 == 0 || i < NumProjectiles)
			{
				num = ((i % 2 != 0) ? (-i) : (i - 1));
				num2 = ((i % 2 != 0) ? (NumProjectiles - i) : (NumProjectiles - i + 1));
			}
			vector.x += (float)num * HorizontalSpacingFactor;
			vector.y += (float)num2 * VerticalSpacingFactor;
			PoolManager.ObjectInstance objectInstance = PoolManager.Instance.ReuseObject(ProjectileWeapon.gameObject, vector, Quaternion.identity);
			HomingProjectile component = objectInstance.GameObject.GetComponent<HomingProjectile>();
			float num3 = 5f;
			Vector2 targetOffset = component.TargetOffset;
			if (ChargesIsidora && prevOffestWasPositive)
			{
				num3 *= -1f;
				num2 *= -1;
			}
			Vector2 normalized = new Vector2(num, num2).normalized;
			SetupHomingProjectile(component, normalized, num3, targetOffset);
			if (!homingProjectiles.Contains(component))
			{
				homingProjectiles.Add(component);
			}
			if (ChargesIsidora)
			{
				component.OnLifeEndedEvent += HomingProjectile_OnLifeEndedEvent;
			}
			ProjectileWeapon component2 = objectInstance.GameObject.GetComponent<ProjectileWeapon>();
			SetupProjectileWeapon(component2);
		}
		prevOffestWasPositive = !prevOffestWasPositive;
	}

	private void HomingProjectile_OnLifeEndedEvent(Projectile obj)
	{
		HomingProjectile homingProjectile = obj as HomingProjectile;
		homingProjectile.OnLifeEndedEvent -= HomingProjectile_OnLifeEndedEvent;
		Vector3 position = homingProjectile.gameObject.transform.position;
		Vector2 b = homingProjectile.CalculateTargetPosition();
		if (Vector2.Distance(position, b) < 1.3f)
		{
			isidoraBehaviour.ProjectileAbsortion(homingProjectile.transform.position, homingProjectile.currentDirection);
		}
	}
}
