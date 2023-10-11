using System;
using System.Collections.Generic;

public class PoolStructStackHandle<T> : IDisposable where T : struct
{
	public struct Enumerator : IDisposable
	{
		private readonly PoolStructStackHandle<T> _handle;

		private Stack<T>.Enumerator _enumerator;

		public T Current => _enumerator.Current;

		public Enumerator(PoolStructStackHandle<T> handle)
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

	public static PoolStructStackHandle<T> GetHandle()
	{
		return Pools.Unpool<PoolStructStackHandle<T>>();
	}

	static PoolStructStackHandle()
	{
		Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new PoolStructStackHandle<T>(), delegate(PoolStructStackHandle<T> handle)
		{
			handle.Clear();
		}, delegate(PoolStructStackHandle<T> handle)
		{
			handle.OnUnpool();
		});
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

	public static implicit operator Stack<T>(PoolStructStackHandle<T> handle)
	{
		return handle.value;
	}

	public static implicit operator bool(PoolStructStackHandle<T> handle)
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
