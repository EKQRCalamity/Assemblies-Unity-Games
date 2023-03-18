using System;
using DamageEffect;
using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.BejeweledSaint.Animation;
using Gameplay.GameControllers.Bosses.BejeweledSaint.IA;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.BejeweledSaint;

public class BejeweledSaintHead : Enemy, IDamageable
{
	public AnimationCurve slowTimeCurve;

	[SerializeField]
	[BoxGroup("Audio", true, false, 0)]
	[EventRef]
	private string finalHit;

	public BejeweledSaintAnimatorInyector AnimatorInyector { get; set; }

	public BejeweledSaintBoss WholeBoss { get; set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public DamageEffectScript DamageEffect { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		base.EnemyBehaviour = GetComponent<BejeweledSaintBehaviour>();
		AnimatorInyector = GetComponentInChildren<BejeweledSaintAnimatorInyector>();
		WholeBoss = GetComponentInParent<BejeweledSaintBoss>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		DamageEffect = GetComponentInChildren<DamageEffectScript>();
		SpawnManager.OnPlayerSpawn += OnPlayerSpawn;
		base.EnemyBehaviour.enabled = false;
	}

	private void OnPlayerSpawn(Gameplay.GameControllers.Penitent.Penitent penitent)
	{
		base.Target = penitent.gameObject;
		if ((bool)base.EnemyBehaviour)
		{
			base.EnemyBehaviour.enabled = true;
			base.EnemyBehaviour.StartBehaviour();
		}
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
			}
			else
			{
				WholeBoss.Audio.PlayHeadDamage();
				DamageEffect.Blink(0f, 0.1f);
				AnimatorInyector.HeadDamage();
				base.EnemyBehaviour.Damage();
				SleepTimeByHit(hit);
			}
		}
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	public void EnableDamageArea(bool enableDamageArea)
	{
		if (!(DamageArea == null))
		{
			DamageArea.DamageAreaCollider.enabled = enableDamageArea;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		SpawnManager.OnPlayerSpawn -= OnPlayerSpawn;
	}
}
