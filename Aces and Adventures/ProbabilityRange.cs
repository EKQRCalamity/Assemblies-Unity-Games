using UnityEngine;

public struct ProbabilityRange
{
	public static readonly ProbabilityRange Identity = new ProbabilityRange(1f, 0f, 0f);

	public float probability;

	public float min;

	public float max;

	public Vector2 range => this;

	public ProbabilityRange(float probability, float min, float max)
	{
		this.probability = probability;
		this.min = min;
		this.max = max;
	}

	public ProbabilityRange(float probability, Int2 range)
		: this(probability, range.x, range.y)
	{
	}

	public ProbabilityRange(float probability, Vector2 range)
		: this(probability, range.x, range.y)
	{
	}

	public static ProbabilityRange operator +(ProbabilityRange a, ProbabilityRange b)
	{
		return new ProbabilityRange(ProbabilityUtil.And(a.probability, b.probability), a.min + b.min, a.max + b.max);
	}

	public static implicit operator Vector2(ProbabilityRange range)
	{
		return new Vector2(range.min, range.max);
	}
}
