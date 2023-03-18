using System;
using BezierSplines;
using Framework.FrameworkCore;
using Gameplay.GameControllers.Enemies.Framework.IA;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.PatrollingFlyingEnemy.AI;

public class PatrollingFlyingEnemyBehaviour : EnemyBehaviour
{
	public BezierSpline currentPath;

	public AnimationCurve currentCurve;

	public float secondsToFullLoop;

	private Vector3 _pathOrigin;

	private float _updateCounter;

	private bool followPath = true;

	private PatrollingFlyingEnemy PatrollingFlyingEnemy;

	public void SetPath(BezierSpline s)
	{
		currentPath = s;
		_pathOrigin = currentPath.transform.position;
	}

	public override void OnAwake()
	{
		base.OnAwake();
		_pathOrigin = currentPath.transform.position;
	}

	public override void OnStart()
	{
		base.OnStart();
		PatrollingFlyingEnemy = (PatrollingFlyingEnemy)Entity;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (followPath)
		{
			FollowPathUpdate();
		}
	}

	private void LateUpdate()
	{
		currentPath.transform.position = _pathOrigin;
	}

	private void FollowPathUpdate()
	{
		float t = currentCurve.Evaluate(_updateCounter / secondsToFullLoop);
		Vector3 point = currentPath.GetPoint(t);
		LookAtTarget(base.transform.position + (point - base.transform.position).normalized * 5f);
		base.transform.position = point;
		_updateCounter += Time.deltaTime;
		_updateCounter %= secondsToFullLoop;
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
			}
		}
		else if (Entity.transform.position.x < targetPos.x - 1f && Entity.Status.Orientation != 0)
		{
			if (OnTurning != null)
			{
				OnTurning();
			}
			Entity.SetOrientation(EntityOrientation.Right);
		}
	}

	public void Death()
	{
		followPath = false;
		PatrollingFlyingEnemy.AnimatorInyector.Death();
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
}
