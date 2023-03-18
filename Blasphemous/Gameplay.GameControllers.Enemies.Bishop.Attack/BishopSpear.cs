using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;

namespace Gameplay.GameControllers.Enemies.Bishop.Attack;

public class BishopSpear : Weapon
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
