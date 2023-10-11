using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

[ProtoContract(IgnoreListHandling = true)]
public class WRandomD<T>
{
	public const double MinWeight = 1E-08;

	public const double MaxWeight = 10000.0;

	[ProtoMember(1)]
	private SortedList<double, T> _list;

	private double _highestWeight
	{
		get
		{
			if (_list.Count != 0)
			{
				return _list.Keys[_list.Keys.Count - 1];
			}
			return 0.0;
		}
	}

	public int Count => _list.Count;

	public WRandomD()
	{
		_list = new SortedList<double, T>();
	}

	public WRandomD(Dictionary<T, double> weights)
		: this()
	{
		if (weights == null)
		{
			return;
		}
		foreach (T key in weights.Keys)
		{
			Add(weights[key], key);
		}
	}

	public T Add(double weight, T obj)
	{
		_list.Add(_highestWeight + MathUtil.Clamp(weight, 1E-08, 10000.0), obj);
		return obj;
	}

	public WRandomD<T> Add(Dictionary<T, double> weights)
	{
		foreach (KeyValuePair<T, double> weight in weights)
		{
			Add(weight.Value, weight.Key);
		}
		return this;
	}

	public WRandomD<T> Add(Dictionary<T, float> weights)
	{
		foreach (KeyValuePair<T, float> weight in weights)
		{
			Add(weight.Value, weight.Key);
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
			double num = ((index > 0) ? (_list.Keys[index] - _list.Keys[index - 1]) : _list.Keys[index]);
			_list.RemoveAt(index);
			for (int i = index; i < _list.Keys.Count; i++)
			{
				double key = _list.Keys[i] - num;
				T value = _list[_list.Keys[i]];
				_list.RemoveAt(i);
				_list.Add(key, value);
			}
		}
	}

	public void Clear()
	{
		_list.Clear();
	}

	public void RemoveMinWeights()
	{
		using PoolStructListHandle<int> poolStructListHandle = Pools.UseStructList<int>();
		int num = 0;
		bool flag = false;
		foreach (KeyValuePair<double, T> item in _list)
		{
			if (item.Key <= 1E-08)
			{
				poolStructListHandle.Add(num);
			}
			else
			{
				flag = true;
			}
			num++;
		}
		if (flag)
		{
			for (int num2 = poolStructListHandle.Count - 1; num2 >= 0; num2--)
			{
				_RemoveAt(poolStructListHandle[num2]);
			}
		}
	}

	public T Random(double zeroToOne)
	{
		if (_list.Keys.Count == 0)
		{
			return default(T);
		}
		if (_list.Keys.Count == 1)
		{
			return _list.Values[0];
		}
		double num = _highestWeight * MathUtil.Clamp01(zeroToOne);
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

	public PoolKeepItemListHandle<T> EnumerateInRandomOrder(Random random)
	{
		PoolKeepItemListHandle<T> poolKeepItemListHandle = Pools.UseKeepItemList<T>();
		using PoolWRandomDHandle<T> poolWRandomDHandle = Pools.UseWRandomD<T>();
		WRandomD<T> value = poolWRandomDHandle.value;
		foreach (KeyValuePair<double, T> item in _list)
		{
			value._list.Add(item.Key, item.Value);
		}
		while (value.Count > 0)
		{
			T val = value.Random(random.NextDouble());
			poolKeepItemListHandle.Add(val);
			value.Remove(val);
		}
		return poolKeepItemListHandle;
	}

	public IEnumerable<Couple<double, T>> EnumeratePairInOrder()
	{
		double num = 0.0;
		foreach (double weight in _list.Keys)
		{
			yield return new Couple<double, T>(weight - num, _list[weight]);
			num = weight;
		}
	}

	public Dictionary<T, double> ToWeightsDictionary()
	{
		Dictionary<T, double> dictionary = new Dictionary<T, double>();
		foreach (Couple<double, T> item in EnumeratePairInOrder())
		{
			dictionary.Add(item, item);
		}
		return dictionary;
	}

	public Dictionary<T, double> ToProbabilityDictionary()
	{
		return ToWeightsDictionary().NormalizeWeights();
	}

	public string ToStringProbabilityDistribution()
	{
		return (from c in EnumeratePairInOrder()
			orderby c.a descending
			select c).Select(delegate(Couple<double, T> pair)
		{
			string a = (pair.a / _highestWeight * 100.0).ToString("#.##") + "%";
			T b = pair.b;
			return new Couple<string, string>(a, b.ToString());
		}).ToStringSmart();
	}
}
