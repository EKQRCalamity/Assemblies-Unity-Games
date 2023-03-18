using System;
using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.PontiffHusk.Audio;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.PontiffHusk;

public class PontiffHuskBoss : Enemy, IDamageable
{
	public AnimationCurve slowTimeCurve;

	[SerializeField]
	[BoxGroup("Audio", true, false, 0)]
	[EventRef]
	private string finalHit = "event:/SFX/Penitent/Damage/PenitentBossDeathHit";

	public PontiffHuskBossBehaviour Behaviour { get; private set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public MasterShaderEffects DamageEffect { get; private set; }

	public PontiffHuskBossAudio Audio { get; private set; }

	public PontiffHuskBossAnimatorInyector AnimatorInyector { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		Audio = GetComponentInChildren<PontiffHuskBossAudio>();
		Behaviour = GetComponent<PontiffHuskBossBehaviour>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		DamageEffect = GetComponentInChildren<MasterShaderEffects>();
		AnimatorInyector = GetComponentInChildren<PontiffHuskBossAnimatorInyector>();
	}

	public void DamageFlash()
	{
		DamageEffect.DamageEffectBlink(0f, 0.07f);
	}

	public void Damage(Hit hit)
	{
		if (!GuardHit(hit))
		{
			if (WillDieByHit(hit))
			{
				hit.HitSoundId = finalHit;
			}
			if (!DamageArea.enabled)
			{
				DamageArea.enabled = true;
				DamageArea.ClearAccumDamage();
			}
			Behaviour.Damage(hit);
			DamageArea.TakeDamage(hit);
			if (Status.Dead)
			{
				Core.Logic.ScreenFreeze.Freeze(0.05f, 4f, 0f, slowTimeCurve);
				Core.Logic.CameraManager.ScreenEffectsManager.ScreenModeBW(0.5f);
				DamageArea.DamageAreaCollider.enabled = false;
				Behaviour.Death();
			}
			else
			{
				DamageFlash();
				SleepTimeByHit(hit);
			}
		}
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
