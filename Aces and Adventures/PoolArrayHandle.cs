using System;

public class PoolArrayHandle<T> : IDisposable where T : class
{
	public struct Enumerator : IDisposable
	{
		private readonly PoolArrayHandle<T> _handle;

		private int _index;

		public T Current => _handle.value[_index];

		public Enumerator(PoolArrayHandle<T> handle)
		{
			_handle = handle;
			_index = -1;
		}

		public bool MoveNext()
		{
			return ++_index < _handle.value.Length;
		}

		public void Dispose()
		{
			_handle.Dispose();
		}
	}

	public T[] value { get; private set; }

	public int Length => value.Length;

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

	public static PoolArrayHandle<T> GetHandle()
	{
		return Pools.Unpool<PoolArrayHandle<T>>();
	}

	static PoolArrayHandle()
	{
		Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new PoolArrayHandle<T>(), delegate(PoolArrayHandle<T> handle)
		{
			handle.Clear();
		}, delegate
		{
		});
		Pools.CreatePool<T>();
		Pools.ArrayPool<T>.Initialize();
	}

	public PoolArrayHandle<T> _SetValue(T[] value)
	{
		this.value = value;
		return this;
	}

	public static implicit operator T[](PoolArrayHandle<T> handle)
	{
		return handle.value;
	}

	public static implicit operator bool(PoolArrayHandle<T> handle)
	{
		return handle.value != null;
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
