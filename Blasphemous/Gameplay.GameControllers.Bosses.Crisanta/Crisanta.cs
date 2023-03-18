using System;
using CreativeSpore.SmartColliders;
using DamageEffect;
using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.Crisanta.Animator;
using Gameplay.GameControllers.Bosses.Crisanta.Audio;
using Gameplay.GameControllers.Effects.Player.GhostTrail;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Crisanta;

public class Crisanta : Enemy, IDamageable
{
	public DamageEffectScript DamageEffect;

	public AnimationCurve slowTimeCurve;

	public bool IsCrisantaRedux;

	[HideIf("IsCrisantaRedux", true)]
	public GameObject CrisantaRedux;

	[SerializeField]
	[BoxGroup("Audio", true, false, 0)]
	[EventRef]
	private string finalHit;

	public CrisantaBehaviour Behaviour { get; private set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public NPCInputs Input { get; set; }

	public CrisantaAnimatorInyector AnimatorInyector { get; private set; }

	public CrisantaAudio Audio { get; private set; }

	public GhostTrailGenerator GhostTrail { get; private set; }

	public EntityMotionChecker MotionChecker { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		base.Controller = GetComponent<PlatformCharacterController>();
		Behaviour = GetComponent<CrisantaBehaviour>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		AnimatorInyector = GetComponentInChildren<CrisantaAnimatorInyector>();
		Audio = GetComponentInChildren<CrisantaAudio>();
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
		DamageEffect.Blink(0f, 0.07f);
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
			Behaviour.OnGuardHit();
			return;
		}
		if (WillDieByHit(hit))
		{
			hit.HitSoundId = finalHit;
		}
		DamageArea.TakeDamage(hit);
		if (Status.Dead)
		{
			Core.Logic.ScreenFreeze.Freeze(0.01f, 3f, 0f, slowTimeCurve);
			Core.Logic.CameraManager.ScreenEffectsManager.ScreenModeBW(0.5f);
			DamageArea.DamageAreaCollider.enabled = false;
			Behaviour.Death();
		}
		else
		{
			Behaviour.Damage();
			SleepTimeByHit(hit);
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
