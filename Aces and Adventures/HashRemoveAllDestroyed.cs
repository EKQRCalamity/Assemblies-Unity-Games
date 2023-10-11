using System;
using System.Collections.Generic;
using UnityEngine;

public static class HashRemoveAllDestroyed
{
	private static class RemoveAllDestroyedPredicateCache<T> where T : UnityEngine.Object
	{
		public static Predicate<T> Predicate;

		static RemoveAllDestroyedPredicateCache()
		{
			Predicate = (T t) => !(UnityEngine.Object)t;
		}
	}

	public static int RemoveAllDestroyed<T>(this HashSet<T> hash) where T : UnityEngine.Object
	{
		if (hash.IsNullOrEmpty())
		{
			return 0;
		}
		return hash.RemoveWhere(RemoveAllDestroyedPredicateCache<T>.Predicate);
	}
}
