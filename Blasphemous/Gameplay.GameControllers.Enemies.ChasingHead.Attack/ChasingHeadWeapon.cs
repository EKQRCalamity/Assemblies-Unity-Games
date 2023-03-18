using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;

namespace Gameplay.GameControllers.Enemies.ChasingHead.Attack;

public class ChasingHeadWeapon : Weapon
{
	public override void Attack(Hit weapondHit)
	{
		GetDamageableEntities();
		AttackDamageableEntities(weapondHit);
	}

	public override void OnHit(Hit weaponHit)
	{
		DisableDamageArea();
	}

	private void DisableDamageArea()
	{
		ChasingHead chasingHead = (ChasingHead)WeaponOwner;
		chasingHead.DamageArea.DamageAreaCollider.enabled = false;
	}
}
