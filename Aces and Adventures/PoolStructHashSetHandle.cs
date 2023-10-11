using System;
using System.Collections.Generic;

public class PoolStructHashSetHandle<T> : IDisposable where T : struct
{
	public struct Enumerator : IDisposable
	{
		private readonly PoolStructHashSetHandle<T> _handle;

		private HashSet<T>.Enumerator _enumerator;

		public T Current => _enumerator.Current;

		public Enumerator(PoolStructHashSetHandle<T> handle)
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

	public static PoolStructHashSetHandle<T> GetHandle()
	{
		return Pools.Unpool<PoolStructHashSetHandle<T>>();
	}

	static PoolStructHashSetHandle()
	{
		Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new PoolStructHashSetHandle<T>(), delegate(PoolStructHashSetHandle<T> handle)
		{
			handle.Clear();
		}, delegate(PoolStructHashSetHandle<T> handle)
		{
			handle.OnUnpool();
		});
		Pools.CreatePoolHashSet<T>();
	}

	public bool Add(T value)
	{
		return this.value.Add(value);
	}

	public PoolStructHashSetHandle<T> Append(T value)
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

	public PoolStructHashSetHandle<T> CopyFrom(HashSet<T> copyFrom)
	{
		if (copyFrom != null)
		{
			value.CopyFrom(copyFrom);
		}
		return this;
	}

	public PoolStructHashSetHandle<T> CopyFromAndRepool(PoolStructHashSetHandle<T> copyFrom)
	{
		if (copyFrom != null)
		{
			foreach (T item in copyFrom)
			{
				value.Add(item);
			}
			return this;
		}
		return this;
	}

	public PoolStructHashSetHandle<T> CopyFrom(List<T> copyFrom, int? count = null)
	{
		value.CopyFrom(copyFrom, count);
		return this;
	}

	public PoolStructHashSetHandle<T> CopyFrom(IEnumerable<T> copyFrom)
	{
		foreach (T item in copyFrom)
		{
			value.Add(item);
		}
		return this;
	}

	public PoolStructHashSetHandle<T> CreateCopyHandle()
	{
		return Pools.UseStructHashSet(value);
	}

	public IEnumerable<T> AsEnumerable()
	{
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			yield return enumerator.Current;
		}
	}

	public static implicit operator HashSet<T>(PoolStructHashSetHandle<T> handle)
	{
		return handle.value;
	}

	public static implicit operator bool(PoolStructHashSetHandle<T> handle)
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
