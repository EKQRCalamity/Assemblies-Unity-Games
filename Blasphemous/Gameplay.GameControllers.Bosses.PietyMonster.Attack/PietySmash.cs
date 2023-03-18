using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;

namespace Gameplay.GameControllers.Bosses.PietyMonster.Attack;

public class PietySmash : Weapon
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
