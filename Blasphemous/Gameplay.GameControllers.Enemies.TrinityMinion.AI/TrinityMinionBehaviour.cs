using System;
using Framework.FrameworkCore;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Maikel.SteeringBehaviors;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.TrinityMinion.AI;

public class TrinityMinionBehaviour : EnemyBehaviour
{
	private TrinityMinion TrinityMinion;

	private Transform _currentTarget;

	public Seek seekBehavior;

	public AutonomousAgent agent;

	public Vector2 chaseOffset;

	public float randomMaxSpeedRange = 2f;

	public override void OnAwake()
	{
		base.OnAwake();
	}

	public override void OnStart()
	{
		base.OnStart();
		TrinityMinion = (TrinityMinion)Entity;
		float num = UnityEngine.Random.Range(0f - randomMaxSpeedRange, randomMaxSpeedRange);
		agent.maxSpeed += num;
		agent.maxForce += num;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		UpdateTarget();
	}

	private void LateUpdate()
	{
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
		if (Entity.Status.Dead)
		{
			return;
		}
		if (Entity.transform.position.x >= targetPos.x + 1f)
		{
			if (Entity.Status.Orientation != EntityOrientation.Left)
			{
				if (OnTurning != null)
				{
					OnTurning();
				}
				Entity.SetOrientation(EntityOrientation.Left);
				TrinityMinion.Collider.offset = new Vector2(0f - TrinityMinion.Collider.offset.x, TrinityMinion.Collider.offset.y);
			}
		}
		else if (Entity.transform.position.x < targetPos.x - 1f && Entity.Status.Orientation != 0)
		{
			if (OnTurning != null)
			{
				OnTurning();
			}
			Entity.SetOrientation(EntityOrientation.Right);
			TrinityMinion.Collider.offset = new Vector2(0f - TrinityMinion.Collider.offset.x, TrinityMinion.Collider.offset.y);
		}
	}

	public void Death()
	{
		agent.enabled = false;
		TrinityMinion.AnimatorInyector.Death();
	}

	public override void Idle()
	{
		throw new NotImplementedException();
	}

	public override void Wander()
	{
		throw new NotImplementedException();
	}

	public override void Chase(Transform targetPosition)
	{
		throw new NotImplementedException();
	}

	public override void Attack()
	{
		throw new NotImplementedException();
	}

	public override void Damage()
	{
	}

	public override void StopMovement()
	{
		throw new NotImplementedException();
	}

	public void SetTarget(Transform t)
	{
		_currentTarget = t;
	}

	private void UpdateTarget()
	{
		if (_currentTarget != null)
		{
			seekBehavior.target = _currentTarget.position + (Vector3)chaseOffset;
			LookAtTarget(_currentTarget.position);
		}
	}
}
