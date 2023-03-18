using System;
using CreativeSpore.SmartColliders;
using DamageEffect;
using Framework.Managers;
using Gameplay.GameControllers.Combat;
using Gameplay.GameControllers.Enemies.CrossCrawler.Animator;
using Gameplay.GameControllers.Enemies.CrossCrawler.Audio;
using Gameplay.GameControllers.Enemies.CrossCrawler.IA;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Traits;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.CrossCrawler;

public class CrossCrawler : Enemy, IDamageable
{
	public CrossCrawlerBehaviour Behaviour { get; set; }

	public NPCInputs Input { get; set; }

	public SmartPlatformCollider Collider { get; set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public CrossCrawlerAnimatorInyector AnimatorInyector { get; set; }

	public EnemyAttack Attack { get; set; }

	public EntityExecution EntExecution { get; set; }

	public DamageEffectScript DamageEffect { get; set; }

	public CrossCrawlerAudio Audio { get; set; }

	public VulnerablePeriodTrait VulnerablePeriod { get; set; }

	public EntityMotionChecker MotionChecker { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		Behaviour = GetComponent<CrossCrawlerBehaviour>();
		Input = GetComponent<NPCInputs>();
		base.Controller = GetComponent<PlatformCharacterController>();
		Collider = GetComponent<SmartPlatformCollider>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		Attack = GetComponentInChildren<EnemyAttack>();
		AnimatorInyector = GetComponentInChildren<CrossCrawlerAnimatorInyector>();
		EntExecution = GetComponent<EntityExecution>();
		DamageEffect = GetComponentInChildren<DamageEffectScript>();
		Audio = GetComponentInChildren<CrossCrawlerAudio>();
		VulnerablePeriod = GetComponentInChildren<VulnerablePeriodTrait>();
		MotionChecker = GetComponentInChildren<EntityMotionChecker>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		base.Target = Core.Logic.Penitent.gameObject;
		Status.CastShadow = true;
		Status.IsGrounded = true;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (base.Target == null)
		{
			base.Target = Core.Logic.Penitent.gameObject;
		}
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
		}
		else
		{
			DamageEffect.Blink(0f, 0.1f);
			SleepTimeByHit(hit);
		}
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
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
