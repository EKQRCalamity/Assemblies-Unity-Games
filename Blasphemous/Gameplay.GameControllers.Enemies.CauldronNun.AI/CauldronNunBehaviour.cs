using System;
using System.Collections.Generic;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Tools.Level.Actionables;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.CauldronNun.AI;

public class CauldronNunBehaviour : EnemyBehaviour
{
	public GlobalTrapTriggerer trapTriggerer;

	public float pullLapse = 5f;

	private float _currentPullLapse;

	private List<CauldronTrap> _cauldronTraps;

	public CauldronNun CauldronNun { get; private set; }

	public override void OnStart()
	{
		base.OnStart();
		CauldronNun = (CauldronNun)Entity;
		_currentPullLapse = pullLapse;
		_cauldronTraps = new List<CauldronTrap>(UnityEngine.Object.FindObjectsOfType<CauldronTrap>());
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (!CauldronNun.Status.Dead)
		{
			_currentPullLapse += Time.deltaTime;
			if (_currentPullLapse >= pullLapse)
			{
				_currentPullLapse = 0f;
				PullChain();
			}
		}
	}

	public override void Idle()
	{
	}

	public override void Wander()
	{
	}

	public override void Chase(Transform targetPosition)
	{
	}

	public override void Attack()
	{
	}

	private void PullChain()
	{
		CauldronNun.AnimatorInyector.PullChain();
	}

	public void TriggerAllTraps()
	{
		foreach (CauldronTrap cauldronTrap in _cauldronTraps)
		{
			cauldronTrap.Use();
		}
	}

	public override void ReadSpawnerConfig(SpawnBehaviourConfig config)
	{
		base.ReadSpawnerConfig(config);
		pullLapse = config.TryGetFloat("DELAY");
	}

	public override void Damage()
	{
		if (!(CauldronNun == null))
		{
			CauldronNun.AnimatorInyector.Hurt();
		}
	}

	public void Death()
	{
		if (!(CauldronNun == null))
		{
			CauldronNun.AnimatorInyector.Death();
		}
	}

	public override void StopMovement()
	{
		throw new NotImplementedException();
	}
}
