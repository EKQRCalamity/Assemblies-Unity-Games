using System.Runtime.CompilerServices;
using System.Threading.Tasks;

public class AwaitValueSet<T>
{
	private TaskCompletionSource<T> _taskCompletionSource;

	public AwaitValueSet()
	{
		_taskCompletionSource = new TaskCompletionSource<T>();
	}

	public TaskAwaiter<T> GetAwaiter()
	{
		return _taskCompletionSource.Task.GetAwaiter();
	}

	public void SetValue(T value)
	{
		_taskCompletionSource.SetResult(value);
	}
}
