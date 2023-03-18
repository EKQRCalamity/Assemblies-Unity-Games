using System;
using Framework.Managers;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Enemies.MeltedLady.Animator;
using Gameplay.GameControllers.Enemies.MeltedLady.Audio;
using Gameplay.GameControllers.Enemies.MeltedLady.IA;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.StateMachine;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Gizmos;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.MeltedLady;

public class FloatingLady : Enemy
{
	public FloatingLadyAnimatorInjector AnimatorInyector;

	public MeltedLadyBehaviour Behaviour { get; private set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public MeltedLadyAudio Audio { get; private set; }

	public ColorFlash ColorFlash { get; private set; }

	public StateMachine StateMachine { get; private set; }

	public RootMotionDriver ProjectileLaunchRoot { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		Behaviour = GetComponent<MeltedLadyBehaviour>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		Audio = GetComponentInChildren<MeltedLadyAudio>();
		ColorFlash = GetComponentInChildren<ColorFlash>();
		StateMachine = GetComponentInChildren<StateMachine>();
		ProjectileLaunchRoot = GetComponentInChildren<RootMotionDriver>();
		Behaviour.enabled = false;
		SpawnManager.OnPlayerSpawn += OnPlayerSpawn;
	}

	private void OnPlayerSpawn(Gameplay.GameControllers.Penitent.Penitent penitent)
	{
		if ((bool)Behaviour)
		{
			Behaviour.enabled = true;
		}
		base.Target = penitent.gameObject;
		SpawnManager.OnPlayerSpawn -= OnPlayerSpawn;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (base.Target == null)
		{
			base.Target = Core.Logic.Penitent.gameObject;
		}
		if (!base.Landing)
		{
			SetPositionAtStart();
		}
	}

	public override void SetPositionAtStart()
	{
		base.SetPositionAtStart();
		float distance = Physics2D.Raycast(base.transform.position, -Vector2.up, 5f, Behaviour.BlockLayerMask).distance;
		Vector3 position = base.transform.position;
		position.y -= distance;
		base.transform.position = position;
		base.Landing = true;
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
