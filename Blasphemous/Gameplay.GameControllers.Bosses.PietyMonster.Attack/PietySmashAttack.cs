using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.PietyMonster.Attack;

public class PietySmashAttack : EnemyAttack
{
	public Weapon PietySmash;

	private PietyMonster _pietyMonster;

	[Range(0f, 2f)]
	public float DamageFactor = 1f;

	public float DamageAmount = 35f;

	protected override void OnAwake()
	{
		base.OnAwake();
		_pietyMonster = (PietyMonster)base.EntityOwner;
		base.CurrentEnemyWeapon = PietySmash;
	}

	public override void CurrentWeaponAttack()
	{
		base.CurrentWeaponAttack();
		if (!(base.CurrentEnemyWeapon == null) && !_pietyMonster.Status.Dead)
		{
			Hit hit = default(Hit);
			hit.AttackingEntity = _pietyMonster.gameObject;
			hit.DamageType = DamageType;
			hit.DamageAmount = DamageAmount;
			hit.Force = Force;
			hit.HitSoundId = HitSound;
			Hit weapondHit = hit;
			base.CurrentEnemyWeapon.Attack(weapondHit);
		}
	}
}
