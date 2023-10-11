using ProtoBuf;

[ProtoContract]
public class BString
{
	public delegate void OnValueChange(string previous, string current);

	[ProtoMember(1)]
	private string _value;

	public string value
	{
		get
		{
			return _value;
		}
		set
		{
			if (!(value == _value))
			{
				string previous = _value;
				_value = value;
				this.onValueChanged?.Invoke(previous, _value);
			}
		}
	}

	public event OnValueChange onValueChanged;

	public BString()
	{
	}

	public BString(string value)
	{
		_value = value;
	}

	public bool Equals(BString other)
	{
		return value == other?.value;
	}

	public override bool Equals(object obj)
	{
		if (obj is BString other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return value.GetHashCode();
	}

	public static bool operator ==(BString a, BString b)
	{
		return a?.value == b?.value;
	}

	public static bool operator !=(BString a, BString b)
	{
		return a?.value != b?.value;
	}

	public static implicit operator string(BString a)
	{
		return a.value;
	}

	public override string ToString()
	{
		return value;
	}
}
