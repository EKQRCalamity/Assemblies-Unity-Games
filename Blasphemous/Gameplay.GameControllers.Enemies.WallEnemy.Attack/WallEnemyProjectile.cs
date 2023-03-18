using Framework.Util;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.WallEnemy.Attack;

public class WallEnemyProjectile : Weapon
{
	public SpriteRenderer SpriteRenderer;

	private Hit _rangedProjectileHit;

	public AttackArea AttackArea { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		AttackArea = GetComponentInChildren<AttackArea>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		SpriteRenderer = GetComponent<SpriteRenderer>();
		AttackArea.OnEnter += OnEnterAttackArea;
		_rangedProjectileHit = new Hit
		{
			AttackingEntity = WeaponOwner.gameObject,
			DamageAmount = WeaponOwner.Stats.Strength.Final,
			DamageType = DamageArea.DamageType.Normal,
			Unnavoidable = false
		};
	}

	private void OnEnterAttackArea(object sender, Collider2DParam e)
	{
		e.Collider2DArg.GetComponentInParent<IDamageable>().Damage(_rangedProjectileHit);
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!SpriteRenderer.isVisible)
		{
			Destroy();
		}
	}

	public override void Attack(Hit weapondHit)
	{
	}

	public override void OnHit(Hit weaponHit)
	{
	}

	public override void OnObjectReuse()
	{
		base.OnObjectReuse();
	}

	public void SetOwner(Entity owner)
	{
		WeaponOwner = owner;
		AttackArea.Entity = owner;
	}

	public void Dispose()
	{
		Destroy();
	}
}
