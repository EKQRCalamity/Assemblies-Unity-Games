using ProtoBuf;

[ProtoContract]
[UIField]
public struct OptionalContent<T> where T : class, new()
{
	[ProtoMember(1)]
	[UIField(validateOnChange = true)]
	private bool _enabled;

	[ProtoMember(2)]
	[UIField("Content", 0u, null, null, null, null, null, null, false, null, 5, false, null, collapse = UICollapseType.Hide)]
	[UIHideIf("_hideContent")]
	private T _value;

	public bool enabled => _enabled;

	public T value => _value ?? (_value = new T());

	private bool _hideContent => !_enabled;

	private bool _valueSpecified => _enabled;

	public OptionalContent(T value)
	{
		_enabled = false;
		_value = value;
	}

	public static implicit operator T(OptionalContent<T> c)
	{
		return c.value;
	}

	public static implicit operator bool(OptionalContent<T> optional)
	{
		if (optional._enabled)
		{
			return optional.value != null;
		}
		return false;
	}
}
