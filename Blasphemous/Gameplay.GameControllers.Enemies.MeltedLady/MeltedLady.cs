using System;
using Gameplay.GameControllers.Bosses.Quirce.Attack;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.MeltedLady;

public class MeltedLady : FloatingLady, IDamageable
{
	private EnemyBehaviour behaviour;

	public BossInstantProjectileAttack Attack { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		Attack = GetComponentInChildren<BossInstantProjectileAttack>();
		behaviour = GetComponent<EnemyBehaviour>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		Attack.SetDamage((int)Stats.Strength.Final);
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!behaviour.enabled)
		{
			behaviour.enabled = true;
		}
	}

	public void Damage(Hit hit)
	{
		base.DamageArea.TakeDamage(hit);
		base.ColorFlash.TriggerColorFlash();
		if (Status.Dead)
		{
			base.DamageArea.DamageAreaCollider.enabled = false;
			AnimatorInyector.Death();
		}
		else
		{
			AnimatorInyector.Hurt();
			base.Audio.Hurt();
		}
		SleepTimeByHit(hit);
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
