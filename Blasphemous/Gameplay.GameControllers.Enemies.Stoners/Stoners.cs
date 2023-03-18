using System;
using Framework.Managers;
using Gameplay.GameControllers.Effects.NPCs.BloodDecals;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Enemies.Stoners.AI;
using Gameplay.GameControllers.Enemies.Stoners.Animator;
using Gameplay.GameControllers.Enemies.Stoners.Attack;
using Gameplay.GameControllers.Enemies.Stoners.Audio;
using Gameplay.GameControllers.Enemies.Stoners.Rock;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Stoners;

public class Stoners : Enemy, IDamageable
{
	public StonerAnimatorInyector AnimatorInyector;

	[Range(0f, 1f)]
	public float freezeTime = 0.1f;

	[Range(0f, 1f)]
	public float freezeTimeFactor = 0.2f;

	public EnemyDamageArea StonersDamageArea { get; set; }

	public BloodDecalPumper BloodDecalPumper { get; set; }

	public StonerBehaviour StonerBehaviour { get; set; }

	public RockPool RockPool { get; set; }

	public SpawnPoint RockSpawnPoint { get; set; }

	public StonersAttack Attack { get; set; }

	public StonersAudio Audio { get; set; }

	public void Damage(Hit hit)
	{
		StonersDamageArea.TakeDamage(hit);
		if (Status.Dead)
		{
			Audio.Death();
		}
		else
		{
			Audio.Damage();
		}
		AnimatorInyector.Hurt();
		SleepTimeByHit(hit);
		PumpBloodDecal();
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		AnimatorInyector = GetComponentInChildren<StonerAnimatorInyector>();
		StonersDamageArea = GetComponentInChildren<EnemyDamageArea>();
		BloodDecalPumper = GetComponentInChildren<BloodDecalPumper>();
		StonerBehaviour = GetComponentInChildren<StonerBehaviour>();
		RockPool = GetComponentInChildren<RockPool>();
		RockSpawnPoint = GetComponentInChildren<SpawnPoint>();
		Attack = GetComponentInChildren<StonersAttack>();
		Audio = GetComponentInChildren<StonersAudio>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		Status.IsVisibleOnCamera = IsVisible();
		base.Target = Core.Logic.Penitent.gameObject;
		StonersDamageArea.DamageAreaCollider.enabled = false;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		Status.IsVisibleOnCamera = IsVisible();
		base.DistanceToTarget = Vector2.Distance(base.transform.position, base.Target.transform.position);
		if (base.DistanceToTarget <= ActivationRange)
		{
			if (DisableBehaviourWhenInvisible)
			{
				StonerBehaviour.StartBehaviour();
			}
		}
		else if (DisableBehaviourWhenInvisible)
		{
			StonerBehaviour.PauseBehaviour();
		}
	}

	public override EnemyFloorChecker EnemyFloorChecker()
	{
		throw new NotImplementedException();
	}

	public override EnemyAttack EnemyAttack()
	{
		return Attack;
	}

	public override EnemyBumper EnemyBumper()
	{
		throw new NotImplementedException();
	}

	protected override void EnablePhysics(bool enable)
	{
		throw new NotImplementedException();
	}

	public bool IsVisible()
	{
		return Entity.IsVisibleFrom(spriteRenderer, UnityEngine.Camera.main);
	}

	protected void PumpBloodDecal()
	{
	}
}
