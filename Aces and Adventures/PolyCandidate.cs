using System;
using UnityEngine;

public struct PolyCandidate : IComparable<PolyCandidate>
{
	public float depth;

	public Vector2 pos;

	public float sinThetaSqrd;

	public float sinPsiSqrd;

	public PolyCandidate(float depth, Vector2 pos, float sinThetaSqrd, float sinPsiSqrd)
	{
		this.depth = depth;
		this.pos = pos;
		this.sinThetaSqrd = sinThetaSqrd;
		this.sinPsiSqrd = sinPsiSqrd;
	}

	public int CompareTo(PolyCandidate other)
	{
		return MathUtil.Compare(depth, other.depth);
	}
}
