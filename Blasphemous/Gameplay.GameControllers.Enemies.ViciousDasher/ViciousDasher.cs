using System;
using CreativeSpore.SmartColliders;
using Framework.Managers;
using Gameplay.GameControllers.Combat;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Enemies.ViciousDasher.AI;
using Gameplay.GameControllers.Enemies.ViciousDasher.Animator;
using Gameplay.GameControllers.Enemies.ViciousDasher.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.StateMachine;
using Plugins.GhostSprites2D.Scripts.GhostSprites;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.ViciousDasher;

public class ViciousDasher : Enemy, IDamageable
{
	public StateMachine StateMachine { get; private set; }

	public ViciousDasherBehaviour ViciousDasherBehaviour { get; private set; }

	public ViciousDasherAnimatorInyector AnimatorInjector { get; private set; }

	public MotionLerper MotionLerper { get; set; }

	public EnemyDamageArea DamageArea { get; set; }

	public ViciousDasherAttack Attack { get; private set; }

	public EntityMotionChecker MotionChecker { get; private set; }

	public VisionCone VisionCone { get; private set; }

	public ColorFlash ColorFlash { get; private set; }

	public bool IsTargetVisible { get; private set; }

	public EntityExecution EntExecution { get; private set; }

	public GhostSprites GhostSprites { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		StateMachine = GetComponent<StateMachine>();
		ViciousDasherBehaviour = GetComponent<ViciousDasherBehaviour>();
		AnimatorInjector = GetComponentInChildren<ViciousDasherAnimatorInyector>();
		MotionLerper = GetComponentInChildren<MotionLerper>();
		base.Controller = GetComponentInChildren<PlatformCharacterController>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		Attack = GetComponentInChildren<ViciousDasherAttack>();
		MotionChecker = GetComponentInChildren<EntityMotionChecker>();
		VisionCone = GetComponentInChildren<VisionCone>();
		ColorFlash = GetComponentInChildren<ColorFlash>();
		EntExecution = GetComponentInChildren<EntityExecution>();
		GhostSprites = GetComponentInChildren<GhostSprites>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		Status.CastShadow = true;
		MotionLerper motionLerper = MotionLerper;
		motionLerper.OnLerpStart = (Core.SimpleEvent)Delegate.Combine(motionLerper.OnLerpStart, new Core.SimpleEvent(OnLerpStart));
		MotionLerper motionLerper2 = MotionLerper;
		motionLerper2.OnLerpStop = (Core.SimpleEvent)Delegate.Combine(motionLerper2.OnLerpStop, new Core.SimpleEvent(OnLerpStop));
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		Status.IsGrounded = base.Controller.IsGrounded;
		if (base.Target != null)
		{
			base.DistanceToTarget = Vector2.Distance(base.transform.position, base.Target.transform.position);
			IsTargetVisible = VisionCone.CanSeeTarget(base.Target.transform, "Penitent");
		}
		else
		{
			base.Target = Core.Logic.Penitent.gameObject;
		}
		if (!base.Landing)
		{
			base.Landing = true;
			SetPositionAtStart();
		}
	}

	private void OnLerpStart()
	{
		GhostSprites.EnableGhostTrail = true;
		DamageArea.DamageAreaCollider.enabled = false;
	}

	private void OnLerpStop()
	{
		GhostSprites.EnableGhostTrail = false;
		DamageArea.DamageAreaCollider.enabled = true;
	}

	public override void SetPositionAtStart()
	{
		base.SetPositionAtStart();
		float groundDist = base.Controller.GroundDist;
		Vector3 position = new Vector3(base.transform.position.x, base.transform.position.y - groundDist, base.transform.position.z);
		base.transform.position = position;
	}

	public void Damage(Hit hit)
	{
		if (MotionLerper.IsLerping)
		{
			return;
		}
		if (Execution(hit))
		{
			GetStun(hit);
			return;
		}
		DamageArea.TakeDamage(hit);
		if (Status.Dead)
		{
			AnimatorInjector.Death();
			return;
		}
		if (ViciousDasherBehaviour.GotParry)
		{
			ViciousDasherBehaviour.GotParry = false;
			ViciousDasherBehaviour.ResetParryRecover();
		}
		AnimatorInjector.IsParried(isParried: false);
		ColorFlash.TriggerColorFlash();
	}

	public override void Parry()
	{
		base.Parry();
		ViciousDasherBehaviour.Parry();
	}

	public override void GetStun(Hit hit)
	{
		base.GetStun(hit);
		if (!base.IsStunt && Mathf.Abs(base.Controller.SlopeAngle) < 1f)
		{
			Core.Audio.EventOneShotPanned(hit.HitSoundId, base.transform.position);
			AnimatorInjector.IsParried(isParried: false);
			ViciousDasherBehaviour.Execution();
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
		return Attack;
	}

	public override EnemyBumper EnemyBumper()
	{
		throw new NotImplementedException();
	}

	protected override void EnablePhysics(bool enable = true)
	{
		throw new NotImplementedException();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (MotionLerper.OnLerpStart != null)
		{
			MotionLerper motionLerper = MotionLerper;
			motionLerper.OnLerpStart = (Core.SimpleEvent)Delegate.Remove(motionLerper.OnLerpStart, new Core.SimpleEvent(OnLerpStart));
		}
		if (MotionLerper.OnLerpStop != null)
		{
			MotionLerper motionLerper2 = MotionLerper;
			motionLerper2.OnLerpStop = (Core.SimpleEvent)Delegate.Remove(motionLerper2.OnLerpStop, new Core.SimpleEvent(OnLerpStop));
		}
	}
}
