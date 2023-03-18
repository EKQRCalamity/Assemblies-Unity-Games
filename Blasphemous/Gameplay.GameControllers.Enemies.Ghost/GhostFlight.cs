using System;
using Framework.Managers;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Ghost;

public class GhostFlight : MonoBehaviour
{
	public Core.SimpleEvent OnStopFloating;

	public Core.SimpleEvent OnLanding;

	protected Ghost ghost;

	public float amplitudeX = 10f;

	protected float defaultAmplitudeX;

	public float amplitudeY = 5f;

	protected float defaultAmplitudeY;

	public float speedX = 1f;

	public float speedY = 2f;

	private float index;

	private float deltaAttackTime;

	[Range(0f, 1f)]
	public float attackSpeed = 0.75f;

	public Vector3 StartPos { get; set; }

	public Vector3 EndPos { get; set; }

	public float TrajectoryHeight { get; set; }

	public bool GetLanding { get; set; }

	public GhostFlight()
	{
		TrajectoryHeight = 4f;
		EndPos = Vector3.zero;
		StartPos = Vector3.zero;
	}

	private void Start()
	{
		defaultAmplitudeX = amplitudeX;
		defaultAmplitudeY = amplitudeY;
		deltaAttackTime = 0f;
		ghost = GetComponentInParent<Ghost>();
		if (ghost != null)
		{
			int currentWayPointId = UnityEngine.Random.Range(0, ghost.GhostPath.waypoints.Length);
			ghost.CurrentWayPointId = currentWayPointId;
			Vector3 randomWaypointPosition = GetRandomWaypointPosition();
			ghost.transform.position = randomWaypointPosition;
		}
	}

	public void Floating()
	{
		index += Time.deltaTime;
		float x = amplitudeX * Mathf.Cos(speedX * index);
		float y = Mathf.Sin(speedY * index) * amplitudeY;
		base.transform.localPosition = new Vector3(x, y, 0f);
	}

	public void EnableFloating(bool enable = true)
	{
		if (OnStopFloating != null)
		{
			OnStopFloating();
		}
	}

	public void Landing(bool down = true)
	{
		deltaAttackTime += Time.deltaTime * attackSpeed;
		Vector3 position = Vector3.Lerp(StartPos, EndPos, deltaAttackTime);
		float num = TrajectoryHeight * Mathf.Sin(Mathf.Clamp01(deltaAttackTime) * (float)Math.PI);
		position.y -= num;
		base.transform.parent.position = position;
		if (V3Equal(base.transform.parent.position, EndPos) && !GetLanding)
		{
			GetLanding = true;
			if (OnLanding != null)
			{
				OnLanding();
			}
		}
	}

	public void SetTargetPosition(Vector3 currenStartPos, Vector3 targetPosition)
	{
		StartPos = currenStartPos;
		EndPos = targetPosition;
		GetLanding = false;
		resetDeltaAttackTime();
	}

	protected void resetDeltaAttackTime()
	{
		if (deltaAttackTime > 0f)
		{
			deltaAttackTime = 0f;
		}
	}

	protected bool V3Equal(Vector3 a, Vector3 b)
	{
		return Vector3.SqrMagnitude(a - b) < 0.0001f;
	}

	public Vector3 GetRandomWaypointPosition()
	{
		Vector3 zero = Vector3.zero;
		do
		{
			int num = UnityEngine.Random.Range(0, ghost.GhostPath.waypoints.Length);
			ghost.GhostPath.NextWayPointVisitedId = num;
			zero = ghost.GhostPath.GetWaypointPosition(num);
		}
		while (ghost.CurrentWayPointId == ghost.GhostPath.NextWayPointVisitedId);
		ghost.CurrentWayPointId = ghost.GhostPath.NextWayPointVisitedId;
		return zero;
	}
}
