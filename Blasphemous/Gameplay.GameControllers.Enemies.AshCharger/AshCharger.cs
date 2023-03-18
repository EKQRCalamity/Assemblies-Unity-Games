using System;
using CreativeSpore.SmartColliders;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Enemies.AshCharger.AI;
using Gameplay.GameControllers.Enemies.AshCharger.Animator;
using Gameplay.GameControllers.Enemies.AshCharger.Audio;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.StateMachine;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.AshCharger;

public class AshCharger : Enemy, IDamageable
{
	public NPCInputs Inputs { get; private set; }

	public VisionCone VisionCone { get; private set; }

	public ColorFlash ColorFlash { get; private set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public StateMachine StateMachine { get; private set; }

	public AshChargerAnimatorInyector AnimatorInyector { get; private set; }

	public EntityMotionChecker MotionChecker { get; private set; }

	public AshChargerBehaviour Behaviour { get; private set; }

	public AshChargerAudio Audio { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		Audio = GetComponentInChildren<AshChargerAudio>();
		Inputs = GetComponentInChildren<NPCInputs>();
		base.Controller = GetComponentInChildren<PlatformCharacterController>();
		ColorFlash = GetComponentInChildren<ColorFlash>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		VisionCone = GetComponentInChildren<VisionCone>();
		Behaviour = GetComponentInChildren<AshChargerBehaviour>();
		StateMachine = GetComponentInChildren<StateMachine>();
		MotionChecker = GetComponentInChildren<EntityMotionChecker>();
		AnimatorInyector = GetComponentInChildren<AshChargerAnimatorInyector>();
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
			DamageArea.TakeDamage(hit);
			if (Status.Dead)
			{
				AnimatorInyector.Death();
				return;
			}
			ColorFlash.TriggerColorFlash();
		}
		SleepTimeByHit(hit);
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}
}
