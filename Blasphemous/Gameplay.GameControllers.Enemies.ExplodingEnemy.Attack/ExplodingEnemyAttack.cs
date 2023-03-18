using FMODUnity;
using Framework.Util;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.ExplodingEnemy.Attack;

public class ExplodingEnemyAttack : EnemyAttack
{
	private Hit _attackHit;

	[SerializeField]
	[BoxGroup("Explosion Settings", true, false, 0)]
	public Vector2 ExplodingAttackAreaOffset;

	private Vector2 _defaultAttackAreaOffset;

	[SerializeField]
	[BoxGroup("Explosion Settings", true, false, 0)]
	public Vector2 ExplodingAttackAreaSize;

	private Vector2 _defaultAttackAreaSize;

	[SerializeField]
	[BoxGroup("Audio", true, false, 0)]
	[EventRef]
	protected string ExplosionHitSound;

	private bool _attackDone;

	public AttackArea AttackArea { get; private set; }

	public bool HasExplode { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		base.CurrentEnemyWeapon = GetComponentInChildren<Weapon>();
		AttackArea = GetComponentInChildren<AttackArea>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		AttackArea.OnStay += OnStayAttackArea;
		AttackArea.OnExit += OnExitAttackArea;
		_defaultAttackAreaOffset = new Vector2(AttackArea.WeaponCollider.offset.x, AttackArea.WeaponCollider.offset.y);
		_defaultAttackAreaSize = new Vector2(AttackArea.WeaponCollider.bounds.size.x, AttackArea.WeaponCollider.bounds.size.y);
	}

	private void OnStayAttackArea(object sender, Collider2DParam e)
	{
		if (!_attackDone && !HasExplode)
		{
			_attackDone = true;
			CurrentWeaponAttack();
		}
	}

	private void OnExitAttackArea(object sender, Collider2DParam e)
	{
		_attackDone = false;
	}

	public override void CurrentWeaponAttack()
	{
		base.CurrentWeaponAttack();
		ExplodingEnemy explodingEnemy = (ExplodingEnemy)base.EntityOwner;
		Hit weapondHit = ((!explodingEnemy.AnimatorInyector.IsExploding) ? GetSimpleAttack() : GetAreaAttack());
		if (weapondHit.DamageType == DamageArea.DamageType.Heavy)
		{
			SetAttackAreaExplosionCollider();
		}
		base.CurrentEnemyWeapon.Attack(weapondHit);
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

	private Hit GetAreaAttack()
	{
		_attackHit.AttackingEntity = base.EntityOwner.gameObject;
		_attackHit.DamageType = DamageArea.DamageType.Heavy;
		_attackHit.DamageAmount = base.EntityOwner.Stats.Strength.Final;
		_attackHit.Force = Force;
		_attackHit.HitSoundId = ExplosionHitSound;
		return _attackHit;
	}

	private void SetAttackAreaExplosionCollider()
	{
		if (!(AttackArea == null))
		{
			AttackArea.SetSize(ExplodingAttackAreaSize);
			AttackArea.SetOffset(ExplodingAttackAreaOffset);
		}
	}

	public void SetDefaultAttackAreaSize()
	{
		if (!(AttackArea == null))
		{
			AttackArea.SetSize(_defaultAttackAreaSize);
			AttackArea.SetOffset(_defaultAttackAreaOffset);
		}
	}

	private void OnDestroy()
	{
		AttackArea.OnStay -= OnStayAttackArea;
		AttackArea.OnExit -= OnExitAttackArea;
	}
}
