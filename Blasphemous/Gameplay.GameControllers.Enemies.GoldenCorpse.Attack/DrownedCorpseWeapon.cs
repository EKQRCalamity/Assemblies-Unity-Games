using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;

namespace Gameplay.GameControllers.Enemies.GoldenCorpse.Attack;

public class DrownedCorpseWeapon : Weapon
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
