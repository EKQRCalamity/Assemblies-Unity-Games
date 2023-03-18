using System;
using CreativeSpore.SmartColliders;
using DamageEffect;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.PontiffSword.Animator;
using Gameplay.GameControllers.Bosses.PontiffSword.Audio;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.PontiffSword;

public class PontiffSword : Enemy, IDamageable
{
	public AnimationCurve slowTimeCurve;

	public DamageEffectScript damageEffectScript;

	public PontiffSwordAnimatorInyector animatorInyector;

	public PontiffSwordBehaviour Behaviour { get; private set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public NPCInputs Input { get; set; }

	public PontiffSwordAudio Audio { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		base.Controller = GetComponent<PlatformCharacterController>();
		animatorInyector = GetComponentInChildren<PontiffSwordAnimatorInyector>();
		Behaviour = GetComponent<PontiffSwordBehaviour>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		Audio = GetComponentInChildren<PontiffSwordAudio>();
		Input = GetComponentInChildren<NPCInputs>();
		DamageArea.DamageAreaCollider.enabled = false;
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
		damageEffectScript.Blink(0f, 0.07f);
	}

	public void Damage(Hit hit)
	{
		if (Status.Dead || Core.Logic.Penitent.Stats.Life.Current <= 0f)
		{
			return;
		}
		DamageFlash();
		if (!GuardHit(hit))
		{
			DamageArea.TakeDamage(hit);
			if (Status.Dead)
			{
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

	public override void Revive()
	{
		base.Revive();
		Behaviour.Revive();
		DamageArea.DamageAreaCollider.enabled = true;
	}

	public Vector3 GetPosition()
	{
		throw new NotImplementedException();
	}
}
