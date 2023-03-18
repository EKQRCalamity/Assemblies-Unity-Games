using System.Collections.Generic;
using Gameplay.GameControllers.Enemies.GhostKnight.AI;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using Gameplay.GameControllers.Penitent;

namespace Gameplay.GameControllers.Enemies.GhostKnight.Attack;

public class GhostKnightSword : Weapon
{
	private List<IDamageable> _damageables;

	public GhostKnightBehaviour Behaviour { get; private set; }

	protected override void OnStart()
	{
		base.OnStart();
		Behaviour = WeaponOwner.GetComponentInChildren<GhostKnightBehaviour>();
	}

	public override void Attack(Hit weapondHit)
	{
		_damageables = GetDamageableEntities();
		AttackDamageableEntities(weapondHit);
	}

	public override void OnHit(Hit weaponHit)
	{
	}

	public bool TargetIsOnParryChance()
	{
		bool result = false;
		for (int i = 0; i < _damageables.Count; i++)
		{
			Gameplay.GameControllers.Penitent.Penitent penitent = (Gameplay.GameControllers.Penitent.Penitent)_damageables[i];
			if (penitent.Parry.IsOnParryChance)
			{
				result = true;
				break;
			}
		}
		return result;
	}
}
