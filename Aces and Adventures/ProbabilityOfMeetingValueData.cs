using UnityEngine;

public struct ProbabilityOfMeetingValueData
{
	public float probability;

	public Vector2 range;

	public float average;

	public ProbabilityOfMeetingValueData(float probability, Vector2 range, float average)
	{
		this.probability = probability;
		this.range = range;
		this.average = average;
	}

	public static implicit operator float(ProbabilityOfMeetingValueData data)
	{
		return data.probability;
	}

	public static implicit operator Vector2(ProbabilityOfMeetingValueData data)
	{
		return data.range;
	}

	public static implicit operator Int2(ProbabilityOfMeetingValueData data)
	{
		return new Int2(data);
	}
}
