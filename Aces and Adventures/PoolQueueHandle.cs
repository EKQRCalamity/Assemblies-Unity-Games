using System;
using System.Collections.Generic;

public class PoolQueueHandle<T> : IDisposable where T : class
{
	public struct Enumerator : IDisposable
	{
		private readonly PoolQueueHandle<T> _handle;

		private Queue<T>.Enumerator _enumerator;

		public T Current => _enumerator.Current;

		public Enumerator(PoolQueueHandle<T> handle)
		{
			_handle = handle;
			_enumerator = handle.value.GetEnumerator();
		}

		public bool MoveNext()
		{
			return _enumerator.MoveNext();
		}

		void IDisposable.Dispose()
		{
			_handle.Dispose();
		}
	}

	public Queue<T> value { get; private set; }

	public int Count => value.Count;

	public static PoolQueueHandle<T> GetHandle()
	{
		return Pools.Unpool<PoolQueueHandle<T>>();
	}

	static PoolQueueHandle()
	{
		Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new PoolQueueHandle<T>(), delegate(PoolQueueHandle<T> handle)
		{
			handle.Clear();
		}, delegate(PoolQueueHandle<T> handle)
		{
			handle.OnUnpool();
		});
		Pools.CreatePool<T>();
		Pools.CreatePoolQueue<T>();
	}

	public void Enqueue(T value)
	{
		this.value.Enqueue(value);
	}

	public T Dequeue()
	{
		return value.Dequeue();
	}

	public static implicit operator Queue<T>(PoolQueueHandle<T> handle)
	{
		return handle.value;
	}

	public static implicit operator bool(PoolQueueHandle<T> handle)
	{
		return handle.value != null;
	}

	private void OnUnpool()
	{
		value = Pools.Unpool<Queue<T>>();
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
