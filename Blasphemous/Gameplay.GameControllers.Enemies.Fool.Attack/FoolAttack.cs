using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Fool.Attack;

public class FoolAttack : EnemyAttack
{
	private Hit _attackHit;

	private bool _attackDone;

	private const float coolDown = 0.1f;

	private float contactDamageCoolDown = 0.1f;

	public AttackArea AttackArea { get; private set; }

	private Fool Fool { get; set; }

	private ContactDamage ContactDamage { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		base.CurrentEnemyWeapon = GetComponentInChildren<Weapon>();
		AttackArea = GetComponentInChildren<AttackArea>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		Fool = (Fool)base.EntityOwner;
		ContactDamage = Fool.GetComponentInChildren<ContactDamage>();
		AttackArea.OnStay += AttackAreaOnStay;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (ContactDamage.IsTargetOverlapped && !Fool.Status.Dead)
		{
			contactDamageCoolDown -= Time.deltaTime;
			if (contactDamageCoolDown <= 0f)
			{
				contactDamageCoolDown = 0.1f;
				ContactAttack(Core.Logic.Penitent);
			}
		}
		else
		{
			contactDamageCoolDown = 0.1f;
		}
	}

	private void AttackAreaOnStay(object sender, Collider2DParam e)
	{
		if (!Fool.Behaviour.TurningAround && !Fool.Status.Dead && Core.Logic.Penitent.Status.IsGrounded)
		{
			CurrentWeaponAttack();
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
		_attackHit.DamageAmount = base.EntityOwner.Stats.Strength.Final;
		_attackHit.Force = Force;
		_attackHit.HitSoundId = HitSound;
		return _attackHit;
	}

	private void OnDestroy()
	{
		AttackArea.OnEnter -= AttackAreaOnStay;
	}
}
