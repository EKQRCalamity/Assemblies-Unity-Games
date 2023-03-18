using System;
using CreativeSpore.SmartColliders;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.Quirce.Attack;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Enemies.CowardTrapper.AI;
using Gameplay.GameControllers.Enemies.CowardTrapper.Animator;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.StateMachine;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.CowardTrapper;

public class CowardTrapper : Enemy, IDamageable
{
	public CowardTrapperBehaviour Behaviour { get; private set; }

	public NPCInputs Input { get; private set; }

	public CowardTrapperAnimatorInjector AnimatorInjector { get; private set; }

	public BossAreaSummonAttack Attack { get; private set; }

	public ColorFlash ColorFlash { get; private set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public StateMachine StateMachine { get; private set; }

	public EntityMotionChecker MotionChecker { get; private set; }

	public VisionCone VisionCone { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		base.Controller = GetComponent<PlatformCharacterController>();
		Behaviour = GetComponent<CowardTrapperBehaviour>();
		Input = GetComponent<NPCInputs>();
		AnimatorInjector = GetComponentInChildren<CowardTrapperAnimatorInjector>();
		Attack = GetComponentInChildren<BossAreaSummonAttack>();
		ColorFlash = GetComponentInChildren<ColorFlash>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		StateMachine = GetComponent<StateMachine>();
		MotionChecker = GetComponentInChildren<EntityMotionChecker>();
		VisionCone = GetComponentInChildren<VisionCone>();
		Behaviour.enabled = false;
	}

	protected override void OnStart()
	{
		base.OnStart();
		Status.CastShadow = true;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (base.Target != null)
		{
			Behaviour.enabled = true;
			base.DistanceToTarget = Vector2.Distance(base.transform.position, base.Target.transform.position);
		}
		else
		{
			base.Target = Core.Logic.Penitent.gameObject;
		}
		Status.IsGrounded = base.Controller.IsGrounded;
		if (!base.Landing)
		{
			SetPositionAtStart();
			base.Landing = true;
		}
	}

	public void Damage(Hit hit)
	{
		DamageArea.TakeDamage(hit);
		if (Status.Dead)
		{
			AnimatorInjector.Death();
			DamageArea.DamageAreaCollider.enabled = false;
		}
		else
		{
			AnimatorInjector.Hurt();
			ColorFlash.TriggerColorFlash();
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("SpikeTrap"))
		{
			Kill();
			AnimatorInjector.Death();
		}
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
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
}
