using System;
using System.Collections.Generic;

public class PoolDictionaryStructCollectionHandle<K, V, E> : IDisposable where K : struct where V : class, ICollection<E> where E : struct
{
	public struct Enumerator : IDisposable
	{
		private readonly PoolDictionaryStructCollectionHandle<K, V, E> _handle;

		private Dictionary<K, V>.Enumerator _enumerator;

		public KeyValuePair<K, V> Current => _enumerator.Current;

		public Enumerator(PoolDictionaryStructCollectionHandle<K, V, E> handle)
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

	public static PoolDictionaryStructCollectionHandle<K, V, E> GetHandle()
	{
		return Pools.Unpool<PoolDictionaryStructCollectionHandle<K, V, E>>();
	}

	static PoolDictionaryStructCollectionHandle()
	{
		Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new PoolDictionaryStructCollectionHandle<K, V, E>(), delegate(PoolDictionaryStructCollectionHandle<K, V, E> handle)
		{
			handle.Clear();
		}, delegate(PoolDictionaryStructCollectionHandle<K, V, E> handle)
		{
			handle.OnUnpool();
		});
		Pools.CreatePoolDictionary<K, V>();
		Pools.CreatePool<V>();
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

	public static implicit operator Dictionary<K, V>(PoolDictionaryStructCollectionHandle<K, V, E> handle)
	{
		return handle.value;
	}

	public static implicit operator bool(PoolDictionaryStructCollectionHandle<K, V, E> handle)
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
			DictionaryValueEnumerator<K, V>.Enumerator enumerator = value.EnumerateValues().GetEnumerator();
			while (enumerator.MoveNext())
			{
				Pools.Repool(enumerator.Current);
			}
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
