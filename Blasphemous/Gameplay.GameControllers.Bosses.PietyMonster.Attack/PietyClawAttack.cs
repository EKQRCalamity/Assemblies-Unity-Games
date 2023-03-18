using Gameplay.GameControllers.Bosses.PietyMonster.IA;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.PietyMonster.Attack;

public class PietyClawAttack : EnemyAttack
{
	public PietyMonsterBehaviour PietyMonsterBehaviour;

	public Weapon PietyClaw;

	public Vector2 DamageAreaOffset;

	public Vector2 DamageAreaSize;

	public float DamageAmount = 25f;

	public bool Unavoidable = true;

	private PietyMonster _pietyMonster;

	[Range(0f, 2f)]
	public float DamageFactor = 1f;

	protected override void OnAwake()
	{
		base.OnAwake();
		_pietyMonster = (PietyMonster)base.EntityOwner;
		base.CurrentEnemyWeapon = PietyClaw;
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
			hit.Unnavoidable = Unavoidable;
			Hit weapondHit = hit;
			base.CurrentEnemyWeapon.Attack(weapondHit);
		}
	}

	public override void OnAttack(Hit hit)
	{
	}
}
