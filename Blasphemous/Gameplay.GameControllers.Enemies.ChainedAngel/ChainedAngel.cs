using System;
using System.Linq;
using Framework.Managers;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Enemies.BellGhost;
using Gameplay.GameControllers.Enemies.ChainedAngel.AI;
using Gameplay.GameControllers.Enemies.ChainedAngel.Animator;
using Gameplay.GameControllers.Enemies.ChainedAngel.Audio;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.StateMachine;
using Gameplay.GameControllers.Penitent;
using Tools.Level.Layout;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.ChainedAngel;

public class ChainedAngel : Enemy, IDamageable
{
	public bool IsAnchored;

	public BodyChainMaster BodyChainMaster { get; private set; }

	public BellGhostFloatingMotion FloatingMotion { get; private set; }

	public VisionCone VisionCone { get; private set; }

	public ChainedAngelBehaviour Behaviour { get; private set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public ColorFlash DamageEffect { get; private set; }

	public ChainedAngelAnimatorInjector AnimatorInjector { get; private set; }

	public StateMachine StateMachine { get; private set; }

	public ChainedAngelAudio Audio { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		BodyChainMaster = GetComponentInChildren<BodyChainMaster>();
		FloatingMotion = GetComponentInChildren<BellGhostFloatingMotion>();
		VisionCone = GetComponentInChildren<VisionCone>();
		Behaviour = GetComponentInChildren<ChainedAngelBehaviour>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		DamageEffect = GetComponentInChildren<ColorFlash>();
		AnimatorInjector = GetComponentInChildren<ChainedAngelAnimatorInjector>();
		StateMachine = GetComponentInChildren<StateMachine>();
		Audio = GetComponentInChildren<ChainedAngelAudio>();
		Behaviour.enabled = false;
		SpawnManager.OnPlayerSpawn += OnPlayerSpawn;
	}

	private void OnPlayerSpawn(Gameplay.GameControllers.Penitent.Penitent penitent)
	{
		Behaviour.enabled = true;
		base.Target = penitent.gameObject;
	}

	protected override void OnStart()
	{
		base.OnStart();
		GameObject lowerLink = GetLowerLink();
		Vector3 spawnPosition = GetSpawnPosition();
		if (IsAnchored)
		{
			spawnPosition.y -= 2f;
		}
		lowerLink.transform.position = spawnPosition;
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
			AnimatorInjector.Death();
			return;
		}
		DamageEffect.TriggerColorFlash();
		SleepTimeByHit(hit);
		if (BodyChainMaster.IsAttacking)
		{
			BodyChainMaster.Repullo();
		}
	}

	private Vector3 GetSpawnPosition()
	{
		Vector3 position = base.transform.position;
		EnemySpawnPoint enemySpawnPoint = UnityEngine.Object.FindObjectsOfType<EnemySpawnPoint>().FirstOrDefault((EnemySpawnPoint x) => x.SpawningId == base.SpawningId);
		if ((bool)enemySpawnPoint)
		{
			position = enemySpawnPoint.Position;
		}
		return position;
	}

	public GameObject GetLowerLink()
	{
		GameObject result = null;
		float num = float.MaxValue;
		foreach (BodyChainLink link in BodyChainMaster.links)
		{
			if (link.transform.position.y < num)
			{
				num = link.transform.position.y;
				result = link.gameObject;
			}
		}
		return result;
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		SpawnManager.OnPlayerSpawn -= OnPlayerSpawn;
	}
}
