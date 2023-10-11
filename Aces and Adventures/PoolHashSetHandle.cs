using System;
using System.Collections.Generic;

public class PoolHashSetHandle<T> : IDisposable where T : class
{
	public struct Enumerator : IDisposable
	{
		private readonly PoolHashSetHandle<T> _handle;

		private HashSet<T>.Enumerator _enumerator;

		public T Current => _enumerator.Current;

		public Enumerator(PoolHashSetHandle<T> handle)
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

	public static PoolHashSetHandle<T> GetHandle()
	{
		return Pools.Unpool<PoolHashSetHandle<T>>();
	}

	static PoolHashSetHandle()
	{
		Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new PoolHashSetHandle<T>(), delegate(PoolHashSetHandle<T> handle)
		{
			handle.Clear();
		}, delegate(PoolHashSetHandle<T> handle)
		{
			handle.OnUnpool();
		});
		Pools.CreatePool<T>();
		Pools.CreatePoolHashSet<T>();
	}

	public bool Add(T value)
	{
		return this.value.Add(value);
	}

	public bool Remove(T value)
	{
		if (this.value.Remove(value))
		{
			Pools.Repool(value);
			return true;
		}
		return false;
	}

	public bool Contains(T value)
	{
		return this.value.Contains(value);
	}

	public static implicit operator HashSet<T>(PoolHashSetHandle<T> handle)
	{
		return handle.value;
	}

	public static implicit operator bool(PoolHashSetHandle<T> handle)
	{
		return handle.value != null;
	}

	private void OnUnpool()
	{
		value = Pools.Unpool<HashSet<T>>();
	}

	private void Clear()
	{
		if (value != null)
		{
			Pools.RepoolItems(value, repoolCollection: true);
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
