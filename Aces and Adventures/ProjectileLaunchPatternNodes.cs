using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileLaunchPatternNodes
{
	private static readonly Quaternion UpRotation = Quaternion.Euler(-90f, 0f, 0f);

	public List<PositionRotation> nodes;

	public int Count => nodes.Count;

	public void RotateNodePositions(float pitch, float yaw, float roll)
	{
		Quaternion quaternion = Quaternion.Euler(pitch, yaw, roll);
		if (!(quaternion == Quaternion.identity))
		{
			for (int i = 0; i < nodes.Count; i++)
			{
				nodes[i] = new PositionRotation(quaternion * nodes[i].position, nodes[i].rotation);
			}
		}
	}

	public void SetNodeRotations(ProjectileMediaData.ShapeUpDirectionType upDirection)
	{
		for (int i = 0; i < nodes.Count; i++)
		{
			Vector3 position = nodes[i].position;
			switch (upDirection)
			{
			case ProjectileMediaData.ShapeUpDirectionType.Upward:
				nodes[i] = new PositionRotation(position, UpRotation);
				break;
			case ProjectileMediaData.ShapeUpDirectionType.Outward:
				nodes[i] = new PositionRotation(position, (position != Vector3.zero) ? Quaternion.LookRotation(position) : UpRotation);
				break;
			case ProjectileMediaData.ShapeUpDirectionType.Inward:
				nodes[i] = new PositionRotation(position, (position != Vector3.zero) ? Quaternion.LookRotation(-position) : UpRotation);
				break;
			}
		}
	}

	public void RotateNodeDirections(System.Random random, RangeInt pitch, RangeInt yaw, RangeInt roll)
	{
		if (pitch.absMax + yaw.absMax + roll.absMax != 0)
		{
			for (int i = 0; i < nodes.Count; i++)
			{
				nodes[i] = new PositionRotation(nodes[i].position, Quaternion.Euler(random.RangeInt(pitch), random.RangeInt(yaw), random.RangeInt(roll)) * nodes[i].rotation);
			}
		}
	}

	public void RandomizeNodeDirections(System.Random random, int coneAngle)
	{
		if (coneAngle > 0)
		{
			for (int i = 0; i < nodes.Count; i++)
			{
				nodes[i] = new PositionRotation(nodes[i].position, Quaternion.LookRotation(random.RandomInCone(nodes[i].rotation.Forward(), random.Range(0f, coneAngle))));
			}
		}
	}

	public void OffsetNodes(float horizontal, float vertical, float longitudinal)
	{
		Vector3 vector = new Vector3(horizontal, vertical, longitudinal);
		if (!(vector == Vector3.zero))
		{
			for (int i = 0; i < nodes.Count; i++)
			{
				nodes[i] = new PositionRotation(vector + nodes[i].position, nodes[i].rotation);
			}
		}
	}

	public PositionRotation GetNextPositionRotation(PositionRotation transform)
	{
		int index = nodes.Count - 1;
		PositionRotation result = transform * nodes[index];
		nodes.RemoveAt(index);
		return result;
	}

	private void OnUnpool()
	{
		Pools.TryUnpool(ref nodes);
	}

	private void Clear()
	{
		Pools.Repool(ref nodes);
	}

	public static implicit operator bool(ProjectileLaunchPatternNodes launchNodes)
	{
		if (launchNodes != null)
		{
			return launchNodes.Count > 0;
		}
		return false;
	}
}
