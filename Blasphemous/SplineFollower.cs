using System;
using BezierSplines;
using Sirenix.OdinInspector;
using UnityEngine;

public class SplineFollower : MonoBehaviour
{
	[FoldoutGroup("References", 0)]
	public BezierSpline spline;

	[FoldoutGroup("Following spline config", 0)]
	public float currentCounter;

	[FoldoutGroup("Following spline config", 0)]
	public float duration = 10f;

	[FoldoutGroup("Following spline config", 0)]
	public AnimationCurve movementCurve;

	[FoldoutGroup("Runtime Behaviour", 0)]
	public bool followActivated;

	[FoldoutGroup("Runtime Behaviour", 0)]
	public bool loop;

	public event Action<Vector2> OnMovingToNextPoint;

	public event Action OnMovementCompleted;

	public void SetData(SplineThrowData data)
	{
		spline = data.spline;
		movementCurve = data.curve;
		duration = data.duration;
	}

	private void Update()
	{
		if (followActivated)
		{
			FollowSpline();
		}
	}

	public Vector2 GetDirection()
	{
		float t = movementCurve.Evaluate(currentCounter / duration);
		return spline.GetDirection(t);
	}

	public bool HasFinished()
	{
		return currentCounter == duration;
	}

	public void StartFollowing(bool loop)
	{
		this.loop = loop;
		currentCounter = 0f;
		followActivated = true;
	}

	private void FollowSpline()
	{
		float t = movementCurve.Evaluate(currentCounter / duration);
		Vector3 point = spline.GetPoint(t);
		BeforeMoving(point);
		base.transform.position = point;
		if (!loop && currentCounter == duration)
		{
			followActivated = false;
			if (this.OnMovementCompleted != null)
			{
				this.OnMovementCompleted();
			}
			return;
		}
		currentCounter += Time.deltaTime;
		if (loop)
		{
			currentCounter %= duration;
		}
		else if (currentCounter > duration)
		{
			currentCounter = duration;
		}
	}

	private void BeforeMoving(Vector2 nextPoint)
	{
		if (this.OnMovingToNextPoint != null)
		{
			this.OnMovingToNextPoint(nextPoint);
		}
	}
}
