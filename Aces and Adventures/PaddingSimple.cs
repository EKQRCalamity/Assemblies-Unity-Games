using System;

[Serializable]
public struct PaddingSimple
{
	public float horizontal;

	public float vertical;

	public PaddingSimple(float horizontal, float vertical)
	{
		this.horizontal = horizontal;
		this.vertical = vertical;
	}
}
