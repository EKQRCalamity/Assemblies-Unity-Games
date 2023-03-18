using System;
using CreativeSpore.SmartColliders;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Combat;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Enemies.ShieldMaiden.Animator;
using Gameplay.GameControllers.Enemies.ShieldMaiden.Audio;
using Gameplay.GameControllers.Enemies.ShieldMaiden.IA;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.ShieldMaiden;

public class ShieldMaiden : Enemy, IDamageable
{
	[FoldoutGroup("Overlap fixer settings", 0)]
	public ContactFilter2D filter;

	public Collider2D[] contacts;

	public ShieldMaidenBehaviour Behaviour { get; set; }

	public NPCInputs Input { get; set; }

	public SmartPlatformCollider Collider { get; set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public ShieldMaidenAnimatorInyector AnimatorInyector { get; set; }

	public EnemyAttack Attack { get; set; }

	public ColorFlash ColorFlash { get; set; }

	public EntityExecution EntExecution { get; set; }

	public VisionCone VisionCone { get; set; }

	public ShieldMaidenAudio Audio { get; private set; }

	public MotionLerper MotionLerper { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		Behaviour = GetComponent<ShieldMaidenBehaviour>();
		Input = GetComponent<NPCInputs>();
		base.Controller = GetComponent<PlatformCharacterController>();
		Collider = GetComponent<SmartPlatformCollider>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		Attack = GetComponentInChildren<EnemyAttack>();
		AnimatorInyector = GetComponentInChildren<ShieldMaidenAnimatorInyector>();
		ColorFlash = GetComponentInChildren<ColorFlash>();
		EntExecution = GetComponentInChildren<EntityExecution>();
		Audio = GetComponentInChildren<ShieldMaidenAudio>();
		VisionCone = GetComponentInChildren<VisionCone>();
		MotionLerper = GetComponentInChildren<MotionLerper>();
		contacts = new Collider2D[10];
	}

	protected override void OnStart()
	{
		base.OnStart();
		Status.CastShadow = true;
		Status.IsGrounded = true;
		SpawnManager.OnPlayerSpawn += OnPlayerSpawn;
	}

	private void OnPlayerSpawn(Gameplay.GameControllers.Penitent.Penitent penitent)
	{
		base.Target = Core.Logic.Penitent.gameObject;
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

	private void LateUpdate()
	{
		CheckOverlappingEnemies();
	}

	public bool IsOverlappingOtherEnemies()
	{
		Collider2D damageAreaCollider = DamageArea.DamageAreaCollider;
		int num = damageAreaCollider.OverlapCollider(filter, contacts);
		if (num > 0)
		{
			for (int i = 0; i < num; i++)
			{
				Enemy componentInChildren = contacts[i].GetComponentInChildren<Enemy>();
				if (componentInChildren != null)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void CheckOverlappingEnemies()
	{
		Collider2D damageAreaCollider = DamageArea.DamageAreaCollider;
		int num = damageAreaCollider.OverlapCollider(filter, contacts);
		if (num <= 0)
		{
			return;
		}
		for (int i = 0; i < num; i++)
		{
			if (MotionLerper.IsLerping)
			{
				break;
			}
			Enemy componentInChildren = contacts[i].GetComponentInChildren<Enemy>();
			if (componentInChildren != null)
			{
				Vector2 vector = new Vector2((componentInChildren.transform.position.x < base.transform.position.x) ? 1 : (-1), 0f);
				if ((vector.x == 1f && Status.Orientation == EntityOrientation.Left) || (vector.x == -1f && Status.Orientation == EntityOrientation.Right))
				{
					Behaviour.OnBouncedBackByOverlapping();
				}
				MotionLerper.StartLerping(vector);
			}
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

	public override void Parry()
	{
		base.Parry();
		Behaviour.OnParry();
	}

	public void Damage(Hit hit)
	{
		if (GuardHit(hit))
		{
			SleepTimeByHit(hit);
			Audio.PlayHitShield();
			Behaviour.OnShieldHit();
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
			DamageArea.DamageAreaCollider.enabled = false;
			Behaviour.Death();
		}
		Behaviour.Damage();
		ColorFlash.TriggerColorFlash();
		SleepTimeByHit(hit);
	}

	public override void GetStun(Hit hit)
	{
		base.GetStun(hit);
		if (!base.IsStunt && Mathf.Abs(base.Controller.SlopeAngle) < 1f)
		{
			AnimatorInyector.StopAll();
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

	private new void OnDestroy()
	{
		SpawnManager.OnPlayerSpawn -= OnPlayerSpawn;
	}
}
