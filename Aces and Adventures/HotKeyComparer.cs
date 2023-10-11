using System.Collections.Generic;

public class HotKeyComparer : IComparer<HotKey?>
{
	public static HotKeyComparer Default = new HotKeyComparer();

	public int Compare(HotKey? a, HotKey? b)
	{
		if (!a.HasValue)
		{
			if (b.HasValue)
			{
				return 1;
			}
			return 0;
		}
		if (!b.HasValue)
		{
			return -1;
		}
		return a.Value.CompareTo(b.Value);
	}
}
