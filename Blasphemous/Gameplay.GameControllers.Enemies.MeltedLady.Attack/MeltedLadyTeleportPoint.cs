using System;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.MeltedLady.Attack;

public class MeltedLadyTeleportPoint : SpawnPoint
{
	[FoldoutGroup("Attack Settings", true, 0)]
	[Range(0f, 2f)]
	public float RndHorizontalTeleportPosition;

	[FoldoutGroup("Attack Settings", true, 0)]
	[Range(0f, 2f)]
	public float RndVerticalTeleportPosition;

	public bool EnableGizmoReference;

	public Vector3 TeleportPosition
	{
		get
		{
			Vector3 position = base.transform.position;
			float x = position.x + UnityEngine.Random.Range(0f - RndHorizontalTeleportPosition, RndHorizontalTeleportPosition);
			float y = position.y + UnityEngine.Random.Range(0f - RndVerticalTeleportPosition, RndVerticalTeleportPosition);
			return new Vector3(x, y, position.z);
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawIcon(base.transform.position, "Blasphemous/MeltedLadySpawnReference.png", allowScaling: true);
		if (EnableGizmoReference)
		{
			DrawCircle(RndHorizontalTeleportPosition, Color.magenta);
		}
	}

	private void DrawCircle(float radius, Color color)
	{
		Gizmos.color = color;
		float f = 0f;
		float x = radius * Mathf.Cos(f);
		float y = radius * Mathf.Sin(f);
		Vector3 vector = base.transform.position + new Vector3(x, y);
		Vector3 to = vector;
		for (f = 0.1f; f < (float)Math.PI * 2f; f += 0.1f)
		{
			x = radius * Mathf.Cos(f);
			y = radius * Mathf.Sin(f);
			Vector3 vector2 = base.transform.position + new Vector3(x, y);
			Gizmos.DrawLine(vector, vector2);
			vector = vector2;
		}
		Gizmos.DrawLine(vector, to);
	}
}
