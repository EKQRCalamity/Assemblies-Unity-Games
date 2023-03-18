using System;
using DamageEffect;
using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.TresAngustias.Animator;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Enemies.MasterAnguish.Audio;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.TresAngustias;

public class TresAngustiasMaster : Enemy, IDamageable
{
	public AnguishBossfightConfig bossfightPoints;

	public AnimationCurve slowTimeCurve;

	public bool Invencible = true;

	[SerializeField]
	[BoxGroup("Audio", true, false, 0)]
	[EventRef]
	private string finalHit;

	public TresAngustiasMasterBehaviour Behaviour { get; private set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public TresAngustiasMasterAnimatorInyector AnimatorInyector { get; private set; }

	public DamageEffectScript damageEffect { get; private set; }

	public MasterAnguishAudio Audio { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		Audio = GetComponentInChildren<MasterAnguishAudio>();
		Behaviour = GetComponent<TresAngustiasMasterBehaviour>();
		base.EnemyBehaviour = Behaviour;
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		AnimatorInyector = GetComponentInChildren<TresAngustiasMasterAnimatorInyector>();
		damageEffect = GetComponentInChildren<DamageEffectScript>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		Behaviour.singleAnguishLance.OnSingleAnguishDamaged += OnSingleAnguishDamaged;
		Behaviour.singleAnguishMace.OnSingleAnguishDamaged += OnSingleAnguishDamaged;
		Behaviour.singleAnguishShield.OnSingleAnguishDamaged += OnSingleAnguishDamaged;
	}

	private void OnSingleAnguishDamaged(SingleAnguish arg1, Hit arg2)
	{
		Damage(arg2);
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
		if ((bool)damageEffect)
		{
			damageEffect.Blink(0f, 0.07f);
		}
	}

	public void Damage(Hit hit)
	{
		if (!Status.Dead && !Invencible && !(Core.Logic.Penitent.Stats.Life.Current <= 0f))
		{
			if (WillDieByHit(hit))
			{
				hit.HitSoundId = finalHit;
			}
			DamageArea.TakeDamage(hit);
			if (Status.Dead)
			{
				Core.Logic.ScreenFreeze.Freeze(0.01f, 2f, 0f, slowTimeCurve);
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
	}

	public Vector3 GetPosition()
	{
		throw new NotImplementedException();
	}
}
