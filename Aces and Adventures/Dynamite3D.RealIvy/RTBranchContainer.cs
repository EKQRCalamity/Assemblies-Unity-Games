using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dynamite3D.RealIvy;

public class RTBranchContainer
{
	public List<RTBranchPoint> branchPoints;

	public RTLeafPoint[][] leavesOrderedByInitSegment;

	public float totalLength;

	public Vector3 growDirection;

	public float randomizeHeight;

	public float heightVar;

	public float newHeight;

	public float heightParameter;

	public float deltaHeight;

	public float currentHeight;

	public int branchSense;

	public bool falling;

	public Quaternion rotationOnFallIteration;

	public float fallIteration;

	public int branchNumber;

	public RTBranchContainer(BranchContainer branchContainer, IvyParameters ivyParameters, RTIvyContainer rtIvyContainer, GameObject ivyGO, RTMeshData[] leavesMeshesByChosenLeaf)
	{
		totalLength = branchContainer.totalLenght;
		growDirection = branchContainer.growDirection;
		randomizeHeight = branchContainer.randomizeHeight;
		heightVar = branchContainer.heightVar;
		newHeight = branchContainer.newHeight;
		heightParameter = branchContainer.heightParameter;
		deltaHeight = branchContainer.deltaHeight;
		currentHeight = branchContainer.currentHeight;
		branchSense = branchContainer.branchSense;
		falling = branchContainer.falling;
		rotationOnFallIteration = branchContainer.rotationOnFallIteration;
		branchNumber = branchContainer.branchNumber;
		branchPoints = new List<RTBranchPoint>(branchContainer.branchPoints.Count);
		for (int i = 0; i < branchContainer.branchPoints.Count; i++)
		{
			RTBranchPoint rTBranchPoint = new RTBranchPoint(branchContainer.branchPoints[i], this);
			rTBranchPoint.CalculateCenterLoop(ivyGO);
			rTBranchPoint.PreInit(ivyParameters);
			rTBranchPoint.CalculateVerticesLoop(ivyParameters, rtIvyContainer, ivyGO);
			branchPoints.Add(rTBranchPoint);
		}
		branchContainer.PrepareRTLeavesDict();
		if (!ivyParameters.generateLeaves)
		{
			return;
		}
		leavesOrderedByInitSegment = new RTLeafPoint[branchPoints.Count][];
		for (int j = 0; j < branchPoints.Count; j++)
		{
			List<LeafPoint> list = branchContainer.dictRTLeavesByInitSegment[j];
			int num = 0;
			if (list != null)
			{
				num = list.Count;
			}
			leavesOrderedByInitSegment[j] = new RTLeafPoint[num];
			for (int k = 0; k < num; k++)
			{
				RTLeafPoint rTLeafPoint = new RTLeafPoint(list[k], ivyParameters);
				RTMeshData leafMeshData = leavesMeshesByChosenLeaf[rTLeafPoint.chosenLeave];
				rTLeafPoint.CreateVertices(ivyParameters, leafMeshData, ivyGO);
				leavesOrderedByInitSegment[j][k] = rTLeafPoint;
			}
		}
	}

	public Vector2 GetLastUV(IvyParameters ivyParameters)
	{
		return new Vector2(totalLength * ivyParameters.uvScale.y + ivyParameters.uvOffset.y, 0.5f * ivyParameters.uvScale.x + ivyParameters.uvOffset.x);
	}

	public RTBranchContainer(int numPoints, int numLeaves)
	{
		Init(numPoints, numLeaves);
	}

	private void Init(int numPoints, int numLeaves)
	{
		branchPoints = new List<RTBranchPoint>(numPoints);
		leavesOrderedByInitSegment = new RTLeafPoint[numPoints][];
		for (int i = 0; i < numPoints; i++)
		{
			leavesOrderedByInitSegment[i] = new RTLeafPoint[1];
		}
	}

	public void AddBranchPoint(RTBranchPoint rtBranchPoint, float deltaLength)
	{
		totalLength += deltaLength;
		rtBranchPoint.length = totalLength;
		rtBranchPoint.index = branchPoints.Count;
		rtBranchPoint.branchContainer = this;
		branchPoints.Add(rtBranchPoint);
	}

	public RTBranchPoint GetLastBranchPoint()
	{
		return branchPoints[branchPoints.Count - 1];
	}

	public void AddLeaf(RTLeafPoint leafAdded)
	{
		if (leafAdded.initSegmentIdx >= leavesOrderedByInitSegment.Length)
		{
			Array.Resize(ref leavesOrderedByInitSegment, leavesOrderedByInitSegment.Length * 2);
			for (int i = leafAdded.initSegmentIdx; i < leavesOrderedByInitSegment.Length; i++)
			{
				leavesOrderedByInitSegment[i] = new RTLeafPoint[1];
			}
		}
		leavesOrderedByInitSegment[leafAdded.initSegmentIdx][0] = leafAdded;
	}
}
