using System.Collections.Generic;
using UnityEngine;

namespace Dynamite3D.RealIvy;

public abstract class RTIvy : MonoBehaviour
{
	protected IvyParameters ivyParameters;

	protected RTIvyContainer rtIvyContainer;

	protected RTIvyContainer rtBuildingIvyContainer;

	public MeshFilter meshFilter;

	public MeshRenderer meshRenderer;

	public MeshRenderer mrProcessedMesh;

	public MeshFilter mfProcessedMesh;

	protected List<RTBranchContainer> activeBakedBranches;

	protected List<RTBranchContainer> activeBuildingBranches;

	protected int lastIdxActiveBranch;

	public List<float> srcTotalLengthPerBranch;

	public List<float> dstTotalLengthPerBranch;

	public List<float> growingFactorPerBranch;

	public List<float> lengthPerBranch;

	protected List<int> lastCopiedIndexPerBranch;

	protected List<Vector3> srcPoints;

	protected List<Vector3> dstPoints;

	protected List<LeafPoint> leavesToCopyMesh;

	public RTBakedMeshBuilder meshBuilder;

	private Mesh bakedMesh;

	private Mesh processedMesh;

	private bool refreshProcessedMesh;

	private int backtrackingPoints;

	protected float currentLifetime;

	protected float currentSpeed;

	protected float currentGrowthSpeed;

	protected float leafLengthCorrrectionFactor;

	protected float currentTimer;

	protected RuntimeGrowthParameters growthParameters;

	protected List<Material> leavesMaterials;

	protected RTMeshData[] leavesMeshesByChosenLeaf;

	protected int[] submeshByChoseLeaf;

	protected int maxBranches;

	public void AwakeInit()
	{
		bakedMesh = meshFilter.sharedMesh;
		meshFilter.sharedMesh = null;
	}

	protected virtual void Init(IvyContainer ivyContainer, IvyParameters ivyParameters)
	{
		rtIvyContainer = new RTIvyContainer();
		this.ivyParameters = new IvyParameters();
		this.ivyParameters.CopyFrom(ivyParameters);
		CreateLeavesDict();
		if (ivyContainer != null)
		{
			rtIvyContainer.Initialize(ivyContainer, ivyParameters, base.gameObject, leavesMeshesByChosenLeaf, ivyContainer.firstVertexVector);
		}
		else
		{
			rtIvyContainer.Initialize();
		}
		SetUpMaxBranches(ivyContainer);
		activeBakedBranches = new List<RTBranchContainer>(maxBranches);
		activeBuildingBranches = new List<RTBranchContainer>(maxBranches);
		rtBuildingIvyContainer = new RTIvyContainer();
		Vector3 firstVertexVector = ((ivyContainer == null) ? CalculateFirstVertexVector() : ivyContainer.firstVertexVector);
		rtBuildingIvyContainer.Initialize(firstVertexVector);
		lastIdxActiveBranch = -1;
		leafLengthCorrrectionFactor = 1f;
		int subMeshCount = ivyParameters.leavesPrefabs.Length + 1;
		processedMesh = new Mesh();
		processedMesh.subMeshCount = subMeshCount;
		mfProcessedMesh.sharedMesh = processedMesh;
		refreshProcessedMesh = false;
		backtrackingPoints = GetBacktrackingPoints();
		if (bakedMesh == null)
		{
			bakedMesh = new Mesh();
			bakedMesh.subMeshCount = subMeshCount;
		}
		lastCopiedIndexPerBranch = new List<int>(maxBranches);
		leavesToCopyMesh = new List<LeafPoint>(50);
		srcPoints = new List<Vector3>(maxBranches);
		dstPoints = new List<Vector3>(maxBranches);
		growingFactorPerBranch = new List<float>(maxBranches);
		srcTotalLengthPerBranch = new List<float>(maxBranches);
		dstTotalLengthPerBranch = new List<float>(maxBranches);
		lengthPerBranch = new List<float>(maxBranches);
		for (int i = 0; i < maxBranches; i++)
		{
			srcPoints.Add(Vector3.zero);
			dstPoints.Add(Vector3.zero);
			growingFactorPerBranch.Add(0f);
			srcTotalLengthPerBranch.Add(0f);
			dstTotalLengthPerBranch.Add(0f);
			lastCopiedIndexPerBranch.Add(-1);
			lengthPerBranch.Add(0f);
			int maxNumPoints = GetMaxNumPoints();
			int maxNumLeaves = GetMaxNumLeaves();
			RTBranchContainer item = new RTBranchContainer(maxNumPoints, maxNumLeaves);
			activeBuildingBranches.Add(item);
		}
	}

	private void SetUpMaxBranches(IvyContainer ivyContainer)
	{
		maxBranches = ivyParameters.maxBranchs;
		if (ivyContainer != null)
		{
			maxBranches = Mathf.Max(ivyParameters.maxBranchs, ivyContainer.branches.Count);
		}
	}

	protected void InitMeshBuilder()
	{
		meshBuilder = new RTBakedMeshBuilder(rtIvyContainer, base.gameObject);
		meshBuilder.InitializeMeshBuilder(ivyParameters, rtBuildingIvyContainer, rtIvyContainer, base.gameObject, bakedMesh, meshRenderer, meshFilter, maxBranches, processedMesh, growthParameters.growthSpeed, mrProcessedMesh, backtrackingPoints, submeshByChoseLeaf, leavesMeshesByChosenLeaf, leavesMaterials.ToArray());
		InitializeMeshesData(bakedMesh, maxBranches);
	}

	protected virtual void AddFirstBranch()
	{
		AddNextBranch(0);
	}

	private int GetBacktrackingPoints()
	{
		return Mathf.CeilToInt(ivyParameters.tipInfluence / ivyParameters.stepSize);
	}

	public virtual void UpdateIvy(float deltaTime)
	{
		UpdateGrowthSpeed();
		for (int i = 0; i < activeBakedBranches.Count; i++)
		{
			Growing(i, deltaTime);
		}
		currentTimer += deltaTime;
		RefreshGeometry();
		if (refreshProcessedMesh)
		{
			meshBuilder.RefreshProcessedMesh();
			refreshProcessedMesh = false;
		}
	}

	protected virtual void Growing(int branchIndex, float deltaTime)
	{
		RTBranchContainer rTBranchContainer = activeBuildingBranches[branchIndex];
		CalculateFactors(srcPoints[branchIndex], dstPoints[branchIndex]);
		meshBuilder.SetLeafLengthCorrectionFactor(leafLengthCorrrectionFactor);
		growingFactorPerBranch[branchIndex] += currentSpeed * deltaTime;
		growingFactorPerBranch[branchIndex] = Mathf.Clamp(growingFactorPerBranch[branchIndex], 0f, 1f);
		rTBranchContainer.totalLength = Mathf.Lerp(srcTotalLengthPerBranch[branchIndex], dstTotalLengthPerBranch[branchIndex], growingFactorPerBranch[branchIndex]);
		RTBranchPoint lastBranchPoint = rTBranchContainer.GetLastBranchPoint();
		lastBranchPoint.length = rTBranchContainer.totalLength;
		lastBranchPoint.point = Vector3.Lerp(srcPoints[branchIndex], dstPoints[branchIndex], growingFactorPerBranch[branchIndex]);
		if (growingFactorPerBranch[branchIndex] >= 1f)
		{
			RefreshGeometry();
			NextPoints(branchIndex);
		}
	}

	protected virtual void NextPoints(int branchIndex)
	{
		if (rtBuildingIvyContainer.branches[branchIndex].branchPoints.Count <= 0)
		{
			return;
		}
		RTBranchPoint lastBranchPoint = rtBuildingIvyContainer.branches[branchIndex].GetLastBranchPoint();
		if (lastBranchPoint.index >= activeBakedBranches[branchIndex].branchPoints.Count - 1)
		{
			return;
		}
		int index = lastBranchPoint.index;
		index++;
		RTBranchPoint rTBranchPoint = activeBakedBranches[branchIndex].branchPoints[index];
		rtBuildingIvyContainer.branches[branchIndex].AddBranchPoint(rTBranchPoint, ivyParameters.stepSize);
		if (rTBranchPoint.newBranch && rtIvyContainer.GetBranchContainerByBranchNumber(rTBranchPoint.newBranchNumber).branchPoints.Count >= 2)
		{
			AddNextBranch(rTBranchPoint.newBranchNumber);
		}
		UpdateGrowingPoints(branchIndex);
		if (rtBuildingIvyContainer.branches[branchIndex].branchPoints.Count > backtrackingPoints)
		{
			if (!IsVertexLimitReached())
			{
				meshBuilder.CheckCopyMesh(branchIndex, activeBakedBranches);
				refreshProcessedMesh = true;
			}
			else
			{
				Debug.LogWarning("Limit vertices reached! --> " + 65535 + " vertices", meshBuilder.ivyGO);
			}
		}
	}

	private void CalculateFactors(Vector3 srcPoint, Vector3 dstPoint)
	{
		float num = Vector3.Distance(srcPoint, dstPoint) / ivyParameters.stepSize;
		num = 1f / num;
		currentSpeed = num * currentGrowthSpeed;
		leafLengthCorrrectionFactor = Mathf.Lerp(0.92f, 1f, num);
	}

	protected virtual void AddNextBranch(int branchNumber)
	{
		lastIdxActiveBranch++;
		RTBranchContainer rTBranchContainer = activeBuildingBranches[lastIdxActiveBranch];
		RTBranchContainer branchContainerByBranchNumber = rtIvyContainer.GetBranchContainerByBranchNumber(branchNumber);
		rTBranchContainer.AddBranchPoint(branchContainerByBranchNumber.branchPoints[0], ivyParameters.stepSize);
		rTBranchContainer.AddBranchPoint(branchContainerByBranchNumber.branchPoints[1], ivyParameters.stepSize);
		rTBranchContainer.leavesOrderedByInitSegment = branchContainerByBranchNumber.leavesOrderedByInitSegment;
		rtBuildingIvyContainer.AddBranch(rTBranchContainer);
		activeBakedBranches.Add(branchContainerByBranchNumber);
		activeBuildingBranches.Add(rTBranchContainer);
		meshBuilder.activeBranches.Add(rTBranchContainer);
		UpdateGrowingPoints(rtBuildingIvyContainer.branches.Count - 1);
		RTBranchPoint lastBranchPoint = rTBranchContainer.GetLastBranchPoint();
		if (lastBranchPoint.newBranch)
		{
			AddNextBranch(lastBranchPoint.newBranchNumber);
		}
	}

	private void UpdateGrowingPoints(int branchIndex)
	{
		if (rtBuildingIvyContainer.branches[branchIndex].branchPoints.Count > 0)
		{
			RTBranchPoint lastBranchPoint = rtBuildingIvyContainer.branches[branchIndex].GetLastBranchPoint();
			if (lastBranchPoint.index < activeBakedBranches[branchIndex].branchPoints.Count - 1)
			{
				RTBranchPoint rTBranchPoint = activeBakedBranches[branchIndex].branchPoints[lastBranchPoint.index + 1];
				growingFactorPerBranch[branchIndex] = 0f;
				srcPoints[branchIndex] = lastBranchPoint.point;
				dstPoints[branchIndex] = rTBranchPoint.point;
				srcTotalLengthPerBranch[branchIndex] = lastBranchPoint.length;
				dstTotalLengthPerBranch[branchIndex] = lastBranchPoint.length + ivyParameters.stepSize;
			}
		}
	}

	private void RefreshGeometry()
	{
		meshBuilder.BuildGeometry02(activeBakedBranches, activeBuildingBranches);
	}

	private void UpdateGrowthSpeed()
	{
		currentGrowthSpeed = growthParameters.growthSpeed;
		if (growthParameters.speedOverLifetimeEnabled)
		{
			float normalizedLifeTime = GetNormalizedLifeTime();
			currentGrowthSpeed = growthParameters.growthSpeed * growthParameters.speedOverLifetimeCurve.Evaluate(normalizedLifeTime);
		}
	}

	public bool IsVertexLimitReached()
	{
		return meshBuilder.processedMeshData.VertexCount() + ivyParameters.sides + 1 >= 65535;
	}

	private Vector3 CalculateFirstVertexVector()
	{
		return Quaternion.AngleAxis(Random.value * 360f, base.transform.up) * base.transform.forward;
	}

	private void CreateLeavesDict()
	{
		new List<List<int>>();
		leavesMaterials = new List<Material>();
		leavesMeshesByChosenLeaf = new RTMeshData[ivyParameters.leavesPrefabs.Length];
		leavesMaterials.Add(ivyParameters.branchesMaterial);
		submeshByChoseLeaf = new int[ivyParameters.leavesPrefabs.Length];
		int num = 0;
		for (int i = 0; i < ivyParameters.leavesPrefabs.Length; i++)
		{
			MeshRenderer component = ivyParameters.leavesPrefabs[i].GetComponent<MeshRenderer>();
			MeshFilter component2 = ivyParameters.leavesPrefabs[i].GetComponent<MeshFilter>();
			if (!leavesMaterials.Contains(component.sharedMaterial))
			{
				leavesMaterials.Add(component.sharedMaterial);
				num++;
			}
			submeshByChoseLeaf[i] = num;
			RTMeshData rTMeshData = new RTMeshData(component2.sharedMesh);
			leavesMeshesByChosenLeaf[i] = rTMeshData;
		}
		Material[] sharedMaterials = leavesMaterials.ToArray();
		mrProcessedMesh.sharedMaterials = sharedMaterials;
	}

	protected abstract void InitializeMeshesData(Mesh bakedMesh, int numBranches);

	protected abstract float GetNormalizedLifeTime();

	protected abstract int GetMaxNumPoints();

	protected abstract int GetMaxNumLeaves();

	public abstract bool IsGrowingFinished();

	public abstract void InitIvy(RuntimeGrowthParameters growthParameters, IvyContainer ivyContainer, IvyParameters ivyParameters);
}
