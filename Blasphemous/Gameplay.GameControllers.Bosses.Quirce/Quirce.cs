using System;
using CreativeSpore.SmartColliders;
using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.Quirce.Animation;
using Gameplay.GameControllers.Bosses.Quirce.Audio;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Quirce;

public class Quirce : Enemy, IDamageable
{
	public AnimationCurve slowTimeCurve;

	public QuirceBossFightPoints BossFightPoints;

	[SerializeField]
	[BoxGroup("Audio", true, false, 0)]
	[EventRef]
	private string finalHit;

	public QuirceBehaviour Behaviour { get; set; }

	public NPCInputs Input { get; set; }

	public SmartPlatformCollider Collider { get; set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public QuirceAnimatorInyector AnimatorInyector { get; set; }

	public EnemyAttack Attack { get; set; }

	public ColorFlash ColorFlash { get; set; }

	public QuirceAudio Audio { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		Behaviour = GetComponent<QuirceBehaviour>();
		Input = GetComponent<NPCInputs>();
		base.Controller = GetComponent<PlatformCharacterController>();
		Collider = GetComponent<SmartPlatformCollider>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		Attack = GetComponentInChildren<EnemyAttack>();
		AnimatorInyector = GetComponentInChildren<QuirceAnimatorInyector>();
		ColorFlash = GetComponentInChildren<ColorFlash>();
		Audio = GetComponentInChildren<QuirceAudio>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		Status.CastShadow = true;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		Status.IsGrounded = Behaviour.motionChecker.HitsFloor;
	}

	public override void SetPositionAtStart()
	{
		base.SetPositionAtStart();
		Vector3 position = new Vector3(base.transform.position.x, base.transform.position.y, base.transform.position.z);
		base.transform.position = position;
	}

	public void Damage(Hit hit)
	{
		if (Status.Dead || Core.Logic.Penitent.Stats.Life.Current <= 0f)
		{
			return;
		}
		if (GuardHit(hit))
		{
			Debug.Log("GUARDED HIT");
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
			Core.Logic.CameraManager.ScreenEffectsManager.ScreenModeBW(0.5f);
			Core.Logic.ScreenFreeze.Freeze(0.1f, 3f, 0f, slowTimeCurve);
			DamageArea.DamageAreaCollider.enabled = false;
			Behaviour.Death();
			Audio.PlayDeath();
		}
		else
		{
			SleepTimeByHit(hit);
			Behaviour.Damage();
			ColorFlash.TriggerColorFlash();
		}
	}

	public void ActivateColliders(bool activate)
	{
		DamageArea.DamageAreaCollider.enabled = activate;
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
