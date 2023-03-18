using System;
using CreativeSpore.SmartColliders;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Enemies.GoldenCorpse.AI;
using Gameplay.GameControllers.Enemies.GoldenCorpse.Animator;
using Gameplay.GameControllers.Enemies.GoldenCorpse.Audio;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.GoldenCorpse;

public class GoldenCorpse : Enemy, IDamageable
{
	public NPCInputs Input { get; private set; }

	public GoldenCorpseAnimatorInyector AnimatorInyector { get; private set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public GoldenCorpseBehaviour Behaviour { get; private set; }

	public EntityMotionChecker MotionChecker { get; private set; }

	public Vector2 StartPosition { get; private set; }

	public GoldenCorpseAudio Audio { get; private set; }

	public bool IsSummoned { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		Input = GetComponent<NPCInputs>();
		base.Controller = GetComponent<PlatformCharacterController>();
		AnimatorInyector = GetComponentInChildren<GoldenCorpseAnimatorInyector>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		Behaviour = GetComponentInChildren<GoldenCorpseBehaviour>();
		enemyAttack = GetComponentInChildren<EnemyAttack>();
		MotionChecker = GetComponentInChildren<EntityMotionChecker>();
		Audio = GetComponentInChildren<GoldenCorpseAudio>();
	}

	protected override void OnStart()
	{
		base.OnStart();
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
		Vector3 position = base.transform.position;
		float groundDist = base.Controller.GroundDist;
		Vector3 position2 = new Vector3(position.x, position.y - groundDist, position.z);
		base.transform.position = position2;
		base.Controller.PlatformCharacterPhysics.GravityScale = 0f;
	}

	public void Damage(Hit hit)
	{
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
