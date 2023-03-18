using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;

namespace Gameplay.GameControllers.Enemies.Fool.Attack;

public class FoolWeapon : Weapon
{
	public override void Attack(Hit weapondHit)
	{
		GetDamageableEntities();
		AttackDamageableEntities(weapondHit);
	}

	public override void OnHit(Hit weaponHit)
	{
	}
}
