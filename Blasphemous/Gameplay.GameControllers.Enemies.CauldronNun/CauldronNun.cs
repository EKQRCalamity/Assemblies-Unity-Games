using System;
using Framework.FrameworkCore;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Enemies.CauldronNun.AI;
using Gameplay.GameControllers.Enemies.CauldronNun.Animator;
using Gameplay.GameControllers.Enemies.CauldronNun.Audio;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.CauldronNun;

public class CauldronNun : Enemy, IDamageable
{
	public CauldronNunBehaviour Behaviour { get; private set; }

	public CauldronNunAnimatorInyector AnimatorInyector { get; private set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public ColorFlash colorFlash { get; private set; }

	public CauldronNunAudio Audio { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		Behaviour = GetComponentInChildren<CauldronNunBehaviour>();
		AnimatorInyector = GetComponentInChildren<CauldronNunAnimatorInyector>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		colorFlash = GetComponentInChildren<ColorFlash>();
		Audio = GetComponentInChildren<CauldronNunAudio>();
	}

	private void SetDefaultOrientation(Vector3 targetPos)
	{
		SetOrientation((!(targetPos.x >= base.transform.position.x)) ? EntityOrientation.Left : EntityOrientation.Right);
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

	public void Damage(Hit hit)
	{
		DamageArea.TakeDamage(hit);
		colorFlash.TriggerColorFlash();
		if (Status.Dead)
		{
			DamageArea.DamageAreaCollider.enabled = false;
			Behaviour.Death();
		}
		else
		{
			Behaviour.Damage();
		}
		SleepTimeByHit(hit);
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}
}
