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
using Gameplay.GameControllers.Enemies.NewFlagellant.AI;
using Gameplay.GameControllers.Enemies.NewFlagellant.Animator;
using Gameplay.GameControllers.Enemies.NewFlagellant.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.StateMachine;
using Plugins.GhostSprites2D.Scripts.GhostSprites;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.NewFlagellant;

public class NewFlagellant : Enemy, IDamageable
{
	public NewFlagellantAttack Attack;

	public NewFlagellantAttack FastAttack;

	[FoldoutGroup("Overlap fixer settings", 0)]
	public ContactFilter2D overlapFilter;

	[FoldoutGroup("Overlap fixer settings", 0)]
	public Collider2D[] contacts;

	public float MAX_SPEED = 3.5f;

	public float MIN_SPEED = 1.4f;

	public StateMachine StateMachine { get; private set; }

	public NPCInputs Input { get; private set; }

	public NewFlagellantBehaviour NewFlagellantBehaviour { get; private set; }

	public EntityDisplacement EntityDisplacement { get; set; }

	public NewFlagellantAnimatorInyector AnimatorInyector { get; private set; }

	public MotionLerper MotionLerper { get; set; }

	public EnemyDamageArea DamageArea { get; set; }

	public EntityMotionChecker MotionChecker { get; private set; }

	public VisionCone VisionCone { get; private set; }

	public MasterShaderEffects MasterShaderEffects { get; private set; }

	public bool IsTargetVisible { get; private set; }

	public EntityExecution EntExecution { get; private set; }

	public GhostSprites GhostSprites { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		StateMachine = GetComponent<StateMachine>();
		EntityDisplacement = GetComponent<EntityDisplacement>();
		Input = GetComponent<NPCInputs>();
		NewFlagellantBehaviour = GetComponent<NewFlagellantBehaviour>();
		AnimatorInyector = GetComponentInChildren<NewFlagellantAnimatorInyector>();
		MotionLerper = GetComponentInChildren<MotionLerper>();
		base.Controller = GetComponentInChildren<PlatformCharacterController>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		MotionChecker = GetComponentInChildren<EntityMotionChecker>();
		VisionCone = GetComponentInChildren<VisionCone>();
		MasterShaderEffects = GetComponentInChildren<MasterShaderEffects>();
		EntExecution = GetComponentInChildren<EntityExecution>();
		GhostSprites = GetComponentInChildren<GhostSprites>();
		contacts = new Collider2D[3];
	}

	protected override void OnStart()
	{
		base.OnStart();
		Status.CastShadow = true;
		MAX_SPEED += UnityEngine.Random.Range(-0.2f, 0.1f);
		MIN_SPEED += UnityEngine.Random.Range(-0.2f, 0.1f);
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		Status.IsGrounded = base.Controller.IsGrounded;
		if (base.Target != null)
		{
			base.DistanceToTarget = Vector2.Distance(base.transform.position, base.Target.transform.position);
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

	public void CheckOverlappingEnemies()
	{
		Collider2D damageAreaCollider = DamageArea.DamageAreaCollider;
		int num = damageAreaCollider.OverlapCollider(overlapFilter, contacts);
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
					NewFlagellantBehaviour.OnBouncedBackByOverlapping();
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
	}

	public void SetMovementSpeed(float newSpeed)
	{
		base.Controller.MaxWalkingSpeed = newSpeed;
	}

	public void Damage(Hit hit)
	{
		if (MotionLerper.IsLerping)
		{
			return;
		}
		DamageArea.TakeDamage(hit);
		if (Status.Dead)
		{
			AnimatorInyector.Death();
			return;
		}
		if (Execution(hit))
		{
			GetStun(hit);
			return;
		}
		if (NewFlagellantBehaviour.GotParry)
		{
			NewFlagellantBehaviour.GotParry = false;
			NewFlagellantBehaviour.ResetParryRecover();
		}
		NewFlagellantBehaviour.Damage();
		AnimatorInyector.IsParried(isParried: false);
		MasterShaderEffects.DamageEffectBlink(0f, 0.07f);
		SleepTimeByHit(hit);
	}

	private void KillByTrap()
	{
		if (!Status.Dead)
		{
			NewFlagellantBehaviour.StopBehaviour();
			Kill();
			AnimatorInyector.Death();
		}
	}

	public override void Parry()
	{
		base.Parry();
		NewFlagellantBehaviour.Parry();
	}

	public override void GetStun(Hit hit)
	{
		base.GetStun(hit);
		if (!base.IsStunt && Mathf.Abs(base.Controller.SlopeAngle) < 1f)
		{
			Core.Audio.EventOneShotPanned(hit.HitSoundId, base.transform.position);
			NewFlagellantBehaviour.Execution();
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (!base.Controller.IsGrounded && other.CompareTag("SpikeTrap"))
		{
			KillByTrap();
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
}
