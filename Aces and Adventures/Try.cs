public class Try<T>
{
	protected T _result;

	protected bool _success;

	public T result => _result;

	public Try(T result, bool success)
	{
		_result = result;
		_success = success;
	}

	public static implicit operator bool(Try<T> t)
	{
		return t?._success ?? false;
	}

	public static implicit operator T(Try<T> t)
	{
		if (t == null)
		{
			return default(T);
		}
		return t._result;
	}
}
