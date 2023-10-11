using System;

public class PoolWRandomDHandle<T> : IDisposable
{
	public WRandomD<T> value { get; private set; }

	public static PoolWRandomDHandle<T> GetHandle()
	{
		return Pools.Unpool<PoolWRandomDHandle<T>>();
	}

	static PoolWRandomDHandle()
	{
		Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new PoolWRandomDHandle<T>(), delegate(PoolWRandomDHandle<T> handle)
		{
			handle.Clear();
		}, delegate(PoolWRandomDHandle<T> handle)
		{
			handle.OnUnpool();
		});
		Pools.CreatePoolWRandomD<T>();
	}

	public static implicit operator WRandomD<T>(PoolWRandomDHandle<T> handle)
	{
		return handle.value;
	}

	public static implicit operator bool(PoolWRandomDHandle<T> handle)
	{
		return handle.value != null;
	}

	private void OnUnpool()
	{
		value = Pools.Unpool<WRandomD<T>>();
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
