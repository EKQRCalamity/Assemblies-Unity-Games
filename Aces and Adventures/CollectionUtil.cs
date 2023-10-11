using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public static class CollectionUtil
{
	public static bool AddUnique<T>(this ICollection<T> collection, T value)
	{
		if (collection.Contains(value))
		{
			return false;
		}
		collection.Add(value);
		return true;
	}

	public static bool AddUnique<T>(this List<T> list, T value)
	{
		bool flag = false;
		for (int i = 0; i < list.Count; i++)
		{
			if (EqualityComparer<T>.Default.Equals(list[i], value))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			list.Add(value);
		}
		return !flag;
	}

	public static void ReplaceLast<T>(this List<T> list, T value)
	{
		if (list.Count > 0)
		{
			list[list.Count - 1] = value;
		}
		else
		{
			list.Add(value);
		}
	}

	public static bool Toggle<T>(this List<T> list, T value)
	{
		int num = list.IndexOf(value);
		if (num >= 0)
		{
			list.RemoveAt(num);
		}
		else
		{
			list.Add(value);
		}
		return num < 0;
	}

	public static int IndexOf<T>(this IList<T> list, T value, IEqualityComparer<T> equalityComparer)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (equalityComparer.Equals(value, list[i]))
			{
				return i;
			}
		}
		return -1;
	}

	public static int IndexOfByReference(this IList list, object value)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] == value)
			{
				return i;
			}
		}
		return -1;
	}

	public static int IndexOfStartFromLast<T>(this IList<T> list, T value, IEqualityComparer<T> equalityComparer = null)
	{
		if (equalityComparer == null)
		{
			equalityComparer = EqualityComparer<T>.Default;
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (equalityComparer.Equals(value, list[num]))
			{
				return num;
			}
		}
		return -1;
	}

	public static int LastIndexOf<T>(this IList<T> list, Func<T, bool> valid)
	{
		if (list == null)
		{
			return -1;
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (valid(list[num]))
			{
				return num;
			}
		}
		return -1;
	}

	public static bool RemoveFromEnd<T>(this List<T> list, T item)
	{
		int num = list.LastIndexOf(item);
		if (num < 0)
		{
			return false;
		}
		list.RemoveAt(num);
		return true;
	}

	public static IEnumerable<T> Interweave<T>(this IEnumerable<IEnumerable<T>> enumerables)
	{
		using PoolKeepItemListHandle<IEnumerator<T>> enumerators = Pools.UseKeepItemList<IEnumerator<T>>();
		foreach (IEnumerable<T> enumerable in enumerables)
		{
			enumerators.Add(enumerable.GetEnumerator());
		}
		while (enumerators.Count > 0)
		{
			foreach (IEnumerator<T> item in enumerators.value.EnumerateSafe())
			{
				if (item.MoveNext())
				{
					yield return item.Current;
				}
				else
				{
					enumerators.Remove(item);
				}
			}
		}
	}

	public static void AddMany<T>(this ICollection<T> collection, IEnumerable<T> itemsToAdd)
	{
		foreach (T item in itemsToAdd)
		{
			collection.Add(item);
		}
	}

	public static List<T> RemoveAndInstertIndex<T>(this List<T> list, int indexToRemove, int insertIndex)
	{
		T item = list[indexToRemove];
		list.RemoveAt(indexToRemove);
		list.Insert(insertIndex, item);
		return list;
	}

	public static bool AddOrRemove<T>(this HashSet<T> hash, T item, bool add)
	{
		if (!add)
		{
			return hash.Remove(item);
		}
		return hash.Add(item);
	}

	public static bool AddTrue<T>(this HashSet<T> hash, T item)
	{
		hash.Add(item);
		return true;
	}

	public static IEnumerable<IEnumerable<T>> ToPages<T>(this IEnumerable<T> items, int pageSize)
	{
		using PoolKeepItemListHandle<T> itemList = Pools.UseKeepItemList(items);
		int maxPageNumber = itemList.value.GetMaxPageNumber(pageSize);
		for (int x = 1; x <= maxPageNumber; x++)
		{
			yield return itemList.value.GetPagedResults(x, pageSize);
		}
	}

	public static IEnumerable<T> GetPagedResults<T>(this List<T> results, int pageNumber, int pageSize)
	{
		for (int x = (pageNumber - 1) * pageSize; x < Mathf.Min(results.Count, pageNumber * pageSize); x++)
		{
			yield return results[x];
		}
	}

	public static int GetMaxPageNumber(this ICollection collection, int pageSize)
	{
		return Math.Max(0, collection.Count - 1) / pageSize + 1;
	}

	public static int GetMaxPageNumber(this uint count, int pageSize)
	{
		if (count != 0)
		{
			return (int)((long)(count - 1) / (long)pageSize + 1);
		}
		return 1;
	}

	public static void InsureValidPageNumber(this ICollection collection, ref int pageNumber, int pageSize)
	{
		pageNumber = Mathf.Clamp(pageNumber, 1, collection.GetMaxPageNumber(pageSize));
	}

	public static V AddPairIfUnique<K, V>(this IDictionary<K, V> dictionary, K key, Func<V> value)
	{
		if (dictionary.ContainsKey(key))
		{
			return default(V);
		}
		V val = value();
		dictionary.Add(key, val);
		return val;
	}

	public static V GetOrAdd<K, V>(this IDictionary<K, V> dictionary, K key, Func<V> value)
	{
		if (dictionary.ContainsKey(key))
		{
			return dictionary[key];
		}
		V val = value();
		dictionary.Add(key, val);
		return val;
	}

	public static V GetIfExists<K, V>(this IDictionary<K, V> dictionary, K key)
	{
		if (!dictionary.ContainsKey(key))
		{
			return default(V);
		}
		return dictionary[key];
	}

	public static K GetKeyOrDefault<K, V>(this IDictionary<K, V> dictionary, K key)
	{
		if (!dictionary.ContainsKey(key))
		{
			return default(K);
		}
		return key;
	}

	public static List<T> Clone<T>(this List<T> list)
	{
		return new List<T>(list);
	}

	public static void ClearAndCopyTo<T>(this ICollection<T> collection, ICollection<T> filterTo)
	{
		filterTo.Clear();
		foreach (T item in collection)
		{
			filterTo.Add(item);
		}
	}

	public static void SetItemsToDefault<T>(this IList<T> list)
	{
		for (int i = 0; i < list.Count; i++)
		{
			list[i] = default(T);
		}
	}

	public static bool Contains<T>(this T[] array, T value)
	{
		for (int i = 0; i < array.Length; i++)
		{
			if (ReflectionUtil.SafeEquals(array[i], value))
			{
				return true;
			}
		}
		return false;
	}

	public static T[] Resize<T>(ref T[] array, int newSize)
	{
		Array.Resize(ref array, newSize);
		return array;
	}

	public static bool Contains<T>(this IEnumerable<T> i, Func<T, bool> valid)
	{
		foreach (T item in i)
		{
			if (valid(item))
			{
				return true;
			}
		}
		return false;
	}

	public static bool ContainsAll<T>(this HashSet<T> hash, IEnumerable<T> enumerable)
	{
		foreach (T item in enumerable)
		{
			if (!hash.Contains(item))
			{
				return false;
			}
		}
		return true;
	}

	public static void EffectAll<T>(this IEnumerable<T> i, Action<T> effect)
	{
		foreach (T item in i)
		{
			effect(item);
		}
	}

	public static void EffectAll<T>(this List<T> list, Action<T> effect)
	{
		for (int i = 0; i < list.Count; i++)
		{
			effect(list[i]);
		}
	}

	public static IEnumerable<T> Process<T>(this IEnumerable<T> items, Action<T> process)
	{
		foreach (T item in items)
		{
			process(item);
			yield return item;
		}
	}

	public static void RemoveFromCenter<T>(this List<T> list, int numberOfItemsToRemove, bool pinFirstItem = true, bool pinLastItem = true)
	{
		numberOfItemsToRemove = Mathf.Min(numberOfItemsToRemove, list.Count - pinFirstItem.ToInt() - pinLastItem.ToInt());
		if (numberOfItemsToRemove > 0)
		{
			list.RemoveRange(Mathf.RoundToInt((float)(list.Count - numberOfItemsToRemove) * 0.5f), numberOfItemsToRemove);
		}
	}

	public static void RemoveLast<T>(this List<T> list)
	{
		if (!list.IsNullOrEmpty())
		{
			list.RemoveAt(list.Count - 1);
		}
	}

	public static IEnumerable<T> WeightedShuffleDistinct<T>(this IEnumerable<T> i, WRandom<T> weights, System.Random random, IEqualityComparer<T> equalityComparer = null)
	{
		HashSet<T> hash = i.ToHash(equalityComparer);
		if (hash.Count == 1)
		{
			yield return i.First();
			yield break;
		}
		foreach (T item in weights.EnumerateInRandomOrder(random))
		{
			if (hash.Contains(item))
			{
				yield return item;
			}
		}
	}

	public static void Swap<T>(this IList<T> list, int i, int j)
	{
		T value = list[i];
		list[i] = list[j];
		list[j] = value;
	}

	public static string ToStringSmart<T>(this IEnumerable<T> i, string delimiter = ", ")
	{
		if (i == null)
		{
			return "";
		}
		string text = "";
		foreach (T item in i)
		{
			string text2 = text;
			T val = item;
			text = text2 + val?.ToString() + delimiter;
		}
		return text.Substring(0, Math.Max(0, text.Length - delimiter.Length));
	}

	public static string ToStringSmart(this IEnumerable i, string delimiter = ", ")
	{
		if (i == null)
		{
			return "";
		}
		string text = "";
		foreach (object item in i)
		{
			text = text + item?.ToString() + delimiter;
		}
		return text.Substring(0, Math.Max(0, text.Length - delimiter.Length));
	}

	public static string ToStringSmart<T>(this IEnumerable<T> i, Func<T, string> toString, string delimiter = ", ", bool visibleEntriesOnly = false)
	{
		string text = "";
		foreach (T item in i)
		{
			string text2 = toString(item);
			if (!visibleEntriesOnly || text2.HasVisibleCharacter())
			{
				text = text + text2 + delimiter;
			}
		}
		if (text.Length <= 0)
		{
			return text;
		}
		return text.Substring(0, Math.Max(0, text.Length - delimiter.Length));
	}

	public static HashSet<T> ToHash<T>(this IEnumerable<T> i, IEqualityComparer<T> comparer = null)
	{
		return new HashSet<T>(i, comparer);
	}

	public static PoolKeepItemListHandle<KeyValuePair<K, int>> GetDeltas<K>(this Dictionary<K, int> dictionary, Dictionary<K, int> previousDictionary)
	{
		PoolKeepItemListHandle<KeyValuePair<K, int>> poolKeepItemListHandle = Pools.UseKeepItemList<KeyValuePair<K, int>>();
		foreach (KeyValuePair<K, int> item in dictionary)
		{
			poolKeepItemListHandle.Add(previousDictionary.ContainsKey(item.Key) ? new KeyValuePair<K, int>(item.Key, item.Value - previousDictionary[item.Key]) : item);
		}
		foreach (KeyValuePair<K, int> item2 in previousDictionary)
		{
			if (!dictionary.ContainsKey(item2.Key))
			{
				poolKeepItemListHandle.Add(new KeyValuePair<K, int>(item2.Key, -item2.Value));
			}
		}
		return poolKeepItemListHandle;
	}

	public static PoolKeepItemListHandle<KeyValuePair<K, V>> GetDeltas<K, V>(this Dictionary<K, V> dictionary, Dictionary<K, V> previousDictionary)
	{
		PoolKeepItemListHandle<KeyValuePair<K, V>> poolKeepItemListHandle = Pools.UseKeepItemList<KeyValuePair<K, V>>();
		foreach (KeyValuePair<K, V> item in dictionary)
		{
			if (!previousDictionary.ContainsKey(item.Key) || !EqualityComparer<V>.Default.Equals(item.Value, previousDictionary[item.Key]))
			{
				poolKeepItemListHandle.Add(item);
			}
		}
		return poolKeepItemListHandle;
	}

	public static Stack<T> ToStack<T>(this IEnumerable<T> i)
	{
		return new Stack<T>(i);
	}

	public static Stack<T> ReverseStack<T>(this Stack<T> stack)
	{
		using PoolKeepItemListHandle<T> poolKeepItemListHandle = Pools.UseKeepItemList<T>();
		while (stack.Count > 0)
		{
			poolKeepItemListHandle.value.Add(stack.Pop());
		}
		foreach (T item in poolKeepItemListHandle.value)
		{
			stack.Push(item);
		}
		return stack;
	}

	public static IEnumerable<T> ReverseIf<T>(this IEnumerable<T> items, bool reverse)
	{
		if (!reverse)
		{
			return items;
		}
		return items.Reverse();
	}

	public static LinkedList<T> ToLinkedList<T>(this IEnumerable<T> i)
	{
		LinkedList<T> linkedList = new LinkedList<T>();
		foreach (T item in i)
		{
			linkedList.AddLast(item);
		}
		return linkedList;
	}

	public static void RemoveNull<T>(this IList<T> list)
	{
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num] == null)
			{
				list.RemoveAt(num);
			}
		}
	}

	public static Dictionary<K, V> RemoveKeys<K, V>(this Dictionary<K, V> dictionary, Func<K, bool> removeKey)
	{
		foreach (K item in dictionary.EnumerateKeysSafe())
		{
			if (removeKey(item))
			{
				dictionary.Remove(item);
			}
		}
		return dictionary;
	}

	public static Dictionary<K, V> RemoveValues<K, V>(this Dictionary<K, V> dictionary, Func<V, bool> removeValue)
	{
		foreach (KeyValuePair<K, V> item in dictionary.EnumeratePairsSafe())
		{
			if (removeValue(item.Value))
			{
				dictionary.Remove(item.Key);
			}
		}
		return dictionary;
	}

	public static T MinBy<T>(this IEnumerable<T> i, Func<T, float> toValue)
	{
		T result = default(T);
		float num = float.MaxValue;
		foreach (T item in i)
		{
			float num2 = toValue(item);
			if (num2 < num)
			{
				result = item;
				num = num2;
			}
		}
		return result;
	}

	public static T MaxBy<T>(this IEnumerable<T> i, Func<T, float> toValue)
	{
		T result = default(T);
		float num = float.MinValue;
		foreach (T item in i)
		{
			float num2 = toValue(item);
			if (num2 > num)
			{
				result = item;
				num = num2;
			}
		}
		return result;
	}

	public static T MinOrDefault<T>(this IEnumerable<T> items, Comparison<T> comparison, T defaultValue = default(T))
	{
		bool flag = false;
		T val = default(T);
		foreach (T item in items)
		{
			if (!flag && (flag = true))
			{
				val = item;
			}
			else if (comparison(item, val) < 0)
			{
				val = item;
			}
		}
		if (!flag)
		{
			return defaultValue;
		}
		return val;
	}

	public static double? MaxOrDefault<T>(this IEnumerable<T> items, Func<T, double?> getValue, double? startingValue = null, double? defaultValue = null)
	{
		double? num = startingValue;
		foreach (T item in items)
		{
			double? num2 = getValue(item);
			if (num2.HasValue)
			{
				double valueOrDefault = num2.GetValueOrDefault();
				if (!num.HasValue || valueOrDefault > num)
				{
					num = valueOrDefault;
				}
			}
		}
		return num ?? defaultValue;
	}

	public static K RandomKey<K>(this Dictionary<K, float> weights, System.Random random)
	{
		using PoolWRandomDHandle<K> poolWRandomDHandle = Pools.UseWRandomD<K>();
		return poolWRandomDHandle.value.Add(weights).Random(random.NextDouble());
	}

	public static K RandomKey<K>(this Dictionary<K, double> weights, System.Random random)
	{
		using PoolWRandomDHandle<K> poolWRandomDHandle = Pools.UseWRandomD<K>();
		return poolWRandomDHandle.value.Add(weights).Random(random.NextDouble());
	}

	public static double TotalWeight<K>(this Dictionary<K, double> weights)
	{
		double num = 0.0;
		foreach (double value in weights.Values)
		{
			num += value;
		}
		return num;
	}

	public static Dictionary<K, double> NormalizeWeights<K>(this Dictionary<K, double> weights, double newTotalWeight = 1.0)
	{
		double num = newTotalWeight / weights.TotalWeight().InsureNonZero();
		foreach (K item in weights.EnumerateKeysSafe())
		{
			weights[item] *= num;
		}
		return weights;
	}

	public static Dictionary<K, V> AddKeys<K, V>(this Dictionary<K, V> dict, IEnumerable<K> keys, Func<V> valueConstructor = null)
	{
		foreach (K key in keys)
		{
			if (!dict.ContainsKey(key))
			{
				dict.Add(key, (valueConstructor != null) ? valueConstructor() : default(V));
			}
		}
		return dict;
	}

	public static bool TryAdd<K, V>(this IDictionary<K, V> dict, K key, V value)
	{
		if (dict.ContainsKey(key))
		{
			return false;
		}
		dict[key] = value;
		return true;
	}

	public static Dictionary<K, V> SortKeysAlphabetically<K, V>(this Dictionary<K, V> dictionary)
	{
		return dictionary.OrderBy(delegate(KeyValuePair<K, V> pair)
		{
			KeyValuePair<K, V> keyValuePair = pair;
			return keyValuePair.Key.ToString();
		}).ToDictionary((KeyValuePair<K, V> p) => p.Key, (KeyValuePair<K, V> p) => p.Value);
	}

	public static void RemoveDictionaryKeys<T>(this IEnumerable enumerable, Func<T, bool> removeKey, Action actionIfAtleastOneKeyRemoved = null, Action<string> actionOnNestedKeyRemoved = null, object parentKey = null, List<object> parentKeys = null)
	{
		if (actionOnNestedKeyRemoved != null && parentKey != null)
		{
			parentKeys = ((parentKeys == null) ? new List<object>() : parentKeys.ToList());
			parentKeys.Add(parentKey);
		}
		if (enumerable is IDictionary)
		{
			IDictionary dictionary = (IDictionary)enumerable;
			List<object> list = new List<object>();
			foreach (object key in dictionary.Keys)
			{
				if (key is T && removeKey((T)key))
				{
					list.Add(key);
				}
				else if (key is IEnumerable)
				{
					(key as IEnumerable).RemoveDictionaryKeys(removeKey, actionIfAtleastOneKeyRemoved, actionOnNestedKeyRemoved, key, parentKeys);
				}
				if (dictionary[key] is IEnumerable)
				{
					(dictionary[key] as IEnumerable).RemoveDictionaryKeys(removeKey, actionIfAtleastOneKeyRemoved, actionOnNestedKeyRemoved, key, parentKeys);
				}
			}
			if (actionIfAtleastOneKeyRemoved != null && list.Count > 0)
			{
				actionIfAtleastOneKeyRemoved();
			}
			list.EffectAll(delegate(object key)
			{
				dictionary.Remove(key);
				if (actionOnNestedKeyRemoved != null && parentKeys.Count > 0)
				{
					actionOnNestedKeyRemoved(parentKeys.ToStringSmart());
				}
			});
			return;
		}
		foreach (IEnumerable item in enumerable.OfType<IEnumerable>())
		{
			item.RemoveDictionaryKeys(removeKey, actionIfAtleastOneKeyRemoved, actionOnNestedKeyRemoved, null, parentKeys);
		}
	}

	public static IEnumerable<O> SelectValid<T, O>(this IEnumerable<T> i, Func<T, O> conversion, O invalidConversionValue = default(O))
	{
		return from output in i.Select(conversion)
			where output != null && !output.Equals(invalidConversionValue)
			select output;
	}

	public static T FirstOrDefault<T>(this IEnumerable<T> i, Func<T, bool> valid, T valueToReturnIfDefault)
	{
		T val = i.FirstOrDefault(valid);
		if (val == null || val.Equals(default(T)))
		{
			return valueToReturnIfDefault;
		}
		return val;
	}

	public static T LastRef<T>(this List<T> list) where T : class
	{
		if (list.Count <= 0)
		{
			return null;
		}
		return list[list.Count - 1];
	}

	public static T? FirstValue<T>(this List<T> list) where T : struct
	{
		if (list.Count <= 0)
		{
			return null;
		}
		return list[0];
	}

	public static T? LastValue<T>(this List<T> list) where T : struct
	{
		if (list.Count <= 0)
		{
			return null;
		}
		return list[list.Count - 1];
	}

	public static bool Atleast<T>(this IEnumerable<T> i, uint atleastCount)
	{
		if (i is ICollection)
		{
			return (i as ICollection).Count >= atleastCount;
		}
		int num = 0;
		foreach (T item in i)
		{
			_ = item;
			num++;
			if (num == atleastCount)
			{
				return true;
			}
		}
		return false;
	}

	public static IEnumerable<Couple<T, T>> Pairings<T>(this IEnumerable<T> a, IEnumerable<T> b)
	{
		foreach (T itemA in a)
		{
			foreach (T item in b)
			{
				yield return new Couple<T, T>(itemA, item);
			}
		}
	}

	public static void IntersectNonEmpty<T>(this HashSet<T> collection, IEnumerable<T> intersectWith)
	{
		if (collection.Count == 0)
		{
			collection.AddMany(intersectWith);
		}
		else if (intersectWith.Any())
		{
			collection.IntersectWith(intersectWith);
		}
	}

	public static List<T> FillToCapacityWithDefault<T>(this List<T> list, int? capacity = null)
	{
		int num = (capacity ?? list.Capacity) - list.Count;
		for (int i = 0; i < num; i++)
		{
			list.Add(default(T));
		}
		return list;
	}

	public static List<T> CopyFrom<T>(this List<T> list, List<T> copyFrom, int? count = null)
	{
		if (copyFrom == null)
		{
			return list;
		}
		count = (count.HasValue ? Math.Min(count.Value, copyFrom.Count) : copyFrom.Count);
		for (int i = 0; i < count; i++)
		{
			list.Add(copyFrom[i]);
		}
		return list;
	}

	public static List<T> CopyFrom<T>(this List<T> list, HashSet<T> copyFrom)
	{
		foreach (T item in copyFrom)
		{
			list.Add(item);
		}
		return list;
	}

	public static HashSet<T> CopyFrom<T>(this HashSet<T> hashSet, HashSet<T> copyFrom, bool clearExisting = false, bool clearCopyFrom = false)
	{
		if (clearExisting)
		{
			hashSet.Clear();
		}
		foreach (T item in copyFrom)
		{
			hashSet.Add(item);
		}
		if (clearCopyFrom)
		{
			copyFrom.Clear();
		}
		return hashSet;
	}

	public static HashSet<T> CopyFrom<T>(this HashSet<T> hashSet, List<T> copyFrom, int? count = null, bool clearExisting = false, bool clearCopyFrom = false)
	{
		if (clearExisting)
		{
			hashSet.Clear();
		}
		int num = count ?? copyFrom.Count;
		for (int i = 0; i < num; i++)
		{
			hashSet.Add(copyFrom[i]);
		}
		if (clearCopyFrom)
		{
			copyFrom.Clear();
		}
		return hashSet;
	}

	public static Dictionary<K, V> CopyFrom<K, V>(this Dictionary<K, V> dictionary, Dictionary<K, V> copyFrom)
	{
		if (copyFrom != null)
		{
			foreach (KeyValuePair<K, V> item in copyFrom)
			{
				dictionary.Add(item.Key, item.Value);
			}
			return dictionary;
		}
		return dictionary;
	}

	public static Dictionary<K, V> Clone<K, V>(this Dictionary<K, V> dictionary)
	{
		return new Dictionary<K, V>().CopyFrom(dictionary);
	}

	public static List<T> ClearAndCopyFrom<T>(this List<T> list, List<T> copyFrom, int? count = null)
	{
		list.Clear();
		count = count ?? copyFrom.Count;
		for (int i = 0; i < count; i++)
		{
			list.Add(copyFrom[i]);
		}
		return list;
	}

	public static List<T> ClearAndCopyFrom<T>(this List<T> list, IEnumerable<T> copyFrom)
	{
		list.Clear();
		foreach (T item in copyFrom)
		{
			list.Add(item);
		}
		return list;
	}

	public static Dictionary<K, V> ClearAndCopyFrom<K, V>(this Dictionary<K, V> dictionary, Dictionary<K, V> copyFrom)
	{
		dictionary.Clear();
		foreach (KeyValuePair<K, V> item in copyFrom)
		{
			dictionary.Add(item.Key, item.Value);
		}
		return dictionary;
	}

	public static void StableSort<T>(this IList<T> list) where T : IComparable<T>
	{
		for (int i = 1; i < list.Count; i++)
		{
			T val = list[i];
			int num = i;
			while (num > 0 && list[num - 1].CompareTo(val) > 0)
			{
				list[num] = list[--num];
			}
			list[num] = val;
		}
	}

	public static void StableSort<T>(this IList<T> list, IComparer<T> comparer)
	{
		for (int i = 1; i < list.Count; i++)
		{
			T val = list[i];
			int num = i;
			while (num > 0 && comparer.Compare(list[num - 1], val) > 0)
			{
				list[num] = list[--num];
			}
			list[num] = val;
		}
	}

	public static void StableSort<T>(this IList<T> list, Comparison<T> comparison)
	{
		for (int i = 1; i < list.Count; i++)
		{
			T val = list[i];
			int num = i;
			while (num > 0 && comparison(list[num - 1], val) > 0)
			{
				list[num] = list[--num];
			}
			list[num] = val;
		}
	}

	public static bool IsSorted<T>(this IList<T> list) where T : IComparable<T>
	{
		for (int i = 1; i < list.Count; i++)
		{
			if (list[i - 1].CompareTo(list[i]) > 0)
			{
				return false;
			}
		}
		return true;
	}

	public static bool SequenceEqual(this byte[] a, byte[] b)
	{
		if (a.Length != b.Length)
		{
			return false;
		}
		for (int i = 0; i < a.Length; i++)
		{
			if (a[i] != b[i])
			{
				return false;
			}
		}
		return true;
	}

	public static bool SequenceEqualSafe<T>(this T[] a, T[] b, IEqualityComparer<T> equalityComparer = null)
	{
		if (a == null)
		{
			return b == null;
		}
		if (b == null)
		{
			return false;
		}
		if (a.Length != b.Length)
		{
			return false;
		}
		IEqualityComparer<T> equalityComparer2 = equalityComparer ?? EqualityComparer<T>.Default;
		for (int i = 0; i < a.Length; i++)
		{
			if (!equalityComparer2.Equals(a[i], b[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static bool SequenceEqual<T>(this List<T> a, List<T> b, IEqualityComparer<T> equalityComparer = null)
	{
		if (a.IsNullOrEmpty())
		{
			return b.IsNullOrEmpty();
		}
		if (b.IsNullOrEmpty())
		{
			return false;
		}
		if (a.Count != b.Count)
		{
			return false;
		}
		if (equalityComparer == null)
		{
			equalityComparer = EqualityComparer<T>.Default;
		}
		for (int i = 0; i < a.Count; i++)
		{
			if (!equalityComparer.Equals(a[i], b[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static int AddSorted<T>(this List<T> list, T item, IComparer<T> comparer = null)
	{
		if (comparer == null)
		{
			comparer = Comparer<T>.Default;
		}
		if (list.Count == 0)
		{
			list.Add(item);
			return 0;
		}
		if (comparer.Compare(list[list.Count - 1], item) <= 0)
		{
			list.Add(item);
			return list.Count - 1;
		}
		if (comparer.Compare(list[0], item) >= 0)
		{
			list.Insert(0, item);
			return 0;
		}
		int num = list.BinarySearch(item, comparer);
		int num2 = ((num >= 0) ? num : (~num));
		list.Insert(num2, item);
		return num2;
	}

	public static int AddSortedStable<T>(this List<T> list, T item, IComparer<T> comparer = null)
	{
		if (comparer == null)
		{
			comparer = Comparer<T>.Default;
		}
		if (list.Count == 0)
		{
			list.Add(item);
			return 0;
		}
		if (comparer.Compare(list[list.Count - 1], item) <= 0)
		{
			list.Add(item);
			return list.Count - 1;
		}
		int num = list.BinarySearch(item, comparer);
		int num2 = ((num >= 0) ? num : (~num));
		for (int i = num2; i < list.Count && comparer.Compare(list[i], item) == 0; i++)
		{
			num2++;
		}
		list.Insert(num2, item);
		return num2;
	}

	public static int AddSorted<T>(this List<T> list, T item, bool sorted, IComparer<T> comparer = null)
	{
		if (sorted)
		{
			return list.AddSorted(item, comparer);
		}
		list.Add(item);
		return list.Count - 1;
	}

	public static int? AddSortedUnique<T>(this List<T> list, T item, IComparer<T> comparer = null)
	{
		int num = list.BinarySearch(item, comparer);
		if (num >= 0)
		{
			return null;
		}
		int num2 = ~num;
		list.Insert(num2, item);
		return num2;
	}

	public static int? BinarySearchNext<T>(this List<T> list, T item, IComparer<T> comparer = null)
	{
		int num = list.BinarySearch(item, comparer);
		if (num >= 0)
		{
			if (num < list.Count - 1)
			{
				return num + 1;
			}
		}
		else
		{
			int num2 = ~num;
			if (num2 < list.Count)
			{
				return num2;
			}
		}
		return null;
	}

	public static int? BinarySearchPrevious<T>(this List<T> list, T item, IComparer<T> comparer = null)
	{
		int num = list.BinarySearch(item, comparer);
		if (num >= 0)
		{
			if (num > 0)
			{
				return num - 1;
			}
		}
		else
		{
			int num2 = ~num;
			if (num2 > 0)
			{
				return num2 - 1;
			}
		}
		return null;
	}

	public static List<T> AddReturnList<T>(this List<T> list, T item)
	{
		list.Add(item);
		return list;
	}

	public static List<T> AddReturnListIf<T>(this List<T> list, T item, bool add)
	{
		if (add)
		{
			list.Add(item);
		}
		return list;
	}

	public static int SafeCount<T>(this List<T> list, int countOnNull = -1)
	{
		return list?.Count ?? countOnNull;
	}

	public static int SafeIndex(this IList list, int index, int indexOnNull = -1)
	{
		if (list == null)
		{
			return indexOnNull;
		}
		return Math.Max(0, Math.Min(index, list.Count - 1));
	}

	public static PoolStructListHandle<SimpleListDifferenceData<T>> GetSimpleDifferences<T>(this List<T> oldList, List<T> newList, IEqualityComparer<T> equalityComparer)
	{
		equalityComparer = equalityComparer ?? EqualityComparer<T>.Default;
		PoolStructListHandle<SimpleListDifferenceData<T>> poolStructListHandle = Pools.UseStructList<SimpleListDifferenceData<T>>();
		int num = Math.Min(oldList.Count, newList.Count);
		for (int i = 0; i < num; i++)
		{
			if (!equalityComparer.Equals(oldList[i], newList[i]))
			{
				poolStructListHandle.Add(new SimpleListDifferenceData<T>(newList[i], SimpleListDifferenceType.Replace, i));
			}
		}
		for (int j = num; j < oldList.Count; j++)
		{
			poolStructListHandle.Add(new SimpleListDifferenceData<T>(oldList[j], SimpleListDifferenceType.Remove, j));
		}
		for (int k = num; k < newList.Count; k++)
		{
			poolStructListHandle.Add(new SimpleListDifferenceData<T>(newList[k], SimpleListDifferenceType.Add, k));
		}
		return poolStructListHandle;
	}

	public static PoolStructListHandle<int> IndicesOf<T>(this List<T> list, T value)
	{
		PoolStructListHandle<int> poolStructListHandle = Pools.UseStructList<int>();
		int num = -1;
		while ((num = list.IndexOf(value, num + 1)) >= 0)
		{
			poolStructListHandle.Add(num);
		}
		return poolStructListHandle;
	}

	public static bool ContainsAtIndexOtherThan<T>(this List<T> list, T value, int index)
	{
		using PoolStructListHandle<int> poolStructListHandle = list.IndicesOf(value);
		return poolStructListHandle.Count > 1 || (poolStructListHandle.Count != 0 && poolStructListHandle[0] != index);
	}

	public static PoolKeepItemHashSetHandle<T> SymmetricExcept<T>(this IEnumerable<T> a, IEnumerable<T> b)
	{
		PoolKeepItemHashSetHandle<T> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet(a);
		poolKeepItemHashSetHandle.value.SymmetricExceptWith(b);
		return poolKeepItemHashSetHandle;
	}

	public static float AverageSafe<T>(this IEnumerable<T> enumerable, Func<T, float> getValue, float? fallbackValue = null)
	{
		float num = 0f;
		int num2 = 0;
		foreach (T item in enumerable)
		{
			num += getValue(item);
			num2++;
		}
		if (num2 <= 0)
		{
			return fallbackValue.GetValueOrDefault();
		}
		return num / (float)num2;
	}

	public static IEnumerable<T> OrderByComparer<T>(this IEnumerable<T> enumerable, IComparer<T> comparer)
	{
		if (comparer == null)
		{
			return enumerable;
		}
		return enumerable.OrderBy((T v) => v, comparer);
	}

	public static IEnumerable<T> DistinctBy<T, K>(this IEnumerable<T> enumerable, Func<T, K> keySelector)
	{
		using PoolKeepItemHashSetHandle<K> hash = Pools.UseKeepItemHashSet<K>();
		foreach (T item in enumerable)
		{
			if (hash.Add(keySelector(item)))
			{
				yield return item;
			}
		}
	}

	public static bool SetDifferent<K, V>(this Dictionary<K, V> dictionary, K key, V value, IEqualityComparer<V> valueComparer = null)
	{
		if (dictionary.ContainsKey(key) && (valueComparer ?? EqualityComparer<V>.Default).Equals(dictionary[key], value))
		{
			return false;
		}
		dictionary[key] = value;
		return true;
	}

	public static T FirstOrDefault<T>(this Stack<T> stack)
	{
		if (stack == null || stack.Count <= 0)
		{
			return default(T);
		}
		return stack.Peek();
	}

	public static V GetValueOrDefault<K, V>(this Dictionary<K, V> dictionary, K key)
	{
		dictionary.TryGetValue(key, out var value);
		return value;
	}

	public static V GetValueOrDefault<K, V>(this ConcurrentDictionary<K, V> dictionary, K key)
	{
		dictionary.TryGetValue(key, out var value);
		return value;
	}

	public static T GetValueOrDefault<T>(this List<T> list, int index)
	{
		if (list == null || index < 0 || index >= list.Count)
		{
			return default(T);
		}
		return list[index];
	}

	public static IEnumerable<IEnumerable<T>> Permutations<T>(this IEnumerable<T> items)
	{
		using PoolKeepItemListHandle<T> listHandle = Pools.UseKeepItemList(items);
		List<T> list = listHandle.value;
		int numberOfItems = list.Count;
		int[] stackIndices = new int[numberOfItems];
		yield return list;
		int index = 0;
		while (index < numberOfItems)
		{
			if (stackIndices[index] < index)
			{
				list.Swap((index % 2 != 0) ? stackIndices[index] : 0, index);
				yield return list;
				stackIndices[index]++;
				index = 0;
			}
			else
			{
				stackIndices[index] = 0;
				int num = index + 1;
				index = num;
			}
		}
	}

	public static IEnumerable<IEnumerable<T>> SingleItemPerListPermutations<T>(this List<PoolKeepItemListHandle<T>> sequences)
	{
		int numLoops = sequences.Count;
		if (numLoops == 0)
		{
			yield break;
		}
		using PoolKeepItemListHandle<int> loopIndexHandle = Pools.UseKeepItemList<int>();
		List<int> loopIndex = loopIndexHandle.value;
		for (int i = 0; i < numLoops; i++)
		{
			loopIndex.Add(0);
		}
		using PoolKeepItemListHandle<T> listHandle = Pools.UseKeepItemList<T>();
		List<T> list = listHandle.value;
		while (true)
		{
			for (int j = 0; j < numLoops; j++)
			{
				list.Add(sequences[j][loopIndex[j]]);
			}
			yield return list;
			list.Clear();
			int num = numLoops - 1;
			while (++loopIndex[num] >= sequences[num].Count)
			{
				loopIndex[num--] = 0;
				if (num < 0)
				{
					yield break;
				}
			}
		}
	}

	public static IList<T> Shuffle<T>(this IList<T> list, System.Random random)
	{
		int count = list.Count;
		while (count > 1)
		{
			int i = random.Next(count--);
			list.Swap(i, count);
		}
		return list;
	}

	public static IEnumerable<T> Shuffled<T>(this IEnumerable<T> items, System.Random random)
	{
		PoolKeepItemListHandle<T> poolKeepItemListHandle = Pools.UseKeepItemList(items);
		poolKeepItemListHandle.value.Shuffle(random);
		foreach (T item in poolKeepItemListHandle)
		{
			yield return item;
		}
	}

	public static IEnumerable<T> Shuffled<T>(this IEnumerable<T> items, System.Random random, bool shuffle)
	{
		if (!shuffle)
		{
			return items;
		}
		return items.Shuffled(random);
	}

	public static IEnumerable<(T, T)> SimplePairings<T>(this IEnumerable<T> items)
	{
		int index = 0;
		(T, T) tuple = default((T, T));
		foreach (T item in items)
		{
			if (index++ == 0)
			{
				tuple.Item1 = item;
				continue;
			}
			index = 0;
			tuple.Item2 = item;
			yield return tuple;
		}
	}

	public static bool AtLeast<T>(this IEnumerable<T> items, int count)
	{
		foreach (T item in items)
		{
			_ = item;
			if (--count <= 0)
			{
				return true;
			}
		}
		return count <= 0;
	}

	public static T AddReturn<T>(this List<T> list, T item)
	{
		list.Add(item);
		return item;
	}

	public static T AddReturn<T>(this HashSet<T> hashSet, T item)
	{
		hashSet.Add(item);
		return item;
	}

	public static bool None<T>(this IEnumerable<T> items)
	{
		return !items.Any();
	}

	public static bool None<T>(this IEnumerable<T> items, Func<T, bool> validItem)
	{
		return !items.Any(validItem);
	}

	public static bool NoneValidButSomeExist<T>(this IEnumerable<T> items, Func<T, bool> validItem)
	{
		int num = 0;
		foreach (T item in items)
		{
			if (++num >= 0 && validItem(item))
			{
				return false;
			}
		}
		return num > 0;
	}

	public static IEnumerable<T> TakeWhileAndNext<T>(this IEnumerable<T> items, Func<T, bool> takeWhile)
	{
		foreach (T item in items)
		{
			yield return item;
			if (!takeWhile(item))
			{
				yield break;
			}
		}
	}

	public static T EnqueueAndReturn<T>(this Queue<T> queue, T item)
	{
		queue.Enqueue(item);
		return item;
	}

	public static IEnumerable<T> Concat<T>(this IEnumerable<T> items, T itemToConcat)
	{
		foreach (T item in items)
		{
			yield return item;
		}
		yield return itemToConcat;
	}

	public static IEnumerable<IEnumerable<T>> RateLimit<T>(this IEnumerable<T> items, float countPerSecond)
	{
		float delay = 1f / Math.Max(0.001f, countPerSecond);
		float time = Time.time;
		PoolKeepItemListHandle<T> poolKeepItemListHandle = Pools.UseKeepItemList<T>();
		foreach (T item in items)
		{
			while (Time.time - time < 0f)
			{
				yield return poolKeepItemListHandle?.AsEnumerable() ?? Enumerable.Empty<T>();
				poolKeepItemListHandle = null;
			}
			if (poolKeepItemListHandle == null)
			{
				poolKeepItemListHandle = Pools.UseKeepItemList<T>();
			}
			poolKeepItemListHandle.Add(item);
			time += delay;
		}
		if ((bool)poolKeepItemListHandle)
		{
			yield return poolKeepItemListHandle?.AsEnumerable();
		}
	}

	public static IEnumerable<IEnumerable<T>> Overtime<T>(this IEnumerable<T> items, float duration, int count)
	{
		return items.RateLimit((float)Math.Max(1, count - 1) / Math.Max(0.001f, duration));
	}

	public static T? TryDequeue<T>(this Queue<T> queue) where T : struct
	{
		if (queue == null || queue.Count <= 0)
		{
			return null;
		}
		return queue.Dequeue();
	}

	public static int IndexOf<T>(this IEnumerable<T> items, T value)
	{
		if (items is IList<T> list)
		{
			return list.IndexOf(value);
		}
		int num = 0;
		foreach (T item in items)
		{
			if (EqualityComparer<T>.Default.Equals(item, value))
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public static IEnumerable<T> TakeOrderFrom<T>(this IEnumerable<T> items, IEnumerable<T> takeOrderFrom)
	{
		using PoolKeepItemHashSetHandle<T> itemHash = Pools.UseKeepItemHashSet(items);
		foreach (T item in takeOrderFrom)
		{
			if (itemHash.Contains(item))
			{
				yield return item;
			}
		}
	}

	public static IEnumerable<T> ConcatIfNotNull<T>(this IEnumerable<T> items, T itemToConcat)
	{
		foreach (T item in items)
		{
			yield return item;
		}
		if (itemToConcat != null)
		{
			yield return itemToConcat;
		}
	}

	public static IEnumerable<T> ConcatIfNotNull<T>(this IEnumerable<T> items, IEnumerable<T> itemsToConcat)
	{
		foreach (T item in items)
		{
			yield return item;
		}
		if (itemsToConcat == null)
		{
			yield break;
		}
		foreach (T item2 in itemsToConcat)
		{
			yield return item2;
		}
	}

	public static T GetLastValue<T>(this Array array)
	{
		return (T)array.GetValue(array.Length - 1);
	}

	public static IEnumerable<T> TakeWhile<T>(T initialValue, Func<T, T> getNext, bool includeInitialValue = false, Func<T, bool> valid = null)
	{
		valid = valid ?? ((Func<T, bool>)((T t) => !ReflectionUtil.IsNullOrDefault(t)));
		if (includeInitialValue && valid(initialValue))
		{
			yield return initialValue;
		}
		while (true)
		{
			Func<T, bool> func = valid;
			T arg;
			initialValue = (arg = getNext(initialValue));
			if (func(arg))
			{
				yield return initialValue;
				continue;
			}
			break;
		}
	}

	public static bool IsNullOrEmpty<T>(this T[] array)
	{
		if (array != null)
		{
			return array.Length == 0;
		}
		return true;
	}

	public static bool IsNullOrEmpty<T>(this ICollection<T> collection)
	{
		if (collection != null)
		{
			return collection.Count == 0;
		}
		return true;
	}

	public static PoolListHandle<List<T>> GetSingleItemPerListPermutations<T>(List<List<T>> sequences)
	{
		PoolListHandle<List<T>> poolListHandle = Pools.UseList<List<T>>();
		int count = sequences.Count;
		if (count == 0)
		{
			return poolListHandle;
		}
		using PoolStructListHandle<int> poolStructListHandle = Pools.UseStructList<int>(count);
		bool flag = false;
		while (!flag)
		{
			List<T> list = Pools.TryUnpoolList<T>();
			for (int i = 0; i < count; i++)
			{
				list.Add(sequences[i][poolStructListHandle[i]]);
			}
			poolListHandle.Add(list);
			int num = count - 1;
			while (++poolStructListHandle[num] >= sequences[num].Count)
			{
				poolStructListHandle[num--] = 0;
				if (num < 0)
				{
					flag = true;
					break;
				}
			}
		}
		return poolListHandle;
	}

	public static IEnumerable<T> Repeat<T>(Func<T> generateItem, int count)
	{
		for (int x = 0; x < count; x++)
		{
			yield return generateItem();
		}
	}

	public static ListEnumerator<T> Enumerate<T>(this List<T> list)
	{
		return new ListEnumerator<T>(list);
	}

	public static PoolKeepItemListHandle<T> EnumerateSafe<T>(this List<T> list)
	{
		return Pools.UseKeepItemList(list);
	}

	public static PoolKeepItemListHandle<T> EnumerateSafe<T>(this HashSet<T> hashSet)
	{
		return Pools.UseKeepItemList(hashSet);
	}

	public static DictionaryPairEnumerator<K, V> EnumeratePairs<K, V>(this Dictionary<K, V> dictionary)
	{
		return new DictionaryPairEnumerator<K, V>(dictionary);
	}

	public static PoolKeepItemListHandle<KeyValuePair<K, V>> EnumeratePairsSafe<K, V>(this Dictionary<K, V> dictionary)
	{
		PoolKeepItemListHandle<KeyValuePair<K, V>> poolKeepItemListHandle = Pools.UseKeepItemList<KeyValuePair<K, V>>();
		List<KeyValuePair<K, V>> value = poolKeepItemListHandle.value;
		DictionaryPairEnumerator<K, V>.Enumerator enumerator = dictionary.EnumeratePairs().GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<K, V> current = enumerator.Current;
			value.Add(current);
		}
		return poolKeepItemListHandle;
	}

	public static DictionaryKeyEnumerator<K, V> EnumerateKeys<K, V>(this Dictionary<K, V> dictionary)
	{
		return new DictionaryKeyEnumerator<K, V>(dictionary);
	}

	public static PoolKeepItemListHandle<K> EnumerateKeysSafe<K, V>(this Dictionary<K, V> dictionary)
	{
		PoolKeepItemListHandle<K> poolKeepItemListHandle = Pools.UseKeepItemList<K>();
		List<K> value = poolKeepItemListHandle.value;
		DictionaryKeyEnumerator<K, V>.Enumerator enumerator = dictionary.EnumerateKeys().GetEnumerator();
		while (enumerator.MoveNext())
		{
			K current = enumerator.Current;
			value.Add(current);
		}
		return poolKeepItemListHandle;
	}

	public static DictionaryValueEnumerator<K, V> EnumerateValues<K, V>(this Dictionary<K, V> dictionary)
	{
		return new DictionaryValueEnumerator<K, V>(dictionary);
	}

	public static PoolKeepItemListHandle<V> EnumerateValuesSafe<K, V>(this Dictionary<K, V> dictionary)
	{
		PoolKeepItemListHandle<V> poolKeepItemListHandle = Pools.UseKeepItemList<V>();
		DictionaryValueEnumerator<K, V>.Enumerator enumerator = dictionary.EnumerateValues().GetEnumerator();
		while (enumerator.MoveNext())
		{
			V current = enumerator.Current;
			poolKeepItemListHandle.Add(current);
		}
		return poolKeepItemListHandle;
	}

	public static async IAsyncEnumerable<T> Concat<T>(this IAsyncEnumerable<T> a, IAsyncEnumerable<T> b)
	{
		await foreach (T item in a)
		{
			yield return item;
		}
		await foreach (T item2 in b)
		{
			yield return item2;
		}
	}

	public static async IAsyncEnumerable<T> Concat<T>(this IAsyncEnumerable<T> a, IEnumerable<T> b)
	{
		await foreach (T item in a)
		{
			yield return item;
		}
		foreach (T item2 in b)
		{
			yield return item2;
		}
	}

	public static async IAsyncEnumerable<T> Concat<T>(this IEnumerable<T> a, IAsyncEnumerable<T> b)
	{
		foreach (T item in a)
		{
			yield return item;
		}
		await foreach (T item2 in b)
		{
			yield return item2;
		}
	}

	public static async IAsyncEnumerable<T> Concat<T>(this IAsyncEnumerable<T> a, T b)
	{
		await foreach (T item in a)
		{
			yield return item;
		}
		yield return b;
	}

	public static async IAsyncEnumerable<T> Concat<T>(this IAsyncEnumerable<T> a, Task<T> b)
	{
		await foreach (T item in a)
		{
			yield return item;
		}
		yield return await b;
	}

	public static async IAsyncEnumerable<T> WhereAsync<T>(this IEnumerable<T> items, Func<T, Task<bool>> predicate)
	{
		foreach (T item in items)
		{
			if (await predicate(item))
			{
				yield return item;
			}
		}
	}

	public static async IAsyncEnumerable<T> WhereAsyncParallel<T>(this IEnumerable<T> items, Func<T, Task<bool>> predicate)
	{
		using PoolKeepItemDictionaryHandle<T, Task<bool>> taskMap = Pools.UseKeepItemDictionary<T, Task<bool>>();
		foreach (T item in items)
		{
			taskMap[item] = predicate(item);
		}
		foreach (KeyValuePair<T, Task<bool>> task in taskMap)
		{
			if (await task.Value)
			{
				yield return task.Key;
			}
		}
	}

	public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> a)
	{
		List<T> list = new List<T>();
		await foreach (T item in a)
		{
			list.Add(item);
		}
		return list;
	}

	public static IEnumerable<T> AsEnumerable<T>(this IAsyncEnumerable<T> asyncEnumerable)
	{
		IAsyncEnumerator<T> asyncEnumerator = asyncEnumerable.GetAsyncEnumerator();
		while (asyncEnumerator.MoveNextAsync().Result)
		{
			yield return asyncEnumerator.Current;
		}
	}

	public static int Increment<K>(this Dictionary<K, int> dictionary, K key, int incrementAmount = 1)
	{
		return dictionary[key] = dictionary.GetValueOrDefault(key) + incrementAmount;
	}

	public static int IncrementedCount<K>(this Dictionary<K, int> dictionary, K key)
	{
		if (!dictionary.ContainsKey(key))
		{
			return 0;
		}
		return dictionary[key];
	}

	public static uint GetUniqueId<V>(this Dictionary<uint, V> dictionary, ref uint lastAddedId, uint? nullId = null)
	{
		do
		{
			lastAddedId++;
		}
		while (dictionary.ContainsKey(lastAddedId) || lastAddedId == nullId);
		return lastAddedId;
	}

	public static string GetUniqueKey(this IDictionary dictionary, string desiredKey)
	{
		if (!dictionary.Contains(desiredKey))
		{
			return desiredKey;
		}
		int num = 0;
		string text;
		do
		{
			int num2 = ++num;
			text = desiredKey + " " + num2;
		}
		while (dictionary.Contains(text));
		return text;
	}

	public static V GetValueSafe<K, V>(this Dictionary<K, V> dictionary, K key, V fallback = default(V))
	{
		if (dictionary == null || key == null || !dictionary.ContainsKey(key))
		{
			return fallback;
		}
		return dictionary[key];
	}

	public static IEnumerable<NodeData> GetNodesDeep(this IEnumerable<NodeData> nodes, bool stopAtCannotBeFlattened = false)
	{
		foreach (NodeData node in nodes)
		{
			yield return node;
			if (!node.ShouldEvaluateSubGraph(stopAtCannotBeFlattened))
			{
				continue;
			}
			foreach (NodeData item in node.subGraph.GetNodes().AsEnumerable().GetNodesDeep(stopAtCannotBeFlattened))
			{
				yield return item;
			}
		}
	}

	public static PoolKeepItemHashSetHandle<NodeData> Flatten(this Dictionary<NodeData, PoolKeepItemHashSetHandle<NodeData>> branches)
	{
		PoolKeepItemHashSetHandle<NodeData> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet<NodeData>();
		foreach (KeyValuePair<NodeData, PoolKeepItemHashSetHandle<NodeData>> branch in branches)
		{
			foreach (NodeData item in branch.Value.value)
			{
				poolKeepItemHashSetHandle.Add(item);
			}
			poolKeepItemHashSetHandle.Add(branch.Key);
		}
		return poolKeepItemHashSetHandle;
	}

	public static bool All<T>(this List<T> conditions, ActionContext context) where T : AAction.Condition
	{
		if (!conditions.IsNullOrEmpty())
		{
			foreach (T condition in conditions)
			{
				if (!condition.IsValid(context))
				{
					return false;
				}
			}
		}
		return true;
	}

	public static bool All(this List<AAction.Condition.AAbility> conditions, ActionContext context, bool allowEncounterCondition = false)
	{
		if (conditions.All<AAction.Condition.AAbility>(context))
		{
			if (!allowEncounterCondition)
			{
				if (context.target is Ability ability)
				{
					return !ability.isEncounterCondition;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public static bool All(this List<AAction.Filter> filters, ActionContext context, AAction action)
	{
		if (!filters.IsNullOrEmpty())
		{
			foreach (AAction.Filter filter in filters)
			{
				if (!filter.IsValid(context, action))
				{
					return false;
				}
			}
		}
		return true;
	}

	public static AbilityPreventedBy? GetAbilityPreventedBy<T>(this List<T> conditions, ActionContext context) where T : AAction.Condition
	{
		if (!conditions.IsNullOrEmpty())
		{
			foreach (T condition in conditions)
			{
				AbilityPreventedBy? abilityPreventedBy = condition.GetAbilityPreventedBy(context);
				if (abilityPreventedBy.HasValue)
				{
					return abilityPreventedBy.GetValueOrDefault();
				}
			}
		}
		return null;
	}

	public static void Register<T>(this List<T> conditions, ActionContext context, Action<ATarget> onDirty = null) where T : AAction.Condition
	{
		if (conditions.IsNullOrEmpty())
		{
			return;
		}
		foreach (T condition in conditions)
		{
			T current = condition;
			current.Register(context);
			if (onDirty != null)
			{
				current.onDirty = (Action<ATarget>)Delegate.Combine(current.onDirty, onDirty);
			}
		}
	}

	public static void Unregister<T>(this List<T> conditions, ActionContext context, Action<ATarget> onDirty = null) where T : AAction.Condition
	{
		if (conditions.IsNullOrEmpty())
		{
			return;
		}
		foreach (T condition in conditions)
		{
			T current = condition;
			current.Unregister(context);
			if (onDirty != null)
			{
				current.onDirty = (Action<ATarget>)Delegate.Remove(current.onDirty, onDirty);
			}
		}
	}

	public static IEnumerable<AbilityKeyword> GetKeywords<T>(this List<T> conditions) where T : AAction.Condition
	{
		if (conditions.IsNullOrEmpty())
		{
			yield break;
		}
		foreach (T condition in conditions)
		{
			foreach (AbilityKeyword keyword in condition.GetKeywords())
			{
				yield return keyword;
			}
		}
	}

	public static float GetExperienceValue(this IEnumerable<DataRef<AbilityData>> traits)
	{
		float num = 0f;
		foreach (DataRef<AbilityData> trait in traits)
		{
			num += trait.data.experience;
		}
		return num;
	}
}
