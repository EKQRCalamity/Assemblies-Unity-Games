using System.Collections.Generic;
using Framework.Managers;
using Gameplay.GameControllers.Entities.Weapon;

namespace Gameplay.GameControllers.Entities.MiriamPortal;

public class MiriamPortalPrayerWeapon : Gameplay.GameControllers.Entities.Weapon.Weapon
{
	private Hit hit;

	private bool isDealingDamage;

	private List<IDamageable> alreadyDamaged = new List<IDamageable>();

	protected override void OnStart()
	{
		base.OnStart();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (isDealingDamage)
		{
			Attack(hit);
		}
	}

	public override void Attack(Hit weaponHit)
	{
		List<IDamageable> damageableEntities = GetDamageableEntities();
		foreach (IDamageable item in alreadyDamaged)
		{
			damageableEntities.Remove(item);
		}
		if (damageableEntities.Count == 0)
		{
			return;
		}
		foreach (IDamageable item2 in damageableEntities)
		{
			alreadyDamaged.Add(item2);
		}
		AttackDamageableEntities(weaponHit);
	}

	public void StartAttacking(Hit weaponHit)
	{
		hit = weaponHit;
		isDealingDamage = true;
		alreadyDamaged.Clear();
	}

	public void StopAttacking()
	{
		isDealingDamage = false;
	}

	public override void OnHit(Hit weaponHit)
	{
		Core.Logic.CameraManager.ProCamera2DShake.ShakeUsingPreset("HardFall");
		WeaponOwner.SleepTimeByHit(weaponHit);
	}
}
