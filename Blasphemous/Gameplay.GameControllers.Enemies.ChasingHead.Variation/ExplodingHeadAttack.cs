using System;
using DG.Tweening;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Enemies.ChasingHead.AI;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.ChasingHead.Variation;

public class ExplodingHeadAttack : EnemyAttack
{
	public readonly int DeathAnim = UnityEngine.Animator.StringToHash("Death");

	private Hit _chasingHeadNormalHit;

	private Hit _chasingHeadHeavyHit;

	private int _enemyLayer;

	public AttackArea AttackArea { get; set; }

	public bool EntityAttacked { get; set; }

	public ChasingHead _chasingHead { get; private set; }

	private ChasingHeadBehaviour ChasingHeadBehaviour { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		base.CurrentEnemyWeapon = base.EntityOwner.GetComponentInChildren<Weapon>();
		AttackArea = base.EntityOwner.GetComponentInChildren<AttackArea>();
		ChasingHeadBehaviour = base.EntityOwner.GetComponentInChildren<ChasingHeadBehaviour>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		_enemyLayer = LayerMask.NameToLayer("Enemy");
		float final = base.EntityOwner.Stats.Strength.Final;
		GameObject attackingEntity = base.EntityOwner.gameObject;
		_chasingHeadNormalHit = new Hit
		{
			AttackingEntity = attackingEntity,
			DamageType = DamageArea.DamageType.Normal,
			DamageAmount = ContactDamageAmount,
			HitSoundId = HitSound,
			Force = Force
		};
		_chasingHeadHeavyHit = new Hit
		{
			AttackingEntity = attackingEntity,
			DamageType = DamageArea.DamageType.Heavy,
			DamageAmount = final,
			HitSoundId = HitSound,
			Force = Force
		};
		_chasingHead = (ChasingHead)base.EntityOwner;
		ChasingHeadBehaviour chasingHeadBehaviour = ChasingHeadBehaviour;
		chasingHeadBehaviour.OnHurtDisplacement = (Core.SimpleEvent)Delegate.Combine(chasingHeadBehaviour.OnHurtDisplacement, new Core.SimpleEvent(OnHurtDisplacement));
		AttackArea.OnStay += AttackAreaOnStay;
	}

	private void OnHurtDisplacement()
	{
		DOTween.Kill(_chasingHead.transform);
	}

	private void AttackAreaOnStay(object sender, Collider2DParam e)
	{
		if (e.Collider2DArg.CompareTag("Penitent") && !EntityAttacked)
		{
			EntityAttacked = true;
			ContactAttack();
			_chasingHead.Behaviour.enabled = false;
			base.EntityOwner.Animator.Play(DeathAnim);
		}
	}

	public override void CurrentWeaponAttack()
	{
		base.CurrentWeaponAttack();
		if (!(base.CurrentEnemyWeapon == null))
		{
			base.CurrentEnemyWeapon.Attack(_chasingHeadHeavyHit);
		}
	}

	private void ContactAttack()
	{
		if (!(base.CurrentEnemyWeapon == null))
		{
			AttackArea attackArea = AttackArea;
			attackArea.enemyLayerMask = (int)attackArea.enemyLayerMask ^ (1 << _enemyLayer);
			base.CurrentEnemyWeapon.Attack(_chasingHeadNormalHit);
		}
	}

	public void RangeAttack()
	{
		AttackArea.enemyLayerMask = (int)AttackArea.enemyLayerMask | (1 << _enemyLayer);
		SetDamageAreaExplosionSize();
		CurrentWeaponAttack();
	}

	private void SetDamageAreaExplosionSize()
	{
		((BoxCollider2D)AttackArea.WeaponCollider).size = new Vector2(4f, 4f);
	}

	private void OnDestroy()
	{
		if ((bool)AttackArea)
		{
			AttackArea.OnStay -= AttackAreaOnStay;
		}
		if ((bool)ChasingHeadBehaviour)
		{
			ChasingHeadBehaviour chasingHeadBehaviour = ChasingHeadBehaviour;
			chasingHeadBehaviour.OnHurtDisplacement = (Core.SimpleEvent)Delegate.Remove(chasingHeadBehaviour.OnHurtDisplacement, new Core.SimpleEvent(OnHurtDisplacement));
		}
	}
}
