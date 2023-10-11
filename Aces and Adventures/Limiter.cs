using System;
using System.Collections.Generic;

public class Limiter<T>
{
	private List<T> _active;

	private Func<T, bool> _alive;

	private Func<T, T, bool> _alike;

	static Limiter()
	{
		Pools.CreatePool<Limiter<T>>();
	}

	public static Limiter<T> Create(Func<T, bool> alive, Func<T, T, bool> alike)
	{
		return Pools.Unpool<Limiter<T>>()._SetData(alive, alike);
	}

	private Limiter<T> _SetData(Func<T, bool> alive, Func<T, T, bool> alike)
	{
		_alive = alive;
		_alike = alike;
		return this;
	}

	public bool ShouldAdd(T item)
	{
		for (int num = _active.Count - 1; num >= 0; num--)
		{
			if (!_alive(_active[num]))
			{
				_active.RemoveAt(num);
			}
		}
		for (int i = 0; i < _active.Count; i++)
		{
			if (_alike(_active[i], item))
			{
				return false;
			}
		}
		_active.Add(item);
		return true;
	}

	private void OnUnpool()
	{
		Pools.TryUnpool(ref _active);
	}

	private void Clear()
	{
		Pools.Repool(ref _active);
		_alive = null;
		_alike = null;
	}
}
