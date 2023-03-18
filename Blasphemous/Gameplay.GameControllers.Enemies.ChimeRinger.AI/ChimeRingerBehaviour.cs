using System;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Sirenix.OdinInspector;
using Tools.Level.Actionables;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.ChimeRinger.AI;

public class ChimeRingerBehaviour : EnemyBehaviour
{
	public GlobalTrapTriggerer trapTriggerer;

	[FoldoutGroup("Overwritten by scene trap manager", false, 0)]
	public float ringLapse = 5f;

	private float _currentRingLapse;

	public ChimeRinger ChimeRinger { get; private set; }

	public override void OnStart()
	{
		base.OnStart();
		ChimeRinger = (ChimeRinger)Entity;
		ringLapse = trapTriggerer.trapManager.GetSceneTrapLapse();
		_currentRingLapse = ringLapse - trapTriggerer.trapManager.GetFirstTrapLapse();
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (!ChimeRinger.Status.Dead)
		{
			_currentRingLapse += Time.deltaTime;
			if (_currentRingLapse >= ringLapse)
			{
				_currentRingLapse = 0f;
				RingTheBell();
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

	private void RingTheBell()
	{
		ChimeRinger.AnimatorInyector.Ring();
	}

	public void TriggerAllTraps()
	{
		trapTriggerer.TriggerAllTrapsInTheScene();
	}

	public override void Damage()
	{
		if (!(ChimeRinger == null))
		{
			ChimeRinger.AnimatorInyector.Hurt();
		}
	}

	public void Death()
	{
		if (!(ChimeRinger == null))
		{
			ChimeRinger.AnimatorInyector.Death();
		}
	}

	public override void StopMovement()
	{
		throw new NotImplementedException();
	}
}
