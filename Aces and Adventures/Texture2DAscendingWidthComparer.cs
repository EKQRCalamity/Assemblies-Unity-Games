using System.Collections.Generic;
using UnityEngine;

public class Texture2DAscendingWidthComparer : IComparer<Texture2D>
{
	public static readonly Texture2DAscendingWidthComparer Default = new Texture2DAscendingWidthComparer();

	public int Compare(Texture2D x, Texture2D y)
	{
		return -Texture2DDescendingWidthComparer.Default.Compare(x, y);
	}
}
