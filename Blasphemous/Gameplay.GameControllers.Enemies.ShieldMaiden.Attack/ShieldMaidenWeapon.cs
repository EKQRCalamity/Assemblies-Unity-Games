using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.ShieldMaiden.Attack;

public class ShieldMaidenWeapon : Weapon
{
	public override void Attack(Hit weaponHit)
	{
		GetDamageableEntities();
		AttackDamageableEntities(weaponHit);
		Debug.Log("ShieldMaiden WEAPON ATTACK");
	}

	public override void OnHit(Hit weaponHit)
	{
	}
}
