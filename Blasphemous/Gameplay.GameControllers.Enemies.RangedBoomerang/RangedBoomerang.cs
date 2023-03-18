using System;
using CreativeSpore.SmartColliders;
using Framework.Managers;
using Gameplay.GameControllers.Combat;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Enemies.RangedBoomerang.Animator;
using Gameplay.GameControllers.Enemies.RangedBoomerang.Audio;
using Gameplay.GameControllers.Enemies.RangedBoomerang.IA;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.RangedBoomerang;

public class RangedBoomerang : Enemy, IDamageable
{
	public RangedBoomerangBehaviour Behaviour { get; private set; }

	public NPCInputs Input { get; private set; }

	public SmartPlatformCollider Collider { get; private set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public RangedBoomerangAnimatorInyector AnimatorInyector { get; private set; }

	public RangedBoomerangAudio Audio { get; private set; }

	public EnemyAttack Attack { get; private set; }

	public ColorFlash ColorFlash { get; private set; }

	public VisionCone VisionCone { get; private set; }

	public EntityExecution EntExecution { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		Behaviour = GetComponentInChildren<RangedBoomerangBehaviour>();
		Input = GetComponent<NPCInputs>();
		base.Controller = GetComponent<PlatformCharacterController>();
		Collider = GetComponent<SmartPlatformCollider>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		Attack = GetComponentInChildren<EnemyAttack>();
		AnimatorInyector = GetComponentInChildren<RangedBoomerangAnimatorInyector>();
		Audio = GetComponentInChildren<RangedBoomerangAudio>();
		ColorFlash = GetComponentInChildren<ColorFlash>();
		VisionCone = GetComponentInChildren<VisionCone>();
		Behaviour.enabled = false;
		EntExecution = GetComponentInChildren<EntityExecution>();
		SpawnManager.OnPlayerSpawn += OnPlayerSpawn;
	}

	private void OnPlayerSpawn(Gameplay.GameControllers.Penitent.Penitent penitent)
	{
		SpawnManager.OnPlayerSpawn -= OnPlayerSpawn;
		base.Target = penitent.gameObject;
	}

	protected override void OnStart()
	{
		base.OnStart();
		Status.CastShadow = true;
		Behaviour.enabled = true;
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
		ColorFlash.TriggerColorFlash();
		if (Status.Dead)
		{
			DamageArea.DamageAreaCollider.enabled = false;
			Behaviour.Death();
		}
		else
		{
			SleepTimeByHit(hit);
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
