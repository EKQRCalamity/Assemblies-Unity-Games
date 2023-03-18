using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;

namespace Gameplay.GameControllers.Enemies.FlyingPortrait.Attack;

public class FlyingPortraitAttack : EnemyAttack
{
	private Hit _flyingPortraitHit;

	protected override void OnAwake()
	{
		base.OnAwake();
		base.CurrentEnemyWeapon = GetComponentInChildren<Weapon>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		_flyingPortraitHit = new Hit
		{
			AttackingEntity = base.EntityOwner.gameObject,
			DamageAmount = base.EntityOwner.Stats.Strength.Final,
			DamageType = DamageArea.DamageType.Normal,
			Force = 0f,
			HitSoundId = HitSound,
			Unnavoidable = false
		};
	}

	public override void CurrentWeaponAttack()
	{
		base.CurrentWeaponAttack();
		base.CurrentEnemyWeapon.Attack(_flyingPortraitHit);
	}
}
