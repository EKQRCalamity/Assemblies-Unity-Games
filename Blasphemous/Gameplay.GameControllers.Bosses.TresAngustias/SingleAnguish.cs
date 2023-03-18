using System;
using BezierSplines;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Enemies.MasterAnguish.Audio;
using Gameplay.GameControllers.Enemies.SingleAnguish.Animator;
using Gameplay.GameControllers.Entities;
using Maikel.SteeringBehaviors;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.TresAngustias;

public class SingleAnguish : Enemy, IDamageable
{
	public MasterShaderEffects shaderEffects;

	public SingleAnguishBehaviour Behaviour { get; private set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public SingleAnguishAnimatorInyector AnimatorInyector { get; private set; }

	public AutonomousAgent autonomousAgent { get; private set; }

	public SingleAnguishAudio Audio { get; private set; }

	public Arrive arriveBehaviour { get; private set; }

	public event Action<SingleAnguish, Hit> OnSingleAnguishDamaged;

	protected override void OnAwake()
	{
		base.OnAwake();
		Behaviour = GetComponent<SingleAnguishBehaviour>();
		Audio = GetComponentInChildren<SingleAnguishAudio>();
		arriveBehaviour = GetComponent<Arrive>();
		autonomousAgent = GetComponent<AutonomousAgent>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		AnimatorInyector = GetComponentInChildren<SingleAnguishAnimatorInyector>();
		shaderEffects = GetComponentInChildren<MasterShaderEffects>();
	}

	protected override void OnFixedUpdated()
	{
		base.OnFixedUpdated();
		SetSortingLayerFromY();
	}

	private void SetSortingLayerFromY()
	{
		base.SpriteRenderer.sortingOrder = -(int)base.transform.position.y;
	}

	public void SetPath(BezierSpline s)
	{
		Behaviour = GetComponent<SingleAnguishBehaviour>();
		Behaviour.SetPath(s);
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
		if (this.OnSingleAnguishDamaged != null)
		{
			this.OnSingleAnguishDamaged(this, hit);
		}
		shaderEffects.DamageEffectBlink(0f, 0.2f);
		SleepTimeByHit(hit);
	}

	public override void Kill()
	{
		base.Kill();
		Debug.Log("KILL OVERRIDE");
		DamageArea.TakeDamage(new Hit
		{
			DamageAmount = 9999f
		});
		if (Status.Dead)
		{
			Behaviour.Death();
		}
		else
		{
			Behaviour.Damage();
		}
	}

	public Vector3 GetPosition()
	{
		throw new NotImplementedException();
	}
}
