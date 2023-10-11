using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RamBuoyancy : MonoBehaviour
{
	public float buoyancy = 30f;

	public float viscosity = 2f;

	public float viscosityAngular = 0.4f;

	public LayerMask layer = 16;

	public Collider collider;

	[Range(2f, 10f)]
	public int pointsInAxis = 2;

	private Rigidbody rigidbody;

	private static RamSpline[] ramSplines;

	private static LakePolygon[] lakePolygons;

	public List<Vector3> volumePoints = new List<Vector3>();

	public bool autoGenerateVolumePoints = true;

	private Vector3[] volumePointsMatrix;

	private Vector3 lowestPoint;

	private Vector3 center = Vector3.zero;

	public bool debug;

	private void Start()
	{
		rigidbody = GetComponent<Rigidbody>();
		if (ramSplines == null)
		{
			ramSplines = Object.FindObjectsOfType<RamSpline>();
		}
		if (lakePolygons == null)
		{
			lakePolygons = Object.FindObjectsOfType<LakePolygon>();
		}
		if (collider == null)
		{
			collider = GetComponent<Collider>();
		}
		if (collider == null)
		{
			Debug.LogError("Buoyancy doesn't have collider");
			base.enabled = false;
			return;
		}
		if (autoGenerateVolumePoints)
		{
			Vector3 size = collider.bounds.size;
			Vector3 min = collider.bounds.min;
			Vector3 vector = new Vector3(size.x / (float)pointsInAxis, size.y / (float)pointsInAxis, size.z / (float)pointsInAxis);
			for (int i = 0; i <= pointsInAxis; i++)
			{
				for (int j = 0; j <= pointsInAxis; j++)
				{
					for (int k = 0; k <= pointsInAxis; k++)
					{
						Vector3 vector2 = new Vector3(min.x + (float)i * vector.x, min.y + (float)j * vector.y, min.z + (float)k * vector.z);
						if (Vector3.Distance(collider.ClosestPoint(vector2), vector2) < float.Epsilon)
						{
							volumePoints.Add(base.transform.InverseTransformPoint(vector2));
						}
					}
				}
			}
		}
		volumePointsMatrix = new Vector3[volumePoints.Count];
	}

	private void FixedUpdate()
	{
		WaterPhysics();
	}

	public void WaterPhysics()
	{
		if (volumePoints.Count == 0)
		{
			Debug.Log("Not initiated Buoyancy");
			return;
		}
		Ray ray = default(Ray);
		ray.direction = Vector3.up;
		bool queriesHitBackfaces = Physics.queriesHitBackfaces;
		Physics.queriesHitBackfaces = true;
		Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
		lowestPoint = volumePoints[0];
		float num = float.MaxValue;
		for (int i = 0; i < volumePoints.Count; i++)
		{
			volumePointsMatrix[i] = localToWorldMatrix.MultiplyPoint3x4(volumePoints[i]);
			if (num > volumePointsMatrix[i].y)
			{
				lowestPoint = volumePointsMatrix[i];
				num = lowestPoint.y;
			}
		}
		ray.origin = lowestPoint;
		center = Vector3.zero;
		if (Physics.Raycast(ray, out var hitInfo, 100f, layer))
		{
			Mathf.Max(collider.bounds.size.x, collider.bounds.size.z);
			int num2 = 0;
			Vector3 velocity = rigidbody.velocity;
			Vector3 normalized = velocity.normalized;
			num = hitInfo.point.y;
			for (int j = 0; j < volumePointsMatrix.Length; j++)
			{
				if (volumePointsMatrix[j].y <= num)
				{
					center += volumePointsMatrix[j];
					num2++;
				}
			}
			center /= (float)num2;
			rigidbody.AddForceAtPosition(Vector3.up * buoyancy * (num - center.y), center);
			rigidbody.AddForce(velocity * -1f * viscosity);
			if (velocity.magnitude > 0.01f)
			{
				Vector3 normalized2 = Vector3.Cross(velocity, new Vector3(1f, 1f, 1f)).normalized;
				_ = Vector3.Cross(velocity, normalized2).normalized;
				Vector3 vector = velocity.normalized * 10f;
				Vector3[] array = volumePointsMatrix;
				foreach (Vector3 vector2 in array)
				{
					Vector3 origin = vector + vector2;
					Ray ray2 = new Ray(origin, -normalized);
					if (collider.Raycast(ray2, out var hitInfo2, 50f))
					{
						Vector3 pointVelocity = rigidbody.GetPointVelocity(hitInfo2.point);
						rigidbody.AddForceAtPosition(-pointVelocity * viscosityAngular, hitInfo2.point);
						if (debug)
						{
							Debug.DrawRay(hitInfo2.point, -pointVelocity * viscosityAngular, Color.red, 0.1f);
						}
					}
				}
			}
			RamSpline component = hitInfo.collider.GetComponent<RamSpline>();
			LakePolygon component2 = hitInfo.collider.GetComponent<LakePolygon>();
			if (component != null)
			{
				Mesh sharedMesh = component.meshfilter.sharedMesh;
				int num3 = sharedMesh.triangles[hitInfo.triangleIndex * 3];
				Vector3 vector3 = component.verticeDirection[num3];
				Vector2 vector4 = sharedMesh.uv4[num3];
				vector3 = vector3 * vector4.y - new Vector3(vector3.z, vector3.y, 0f - vector3.x) * vector4.x;
				rigidbody.AddForce(new Vector3(vector3.x, 0f, vector3.z) * component.floatSpeed);
				if (debug)
				{
					Debug.DrawRay(center, Vector3.up * buoyancy * (num - center.y) * 5f, Color.blue);
				}
				if (debug)
				{
					Debug.DrawRay(base.transform.position, velocity * -1f * viscosity * 5f, Color.magenta);
				}
				if (debug)
				{
					Debug.DrawRay(base.transform.position, velocity * 5f, Color.grey);
				}
				if (debug)
				{
					Debug.DrawRay(base.transform.position, rigidbody.angularVelocity * 5f, Color.black);
				}
			}
			else if (component2 != null)
			{
				Mesh sharedMesh2 = component2.meshfilter.sharedMesh;
				int num4 = sharedMesh2.triangles[hitInfo.triangleIndex * 3];
				Vector2 vector5 = -sharedMesh2.uv4[num4];
				Vector3 vector6 = new Vector3(vector5.x, 0f, vector5.y);
				rigidbody.AddForce(new Vector3(vector6.x, 0f, vector6.z) * component2.floatSpeed);
				if (debug)
				{
					Debug.DrawRay(base.transform.position + Vector3.up, vector6 * 5f, Color.red);
				}
				if (debug)
				{
					Debug.DrawRay(center, Vector3.up * buoyancy * (num - center.y) * 5f, Color.blue);
				}
				if (debug)
				{
					Debug.DrawRay(base.transform.position, velocity * -1f * viscosity * 5f, Color.magenta);
				}
				if (debug)
				{
					Debug.DrawRay(base.transform.position, velocity * 5f, Color.grey);
				}
				if (debug)
				{
					Debug.DrawRay(base.transform.position, rigidbody.angularVelocity * 5f, Color.black);
				}
			}
		}
		Physics.queriesHitBackfaces = queriesHitBackfaces;
	}

	private void OnDrawGizmosSelected()
	{
		if (!debug)
		{
			return;
		}
		if (collider != null && volumePointsMatrix != null)
		{
			_ = base.transform.localToWorldMatrix;
			Vector3[] array = volumePointsMatrix;
			foreach (Vector3 vector in array)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawSphere(vector, 0.08f);
			}
		}
		_ = lowestPoint;
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(lowestPoint, 0.08f);
		_ = center;
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(center, 0.08f);
	}
}
