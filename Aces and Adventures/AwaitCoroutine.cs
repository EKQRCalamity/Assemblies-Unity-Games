using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

public class AwaitCoroutine<T>
{
	private IEnumerator _coroutine;

	private TaskCompletionSource<T> _taskCompletionSource;

	public AwaitCoroutine(IEnumerator coroutine)
	{
		_coroutine = coroutine;
		_taskCompletionSource = new TaskCompletionSource<T>();
		UJobManager.Instance.StartCoroutine(_Run());
	}

	public AwaitCoroutine(object value)
		: this(_Run(value))
	{
	}

	private static IEnumerator _Run(object value)
	{
		yield return value;
	}

	private IEnumerator _Run()
	{
		while (_coroutine.MoveNext())
		{
			yield return _coroutine.Current;
		}
		_taskCompletionSource.SetResult((T)_coroutine.Current);
	}

	public TaskAwaiter<T> GetAwaiter()
	{
		return _taskCompletionSource.Task.GetAwaiter();
	}
}
