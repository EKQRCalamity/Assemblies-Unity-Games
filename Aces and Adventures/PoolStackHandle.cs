using System;
using System.Collections.Generic;

public class PoolStackHandle<T> : IDisposable where T : class
{
	public struct Enumerator : IDisposable
	{
		private readonly PoolStackHandle<T> _handle;

		private Stack<T>.Enumerator _enumerator;

		public T Current => _enumerator.Current;

		public Enumerator(PoolStackHandle<T> handle)
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

	public static PoolStackHandle<T> GetHandle()
	{
		return Pools.Unpool<PoolStackHandle<T>>();
	}

	static PoolStackHandle()
	{
		Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new PoolStackHandle<T>(), delegate(PoolStackHandle<T> handle)
		{
			handle.Clear();
		}, delegate(PoolStackHandle<T> handle)
		{
			handle.OnUnpool();
		});
		Pools.CreatePool<T>();
		Pools.CreatePoolStack<T>();
	}

	public void Push(T value)
	{
		this.value.Push(value);
	}

	public T Pop()
	{
		return value.Pop();
	}

	public static implicit operator Stack<T>(PoolStackHandle<T> handle)
	{
		return handle.value;
	}

	public static implicit operator bool(PoolStackHandle<T> handle)
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
