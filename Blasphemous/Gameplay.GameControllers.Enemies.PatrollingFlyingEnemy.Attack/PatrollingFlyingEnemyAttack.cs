using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;

namespace Gameplay.GameControllers.Enemies.PatrollingFlyingEnemy.Attack;

public class PatrollingFlyingEnemyAttack : EnemyAttack
{
	public AttackArea AttackArea { get; set; }

	public bool EntityAttacked { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		base.CurrentEnemyWeapon = base.EntityOwner.GetComponentInChildren<Weapon>();
	}

	protected override void OnStart()
	{
		base.OnStart();
	}

	private void OnDeath()
	{
	}

	public override void CurrentWeaponAttack(DamageArea.DamageType damageType)
	{
		base.CurrentWeaponAttack(damageType);
		if (!(base.CurrentEnemyWeapon == null))
		{
			float final = base.EntityOwner.Stats.Strength.Final;
			Hit hit = default(Hit);
			hit.AttackingEntity = base.EntityOwner.gameObject;
			hit.DamageType = damageType;
			hit.DamageAmount = final;
			hit.HitSoundId = HitSound;
			Hit weapondHit = hit;
			base.CurrentEnemyWeapon.Attack(weapondHit);
		}
	}
}
