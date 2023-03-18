using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;

namespace Gameplay.GameControllers.Enemies.Firethrower.Attack;

public class FirethrowerWeapon : Weapon
{
	public override void Attack(Hit weaponHit)
	{
		GetDamageableEntities();
		AttackDamageableEntities(weaponHit);
	}

	public override void OnHit(Hit weaponHit)
	{
	}
}
