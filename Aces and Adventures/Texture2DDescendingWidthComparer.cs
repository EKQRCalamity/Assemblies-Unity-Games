using System.Collections.Generic;
using UnityEngine;

public class Texture2DDescendingWidthComparer : IComparer<Texture2D>
{
	public static readonly Texture2DDescendingWidthComparer Default = new Texture2DDescendingWidthComparer();

	public int Compare(Texture2D x, Texture2D y)
	{
		int num = y.width - x.width;
		if (num != 0)
		{
			return num;
		}
		int num2 = y.height - x.height;
		if (num2 != 0)
		{
			return num2;
		}
		return x.GetInstanceID() - y.GetInstanceID();
	}
}
