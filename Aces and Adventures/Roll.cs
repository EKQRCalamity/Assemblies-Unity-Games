using System;
using System.Collections.Generic;

public static class Roll
{
	private static int _RollManyKeepOne(Random random, int minValue, int maxValue, int rollCountModifier)
	{
		int num = random.Next(minValue, maxValue);
		int num2 = Math.Sign(rollCountModifier);
		int num3 = rollCountModifier * num2;
		while (num3-- > 0)
		{
			num = ((num2 >= 0) ? Math.Max(random.Next(minValue, maxValue), num) : Math.Min(random.Next(minValue, maxValue), num));
		}
		return num;
	}

	private static int _RollManyKeepOne(List<int> rolls, Random random, int minValue, int maxValue, int rollCountModifier)
	{
		rolls.Clear();
		int num = random.Next(minValue, maxValue);
		rolls.Add(num);
		int num2 = Math.Sign(rollCountModifier);
		int num3 = rollCountModifier * num2;
		while (num3-- > 0)
		{
			int num4 = random.Next(minValue, maxValue);
			rolls.Add(num4);
			num = ((num2 >= 0) ? Math.Max(num4, num) : Math.Min(num4, num));
		}
		return num;
	}

	public static int D(int maxValue, Random random, int rollCountModifier = 0, int minValue = 1, int valueModifier = 0)
	{
		return _RollManyKeepOne(random, minValue, ++maxValue, rollCountModifier) + valueModifier;
	}

	public static int D(int maxValue, List<int> rolls, Random random, int rollCountModifier, int minValue = 1, int valueModifier = 0)
	{
		return _RollManyKeepOne(rolls, random, minValue, ++maxValue, rollCountModifier) + valueModifier;
	}
}
