using System.Collections.Generic;
using System.ComponentModel;
using ProtoBuf;

[ProtoContract]
[UIField]
public class WildSuit : AWild
{
	[ProtoMember(1)]
	[UIField(tooltip = "Suits that card can be wilded into.")]
	[DefaultValue(PlayingCardSuits.Club | PlayingCardSuits.Diamond | PlayingCardSuits.Heart | PlayingCardSuits.Spade)]
	public readonly PlayingCardSuits suits = PlayingCardSuits.Club | PlayingCardSuits.Diamond | PlayingCardSuits.Heart | PlayingCardSuits.Spade;

	private WildSuit()
	{
	}

	public WildSuit(PlayingCardSuits suits, PlayingCard.Filter? filter = null)
		: base(filter)
	{
		this.suits = suits;
	}

	protected override void _Process(ref PlayingCardTypes output)
	{
		output |= suits.ToPlayingCardTypes(output.Values());
	}

	public override IEnumerable<AbilityKeyword> GetKeywords()
	{
		yield return (suits == (PlayingCardSuits.Club | PlayingCardSuits.Diamond | PlayingCardSuits.Heart | PlayingCardSuits.Spade)) ? AbilityKeyword.WildSuit : AbilityKeyword.Wild;
	}

	public override string ToString()
	{
		return "Wild Suit (" + suits.ToText() + ")" + base._filterString;
	}

	public override bool Equals(AWild other)
	{
		if (base.Equals(other) && other is WildSuit wildSuit)
		{
			return wildSuit.suits == suits;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode() ^ (int)suits;
	}

	public override AWild CombineWith(AWild other)
	{
		if (!(other is WildSuit wildSuit))
		{
			return null;
		}
		if (_filter.Contains(wildSuit._filter) && EnumUtil.HasFlags(suits, wildSuit.suits))
		{
			return this;
		}
		if (wildSuit._filter.Contains(_filter) && EnumUtil.HasFlags(wildSuit.suits, suits))
		{
			return wildSuit;
		}
		return null;
	}
}
