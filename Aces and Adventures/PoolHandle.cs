using System;

public class PoolHandle<T> : IDisposable where T : class
{
	public T value { get; private set; }

	public static PoolHandle<T> GetHandle()
	{
		return Pools.Unpool<PoolHandle<T>>();
	}

	static PoolHandle()
	{
		Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new PoolHandle<T>(), delegate(PoolHandle<T> handle)
		{
			handle.Clear();
		}, delegate(PoolHandle<T> handle)
		{
			handle.OnUnpool();
		});
		Pools.CreatePool<T>();
	}

	public static implicit operator T(PoolHandle<T> handle)
	{
		return handle.value;
	}

	public static implicit operator bool(PoolHandle<T> handle)
	{
		return handle.value != null;
	}

	private void OnUnpool()
	{
		value = Pools.Unpool<T>();
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
}
