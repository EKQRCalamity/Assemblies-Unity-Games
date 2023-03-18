using System;
using Gameplay.GameControllers.Enemies.ChasingHead.AI;
using Gameplay.GameControllers.Enemies.ChasingHead.Audio;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Enemies.HeadThrower;
using Gameplay.GameControllers.Entities;
using Plugins.GhostSprites2D.Scripts.GhostSprites;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.ChasingHead;

public class ChasingHead : Enemy, IDamageable
{
	public ChasingHeadAudio Audio { get; private set; }

	public ChasingHeadBehaviour Behaviour { get; private set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public Gameplay.GameControllers.Enemies.HeadThrower.HeadThrower OwnHeadThrower { get; set; }

	public GhostSprites GhostSprites { get; set; }

	public void Damage(Hit hit)
	{
		DamageArea.TakeDamage(hit);
		if (Status.Dead)
		{
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

	public void SetTarget(GameObject target)
	{
		base.Target = target;
	}

	protected override void OnStart()
	{
		base.OnStart();
		Behaviour = GetComponent<ChasingHeadBehaviour>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		Audio = GetComponentInChildren<ChasingHeadAudio>();
		GhostSprites = GetComponentInChildren<GhostSprites>();
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
