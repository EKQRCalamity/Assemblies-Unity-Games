using System.ComponentModel;
using ProtoBuf;

[ProtoContract]
[UIField]
public class WildRemapValue : AWild
{
	[ProtoMember(1)]
	[UIField]
	[DefaultValue(PlayingCardValues.Ace)]
	public readonly PlayingCardValues values = PlayingCardValues.Ace;

	private WildRemapValue()
	{
	}

	public WildRemapValue(PlayingCardValues values, PlayingCard.Filter? filter = null)
		: base(filter)
	{
		this.values = values;
	}

	protected override void _Process(ref PlayingCardTypes output)
	{
		output |= values.ToPlayingCardTypes(output.Suits());
	}

	public override string ToString()
	{
		return "Remap to (" + EnumUtil.FriendlyNameFlagRanges(values) + ")" + base._filterString;
	}

	public override bool Equals(AWild other)
	{
		if (base.Equals(other) && other is WildRemapValue wildRemapValue)
		{
			return wildRemapValue.values == values;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode() ^ (int)values;
	}

	public override AWild CombineWith(AWild other)
	{
		if (!(other is WildRemapValue wildRemapValue))
		{
			return null;
		}
		if (_filter.Contains(wildRemapValue._filter) && EnumUtil.HasFlags(values, wildRemapValue.values))
		{
			return this;
		}
		if (wildRemapValue._filter.Contains(_filter) && EnumUtil.HasFlags(wildRemapValue.values, values))
		{
			return wildRemapValue;
		}
		return null;
	}
}
