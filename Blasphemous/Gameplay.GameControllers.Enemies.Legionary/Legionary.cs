using System;
using CreativeSpore.SmartColliders;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.EcclesiaBros.Esdras;
using Gameplay.GameControllers.Bosses.Quirce.Attack;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Enemies.Legionary.AI;
using Gameplay.GameControllers.Enemies.Legionary.Animator;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.StateMachine;
using Gameplay.GameControllers.Penitent;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Legionary;

public class Legionary : Enemy, IDamageable
{
	[FoldoutGroup("Attacks", 0)]
	public EsdrasMeleeAttack LightAttack;

	[FoldoutGroup("Attacks", 0)]
	public EsdrasMeleeAttack SpinAttack;

	[FoldoutGroup("Attacks", 0)]
	public BossAreaSummonAttack LightningSummonAttack;

	public LegionaryBehaviour Behaviour { get; private set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public VisionCone VisionCone { get; private set; }

	public EntityMotionChecker MotionChecker { get; private set; }

	public NPCInputs Inputs { get; private set; }

	public StateMachine StateMachine { get; private set; }

	public MotionLerper MotionLerper { get; private set; }

	public ColorFlash ColorFlash { get; private set; }

	public bool CanTakeDamage { get; set; }

	public LegionaryAnimator AnimatorInjector { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		base.Controller = GetComponentInChildren<PlatformCharacterController>();
		Behaviour = GetComponentInChildren<LegionaryBehaviour>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		VisionCone = GetComponentInChildren<VisionCone>();
		MotionChecker = GetComponentInChildren<EntityMotionChecker>();
		Inputs = GetComponentInChildren<NPCInputs>();
		StateMachine = GetComponentInChildren<StateMachine>();
		MotionLerper = GetComponentInChildren<MotionLerper>();
		ColorFlash = GetComponentInChildren<ColorFlash>();
		Behaviour.enabled = false;
		AnimatorInjector = GetComponentInChildren<LegionaryAnimator>();
		SpawnManager.OnPlayerSpawn += OnSpawn;
		EsdrasMeleeAttack spinAttack = SpinAttack;
		spinAttack.OnAttackGuarded = (Core.SimpleEvent)Delegate.Combine(spinAttack.OnAttackGuarded, new Core.SimpleEvent(OnAttackGuarded));
	}

	private void OnSpawn(Gameplay.GameControllers.Penitent.Penitent penitent)
	{
		SpawnManager.OnPlayerSpawn -= OnSpawn;
		base.Target = penitent.gameObject;
		Behaviour.enabled = true;
		Status.CastShadow = true;
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
		if (!GuardHit(hit) && CanTakeDamage)
		{
			SleepTimeByHit(hit);
			Behaviour.Damage();
			DamageArea.TakeDamage(hit);
			ColorFlash.TriggerColorFlash();
			if (Behaviour.GotParry)
			{
				CanTakeDamage = false;
				Behaviour.GotParry = false;
			}
			if (Status.Dead)
			{
				AnimatorInjector.Death();
			}
		}
	}

	private void OnAttackGuarded()
	{
		MotionLerper.StopLerping();
		MotionLerper.distanceToMove = 1f;
		MotionLerper.TimeTakenDuringLerp = 1f;
		Vector3 dir = ((Status.Orientation != 0) ? (-base.transform.right) : base.transform.right);
		MotionLerper.StartLerping(dir);
	}

	public override void Parry()
	{
		base.Parry();
		Behaviour.GotParry = true;
		Behaviour.Parry();
		base.IsGuarding = false;
		CanTakeDamage = true;
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		EsdrasMeleeAttack spinAttack = SpinAttack;
		spinAttack.OnAttackGuarded = (Core.SimpleEvent)Delegate.Remove(spinAttack.OnAttackGuarded, new Core.SimpleEvent(OnAttackGuarded));
	}
}
