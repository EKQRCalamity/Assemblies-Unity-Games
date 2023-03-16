using System;

namespace GCFreeUtils;

public class GCFreePredicateList<T>
{
	private const string ERR_BUFFER_TOO_SMALL = "[GCFreeActionList] Current buffer too small. Consider increasing the initial size or set as auto resizeable.";

	private const string LOG_RESIZING = "[GCFreeActionList] Resizing buffer. Maybe you want to increase the initial size.";

	private Predicate<T>[] actionList;

	private bool autoResizeable;

	public int Count { get; private set; }

	public GCFreePredicateList(int size)
		: this(size, autoResizeable: true)
	{
	}

	public GCFreePredicateList(int size, bool autoResizeable)
	{
		actionList = new Predicate<T>[size];
		this.autoResizeable = autoResizeable;
		Count = 0;
	}

	public void Add(Predicate<T> action)
	{
		if (Count == actionList.Length)
		{
			if (!autoResizeable)
			{
				Debug.LogError("[GCFreeActionList] Current buffer too small. Consider increasing the initial size or set as auto resizeable.");
				return;
			}
			Predicate<T>[] destinationArray = new Predicate<T>[actionList.Length * 2];
			Array.Copy(actionList, destinationArray, actionList.Length);
			actionList = destinationArray;
		}
		actionList[Count] = action;
		Count++;
	}

	public void Remove(Predicate<T> action)
	{
		if (Count <= 0)
		{
			return;
		}
		for (int i = 0; i < Count; i++)
		{
			if (actionList[i] == action)
			{
				if (Count > 1)
				{
					actionList[i] = actionList[Count - 1];
				}
				else
				{
					actionList[i] = null;
				}
				Count--;
				break;
			}
		}
	}

	public bool CallAnyTrue(T parameter)
	{
		for (int i = 0; i < Count; i++)
		{
			try
			{
				if (actionList[i] != null && actionList[i](parameter))
				{
					return true;
				}
			}
			catch (Exception message)
			{
				Debug.LogError(message);
			}
		}
		return false;
	}
}
