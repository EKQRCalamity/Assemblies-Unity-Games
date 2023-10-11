using System;
using UnityEngine;

namespace Dynamite3D.RealIvy;

public class RuntimeIvyGrowth
{
	private RTIvyContainer rtIvyContainer;

	private IvyParameters ivyParameters;

	private GameObject ivyGO;

	private RTMeshData[] leavesMeshesByChosenLeaf;

	private RTBranchContainer[] branchesPool;

	private int branchesPoolIndex;

	private RTBranchPoint[] branchPointsPool;

	private int branchPointPoolIndex;

	private RTLeafPoint[] leavesPool;

	private int leavesPoolIndex;

	private int numPoints;

	private int numLeaves;

	private int maxNumVerticesPerLeaf;

	public UnityEngine.Random.State randomstate;

	public void Init(RTIvyContainer ivyContainer, IvyParameters ivyParameters, GameObject ivyGO, RTMeshData[] leavesMeshesByChosenLeaf, int numPoints, int numLeaves, int maxNumVerticesPerLeaf)
	{
		rtIvyContainer = ivyContainer;
		this.ivyParameters = ivyParameters;
		this.ivyGO = ivyGO;
		this.leavesMeshesByChosenLeaf = leavesMeshesByChosenLeaf;
		this.numPoints = numPoints;
		this.numLeaves = numLeaves;
		this.maxNumVerticesPerLeaf = maxNumVerticesPerLeaf;
		branchPointsPool = new RTBranchPoint[numPoints];
		branchPointPoolIndex = 0;
		for (int i = 0; i < numPoints; i++)
		{
			RTBranchPoint rTBranchPoint = new RTBranchPoint();
			rTBranchPoint.PreInit(ivyParameters);
			branchPointsPool[i] = rTBranchPoint;
		}
		leavesPool = new RTLeafPoint[numLeaves];
		leavesPoolIndex = 0;
		for (int j = 0; j < numLeaves; j++)
		{
			RTLeafPoint rTLeafPoint = new RTLeafPoint();
			rTLeafPoint.PreInit(maxNumVerticesPerLeaf);
			leavesPool[j] = rTLeafPoint;
		}
		branchesPool = new RTBranchContainer[ivyParameters.maxBranchs];
		for (int k = 0; k < ivyParameters.maxBranchs; k++)
		{
			branchesPool[k] = new RTBranchContainer(numPoints, numLeaves);
		}
		UnityEngine.Random.InitState(Environment.TickCount);
		RTBranchContainer nextBranchContainer = GetNextBranchContainer();
		ivyContainer.AddBranch(nextBranchContainer);
		RTBranchPoint nextFreeBranchPoint = GetNextFreeBranchPoint();
		nextFreeBranchPoint.SetValues(ivyGO.transform.position, -ivyGO.transform.up, newBranch: false, 0);
		nextBranchContainer.AddBranchPoint(nextFreeBranchPoint, ivyParameters.stepSize);
		CalculateVerticesLastPoint(nextBranchContainer);
		ivyContainer.branches[0].growDirection = Quaternion.AngleAxis(UnityEngine.Random.value * 360f, ivyGO.transform.up) * ivyGO.transform.forward;
		ivyContainer.firstVertexVector = ivyContainer.branches[0].growDirection;
		ivyContainer.branches[0].randomizeHeight = UnityEngine.Random.Range(4f, 8f);
		CalculateNewHeight(ivyContainer.branches[0]);
		ivyContainer.branches[0].branchSense = ChooseBranchSense();
		randomstate = UnityEngine.Random.state;
	}

	private void CalculateNewHeight(RTBranchContainer branch)
	{
		branch.heightVar = (Mathf.Sin(branch.heightParameter * ivyParameters.DTSFrequency - 45f) + 1f) / 2f;
		branch.newHeight = Mathf.Lerp(ivyParameters.minDistanceToSurface, ivyParameters.maxDistanceToSurface, branch.heightVar);
		branch.newHeight += (Mathf.Sin(branch.heightParameter * ivyParameters.DTSFrequency * branch.randomizeHeight) + 1f) / 2f * ivyParameters.maxDistanceToSurface / 4f * ivyParameters.DTSRandomness;
		branch.deltaHeight = branch.currentHeight - branch.newHeight;
		branch.currentHeight = branch.newHeight;
	}

	private int ChooseBranchSense()
	{
		if (UnityEngine.Random.value < 0.5f)
		{
			return -1;
		}
		return 1;
	}

	public void Step()
	{
		UnityEngine.Random.state = randomstate;
		for (int i = 0; i < rtIvyContainer.branches.Count; i++)
		{
			rtIvyContainer.branches[i].heightParameter += ivyParameters.stepSize;
			CalculateNewPoint(rtIvyContainer.branches[i]);
		}
		randomstate = UnityEngine.Random.state;
	}

	private void CalculateNewPoint(RTBranchContainer branch)
	{
		if (!branch.falling)
		{
			CalculateNewHeight(branch);
			CheckWall(branch);
		}
		else
		{
			CheckFall(branch);
		}
	}

	private void CheckWall(RTBranchContainer branch)
	{
		RTBranchPoint nextFreeBranchPoint = GetNextFreeBranchPoint();
		nextFreeBranchPoint.point = branch.GetLastBranchPoint().point + branch.growDirection * ivyParameters.stepSize + branch.GetLastBranchPoint().grabVector * branch.deltaHeight;
		nextFreeBranchPoint.index = branch.branchPoints.Count;
		Vector3 direction = nextFreeBranchPoint.point - branch.GetLastBranchPoint().point;
		if (!Physics.Raycast(new Ray(branch.branchPoints[branch.branchPoints.Count - 1].point, direction), out var hitInfo, ivyParameters.stepSize * 1.15f, ivyParameters.layerMask.value))
		{
			CheckFloor(branch, nextFreeBranchPoint, -branch.GetLastBranchPoint().grabVector);
			return;
		}
		NewGrowDirectionAfterWall(branch, -branch.GetLastBranchPoint().grabVector, hitInfo.normal);
		AddPoint(branch, hitInfo.point, hitInfo.normal);
	}

	private void CheckFloor(RTBranchContainer branch, RTBranchPoint potentialPoint, Vector3 oldSurfaceNormal)
	{
		if (Physics.Raycast(new Ray(potentialPoint.point, -oldSurfaceNormal), out var hitInfo, branch.currentHeight * 2f, ivyParameters.layerMask.value))
		{
			AddPoint(branch, hitInfo.point, hitInfo.normal);
			NewGrowDirection(branch);
			branch.fallIteration = 0f;
			branch.falling = false;
		}
		else if (UnityEngine.Random.value < ivyParameters.grabProvabilityOnFall)
		{
			CheckCorner(branch, potentialPoint, oldSurfaceNormal);
		}
		else
		{
			AddFallingPoint(branch);
			branch.fallIteration += 1f - ivyParameters.stiffness;
			branch.falling = true;
			branch.currentHeight = 0f;
			branch.heightParameter = -45f;
		}
	}

	private void CheckCorner(RTBranchContainer branch, RTBranchPoint potentialPoint, Vector3 oldSurfaceNormal)
	{
		if (Physics.Raycast(new Ray(potentialPoint.point + branch.branchPoints[branch.branchPoints.Count - 1].grabVector * 2f * branch.currentHeight, -branch.growDirection), out var hitInfo, ivyParameters.stepSize * 1.15f, ivyParameters.layerMask.value))
		{
			AddPoint(branch, potentialPoint.point, oldSurfaceNormal);
			AddPoint(branch, hitInfo.point, hitInfo.normal);
			NewGrowDirectionAfterCorner(branch, oldSurfaceNormal, hitInfo.normal);
		}
		else
		{
			AddFallingPoint(branch);
			branch.fallIteration += 1f - ivyParameters.stiffness;
			branch.falling = true;
			branch.currentHeight = 0f;
			branch.heightParameter = -45f;
		}
	}

	private void CheckFall(RTBranchContainer branch)
	{
		if (!Physics.Raycast(new Ray(branch.branchPoints[branch.branchPoints.Count - 1].point, branch.growDirection), out var hitInfo, ivyParameters.stepSize * 1.15f, ivyParameters.layerMask.value))
		{
			if (UnityEngine.Random.value < ivyParameters.grabProvabilityOnFall)
			{
				CheckGrabPoint(branch);
				return;
			}
			NewGrowDirectionFalling(branch);
			AddFallingPoint(branch);
			branch.fallIteration += 1f - ivyParameters.stiffness;
			branch.falling = true;
		}
		else
		{
			NewGrowDirectionAfterFall(branch, hitInfo.normal);
			AddPoint(branch, hitInfo.point, hitInfo.normal);
			branch.fallIteration = 0f;
			branch.falling = false;
		}
	}

	private void CheckGrabPoint(RTBranchContainer branch)
	{
		for (int i = 0; i < 6; i++)
		{
			float angle = 60f * (float)i;
			if (Physics.Raycast(new Ray(branch.branchPoints[branch.branchPoints.Count - 1].point + branch.growDirection * ivyParameters.stepSize, Quaternion.AngleAxis(angle, branch.growDirection) * branch.GetLastBranchPoint().grabVector), out var hitInfo, ivyParameters.stepSize * 2f, ivyParameters.layerMask.value))
			{
				AddPoint(branch, hitInfo.point, hitInfo.normal);
				NewGrowDirectionAfterGrab(branch, hitInfo.normal);
				branch.fallIteration = 0f;
				branch.falling = false;
				break;
			}
			if (i == 5)
			{
				AddFallingPoint(branch);
				NewGrowDirectionFalling(branch);
				branch.fallIteration += 1f - ivyParameters.stiffness;
				branch.falling = true;
			}
		}
	}

	public void AddPoint(RTBranchContainer branch, Vector3 point, Vector3 normal)
	{
		branch.totalLength += ivyParameters.stepSize;
		RTBranchPoint nextFreeBranchPoint = GetNextFreeBranchPoint();
		nextFreeBranchPoint.SetValues(point + normal * branch.currentHeight, -normal);
		branch.AddBranchPoint(nextFreeBranchPoint, ivyParameters.stepSize);
		CalculateVerticesLastPoint(branch);
		if (UnityEngine.Random.value < ivyParameters.branchProvability && rtIvyContainer.branches.Count < ivyParameters.maxBranchs)
		{
			AddBranch(branch, branch.GetLastBranchPoint(), branch.branchPoints[branch.branchPoints.Count - 1].point, normal);
		}
		if (ivyParameters.generateLeaves)
		{
			AddLeave(branch);
		}
	}

	private float CalculateRadius(float lenght)
	{
		float t = (Mathf.Sin(lenght * ivyParameters.radiusVarFreq + ivyParameters.radiusVarOffset) + 1f) / 2f;
		return Mathf.Lerp(ivyParameters.minRadius, ivyParameters.maxRadius, t);
	}

	private float CalculateLeafScale(BranchContainer branch, LeafPoint leafPoint)
	{
		float num = UnityEngine.Random.Range(ivyParameters.minScale, ivyParameters.maxScale);
		if (leafPoint.lpLength - 0.1f >= branch.totalLenght - ivyParameters.tipInfluence)
		{
			num *= Mathf.InverseLerp(branch.totalLenght, branch.totalLenght - ivyParameters.tipInfluence, leafPoint.lpLength);
		}
		return num;
	}

	private Quaternion CalculateLeafRotation(LeafPoint leafPoint)
	{
		Vector3 vector;
		Vector3 axis;
		if (!ivyParameters.globalOrientation)
		{
			vector = leafPoint.lpForward;
			axis = leafPoint.left;
		}
		else
		{
			vector = ivyParameters.globalRotation;
			axis = Vector3.Normalize(Vector3.Cross(ivyParameters.globalRotation, leafPoint.lpUpward));
		}
		Quaternion quaternion = Quaternion.LookRotation(leafPoint.lpUpward, vector);
		quaternion = Quaternion.AngleAxis(ivyParameters.rotation.x, axis) * Quaternion.AngleAxis(ivyParameters.rotation.y, leafPoint.lpUpward) * Quaternion.AngleAxis(ivyParameters.rotation.z, vector) * quaternion;
		return Quaternion.AngleAxis(UnityEngine.Random.Range(0f - ivyParameters.randomRotation.x, ivyParameters.randomRotation.x), axis) * Quaternion.AngleAxis(UnityEngine.Random.Range(0f - ivyParameters.randomRotation.y, ivyParameters.randomRotation.y), leafPoint.lpUpward) * Quaternion.AngleAxis(UnityEngine.Random.Range(0f - ivyParameters.randomRotation.z, ivyParameters.randomRotation.z), vector) * quaternion;
	}

	private void AddFallingPoint(RTBranchContainer branch)
	{
		Vector3 grabVector = branch.rotationOnFallIteration * branch.GetLastBranchPoint().grabVector;
		RTBranchPoint nextFreeBranchPoint = GetNextFreeBranchPoint();
		nextFreeBranchPoint.point = branch.branchPoints[branch.branchPoints.Count - 1].point + branch.growDirection * ivyParameters.stepSize;
		nextFreeBranchPoint.grabVector = grabVector;
		branch.AddBranchPoint(nextFreeBranchPoint, ivyParameters.stepSize);
		CalculateVerticesLastPoint(branch);
		if (UnityEngine.Random.value < ivyParameters.branchProvability && rtIvyContainer.branches.Count < ivyParameters.maxBranchs)
		{
			AddBranch(branch, branch.GetLastBranchPoint(), branch.branchPoints[branch.branchPoints.Count - 1].point, -branch.GetLastBranchPoint().grabVector);
		}
		if (ivyParameters.generateLeaves)
		{
			AddLeave(branch);
		}
	}

	private void CalculateVerticesLastPoint(RTBranchContainer rtBranchContainer)
	{
		if (rtBranchContainer.branchPoints.Count > 1)
		{
			RTBranchPoint rTBranchPoint = rtBranchContainer.branchPoints[rtBranchContainer.branchPoints.Count - 2];
			float radius = CalculateRadius(rTBranchPoint.length);
			Vector3 loopAxis = GetLoopAxis(rTBranchPoint, rtBranchContainer, rtIvyContainer, ivyGO);
			Vector3 firstVector = GetFirstVector(rTBranchPoint, rtBranchContainer, rtIvyContainer, ivyParameters, loopAxis);
			rTBranchPoint.CalculateCenterLoop(ivyGO);
			rTBranchPoint.CalculateVerticesLoop(ivyParameters, rtIvyContainer, ivyGO, firstVector, loopAxis, radius);
		}
	}

	private void AddLeave(RTBranchContainer branch)
	{
		if (branch.branchPoints.Count % (ivyParameters.leaveEvery + UnityEngine.Random.Range(0, ivyParameters.randomLeaveEvery)) == 0)
		{
			int chosenLeave = UnityEngine.Random.Range(0, ivyParameters.leavesPrefabs.Length);
			RTBranchPoint rTBranchPoint = branch.branchPoints[branch.branchPoints.Count - 2];
			RTBranchPoint rTBranchPoint2 = branch.branchPoints[branch.branchPoints.Count - 1];
			Vector3 point = Vector3.Lerp(rTBranchPoint.point, rTBranchPoint2.point, 0.5f);
			float leafScale = UnityEngine.Random.Range(ivyParameters.minScale, ivyParameters.maxScale);
			RTLeafPoint nextLeafPoint = GetNextLeafPoint();
			nextLeafPoint.SetValues(point, branch.totalLength, branch.growDirection, -branch.GetLastBranchPoint().grabVector, chosenLeave, rTBranchPoint, rTBranchPoint2, leafScale, ivyParameters);
			RTMeshData leafMeshData = leavesMeshesByChosenLeaf[nextLeafPoint.chosenLeave];
			nextLeafPoint.CreateVertices(ivyParameters, leafMeshData, ivyGO);
			branch.AddLeaf(nextLeafPoint);
		}
	}

	public void DeleteLastBranch()
	{
		rtIvyContainer.branches.RemoveAt(rtIvyContainer.branches.Count - 1);
	}

	public void AddBranch(RTBranchContainer branch, RTBranchPoint originBranchPoint, Vector3 point, Vector3 normal)
	{
		RTBranchContainer nextBranchContainer = GetNextBranchContainer();
		RTBranchPoint nextFreeBranchPoint = GetNextFreeBranchPoint();
		nextFreeBranchPoint.SetValues(point, -normal);
		nextBranchContainer.AddBranchPoint(nextFreeBranchPoint, ivyParameters.stepSize);
		nextBranchContainer.growDirection = Vector3.Normalize(Vector3.ProjectOnPlane(branch.growDirection, normal));
		nextBranchContainer.randomizeHeight = UnityEngine.Random.Range(4f, 8f);
		nextBranchContainer.currentHeight = branch.currentHeight;
		nextBranchContainer.heightParameter = branch.heightParameter;
		nextBranchContainer.branchSense = ChooseBranchSense();
		rtIvyContainer.AddBranch(nextBranchContainer);
		originBranchPoint.InitBranchInThisPoint(nextBranchContainer.branchNumber);
	}

	private void NewGrowDirection(RTBranchContainer branch)
	{
		branch.growDirection = Vector3.Normalize(Vector3.ProjectOnPlane(Quaternion.AngleAxis(Mathf.Sin((float)branch.branchSense * branch.totalLength * ivyParameters.directionFrequency * (1f + UnityEngine.Random.Range(0f - ivyParameters.directionRandomness, ivyParameters.directionRandomness))) * ivyParameters.directionAmplitude * ivyParameters.stepSize * 10f * Mathf.Max(ivyParameters.directionRandomness, 1f), branch.GetLastBranchPoint().grabVector) * branch.growDirection, branch.GetLastBranchPoint().grabVector));
	}

	private void NewGrowDirectionAfterWall(RTBranchContainer branch, Vector3 oldSurfaceNormal, Vector3 newSurfaceNormal)
	{
		branch.growDirection = Vector3.Normalize(Vector3.ProjectOnPlane(oldSurfaceNormal, newSurfaceNormal));
	}

	private void NewGrowDirectionFalling(RTBranchContainer branch)
	{
		Vector3 vector = Vector3.Lerp(branch.growDirection, ivyParameters.gravity, branch.fallIteration / 10f);
		vector = Quaternion.AngleAxis(Mathf.Sin((float)branch.branchSense * branch.totalLength * ivyParameters.directionFrequency * (1f + UnityEngine.Random.Range((0f - ivyParameters.directionRandomness) / 8f, ivyParameters.directionRandomness / 8f))) * ivyParameters.directionAmplitude * ivyParameters.stepSize * 5f * Mathf.Max(ivyParameters.directionRandomness / 8f, 1f), branch.GetLastBranchPoint().grabVector) * vector;
		vector = Quaternion.AngleAxis(Mathf.Sin((float)branch.branchSense * branch.totalLength * ivyParameters.directionFrequency / 2f * (1f + UnityEngine.Random.Range((0f - ivyParameters.directionRandomness) / 8f, ivyParameters.directionRandomness / 8f))) * ivyParameters.directionAmplitude * ivyParameters.stepSize * 5f * Mathf.Max(ivyParameters.directionRandomness / 8f, 1f), Vector3.Cross(branch.GetLastBranchPoint().grabVector, branch.growDirection)) * vector;
		branch.rotationOnFallIteration = Quaternion.FromToRotation(branch.growDirection, vector);
		branch.growDirection = vector;
	}

	private void NewGrowDirectionAfterFall(RTBranchContainer branch, Vector3 newSurfaceNormal)
	{
		branch.growDirection = Vector3.Normalize(Vector3.ProjectOnPlane(-branch.GetLastBranchPoint().grabVector, newSurfaceNormal));
	}

	private void NewGrowDirectionAfterGrab(RTBranchContainer branch, Vector3 newSurfaceNormal)
	{
		branch.growDirection = Vector3.Normalize(Vector3.ProjectOnPlane(branch.growDirection, newSurfaceNormal));
	}

	private void NewGrowDirectionAfterCorner(RTBranchContainer branch, Vector3 oldSurfaceNormal, Vector3 newSurfaceNormal)
	{
		branch.growDirection = Vector3.Normalize(Vector3.ProjectOnPlane(-oldSurfaceNormal, newSurfaceNormal));
	}

	public Vector3 GetFirstVector(RTBranchPoint rtBranchPoint, RTBranchContainer rtBranchContainer, RTIvyContainer rtIvyContainer, IvyParameters ivyParameters, Vector3 axis)
	{
		Vector3 zero = Vector3.zero;
		if (rtBranchContainer.branchNumber == 0 && rtBranchPoint.index == 0)
		{
			if (!ivyParameters.halfgeom)
			{
				return rtIvyContainer.firstVertexVector;
			}
			return Quaternion.AngleAxis(90f, axis) * rtIvyContainer.firstVertexVector;
		}
		if (!ivyParameters.halfgeom)
		{
			return Vector3.Normalize(Vector3.ProjectOnPlane(rtBranchPoint.grabVector, axis));
		}
		return Quaternion.AngleAxis(90f, axis) * Vector3.Normalize(Vector3.ProjectOnPlane(rtBranchPoint.grabVector, axis));
	}

	public Vector3 GetLoopAxis(RTBranchPoint rtBranchPoint, RTBranchContainer rtBranchContainer, RTIvyContainer rtIvyContainer, GameObject ivyGo)
	{
		Vector3 zero = Vector3.zero;
		if (rtBranchPoint.index == 0 && rtBranchContainer.branchNumber == 0)
		{
			return ivyGo.transform.up;
		}
		if (rtBranchPoint.index == 0)
		{
			return rtBranchPoint.GetNextPoint().point - rtBranchPoint.point;
		}
		return Vector3.Normalize(Vector3.Lerp(rtBranchPoint.point - rtBranchPoint.GetPreviousPoint().point, rtBranchPoint.GetNextPoint().point - rtBranchPoint.point, 0.5f));
	}

	private RTBranchPoint GetNextFreeBranchPoint()
	{
		RTBranchPoint result = branchPointsPool[branchPointPoolIndex];
		branchPointPoolIndex++;
		if (branchPointPoolIndex >= branchPointsPool.Length)
		{
			Array.Resize(ref branchPointsPool, branchPointsPool.Length * 2);
			for (int i = branchPointPoolIndex; i < branchPointsPool.Length; i++)
			{
				RTBranchPoint rTBranchPoint = new RTBranchPoint();
				rTBranchPoint.PreInit(ivyParameters);
				branchPointsPool[i] = rTBranchPoint;
			}
		}
		return result;
	}

	private RTLeafPoint GetNextLeafPoint()
	{
		RTLeafPoint result = leavesPool[leavesPoolIndex];
		leavesPoolIndex++;
		if (leavesPoolIndex >= leavesPool.Length)
		{
			Array.Resize(ref leavesPool, leavesPool.Length * 2);
			for (int i = leavesPoolIndex; i < leavesPool.Length; i++)
			{
				leavesPool[i] = new RTLeafPoint();
				leavesPool[i].PreInit(maxNumVerticesPerLeaf);
			}
		}
		return result;
	}

	private RTBranchContainer GetNextBranchContainer()
	{
		RTBranchContainer result = branchesPool[branchesPoolIndex];
		branchesPoolIndex++;
		return result;
	}
}
