using System;
using CreativeSpore.SmartColliders;
using DamageEffect;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Combat;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Enemies.Menina.Animator;
using Gameplay.GameControllers.Enemies.Menina.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.StateMachine;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Menina;

public class Menina : Enemy, IDamageable
{
	public NPCInputs Inputs { get; private set; }

	public MeninaAnimatorInyector AnimatorInyector { get; private set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public MeninaAttack Attack { get; private set; }

	public SmartCollider2D SmartCollider { get; private set; }

	public DamageEffectScript DamageEffect { get; private set; }

	public MeninaAudio Audio { get; private set; }

	public Vector2 StartPosition { get; private set; }

	public StateMachine StateMachine { get; private set; }

	public EntityMotionChecker MotionChecker { get; private set; }

	public EntityExecution EntExecution { get; private set; }

	private void OnEnable()
	{
		if (base.Landing)
		{
			base.Landing = !base.Landing;
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		base.Controller = GetComponent<PlatformCharacterController>();
		base.EnemyBehaviour = GetComponent<EnemyBehaviour>();
		AnimatorInyector = GetComponentInChildren<MeninaAnimatorInyector>();
		Inputs = GetComponent<NPCInputs>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		Attack = GetComponentInChildren<MeninaAttack>();
		SmartCollider = GetComponent<SmartCollider2D>();
		DamageEffect = GetComponentInChildren<DamageEffectScript>();
		Audio = GetComponentInChildren<MeninaAudio>();
		StateMachine = GetComponent<StateMachine>();
		MotionChecker = GetComponentInChildren<EntityMotionChecker>();
		EntExecution = GetComponentInChildren<EntityExecution>();
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
			if (!(Core.Logic.Penitent != null))
			{
				return;
			}
			base.Target = Core.Logic.Penitent.gameObject;
		}
		base.DistanceToTarget = Vector2.Distance(base.transform.position, base.Target.transform.position);
		Status.IsGrounded = base.Controller.IsGrounded;
		bool enable = base.DistanceToTarget <= ActivationRange;
		if (Status.Dead)
		{
			if (DamageArea.DamageAreaCollider.enabled)
			{
				DamageArea.DamageAreaCollider.enabled = false;
			}
		}
		else if (base.Landing)
		{
			EnablePhysics(enable);
		}
		if (!base.Landing)
		{
			base.Landing = true;
			SetPositionAtStart();
			base.Controller.PlatformCharacterPhysics.GravityScale = 0f;
			StartPosition = new Vector2(base.transform.position.x, base.transform.position.y);
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
		if (enable)
		{
			if (!base.Controller.enabled)
			{
				base.Controller.enabled = true;
				base.Controller.PlatformCharacterPhysics.GravityScale = 1f;
			}
			if (!Inputs.enabled)
			{
				Inputs.enabled = true;
			}
			if (!SmartCollider.EnableCollision2D)
			{
				SmartCollider.EnableCollision2D = true;
			}
		}
		else
		{
			if (base.Controller.enabled)
			{
				base.Controller.enabled = false;
				base.Controller.PlatformCharacterPhysics.GravityScale = 0f;
			}
			if (Inputs.enabled)
			{
				Inputs.enabled = false;
			}
			if (SmartCollider.EnableCollision2D)
			{
				SmartCollider.EnableCollision2D = false;
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

	public void SetOrientationAtStart(Vector3 targetPos)
	{
		SetOrientation((targetPos.x <= base.transform.position.x) ? EntityOrientation.Left : EntityOrientation.Right);
	}

	public void Damage(Hit hit)
	{
		if (!(DamageArea == null) && !Status.Dead)
		{
			if (IsExecutable && Execution(hit))
			{
				GetStun(hit);
				return;
			}
			DamageEffect.Blink(0f, 0.1f);
			DamageArea.TakeDamage(hit);
			SleepTimeByHit(hit);
			base.EnemyBehaviour.Damage();
		}
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
}
