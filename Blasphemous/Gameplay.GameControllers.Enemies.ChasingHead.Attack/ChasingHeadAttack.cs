using Framework.Util;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.ChasingHead.Attack;

public class ChasingHeadAttack : EnemyAttack
{
	public readonly int DeathAnim = UnityEngine.Animator.StringToHash("Death");

	public AttackArea AttackArea { get; set; }

	public bool EntityAttacked { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		base.CurrentEnemyWeapon = base.EntityOwner.GetComponentInChildren<Weapon>();
		AttackArea = base.EntityOwner.GetComponentInChildren<AttackArea>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		AttackArea.OnStay += AttackAreaOnStay;
		AttackArea.OnExit += AttackAreaOnExit;
		base.EntityOwner.OnDeath += OnDeath;
	}

	private void AttackAreaOnStay(object sender, Collider2DParam e)
	{
		if (!EntityAttacked)
		{
			EntityAttacked = true;
			CurrentWeaponAttack(DamageArea.DamageType.Normal);
			base.EntityOwner.Animator.Play(DeathAnim);
		}
	}

	private void OnDeath()
	{
		if (AttackArea.WeaponCollider.enabled)
		{
			AttackArea.WeaponCollider.enabled = false;
		}
	}

	private void AttackAreaOnExit(object sender, Collider2DParam e)
	{
		if (EntityAttacked)
		{
			EntityAttacked = !EntityAttacked;
		}
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
