using System;
using System.Collections.Generic;

public class PoolStructQueueHandle<T> : IDisposable where T : struct
{
	public struct Enumerator : IDisposable
	{
		private readonly PoolStructQueueHandle<T> _handle;

		private Queue<T>.Enumerator _enumerator;

		public T Current => _enumerator.Current;

		public Enumerator(PoolStructQueueHandle<T> handle)
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

	public Queue<T> value { get; private set; }

	public int Count => value.Count;

	public static PoolStructQueueHandle<T> GetHandle()
	{
		return Pools.Unpool<PoolStructQueueHandle<T>>();
	}

	static PoolStructQueueHandle()
	{
		Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new PoolStructQueueHandle<T>(), delegate(PoolStructQueueHandle<T> handle)
		{
			handle.Clear();
		}, delegate(PoolStructQueueHandle<T> handle)
		{
			handle.OnUnpool();
		});
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

	public static implicit operator Queue<T>(PoolStructQueueHandle<T> handle)
	{
		return handle.value;
	}

	public static implicit operator bool(PoolStructQueueHandle<T> handle)
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
