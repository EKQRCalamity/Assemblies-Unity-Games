using System.Collections.Generic;
using UnityEngine;

public class MeshColoringRam : MonoBehaviour
{
	public float height = 0.5f;

	public float threshold = 0.5f;

	public bool autoColor = true;

	public bool newMesh = true;

	public Vector3 oldPosition = Vector3.zero;

	public bool colorMeshLive;

	public LayerMask layer;

	private MeshFilter[] meshFilters;

	private bool colored;

	private static RamSpline[] ramSplines;

	private static LakePolygon[] lakePolygons;

	private void Start()
	{
		if (colorMeshLive)
		{
			if (ramSplines == null)
			{
				ramSplines = Object.FindObjectsOfType<RamSpline>();
			}
			if (lakePolygons == null)
			{
				lakePolygons = Object.FindObjectsOfType<LakePolygon>();
			}
			colored = false;
			meshFilters = base.gameObject.GetComponentsInChildren<MeshFilter>();
		}
	}

	private void Update()
	{
		if (colorMeshLive)
		{
			ColorMeshLive();
		}
	}

	public void ColorMeshLive()
	{
		colored = true;
		Ray ray = default(Ray);
		ray.direction = Vector3.up;
		Vector3 vector = -Vector3.up * (height + threshold);
		Color white = Color.white;
		List<MeshCollider> list = new List<MeshCollider>();
		RamSpline[] array = ramSplines;
		foreach (RamSpline ramSpline in array)
		{
			list.Add(ramSpline.gameObject.AddComponent<MeshCollider>());
		}
		LakePolygon[] array2 = lakePolygons;
		foreach (LakePolygon lakePolygon in array2)
		{
			list.Add(lakePolygon.gameObject.AddComponent<MeshCollider>());
		}
		bool queriesHitBackfaces = Physics.queriesHitBackfaces;
		Physics.queriesHitBackfaces = true;
		MeshFilter[] array3 = meshFilters;
		foreach (MeshFilter meshFilter in array3)
		{
			Mesh mesh = meshFilter.sharedMesh;
			if (!(meshFilter.sharedMesh != null))
			{
				continue;
			}
			if (!colored)
			{
				mesh = (meshFilter.sharedMesh = Object.Instantiate(meshFilter.sharedMesh));
				colored = true;
			}
			int num = mesh.vertices.Length;
			Vector3[] vertices = mesh.vertices;
			Color[] array4 = mesh.colors;
			Transform transform = meshFilter.transform;
			float num2 = float.MaxValue;
			Vector3 origin = vertices[0];
			for (int j = 0; j < num; j++)
			{
				vertices[j] = transform.TransformPoint(vertices[j]) + vector;
				if (vertices[j].y < num2)
				{
					num2 = vertices[j].y;
					origin = vertices[j];
				}
			}
			if (array4.Length == 0)
			{
				array4 = new Color[num];
				for (int k = 0; k < array4.Length; k++)
				{
					array4[k] = white;
				}
			}
			ray.origin = origin;
			num2 = float.MinValue;
			if (Physics.Raycast(ray, out var hitInfo, 100f, layer))
			{
				num2 = hitInfo.point.y;
			}
			for (int l = 0; l < num; l++)
			{
				if (vertices[l].y < num2)
				{
					float num3 = Mathf.Abs(vertices[l].y - num2);
					if (num3 > threshold)
					{
						array4[l].r = 0f;
					}
					else
					{
						array4[l].r = Mathf.Lerp(1f, 0f, num3 / threshold);
					}
				}
				else
				{
					array4[l] = white;
				}
			}
			mesh.colors = array4;
		}
		foreach (MeshCollider item in list)
		{
			Object.Destroy(item);
		}
		Physics.queriesHitBackfaces = queriesHitBackfaces;
	}
}
