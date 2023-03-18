using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;

namespace Gameplay.GameControllers.Enemies.Flagellant.Attack;

public class FlagellantAttack : EnemyAttack
{
	private Flagellant _flagellant;

	protected override void OnAwake()
	{
		base.OnAwake();
		_flagellant = (Flagellant)base.EntityOwner;
		base.CurrentEnemyWeapon = base.EntityOwner.GetComponentInChildren<Weapon>();
	}

	public override void CurrentWeaponAttack(DamageArea.DamageType damageType)
	{
		base.CurrentWeaponAttack(damageType);
		if (!(base.CurrentEnemyWeapon == null))
		{
			float final = _flagellant.Stats.Strength.Final;
			Hit weapondHit = default(Hit);
			weapondHit.AttackingEntity = _flagellant.gameObject;
			weapondHit.DamageType = DamageType;
			weapondHit.DamageAmount = final;
			weapondHit.HitSoundId = HitSound;
			base.CurrentEnemyWeapon.Attack(weapondHit);
		}
	}
}
