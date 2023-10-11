using System;
using System.Collections.Generic;

public class PooledList<T>
{
	private List<T> _Active;

	protected Queue<T> _Pool;

	protected Func<T> Constructor;

	protected Action<T> Cleaner;

	protected Func<T, bool> ValidityCheck;

	protected Action<T> OnValid;

	protected Action<T> Initialize;

	protected bool ExpandPool;

	public T this[int index] => _Active[index];

	public int Count => _Active.Count;

	public PooledList(int poolSize, Func<T> constructor, bool expandPool = true, Action<T> initialize = null, Func<T, bool> validityCheck = null, Action<T> onValid = null, Action<T> instanceClean = null)
	{
		ExpandPool = expandPool;
		_Active = new List<T>(poolSize);
		_Pool = new Queue<T>(poolSize);
		Constructor = constructor;
		Cleaner = instanceClean;
		ValidityCheck = validityCheck;
		OnValid = onValid;
		Initialize = initialize;
		for (int i = 0; i < poolSize; i++)
		{
			_Pool.Enqueue(constructor());
		}
	}

	public T Unpool()
	{
		if (_Pool.Count == 0)
		{
			if (!ExpandPool)
			{
				return default(T);
			}
			_Pool.Enqueue(Constructor());
		}
		T val = _Pool.Dequeue();
		if (Initialize != null)
		{
			Initialize(val);
		}
		bool flag = true;
		if (ValidityCheck != null)
		{
			flag = ValidityCheck(val);
		}
		if (flag)
		{
			if (OnValid != null)
			{
				OnValid(val);
			}
			_Active.Add(val);
			return val;
		}
		Repool(val);
		return default(T);
	}

	protected void Repool(T item)
	{
		_Pool.Enqueue(item);
		if (Cleaner != null)
		{
			Cleaner(item);
		}
	}

	public void RemoveAt(int index)
	{
		Repool(_Active[index]);
		_Active.RemoveAt(index);
	}

	public void Remove(T item)
	{
		Repool(item);
		_Active.Remove(item);
	}

	public void Clear()
	{
		for (int num = _Active.Count - 1; num >= 0; num--)
		{
			RemoveAt(num);
		}
	}
}
