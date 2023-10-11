using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dynamite3D.RealIvy;

[Serializable]
public class LeafPoint
{
	public Vector3 point;

	public Vector2 pointSS;

	public float lpLength;

	public Vector3 left;

	public Vector3 lpForward;

	public Vector3 lpUpward;

	public int chosenLeave;

	public Quaternion forwarRot;

	public int initSegmentIdx;

	public int endSegmentIdx;

	public float displacementFromInitSegment;

	public Quaternion leafRotation;

	public float currentScale;

	public float dstScale;

	public Vector3 leafCenter;

	public List<RTVertexData> verticesLeaves;

	public float leafScale;

	public void InitializeRuntime()
	{
		verticesLeaves = new List<RTVertexData>(4);
	}

	public LeafPoint()
	{
	}

	public LeafPoint(Vector3 point, float lpLength, Vector3 lpForward, Vector3 lpUpward, int chosenLeave, BranchPoint initSegment, BranchPoint endSegment)
	{
		SetValues(point, lpLength, lpForward, lpUpward, chosenLeave, initSegment, endSegment);
	}

	public void SetValues(Vector3 point, float lpLength, Vector3 lpForward, Vector3 lpUpward, int chosenLeave, BranchPoint initSegment, BranchPoint endSegment)
	{
		this.point = point;
		this.lpLength = lpLength;
		this.lpForward = lpForward;
		this.lpUpward = lpUpward;
		this.chosenLeave = chosenLeave;
		initSegmentIdx = initSegment.index;
		endSegmentIdx = endSegment.index;
		forwarRot = Quaternion.identity;
		float magnitude = (initSegment.point - endSegment.point).magnitude;
		float value = (point - initSegment.point).magnitude / magnitude;
		displacementFromInitSegment = Mathf.Clamp(value, 0.01f, 0.99f);
		left = Vector3.Cross(lpForward, lpUpward).normalized;
	}

	public void DrawVectors()
	{
		Debug.DrawLine(point, point + lpForward * 0.25f, Color.red, 5f);
		Debug.DrawLine(point, point + lpUpward * 0.25f, Color.blue, 5f);
		Debug.DrawLine(point, point + left * 0.25f, Color.green, 5f);
	}

	public float GetLengthFactor(BranchContainer branchContainer, float correctionFactor)
	{
		if (!(lpLength <= branchContainer.totalLenght * 1.15f * correctionFactor))
		{
			return 0f;
		}
		return 1f;
	}

	public void CreateVertices(IvyParameters ivyParameters, RTMeshData leafMeshData, GameObject ivyGO)
	{
		Vector3 globalRotation;
		Vector3 vector;
		if (!ivyParameters.globalOrientation)
		{
			globalRotation = lpForward;
			vector = left;
		}
		else
		{
			globalRotation = ivyParameters.globalRotation;
			vector = Vector3.Normalize(Vector3.Cross(ivyParameters.globalRotation, lpUpward));
		}
		Quaternion quaternion = Quaternion.LookRotation(lpUpward, globalRotation);
		quaternion = Quaternion.AngleAxis(ivyParameters.rotation.x, vector) * Quaternion.AngleAxis(ivyParameters.rotation.y, lpUpward) * Quaternion.AngleAxis(ivyParameters.rotation.z, globalRotation) * quaternion;
		quaternion = Quaternion.AngleAxis(UnityEngine.Random.Range(0f - ivyParameters.randomRotation.x, ivyParameters.randomRotation.x), vector) * Quaternion.AngleAxis(UnityEngine.Random.Range(0f - ivyParameters.randomRotation.y, ivyParameters.randomRotation.y), lpUpward) * Quaternion.AngleAxis(UnityEngine.Random.Range(0f - ivyParameters.randomRotation.z, ivyParameters.randomRotation.z), globalRotation) * quaternion;
		quaternion = forwarRot * quaternion;
		float num = UnityEngine.Random.Range(ivyParameters.minScale, ivyParameters.maxScale);
		leafRotation = quaternion;
		leafCenter = point - ivyGO.transform.position;
		leafCenter = Quaternion.Inverse(ivyGO.transform.rotation) * leafCenter;
		if (verticesLeaves == null)
		{
			verticesLeaves = new List<RTVertexData>(4);
		}
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		Vector2 zero3 = Vector2.zero;
		Color black = Color.black;
		Quaternion quaternion2 = Quaternion.Inverse(ivyGO.transform.rotation);
		for (int i = 0; i < leafMeshData.vertices.Length; i++)
		{
			Vector3 vector2 = vector * ivyParameters.offset.x + lpUpward * ivyParameters.offset.y + lpForward * ivyParameters.offset.z;
			zero = quaternion * leafMeshData.vertices[i] * num + leafCenter + vector2;
			zero2 = quaternion * leafMeshData.normals[i];
			zero2 = quaternion2 * zero2;
			zero3 = leafMeshData.uv[i];
			black = leafMeshData.colors[i];
			RTVertexData item = new RTVertexData(zero, zero2, zero3, Vector2.zero, black);
			verticesLeaves.Add(item);
		}
	}
}
