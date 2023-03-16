using System;
using UnityEngine;

public static class MathUtilities
{
	public const float Sqrt2 = 1.4142135f;

	public const float InverseSqrt2 = 0.70710677f;

	public const float TwoPi = (float)Math.PI * 2f;

	public const float HalfPi = (float)Math.PI / 2f;

	public static bool SameSign(float a, float b)
	{
		if (Mathf.Approximately(a, 0f) && Mathf.Approximately(b, 0f))
		{
			return true;
		}
		if (a > 0f && b > 0f)
		{
			return true;
		}
		if (a < 0f && b < 0f)
		{
			return true;
		}
		return false;
	}

	public static float LerpMapping(float value, float fromStart, float fromEnd, float toStart, float toEnd, bool clamp = false)
	{
		float num = (value - fromStart) / (fromEnd - fromStart);
		if (clamp)
		{
			num = Mathf.Max(0f, Mathf.Min(1f, num));
		}
		return toStart + (toEnd - toStart) * num;
	}

	public static float SqrDistanceToLine(Ray ray, Vector3 point)
	{
		return Vector3.Cross(ray.direction, point - ray.origin).sqrMagnitude;
	}

	public static float DistanceToLine(Ray ray, Vector3 point)
	{
		return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
	}

	public static float DecimalPart(float value)
	{
		if (value < 0f)
		{
			return value - Mathf.Ceil(value);
		}
		return value - Mathf.Floor(value);
	}

	public static int NextIndex(int currentIndex, int indexLength)
	{
		currentIndex++;
		if (currentIndex >= indexLength)
		{
			currentIndex = 0;
		}
		return currentIndex;
	}

	public static int PreviousIndex(int currentIndex, int indexLength)
	{
		currentIndex--;
		if (currentIndex < 0)
		{
			currentIndex = indexLength - 1;
		}
		return currentIndex;
	}

	public static bool LinesIntersect(Vector2 s1, Vector2 e1, Vector2 s2, Vector2 e2, out Vector2 intersectionPoint)
	{
		float num = e1.y - s1.y;
		float num2 = s1.x - e1.x;
		float num3 = num * s1.x + num2 * s1.y;
		float num4 = e2.y - s2.y;
		float num5 = s2.x - e2.x;
		float num6 = num4 * s2.x + num5 * s2.y;
		float num7 = num * num5 - num4 * num2;
		if (Mathf.Approximately(num7, 0f))
		{
			intersectionPoint = Vector2.zero;
			return false;
		}
		float num8 = 1f / num7;
		intersectionPoint = new Vector2((num5 * num3 - num2 * num6) * num8, (num * num6 - num4 * num3) * num8);
		return true;
	}

	public static Vector2 HadamardProduct(Vector2 v1, Vector2 v2)
	{
		return new Vector2(v1.x * v2.x, v1.y * v2.y);
	}

	public static Vector3 HadamardProduct(Vector3 v1, Vector3 v2)
	{
		return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
	}

	public static bool BetweenInclusive(int value, int min, int max)
	{
		return value >= min && value <= max;
	}

	public static bool BetweenInclusive(float value, float min, float max)
	{
		return value >= min && value <= max;
	}

	public static bool BetweenExclusive(float value, float min, float max)
	{
		return value > min && value < max;
	}

	public static bool BetweenInclusiveExclusive(float value, float min, float max)
	{
		return value >= min && value < max;
	}

	public static bool BetweenExclusiveInclusive(float value, float min, float max)
	{
		return value > min && value <= max;
	}

	public static float ClampAngleSoft(float angle)
	{
		if (angle >= (float)Math.PI * 2f)
		{
			angle -= (float)Math.PI * 2f;
		}
		else if (angle < 0f)
		{
			angle += (float)Math.PI * 2f;
		}
		return angle;
	}

	public static float DirectionToAngle(Vector2 direction)
	{
		return Mathf.Atan2(direction.y, direction.x) * 57.29578f;
	}

	public static Vector2 AngleToDirection(float angle)
	{
		float f = angle * ((float)Math.PI / 180f);
		return new Vector2(Mathf.Cos(f), Mathf.Sin(f));
	}

	public static Vector2 TrigonmetricVector(float t, float amplitude, float frequency, float phaseShift = 0f, float globalPhaseShift = 0f)
	{
		Vector2 result = default(Vector2);
		result.x = amplitude * Mathf.Cos(frequency * (t + phaseShift) + globalPhaseShift);
		result.y = amplitude * Mathf.Sin(frequency * (t + phaseShift) + globalPhaseShift);
		return result;
	}
}
