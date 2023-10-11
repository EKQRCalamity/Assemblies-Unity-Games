using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

public class AwaitCondition : IAwaitUpdate
{
	private readonly Func<bool> _condition;

	private readonly TaskCompletionSource<bool> _taskCompletionSource;

	public AwaitCondition(Func<bool> condition)
	{
		_condition = condition;
		_taskCompletionSource = new TaskCompletionSource<bool>();
		if (_condition())
		{
			_taskCompletionSource.SetResult(result: true);
		}
		else
		{
			AsyncTask.AddAwaitUpdate(this);
		}
	}

	public TaskAwaiter GetAwaiter()
	{
		return ((Task)_taskCompletionSource.Task).GetAwaiter();
	}

	public bool Update()
	{
		if (!_condition())
		{
			return false;
		}
		_taskCompletionSource.SetResult(result: true);
		return true;
	}
}
