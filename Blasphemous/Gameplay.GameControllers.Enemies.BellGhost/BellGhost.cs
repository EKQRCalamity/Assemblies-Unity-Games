using System;
using Framework.Managers;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Effects.Player.GhostTrail;
using Gameplay.GameControllers.Enemies.BellGhost.AI;
using Gameplay.GameControllers.Enemies.BellGhost.Attack;
using Gameplay.GameControllers.Enemies.BellGhost.Audio;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.BellGhost;

public class BellGhost : Enemy, IDamageable
{
	protected EnemyDamageArea DamageArea;

	public SpriteRenderer Sprite { get; private set; }

	public BellGhostAttack BellGhostAttack { get; private set; }

	public AttackArea AttackArea { get; private set; }

	public MotionLerper MotionLerper { get; private set; }

	public BellGhostFloatingMotion FloatingMotion { get; private set; }

	public BellGhostBehaviour Behaviour { get; private set; }

	public GhostTrailGenerator GhostTrail { get; set; }

	public BellGhostAudio Audio { get; set; }

	public ColorFlash ColorFlash { get; private set; }

	public float TargetDistance => Vector2.Distance(base.transform.position, base.Target.transform.position);

	public void Damage(Hit hit)
	{
		DamageArea.TakeDamage(hit);
		ColorFlash.TriggerColorFlash();
		if (Status.Dead)
		{
			Behaviour.AnimatorInyector.Death();
			Audio.Death();
		}
		else
		{
			Behaviour.AnimatorInyector.Hurt();
			Behaviour.HurtDisplacement(hit.AttackingEntity);
			if (Behaviour.bellGhostVariant == BELL_GHOST_TYPES.BULLET)
			{
				Audio.HurtVariant();
			}
			else
			{
				Audio.Hurt();
			}
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
		Audio = GetComponentInChildren<BellGhostAudio>();
		Behaviour = GetComponent<BellGhostBehaviour>();
		Sprite = GetComponentInChildren<SpriteRenderer>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		BellGhostAttack = GetComponentInChildren<BellGhostAttack>();
		FloatingMotion = GetComponentInChildren<BellGhostFloatingMotion>();
		AttackArea = GetComponentInChildren<AttackArea>();
		GhostTrail = GetComponentInChildren<GhostTrailGenerator>();
		ColorFlash = GetComponentInChildren<ColorFlash>();
		Behaviour.enabled = false;
		Behaviour.PauseBehaviour();
	}

	protected override void OnStart()
	{
		base.OnStart();
		if ((bool)BellGhostAttack)
		{
			BellGhostAttack bellGhostAttack = BellGhostAttack;
			bellGhostAttack.OnEntityAttack = (Core.GenericEvent)Delegate.Combine(bellGhostAttack.OnEntityAttack, new Core.GenericEvent(bellGhost_OnEntityAttack));
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

	private void bellGhost_OnEntityAttack(object hit)
	{
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)BellGhostAttack)
		{
			BellGhostAttack bellGhostAttack = BellGhostAttack;
			bellGhostAttack.OnEntityAttack = (Core.GenericEvent)Delegate.Remove(bellGhostAttack.OnEntityAttack, new Core.GenericEvent(bellGhost_OnEntityAttack));
		}
	}

	public override EnemyAttack EnemyAttack()
	{
		return BellGhostAttack;
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
