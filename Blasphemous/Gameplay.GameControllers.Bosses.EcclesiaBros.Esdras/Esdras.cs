using System;
using CreativeSpore.SmartColliders;
using DamageEffect;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.EcclesiaBros.Esdras.Animator;
using Gameplay.GameControllers.Bosses.EcclesiaBros.Esdras.Audio;
using Gameplay.GameControllers.Effects.Player.GhostTrail;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.EcclesiaBros.Esdras;

public class Esdras : Enemy, IDamageable
{
	public AnimationCurve slowTimeCurve;

	[SerializeField]
	[BoxGroup("Audio", true, false, 0)]
	[EventRef]
	private string finalHit;

	public bool lookLeftOnStart = true;

	public EsdrasBehaviour Behaviour { get; private set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public NPCInputs Input { get; set; }

	public EsdrasAnimatorInyector AnimatorInyector { get; private set; }

	public EsdrasAudio Audio { get; private set; }

	public DamageEffectScript damageEffect { get; private set; }

	public GhostTrailGenerator GhostTrail { get; private set; }

	public EntityMotionChecker MotionChecker { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		base.Controller = GetComponent<PlatformCharacterController>();
		Behaviour = GetComponent<EsdrasBehaviour>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		AnimatorInyector = GetComponentInChildren<EsdrasAnimatorInyector>();
		Audio = GetComponentInChildren<EsdrasAudio>();
		damageEffect = GetComponentInChildren<DamageEffectScript>();
		Input = GetComponentInChildren<NPCInputs>();
		GhostTrail = GetComponentInChildren<GhostTrailGenerator>();
		MotionChecker = GetComponentInChildren<EntityMotionChecker>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		SetOrientation(lookLeftOnStart ? EntityOrientation.Left : EntityOrientation.Right);
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

	public void CounterFlash()
	{
		damageEffect.Blink(0f, 0.15f);
	}

	public void Damage(Hit hit)
	{
		if (Status.Dead || Core.Logic.Penitent.Stats.Life.Current <= 0f)
		{
			return;
		}
		DamageFlash();
		if (GuardHit(hit))
		{
			SleepTimeByHit(hit);
			return;
		}
		if (WillDieByHit(hit))
		{
			hit.HitSoundId = finalHit;
		}
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
			Behaviour.Damage();
			SleepTimeByHit(hit);
		}
	}

	public void PerpetuaSummonSlowmo()
	{
		Core.Logic.ScreenFreeze.Freeze(0.1f, 1f, 0f, slowTimeCurve);
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
