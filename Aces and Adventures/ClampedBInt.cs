using System.ComponentModel;
using ProtoBuf;

[ProtoContract]
public class ClampedBInt : BInt
{
	private const int DEFAULT_MAX = 20;

	[ProtoMember(1)]
	[DefaultValue(20)]
	private int _maxValue = 20;

	public ClampedBInt()
	{
	}

	public ClampedBInt(int value, int maxValue = 20)
		: base(value)
	{
		_maxValue = maxValue;
	}

	protected override int _ToInt()
	{
		if (base.value >= _maxValue)
		{
			return _maxValue;
		}
		return base.value;
	}
}
