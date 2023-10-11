using System;
using System.Collections.Generic;

public static class PoolExtensions
{
	public static void SafeClear<T>(this ICollection<T> collection)
	{
		collection?.Clear();
	}

	public static void RepoolItems<T>(this List<T> list, bool repoolList = false) where T : class
	{
		Pools.RepoolItems(list, repoolList);
	}

	public static void RepoolItems<T>(this T[] array, bool repoolArray = false) where T : class
	{
		Pools.RepoolItems(array, repoolArray);
	}

	public static void RepoolItems<T>(this HashSet<T> hashSet, bool repoolHashSet = false) where T : class
	{
		Pools.RepoolItems(hashSet, repoolHashSet);
	}

	public static void RepoolItems<T>(this Stack<T> stack, bool repoolStack = false) where T : class
	{
		Pools.RepoolItems(stack, repoolStack);
	}

	public static void RepoolItems<T>(this Queue<T> queue, bool repoolQueue = false) where T : class
	{
		Pools.RepoolItems(queue, repoolQueue);
	}

	public static void RepoolItems<K, V>(this Dictionary<K, V> dictionary, bool repoolDictionary = false) where K : class where V : class
	{
		Pools.RepoolDictionaryItems(dictionary, repoolDictionary);
	}

	public static void RepoolKeys<K, V>(this Dictionary<K, V> dictionary, bool repoolDictionary = false) where K : class where V : struct
	{
		Pools.RepoolDictionaryKeys(dictionary, repoolDictionary);
	}

	public static void RepoolValues<K, V>(this Dictionary<K, V> dictionary, bool repoolDictionary = false) where K : struct where V : class
	{
		Pools.RepoolDictionaryValues(dictionary, repoolDictionary);
	}

	public static void RepoolValuesAndClear<K, V>(this Dictionary<K, V> dictionary) where V : class
	{
		DictionaryValueEnumerator<K, V>.Enumerator enumerator = dictionary.EnumerateValues().GetEnumerator();
		while (enumerator.MoveNext())
		{
			Pools.Repool(enumerator.Current);
		}
		dictionary.Clear();
	}

	public static void RepoolSubCollectionItems<K, V, E>(this Dictionary<K, V> dictionary) where V : class, ICollection<E> where E : class
	{
		Dictionary<K, V>.Enumerator enumerator = dictionary.GetEnumerator();
		while (enumerator.MoveNext())
		{
			Pools.RepoolItems<V, E>(enumerator.Current.Value);
		}
	}

	public static void RepoolStructCollections<K, V, E>(this Dictionary<K, V> dictionary, bool repoolDictionary = false) where K : struct where V : class, ICollection<E> where E : struct
	{
		DictionaryValueEnumerator<K, V>.Enumerator enumerator = dictionary.EnumerateValues().GetEnumerator();
		while (enumerator.MoveNext())
		{
			Pools.Repool(enumerator.Current);
		}
		if (repoolDictionary)
		{
			Pools.Repool(dictionary);
		}
		else
		{
			dictionary.Clear();
		}
	}

	public static void RemoveAndRepool<K, V>(this Dictionary<K, V> dictionary, K keyToRemove) where V : class
	{
		Pools.Repool(dictionary[keyToRemove]);
		dictionary.Remove(keyToRemove);
	}

	public static bool IfNotNullDispose(this IDisposable disposable)
	{
		bool num = disposable != null;
		if (num)
		{
			disposable.Dispose();
		}
		return num;
	}
}
