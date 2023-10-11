using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public static class DebugIf
{
	[Conditional("UNITY_EDITOR")]
	public static void Log(bool condition, object message, Object context = null)
	{
		if (condition)
		{
			UnityEngine.Debug.Log(message, context);
		}
	}

	[Conditional("UNITY_EDITOR")]
	public static void LogWarning(bool condition, object message, Object context = null)
	{
		if (condition)
		{
			UnityEngine.Debug.LogWarning(message, context);
		}
	}

	[Conditional("UNITY_EDITOR")]
	public static void LogError(bool condition, object message, Object context = null)
	{
		if (condition)
		{
			UnityEngine.Debug.LogError(message, context);
		}
	}

	[Conditional("UNITY_EDITOR")]
	public static void DrawBounds(Bounds bounds, Color color, float duration = 0f)
	{
		List<Vector3> list = bounds.GetCorners().ToList();
		UnityEngine.Debug.DrawLine(list[3], list[0], color, duration);
		UnityEngine.Debug.DrawLine(list[7], list[4], color, duration);
		UnityEngine.Debug.DrawLine(list[3], list[7], color, duration);
		for (int i = 0; i < 3; i++)
		{
			int num = i + 4;
			UnityEngine.Debug.DrawLine(list[i], list[num], color, duration);
			UnityEngine.Debug.DrawLine(list[i], list[i + 1], color, duration);
			UnityEngine.Debug.DrawLine(list[num], list[num + 1], color, duration);
		}
	}

	[Conditional("UNITY_EDITOR")]
	public static void DrawRect3D(Rect3D rect3D, Color color, float duration = 0f)
	{
		using PoolStructListHandle<Vector3> poolStructListHandle = rect3D.Corners();
		UnityEngine.Debug.DrawLine(poolStructListHandle[0], poolStructListHandle[1], color, duration);
		UnityEngine.Debug.DrawLine(poolStructListHandle[1], poolStructListHandle[2], color, duration);
		UnityEngine.Debug.DrawLine(poolStructListHandle[2], poolStructListHandle[3], color, duration);
		UnityEngine.Debug.DrawLine(poolStructListHandle[3], poolStructListHandle[0], color, duration);
	}

	[Conditional("UNITY_EDITOR")]
	public static void DrawLineSegment(LineSegment lineSegment, Color color, float duration = 0f)
	{
		UnityEngine.Debug.DrawLine(lineSegment.a, lineSegment.b, color, duration);
	}
}
