using System;
using CreativeSpore.SmartColliders;
using Framework.Managers;
using Gameplay.GameControllers.Combat;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Enemies.Runner.AI;
using Gameplay.GameControllers.Enemies.Runner.Animator;
using Gameplay.GameControllers.Enemies.Runner.Attack;
using Gameplay.GameControllers.Enemies.Runner.Audio;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.StateMachine;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Runner;

public class Runner : Enemy, IDamageable
{
	public RunnerAttack Attack { get; private set; }

	public RunnerBehaviour Behaviour { get; private set; }

	public EntityMotionChecker MotionChecker { get; private set; }

	public RunnerAudio Audio { get; private set; }

	public RunnerAnimatorInjector AnimatorInjector { get; private set; }

	public VisionCone VisionCone { get; private set; }

	public StateMachine StateMachine { get; private set; }

	public NPCInputs Input { get; private set; }

	public ContactDamage ContactDamage { get; private set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public ColorFlash ColorFlash { get; private set; }

	public EntityExecution EntExecution { get; private set; }

	private void OnEnable()
	{
		base.Landing = false;
		if ((bool)DamageArea)
		{
			DamageArea.DamageAreaCollider.enabled = true;
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		Attack = GetComponentInChildren<RunnerAttack>();
		base.Controller = GetComponentInChildren<PlatformCharacterController>();
		Behaviour = GetComponentInChildren<RunnerBehaviour>();
		MotionChecker = GetComponentInChildren<EntityMotionChecker>();
		Audio = GetComponentInChildren<RunnerAudio>();
		AnimatorInjector = GetComponentInChildren<RunnerAnimatorInjector>();
		VisionCone = GetComponentInChildren<VisionCone>();
		StateMachine = GetComponentInChildren<StateMachine>();
		ContactDamage = GetComponentInChildren<ContactDamage>();
		Input = GetComponent<NPCInputs>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		ColorFlash = GetComponentInChildren<ColorFlash>();
		SpawnManager.OnPlayerSpawn += OnPlayerSpawn;
		EntExecution = GetComponentInChildren<EntityExecution>();
	}

	private void OnPlayerSpawn(Gameplay.GameControllers.Penitent.Penitent penitent)
	{
		base.Target = penitent.gameObject;
		SpawnManager.OnPlayerSpawn -= OnPlayerSpawn;
	}

	protected override void OnStart()
	{
		base.OnStart();
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
		if (Execution(hit))
		{
			GetStun(hit);
			return;
		}
		DamageArea.TakeDamage(hit);
		if (Status.Dead)
		{
			AnimatorInjector.Death();
		}
		else
		{
			ColorFlash.TriggerColorFlash();
		}
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
