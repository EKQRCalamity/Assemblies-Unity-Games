using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.GoldenCorpse.Attack;

public class GoldenCorpseAttack : EnemyAttack
{
	private Hit _attackHit;

	private bool _attackDone;

	private float damageContactCooldown = 0.1f;

	private float cooldown;

	private ContactDamage _contactDamage { get; set; }

	private GoldenCorpse GoldenCorpse { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		base.CurrentEnemyWeapon = GetComponentInChildren<Weapon>();
		_contactDamage = base.EntityOwner.GetComponentInChildren<ContactDamage>();
		GoldenCorpse = (GoldenCorpse)base.EntityOwner;
	}

	protected override void OnStart()
	{
		base.OnStart();
		SetContactDamage(ContactDamageAmount);
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		bool isNapping = GoldenCorpse.Behaviour.isNapping;
		_contactDamage.enabled = !isNapping;
		if (_contactDamage.IsTargetOverlapped && !isNapping)
		{
			cooldown += Time.deltaTime;
			if (cooldown >= damageContactCooldown)
			{
				ContactAttack(Core.Logic.Penitent);
			}
		}
		else
		{
			cooldown = 0f;
		}
	}

	public override void CurrentWeaponAttack()
	{
		base.CurrentWeaponAttack();
		Hit simpleAttack = GetSimpleAttack();
		base.CurrentEnemyWeapon.Attack(simpleAttack);
	}

	private Hit GetSimpleAttack()
	{
		_attackHit.AttackingEntity = base.EntityOwner.gameObject;
		_attackHit.DamageType = DamageArea.DamageType.Normal;
		_attackHit.DamageAmount = ContactDamageAmount;
		_attackHit.Force = Force;
		_attackHit.HitSoundId = HitSound;
		return _attackHit;
	}
}
