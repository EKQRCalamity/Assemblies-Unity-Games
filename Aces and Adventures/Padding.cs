using System;

[Serializable]
public struct Padding
{
	public float left;

	public float right;

	public float bottom;

	public float top;

	public Padding(float left, float right, float bottom, float top)
	{
		this.left = left;
		this.right = right;
		this.bottom = bottom;
		this.top = top;
	}
}
