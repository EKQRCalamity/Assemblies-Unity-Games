using System;
using CreativeSpore.SmartColliders;
using DamageEffect;
using Framework.Managers;
using Gameplay.GameControllers.Combat;
using Gameplay.GameControllers.Enemies.Bishop.AI;
using Gameplay.GameControllers.Enemies.Bishop.Animation;
using Gameplay.GameControllers.Enemies.Bishop.Attack;
using Gameplay.GameControllers.Enemies.Bishop.Audio;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Bishop;

public class Bishop : Enemy, IDamageable
{
	public BishopAnimatorInyector AnimatorInyector { get; private set; }

	public NPCInputs Inputs { get; private set; }

	public EntityMotionChecker MotionChecker { get; set; }

	public EnemyDamageArea DamageArea { get; set; }

	public BishopAttack Attack { get; private set; }

	public DamageEffectScript DamageEffect { get; private set; }

	public SmartCollider2D SmartCollider { get; private set; }

	public BishopAudio Audio { get; private set; }

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
		AnimatorInyector = GetComponentInChildren<BishopAnimatorInyector>();
		base.EnemyBehaviour = GetComponent<EnemyBehaviour>();
		Inputs = GetComponentInChildren<NPCInputs>();
		MotionChecker = GetComponentInChildren<EntityMotionChecker>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		Attack = GetComponentInChildren<BishopAttack>();
		DamageEffect = GetComponentInChildren<DamageEffectScript>();
		SmartCollider = GetComponent<SmartCollider2D>();
		Audio = GetComponentInChildren<BishopAudio>();
		EntExecution = GetComponentInChildren<EntityExecution>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		base.Target = Core.Logic.Penitent.gameObject;
	}

	protected override void OnUpdate()
	{
		if (base.Target == null)
		{
			base.Target = Core.Logic.Penitent.gameObject;
		}
		base.DistanceToTarget = Vector2.Distance(base.transform.position, base.Target.transform.position);
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
				base.EnemyBehaviour.StartBehaviour();
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
				base.EnemyBehaviour.PauseBehaviour();
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
			AnimatorInyector.Death();
			return;
		}
		BishopBehaviour bishopBehaviour = (BishopBehaviour)base.EnemyBehaviour;
		bishopBehaviour.Damage();
		DamageEffect.Blink(0f, 0.1f);
		bishopBehaviour.HitDisplacement(hit.AttackingEntity.transform.position);
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
