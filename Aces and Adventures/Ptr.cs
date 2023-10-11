using System;

public sealed class Ptr<T>
{
	private readonly Func<T> _getter;

	private readonly Action<T> _setter;

	public T value
	{
		get
		{
			return _getter();
		}
		set
		{
			_setter(value);
		}
	}

	public Ptr(Func<T> getter, Action<T> setter)
	{
		_getter = getter;
		_setter = setter;
	}
}
