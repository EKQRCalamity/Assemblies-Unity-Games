using System;

public static class OnUnpoolMethodCache<T>
{
	public static readonly Action<T> OnUnpoolMethod;

	public static T OnUnpool(T item)
	{
		if (OnUnpoolMethod != null)
		{
			OnUnpoolMethod(item);
		}
		return item;
	}

	static OnUnpoolMethodCache()
	{
		OnUnpoolMethod = typeof(T).CacheMethod<Action<T>>("OnUnpool");
	}
}
