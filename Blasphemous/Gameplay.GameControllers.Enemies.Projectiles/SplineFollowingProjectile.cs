using System;
using BezierSplines;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Projectiles;

public class SplineFollowingProjectile : Projectile
{
	public bool faceVelocityDirection;

	private BezierSpline _spline;

	public float totalSeconds;

	public float elapsedSeconds;

	private AnimationCurve _curve;

	private bool play;

	private bool finished;

	public event Action<SplineFollowingProjectile, float, float> OnSplineAdvancedEvent;

	public event Action<SplineFollowingProjectile> OnSplineCompletedEvent;

	public virtual void Init(Vector3 origin, BezierSpline spline, float totalSeconds, AnimationCurve curve)
	{
		_spline = spline;
		this.totalSeconds = totalSeconds;
		elapsedSeconds = 0f;
		_curve = curve;
		base.transform.position = spline.GetPoint(0f);
		play = true;
		finished = false;
	}

	protected override void OnUpdate()
	{
		if (play)
		{
			float time = elapsedSeconds / totalSeconds;
			Vector3 point = _spline.GetPoint(_curve.Evaluate(time));
			base.transform.position = point;
			if (this.OnSplineAdvancedEvent != null)
			{
				this.OnSplineAdvancedEvent(this, totalSeconds, elapsedSeconds);
			}
			elapsedSeconds += Time.deltaTime;
			if (elapsedSeconds >= totalSeconds)
			{
				OnTotalSecondsElapsed();
			}
		}
	}

	public void Stop()
	{
		play = false;
		finished = true;
	}

	public bool IsFollowing()
	{
		return play;
	}

	private void OnTotalSecondsElapsed()
	{
		finished = true;
		play = false;
		if (this.OnSplineCompletedEvent != null)
		{
			this.OnSplineCompletedEvent(this);
		}
	}

	public void ForceDestroy()
	{
		OnLifeEnded();
	}
}
