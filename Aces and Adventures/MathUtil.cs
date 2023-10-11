using System;
using System.Collections.Generic;
using System.Linq;
using LibTessDotNet;
using UnityEngine;
using UnityEngine.EventSystems;

public static class MathUtil
{
	public const float TwoPI = MathF.PI * 2f;

	public const float PIOver2 = MathF.PI / 2f;

	public static readonly float ThreePIOver2 = 4.712389f;

	public static readonly float PIOver4 = MathF.PI / 4f;

	public static readonly float PIOver12 = MathF.PI / 12f;

	public static readonly float PIOver36 = MathF.PI / 36f;

	public static readonly float ThreePIOver4 = PIOver4 * 3f;

	public static readonly float SqrtTwo = Mathf.Sqrt(2f);

	public static readonly float OneOverSqrtTwo = 1f / SqrtTwo;

	public static readonly float CosOneDegree = Mathf.Cos(MathF.PI / 180f);

	public static readonly float SinMinusOneDegree = Mathf.Sin(-MathF.PI / 180f);

	public const float OneThird = 1f / 3f;

	public static readonly float TwoThirds = 2f / 3f;

	public static readonly float OneSixth = 1f / 6f;

	public static readonly float OneSeventh = 1f / 7f;

	public static readonly float OneEighth = 0.125f;

	public static readonly float OneMinusEpsilon = 1f - Mathf.Epsilon;

	public static readonly float BigEpsilon = (float)Math.Pow(2.0, -16.0);

	public static readonly float Epsilon = (float)Math.Pow(2.0, -32.0);

	public static readonly float NegEpsilon = 0f - Epsilon;

	public static readonly float LargeNumber = (float)Math.Pow(2.0, 30.0);

	public static readonly float OneOneTwentyEigth = 1f / 128f;

	public static readonly float OneTwoFiftyFifth = 0.003921569f;

	public static readonly float TwoFiftyFivePow2 = 65025f;

	public static readonly float TwoFiftyFivePow3 = 16581375f;

	public static readonly float OneTwoFiftyFifthPow2 = 1f / TwoFiftyFivePow2;

	public static readonly float OneTwoFiftyFifthPow3 = 1f / TwoFiftyFivePow3;

	public static readonly float TwoPow15 = (float)Math.Pow(2.0, 15.0);

	public static readonly float OneOverTwoPow15 = 1f / TwoPow15;

	public static readonly float OneOver60 = 1f / 60f;

	public static readonly float OneOver30 = 1f / 30f;

	public static readonly float DiscretePhysicsRate = 1f / 144f;

	public static readonly float TimeSpanTickToSecond = (float)Math.Pow(10.0, 7.0);

	public const long HALF_LONG = 4611686018427387903L;

	public const int RAYHIT_CACHE_SIZE = 256;

	public const float E = MathF.E;

	public const float ConstEpsilon = 2.5E-10f;

	public static readonly Vector3[] CardinalDirections = new Vector3[4]
	{
		Vector3.forward,
		Vector3.right,
		Vector3.back,
		Vector3.left
	};

	public static readonly Int2[] CardinalDirectionsInt2 = new Int2[4]
	{
		Vector3.forward.ToInt2(),
		Vector3.right.ToInt2(),
		Vector3.back.ToInt2(),
		Vector3.left.ToInt2()
	};

	public const float MinWRandomWeight = 0.001f;

	public const float MaxWRandomWeight = 1000f;

	public static readonly System.Random random = new System.Random();

	public static readonly AxisType[] Axes = new AxisType[3]
	{
		AxisType.X,
		AxisType.Y,
		AxisType.Z
	};

	private static readonly Vector3[] _FrustumDirections = new Vector3[6]
	{
		Vector3.right,
		Vector3.up,
		Vector3.forward,
		-Vector3.right,
		-Vector3.up,
		-Vector3.forward
	};

	private static readonly DateTime EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	private static readonly string NO_SCIENTIFIC_NOTATION_FORMAT = "0." + new string('#', 38);

	private static readonly int[] _GuidByteOrder = new int[16]
	{
		15, 14, 13, 12, 11, 10, 9, 8, 6, 7,
		4, 5, 0, 1, 2, 3
	};

	public static int RoundToIntWithRoundOff(float value, ref float roundOff)
	{
		float num = value + roundOff;
		int num2 = Mathf.RoundToInt(num);
		roundOff = num - (float)num2;
		return num2;
	}

	public static float RoundToNearestMultipleOf(float value, float multipleOf)
	{
		return (float)(int)(value / multipleOf + 0.5f * Mathf.Sign(value)) * multipleOf;
	}

	public static Vector2 RoundToNearestMultipleOf(this Vector2 value, Vector2 multipleOf)
	{
		return new Vector2((multipleOf.x > 0f) ? RoundToNearestMultipleOf(value.x, multipleOf.x) : value.x, (multipleOf.y > 0f) ? RoundToNearestMultipleOf(value.y, multipleOf.y) : value.y);
	}

	public static int RoundToNearestMultipleOfInt(float value, int multipleOf = 1)
	{
		if (multipleOf == 1)
		{
			return Mathf.RoundToInt(value);
		}
		return (int)(RoundToNearestMultipleOf(value, multipleOf) + 0.5f * Mathf.Sign(value));
	}

	public static float FloorToNearestMultipleOf(float value, float multipleOf)
	{
		return (float)Mathf.FloorToInt(value / multipleOf) * multipleOf;
	}

	public static int FloorToNearestMultipleOfInt(float value, int multipleOf = 1)
	{
		if (multipleOf == 1)
		{
			return Mathf.FloorToInt(value);
		}
		return (int)(FloorToNearestMultipleOf(value, multipleOf) + 0.5f * Mathf.Sign(value));
	}

	public static float CeilingToNearestMultipleOf(float value, float multipleOf)
	{
		return (float)Mathf.CeilToInt(value / multipleOf) * multipleOf;
	}

	public static int CeilingToNearestMultipleOfInt(float value, int multipleOf = 1)
	{
		if (multipleOf == 1)
		{
			return Mathf.CeilToInt(value);
		}
		return (int)Math.Ceiling(value / (float)multipleOf) * multipleOf;
	}

	public static float RoundToNearestFactorOf(float value, float factorOf)
	{
		if (value >= factorOf)
		{
			return factorOf;
		}
		float num = factorOf % value;
		return value - ((num >= value * 0.5f) ? (value - num) : num);
	}

	public static int RoundToNearestFactorOf(int value, int factorOf)
	{
		if (value >= factorOf)
		{
			return factorOf;
		}
		int num = Math.Max(value - 1, factorOf - value);
		for (int i = 0; i < num; i++)
		{
			int num2 = value + i;
			if (num2 <= factorOf && factorOf % num2 == 0)
			{
				return num2;
			}
			if (value > i && factorOf % (value - i) == 0)
			{
				return value - i;
			}
		}
		return value;
	}

	public static float RoundToNearestPowerOf(float value, float powerOf)
	{
		return (float)Math.Pow(powerOf, Math.Round(Math.Log(value, powerOf)));
	}

	public static int RoundToNearestPowerOfInt(float value, float powerOf)
	{
		return (int)(RoundToNearestPowerOf(value, powerOf) + 0.5f * Mathf.Sign(value));
	}

	public static float CeilingToNearestPowerOf(float value, float powerOf)
	{
		return (float)Math.Pow(powerOf, Math.Ceiling(Math.Log(value, powerOf)));
	}

	public static int CeilingToNearestPowerOfInt(float value, float powerOf)
	{
		return (int)(CeilingToNearestPowerOf(value, powerOf) + 0.5f * Mathf.Sign(value));
	}

	public static float FloatModulus(float numerator, float denomenator)
	{
		float num = numerator / denomenator;
		return (num - (float)(int)num) * denomenator;
	}

	public static int RoundToInt(double value)
	{
		return (int)Math.Round(value);
	}

	public static int CeilToInt(double value)
	{
		return (int)Math.Ceiling(value);
	}

	public static int Modulus(this int value, int modulus)
	{
		if (value >= 0)
		{
			return value % modulus;
		}
		return -(-value % modulus);
	}

	public static float SignedRatioOf(this float a, float b)
	{
		b = b.InsureNonZero();
		if (a >= 0f)
		{
			if (b > 0f)
			{
				return a / b;
			}
			return float.MaxValue;
		}
		if (b >= 0f)
		{
			return float.MinValue;
		}
		return b / a;
	}

	public static void Wrap(ref int value, int adjustment, int min, int max)
	{
		value += adjustment;
		int num = max - min;
		if (value >= max)
		{
			value = (value - min) % num + min;
		}
		else if (value < min)
		{
			value = (max - (min - value) % num) % max + min;
		}
	}

	public static int Wrap(int value, int adjustment, int min, int max)
	{
		Wrap(ref value, adjustment, min, max);
		return value;
	}

	public static void Swap<T>(ref T a, ref T b)
	{
		T val = a;
		a = b;
		b = val;
	}

	public static float CubicSplineInterpolant(float t)
	{
		return (3f - 2f * t) * t * t;
	}

	public static float CubicSplineInterpolantToLinear(float t)
	{
		return SolveCubicEquation(-2f, 3f, 0f, 0f - t).z;
	}

	public static int ClosestInt(int targetValue, params int[] validValues)
	{
		int num = int.MaxValue;
		int result = validValues[0];
		for (int i = 0; i < validValues.Length; i++)
		{
			int num2 = targetValue - validValues[i];
			num2 *= num2;
			if (num2 < num)
			{
				num = num2;
				result = validValues[i];
			}
		}
		return result;
	}

	public static int Compare(float a, float b)
	{
		if (!(a > b))
		{
			if (!(a < b))
			{
				return 0;
			}
			return -1;
		}
		return 1;
	}

	public static int Compare(double a, double b)
	{
		if (!(a > b))
		{
			if (!(a < b))
			{
				return 0;
			}
			return -1;
		}
		return 1;
	}

	public static int Compare(uint a, uint b)
	{
		if (a <= b)
		{
			if (a >= b)
			{
				return 0;
			}
			return -1;
		}
		return 1;
	}

	public static float GetLerpAmount(float start, float end, float currentAmount)
	{
		float num = end - start;
		if (num == 0f)
		{
			return (currentAmount >= start) ? 1 : 0;
		}
		return (currentAmount - start) / num;
	}

	public static float GetLerpAmountClamped(float start, float end, float currentAmount)
	{
		return Mathf.Clamp01(GetLerpAmount(start, end, currentAmount));
	}

	private static float? _QuadraticEquationIsLinearCheck(float a, float b, float c)
	{
		if (a != 0f || b == 0f)
		{
			return null;
		}
		return (0f - c) / b;
	}

	public static float QuadraticPositive(float a, float b, float c)
	{
		return _QuadraticEquationIsLinearCheck(a, b, c) ?? ((0f - b + (float)Math.Sqrt(b * b - 4f * a * c)) / (a + a));
	}

	public static float QuadraticNegative(float a, float b, float c)
	{
		return _QuadraticEquationIsLinearCheck(a, b, c) ?? ((0f - b - (float)Math.Sqrt(b * b - 4f * a * c)) / (a + a));
	}

	public static float QuadraticSmallestReal(float a, float b, float c)
	{
		float num = QuadraticNegative(a, b, c);
		if (num == num)
		{
			return num;
		}
		return QuadraticPositive(a, b, c);
	}

	public static PoolStructListHandle<float> QuadraticInRange(float a, float b, float c, float rangeMin = float.MinValue, float rangeMax = float.MaxValue)
	{
		PoolStructListHandle<float> poolStructListHandle = Pools.UseStructList<float>();
		float num = QuadraticNegative(a, b, c);
		if (num >= rangeMin && num <= rangeMax)
		{
			poolStructListHandle.Add(num);
		}
		float num2 = QuadraticPositive(a, b, c);
		if (num2 >= rangeMin && num2 <= rangeMax)
		{
			poolStructListHandle.Add(num2);
		}
		return poolStructListHandle;
	}

	public static int ToPercentInt(float ratio)
	{
		return (int)(ratio * 100f + ((ratio < 0.99f || ratio > 1f) ? 0.5f : 0f));
	}

	public static int ToPercentIntSigned(float ratio)
	{
		int num = Math.Sign(ratio);
		return ToPercentInt(ratio * (float)num) * num;
	}

	public static float SumMultiplesOf(float multiplesOf, float count)
	{
		return multiplesOf * 0.5f * (count + 1f) * count;
	}

	public static int SumMultiplesOf(int multiplesOf, int count)
	{
		return multiplesOf * ((count + 1) * count) / 2;
	}

	public static float SumMultiplesOf(float multiplesOf, int startIndex, int endIndex)
	{
		return SumMultiplesOf(multiplesOf, endIndex) - SumMultiplesOf(multiplesOf, startIndex);
	}

	public static float ChanceMultitap(float chance, int numTaps)
	{
		chance = Mathf.Clamp01(chance);
		return 1f - Mathf.Pow(1f - chance, 1f / (float)numTaps);
	}

	public static List<int> IntPermutation(int minVal, int maxVal, int targetSum, int maxElements, float distribution = 0.5f, System.Random random = null)
	{
		List<int> list = new List<int>();
		int num = 0;
		distribution = Mathf.Clamp(distribution, Mathf.Epsilon, 1f);
		float p = ((distribution > 0.5f) ? ((1f - distribution) * 2f) : (1f / (distribution * 2f)));
		while (num != targetSum && list.Count < maxElements)
		{
			int num2 = maxElements - list.Count - 1;
			int num3 = 0;
			if (num2 > 0)
			{
				while (minVal < maxVal && num + minVal + num2 * maxVal < targetSum)
				{
					minVal++;
				}
				float f = ((random != null) ? ((float)random.NextDouble()) : UnityEngine.Random.value);
				num3 = LerpInt(minVal, maxVal, Mathf.Pow(f, p));
			}
			else
			{
				num3 = Mathf.Clamp(targetSum - num, minVal, maxVal);
			}
			num3 = Mathf.Clamp(num3, minVal, targetSum - num);
			num += num3;
			list.Add(num3);
		}
		return list;
	}

	public static List<int> IntPermutation(int targetSum, int maxElements, float distribution, System.Random random, params int[] validValuesAscending)
	{
		List<int> list = new List<int>();
		int num = 0;
		distribution = Mathf.Clamp(distribution, Mathf.Epsilon, 1f);
		float p = ((distribution > 0.5f) ? ((1f - distribution) * 2f) : (1f / (distribution * 2f)));
		int i = 0;
		int num2 = validValuesAscending.Length - 1;
		while (num != targetSum && list.Count < maxElements)
		{
			int num3 = maxElements - list.Count - 1;
			int num4 = 0;
			if (num3 > 0)
			{
				for (; i < num2 && num + validValuesAscending[i] + num3 * validValuesAscending[num2] < targetSum; i++)
				{
				}
				float f = ((random != null) ? ((float)random.NextDouble()) : UnityEngine.Random.value);
				num4 = validValuesAscending[LerpInt(i, num2, Mathf.Pow(f, p))];
			}
			else
			{
				num4 = ClosestInt(Mathf.Clamp(targetSum - num, validValuesAscending[i], validValuesAscending[num2]), validValuesAscending);
			}
			num4 = ClosestInt(Mathf.Clamp(num4, validValuesAscending[i], targetSum - num), validValuesAscending);
			num += num4;
			list.Add(num4);
		}
		return list;
	}

	public static float Frac(float f)
	{
		return f - (float)(int)f;
	}

	public static bool IsHalfFrac(float f, float tolerenace = 2.5E-10f)
	{
		float num = f - (float)(int)f;
		float num2 = ((num > 0f) ? num : (0f - num)) - 0.5f;
		return ((num2 > 0f) ? num2 : (0f - num2)) < tolerenace;
	}

	public static int Truncate(float value, ref float accumuleted, float roundingAmount = 0f)
	{
		float num = value + roundingAmount;
		int num2 = (int)value;
		int result = num2;
		int num3 = (int)(num + accumuleted);
		if (num3 > num2)
		{
			float num4 = Mathf.Max(0f, (float)num3 - num);
			accumuleted -= num4;
			result = num3;
		}
		else
		{
			accumuleted += value - (float)num2;
		}
		return result;
	}

	public static float GetToggleTime(float currentTime, float lastToggleTime, float transitionTime)
	{
		return currentTime - Mathf.Clamp01(1f - (currentTime - lastToggleTime) / transitionTime) * transitionTime;
	}

	public static float PowExtrema(float value, float power, float extremaPower)
	{
		value = (float)Math.Pow(value, power);
		return (float)Math.Pow(value, Math.Pow(2.7182817459106445, (0.5f - value) * Math.Abs(extremaPower)));
	}

	public static void OpposingProbabilityCalculator(int minValue = 2, int maxValue = 14, int minBonus = -13, int maxBonus = 13, int criticalFailure = 2, int criticalSuccess = 14, int iterations = 20000)
	{
		System.Random random = new System.Random();
		int maxValue2 = maxValue + 1;
		float num = 1f / (float)iterations;
		for (int i = minBonus; i < maxBonus; i++)
		{
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			int num7 = 0;
			for (int j = 0; j < iterations; j++)
			{
				int num8 = random.Next(minValue, maxValue2);
				int num9 = num8 + i;
				int val = random.Next(minValue, maxValue2);
				int num10 = Math.Max(num8, val);
				int num11 = Math.Min(num8, val);
				int num12 = num10 + i;
				int num13 = num11 + i;
				int num14 = random.Next(minValue, maxValue2);
				if ((num8 > criticalFailure || num14 <= criticalFailure) && (num14 < criticalSuccess || num8 >= criticalSuccess))
				{
					if (num14 <= criticalFailure && num8 > criticalFailure)
					{
						num2++;
					}
					else if (num8 >= criticalSuccess && num14 < criticalSuccess)
					{
						num2++;
					}
					else if (num9 > num14)
					{
						num2++;
					}
					else if (num9 == num14)
					{
						num3++;
					}
				}
				if ((num10 > criticalFailure || num14 <= criticalFailure) && (num14 < criticalSuccess || num10 >= criticalSuccess))
				{
					if (num14 <= criticalFailure && num10 > criticalFailure)
					{
						num4++;
					}
					else if (num10 >= criticalSuccess && num14 < criticalSuccess)
					{
						num4++;
					}
					else if (num12 > num14)
					{
						num4++;
					}
					else if (num12 == num14)
					{
						num5++;
					}
				}
				if ((num11 > criticalFailure || num14 <= criticalFailure) && (num14 < criticalSuccess || num11 >= criticalSuccess))
				{
					if (num14 <= criticalFailure && num11 > criticalFailure)
					{
						num6++;
					}
					else if (num11 >= criticalSuccess && num14 < criticalSuccess)
					{
						num6++;
					}
					else if (num13 > num14)
					{
						num6++;
					}
					else if (num13 == num14)
					{
						num7++;
					}
				}
			}
			Debug.Log("Stat Bonus of [" + i + "]: Win% = " + ToPercentInt((float)num2 * num) + ", Draw% = " + ToPercentInt((float)num3 * num) + ", WinWithDraw% = " + ToPercentInt((float)(num2 + num3) * num));
			Debug.Log("Stat Bonus of [" + i + "] with [Advantage]: Win% = " + ToPercentInt((float)num4 * num) + ", Draw% = " + ToPercentInt((float)num5 * num) + ", WinWithDraw% = " + ToPercentInt((float)(num4 + num5) * num));
			Debug.Log("Stat Bonus of [" + i + "] with [Disadvantage]: Win% = " + ToPercentInt((float)num6 * num) + ", Draw% = " + ToPercentInt((float)num7 * num) + ", WinWithDraw% = " + ToPercentInt((float)(num6 + num7) * num));
		}
	}

	public static Vector4 SolveCubicEquation(float a, float b, float c, float d)
	{
		Vector4 result = new Vector4(float.MinValue, float.MinValue, float.MinValue, 0f);
		if (Math.Abs(a) > Epsilon)
		{
			float num = a;
			a = b / num;
			b = c / num;
			c = d / num;
			float num2 = b - a * a / 3f;
			float num3 = a * (2f * a * a - 9f * b) / 27f + c;
			float num4 = num2 * num2 * num2;
			float num5 = num3 * num3 + 4f * num4 / 27f;
			float num6 = (0f - a) / 3f;
			if (num5 > Epsilon)
			{
				num = Mathf.Sqrt(num5);
				float num7 = (0f - num3 + num) / 2f;
				float num8 = (0f - num3 - num) / 2f;
				num7 = ((num7 >= 0f) ? Mathf.Pow(num7, 1f / 3f) : (0f - Mathf.Pow(0f - num7, 1f / 3f)));
				num8 = ((num8 >= 0f) ? Mathf.Pow(num8, 1f / 3f) : (0f - Mathf.Pow(0f - num8, 1f / 3f)));
				result.x = num7 + num8 + num6;
				result.w = 1f;
				return result;
			}
			if (num5 < 0f - Epsilon)
			{
				float num9 = 2f * Mathf.Sqrt((0f - num2) / 3f);
				float num10 = Mathf.Acos((0f - Mathf.Sqrt(-27f / num4)) * num3 / 2f) * (1f / 3f);
				result.x = num9 * Mathf.Cos(num10) + num6;
				result.y = num9 * Mathf.Cos(num10 + MathF.PI * 2f / 3f) + num6;
				result.z = num9 * Mathf.Cos(num10 + 4.1887903f) + num6;
				result.w = 3f;
				return result;
			}
			float num11 = ((num3 < 0f) ? Mathf.Pow((0f - num3) * 0.5f, 1f / 3f) : (0f - Mathf.Pow(num3 * 0.5f, 1f / 3f)));
			result.x = 2f * num11 + num6;
			result.y = 0f - num11 + num6;
			result.w = 2f;
			return result;
		}
		a = b;
		b = c;
		c = d;
		if (Math.Abs(a) <= Epsilon)
		{
			if (Math.Abs(b) <= Epsilon)
			{
				return result;
			}
			result.x = (0f - c) / b;
			result.w = 1f;
			return result;
		}
		float num12 = b * b - 4f * a * c;
		if (num12 <= 0f - Epsilon)
		{
			return result;
		}
		if (num12 > Epsilon)
		{
			num12 = Mathf.Sqrt(num12);
			result.x = (0f - b - num12) / (2f * a);
			result.y = (0f - b + num12) / (2f * a);
			result.w = 2f;
			return result;
		}
		if (num12 < 0f - Epsilon)
		{
			return result;
		}
		result.x = (0f - b) / (2f * a);
		result.w = 1f;
		return result;
	}

	public static float Remap(float value, Vector2 range, Vector2 newRange)
	{
		return Mathf.Lerp(newRange.x, newRange.y, GetLerpAmount(range.x, range.y, value));
	}

	public static float Remap(float value, Vector2 range, Vector2 newRange, AnimationCurve curve)
	{
		return Mathf.Lerp(newRange.x, newRange.y, curve.Evaluate(GetLerpAmount(range.x, range.y, value)));
	}

	public static Vector2 Remap(Vector2 value, Vector2 range, Vector2 newRange)
	{
		return new Vector2(Remap(value.x, range, newRange), Remap(value.y, range, newRange));
	}

	public static Vector3 Remap(Vector3 value, Vector2 range, Vector2 newRange)
	{
		return new Vector3(Remap(value.x, range, newRange), Remap(value.y, range, newRange), Remap(value.z, range, newRange));
	}

	public static float RemapUnclamped(float value, Vector2 range, Vector2 newRange)
	{
		return Lerp(newRange.x, newRange.y, GetLerpAmount(range.x, range.y, value));
	}

	public static float RemapUnclamped(float value, Vector2 range, Vector2 newRange, AnimationCurve curve)
	{
		return Lerp(newRange.x, newRange.y, curve.Evaluate(GetLerpAmount(range.x, range.y, value)));
	}

	public static float SignedPow(float value, float pow)
	{
		int num = Math.Sign(value);
		return (float)Math.Pow(value * (float)num, pow) * (float)num;
	}

	public static float PerlinNoise(float x, float y, float extremaPower = 1f, float min = -1f, float max = 1f, bool cubicInterpolation = true)
	{
		float num = Mathf.PerlinNoise(x, y);
		if (cubicInterpolation)
		{
			num = CubicSplineInterpolant(num);
		}
		if (extremaPower != 1f)
		{
			num = LerpExtrema(0f, 1f, num, extremaPower);
		}
		return Remap(num, new Vector2(0f, 1f), new Vector2(min, max));
	}

	public static float PowSigned(float value, float power)
	{
		int num = Math.Sign(value);
		return Mathf.Pow(value * (float)num, power) * (float)num;
	}

	public static int ToNextInt(float value)
	{
		if (!(value >= 0f))
		{
			return Mathf.FloorToInt(value);
		}
		return Mathf.CeilToInt(value);
	}

	public static int? Max(int? a, int? b)
	{
		if (!a.HasValue)
		{
			return b;
		}
		return b.HasValue ? Math.Max(a.Value, b.Value) : a.Value;
	}

	public static int? Max(int a, int? b)
	{
		if (!b.HasValue)
		{
			return b;
		}
		return Math.Max(a, b.Value);
	}

	public static float MoveToward(ref float position, float targetPosition, float speed, float time)
	{
		float num = targetPosition - position;
		int num2 = Math.Sign(num);
		position += Mathf.Min(num * (float)num2, speed * time) * (float)num2;
		return position;
	}

	public static bool MoveTowardAndStop(ref float position, float targetPosition, float speed, float time)
	{
		if (position == targetPosition)
		{
			return true;
		}
		float num = targetPosition - position;
		int num2 = Math.Sign(num);
		position += (float)num2 * Mathf.Min(num * (float)num2, speed * time);
		return position == targetPosition;
	}

	public static float MoveTowardAndStop(float position, float targetPosition, float speed, float time)
	{
		if (position == targetPosition)
		{
			return position;
		}
		float num = targetPosition - position;
		int num2 = Math.Sign(num);
		position += (float)num2 * Mathf.Min(num * (float)num2, speed * time);
		return position;
	}

	public static float CalculateTimeInSecondsForEaseComplete(float easeStiffness, float easeDistance, float distanceDelta)
	{
		return 1f / (0f - easeStiffness) * Mathf.Log(Mathf.Min(easeDistance, distanceDelta) / easeDistance);
	}

	public static float CalculateDistanceForEaseToCompleteInGivenTime(float easeStiffness, float easeTimeInSeconds, float distanceDelta)
	{
		return distanceDelta * Mathf.Pow(MathF.E, easeStiffness * easeTimeInSeconds);
	}

	public static float CalculateDistanceForEaseToCompleteInGivenTime2(float easeStiffness, float distance, float distanceDelta)
	{
		return distanceDelta * Mathf.Pow(MathF.E, easeStiffness * CalculateTimeInSecondsForEaseComplete(easeStiffness, Mathf.Abs(distance) + Mathf.Epsilon, distanceDelta));
	}

	public static float CalculateEaseStiffnessToCompleteEaseOfGivenTimeAndDistance(float easeTimeInSeconds, float easeDistance, float distanceDelta)
	{
		easeTimeInSeconds = Mathf.Max(Mathf.Epsilon, easeTimeInSeconds);
		easeDistance = Mathf.Max(Mathf.Epsilon, easeDistance);
		distanceDelta = Mathf.Max(Mathf.Epsilon, distanceDelta);
		distanceDelta = Mathf.Min(easeDistance, distanceDelta);
		return Mathf.Clamp01(1f / (0f - easeTimeInSeconds) * Mathf.Log(distanceDelta / easeDistance));
	}

	public static float CalculateEaseStiffnessSubjectToTime(float easeStiffness, float time)
	{
		return Mathf.Clamp01(1f - (float)Math.Pow(2.7182817459106445, (0f - easeStiffness) * time));
	}

	public static void Ease(ref float value, float desiredValue, float easeStiffnessSubjectToTime)
	{
		value += (desiredValue - value) * easeStiffnessSubjectToTime;
	}

	public static void Ease(ref float value, float desiredValue, float easeStiffness, float time)
	{
		value += (desiredValue - value) * CalculateEaseStiffnessSubjectToTime(easeStiffness, time);
	}

	public static void EaseSnap(ref float value, float desiredValue, float easeStiffness, float time, float snapDistance = 0.01f)
	{
		value = DeltaSnap(Ease(value, desiredValue, easeStiffness, time), desiredValue, snapDistance);
	}

	public static void EasePow(ref float value, float desiredValue, float easeStiffness, float time, float pow)
	{
		EasePow(ref value, desiredValue, CalculateEaseStiffnessSubjectToTime(easeStiffness, time), pow);
	}

	public static void EasePow(ref float value, float desiredValue, float easeStiffnessSubjectToTime, float pow)
	{
		float num = desiredValue - value;
		float num2 = Mathf.Sign(num);
		float num3 = num * num2;
		value += Mathf.Min(num3, Mathf.Pow(num3, pow) * easeStiffnessSubjectToTime) * num2;
	}

	public static float Ease(float value, float desiredValue, float easeStiffnessSubjectToTime)
	{
		Ease(ref value, desiredValue, easeStiffnessSubjectToTime);
		return value;
	}

	public static float Ease(float value, float desiredValue, float easeStiffness, float time)
	{
		Ease(ref value, desiredValue, easeStiffness, time);
		return value;
	}

	public static float EaseSnap(float value, float desiredValue, float easeStiffness, float time, float snapDistance = 0.01f)
	{
		Ease(ref value, desiredValue, easeStiffness, time);
		return DeltaSnap(value, desiredValue, snapDistance);
	}

	public static void EaseIn(ref float value, float desiredValue, float easeStiffness, float time, float deltaSnap = 0.01f)
	{
		if (!DeltaSnap(ref value, desiredValue, deltaSnap))
		{
			float num = desiredValue - value;
			int num2 = Math.Sign(num);
			num *= (float)num2;
			value += Math.Min(num, CalculateEaseStiffnessSubjectToTime(easeStiffness, time) / num) * (float)num2;
		}
	}

	public static void EaseV2(ref Vector2 v, Vector2 target, float easeStiffness, float time)
	{
		easeStiffness = CalculateEaseStiffnessSubjectToTime(easeStiffness, time);
		v.x += (target.x - v.x) * easeStiffness;
		v.y += (target.y - v.y) * easeStiffness;
	}

	public static Vector3 EaseV3(ref Vector3 v, Vector3 target, float easeStiffness, float time)
	{
		easeStiffness = CalculateEaseStiffnessSubjectToTime(easeStiffness, time);
		v.x += (target.x - v.x) * easeStiffness;
		v.y += (target.y - v.y) * easeStiffness;
		v.z += (target.z - v.z) * easeStiffness;
		return v;
	}

	public static Vector3 EaseV3(Vector3 v, Vector3 target, float easeStiffness, float time)
	{
		return EaseV3(ref v, target, easeStiffness, time);
	}

	public static float DiscretePhysicsTick(ref float deltaTime)
	{
		float result = ((deltaTime > DiscretePhysicsRate) ? DiscretePhysicsRate : deltaTime);
		deltaTime -= DiscretePhysicsRate;
		return result;
	}

	public static float Spring(ref float position, ref float velocity, float targetPosition, float springConstant, float damping, float time)
	{
		while (time > 0f)
		{
			float num = ((time > DiscretePhysicsRate) ? DiscretePhysicsRate : time);
			velocity += ((targetPosition - position) * springConstant - damping * velocity) * num;
			position += velocity * num;
			time -= DiscretePhysicsRate;
		}
		return position;
	}

	public static float SpringPush(ref float position, ref float velocity, float springPosition, float targetPosition, float springConstant, float damping, float time)
	{
		float num = targetPosition - springPosition;
		while (time > 0f)
		{
			float num2 = ((time > DiscretePhysicsRate) ? DiscretePhysicsRate : time);
			float num3 = targetPosition - position;
			velocity += (((num3 * num > 0f) ? (num3 * springConstant) : 0f) - damping * velocity) * num2;
			position += velocity * num2;
			time -= DiscretePhysicsRate;
		}
		return position;
	}

	public static float SpringPull(ref float position, ref float velocity, float springPosition, float targetPosition, float springConstant, float damping, float time)
	{
		float num = targetPosition - springPosition;
		while (time > 0f)
		{
			float num2 = ((time > DiscretePhysicsRate) ? DiscretePhysicsRate : time);
			float num3 = targetPosition - position;
			velocity += (((num3 * num < 0f) ? (num3 * springConstant) : 0f) - damping * velocity) * num2;
			position += velocity * num2;
			time -= DiscretePhysicsRate;
		}
		return position;
	}

	public static Vector2 Spring(ref Vector2 position, ref Vector2 velocity, Vector2 targetPosition, float springConstant, float damping, float time)
	{
		Spring(ref position.x, ref velocity.x, targetPosition.x, springConstant, damping, time);
		Spring(ref position.y, ref velocity.y, targetPosition.y, springConstant, damping, time);
		return position;
	}

	public static Vector3 Spring(ref Vector3 position, ref Vector3 velocity, Vector3 targetPosition, float springConstant, float damping, float time)
	{
		Spring(ref position.x, ref velocity.x, targetPosition.x, springConstant, damping, time);
		Spring(ref position.y, ref velocity.y, targetPosition.y, springConstant, damping, time);
		Spring(ref position.z, ref velocity.z, targetPosition.z, springConstant, damping, time);
		return position;
	}

	public static Vector4 Spring(ref Vector4 position, ref Vector4 velocity, Vector4 targetPosition, float springConstant, float damping, float time)
	{
		Spring(ref position.x, ref velocity.x, targetPosition.x, springConstant, damping, time);
		Spring(ref position.y, ref velocity.y, targetPosition.y, springConstant, damping, time);
		Spring(ref position.z, ref velocity.z, targetPosition.z, springConstant, damping, time);
		Spring(ref position.w, ref velocity.w, targetPosition.w, springConstant, damping, time);
		return position;
	}

	public static Quaternion Spring(ref float lerp, ref float velocity, Quaternion start, Quaternion end, float springConstant, float damping, float time, float targetLerp = 1f)
	{
		Spring(ref lerp, ref velocity, targetLerp, springConstant, damping, time);
		return start.SlerpUnclamped(end, lerp);
	}

	public static Quaternion Spring(ref Quaternion current, ref Vector4 velocity, Quaternion target, float springConstant, float damping, float time)
	{
		Vector4 position = current.ToVector4();
		Vector4 vector = target.ToVector4();
		Spring(ref position, ref velocity, (Vector4.Dot(vector, position) >= 0f) ? vector : (-vector), springConstant, damping, time);
		current = position.ToQuaternion();
		return current;
	}

	public static Color Spring(ref Color position, ref Color velocity, Color targetPosition, float springConstant, float damping, float time, bool springAlpha = false)
	{
		Spring(ref position.r, ref velocity.r, targetPosition.r, springConstant, damping, time);
		Spring(ref position.g, ref velocity.g, targetPosition.g, springConstant, damping, time);
		Spring(ref position.b, ref velocity.b, targetPosition.b, springConstant, damping, time);
		if (springAlpha)
		{
			Spring(ref position.a, ref velocity.a, targetPosition.a, springConstant, damping, time);
		}
		return position;
	}

	public static Vector3 Gravitate(ref Vector3 position, ref Vector3 velocity, Vector3 targetPosition, float gravity, float damping, float time, float distancePower = 1.5f, float minDistance = 1f)
	{
		while (time > 0f)
		{
			float num = ((time > DiscretePhysicsRate) ? DiscretePhysicsRate : time);
			Vector3 vector = targetPosition - position;
			float num2 = vector.magnitude.InsureNonZero();
			velocity += (gravity * (vector / num2) / Mathf.Pow(Mathf.Max(minDistance, num2), distancePower) - damping * velocity) * num;
			position += velocity * num;
			time -= DiscretePhysicsRate;
		}
		return position;
	}

	public static bool DeltaSnap(ref float value, float targetValue, float deltaSnap = 0.01f)
	{
		if (Math.Abs(targetValue - value) <= deltaSnap)
		{
			value = targetValue;
			return true;
		}
		return false;
	}

	public static float DeltaSnap(float value, float targetValue, float deltaSnap = 0.01f)
	{
		if (!(Math.Abs(targetValue - value) > deltaSnap))
		{
			return targetValue;
		}
		return value;
	}

	public static double DeltaSnap(double value, double targetValue, double deltaSnap = 0.009999999776482582)
	{
		if (!(Math.Abs(targetValue - value) > deltaSnap))
		{
			return targetValue;
		}
		return value;
	}

	public static float FrictionSubjectToTimeSmooth(float friction, float timeSmooth)
	{
		return (float)Math.Pow(friction, timeSmooth);
	}

	public static Vector3 GenerateOrthonormalVectorFromDirection(Vector3 direction)
	{
		Vector3 vector;
		if (Math.Abs(direction.x) <= Math.Abs(direction.y) && Math.Abs(direction.x) <= Math.Abs(direction.z))
		{
			vector = new Vector3(0f, 0f - direction.z, direction.y);
		}
		vector = ((!(Math.Abs(direction.y) <= Math.Abs(direction.x)) || !(Math.Abs(direction.y) <= Math.Abs(direction.z))) ? new Vector3(0f - direction.y, direction.x, 0f) : new Vector3(0f - direction.z, 0f, direction.x));
		vector.Normalize();
		return vector;
	}

	public static Vector2 RandomOnUnitCircle2D(this System.Random random)
	{
		float f = random.Value() * (MathF.PI * 2f);
		return new Vector2(Mathf.Cos(f), Mathf.Sin(f));
	}

	public static Vector3 RandomOnUnitCircle(this System.Random random, Vector3 right, Vector3 up)
	{
		float f = random.Value() * (MathF.PI * 2f);
		return right * Mathf.Cos(f) + up * Mathf.Sin(f);
	}

	public static Vector3 RandomOnUnitSphere(this System.Random random, float minTheta = 0f, float maxTheta = MathF.PI * 2f, float minPhi = -MathF.PI / 2f, float maxPhi = MathF.PI / 2f)
	{
		return SphereCoordinate(random.Range(minTheta, maxTheta), random.Range(minPhi, maxPhi));
	}

	public static PoolStructListHandle<Vector3> RandomOnUnitSphere(this System.Random random, int numberOfPoints, float initialAngleRatio = 0f, float endAngleRatio = 1f)
	{
		PoolStructListHandle<Vector3> poolStructListHandle = Pools.UseStructList<Vector3>();
		for (int i = 0; i < numberOfPoints; i++)
		{
			poolStructListHandle.Add(random.RandomOnUnitSphere(0f, MathF.PI * 2f, Mathf.Lerp(-MathF.PI / 2f, MathF.PI / 2f, initialAngleRatio), Mathf.Lerp(-MathF.PI / 2f, MathF.PI / 2f, endAngleRatio)));
		}
		return poolStructListHandle;
	}

	public static Vector3 RandomInUnitCube(this System.Random random)
	{
		return Remap(random.Value3(), new Vector2(0f, 1f), new Vector2(-1f, 1f));
	}

	public static PoolStructListHandle<Vector3> RandomInUnitCube(this System.Random random, int numberOfPoints)
	{
		PoolStructListHandle<Vector3> poolStructListHandle = Pools.UseStructList<Vector3>();
		for (int i = 0; i < numberOfPoints; i++)
		{
			poolStructListHandle.Add(random.RandomInUnitCube());
		}
		return poolStructListHandle;
	}

	public static PoolStructListHandle<Vector3> RandomInRectPrismShell(this System.Random random, Vector3 minExtents, Vector3 maxExtents, int numberOfPoints)
	{
		PoolStructListHandle<Vector3> poolStructListHandle = Pools.UseStructList<Vector3>();
		using PoolWRandomDHandle<NearFarPlanes> poolWRandomDHandle = Pools.UseWRandomD<NearFarPlanes>();
		WRandomD<NearFarPlanes> value = poolWRandomDHandle.value;
		for (int i = 0; i < 6; i++)
		{
			Vector3 vector = _FrustumDirections[i];
			Vector3 vector2 = _FrustumDirections[(i + 1) % 6];
			Vector3 vector3 = _FrustumDirections[(i + 2) % 6];
			int index = vector.AxisIndex();
			int index2 = vector2.AxisIndex();
			int index3 = vector3.AxisIndex();
			NearFarPlanes obj = new NearFarPlanes(vector, vector2, vector3, minExtents[index3], new Vector2(minExtents[index], minExtents[index2]), maxExtents[index3], new Vector2(maxExtents[index], maxExtents[index2]));
			value.Add(obj.farPlaneArea + obj.volume, obj);
		}
		for (int j = 0; j < numberOfPoints; j++)
		{
			poolStructListHandle.Add(value.Random(random.NextDouble()).GetRandomPoint(random));
		}
		return poolStructListHandle;
	}

	public static Vector3 RandomInCone(this System.Random random, Vector3 direction, float coneAngleInDegrees)
	{
		Quaternion quaternion = Quaternion.FromToRotation(new Vector3(0f, 0f, 1f), direction);
		float f = coneAngleInDegrees * (MathF.PI / 180f) * 0.5f;
		float num = random.Range(Mathf.Cos(f), 1f);
		float f2 = random.Range(0f, MathF.PI * 2f);
		return quaternion * new Vector3(Mathf.Sqrt(1f - num * num) * Mathf.Cos(f2), Mathf.Sqrt(1f - num * num) * Mathf.Sin(f2), num);
	}

	public static PoolStructListHandle<Vector3> UniformPointsOnUnitSphere(int numberOfPoints, float initialAngleRatio = 0f, float endAngleRatio = 1f)
	{
		PoolStructListHandle<Vector3> poolStructListHandle = Pools.UseStructList<Vector3>();
		float num = MathF.PI * (3f - Mathf.Sqrt(5f));
		float num2 = 2f / (float)numberOfPoints * (endAngleRatio - initialAngleRatio);
		float num3 = -1f + initialAngleRatio * 2f;
		for (int i = 0; i < numberOfPoints; i++)
		{
			float num4 = (float)i * num2 + num3;
			float num5 = Mathf.Sqrt(1f - num4 * num4);
			float f = (float)i * num;
			float x = Mathf.Cos(f) * num5;
			float z = Mathf.Sin(f) * num5;
			poolStructListHandle.Add(new Vector3(x, num4, z));
		}
		return poolStructListHandle;
	}

	public static float CCWAngle(Vector2 from, Vector2 to)
	{
		float num = from.x * to.x + from.y * to.y;
		float num2 = from.x * to.y - from.y * to.x;
		float num3 = (float)Math.Atan2(num2, num);
		if (num2 < 0f)
		{
			num3 = MathF.PI * 2f + num3;
		}
		return num3;
	}

	public static Vector3 Abs(this Vector3 v)
	{
		return new Vector3(Math.Abs(v.x), Math.Abs(v.y), Math.Abs(v.z));
	}

	public static Vector3 Sign(this Vector3 v)
	{
		return new Vector3(Math.Sign(v.x), Math.Sign(v.y), Math.Sign(v.z));
	}

	public static int CombinedSign(this Vector3 v)
	{
		return Math.Sign(v.x) * Math.Sign(v.y) * Math.Sign(v.z);
	}

	public static int AxisIndex(this Vector3 v)
	{
		if (v.x == 0f)
		{
			if (v.y == 0f)
			{
				return 2;
			}
			return 1;
		}
		return 0;
	}

	public static Vector3 NearestMultipleOf(this Vector3 v, float multipleOf)
	{
		return new Vector3(RoundToNearestMultipleOf(v.x, multipleOf), RoundToNearestMultipleOf(v.y, multipleOf), RoundToNearestMultipleOf(v.z, multipleOf));
	}

	public static Vector2 TransformV2(Vector2 v, Vector2 t1, Vector2 t2)
	{
		Vector2 result = default(Vector2);
		result.x = t1.x * v.x + t2.x * v.y;
		result.y = t1.y * v.x + t2.y * v.y;
		return result;
	}

	public static float RoundIf(this float f, bool condition)
	{
		if (!condition)
		{
			return f;
		}
		return Mathf.Round(f);
	}

	public static float Max(this float f, float b)
	{
		if (!(f > b))
		{
			return b;
		}
		return f;
	}

	public static Vector2 Round(this Vector2 v)
	{
		return new Vector2((float)Math.Round(v.x), (float)Math.Round(v.y));
	}

	public static Short2 RoundToShort2(this Vector2 v)
	{
		return new Short2(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
	}

	public static Short2 FloorToShort2(this Vector2 v)
	{
		return new Short2(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y));
	}

	public static Short2 CeilToShort2(this Vector2 v)
	{
		return new Short2(Mathf.CeilToInt(v.x), Mathf.CeilToInt(v.y));
	}

	public static Short2 TruncateToShort2(this Vector2 v)
	{
		return new Short2((short)v.x, (short)v.y);
	}

	public static Vector3 NearestParallel(this Vector3 v, Vector3? direction)
	{
		if (!direction.HasValue)
		{
			return v;
		}
		if (!(Vector3.Dot(v, direction.Value) >= 0f))
		{
			return -v;
		}
		return v;
	}

	public static Vector2 ToDiagonalCardinal(this Vector2 v)
	{
		return new Vector2((v.x >= 0f) ? OneOverSqrtTwo : (0f - OneOverSqrtTwo), (v.y >= 0f) ? OneOverSqrtTwo : (0f - OneOverSqrtTwo));
	}

	public static Vector2 ToCardinalIncludeDiagonals(this Vector2 v)
	{
		Int2 @int = v.ToCardinal();
		Vector2 vector = v.ToDiagonalCardinal();
		if (!(Vector2.Dot(v, @int) >= Vector2.Dot(v, vector)))
		{
			return vector;
		}
		return @int;
	}

	public static Vector3 GetAxis(this Vector3 v, AxisType axis)
	{
		return axis switch
		{
			AxisType.X => new Vector3(v.x, 0f, 0f), 
			AxisType.Y => new Vector3(0f, v.y, 0f), 
			AxisType.Z => new Vector3(0f, 0f, v.z), 
			_ => throw new ArgumentOutOfRangeException("axis", axis, null), 
		};
	}

	public static bool IsBetweenRange(this Vector3 v, Vector3 min, Vector3 max)
	{
		if (v.x >= min.x && v.y >= min.y && v.z >= min.z && v.x <= max.x && v.y <= max.y)
		{
			return v.z <= max.z;
		}
		return false;
	}

	public static Vector3 Rejection(this Vector3 v, Vector3 onNormal)
	{
		return v - Vector3.Project(v, onNormal);
	}

	public static float Lerp(float start, float end, float t)
	{
		return start + (end - start) * t;
	}

	public static float LerpPow(float start, float end, float t, float pow, bool clamp = true)
	{
		return Lerp(start, end, clamp ? Mathf.Clamp01(Mathf.Pow(t, pow)) : Mathf.Pow(t, pow));
	}

	public static float LerpExtrema(float min, float max, float lerpAmount, float extremaPower)
	{
		return Mathf.Lerp(min, max, (float)Math.Pow(lerpAmount, Math.Pow(2.7182817459106445, (0.5f - lerpAmount) * Math.Abs(extremaPower))));
	}

	public static float LerpMean(float min, float max, float lerpAmount, int meanPower)
	{
		lerpAmount += lerpAmount;
		lerpAmount -= 1f;
		meanPower += 1 - meanPower % 2;
		lerpAmount = Mathf.Pow(lerpAmount, meanPower);
		lerpAmount += 1f;
		lerpAmount *= 0.5f;
		return Mathf.Lerp(min, max, lerpAmount);
	}

	public static float LerpExtremaOrMean(float min, float max, float t, float lerpPower)
	{
		if (lerpPower != 1f)
		{
			if (!(Mathf.Abs(t - 0.5f) >= 0.25f))
			{
				return LerpMean(min, max, t, Mathf.RoundToInt(lerpPower));
			}
			return LerpExtrema(min, max, t, lerpPower);
		}
		return Lerp(min, max, t);
	}

	public static int LerpInt(float min, float max, float lerpAmount)
	{
		return (int)Mathf.Lerp(min, max + OneMinusEpsilon, lerpAmount);
	}

	public static float LerpSnap(ref float current, float target, float lerpAmount, float snapDistance = 0.01f)
	{
		current = Mathf.Lerp(current, target, lerpAmount);
		if (Math.Abs(target - current) <= snapDistance)
		{
			current = target;
		}
		return current;
	}

	public static float Lerp(this Vector2 a, float t)
	{
		return Lerp(a.x, a.y, t);
	}

	public static Vector2 Lerp(this Vector2 a, Vector2 b, float t)
	{
		Vector2 result = default(Vector2);
		result.x = a.x + (b.x - a.x) * t;
		result.y = a.y + (b.y - a.y) * t;
		return result;
	}

	public static Vector3 Lerp(this Vector3 a, Vector3 b, float t)
	{
		Vector3 result = default(Vector3);
		result.x = a.x + (b.x - a.x) * t;
		result.y = a.y + (b.y - a.y) * t;
		result.z = a.z + (b.z - a.z) * t;
		return result;
	}

	public static Vector3 Lerp(this Vector3 a, Vector3 b, Vector3 t)
	{
		Vector3 result = default(Vector3);
		result.x = a.x + (b.x - a.x) * t.x;
		result.y = a.y + (b.y - a.y) * t.y;
		result.z = a.z + (b.z - a.z) * t.z;
		return result;
	}

	public static Color Lerp(this Color a, Color b, float t, bool includeAlpha = true)
	{
		Color result = default(Color);
		result.r = a.r + (b.r - a.r) * t;
		result.g = a.g + (b.g - a.g) * t;
		result.b = a.b + (b.b - a.b) * t;
		if (includeAlpha)
		{
			result.a = a.a + (b.a - a.a) * t;
		}
		return result;
	}

	public static void Lerp(this AnimationCurve curve, float value, float lerp)
	{
		Keyframe[] keys = curve.keys;
		for (int i = 0; i < keys.Length; i++)
		{
			keys[i].value = Lerp(keys[i].value, value, lerp);
		}
		curve.keys = keys;
	}

	public static void LerpCurve(this AnimationCurve a, AnimationCurve b, AnimationCurve output, Vector2 range, float lerp, int fidelity = 10)
	{
		output.Clear();
		int num = fidelity - 1;
		for (int i = 0; i < fidelity; i++)
		{
			float time = (float)i / (float)num;
			output.AddKey(time, Mathf.Lerp(range.x, range.y, Mathf.Lerp(a.Evaluate(time), b.Evaluate(time), lerp)));
		}
	}

	public static float Cos01(float theta)
	{
		return (Mathf.Cos(theta) + 1f) * 0.5f;
	}

	public static float CosLerp(float min, float max, float theta)
	{
		return Lerp(min, max, Cos01(theta));
	}

	public static float Sin01(float theta)
	{
		return (Mathf.Sin(theta) + 1f) * 0.5f;
	}

	public static float SinLerp(float min, float max, float theta)
	{
		return Lerp(min, max, Sin01(theta));
	}

	public static float SinLerpPow(float min, float max, float theta, float power)
	{
		return Lerp(min, max, Mathf.Pow(Sin01(theta), power));
	}

	public static float SinLerpSpline(float min, float max, float theta)
	{
		return Lerp(min, max, CubicSplineInterpolant(Sin01(theta)));
	}

	public static Rect GetOptimalInscirbedAspectRatioRect(this Rect r, float aspectRatio, Vector2 pivot)
	{
		Rect result = r;
		if (r.width == 0f || r.height == 0f || aspectRatio == 0f)
		{
			return result;
		}
		float num = Mathf.Abs(r.width / r.height);
		if (aspectRatio < num)
		{
			result.width = (float)Math.Sign(result.width) * Math.Abs(aspectRatio * result.height);
			result.x += (r.width - result.width) * pivot.x;
		}
		else if (aspectRatio > num)
		{
			result.height = (float)Math.Sign(result.height) * Math.Abs(result.width / aspectRatio);
			result.y -= (r.height - result.height) * pivot.y;
		}
		return result;
	}

	public static Rect Flip(this Rect r, bool horizontal, bool vertical)
	{
		Rect result = r;
		if (horizontal)
		{
			result.xMin = r.xMax;
			result.xMax = r.xMin;
		}
		if (vertical)
		{
			result.yMin = r.yMax;
			result.yMax = r.yMin;
		}
		return result;
	}

	public static Rect Flip(this Rect r, FlipAxisFlags axisFlags)
	{
		return r.Flip(EnumUtil.HasFlag(axisFlags, FlipAxisFlags.FlipHorizontal), EnumUtil.HasFlag(axisFlags, FlipAxisFlags.FlipVertical));
	}

	public static Vector3 GetAxis(AxisType axis)
	{
		return axis switch
		{
			AxisType.X => Vector3.right, 
			AxisType.Y => Vector3.up, 
			AxisType.Z => Vector3.forward, 
			_ => Vector3.forward, 
		};
	}

	public static IEnumerable<Vector3> GetOtherAxes(AxisType ignore)
	{
		for (int x = 0; x < Axes.Length; x++)
		{
			AxisType axisType = Axes[x];
			if (axisType != ignore)
			{
				yield return GetAxis(axisType);
			}
		}
	}

	public static AxisType GetAxisNearestTo(Vector3 direction, bool doubleSidedCheck = true)
	{
		AxisType result = AxisType.X;
		float num = float.MinValue;
		for (int i = 0; i < Axes.Length; i++)
		{
			AxisType axisType = Axes[i];
			float num2 = Vector3.Dot(GetAxis(axisType), direction);
			if (doubleSidedCheck)
			{
				num2 = Math.Abs(num2);
			}
			if (num2 > num)
			{
				num = num2;
				result = axisType;
			}
		}
		return result;
	}

	public static List<Vector2> CreatePointsOnCircle(Vector2 center, float radius, int numPoints = 360)
	{
		List<Vector2> list = new List<Vector2>(numPoints);
		Vector2 vector = new Vector2(radius, 0f);
		float num = MathF.PI * 2f / (float)numPoints;
		float cos = (float)Math.Cos(num);
		float sin = (float)Math.Sin(0f - num);
		for (int i = 0; i < numPoints; i++)
		{
			list.Add(center + vector);
			vector = vector.Rotate(cos, sin);
		}
		return list;
	}

	public static Vector3 SphereCoordinate(double theta, double phi, float radius = 1f)
	{
		float num = (float)Math.Cos(phi) * radius;
		return new Vector3((float)Math.Cos(theta) * num, (float)Math.Sin(phi) * radius, (float)Math.Sin(theta) * num);
	}

	public static Bounds BoundsFromPoints(List<Vector3> points)
	{
		if (points.Count == 0)
		{
			return default(Bounds);
		}
		Bounds result = new Bounds(points[0], Vector3.zero);
		for (int i = 1; i < points.Count; i++)
		{
			result.Encapsulate(points[i]);
		}
		return result;
	}

	public static Bounds BoundsFromPoints(params Vector3[] points)
	{
		if (points.Length == 0)
		{
			return default(Bounds);
		}
		Bounds result = new Bounds(points[0], Vector3.zero);
		for (int i = 1; i < points.Length; i++)
		{
			result.Encapsulate(points[i]);
		}
		return result;
	}

	public static Bounds BoundsFromMinMax(Vector3 min, Vector3 max)
	{
		return new Bounds((min + max) * 0.5f, (max - min).Abs());
	}

	public static BoundingSphere BoundingSphereFromPoints(params Vector3[] points)
	{
		Vector3 vector2;
		Vector3 vector;
		Vector3 vector3 = (vector2 = (vector = Vector3.one * float.PositiveInfinity));
		Vector3 vector5;
		Vector3 vector4;
		Vector3 vector6 = (vector5 = (vector4 = Vector3.one * float.NegativeInfinity));
		Vector3[] array = points;
		for (int i = 0; i < array.Length; i++)
		{
			Vector3 vector7 = array[i];
			if (vector7.x < vector3.x)
			{
				vector3 = vector7;
			}
			if (vector7.x > vector6.x)
			{
				vector6 = vector7;
			}
			if (vector7.y < vector2.y)
			{
				vector2 = vector7;
			}
			if (vector7.y > vector5.y)
			{
				vector5 = vector7;
			}
			if (vector7.z < vector.z)
			{
				vector = vector7;
			}
			if (vector7.z > vector4.z)
			{
				vector4 = vector7;
			}
		}
		float sqrMagnitude = (vector6 - vector3).sqrMagnitude;
		float sqrMagnitude2 = (vector5 - vector2).sqrMagnitude;
		float sqrMagnitude3 = (vector4 - vector).sqrMagnitude;
		Vector3 vector8 = vector3;
		Vector3 vector9 = vector6;
		float num = sqrMagnitude;
		if (sqrMagnitude2 > num)
		{
			num = sqrMagnitude2;
			vector8 = vector2;
			vector9 = vector5;
		}
		if (sqrMagnitude3 > num)
		{
			vector8 = vector;
			vector9 = vector4;
		}
		Vector3 vector10 = (vector8 + vector9) * 0.5f;
		float num2 = (vector9 - vector10).sqrMagnitude;
		float num3 = Mathf.Sqrt(num2);
		array = points;
		foreach (Vector3 vector11 in array)
		{
			float sqrMagnitude4 = (vector11 - vector10).sqrMagnitude;
			if (sqrMagnitude4 > num2)
			{
				float num4 = Mathf.Sqrt(sqrMagnitude4);
				num3 = (num3 + num4) * 0.5f;
				num2 = num3 * num3;
				float num5 = num4 - num3;
				vector10 = (num3 * vector10 + num5 * vector11) / num4;
			}
		}
		return new BoundingSphere(vector10, num3);
	}

	public static bool PointsAreCollinear(Vector3 p0, Vector3 p1, Vector3 p2, float? threshold = null)
	{
		return Math.Abs(Vector3.Cross((p2 - p0).normalized, (p1 - p0).normalized).sqrMagnitude) < (threshold ?? BigEpsilon);
	}

	public static bool PointsAreCollinear(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		Vector3 normalized = (p1 - p0).normalized;
		if (Math.Abs(Vector3.Cross((p2 - p0).normalized, normalized).sqrMagnitude) < BigEpsilon)
		{
			return Math.Abs(Vector3.Cross((p3 - p0).normalized, normalized).sqrMagnitude) < BigEpsilon;
		}
		return false;
	}

	public static Quaternion SlerpUnclamped(this Quaternion a, Quaternion b, float lerp)
	{
		float num = a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
		bool flag = false;
		if (num < 0f)
		{
			flag = true;
			num = 0f - num;
		}
		float num2;
		float num3;
		if (num > 0.999999f)
		{
			num2 = 1f - lerp;
			num3 = (flag ? (0f - lerp) : lerp);
		}
		else
		{
			float num4 = Mathf.Acos(num);
			float num5 = 1f / Mathf.Sin(num4);
			num2 = Mathf.Sin((1f - lerp) * num4) * num5;
			num3 = (flag ? ((0f - Mathf.Sin(lerp * num4)) * num5) : (Mathf.Sin(lerp * num4) * num5));
		}
		Quaternion result = default(Quaternion);
		result.x = num2 * a.x + num3 * b.x;
		result.y = num2 * a.y + num3 * b.y;
		result.z = num2 * a.z + num3 * b.z;
		result.w = num2 * a.w + num3 * b.w;
		return result;
	}

	public static Vector4 ToVector4(this Quaternion q)
	{
		return new Vector4(q.x, q.y, q.z, q.w);
	}

	public static Vector3 Right(this Quaternion q)
	{
		return q * Vector3.right;
	}

	public static Vector3 Up(this Quaternion q)
	{
		return q * Vector3.up;
	}

	public static Vector3 Forward(this Quaternion q)
	{
		return q * Vector3.forward;
	}

	public static Vector3 GetAxis(this Quaternion q, AxisType axis)
	{
		return axis switch
		{
			AxisType.X => q.Right(), 
			AxisType.Y => q.Up(), 
			_ => q.Forward(), 
		};
	}

	public static Quaternion Average(this IEnumerable<Quaternion> quaternions)
	{
		Quaternion quaternion = new Quaternion(0f, 0f, 0f, 0f);
		float num = 0f;
		foreach (Quaternion quaternion2 in quaternions)
		{
			quaternion = Quaternion.Slerp(quaternion, quaternion2, 1f / (num += 1f));
		}
		return quaternion;
	}

	public static float FrequencyFromWavelength(float waveLength, float velocity)
	{
		return velocity / waveLength * (MathF.PI * 2f);
	}

	public static float WavelengthCorrection(float wavelength, float distance)
	{
		return RoundToNearestFactorOf(wavelength, distance) / wavelength;
	}

	public static float Circumference(float radius)
	{
		return MathF.PI * 2f * radius;
	}

	public static float CircleArea(float radius)
	{
		return MathF.PI * radius * radius;
	}

	public static float RingArea(float minRadius, float maxRadius)
	{
		return CircleArea(maxRadius) - CircleArea(minRadius);
	}

	public static float GeometricSeriesSum(float initialValue, float ratio, float count)
	{
		if (ratio == 1f)
		{
			return initialValue * count;
		}
		return initialValue * ((1f - Mathf.Pow(ratio, count)) / (1f - ratio));
	}

	public static void InflateMesh(List<Vector3> points, float inflateRatio)
	{
		Bounds bounds = BoundsFromPoints(points);
		Vector3 center = bounds.center;
		for (int i = 0; i < points.Count; i++)
		{
			Vector3 vector = points[i];
			Ray ray = new Ray(center, (vector - center).normalized);
			if (bounds.IntersectRay(ray, out var distance))
			{
				vector = Vector3.Lerp(vector, ray.origin + ray.direction * Mathf.Abs(distance), inflateRatio);
			}
			points[i] = vector;
		}
	}

	public static PoolStructListHandle<RaycastHit> RayCastAllMousePointer(this Camera camera, float distance, int layerMask)
	{
		return camera.RayCastAllScreenPosition(Input.mousePosition, distance, layerMask);
	}

	public static PoolStructListHandle<RaycastHit> RayCastAllScreenPosition(this Camera camera, Vector3 screenPosition, float distance, int layerMask)
	{
		using PoolStructArrayHandle<RaycastHit> poolStructArrayHandle = Pools.UseArray<RaycastHit>(256);
		RaycastHit[] value = poolStructArrayHandle.value;
		int num = Physics.RaycastNonAlloc(camera.ScreenPointToRay(screenPosition), value, distance, layerMask);
		PoolStructListHandle<RaycastHit> poolStructListHandle = Pools.UseStructList<RaycastHit>();
		List<RaycastHit> value2 = poolStructListHandle.value;
		for (int i = 0; i < num; i++)
		{
			value2.Add(value[i]);
		}
		return poolStructListHandle;
	}

	public static RaycastHit? RayCastMousePointer(this Camera camera, float distance, int layerMask)
	{
		return camera.RayCastScreenPosition(Input.mousePosition, distance, layerMask);
	}

	public static RaycastHit? RayCastScreenPosition(this Camera camera, Vector3 screenPosition, float distance, int layerMask)
	{
		if (Physics.Raycast(camera.ScreenPointToRay(screenPosition), out var hitInfo, Math.Min(distance, camera.MaxViewDistance()), layerMask))
		{
			return hitInfo;
		}
		return null;
	}

	public static float MaxViewDistance(this Camera camera)
	{
		float farClipPlane = camera.farClipPlane;
		float num = Mathf.Tan(camera.fieldOfView * 0.5f * (MathF.PI / 180f)) * farClipPlane;
		float num2 = Mathf.Tan(camera.fieldOfView * camera.aspect * 0.5f * (MathF.PI / 180f)) * farClipPlane;
		return Mathf.Sqrt(farClipPlane * farClipPlane + num * num + num2 * num2);
	}

	public static Vector3 WorldMousePositionXZ(this Camera camera, Vector3 mousePosition)
	{
		Ray ray = camera.ScreenPointToRay(mousePosition);
		Transform transform = camera.transform;
		return transform.position - ray.direction * (transform.position.y / ray.direction.y);
	}

	public static RaycastHit FilterRayCastHits(this RaycastHit[] hits, Func<GameObject, bool> target, Func<GameObject, bool> blocking = null, int? length = null)
	{
		RaycastHit result = default(RaycastHit);
		float num = float.MaxValue;
		int num2 = length ?? hits.Length;
		for (int i = 0; i < num2; i++)
		{
			if (hits[i].distance < num)
			{
				GameObject gameObject = hits[i].collider.gameObject;
				if (blocking != null && blocking(gameObject))
				{
					num = hits[i].distance;
					result = default(RaycastHit);
				}
				else if (target(gameObject))
				{
					num = hits[i].distance;
					result = hits[i];
				}
			}
		}
		return result;
	}

	public static bool RayPolyIntersects(Vector2 p, Vector2 t, LinkedList<Vector2> hull)
	{
		LinkedListNode<Vector2> linkedListNode = hull.Last;
		for (LinkedListNode<Vector2> linkedListNode2 = hull.First; linkedListNode2 != null; linkedListNode2 = linkedListNode2.Next)
		{
			if (LineSegmentsCross(p, t, linkedListNode.Value, linkedListNode2.Value))
			{
				return true;
			}
			linkedListNode = linkedListNode2;
		}
		return false;
	}

	public static void PolyEdgesThatIntersectOBB(LinkedList<Vector2> hull, List<Vector2> output, Vector2 obbOrigin, Vector2 obbRight, Vector2 obbUp, float obbHeight, float obbLength)
	{
		LinkedListNode<Vector2> linkedListNode = hull.Last;
		for (LinkedListNode<Vector2> linkedListNode2 = hull.First; linkedListNode2 != null; linkedListNode2 = linkedListNode2.Next)
		{
			if (LineSegmentOBBIntersection(linkedListNode.Value, linkedListNode2.Value, obbOrigin, obbRight, obbUp, obbHeight, obbLength))
			{
				output.Add(linkedListNode.Value);
				output.Add(linkedListNode2.Value);
			}
			linkedListNode = linkedListNode2;
		}
	}

	public static float CalculateSmoothCollisionHeightOffset(Vector3 position, float collisionRadius, float collisionHeightPadding, LayerMask collidableLayers, float rayLength = 10f, Func<Collider, bool> ignoreCollider = null)
	{
		if (collisionRadius <= 0f)
		{
			return 0f;
		}
		float num = 0f;
		Vector3 vector = position + new Vector3(0f, rayLength, 0f);
		Vector3 down = Vector3.down;
		using PoolStructArrayHandle<RaycastHit> poolStructArrayHandle = Pools.UseArray<RaycastHit>(256);
		int num2 = Physics.SphereCastNonAlloc(vector, collisionRadius, down, poolStructArrayHandle, rayLength, collidableLayers);
		RaycastHit[] value = poolStructArrayHandle.value;
		for (int i = 0; i < num2; i++)
		{
			RaycastHit raycastHit = value[i];
			if (ignoreCollider == null || !ignoreCollider(raycastHit.collider))
			{
				Vector2 vector2 = vector.Project(AxisType.Y);
				float num3 = (raycastHit.point.Project(AxisType.Y) - vector2).magnitude / collisionRadius;
				num = Math.Max(num, (raycastHit.collider.bounds.max.y + collisionHeightPadding - position.y) * CubicSplineInterpolant(1f - num3));
			}
		}
		return num;
	}

	public static bool LineSegmentsCross(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
	{
		float num = (b.x - a.x) * (d.y - c.y) - (b.y - a.y) * (d.x - c.x);
		if (num == 0f)
		{
			return false;
		}
		float num2 = (a.y - c.y) * (d.x - c.x) - (a.x - c.x) * (d.y - c.y);
		float num3 = (a.y - c.y) * (b.x - a.x) - (a.x - c.x) * (b.y - a.y);
		if (num2 == 0f || num3 == 0f)
		{
			return false;
		}
		float num4 = num2 / num;
		float num5 = num3 / num;
		if (num4 > 0f && num4 < 1f)
		{
			if (num5 > 0f)
			{
				return num5 < 1f;
			}
			return false;
		}
		return false;
	}

	public static bool LineSegmentAABBIntersection(Vector2 start, Vector2 end, Rect clipArea)
	{
		float num = end.x - start.x;
		float num2 = end.y - start.y;
		float num3 = 0f;
		float num4 = 1f;
		float[] array = new float[4]
		{
			0f - num,
			num,
			0f - num2,
			num2
		};
		float[] array2 = new float[4]
		{
			start.x - clipArea.xMin,
			clipArea.xMax - start.x,
			start.y - clipArea.yMin,
			clipArea.yMax - start.y
		};
		for (byte b = 0; b < 4; b = (byte)(b + 1))
		{
			if (array[b] == 0f && array2[b] < 0f)
			{
				return false;
			}
			float num5 = array2[b] / array[b];
			if (array[b] < 0f)
			{
				if (num5 > num4)
				{
					return false;
				}
				if (num5 > num3)
				{
					num3 = num5;
				}
			}
			else if (array[b] > 0f)
			{
				if (num5 < num3)
				{
					return false;
				}
				if (num5 < num4)
				{
					num4 = num5;
				}
			}
		}
		return true;
	}

	public static bool LineSegmentOBBIntersection(Vector2 start, Vector2 end, Vector2 obbOrigin, Vector2 obbRight, Vector2 obbUp, float obbHeight, float obbLength)
	{
		start -= obbOrigin;
		end -= obbOrigin;
		float y = obbRight.y;
		obbRight.y = obbUp.x;
		obbUp.x = y;
		start = TransformV2(start, obbRight, obbUp);
		end = TransformV2(end, obbRight, obbUp);
		return LineSegmentAABBIntersection(start, end, new Rect(0f, 0f, obbLength, obbHeight));
	}

	public static bool RectRectSAT(Vector2 aPos, Vector2 aDir, float aLength, float aWidth, Vector2 bPos, Vector2 bDir, float bLength, float bWidth)
	{
		Vector2 vector = default(Vector2);
		vector.x = aDir.y;
		vector.y = 0f - aDir.x;
		Vector2 vector2 = default(Vector2);
		vector2.x = vector.x * aWidth;
		vector2.y = vector.y * aWidth;
		Vector2 vector3 = default(Vector2);
		vector3.x = aPos.x + vector2.x;
		vector3.y = aPos.y + vector2.y;
		Vector2 vector4 = default(Vector2);
		vector4.x = aPos.x - vector2.x;
		vector4.y = aPos.y - vector2.y;
		Vector2 vector5 = default(Vector2);
		vector5.x = aPos.x + aDir.x * aLength;
		vector5.y = aPos.y + aDir.y * aLength;
		Vector2 vector6 = default(Vector2);
		vector6.x = vector5.x + vector2.x;
		vector6.y = vector5.y + vector2.y;
		Vector2 vector7 = default(Vector2);
		vector7.x = vector5.x - vector2.x;
		vector7.y = vector5.y - vector2.y;
		Vector2 vector8 = default(Vector2);
		vector8.x = bDir.y;
		vector8.y = 0f - bDir.x;
		Vector2 vector9 = default(Vector2);
		vector9.x = vector8.x * bWidth;
		vector9.y = vector8.y * bWidth;
		Vector2 vector10 = default(Vector2);
		vector10.x = bPos.x + vector9.x;
		vector10.y = bPos.y + vector9.y;
		Vector2 vector11 = default(Vector2);
		vector11.x = bPos.x - vector9.x;
		vector11.y = bPos.y - vector9.y;
		Vector2 vector12 = default(Vector2);
		vector12.x = bPos.x + bDir.x * bLength;
		vector12.y = bPos.y + bDir.y * bLength;
		Vector2 vector13 = default(Vector2);
		vector13.x = vector12.x + vector9.x;
		vector13.y = vector12.y + vector9.y;
		Vector2 vector14 = default(Vector2);
		vector14.x = vector12.x - vector9.x;
		vector14.y = vector12.y - vector9.y;
		float num = vector3.x * aDir.x + vector3.y * aDir.y;
		float num2 = vector4.x * aDir.x + vector4.y * aDir.y;
		float num3 = vector6.x * aDir.x + vector6.y * aDir.y;
		float num4 = vector7.x * aDir.x + vector7.y * aDir.y;
		float num5 = ((num < num2) ? num : num2);
		num5 = ((num5 < num3) ? num5 : num3);
		num5 = ((num5 < num4) ? num5 : num4);
		float num6 = ((num > num2) ? num : num2);
		num6 = ((num6 > num3) ? num6 : num3);
		num6 = ((num6 > num4) ? num6 : num4);
		float num7 = vector10.x * aDir.x + vector10.y * aDir.y;
		float num8 = vector11.x * aDir.x + vector11.y * aDir.y;
		float num9 = vector13.x * aDir.x + vector13.y * aDir.y;
		float num10 = vector14.x * aDir.x + vector14.y * aDir.y;
		float num11 = ((num7 < num8) ? num7 : num8);
		num11 = ((num11 < num9) ? num11 : num9);
		num11 = ((num11 < num10) ? num11 : num10);
		float num12 = ((num7 > num8) ? num7 : num8);
		num12 = ((num12 > num9) ? num12 : num9);
		num12 = ((num12 > num10) ? num12 : num10);
		if (num5 < num11)
		{
			if (num11 - num6 > 0f)
			{
				return false;
			}
		}
		else if (num5 - num12 > 0f)
		{
			return false;
		}
		num = vector3.x * vector.x + vector3.y * vector.y;
		num2 = vector4.x * vector.x + vector4.y * vector.y;
		num3 = vector6.x * vector.x + vector6.y * vector.y;
		num4 = vector7.x * vector.x + vector7.y * vector.y;
		num5 = ((num < num2) ? num : num2);
		num5 = ((num5 < num3) ? num5 : num3);
		num5 = ((num5 < num4) ? num5 : num4);
		num6 = ((num > num2) ? num : num2);
		num6 = ((num6 > num3) ? num6 : num3);
		num6 = ((num6 > num4) ? num6 : num4);
		num7 = vector10.x * vector.x + vector10.y * vector.y;
		num8 = vector11.x * vector.x + vector11.y * vector.y;
		num9 = vector13.x * vector.x + vector13.y * vector.y;
		num10 = vector14.x * vector.x + vector14.y * vector.y;
		num11 = ((num7 < num8) ? num7 : num8);
		num11 = ((num11 < num9) ? num11 : num9);
		num11 = ((num11 < num10) ? num11 : num10);
		num12 = ((num7 > num8) ? num7 : num8);
		num12 = ((num12 > num9) ? num12 : num9);
		num12 = ((num12 > num10) ? num12 : num10);
		if (num5 < num11)
		{
			if (num11 - num6 > 0f)
			{
				return false;
			}
		}
		else if (num5 - num12 > 0f)
		{
			return false;
		}
		num = vector3.x * bDir.x + vector3.y * bDir.y;
		num2 = vector4.x * bDir.x + vector4.y * bDir.y;
		num3 = vector6.x * bDir.x + vector6.y * bDir.y;
		num4 = vector7.x * bDir.x + vector7.y * bDir.y;
		num5 = ((num < num2) ? num : num2);
		num5 = ((num5 < num3) ? num5 : num3);
		num5 = ((num5 < num4) ? num5 : num4);
		num6 = ((num > num2) ? num : num2);
		num6 = ((num6 > num3) ? num6 : num3);
		num6 = ((num6 > num4) ? num6 : num4);
		num7 = vector10.x * bDir.x + vector10.y * bDir.y;
		num8 = vector11.x * bDir.x + vector11.y * bDir.y;
		num9 = vector13.x * bDir.x + vector13.y * bDir.y;
		num10 = vector14.x * bDir.x + vector14.y * bDir.y;
		num11 = ((num7 < num8) ? num7 : num8);
		num11 = ((num11 < num9) ? num11 : num9);
		num11 = ((num11 < num10) ? num11 : num10);
		num12 = ((num7 > num8) ? num7 : num8);
		num12 = ((num12 > num9) ? num12 : num9);
		num12 = ((num12 > num10) ? num12 : num10);
		if (num5 < num11)
		{
			if (num11 - num6 > 0f)
			{
				return false;
			}
		}
		else if (num5 - num12 > 0f)
		{
			return false;
		}
		num = vector3.x * vector8.x + vector3.y * vector8.y;
		num2 = vector4.x * vector8.x + vector4.y * vector8.y;
		num3 = vector6.x * vector8.x + vector6.y * vector8.y;
		num4 = vector7.x * vector8.x + vector7.y * vector8.y;
		num5 = ((num < num2) ? num : num2);
		num5 = ((num5 < num3) ? num5 : num3);
		num5 = ((num5 < num4) ? num5 : num4);
		num6 = ((num > num2) ? num : num2);
		num6 = ((num6 > num3) ? num6 : num3);
		num6 = ((num6 > num4) ? num6 : num4);
		num7 = vector10.x * vector8.x + vector10.y * vector8.y;
		num8 = vector11.x * vector8.x + vector11.y * vector8.y;
		num9 = vector13.x * vector8.x + vector13.y * vector8.y;
		num10 = vector14.x * vector8.x + vector14.y * vector8.y;
		num11 = ((num7 < num8) ? num7 : num8);
		num11 = ((num11 < num9) ? num11 : num9);
		num11 = ((num11 < num10) ? num11 : num10);
		num12 = ((num7 > num8) ? num7 : num8);
		num12 = ((num12 > num9) ? num12 : num9);
		num12 = ((num12 > num10) ? num12 : num10);
		if (num5 < num11)
		{
			if (num11 - num6 > 0f)
			{
				return false;
			}
		}
		else if (num5 - num12 > 0f)
		{
			return false;
		}
		return true;
	}

	public static bool PointInRectRay2D(Vector2 rayOrigin, Vector2 rayDir, float rayLength, float rayWidth, Vector2 point)
	{
		point -= rayOrigin;
		Vector2 t = rayDir.PerpCCW();
		float y = rayDir.y;
		rayDir.y = t.x;
		t.x = y;
		point = TransformV2(point, rayDir, t);
		return new Rect(0f, rayWidth * 0.5f, rayLength, rayWidth).Contains(point);
	}

	public static bool Contain(this SRect[] rects, Short2 point)
	{
		for (int i = 0; i < rects.Length; i++)
		{
			if (rects[i].Contains(point))
			{
				return true;
			}
		}
		return false;
	}

	public static bool Match(this LayerMask layerMask, GameObject go)
	{
		return (layerMask.value & (1 << go.layer)) != 0;
	}

	public static Vector3 GetClosestPointInSphere(Vector3 point, Vector3 sphereCenter, float radius)
	{
		Vector3 vector = default(Vector3);
		vector.x = sphereCenter.x - point.x;
		vector.y = sphereCenter.y - point.y;
		vector.z = sphereCenter.z - point.z;
		float magnitude = vector.magnitude;
		if (magnitude <= Mathf.Epsilon)
		{
			return point;
		}
		float num = ((magnitude < radius) ? magnitude : radius) / magnitude;
		vector.x *= num;
		vector.y *= num;
		vector.z *= num;
		sphereCenter.x += vector.x;
		sphereCenter.y += vector.y;
		sphereCenter.z += vector.z;
		return sphereCenter;
	}

	public static Vector2 FindPointOnLineSegmentClosestToPoint(Vector2 pos, Vector2 dir, float length, Vector2 point)
	{
		Vector2 vector = default(Vector2);
		vector.x = point.x - pos.x;
		vector.y = point.y - pos.y;
		float num = Mathf.Clamp(vector.x * dir.x + vector.y * dir.y, 0f, length);
		return new Vector2(pos.x + dir.x * num, pos.y + dir.y * num);
	}

	public static Vector3 FindPointOnLineSegmentClosestToPoint(Vector3 pos, Vector3 dir, float length, Vector3 point)
	{
		float num = Mathf.Clamp(Vector3.Dot(point - pos, dir), 0f, length);
		return pos + dir * num;
	}

	public static Vector3 FindPointOnLineSegmentClosestToPoint(Vector3 start, Vector3 end, Vector3 point)
	{
		Vector3 vector = end - start;
		float num = vector.magnitude.InsureNonZero();
		return FindPointOnLineSegmentClosestToPoint(start, vector / num, num, point);
	}

	public static float DistanceToLineSegmentSqrd(Vector3 a, Vector3 b, Vector3 point)
	{
		Vector3 vector = b - a;
		Vector3 vector2 = a - point;
		float num = Vector3.Dot(vector, vector2);
		if (num > 0f)
		{
			return Vector3.Dot(vector2, vector2);
		}
		Vector3 vector3 = point - b;
		if (Vector3.Dot(vector, vector3) > 0f)
		{
			return Vector3.Dot(vector3, vector3);
		}
		Vector3 vector4 = vector2 - vector * (num / Vector3.Dot(vector, vector));
		return Vector3.Dot(vector4, vector4);
	}

	public static float GetLerpAmount(Vector3 start, Vector3 end, Vector3 value)
	{
		Vector3 rhs = end - start;
		float num = rhs.magnitude.InsureNonZero();
		rhs /= num;
		return Mathf.Clamp(Vector3.Dot(value - start, rhs), 0f, num) / num;
	}

	public static float DistanceSquaredToPointFromLineSegment(Vector2 pos, Vector2 dir, float length, Vector2 point)
	{
		Vector2 vector = FindPointOnLineSegmentClosestToPoint(pos, dir, length, point);
		Vector2 vector2 = default(Vector2);
		vector2.x = point.x - vector.x;
		vector2.y = point.y - vector.y;
		return vector2.x * vector2.x + vector2.y * vector2.y;
	}

	public static Vector3 ClosestPointOnPlane(this Plane plane, Ray ray)
	{
		plane.Raycast(ray, out var enter);
		if (enter == 0f)
		{
			return plane.ClosestPointOnPlane(ray.origin);
		}
		return ray.GetPoint(enter);
	}

	public static Rect PolyBounds(List<Vector2> points, float padding = 2.5E-10f)
	{
		float num = float.MaxValue;
		float num2 = float.MinValue;
		float num3 = float.MaxValue;
		float num4 = float.MinValue;
		float num5 = padding + padding;
		for (int i = 0; i < points.Count; i++)
		{
			Vector2 vector = points[i];
			float x = vector.x;
			float y = vector.y;
			num = ((x < num) ? x : num);
			num2 = ((x > num2) ? x : num2);
			num3 = ((y < num3) ? y : num3);
			num4 = ((y > num4) ? y : num4);
		}
		return new Rect(num - padding, num3 - padding, num2 - num + num5, num4 - num3 + num5);
	}

	public static bool PointInPoly(Vector2 c, List<Vector2> p)
	{
		int index = p.Count - 1;
		bool flag = false;
		for (int i = 0; i < p.Count; i++)
		{
			if ((c.y > p[index].y && c.y <= p[i].y) || (c.y > p[i].y && c.y <= p[index].y))
			{
				float num = p[index].x + (p[i].x - p[index].x) * ((c.y - p[index].y) / (p[i].y - p[index].y));
				flag ^= num < c.x;
			}
			index = i;
		}
		return flag;
	}

	public static bool PointInConvexPoly(Vector2 point, List<Vector2> polygon)
	{
		if (polygon.Count == 0)
		{
			return false;
		}
		if (polygon.Count == 1)
		{
			return polygon[0] == point;
		}
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < polygon.Count; i++)
		{
			if (polygon[i] == point)
			{
				return true;
			}
			float x = polygon[i].x;
			float y = polygon[i].y;
			int index = ((i < polygon.Count - 1) ? (i + 1) : 0);
			float x2 = polygon[index].x;
			float y2 = polygon[index].y;
			float x3 = point.x;
			float y3 = point.y;
			float num3 = (x3 - x) * (y2 - y) - (y3 - y) * (x2 - x);
			if (num3 > 0f)
			{
				num++;
			}
			if (num3 < 0f)
			{
				num2++;
			}
			if (num > 0 && num2 > 0)
			{
				return false;
			}
		}
		return true;
	}

	public static void SortByCCWAngleDescending(List<Vector2> points, Vector2 center, Vector2 previousDir)
	{
		points.Sort((Vector2 a, Vector2 b) => Compare(CCWAngle(b - center, previousDir), CCWAngle(a - center, previousDir)));
	}

	public static int MinYIndex(this List<Vector2> p)
	{
		int result = 0;
		float num = float.MaxValue;
		for (int i = 0; i < p.Count; i++)
		{
			if (p[i].y < num)
			{
				num = p[i].y;
				result = i;
			}
		}
		return result;
	}

	public static float Cross(Vector2 a, Vector2 b, Vector2 c)
	{
		return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
	}

	public static bool IsCCW(Vector2 a, Vector2 b, Vector2 c)
	{
		return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x) > 0f;
	}

	private static List<Vector2> PointsInContour(Vector2 p, QuadTreeV2 points, float radius, int numPointsThreshold)
	{
		List<Vector2> list = new List<Vector2>();
		List<Vector2> list2 = new List<Vector2>();
		List<Vector2> list3 = new List<Vector2>(numPointsThreshold);
		list.Add(p);
		list3.Add(p);
		points.Remove(p);
		while (list3.Count < numPointsThreshold)
		{
			for (int i = 0; i < list.Count; i++)
			{
				points.FilterCircle(list[i], radius);
				for (int j = 0; j < points.FilteredResult.Count; j++)
				{
					list2.Add(points.FilteredResult[j]);
					list3.Add(points.FilteredResult[j]);
					points.Remove(points.FilteredResult[j]);
				}
			}
			if (list2.Count == 0)
			{
				break;
			}
			list2.ClearAndCopyTo(list);
			list2.Clear();
		}
		return list3;
	}

	public static void RemoveSmallContours(QuadTreeV2 points, float distanceThreshold, int numPointThreshold)
	{
		int num = points.Count();
		List<Vector2> list = new List<Vector2>(num);
		while (num > 0)
		{
			List<Vector2> list2 = PointsInContour(points.First(), points, distanceThreshold, num);
			if (list2.Count >= numPointThreshold)
			{
				for (int i = 0; i < list2.Count; i++)
				{
					list.Add(list2[i]);
				}
			}
			num = points.Count();
		}
		for (int j = 0; j < list.Count; j++)
		{
			points.Add(list[j]);
		}
	}

	public static void KeepPrimaryContoursOnly(QuadTreeV2 points, float distanceThreshold, int numContoursToKeep, Rect boundary)
	{
		int num = points.Count();
		List<List<Vector2>> list = new List<List<Vector2>>();
		while (num > 0)
		{
			list.Add(PointsInContour(points.First(), points, distanceThreshold, num));
			num = points.Count();
		}
		SortedList<float, List<Vector2>> sortedList = new SortedList<float, List<Vector2>>(FloatDescendingCompare.Default);
		for (int i = 0; i < list.Count; i++)
		{
			List<Vector2> list2 = list[i];
			float num2 = float.MaxValue;
			float num3 = float.MinValue;
			float num4 = float.MaxValue;
			float num5 = float.MinValue;
			Vector2 vector = default(Vector2);
			for (int j = 0; j < list2.Count; j++)
			{
				Vector2 vector2 = list2[j];
				num2 = ((vector2.x < num2) ? vector2.x : num2);
				num3 = ((vector2.x > num3) ? vector2.x : num3);
				num4 = ((vector2.y < num4) ? vector2.y : num4);
				num5 = ((vector2.y > num5) ? vector2.y : num5);
				vector.x += vector2.x;
				vector.y += vector2.y;
			}
			vector.x /= list2.Count;
			vector.y /= list2.Count;
			float num6 = (1f - CubicSplineInterpolant(Math.Abs(vector.x) / (boundary.width * 0.5f))) * (1f - CubicSplineInterpolant(Math.Abs(vector.y) / (boundary.height * 0.5f)));
			sortedList.Add((num3 - num2) * (num5 - num4) * num6, list2);
		}
		numContoursToKeep = Math.Min(numContoursToKeep, list.Count);
		for (int k = 0; k < numContoursToKeep; k++)
		{
			List<Vector2> list3 = sortedList.Values[k];
			for (int l = 0; l < list3.Count; l++)
			{
				points.Add(list3[l]);
			}
		}
	}

	public static List<Vector2> ConvexHull(List<Vector2> points, bool sort = true)
	{
		if (sort)
		{
			points.Sort(LexigraphicComparer.Default);
		}
		List<Vector2> list = new List<Vector2>();
		for (int i = 0; i < points.Count; i++)
		{
			while (list.Count >= 2 && Cross(list[list.Count - 2], list[list.Count - 1], points[i]) < 0f)
			{
				list.RemoveAt(list.Count - 1);
			}
			list.Add(points[i]);
		}
		List<Vector2> list2 = new List<Vector2>();
		for (int num = points.Count - 1; num >= 0; num--)
		{
			while (list2.Count >= 2 && Cross(list2[list2.Count - 2], list2[list2.Count - 1], points[num]) < 0f)
			{
				list2.RemoveAt(list2.Count - 1);
			}
			list2.Add(points[num]);
		}
		list.RemoveAt(list.Count - 1);
		list2.RemoveAt(list2.Count - 1);
		list.AddRange(list2);
		return list;
	}

	public static List<Vector2> FillConvexHullAlongBoundary(List<Vector2> convexHull, QuadTreeV2 points, float distanceThreshold, Rect boundary, float boundaryDistanceThreshold, float openBoundaryCheckDistanceThreshold)
	{
		List<Vector2> list = new List<Vector2>();
		float xMin = boundary.xMin;
		float xMax = boundary.xMax;
		float yMin = boundary.yMin;
		float yMax = boundary.yMax;
		int num = 0;
		while (num < convexHull.Count)
		{
			Vector2 vector = convexHull[num++];
			Vector2 vector2 = convexHull[num % convexHull.Count];
			list.Add(vector);
			if ((!vector.x.Prox(vector2.x, boundaryDistanceThreshold) || (!vector.x.Prox(xMin, boundaryDistanceThreshold) && !vector.x.Prox(xMax, boundaryDistanceThreshold))) && (!vector.y.Prox(vector2.y, boundaryDistanceThreshold) || (!vector.y.Prox(yMin, boundaryDistanceThreshold) && !vector.y.Prox(yMax, boundaryDistanceThreshold))))
			{
				continue;
			}
			Vector2 u = vector2 - vector;
			float magnitude = u.magnitude;
			if (magnitude >= openBoundaryCheckDistanceThreshold)
			{
				Vector2 position = (vector2 + vector) * 0.5f;
				u /= magnitude;
				Vector2 direction = u.PerpCCW();
				points.FilterRectRay(position, direction, magnitude * 0.05f, magnitude * 0.25f);
				if (points.FilteredResult.Count == 0)
				{
					continue;
				}
			}
			int num2 = (int)Math.Ceiling(magnitude / distanceThreshold + 0.5f) + 1;
			float num3 = 1f / (float)num2;
			for (int i = 1; i < num2; i++)
			{
				list.Add(Vector2.Lerp(vector, vector2, (float)i * num3));
			}
		}
		return list;
	}

	public static List<Vector2> ConcaveHull(List<Vector2> points, float distanceThreshold, Rect boundary, float boundaryThreshold, float? lookForOpenBoundaries, float keepContoursDistanceThreshold)
	{
		QuadTreeV2 quadTreeV = new QuadTreeV2(boundary, (boundary.width + boundary.height) * 0.05f, points);
		KeepPrimaryContoursOnly(quadTreeV, keepContoursDistanceThreshold, 1, boundary);
		points = quadTreeV.GetLexigraphicList();
		List<Vector2> list = ConvexHull(points, sort: false);
		if (boundaryThreshold > 0f)
		{
			list = FillConvexHullAlongBoundary(list, quadTreeV, distanceThreshold * 0.95f - Epsilon, boundary, boundaryThreshold, lookForOpenBoundaries.HasValue ? (distanceThreshold * lookForOpenBoundaries.Value) : float.MaxValue);
		}
		Rect bounds = PolyBounds(list);
		QuadTreeV2 quadTreeV2 = new QuadTreeV2(bounds, (bounds.width + bounds.height) * 0.05f, points);
		Dictionary<LinkedListNode<Vector2>, List<PolyCandidate>> candidates = new Dictionary<LinkedListNode<Vector2>, List<PolyCandidate>>();
		Dictionary<Vector2, float> minDepths = new Dictionary<Vector2, float>();
		List<PolyEdge> edgesToCheck = new List<PolyEdge>();
		HashSet<LinkedListNode<Vector2>> hashSet = new HashSet<LinkedListNode<Vector2>>();
		PooledList<List<PolyCandidate>> polyCandidatePool = new PooledList<List<PolyCandidate>>(list.Count, () => new List<PolyCandidate>(), expandPool: true, null, null, null, delegate(List<PolyCandidate> l)
		{
			l.Clear();
		});
		for (int i = 0; i < list.Count; i++)
		{
			quadTreeV2.Remove(list[i]);
		}
		LinkedList<Vector2> linkedList = list.ToLinkedList();
		Dictionary<Vector2, PolyEdge> dictionary = new Dictionary<Vector2, PolyEdge>();
		QuadTree<PolyEdge> quadTree = new QuadTree<PolyEdge>(bounds, (bounds.width + bounds.height) * 0.1f);
		LinkedListNode<Vector2> linkedListNode = linkedList.Last;
		for (LinkedListNode<Vector2> linkedListNode2 = linkedList.First; linkedListNode2 != null; linkedListNode2 = linkedListNode2.Next)
		{
			PolyEdge polyEdge = new PolyEdge(linkedListNode.Value, linkedListNode2.Value);
			polyEdge.AddToQuadTree(quadTree);
			dictionary.Add(linkedListNode.Value, polyEdge);
			linkedListNode = linkedListNode2;
		}
		float distThreshSqrd = distanceThreshold * distanceThreshold;
		bool flag = true;
		while (flag)
		{
			flag = RefineConcaveHull(distThreshSqrd, linkedList, quadTreeV2, quadTree, polyCandidatePool, edgesToCheck, candidates, hashSet, dictionary, minDepths);
			hashSet.Clear();
			List<Vector2> list2 = linkedList.ToList();
			if (RemoveEdges(list2, distanceThreshold, quadTreeV2, dictionary, quadTree) == 0)
			{
				break;
			}
			linkedList = list2.ToLinkedList();
		}
		return linkedList.ToList();
	}

	private static bool RefineConcaveHull(float distThreshSqrd, LinkedList<Vector2> hull, QuadTreeV2 interiorPoints, QuadTree<PolyEdge> edges, PooledList<List<PolyCandidate>> polyCandidatePool, List<PolyEdge> edgesToCheck, Dictionary<LinkedListNode<Vector2>, List<PolyCandidate>> candidates, HashSet<LinkedListNode<Vector2>> refinedEdges, Dictionary<Vector2, PolyEdge> edgeMap, Dictionary<Vector2, float> minDepths)
	{
		bool flag = false;
		bool flag2 = true;
		while (flag2)
		{
			flag2 = false;
			bool flag3 = false;
			LinkedListNode<Vector2> linkedListNode = hull.First;
			while (!flag3)
			{
				LinkedListNode<Vector2> linkedListNode2 = linkedListNode.Next;
				if (linkedListNode2 == null)
				{
					linkedListNode2 = hull.First;
					flag3 = true;
				}
				if (refinedEdges.Contains(linkedListNode))
				{
					linkedListNode = linkedListNode2;
					continue;
				}
				Vector2 value = linkedListNode.Value;
				Vector2 value2 = linkedListNode2.Value;
				Vector2 vector = default(Vector2);
				vector.x = value2.x - value.x;
				vector.y = value2.y - value.y;
				float num = vector.x * vector.x + vector.y * vector.y;
				if (num <= distThreshSqrd)
				{
					refinedEdges.Add(linkedListNode);
					linkedListNode = linkedListNode2;
					continue;
				}
				float num2 = (float)Math.Sqrt(num);
				float num3 = num2 * 0.5f;
				vector.x /= num2;
				vector.y /= num2;
				Vector2 direction = default(Vector2);
				direction.x = 0f - vector.y;
				direction.y = vector.x;
				Vector2 position = default(Vector2);
				position.x = value.x + vector.x * num3;
				position.y = value.y + vector.y * num3;
				List<PolyCandidate> list = polyCandidatePool.Unpool();
				interiorPoints.Filter(position, direction, LargeNumber, num3);
				List<Vector2> filteredResult = interiorPoints.FilteredResult;
				for (int i = 0; i < filteredResult.Count; i++)
				{
					Vector2 pos = filteredResult[i];
					Vector2 vector2 = default(Vector2);
					vector2.x = pos.x - value.x;
					vector2.y = pos.y - value.y;
					float num4 = vector2.x * vector.x + vector2.y * vector.y;
					if (!(num4 < 0f) && !(num4 > num2))
					{
						float num5 = vector2.x * direction.x + vector2.y * direction.y;
						if (!(num5 < 0f))
						{
							float num6 = num5 * num5;
							float num7 = vector2.x * vector2.x + vector2.y * vector2.y;
							float sinThetaSqrd = num6 / num7;
							Vector2 vector3 = new Vector2
							{
								x = pos.x - value2.x,
								y = pos.y - value2.y
							};
							float num8 = vector3.x * vector3.x + vector3.y * vector3.y;
							float sinPsiSqrd = num6 / num8;
							list.Add(new PolyCandidate(num5, pos, sinThetaSqrd, sinPsiSqrd));
						}
					}
				}
				if (list.Count == 0)
				{
					refinedEdges.Add(linkedListNode);
					linkedListNode = linkedListNode2;
					continue;
				}
				list.Sort();
				edges.Filter(position, direction, LargeNumber, num3);
				edgesToCheck = edges.FilteredResult;
				for (int num9 = list.Count - 1; num9 >= 0; num9--)
				{
					PolyCandidate polyCandidate = list[num9];
					bool flag4 = true;
					for (int j = 0; j < num9; j++)
					{
						PolyCandidate polyCandidate2 = list[j];
						if (polyCandidate.sinThetaSqrd >= polyCandidate2.sinThetaSqrd && polyCandidate.sinPsiSqrd >= polyCandidate2.sinPsiSqrd)
						{
							flag4 = false;
							break;
						}
					}
					if (!flag4)
					{
						list.RemoveAt(num9);
					}
					else
					{
						for (int k = 0; k < edgesToCheck.Count; k++)
						{
							if (LineSegmentsCross(value, polyCandidate.pos, edgesToCheck[k].a, edgesToCheck[k].b))
							{
								flag4 = false;
								break;
							}
						}
						if (!flag4)
						{
							list.RemoveAt(num9);
						}
					}
				}
				if (list.Count > 0)
				{
					candidates.Add(linkedListNode, list);
				}
				else
				{
					refinedEdges.Add(linkedListNode);
				}
				linkedListNode = linkedListNode2;
			}
			foreach (LinkedListNode<Vector2> key in candidates.Keys)
			{
				List<PolyCandidate> list2 = candidates[key];
				for (int l = 0; l < list2.Count; l++)
				{
					PolyCandidate polyCandidate3 = list2[l];
					if (!minDepths.ContainsKey(polyCandidate3.pos))
					{
						minDepths.Add(polyCandidate3.pos, float.MaxValue);
					}
					minDepths[polyCandidate3.pos] = Math.Min(minDepths[polyCandidate3.pos], polyCandidate3.depth);
				}
			}
			foreach (LinkedListNode<Vector2> key2 in candidates.Keys)
			{
				List<PolyCandidate> list3 = candidates[key2];
				for (int num10 = list3.Count - 1; num10 >= 0; num10--)
				{
					Vector2 value3 = key2.Value;
					PolyCandidate polyCandidate4 = list3[num10];
					Vector2 pos2 = polyCandidate4.pos;
					LinkedListNode<Vector2> linkedListNode3 = key2.Next ?? hull.First;
					if (minDepths[pos2] == polyCandidate4.depth && interiorPoints.Contains(pos2))
					{
						flag2 = true;
						interiorPoints.Remove(pos2);
						hull.AddAfter(key2, pos2);
						PolyEdge obj = edgeMap[value3];
						edges.Remove(obj);
						edgeMap.Remove(value3);
						PolyEdge polyEdge = new PolyEdge(value3, pos2);
						polyEdge.AddToQuadTree(edges);
						edgeMap.Add(value3, polyEdge);
						PolyEdge polyEdge2 = new PolyEdge(pos2, linkedListNode3.Value);
						polyEdge2.AddToQuadTree(edges);
						edgeMap.Add(pos2, polyEdge2);
						break;
					}
				}
			}
			candidates.Clear();
			edgesToCheck.Clear();
			minDepths.Clear();
			polyCandidatePool.Clear();
			flag = flag || flag2;
		}
		return flag;
	}

	public static int RemoveEdges(List<Vector2> hull, float lengthThreshold, QuadTreeV2 interiorPoints, Dictionary<Vector2, PolyEdge> edgeMap, QuadTree<PolyEdge> edges)
	{
		int num = 0;
		List<int> list = new List<int>();
		bool flag = true;
		lengthThreshold *= lengthThreshold;
		Vector2 vector3 = default(Vector2);
		Vector2 vector4 = default(Vector2);
		while (flag)
		{
			flag = false;
			int num2 = Math.Max(0, (hull.Count - 1) / 2 - 1);
			for (int i = 0; i < hull.Count; i++)
			{
				Vector2 vector = hull[i];
				Vector2 vector2 = hull[(i + 1) % hull.Count];
				vector3.x = vector2.x - vector.x;
				vector3.y = vector2.y - vector.y;
				float num3 = vector3.x * vector3.x + vector3.y * vector3.y;
				if (num3 > lengthThreshold)
				{
					int? num4 = null;
					float num5 = num3 * 0.5f;
					int num6 = (i + num2) % hull.Count;
					int num7 = i + 1;
					while (num7 != num6)
					{
						num7 = (num7 + 1) % hull.Count;
						vector4.x = hull[num7].x - vector.x;
						vector4.y = hull[num7].y - vector.y;
						float num8 = vector4.x * vector4.x + vector4.y * vector4.y;
						if (num8 < num5)
						{
							num5 = num8;
							num4 = num7;
						}
					}
					if (num4.HasValue)
					{
						for (int num9 = (i + 1) % hull.Count; num9 != num4.Value; num9 = (num9 + 1) % hull.Count)
						{
							list.Add(num9);
						}
						flag = true;
					}
				}
				if (flag)
				{
					break;
				}
			}
			if (!flag)
			{
				for (int num10 = hull.Count - 1; num10 >= 0; num10--)
				{
					Vector2 vector = hull[num10];
					int num11 = ((num10 - 1 >= 0) ? (num10 - 1) : (hull.Count - 1));
					Vector2 vector2 = hull[num11];
					vector3.x = vector2.x - vector.x;
					vector3.y = vector2.y - vector.y;
					float num12 = vector3.x * vector3.x + vector3.y * vector3.y;
					if (num12 > lengthThreshold)
					{
						int? num13 = null;
						float num14 = num12 * 0.5f;
						int num15 = num10 - num2;
						if (num15 < 0)
						{
							num15 = hull.Count + num15;
						}
						for (int num16 = ((num11 - 1 >= 0) ? (num11 - 1) : (hull.Count - 1)); num16 != num15; num16 = ((num16 - 1 >= 0) ? (num16 - 1) : (hull.Count - 1)))
						{
							vector4.x = hull[num16].x - vector.x;
							vector4.y = hull[num16].y - vector.y;
							float num17 = vector4.x * vector4.x + vector4.y * vector4.y;
							if (num17 < num14)
							{
								num14 = num17;
								num13 = num16;
							}
						}
						if (num13.HasValue)
						{
							for (int num18 = num11; num18 != num13.Value; num18 = ((num18 - 1 >= 0) ? (num18 - 1) : (hull.Count - 1)))
							{
								list.Add(num18);
							}
							flag = true;
						}
					}
					if (flag)
					{
						break;
					}
				}
			}
			if (list.Count > 0)
			{
				list.Sort();
				int num19 = list[0] - 1;
				num19 = ((num19 >= 0) ? num19 : (hull.Count - 1));
				int index = (list[list.Count - 1] + 1) % hull.Count;
				Vector2 vector5 = hull[num19];
				Vector2 b = hull[index];
				edges.Remove(edgeMap[vector5]);
				edgeMap.Remove(vector5);
				PolyEdge polyEdge = new PolyEdge(vector5, b);
				edgeMap.Add(vector5, polyEdge);
				polyEdge.AddToQuadTree(edges);
				for (int num20 = list.Count - 1; num20 >= 0; num20--)
				{
					num++;
					Vector2 key = hull[list[num20]];
					edges.Remove(edgeMap[key]);
					edgeMap.Remove(key);
					hull.RemoveAt(list[num20]);
				}
				list.Clear();
			}
		}
		return num;
	}

	public static Mesh PolyToMesh(List<Vector2> poly, Color? color = null)
	{
		color = color ?? Color.white;
		ContourOrientation forceOrientation = ContourOrientation.CounterClockwise;
		WindingRule windingRule = WindingRule.EvenOdd;
		ElementType elementType = ElementType.Polygons;
		int polySize = 3;
		Tess tess = new Tess();
		ContourVertex[] array = new ContourVertex[poly.Count];
		for (int i = 0; i < poly.Count; i++)
		{
			array[i].Position.X = poly[i].x;
			array[i].Position.Y = poly[i].y;
			array[i].Position.Z = 0f;
		}
		tess.AddContour(array, forceOrientation);
		tess.Tessellate(windingRule, elementType, polySize);
		Vector3[] array2 = new Vector3[tess.VertexCount];
		Vector3[] array3 = new Vector3[tess.VertexCount];
		Vector2[] uv = new Vector2[tess.VertexCount];
		Color[] array4 = new Color[tess.VertexCount];
		for (int j = 0; j < tess.VertexCount; j++)
		{
			Vector3 vector = tess.Vertices[j].Position.ToVector3();
			array2[j] = vector;
			array3[j] = -Vector3.forward;
			array4[j] = color.Value;
		}
		return new Mesh
		{
			vertices = array2,
			normals = array3,
			uv = uv,
			colors = array4,
			triangles = tess.Elements
		};
	}

	public static Rect GetBoundingRect(this PolygonCollider2D p)
	{
		float num = float.MaxValue;
		float num2 = float.MinValue;
		float num3 = float.MaxValue;
		float num4 = float.MinValue;
		for (int i = 0; i < p.pathCount; i++)
		{
			Vector2[] path = p.GetPath(i);
			for (int j = 0; j < path.Length; j++)
			{
				Vector2 vector = path[j];
				num = ((vector.x < num) ? vector.x : num);
				num2 = ((vector.x > num2) ? vector.x : num2);
				num3 = ((vector.y < num3) ? vector.y : num3);
				num4 = ((vector.y > num4) ? vector.y : num4);
			}
		}
		return new Rect(num, num3, num2 - num, num4 - num3);
	}

	private static float _side(float x1, float y1, float x2, float y2, float x, float y)
	{
		return (y2 - y1) * (x - x1) + (0f - x2 + x1) * (y - y1);
	}

	public static bool NaivePointInTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
	{
		if (_side(a.x, a.y, b.x, b.y, p.x, p.y) >= 0f && _side(b.x, b.y, c.x, c.y, p.x, p.y) >= 0f)
		{
			return _side(c.x, c.y, a.x, a.y, p.x, p.y) >= 0f;
		}
		return false;
	}

	private static bool _pointInTriangleBoundingBox(float x1, float y1, float x2, float y2, float x3, float y3, float x, float y)
	{
		float num = Math.Min(x1, Math.Min(x2, x3)) - Epsilon;
		float num2 = Math.Max(x1, Math.Max(x2, x3)) + Epsilon;
		float num3 = Math.Min(y1, Math.Min(y2, y3)) - Epsilon;
		float num4 = Math.Max(y1, Math.Max(y2, y3)) + Epsilon;
		if (x >= num && x <= num2 && y >= num3)
		{
			return y <= num4;
		}
		return false;
	}

	private static float _distanceSquarePointToSegment(float x1, float y1, float x2, float y2, float x, float y)
	{
		float num = (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1);
		float num2 = ((x - x1) * (x2 - x1) + (y - y1) * (y2 - y1)) / num;
		if (num2 < 0f)
		{
			return (x - x1) * (x - x1) + (y - y1) * (y - y1);
		}
		if (num2 <= 1f)
		{
			return (x1 - x) * (x1 - x) + (y1 - y) * (y1 - y) - num2 * num2 * num;
		}
		return (x - x2) * (x - x2) + (y - y2) * (y - y2);
	}

	public static bool AccuratePointInTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
	{
		if (!_pointInTriangleBoundingBox(a.x, a.y, b.x, b.y, c.x, c.y, p.x, p.y))
		{
			return false;
		}
		if (NaivePointInTriangle(a, b, c, p))
		{
			return true;
		}
		if (_distanceSquarePointToSegment(a.x, a.y, b.x, b.y, p.x, p.y) <= Epsilon)
		{
			return true;
		}
		if (_distanceSquarePointToSegment(b.x, b.y, c.x, c.y, p.x, p.y) <= Epsilon)
		{
			return true;
		}
		if (_distanceSquarePointToSegment(c.x, c.y, a.x, a.y, p.x, p.y) <= Epsilon)
		{
			return true;
		}
		return false;
	}

	public static Vector3 TriangleNormal(Vector3 a, Vector3 b, Vector3 c)
	{
		Vector3 lhs = default(Vector3);
		Vector3 rhs = default(Vector3);
		lhs.x = b.x - a.x;
		lhs.y = b.y - a.y;
		lhs.z = b.z - a.z;
		rhs.x = c.x - b.x;
		rhs.y = c.y - b.y;
		rhs.z = c.z - b.z;
		Vector3 result = Vector3.Cross(lhs, rhs);
		float num = (float)Math.Sqrt(result.x * result.x + result.y * result.y + result.z * result.z);
		num = ((num > Epsilon) ? num : Epsilon);
		result.x /= num;
		result.y /= num;
		result.z /= num;
		return result;
	}

	public static Color ToColor01(this float f)
	{
		Color result = new Color(f, 255f * f, TwoFiftyFivePow2 * f, TwoFiftyFivePow3 * f);
		result.r -= (int)result.r;
		result.g -= (int)result.g;
		result.b -= (int)result.b;
		result.a -= (int)result.a;
		result.r -= result.g * OneTwoFiftyFifth;
		result.g -= result.b * OneTwoFiftyFifth;
		result.b -= result.a * OneTwoFiftyFifth;
		return result;
	}

	public static float ToFloat01(this Color c)
	{
		return Vector4.Dot(c, new Vector4(1f, OneTwoFiftyFifth, OneTwoFiftyFifthPow2, OneTwoFiftyFifthPow3));
	}

	public static Color ToColor(this float f)
	{
		Color color = default(Color);
		float num = Math.Sign(f);
		f *= num;
		f = Math.Min(f, 65279f);
		color.r = (int)(f * OneTwoFiftyFifth);
		f -= color.r * 255f;
		color.g = (int)f;
		f -= color.g;
		color.b = (int)(f * 255f);
		f -= color.b * OneTwoFiftyFifth;
		color.a = Math.Min((int)(f * TwoPow15), 127) + ((num >= 0f) ? 128 : 0);
		return color * OneTwoFiftyFifth;
	}

	public static float ToFloat(this Color c)
	{
		c *= 255f;
		float num = 0f + c.r * 255f + c.g + c.b * OneTwoFiftyFifth;
		int num2 = (int)Math.Min(1f, c.a * OneOneTwentyEigth);
		c.a -= num2 * 128;
		return (num + c.a * OneOverTwoPow15) * (float)(num2 + num2 - 1);
	}

	public static float TrajectoryMinSpeed(Vector3 start, Vector3 end, float g)
	{
		Vector3 vector = new Vector3(end.x - start.x, 0f, end.z - start.z);
		float num = start.y - end.y;
		float magnitude = vector.magnitude;
		return (float)Math.Sqrt(QuadraticPositive(1f, -2f * g * num, 0f - g * g * magnitude * magnitude)) + 0.1f;
	}

	public static Vector4 TrajectoryVelocityAndLifetime(Vector3 start, Vector3 end, float speed, float g)
	{
		speed = Math.Max(TrajectoryMinSpeed(start, end, g), speed);
		Vector3 vector = end - start;
		float num = 0f - vector.y;
		vector.y = 0f;
		float magnitude = vector.magnitude;
		float num2 = speed;
		Vector3 vector2 = vector / magnitude;
		float num3 = Mathf.Atan(QuadraticSmallestReal(0.5f * g * magnitude, 0f - num2 * num2, (g * magnitude * magnitude + 2f * num * num2 * num2) / (magnitude + magnitude)));
		Vector3 axis = Vector3.Cross(vector2, Vector3.up);
		axis.Normalize();
		vector2 = Quaternion.AngleAxis((0f - num3) * 57.29578f, axis) * vector2;
		Vector3 vector3 = vector2 * speed;
		float w = magnitude / (num2 * Mathf.Cos(num3));
		return new Vector4(vector3.x, vector3.y, vector3.z, w);
	}

	public static Vector4 TrajectoryVelocityAndLifetimeConstantSpeed(Vector3 start, Vector3 end, float speed, float g)
	{
		Vector3 vector = end - start;
		Vector2 vector2 = end.Project(AxisType.Y) - start.Project(AxisType.Y);
		float magnitude = vector2.magnitude;
		Vector2 vector3 = vector2 / magnitude;
		float num = magnitude / speed;
		float num2 = 0f - vector.y;
		return new Vector4(vector3.x * speed, g * num * -0.5f - num2 / num, vector3.y * speed, num);
	}

	public static float TimeToLinearImpact(float distance, float initialSpeed, float acceleration)
	{
		return QuadraticPositive(acceleration * 0.5f, initialSpeed, 0f - distance);
	}

	public static float TimeToLinearImpact(float distance, float initialSpeed, float acceleration, float topSpeed)
	{
		if (initialSpeed >= topSpeed)
		{
			acceleration = 0f;
		}
		initialSpeed = Math.Min(initialSpeed, topSpeed);
		float num = acceleration * 0.5f;
		if (acceleration > 0f)
		{
			float num2 = (topSpeed - initialSpeed) / acceleration;
			float num3 = initialSpeed * num2 + num * num2 * num2;
			if (num3 < distance)
			{
				return TimeToLinearImpact(num3, initialSpeed, acceleration, topSpeed) + TimeToLinearImpact(distance - num3, topSpeed, 0f, topSpeed);
			}
		}
		return QuadraticPositive(num, initialSpeed, 0f - distance);
	}

	public static float LinearDistanceTraveled(float time, float initialSpeed, float acceleration)
	{
		return initialSpeed * time + acceleration * 0.5f * time * time;
	}

	public static float LinearDistanceTraveled(float time, float initialSpeed, float acceleration, float topSpeed)
	{
		if (initialSpeed >= topSpeed)
		{
			acceleration = 0f;
		}
		initialSpeed = Math.Min(initialSpeed, topSpeed);
		if (acceleration > 0f)
		{
			float num = (topSpeed - initialSpeed) / acceleration;
			if (num < time)
			{
				return LinearDistanceTraveled(num, initialSpeed, acceleration, topSpeed) + LinearDistanceTraveled(time - num, topSpeed, 0f, topSpeed);
			}
		}
		return initialSpeed * time + acceleration * 0.5f * time * time;
	}

	public static float InitialVelocityToImpactAtTime(float time, float distance, float acceleration)
	{
		return (0f - (distance + acceleration * 0.5f * time * time)) / time;
	}

	public static (float height, float bounceSpeed) BounceHeight(float time, float startingHeight, float initialUpwardSpeed, float gravity, float radius, float bounciness, float bounceHeightThreshold = 0.01f)
	{
		float num = TimeToLinearImpact(startingHeight - radius, 0f - initialUpwardSpeed, 0f - gravity);
		if (time <= num)
		{
			return (startingHeight + LinearDistanceTraveled(time, initialUpwardSpeed, gravity), initialUpwardSpeed);
		}
		float num2 = (initialUpwardSpeed + gravity * num) * (0f - bounciness);
		if (LinearDistanceTraveled((0f - num2) / gravity, num2, gravity) < bounceHeightThreshold)
		{
			return (radius, 0f);
		}
		return BounceHeight(time - num, radius, num2, gravity, radius, bounciness, bounceHeightThreshold);
	}

	public static float StoppingDistance(float speed, float acceleration)
	{
		return speed * speed / Math.Abs(acceleration + acceleration).InsureNonZero();
	}

	public static float StopDistanceDeceleration(float speed, float distance)
	{
		return speed * speed / (distance + distance).InsureNonZero() * (float)(-Math.Sign(speed));
	}

	public static float AccelerationWithStoppingDistance(float speed, float desiredAccelerationMagnitude, float distanceRemaining)
	{
		if (!(distanceRemaining < StoppingDistance(speed, desiredAccelerationMagnitude)))
		{
			return desiredAccelerationMagnitude;
		}
		return StopDistanceDeceleration(speed, distanceRemaining);
	}

	public static bool DoPhysicalMovement1D(ref float position, ref float velocity, float accelerationMagnitude, float desiredPosition, float time)
	{
		float num = desiredPosition - position;
		int num2 = Math.Sign(num);
		float num3 = num * (float)num2;
		int num4 = Math.Sign(velocity);
		float num5 = velocity * (float)num4;
		if (num5 * time >= num3)
		{
			position = desiredPosition;
			velocity = 0f;
			return true;
		}
		float num6 = ((StoppingDistance(velocity, accelerationMagnitude) < num3) ? accelerationMagnitude : StopDistanceDeceleration(num5, num3));
		velocity += num6 * (float)num2 * time;
		position += velocity * time;
		return false;
	}

	public static Vector3 ToVector3(this float f)
	{
		return new Vector3(f, f, f);
	}

	public static Vector2 Direction2DFromRadians(this float radians)
	{
		return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
	}

	public static Vector4 ToVector4(this Vector3 v, float w)
	{
		return new Vector4(v.x, v.y, v.z, w);
	}

	public static Vector2 ToVector2(this Vector4 v)
	{
		return new Vector2(v.x, v.y);
	}

	public static Vector3 ToVector3(this Vector4 v)
	{
		return new Vector3(v.x, v.y, v.z);
	}

	public static Quaternion ToQuaternion(this Vector4 v)
	{
		v = v.normalized;
		return new Quaternion(v.x, v.y, v.z, v.w);
	}

	public static Color DeltaSnap(this Color c, Color target, float delta = 0.01f)
	{
		DeltaSnap(ref c.r, target.r, delta);
		DeltaSnap(ref c.g, target.g, delta);
		DeltaSnap(ref c.b, target.b, delta);
		DeltaSnap(ref c.a, target.a, delta);
		return c;
	}

	public static Vector3 Unproject(this Vector2 v, AxisType axisThatWasProjected, float axisThatWasProjectedValue = 0f)
	{
		return axisThatWasProjected switch
		{
			AxisType.X => new Vector3(axisThatWasProjectedValue, v.x, v.y), 
			AxisType.Y => new Vector3(v.x, axisThatWasProjectedValue, v.y), 
			_ => new Vector3(v.x, v.y, axisThatWasProjectedValue), 
		};
	}

	public static Vector2 Project(this Vector3 v, AxisType axisToRemove)
	{
		return axisToRemove switch
		{
			AxisType.X => new Vector2(v.y, v.z), 
			AxisType.Y => new Vector2(v.x, v.z), 
			_ => new Vector2(v.x, v.y), 
		};
	}

	public static Vector3 AbsProject(this Vector3 v, Vector3 normal, float minMagnitude = 0f)
	{
		return normal * Math.Max(Mathf.Abs(Vector3.Dot(v, normal)), minMagnitude);
	}

	public static Vector3 NormalizeSafe(this Vector3 v, Vector3 fallBackNormal)
	{
		Vector3 normalized = v.normalized;
		if (!(normalized.sqrMagnitude > 0f))
		{
			return fallBackNormal;
		}
		return normalized;
	}

	public static Int2 ToCardinal(this Vector2 v)
	{
		bool flag = Math.Abs(v.x) >= Math.Abs(v.y);
		return new Int2(flag ? Math.Sign(v.x) : 0, (!flag) ? Math.Sign(v.y) : 0);
	}

	public static Int2 ToInt2(this Vector3 v, int multipleOf = 1)
	{
		return new Int2(RoundToNearestMultipleOfInt(v.x, multipleOf), RoundToNearestMultipleOfInt(v.z, multipleOf));
	}

	public static Vector2 Multiply(this Vector2 v, Vector2 multiplier)
	{
		return new Vector2(v.x * multiplier.x, v.y * multiplier.y);
	}

	public static Vector3 Multiply(this Vector3 v, Vector3 multiplier)
	{
		return new Vector3(v.x * multiplier.x, v.y * multiplier.y, v.z * multiplier.z);
	}

	public static Vector4 MultiplyV4(this Vector4 v, Vector4 multiplier)
	{
		return new Vector4(v.x * multiplier.x, v.y * multiplier.y, v.z * multiplier.z, v.w * multiplier.w);
	}

	public static Vector3 Multiply(this Vector3 v, float x, float y, float z)
	{
		return new Vector3(v.x * x, v.y * y, v.z * z);
	}

	public static float Negate(this float f, bool condition)
	{
		if (!condition)
		{
			return f;
		}
		return 0f - f;
	}

	public static float Inverse(this float f, bool condition)
	{
		if (!condition)
		{
			return f;
		}
		return 1f / f.InsureNonZero();
	}

	public static Vector2 Inverse(this Vector2 v)
	{
		return new Vector2(1f / v.x, 1f / v.y);
	}

	public static Vector2 OneMinus(this Vector2 v)
	{
		return new Vector2(1f - v.x, 1f - v.y);
	}

	public static Vector2 OneMinus(this Vector2 v, bool xAxis, bool yAxis)
	{
		return new Vector2(xAxis ? (1f - v.x) : v.x, yAxis ? (1f - v.y) : v.y);
	}

	public static float OneMinusIf(this float f, bool condition = true)
	{
		if (!condition)
		{
			return f;
		}
		return 1f - f;
	}

	public static int OneMinusIf(this int i, bool condition)
	{
		if (!condition)
		{
			return i;
		}
		return 1 - i;
	}

	public static byte ToByte(this int i)
	{
		return (byte)((i > 0) ? ((i >= 255) ? 255u : ((uint)i)) : 0u);
	}

	public static byte ToByte(this ushort i)
	{
		return (byte)((i > 0) ? ((uint)((i >= 255) ? 255 : i)) : 0u);
	}

	public static Vector2 MinusOne(this Vector2 v)
	{
		return new Vector2(v.x - 1f, v.y - 1f);
	}

	public static Vector2 Negative(this Vector2 v, bool xAxis = true, bool yAxis = true)
	{
		return new Vector2(xAxis ? (0f - v.x) : v.x, yAxis ? (0f - v.y) : v.y);
	}

	public static Vector2 InsureNonZero(this Vector2 v)
	{
		return new Vector2(v.x.InsureNonZero(), v.y.InsureNonZero());
	}

	public static Vector3 InsureNonZero(this Vector3 v)
	{
		return new Vector3(v.x.InsureNonZero(), v.y.InsureNonZero(), v.z.InsureNonZero());
	}

	public static bool IsInRange(this Vector2 v, float f)
	{
		if (f >= v.x)
		{
			return f <= v.y;
		}
		return false;
	}

	public static Vector2 Rotate(this Vector2 vector, float degrees)
	{
		degrees *= MathF.PI / 180f;
		float num = Mathf.Cos(degrees);
		float num2 = Mathf.Sin(0f - degrees);
		Vector2 vector2 = vector;
		vector.x = vector2.x * num - vector2.y * num2;
		vector.y = vector2.x * num2 + vector2.y * num;
		return vector;
	}

	public static Vector2 Rotate(this Vector2 vector, float cos, float sin)
	{
		Vector2 vector2 = vector;
		vector.x = vector2.x * cos - vector2.y * sin;
		vector.y = vector2.x * sin + vector2.y * cos;
		return vector;
	}

	public static float ToAngle(this Vector2 v)
	{
		return Mathf.Atan2(v.y, v.x);
	}

	public static Vector3 Inverse(this Vector3 v)
	{
		return new Vector3(1f / v.x.InsureNonZero(), 1f / v.y.InsureNonZero(), 1f / v.z.InsureNonZero());
	}

	public static Vector3 Clamp(this Vector3 v, float min = 0f, float max = 1f)
	{
		return new Vector3(Mathf.Clamp(v.x, min, max), Mathf.Clamp(v.y, min, max), Mathf.Clamp(v.z, min, max));
	}

	public static Vector3 Clamp(this Vector3 v, Vector3 min, Vector3 max)
	{
		return new Vector3(Mathf.Clamp(v.x, min.x, max.x), Mathf.Clamp(v.y, min.y, max.y), Mathf.Clamp(v.z, min.z, max.z));
	}

	public static Vector3 OneMinus(this Vector3 v)
	{
		return new Vector3(1f - v.x, 1f - v.y, 1f - v.z);
	}

	public static Vector3 Min(this Vector3 a, Vector3 b)
	{
		return new Vector3(Math.Min(a.x, b.x), Math.Min(a.y, b.y), Math.Min(a.z, b.z));
	}

	public static Vector3 Max(this Vector3 a, Vector3 b)
	{
		return new Vector3(Math.Max(a.x, b.x), Math.Max(a.y, b.y), Math.Max(a.z, b.z));
	}

	public static Vector3 Max(this Vector3 a, float b)
	{
		return new Vector3(Math.Max(a.x, b), Math.Max(a.y, b), Math.Max(a.z, b));
	}

	public static float Max(this Vector3 a)
	{
		return Math.Max(a.x, Math.Max(a.y, a.z));
	}

	public static Vector2 SetAxis(this Vector2 v, int axis, float value)
	{
		v[axis] = value;
		return v;
	}

	public static Vector3 SetAxisValues(this Vector3 v, float? x = null, float? y = null, float? z = null)
	{
		return new Vector3(x ?? v.x, y ?? v.y, z ?? v.z);
	}

	public static Vector3 SetAxis(this Vector3 v, AxisType axis, float value = 0f)
	{
		v[(int)axis] = value;
		return v;
	}

	public static void SetComponentValues(this Vector2 v, ref float x, ref float y)
	{
		x = v.x;
		y = v.y;
	}

	public static Vector3 RoundAxisToNearestMultipleOf(this Vector3 v, AxisType axis, float multipleOf)
	{
		v[(int)axis] = RoundToNearestMultipleOf(v[(int)axis], multipleOf);
		return v;
	}

	public static Vector3 CeilToMultipleOf(this Vector3 v, Vector3 multipleOf)
	{
		return new Vector3(CeilingToNearestMultipleOf(v.x, multipleOf.x), CeilingToNearestMultipleOf(v.y, multipleOf.y), CeilingToNearestMultipleOf(v.z, multipleOf.z));
	}

	public static float AngleSigned(this Vector3 from, Vector3 to, Vector3 up)
	{
		return Mathf.Atan2(Vector3.Dot(up, Vector3.Cross(from, to)), Vector3.Dot(from, to)) * 57.29578f;
	}

	public static Bounds InverseTransform(this Bounds bounds, Transform transform)
	{
		bounds.center = transform.InverseTransformPoint(bounds.center);
		bounds.size = transform.InverseTransformDirection(bounds.size);
		bounds.size = bounds.size.Multiply(transform.localScale.Inverse());
		return bounds;
	}

	public static Bounds Transform(this Bounds localBounds, Transform transform)
	{
		Vector3 center = transform.TransformPoint(localBounds.center);
		Vector3 extents = localBounds.extents;
		Vector3 vector = transform.TransformVector(extents.x, 0f, 0f);
		Vector3 vector2 = transform.TransformVector(0f, extents.y, 0f);
		Vector3 vector3 = transform.TransformVector(0f, 0f, extents.z);
		extents.x = Mathf.Abs(vector.x) + Mathf.Abs(vector2.x) + Mathf.Abs(vector3.x);
		extents.y = Mathf.Abs(vector.y) + Mathf.Abs(vector2.y) + Mathf.Abs(vector3.y);
		extents.z = Mathf.Abs(vector.z) + Mathf.Abs(vector2.z) + Mathf.Abs(vector3.z);
		Bounds result = default(Bounds);
		result.center = center;
		result.extents = extents;
		return result;
	}

	public static Rect Project(this Bounds bounds, AxisType axisToProject)
	{
		Vector3 min = bounds.min;
		Vector3 max = bounds.max;
		return axisToProject switch
		{
			AxisType.X => Rect.MinMaxRect(min.y, min.z, max.y, max.z), 
			AxisType.Y => Rect.MinMaxRect(min.x, min.z, max.x, max.z), 
			AxisType.Z => Rect.MinMaxRect(min.x, min.y, max.x, max.y), 
			_ => throw new ArgumentException(), 
		};
	}

	public static Bounds Unproject(this Rect rect, AxisType axisThatWasProjected, Vector2 axisThatWasProjectedCenterAndSize)
	{
		Vector2 min = rect.min;
		Vector2 max = rect.max;
		Vector2 vector = (max + min) * 0.5f;
		Vector2 vector2 = max - min;
		return axisThatWasProjected switch
		{
			AxisType.X => new Bounds(new Vector3(axisThatWasProjectedCenterAndSize.x, vector.x, vector.y), new Vector3(axisThatWasProjectedCenterAndSize.y, vector2.x, vector2.y)), 
			AxisType.Y => new Bounds(new Vector3(vector.x, axisThatWasProjectedCenterAndSize.x, vector.y), new Vector3(vector2.x, axisThatWasProjectedCenterAndSize.y, vector2.y)), 
			AxisType.Z => new Bounds(new Vector3(vector.x, vector.y, axisThatWasProjectedCenterAndSize.x), new Vector3(vector2.x, vector2.y, axisThatWasProjectedCenterAndSize.y)), 
			_ => throw new ArgumentException(), 
		};
	}

	public static Bounds Scale(this Bounds bounds, Vector3 scale)
	{
		return new Bounds(bounds.center, bounds.size.Multiply(scale));
	}

	public static Bounds Scale(this Bounds bounds, float scale)
	{
		return new Bounds(bounds.center, bounds.size * scale);
	}

	public static Bounds ScaleAndTranslate(this Bounds bounds, float scale, Vector3 translate)
	{
		return new Bounds(bounds.center + translate, bounds.size * scale);
	}

	public static float Min(this Vector2 v)
	{
		if (!(v.x < v.y))
		{
			return v.y;
		}
		return v.x;
	}

	public static float Max(this Vector2 v)
	{
		if (!(v.x > v.y))
		{
			return v.y;
		}
		return v.x;
	}

	public static float AbsMin(this Vector2 v)
	{
		float num = ((v.x > 0f) ? v.x : (0f - v.x));
		float num2 = ((v.y > 0f) ? v.y : (0f - v.y));
		if (!(num < num2))
		{
			return num2;
		}
		return num;
	}

	public static float AbsMax(this Vector2 v)
	{
		float num = ((v.x > 0f) ? v.x : (0f - v.x));
		float num2 = ((v.y > 0f) ? v.y : (0f - v.y));
		if (!(num > num2))
		{
			return num2;
		}
		return num;
	}

	public static int SignedMin(int a, int b)
	{
		if (Math.Abs(a) > Math.Abs(b))
		{
			return b;
		}
		return a;
	}

	public static int SignedMax(int a, int b)
	{
		if (Math.Abs(a) < Math.Abs(b))
		{
			return b;
		}
		return a;
	}

	public static float SignedMin(float a, float b)
	{
		if (!(Math.Abs(a) <= Math.Abs(b)))
		{
			return b;
		}
		return a;
	}

	public static float SignedMax(float a, float b)
	{
		if (!(Math.Abs(a) >= Math.Abs(b)))
		{
			return b;
		}
		return a;
	}

	public static Vector2 SignedMinMax(this Vector2 a, Vector2 b)
	{
		return new Vector2(SignedMin(a.x, b.x), SignedMax(a.y, b.y));
	}

	public static AxisType AbsMaxAxis(this Vector2 v)
	{
		if (!(Math.Abs(v.x) >= Math.Abs(v.y)))
		{
			return AxisType.Y;
		}
		return AxisType.X;
	}

	public static Vector2 KeepAbsMaxAxis(this Vector2 v)
	{
		if (!(Math.Abs(v.x) >= Math.Abs(v.y)))
		{
			return new Vector2(0f, v.y);
		}
		return new Vector2(v.x, 0f);
	}

	public static Vector2 Abs(this Vector2 v)
	{
		return new Vector2((v.x > 0f) ? v.x : (0f - v.x), (v.y > 0f) ? v.y : (0f - v.y));
	}

	public static Vector2 Pow(this Vector2 v, float power)
	{
		return new Vector2(Mathf.Pow(v.x, power), Mathf.Pow(v.y, power));
	}

	public static float AspectRatio(this Vector2 v)
	{
		return Mathf.Abs(v.x / v.y.InsureNonZero());
	}

	public static float InverseAspectRatio(this Vector2 v)
	{
		return Mathf.Abs(v.y / v.x.InsureNonZero());
	}

	public static Vector2 AspectCorrectMin(this Vector2 v)
	{
		float num = ((v.x > 0f) ? v.x : (0f - v.x)).InsureNonZero();
		float num2 = ((v.y > 0f) ? v.y : (0f - v.y)).InsureNonZero();
		if (!(num > num2))
		{
			return new Vector2(1f, num / num2);
		}
		return new Vector2(num2 / num, 1f);
	}

	public static Vector2 AspectCorrectMax(this Vector2 v)
	{
		float num = ((v.x > 0f) ? v.x : (0f - v.x)).InsureNonZero();
		float num2 = ((v.y > 0f) ? v.y : (0f - v.y)).InsureNonZero();
		if (!(num > num2))
		{
			return new Vector2(num2 / num, 1f);
		}
		return new Vector2(1f, num / num2);
	}

	public static Vector2 GetExtrudeScale(this Vector2 size, float scale)
	{
		Vector2 v = size * scale - size;
		float num = v.Min() / v.Max();
		return Vector2.one.Lerp(new Vector2(scale, scale), new Vector2((size.x > size.y) ? num : 1f, (size.y > size.x) ? num : 1f));
	}

	public static float AbsMin(this Vector3 v)
	{
		float num = ((v.x > 0f) ? v.x : (0f - v.x));
		float num2 = ((v.y > 0f) ? v.y : (0f - v.y));
		float num3 = ((v.z > 0f) ? v.z : (0f - v.z));
		if (!(num < num2))
		{
			if (!(num2 < num3))
			{
				return num3;
			}
			return num2;
		}
		if (!(num < num3))
		{
			return num3;
		}
		return num;
	}

	public static float AbsMax(this Vector3 v)
	{
		float num = ((v.x > 0f) ? v.x : (0f - v.x));
		float num2 = ((v.y > 0f) ? v.y : (0f - v.y));
		float num3 = ((v.z > 0f) ? v.z : (0f - v.z));
		if (!(num > num2))
		{
			if (!(num2 > num3))
			{
				return num3;
			}
			return num2;
		}
		if (!(num > num3))
		{
			return num3;
		}
		return num;
	}

	public static bool IsPowerOf(this int i, int p)
	{
		float num = (float)Math.Log(i, p);
		return Math.Abs((double)num - Math.Truncate(num)) < (double)Mathf.Epsilon;
	}

	public static Vector3 ToVector3(this Vec3 v)
	{
		return new Vector3(v.X, v.Y, v.Z);
	}

	public static float NextFloat(this System.Random random)
	{
		return (float)random.NextDouble();
	}

	public static float Range(this System.Random random, float min, float max)
	{
		return Mathf.Lerp(min, max, (float)random.NextDouble());
	}

	public static float RangePow(this System.Random random, float min, float max, float power)
	{
		return Mathf.Lerp(min, max, Mathf.Pow((float)random.NextDouble(), power));
	}

	public static float Range(this System.Random random, Vector2 range)
	{
		return Mathf.Lerp(range.x, range.y, (float)random.NextDouble());
	}

	public static float RangePow(this System.Random random, Vector2 range, float power)
	{
		return Mathf.Lerp(range.x, range.y, Mathf.Pow((float)random.NextDouble(), power));
	}

	public static float RangeExtrema(this System.Random random, Vector2 range, float extremaPower)
	{
		return LerpExtrema(range.x, range.y, random.Value(), extremaPower);
	}

	public static float RangePowExtrema(this System.Random random, Vector2 range, float power, float extremaPower)
	{
		return LerpExtrema(range.x, range.y, (float)Math.Pow(random.Value(), power), extremaPower);
	}

	public static int RangeInt(this System.Random random, Vector2 range, bool inclusiveMax = true)
	{
		return random.Next((int)range.x, (int)range.y + (inclusiveMax ? 1 : 0));
	}

	public static int RangeInt(this System.Random random, Int2 range, bool inclusiveMax = true)
	{
		return random.Next(range.x, range.y + (inclusiveMax ? 1 : 0));
	}

	public static int RangeInt(this System.Random random, Byte2 range, bool inclusiveMax = true)
	{
		return random.Next(range.x, range.y + (inclusiveMax ? 1 : 0));
	}

	public static int RangeInt(this System.Random random, int minRange, int maxRange, bool inclusiveMax = true)
	{
		return random.Next(minRange, maxRange + (inclusiveMax ? 1 : 0));
	}

	public static Vector2 Range(this System.Random random, Vector2 min, Vector2 max)
	{
		return new Vector2(Mathf.Lerp(min.x, max.x, (float)random.NextDouble()), Mathf.Lerp(min.y, max.y, (float)random.NextDouble()));
	}

	public static Vector3 Range(this System.Random random, Vector3 min, Vector3 max)
	{
		return new Vector3(Mathf.Lerp(min.x, max.x, (float)random.NextDouble()), Mathf.Lerp(min.y, max.y, (float)random.NextDouble()), Mathf.Lerp(min.z, max.z, (float)random.NextDouble()));
	}

	public static Vector4 Range(this System.Random random, Vector4 min, Vector4 max)
	{
		return new Vector4(Mathf.Lerp(min.x, max.x, (float)random.NextDouble()), Mathf.Lerp(min.y, max.y, (float)random.NextDouble()), Mathf.Lerp(min.z, max.z, (float)random.NextDouble()), Mathf.Lerp(min.w, max.w, (float)random.NextDouble()));
	}

	public static Vector2 Range(this System.Random random, Vector4 range)
	{
		return random.Range(new Vector2(range.x, range.y), new Vector2(range.z, range.w));
	}

	public static bool Chance(this System.Random random, float chance)
	{
		return random.NextDouble() < (double)chance;
	}

	public static int ChanceRepeating(this System.Random random, float chance, ushort maxRepeats = 100)
	{
		int num = 0;
		while (random.Chance(chance) && num < maxRepeats)
		{
			num++;
		}
		return num;
	}

	public static float Value(this System.Random random)
	{
		return (float)random.NextDouble();
	}

	public static Vector2 Value2(this System.Random random)
	{
		return new Vector2((float)random.NextDouble(), (float)random.NextDouble());
	}

	public static Vector3 Value3(this System.Random random)
	{
		return new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
	}

	public static Vector4 Value4(this System.Random random)
	{
		return new Vector4((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
	}

	public static Vector2 Direction2D(this System.Random random)
	{
		return random.RandomOnUnitCircle2D();
	}

	public static Vector3 Direction(this System.Random random)
	{
		return SphereCoordinate(random.Range(0f, MathF.PI * 2f), random.Range(-MathF.PI / 2f, MathF.PI / 2f));
	}

	public static T Item<T>(this System.Random random, IList<T> list)
	{
		return list[random.Next(0, list.Count)];
	}

	public static T ItemSafe<T>(this System.Random random, IList<T> list)
	{
		if (list.IsNullOrEmpty())
		{
			return default(T);
		}
		return list[random.Next(0, list.Count)];
	}

	public static T ItemSafe<T>(this System.Random random, IList<T> list, T fallback)
	{
		if (list.IsNullOrEmpty())
		{
			return fallback;
		}
		return list[random.Next(0, list.Count)];
	}

	public static T? ItemSafeNullable<T>(this System.Random random, IList<T> list) where T : struct
	{
		if (list.IsNullOrEmpty())
		{
			return null;
		}
		return list[random.Next(0, list.Count)];
	}

	public static T Item<T>(this System.Random random, HashSet<T> hashSet)
	{
		int num = 0;
		int num2 = random.Next(0, hashSet.Count);
		foreach (T item in hashSet)
		{
			if (num++ == num2)
			{
				return item;
			}
		}
		return default(T);
	}

	public static K Key<K, V>(this System.Random random, Dictionary<K, V> dictionary)
	{
		using PoolKeepItemListHandle<K> poolKeepItemListHandle = dictionary.EnumerateKeysSafe();
		return (poolKeepItemListHandle.Count > 0) ? random.Item(poolKeepItemListHandle.value) : default(K);
	}

	public static void Shuffle<T>(this System.Random random, IList<T> list)
	{
		list.Shuffle(random);
	}

	public static IEnumerable<T> Items<T>(this System.Random random, IEnumerable<T> enumerable)
	{
		List<T> items = enumerable.ToList();
		for (int i = items.Count - 1; i >= 0; i--)
		{
			int swapIndex = random.Next(i + 1);
			yield return items[swapIndex];
			items[swapIndex] = items[i];
		}
	}

	public static int Sign(this System.Random random)
	{
		int num = random.Next(0, 2);
		return num + num - 1;
	}

	public static Vector2 Sign2(this System.Random random)
	{
		return new Vector2(random.Sign(), random.Sign());
	}

	public static Vector3 Sign3(this System.Random random)
	{
		return new Vector3(random.Sign(), random.Sign(), random.Sign());
	}

	public static bool Boolean(this System.Random random)
	{
		return random.Next(0, 2) == 1;
	}

	public static Quaternion Rotation(this System.Random random, float degreeIntervals = 90f, AxisType axis = AxisType.Y)
	{
		int maxValue = Mathf.RoundToInt(360f / degreeIntervals);
		float num = (float)random.Next(maxValue) * degreeIntervals;
		return axis switch
		{
			AxisType.X => Quaternion.Euler(num, 0f, 0f), 
			AxisType.Y => Quaternion.Euler(0f, num, 0f), 
			_ => Quaternion.Euler(0f, 0f, num), 
		};
	}

	public static Quaternion LookRotationSafe(this Quaternion q, Vector3 forward, Vector3 upwards)
	{
		if (!(forward != Vector3.zero) || !(upwards != Vector3.zero))
		{
			return q;
		}
		return Quaternion.LookRotation(forward, upwards);
	}

	public static uint UInt(this System.Random random, uint min = 0u, uint max = uint.MaxValue)
	{
		return (uint)Math.Round((double)min + (double)(max - min) * random.NextDouble());
	}

	public static Vector2 Centroid(this IEnumerable<Vector2> vectors)
	{
		Vector2 zero = Vector2.zero;
		int num = 0;
		foreach (Vector2 vector in vectors)
		{
			num++;
			zero += vector;
		}
		return zero / Math.Max(1, num);
	}

	public static Vector3 Centroid(this IEnumerable<Vector3> vectors)
	{
		Vector3 zero = Vector3.zero;
		int num = 0;
		foreach (Vector3 vector in vectors)
		{
			num++;
			zero += vector;
		}
		return zero / Math.Max(1, num);
	}

	public static Vector3? CentroidNullable(this IEnumerable<Vector3> vectors)
	{
		Vector3 zero = Vector3.zero;
		int num = 0;
		foreach (Vector3 vector in vectors)
		{
			num++;
			zero += vector;
		}
		if (num <= 0)
		{
			return null;
		}
		return zero / num;
	}

	public static Short2 Centroid(this List<Short2> points)
	{
		Short2 zero = Short2.Zero;
		float num = 0f;
		foreach (Short2 point in points)
		{
			zero += point;
			num += 1f;
		}
		return (zero.ToVector2() / num).RoundToShort2();
	}

	public static Short2 Centroid(this PoolStructHashSetHandle<Short2> points)
	{
		Short2 zero = Short2.Zero;
		float num = 0f;
		foreach (Short2 point in points)
		{
			zero += point;
			num += 1f;
		}
		return (zero.ToVector2() / num).RoundToShort2();
	}

	public static float ManhattanDistance(this Vector3 v)
	{
		return Math.Abs(v.x) + Math.Abs(v.y) + Math.Abs(v.z);
	}

	public static bool Prox(this float f, float value, float proximityThreshold = 2.5E-10f)
	{
		float num = f - value;
		num = ((num > 0f) ? num : (0f - num));
		return num <= proximityThreshold;
	}

	public static float Cross(this Vector2 u, Vector2 v)
	{
		return u.y * v.x - u.x * v.y;
	}

	public static Vector2 PerpCCW(this Vector2 u)
	{
		return new Vector2(0f - u.y, u.x);
	}

	public static Vector2 PerpCW(this Vector2 u)
	{
		return new Vector2(u.y, 0f - u.x);
	}

	public static Vector2 Swap(this Vector2 u)
	{
		return new Vector2(u.y, u.x);
	}

	public static float Range(this Vector2 u)
	{
		float num = u.y - u.x;
		if (!(num > 0f))
		{
			return 0f - num;
		}
		return num;
	}

	public static float Ratio(this Vector2 u)
	{
		return u.x / u.y.InsureNonZero();
	}

	public static uint GetUnixEpoch(this DateTime dateTime)
	{
		return (uint)((dateTime.ToUniversalTime() - EPOCH).TotalSeconds + 0.5);
	}

	public static DateTime FromUnixEpoch(uint seconds)
	{
		DateTime ePOCH = EPOCH;
		return ePOCH.AddSeconds(seconds);
	}

	public static Vector2 Clamp(this Vector2 v, Vector2 min, Vector2 max)
	{
		v.x = Mathf.Clamp(v.x, min.x, max.x);
		v.y = Mathf.Clamp(v.y, min.y, max.y);
		return v;
	}

	public static Vector4 Clamp(this Vector4 v, Vector4 min, Vector4 max)
	{
		v.x = Mathf.Clamp(v.x, min.x, max.x);
		v.y = Mathf.Clamp(v.y, min.y, max.y);
		v.z = Mathf.Clamp(v.z, min.z, max.z);
		v.w = Mathf.Clamp(v.w, min.w, max.w);
		return v;
	}

	public static Vector4 RotateMinMax(this Vector4 v, int cardinalRotation)
	{
		Vector2 vector = new Vector2(v.x, v.y);
		Vector2 vector2 = new Vector2(v.z, v.w);
		Vector2 vector3 = (vector + vector2) * 0.5f;
		Vector2 vector4 = vector - vector3;
		Vector2 vector5 = vector2 - vector3;
		float num = (float)cardinalRotation * (MathF.PI / 2f);
		float cos = Mathf.Cos(num);
		float sin = Mathf.Sin(0f - num);
		vector4 = vector4.Rotate(cos, sin);
		vector5 = vector5.Rotate(cos, sin);
		return new Vector4(vector3.x + vector4.x, vector3.y + vector4.y, vector3.x + vector5.x, vector3.y + vector5.y);
	}

	public static Vector4 ScaleMinMax(this Vector4 v, Vector2 scale)
	{
		Vector2 vector = new Vector2(v.x, v.y);
		Vector2 vector2 = new Vector2(v.z, v.w);
		Vector2 vector3 = (vector + vector2) * 0.5f;
		Vector2 v2 = vector - vector3;
		Vector2 v3 = vector2 - vector3;
		v2 = v2.Multiply(scale);
		v3 = v3.Multiply(scale);
		return new Vector4(vector3.x + v2.x, vector3.y + v2.y, vector3.x + v3.x, vector3.y + v3.y);
	}

	public static Vector2 LerpMinMax(this Vector4 v, Vector2 t)
	{
		return new Vector2(Lerp(v.x, v.z, t.x), Lerp(v.y, v.w, t.y));
	}

	public static Vector2 Clamp01(this Vector2 v)
	{
		return v.Clamp(Vector2.zero, Vector2.one);
	}

	public static Vector2 ClampMagnitude(this Vector2 v, float maxMagnitude = 1f)
	{
		float magnitude = v.magnitude;
		if (!(magnitude > maxMagnitude))
		{
			return v;
		}
		return v * (maxMagnitude / magnitude);
	}

	public static Vector4 Clamp01(this Vector4 v)
	{
		return v.Clamp(Vector4.zero, Vector4.one);
	}

	public static double Clamp01(double v)
	{
		if (!(v > 1.0))
		{
			if (!(v < 0.0))
			{
				return v;
			}
			return 0.0;
		}
		return 1.0;
	}

	public static double Clamp(double v, double min, double max)
	{
		if (!(v > max))
		{
			if (!(v < min))
			{
				return v;
			}
			return min;
		}
		return max;
	}

	public static float Clamp01(float f, bool clamp = true)
	{
		if (!clamp)
		{
			return f;
		}
		return Mathf.Clamp01(f);
	}

	public static float Average(this Vector2 v)
	{
		return (v.x + v.y) * 0.5f;
	}

	public static float Average(this Vector3 v)
	{
		return (v.x + v.y + v.z) * (1f / 3f);
	}

	public static Vector2 AverageVector(this Vector2 v)
	{
		float num = v.Average();
		return new Vector2(num, num);
	}

	public static Vector2 MakeMinToMax(this Vector2 v)
	{
		return new Vector2((v.x < v.y) ? v.x : v.y, (v.x > v.y) ? v.x : v.y);
	}

	public static void InsureMinToMax(ref float min, ref float max)
	{
		if (min > max)
		{
			max = min;
		}
	}

	public static void InsureMinToMax(ref int min, ref int max)
	{
		if (min > max)
		{
			max = min;
		}
	}

	public static void InsureMinToMax(float previousMin, ref float min, ref float max)
	{
		if (min > max)
		{
			if (previousMin <= max)
			{
				max = min;
			}
			min = max;
		}
	}

	public static void InsureWithinDistances(ref float min, float minPrevious, ref float max, float maxPrevious, float minDistance = 0f, float maxDistance = float.MaxValue, float minRange = float.MinValue, float maxRange = float.MaxValue)
	{
		float num = max - min;
		float num2 = ((num < minDistance) ? (minDistance - num) : ((num > maxDistance) ? (maxDistance - num) : 0f));
		if (num2 != 0f)
		{
			float value = min - minPrevious;
			float value2 = max - maxPrevious;
			float num3 = Math.Abs(value);
			float num4 = Math.Abs(value2);
			float val = min - minRange;
			float val2 = maxRange - max;
			if (num3 > num4)
			{
				float num5 = Math.Min(num2, val2);
				max += num5;
				min -= num2 - num5;
			}
			else
			{
				float num6 = Math.Min(num2, val);
				min -= num6;
				max += num2 - num6;
			}
		}
	}

	public static Vector2 Step(this Vector2 v, Vector2 step)
	{
		if (step.x > 0f)
		{
			v.x = RoundToNearestMultipleOf(v.x, step.x);
		}
		if (step.y > 0f)
		{
			v.y = RoundToNearestMultipleOf(v.y, step.y);
		}
		return v;
	}

	public static Vector2 Lerp(this Vector2 start, Vector2 end, Vector2 t)
	{
		return new Vector2(Mathf.Lerp(start.x, end.x, t.x), Mathf.Lerp(start.y, end.y, t.y));
	}

	public static Vector2 GetLerpAmounts(this Vector2 start, Vector2 end, Vector2 amount)
	{
		return new Vector2(GetLerpAmount(start.x, end.x, amount.x), GetLerpAmount(start.y, end.y, amount.y));
	}

	public static bool LessThanOrEqualTo(this Vector2 a, Vector2 b)
	{
		if (a.x <= b.x)
		{
			return a.y <= b.y;
		}
		return false;
	}

	public static float Decimal(this float f)
	{
		return f - (float)(int)f;
	}

	public static float Abs(this float f)
	{
		if (!(f >= 0f))
		{
			return 0f - f;
		}
		return f;
	}

	public static float AbsDecimal(this float f)
	{
		float num = f - (float)(int)f;
		if (!(num > 0f))
		{
			return 0f - num;
		}
		return num;
	}

	public static float InsureNonZero(this float f)
	{
		if (f == 0f)
		{
			return BigEpsilon;
		}
		return f;
	}

	public static float InsureNonZero(this float f, float valueIfZero)
	{
		if (f == 0f)
		{
			return valueIfZero;
		}
		return f;
	}

	public static double InsureNonZero(this double d)
	{
		if (d == 0.0)
		{
			return BigEpsilon;
		}
		return d;
	}

	public static int InsureNonZero(this int i)
	{
		if (i == 0)
		{
			return 1;
		}
		return i;
	}

	public static Vector2 Pad(this float f, float padding)
	{
		return new Vector2(f - padding, f + padding);
	}

	public static Rect Pad(this Rect r, Vector2 pad)
	{
		return new Rect(r.xMin - pad.x, r.yMin - pad.y, r.width + pad.x + pad.x, r.height + pad.y + pad.y);
	}

	public static float Area(this Rect r)
	{
		return r.width * r.height;
	}

	public static Rect Clip(this Rect r, Rect clipRect)
	{
		Vector2 vector = Vector2.Max(r.min, clipRect.min);
		Vector2 vector2 = Vector2.Min(r.max, clipRect.max);
		return Rect.MinMaxRect(vector.x, vector.y, vector2.x, vector2.y);
	}

	public static Vector2 Lerp(this Rect r, Vector2 lerp)
	{
		return r.min.Lerp(r.max, lerp);
	}

	public static Vector2 GetLerpAmounts(this Rect r, Vector2 value)
	{
		return r.min.GetLerpAmounts(r.max, value);
	}

	public static Rect ScaleFromCenter(this Rect r, float scale)
	{
		Vector2 center = r.center;
		r.max = center + (r.max - center) * scale;
		r.min = center + (r.min - center) * scale;
		return r;
	}

	public static Rect ScaleFromCenter(this Rect r, Vector2 scaleVector)
	{
		Vector2 center = r.center;
		r.max = center + (r.max - center).Multiply(scaleVector);
		r.min = center + (r.min - center).Multiply(scaleVector);
		return r;
	}

	public static Rect ExtrudeFromCenter(this Rect r, float scale)
	{
		return r.ScaleFromCenter(r.size.GetExtrudeScale(scale));
	}

	public static Vector2 NormalizedPosition(this Rect r, Vector2 position)
	{
		return Rect.PointToNormalized(r, position);
	}

	public static Rect NormalizedRect(this Rect r, Rect otherRect)
	{
		Vector2 vector = r.NormalizedPosition(otherRect.min);
		Vector2 vector2 = r.NormalizedPosition(otherRect.max);
		return Rect.MinMaxRect(vector.x, vector.y, vector2.x, vector2.y);
	}

	public static Rect CenterSizeRect(Vector2 center, Vector2 size)
	{
		Vector2 vector = size * 0.5f;
		Vector2 vector2 = center - vector;
		Vector2 vector3 = center + vector;
		return Rect.MinMaxRect(vector2.x, vector2.y, vector3.x, vector3.y);
	}

	public static Rect SetHeight(this Rect rect, float height)
	{
		rect.height = height;
		return rect;
	}

	public static Rect Rotate(this Rect rect, int cardinalRotation)
	{
		Vector2 center = rect.center;
		Vector2 vector = rect.min - center;
		Vector2 vector2 = rect.max - center;
		float num = (float)cardinalRotation * (MathF.PI / 2f);
		float cos = Mathf.Cos(num);
		float sin = Mathf.Sin(0f - num);
		vector = vector.Rotate(cos, sin);
		vector2 = vector2.Rotate(cos, sin);
		Vector2 vector3 = center + Vector2.Min(vector, vector2);
		Vector2 vector4 = center + Vector2.Max(vector, vector2);
		return Rect.MinMaxRect(vector3.x, vector3.y, vector4.x, vector4.y);
	}

	public static Rect Encapsulate(this Rect r, Vector2 position)
	{
		r.min = Vector2.Min(r.min, position);
		r.max = Vector2.Max(r.max, position);
		return r;
	}

	public static Rect Encapsulate(this Rect r, Rect rect)
	{
		return r.Encapsulate(rect.min).Encapsulate(rect.max);
	}

	public static bool Contains(this Rect container, Rect rect)
	{
		if (container.min.LessThanOrEqualTo(rect.min))
		{
			return rect.max.LessThanOrEqualTo(container.max);
		}
		return false;
	}

	public static Vector2 MaximalOverlapOffset(this Rect r, Rect container)
	{
		Vector2 vector = (container.min - r.min).Clamp(Vector2.zero, new Vector2(float.MaxValue, float.MaxValue));
		Vector2 vector2 = (r.max - container.max).Clamp(Vector2.zero, new Vector2(float.MaxValue, float.MaxValue));
		return new Vector2((vector2[0] > vector[0]) ? (0f - vector2[0]) : vector[0], (vector2[1] > vector[1]) ? (0f - vector2[1]) : vector[1]);
	}

	public static Vector2 ContainmentOffset(this Rect r, Rect container)
	{
		Vector2 min = r.min;
		Vector2 max = r.max;
		Vector2 min2 = container.min;
		Vector2 max2 = container.max;
		Vector2 result = default(Vector2);
		for (int i = 0; i < 2; i++)
		{
			float value = min2[i] - min[i];
			float value2 = max2[i] - max[i];
			if (Math.Sign(value) == Math.Sign(value2))
			{
				result[i] = Math.Min(Math.Abs(value), Math.Abs(value2)) * (float)Math.Sign(value);
			}
		}
		return result;
	}

	public static Vector2 GetClosestPointTo(this Rect r, Vector2 point)
	{
		return point.Clamp(r.min, r.max);
	}

	public static Rect Multiply(this Rect r, float multiplier)
	{
		return new Rect(r.xMin * multiplier, r.yMin * multiplier, r.width * multiplier, r.height * multiplier);
	}

	public static Rect Multiply(this Rect r, Vector2 multiplier)
	{
		return new Rect(r.xMin * multiplier.x, r.yMin * multiplier.y, r.width * multiplier.x, r.height * multiplier.y);
	}

	public static Rect ToRect(this Vector3[] corners)
	{
		Rect result = default(Rect);
		result.min = corners[0].Project(AxisType.Z);
		result.max = corners[2].Project(AxisType.Z);
		return result;
	}

	public static bool IsWithinEdgeProjections(this Rect r, Vector2 v)
	{
		if (!(v.x >= r.xMin) || !(v.x <= r.xMax))
		{
			if (v.y >= r.yMin)
			{
				return v.y <= r.yMax;
			}
			return false;
		}
		return true;
	}

	public static SRect ToAlignedSRect(this Rect rect)
	{
		Vector2 size = rect.size;
		Vector2 vector = rect.min + new Vector2(0.5f, 0.5f);
		Short2 @short = new Short2(Mathf.CeilToInt(size.x) - 1, Mathf.CeilToInt(size.y) - 1);
		Short2 short2 = new Short2(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y));
		return new SRect(short2, short2 + @short);
	}

	public static SRect ToBoundingSRect(this Bounds bounds)
	{
		Vector2 vector = bounds.min.Project(AxisType.Y);
		Vector2 vector2 = bounds.max.Project(AxisType.Y);
		return new SRect(new Short2(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y)), new Short2(Mathf.CeilToInt(vector2.x), Mathf.CeilToInt(vector2.y)));
	}

	public static SRect ToAlignedSRect(this Bounds bounds)
	{
		Vector3 size = bounds.size;
		Vector3 vector = bounds.min + new Vector3(0.5f, 0f, 0.5f);
		Short2 @short = new Short2(Mathf.CeilToInt(size.x) - 1, Mathf.CeilToInt(size.z) - 1);
		Short2 short2 = new Short2(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.z));
		return new SRect(short2, short2 + @short);
	}

	public static Rect ToRect(this Bounds bounds, AxisType axisToRemove = AxisType.Y)
	{
		Vector2 vector = bounds.min.Project(axisToRemove);
		Vector2 vector2 = bounds.max.Project(axisToRemove);
		return new Rect(vector.x, vector.y, vector2.x - vector.x, vector2.y - vector.y);
	}

	public static Vector3 Transform(this Vector3 vertex, Vector3 translation, Quaternion rotation, Vector3 scale)
	{
		return (rotation * (vertex + translation)).Multiply(scale);
	}

	public static Bounds Rotate(this Bounds bounds, Quaternion rotation)
	{
		List<Vector3> source = (from p in bounds.GetCorners()
			select rotation * p).ToList();
		Bounds result = default(Bounds);
		result.SetMinMax(source.Aggregate((Vector3 a, Vector3 b) => a.Min(b)), source.Aggregate((Vector3 a, Vector3 b) => a.Max(b)));
		return result;
	}

	public static Bounds Translate(this Bounds bounds, Vector3 translation)
	{
		return new Bounds(bounds.center + translation, bounds.size);
	}

	public static Bounds Pad(this Bounds bounds, Vector3 pad)
	{
		Bounds result = default(Bounds);
		result.SetMinMax(bounds.min - pad, bounds.max + pad);
		return result;
	}

	public static Bounds ClampSize(this Bounds bounds, Vector3 minClamp)
	{
		return new Bounds(bounds.center, bounds.size.Max(minClamp));
	}

	public static Vector3 LerpedPosition(this Bounds bounds, Vector3 lerp)
	{
		return bounds.min.Lerp(bounds.max, lerp);
	}

	public static Vector3 EdgePosition(this Bounds bounds, byte orient)
	{
		Vector3 center = bounds.center;
		Vector3 extents = bounds.extents;
		return orient switch
		{
			0 => new Vector3(center.x + extents.x, center.y, center.z), 
			1 => new Vector3(center.x, center.y, center.z + extents.z), 
			2 => new Vector3(center.x - extents.x, center.y, center.z), 
			3 => new Vector3(center.x, center.y, center.z - extents.z), 
			_ => center, 
		};
	}

	public static Vector3 EdgePosition8(this Bounds bounds, byte orient8)
	{
		if ((int)orient8 % 2 == 0)
		{
			return bounds.EdgePosition((byte)((int)orient8 / 2));
		}
		Vector3 center = bounds.center;
		Vector3 extents = bounds.extents;
		return orient8 switch
		{
			1 => new Vector3(center.x + extents.x, center.y, center.z + extents.z), 
			3 => new Vector3(center.x - extents.x, center.y, center.z + extents.z), 
			5 => new Vector3(center.x - extents.x, center.y, center.z - extents.z), 
			7 => new Vector3(center.x + extents.x, center.y, center.z - extents.z), 
			_ => center, 
		};
	}

	public static float DistanceToPoint(this Bounds bounds, Vector3 point)
	{
		if (!bounds.Contains(point))
		{
			return (bounds.ClosestPoint(point) - point).magnitude;
		}
		return 0f;
	}

	public static float DistanceToPoint(this BoundingSphere sphere, Vector3 point)
	{
		return Math.Max(0f, (point - sphere.position).magnitude - sphere.radius);
	}

	public static IEnumerable<Vector3> GetCorners(this Bounds bounds)
	{
		Vector3 bMin = bounds.min;
		Vector3 bMax = bounds.max;
		yield return bMin;
		yield return new Vector3(bMin.x, bMin.y, bMax.z);
		yield return new Vector3(bMax.x, bMin.y, bMax.z);
		yield return new Vector3(bMax.x, bMin.y, bMin.z);
		yield return new Vector3(bMin.x, bMax.y, bMin.z);
		yield return new Vector3(bMin.x, bMax.y, bMax.z);
		yield return bMax;
		yield return new Vector3(bMax.x, bMax.y, bMin.z);
	}

	public static void GetVertices(this Bounds bounds, List<Vector3> vertices)
	{
		vertices.Clear();
		Vector3 min = bounds.min;
		Vector3 max = bounds.max;
		vertices.Add(min);
		vertices.Add(new Vector3(min.x, min.y, max.z));
		vertices.Add(new Vector3(max.x, min.y, max.z));
		vertices.Add(new Vector3(max.x, min.y, min.z));
		vertices.Add(new Vector3(min.x, max.y, min.z));
		vertices.Add(new Vector3(min.x, max.y, max.z));
		vertices.Add(max);
		vertices.Add(new Vector3(max.x, max.y, min.z));
	}

	public static PoolKeepItemListHandle<Vector3> GetVertices(this Bounds bounds)
	{
		PoolKeepItemListHandle<Vector3> poolKeepItemListHandle = Pools.UseKeepItemList<Vector3>();
		bounds.GetVertices(poolKeepItemListHandle);
		return poolKeepItemListHandle;
	}

	public static PoolStructArrayHandle<Vector3> GetWorldVertices(this BoxCollider boxCollider)
	{
		PoolStructArrayHandle<Vector3> poolStructArrayHandle = Pools.UseArray<Vector3>(8);
		Matrix4x4 localToWorldMatrix = boxCollider.transform.localToWorldMatrix;
		TransformData transformData = new TransformData(boxCollider.transform);
		boxCollider.transform.SetWorldToIdentity();
		Vector3 vector = boxCollider.size * 0.5f;
		Vector3 center = boxCollider.center;
		poolStructArrayHandle[0] = localToWorldMatrix.MultiplyPoint3x4(center + vector);
		poolStructArrayHandle[1] = localToWorldMatrix.MultiplyPoint3x4(center + new Vector3(0f - vector.x, vector.y, vector.z));
		poolStructArrayHandle[2] = localToWorldMatrix.MultiplyPoint3x4(center + new Vector3(vector.x, vector.y, 0f - vector.z));
		poolStructArrayHandle[3] = localToWorldMatrix.MultiplyPoint3x4(center + new Vector3(0f - vector.x, vector.y, 0f - vector.z));
		poolStructArrayHandle[4] = localToWorldMatrix.MultiplyPoint3x4(center + new Vector3(vector.x, 0f - vector.y, vector.z));
		poolStructArrayHandle[5] = localToWorldMatrix.MultiplyPoint3x4(center + new Vector3(0f - vector.x, 0f - vector.y, vector.z));
		poolStructArrayHandle[6] = localToWorldMatrix.MultiplyPoint3x4(center + new Vector3(vector.x, 0f - vector.y, 0f - vector.z));
		poolStructArrayHandle[7] = localToWorldMatrix.MultiplyPoint3x4(center - vector);
		transformData.SetValues(boxCollider.transform);
		return poolStructArrayHandle;
	}

	public static IEnumerable<Line3> GetEdges(this Bounds bounds)
	{
		List<Vector3> points = bounds.GetCorners().ToList();
		yield return new Line3(points[3], points[0]);
		yield return new Line3(points[7], points[4]);
		yield return new Line3(points[3], points[7]);
		for (int x = 0; x < 3; x++)
		{
			int topIndex = x + 4;
			yield return new Line3(points[x], points[topIndex]);
			yield return new Line3(points[x], points[x + 1]);
			yield return new Line3(points[topIndex], points[topIndex + 1]);
		}
	}

	public static IEnumerable<Vector3> GetLaticePoints(this Bounds bounds, float laticeSpacing = 0.1f, float beginExtentX = 0f, float beginExtentY = 0f, float beginExtentZ = 0f, float endExtentX = 1f, float endExtentY = 1f, float endExtentZ = 1f)
	{
		laticeSpacing = Mathf.Max(laticeSpacing, 0.001f);
		Bounds b = bounds.ExtentSectionBounds(new Vector3(beginExtentX, beginExtentY, beginExtentZ), new Vector3(endExtentX, endExtentY, endExtentZ));
		int xDivisions = Math.Max(1, Mathf.RoundToInt(b.size.x / laticeSpacing));
		int yDivisions = Math.Max(1, Mathf.RoundToInt(b.size.y / laticeSpacing));
		int zDivisions = Math.Max(1, Mathf.RoundToInt(b.size.z / laticeSpacing));
		b.size = new Vector3((xDivisions == 1) ? 0f : b.size.x, (yDivisions == 1) ? 0f : b.size.y, (zDivisions == 1) ? 0f : b.size.z);
		float xSpacing = b.size.x / (float)xDivisions;
		float ySpacing = b.size.y / (float)yDivisions;
		float zSpacing = b.size.z / (float)zDivisions;
		for (int x = 0; x < xDivisions; x++)
		{
			for (int y = 0; y < yDivisions; y++)
			{
				for (int z = 0; z < zDivisions; z++)
				{
					yield return new Vector3(b.min.x + xSpacing * (float)x, b.min.y + ySpacing * (float)y, b.min.z + zSpacing * (float)z);
				}
			}
		}
	}

	public static IEnumerable<Vector3> GetLaticePoints(this Bounds bounds, float laticeSpacing, Vector3 beginExtent, Vector3 endExtent)
	{
		return bounds.GetLaticePoints(laticeSpacing, beginExtent.x, beginExtent.y, beginExtent.z, endExtent.x, endExtent.y, endExtent.z);
	}

	public static PoolKeepItemListHandle<Plane> GetPlanes(this Bounds bounds)
	{
		PoolKeepItemListHandle<Plane> poolKeepItemListHandle = Pools.UseKeepItemList<Plane>();
		Vector3 center = bounds.center;
		Vector3 extents = bounds.extents;
		poolKeepItemListHandle.Add(new Plane(new Vector3(-1f, 0f, 0f), center + new Vector3(0f - extents.x, 0f, 0f)));
		poolKeepItemListHandle.Add(new Plane(new Vector3(0f, -1f, 0f), center + new Vector3(0f, 0f - extents.y, 0f)));
		poolKeepItemListHandle.Add(new Plane(new Vector3(0f, 0f, -1f), center + new Vector3(0f, 0f, 0f - extents.z)));
		poolKeepItemListHandle.Add(new Plane(new Vector3(1f, 0f, 0f), center + new Vector3(extents.x, 0f, 0f)));
		poolKeepItemListHandle.Add(new Plane(new Vector3(0f, 1f, 0f), center + new Vector3(0f, extents.y, 0f)));
		poolKeepItemListHandle.Add(new Plane(new Vector3(0f, 0f, 1f), center + new Vector3(0f, 0f, extents.z)));
		return poolKeepItemListHandle;
	}

	public static Bounds ExtentSectionBounds(this Bounds bounds, Vector3 startExtent, Vector3 endExtent)
	{
		Bounds result = default(Bounds);
		result.SetMinMax(bounds.min.Lerp(bounds.max, startExtent), bounds.min.Lerp(bounds.max, endExtent));
		return result;
	}

	public static float Volume(this Bounds bounds)
	{
		Vector3 size = bounds.size;
		return size.x * size.y * size.z;
	}

	public static float SurfaceArea(this Bounds bounds)
	{
		Vector3 size = bounds.size;
		float num = size.y * size.z;
		float num2 = size.x * size.z;
		float num3 = size.x * size.y;
		return num + num + num2 + num2 + num3 + num3;
	}

	public static Bounds AggregateBounds(Bounds a, Bounds b)
	{
		a.Encapsulate(b);
		return a;
	}

	public static Quaternion EulerRotate(this Quaternion q, float x, float y, float z)
	{
		Vector3 eulerAngles = q.eulerAngles;
		return Quaternion.Euler(eulerAngles.x + x, eulerAngles.y + y, eulerAngles.z + z);
	}

	public static float RootMeanSquare(this IEnumerable<float> values)
	{
		int num = 0;
		float num2 = 0f;
		foreach (float value in values)
		{
			num++;
			num2 += value * value;
		}
		return (float)Math.Sqrt(num2 / (float)num);
	}

	public static string ToStringNoScientificNotation(this float f)
	{
		return f.ToString(NO_SCIENTIFIC_NOTATION_FORMAT);
	}

	public static float Reciprocal(this float f)
	{
		return 1f / f;
	}

	public static float DotVector(this Quaternion a, Quaternion b)
	{
		return Mathf.Cos(Quaternion.Angle(a, b) * (MathF.PI / 180f));
	}

	public static float InnerProduct(this Quaternion a, Quaternion b)
	{
		return Vector4.Dot(a.ToVector4(), b.ToVector4());
	}

	public static Quaternion Inverse(this Quaternion q)
	{
		return Quaternion.Inverse(q);
	}

	public static Quaternion Inverse(this Quaternion q, bool inverse)
	{
		if (!inverse)
		{
			return q;
		}
		return Quaternion.Inverse(q);
	}

	public static Quaternion Opposite(this Quaternion q, bool opposite = true)
	{
		if (!opposite)
		{
			return q;
		}
		Quaternion quaternion = Quaternion.AngleAxis(180f, q.Right()) * q;
		return Quaternion.AngleAxis(180f, quaternion.Forward()) * quaternion;
	}

	public static Vector3 AngularVelocity(this Quaternion current, Quaternion last, float deltaTime)
	{
		Vector3 eulerAngles = (current * Quaternion.Inverse(last)).eulerAngles;
		return new Vector3(Mathf.DeltaAngle(0f, eulerAngles.x), Mathf.DeltaAngle(0f, eulerAngles.y), Mathf.DeltaAngle(0f, eulerAngles.z)) * (MathF.PI / 180f / deltaTime.InsureNonZero());
	}

	public static Vector3 EulerAngleDeltas(this Quaternion q)
	{
		Vector3 eulerAngles = q.eulerAngles;
		return new Vector3(Mathf.DeltaAngle(0f, eulerAngles.x), Mathf.DeltaAngle(0f, eulerAngles.y), Mathf.DeltaAngle(0f, eulerAngles.z));
	}

	public static Quaternion Cross(this Quaternion a, Quaternion b)
	{
		Vector3 vector = a * Vector3.forward;
		Vector3 rhs = b * Vector3.forward;
		return Quaternion.LookRotation(Vector3.Cross(vector, rhs).normalized, vector);
	}

	public static Vector2 Extrema(this List<float> values)
	{
		if (values.Count == 0)
		{
			return Vector2.zero;
		}
		float num = float.MaxValue;
		float num2 = float.MinValue;
		for (int i = 0; i < values.Count; i++)
		{
			float num3 = values[i];
			num = ((num3 < num) ? num3 : num);
			num2 = ((num3 > num2) ? num3 : num2);
		}
		return new Vector2(num, num2);
	}

	public static float Average(this List<float> values)
	{
		if (values.Count == 0)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i < values.Count; i++)
		{
			num += values[i];
		}
		return num / (float)values.Count;
	}

	public static float Variance(this List<float> values, float? preCalculatedAverage = null)
	{
		if (values.Count == 0)
		{
			return 0f;
		}
		float num = preCalculatedAverage ?? values.Average();
		float num2 = 0f;
		for (int i = 0; i < values.Count; i++)
		{
			float num3 = values[i] - num;
			num3 *= num3;
			num2 += num3;
		}
		return num2 / (float)values.Count;
	}

	public static float StandardDeviation(this List<float> values, float? preCalculatedAverage = null)
	{
		return Mathf.Sqrt(values.Variance(preCalculatedAverage));
	}

	public static float CoefficientOfVariation(this List<float> values)
	{
		float num = values.Average();
		return values.StandardDeviation(num) / num.InsureNonZero();
	}

	public static float WeightedAverage(this List<Vector2> weightValuePair)
	{
		float num = 0f;
		float num2 = 0f;
		foreach (Vector2 item in weightValuePair)
		{
			num += item.x;
			num2 += item.y * item.x;
		}
		return num2 / num.InsureNonZero();
	}

	public static Vector3 Average(this IEnumerable<Vector3> vectors)
	{
		float num = 0f;
		Vector3 zero = Vector3.zero;
		foreach (Vector3 vector in vectors)
		{
			num += 1f;
			zero += vector;
		}
		return zero / num;
	}

	public static int ToInt(this bool b, int trueValue = 1, int falseValue = 0)
	{
		if (!b)
		{
			return falseValue;
		}
		return trueValue;
	}

	public static byte ToByte(this bool b, byte trueValue = 1, byte falseValue = 0)
	{
		if (!b)
		{
			return falseValue;
		}
		return trueValue;
	}

	public static float ToFloat(this bool b, float trueValue = 1f, float falseValue = 0f)
	{
		if (!b)
		{
			return falseValue;
		}
		return trueValue;
	}

	public static double ToDouble(this bool b, double trueValue = 1.0, double falseValue = 0.0)
	{
		if (!b)
		{
			return falseValue;
		}
		return trueValue;
	}

	public static Vector3 GetTranslation(this Matrix4x4 matrix)
	{
		return matrix.GetColumn(3);
	}

	public static Matrix4x4 SetTranslation(this Matrix4x4 matrix, Vector3 translation)
	{
		matrix[0, 3] = translation.x;
		matrix[1, 3] = translation.y;
		matrix[2, 3] = translation.z;
		return matrix;
	}

	public static Matrix4x4 Translation(this Matrix4x4 matrix, Vector3 translation)
	{
		matrix[0, 3] += translation.x;
		matrix[1, 3] += translation.y;
		matrix[2, 3] += translation.z;
		return matrix;
	}

	public static Quaternion GetRotation(this Matrix4x4 matrix)
	{
		return Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
	}

	public static Vector3 GetScale(this Matrix4x4 matrix)
	{
		return new Vector3(matrix.GetColumn(0).magnitude, matrix.GetColumn(1).magnitude, matrix.GetColumn(2).magnitude);
	}

	public static Vector3 Direction(this RaycastResult ray)
	{
		return (ray.worldPosition - ray.module.eventCamera.transform.position).normalized;
	}

	public static Vector3? Raycast(this Plane plane, Ray ray)
	{
		if (!plane.Raycast(ray, out var enter))
		{
			return null;
		}
		return ray.origin + ray.direction * enter;
	}

	public static Plane Lerp(this Plane a, Plane b, float t)
	{
		return new Plane(Vector3.Slerp(a.normal, b.normal, t), Vector3.Lerp(a.normal * (0f - a.distance), b.normal * (0f - b.distance), t));
	}

	public static Plane TranslateReturn(this Plane p, Vector3 translation)
	{
		p.Translate(translation);
		return p;
	}

	public static Plane InvertNormal(this Plane p)
	{
		p.normal *= -1f;
		p.distance *= -1f;
		return p;
	}

	public static Guid Increment(this Guid guid)
	{
		byte[] array = guid.ToByteArray();
		bool flag = true;
		for (int i = 0; i < _GuidByteOrder.Length && flag; i++)
		{
			int num = _GuidByteOrder[i];
			flag = array[num]++ > array[num];
		}
		return new Guid(array);
	}

	public static Matrix4x4 AffineLerp(this Matrix4x4 start, Matrix4x4 end, float t)
	{
		return Matrix4x4.TRS(start.GetTranslation().Lerp(end.GetTranslation(), t), Quaternion.Slerp(start.rotation, end.rotation, t), start.lossyScale.Lerp(end.lossyScale, t));
	}

	public static RaycastResult SetWorldPosition(this RaycastResult raycastResult, Vector3 worldPosition)
	{
		raycastResult.worldPosition = worldPosition;
		return raycastResult;
	}

	public static Vector4 ToVector4(this Vector2 a, Vector2 b)
	{
		return new Vector4(a.x, a.y, b.x, b.y);
	}
}
