using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public abstract class AsyncTask
{
	protected static readonly Queue<AsyncTask> PendingResults = new Queue<AsyncTask>();

	private static readonly List<IAwaitUpdate> AwaitUpdates = new List<IAwaitUpdate>();

	protected bool _done;

	protected bool _running;

	public static void _Update()
	{
		lock (PendingResults)
		{
			while (PendingResults.Count > 0)
			{
				PendingResults.Dequeue()._BroadcastResult();
			}
		}
		for (int num = AwaitUpdates.Count - 1; num >= 0; num--)
		{
			if (AwaitUpdates[num].Update())
			{
				AwaitUpdates.RemoveAt(num);
			}
		}
	}

	public static void AddAwaitUpdate(IAwaitUpdate awaitUpdate)
	{
		AwaitUpdates.Add(awaitUpdate);
	}

	protected void _BeginCalculation()
	{
		if (!_running)
		{
			_running = true;
			_BeginCalculationUnique();
		}
	}

	protected abstract void _BroadcastResult();

	protected abstract void _BeginCalculationUnique();
}
public class AsyncTask<T> : AsyncTask
{
	private static readonly Func<T> CalculateResultDefaultFunc;

	private T _result;

	private Func<T> _calculateResultFunc;

	private event Action<T> _OnResult;

	static AsyncTask()
	{
		CalculateResultDefaultFunc = delegate
		{
			Debug.LogWarning("CalculateResultDefaultFunc<" + typeof(T)?.ToString() + "> called.");
			return default(T);
		};
	}

	public static AsyncTask<T> Do(Func<T> calculateResult)
	{
		return new AsyncTask<T>()._Run(calculateResult);
	}

	protected override void _BeginCalculationUnique()
	{
		ThreadPool.QueueUserWorkItem(_CalculateResult);
	}

	protected override void _BroadcastResult()
	{
		if (this._OnResult != null)
		{
			this._OnResult(_result);
		}
		this._OnResult = null;
	}

	private AsyncTask<T> _Run(Func<T> calculateResult)
	{
		_calculateResultFunc = calculateResult;
		_BeginCalculation();
		return this;
	}

	private void _CalculateResult(object state)
	{
		try
		{
			_result = (_calculateResultFunc ?? CalculateResultDefaultFunc)();
		}
		catch (Exception e)
		{
			Debug.LogError(e.ErrorString());
		}
		_done = true;
		if (this._OnResult != null)
		{
			lock (AsyncTask.PendingResults)
			{
				AsyncTask.PendingResults.Enqueue(this);
			}
		}
		_running = false;
		_calculateResultFunc = null;
	}

	public void GetResult(Action<T> onResult)
	{
		if (_done)
		{
			onResult(_result);
		}
		else
		{
			_OnResult += onResult;
		}
	}
}
