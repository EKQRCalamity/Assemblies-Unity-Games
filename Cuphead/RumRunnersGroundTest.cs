using UnityEngine;

public class RumRunnersGroundTest : MonoBehaviour
{
	[SerializeField]
	private Collider2D collider;

	[SerializeField]
	private float yOffset;

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.white;
		Gizmos.DrawSphere(new Vector3(base.transform.position.x, RumRunnersLevel.GroundWalkingPosY(base.transform.position + Vector3.up * 50f, collider, yOffset)), 20f);
		Gizmos.color = Color.cyan;
		Gizmos.DrawSphere(new Vector3(base.transform.position.x, RumRunnersLevel.GroundWalkingPosY(new Vector3(base.transform.position.x, RumRunnersLevel.GroundWalkingPosY(base.transform.position, collider, yOffset)), collider, yOffset)), 20f);
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(base.transform.position, 20f);
	}
}
