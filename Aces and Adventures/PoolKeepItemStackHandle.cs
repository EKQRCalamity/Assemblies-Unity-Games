using System;
using System.Collections.Generic;

public class PoolKeepItemStackHandle<T> : IDisposable
{
	public struct Enumerator : IDisposable
	{
		private readonly PoolKeepItemStackHandle<T> _handle;

		private Stack<T>.Enumerator _enumerator;

		public T Current => _enumerator.Current;

		public Enumerator(PoolKeepItemStackHandle<T> handle)
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

	public Stack<T> value { get; private set; }

	public int Count => value.Count;

	public static PoolKeepItemStackHandle<T> GetHandle()
	{
		return Pools.Unpool<PoolKeepItemStackHandle<T>>();
	}

	static PoolKeepItemStackHandle()
	{
		Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new PoolKeepItemStackHandle<T>(), delegate(PoolKeepItemStackHandle<T> handle)
		{
			handle.Clear();
		}, delegate(PoolKeepItemStackHandle<T> handle)
		{
			handle.OnUnpool();
		});
		Pools.CreatePoolStack<T>();
	}

	public void Push(T item)
	{
		value.Push(item);
	}

	public T Pop()
	{
		return value.Pop();
	}

	public PoolKeepItemStackHandle<T> CopyFrom(List<T> copyFrom)
	{
		foreach (T item in copyFrom)
		{
			value.Push(item);
		}
		return this;
	}

	public PoolKeepItemStackHandle<T> CopyFrom(IEnumerable<T> copyFrom)
	{
		foreach (T item in copyFrom)
		{
			value.Push(item);
		}
		return this;
	}

	public static implicit operator Stack<T>(PoolKeepItemStackHandle<T> handle)
	{
		return handle.value;
	}

	public static implicit operator bool(PoolKeepItemStackHandle<T> handle)
	{
		return handle.value != null;
	}

	private void OnUnpool()
	{
		value = Pools.Unpool<Stack<T>>();
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
