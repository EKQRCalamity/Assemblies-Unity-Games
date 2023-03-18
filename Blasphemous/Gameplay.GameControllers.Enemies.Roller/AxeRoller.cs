using System;
using CreativeSpore.SmartColliders;
using DamageEffect;
using Framework.Managers;
using Gameplay.GameControllers.Combat;
using Gameplay.GameControllers.Effects.Player.GhostTrail;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Enemies.Roller.AI;
using Gameplay.GameControllers.Enemies.Roller.Animator;
using Gameplay.GameControllers.Enemies.Roller.Attack;
using Gameplay.GameControllers.Enemies.Roller.Audio;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Roller;

public class AxeRoller : Enemy, IDamageable
{
	public EntityMotionChecker MotionChecker { get; private set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public NPCInputs Input { get; private set; }

	public AxeRollerAnimatorInjector AnimatorInjector { get; set; }

	public AxeRollerAttack AxeAttack { get; private set; }

	public AxeRollerMeleeAttack RollingAttack { get; private set; }

	public BoxCollider2D DamageCollider { get; private set; }

	public DamageEffectScript DamageEffect { get; private set; }

	public AxeRollerAudio Audio { get; private set; }

	public EntityExecution EntExecution { get; private set; }

	public VisionCone VisionCone { get; private set; }

	public GhostTrailGenerator GhostTrailGenerator { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		base.Controller = GetComponent<PlatformCharacterController>();
		MotionChecker = GetComponent<EntityMotionChecker>();
		base.EnemyBehaviour = GetComponent<AxeRollerBehaviour>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		Input = GetComponent<NPCInputs>();
		AnimatorInjector = GetComponentInChildren<AxeRollerAnimatorInjector>();
		AxeAttack = GetComponentInChildren<AxeRollerAttack>();
		RollingAttack = GetComponentInChildren<AxeRollerMeleeAttack>();
		DamageCollider = GetComponent<BoxCollider2D>();
		DamageEffect = GetComponentInChildren<DamageEffectScript>();
		Audio = GetComponentInChildren<AxeRollerAudio>();
		EntExecution = GetComponentInChildren<EntityExecution>();
		VisionCone = GetComponentInChildren<VisionCone>();
		GhostTrailGenerator = GetComponentInChildren<GhostTrailGenerator>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		SpawnManager.OnPlayerSpawn += OnPlayerSpawn;
		Status.CastShadow = true;
	}

	private void OnPlayerSpawn(Gameplay.GameControllers.Penitent.Penitent penitent)
	{
		base.Target = Core.Logic.Penitent.gameObject;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (base.Landing && base.Controller.PlatformCharacterPhysics.GravityScale <= 0f)
		{
			base.Controller.PlatformCharacterPhysics.GravityScale = 3f;
		}
		if (!base.Landing)
		{
			base.Landing = true;
			SetPositionAtStart();
		}
		Status.IsGrounded = base.Controller.IsGrounded;
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
			DamageCollider.enabled = false;
			AnimatorInjector.Death();
			return;
		}
		AxeRollerBehaviour axeRollerBehaviour = (AxeRollerBehaviour)base.EnemyBehaviour;
		DamageEffect.Blink(0f, 0.1f);
		axeRollerBehaviour.HitDisplacement(hit.AttackingEntity.transform.position);
		axeRollerBehaviour.RollIfCantSeePenitent();
	}

	public override void GetStun(Hit hit)
	{
		base.GetStun(hit);
		if (!base.IsStunt && Mathf.Abs(base.Controller.SlopeAngle) < 1f)
		{
			Core.Audio.EventOneShotPanned(hit.HitSoundId, base.transform.position);
			base.EnemyBehaviour.Execution();
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

	private new void OnDestroy()
	{
		SpawnManager.OnPlayerSpawn -= OnPlayerSpawn;
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
