using System;
using CreativeSpore.SmartColliders;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.Isidora.Audio;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Effects.Player.GhostTrail;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Isidora;

public class Isidora : Enemy, IDamageable, IPaintDamageableCollider
{
	public AnimationCurve slowTimeCurve;

	public SpriteRenderer spriteRendererVfx;

	[SerializeField]
	[BoxGroup("Audio", true, false, 0)]
	[EventRef]
	private string finalHit;

	public IsidoraBehaviour Behaviour { get; private set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public NPCInputs Input { get; set; }

	public MasterShaderEffects DamageEffect { get; private set; }

	public GhostTrailGenerator GhostTrail { get; private set; }

	public IsidoraAnimatorInyector AnimatorInyector { get; private set; }

	public IsidoraAudio Audio { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		base.Controller = GetComponent<PlatformCharacterController>();
		Behaviour = GetComponent<IsidoraBehaviour>();
		AnimatorInyector = GetComponentInChildren<IsidoraAnimatorInyector>();
		Audio = GetComponent<IsidoraAudio>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		Input = GetComponentInChildren<NPCInputs>();
		GhostTrail = GetComponentInChildren<GhostTrailGenerator>();
		DamageEffect = GetComponentInChildren<MasterShaderEffects>();
		AttachShowScriptIfNeeded();
	}

	public override void SetOrientation(EntityOrientation orientation, bool allowFlipRenderer = true, bool searchForRenderer = false)
	{
		Status.Orientation = orientation;
		if (allowFlipRenderer)
		{
			switch (Status.Orientation)
			{
			case EntityOrientation.Left:
				spriteRenderer.flipX = true;
				spriteRendererVfx.flipX = true;
				break;
			case EntityOrientation.Right:
				spriteRenderer.flipX = false;
				spriteRendererVfx.flipX = false;
				break;
			}
		}
	}

	public void DamageFlash()
	{
		DamageEffect.DamageEffectBlink(0f, 0.07f);
	}

	public override void Parry()
	{
		base.Parry();
		Behaviour.Parry();
	}

	public void Damage(Hit hit)
	{
		if (GuardHit(hit))
		{
			SleepTimeByHit(hit);
		}
		else if (!(Core.Logic.Penitent.Stats.Life.Current <= 0f))
		{
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
				Behaviour.Damage(hit);
				SleepTimeByHit(hit);
			}
		}
	}

	public bool IsCurrentlyDamageable()
	{
		return DamageArea.DamageAreaCollider.enabled;
	}

	public void AttachShowScriptIfNeeded()
	{
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

	internal float GetLifePercentage()
	{
		throw new NotImplementedException();
	}
}
