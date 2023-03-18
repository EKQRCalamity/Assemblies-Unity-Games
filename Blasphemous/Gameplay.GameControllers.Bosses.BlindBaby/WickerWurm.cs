using System;
using DamageEffect;
using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.BurntFace;
using Gameplay.GameControllers.Bosses.WickerWurm;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.BlindBaby;

public class WickerWurm : Enemy, IDamageable
{
	public AnimationCurve slowTimeCurve;

	public WickerWurmAudio Audio;

	[SerializeField]
	[BoxGroup("Audio", true, false, 0)]
	[EventRef]
	private string finalHit;

	public WickerWurmBehaviour Behaviour { get; private set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public WickerWurmAnimatorInyector AnimatorInyector { get; private set; }

	public DamageEffectScript damageEffect { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		Behaviour = GetComponent<WickerWurmBehaviour>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		AnimatorInyector = GetComponentInChildren<WickerWurmAnimatorInyector>();
		damageEffect = GetComponentInChildren<DamageEffectScript>();
		Audio = GetComponentInChildren<WickerWurmAudio>();
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
		damageEffect.Blink(0f, 0.1f);
	}

	public void Damage(Hit hit)
	{
		if (!Status.Dead && !(Core.Logic.Penitent.Stats.Life.Current <= 0f) && !Behaviour.HasGrabbedPenitent())
		{
			if (WillDieByHit(hit))
			{
				hit.HitSoundId = finalHit;
			}
			DamageArea.TakeDamage(hit);
			DamageFlash();
			if (Status.Dead)
			{
				DamageArea.DamageAreaCollider.enabled = false;
				Core.Logic.ScreenFreeze.Freeze(0.05f, 4f, 0f, slowTimeCurve);
				Core.Logic.CameraManager.ScreenEffectsManager.ScreenModeBW(0.5f);
				Behaviour.Death();
			}
			else
			{
				Behaviour.Damage();
				SleepTimeByHit(hit);
			}
		}
	}

	public Vector3 GetPosition()
	{
		throw new NotImplementedException();
	}
}
