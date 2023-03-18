using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;

namespace Gameplay.GameControllers.Bosses.BejeweledSaint.Attack;

public class BejeweledSaintStaff : Weapon
{
	public static Core.SimpleEvent OnSucceedHit;

	public override void Attack(Hit weapondHit)
	{
		GetDamageableEntities();
		AttackDamageableEntities(weapondHit);
	}

	public override void OnHit(Hit weaponHit)
	{
		if (OnSucceedHit != null)
		{
			OnSucceedHit();
		}
	}
}
