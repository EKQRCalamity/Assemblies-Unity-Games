using System;
using System.Collections.Generic;

public static class ListRemoveAllNull
{
	private static class ListRemoveAllNullPredicateCache<T> where T : class
	{
		public static Predicate<T> Predicate;

		static ListRemoveAllNullPredicateCache()
		{
			Predicate = (T t) => t == null;
		}
	}

	public static void RemoveAllNull<T>(this List<T> list) where T : class
	{
		if (list.Count > 0)
		{
			list.RemoveAll(ListRemoveAllNullPredicateCache<T>.Predicate);
		}
	}
}
