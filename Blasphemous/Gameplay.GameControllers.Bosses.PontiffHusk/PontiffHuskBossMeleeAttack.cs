using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.PontiffHusk;

public class PontiffHuskBossMeleeAttack : EnemyAttack, IDirectAttack, IPaintAttackCollider
{
	[FoldoutGroup("Additional attack settings", 0)]
	public bool unavoidable;

	[FoldoutGroup("Additional attack settings", 0)]
	public float damage;

	[HideInInspector]
	public bool dealsDamage;

	public Core.SimpleEvent OnAttackGuarded;

	public Hit WeaponHit { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		CreateHit();
		base.CurrentEnemyWeapon = GetComponentInChildren<PontiffHuskBossWeapon>();
		AttachShowScriptIfNeeded();
	}

	protected override void OnStart()
	{
		base.OnStart();
		base.CurrentEnemyWeapon.AttackAreas[0].OnEnter += OnEnterOrStayAttackArea;
		base.CurrentEnemyWeapon.AttackAreas[0].OnStay += OnEnterOrStayAttackArea;
	}

	private void OnEnterOrStayAttackArea(object sender, Collider2DParam e)
	{
		if (dealsDamage)
		{
			CurrentWeaponAttack();
		}
	}

	public override void CurrentWeaponAttack()
	{
		base.CurrentWeaponAttack();
		base.CurrentEnemyWeapon.Attack(WeaponHit);
	}

	private void OnGuardCallback(Hit obj)
	{
		if (OnAttackGuarded != null)
		{
			OnAttackGuarded();
		}
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
			HitSoundId = HitSound,
			Force = Force,
			OnGuardCallback = OnGuardCallback
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
		return dealsDamage;
	}

	public void AttachShowScriptIfNeeded()
	{
	}
}
