using System;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Enemies.ChimeRinger.AI;
using Gameplay.GameControllers.Enemies.ChimeRinger.Animator;
using Gameplay.GameControllers.Enemies.ChimeRinger.Audio;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.ChimeRinger;

public class ChimeRinger : Enemy, IDamageable
{
	public ChimeRingerBehaviour Behaviour { get; private set; }

	public ChimeRingerAnimatorInyector AnimatorInyector { get; private set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public ColorFlash colorFlash { get; private set; }

	public ChimeRingerAudio Audio { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		Behaviour = GetComponentInChildren<ChimeRingerBehaviour>();
		AnimatorInyector = GetComponentInChildren<ChimeRingerAnimatorInyector>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		colorFlash = GetComponentInChildren<ColorFlash>();
		Audio = GetComponentInChildren<ChimeRingerAudio>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		base.Target = Core.Logic.Penitent.gameObject;
		SetDefaultOrientation(base.Target.transform.position);
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (base.Target == null)
		{
			base.Target = Core.Logic.Penitent.gameObject;
		}
	}

	private void SetDefaultOrientation(Vector3 targetPos)
	{
		SetOrientation((!(targetPos.x >= base.transform.position.x)) ? EntityOrientation.Left : EntityOrientation.Right);
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
		DamageArea.TakeDamage(hit);
		if (Status.Dead)
		{
			DamageArea.DamageAreaCollider.enabled = false;
			Behaviour.Death();
		}
		else
		{
			colorFlash.TriggerColorFlash();
			Behaviour.Damage();
		}
		SleepTimeByHit(hit);
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}
}
