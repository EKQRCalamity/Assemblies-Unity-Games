using System;
using System.Collections.Generic;
using TriangleNet;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using TriangleNet.Topology;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter))]
public class LakePolygon : MonoBehaviour
{
	public int toolbarInt;

	public LakePolygonProfile currentProfile;

	public LakePolygonProfile oldProfile;

	public List<Vector3> points = new List<Vector3>();

	public List<Vector3> splinePoints = new List<Vector3>();

	public AnimationCurve terrainCarve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(10f, -2f));

	public float distSmooth = 5f;

	public float terrainSmoothMultiplier = 5f;

	public bool overrideLakeRender;

	public float uvScale = 1f;

	public bool receiveShadows;

	public ShadowCastingMode shadowCastingMode;

	public AnimationCurve terrainPaintCarve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public int currentSplatMap = 1;

	public float distanceClearFoliage = 1f;

	public float distanceClearFoliageTrees = 1f;

	public bool mixTwoSplatMaps;

	public int secondSplatMap = 1;

	public bool addCliffSplatMap;

	public int cliffSplatMap = 1;

	public float cliffAngle = 25f;

	public float cliffBlend = 1f;

	public int cliffSplatMapOutside = 1;

	public float cliffAngleOutside = 45f;

	public float cliffBlendOutside = 1f;

	public bool noiseCarve;

	public float noiseMultiplierInside = 1f;

	public float noiseMultiplierOutside = 0.25f;

	public float noiseSizeX = 0.2f;

	public float noiseSizeZ = 0.2f;

	public bool noisePaint;

	public float noiseMultiplierInsidePaint = 1f;

	public float noiseMultiplierOutsidePaint = 0.5f;

	public float noiseSizeXPaint = 0.2f;

	public float noiseSizeZPaint = 0.2f;

	public float maximumTriangleSize = 50f;

	public float traingleDensity = 0.2f;

	public float height;

	public bool lockHeight = true;

	public float yOffset;

	public float trianglesGenerated;

	public float vertsGenerated;

	public UnityEngine.Mesh currentMesh;

	public MeshFilter meshfilter;

	public bool showVertexColors;

	public bool showFlowMap;

	public bool overrideFlowMap;

	public float automaticFlowMapScale = 0.2f;

	public bool noiseflowMap;

	public float noiseMultiplierflowMap = 1f;

	public float noiseSizeXflowMap = 0.2f;

	public float noiseSizeZflowMap = 0.2f;

	public bool drawOnMesh;

	public bool drawOnMeshFlowMap;

	public Color drawColor = Color.black;

	public bool drawColorR = true;

	public bool drawColorG = true;

	public bool drawColorB = true;

	public bool drawColorA = true;

	public bool drawOnMultiple;

	public float opacity = 0.1f;

	public float drawSize = 1f;

	public Material oldMaterial;

	public Color[] colors;

	public List<Vector2> colorsFlowMap = new List<Vector2>();

	public float floatSpeed = 10f;

	public float flowSpeed = 1f;

	public float flowDirection;

	public float closeDistanceSimulation = 5f;

	public int angleSimulation = 5;

	public float checkDistanceSimulation = 50f;

	public bool removeFirstPointSimulation = true;

	public bool normalFromRaycast;

	public bool snapToTerrain;

	public LayerMask snapMask = 1;

	public LakePolygonCarveData lakePolygonCarveData;

	public LakePolygonCarveData lakePolygonPaintData;

	public LakePolygonCarveData lakePolygonClearData;

	public List<GameObject> meshGOs = new List<GameObject>();

	public void AddPoint(Vector3 position)
	{
		points.Add(position);
	}

	public void AddPointAfter(int i)
	{
		Vector3 vector = points[i];
		if (i < points.Count - 1 && points.Count > i + 1)
		{
			Vector3 vector2 = points[i + 1];
			if (Vector3.Distance(vector2, vector) > 0f)
			{
				vector = (vector + vector2) * 0.5f;
			}
			else
			{
				vector.x += 1f;
			}
		}
		else if (points.Count > 1 && i == points.Count - 1)
		{
			Vector3 vector3 = points[i - 1];
			if (Vector3.Distance(vector3, vector) > 0f)
			{
				vector += vector - vector3;
			}
			else
			{
				vector.x += 1f;
			}
		}
		else
		{
			vector.x += 1f;
		}
		points.Insert(i + 1, vector);
	}

	public void ChangePointPosition(int i, Vector3 position)
	{
		points[i] = position;
	}

	public void RemovePoint(int i)
	{
		if (i < points.Count)
		{
			points.RemoveAt(i);
		}
	}

	public void RemovePoints(int fromID = -1)
	{
		for (int num = points.Count - 1; num > fromID; num--)
		{
			RemovePoint(num);
		}
	}

	private void CenterPivot()
	{
		_ = base.transform.position;
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < points.Count; i++)
		{
			zero += points[i];
		}
		zero /= (float)points.Count;
		for (int j = 0; j < points.Count; j++)
		{
			Vector3 value = points[j];
			value.x -= zero.x;
			value.y -= zero.y;
			value.z -= zero.z;
			points[j] = value;
		}
		base.transform.position += zero;
	}

	public void GeneratePolygon(bool quick = false)
	{
		MeshRenderer component = base.gameObject.GetComponent<MeshRenderer>();
		if (component != null)
		{
			component.receiveShadows = receiveShadows;
			component.shadowCastingMode = shadowCastingMode;
		}
		if (lockHeight)
		{
			for (int i = 1; i < points.Count; i++)
			{
				Vector3 value = points[i];
				value.y = points[0].y;
				points[i] = value;
			}
		}
		if (points.Count < 3)
		{
			return;
		}
		CenterPivot();
		splinePoints.Clear();
		for (int j = 0; j < points.Count; j++)
		{
			CalculateCatmullRomSpline(j);
		}
		List<Vector3> list = new List<Vector3>();
		List<Vector3> list2 = new List<Vector3>();
		List<int> list3 = new List<int>();
		list.AddRange(splinePoints.ToArray());
		Polygon polygon = new Polygon();
		List<Vertex> list4 = new List<Vertex>();
		for (int k = 0; k < list.Count; k++)
		{
			Vertex vertex = new Vertex(list[k].x, list[k].z);
			vertex.z = list[k].y;
			list4.Add(vertex);
		}
		polygon.Add(new Contour(list4));
		ConstraintOptions options = new ConstraintOptions
		{
			ConformingDelaunay = true
		};
		QualityOptions quality = new QualityOptions
		{
			MinimumAngle = 90.0,
			MaximumArea = maximumTriangleSize
		};
		TriangleNet.Mesh mesh = (TriangleNet.Mesh)polygon.Triangulate(options, quality);
		polygon.Triangulate(options, quality);
		list3.Clear();
		foreach (TriangleNet.Topology.Triangle triangle in mesh.triangles)
		{
			Vertex vertex2 = mesh.vertices[triangle.vertices[2].id];
			Vector3 item = new Vector3((float)vertex2.x, (float)vertex2.z, (float)vertex2.y);
			vertex2 = mesh.vertices[triangle.vertices[1].id];
			Vector3 item2 = new Vector3((float)vertex2.x, (float)vertex2.z, (float)vertex2.y);
			vertex2 = mesh.vertices[triangle.vertices[0].id];
			Vector3 item3 = new Vector3((float)vertex2.x, (float)vertex2.z, (float)vertex2.y);
			list3.Add(list2.Count);
			list3.Add(list2.Count + 1);
			list3.Add(list2.Count + 2);
			list2.Add(item);
			list2.Add(item2);
			list2.Add(item3);
		}
		Vector3[] array = list2.ToArray();
		int num = array.Length;
		Vector3[] array2 = new Vector3[num];
		Vector2[] array3 = new Vector2[num];
		colors = new Color[num];
		for (int l = 0; l < num; l++)
		{
			if (Physics.Raycast(array[l] + base.transform.position + Vector3.up * 10f, Vector3.down, out var hitInfo, 1000f, snapMask.value) && snapToTerrain)
			{
				array[l] = hitInfo.point - base.transform.position + new Vector3(0f, 0.1f, 0f);
			}
			array[l].y += yOffset;
			if (normalFromRaycast)
			{
				array2[l] = hitInfo.normal;
			}
			else
			{
				array2[l] = Vector3.up;
			}
			array3[l] = new Vector2(array[l].x, array[l].z) * 0.01f * uvScale;
			colors[l] = Color.black;
		}
		if (overrideFlowMap || quick)
		{
			while (colorsFlowMap.Count < num)
			{
				colorsFlowMap.Add(new Vector2(0f, 1f));
			}
			while (colorsFlowMap.Count > num)
			{
				colorsFlowMap.RemoveAt(colorsFlowMap.Count - 1);
			}
		}
		else
		{
			List<Vector2> list5 = new List<Vector2>();
			List<Vector2> list6 = new List<Vector2>();
			for (int m = 0; m < splinePoints.Count; m++)
			{
				list5.Add(new Vector2(splinePoints[m].x, splinePoints[m].z));
			}
			for (int n = 0; n < array.Length; n++)
			{
				list6.Add(new Vector2(array[n].x, array[n].z));
			}
			colorsFlowMap.Clear();
			Vector2 zero = Vector2.zero;
			for (int num2 = 0; num2 < num; num2++)
			{
				float num3 = float.MaxValue;
				Vector2 vector = list6[num2];
				for (int num4 = 0; num4 < splinePoints.Count; num4++)
				{
					int index = num4;
					int index2 = (num4 + 1) % list5.Count;
					Vector2 pointProject;
					float num5 = DistancePointLine(list6[num2], list5[index], list5[index2], out pointProject);
					if (num3 > num5)
					{
						num3 = num5;
						vector = pointProject;
					}
				}
				zero = -(vector - list6[num2]).normalized * (automaticFlowMapScale + (noiseflowMap ? (Mathf.PerlinNoise(list6[num2].x * noiseSizeXflowMap, list6[num2].y * noiseSizeZflowMap) * noiseMultiplierflowMap - noiseMultiplierflowMap * 0.5f) : 0f));
				colorsFlowMap.Add(zero);
			}
		}
		currentMesh = new UnityEngine.Mesh();
		vertsGenerated = num;
		if (num > 65000)
		{
			currentMesh.indexFormat = IndexFormat.UInt32;
		}
		currentMesh.vertices = array;
		currentMesh.subMeshCount = 1;
		currentMesh.SetTriangles(list3, 0);
		currentMesh.uv = array3;
		currentMesh.uv4 = colorsFlowMap.ToArray();
		currentMesh.normals = array2;
		currentMesh.colors = colors;
		currentMesh.RecalculateTangents();
		currentMesh.RecalculateBounds();
		currentMesh.RecalculateTangents();
		currentMesh.RecalculateBounds();
		trianglesGenerated = list3.Count / 3;
		meshfilter = GetComponent<MeshFilter>();
		meshfilter.sharedMesh = currentMesh;
		MeshCollider component2 = GetComponent<MeshCollider>();
		if (component2 != null)
		{
			component2.sharedMesh = currentMesh;
		}
	}

	public static LakePolygon CreatePolygon(Material material, List<Vector3> positions = null)
	{
		GameObject obj = new GameObject("Lake Polygon")
		{
			layer = LayerMask.NameToLayer("Water")
		};
		LakePolygon lakePolygon = obj.AddComponent<LakePolygon>();
		MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
		meshRenderer.receiveShadows = false;
		meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
		if (material != null)
		{
			meshRenderer.sharedMaterial = material;
		}
		if (positions != null)
		{
			for (int i = 0; i < positions.Count; i++)
			{
				lakePolygon.AddPoint(positions[i]);
			}
		}
		return lakePolygon;
	}

	private void CalculateCatmullRomSpline(int pos)
	{
		Vector3 p = points[ClampListPos(pos - 1)];
		Vector3 p2 = points[pos];
		Vector3 p3 = points[ClampListPos(pos + 1)];
		Vector3 p4 = points[ClampListPos(pos + 2)];
		int num = Mathf.FloorToInt(1f / traingleDensity);
		for (int i = 1; i <= num; i++)
		{
			float t = (float)i * traingleDensity;
			splinePoints.Add(GetCatmullRomPosition(t, p, p2, p3, p4));
		}
	}

	public int ClampListPos(int pos)
	{
		if (pos < 0)
		{
			pos = points.Count - 1;
		}
		if (pos > points.Count)
		{
			pos = 1;
		}
		else if (pos > points.Count - 1)
		{
			pos = 0;
		}
		return pos;
	}

	private Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		Vector3 vector = 2f * p1;
		Vector3 vector2 = p2 - p0;
		Vector3 vector3 = 2f * p0 - 5f * p1 + 4f * p2 - p3;
		Vector3 vector4 = -p0 + 3f * p1 - 3f * p2 + p3;
		return 0.5f * (vector + vector2 * t + vector3 * t * t + vector4 * t * t * t);
	}

	public float DistancePointLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd, out Vector2 pointProject)
	{
		pointProject = ProjectPointLine(point, lineStart, lineEnd);
		return Vector2.Distance(pointProject, point);
	}

	public Vector2 ProjectPointLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
	{
		Vector2 rhs = point - lineStart;
		Vector2 vector = lineEnd - lineStart;
		float magnitude = vector.magnitude;
		Vector2 vector2 = vector;
		if (magnitude > 1E-06f)
		{
			vector2 /= magnitude;
		}
		float num = Mathf.Clamp(Vector2.Dot(vector2, rhs), 0f, magnitude);
		return lineStart + vector2 * num;
	}

	public void TerrainCarve(bool terrainShow = false)
	{
		Terrain[] activeTerrains = Terrain.activeTerrains;
		Physics.autoSyncTransforms = false;
		if (meshGOs != null && meshGOs.Count > 0)
		{
			foreach (GameObject meshGO in meshGOs)
			{
				UnityEngine.Object.DestroyImmediate(meshGO);
			}
			meshGOs.Clear();
		}
		Terrain[] array = activeTerrains;
		foreach (Terrain terrain in array)
		{
			TerrainData terrainData = terrain.terrainData;
			float y = base.transform.position.y;
			float y2 = terrain.transform.position.y;
			float x = terrain.terrainData.size.x;
			float y3 = terrain.terrainData.size.y;
			float z = terrain.terrainData.size.z;
			float[,] heights;
			if (lakePolygonCarveData == null || distSmooth != lakePolygonCarveData.distSmooth)
			{
				float num = float.MaxValue;
				float num2 = float.MinValue;
				float num3 = float.MaxValue;
				float num4 = float.MinValue;
				for (int j = 0; j < splinePoints.Count; j++)
				{
					Vector3 vector = base.transform.TransformPoint(splinePoints[j]);
					if (num > vector.x)
					{
						num = vector.x;
					}
					if (num2 < vector.x)
					{
						num2 = vector.x;
					}
					if (num3 > vector.z)
					{
						num3 = vector.z;
					}
					if (num4 < vector.z)
					{
						num4 = vector.z;
					}
				}
				float num5 = 1f / z * (float)(terrainData.heightmapResolution - 1);
				float num6 = 1f / x * (float)(terrainData.heightmapResolution - 1);
				num -= terrain.transform.position.x + distSmooth;
				num2 -= terrain.transform.position.x - distSmooth;
				num3 -= terrain.transform.position.z + distSmooth;
				num4 -= terrain.transform.position.z - distSmooth;
				num *= num6;
				num2 *= num6;
				num3 *= num5;
				num4 *= num5;
				num = (int)Mathf.Clamp(num, 0f, terrainData.heightmapResolution);
				num2 = (int)Mathf.Clamp(num2, 0f, terrainData.heightmapResolution);
				num3 = (int)Mathf.Clamp(num3, 0f, terrainData.heightmapResolution);
				num4 = (int)Mathf.Clamp(num4, 0f, terrainData.heightmapResolution);
				heights = terrainData.GetHeights((int)num, (int)num3, (int)(num2 - num), (int)(num4 - num3));
				Vector4[,] array2 = new Vector4[heights.GetLength(0), heights.GetLength(1)];
				MeshCollider meshCollider = base.gameObject.AddComponent<MeshCollider>();
				Transform transform = terrain.transform;
				Vector3 zero = Vector3.zero;
				zero.y = y;
				for (int k = 0; k < heights.GetLength(0); k++)
				{
					for (int l = 0; l < heights.GetLength(1); l++)
					{
						zero.x = ((float)l + num) / num6 + transform.position.x;
						zero.z = ((float)k + num3) / num5 + transform.position.z;
						Ray ray = new Ray(zero + Vector3.up * 1000f, Vector3.down);
						if (meshCollider.Raycast(ray, out var hitInfo, 10000f))
						{
							float num7 = float.MaxValue;
							for (int m = 0; m < splinePoints.Count; m++)
							{
								int index = m;
								int index2 = (m + 1) % splinePoints.Count;
								float num8 = DistancePointLine(hitInfo.point, base.transform.TransformPoint(splinePoints[index]), base.transform.TransformPoint(splinePoints[index2]));
								if (num7 > num8)
								{
									num7 = num8;
								}
							}
							array2[k, l] = new Vector3(hitInfo.point.x, num7, hitInfo.point.z);
							continue;
						}
						float num9 = float.MaxValue;
						for (int n = 0; n < splinePoints.Count; n++)
						{
							int index3 = n;
							int index4 = (n + 1) % splinePoints.Count;
							float num10 = DistancePointLine(zero, base.transform.TransformPoint(splinePoints[index3]), base.transform.TransformPoint(splinePoints[index4]));
							if (num9 > num10)
							{
								num9 = num10;
							}
						}
						array2[k, l] = new Vector3(zero.x, 0f - num9, zero.z);
					}
				}
				UnityEngine.Object.DestroyImmediate(meshCollider);
				lakePolygonCarveData = new LakePolygonCarveData();
				lakePolygonCarveData.minX = num;
				lakePolygonCarveData.maxX = num2;
				lakePolygonCarveData.minZ = num3;
				lakePolygonCarveData.maxZ = num4;
				lakePolygonCarveData.distances = array2;
			}
			heights = terrainData.GetHeights((int)lakePolygonCarveData.minX, (int)lakePolygonCarveData.minZ, (int)(lakePolygonCarveData.maxX - lakePolygonCarveData.minX), (int)(lakePolygonCarveData.maxZ - lakePolygonCarveData.minZ));
			float num11 = 0f;
			List<List<Vector4>> list = new List<List<Vector4>>();
			for (int num12 = 0; num12 < heights.GetLength(0); num12++)
			{
				List<Vector4> list2 = new List<Vector4>();
				for (int num13 = 0; num13 < heights.GetLength(1); num13++)
				{
					Vector3 vector2 = lakePolygonCarveData.distances[num12, num13];
					if (vector2.y > 0f)
					{
						num11 = ((!noiseCarve) ? 0f : (Mathf.PerlinNoise((float)num12 * noiseSizeX, (float)num13 * noiseSizeZ) * noiseMultiplierInside - noiseMultiplierInside * 0.5f));
						float y4 = vector2.y;
						heights[num12, num13] = (num11 + y + terrainCarve.Evaluate(y4) - y2) / y3;
						list2.Add(new Vector4(vector2.x, heights[num12, num13] * y3 + y2, vector2.z, 1f));
					}
					else if (0f - vector2.y <= distSmooth)
					{
						num11 = ((!noiseCarve) ? 0f : (Mathf.PerlinNoise((float)num12 * noiseSizeX, (float)num13 * noiseSizeZ) * noiseMultiplierOutside - noiseMultiplierOutside * 0.5f));
						float b = heights[num12, num13] * y3 + y2;
						float a = y + terrainCarve.Evaluate(vector2.y);
						float f = (0f - vector2.y) / distSmooth;
						f = Mathf.Pow(f, terrainSmoothMultiplier);
						a = num11 + Mathf.Lerp(a, b, f) - y2;
						heights[num12, num13] = a / y3;
						list2.Add(new Vector4(vector2.x, heights[num12, num13] * y3 + y2, vector2.z, Mathf.Pow(1f + vector2.y / distSmooth, 0.5f)));
					}
					else
					{
						list2.Add(new Vector4(vector2.x, heights[num12, num13] * y3 + y2, vector2.z, 0f));
					}
				}
				list.Add(list2);
			}
			if (terrainShow)
			{
				UnityEngine.Mesh mesh = new UnityEngine.Mesh();
				mesh.indexFormat = IndexFormat.UInt32;
				List<Vector3> list3 = new List<Vector3>();
				List<int> list4 = new List<int>();
				List<Color> list5 = new List<Color>();
				foreach (List<Vector4> item in list)
				{
					foreach (Vector4 item2 in item)
					{
						list3.Add(item2);
						list5.Add(new Color(item2.w, item2.w, item2.w, item2.w));
					}
				}
				for (int num14 = 0; num14 < list.Count - 1; num14++)
				{
					List<Vector4> list6 = list[num14];
					for (int num15 = 0; num15 < list6.Count - 1; num15++)
					{
						list4.Add(num15 + num14 * list6.Count);
						list4.Add(num15 + (num14 + 1) * list6.Count);
						list4.Add(num15 + 1 + num14 * list6.Count);
						list4.Add(num15 + 1 + num14 * list6.Count);
						list4.Add(num15 + (num14 + 1) * list6.Count);
						list4.Add(num15 + 1 + (num14 + 1) * list6.Count);
					}
				}
				mesh.SetVertices(list3);
				mesh.SetTriangles(list4, 0);
				mesh.SetColors(list5);
				mesh.RecalculateNormals();
				mesh.RecalculateTangents();
				mesh.RecalculateBounds();
				GameObject gameObject = new GameObject("TerrainMesh");
				gameObject.transform.parent = base.transform;
				gameObject.AddComponent<MeshFilter>();
				MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
				meshRenderer.sharedMaterial = new Material(Shader.Find("Debug Terrain Carve"));
				meshRenderer.sharedMaterial.color = new Color(0f, 0.5f, 0f);
				gameObject.transform.position = Vector3.zero;
				gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
				if (overrideLakeRender)
				{
					gameObject.GetComponent<MeshRenderer>().sharedMaterial.renderQueue = 5000;
				}
				else
				{
					gameObject.GetComponent<MeshRenderer>().sharedMaterial.renderQueue = 2980;
				}
				meshGOs.Add(gameObject);
				continue;
			}
			if (meshGOs != null && meshGOs.Count > 0)
			{
				foreach (GameObject meshGO2 in meshGOs)
				{
					UnityEngine.Object.DestroyImmediate(meshGO2);
				}
				meshGOs.Clear();
			}
			terrainData.SetHeights((int)lakePolygonCarveData.minX, (int)lakePolygonCarveData.minZ, heights);
			terrain.Flush();
			lakePolygonCarveData = null;
		}
		Physics.autoSyncTransforms = true;
	}

	public void TerrainPaint(bool terrainShow = false)
	{
		Terrain[] activeTerrains = Terrain.activeTerrains;
		Physics.autoSyncTransforms = false;
		if (meshGOs != null && meshGOs.Count > 0)
		{
			foreach (GameObject meshGO in meshGOs)
			{
				UnityEngine.Object.DestroyImmediate(meshGO);
			}
			meshGOs.Clear();
		}
		float num = distSmooth;
		float num2 = float.MaxValue;
		Keyframe[] keys = terrainPaintCarve.keys;
		for (int i = 0; i < keys.Length; i++)
		{
			Keyframe keyframe = keys[i];
			if (keyframe.time < num2)
			{
				num2 = keyframe.time;
			}
		}
		if (num2 < 0f)
		{
			num = 0f - num2;
		}
		Terrain[] array = activeTerrains;
		foreach (Terrain terrain in array)
		{
			TerrainData terrainData = terrain.terrainData;
			float y = base.transform.position.y;
			float x = terrain.terrainData.size.x;
			float z = terrain.terrainData.size.z;
			float[,,] alphamaps;
			if (lakePolygonPaintData == null || num != lakePolygonPaintData.distSmooth)
			{
				float num3 = float.MaxValue;
				float num4 = float.MinValue;
				float num5 = float.MaxValue;
				float num6 = float.MinValue;
				for (int j = 0; j < splinePoints.Count; j++)
				{
					Vector3 vector = base.transform.TransformPoint(splinePoints[j]);
					if (num3 > vector.x)
					{
						num3 = vector.x;
					}
					if (num4 < vector.x)
					{
						num4 = vector.x;
					}
					if (num5 > vector.z)
					{
						num5 = vector.z;
					}
					if (num6 < vector.z)
					{
						num6 = vector.z;
					}
				}
				float num7 = 1f / z * (float)(terrainData.alphamapWidth - 1);
				float num8 = 1f / x * (float)(terrainData.alphamapHeight - 1);
				Debug.Log(num7 + " " + num8);
				num3 -= terrain.transform.position.x + num;
				num4 -= terrain.transform.position.x - num;
				num5 -= terrain.transform.position.z + num;
				num6 -= terrain.transform.position.z - num;
				num3 *= num8;
				num4 *= num8;
				num5 *= num7;
				num6 *= num7;
				num3 = (int)Mathf.Clamp(num3, 0f, terrainData.alphamapWidth);
				num4 = (int)Mathf.Clamp(num4, 0f, terrainData.alphamapWidth);
				num5 = (int)Mathf.Clamp(num5, 0f, terrainData.alphamapHeight);
				num6 = (int)Mathf.Clamp(num6, 0f, terrainData.alphamapHeight);
				alphamaps = terrainData.GetAlphamaps((int)num3, (int)num5, (int)(num4 - num3), (int)(num6 - num5));
				Vector4[,] array2 = new Vector4[alphamaps.GetLength(0), alphamaps.GetLength(1)];
				MeshCollider meshCollider = base.gameObject.AddComponent<MeshCollider>();
				Transform transform = terrain.transform;
				Vector3 zero = Vector3.zero;
				zero.y = y;
				for (int k = 0; k < alphamaps.GetLength(0); k++)
				{
					for (int l = 0; l < alphamaps.GetLength(1); l++)
					{
						zero.x = ((float)l + num3) / num8 + transform.position.x;
						zero.z = ((float)k + num5) / num7 + transform.position.z;
						Ray ray = new Ray(zero + Vector3.up * 1000f, Vector3.down);
						if (meshCollider.Raycast(ray, out var hitInfo, 10000f))
						{
							float num9 = float.MaxValue;
							for (int m = 0; m < splinePoints.Count; m++)
							{
								int index = m;
								int index2 = (m + 1) % splinePoints.Count;
								float num10 = DistancePointLine(hitInfo.point, base.transform.TransformPoint(splinePoints[index]), base.transform.TransformPoint(splinePoints[index2]));
								if (num9 > num10)
								{
									num9 = num10;
								}
							}
							float w = 0f;
							if (addCliffSplatMap)
							{
								ray = new Ray(zero + Vector3.up * 1000f, Vector3.down);
								if (terrain.GetComponent<TerrainCollider>().Raycast(ray, out hitInfo, 10000f))
								{
									w = Vector3.Angle(hitInfo.normal, Vector3.up);
								}
							}
							array2[k, l] = new Vector4(hitInfo.point.x, num9, hitInfo.point.z, w);
							continue;
						}
						float num11 = float.MaxValue;
						for (int n = 0; n < splinePoints.Count; n++)
						{
							int index3 = n;
							int index4 = (n + 1) % splinePoints.Count;
							float num12 = DistancePointLine(zero, base.transform.TransformPoint(splinePoints[index3]), base.transform.TransformPoint(splinePoints[index4]));
							if (num11 > num12)
							{
								num11 = num12;
							}
						}
						float w2 = 0f;
						if (addCliffSplatMap)
						{
							ray = new Ray(zero + Vector3.up * 1000f, Vector3.down);
							if (terrain.GetComponent<TerrainCollider>().Raycast(ray, out hitInfo, 10000f))
							{
								w2 = Vector3.Angle(hitInfo.normal, Vector3.up);
							}
						}
						array2[k, l] = new Vector4(zero.x, 0f - num11, zero.z, w2);
					}
				}
				UnityEngine.Object.DestroyImmediate(meshCollider);
				lakePolygonPaintData = new LakePolygonCarveData();
				lakePolygonPaintData.minX = num3;
				lakePolygonPaintData.maxX = num4;
				lakePolygonPaintData.minZ = num5;
				lakePolygonPaintData.maxZ = num6;
				lakePolygonPaintData.distances = array2;
			}
			alphamaps = terrainData.GetAlphamaps((int)lakePolygonPaintData.minX, (int)lakePolygonPaintData.minZ, (int)(lakePolygonPaintData.maxX - lakePolygonPaintData.minX), (int)(lakePolygonPaintData.maxZ - lakePolygonPaintData.minZ));
			float num13 = 0f;
			new List<List<Vector4>>();
			for (int num14 = 0; num14 < alphamaps.GetLength(0); num14++)
			{
				new List<Vector4>();
				for (int num15 = 0; num15 < alphamaps.GetLength(1); num15++)
				{
					Vector4 vector2 = lakePolygonPaintData.distances[num14, num15];
					if (!(0f - vector2.y <= num) && !(vector2.y > 0f))
					{
						continue;
					}
					if (!mixTwoSplatMaps)
					{
						num13 = ((!noisePaint) ? 0f : ((!(vector2.y > 0f)) ? (Mathf.PerlinNoise((float)num14 * noiseSizeXPaint, (float)num15 * noiseSizeZPaint) * noiseMultiplierOutsidePaint - noiseMultiplierOutsidePaint * 0.5f) : (Mathf.PerlinNoise((float)num14 * noiseSizeXPaint, (float)num15 * noiseSizeZPaint) * noiseMultiplierInsidePaint - noiseMultiplierInsidePaint * 0.5f)));
						float num16 = alphamaps[num14, num15, currentSplatMap];
						alphamaps[num14, num15, currentSplatMap] = Mathf.Clamp01(Mathf.Lerp(alphamaps[num14, num15, currentSplatMap], 1f, terrainPaintCarve.Evaluate(vector2.y) + num13 * terrainPaintCarve.Evaluate(vector2.y)));
						for (int num17 = 0; num17 < terrainData.terrainLayers.Length; num17++)
						{
							if (num17 != currentSplatMap)
							{
								alphamaps[num14, num15, num17] = ((num16 == 1f) ? 0f : Mathf.Clamp01(alphamaps[num14, num15, num17] * ((1f - alphamaps[num14, num15, currentSplatMap]) / (1f - num16))));
							}
						}
					}
					else
					{
						num13 = ((!(vector2.y > 0f)) ? (Mathf.PerlinNoise((float)num14 * noiseSizeXPaint, (float)num15 * noiseSizeZPaint) * noiseMultiplierOutsidePaint - noiseMultiplierOutsidePaint * 0.5f) : (Mathf.PerlinNoise((float)num14 * noiseSizeXPaint, (float)num15 * noiseSizeZPaint) * noiseMultiplierInsidePaint - noiseMultiplierInsidePaint * 0.5f));
						float num18 = alphamaps[num14, num15, currentSplatMap];
						alphamaps[num14, num15, currentSplatMap] = Mathf.Clamp01(Mathf.Lerp(alphamaps[num14, num15, currentSplatMap], 1f, terrainPaintCarve.Evaluate(vector2.y)));
						for (int num19 = 0; num19 < terrainData.terrainLayers.Length; num19++)
						{
							if (num19 != currentSplatMap)
							{
								alphamaps[num14, num15, num19] = ((num18 == 1f) ? 0f : Mathf.Clamp01(alphamaps[num14, num15, num19] * ((1f - alphamaps[num14, num15, currentSplatMap]) / (1f - num18))));
							}
						}
						if (num13 > 0f)
						{
							num18 = alphamaps[num14, num15, secondSplatMap];
							alphamaps[num14, num15, secondSplatMap] = Mathf.Clamp01(Mathf.Lerp(alphamaps[num14, num15, secondSplatMap], 1f, num13));
							for (int num20 = 0; num20 < terrainData.terrainLayers.Length; num20++)
							{
								if (num20 != secondSplatMap)
								{
									alphamaps[num14, num15, num20] = ((num18 == 1f) ? 0f : Mathf.Clamp01(alphamaps[num14, num15, num20] * ((1f - alphamaps[num14, num15, secondSplatMap]) / (1f - num18))));
								}
							}
						}
					}
					if (!addCliffSplatMap)
					{
						continue;
					}
					float num21 = alphamaps[num14, num15, cliffSplatMap];
					if (vector2.y > 0f)
					{
						if (!(vector2.w > cliffAngle))
						{
							continue;
						}
						alphamaps[num14, num15, cliffSplatMap] = cliffBlend;
						for (int num22 = 0; num22 < terrainData.terrainLayers.Length; num22++)
						{
							if (num22 != cliffSplatMap)
							{
								alphamaps[num14, num15, num22] = ((num21 == 1f) ? 0f : Mathf.Clamp01(alphamaps[num14, num15, num22] * ((1f - alphamaps[num14, num15, cliffSplatMap]) / (1f - num21))));
							}
						}
					}
					else
					{
						if (!(vector2.w > cliffAngleOutside))
						{
							continue;
						}
						alphamaps[num14, num15, cliffSplatMapOutside] = cliffBlendOutside;
						for (int num23 = 0; num23 < terrainData.terrainLayers.Length; num23++)
						{
							if (num23 != cliffSplatMapOutside)
							{
								alphamaps[num14, num15, num23] = ((num21 == 1f) ? 0f : Mathf.Clamp01(alphamaps[num14, num15, num23] * ((1f - alphamaps[num14, num15, cliffSplatMapOutside]) / (1f - num21))));
							}
						}
					}
				}
			}
			if (meshGOs != null && meshGOs.Count > 0)
			{
				foreach (GameObject meshGO2 in meshGOs)
				{
					UnityEngine.Object.DestroyImmediate(meshGO2);
				}
				meshGOs.Clear();
			}
			terrainData.SetAlphamaps((int)lakePolygonPaintData.minX, (int)lakePolygonPaintData.minZ, alphamaps);
			terrain.Flush();
			lakePolygonPaintData = null;
		}
		Physics.autoSyncTransforms = true;
	}

	public void TerrainClearTrees(bool details = true)
	{
		Terrain[] activeTerrains = Terrain.activeTerrains;
		Physics.autoSyncTransforms = false;
		if (meshGOs != null && meshGOs.Count > 0)
		{
			foreach (GameObject meshGO in meshGOs)
			{
				UnityEngine.Object.DestroyImmediate(meshGO);
			}
			meshGOs.Clear();
		}
		Terrain[] array = activeTerrains;
		foreach (Terrain terrain in array)
		{
			TerrainData terrainData = terrain.terrainData;
			Transform transform = terrain.transform;
			float y = base.transform.position.y;
			_ = terrain.transform.position;
			float x = terrain.terrainData.size.x;
			_ = terrain.terrainData.size;
			float z = terrain.terrainData.size.z;
			float num = float.MaxValue;
			float num2 = float.MinValue;
			float num3 = float.MaxValue;
			float num4 = float.MinValue;
			for (int j = 0; j < splinePoints.Count; j++)
			{
				Vector3 vector = base.transform.TransformPoint(splinePoints[j]);
				if (num > vector.x)
				{
					num = vector.x;
				}
				if (num2 < vector.x)
				{
					num2 = vector.x;
				}
				if (num3 > vector.z)
				{
					num3 = vector.z;
				}
				if (num4 < vector.z)
				{
					num4 = vector.z;
				}
			}
			float num5 = 1f / z * (float)(terrainData.detailWidth - 1);
			float num6 = 1f / x * (float)(terrainData.detailHeight - 1);
			num -= terrain.transform.position.x + distanceClearFoliage;
			num2 -= terrain.transform.position.x - distanceClearFoliage;
			num3 -= terrain.transform.position.z + distanceClearFoliage;
			num4 -= terrain.transform.position.z - distanceClearFoliage;
			num *= num6;
			num2 *= num6;
			num3 *= num5;
			num4 *= num5;
			num = (int)Mathf.Clamp(num, 0f, terrainData.detailWidth);
			num2 = (int)Mathf.Clamp(num2, 0f, terrainData.detailWidth);
			num3 = (int)Mathf.Clamp(num3, 0f, terrainData.detailHeight);
			num4 = (int)Mathf.Clamp(num4, 0f, terrainData.detailHeight);
			int[,] detailLayer = terrainData.GetDetailLayer((int)num, (int)num3, (int)(num2 - num), (int)(num4 - num3), 0);
			Vector4[,] array2 = new Vector4[detailLayer.GetLength(0), detailLayer.GetLength(1)];
			MeshCollider meshCollider = base.gameObject.AddComponent<MeshCollider>();
			Vector3 zero = Vector3.zero;
			zero.y = y;
			for (int k = 0; k < detailLayer.GetLength(0); k++)
			{
				for (int l = 0; l < detailLayer.GetLength(1); l++)
				{
					zero.x = ((float)l + num) / num6 + transform.position.x;
					zero.z = ((float)k + num3) / num5 + transform.position.z;
					Ray ray = new Ray(zero + Vector3.up * 1000f, Vector3.down);
					if (meshCollider.Raycast(ray, out var hitInfo, 10000f))
					{
						float num7 = float.MaxValue;
						for (int m = 0; m < splinePoints.Count; m++)
						{
							int index = m;
							int index2 = (m + 1) % splinePoints.Count;
							float num8 = DistancePointLine(hitInfo.point, base.transform.TransformPoint(splinePoints[index]), base.transform.TransformPoint(splinePoints[index2]));
							if (num7 > num8)
							{
								num7 = num8;
							}
						}
						float w = 0f;
						array2[k, l] = new Vector4(hitInfo.point.x, num7, hitInfo.point.z, w);
						continue;
					}
					float num9 = float.MaxValue;
					for (int n = 0; n < splinePoints.Count; n++)
					{
						int index3 = n;
						int index4 = (n + 1) % splinePoints.Count;
						float num10 = DistancePointLine(zero, base.transform.TransformPoint(splinePoints[index3]), base.transform.TransformPoint(splinePoints[index4]));
						if (num9 > num10)
						{
							num9 = num10;
						}
					}
					float w2 = 0f;
					array2[k, l] = new Vector4(zero.x, 0f - num9, zero.z, w2);
				}
			}
			if (!details)
			{
				List<TreeInstance> list = new List<TreeInstance>();
				TreeInstance[] treeInstances = terrainData.treeInstances;
				zero.y = y;
				TreeInstance[] array3 = treeInstances;
				for (int num11 = 0; num11 < array3.Length; num11++)
				{
					TreeInstance item = array3[num11];
					zero.x = item.position.x * x + transform.position.x;
					zero.z = item.position.z * z + transform.position.z;
					Ray ray2 = new Ray(zero + Vector3.up * 1000f, Vector3.down);
					if (meshCollider.Raycast(ray2, out var _, 10000f))
					{
						continue;
					}
					float num12 = float.MaxValue;
					for (int num13 = 0; num13 < splinePoints.Count; num13++)
					{
						int index5 = num13;
						int index6 = (num13 + 1) % splinePoints.Count;
						float num14 = DistancePointLine(zero, base.transform.TransformPoint(splinePoints[index5]), base.transform.TransformPoint(splinePoints[index6]));
						if (num12 > num14)
						{
							num12 = num14;
						}
					}
					if (num12 > distanceClearFoliageTrees)
					{
						list.Add(item);
					}
				}
				terrainData.treeInstances = list.ToArray();
				UnityEngine.Object.DestroyImmediate(meshCollider);
			}
			lakePolygonClearData = new LakePolygonCarveData();
			lakePolygonClearData.minX = num;
			lakePolygonClearData.maxX = num2;
			lakePolygonClearData.minZ = num3;
			lakePolygonClearData.maxZ = num4;
			lakePolygonClearData.distances = array2;
			if (details)
			{
				for (int num15 = 0; num15 < terrainData.detailPrototypes.Length; num15++)
				{
					detailLayer = terrainData.GetDetailLayer((int)lakePolygonClearData.minX, (int)lakePolygonClearData.minZ, (int)(lakePolygonClearData.maxX - lakePolygonClearData.minX), (int)(lakePolygonClearData.maxZ - lakePolygonClearData.minZ), num15);
					new List<List<Vector4>>();
					for (int num16 = 0; num16 < detailLayer.GetLength(0); num16++)
					{
						new List<Vector4>();
						for (int num17 = 0; num17 < detailLayer.GetLength(1); num17++)
						{
							Vector4 vector2 = lakePolygonClearData.distances[num16, num17];
							if (0f - vector2.y <= distanceClearFoliage || vector2.y > 0f)
							{
								_ = detailLayer[num16, num17];
								detailLayer[num16, num17] = 0;
							}
						}
					}
					terrainData.SetDetailLayer((int)lakePolygonClearData.minX, (int)lakePolygonClearData.minZ, num15, detailLayer);
				}
			}
			if (meshGOs != null && meshGOs.Count > 0)
			{
				foreach (GameObject meshGO2 in meshGOs)
				{
					UnityEngine.Object.DestroyImmediate(meshGO2);
				}
				meshGOs.Clear();
			}
			terrain.Flush();
			lakePolygonClearData = null;
		}
		Physics.autoSyncTransforms = true;
	}

	public void Simulation()
	{
		List<Vector3> list = new List<Vector3>();
		list.Add(base.transform.TransformPoint(points[0]));
		int num = 1;
		for (int i = 0; i < num; i++)
		{
			List<Vector3> list2 = new List<Vector3>();
			foreach (Vector3 item in list)
			{
				for (int j = 0; j <= 360; j += angleSimulation)
				{
					Ray ray = new Ray(item, new Vector3(Mathf.Cos((float)j * (MathF.PI / 180f)), 0f, Mathf.Sin((float)j * (MathF.PI / 180f))).normalized);
					if (Physics.Raycast(ray, out var hitInfo, checkDistanceSimulation))
					{
						bool flag = false;
						Vector3 point = hitInfo.point;
						foreach (Vector3 item2 in list)
						{
							if (Vector3.Distance(point, item2) < closeDistanceSimulation)
							{
								flag = true;
								break;
							}
						}
						foreach (Vector3 item3 in list2)
						{
							if (Vector3.Distance(point, item3) < closeDistanceSimulation)
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							list2.Add(point + ray.direction * 0.3f);
						}
						continue;
					}
					bool flag2 = false;
					Vector3 vector = ray.origin + ray.direction * 50f;
					foreach (Vector3 item4 in list)
					{
						if (Vector3.Distance(vector, item4) < closeDistanceSimulation)
						{
							flag2 = true;
							break;
						}
					}
					foreach (Vector3 item5 in list2)
					{
						if (Vector3.Distance(vector, item5) < closeDistanceSimulation)
						{
							flag2 = true;
							break;
						}
					}
					if (!flag2)
					{
						list2.Add(vector);
					}
				}
			}
			if (i == 0)
			{
				list.AddRange(list2);
			}
			else
			{
				for (int k = 0; k < list2.Count; k++)
				{
					float num2 = float.MaxValue;
					int num3 = -1;
					Vector3 vector2 = list2[k];
					for (int l = 0; l < list.Count; l++)
					{
						Vector3 l1_p = list[l];
						Vector3 vector3 = list[(l + 1) % list.Count];
						bool flag3 = false;
						for (int m = 0; m < list.Count; m++)
						{
							if (l != m)
							{
								Vector3 l2_p = list[m];
								Vector3 l2_p2 = list[(m + 1) % list.Count];
								if (AreLinesIntersecting(l1_p, vector2, l2_p, l2_p2) || AreLinesIntersecting(vector2, vector3, l2_p, l2_p2))
								{
									flag3 = true;
									break;
								}
							}
						}
						if (!flag3)
						{
							float num4 = Vector3.Distance(vector2, vector3);
							if (num2 > num4)
							{
								num2 = num4;
								num3 = (l + 1) % list.Count;
							}
						}
					}
					if (num3 > -1)
					{
						list.Insert(num3, vector2);
					}
				}
			}
			if (i == 0 && removeFirstPointSimulation)
			{
				list.RemoveAt(0);
			}
		}
		points.Clear();
		foreach (Vector3 item6 in list)
		{
			points.Add(base.transform.InverseTransformPoint(item6));
		}
		GeneratePolygon();
	}

	public static bool AreLinesIntersecting(Vector3 l1_p1, Vector3 l1_p2, Vector3 l2_p1, Vector3 l2_p2, bool shouldIncludeEndPoints = true)
	{
		float num = 1E-05f;
		bool result = false;
		float num2 = (l2_p2.z - l2_p1.z) * (l1_p2.x - l1_p1.x) - (l2_p2.x - l2_p1.x) * (l1_p2.z - l1_p1.z);
		if (num2 != 0f)
		{
			float num3 = ((l2_p2.x - l2_p1.x) * (l1_p1.z - l2_p1.z) - (l2_p2.z - l2_p1.z) * (l1_p1.x - l2_p1.x)) / num2;
			float num4 = ((l1_p2.x - l1_p1.x) * (l1_p1.z - l2_p1.z) - (l1_p2.z - l1_p1.z) * (l1_p1.x - l2_p1.x)) / num2;
			if (shouldIncludeEndPoints)
			{
				if (num3 >= 0f + num && num3 <= 1f - num && num4 >= 0f + num && num4 <= 1f - num)
				{
					result = true;
				}
			}
			else if (num3 > 0f + num && num3 < 1f - num && num4 > 0f + num && num4 < 1f - num)
			{
				result = true;
			}
		}
		return result;
	}

	public static float DistancePointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
	{
		return Vector3.Distance(ProjectPointLine(point, lineStart, lineEnd), point);
	}

	public static Vector3 ProjectPointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
	{
		Vector3 rhs = point - lineStart;
		Vector3 vector = lineEnd - lineStart;
		float magnitude = vector.magnitude;
		Vector3 vector2 = vector;
		if (magnitude > 1E-06f)
		{
			vector2 /= magnitude;
		}
		float num = Mathf.Clamp(Vector3.Dot(vector2, rhs), 0f, magnitude);
		return lineStart + vector2 * num;
	}
}
