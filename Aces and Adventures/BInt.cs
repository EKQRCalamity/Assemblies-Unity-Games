using System;
using ProtoBuf;

[ProtoContract]
[ProtoInclude(14, typeof(CappedBInt))]
[ProtoInclude(15, typeof(ClampedBInt))]
public class BInt : IEquatable<BInt>
{
	public delegate void OnValueChange(int previous, int current);

	public struct SuppressEvents : IDisposable
	{
		private BInt _int;

		private OnValueChange _event;

		private int _value;

		public SuppressEvents(BInt value)
		{
			_int = value;
			_event = value.onValueChanged;
			_value = value.value;
			value.onValueChanged = null;
		}

		public void Dispose()
		{
			_int.onValueChanged = _event;
			if (_value != _int.value)
			{
				_int.onValueChanged?.Invoke(_value, _int.value);
			}
		}
	}

	[ProtoMember(1)]
	private int _value;

	public int value
	{
		get
		{
			return _value;
		}
		set
		{
			value = _ProcessInput(value);
			if (value != _value)
			{
				int num = _ToInt();
				_value = value;
				int num2 = _ToInt();
				if (num2 != num)
				{
					this.onValueChanged?.Invoke(num, num2);
				}
			}
		}
	}

	public event OnValueChange onValueChanged;

	public BInt()
	{
	}

	public BInt(int value)
	{
		_value = value;
	}

	protected virtual int _ToInt()
	{
		return _value;
	}

	protected virtual int _ProcessInput(int inputValue)
	{
		return inputValue;
	}

	public SuppressEvents Suppress()
	{
		return new SuppressEvents(this);
	}

	public bool Equals(BInt other)
	{
		return value == other?.value;
	}

	public override bool Equals(object obj)
	{
		if (obj is BInt other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return value;
	}

	public static bool operator ==(BInt a, BInt b)
	{
		return a?.value == b?.value;
	}

	public static bool operator !=(BInt a, BInt b)
	{
		return a?.value != b?.value;
	}

	public static implicit operator int(BInt a)
	{
		return a?._ToInt() ?? 0;
	}

	public static implicit operator string(BInt a)
	{
		return ((int)a).ToString();
	}

	public override string ToString()
	{
		return value.ToString();
	}
}
