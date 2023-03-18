using System;
using CreativeSpore.SmartColliders;
using Framework.Managers;
using Gameplay.GameControllers.Combat;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Enemies.WalkingTomb.AI;
using Gameplay.GameControllers.Enemies.WalkingTomb.Animator;
using Gameplay.GameControllers.Enemies.WalkingTomb.Attack;
using Gameplay.GameControllers.Enemies.WalkingTomb.Audio;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.StateMachine;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.WalkingTomb;

public class WalkingTomb : Enemy, IDamageable
{
	public NPCInputs Inputs { get; private set; }

	public VisionCone VisionCone { get; private set; }

	public ColorFlash ColorFlash { get; private set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public StateMachine StateMachine { get; private set; }

	public WalkingTombAttack Attack { get; private set; }

	public WalkingTombAnimatorInjector AnimatorInjector { get; private set; }

	public EntityMotionChecker MotionChecker { get; private set; }

	public WalkingTombBehaviour Behaviour { get; private set; }

	public WalkingTombAudio Audio { get; private set; }

	public EntityExecution EntExecution { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		Attack = GetComponentInChildren<WalkingTombAttack>();
		Audio = GetComponentInChildren<WalkingTombAudio>();
		Inputs = GetComponentInChildren<NPCInputs>();
		base.Controller = GetComponentInChildren<PlatformCharacterController>();
		ColorFlash = GetComponentInChildren<ColorFlash>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		VisionCone = GetComponentInChildren<VisionCone>();
		Behaviour = GetComponentInChildren<WalkingTombBehaviour>();
		StateMachine = GetComponentInChildren<StateMachine>();
		MotionChecker = GetComponentInChildren<EntityMotionChecker>();
		AnimatorInjector = GetComponentInChildren<WalkingTombAnimatorInjector>();
		EntExecution = GetComponentInChildren<EntityExecution>();
		Behaviour.enabled = false;
		SpawnManager.OnPlayerSpawn += OnPlayerSpawn;
	}

	private void OnPlayerSpawn(Gameplay.GameControllers.Penitent.Penitent penitent)
	{
		SpawnManager.OnPlayerSpawn -= OnPlayerSpawn;
		base.Target = penitent.gameObject;
		Behaviour.enabled = true;
		Status.CastShadow = true;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		Status.IsGrounded = base.Controller.IsGrounded;
		if (!base.Landing)
		{
			base.Landing = true;
			SetPositionAtStart();
		}
	}

	public override void SetPositionAtStart()
	{
		base.SetPositionAtStart();
		float groundDist = base.Controller.GroundDist;
		Vector3 position = new Vector3(base.transform.position.x, base.transform.position.y - groundDist, base.transform.position.z);
		base.transform.position = position;
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
		if (GuardHit(hit))
		{
			Audio.PlayGuardHit();
		}
		else
		{
			if (Execution(hit))
			{
				GetStun(hit);
				SleepTimeByHit(hit);
				return;
			}
			DamageArea.TakeDamage(hit);
			if (Status.Dead)
			{
				AnimatorInjector.Death();
				return;
			}
			ColorFlash.TriggerColorFlash();
		}
		SleepTimeByHit(hit);
	}

	public override void GetStun(Hit hit)
	{
		base.GetStun(hit);
		if (!base.IsStunt && Mathf.Abs(base.Controller.SlopeAngle) < 1f)
		{
			Core.Audio.EventOneShotPanned(hit.HitSoundId, base.transform.position);
			Behaviour.Execution();
		}
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}
}
