using System.Collections.Generic;
using UnityEngine;

public static class ProbabilityUtil
{
	static ProbabilityUtil()
	{
		Pools.CreatePoolList<ProbabilityRange>();
	}

	public static float Or(float chanceA, float chanceB)
	{
		return 1f - (1f - chanceA) * (1f - chanceB);
	}

	public static double Or(double chanceA, double chanceB)
	{
		return 1.0 - (1.0 - chanceA) * (1.0 - chanceB);
	}

	public static double Or(IEnumerable<double> chances)
	{
		double num = 0.0;
		foreach (double chance in chances)
		{
			num = Or(num, chance);
		}
		return num;
	}

	public static float OrExclusive(float chanceA, float chanceB)
	{
		return chanceA + chanceB;
	}

	public static float OrRepeating(float chancePerIteration, float iterations)
	{
		return 1f - Mathf.Pow(Mathf.Clamp01(1f - chancePerIteration), Mathf.Max(0f, iterations));
	}

	public static float And(float chanceA, float chanceB)
	{
		return chanceA * chanceB;
	}

	public static double And(double chanceA, double chanceB)
	{
		return chanceA * chanceB;
	}

	public static double AndNot(double a, double notB)
	{
		return a * (1.0 - notB);
	}

	public static float LinearProbability(float minChance, float maxChance)
	{
		if (minChance >= 1f)
		{
			return 1f;
		}
		if (maxChance <= 1f)
		{
			return (minChance + maxChance) * 0.5f;
		}
		float num = maxChance - minChance;
		if (num < MathUtil.BigEpsilon)
		{
			return Mathf.Clamp01(maxChance);
		}
		float num2 = 1f - minChance;
		float num3 = num2 / num;
		return 1f - Mathf.Clamp01(0.5f * num3 * num2);
	}

	public static ProbabilityOfMeetingValueData ProbabilityOfMeetingValue(List<List<ProbabilityRange>> probabilityRangeList, float valueToMeet)
	{
		float num = 0f;
		Vector2? vector = null;
		float num2 = 0f;
		foreach (List<ProbabilityRange> singleItemPerListPermutation in CollectionUtil.GetSingleItemPerListPermutations(probabilityRangeList))
		{
			ProbabilityRange identity = ProbabilityRange.Identity;
			foreach (ProbabilityRange item in singleItemPerListPermutation)
			{
				identity += item;
			}
			vector = (vector.HasValue ? vector.Value.SignedMinMax(identity) : ((Vector2)identity));
			num2 += identity.probability * identity.range.Average();
			num = OrExclusive(num, And(identity.probability, ProbabilityOfMeetingValue(identity.min, identity.max, valueToMeet)));
		}
		return new ProbabilityOfMeetingValueData(num, vector ?? Vector2.zero, num2);
	}

	public static float ProbabilityOfMeetingValue(float min, float max, float valueToMeet)
	{
		new Vector2(min, max).MakeMinToMax().SetComponentValues(ref min, ref max);
		max += 1f;
		return Mathf.Clamp01((max - valueToMeet) / (max - min));
	}

	public static ProbabilityOfMeetingValueData ProbabilityOfMeetingValue(List<ProbabilityRanges> probabilityRanges, float valueToMeet)
	{
		using PoolListHandle<List<ProbabilityRange>> poolListHandle = Pools.UseList<List<ProbabilityRange>>();
		foreach (ProbabilityRanges probabilityRange in probabilityRanges)
		{
			List<ProbabilityRange> list = Pools.Unpool<List<ProbabilityRange>>();
			list.Add(probabilityRange.reliable);
			if (probabilityRange.critical.probability > 0f)
			{
				list.Add(probabilityRange.critical);
			}
			if (probabilityRange.miss.probability > 0f)
			{
				list.Add(probabilityRange.miss);
			}
			poolListHandle.Add(list);
		}
		return ProbabilityOfMeetingValue(poolListHandle, valueToMeet);
	}
}
