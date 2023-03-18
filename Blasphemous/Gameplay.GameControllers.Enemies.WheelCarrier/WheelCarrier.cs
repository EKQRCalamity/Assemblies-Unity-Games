using System;
using CreativeSpore.SmartColliders;
using DamageEffect;
using Framework.Managers;
using Gameplay.GameControllers.Combat;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Enemies.WheelCarrier.Animator;
using Gameplay.GameControllers.Enemies.WheelCarrier.Audio;
using Gameplay.GameControllers.Enemies.WheelCarrier.IA;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Traits;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.WheelCarrier;

public class WheelCarrier : Enemy, IDamageable
{
	public WheelCarrierBehaviour Behaviour { get; private set; }

	public NPCInputs Input { get; private set; }

	public SmartPlatformCollider Collider { get; private set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public WheelCarrierAnimatorInyector AnimatorInyector { get; private set; }

	public EnemyAttack Attack { get; private set; }

	public EntityExecution EntExecution { get; private set; }

	public DamageEffectScript DamageEffect { get; private set; }

	public WheelCarrierAudio Audio { get; private set; }

	public VulnerablePeriodTrait VulnerablePeriod { get; private set; }

	public EntityMotionChecker MotionChecker { get; private set; }

	public VisionCone VisionCone { get; private set; }

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		Behaviour = GetComponent<WheelCarrierBehaviour>();
		Input = GetComponent<NPCInputs>();
		base.Controller = GetComponent<PlatformCharacterController>();
		Collider = GetComponent<SmartPlatformCollider>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		Attack = GetComponentInChildren<EnemyAttack>();
		AnimatorInyector = GetComponentInChildren<WheelCarrierAnimatorInyector>();
		EntExecution = GetComponent<EntityExecution>();
		DamageEffect = GetComponentInChildren<DamageEffectScript>();
		Audio = GetComponentInChildren<WheelCarrierAudio>();
		VulnerablePeriod = GetComponentInChildren<VulnerablePeriodTrait>();
		MotionChecker = GetComponentInChildren<EntityMotionChecker>();
		VisionCone = GetComponentInChildren<VisionCone>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		base.Target = Core.Logic.Penitent.gameObject;
		Status.CastShadow = true;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (base.Target == null)
		{
			base.Target = Core.Logic.Penitent.gameObject;
		}
		Status.IsGrounded = base.Controller.IsGrounded;
		if (!base.Landing)
		{
			base.Landing = true;
			SetPositionAtStart();
			base.Controller.PlatformCharacterPhysics.GravityScale = 1f;
		}
	}

	public override void SetPositionAtStart()
	{
		base.SetPositionAtStart();
		float groundDist = base.Controller.GroundDist;
		Vector3 position = new Vector3(base.transform.position.x, base.transform.position.y - groundDist, base.transform.position.z);
		base.transform.position = position;
		base.Controller.PlatformCharacterPhysics.GravityScale = 0f;
	}

	public void Damage(Hit hit)
	{
		if (Execution(hit))
		{
			GetStun(hit);
			return;
		}
		DamageArea.TakeDamage(hit);
		if (Status.Dead)
		{
			DamageArea.DamageAreaCollider.enabled = false;
			Behaviour.Death();
			Audio.PlayDeath();
			return;
		}
		if (Behaviour.GotParry)
		{
			Behaviour.GotParry = false;
		}
		DamageEffect.Blink(0f, 0.1f);
		SleepTimeByHit(hit);
	}

	public override void Parry()
	{
		base.Parry();
		Behaviour.Parry();
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
