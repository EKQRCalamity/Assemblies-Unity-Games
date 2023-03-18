using System;
using CreativeSpore.SmartColliders;
using Gameplay.GameControllers.Enemies.Fool.AI;
using Gameplay.GameControllers.Enemies.Fool.Animator;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Fool;

public class Fool : Enemy, IDamageable
{
	public NPCInputs Input { get; private set; }

	public FoolAnimatorInyector AnimatorInyector { get; private set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public FoolBehaviour Behaviour { get; private set; }

	public EntityMotionChecker MotionChecker { get; private set; }

	public Vector2 StartPosition { get; private set; }

	public bool IsSummoned { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		Input = GetComponent<NPCInputs>();
		base.Controller = GetComponent<PlatformCharacterController>();
		AnimatorInyector = GetComponentInChildren<FoolAnimatorInyector>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		Behaviour = GetComponentInChildren<FoolBehaviour>();
		enemyAttack = GetComponentInChildren<EnemyAttack>();
		MotionChecker = GetComponentInChildren<EntityMotionChecker>();
		Status.CastShadow = true;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (base.Target == null)
		{
			base.Target = Behaviour.GetTarget().gameObject;
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
		base.Controller.PlatformCharacterPhysics.GravityScale = 0f;
	}

	public void Damage(Hit hit)
	{
		DamageArea.TakeDamage(hit);
		if (Status.Dead)
		{
			Behaviour.Death();
		}
		else
		{
			Behaviour.Damage();
		}
		SleepTimeByHit(hit);
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	public void ReuseObject()
	{
		IsSummoned = true;
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
