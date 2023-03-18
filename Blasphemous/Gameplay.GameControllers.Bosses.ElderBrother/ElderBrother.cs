using System;
using CreativeSpore.SmartColliders;
using DamageEffect;
using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.ElderBrother;

public class ElderBrother : Enemy, IDamageable
{
	public AnimationCurve slowTimeCurve;

	public bool showBossDeathEffect = true;

	[SerializeField]
	[BoxGroup("Audio", true, false, 0)]
	[EventRef]
	private string finalHit;

	public ElderBrotherBehaviour Behaviour { get; set; }

	public NPCInputs Input { get; set; }

	public SmartPlatformCollider Collider { get; set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public ElderBrotherAnimatorInyector AnimatorInyector { get; set; }

	public EnemyAttack Attack { get; set; }

	public DamageEffectScript DamageEffect { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		Behaviour = GetComponent<ElderBrotherBehaviour>();
		Input = GetComponent<NPCInputs>();
		base.Controller = GetComponent<PlatformCharacterController>();
		Collider = GetComponent<SmartPlatformCollider>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		Attack = GetComponentInChildren<EnemyAttack>();
		AnimatorInyector = GetComponentInChildren<ElderBrotherAnimatorInyector>();
		DamageEffect = GetComponentInChildren<DamageEffectScript>();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!base.Landing)
		{
			base.Landing = true;
			SetPositionAtStart();
			base.Controller.PlatformCharacterPhysics.GravityScale = 1f;
		}
	}

	public override void SetPositionAtStart()
	{
		base.SetPositionAtStart();
		float groundDist = base.Controller.GroundDist;
		Vector3 position = new Vector3(base.transform.position.x, base.transform.position.y - groundDist, base.transform.position.z);
		base.transform.position = position;
		base.Controller.PlatformCharacterPhysics.GravityScale = 0f;
	}

	public void Damage(Hit hit)
	{
		if (Status.Dead || Core.Logic.Penitent.Stats.Life.Current <= 0f)
		{
			return;
		}
		if (WillDieByHit(hit))
		{
			hit.HitSoundId = finalHit;
		}
		Behaviour.Damage();
		DamageArea.TakeDamage(hit);
		if (Status.Dead)
		{
			DamageArea.DamageAreaCollider.enabled = false;
			Core.Logic.ScreenFreeze.Freeze(0.01f, 2f, 0f, slowTimeCurve);
			if (showBossDeathEffect)
			{
				Core.Logic.CameraManager.ScreenEffectsManager.ScreenModeBW(1.5f);
			}
			Behaviour.Death();
		}
		else
		{
			DamageEffect.Blink(0f, 0.15f);
			SleepTimeByHit(hit);
		}
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
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
}
