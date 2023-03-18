using System;
using System.Collections;
using DamageEffect;
using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.BurntFace.AI;
using Gameplay.GameControllers.Bosses.BurntFace.Animation;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.BurntFace;

public class BurntFace : Enemy, IDamageable
{
	public AnimationCurve slowTimeCurve;

	public BurntFaceBossFightPoints bossFightPoints;

	public SpriteRenderer leftEye;

	public SpriteRenderer rightEye;

	[SerializeField]
	[BoxGroup("Audio", true, false, 0)]
	[EventRef]
	private string finalHit;

	public BurntFaceBehaviour Behaviour { get; private set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public BurntFaceAnimatorInyector AnimatorInyector { get; private set; }

	public DamageEffectScript damageEffect { get; private set; }

	public DamageEffectScript leftEyeDamageEffect { get; private set; }

	public DamageEffectScript rightEyeDamageEffect { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		Behaviour = GetComponent<BurntFaceBehaviour>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		AnimatorInyector = GetComponentInChildren<BurntFaceAnimatorInyector>();
		damageEffect = GetComponentInChildren<DamageEffectScript>();
		leftEyeDamageEffect = leftEye.GetComponent<DamageEffectScript>();
		rightEyeDamageEffect = rightEye.GetComponent<DamageEffectScript>();
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
		if (!Status.Dead && !(Core.Logic.Penitent.Stats.Life.Current <= 0f))
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
				DamageFlash();
				Behaviour.Damage();
				SleepTimeByHit(hit);
			}
		}
	}

	private IEnumerator PopEyeCoroutine(SpriteRenderer spr, float seconds)
	{
		int origSortingOrder = spr.sortingOrder;
		spr.sortingOrder += 20;
		yield return new WaitForSeconds(seconds);
		spr.sortingOrder = origSortingOrder;
	}

	public Vector3 GetPosition()
	{
		throw new NotImplementedException();
	}
}
