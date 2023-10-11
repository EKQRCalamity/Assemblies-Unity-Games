using System;
using System.Collections.Generic;

public class PoolStructListHandle<T> : IDisposable where T : struct
{
	public struct Enumerator : IDisposable
	{
		private readonly PoolStructListHandle<T> _handle;

		private List<T>.Enumerator _enumerator;

		public T Current => _enumerator.Current;

		public Enumerator(PoolStructListHandle<T> handle)
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

	public static PoolStructListHandle<T> GetHandle()
	{
		return Pools.Unpool<PoolStructListHandle<T>>();
	}

	static PoolStructListHandle()
	{
		Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new PoolStructListHandle<T>(), delegate(PoolStructListHandle<T> handle)
		{
			handle.Clear();
		}, delegate(PoolStructListHandle<T> handle)
		{
			handle.OnUnpool();
		});
		Pools.CreatePoolList<T>();
	}

	public List<T> CreateCopy()
	{
		List<T> list = Pools.Unpool<List<T>>();
		for (int i = 0; i < value.Count; i++)
		{
			list.Add(value[i]);
		}
		return list;
	}

	public PoolStructListHandle<T> CreateCopyHandle()
	{
		PoolStructListHandle<T> poolStructListHandle = Pools.UseStructList<T>();
		for (int i = 0; i < value.Count; i++)
		{
			poolStructListHandle.value.Add(value[i]);
		}
		return poolStructListHandle;
	}

	public List<T> CreateNonPooledCopy()
	{
		if (Count == 0)
		{
			return null;
		}
		List<T> list = new List<T>(Count);
		for (int i = 0; i < value.Count; i++)
		{
			list.Add(value[i]);
		}
		return list;
	}

	public PoolStructListHandle<T> CopyFrom(List<T> copyFrom, int? count = null)
	{
		if (copyFrom != null)
		{
			value.CopyFrom(copyFrom, count);
		}
		return this;
	}

	public PoolStructListHandle<T> CopyFrom(HashSet<T> copyFrom)
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

	public PoolStructListHandle<T> CopyFrom(IEnumerable<T> copyFrom)
	{
		foreach (T item in copyFrom)
		{
			value.Add(item);
		}
		return this;
	}

	public PoolStructListHandle<T> CopyFromAndRepool(PoolStructListHandle<T> copyFrom)
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

	public PoolStructListHandle<T> Insert(int insertIndex, List<T> insertedItems, int? insertCount = null)
	{
		int num = insertCount ?? insertedItems.Count;
		for (int i = 0; i < num; i++)
		{
			value.Insert(insertIndex + i, insertedItems[i]);
		}
		return this;
	}

	public PoolStructListHandle<T> Append(T item)
	{
		value.Add(item);
		return this;
	}

	public PoolStructListHandle<T> Add(T value)
	{
		this.value.Add(value);
		return this;
	}

	public bool Remove(T value)
	{
		return this.value.Remove(value);
	}

	public void RemoveAt(int index)
	{
		value.RemoveAt(index);
	}

	public T? Last()
	{
		return value.LastValue();
	}

	public T? GetLastAndRepool()
	{
		using (this)
		{
			return Last();
		}
	}

	public IEnumerable<T> AsEnumerable()
	{
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			yield return enumerator.Current;
		}
	}

	public static implicit operator List<T>(PoolStructListHandle<T> handle)
	{
		return handle?.value;
	}

	public static implicit operator bool(PoolStructListHandle<T> handle)
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
