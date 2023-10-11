using System;

public class PoolStructArrayHandle<T> : IDisposable where T : struct
{
	public struct Enumerator : IDisposable
	{
		private readonly PoolStructArrayHandle<T> _handle;

		private int _index;

		public T Current => _handle.value[_index];

		public Enumerator(PoolStructArrayHandle<T> handle)
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

	public static PoolStructArrayHandle<T> GetHandle()
	{
		return Pools.Unpool<PoolStructArrayHandle<T>>();
	}

	static PoolStructArrayHandle()
	{
		Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new PoolStructArrayHandle<T>(), delegate(PoolStructArrayHandle<T> handle)
		{
			handle.Clear();
		}, delegate
		{
		});
		Pools.ArrayPool<T>.Initialize();
	}

	public PoolStructArrayHandle<T> _SetValue(T[] value)
	{
		this.value = value;
		return this;
	}

	public static implicit operator T[](PoolStructArrayHandle<T> handle)
	{
		return handle.value;
	}

	public static implicit operator bool(PoolStructArrayHandle<T> handle)
	{
		return handle.value != null;
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
