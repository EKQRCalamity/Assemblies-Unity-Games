using System;
using System.Collections.Generic;

public class PoolDictionaryValuesHandle<K, V> : IDisposable where V : class
{
	public struct Enumerator : IDisposable
	{
		private readonly PoolDictionaryValuesHandle<K, V> _handle;

		private Dictionary<K, V>.Enumerator _enumerator;

		public KeyValuePair<K, V> Current => _enumerator.Current;

		public Enumerator(PoolDictionaryValuesHandle<K, V> handle)
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

	public static PoolDictionaryValuesHandle<K, V> GetHandle()
	{
		return Pools.Unpool<PoolDictionaryValuesHandle<K, V>>();
	}

	static PoolDictionaryValuesHandle()
	{
		Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new PoolDictionaryValuesHandle<K, V>(), delegate(PoolDictionaryValuesHandle<K, V> handle)
		{
			handle.Clear();
		}, delegate(PoolDictionaryValuesHandle<K, V> handle)
		{
			handle.OnUnpool();
		});
		Pools.CreatePool<V>();
		Pools.CreatePoolDictionary<K, V>();
	}

	public PoolDictionaryValuesHandle<K, V> Add(K key, V value)
	{
		this.value.Add(key, value);
		return this;
	}

	public bool Remove(K key)
	{
		if (value.ContainsKey(key))
		{
			Pools.Repool(value[key]);
		}
		return value.Remove(key);
	}

	public bool Replace(K key, V value)
	{
		bool result = Remove(key);
		Add(key, value);
		return result;
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

	public static implicit operator Dictionary<K, V>(PoolDictionaryValuesHandle<K, V> handle)
	{
		return handle.value;
	}

	public static implicit operator bool(PoolDictionaryValuesHandle<K, V> handle)
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
			Pools.RepoolDictionaryValues(value, repoolCollection: true);
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
