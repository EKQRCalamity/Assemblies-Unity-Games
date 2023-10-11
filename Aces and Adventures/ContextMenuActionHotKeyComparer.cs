using System.Collections.Generic;

public class ContextMenuActionHotKeyComparer : IComparer<ContextMenuAction>
{
	public static ContextMenuActionHotKeyComparer Default = new ContextMenuActionHotKeyComparer();

	public int Compare(ContextMenuAction a, ContextMenuAction b)
	{
		if (a == null)
		{
			if (b != null)
			{
				return 1;
			}
			return 0;
		}
		if (b == null)
		{
			return -1;
		}
		return HotKeyComparer.Default.Compare(a.hotKey, b.hotKey);
	}
}
