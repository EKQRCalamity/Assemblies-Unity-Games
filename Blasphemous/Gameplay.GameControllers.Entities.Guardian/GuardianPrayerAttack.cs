using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities.Weapon;

namespace Gameplay.GameControllers.Entities.Guardian;

public class GuardianPrayerAttack : Attack, IDirectAttack
{
	[EventRef]
	public string HitSound;

	private Hit guardianHit;

	private Gameplay.GameControllers.Entities.Weapon.Weapon guardianWeapon;

	protected override void OnAwake()
	{
		base.OnAwake();
		guardianWeapon = GetComponentInChildren<GuardianPrayerWeapon>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		CreateHit();
	}

	public override void CurrentWeaponAttack()
	{
		base.CurrentWeaponAttack();
		guardianWeapon.Attack(guardianHit);
	}

	public void CreateHit()
	{
		guardianHit = new Hit
		{
			DamageAmount = base.EntityOwner.Stats.Strength.Final * Core.Logic.Penitent.Stats.DamageMultiplier.Final,
			AttackingEntity = base.EntityOwner.gameObject,
			DamageElement = DamageArea.DamageElement.Normal,
			DamageType = DamageArea.DamageType.Heavy,
			DestroysProjectiles = true,
			HitSoundId = HitSound
		};
	}

	public void SetDamage(int damage)
	{
		if (damage >= 0)
		{
			guardianHit.DamageAmount = damage;
		}
	}
}
