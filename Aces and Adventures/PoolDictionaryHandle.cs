using System;
using System.Collections.Generic;

public class PoolDictionaryHandle<K, V> : IDisposable where K : class where V : class
{
	public struct Enumerator : IDisposable
	{
		private readonly PoolDictionaryHandle<K, V> _handle;

		private Dictionary<K, V>.Enumerator _enumerator;

		public KeyValuePair<K, V> Current => _enumerator.Current;

		public Enumerator(PoolDictionaryHandle<K, V> handle)
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

	public static PoolDictionaryHandle<K, V> GetHandle()
	{
		return Pools.Unpool<PoolDictionaryHandle<K, V>>();
	}

	static PoolDictionaryHandle()
	{
		Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new PoolDictionaryHandle<K, V>(), delegate(PoolDictionaryHandle<K, V> handle)
		{
			handle.Clear();
		}, delegate(PoolDictionaryHandle<K, V> handle)
		{
			handle.OnUnpool();
		});
		Pools.CreatePool<K>();
		Pools.CreatePool<V>();
		Pools.CreatePoolDictionary<K, V>();
	}

	public void Add(K key, V value)
	{
		this.value.Add(key, value);
	}

	public bool Remove(K key)
	{
		if (value.ContainsKey(key))
		{
			Pools.Repool(value[key]);
			value.Remove(key);
			Pools.Repool(key);
			return true;
		}
		return false;
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

	public static implicit operator Dictionary<K, V>(PoolDictionaryHandle<K, V> handle)
	{
		return handle.value;
	}

	public static implicit operator bool(PoolDictionaryHandle<K, V> handle)
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
			Pools.RepoolDictionaryItems(value, repoolCollection: true);
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
