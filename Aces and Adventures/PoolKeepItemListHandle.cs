using System;
using System.Collections.Generic;

public class PoolKeepItemListHandle<T> : IDisposable
{
	public struct Enumerator : IDisposable
	{
		private readonly PoolKeepItemListHandle<T> _handle;

		private List<T>.Enumerator _enumerator;

		public T Current => _enumerator.Current;

		public Enumerator(PoolKeepItemListHandle<T> handle)
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

	public static PoolKeepItemListHandle<T> GetHandle()
	{
		return Pools.Unpool<PoolKeepItemListHandle<T>>();
	}

	static PoolKeepItemListHandle()
	{
		Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new PoolKeepItemListHandle<T>(), delegate(PoolKeepItemListHandle<T> handle)
		{
			handle.Clear();
		}, delegate(PoolKeepItemListHandle<T> handle)
		{
			handle.OnUnpool();
		});
		Pools.CreatePoolList<T>();
	}

	public PoolKeepItemListHandle<T> Add(T value)
	{
		this.value.Add(value);
		return this;
	}

	public T AddReturn(T item)
	{
		value.Add(item);
		return item;
	}

	public bool Remove(T value)
	{
		return this.value.Remove(value);
	}

	public void RemoveAt(int index)
	{
		value.RemoveAt(index);
	}

	public PoolKeepItemListHandle<T> CopyFrom(List<T> copyFrom)
	{
		value.CopyFrom(copyFrom);
		return this;
	}

	public PoolKeepItemListHandle<T> CopyFrom(HashSet<T> copyFrom)
	{
		value.CopyFrom(copyFrom);
		return this;
	}

	public PoolKeepItemListHandle<T> CopyFrom(IEnumerable<T> copyFrom)
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

	public PoolKeepItemListHandle<T> FillToCount(int count, T fillWithValue = default(T))
	{
		for (int i = Count; i < count; i++)
		{
			Add(fillWithValue);
		}
		return this;
	}

	public static implicit operator List<T>(PoolKeepItemListHandle<T> handle)
	{
		return handle.value;
	}

	public static implicit operator bool(PoolKeepItemListHandle<T> handle)
	{
		if (handle != null)
		{
			return handle.value != null;
		}
		return false;
	}

	private void OnUnpool()
	{
		value = Pools.Unpool<List<T>>();
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
