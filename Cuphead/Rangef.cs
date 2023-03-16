using System;

[Serializable]
public struct Rangef
{
	public float minimum;

	public float maximum;

	public Rangef(float minimum, float maximum)
	{
		this.minimum = minimum;
		this.maximum = maximum;
	}

	public bool ContainsInclusive(float checkValue)
	{
		return MathUtilities.BetweenInclusive(checkValue, minimum, maximum);
	}

	public bool ContainsExclusive(float checkValue)
	{
		return MathUtilities.BetweenExclusive(checkValue, minimum, maximum);
	}

	public bool ContainsInclusiveExclusive(float checkValue)
	{
		return MathUtilities.BetweenInclusiveExclusive(checkValue, minimum, maximum);
	}

	public bool ContainsExclusiveInclusive(float checkValue)
	{
		return MathUtilities.BetweenExclusiveInclusive(checkValue, minimum, maximum);
	}

	public override string ToString()
	{
		return $"({minimum}, {maximum})";
	}
}
