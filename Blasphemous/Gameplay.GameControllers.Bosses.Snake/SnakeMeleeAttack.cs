using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Snake;

public class SnakeMeleeAttack : EnemyAttack, IDirectAttack, IPaintAttackCollider
{
	[FoldoutGroup("Additional attack settings", 0)]
	public bool Unavoidable;

	[FoldoutGroup("Additional attack settings", 0)]
	public bool Unparriable;

	[FoldoutGroup("Additional attack settings", 0)]
	public bool Unblockable;

	[FoldoutGroup("Additional attack settings", 0)]
	public bool ForceGuardSlideDirection;

	[FoldoutGroup("Additional attack settings", 0)]
	public bool CheckOrientationsForGuardslide;

	[FoldoutGroup("Additional attack settings", 0)]
	public float Damage;

	[HideInInspector]
	public bool DealsDamage;

	public Core.SimpleEvent OnAttackGuarded;

	public Hit WeaponHit { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		CreateHit();
		base.CurrentEnemyWeapon = GetComponentInChildren<SnakeWeapon>();
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
		if (DealsDamage)
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
			DamageAmount = Damage,
			DamageType = DamageType,
			DamageElement = DamageElement,
			Unnavoidable = Unavoidable,
			Unparriable = Unparriable,
			Unblockable = Unblockable,
			ForceGuardSlideDirection = ForceGuardSlideDirection,
			CheckOrientationsForGuardslide = CheckOrientationsForGuardslide,
			HitSoundId = HitSound,
			Force = Force,
			OnGuardCallback = OnGuardCallback
		};
	}

	public void SetDamage(int damage)
	{
		if (damage >= 0)
		{
			Damage = damage;
			CreateHit();
		}
	}

	public bool IsCurrentlyDealingDamage()
	{
		return DealsDamage;
	}

	public void AttachShowScriptIfNeeded()
	{
	}
}
