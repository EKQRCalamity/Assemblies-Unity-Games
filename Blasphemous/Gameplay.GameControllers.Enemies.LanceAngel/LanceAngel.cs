using System;
using BezierSplines;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.Quirce.Attack;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Enemies.LanceAngel.AI;
using Gameplay.GameControllers.Enemies.LanceAngel.Animator;
using Gameplay.GameControllers.Enemies.LanceAngel.Audio;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.StateMachine;
using Gameplay.GameControllers.Penitent;
using Plugins.GhostSprites2D.Scripts.GhostSprites;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.LanceAngel;

public class LanceAngel : Enemy, IDamageable
{
	public BezierSpline Spline { get; private set; }

	public StateMachine StateMachine { get; private set; }

	public LanceAngelBehaviour Behaviour { get; private set; }

	public VisionCone VisionCone { get; private set; }

	public ColorFlash ColorFlash { get; private set; }

	public BossDashAttack DashAttack { get; private set; }

	public EnemyDamageArea DamageArea { get; set; }

	public LanceAngelAudio Audio { get; private set; }

	public GhostSprites GhostSprites { get; private set; }

	public MotionLerper MotionLerper { get; private set; }

	public LanceAngelAnimatorInjector AnimatorInjector { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		Spline = GetComponentInChildren<BezierSpline>();
		Audio = GetComponentInChildren<LanceAngelAudio>();
		Behaviour = GetComponentInChildren<LanceAngelBehaviour>();
		VisionCone = GetComponentInChildren<VisionCone>();
		ColorFlash = GetComponentInChildren<ColorFlash>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		StateMachine = GetComponentInChildren<StateMachine>();
		DashAttack = GetComponentInChildren<BossDashAttack>();
		GhostSprites = GetComponentInChildren<GhostSprites>();
		MotionLerper = GetComponentInChildren<MotionLerper>();
		AnimatorInjector = GetComponentInChildren<LanceAngelAnimatorInjector>();
		Behaviour.enabled = false;
		SpawnManager.OnPlayerSpawn += OnPlayerSpawn;
	}

	private void OnPlayerSpawn(Gameplay.GameControllers.Penitent.Penitent penitent)
	{
		SpawnManager.OnPlayerSpawn -= OnPlayerSpawn;
		base.Target = penitent.gameObject;
		Behaviour.enabled = true;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!base.Landing)
		{
			base.Landing = true;
			SetPositionAtStart();
		}
	}

	public override void SetPositionAtStart()
	{
		base.SetPositionAtStart();
		float distance = Physics2D.Raycast(base.transform.position, -base.transform.up, 5f, Behaviour.BlockLayerMask).distance;
		Vector3 position = new Vector3(base.transform.position.x, base.transform.position.y - distance, base.transform.position.z);
		position.y += Behaviour.StartHeightPosition;
		base.transform.position = position;
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
		SleepTimeByHit(hit);
		if (Status.Dead)
		{
			AnimatorInjector.Death();
			return;
		}
		if (!Behaviour.IsRepositioning)
		{
			Behaviour.Damage();
		}
		ColorFlash.TriggerColorFlash();
	}

	public override void Parry()
	{
		base.Parry();
		Behaviour.Parry();
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}
}
