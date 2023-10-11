using System;
using System.Collections.Generic;

public class PoolKeepItemDictionaryHandle<K, V> : IDisposable
{
	public struct Enumerator : IDisposable
	{
		private readonly PoolKeepItemDictionaryHandle<K, V> _handle;

		private Dictionary<K, V>.Enumerator _enumerator;

		public KeyValuePair<K, V> Current => _enumerator.Current;

		public Enumerator(PoolKeepItemDictionaryHandle<K, V> handle)
		{
			_handle = handle;
			_enumerator = handle.value.GetEnumerator();
		}

		public bool MoveNext()
		{
			return _enumerator.MoveNext();
		}

		public void Dispose()
		{
			_handle.Dispose();
		}
	}

	public Dictionary<K, V> value { get; private set; }

	public V this[K key]
	{
		get
		{
			return value[key];
		}
		set
		{
			this.value[key] = value;
		}
	}

	public int Count => value.Count;

	public static PoolKeepItemDictionaryHandle<K, V> GetHandle()
	{
		return Pools.Unpool<PoolKeepItemDictionaryHandle<K, V>>();
	}

	static PoolKeepItemDictionaryHandle()
	{
		Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new PoolKeepItemDictionaryHandle<K, V>(), delegate(PoolKeepItemDictionaryHandle<K, V> handle)
		{
			handle.Clear();
		}, delegate(PoolKeepItemDictionaryHandle<K, V> handle)
		{
			handle.OnUnpool();
		});
		Pools.CreatePoolDictionary<K, V>();
	}

	public void Add(K key, V value)
	{
		this.value.Add(key, value);
	}

	public bool Remove(K key)
	{
		return value.Remove(key);
	}

	public bool ContainsKey(K key)
	{
		return value.ContainsKey(key);
	}

	public DictionaryPairEnumerator<K, V> Pairs()
	{
		return value.EnumeratePairs();
	}

	public DictionaryKeyEnumerator<K, V> Keys()
	{
		return value.EnumerateKeys();
	}

	public DictionaryValueEnumerator<K, V> Values()
	{
		return value.EnumerateValues();
	}

	public IEnumerable<KeyValuePair<K, V>> AsEnumerable()
	{
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			yield return enumerator.Current;
		}
	}

	public PoolKeepItemDictionaryHandle<K, V> CopyFrom(Dictionary<K, V> dictionary)
	{
		foreach (KeyValuePair<K, V> item in dictionary)
		{
			value.Add(item.Key, item.Value);
		}
		return this;
	}

	public PoolKeepItemDictionaryHandle<K, V> CopyFrom(IEnumerable<KeyValuePair<K, V>> pairs)
	{
		foreach (KeyValuePair<K, V> pair in pairs)
		{
			value[pair.Key] = pair.Value;
		}
		return this;
	}

	public static implicit operator Dictionary<K, V>(PoolKeepItemDictionaryHandle<K, V> handle)
	{
		return handle?.value;
	}

	public static implicit operator bool(PoolKeepItemDictionaryHandle<K, V> handle)
	{
		return handle.value != null;
	}

	private void OnUnpool()
	{
		value = Pools.Unpool<Dictionary<K, V>>();
	}

	private void Clear()
	{
		if (value != null)
		{
			Pools.Repool(value);
			value = null;
			Pools.Repool(this);
		}
	}

	public void Dispose()
	{
		Clear();
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}
}
