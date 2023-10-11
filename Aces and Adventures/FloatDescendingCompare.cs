using System.Collections.Generic;

public class FloatDescendingCompare : IComparer<float>
{
	public static FloatDescendingCompare Default = new FloatDescendingCompare();

	public int Compare(float x, float y)
	{
		if (!(y > x))
		{
			return -1;
		}
		return 1;
	}
}
