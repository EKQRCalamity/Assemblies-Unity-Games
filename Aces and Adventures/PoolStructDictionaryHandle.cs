using System;
using System.Collections.Generic;

public class PoolStructDictionaryHandle<K, V> : IDisposable where K : struct where V : struct
{
	public struct Enumerator : IDisposable
	{
		private readonly PoolStructDictionaryHandle<K, V> _handle;

		private Dictionary<K, V>.Enumerator _enumerator;

		public KeyValuePair<K, V> Current => _enumerator.Current;

		public Enumerator(PoolStructDictionaryHandle<K, V> handle)
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

	public static PoolStructDictionaryHandle<K, V> GetHandle()
	{
		return Pools.Unpool<PoolStructDictionaryHandle<K, V>>();
	}

	static PoolStructDictionaryHandle()
	{
		Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new PoolStructDictionaryHandle<K, V>(), delegate(PoolStructDictionaryHandle<K, V> handle)
		{
			handle.Clear();
		}, delegate(PoolStructDictionaryHandle<K, V> handle)
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

	public static implicit operator Dictionary<K, V>(PoolStructDictionaryHandle<K, V> handle)
	{
		return handle.value;
	}

	public static implicit operator bool(PoolStructDictionaryHandle<K, V> handle)
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
