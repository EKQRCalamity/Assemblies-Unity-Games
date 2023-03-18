using Framework.Util;
using Gameplay.GameControllers.Enemies.DrownedCorpse;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;

namespace Gameplay.GameControllers.Enemies.GoldenCorpse.Attack;

public class DrownedCorpseAttack : EnemyAttack
{
	private bool _attackDone;

	private Hit _attackHit;

	private float cooldown;

	private float damageContactCooldown = 0.1f;

	private ContactDamage _contactDamage { get; set; }

	private Gameplay.GameControllers.Enemies.DrownedCorpse.DrownedCorpse DrownedCorpse { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		base.CurrentEnemyWeapon = GetComponentInChildren<Weapon>();
		_contactDamage = base.EntityOwner.GetComponentInChildren<ContactDamage>();
		DrownedCorpse = (Gameplay.GameControllers.Enemies.DrownedCorpse.DrownedCorpse)base.EntityOwner;
	}

	protected override void OnStart()
	{
		base.OnStart();
		base.CurrentEnemyWeapon.AttackAreas[0].OnStay += OnStayAttackArea;
		base.CurrentEnemyWeapon.AttackAreas[0].OnExit += OnOnExitAttackArea;
		SetContactDamage(ContactDamageAmount);
	}

	private void OnStayAttackArea(object sender, Collider2DParam e)
	{
		bool isChasing = DrownedCorpse.Behaviour.IsChasing;
		if (!base.EntityOwner.Status.Dead && isChasing)
		{
			CurrentWeaponAttack();
		}
	}

	private void OnOnExitAttackArea(object sender, Collider2DParam e)
	{
		_attackDone = false;
	}

	public override void CurrentWeaponAttack()
	{
		base.CurrentWeaponAttack();
		if (!_attackDone)
		{
			_attackDone = true;
			Hit simpleAttack = GetSimpleAttack();
			base.CurrentEnemyWeapon.Attack(simpleAttack);
		}
	}

	private Hit GetSimpleAttack()
	{
		_attackHit.AttackingEntity = base.EntityOwner.gameObject;
		_attackHit.DamageType = DamageArea.DamageType.Normal;
		_attackHit.DamageAmount = base.EntityOwner.Stats.Strength.Final;
		_attackHit.Force = Force;
		_attackHit.forceGuardslide = true;
		_attackHit.HitSoundId = HitSound;
		_attackHit.OnGuardCallback = OnGuardedAttack;
		return _attackHit;
	}

	private void OnGuardedAttack(Hit h)
	{
		if ((bool)DrownedCorpse.Behaviour)
		{
			DrownedCorpse.Behaviour.OnGuarded();
		}
	}

	private void OnDestroy()
	{
		base.CurrentEnemyWeapon.AttackAreas[0].OnEnter -= OnStayAttackArea;
		base.CurrentEnemyWeapon.AttackAreas[0].OnExit -= OnOnExitAttackArea;
	}
}
