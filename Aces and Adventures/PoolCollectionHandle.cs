using System;
using System.Collections.Generic;

public class PoolCollectionHandle<C, E> : IDisposable where C : class, ICollection<E> where E : class
{
	public C value { get; private set; }

	public int Count => value.Count;

	public static PoolCollectionHandle<C, E> GetHandle()
	{
		return Pools.Unpool<PoolCollectionHandle<C, E>>();
	}

	static PoolCollectionHandle()
	{
		Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new PoolCollectionHandle<C, E>(), delegate(PoolCollectionHandle<C, E> handle)
		{
			handle.Clear();
		}, delegate(PoolCollectionHandle<C, E> handle)
		{
			handle.OnUnpool();
		});
		Pools.CreatePool<E>();
		Pools.CreatePool<C>();
	}

	public void Add(E value)
	{
		this.value.Add(value);
	}

	public bool Remove(E value)
	{
		return this.value.Remove(value);
	}

	public static implicit operator C(PoolCollectionHandle<C, E> handle)
	{
		return handle.value;
	}

	public static implicit operator bool(PoolCollectionHandle<C, E> handle)
	{
		return handle.value != null;
	}

	private void OnUnpool()
	{
		value = Pools.Unpool<C>();
	}

	private void Clear()
	{
		if (value != null)
		{
			Pools.RepoolItems<C, E>(value, repoolCollection: true);
			value = null;
			Pools.Repool(this);
		}
	}

	public void Dispose()
	{
		Clear();
	}
}
