using System;
using Framework.Managers;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Effects.Player.GhostTrail;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Enemies.PontiffHusk.AI;
using Gameplay.GameControllers.Enemies.PontiffHusk.Attack;
using Gameplay.GameControllers.Enemies.PontiffHusk.Audio;
using Gameplay.GameControllers.Entities;
using NodeCanvas.BehaviourTrees;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.PontiffHusk;

public class PontiffHuskRanged : Enemy, IDamageable
{
	protected EnemyDamageArea DamageArea;

	public SpriteRenderer Sprite { get; private set; }

	public PontiffHuskRangedAttack PontiffHuskRangedAttack { get; private set; }

	public AttackArea AttackArea { get; private set; }

	public MotionLerper MotionLerper { get; private set; }

	public PontiffHuskFloatingMotion FloatingMotion { get; private set; }

	public PontiffHuskRangedBehaviour Behaviour { get; private set; }

	public GhostTrailGenerator GhostTrail { get; set; }

	public PontiffHuskAudio Audio { get; set; }

	public ColorFlash ColorFlash { get; private set; }

	public BehaviourTreeOwner BehaviourTree { get; private set; }

	public VisionCone VisionCone { get; set; }

	public float TargetDistance => Vector2.Distance(base.transform.position, base.Target.transform.position);

	public void Damage(Hit hit)
	{
		if (hit.DamageAmount < 999f)
		{
			hit.DamageAmount = 999f;
		}
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
		Behaviour = GetComponent<PontiffHuskRangedBehaviour>();
		Sprite = GetComponentInChildren<SpriteRenderer>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		PontiffHuskRangedAttack = GetComponentInChildren<PontiffHuskRangedAttack>();
		FloatingMotion = GetComponentInChildren<PontiffHuskFloatingMotion>();
		AttackArea = GetComponentInChildren<AttackArea>();
		GhostTrail = GetComponentInChildren<GhostTrailGenerator>();
		ColorFlash = GetComponentInChildren<ColorFlash>();
		BehaviourTree = GetComponent<BehaviourTreeOwner>();
		VisionCone = GetComponentInChildren<VisionCone>();
		Behaviour.enabled = false;
		Behaviour.PauseBehaviour();
	}

	protected override void OnStart()
	{
		base.OnStart();
		if ((bool)PontiffHuskRangedAttack)
		{
			PontiffHuskRangedAttack pontiffHuskRangedAttack = PontiffHuskRangedAttack;
			pontiffHuskRangedAttack.OnEntityAttack = (Core.GenericEvent)Delegate.Combine(pontiffHuskRangedAttack.OnEntityAttack, new Core.GenericEvent(PontiffHuskRanged_OnEntityAttack));
		}
		FloatingMotion.IsFloating = true;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (Core.ready)
		{
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
	}

	private void PontiffHuskRanged_OnEntityAttack(object hit)
	{
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)PontiffHuskRangedAttack)
		{
			PontiffHuskRangedAttack pontiffHuskRangedAttack = PontiffHuskRangedAttack;
			pontiffHuskRangedAttack.OnEntityAttack = (Core.GenericEvent)Delegate.Remove(pontiffHuskRangedAttack.OnEntityAttack, new Core.GenericEvent(PontiffHuskRanged_OnEntityAttack));
		}
	}

	public override EnemyAttack EnemyAttack()
	{
		return PontiffHuskRangedAttack;
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
