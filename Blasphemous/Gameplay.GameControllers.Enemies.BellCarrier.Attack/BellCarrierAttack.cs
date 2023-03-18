using System.Collections;
using Framework.Util;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.BellCarrier.Attack;

public class BellCarrierAttack : EnemyAttack
{
	private bool _attackAreasEnabled;

	private bool _attacked;

	private BellCarrier _bellCarrier;

	[SerializeField]
	protected AttackArea BellCarrierAttackArea;

	protected override void OnAwake()
	{
		base.OnAwake();
		_bellCarrier = GetComponentInParent<BellCarrier>();
		base.CurrentEnemyWeapon = GetComponent<Weapon>();
		_attackAreasEnabled = true;
	}

	protected override void OnStart()
	{
		base.OnStart();
		BellCarrierAttackArea.OnStay += BellCarrierAttackAreaOnStay;
		BellCarrierAttackArea.OnExit += BellCarrierAttackAreaOnExit;
		_bellCarrier.OnDeath += BellCarrierOnEntityDie;
	}

	private void BellCarrierAttackAreaOnStay(object sender, Collider2DParam collider2DParam)
	{
		if (!_bellCarrier.AnimatorInyector.Animator.GetCurrentAnimatorStateInfo(0).IsName("Running") || _attacked)
		{
			return;
		}
		_attacked = true;
		Gameplay.GameControllers.Penitent.Penitent componentInParent = collider2DParam.Collider2DArg.gameObject.GetComponentInParent<Gameplay.GameControllers.Penitent.Penitent>();
		Hit attackHit = GetAttackHit();
		if (!(componentInParent == null))
		{
			if (componentInParent.Status.Unattacable)
			{
				componentInParent.DamageArea.TakeDamage(attackHit, force: true);
			}
			else
			{
				CurrentWeaponAttack(attackHit.DamageType);
			}
		}
	}

	private void BellCarrierAttackAreaOnExit(object sender, Collider2DParam collider2DParam)
	{
		_attacked = false;
	}

	private void BellCarrierOnEntityDie()
	{
		if (BellCarrierAttackArea.WeaponCollider.enabled)
		{
			BellCarrierAttackArea.WeaponCollider.enabled = false;
		}
		if (_bellCarrier.EntityDamageArea.DamageAreaCollider.enabled)
		{
			_bellCarrier.EntityDamageArea.DamageAreaCollider.enabled = false;
		}
	}

	private Hit GetAttackHit()
	{
		Hit result = default(Hit);
		result.AttackingEntity = _bellCarrier.gameObject;
		result.DamageType = DamageArea.DamageType.Heavy;
		result.DamageAmount = _bellCarrier.Stats.Strength.Final;
		result.Force = Force;
		result.HitSoundId = HitSound;
		return result;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		bool isChasing = _bellCarrier.EnemyBehaviour.IsChasing;
		EnableAttackAreas(isChasing);
	}

	public override void CurrentWeaponAttack(DamageArea.DamageType damageType)
	{
		if (!(base.CurrentEnemyWeapon == null) && _bellCarrier.EntityDamageArea.DamageAreaCollider.enabled)
		{
			Hit attackHit = GetAttackHit();
			base.CurrentEnemyWeapon.Attack(attackHit);
			DisableDamageArea();
		}
	}

	private void DisableDamageArea()
	{
		if (_bellCarrier.EntityDamageArea.DamageAreaCollider.enabled)
		{
			StartCoroutine(DisableDamageAreaCoroutine());
		}
	}

	private IEnumerator DisableDamageAreaCoroutine()
	{
		Collider2D damageCollider = _bellCarrier.EntityDamageArea.DamageAreaCollider;
		if (damageCollider.enabled)
		{
			damageCollider.enabled = false;
		}
		yield return new WaitForSeconds(0.5f);
		if (!damageCollider.enabled)
		{
			damageCollider.enabled = true;
		}
	}

	private void EnableAttackAreas(bool attackAreaEnabled = true)
	{
		if (attackAreaEnabled)
		{
			if (!_attackAreasEnabled)
			{
				_attackAreasEnabled = true;
				SetAttackAreaStatus(attackAreaEnabled: true);
			}
		}
		else if (_attackAreasEnabled)
		{
			_attackAreasEnabled = false;
			SetAttackAreaStatus(attackAreaEnabled: false);
		}
	}

	private void SetAttackAreaStatus(bool attackAreaEnabled)
	{
		BellCarrierAttackArea.WeaponCollider.enabled = attackAreaEnabled;
	}

	private void OnDestroy()
	{
		_bellCarrier.OnDeath -= BellCarrierOnEntityDie;
	}
}
