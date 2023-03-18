using System;
using Framework.Managers;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Enemies.PontiffHusk.AI;
using Gameplay.GameControllers.Enemies.PontiffHusk.Attack;
using Gameplay.GameControllers.Enemies.PontiffHusk.Audio;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.PontiffHusk;

public class PontiffHuskMelee : Enemy, IDamageable
{
	public EnemyDamageArea DamageArea;

	public SpriteRenderer Sprite { get; private set; }

	public PontiffHuskMeleeAttack PontiffHuskMeleeAttack { get; private set; }

	public AttackArea AttackArea { get; private set; }

	public MotionLerper MotionLerper { get; private set; }

	public PontiffHuskFloatingMotion FloatingMotion { get; private set; }

	public PontiffHuskMeleeBehaviour Behaviour { get; private set; }

	public PontiffHuskAudio Audio { get; set; }

	public ColorFlash ColorFlash { get; private set; }

	public float TargetDistance => Vector2.Distance(base.transform.position, base.Target.transform.position);

	public void Damage(Hit hit)
	{
		DamageArea.TakeDamage(hit);
		ColorFlash.TriggerColorFlash();
		if (Status.Dead)
		{
			bool cut = hit.DamageElement == Gameplay.GameControllers.Entities.DamageArea.DamageElement.Normal;
			Behaviour.AnimatorInyector.Death(cut);
			Audio.Death(cut);
		}
		else
		{
			Behaviour.AnimatorInyector.Hurt();
			Behaviour.HurtDisplacement(hit.AttackingEntity);
		}
		SleepTimeByHit(hit);
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		MotionLerper = GetComponent<MotionLerper>();
		Audio = GetComponentInChildren<PontiffHuskAudio>();
		Behaviour = GetComponent<PontiffHuskMeleeBehaviour>();
		Sprite = GetComponentInChildren<SpriteRenderer>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		PontiffHuskMeleeAttack = GetComponentInChildren<PontiffHuskMeleeAttack>();
		FloatingMotion = GetComponentInChildren<PontiffHuskFloatingMotion>();
		AttackArea = GetComponentInChildren<AttackArea>();
		ColorFlash = GetComponentInChildren<ColorFlash>();
		Behaviour.enabled = false;
		Behaviour.PauseBehaviour();
	}

	protected override void OnStart()
	{
		base.OnStart();
		if ((bool)PontiffHuskMeleeAttack)
		{
			PontiffHuskMeleeAttack pontiffHuskMeleeAttack = PontiffHuskMeleeAttack;
			pontiffHuskMeleeAttack.OnEntityAttack = (Core.GenericEvent)Delegate.Combine(pontiffHuskMeleeAttack.OnEntityAttack, new Core.GenericEvent(PontiffHuskMelee_OnEntityAttack));
		}
		FloatingMotion.IsFloating = true;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (base.Target != null)
		{
			base.DistanceToTarget = Vector2.Distance(base.transform.position, base.Target.transform.position);
		}
		else
		{
			base.Target = Core.Logic.Penitent.gameObject;
		}
		Status.IsVisibleOnCamera = IsVisible();
		if ((bool)base.Target && !Behaviour.enabled)
		{
			Behaviour.enabled = true;
		}
		DamageArea.DamageAreaCollider.enabled = spriteRenderer.color.a > 0.5f;
		if (Status.Dead)
		{
			DamageArea.DamageAreaCollider.enabled = false;
		}
		else if (base.DistanceToTarget <= ActivationRange)
		{
			Behaviour.StartBehaviour();
		}
		else
		{
			Behaviour.PauseBehaviour();
		}
	}

	private void PontiffHuskMelee_OnEntityAttack(object hit)
	{
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)PontiffHuskMeleeAttack)
		{
			PontiffHuskMeleeAttack pontiffHuskMeleeAttack = PontiffHuskMeleeAttack;
			pontiffHuskMeleeAttack.OnEntityAttack = (Core.GenericEvent)Delegate.Remove(pontiffHuskMeleeAttack.OnEntityAttack, new Core.GenericEvent(PontiffHuskMelee_OnEntityAttack));
		}
	}

	public override EnemyAttack EnemyAttack()
	{
		return PontiffHuskMeleeAttack;
	}

	public override EnemyBumper EnemyBumper()
	{
		throw new NotImplementedException();
	}

	public override EnemyFloorChecker EnemyFloorChecker()
	{
		throw new NotImplementedException();
	}

	protected override void EnablePhysics(bool enable)
	{
		throw new NotImplementedException();
	}

	public bool IsVisible()
	{
		return Entity.IsVisibleFrom(Sprite, UnityEngine.Camera.main);
	}
}
