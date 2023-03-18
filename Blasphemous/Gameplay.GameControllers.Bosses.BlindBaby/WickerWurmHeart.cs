using System;
using DamageEffect;
using Gameplay.GameControllers.Bosses.WickerWurm;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.BlindBaby;

public class WickerWurmHeart : Enemy, IDamageable
{
	public AnimationCurve slowTimeCurve;

	public WickerWurm ownerWurm;

	public EnemyDamageArea DamageArea { get; private set; }

	public WickerWurmAnimatorInyector AnimatorInyector { get; private set; }

	public DamageEffectScript damageEffect { get; private set; }

	private AttackArea attackArea { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		AnimatorInyector = GetComponentInChildren<WickerWurmAnimatorInyector>();
		damageEffect = GetComponentInChildren<DamageEffectScript>();
		attackArea = GetComponentInChildren<AttackArea>();
		if ((bool)ownerWurm)
		{
			ownerWurm.OnDeath += OnWurmDeath;
		}
	}

	private void DamageFlash()
	{
		damageEffect.Blink(0f, 0.1f);
	}

	public void Damage(Hit hit)
	{
		DamageFlash();
		ownerWurm.Damage(hit);
	}

	private void OnWurmDeath()
	{
		if ((bool)attackArea)
		{
			attackArea.WeaponCollider.enabled = false;
		}
		if ((bool)ownerWurm)
		{
			ownerWurm.OnDeath -= OnWurmDeath;
		}
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	public override EnemyFloorChecker EnemyFloorChecker()
	{
		throw new NotImplementedException();
	}

	public override EnemyAttack EnemyAttack()
	{
		throw new NotImplementedException();
	}

	public override EnemyBumper EnemyBumper()
	{
		throw new NotImplementedException();
	}

	protected override void EnablePhysics(bool enable = true)
	{
		throw new NotImplementedException();
	}
}
