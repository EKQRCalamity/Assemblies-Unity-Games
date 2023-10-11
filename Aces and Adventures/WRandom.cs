using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using UnityEngine;

[ProtoContract(IgnoreListHandling = true)]
public class WRandom<T>
{
	[ProtoMember(1)]
	private SortedList<float, T> _list;

	private float _highestWeight
	{
		get
		{
			if (_list.Count == 0)
			{
				return 0f;
			}
			return _list.Keys[_list.Keys.Count - 1];
		}
	}

	public SortedList<float, T> underlyingList => _list;

	public int Count => _list.Count;

	public T this[int index] => _list.Values[index];

	public WRandom()
	{
		_list = new SortedList<float, T>();
	}

	public WRandom(Dictionary<T, float> weights)
	{
		_list = new SortedList<float, T>();
		if (weights == null)
		{
			return;
		}
		foreach (T key in weights.Keys)
		{
			Add(weights[key], key);
		}
	}

	public WRandom(Dictionary<T, float> weights, Func<T, bool> shouldBeAdded)
	{
		WRandom<T> wRandom = this;
		_list = new SortedList<float, T>();
		if (weights != null)
		{
			shouldBeAdded = shouldBeAdded ?? ((Func<T, bool>)((T t) => true));
			weights.Keys.Where((T value) => shouldBeAdded(value)).EffectAll(delegate(T value)
			{
				wRandom.Add(weights[value], value);
			});
		}
	}

	private WRandom(SortedList<float, T> sortedWeightList)
	{
		_list = sortedWeightList;
	}

	public T Add(float weight, T obj)
	{
		weight = Mathf.Clamp(weight, 0.001f, 1000f);
		float key = _highestWeight + weight;
		_list.Add(key, obj);
		return obj;
	}

	public WRandom<T> Concat(IEnumerable<KeyValuePair<T, float>> weightedValues)
	{
		WRandom<T> wRandom = new WRandom<T>(new SortedList<float, T>(_list));
		foreach (KeyValuePair<T, float> weightedValue in weightedValues)
		{
			wRandom.Add(weightedValue.Value, weightedValue.Key);
		}
		return wRandom;
	}

	public WRandom<T> Merge(IEnumerable<KeyValuePair<T, float>> weightedValues)
	{
		using PoolKeepItemDictionaryHandle<T, float> poolKeepItemDictionaryHandle = Pools.UseKeepItemDictionary<T, float>();
		Dictionary<T, float> value = poolKeepItemDictionaryHandle.value;
		foreach (KeyValuePair<float, T> item in _list)
		{
			if (!value.ContainsKey(item.Value))
			{
				value.Add(item.Value, item.Key);
			}
			else
			{
				value[item.Value] += item.Key;
			}
		}
		foreach (KeyValuePair<T, float> weightedValue in weightedValues)
		{
			if (!value.ContainsKey(weightedValue.Key))
			{
				value.Add(weightedValue.Key, weightedValue.Value);
			}
			else
			{
				value[weightedValue.Key] += weightedValue.Value;
			}
		}
		return new WRandom<T>(value);
	}

	public WRandom<T> Add(IDictionary<T, float> weightedValues)
	{
		foreach (T key in weightedValues.Keys)
		{
			Add(weightedValues[key], key);
		}
		return this;
	}

	public void Remove(T obj)
	{
		_RemoveAt(_list.IndexOfValue(obj));
	}

	private void _RemoveAt(int index)
	{
		if (index >= 0)
		{
			float num = ((index > 0) ? (_list.Keys[index] - _list.Keys[index - 1]) : _list.Keys[index]);
			_list.RemoveAt(index);
			for (int i = index; i < _list.Keys.Count; i++)
			{
				float key = _list.Keys[i] - num;
				T value = _list[_list.Keys[i]];
				_list.RemoveAt(i);
				_list.Add(key, value);
			}
		}
	}

	public void RemoveAll(Func<T, bool> remove)
	{
		List<T> list = new List<T>();
		foreach (float key in _list.Keys)
		{
			if (remove(_list[key]))
			{
				list.Add(_list[key]);
			}
		}
		RemoveAll(list);
	}

	public void RemoveAll(List<T> objects)
	{
		for (int i = 0; i < objects.Count; i++)
		{
			Remove(objects[i]);
		}
	}

	public void Clear()
	{
		_list.Clear();
	}

	public void RemoveValuesWithMinimumWeightSoLongAsNonMinimumWeightExists()
	{
		List<int> list = new List<int>();
		bool flag = false;
		int num = 0;
		foreach (var item in EnumeratePairInOrder())
		{
			if (item.a == 0.001f)
			{
				list.Add(num);
			}
			else
			{
				flag = true;
			}
			num++;
		}
		if (flag)
		{
			for (int num2 = list.Count - 1; num2 >= 0; num2--)
			{
				_RemoveAt(list[num2]);
			}
		}
	}

	public T Random(float zeroToOne)
	{
		if (_list.Keys.Count == 0)
		{
			return default(T);
		}
		if (_list.Keys.Count == 1)
		{
			return _list.Values[0];
		}
		float num = _highestWeight * Mathf.Clamp01(zeroToOne);
		int num2 = 0;
		int num3 = _list.Keys.Count / 2;
		int num4 = _list.Keys.Count;
		while (true)
		{
			if (num > _list.Keys[num3])
			{
				num2 = num3;
				num3 = (num4 - num2) / 2 + num2;
				continue;
			}
			if (num3 == 0 || num >= _list.Keys[num3 - 1])
			{
				break;
			}
			num4 = num3;
			num3 = (num4 - num2) / 2 + num2;
		}
		return _list[_list.Keys[num3]];
	}

	public IEnumerable<T> EnumerateInRandomOrder(System.Random random)
	{
		WRandom<T> wRandom = new WRandom<T>(new SortedList<float, T>(_list));
		while (wRandom.Count > 0)
		{
			T item = wRandom.Random(random.NextFloat());
			yield return item;
			wRandom.Remove(item);
		}
	}

	public IEnumerable<T> EnumerateInOrder()
	{
		return _list.Values;
	}

	public IEnumerable<(float a, T b)> EnumeratePairInOrder()
	{
		float num = 0f;
		foreach (float weight in _list.Keys)
		{
			yield return (weight - num, _list[weight]);
			num = weight;
		}
	}

	public IEnumerable<T> ShuffleDistinct(IEnumerable<T> i, System.Random random)
	{
		return i.WeightedShuffleDistinct(this, random);
	}

	public WRandom<T> RemapWeightsToNewWRandom(Func<T, float, float> weightMapping)
	{
		WRandom<T> wRandom = new WRandom<T>();
		float num = 0f;
		foreach (float key in _list.Keys)
		{
			float arg = key - num;
			T val = _list[key];
			wRandom.Add(weightMapping(val, arg), val);
			num = key;
		}
		return wRandom;
	}

	public string ToStringProbabilityDistribution()
	{
		return (from pair in EnumeratePairInOrder()
			select ((pair.a / _highestWeight * 100f).ToString("#.##") + "%", pair.b.ToString())).ToStringSmart();
	}
}
