using System;
using UnityEngine;

public static class DebugUtilities
{
	private static class DebugDrawer
	{
		public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration)
		{
			Debug.DrawLine(start, end, color, duration, depthTest: false);
		}
	}

	private const int DefaultEllipseSegments = 20;

	private static readonly Color DefaultColor = Color.white;

	public static void DrawLine(Vector3 start, Vector3 end)
	{
		DrawLine(start, end, DefaultColor);
	}

	public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0f)
	{
		DebugDrawer.DrawLine(start, end, color, duration);
	}

	public static void DrawRay(Vector3 origin, Vector3 direction)
	{
		DrawRay(origin, direction, DefaultColor);
	}

	public static void DrawRay(Vector3 origin, Vector3 direction, Color color, float duration = 0f)
	{
		DebugDrawer.DrawLine(origin, origin + direction, color, duration);
	}

	public static void DrawBox2D(Vector2 origin, Vector2 size, float angle)
	{
		DrawBox2D(origin, size, angle, DefaultColor);
	}

	public static void DrawBox2D(Vector2 origin, Vector2 size, float angle, Color color, float duration = 0f)
	{
		Vector2 vector = size * 0.5f;
		Vector2 vector2 = origin + new Vector2(0f - vector.x, vector.y);
		Vector2 vector3 = origin + new Vector2(0f - vector.x, 0f - vector.y);
		Vector2 vector4 = origin + new Vector2(vector.x, vector.y);
		Vector2 vector5 = origin + new Vector2(vector.x, 0f - vector.y);
		if (!Mathf.Approximately(angle, 0f))
		{
			throw new Exception("Not supported in this library");
		}
		DebugDrawer.DrawLine(vector2, vector4, color, duration);
		DebugDrawer.DrawLine(vector4, vector5, color, duration);
		DebugDrawer.DrawLine(vector5, vector3, color, duration);
		DebugDrawer.DrawLine(vector3, vector2, color, duration);
	}

	public static void DrawVerticalPole(Vector3 center, float height)
	{
		DrawVerticalPole(center, height, DefaultColor);
	}

	public static void DrawVerticalPole(Vector3 center, float height, Color color, float duration = 0f)
	{
		DrawLine(center + Vector3.up * height, center - Vector3.up * height, color, duration);
	}

	public static void DrawHorizontalPole(Vector3 center, float width)
	{
		DrawHorizontalPole(center, width, DefaultColor);
	}

	public static void DrawHorizontalPole(Vector3 center, float width, Color color, float duration = 0f)
	{
		DrawLine(center + Vector3.right * width, center - Vector3.right * width, color, duration);
	}

	public static void DrawCircle2D(Vector3 position, float radius)
	{
		DrawCircle2D(position, radius, DefaultColor);
	}

	public static void DrawCircle2D(Vector3 position, float radius, Color color, float duration = 0f, bool depthTest = true)
	{
		DrawCircle(position, Vector3.forward, Vector3.up, radius, 20, color, duration, depthTest);
	}

	public static void DrawCircle(Vector3 position, Vector3 forward, Vector3 up, float radius, int segments = 20)
	{
		DrawCircle(position, forward, up, radius, segments, DefaultColor);
	}

	public static void DrawCircle(Vector3 position, Vector3 forward, Vector3 up, float radius, int segments, Color color, float duration = 0f, bool depthTest = true)
	{
		DrawEllipse(position, forward, up, radius, radius, segments, color, duration, depthTest);
	}

	public static void DrawEllipse(Vector3 pos, Vector3 forward, Vector3 up, float radiusX, float radiusY, int segments, Color color, float duration = 0f, bool depthTest = true)
	{
		float num = 0f;
		Quaternion quaternion = Quaternion.LookRotation(forward, up);
		Vector3 vector = Vector3.zero;
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < segments + 1; i++)
		{
			zero.x = Mathf.Sin((float)Math.PI / 180f * num) * radiusX;
			zero.y = Mathf.Cos((float)Math.PI / 180f * num) * radiusY;
			if (i > 0)
			{
				DebugDrawer.DrawLine(quaternion * vector + pos, quaternion * zero + pos, color, duration);
			}
			vector = zero;
			num += 360f / (float)segments;
		}
	}
}
