using UnityEngine;

public struct ProjectileUpdateData
{
	public float elapsedTime;

	public float linearDistanceTraveled;

	public Vector3? start;

	public Vector3? end;

	public Vector3? up;

	public Vector3 forward;

	public Vector3 position;

	public ProjectileStateFlags? stateFlags;

	public Vector3 right => Vector3.Cross(up.Value, forward).normalized;

	public Quaternion rotation => Quaternion.LookRotation(forward, up.Value);

	public float distance => (end.Value - start.Value).magnitude;

	public bool inFlight => !finished;

	public bool impacted
	{
		get
		{
			if (stateFlags.HasValue)
			{
				return EnumUtil.HasFlag(stateFlags.Value, ProjectileStateFlags.Impacted);
			}
			return false;
		}
	}

	public bool finished
	{
		get
		{
			if (stateFlags.HasValue)
			{
				return EnumUtil.HasFlag(stateFlags.Value, ProjectileStateFlags.Finished);
			}
			return false;
		}
	}

	public ProjectileUpdateData(float elapsedTime)
	{
		this = default(ProjectileUpdateData);
		this.elapsedTime = elapsedTime;
	}
}
