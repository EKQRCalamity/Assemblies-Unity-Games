using Framework.Util;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.WaxCrawler.Attack;

public class WaxCrawlerAttack : EnemyAttack
{
	private float _accumulatedAttackTime;

	private AttackArea _attackArea;

	public WaxCrawler _waxCrawler;

	public float AttackLapse = 0.75f;

	public bool EnableAttackArea
	{
		get
		{
			return _attackArea.WeaponCollider.enabled;
		}
		set
		{
			Collider2D weaponCollider = _attackArea.WeaponCollider;
			weaponCollider.enabled = value;
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		_waxCrawler = (WaxCrawler)base.EntityOwner;
		base.CurrentEnemyWeapon = base.EntityOwner.GetComponentInChildren<Weapon>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		_attackArea = base.EntityOwner.GetComponentInChildren<AttackArea>();
		_attackArea.OnEnter += AttackAreaOnEnter;
		_attackArea.OnStay += AttackAreaOnStay;
		_attackArea.OnExit += AttackAreaOnExit;
	}

	private void AttackAreaOnEnter(object sender, Collider2DParam collider2DParam)
	{
		_accumulatedAttackTime = 0f;
		CurrentWeaponAttack(DamageArea.DamageType.Normal);
	}

	private void AttackAreaOnStay(object sender, Collider2DParam e)
	{
		_accumulatedAttackTime += Time.deltaTime;
		if (_accumulatedAttackTime >= AttackLapse)
		{
			_accumulatedAttackTime = 0f;
			CurrentWeaponAttack(DamageArea.DamageType.Normal);
		}
	}

	private void AttackAreaOnExit(object sender, Collider2DParam e)
	{
		_accumulatedAttackTime = 0f;
	}

	public override void CurrentWeaponAttack(DamageArea.DamageType damageType)
	{
		base.CurrentWeaponAttack(damageType);
		bool flag = base.EntityOwner.Animator.GetCurrentAnimatorStateInfo(0).IsName("Appear") || base.EntityOwner.Animator.GetCurrentAnimatorStateInfo(0).IsName("Disappear");
		if (!(base.CurrentEnemyWeapon == null) && !base.EntityOwner.Status.Dead && !flag)
		{
			Hit weapondHit = default(Hit);
			float final = _waxCrawler.Stats.Strength.Final;
			weapondHit.AttackingEntity = _waxCrawler.gameObject;
			weapondHit.DamageType = damageType;
			weapondHit.DamageAmount = final;
			weapondHit.HitSoundId = HitSound;
			base.CurrentEnemyWeapon.Attack(weapondHit);
		}
	}
}
