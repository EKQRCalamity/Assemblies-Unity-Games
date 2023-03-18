using System;
using Framework.Util;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Crisanta;

public class CrisantaMeleeAttack : EnemyAttack, IDirectAttack, IPaintAttackCollider
{
	[FoldoutGroup("Additional attack settings", 0)]
	public bool unavoidable;

	[FoldoutGroup("Additional attack settings", 0)]
	public bool forceGuardSlideDirection;

	[FoldoutGroup("Additional attack settings", 0)]
	public float damage;

	public bool damageOnEnterArea;

	public Hit WeaponHit { get; private set; }

	public event Action OnMeleeAttackGuarded;

	protected override void OnAwake()
	{
		base.OnAwake();
		CreateHit();
		base.CurrentEnemyWeapon = GetComponentInChildren<CrisantaWeapon>();
		AttachShowScriptIfNeeded();
	}

	private void OnGuardedAttack(Hit h)
	{
		if (this.OnMeleeAttackGuarded != null)
		{
			this.OnMeleeAttackGuarded();
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		base.CurrentEnemyWeapon.AttackAreas[0].OnEnter += OnEnterAttackArea;
	}

	private void OnEnterAttackArea(object sender, Collider2DParam e)
	{
		Debug.Log("SOMETHING ENTERS AREA");
		if (damageOnEnterArea)
		{
			CurrentWeaponAttack();
		}
	}

	public override void CurrentWeaponAttack()
	{
		base.CurrentWeaponAttack();
		base.CurrentEnemyWeapon.Attack(WeaponHit);
	}

	public void CreateHit()
	{
		WeaponHit = new Hit
		{
			AttackingEntity = base.EntityOwner.gameObject,
			DamageAmount = damage,
			DamageType = DamageType,
			DamageElement = DamageElement,
			Unnavoidable = unavoidable,
			ForceGuardSlideDirection = forceGuardSlideDirection,
			HitSoundId = HitSound,
			Force = Force,
			OnGuardCallback = OnGuardedAttack
		};
	}

	public void SetDamage(int damage)
	{
		if (damage >= 0)
		{
			this.damage = damage;
			CreateHit();
		}
	}

	public bool IsCurrentlyDealingDamage()
	{
		return damageOnEnterArea;
	}

	public void AttachShowScriptIfNeeded()
	{
	}
}
