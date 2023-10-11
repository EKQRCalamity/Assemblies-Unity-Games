using System;
using ProtoBuf;

[ProtoContract]
public class BFloat : IEquatable<BFloat>
{
	public delegate void OnValueChange(float previous, float current);

	[ProtoMember(1)]
	private float _value;

	public float value
	{
		get
		{
			return _value;
		}
		set
		{
			if (value != _value)
			{
				float previous = _value;
				_value = value;
				this.onValueChanged?.Invoke(previous, _value);
			}
		}
	}

	public event OnValueChange onValueChanged;

	public bool Equals(BFloat other)
	{
		return value == other?.value;
	}

	public override bool Equals(object obj)
	{
		if (obj is BFloat other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return value.GetHashCode();
	}

	public static bool operator ==(BFloat a, BFloat b)
	{
		return a?.value == b?.value;
	}

	public static bool operator !=(BFloat a, BFloat b)
	{
		return a?.value != b?.value;
	}

	public static implicit operator float(BFloat a)
	{
		return a?.value ?? 0f;
	}

	public static implicit operator string(BFloat a)
	{
		return ((float)a).ToString();
	}

	public override string ToString()
	{
		return value.ToString();
	}
}
