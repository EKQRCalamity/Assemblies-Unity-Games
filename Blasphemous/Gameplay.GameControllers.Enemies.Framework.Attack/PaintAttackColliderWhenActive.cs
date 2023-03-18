using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Framework.Attack;

public class PaintAttackColliderWhenActive : MonoBehaviour
{
	private PolygonCollider2D polygonCollider2D;

	private IPaintAttackCollider attack;

	private void Awake()
	{
		polygonCollider2D = GetComponent<PolygonCollider2D>();
		attack = GetComponent<IPaintAttackCollider>();
		if (attack == null)
		{
			attack = GetComponentInParent<IPaintAttackCollider>();
		}
	}

	private void OnDrawGizmos()
	{
		if (!(polygonCollider2D == null) && attack.IsCurrentlyDealingDamage())
		{
			Vector2[] points = polygonCollider2D.points;
			Gizmos.color = Color.red;
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
}
