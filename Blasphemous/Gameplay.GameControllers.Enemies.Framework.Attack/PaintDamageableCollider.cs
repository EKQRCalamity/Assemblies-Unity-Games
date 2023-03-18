using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Framework.Attack;

[RequireComponent(typeof(PolygonCollider2D))]
public class PaintDamageableCollider : MonoBehaviour
{
	private PolygonCollider2D polygonCollider2D;

	private void Awake()
	{
		polygonCollider2D = GetComponent<PolygonCollider2D>();
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = ((!polygonCollider2D.enabled) ? Color.gray : Color.cyan);
		Vector2[] points = polygonCollider2D.points;
		Vector3 from = base.transform.TransformPoint(points[points.Length - 1] + polygonCollider2D.offset);
		Vector3 to = base.transform.TransformPoint(points[0] + polygonCollider2D.offset);
		Gizmos.DrawLine(from, to);
		for (int i = 0; i < points.Length - 1; i++)
		{
			from = base.transform.TransformPoint(points[i] + polygonCollider2D.offset);
			to = base.transform.TransformPoint(points[i + 1] + polygonCollider2D.offset);
			Gizmos.DrawLine(from, to);
		}
	}
}
