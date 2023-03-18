using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Ghost;

public class GhostWaypoint : MonoBehaviour
{
	public float amplitudeY = 3f;

	public GhostPath ghostPath;

	protected int id;

	protected float index;

	public float maxSpeedY = 4f;

	protected float speedY;

	protected Transform target;

	public int Id
	{
		get
		{
			return id;
		}
		set
		{
			id = value;
		}
	}

	private void Start()
	{
		index = 0f;
		ghostPath = GetComponentInParent<GhostPath>();
		speedY = Random.Range(0f, maxSpeedY);
	}

	private void Update()
	{
		index += Time.deltaTime;
		float y = Mathf.Sin(speedY * index) * amplitudeY;
		Vector2 vector = new Vector2(base.transform.localPosition.x, y);
		base.transform.localPosition = vector;
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawIcon(base.transform.position, "Blasphemous/ghost.png", allowScaling: true);
		if (ghostPath != null && ghostPath.enableDebugLines && id != ghostPath.waypoints.Length - 1)
		{
			Gizmos.color = Color.cyan;
			GhostWaypoint ghostWaypoint = ghostPath.waypoints[id + 1];
			Gizmos.DrawLine(base.transform.position, ghostWaypoint.transform.position);
		}
	}
}
