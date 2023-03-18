using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;

namespace Gameplay.GameControllers.Enemies.WheelCarrier.Attack;

public class WheelCarrierAttack : EnemyAttack
{
	private Hit _weaponHit;

	protected override void OnAwake()
	{
		base.OnAwake();
		base.CurrentEnemyWeapon = GetComponentInChildren<WheelCarrierWeapon>();
	}

	protected override void OnStart()
	{
		base.OnStart();
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
		base.CurrentEnemyWeapon.Attack(_weaponHit);
	}
}
