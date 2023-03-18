using System.Collections.Generic;
using System.Linq;
using Framework.Managers;
using Gameplay.GameControllers.Entities.Weapon;
using UnityEngine;

namespace Gameplay.GameControllers.Entities.Guardian;

public class GuardianPrayerWeapon : Gameplay.GameControllers.Entities.Weapon.Weapon
{
	public GameObject HitVfx;

	protected override void OnStart()
	{
		base.OnStart();
		if ((bool)HitVfx)
		{
			PoolManager.Instance.CreatePool(HitVfx, 5);
		}
	}

	public override void Attack(Hit weapondHit)
	{
		List<IDamageable> damageableEntities = GetDamageableEntities();
		SpawnVfx(damageableEntities);
		AttackDamageableEntities(weapondHit);
		if (damageableEntities.Count > 0)
		{
			OnHit(weapondHit);
		}
	}

	public override void OnHit(Hit weaponHit)
	{
		Core.Logic.CameraManager.ProCamera2DShake.ShakeUsingPreset("HardFall");
		WeaponOwner.SleepTimeByHit(weaponHit);
	}

	private void SpawnVfx(List<IDamageable> damageable)
	{
		foreach (Enemy item in damageable.Select((IDamageable target) => target as Enemy))
		{
			if (item == null)
			{
				break;
			}
			float y = item.EntityDamageArea.DamageAreaCollider.bounds.max.y - 0.25f;
			Vector2 vector = new Vector2(item.transform.position.x, y);
			bool flipX = vector.x - base.transform.position.x < 0f;
			if ((bool)HitVfx)
			{
				GameObject gameObject = PoolManager.Instance.ReuseObject(HitVfx, vector, Quaternion.identity).GameObject;
				SpriteRenderer component = gameObject.GetComponent<SpriteRenderer>();
				if (component != null)
				{
					component.flipX = flipX;
				}
			}
		}
	}
}
