using System.ComponentModel;
using ProtoBuf;

[ProtoContract]
public class CappedBInt : BInt
{
	private const int DEFAULT_MAX = 20;

	[ProtoMember(1)]
	[DefaultValue(20)]
	private int _maxValue = 20;

	public CappedBInt()
	{
	}

	public CappedBInt(int value, int maxValue = 20)
		: base(value)
	{
		_maxValue = maxValue;
	}

	protected override int _ProcessInput(int inputValue)
	{
		if (inputValue >= _maxValue)
		{
			return _maxValue;
		}
		return inputValue;
	}
}
