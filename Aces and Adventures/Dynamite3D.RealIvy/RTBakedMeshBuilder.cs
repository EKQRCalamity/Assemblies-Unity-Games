using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Dynamite3D.RealIvy;

public class RTBakedMeshBuilder
{
	public IvyParameters ivyParameters;

	public RTIvyContainer rtIvyContainer;

	public RTIvyContainer rtBakedIvyContainer;

	public GameObject ivyGO;

	public MeshRenderer meshRenderer;

	public MeshFilter meshFilter;

	private bool onOptimizedStretch;

	private MeshFilter leavesMeshFilter;

	private MeshRenderer leavesMeshRenderer;

	private MeshRenderer mrProcessedMesh;

	private Mesh processedMesh;

	private Mesh ivyMesh;

	public List<RTBranchContainer> activeBranches;

	public RTMeshData bakedMeshData;

	public RTMeshData buildingMeshData;

	public RTMeshData processedMeshData;

	public List<List<int>> processedVerticesIndicesPerBranch;

	public List<List<int>> processedBranchesVerticesIndicesPerBranch;

	private int[] vertCountsPerBranch;

	private int[] lastTriangleIndexPerBranch;

	private int[] vertCountLeavesPerBranch;

	private int lastPointCopied;

	private int vertCount;

	private int lastVertCount;

	private int triCount;

	private int lastVerticesCountProcessed;

	private int lastLeafVertProcessed;

	private int submeshCount;

	private int[] submeshByChoseLeaf;

	private int initIdxLeaves;

	private int endIdxLeaves;

	private int backtrackingPoints;

	private int[] fromTo;

	private Vector3[] vectors;

	private RTMeshData[] leavesMeshesByChosenLeaf;

	private int lastVertexIndex;

	private float angle;

	public List<Material> leavesMaterials;

	public List<List<int>> typesByMat;

	public Rect[] uv2Rects = new Rect[0];

	public bool leavesDataInitialized;

	private float growthSpeed;

	private float leafLengthCorrrectionFactor;

	private Vector3 ivyGoPosition;

	private Quaternion ivyGoRotation;

	private Quaternion ivyGoInverseRotation;

	private Vector3 zeroVector3;

	private Vector2 zeroVector2;

	private Color blackColor;

	public void InitializeMeshBuilder(IvyParameters ivyParameters, RTIvyContainer ivyContainer, RTIvyContainer bakedIvyContainer, GameObject ivyGO, Mesh bakedMesh, MeshRenderer meshRenderer, MeshFilter meshFilter, int numBranches, Mesh processedMesh, float growSpeed, MeshRenderer mrProcessedMesh, int backtrackingPoints, int[] submeshByChoseLeaf, RTMeshData[] leavesMeshesByChosenLeaf, Material[] materials)
	{
		this.ivyParameters = ivyParameters;
		rtIvyContainer = ivyContainer;
		rtBakedIvyContainer = bakedIvyContainer;
		this.ivyGO = ivyGO;
		this.meshRenderer = meshRenderer;
		this.meshFilter = meshFilter;
		this.processedMesh = processedMesh;
		this.processedMesh.indexFormat = IndexFormat.UInt16;
		this.mrProcessedMesh = mrProcessedMesh;
		this.submeshByChoseLeaf = submeshByChoseLeaf;
		this.leavesMeshesByChosenLeaf = leavesMeshesByChosenLeaf;
		activeBranches = new List<RTBranchContainer>();
		fromTo = new int[2];
		vectors = new Vector3[2];
		growthSpeed = growSpeed;
		this.backtrackingPoints = backtrackingPoints;
		submeshCount = meshRenderer.sharedMaterials.Length;
		vertCountsPerBranch = new int[numBranches];
		lastTriangleIndexPerBranch = new int[numBranches];
		vertCountLeavesPerBranch = new int[numBranches];
		processedVerticesIndicesPerBranch = new List<List<int>>(numBranches);
		processedBranchesVerticesIndicesPerBranch = new List<List<int>>(numBranches);
		for (int i = 0; i < numBranches; i++)
		{
			processedVerticesIndicesPerBranch.Add(new List<int>());
			processedBranchesVerticesIndicesPerBranch.Add(new List<int>());
		}
		vertCount = 0;
		ivyMesh = new Mesh();
		ivyMesh.subMeshCount = submeshCount;
		ivyMesh.name = "IvyMesh";
		meshFilter.mesh = ivyMesh;
		ivyGO.GetComponent<MeshRenderer>().sharedMaterials = materials;
		mrProcessedMesh.sharedMaterials = materials;
		leavesDataInitialized = true;
		ivyGoPosition = ivyGO.transform.position;
		ivyGoRotation = ivyGO.transform.rotation;
		ivyGoInverseRotation = Quaternion.Inverse(ivyGO.transform.rotation);
		zeroVector3 = Vector3.zero;
		zeroVector2 = Vector2.zero;
		blackColor = Color.black;
	}

	public void InitializeMeshesDataBaked(Mesh bakedMesh, int numBranches)
	{
		CreateBuildingMeshData(bakedMesh, numBranches);
		CreateBakedMeshData(bakedMesh);
		CreateProcessedMeshData(bakedMesh);
		bakedMesh.Clear();
	}

	public void InitializeMeshesDataProcedural(Mesh bakedMesh, int numBranches, float lifetime, float velocity)
	{
		CreateBuildingMeshData(bakedMesh, numBranches);
		CreateBakedMeshData(bakedMesh);
		CreateProcessedMeshDataProcedural(bakedMesh, lifetime, velocity);
		bakedMesh.Clear();
	}

	public void CreateBuildingMeshData(Mesh bakedMesh, int numBranches)
	{
		int num = ivyParameters.sides + 1;
		int num2 = backtrackingPoints * num + backtrackingPoints * 2 * 8;
		num2 *= numBranches;
		int subMeshCount = bakedMesh.subMeshCount;
		List<int> list = new List<int>();
		int num3 = (backtrackingPoints - 2) * (ivyParameters.sides * 6) + ivyParameters.sides * 3;
		num3 *= numBranches;
		list.Add(num3);
		for (int i = 1; i < subMeshCount; i++)
		{
			int item = backtrackingPoints * 6 * numBranches;
			list.Add(item);
		}
		buildingMeshData = new RTMeshData(num2, subMeshCount, list);
	}

	public void CreateBakedMeshData(Mesh bakedMesh)
	{
		bakedMeshData = new RTMeshData(bakedMesh);
	}

	public void CreateProcessedMeshDataProcedural(Mesh bakedMesh, float lifetime, float velocity)
	{
		int num = Mathf.CeilToInt(lifetime / velocity * 200f);
		int numVertices = num * (ivyParameters.sides + 1);
		int subMeshCount = bakedMesh.subMeshCount;
		List<int> list = new List<int>();
		for (int i = 0; i < subMeshCount; i++)
		{
			int item = ivyParameters.sides * num * 9;
			list.Add(item);
		}
		processedMeshData = new RTMeshData(numVertices, subMeshCount, list);
	}

	public void CreateProcessedMeshData(Mesh bakedMesh)
	{
		int vertexCount = bakedMesh.vertexCount;
		int subMeshCount = bakedMesh.subMeshCount;
		List<int> list = new List<int>();
		for (int i = 0; i < subMeshCount; i++)
		{
			int item = bakedMesh.GetTriangles(i).Length;
			list.Add(item);
		}
		processedMeshData = new RTMeshData(vertexCount, subMeshCount, list);
	}

	public void SetLeafLengthCorrectionFactor(float leafLengthCorrrectionFactor)
	{
		this.leafLengthCorrrectionFactor = leafLengthCorrrectionFactor;
	}

	public void ClearMesh()
	{
		ivyMesh.Clear();
	}

	public RTBakedMeshBuilder()
	{
	}

	public RTBakedMeshBuilder(RTIvyContainer ivyContainer, GameObject ivyGo)
	{
		rtIvyContainer = ivyContainer;
		ivyGO = ivyGo;
	}

	private void ClearTipMesh()
	{
		buildingMeshData.Clear();
		for (int i = 0; i < vertCountsPerBranch.Length; i++)
		{
			vertCountsPerBranch[i] = 0;
			lastTriangleIndexPerBranch[i] = 0;
			vertCountLeavesPerBranch[i] = 0;
		}
		vertCount = 0;
		triCount = 0;
	}

	public void CheckCopyMesh(int branchIndex, List<RTBranchContainer> bakedBranches)
	{
		RTBranchContainer rTBranchContainer = rtIvyContainer.branches[branchIndex];
		RTBranchContainer bakedBranchContainer = bakedBranches[branchIndex];
		int initSegmentIdx;
		int endSegmentIdx = (initSegmentIdx = Mathf.Clamp(rTBranchContainer.branchPoints.Count - backtrackingPoints - 1, 0, int.MaxValue)) + 1;
		CopyToFixedMesh(branchIndex, initSegmentIdx, endSegmentIdx, rTBranchContainer, bakedBranchContainer);
	}

	public void BuildGeometry02(List<RTBranchContainer> activeBakedBranches, List<RTBranchContainer> activeBuildingBranches)
	{
		if (!ivyParameters.halfgeom)
		{
			angle = 360f / (float)ivyParameters.sides;
		}
		else
		{
			angle = 360f / (float)ivyParameters.sides / 2f;
		}
		if (!leavesDataInitialized)
		{
			return;
		}
		ClearTipMesh();
		for (int i = 0; i < rtIvyContainer.branches.Count; i++)
		{
			int num = vertCount;
			RTBranchContainer rTBranchContainer = activeBuildingBranches[i];
			if (rTBranchContainer.branchPoints.Count > 1)
			{
				lastVertCount = 0;
				int value = rTBranchContainer.branchPoints.Count - backtrackingPoints;
				value = Mathf.Clamp(value, 0, int.MaxValue);
				int count = rTBranchContainer.branchPoints.Count;
				for (int j = value; j < count; j++)
				{
					RTBranchPoint rTBranchPoint = rTBranchContainer.branchPoints[j];
					Vector3 vector = ivyGO.transform.InverseTransformPoint(rTBranchPoint.point);
					Vector3 vector2 = zeroVector3;
					Vector3 vector3 = zeroVector3;
					Vector2 vector4 = zeroVector2;
					_ = zeroVector2;
					_ = blackColor;
					float t = Mathf.InverseLerp(rTBranchContainer.totalLength, rTBranchContainer.totalLength - ivyParameters.tipInfluence, rTBranchPoint.length);
					if (j < rTBranchContainer.branchPoints.Count - 1)
					{
						for (int k = 0; k < rTBranchPoint.verticesLoop.Length; k++)
						{
							if (ivyParameters.generateBranches)
							{
								vector2 = Vector3.LerpUnclamped(rTBranchPoint.centerLoop, rTBranchPoint.verticesLoop[k].vertex, t);
								buildingMeshData.AddVertex(vector2, rTBranchPoint.verticesLoop[k].normal, rTBranchPoint.verticesLoop[k].uv, rTBranchPoint.verticesLoop[k].color);
								vertCountsPerBranch[i]++;
								vertCount++;
								lastVertCount++;
							}
						}
					}
					else
					{
						vector2 = vector;
						vector3 = Vector3.Normalize(rTBranchPoint.point - rTBranchPoint.GetPreviousPoint().point);
						vector3 = ivyGO.transform.InverseTransformVector(vector3);
						vector4 = rTBranchContainer.GetLastUV(ivyParameters);
						buildingMeshData.AddVertex(vector2, vector3, vector4, Color.black);
						vertCountsPerBranch[i]++;
						vertCount++;
						lastVertCount++;
					}
				}
				SetTriangles(rTBranchContainer, vertCount, value, i);
			}
			fromTo[0] = num;
			fromTo[1] = vertCount - 1;
			if (ivyParameters.generateLeaves)
			{
				BuildLeaves(i, activeBuildingBranches[i], activeBakedBranches[i]);
			}
		}
		RefreshMesh();
	}

	private float CalculateRadius(BranchPoint branchPoint, BranchContainer buildingBranchContainer)
	{
		float num = Mathf.InverseLerp(branchPoint.branchContainer.totalLenght, branchPoint.branchContainer.totalLenght - ivyParameters.tipInfluence, branchPoint.length - 0.1f);
		branchPoint.currentRadius = branchPoint.radius * num;
		return branchPoint.currentRadius;
	}

	private void SetTriangles(RTBranchContainer branch, int vertCount, int initIndex, int branchIndex)
	{
		int num = Mathf.Min(branch.branchPoints.Count - 2, branch.branchPoints.Count - initIndex - 2);
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < ivyParameters.sides; j++)
			{
				int num2 = vertCount - lastVertCount;
				int value = j + i * (ivyParameters.sides + 1) + num2;
				int value2 = j + i * (ivyParameters.sides + 1) + 1 + num2;
				int value3 = j + i * (ivyParameters.sides + 1) + ivyParameters.sides + 1 + num2;
				int value4 = j + i * (ivyParameters.sides + 1) + 1 + num2;
				int value5 = j + i * (ivyParameters.sides + 1) + ivyParameters.sides + 2 + num2;
				int value6 = j + i * (ivyParameters.sides + 1) + ivyParameters.sides + 1 + num2;
				buildingMeshData.AddTriangle(0, value);
				buildingMeshData.AddTriangle(0, value2);
				buildingMeshData.AddTriangle(0, value3);
				buildingMeshData.AddTriangle(0, value4);
				buildingMeshData.AddTriangle(0, value5);
				buildingMeshData.AddTriangle(0, value6);
				triCount += 6;
			}
		}
		int num3 = 0;
		int num4 = 0;
		while (num3 < ivyParameters.sides * 3)
		{
			buildingMeshData.AddTriangle(0, vertCount - 1);
			buildingMeshData.AddTriangle(0, vertCount - 3 - num4);
			buildingMeshData.AddTriangle(0, vertCount - 2 - num4);
			triCount += 3;
			num3 += 3;
			num4++;
		}
		lastTriangleIndexPerBranch[branchIndex] = vertCount - 1;
	}

	private void BuildLeaves(int branchIndex, RTBranchContainer buildingBranchContainer, RTBranchContainer bakedBranchContainer)
	{
		for (int i = Mathf.Clamp(buildingBranchContainer.branchPoints.Count - backtrackingPoints, 0, int.MaxValue); i < buildingBranchContainer.branchPoints.Count; i++)
		{
			RTLeafPoint[] array = bakedBranchContainer.leavesOrderedByInitSegment[i];
			RTBranchPoint rTBranchPoint = buildingBranchContainer.branchPoints[i];
			foreach (RTLeafPoint rTLeafPoint in array)
			{
				if (rTLeafPoint != null)
				{
					float t = Mathf.InverseLerp(buildingBranchContainer.totalLength, buildingBranchContainer.totalLength - ivyParameters.tipInfluence, rTBranchPoint.length);
					RTMeshData rTMeshData = leavesMeshesByChosenLeaf[rTLeafPoint.chosenLeave];
					for (int k = 0; k < rTMeshData.triangles[0].Length; k++)
					{
						int value = rTMeshData.triangles[0][k] + vertCount;
						int sumbesh = submeshByChoseLeaf[rTLeafPoint.chosenLeave];
						buildingMeshData.AddTriangle(sumbesh, value);
					}
					for (int l = 0; l < rTLeafPoint.vertices.Length; l++)
					{
						Vector3 vertexValue = Vector3.LerpUnclamped(rTLeafPoint.leafCenter, rTLeafPoint.vertices[l].vertex, t);
						buildingMeshData.AddVertex(vertexValue, rTLeafPoint.vertices[l].normal, rTLeafPoint.vertices[l].uv, rTLeafPoint.vertices[l].color);
						vertCountLeavesPerBranch[branchIndex]++;
						vertCountsPerBranch[branchIndex]++;
						vertCount++;
					}
				}
			}
		}
	}

	public void CopyToFixedMesh(int branchIndex, int initSegmentIdx, int endSegmentIdx, RTBranchContainer branchContainer, RTBranchContainer bakedBranchContainer)
	{
		int num = ivyParameters.sides + 1;
		_ = ivyParameters.sides;
		int num2 = 1;
		_ = vertCountsPerBranch[branchIndex];
		_ = vertCountLeavesPerBranch[branchIndex];
		int num3 = 0;
		for (int i = 1; i <= branchIndex; i++)
		{
			num3 += vertCountsPerBranch[branchIndex];
		}
		if (processedBranchesVerticesIndicesPerBranch[branchIndex].Count <= 0)
		{
			num2 = 2;
		}
		else
		{
			num2 = 1;
			num3 += num;
		}
		for (int num4 = num2 - 1; num4 >= 0; num4--)
		{
			int index = branchContainer.branchPoints.Count - backtrackingPoints - num4;
			RTBranchPoint rTBranchPoint = branchContainer.branchPoints[index];
			for (int j = 0; j < rTBranchPoint.verticesLoop.Length; j++)
			{
				RTVertexData rTVertexData = rTBranchPoint.verticesLoop[j];
				processedMeshData.AddVertex(rTVertexData.vertex, rTVertexData.normal, rTVertexData.uv, rTVertexData.color);
				processedBranchesVerticesIndicesPerBranch[branchIndex].Add(processedMeshData.VertexCount() - 1);
			}
		}
		if (branchIndex > 0)
		{
			_ = lastTriangleIndexPerBranch[branchIndex];
		}
		if (processedBranchesVerticesIndicesPerBranch[branchIndex].Count >= num * 2)
		{
			int num5 = processedBranchesVerticesIndicesPerBranch[branchIndex].Count - num * 2;
			for (int k = 0; k < ivyParameters.sides; k++)
			{
				int value = processedBranchesVerticesIndicesPerBranch[branchIndex][k + num5];
				int value2 = processedBranchesVerticesIndicesPerBranch[branchIndex][k + 1 + num5];
				int value3 = processedBranchesVerticesIndicesPerBranch[branchIndex][k + ivyParameters.sides + 1 + num5];
				int value4 = processedBranchesVerticesIndicesPerBranch[branchIndex][k + 1 + num5];
				int value5 = processedBranchesVerticesIndicesPerBranch[branchIndex][k + ivyParameters.sides + 2 + num5];
				int value6 = processedBranchesVerticesIndicesPerBranch[branchIndex][k + ivyParameters.sides + 1 + num5];
				processedMeshData.AddTriangle(0, value);
				processedMeshData.AddTriangle(0, value2);
				processedMeshData.AddTriangle(0, value3);
				processedMeshData.AddTriangle(0, value4);
				processedMeshData.AddTriangle(0, value5);
				processedMeshData.AddTriangle(0, value6);
			}
		}
		if (!ivyParameters.generateLeaves)
		{
			return;
		}
		int num6 = processedMeshData.VertexCount();
		int num7 = 0;
		for (int l = initSegmentIdx; l < endSegmentIdx; l++)
		{
			RTLeafPoint[] array = bakedBranchContainer.leavesOrderedByInitSegment[l];
			foreach (RTLeafPoint rTLeafPoint in array)
			{
				if (rTLeafPoint != null)
				{
					RTMeshData rTMeshData = leavesMeshesByChosenLeaf[rTLeafPoint.chosenLeave];
					int sumbesh = submeshByChoseLeaf[rTLeafPoint.chosenLeave];
					for (int n = 0; n < rTMeshData.triangles[0].Length; n++)
					{
						int value7 = rTMeshData.triangles[0][n] + num6;
						processedMeshData.AddTriangle(sumbesh, value7);
					}
					for (int num8 = 0; num8 < rTLeafPoint.vertices.Length; num8++)
					{
						RTVertexData rTVertexData2 = rTLeafPoint.vertices[num8];
						processedMeshData.AddVertex(rTVertexData2.vertex, rTVertexData2.normal, rTVertexData2.uv, rTVertexData2.color);
						processedVerticesIndicesPerBranch[branchIndex].Add(processedMeshData.VertexCount() - 1);
						num6++;
					}
					num7++;
				}
			}
		}
	}

	public void RefreshProcessedMesh()
	{
		processedMesh.MarkDynamic();
		processedMesh.subMeshCount = submeshCount;
		processedMesh.vertices = processedMeshData.vertices;
		processedMesh.normals = processedMeshData.normals;
		processedMesh.colors = processedMeshData.colors;
		processedMesh.uv = processedMeshData.uv;
		processedMesh.SetTriangles(processedMeshData.triangles[0], 0);
		if (ivyParameters.generateLeaves)
		{
			for (int i = 1; i < submeshCount; i++)
			{
				processedMesh.SetTriangles(processedMeshData.triangles[i], i);
			}
		}
		processedMesh.RecalculateBounds();
	}

	private void RefreshMesh()
	{
		ivyMesh.Clear();
		ivyMesh.subMeshCount = submeshCount;
		ivyMesh.MarkDynamic();
		ivyMesh.vertices = buildingMeshData.vertices;
		ivyMesh.normals = buildingMeshData.normals;
		ivyMesh.colors = buildingMeshData.colors;
		ivyMesh.uv = buildingMeshData.uv;
		ivyMesh.SetTriangles(buildingMeshData.triangles[0], 0);
		if (ivyParameters.generateLeaves)
		{
			for (int i = 1; i < submeshCount; i++)
			{
				ivyMesh.SetTriangles(buildingMeshData.triangles[i], i);
			}
		}
		ivyMesh.RecalculateBounds();
	}
}
