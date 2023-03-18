using Framework.FrameworkCore;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Entities;

public class VisionCone : Trait
{
	[FoldoutGroup("Vision Settings", true, 0)]
	public LayerMask sightCollisionMask;

	[FoldoutGroup("Vision Settings", true, 0)]
	public Vector2 sightOffset;

	[FoldoutGroup("Vision Settings", true, 0)]
	public float visionAngle;

	[FoldoutGroup("Vision Settings", true, 0)]
	public float sightDistance;

	[FoldoutGroup("Vision Settings", true, 0)]
	public float closeRadius;

	[FoldoutGroup("Vision Settings", true, 0)]
	public float targetHeightOffset = 0.5f;

	[FoldoutGroup("Vision Settings", true, 0)]
	public bool cantSeeBackwards;

	[FoldoutGroup("Gizmo", true, 0)]
	public bool gizmoOn = true;

	[FoldoutGroup("Gizmo", true, 0)]
	[ShowIf("gizmoOn", true)]
	public Color gizmoConeLimitsColor = Color.magenta;

	[FoldoutGroup("Gizmo", true, 0)]
	[ShowIf("gizmoOn", true)]
	public Color gizmoRayColor = Color.cyan;

	[FoldoutGroup("Gizmo", true, 0)]
	[ShowIf("gizmoOn", true)]
	public int gizmoNumberOfRays = 30;

	public bool CanSeeTarget(Transform t, string targetLayer, bool useColliderBounds = false)
	{
		if (t == null)
		{
			return false;
		}
		Vector2 vector = (Vector2)base.transform.position + sightOffset;
		Vector2 vector2 = t.position + Vector3.up * targetHeightOffset;
		if (useColliderBounds)
		{
			Enemy component = t.GetComponent<Enemy>();
			if (component.EntityDamageArea != null)
			{
				vector2 = component.EntityDamageArea.DamageAreaCollider.bounds.center;
			}
		}
		float num = Vector2.Distance(vector, t.position);
		if (num > sightDistance)
		{
			return false;
		}
		if (num < closeRadius)
		{
			return true;
		}
		Vector2 vector3 = vector2 - vector;
		int num2 = ((base.EntityOwner.Status.Orientation == EntityOrientation.Right) ? 1 : (-1));
		float num3 = Mathf.Atan2(Mathf.Abs(vector3.y), Mathf.Abs(vector3.x)) * 57.29578f;
		if (cantSeeBackwards && ((num2 == 1 && vector3.x < 0f) || (num2 == -1 && vector3.x > 0f)))
		{
			return false;
		}
		if (num3 > visionAngle)
		{
			Debug.DrawLine(vector, vector2, Color.black, 1f);
			return false;
		}
		RaycastHit2D[] array = new RaycastHit2D[1];
		if (Physics2D.LinecastNonAlloc(vector, vector2, array, sightCollisionMask) > 0)
		{
			if (array[0].collider.gameObject.layer == LayerMask.NameToLayer(targetLayer))
			{
				Debug.DrawLine(vector, array[0].point, Color.green, 1f);
				return true;
			}
			Debug.DrawLine(vector, array[0].point, Color.red, 1f);
		}
		else
		{
			Debug.DrawLine(vector, vector2, Color.red, 1f);
		}
		return false;
	}

	private void OnDrawGizmosSelected()
	{
		if (gizmoOn)
		{
			Gizmos.color = gizmoConeLimitsColor;
			int num = 1;
			if (base.EntityOwner != null)
			{
				num = ((base.EntityOwner.Status.Orientation == EntityOrientation.Right) ? 1 : (-1));
			}
			Vector2 vector = (Vector2)base.transform.position + sightOffset;
			Vector2 vector2 = Quaternion.Euler(0f, 0f, visionAngle) * Vector2.right * sightDistance * num;
			Vector2 vector3 = vector + vector2;
			Vector2 vector4 = Quaternion.Euler(0f, 0f, 0f - visionAngle) * Vector2.right * sightDistance * num;
			Vector2 vector5 = vector + vector4;
			Gizmos.DrawWireSphere(base.transform.position + (Vector3)sightOffset, closeRadius);
			Gizmos.DrawLine(vector, vector3);
			Gizmos.DrawLine(vector, vector5);
			int num2 = gizmoNumberOfRays;
			Gizmos.color = gizmoRayColor;
			for (int i = 1; i < num2; i++)
			{
				Vector2 vector6 = vector + (Vector2)(Quaternion.Euler(0f, 0f, (float)i * (0f - visionAngle) * 2f / (float)num2) * vector2);
				Gizmos.DrawLine(vector, vector6);
			}
		}
	}
}
