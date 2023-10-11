using System;

public static class ClearMethodCache<T>
{
	public static readonly Action<T> ClearMethod;

	public static void Clear(T item)
	{
		if (ClearMethod != null)
		{
			ClearMethod(item);
		}
	}

	static ClearMethodCache()
	{
		ClearMethod = typeof(T).CacheMethod<Action<T>>("Clear");
	}
}
