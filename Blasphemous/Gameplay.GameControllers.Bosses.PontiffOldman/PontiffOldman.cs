using System;
using CreativeSpore.SmartColliders;
using DamageEffect;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.PontiffOldman.Animator;
using Gameplay.GameControllers.Bosses.PontiffOldman.Audio;
using Gameplay.GameControllers.Effects.Player.GhostTrail;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.PontiffOldman;

public class PontiffOldman : Enemy, IDamageable
{
	public AnimationCurve slowTimeCurve;

	public PontiffOldmanBehaviour Behaviour { get; private set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public NPCInputs Input { get; set; }

	public PontiffOldmanAnimatorInyector AnimatorInyector { get; private set; }

	public PontiffOldmanAudio Audio { get; private set; }

	public DamageEffectScript damageEffect { get; private set; }

	public GhostTrailGenerator GhostTrail { get; private set; }

	public EntityMotionChecker MotionChecker { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		base.Controller = GetComponent<PlatformCharacterController>();
		Behaviour = GetComponent<PontiffOldmanBehaviour>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		AnimatorInyector = GetComponentInChildren<PontiffOldmanAnimatorInyector>();
		Audio = GetComponentInChildren<PontiffOldmanAudio>();
		damageEffect = GetComponentInChildren<DamageEffectScript>();
		Input = GetComponentInChildren<NPCInputs>();
		GhostTrail = GetComponentInChildren<GhostTrailGenerator>();
		MotionChecker = GetComponentInChildren<EntityMotionChecker>();
	}

	protected override void OnStart()
	{
		base.OnStart();
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

	public void DamageFlash()
	{
		damageEffect.Blink(0f, 0.07f);
	}

	public void Damage(Hit hit)
	{
		if (Status.Dead || Core.Logic.Penitent.Stats.Life.Current <= 0f)
		{
			return;
		}
		Debug.Log("<color=blue> DAMAGE REACHED PONTIFF");
		DamageFlash();
		if (hit.Unparriable || !GuardHit(hit))
		{
			DamageArea.TakeDamage(hit);
			if (Status.Dead)
			{
				Core.Logic.ScreenFreeze.Freeze(0.1f, 2f, 0f, slowTimeCurve);
				DamageArea.DamageAreaCollider.enabled = false;
				Behaviour.Death();
			}
			else
			{
				Behaviour.Damage();
				SleepTimeByHit(hit);
			}
		}
	}

	public override void Parry()
	{
		base.Parry();
		Behaviour.Parry();
	}

	public Vector3 GetPosition()
	{
		throw new NotImplementedException();
	}
}
