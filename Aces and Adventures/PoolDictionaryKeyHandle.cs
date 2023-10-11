using System;
using System.Collections.Generic;

public class PoolDictionaryKeyHandle<K, V> : IDisposable where K : class where V : struct
{
	public struct Enumerator : IDisposable
	{
		private readonly PoolDictionaryKeyHandle<K, V> _handle;

		private Dictionary<K, V>.Enumerator _enumerator;

		public KeyValuePair<K, V> Current => _enumerator.Current;

		public Enumerator(PoolDictionaryKeyHandle<K, V> handle)
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

	public static PoolDictionaryKeyHandle<K, V> GetHandle()
	{
		return Pools.Unpool<PoolDictionaryKeyHandle<K, V>>();
	}

	static PoolDictionaryKeyHandle()
	{
		Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new PoolDictionaryKeyHandle<K, V>(), delegate(PoolDictionaryKeyHandle<K, V> handle)
		{
			handle.Clear();
		}, delegate(PoolDictionaryKeyHandle<K, V> handle)
		{
			handle.OnUnpool();
		});
		Pools.CreatePool<K>();
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

	public static implicit operator Dictionary<K, V>(PoolDictionaryKeyHandle<K, V> handle)
	{
		return handle.value;
	}

	public static implicit operator bool(PoolDictionaryKeyHandle<K, V> handle)
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
			Pools.RepoolDictionaryKeys(value, repoolCollection: true);
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
