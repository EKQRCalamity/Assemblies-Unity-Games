using System;

public class PooledValue<T> : IDisposable where T : struct
{
	public T value;

	static PooledValue()
	{
		Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new PooledValue<T>(), delegate
		{
		}, delegate
		{
		});
	}

	public static PooledValue<T> Create(T value)
	{
		PooledValue<T> pooledValue = Pools.Unpool<PooledValue<T>>();
		pooledValue.value = value;
		return pooledValue;
	}

	public void Dispose()
	{
		Pools.Repool(this);
	}

	public static implicit operator T(PooledValue<T> pooledValue)
	{
		return pooledValue.value;
	}
}
