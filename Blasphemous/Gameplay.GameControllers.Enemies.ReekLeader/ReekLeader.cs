using System;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Enemies.ReekLeader.AI;
using Gameplay.GameControllers.Enemies.ReekLeader.Animator;
using Gameplay.GameControllers.Enemies.ReekLeader.Audio;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.ReekLeader;

public class ReekLeader : Enemy, IDamageable
{
	public ReekLeaderBehaviour Behaviour { get; private set; }

	public ReekLeaderAnimatorInyector AnimatorInyector { get; private set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public ReekLeaderAudio Audio { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		Behaviour = GetComponentInChildren<ReekLeaderBehaviour>();
		AnimatorInyector = GetComponentInChildren<ReekLeaderAnimatorInyector>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		Audio = GetComponentInChildren<ReekLeaderAudio>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		base.Target = Core.Logic.Penitent.gameObject;
		Status.CastShadow = true;
		Status.IsGrounded = true;
		if (!base.Landing)
		{
			SetPositionAtStart();
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (base.Target == null)
		{
			base.Target = Core.Logic.Penitent.gameObject;
		}
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

	public override void SetPositionAtStart()
	{
		base.SetPositionAtStart();
		float distance = Physics2D.Raycast(base.transform.position, -Vector2.up, 5f, Behaviour.BlockLayerMask).distance;
		Vector3 position = base.transform.position;
		position.y -= distance;
		base.transform.position = position;
		base.Landing = true;
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}
}
