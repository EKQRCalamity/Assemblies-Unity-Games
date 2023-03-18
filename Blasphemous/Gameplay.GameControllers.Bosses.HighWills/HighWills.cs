using System;
using DG.Tweening;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.HighWills;

public class HighWills : Enemy, IDamageable
{
	public SpriteRenderer LeftHWEyes;

	public SpriteRenderer MiddleHWEyes;

	public SpriteRenderer RightHWEyes;

	public HighWillsBehaviour Behaviour { get; private set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public VisionCone VisionCone { get; private set; }

	public MasterShaderEffects DamageEffect { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		Behaviour = GetComponent<HighWillsBehaviour>();
		VisionCone = GetComponentInChildren<VisionCone>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		DamageEffect = GetComponentInChildren<MasterShaderEffects>();
	}

	public void ActivateLeftHWEyes(float activationTime, float activatedTime, float deactivationTime)
	{
		StartEyesActivationSequence(LeftHWEyes, activationTime, activatedTime, deactivationTime);
	}

	public void ActivateMiddleHWEyes(float activationTime, float activatedTime, float deactivationTime)
	{
		StartEyesActivationSequence(MiddleHWEyes, activationTime, activatedTime, deactivationTime);
	}

	public void ActivateRightHWEyes(float activationTime, float activatedTime, float deactivationTime)
	{
		StartEyesActivationSequence(RightHWEyes, activationTime, activatedTime, deactivationTime);
	}

	private void StartEyesActivationSequence(SpriteRenderer eyes, float activationTime, float activatedTime, float deactivationTime)
	{
		Sequence s = DOTween.Sequence();
		s.Append(eyes.DOFade(1f, activationTime)).AppendInterval(activatedTime).Append(eyes.DOFade(0f, deactivationTime));
	}

	public void DamageFlash()
	{
		DamageEffect.DamageEffectBlink(0f, 0.07f);
	}

	public void Damage(Hit hit)
	{
		if (WillDieByHit(hit))
		{
		}
		DamageArea.TakeDamage(hit);
		if (Status.Dead)
		{
			Behaviour.LaunchDeathAction();
			return;
		}
		DamageFlash();
		SleepTimeByHit(hit);
	}

	public void DamageByCrisanta()
	{
		Hit hit = default(Hit);
		hit.DamageAmount = 20f;
		Hit hit2 = hit;
		Damage(hit2);
	}

	public override EnemyAttack EnemyAttack()
	{
		throw new NotImplementedException();
	}

	public override EnemyBumper EnemyBumper()
	{
		throw new NotImplementedException();
	}

	public override EnemyFloorChecker EnemyFloorChecker()
	{
		throw new NotImplementedException();
	}

	public Vector3 GetPosition()
	{
		throw new NotImplementedException();
	}

	protected override void EnablePhysics(bool enable = true)
	{
		throw new NotImplementedException();
	}

	public float GetHpPercentage()
	{
		return Stats.Life.Current / Stats.Life.CurrentMax;
	}
}
