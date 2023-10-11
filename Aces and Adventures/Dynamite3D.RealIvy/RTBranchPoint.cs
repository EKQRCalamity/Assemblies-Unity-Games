using UnityEngine;

namespace Dynamite3D.RealIvy;

public class RTBranchPoint
{
	public Vector3 point;

	public Vector3 grabVector;

	public float length;

	public int index;

	public bool newBranch;

	public int newBranchNumber;

	public float radius;

	public Vector3 firstVector;

	public Vector3 axis;

	public Vector3 centerLoop;

	public RTBranchContainer branchContainer;

	public RTVertexData[] verticesLoop;

	public Vector3 lastVectorNormal;

	public RTBranchPoint()
	{
	}

	public RTBranchPoint(BranchPoint branchPoint, RTBranchContainer rtBranchContainer)
	{
		point = branchPoint.point;
		grabVector = branchPoint.grabVector;
		length = branchPoint.length;
		index = branchPoint.index;
		newBranch = branchPoint.newBranch;
		newBranchNumber = branchPoint.newBranchNumber;
		branchContainer = rtBranchContainer;
		radius = branchPoint.radius;
		firstVector = branchPoint.firstVector;
		axis = branchPoint.axis;
	}

	public void PreInit(IvyParameters ivyParameters)
	{
		verticesLoop = new RTVertexData[ivyParameters.sides + 1];
	}

	public void SetValues(Vector3 point, Vector3 grabVector)
	{
		SetValues(point, grabVector, newBranch: false, -1);
	}

	public void SetValues(Vector3 point, Vector3 grabVector, bool newBranch, int newBranchNumber)
	{
		this.point = point;
		this.grabVector = grabVector;
		this.newBranch = newBranch;
		this.newBranchNumber = newBranchNumber;
	}

	public void InitBranchInThisPoint(int branchNumber)
	{
		newBranch = true;
		newBranchNumber = branchNumber;
	}

	public void CalculateVerticesLoop(IvyParameters ivyParameters, RTIvyContainer rtIvyContainer, GameObject ivyGO, Vector3 firstVector, Vector3 axis, float radius)
	{
		this.firstVector = firstVector;
		this.axis = axis;
		this.radius = radius;
		CalculateVerticesLoop(ivyParameters, rtIvyContainer, ivyGO);
	}

	public void CalculateVerticesLoop(IvyParameters ivyParameters, RTIvyContainer rtIvyContainer, GameObject ivyGO)
	{
		float num = 0f;
		num = (ivyParameters.halfgeom ? (360f / (float)ivyParameters.sides / 2f) : (360f / (float)ivyParameters.sides));
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		Vector2 zero3 = Vector2.zero;
		_ = Quaternion.identity;
		Vector3 zero4 = Vector3.zero;
		Quaternion quaternion = Quaternion.Inverse(ivyGO.transform.rotation);
		for (int i = 0; i < ivyParameters.sides + 1; i++)
		{
			zero4 = Quaternion.AngleAxis(num * (float)i, axis) * firstVector;
			zero2 = ((!ivyParameters.halfgeom || ivyParameters.sides != 1) ? zero4 : (-grabVector));
			zero2 = quaternion * zero2;
			zero = zero4 * radius + point;
			zero -= ivyGO.transform.position;
			zero = quaternion * zero;
			zero3 = new Vector2(length * ivyParameters.uvScale.y + ivyParameters.uvOffset.y - ivyParameters.stepSize, 1f / (float)ivyParameters.sides * (float)i * ivyParameters.uvScale.x + ivyParameters.uvOffset.x);
			verticesLoop[i] = new RTVertexData(zero, zero2, zero3, Vector2.zero, Color.black);
		}
	}

	public void CalculateCenterLoop(GameObject ivyGO)
	{
		centerLoop = Quaternion.Inverse(ivyGO.transform.rotation) * (point - ivyGO.transform.position);
		lastVectorNormal = ivyGO.transform.InverseTransformVector(grabVector);
	}

	public RTBranchPoint GetNextPoint()
	{
		return branchContainer.branchPoints[index + 1];
	}

	public RTBranchPoint GetPreviousPoint()
	{
		return branchContainer.branchPoints[index - 1];
	}
}
