using System;
using System.Collections.Generic;

public class PoolKeepItemHashSetHandle<T> : IDisposable
{
	public struct Enumerator : IDisposable
	{
		private readonly PoolKeepItemHashSetHandle<T> _handle;

		private HashSet<T>.Enumerator _enumerator;

		public T Current => _enumerator.Current;

		public Enumerator(PoolKeepItemHashSetHandle<T> handle)
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

	public HashSet<T> value { get; private set; }

	public int Count => value.Count;

	public static PoolKeepItemHashSetHandle<T> GetHandle()
	{
		return Pools.Unpool<PoolKeepItemHashSetHandle<T>>();
	}

	static PoolKeepItemHashSetHandle()
	{
		Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new PoolKeepItemHashSetHandle<T>(), delegate(PoolKeepItemHashSetHandle<T> handle)
		{
			handle.Clear();
		}, delegate(PoolKeepItemHashSetHandle<T> handle)
		{
			handle.OnUnpool();
		});
		Pools.CreatePoolHashSet<T>();
	}

	public bool Add(T value)
	{
		return this.value.Add(value);
	}

	public PoolKeepItemHashSetHandle<T> AddReturn(T value)
	{
		this.value.Add(value);
		return this;
	}

	public bool Remove(T value)
	{
		return this.value.Remove(value);
	}

	public bool Contains(T value)
	{
		return this.value.Contains(value);
	}

	public PoolKeepItemHashSetHandle<T> CopyFrom(List<T> copyFrom)
	{
		foreach (T item in copyFrom)
		{
			value.Add(item);
		}
		return this;
	}

	public PoolKeepItemHashSetHandle<T> CopyFrom(HashSet<T> copyFrom)
	{
		foreach (T item in copyFrom)
		{
			value.Add(item);
		}
		return this;
	}

	public PoolKeepItemHashSetHandle<T> CopyFrom(IEnumerable<T> copyFrom)
	{
		foreach (T item in copyFrom)
		{
			value.Add(item);
		}
		return this;
	}

	public IEnumerable<T> AsEnumerable()
	{
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			yield return enumerator.Current;
		}
	}

	public static implicit operator HashSet<T>(PoolKeepItemHashSetHandle<T> handle)
	{
		return handle?.value;
	}

	public static implicit operator bool(PoolKeepItemHashSetHandle<T> handle)
	{
		if (handle != null)
		{
			return handle.value != null;
		}
		return false;
	}

	private void OnUnpool()
	{
		value = Pools.Unpool<HashSet<T>>();
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
