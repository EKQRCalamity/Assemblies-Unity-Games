using System;
using ProtoBuf;

[ProtoContract]
public class BBool
{
	[ProtoMember(1)]
	private bool _value;

	public bool value
	{
		get
		{
			return _value;
		}
		set
		{
			if (value != _value)
			{
				_value = value;
				this.onValueChanged?.Invoke(_value);
			}
		}
	}

	public event Action<bool> onValueChanged;

	public BBool()
	{
	}

	public BBool(bool value)
	{
		_value = value;
	}

	public bool Equals(BBool other)
	{
		if (other != null)
		{
			return other._value == _value;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is BBool other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (!_value)
		{
			return 0;
		}
		return 1;
	}

	public static bool operator ==(BBool a, BBool b)
	{
		return (bool)a == (bool)b;
	}

	public static bool operator !=(BBool a, BBool b)
	{
		return (bool)a != (bool)b;
	}

	public static implicit operator bool(BBool a)
	{
		return a?.value ?? false;
	}

	public static implicit operator string(BBool a)
	{
		return a?.ToString() ?? "false";
	}

	public override string ToString()
	{
		return ((bool)this).ToString();
	}
}
