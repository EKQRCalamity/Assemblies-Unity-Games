using System;
using System.Collections.Generic;

public class PoolListHandle<T> : IDisposable where T : class
{
	public struct Enumerator : IDisposable
	{
		private readonly PoolListHandle<T> _handle;

		private List<T>.Enumerator _enumerator;

		public T Current => _enumerator.Current;

		public Enumerator(PoolListHandle<T> handle)
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

	public List<T> value { get; private set; }

	public T this[int index]
	{
		get
		{
			return value[index];
		}
		set
		{
			this.value[index] = value;
		}
	}

	public int Count => value.Count;

	public static PoolListHandle<T> GetHandle()
	{
		return Pools.Unpool<PoolListHandle<T>>();
	}

	static PoolListHandle()
	{
		Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new PoolListHandle<T>(), delegate(PoolListHandle<T> handle)
		{
			handle.Clear();
		}, delegate(PoolListHandle<T> handle)
		{
			handle.OnUnpool();
		});
		Pools.CreatePool<T>();
		Pools.CreatePoolList<T>();
	}

	public void Add(T value)
	{
		this.value.Add(value);
	}

	public T AddReturn(T value)
	{
		this.value.Add(value);
		return value;
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

	public void RemoveAt(int index)
	{
		Pools.Repool(value[index]);
		value.RemoveAt(index);
	}

	public PoolListHandle<T> CopyFrom(IEnumerable<T> items)
	{
		foreach (T item in items)
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

	public static implicit operator List<T>(PoolListHandle<T> handle)
	{
		return handle.value;
	}

	public static implicit operator bool(PoolListHandle<T> handle)
	{
		return handle.value != null;
	}

	private void OnUnpool()
	{
		value = Pools.Unpool<List<T>>();
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
