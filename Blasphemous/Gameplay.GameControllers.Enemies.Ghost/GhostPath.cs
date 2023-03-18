using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Ghost;

public class GhostPath : MonoBehaviour
{
	public bool enableDebugLines;

	protected int nextWaypointVisitedId;

	public GhostWaypoint[] waypoints;

	public int NextWayPointVisitedId
	{
		get
		{
			return nextWaypointVisitedId;
		}
		set
		{
			nextWaypointVisitedId = value;
		}
	}

	private void Start()
	{
	}

	public Vector3 GetWaypointPosition(int id)
	{
		Vector3 result = Vector3.zero;
		if (waypoints != null && waypoints.Length > 0)
		{
			int num = Mathf.Clamp(id, 0, waypoints.Length);
			result = base.transform.TransformPoint(waypoints[num].transform.localPosition);
		}
		return result;
	}

	private void OnDrawGizmos()
	{
		setWaypointsId();
	}

	protected void setWaypointsId()
	{
		if (waypoints.Length > 0)
		{
			for (int i = 0; i < waypoints.Length; i++)
			{
				waypoints[i].Id = i;
			}
		}
	}
}
