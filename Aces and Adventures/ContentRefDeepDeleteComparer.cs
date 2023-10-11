using System;
using System.Collections.Generic;

public class ContentRefDeepDeleteComparer : IComparer<ContentRef>
{
	public static readonly ContentRefDeepDeleteComparer Default = new ContentRefDeepDeleteComparer();

	public int Compare(ContentRef a, ContentRef b)
	{
		int num = a.sortIndex - b.sortIndex;
		if (num != 0)
		{
			return num;
		}
		int num2 = StringComparer.OrdinalIgnoreCase.Compare(a.specificTypeFriendly, b.specificTypeFriendly);
		if (num2 != 0)
		{
			return num2;
		}
		return StringComparer.OrdinalIgnoreCase.Compare(a.name, b.name);
	}
}
